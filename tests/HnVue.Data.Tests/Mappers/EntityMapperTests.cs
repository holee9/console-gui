using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Mappers;

namespace HnVue.Data.Tests.Mappers;

/// <summary>
/// Unit tests for <see cref="EntityMapper"/> covering all domain record ↔ entity conversions.
/// </summary>
public sealed class EntityMapperTests
{
    // ── PatientRecord ──────────────────────────────────────────────────────────

    [Fact]
    public void ToRecord_PatientEntity_MapsAllFields()
    {
        var entity = new PatientEntity
        {
            PatientId = "P001",
            Name = "Doe^John",
            DateOfBirth = "1980-06-15",
            Sex = "M",
            IsEmergency = false,
            CreatedAtTicks = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "user-01",
        };

        var record = EntityMapper.ToRecord(entity);

        record.PatientId.Should().Be("P001");
        record.Name.Should().Be("Doe^John");
        record.DateOfBirth.Should().Be(new DateOnly(1980, 6, 15));
        record.Sex.Should().Be("M");
        record.IsEmergency.Should().BeFalse();
        record.CreatedBy.Should().Be("user-01");
    }

    [Fact]
    public void ToRecord_PatientEntity_NullDateOfBirth_ReturnsNull()
    {
        var entity = new PatientEntity
        {
            PatientId = "P002",
            Name = "Smith^Jane",
            DateOfBirth = null,
            Sex = null,
            IsEmergency = true,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "user-02",
        };

        var record = EntityMapper.ToRecord(entity);

        record.DateOfBirth.Should().BeNull();
        record.Sex.Should().BeNull();
        record.IsEmergency.Should().BeTrue();
    }

    [Fact]
    public void ToEntity_PatientRecord_MapsAllFields()
    {
        var record = new PatientRecord(
            "P001", "Doe^John", new DateOnly(1980, 6, 15), "M", false,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "user-01");

        var entity = EntityMapper.ToEntity(record);

        entity.PatientId.Should().Be("P001");
        entity.Name.Should().Be("Doe^John");
        entity.DateOfBirth.Should().Be("1980-06-15");
        entity.Sex.Should().Be("M");
        entity.IsEmergency.Should().BeFalse();
        entity.CreatedBy.Should().Be("user-01");
    }

    [Fact]
    public void ToEntity_PatientRecord_NullDateOfBirth_StoresNull()
    {
        var record = new PatientRecord("P002", "Smith^Jane", null, null, true,
            DateTimeOffset.UtcNow, "user-02");

        var entity = EntityMapper.ToEntity(record);

        entity.DateOfBirth.Should().BeNull();
        entity.Sex.Should().BeNull();
    }

    [Fact]
    public void ApplyUpdate_PatientEntity_UpdatesFields()
    {
        var entity = new PatientEntity
        {
            PatientId = "P001",
            Name = "Old^Name",
            DateOfBirth = "1970-01-01",
            Sex = "M",
            IsEmergency = false,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "user-01",
        };
        var record = new PatientRecord("P001", "New^Name", new DateOnly(1985, 3, 20), "F", true,
            DateTimeOffset.UtcNow, "user-01");

        EntityMapper.ApplyUpdate(entity, record);

        entity.Name.Should().Be("New^Name");
        entity.DateOfBirth.Should().Be("1985-03-20");
        entity.Sex.Should().Be("F");
        entity.IsEmergency.Should().BeTrue();
    }

    // ── StudyRecord ────────────────────────────────────────────────────────────

    [Fact]
    public void ToRecord_StudyEntity_MapsAllFields()
    {
        var ts = new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero);
        var entity = new StudyEntity
        {
            StudyInstanceUid = "1.2.3",
            PatientId = "P001",
            StudyDateTicks = ts.UtcTicks,
            StudyDateOffsetMinutes = 0,
            Description = "Chest PA",
            AccessionNumber = "ACC001",
            BodyPart = "CHEST",
        };

        var record = EntityMapper.ToRecord(entity);

