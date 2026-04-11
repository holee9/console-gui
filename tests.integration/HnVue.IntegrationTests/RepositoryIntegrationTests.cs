using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data;
using HnVue.Data.Entities;
using HnVue.Dose;
using HnVue.Incident;
using HnVue.Incident.Models;
using HnVue.PatientManagement;
using HnVue.SystemAdmin;
using HnVue.Update;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Integration tests for EF Core repository implementations (SPEC-COORDINATOR-001).
/// Uses real in-memory SQLite DbContext — no mocks.
/// REQ-COORD-007: Each repository has at least 1 integration test scenario.
/// </summary>
public sealed class RepositoryIntegrationTests : IDisposable
{
    private readonly HnVueDbContext _context;

    public RepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new HnVueDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    // ── T1: EfDoseRepository ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DM-051")]
    public async Task EfDoseRepository_SaveAndGetByStudy_ReturnsRecord()
    {
        // Arrange
        var studyUid = "1.2.3.4.5.6.789";
        var patientId = "P001";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "Test Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid,
            PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await _context.SaveChangesAsync();

        var repo = new EfDoseRepository(_context);
        var dose = new DoseRecord(
            DoseId: "DOSE-001",
            StudyInstanceUid: studyUid,
            Dap: 12.5,
            Ei: 1500.0,
            EffectiveDose: 0.35,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow);

        // Act
        var saveResult = await repo.SaveAsync(dose);
        var getResult = await repo.GetByStudyAsync(studyUid);

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().NotBeNull();
        getResult.Value!.DoseId.Should().Be("DOSE-001");
        getResult.Value.Dap.Should().Be(12.5);
        getResult.Value.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatient_ReturnsRecordsForPatient()
    {
        // Arrange
        var patientId = "P002";
        var studyUid = "1.2.3.4.5.6.999";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "Patient Two",
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid,
            PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await _context.SaveChangesAsync();

        var repo = new EfDoseRepository(_context);
        await repo.SaveAsync(new DoseRecord(
            DoseId: "DOSE-P002-1",
            StudyInstanceUid: studyUid,
            Dap: 5.0,
            Ei: 800.0,
            EffectiveDose: 0.1,
            BodyPart: "ABDOMEN",
            RecordedAt: DateTimeOffset.UtcNow));

        // Act
        var result = await repo.GetByPatientAsync(patientId, null, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].BodyPart.Should().Be("ABDOMEN");
    }

    // ── T2: EfWorklistRepository ──────────────────────────────────────────────

    [Fact]
    public async Task EfWorklistRepository_QueryToday_ReturnsTodaysStudies()
    {
        // Arrange
        var today = DateTimeOffset.UtcNow;
        var patientId = "WL-P001";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "Worklist Patient",
            CreatedAtTicks = today.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "WL-STUDY-001",
            PatientId = patientId,
            StudyDateTicks = today.UtcTicks,
            StudyDateOffsetMinutes = (int)today.Offset.TotalMinutes,
            AccessionNumber = "ACC-001",
            BodyPart = "CHEST",
            Description = "Chest PA",
        });
        await _context.SaveChangesAsync();

