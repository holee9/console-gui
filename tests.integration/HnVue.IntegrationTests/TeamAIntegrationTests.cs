using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using HnVue.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Cross-module integration tests for Team A (Common, Data, Security, SystemAdmin, Update).
/// Uses real services with in-memory SQLite database.
/// Tests verify that Data module provides storage, Security module authenticates against it,
/// and audit entries are stored correctly.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Team", "TeamA")]
public sealed class TeamAIntegrationTests : IDisposable
{
    private readonly HnVueDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly JwtOptions _jwtOptions;
    private readonly IOptions<AuditOptions> _auditOptions;
    private readonly ITokenDenylist _tokenDenylist;
    private readonly SecurityService _securityService;
    private readonly ISecurityContext _securityContext;

    public TeamAIntegrationTests()
    {
        // Create in-memory SQLite context
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _dbContext = new HnVueDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        // Initialize real repositories (not mocks)
        _userRepository = new UserRepository(_dbContext);
        _auditRepository = new AuditRepository(_dbContext);

        // Initialize security dependencies
        _jwtOptions = new JwtOptions
        {
            SecretKey = "IntegrationTestSecretKey-32CharMin!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue"
        };
        _auditOptions = Options.Create(new AuditOptions
        {
            HmacKey = "IntegrationTestHmacKey-32CharMin!"
        });
        _tokenDenylist = new PersistentTokenDenylist(TimeSpan.FromMinutes(15));
        _securityContext = new TestSecurityContext();

        // Initialize SecurityService with real dependencies
        _securityService = new SecurityService(
            _userRepository,
            _auditRepository,
            _securityContext,
            _jwtOptions,
            _auditOptions,
            _tokenDenylist);
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-010")]
    public async Task UserRepository_CreatedUser_CanBeAuthenticated()
    {
        // Arrange
        const string username = "testuser";
        const string password = "TestPass1!";
        var userEntity = new UserEntity
        {
            UserId = Guid.NewGuid().ToString(),
            Username = username,
            DisplayName = "Test User",
            PasswordHash = PasswordHasher.HashPassword(password),
            RoleValue = (int)UserRole.Radiographer,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Authenticate through SecurityService
        var authResult = await _securityService.AuthenticateAsync(username, password);

        // Assert - Authentication succeeded
        authResult.IsSuccess.Should().BeTrue("valid credentials should authenticate");
        authResult.Value.Username.Should().Be(username);
        authResult.Value.Token.Should().NotBeNullOrEmpty();
        authResult.Value.Role.Should().Be(UserRole.Radiographer);

        // Assert - Audit log entry was created
        var auditQuery = await _auditRepository.QueryAsync(
            new AuditQueryFilter { UserId = authResult.Value.UserId, MaxResults = 10 });
        auditQuery.IsSuccess.Should().BeTrue();
        auditQuery.Value.Should().Contain(e => e.Action == "LOGIN");
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-015")]
    public async Task SecurityService_CreateUserAndAuthenticate_AuditLogWritten()
    {
        // Arrange
        const string username = "audituser";
        const string password = "AuditPass1!";
        var userId = Guid.NewGuid().ToString();

        var userEntity = new UserEntity
        {
            UserId = userId,
            Username = username,
            DisplayName = "Audit User",
            PasswordHash = PasswordHasher.HashPassword(password),
            RoleValue = (int)UserRole.Admin,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Authenticate
        var authResult = await _securityService.AuthenticateAsync(username, password);

        // Assert - Authentication succeeded
        authResult.IsSuccess.Should().BeTrue();

        // Assert - Audit log entry was written with correct fields
        var auditQuery = await _auditRepository.QueryAsync(
            new AuditQueryFilter { UserId = userId, MaxResults = 100 });
        auditQuery.IsSuccess.Should().BeTrue();
        var loginEntries = auditQuery.Value.Where(e => e.Action == "LOGIN").ToList();
        loginEntries.Should().HaveCount(1, "exactly one LOGIN audit entry should exist");
        loginEntries[0].UserId.Should().Be(userId);
        loginEntries[0].Action.Should().Be("LOGIN");
        loginEntries[0].Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-020")]
    public async Task UserRepository_WriteAndQuery_RoundTrip()
    {
        // Arrange
        var user = new UserRecord(
            UserId: Guid.NewGuid().ToString(),
            Username: "queryuser",
            DisplayName: "Query User",
            PasswordHash: PasswordHasher.HashPassword("QueryPass1!"),
            Role: UserRole.Radiologist,
            FailedLoginCount: 0,
            IsLocked: false,
            LastLoginAt: null);

        // Act - Create user via DbContext (IUserRepository has no AddAsync)
        var userEntity = new UserEntity
        {
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            RoleValue = (int)user.Role,
            FailedLoginCount = user.FailedLoginCount,
            IsLocked = user.IsLocked,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Query by username
        var queryResult = await _userRepository.GetByUsernameAsync(user.Username);

        // Assert - User retrieved correctly
        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.UserId.Should().Be(user.UserId);
        queryResult.Value.Username.Should().Be(user.Username);
        queryResult.Value.DisplayName.Should().Be(user.DisplayName);
        queryResult.Value.Role.Should().Be(UserRole.Radiologist);
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-025")]
    public async Task AuditRepository_WriteAndQuery_RoundTrip()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var entry = new AuditEntry(
            EntryId: Guid.NewGuid().ToString(),
            Timestamp: DateTimeOffset.UtcNow,
            UserId: userId,
            Action: "TEST_ACTION",
            Details: "Integration test audit entry",
            PreviousHash: null,
            CurrentHash: "test-hash");

        // Act - Write audit entry
        var writeResult = await _auditRepository.AppendAsync(entry);
        writeResult.IsSuccess.Should().BeTrue();

        // Act - Query audit entries
        var queryResult = await _auditRepository.QueryAsync(
            new AuditQueryFilter { UserId = userId, MaxResults = 10 });

        // Assert - Entry retrieved correctly
        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Should().HaveCount(1);
        queryResult.Value[0].UserId.Should().Be(userId);
        queryResult.Value[0].Action.Should().Be("TEST_ACTION");
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-030")]
    public async Task SecurityService_FailedLogin_IncrementsFailedLoginCount()
    {
        // Arrange
        const string username = "failedloginuser";
        var userEntity = new UserEntity
        {
            UserId = Guid.NewGuid().ToString(),
            Username = username,
            DisplayName = "Failed Login User",
            PasswordHash = PasswordHasher.HashPassword("CorrectPass1!"),
            RoleValue = (int)UserRole.Radiographer,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Attempt authentication with wrong password
        var authResult = await _securityService.AuthenticateAsync(username, "WrongPass1!");

        // Assert - Authentication failed
        authResult.IsFailure.Should().BeTrue();
        authResult.Error.Should().Be(ErrorCode.AuthenticationFailed);

        // Assert - Failed login count incremented
        var userResult = await _userRepository.GetByUsernameAsync(username);
        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.FailedLoginCount.Should().Be(1);
    }

    [Fact]
    [Trait("SWR", "SWR-SEC-035")]
    public async Task SecurityService_FiveFailedLogins_LocksAccount()
    {
        // Arrange
        const string username = "lockuser";
        var userEntity = new UserEntity
        {
            UserId = Guid.NewGuid().ToString(),
            Username = username,
            DisplayName = "Lock User",
            PasswordHash = PasswordHasher.HashPassword("CorrectPass1!"),
            RoleValue = (int)UserRole.Radiographer,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Attempt 5 failed logins
        for (int i = 0; i < 5; i++)
        {
            await _securityService.AuthenticateAsync(username, "WrongPass1!");
        }

        // Assert - Account is now locked
        var userResult = await _userRepository.GetByUsernameAsync(username);
        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.IsLocked.Should().BeTrue();

        // Assert - Further login attempts fail with AccountLocked
        var lockResult = await _securityService.AuthenticateAsync(username, "CorrectPass1!");
        lockResult.IsFailure.Should().BeTrue();
        lockResult.Error.Should().Be(ErrorCode.AccountLocked);
    }

    [Fact]
    [Trait("SWR", "SWR-CS-077")]
    public async Task TokenDenylist_RevokeToken_PreventsReuse()
    {
        // Arrange
        const string username = "revokeuser";
        var userId = Guid.NewGuid().ToString();
        var userEntity = new UserEntity
        {
            UserId = userId,
            Username = username,
            DisplayName = "Revoke User",
            PasswordHash = PasswordHasher.HashPassword("RevokePass1!"),
            RoleValue = (int)UserRole.Admin,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null
        };
        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();

        // Act - Authenticate to get token
        var authResult = await _securityService.AuthenticateAsync(username, "RevokePass1!");
        authResult.IsSuccess.Should().BeTrue();
        var jti = authResult.Value.Jti;

        // Act - Revoke the token
        await _tokenDenylist.RevokeAsync(jti);

        // Assert - Token is now revoked
        var isRevoked = await _tokenDenylist.IsRevokedAsync(jti);
        isRevoked.Should().BeTrue("token should be marked as revoked");
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    /// <summary>
    /// Test implementation of ISecurityContext for integration testing.
    /// </summary>
    private sealed class TestSecurityContext : ISecurityContext
    {
        public AuthenticatedUser? CurrentUser { get; private set; }

        public string? CurrentUserId => CurrentUser?.UserId;
        public string? CurrentUsername => CurrentUser?.Username;
        public UserRole? CurrentRole => CurrentUser?.Role;
        public bool IsAuthenticated => CurrentUser is not null;
        public string? CurrentJti => CurrentUser?.Jti;

        public bool HasRole(UserRole role)
        {
            return CurrentUser?.Role == role;
        }

        public void SetCurrentUser(AuthenticatedUser user)
        {
            CurrentUser = user;
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
        }
    }
}
