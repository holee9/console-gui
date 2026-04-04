using HnVue.Common.Models;
using HnVue.Data.Repositories;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="PatientRepository"/> using an in-memory EF Core database.
/// </summary>
public sealed class PatientRepositoryTests
{
    private static PatientRecord CreateSamplePatient(string id = "P001") =>
        new(
            PatientId: id,
            Name: "Doe^John",
            DateOfBirth: new DateOnly(1980, 6, 15),
            Sex: "M",
            IsEmergency: false,
            CreatedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy: "user-01");

    // ── AddAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewPatient_ReturnsSuccess()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        var patient = CreateSamplePatient();

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task AddAsync_PreservesAllFields()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        var patient = CreateSamplePatient();

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Doe^John");
        result.Value.DateOfBirth.Should().Be(new DateOnly(1980, 6, 15));
        result.Value.Sex.Should().Be("M");
        result.Value.IsEmergency.Should().BeFalse();
        result.Value.CreatedBy.Should().Be("user-01");
    }

    [Fact]
    public async Task AddAsync_NullDateOfBirth_IsPreserved()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        var patient = new PatientRecord("P002", "Smith^Jane", null, null, true,
            DateTimeOffset.UtcNow, "user-02");

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.DateOfBirth.Should().BeNull();
        result.Value.Sex.Should().BeNull();
        result.Value.IsEmergency.Should().BeTrue();
    }

    // ── FindByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FindByIdAsync_ExistingPatient_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.FindByIdAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task FindByIdAsync_NonExistentPatient_ReturnsSuccessWithNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);

        var result = await repo.FindByIdAsync("NONE");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── SearchAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatchingPatients()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient("P001"));
        await repo.AddAsync(new PatientRecord("P002", "Smith^Jane", null, "F", false, DateTimeOffset.UtcNow, "user-01"));

        var result = await repo.SearchAsync("doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task SearchAsync_ById_ReturnsMatchingPatient()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient("P001"));

        var result = await repo.SearchAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.SearchAsync("ZZZ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingPatient_PersistsChanges()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient());

        var updated = new PatientRecord("P001", "Updated^Name", null, "F", true,
            DateTimeOffset.UtcNow, "user-01");
        var result = await repo.UpdateAsync(updated);

        result.IsSuccess.Should().BeTrue();

        var found = await repo.FindByIdAsync("P001");
        found.Value!.Name.Should().Be("Updated^Name");
        found.Value.IsEmergency.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPatient_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);

        var result = await repo.UpdateAsync(CreateSamplePatient("NONE"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingPatient_RemovesRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.DeleteAsync("P001");

        result.IsSuccess.Should().BeTrue();
        var found = await repo.FindByIdAsync("P001");
        found.Value.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPatient_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new PatientRepository(ctx);

        var result = await repo.DeleteAsync("NONE");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }
}