        record.StudyInstanceUid.Should().Be("1.2.3");
        record.PatientId.Should().Be("P001");
        record.Description.Should().Be("Chest PA");
        record.AccessionNumber.Should().Be("ACC001");
        record.BodyPart.Should().Be("CHEST");
        record.StudyDate.UtcTicks.Should().Be(ts.UtcTicks);
    }

    [Fact]
    public void ToEntity_StudyRecord_MapsAllFields()
    {
        var ts = new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero);
        var record = new StudyRecord("1.2.3", "P001", ts, "Chest PA", "ACC001", "CHEST");

        var entity = EntityMapper.ToEntity(record);

        entity.StudyInstanceUid.Should().Be("1.2.3");
        entity.PatientId.Should().Be("P001");
        entity.Description.Should().Be("Chest PA");
        entity.AccessionNumber.Should().Be("ACC001");
        entity.BodyPart.Should().Be("CHEST");
        entity.StudyDateTicks.Should().Be(ts.UtcTicks);
    }

    [Fact]
    public void ApplyUpdate_StudyEntity_UpdatesFields()
    {
        var entity = new StudyEntity
        {
            StudyInstanceUid = "1.2.3",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        };
        var ts = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);
        var record = new StudyRecord("1.2.3", "P001", ts, "Hand AP", "ACC999", "HAND");

        EntityMapper.ApplyUpdate(entity, record);

        entity.Description.Should().Be("Hand AP");
        entity.AccessionNumber.Should().Be("ACC999");
        entity.BodyPart.Should().Be("HAND");
        entity.StudyDateTicks.Should().Be(ts.UtcTicks);
    }

    // ── UserRecord ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToRecord_UserEntity_MapsAllFields()
    {
        var lastLoginTs = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var entity = new UserEntity
        {
            UserId = "U001",
            Username = "admin",
            DisplayName = "Admin User",
            PasswordHash = "$2b$12$hash",
            RoleValue = (int)UserRole.Admin,
            FailedLoginCount = 2,
            IsLocked = false,
            LastLoginAtTicks = lastLoginTs.UtcTicks,
            LastLoginAtOffsetMinutes = 0,
        };

        var record = EntityMapper.ToRecord(entity);

        record.UserId.Should().Be("U001");
        record.Username.Should().Be("admin");
        record.DisplayName.Should().Be("Admin User");
        record.Role.Should().Be(UserRole.Admin);
        record.FailedLoginCount.Should().Be(2);
        record.IsLocked.Should().BeFalse();
        record.LastLoginAt.Should().NotBeNull();
        record.LastLoginAt!.Value.UtcTicks.Should().Be(lastLoginTs.UtcTicks);
    }

    [Fact]
    public void ToRecord_UserEntity_NullLastLoginAt_ReturnsNull()
    {
        var entity = new UserEntity
        {
            UserId = "U001",
            Username = "admin",
            DisplayName = "Admin User",
            PasswordHash = "$2b$12$hash",
            RoleValue = (int)UserRole.Radiographer,
            FailedLoginCount = 0,
            IsLocked = false,
            LastLoginAtTicks = null,
            LastLoginAtOffsetMinutes = null,
        };

        var record = EntityMapper.ToRecord(entity);

        record.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void ToEntity_UserRecord_MapsAllFields()
    {
        var lastLogin = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var record = new UserRecord("U001", "admin", "Admin User", "$2b$12$hash",
            UserRole.Service, 1, true, lastLogin);

        var entity = EntityMapper.ToEntity(record);

        entity.UserId.Should().Be("U001");
        entity.Username.Should().Be("admin");
        entity.RoleValue.Should().Be((int)UserRole.Service);
        entity.FailedLoginCount.Should().Be(1);
        entity.IsLocked.Should().BeTrue();
        entity.LastLoginAtTicks.Should().Be(lastLogin.UtcTicks);
        entity.LastLoginAtOffsetMinutes.Should().Be(0);
    }

    [Fact]
    public void ToEntity_UserRecord_NullLastLoginAt_StoresNull()
    {
        var record = new UserRecord("U001", "admin", "Admin", "$2b$12$hash",
            UserRole.Radiologist, 0, false, null);

        var entity = EntityMapper.ToEntity(record);

        entity.LastLoginAtTicks.Should().BeNull();
        entity.LastLoginAtOffsetMinutes.Should().BeNull();
    }

    // ── AuditEntry ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToRecord_AuditLogEntity_MapsAllFields()
    {
        var ts = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var entity = new AuditLogEntity
        {
            EntryId = "entry-001",
            TimestampTicks = ts.UtcTicks,
            TimestampOffsetMinutes = 0,
            UserId = "U001",
            Action = "LOGIN",
            Details = "{\"ip\":\"127.0.0.1\"}",
            PreviousHash = "prev-hash",
            CurrentHash = "curr-hash",
        };

        var record = EntityMapper.ToRecord(entity);

        record.EntryId.Should().Be("entry-001");
        record.UserId.Should().Be("U001");
        record.Action.Should().Be("LOGIN");
        record.Details.Should().Be("{\"ip\":\"127.0.0.1\"}");
        record.PreviousHash.Should().Be("prev-hash");
        record.CurrentHash.Should().Be("curr-hash");
        record.Timestamp.UtcTicks.Should().Be(ts.UtcTicks);
    }

    [Fact]
    public void ToEntity_AuditEntry_MapsAllFields()
    {
        var ts = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var record = new AuditEntry("entry-001", ts, "U001", "EXPOSE",
            "{\"dose\":0.5}", "prev-hash", "curr-hash");

        var entity = EntityMapper.ToEntity(record);

        entity.EntryId.Should().Be("entry-001");
        entity.UserId.Should().Be("U001");
        entity.Action.Should().Be("EXPOSE");
        entity.Details.Should().Be("{\"dose\":0.5}");
        entity.PreviousHash.Should().Be("prev-hash");
        entity.CurrentHash.Should().Be("curr-hash");
        entity.TimestampTicks.Should().Be(ts.UtcTicks);
    }
}