        var repo = new EfWorklistRepository(_context);

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().ContainSingle(x => x.AccessionNumber == "ACC-001");
    }

    [Fact]
    public async Task EfWorklistRepository_QueryToday_NoStudies_ReturnsEmptyList()
    {
        // Arrange — add a study with yesterday's date (should not appear in today's worklist)
        var yesterday = DateTimeOffset.UtcNow.AddDays(-1);
        var patientId = "WL-P002";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "Yesterday Patient",
            CreatedAtTicks = yesterday.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "WL-STUDY-YESTERDAY",
            PatientId = patientId,
            StudyDateTicks = yesterday.UtcTicks,
            StudyDateOffsetMinutes = (int)yesterday.Offset.TotalMinutes,
            AccessionNumber = "ACC-YEST",
            BodyPart = "KNEE",
            Description = "Knee AP",
        });
        await _context.SaveChangesAsync();

        var repo = new EfWorklistRepository(_context);

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── T3: EfIncidentRepository ──────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IN-010")]
    public async Task EfIncidentRepository_SaveAndGetBySeverity_ReturnsRecords()
    {
        // Arrange
        var repo = new EfIncidentRepository(_context);
        var incident = new IncidentRecord(
            IncidentId: "INC-001",
            OccurredAt: DateTimeOffset.UtcNow,
            ReportedByUserId: "user-001",
            Severity: IncidentSeverity.High,
            Category: "DOSE_EXCEEDED",
            Description: "Dose exceeded threshold during chest exam",
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

        // Act
        var saveResult = await repo.SaveAsync(incident);
        var getResult = await repo.GetBySeverityAsync(IncidentSeverity.High);

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().HaveCount(1);
        getResult.Value[0].IncidentId.Should().Be("INC-001");
        getResult.Value[0].Category.Should().Be("DOSE_EXCEEDED");
        getResult.Value[0].IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task EfIncidentRepository_Resolve_UpdatesRecord()
    {
        // Arrange
        var repo = new EfIncidentRepository(_context);
        var incident = new IncidentRecord(
            IncidentId: "INC-002",
            OccurredAt: DateTimeOffset.UtcNow,
            ReportedByUserId: "user-001",
            Severity: IncidentSeverity.Medium,
            Category: "HARDWARE_FAULT",
            Description: "Detector connection timeout",
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

        await repo.SaveAsync(incident);

        // Act
        var resolveResult = await repo.ResolveAsync("INC-002", "Reconnected detector cable");

        // Assert
        resolveResult.IsSuccess.Should().BeTrue();
        var records = await repo.GetBySeverityAsync(IncidentSeverity.Medium);
        records.Value.Should().HaveCount(1);
        records.Value[0].IsResolved.Should().BeTrue();
        records.Value[0].Resolution.Should().Be("Reconnected detector cable");
    }

    // ── T4: EfUpdateRepository ────────────────────────────────────────────────

    [Fact]
    public async Task EfUpdateRepository_CheckForUpdate_ReturnsLatestVersion()
    {
        // Arrange
        _context.UpdateHistories.Add(new UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow,
            FromVersion = "1.0.0",
            ToVersion = "1.1.0",
            Status = "Installed",
            InstalledBy = "admin",
            PackageHash = "abc123",
        });
        await _context.SaveChangesAsync();

        var repo = new EfUpdateRepository(_context);

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.1.0");
    }

    [Fact]
    public async Task EfUpdateRepository_CheckForUpdate_NoHistory_ReturnsNull()
    {
        // Arrange
        var repo = new EfUpdateRepository(_context);

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── T5: EfSystemSettingsRepository ────────────────────────────────────────

    [Fact]
    public async Task EfSystemSettingsRepository_Get_ReturnsDefaults()
    {
        // Arrange
        var repo = new EfSystemSettingsRepository(_context);

        // Act
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Dicom.LocalAeTitle.Should().Be("HNVUE");
        result.Value.Security.SessionTimeoutMinutes.Should().Be(15);
    }

    [Fact]
    public async Task EfSystemSettingsRepository_SaveAndGet_RoundTrips()
    {
        // Arrange
        var repo = new EfSystemSettingsRepository(_context);
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "TESTPACS",
                PacsHost = "192.168.1.100",
                PacsPort = 11112,
                LocalAeTitle = "TESTLOCAL",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM5",
                BaudRate = 115200,
                TimeoutMs = 3000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLogins = 3,
            },
        };

        // Act
        var saveResult = await repo.SaveAsync(settings);
        var getResult = await repo.GetAsync();

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Dicom.PacsAeTitle.Should().Be("TESTPACS");
        getResult.Value.Dicom.PacsPort.Should().Be(11112);
        getResult.Value.Generator.ComPort.Should().Be("COM5");
        getResult.Value.Generator.BaudRate.Should().Be(115200);
        getResult.Value.Security.SessionTimeoutMinutes.Should().Be(30);
        getResult.Value.Security.MaxFailedLogins.Should().Be(3);
    }

    // ── T6: EfCdStudyRepository (StudyRepository) ────────────────────────────

    [Fact]
    public async Task StudyRepository_GetFilesForStudy_ReturnsImagePaths()
    {
        // Arrange
        var studyUid = "CD-STUDY-001";
        var patientId = "CD-P001";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "CD Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid,
            PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await _context.SaveChangesAsync();

        _context.Images.AddRange(
            new ImageEntity
            {
                ImageId = "IMG-001",
                StudyInstanceUid = studyUid,
                FilePath = @"C:\Images\study1\img1.dcm",
                AcquiredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                AcquiredAtOffsetMinutes = 0,
            },
            new ImageEntity
            {
                ImageId = "IMG-002",
                StudyInstanceUid = studyUid,
                FilePath = @"C:\Images\study1\img2.dcm",
                AcquiredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                AcquiredAtOffsetMinutes = 0,
            });
        await _context.SaveChangesAsync();

        var repo = new StudyRepository(_context);

        // Act
        var result = await repo.GetFilesForStudyAsync(studyUid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(@"C:\Images\study1\img1.dcm");
        result.Value.Should().Contain(@"C:\Images\study1\img2.dcm");
    }

    [Fact]
    public async Task StudyRepository_GetFilesForStudy_NoImages_ReturnsEmptyList()
    {
        // Arrange
        var studyUid = "CD-STUDY-EMPTY";
        var patientId = "CD-P002";

        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId,
            Name = "CD Patient 2",
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "admin",
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid,
            PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await _context.SaveChangesAsync();

        var repo = new StudyRepository(_context);

        // Act
        var result = await repo.GetFilesForStudyAsync(studyUid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
