using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.CDBurning.Tests;

/// <summary>
/// Unit tests for <see cref="StudyRepository"/> using an in-memory EF Core database.
/// </summary>
[Trait("SWR", "SWR-CD-030")]
public sealed class StudyRepositoryTests
{
    private static HnVueDbContext CreateInMemoryContext()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new HnVueDbContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    private static PatientEntity CreatePatient(string id = "P001") => new()
    {
        PatientId = id,
        Name = "Test^Patient",
        IsEmergency = false,
        CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
        CreatedAtOffsetMinutes = 0,
        CreatedBy = "test",
    };

    private static StudyEntity CreateStudy(string uid = "1.2.3", string patientId = "P001") => new()
    {
        StudyInstanceUid = uid,
        PatientId = patientId,
        StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
        StudyDateOffsetMinutes = 0,
    };

    private static ImageEntity CreateImage(string studyUid, string filePath) => new()
    {
        ImageId = Guid.NewGuid().ToString(),
        StudyInstanceUid = studyUid,
        FilePath = filePath,
        AcquiredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
        AcquiredAtOffsetMinutes = 0,
    };

    // ── Constructor ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullDbContext_ThrowsArgumentNullException()
    {
        var act = () => new StudyRepository(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    // ── GetFilesForStudyAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetFilesForStudyAsync_StudyWithImages_ReturnsFilePaths()
    {
        await using var ctx = CreateInMemoryContext();
        ctx.Patients.Add(CreatePatient());
        ctx.Studies.Add(CreateStudy());
        ctx.Images.Add(CreateImage("1.2.3", @"C:\dicom\img1.dcm"));
        ctx.Images.Add(CreateImage("1.2.3", @"C:\dicom\img2.dcm"));
        await ctx.SaveChangesAsync();

        var repo = new StudyRepository(ctx);

        var result = await repo.GetFilesForStudyAsync("1.2.3");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(@"C:\dicom\img1.dcm");
        result.Value.Should().Contain(@"C:\dicom\img2.dcm");
    }

    [Fact]
    public async Task GetFilesForStudyAsync_NoImagesForStudy_ReturnsEmptyList()
    {
        await using var ctx = CreateInMemoryContext();
        var repo = new StudyRepository(ctx);

        var result = await repo.GetFilesForStudyAsync("NONE.UID");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilesForStudyAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        await using var ctx = CreateInMemoryContext();
        var repo = new StudyRepository(ctx);

        var act = async () => await repo.GetFilesForStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetFilesForStudyAsync_ImagesForDifferentStudy_ReturnsOnlyMatchingPaths()
    {
        await using var ctx = CreateInMemoryContext();
        ctx.Patients.Add(CreatePatient("P001"));
        ctx.Studies.Add(CreateStudy("1.2.3", "P001"));
        ctx.Studies.Add(CreateStudy("9.8.7", "P001"));
        ctx.Images.Add(CreateImage("1.2.3", @"C:\dicom\study1.dcm"));
        ctx.Images.Add(CreateImage("9.8.7", @"C:\dicom\study2.dcm"));
        await ctx.SaveChangesAsync();

        var repo = new StudyRepository(ctx);

        var result = await repo.GetFilesForStudyAsync("1.2.3");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Should().Be(@"C:\dicom\study1.dcm");
    }

    [Fact]
    public async Task GetFilesForStudyAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = CreateInMemoryContext();
        ctx.Patients.Add(CreatePatient());
        ctx.Studies.Add(CreateStudy());
        ctx.Images.Add(CreateImage("1.2.3", @"C:\dicom\img1.dcm"));
        await ctx.SaveChangesAsync();

        var repo = new StudyRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetFilesForStudyAsync("1.2.3", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
