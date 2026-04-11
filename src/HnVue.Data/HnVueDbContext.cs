using HnVue.Common.Abstractions;
using HnVue.Data.Converters;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data;

// @MX:ANCHOR HnVueDbContext - @MX:REASON: EF Core context with 6 DbSets, IEC 62304 cascade rules for dose records; PHI value converters applied per REQ-PHI-003
/// <summary>
/// EF Core database context for the HnVue console application.
/// Uses SQLite with optional SQLCipher AES-256 encryption.
/// Pass the encryption key via the connection string: <c>Data Source=hnvue.db;Password=&lt;key&gt;</c>.
/// PHI fields on PatientEntity are encrypted at rest via <see cref="PhiEncryptedValueConverter"/> (REQ-PHI-003, SWR-CS-080).
/// </summary>
public sealed class HnVueDbContext : DbContext
{
    private readonly IPhiEncryptionService? _phiEncryptionService;

    /// <summary>Initializes the context without PHI encryption (development/migration use).</summary>
    public HnVueDbContext(DbContextOptions<HnVueDbContext> options) : base(options) { }

    /// <summary>
    /// Initializes the context with PHI column-level encryption (REQ-PHI-003).
    /// </summary>
    /// <param name="options">EF Core context options.</param>
    /// <param name="phiEncryptionService">Service used to encrypt/decrypt PHI fields.</param>
    public HnVueDbContext(DbContextOptions<HnVueDbContext> options, IPhiEncryptionService phiEncryptionService)
        : base(options)
    {
        _phiEncryptionService = phiEncryptionService;
    }

    /// <summary>Gets the patient demographic records.</summary>
    public DbSet<PatientEntity> Patients => Set<PatientEntity>();

    /// <summary>Gets the DICOM study records.</summary>
    public DbSet<StudyEntity> Studies => Set<StudyEntity>();

    /// <summary>Gets the acquired image file references.</summary>
    public DbSet<ImageEntity> Images => Set<ImageEntity>();

    /// <summary>Gets the radiation dose records.</summary>
    public DbSet<DoseRecordEntity> DoseRecords => Set<DoseRecordEntity>();

    /// <summary>Gets the user account records.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>Gets the tamper-evident audit log entries.</summary>
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    /// <summary>Gets the software update installation history records.</summary>
    public DbSet<UpdateHistoryEntity> UpdateHistories => Set<UpdateHistoryEntity>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── PatientEntity ──────────────────────────────────────────────────────
        modelBuilder.Entity<PatientEntity>(e =>
        {
            e.HasKey(p => p.PatientId);
            e.HasIndex(p => p.Name);
            // REQ-DATA-003: Composite index on (Name, IsDeleted) for search performance
            e.HasIndex(p => new { p.Name, p.IsDeleted });

            // REQ-PHI-003: PHI field column-level encryption (SWR-CS-080, SPEC-INFRA-002)
            // Apply value converters to encrypt Name, DateOfBirth, and PatientId when persisting.
            if (_phiEncryptionService is not null)
            {
                var phiConverter = new PhiEncryptedValueConverter(_phiEncryptionService);
                var nullablePhiConverter = new NullablePhiEncryptedValueConverter(_phiEncryptionService);

                e.Property(p => p.Name).HasConversion(phiConverter);
                e.Property(p => p.DateOfBirth).HasConversion(nullablePhiConverter);
                e.Property(p => p.PatientId).HasConversion(phiConverter);
            }

            // IEC 62304 / IEC 62133 data integrity: Restrict prevents accidental
            // deletion of audit-critical dose records when a patient record is removed.
            // Callers must explicitly delete or reassign studies before removing a patient.
            e.HasMany(p => p.Studies)
             .WithOne(s => s.Patient)
             .HasForeignKey(s => s.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── StudyEntity ────────────────────────────────────────────────────────
        modelBuilder.Entity<StudyEntity>(e =>
        {
            e.HasKey(s => s.StudyInstanceUid);
            e.HasIndex(s => s.PatientId);
            // REQ-DATA-003: Index on StudyDateTicks for date-based queries
            e.HasIndex(s => s.StudyDateTicks);

            // Images are non-regulatory data; cascade is acceptable here.
            e.HasMany(s => s.Images)
             .WithOne(i => i.Study)
             .HasForeignKey(i => i.StudyInstanceUid)
             .OnDelete(DeleteBehavior.Cascade);

            // DoseRecords are regulatory audit data (SWR-NF-SC-041).
            // Restrict prevents permanent loss of dose records when a study is deleted.
            // Callers must archive or explicitly delete dose records before removing a study.
            e.HasMany(s => s.DoseRecords)
             .WithOne(d => d.Study)
             .HasForeignKey(d => d.StudyInstanceUid)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ImageEntity ────────────────────────────────────────────────────────
        modelBuilder.Entity<ImageEntity>(e =>
        {
            e.HasKey(i => i.ImageId);
            e.HasIndex(i => i.StudyInstanceUid);
        });

        // ── DoseRecordEntity ───────────────────────────────────────────────────
        modelBuilder.Entity<DoseRecordEntity>(e =>
        {
            e.HasKey(d => d.DoseId);
            e.HasIndex(d => d.StudyInstanceUid);
            // REQ-DATA-003: Composite index on (StudyInstanceUid, RecordedAtTicks) for dose queries
            e.HasIndex(d => new { d.StudyInstanceUid, d.RecordedAtTicks });
        });

        // ── UserEntity ─────────────────────────────────────────────────────────
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasKey(u => u.UserId);
            e.HasIndex(u => u.Username).IsUnique();
        });

        // ── AuditLogEntity ─────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLogEntity>(e =>
        {
            e.HasKey(a => a.EntryId);
            e.HasIndex(a => a.TimestampTicks);
            e.HasIndex(a => a.UserId);
        });

        // ── UpdateHistoryEntity ─────────────────────────────────────────────────
        modelBuilder.Entity<UpdateHistoryEntity>(e =>
        {
            e.HasKey(u => u.UpdateId);
            e.HasIndex(u => u.Timestamp);
            e.HasIndex(u => u.FromVersion);
            e.HasIndex(u => u.ToVersion);
        });
    }
}
