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
        string filePath = "/data/images/image.dcm") =>
        new()
        {
            StudyInstanceUid = studyInstanceUid,
            FilePath = filePath
        };

    // ── GetFilesForStudyAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetFilesForStudyAsync_ExistingStudy_ReturnsFilePaths()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfCdStudyRepository(ctx);

        // Arrange
        ctx.Images.Add(CreateSampleImage("STUDY-001", "/path1/image1.dcm"));
        ctx.Images.Add(CreateSampleImage("STUDY-001", "/path1/image2.dcm"));
        ctx.Images.Add(CreateSampleImage("STUDY-002", "/path2/image3.dcm"));
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
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfCdStudyRepository(ctx);

        // Arrange - No images added for this study
        ctx.Images.Add(CreateSampleImage("STUDY-002", "/path2/image.dcm"));
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
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfCdStudyRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.GetFilesForStudyAsync(null!));
    }

    [Fact]
    public async Task GetFilesForStudyAsync_EmptyStudyInstanceUid_ThrowsArgumentNullException()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfCdStudyRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.GetFilesForStudyAsync(string.Empty));
    }
}
