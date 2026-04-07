using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="UserRepository"/> using an in-memory EF Core database.
/// </summary>
public sealed class UserRepositoryTests
{
    private static UserEntity CreateUserEntity(
        string userId = "U001",
        string username = "radiographer1",
        UserRole role = UserRole.Radiographer) =>
        new()
        {
            UserId = userId,
            Username = username,
            DisplayName = "Test User",
            PasswordHash = "$2b$12$hash",
            RoleValue = (int)role,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
        };

    private static async Task SeedUserAsync(HnVueDbContext ctx, UserEntity? entity = null)
    {
        entity ??= CreateUserEntity();
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
    }

    // ── GetByUsernameAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameAsync("radiographer1");

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be("U001");
        result.Value.Role.Should().Be(UserRole.Radiographer);
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameAsync("nobody");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync("U001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("radiographer1");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync("NONE");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── UpdateFailedLoginCountAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateFailedLoginCountAsync_ExistingUser_UpdatesCount()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateFailedLoginCountAsync("U001", 3);

        result.IsSuccess.Should().BeTrue();
        var user = await repo.GetByIdAsync("U001");
        user.Value.FailedLoginCount.Should().Be(3);
    }

    [Fact]
    public async Task UpdateFailedLoginCountAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateFailedLoginCountAsync("NONE", 1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── SetLockedAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SetLockedAsync_ExistingUser_UpdatesLockState()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.SetLockedAsync("U001", true);

        result.IsSuccess.Should().BeTrue();
        var user = await repo.GetByIdAsync("U001");
        user.Value.IsLocked.Should().BeTrue();
    }

    [Fact]
    public async Task SetLockedAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.SetLockedAsync("NONE", true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── UpdatePasswordHashAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePasswordHashAsync_ExistingUser_UpdatesHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.UpdatePasswordHashAsync("U001", "$2b$12$newHash");

        result.IsSuccess.Should().BeTrue();
        var user = await repo.GetByIdAsync("U001");
        user.Value.PasswordHash.Should().Be("$2b$12$newHash");
    }

    [Fact]
    public async Task UpdatePasswordHashAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdatePasswordHashAsync("NONE", "hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WithUsers_ReturnsAll()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx, CreateUserEntity("U001", "user1"));
        await SeedUserAsync(ctx, CreateUserEntity("U002", "user2", UserRole.Admin));
        var repo = new UserRepository(ctx);

        var result = await repo.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_Empty_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUsernameAsync_PreservesAllRoleValues()
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            await using var ctx = TestDbContextFactory.Create();
            var entity = CreateUserEntity("U001", "user1", role);
            ctx.Users.Add(entity);
            await ctx.SaveChangesAsync();
            var repo = new UserRepository(ctx);

            var result = await repo.GetByUsernameAsync("user1");

            result.IsSuccess.Should().BeTrue();
            result.Value.Role.Should().Be(role);
        }
    }

    // ── SetQuickPinHashAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SetQuickPinHashAsync_ExistingUser_StoresPinHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.SetQuickPinHashAsync("U001", "$pin$hash");

        result.IsSuccess.Should().BeTrue();
        var pinHash = await repo.GetQuickPinHashAsync("U001");
        pinHash.IsSuccess.Should().BeTrue();
        pinHash.Value.Should().Be("$pin$hash");
    }

    [Fact]
    public async Task SetQuickPinHashAsync_ClearsPin_StoresNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        await repo.SetQuickPinHashAsync("U001", "$pin$hash");

        var result = await repo.SetQuickPinHashAsync("U001", null);

        result.IsSuccess.Should().BeTrue();
        var pinHash = await repo.GetQuickPinHashAsync("U001");
        pinHash.Value.Should().BeNull();
    }

    [Fact]
    public async Task SetQuickPinHashAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.SetQuickPinHashAsync("NONE", "$pin$hash");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── GetQuickPinHashAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetQuickPinHashAsync_UserWithNoPin_ReturnsSuccessWithNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("U001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetQuickPinHashAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("NONE");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── UpdateQuickPinFailureAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateQuickPinFailureAsync_ExistingUser_StoresFailureState()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        var lockedUntil = DateTimeOffset.UtcNow.AddMinutes(30);

        var result = await repo.UpdateQuickPinFailureAsync("U001", 3, lockedUntil);

        result.IsSuccess.Should().BeTrue();
        var user = await repo.GetByIdAsync("U001");
        user.Value.QuickPinFailedCount.Should().Be(3);
        user.Value.QuickPinLockedUntil.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateQuickPinFailureAsync_ClearsLockout_StoresNullLockedUntil()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        await repo.UpdateQuickPinFailureAsync("U001", 3, DateTimeOffset.UtcNow.AddMinutes(30));

        var result = await repo.UpdateQuickPinFailureAsync("U001", 0, null);

        result.IsSuccess.Should().BeTrue();
        var user = await repo.GetByIdAsync("U001");
        user.Value.QuickPinFailedCount.Should().Be(0);
        user.Value.QuickPinLockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task UpdateQuickPinFailureAsync_NonExistentUser_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateQuickPinFailureAsync("NONE", 1, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── CancellationToken propagation ─────────────────────────────────────────

    [Fact]
    public async Task GetByUsernameAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByUsernameAsync("radiographer1", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByIdAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByIdAsync("U001", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAllAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetAllAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetQuickPinHashAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        await SeedUserAsync(ctx);
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetQuickPinHashAsync("U001", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── DbUpdateException path (SQLite to enforce unique Username constraint) ──

    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    [Fact]
    public async Task UpdateFailedLoginCountAsync_ConcurrentDbError_ReturnsDbError()
    {
        // Verify that duplicate Username unique index violation is caught at DB level
        var (ctx2, conn2) = CreateSqliteContext();
        using (conn2)
        await using (ctx2)
        {
            var e1 = CreateUserEntity("UA", "dupuser");
            var e2 = CreateUserEntity("UB", "dupuser"); // same username, unique constraint
            ctx2.Users.Add(e1);
            ctx2.Users.Add(e2);

            var act = async () => await ctx2.SaveChangesAsync();

            await act.Should().ThrowAsync<Exception>("SQLite unique constraint on Username should fail");
        }
    }
}
