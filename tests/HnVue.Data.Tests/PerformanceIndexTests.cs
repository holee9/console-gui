using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Data.Tests;

/// <summary>
/// Tests for database performance indexes (REQ-DATA-003).
/// Verifies that required indexes are configured in DbContext.
/// </summary>
public sealed class PerformanceIndexTests
{
    [Fact]
    public void PatientEntity_HasCompositeIndex_OnNameAndIsDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        // Act
        using var ctx = new HnVueDbContext(options);
        var model = ctx.Model;
        var patientEntity = model.FindEntityType(typeof(PatientEntity));

        // Assert
        Assert.NotNull(patientEntity);

        var indexes = patientEntity.GetIndexes();
        var nameIsDeletedIndex = indexes.FirstOrDefault(idx =>
            idx.Properties.Count == 2 &&
            idx.Properties.Any(p => p.Name == "Name") &&
            idx.Properties.Any(p => p.Name == "IsDeleted"));

        Assert.NotNull(nameIsDeletedIndex);
    }

    [Fact]
    public void StudyEntity_HasIndex_OnStudyDateTicks()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        // Act
        using var ctx = new HnVueDbContext(options);
        var model = ctx.Model;
        var studyEntity = model.FindEntityType(typeof(StudyEntity));

        // Assert
        Assert.NotNull(studyEntity);

        var indexes = studyEntity.GetIndexes();
        var studyDateTicksIndex = indexes.FirstOrDefault(idx =>
            idx.Properties.Count == 1 &&
            idx.Properties.Any(p => p.Name == "StudyDateTicks"));

        Assert.NotNull(studyDateTicksIndex);
    }

    [Fact]
    public void DoseRecordEntity_HasCompositeIndex_OnStudyInstanceUidAndRecordedAtTicks()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        // Act
        using var ctx = new HnVueDbContext(options);
        var model = ctx.Model;
        var doseRecordEntity = model.FindEntityType(typeof(DoseRecordEntity));

        // Assert
        Assert.NotNull(doseRecordEntity);

        var indexes = doseRecordEntity.GetIndexes();
        var studyInstanceUidRecordedAtTicksIndex = indexes.FirstOrDefault(idx =>
            idx.Properties.Count == 2 &&
            idx.Properties.Any(p => p.Name == "StudyInstanceUid") &&
            idx.Properties.Any(p => p.Name == "RecordedAtTicks"));

        Assert.NotNull(studyInstanceUidRecordedAtTicksIndex);
    }

    [Fact]
    public void AuditLogEntity_HasIndex_OnTimestampTicks()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        // Act
        using var ctx = new HnVueDbContext(options);
        var model = ctx.Model;
        var auditLogEntity = model.FindEntityType(typeof(AuditLogEntity));

        // Assert
        Assert.NotNull(auditLogEntity);

        var indexes = auditLogEntity.GetIndexes();
        var timestampTicksIndex = indexes.FirstOrDefault(idx =>
            idx.Properties.Count == 1 &&
            idx.Properties.Any(p => p.Name == "TimestampTicks"));

        Assert.NotNull(timestampTicksIndex);
    }
}
