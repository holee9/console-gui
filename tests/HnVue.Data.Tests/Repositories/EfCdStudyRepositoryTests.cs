using HnVue.Common.Results;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfCdStudyRepository"/> using an in-memory EF Core database.
/// REQ-COORD-006: SPEC-COORDINATOR-001 EF Core CD study file path query.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfCdStudyRepositoryTests
{
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

    private static ImageEntity CreateSampleImage(
        string studyInstanceUid = "STUDY-001",
        string filePath = "/data/images/image.dcm",
        string imageId = "IMG-001") =>
        new()
        {
            ImageId = imageId,
            StudyInstanceUid = studyInstanceUid,
            FilePath = filePath
        };

    private static async Task EnsureStudyExistsAsync(HnVueDbContext ctx, string studyInstanceUid)
    {
        if (!await ctx.Studies.AnyAsync(s => s.StudyInstanceUid == studyInstanceUid))
        {
            var patientId = $"P-{studyInstanceUid}";
            if (!await ctx.Patients.AnyAsync(p => p.PatientId == patientId))
            {
                ctx.Patients.Add(new PatientEntity
                {
                    PatientId = patientId,
                    Name = "Test^Patient",
                    CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
                });
            }

            ctx.Studies.Add(new StudyEntity
            {
                StudyInstanceUid = studyInstanceUid,
                PatientId = patientId,
                StudyDateTicks = DateTimeOffset.UtcNow.Ticks
            });
            await ctx.SaveChangesAsync();
        }
    }

    // ── GetFilesForStudyAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetFilesForStudyAsync_ExistingStudy_ReturnsFilePaths()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Arrange
        await EnsureStudyExistsAsync(ctx, "STUDY-001");
        await EnsureStudyExistsAsync(ctx, "STUDY-002");
        ctx.Images.Add(CreateSampleImage("STUDY-001", "/path1/image1.dcm", "IMG-001"));
        ctx.Images.Add(CreateSampleImage("STUDY-001", "/path1/image2.dcm", "IMG-002"));
        ctx.Images.Add(CreateSampleImage("STUDY-002", "/path2/image3.dcm", "IMG-003"));
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetFilesForStudyAsync("STUDY-001");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().ContainInOrder("/path1/image1.dcm", "/path1/image2.dcm");
    }

    [Fact]
    public async Task GetFilesForStudyAsync_NoImages_ReturnsEmptyList()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Arrange - No images added for this study
        await EnsureStudyExistsAsync(ctx, "STUDY-002");
        ctx.Images.Add(CreateSampleImage("STUDY-002", "/path2/image.dcm", "IMG-010"));
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetFilesForStudyAsync("STUDY-001");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilesForStudyAsync_NullStudyInstanceUid_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.GetFilesForStudyAsync(null!));
    }

    [Fact]
    public async Task GetFilesForStudyAsync_EmptyStudyInstanceUid_ReturnsEmptyList()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Act - Empty string is not null, returns empty result
        var result = await repo.GetFilesForStudyAsync(string.Empty);

        // Assert - No match for empty UID
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
