using System.Globalization;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;

namespace HnVue.Data.Mappers;

/// <summary>
/// Provides static conversion methods between EF Core entities and HnVue.Common domain records.
/// All methods are pure functions with no side effects.
/// </summary>
internal static class EntityMapper
{
    // ── PatientRecord ──────────────────────────────────────────────────────────

    /// <summary>Converts a <see cref="PatientEntity"/> to a <see cref="PatientRecord"/>.</summary>
    internal static PatientRecord ToRecord(PatientEntity entity) =>
        new(
            PatientId: entity.PatientId,
            Name: entity.Name,
            DateOfBirth: entity.DateOfBirth is null
                ? null
                : DateOnly.ParseExact(entity.DateOfBirth, "yyyy-MM-dd"),
            Sex: entity.Sex,
            IsEmergency: entity.IsEmergency,
            CreatedAt: new DateTimeOffset(entity.CreatedAtTicks, TimeSpan.FromMinutes(entity.CreatedAtOffsetMinutes)),
            CreatedBy: entity.CreatedBy);

    /// <summary>Converts a <see cref="PatientRecord"/> to a <see cref="PatientEntity"/>.</summary>
    internal static PatientEntity ToEntity(PatientRecord record) =>
        new()
        {
            PatientId = record.PatientId,
            Name = record.Name,
            DateOfBirth = record.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Sex = record.Sex,
            IsEmergency = record.IsEmergency,
            CreatedAtTicks = record.CreatedAt.UtcTicks,
            CreatedAtOffsetMinutes = (int)record.CreatedAt.Offset.TotalMinutes,
            CreatedBy = record.CreatedBy,
        };

    /// <summary>Updates an existing <see cref="PatientEntity"/> from a <see cref="PatientRecord"/>.</summary>
    internal static void ApplyUpdate(PatientEntity entity, PatientRecord record)
    {
        entity.Name = record.Name;
        entity.DateOfBirth = record.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        entity.Sex = record.Sex;
        entity.IsEmergency = record.IsEmergency;
    }

    // ── StudyRecord ────────────────────────────────────────────────────────────

    /// <summary>Converts a <see cref="StudyEntity"/> to a <see cref="StudyRecord"/>.</summary>
    internal static StudyRecord ToRecord(StudyEntity entity) =>
        new(
            StudyInstanceUid: entity.StudyInstanceUid,
            PatientId: entity.PatientId,
            StudyDate: new DateTimeOffset(entity.StudyDateTicks, TimeSpan.FromMinutes(entity.StudyDateOffsetMinutes)),
            Description: entity.Description,
            AccessionNumber: entity.AccessionNumber,
            BodyPart: entity.BodyPart);

    /// <summary>Converts a <see cref="StudyRecord"/> to a <see cref="StudyEntity"/>.</summary>
    internal static StudyEntity ToEntity(StudyRecord record) =>
        new()
        {
            StudyInstanceUid = record.StudyInstanceUid,
            PatientId = record.PatientId,
            StudyDateTicks = record.StudyDate.UtcTicks,
            StudyDateOffsetMinutes = (int)record.StudyDate.Offset.TotalMinutes,
            Description = record.Description,
            AccessionNumber = record.AccessionNumber,
            BodyPart = record.BodyPart,
        };

    /// <summary>Updates an existing <see cref="StudyEntity"/> from a <see cref="StudyRecord"/>.</summary>
    internal static void ApplyUpdate(StudyEntity entity, StudyRecord record)
    {
        entity.StudyDateTicks = record.StudyDate.UtcTicks;
        entity.StudyDateOffsetMinutes = (int)record.StudyDate.Offset.TotalMinutes;
        entity.Description = record.Description;
        entity.AccessionNumber = record.AccessionNumber;
        entity.BodyPart = record.BodyPart;
    }

    // ── UserRecord ─────────────────────────────────────────────────────────────

    /// <summary>Converts a <see cref="UserEntity"/> to a <see cref="UserRecord"/>.</summary>
    internal static UserRecord ToRecord(UserEntity entity) =>
        new(
            UserId: entity.UserId,
            Username: entity.Username,
            DisplayName: entity.DisplayName,
            PasswordHash: entity.PasswordHash,
            Role: (UserRole)entity.RoleValue,
            FailedLoginCount: entity.FailedLoginCount,
            IsLocked: entity.IsLocked,
            LastLoginAt: entity.LastLoginAtTicks.HasValue
                ? new DateTimeOffset(entity.LastLoginAtTicks.Value, TimeSpan.FromMinutes(entity.LastLoginAtOffsetMinutes ?? 0))
                : null,
            QuickPinHash: entity.QuickPinHash,
            QuickPinFailedCount: entity.QuickPinFailedCount,
            QuickPinLockedUntil: entity.QuickPinLockedUntilTicks.HasValue
                ? new DateTimeOffset(entity.QuickPinLockedUntilTicks.Value, TimeSpan.Zero)
                : null);

    /// <summary>Converts a <see cref="UserRecord"/> to a <see cref="UserEntity"/>.</summary>
    internal static UserEntity ToEntity(UserRecord record) =>
        new()
        {
            UserId = record.UserId,
            Username = record.Username,
            DisplayName = record.DisplayName,
            PasswordHash = record.PasswordHash,
            RoleValue = (int)record.Role,
            FailedLoginCount = record.FailedLoginCount,
            IsLocked = record.IsLocked,
            LastLoginAtTicks = record.LastLoginAt?.UtcTicks,
            LastLoginAtOffsetMinutes = record.LastLoginAt.HasValue
                ? (int)record.LastLoginAt.Value.Offset.TotalMinutes
                : null,
        };

    // ── AuditEntry ─────────────────────────────────────────────────────────────

    /// <summary>Converts an <see cref="AuditLogEntity"/> to an <see cref="AuditEntry"/>.</summary>
    internal static AuditEntry ToRecord(AuditLogEntity entity) =>
        new(
            EntryId: entity.EntryId,
            Timestamp: new DateTimeOffset(entity.TimestampTicks, TimeSpan.FromMinutes(entity.TimestampOffsetMinutes)),
            UserId: entity.UserId,
            Action: entity.Action,
            Details: entity.Details,
            PreviousHash: entity.PreviousHash,
            CurrentHash: entity.CurrentHash);

    /// <summary>Converts an <see cref="AuditEntry"/> to an <see cref="AuditLogEntity"/>.</summary>
    internal static AuditLogEntity ToEntity(AuditEntry record) =>
        new()
        {
            EntryId = record.EntryId,
            TimestampTicks = record.Timestamp.UtcTicks,
            TimestampOffsetMinutes = (int)record.Timestamp.Offset.TotalMinutes,
            UserId = record.UserId,
            Action = record.Action,
            Details = record.Details,
            PreviousHash = record.PreviousHash,
            CurrentHash = record.CurrentHash,
        };
}
