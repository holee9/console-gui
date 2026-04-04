using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using Xunit;

namespace HnVue.Common.Tests.Models;

/// <summary>
/// Coverage tests for remaining domain record/class models.
/// </summary>
public sealed class DomainModelsTests
{
    // ── UserRecord ────────────────────────────────────────────────────────────

    [Fact]
    public void UserRecord_SetsAllProperties()
    {
        var lastLogin = DateTimeOffset.UtcNow;
        var user = new UserRecord("u1", "alice", "Alice Smith", "$2b$12$hash", UserRole.Radiographer, 0, false, lastLogin);

        user.UserId.Should().Be("u1");
        user.Username.Should().Be("alice");
        user.DisplayName.Should().Be("Alice Smith");
        user.PasswordHash.Should().Be("$2b$12$hash");
        user.Role.Should().Be(UserRole.Radiographer);
        user.FailedLoginCount.Should().Be(0);
        user.IsLocked.Should().BeFalse();
        user.LastLoginAt.Should().Be(lastLogin);
    }

    [Fact]
    public void UserRecord_Locked_NullLastLogin()
    {
        var user = new UserRecord("u2", "bob", "Bob Jones", "$2b$12$hash2", UserRole.Service, 5, true, null);

        user.IsLocked.Should().BeTrue();
        user.FailedLoginCount.Should().Be(5);
        user.LastLoginAt.Should().BeNull();
    }

    // ── DoseRecord ────────────────────────────────────────────────────────────

    [Fact]
    public void DoseRecord_SetsAllProperties()
    {
        var ts = DateTimeOffset.UtcNow;
        var dose = new DoseRecord("d1", "1.2.3.4", 250.5, 1200.0, 0.15, "CHEST", ts);

        dose.DoseId.Should().Be("d1");
        dose.StudyInstanceUid.Should().Be("1.2.3.4");
        dose.Dap.Should().Be(250.5);
        dose.Ei.Should().Be(1200.0);
        dose.EffectiveDose.Should().Be(0.15);
        dose.BodyPart.Should().Be("CHEST");
        dose.RecordedAt.Should().Be(ts);
    }

    // ── WorklistItem ──────────────────────────────────────────────────────────

    [Fact]
    public void WorklistItem_SetsAllProperties()
    {
        var date = new DateOnly(2026, 4, 1);
        var item = new WorklistItem("ACC001", "P001", "Doe^John", date, "CHEST", "Chest PA");

        item.AccessionNumber.Should().Be("ACC001");
        item.PatientId.Should().Be("P001");
        item.PatientName.Should().Be("Doe^John");
        item.StudyDate.Should().Be(date);
        item.BodyPart.Should().Be("CHEST");
        item.RequestedProcedure.Should().Be("Chest PA");
    }

    [Fact]
    public void WorklistItem_OptionalFieldsAreNullable()
    {
        var item = new WorklistItem("ACC002", "P002", "Smith^Jane", null, null, null);

        item.StudyDate.Should().BeNull();
        item.BodyPart.Should().BeNull();
        item.RequestedProcedure.Should().BeNull();
    }

    // ── WorklistQuery ─────────────────────────────────────────────────────────

    [Fact]
    public void WorklistQuery_SetsAllProperties()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 12, 31);
        var query = new WorklistQuery("P001", from, to, "PACS_AE");

        query.PatientId.Should().Be("P001");
        query.DateFrom.Should().Be(from);
        query.DateTo.Should().Be(to);
        query.AeTitle.Should().Be("PACS_AE");
    }

    [Fact]
    public void WorklistQuery_AllPatientsQuery()
    {
        var query = new WorklistQuery(null, null, null, "PACS_AE");

        query.PatientId.Should().BeNull();
        query.DateFrom.Should().BeNull();
        query.DateTo.Should().BeNull();
    }

    // ── AuditQueryFilter ──────────────────────────────────────────────────────

    [Fact]
    public void AuditQueryFilter_DefaultValues()
    {
        var filter = new AuditQueryFilter();

        filter.UserId.Should().BeNull();
        filter.FromDate.Should().BeNull();
        filter.ToDate.Should().BeNull();
        filter.MaxResults.Should().Be(100);
    }

    [Fact]
    public void AuditQueryFilter_WithAllParameters()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;
        var filter = new AuditQueryFilter("u1", from, to, 50);

        filter.UserId.Should().Be("u1");
        filter.FromDate.Should().Be(from);
        filter.ToDate.Should().Be(to);
        filter.MaxResults.Should().Be(50);
    }

    // ── ProcessingParameters ──────────────────────────────────────────────────

    [Fact]
    public void ProcessingParameters_DefaultValues()
    {
        var p = new ProcessingParameters();

        p.WindowCenter.Should().BeNull();
        p.WindowWidth.Should().BeNull();
        p.AutoWindow.Should().BeTrue();
    }

    [Fact]
    public void ProcessingParameters_ManualWindow()
    {
        var p = new ProcessingParameters(2048.0, 4096.0, false);

        p.WindowCenter.Should().Be(2048.0);
        p.WindowWidth.Should().Be(4096.0);
        p.AutoWindow.Should().BeFalse();
    }

    // ── SystemSettings ────────────────────────────────────────────────────────

    [Fact]
    public void SystemSettings_DefaultValues()
    {
        var settings = new SystemSettings();

        settings.Dicom.PacsAeTitle.Should().BeEmpty();
        settings.Dicom.PacsHost.Should().BeEmpty();
        settings.Dicom.PacsPort.Should().Be(104);
        settings.Dicom.LocalAeTitle.Should().Be("HNVUE");
        settings.Generator.ComPort.Should().BeEmpty();
        settings.Generator.BaudRate.Should().Be(9600);
        settings.Generator.TimeoutMs.Should().Be(5000);
        settings.Security.SessionTimeoutMinutes.Should().Be(15);
        settings.Security.MaxFailedLogins.Should().Be(5);
    }

    [Fact]
    public void SystemSettings_CanConfigureDicom()
    {
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS01",
                PacsHost = "192.168.1.100",
                PacsPort = 11112,
                LocalAeTitle = "HNVUE_CONSOLE",
            }
        };

        settings.Dicom.PacsAeTitle.Should().Be("PACS01");
        settings.Dicom.PacsHost.Should().Be("192.168.1.100");
        settings.Dicom.PacsPort.Should().Be(11112);
        settings.Dicom.LocalAeTitle.Should().Be("HNVUE_CONSOLE");
    }

    [Fact]
    public void SystemSettings_CanConfigureGenerator()
    {
        var settings = new SystemSettings
        {
            Generator = new GeneratorSettings
            {
                ComPort = "COM3",
                BaudRate = 115200,
                TimeoutMs = 3000,
            }
        };

        settings.Generator.ComPort.Should().Be("COM3");
        settings.Generator.BaudRate.Should().Be(115200);
        settings.Generator.TimeoutMs.Should().Be(3000);
    }
}
