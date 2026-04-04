using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using Xunit;

namespace HnVue.Common.Tests.Models;

/// <summary>
/// Coverage tests for pure record/DTO models that have no executable logic
/// beyond property access. Verifies construction and value semantics.
/// </summary>
public sealed class RecordModelsTests
{
    // ── AuthenticationToken ───────────────────────────────────────────────────

    [Fact]
    public void AuthenticationToken_SetsAllProperties()
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(15);
        var token = new AuthenticationToken("u1", "alice", UserRole.Radiographer, "jwt.token.sig", expires);

        token.UserId.Should().Be("u1");
        token.Username.Should().Be("alice");
        token.Role.Should().Be(UserRole.Radiographer);
        token.Token.Should().Be("jwt.token.sig");
        token.ExpiresAt.Should().Be(expires);
    }

    [Fact]
    public void AuthenticationToken_ValueEquality()
    {
        var exp = DateTimeOffset.UtcNow;
        var a = new AuthenticationToken("u1", "alice", UserRole.Admin, "tok", exp);
        var b = new AuthenticationToken("u1", "alice", UserRole.Admin, "tok", exp);

        a.Should().Be(b);
    }

    // ── AuthenticatedUser ─────────────────────────────────────────────────────

    [Fact]
    public void AuthenticatedUser_SetsAllProperties()
    {
        var user = new AuthenticatedUser("u2", "bob", UserRole.Radiologist);

        user.UserId.Should().Be("u2");
        user.Username.Should().Be("bob");
        user.Role.Should().Be(UserRole.Radiologist);
    }

    // ── PatientRecord ─────────────────────────────────────────────────────────

    [Fact]
    public void PatientRecord_SetsAllProperties()
    {
        var dob = new DateOnly(1990, 5, 15);
        var created = DateTimeOffset.UtcNow;
        var patient = new PatientRecord("P001", "Doe^John", dob, "M", false, created, "usr-1");

        patient.PatientId.Should().Be("P001");
        patient.Name.Should().Be("Doe^John");
        patient.DateOfBirth.Should().Be(dob);
        patient.Sex.Should().Be("M");
        patient.IsEmergency.Should().BeFalse();
        patient.CreatedAt.Should().Be(created);
        patient.CreatedBy.Should().Be("usr-1");
    }

    [Fact]
    public void PatientRecord_Emergency_NullOptionalFields()
    {
        var patient = new PatientRecord("P002", "Unknown", null, null, true, DateTimeOffset.UtcNow, "usr-2");

        patient.DateOfBirth.Should().BeNull();
        patient.Sex.Should().BeNull();
        patient.IsEmergency.Should().BeTrue();
    }

    // ── StudyRecord ───────────────────────────────────────────────────────────

    [Fact]
    public void StudyRecord_SetsAllProperties()
    {
        var studyDate = DateTimeOffset.UtcNow;
        var study = new StudyRecord("1.2.3.4", "P001", studyDate, "Chest PA", "ACC001", "CHEST");

        study.StudyInstanceUid.Should().Be("1.2.3.4");
        study.PatientId.Should().Be("P001");
        study.StudyDate.Should().Be(studyDate);
        study.Description.Should().Be("Chest PA");
        study.AccessionNumber.Should().Be("ACC001");
        study.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public void StudyRecord_OptionalFieldsAreNullable()
    {
        var study = new StudyRecord("1.2.3.5", "P002", DateTimeOffset.UtcNow, null, null, null);

        study.Description.Should().BeNull();
        study.AccessionNumber.Should().BeNull();
        study.BodyPart.Should().BeNull();
    }

    // ── ExposureParameters ────────────────────────────────────────────────────

    [Fact]
    public void ExposureParameters_SetsAllProperties()
    {
        var ep = new ExposureParameters("CHEST", 120.0, 3.2, "1.2.3.6");

        ep.BodyPart.Should().Be("CHEST");
        ep.Kvp.Should().Be(120.0);
        ep.Mas.Should().Be(3.2);
        ep.StudyInstanceUid.Should().Be("1.2.3.6");
    }

    // ── UpdateInfo ────────────────────────────────────────────────────────────

    [Fact]
    public void UpdateInfo_SetsAllProperties()
    {
        var info = new UpdateInfo("2.1.0", "Bug fixes", "https://update.example.com/pkg", "abc123hash");

        info.Version.Should().Be("2.1.0");
        info.ReleaseNotes.Should().Be("Bug fixes");
        info.PackageUrl.Should().Be("https://update.example.com/pkg");
        info.Sha256Hash.Should().Be("abc123hash");
    }

    [Fact]
    public void UpdateInfo_ReleaseNotesIsOptional()
    {
        var info = new UpdateInfo("1.0.0", null, "https://update.example.com/pkg", "sha256xyz");

        info.ReleaseNotes.Should().BeNull();
    }

    // ── GeneratorStatus ───────────────────────────────────────────────────────

    [Fact]
    public void GeneratorStatus_DefaultValues()
    {
        var status = new GeneratorStatus();

        status.State.Should().Be(GeneratorState.Disconnected);
        status.HeatUnitPercentage.Should().Be(0.0);
        status.IsReadyToExpose.Should().BeFalse();
        status.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GeneratorStatus_InitProperties_OverrideDefaults()
    {
        var ts = DateTimeOffset.UtcNow;
        var status = new GeneratorStatus
        {
            State = GeneratorState.Ready,
            HeatUnitPercentage = 45.5,
            IsReadyToExpose = true,
            Timestamp = ts,
        };

        status.State.Should().Be(GeneratorState.Ready);
        status.HeatUnitPercentage.Should().Be(45.5);
        status.IsReadyToExpose.Should().BeTrue();
        status.Timestamp.Should().Be(ts);
    }
}
