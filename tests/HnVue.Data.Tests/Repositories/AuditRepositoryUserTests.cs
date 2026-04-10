using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Tests for ISecurityContext integration in audit logging (REQ-DATA-002).
/// Verifies that audit entries use actual user from security context instead of hardcoded "system".
/// </summary>
public sealed class AuditRepositoryUserTests
{
    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    [Fact]
    public async Task PatientRepository_AddAsync_UsesSecurityContextUser()
    {
        // Arrange
        var (ctx, connection) = CreateSqliteContext();
        await using var _ = connection;
        var securityContext = Substitute.For<ISecurityContext>();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        securityContext.CurrentUserId.Returns("test-user");
        securityContext.CurrentUsername.Returns("test-user");

        var patient = new PatientRecord(
            PatientId: "P001",
            Name: "Doe^John",
            DateOfBirth: new DateOnly(1980, 6, 15),
            Sex: "M",
            IsEmergency: false,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "test-user");

        var repo = new PatientRepository(ctx, auditRepo, null, securityContext);

        // Act
        await repo.AddAsync(patient);

        // Assert
        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditEntry>(e => e.UserId == "test-user"));
    }

    [Fact]
    public async Task PatientRepository_AddAsync_NoSecurityContext_UsesAnonymous()
    {
        // Arrange
        var (ctx, connection) = CreateSqliteContext();
        await using var _ = connection;
        var securityContext = Substitute.For<ISecurityContext>();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));

        // No current user set (returns null)
        securityContext.CurrentUserId.Returns((string?)null);

        var patient = new PatientRecord(
            PatientId: "P002",
            Name: "Smith^Jane",
            DateOfBirth: new DateOnly(1990, 3, 20),
            Sex: "F",
            IsEmergency: false,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "test-user");

        var repo = new PatientRepository(ctx, auditRepo, null, securityContext);

        // Act
        await repo.AddAsync(patient);

        // Assert
        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditEntry>(e => e.UserId == "anonymous"));
    }
}


