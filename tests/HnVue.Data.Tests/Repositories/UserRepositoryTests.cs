using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;

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
}
