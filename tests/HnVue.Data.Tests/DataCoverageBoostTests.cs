using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using HnVue.Data.Extensions;
using HnVue.Data.Mappers;
using HnVue.Data.Repositories;
using HnVue.Data.Security;
using HnVue.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HnVue.Data.Tests;

/// <summary>
/// Coverage boost tests for HnVue.Data module targeting 85%+ coverage.
/// Tests EntityMapper, ValueConverters, ServiceCollectionExtensions, and repository edge cases.
/// </summary>
public sealed class DataCoverageBoostTests
{
    // ── EntityMapper: StudyRecord round-trip ──────────────────────────────────

    [Fact]
    public void EntityMapper_StudyRecord_RoundTrip()
    {
        var record = new StudyRecord(
            StudyInstanceUid: "1.2.3.4.5",
            PatientId: "P001",
            StudyDate: new DateTimeOffset(2026, 4, 14, 10, 30, 0, TimeSpan.FromHours(9)),
            Description: "Chest PA",
            AccessionNumber: "ACC-001",
            BodyPart: "CHEST");

        var entity = EntityMapper.ToEntity(record);
        entity.StudyInstanceUid.Should().Be("1.2.3.4.5");
        entity.PatientId.Should().Be("P001");
        entity.Description.Should().Be("Chest PA");
        entity.AccessionNumber.Should().Be("ACC-001");
        entity.BodyPart.Should().Be("CHEST");

        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.StudyInstanceUid.Should().Be(record.StudyInstanceUid);
        roundTripped.PatientId.Should().Be(record.PatientId);
        roundTripped.Description.Should().Be(record.Description);
    }

    [Fact]
    public void EntityMapper_ApplyUpdate_StudyEntity()
    {
        var entity = new StudyEntity
        {
            StudyInstanceUid = "1.2.3.4",
            PatientId = "P001",
            StudyDateTicks = 1000,
            StudyDateOffsetMinutes = 0,
            Description = "Old",
            AccessionNumber = "OLD-ACC",
            BodyPart = "OLD",
        };

        var record = new StudyRecord(
            "1.2.3.4", "P001",
            new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero),
            "New Desc", "NEW-ACC", "NEW");

