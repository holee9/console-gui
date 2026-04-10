using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Tampering detection tests for AuditService (REQ-SEC-001).
/// Verifies that the audit chain integrity check properly detects tampering.
/// </summary>
public sealed class AuditServiceTamperingTests
{
    private const string TestHmacKey = "HnVue-Test-HMAC-Key-32CharMin!!";

    private readonly IAuditRepository _auditRepository;
    private readonly AuditService _sut;

    public AuditServiceTamperingTests()
    {
        _auditRepository = Substitute.For<IAuditRepository>();
        var options = Options.Create(new AuditOptions { HmacKey = TestHmacKey });
        _sut = new AuditService(_auditRepository, options);
    }

    /// <summary>
    /// CRITICAL BUG TEST: This test verifies that tampering with the PreviousHash
    /// field is properly detected as a failure, not incorrectly returned as Success(false).
    /// </summary>
    [Fact]
    public async Task VerifyChainIntegrityAsync_WhenPreviousHashTampered_ReturnsFailure()
    {
        // Arrange - Create a valid chain with 3 entries
        var now = DateTimeOffset.UtcNow;
        var entry1 = new AuditEntry(
            EntryId: "entry-1",
            Timestamp: now,
            UserId: "user-1",
            Action: "LOGIN",
            Details: null,
            PreviousHash: null,
            CurrentHash: "hash1");
        var entry2 = new AuditEntry(
            EntryId: "entry-2",
            Timestamp: now.AddMinutes(1),
            UserId: "user-1",
            Action: "EXPOSE",
            Details: null,
            PreviousHash: "hash1",  // Correct previous hash
            CurrentHash: "hash2");
        var entry3 = new AuditEntry(
            EntryId: "entry-3",
            Timestamp: now.AddMinutes(2),
            UserId: "user-1",
            Action: "LOGOUT",
            Details: null,
            PreviousHash: "TAMPERED",  // TAMPERED: Should be "hash2"
            CurrentHash: "hash3");

        _auditRepository
            .QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1, entry2, entry3 }));

        // Act
        var result = await _sut.VerifyChainIntegrityAsync();

        // Assert - BUG: Currently returns Success(false), should return Failure
        result.IsFailure.Should().BeTrue("tampering should be detected as a failure, not Success(false)");
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    /// <summary>
    /// CRITICAL BUG TEST: This test verifies that tampering with the CurrentHash
    /// field (HMAC) is properly detected as a failure, not incorrectly returned as Success(false).
    /// </summary>
    [Fact]
    public async Task VerifyChainIntegrityAsync_WhenCurrentHashTampered_ReturnsFailure()
    {
        // Arrange - Create a chain where entry2's HMAC is tampered
        var now = DateTimeOffset.UtcNow;
        var entry1 = new AuditEntry(
            EntryId: "entry-1",
            Timestamp: now,
            UserId: "user-1",
            Action: "LOGIN",
            Details: null,
            PreviousHash: null,
            CurrentHash: "hash1");
        var entry2 = new AuditEntry(
            EntryId: "entry-2",
            Timestamp: now.AddMinutes(1),
            UserId: "user-1",
            Action: "EXPOSE",
            Details: null,
            PreviousHash: "hash1",
            CurrentHash: "TAMPERED_HASH");  // TAMPERED: Invalid HMAC

        _auditRepository
            .QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1, entry2 }));

        // Act
        var result = await _sut.VerifyChainIntegrityAsync();

        // Assert - BUG: Currently returns Success(false), should return Failure
        result.IsFailure.Should().BeTrue("HMAC tampering should be detected as a failure, not Success(false)");
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    /// <summary>
    /// Verify that a valid chain returns Success(true).
    /// </summary>
    [Fact]
    public async Task VerifyChainIntegrityAsync_WhenChainValid_ReturnsSuccessTrue()
    {
        // Arrange - Create a valid chain
        var timestamp = DateTimeOffset.UtcNow;

        var id1 = Guid.NewGuid().ToString();
        var payload1 = $"{id1}|{timestamp:O}|user-1|LOGIN||";
        var hash1 = AuditService.ComputeHmacInternal(payload1, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry1 = new AuditEntry(id1, timestamp, "user-1", "LOGIN", null, null, hash1);

        var id2 = Guid.NewGuid().ToString();
        var payload2 = $"{id2}|{timestamp:O}|user-1|EXPOSE||{hash1}";
        var hash2 = AuditService.ComputeHmacInternal(payload2, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry2 = new AuditEntry(id2, timestamp, "user-1", "EXPOSE", null, hash1, hash2);

        _auditRepository
            .QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1, entry2 }));

        // Act
        var result = await _sut.VerifyChainIntegrityAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}
