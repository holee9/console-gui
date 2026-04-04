using HnVue.Data.Entities;

namespace HnVue.Data.Tests.EntityTests;

/// <summary>
/// Ensures all entity classes can be instantiated and their properties set correctly.
/// These tests primarily serve to achieve coverage on entity property setters/getters
/// that are not exercised through the repository layer.
/// </summary>
public sealed class EntityInstantiationTests
{
    [Fact]
    public void DoseRecordEntity_PropertiesSetAndGet()
    {
        var entity = new DoseRecordEntity
        {
            DoseId = "D001",
            StudyInstanceUid = "1.2.3",
            Dap = 25.5,
            Ei = 300.0,
            EffectiveDose = 0.3,
            BodyPart = "CHEST",
            RecordedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            RecordedAtOffsetMinutes = 0,
        };

        entity.DoseId.Should().Be("D001");
        entity.StudyInstanceUid.Should().Be("1.2.3");
        entity.Dap.Should().Be(25.5);
        entity.Ei.Should().Be(300.0);
        entity.EffectiveDose.Should().Be(0.3);
        entity.BodyPart.Should().Be("CHEST");
        entity.Study.Should().BeNull();
    }

    [Fact]
    public void ImageEntity_PropertiesSetAndGet()
    {
        var entity = new ImageEntity
        {
            ImageId = "IMG001",
            StudyInstanceUid = "1.2.3",
            FilePath = @"C:\images\img001.dcm",
            AcquiredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            AcquiredAtOffsetMinutes = 0,
        };

        entity.ImageId.Should().Be("IMG001");
        entity.StudyInstanceUid.Should().Be("1.2.3");
        entity.FilePath.Should().Be(@"C:\images\img001.dcm");
        entity.Study.Should().BeNull();
    }

    [Fact]
    public void DoseRecordEntity_CanBeSavedAndQueried()
    {
        using var ctx = TestDbContextFactory.Create();

        // First add a patient and study (FK constraints)
        ctx.Patients.Add(new PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            IsEmergency = false,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "user-01",
        });
        ctx.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "1.2.3",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        ctx.DoseRecords.Add(new DoseRecordEntity
        {
            DoseId = "D001",
            StudyInstanceUid = "1.2.3",
            Dap = 25.5,
            Ei = 300.0,
            EffectiveDose = 0.3,
            BodyPart = "CHEST",
            RecordedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            RecordedAtOffsetMinutes = 0,
        });
        ctx.SaveChanges();

        ctx.DoseRecords.Should().HaveCount(1);
        ctx.DoseRecords.First().DoseId.Should().Be("D001");
    }

    [Fact]
    public void ImageEntity_CanBeSavedAndQueried()
    {
        using var ctx = TestDbContextFactory.Create();

        ctx.Patients.Add(new PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            IsEmergency = false,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "user-01",
        });
        ctx.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "1.2.3",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        ctx.Images.Add(new ImageEntity
        {
            ImageId = "IMG001",
            StudyInstanceUid = "1.2.3",
            FilePath = @"C:\images\img001.dcm",
            AcquiredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            AcquiredAtOffsetMinutes = 0,
        });
        ctx.SaveChanges();

        ctx.Images.Should().HaveCount(1);
        ctx.Images.First().ImageId.Should().Be("IMG001");
    }
}