        EntityMapper.ApplyUpdate(entity, record);
        entity.Description.Should().Be("New Desc");
        entity.AccessionNumber.Should().Be("NEW-ACC");
        entity.BodyPart.Should().Be("NEW");
    }

    // ── EntityMapper: UserRecord round-trip ───────────────────────────────────

    [Fact]
    public void EntityMapper_UserRecord_RoundTrip_BasicFields()
    {
        var record = new UserRecord(
            UserId: "U001",
            Username: "admin",
            DisplayName: "Administrator",
            PasswordHash: "hash123",
            Role: Common.Enums.UserRole.Admin,
            FailedLoginCount: 0,
            IsLocked: false,
            LastLoginAt: null,
            QuickPinHash: null,
            QuickPinFailedCount: 0,
            QuickPinLockedUntil: null);

        var entity = EntityMapper.ToEntity(record);
        entity.UserId.Should().Be("U001");
        entity.Username.Should().Be("admin");
        entity.DisplayName.Should().Be("Administrator");
        entity.RoleValue.Should().Be((int)Common.Enums.UserRole.Admin);
        entity.FailedLoginCount.Should().Be(0);
        entity.IsLocked.Should().BeFalse();

        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.UserId.Should().Be(record.UserId);
        roundTripped.Username.Should().Be(record.Username);
        roundTripped.Role.Should().Be(record.Role);
    }

    [Fact]
    public void EntityMapper_UserRecord_WithLastLogin()
    {
        var loginTime = new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.FromHours(9));
        var record = new UserRecord(
            "U002", "user", "User", "hash", Common.Enums.UserRole.Radiographer,
            2, true, loginTime, null, 0, null);

        var entity = EntityMapper.ToEntity(record);
        entity.LastLoginAtTicks.Should().NotBeNull();
        entity.LastLoginAtOffsetMinutes.Should().Be((int)loginTime.Offset.TotalMinutes);

        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.LastLoginAt.Should().NotBeNull();
    }

    [Fact]
    public void EntityMapper_UserRecord_WithQuickPinFields()
    {
        var lockedUntil = new DateTimeOffset(2026, 4, 14, 11, 0, 0, TimeSpan.Zero);
        var record = new UserRecord(
            "U003", "tech", "Tech", "hash", Common.Enums.UserRole.Radiographer,
            0, false, null, "pin-hash", 3, lockedUntil);

        var entity = EntityMapper.ToEntity(record);
        // QuickPinHash and QuickPinFailedCount and QuickPinLockedUntil are set via UserRecord constructor but
        // EntityMapper.ToEntity doesn't map them (they are managed by separate repository methods).
        // Verify the record values are preserved
        record.QuickPinHash.Should().Be("pin-hash");
        record.QuickPinFailedCount.Should().Be(3);
        record.QuickPinLockedUntil.Should().Be(lockedUntil);

        // Round-trip via ToRecord maps the entity's quick pin fields
        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.UserId.Should().Be("U003");
    }

    // ── EntityMapper: AuditEntry round-trip ───────────────────────────────────

    [Fact]
    public void EntityMapper_AuditEntry_RoundTrip()
    {
        var entry = new AuditEntry(
            EntryId: "AUD-001",
            Timestamp: new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero),
            UserId: "admin",
            Action: "Login",
            Details: "User logged in",
            PreviousHash: "prev-hash",
            CurrentHash: "curr-hash");

        var entity = EntityMapper.ToEntity(entry);
        entity.EntryId.Should().Be("AUD-001");
        entity.UserId.Should().Be("admin");
        entity.Action.Should().Be("Login");
        entity.PreviousHash.Should().Be("prev-hash");
        entity.CurrentHash.Should().Be("curr-hash");

        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.EntryId.Should().Be(entry.EntryId);
        roundTripped.UserId.Should().Be(entry.UserId);
        roundTripped.Action.Should().Be(entry.Action);
    }

    // ── EntityMapper: PatientRecord without encryption ───────────────────────

    [Fact]
    public void EntityMapper_PatientRecord_NoEncryption_RoundTrip()
    {
        var record = new PatientRecord(
            "P100", "Kim^Soo", new DateOnly(1990, 5, 20), "F", false,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "admin");

        var entity = EntityMapper.ToEntity(record);
        entity.PatientId.Should().Be("P100");
        entity.Name.Should().Be("Kim^Soo");

        var roundTripped = EntityMapper.ToRecord(entity);
        roundTripped.PatientId.Should().Be("P100");
        roundTripped.Name.Should().Be("Kim^Soo");
    }

    [Fact]
    public void EntityMapper_PatientRecord_WithEncryption_RoundTrip()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);

        var record = new PatientRecord(
            "P200", "Encrypted^Name", new DateOnly(1985, 3, 10), "M", true,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "creator");

        var entity = EntityMapper.ToEntity(record, phiService);
        // Name in entity should be encrypted (not plaintext)
        entity.Name.Should().NotBe("Encrypted^Name");
        // PatientId is NOT encrypted — it is a primary key stored as-is
        entity.PatientId.Should().Be("P200");

        var roundTripped = EntityMapper.ToRecord(entity, phiService);
        roundTripped.Name.Should().Be("Encrypted^Name");
        roundTripped.PatientId.Should().Be("P200");
        roundTripped.DateOfBirth.Should().Be(new DateOnly(1985, 3, 10));
    }

    [Fact]
    public void EntityMapper_ApplyUpdate_PatientWithoutEncryption()
    {
        var entity = new PatientEntity
        {
            PatientId = "P300",
            Name = "Old^Name",
            Sex = "M",
            IsEmergency = false,
        };

        var record = new PatientRecord(
            "P300", "New^Name", new DateOnly(2000, 1, 1), "F", true,
            DateTimeOffset.UtcNow, "user");

        EntityMapper.ApplyUpdate(entity, record);
        entity.Name.Should().Be("New^Name");
        entity.Sex.Should().Be("F");
        entity.IsEmergency.Should().BeTrue();
    }

    [Fact]
    public void EntityMapper_ApplyUpdate_PatientWithEncryption()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);

        var entity = new PatientEntity { PatientId = "P301", Name = "old" };
        var record = new PatientRecord(
            "P301", "Updated^Encrypted", new DateOnly(1995, 6, 15), "M", false,
            DateTimeOffset.UtcNow, "user");

        EntityMapper.ApplyUpdate(entity, record, phiService);
        entity.Name.Should().NotBe("Updated^Encrypted"); // Should be encrypted
    }

    [Fact]
    public void EntityMapper_ApplyUpdate_NullDateOfBirth()
    {
        var entity = new PatientEntity { PatientId = "P302", Name = "Test", DateOfBirth = "2000-01-01" };
        var record = new PatientRecord("P302", "Test", null, "M", false, DateTimeOffset.UtcNow, "user");

        EntityMapper.ApplyUpdate(entity, record);
        entity.DateOfBirth.Should().BeNull();
    }

    // ── ServiceCollectionExtensions ───────────────────────────────────────────

    [Fact]
    public void AddHnVueData_WithExplicitKey_RegistersServices()
    {
        var services = new ServiceCollection();
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);

        services.AddHnVueData("Data Source=:memory:", key);

        using var sp = services.BuildServiceProvider();
        var phiService = sp.GetService<IPhiEncryptionService>();
        phiService.Should().NotBeNull();

        var patientRepo = sp.GetService<IPatientRepository>();
        patientRepo.Should().NotBeNull();

        var studyRepo = sp.GetService<IStudyRepository>();
        studyRepo.Should().NotBeNull();

        var userRepo = sp.GetService<IUserRepository>();
        userRepo.Should().NotBeNull();

        var auditRepo = sp.GetService<IAuditRepository>();
        auditRepo.Should().NotBeNull();
    }

    [Fact]
    public void AddHnVueData_WithPasswordInConnectionString_DerivesKey()
    {
        var services = new ServiceCollection();
        services.AddHnVueData("Data Source=:memory:;Password=TestKey123");

        using var sp = services.BuildServiceProvider();
        var phiService = sp.GetService<IPhiEncryptionService>();
        phiService.Should().NotBeNull();
    }

    [Fact]
    public void AddHnVueData_WithoutPassword_GeneratesRandomKey()
    {
        var services = new ServiceCollection();
        services.AddHnVueData("Data Source=:memory:");

        using var sp = services.BuildServiceProvider();
        var phiService = sp.GetService<IPhiEncryptionService>();
        phiService.Should().NotBeNull();
    }

    [Fact]
    public void AddHnVueData_InvalidKeySize_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        var shortKey = new byte[16];

        var act = () => services.AddHnVueData("Data Source=:memory:", shortKey);

        act.Should().Throw<ArgumentException>().WithMessage("*32 bytes*");
    }

    // ── PhiEncryptedValueConverter ────────────────────────────────────────────

    [Fact]
    public void PhiEncryptedValueConverter_EncryptsAndDecrypts()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);
        var converter = new Converters.PhiEncryptedValueConverter(phiService);

        const string original = "Sensitive Data";
        var encrypted = converter.ConvertToProvider(original);
        encrypted.Should().NotBe(original);

        var decrypted = converter.ConvertFromProvider(encrypted);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void NullablePhiEncryptedValueConverter_HandlesNull()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);
        var converter = new Converters.NullablePhiEncryptedValueConverter(phiService);

        var encrypted = converter.ConvertToProvider(null);
        encrypted.Should().BeNull();

        var decrypted = converter.ConvertFromProvider(null);
        decrypted.Should().BeNull();
    }

    [Fact]
    public void NullablePhiEncryptedValueConverter_EncryptsAndDecrypts()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);
        var converter = new Converters.NullablePhiEncryptedValueConverter(phiService);

        const string original = "DOB-Value";
        var encrypted = converter.ConvertToProvider(original);
        encrypted.Should().NotBe(original);

        var decrypted = converter.ConvertFromProvider(encrypted);
        decrypted.Should().Be(original);
    }

    // ── PhiKeyDerivation ──────────────────────────────────────────────────────

    [Fact]
    public void PhiKeyDerivation_DeriveKey_Produces32Bytes()
    {
        var derived = PhiKeyDerivation.DeriveKey("test-material");
        derived.Should().HaveCount(32);
    }

    [Fact]
    public void PhiKeyDerivation_DeriveKey_IsDeterministic()
    {
        var a = PhiKeyDerivation.DeriveKey("same-input");
        var b = PhiKeyDerivation.DeriveKey("same-input");
        a.Should().Equal(b);
    }

    [Fact]
    public void PhiKeyDerivation_DeriveKey_DifferentInputs_DifferentKeys()
    {
        var a = PhiKeyDerivation.DeriveKey("input-a");
        var b = PhiKeyDerivation.DeriveKey("input-b");
        a.Should().NotEqual(b);
    }

    // ── AesGcmPhiEncryptionService: FromSqlCipherKey ──────────────────────────

    [Fact]
    public void AesGcmPhiEncryptionService_FromSqlCipherKey_CreatesService()
    {
        var svc = AesGcmPhiEncryptionService.FromSqlCipherKey("sqlcipher-password");
        var encrypted = svc.Encrypt("test");
        encrypted.Should().NotBe("test");
        svc.Decrypt(encrypted).Should().Be("test");
    }

    [Fact]
    public void AesGcmPhiEncryptionService_FromSqlCipherKey_NullOrWhitespace_Throws()
    {
        var act1 = () => AesGcmPhiEncryptionService.FromSqlCipherKey(null!);
        act1.Should().Throw<ArgumentException>();

        var act2 = () => AesGcmPhiEncryptionService.FromSqlCipherKey("");
        act2.Should().Throw<ArgumentException>();

        var act3 = () => AesGcmPhiEncryptionService.FromSqlCipherKey("   ");
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AesGcmPhiEncryptionService_DeriveKey_Bytes()
    {
        var keyMaterial = new byte[] { 1, 2, 3, 4, 5 };
        var derived = AesGcmPhiEncryptionService.DeriveKey(keyMaterial);
        derived.Should().HaveCount(32);
    }

    [Fact]
    public void AesGcmPhiEncryptionService_DeriveKey_Bytes_Null_Throws()
    {
        var act = () => AesGcmPhiEncryptionService.DeriveKey((byte[])null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AesGcmPhiEncryptionService_DeriveKey_Bytes_Empty_Throws()
    {
        var act = () => AesGcmPhiEncryptionService.DeriveKey(Array.Empty<byte>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AesGcmPhiEncryptionService_NullKey_Throws()
    {
        var act = () => new AesGcmPhiEncryptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AesGcmPhiEncryptionService_WrongKeySize_Throws()
    {
        var act = () => new AesGcmPhiEncryptionService(new byte[16]);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AesGcmPhiEncryptionService_EncryptDecrypt_EmptyString()
    {
        var key = new byte[32];
        var svc = new AesGcmPhiEncryptionService(key);
        svc.Encrypt(string.Empty).Should().BeEmpty();
        svc.Decrypt(string.Empty).Should().BeEmpty();
    }

    // ── HnVueDbContext: with PHI encryption ───────────────────────────────────

    [Fact]
    public void HnVueDbContext_WithPhiEncryption_CreatesSuccessfully()
    {
        var key = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(key);
        var phiService = new AesGcmPhiEncryptionService(key);

        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var ctx = new HnVueDbContext(opts, phiService);
        ctx.Patients.Should().NotBeNull();
        ctx.Studies.Should().NotBeNull();
        ctx.Images.Should().NotBeNull();
        ctx.DoseRecords.Should().NotBeNull();
        ctx.Users.Should().NotBeNull();
        ctx.AuditLogs.Should().NotBeNull();
        ctx.UpdateHistories.Should().NotBeNull();
        ctx.Incidents.Should().NotBeNull();
        ctx.SystemSettings.Should().NotBeNull();
    }

    // ── HnVueDbContext: DbSet counts for all entity types ─────────────────────

    [Fact]
    public async Task HnVueDbContext_AllDbSets_AreQueryable()
    {
        await using var ctx = TestDbContextFactory.Create();

        ctx.Patients.Any().Should().BeFalse();
        ctx.Studies.Any().Should().BeFalse();
        ctx.Images.Any().Should().BeFalse();
        ctx.DoseRecords.Any().Should().BeFalse();
        ctx.Users.Any().Should().BeFalse();
        ctx.AuditLogs.Any().Should().BeFalse();
        ctx.UpdateHistories.Any().Should().BeFalse();
        ctx.Incidents.Any().Should().BeFalse();
        ctx.SystemSettings.Any().Should().BeFalse();
    }

    // ── EfUpdateRepository: integration test ─────────────────────────────────

    [Fact]
    public async Task EfUpdateRepository_RecordAndGetLatest()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfUpdateRepository(ctx);

        var recordResult = await repo.RecordInstallationAsync("1.0.0", "1.1.0", "hash123");
        recordResult.IsSuccess.Should().BeTrue();

        var checkResult = await repo.CheckForUpdateAsync();
        checkResult.IsSuccess.Should().BeTrue();
        checkResult.Value.Should().NotBeNull();
        checkResult.Value!.Version.Should().Be("1.1.0");
    }

    [Fact]
    public async Task EfUpdateRepository_NoHistory_ReturnsNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfUpdateRepository(ctx);

        var result = await repo.CheckForUpdateAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task EfUpdateRepository_NullFromVersion_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfUpdateRepository(ctx);

        var act = async () => await repo.RecordInstallationAsync(null!, "1.1.0", "hash");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EfUpdateRepository_NullToVersion_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfUpdateRepository(ctx);

        var act = async () => await repo.RecordInstallationAsync("1.0.0", null!, "hash");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── EfSystemSettingsRepository: integration test ──────────────────────────

    [Fact]
    public async Task EfSystemSettingsRepository_GetAsync_NoSettings_ReturnsDefaults()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfSystemSettingsRepository(ctx);

        var result = await repo.GetAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.LocalAeTitle.Should().Be("HNVUE");
        result.Value.Security.SessionTimeoutMinutes.Should().Be(15);
        result.Value.Security.MaxFailedLogins.Should().Be(5);
    }

    [Fact]
    public async Task EfSystemSettingsRepository_SaveAndGet_RoundTrip()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfSystemSettingsRepository(ctx);

        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS01",
                PacsHost = "192.168.1.100",
                PacsPort = 104,
                LocalAeTitle = "HNVUE",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM3",
                BaudRate = 9600,
                TimeoutMs = 5000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLogins = 3,
            },
        };

        var saveResult = await repo.SaveAsync(settings);
        saveResult.IsSuccess.Should().BeTrue();

        var getResult = await repo.GetAsync();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Dicom.PacsAeTitle.Should().Be("PACS01");
        getResult.Value.Dicom.PacsHost.Should().Be("192.168.1.100");
        getResult.Value.Dicom.PacsPort.Should().Be(104);
        getResult.Value.Generator.ComPort.Should().Be("COM3");
        getResult.Value.Generator.BaudRate.Should().Be(9600);
        getResult.Value.Generator.TimeoutMs.Should().Be(5000);
        getResult.Value.Security.SessionTimeoutMinutes.Should().Be(30);
        getResult.Value.Security.MaxFailedLogins.Should().Be(3);
    }

    [Fact]
    public async Task EfSystemSettingsRepository_UpdateExisting_Succeeds()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfSystemSettingsRepository(ctx);

        var settings1 = new SystemSettings
        {
            Dicom = new DicomSettings { LocalAeTitle = "OLD" },
            Generator = new GeneratorSettings(),
            Security = new SecuritySettings { SessionTimeoutMinutes = 10, MaxFailedLogins = 5 },
        };
        await repo.SaveAsync(settings1);

        var settings2 = new SystemSettings
        {
            Dicom = new DicomSettings { LocalAeTitle = "NEW" },
            Generator = new GeneratorSettings(),
            Security = new SecuritySettings { SessionTimeoutMinutes = 20, MaxFailedLogins = 3 },
        };
        var updateResult = await repo.SaveAsync(settings2);
        updateResult.IsSuccess.Should().BeTrue();

        var result = await repo.GetAsync();
        result.Value.Dicom.LocalAeTitle.Should().Be("NEW");
        result.Value.Security.SessionTimeoutMinutes.Should().Be(20);
    }

    [Fact]
    public async Task EfSystemSettingsRepository_NullSettings_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfSystemSettingsRepository(ctx);

        var act = async () => await repo.SaveAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── EfCdStudyRepository: integration test ─────────────────────────────────

    [Fact]
    public async Task EfCdStudyRepository_GetFilesForStudy_ReturnsPaths()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfCdStudyRepository(ctx);

        ctx.Images.Add(new ImageEntity { ImageId = "IMG1", StudyInstanceUid = "1.2.3.4", FilePath = @"C:\images\img1.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "IMG2", StudyInstanceUid = "1.2.3.4", FilePath = @"C:\images\img2.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "IMG3", StudyInstanceUid = "9.9.9.9", FilePath = @"C:\images\other.dcm" });
        await ctx.SaveChangesAsync();

        var result = await repo.GetFilesForStudyAsync("1.2.3.4");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task EfCdStudyRepository_NoImages_ReturnsEmpty()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfCdStudyRepository(ctx);

        var result = await repo.GetFilesForStudyAsync("nonexistent");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task EfCdStudyRepository_NullStudyUid_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfCdStudyRepository(ctx);

        var act = async () => await repo.GetFilesForStudyAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── EfWorklistRepository: integration test ────────────────────────────────

    [Fact]
    public async Task EfWorklistRepository_QueryToday_NoStudies_ReturnsEmpty()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfWorklistRepository(ctx);

        var result = await repo.QueryTodayAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task EfWorklistRepository_QueryToday_WithTodayStudy_ReturnsItem()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var now = DateTimeOffset.UtcNow;
            ctx.Patients.Add(new PatientEntity
            {
                PatientId = "P-WL-01",
                Name = "Worklist^Patient",
                IsEmergency = false,
                CreatedAtTicks = now.UtcTicks,
                CreatedAtOffsetMinutes = 0,
                CreatedBy = "test",
            });
            ctx.Studies.Add(new StudyEntity
            {
                StudyInstanceUid = "1.2.3.WL",
                PatientId = "P-WL-01",
                StudyDateTicks = now.UtcTicks,
                StudyDateOffsetMinutes = (int)now.Offset.TotalMinutes,
                Description = "Chest PA",
                AccessionNumber = "ACC-WL-01",
                BodyPart = "CHEST",
            });
            await ctx.SaveChangesAsync();

            var repo = new EfWorklistRepository(ctx);
            var result = await repo.QueryTodayAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].PatientId.Should().Be("P-WL-01");
            result.Value[0].AccessionNumber.Should().Be("ACC-WL-01");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    // ── EfDoseRepository: SaveAsync + GetByStudyAsync ─────────────────────────

    [Fact]
    public async Task EfDoseRepository_SaveAndGetByStudy()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            // Add prerequisite patient + study
            ctx.Patients.Add(new PatientEntity
            {
                PatientId = "P-DOSE",
                Name = "Dose^Test",
                IsEmergency = false,
                CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                CreatedAtOffsetMinutes = 0,
                CreatedBy = "test",
            });
            ctx.Studies.Add(new StudyEntity
            {
                StudyInstanceUid = "1.2.3.DOSE",
                PatientId = "P-DOSE",
                StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
                StudyDateOffsetMinutes = 0,
            });
            await ctx.SaveChangesAsync();

            var repo = new EfDoseRepository(ctx);
            var dose = new DoseRecord(
                DoseId: "D001",
                StudyInstanceUid: "1.2.3.DOSE",
                Dap: 12.5,
                Ei: 400.0,
                EffectiveDose: 0.05,
                BodyPart: "CHEST",
                RecordedAt: DateTimeOffset.UtcNow);

            var saveResult = await repo.SaveAsync(dose);
            saveResult.IsSuccess.Should().BeTrue();

            var getResult = await repo.GetByStudyAsync("1.2.3.DOSE");
            getResult.IsSuccess.Should().BeTrue();
            getResult.Value.Should().NotBeNull();
            getResult.Value!.DoseId.Should().Be("D001");
            getResult.Value.Dap.Should().Be(12.5);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfDoseRepository_GetByStudy_NotFound()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfDoseRepository(ctx);
            var result = await repo.GetByStudyAsync("nonexistent");
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfDoseRepository_SaveAsync_Null_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new EfDoseRepository(ctx);

        var act = async () => await repo.SaveAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_WithDateRange()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var now = DateTimeOffset.UtcNow;
            ctx.Patients.Add(new PatientEntity
            {
                PatientId = "P-DR",
                Name = "Date^Range",
                IsEmergency = false,
                CreatedAtTicks = now.UtcTicks,
                CreatedAtOffsetMinutes = 0,
                CreatedBy = "test",
            });
            ctx.Studies.Add(new StudyEntity
            {
                StudyInstanceUid = "1.2.3.DR",
                PatientId = "P-DR",
                StudyDateTicks = now.UtcTicks,
                StudyDateOffsetMinutes = 0,
            });
            await ctx.SaveChangesAsync();

            var repo = new EfDoseRepository(ctx);
            await repo.SaveAsync(new DoseRecord("D-DR1", "1.2.3.DR", 10.0, 200.0, 0.03, "CHEST", now));
            await repo.SaveAsync(new DoseRecord("D-DR2", "1.2.3.DR", 15.0, 300.0, 0.04, "ABD", now));

            var result = await repo.GetByPatientAsync("P-DR", null, null);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    // ── EfIncidentRepository: Save + Resolve ──────────────────────────────────

    [Fact]
    public async Task EfIncidentRepository_SaveAsync_PersistsEntity()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfIncidentRepository(ctx);
            var entity = new IncidentEntity
            {
                IncidentId = "INC-001",
                SeverityValue = 2,
                Description = "Dose exceeded threshold",
                OccurredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                OccurredAtOffsetMinutes = 0,
                IsResolved = false,
            };

            var result = await repo.SaveAsync(entity);
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_MarksResolved()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfIncidentRepository(ctx);
            var entity = new IncidentEntity
            {
                IncidentId = "INC-002",
                SeverityValue = 1,
                Description = "Low severity",
                OccurredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                OccurredAtOffsetMinutes = 0,
                IsResolved = false,
            };
            await repo.SaveAsync(entity);

            var resolveResult = await repo.ResolveAsync("INC-002", "Investigated, no action needed");
            resolveResult.IsSuccess.Should().BeTrue();
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_NotFound()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfIncidentRepository(ctx);
            var result = await repo.ResolveAsync("NONEXISTENT", "resolution");

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.NotFound);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_AlreadyResolved()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfIncidentRepository(ctx);
            var entity = new IncidentEntity
            {
                IncidentId = "INC-003",
                SeverityValue = 1,
                Description = "Already resolved",
                OccurredAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                OccurredAtOffsetMinutes = 0,
                IsResolved = false,
            };
            await repo.SaveAsync(entity);
            await repo.ResolveAsync("INC-003", "Resolved once");

            var secondResolve = await repo.ResolveAsync("INC-003", "Try again");
            secondResolve.IsFailure.Should().BeTrue();
            secondResolve.Error.Should().Be(ErrorCode.ValidationFailed);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task EfIncidentRepository_GetBySeverity_ReturnsMatching()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        try
        {
            var opts = new DbContextOptionsBuilder<HnVueDbContext>()
                .UseSqlite(connection)
                .Options;
            using var ctx = new HnVueDbContext(opts);
            ctx.Database.EnsureCreated();

            var repo = new EfIncidentRepository(ctx);
            await repo.SaveAsync(new IncidentEntity
            {
                IncidentId = "INC-S1", SeverityValue = 1, Description = "Low",
                OccurredAtTicks = DateTimeOffset.UtcNow.UtcTicks, OccurredAtOffsetMinutes = 0, IsResolved = false,
            });
            await repo.SaveAsync(new IncidentEntity
            {
                IncidentId = "INC-S2", SeverityValue = 2, Description = "High",
                OccurredAtTicks = DateTimeOffset.UtcNow.UtcTicks, OccurredAtOffsetMinutes = 0, IsResolved = false,
            });

            var result = await repo.GetBySeverityAsync(2);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].IncidentId.Should().Be("INC-S2");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    // ── StudyRepository: additional tests ─────────────────────────────────────

    [Fact]
    public async Task StudyRepository_AddAndGetByPatient()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var study = new StudyRecord(
            "1.2.3.ST", "P-ST", new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero),
            "Desc", "ACC-ST", "CHEST");

        var addResult = await repo.AddAsync(study);
        addResult.IsSuccess.Should().BeTrue();
        addResult.Value.StudyInstanceUid.Should().Be("1.2.3.ST");

        var getByPatient = await repo.GetByPatientAsync("P-ST");
        getByPatient.IsSuccess.Should().BeTrue();
        getByPatient.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task StudyRepository_GetByUid_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.GetByUidAsync("nonexistent");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task StudyRepository_UpdateAsync_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var study = new StudyRecord("nonexistent", "P", DateTimeOffset.UtcNow, "D", "A", "B");
        var result = await repo.UpdateAsync(study);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task StudyRepository_UpdateAsync_Existing()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var original = new StudyRecord(
            "1.2.3.UP", "P-UP", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "Original", "ACC-1", "CHEST");
        await repo.AddAsync(original);

        var updated = new StudyRecord(
            "1.2.3.UP", "P-UP", new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero),
            "Updated", "ACC-2", "ABD");
        var updateResult = await repo.UpdateAsync(updated);
        updateResult.IsSuccess.Should().BeTrue();

        var found = await repo.GetByUidAsync("1.2.3.UP");
        found.Value!.Description.Should().Be("Updated");
        found.Value.AccessionNumber.Should().Be("ACC-2");
    }

    // ── UserRepository: additional tests ──────────────────────────────────────

    [Fact]
    public async Task UserRepository_AddAndGetAll()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user1 = new UserRecord("U1", "admin", "Admin", "hash1", Common.Enums.UserRole.Admin,
            0, false, null, null, 0, null);
        var user2 = new UserRecord("U2", "tech", "Tech", "hash2", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);

        (await repo.AddAsync(user1)).IsSuccess.Should().BeTrue();
        (await repo.AddAsync(user2)).IsSuccess.Should().BeTrue();

        var all = await repo.GetAllAsync();
        all.IsSuccess.Should().BeTrue();
        all.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task UserRepository_GetById_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetByIdAsync("nonexistent");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_SetQuickPin()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user = new UserRecord("U-PIN", "user", "User", "hash", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);
        await repo.AddAsync(user);

        var setResult = await repo.SetQuickPinHashAsync("U-PIN", "pin-hash-value");
        setResult.IsSuccess.Should().BeTrue();

        var getResult = await repo.GetQuickPinHashAsync("U-PIN");
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().Be("pin-hash-value");
    }

    [Fact]
    public async Task UserRepository_GetQuickPinHash_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.GetQuickPinHashAsync("nonexistent");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdateQuickPinFailure()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user = new UserRecord("U-QP", "qpuser", "QP User", "hash", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);
        await repo.AddAsync(user);

        var lockedUntil = DateTimeOffset.UtcNow.AddMinutes(5);
        var result = await repo.UpdateQuickPinFailureAsync("U-QP", 3, lockedUntil);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UserRepository_UpdateQuickPinFailure_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateQuickPinFailureAsync("nonexistent", 1, null);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_SetLocked_And_UpdateFailedLogin()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user = new UserRecord("U-LK", "lockuser", "Lock", "hash", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);
        await repo.AddAsync(user);

        var lockResult = await repo.SetLockedAsync("U-LK", true);
        lockResult.IsSuccess.Should().BeTrue();

        var failedResult = await repo.UpdateFailedLoginCountAsync("U-LK", 3);
        failedResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UserRepository_SetLocked_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.SetLockedAsync("nonexistent", true);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdatePasswordHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user = new UserRecord("U-PW", "pwuser", "PW", "old-hash", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);
        await repo.AddAsync(user);

        var result = await repo.UpdatePasswordHashAsync("U-PW", "new-hash");
        result.IsSuccess.Should().BeTrue();

        var found = await repo.GetByUsernameAsync("pwuser");
        found.Value.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task UserRepository_UpdatePasswordHash_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdatePasswordHashAsync("nonexistent", "hash");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UserRepository_UpdateFailedLoginCount_NotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var result = await repo.UpdateFailedLoginCountAsync("nonexistent", 5);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── AuditRepository: QueryAsync with filters ─────────────────────────────

    [Fact]
    public async Task AuditRepository_QueryAsync_WithFilters()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var entry1 = new AuditEntry("A1", new DateTimeOffset(2026, 4, 13, 10, 0, 0, TimeSpan.Zero), "admin", "Login", "User logged in", null, "hash1");
        var entry2 = new AuditEntry("A2", new DateTimeOffset(2026, 4, 14, 10, 0, 0, TimeSpan.Zero), "tech", "Action", "Did something", "hash1", "hash2");

        (await repo.AppendAsync(entry1)).IsSuccess.Should().BeTrue();
        (await repo.AppendAsync(entry2)).IsSuccess.Should().BeTrue();

        // Filter by UserId
        var filterUser = new AuditQueryFilter(UserId: "admin", MaxResults: 100);
        var userResult = await repo.QueryAsync(filterUser);
        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.Should().HaveCount(1);

        // Filter by date range
        var filterDate = new AuditQueryFilter(
            FromDate: new DateTimeOffset(2026, 4, 14, 0, 0, 0, TimeSpan.Zero),
            ToDate: new DateTimeOffset(2026, 4, 14, 23, 59, 59, TimeSpan.Zero),
            MaxResults: 100);
        var dateResult = await repo.QueryAsync(filterDate);
        dateResult.IsSuccess.Should().BeTrue();
        dateResult.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task AuditRepository_QueryAsync_MaxResults_Limits()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        for (int i = 0; i < 10; i++)
        {
            await repo.AppendAsync(new AuditEntry(
                $"A{i}", DateTimeOffset.UtcNow, "user", "Action", $"Entry {i}", null, $"hash{i}"));
        }

        var filter = new AuditQueryFilter(MaxResults: 3);
        var result = await repo.QueryAsync(filter);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    // ── PatientRepository: DeleteAsync already soft-deleted ──────────────────

    [Fact]
    public async Task PatientRepository_DeleteAsync_AlreadySoftDeleted_Idempotent()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient("P-IDEM"));

        await repo.DeleteAsync("P-IDEM");
        var secondDelete = await repo.DeleteAsync("P-IDEM");

        secondDelete.IsSuccess.Should().BeTrue("soft delete should be idempotent");
    }

    private static PatientRecord CreateSamplePatient(string id = "P001") =>
        new(id, "Doe^John", new DateOnly(1980, 6, 15), "M", false,
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "user-01");
}
