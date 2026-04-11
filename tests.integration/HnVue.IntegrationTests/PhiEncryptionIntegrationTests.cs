using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Data;
using HnVue.Data.Entities;
using HnVue.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Integration tests for PHI AES-256-GCM encryption (SPEC-INFRA-002, REQ-PHI-003, REQ-PHI-004).
/// Verifies that PatientEntity PHI fields are encrypted when stored and decrypted when retrieved,
/// and that DI container correctly resolves AesGcmPhiEncryptionService.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Team", "TeamA")]
[Trait("SWR", "SWR-CS-080")]
[Trait("SPEC", "SPEC-INFRA-002")]
public sealed class PhiEncryptionIntegrationTests : IDisposable
{
    private readonly HnVueDbContext _dbContext;
    private readonly AesGcmPhiEncryptionService _phiService;
    private readonly string _connectionString;

    public PhiEncryptionIntegrationTests()
    {
        // Use real SQLite in-memory database (not EF InMemory) for authentic encryption test
        _connectionString = "Data Source=:memory:";

        // Create PHI service from a deterministic key (simulating SQLCipher password derivation)
        _phiService = AesGcmPhiEncryptionService.FromSqlCipherKey("integration-test-sqlcipher-key");

        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(_connectionString)
            .UseInternalServiceProvider(new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider())
            .Options;

        _dbContext = new HnVueDbContext(options, _phiService);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// REQ-PHI-003: Verifies that PHI fields (Name, DateOfBirth, PatientId) are encrypted
    /// when stored and correctly decrypted when retrieved via DbContext.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public async Task PatientEntity_PhiFields_AreEncryptedInStorageAndDecryptedOnRead()
    {
        // Arrange - create a patient with PHI data
        var patient = new PatientEntity
        {
            PatientId = "PAT-001",
            Name = "홍길동",
            DateOfBirth = "1990-01-15",
            Sex = "M",
            CreatedBy = "admin",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        };

        // Act - save to database (PHI fields should be encrypted by value converters)
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        // Detach from context to force reload from DB
        _dbContext.ChangeTracker.Clear();

        // Act - reload from database (PHI fields should be decrypted transparently)
        // Note: FindAsync cannot be used with non-deterministic encrypted PKs (AES-GCM random nonce).
        // Using FirstOrDefaultAsync to read all rows and let value converters decrypt transparently.
        var retrieved = await _dbContext.Patients.FirstOrDefaultAsync();

        // Assert - PHI data is correctly decrypted on read
        retrieved.Should().NotBeNull("patient should exist in database");
        retrieved!.Name.Should().Be("홍길동", "Name must round-trip through encryption");
        retrieved.DateOfBirth.Should().Be("1990-01-15", "DateOfBirth must round-trip through encryption");
        retrieved.PatientId.Should().Be("PAT-001", "PatientId must round-trip through encryption");

        // Assert - Verify raw DB values are encrypted (not plaintext)
        // Use ADO.NET to bypass EF Core model cache which shares the converter-including model.
        var connection = _dbContext.Database.GetDbConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Name, DateOfBirth FROM Patients LIMIT 1";
        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        var rawName = reader.GetString(0);
        var rawDob = reader.IsDBNull(1) ? null : reader.GetString(1);
        rawName.Should().NotBe("홍길동", "raw DB Name should be encrypted, not plaintext");
        rawDob.Should().NotBe("1990-01-15", "raw DB DateOfBirth should be encrypted, not plaintext");
    }

    /// <summary>
    /// REQ-PHI-004: Verifies that DI container correctly resolves IPhiEncryptionService
    /// as AesGcmPhiEncryptionService, and that the service encrypts/decrypts correctly.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CS-080")]
    public void DiContainer_ResolvesAesGcmPhiEncryptionService_AndFunctionsCorrectly()
    {
        // Arrange - build service collection mimicking App.xaml.cs registration
        var services = new ServiceCollection();

        var encryptionKey = AesGcmPhiEncryptionService.DeriveKey("di-test-password");
        var phiService = new AesGcmPhiEncryptionService(encryptionKey);
        services.AddSingleton<IPhiEncryptionService>(phiService);
        services.AddSingleton<AesGcmPhiEncryptionService>(phiService);

        var sp = services.BuildServiceProvider();

        // Act - resolve via interface
        var resolved = sp.GetRequiredService<IPhiEncryptionService>();

        // Assert - resolved service is AesGcmPhiEncryptionService
        resolved.Should().BeOfType<AesGcmPhiEncryptionService>(
            "DI container should resolve IPhiEncryptionService as AesGcmPhiEncryptionService (REQ-PHI-004)");

        // Assert - service correctly encrypts and decrypts
        const string testData = "P-DI-TEST-001";
        var encrypted = resolved.Encrypt(testData);
        var decrypted = resolved.Decrypt(encrypted);

        encrypted.Should().NotBe(testData, "data must be encrypted");
        decrypted.Should().Be(testData, "data must decrypt back to original");
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }
}
