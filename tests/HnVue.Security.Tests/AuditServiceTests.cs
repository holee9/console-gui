using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Security;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

public sealed class AuditServiceTests
{
    /// <summary>
    /// Test HMAC key used across all audit service tests.
    /// This key is only for test execution and must never be used in production.
    /// </summary>
    private const string TestHmacKey = "HnVue-Test-HMAC-Key-32CharMin!!";

    private readonly IAuditRepository _auditRepository;
    private readonly AuditService _sut;

    public AuditServiceTests()
    {
        _auditRepository = Substitute.For<IAuditRepository>();
        var options = Options.Create(new AuditOptions { HmacKey = TestHmacKey });
        _sut = new AuditService(_auditRepository, options);
    }

    // ── WriteAuditAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task WriteAudit_FirstEntry_PreviousHashIsNull()
    {
        // Empty log: repository returns NotFound (no previous entry).
        _auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty log"));

        AuditEntry? captured = null;
        _auditRepository.AppendAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var entry = MakeEntry("user-1", "LOGIN");

        var result = await _sut.WriteAuditAsync(entry);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.PreviousHash.Should().BeNull();
        captured.CurrentHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WriteAudit_SubsequentEntry_ChainsHash()
    {
        const string previousHash = "abc123previoushash";
        // Simulate a non-null previous hash by wrapping in a success result using a non-null wrapper.
        // Since Result<string?> cannot hold null, we use a known non-null string here.
        _auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<string?>(previousHash));

        AuditEntry? captured = null;
        _auditRepository.AppendAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var entry = MakeEntry("user-1", "EXPOSE");

        var result = await _sut.WriteAuditAsync(entry);

        result.IsSuccess.Should().BeTrue();
        captured!.PreviousHash.Should().Be(previousHash);
        captured.CurrentHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WriteAudit_WhenGetLastHashFails_ReturnsIncidentLogFailed()
    {
        // DatabaseError is a fatal failure (not NotFound), so the service propagates it.
        _auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.DatabaseError, "db error"));

        var entry = MakeEntry("user-1", "LOGIN");

        var result = await _sut.WriteAuditAsync(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    [Fact]
    public async Task WriteAudit_WhenAppendFails_ReturnsIncidentLogFailed()
    {
        _auditRepository.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty log"));
        _auditRepository.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "append failed"));

        var entry = MakeEntry("user-1", "LOGIN");

        var result = await _sut.WriteAuditAsync(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    // ── VerifyChainIntegrityAsync ──────────────────────────────────────────────

    [Fact]
    public async Task VerifyChainIntegrity_ValidChain_ReturnsTrue()
    {
        // Build a two-entry chain with proper hashes.
        var timestamp = DateTimeOffset.UtcNow;

        var id1 = Guid.NewGuid().ToString();
        var payload1 = $"{id1}|{timestamp:O}|user-1|LOGIN||";
        var hash1 = AuditService.ComputeHmacInternal(payload1, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry1 = new AuditEntry(id1, timestamp, "user-1", "LOGIN", null, null, hash1);

        var id2 = Guid.NewGuid().ToString();
        var payload2 = $"{id2}|{timestamp:O}|user-1|EXPOSE||{hash1}";
        var hash2 = AuditService.ComputeHmacInternal(payload2, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry2 = new AuditEntry(id2, timestamp, "user-1", "EXPOSE", null, hash1, hash2);

        _auditRepository.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1, entry2 }));

        var result = await _sut.VerifyChainIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyChainIntegrity_TamperedEntry_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow;

        var id1 = Guid.NewGuid().ToString();
        var payload1 = $"{id1}|{timestamp:O}|user-1|LOGIN||";
        var hash1 = AuditService.ComputeHmacInternal(payload1, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        // Tamper: store a wrong hash
        var entry1 = new AuditEntry(id1, timestamp, "user-1", "LOGIN", null, null, "tampered-hash");

        _auditRepository.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1 }));

        var result = await _sut.VerifyChainIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyChainIntegrity_BrokenPreviousHashLink_ReturnsFalse()
    {
        var timestamp = DateTimeOffset.UtcNow;

        var id1 = Guid.NewGuid().ToString();
        var payload1 = $"{id1}|{timestamp:O}|user-1|LOGIN||";
        var hash1 = AuditService.ComputeHmacInternal(payload1, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry1 = new AuditEntry(id1, timestamp, "user-1", "LOGIN", null, null, hash1);

        // entry2 references a wrong previousHash
        var id2 = Guid.NewGuid().ToString();
        var wrongPrevHash = "wrong-previous-hash";
        var payload2 = $"{id2}|{timestamp:O}|user-1|EXPOSE||{wrongPrevHash}";
        var hash2 = AuditService.ComputeHmacInternal(payload2, System.Text.Encoding.UTF8.GetBytes(TestHmacKey));
        var entry2 = new AuditEntry(id2, timestamp, "user-1", "EXPOSE", null, wrongPrevHash, hash2);

        _auditRepository.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(new[] { entry1, entry2 }));

        var result = await _sut.VerifyChainIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyChainIntegrity_EmptyLog_ReturnsTrue()
    {
        _auditRepository.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>()));

        var result = await _sut.VerifyChainIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyChainIntegrity_WhenQueryFails_ReturnsFailure()
    {
        _auditRepository.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AuditEntry>>(ErrorCode.DatabaseError, "db error"));

        var result = await _sut.VerifyChainIntegrityAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    // ── GetAuditLogsAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_DelegatesToRepository()
    {
        var filter = new AuditQueryFilter(UserId: "user-1");
        var expected = new[] { MakeEntry("user-1", "LOGIN") };
        _auditRepository.QueryAsync(filter, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<AuditEntry>>(expected));

        var result = await _sut.GetAuditLogsAsync(filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AuditEntry MakeEntry(string userId, string action)
        => new(
            DateTimeOffset.UtcNow,
            userId,
            action,
            currentHash: "placeholder",
            details: null,
            previousHash: null);
}
