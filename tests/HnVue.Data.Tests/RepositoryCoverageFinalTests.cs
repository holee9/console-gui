using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Mappers;
using HnVue.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace HnVue.Data.Tests;

/// <summary>
/// Coverage boost tests for repository methods targeting 85%+ coverage.
/// Focuses on UserRepository, PatientRepository edge cases.
/// </summary>
public sealed class RepositoryCoverageFinalTests
{
    private static UserRecord CreateUser(
        string userId = "U001",
        string username = "testuser",
        string displayName = "Test User",
        string passwordHash = "hash123",
        UserRole role = UserRole.Admin,
        int failedCount = 0,
        bool isLocked = false,
        DateTimeOffset? lastLogin = null,
        string? quickPinHash = null,
        int quickPinFailedCount = 0,
        DateTimeOffset? quickPinLockedUntil = null) =>
        new(userId, username, displayName, passwordHash, role, failedCount,
            isLocked, lastLogin, quickPinHash, quickPinFailedCount, quickPinLockedUntil);

    private static PatientRecord CreatePatient(
        string patientId = "P001",
        string name = "Test^Patient",
        string? sex = "M",
        bool isEmergency = false) =>
        new(patientId, name, new DateOnly(1990, 1, 1), sex, isEmergency,
            DateTimeOffset.UtcNow, "tester");

    // ── UserRepository: GetByUsernameAsync ──────────────────────────────────

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_Found_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(username: "findme")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByUsernameAsync("findme");

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("findme");
    }

    // ── UserRepository: GetByIdAsync ────────────────────────────────────────

    [Fact]
    public async Task UserRepository_GetByIdAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_GetByIdAsync_Found_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var user = CreateUser(userId: "U-FIND");
        ctx.Users.Add(EntityMapper.ToEntity(user));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync("U-FIND");

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be("U-FIND");
    }

    // ── UserRepository: GetAllAsync ─────────────────────────────────────────

    [Fact]
    public async Task UserRepository_GetAllAsync_Empty_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task UserRepository_GetAllAsync_MultipleUsers_ReturnsAll()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U1", username: "user1")));
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U2", username: "user2")));
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U3", username: "user3")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    // ── UserRepository: AddAsync ────────────────────────────────────────────

    [Fact]
    public async Task UserRepository_AddAsync_Success()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.AddAsync(CreateUser(userId: "U-ADD"));

        result.IsSuccess.Should().BeTrue();
        (await ctx.Users.CountAsync()).Should().Be(1);
    }

    // ── UserRepository: SetLockedAsync ──────────────────────────────────────

    [Fact]
    public async Task UserRepository_SetLockedAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.SetLockedAsync("nonexistent", true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_SetLockedAsync_LockAndUnlock()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-LCK")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var lockResult = await repo.SetLockedAsync("U-LCK", true);
        lockResult.IsSuccess.Should().BeTrue();

        var unlockResult = await repo.SetLockedAsync("U-LCK", false);
        unlockResult.IsSuccess.Should().BeTrue();
    }

    // ── UserRepository: UpdatePasswordHashAsync ─────────────────────────────

    [Fact]
    public async Task UserRepository_UpdatePasswordHashAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdatePasswordHashAsync("nonexistent", "newhash");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdatePasswordHashAsync_Success()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-PWD")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdatePasswordHashAsync("U-PWD", "newhash");

        result.IsSuccess.Should().BeTrue();
    }

    // ── UserRepository: UpdateFailedLoginCountAsync ─────────────────────────

    [Fact]
    public async Task UserRepository_UpdateFailedLoginCountAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateFailedLoginCountAsync("nonexistent", 3);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdateFailedLoginCountAsync_Success()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-FLC")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateFailedLoginCountAsync("U-FLC", 5);

        result.IsSuccess.Should().BeTrue();
    }

    // ── UserRepository: SetQuickPinHashAsync ────────────────────────────────

    [Fact]
    public async Task UserRepository_SetQuickPinHashAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.SetQuickPinHashAsync("nonexistent", "pinhash");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_SetQuickPinHashAsync_SetAndClear()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-PIN")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var set = await repo.SetQuickPinHashAsync("U-PIN", "pinhash123");
        set.IsSuccess.Should().BeTrue();

        var clear = await repo.SetQuickPinHashAsync("U-PIN", null);
        clear.IsSuccess.Should().BeTrue();
    }

    // ── UserRepository: GetQuickPinHashAsync ────────────────────────────────

    [Fact]
    public async Task UserRepository_GetQuickPinHashAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_GetQuickPinHashAsync_NoPin_ReturnsNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-NOPIN")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("U-NOPIN");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UserRepository_GetQuickPinHashAsync_WithPin_ReturnsHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = EntityMapper.ToEntity(CreateUser(userId: "U-WPIN"));
        entity.QuickPinHash = "pinhash";
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("U-WPIN");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("pinhash");
    }

    // ── UserRepository: UpdateQuickPinFailureAsync ──────────────────────────

    [Fact]
    public async Task UserRepository_UpdateQuickPinFailureAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateQuickPinFailureAsync("nonexistent", 1, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdateQuickPinFailureAsync_WithLockout()
    {
        await using var ctx = TestDbContextFactory.Create();
        ctx.Users.Add(EntityMapper.ToEntity(CreateUser(userId: "U-QPF")));
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);

        var lockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
        var result = await repo.UpdateQuickPinFailureAsync("U-QPF", 3, lockedUntil);

        result.IsSuccess.Should().BeTrue();
    }

    // ── PatientRepository: FindByIdAsync ────────────────────────────────────

    [Fact]
    public async Task PatientRepository_FindByIdAsync_NotFound_ReturnsNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.FindByIdAsync("nonexistent");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task PatientRepository_FindByIdAsync_Deleted_ReturnsNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), default).ReturnsForAnyArgs(Result.Success());

        ctx.Patients.Add(EntityMapper.ToEntity(CreatePatient("P-DEL"), null));
        await ctx.SaveChangesAsync();
        var repo = new PatientRepository(ctx, auditRepo, null);

        await repo.DeleteAsync("P-DEL");

        var result = await repo.FindByIdAsync("P-DEL");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── PatientRepository: SearchAsync ──────────────────────────────────────

    [Fact]
    public async Task PatientRepository_SearchAsync_NoMatches_ReturnsEmpty()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.SearchAsync("nonexistent");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── PatientRepository: UpdateAsync ──────────────────────────────────────

    [Fact]
    public async Task PatientRepository_UpdateAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.UpdateAsync(CreatePatient("P-NONEXISTENT"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── PatientRepository: DeleteAsync ──────────────────────────────────────

    [Fact]
    public async Task PatientRepository_DeleteAsync_NotFound_ReturnsFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.DeleteAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task PatientRepository_DeleteAsync_AlreadyDeleted_IsIdempotent()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), default).ReturnsForAnyArgs(Result.Success());

        ctx.Patients.Add(EntityMapper.ToEntity(CreatePatient("P-IDEM"), null));
        await ctx.SaveChangesAsync();
        var repo = new PatientRepository(ctx, auditRepo, null);

        await repo.DeleteAsync("P-IDEM");
        var second = await repo.DeleteAsync("P-IDEM");

        second.IsSuccess.Should().BeTrue();
    }
}
