# Infrastructure Implementation Quality Guide

Team A agent reads this file when implementing code in Common, Data, Security, SystemAdmin, Update modules.

## Pre-Implementation Checklist

Before writing any code in infrastructure modules:

1. Read the existing interface in HnVue.Common.Abstractions/ — never create a duplicate
2. Check Directory.Packages.props for existing NuGet versions — never add a second version
3. Verify the ErrorCode range — Security uses 2xxx, Data uses 3xxx (see HnVue.Common/ErrorCode.cs)
4. Check if a migration already exists for the same date — use unique YYYYMMDD prefix

## Result<T> Implementation Patterns

### Correct: Service method returning Result<T>

```csharp
public async Task<Result<UserRecord>> GetUserAsync(string userId, CancellationToken ct)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(userId);
    
    var entity = await _repository.FindByIdAsync(userId, ct);
    if (entity is null)
        return Result<UserRecord>.Failure(ErrorCode.UserNotFound);
    
    if (entity.IsLocked)
        return Result<UserRecord>.Failure(ErrorCode.AccountLocked);
    
    return Result<UserRecord>.Success(entity.ToRecord());
}
```

### Anti-Pattern: Throwing exceptions for domain errors

```csharp
// WRONG — exceptions are for infrastructure failures, not domain logic
public async Task<UserRecord> GetUserAsync(string userId, CancellationToken ct)
{
    var entity = await _repository.FindByIdAsync(userId, ct);
    if (entity is null)
        throw new UserNotFoundException(userId); // WRONG
    return entity.ToRecord();
}
```

### Anti-Pattern: Swallowing errors

```csharp
// WRONG — silently returning default hides failures
public async Task<Result<UserRecord>> GetUserAsync(string userId, CancellationToken ct)
{
    try {
        var entity = await _repository.FindByIdAsync(userId, ct);
        return Result<UserRecord>.Success(entity!.ToRecord());
    }
    catch { return Result<UserRecord>.Success(default!); } // WRONG — hides null
}
```

## Repository Implementation Pattern

### Correct Pattern

```csharp
public class UserRepository : IUserRepository
{
    private readonly HnVueDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(HnVueDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UserEntity>> FindByIdAsync(string userId, CancellationToken ct)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
        
        return entity is not null
            ? Result<UserEntity>.Success(entity)
            : Result<UserEntity>.Failure(ErrorCode.UserNotFound);
    }
}
```

### Anti-Patterns to Avoid

- Missing CancellationToken parameter on ANY async method
- Using .Result or .Wait() instead of await (deadlock risk in WPF)
- Missing AsNoTracking() for read-only queries
- Exposing DbContext directly (always go through repository)
- Hardcoding connection strings (use IConfiguration)

## EF Core Migration Quality Gate

Before creating a migration:

1. Name format: `YYYYMMDD_DescriptiveName` (e.g., `20260411_AddAuditLogIndex`)
2. Both Up() and Down() implemented
3. Test the migration: `dotnet ef database update` then `dotnet ef database update 0` (rollback)
4. No data loss operations without explicit confirmation
5. Index naming convention: `IX_{Table}_{Column1}_{Column2}`

### Migration Anti-Patterns

- Empty Down() method — always implement rollback
- ALTER COLUMN on SQLite (not supported — must recreate table)
- Dropping columns with data without backup step
- Multiple migrations for the same date prefix

## Security Module Quality Gate

### Password Hashing

- bcrypt cost factor: EXACTLY 12 (not 10, not 15)
- Verify with: `BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)`
- Never store plain text, MD5, or SHA-based password hashes
- Password policy regex MUST have 100ms timeout (ReDoS prevention)

### JWT Token

- Algorithm: HS256 only (not RS256 — no PKI infrastructure yet)
- Include JTI (JWT ID) in every token for revocation support
- Expiry: configurable, default 8 hours
- Key rotation: always check PreviousSecretKey as fallback

### Audit Chain

- Every audit entry MUST reference the previous entry's hash
- HMAC-SHA256 with consistent key
- Never skip chain verification in tests
- Critical incidents: add CRITICAL_INCIDENT tag

### Anti-Patterns

- Hardcoded secrets anywhere in code
- JWT tokens in URL query parameters
- bcrypt cost factor < 12
- Missing token denylist check on validation
- Audit entries without hash chain link

## Update Module Safety Patterns

- SHA-256 verification is MANDATORY before any file replacement
- Backup MUST exist before staging
- State machine: InProgress -> Staged -> Completed/Failed/RolledBack
- RollbackAsync must work even if the staged update is corrupted
- Never delete backup until next successful update completes

## Testing Quality Requirements

### Test Structure

```csharp
[Fact]
[Trait("SWR", "SWR-CS-077")] // ALWAYS include SWR trait for traceability
public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccessWithToken()
{
    // Arrange
    var sut = CreateSecurityService();
    await sut.RegisterUserAsync("testuser", "P@ssw0rd!", ct: default);
    
    // Act
    var result = await sut.AuthenticateAsync("testuser", "P@ssw0rd!", ct: default);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Token.Should().NotBeNullOrWhiteSpace();
}
```

### Coverage Targets

| Module | Minimum | Current Priority |
|--------|---------|-----------------|
| Security | 90%+ | Safety-critical — every branch |
| Update | 90%+ | Safety-critical — rollback paths |
| Common | 85% | Core shared code |
| Data | 85% | Repository + migration |
| SystemAdmin | 85% | Standard |

### Anti-Patterns in Tests

- Using Thread.Sleep() instead of async patterns
- Missing CancellationToken in test async calls
- Not testing failure paths (only happy path)
- Mock-heavy tests that don't verify real behavior
- Missing [Trait("SWR", "...")] on safety-critical tests

## Post-Implementation Verification Script

After completing any infrastructure code change, run these commands in order:

```bash
# 1. Build owned modules
dotnet build src/HnVue.Common/ src/HnVue.Data/ src/HnVue.Security/ src/HnVue.SystemAdmin/ src/HnVue.Update/

# 2. Run owned tests
dotnet test tests/HnVue.Common.Tests/ tests/HnVue.Data.Tests/ tests/HnVue.Security.Tests/ tests/HnVue.SystemAdmin.Tests/ tests/HnVue.Update.Tests/ --verbosity normal

# 3. Full solution build (detect cross-module breakage)
dotnet build HnVue.sln -c Release

# 4. Architecture tests (verify no dependency violations)
dotnet test tests/HnVue.Architecture.Tests/ --verbosity normal
```

ALL four must pass before reporting COMPLETED. If step 3 fails due to other team's code, document the error but own modules must pass.

## Cross-Module Change Protocol

| Change Type | Notify | Issue Label | Before/After |
|-------------|--------|-------------|-------------|
| New interface in Common | Coordinator | breaking-change | Before implementation |
| New NuGet package | RA team | soup-update | After adding to Directory.Packages.props |
| DB schema change | Coordinator | team-a + feat | Before migration |
| Security policy change | RA team | ra-update | After implementation |
| ErrorCode addition | None required | — | Document in ErrorCode.cs comments |
