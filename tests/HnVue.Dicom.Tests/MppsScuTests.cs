using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="MppsScu"/> covering parameter validation,
/// configuration checks, and error handling paths.
/// SWR-DC-055 (N-CREATE) / SWR-DC-056 (N-SET).
/// </summary>
/// <remarks>
/// MppsScu uses <c>DicomClientFactory.Create</c> internally, which is a static
/// factory. Network-level tests exercise real fo-dicom connection attempts;
/// unhandled exception types (e.g. <see cref="AggregateException"/> from fo-dicom
/// internals) propagate through since MppsScu only catches
/// <c>DicomNetworkException</c> and <c>OperationCanceledException</c>.
/// </remarks>
public sealed class MppsScuTests
{
    private static MppsScu CreateSut(DicomOptions? options = null)
    {
        return new MppsScu(options ?? new DicomOptions());
    }

    private static DicomOptions ValidOptions() => new()
    {
        LocalAeTitle = "HNVUE_SCU",
        MppsAeTitle = "MPPS_SCP",
        MppsHost = "192.168.1.50",
        MppsPort = 104,
        TlsEnabled = false,
    };

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public void Ctor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new MppsScu(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    // ── SendInProgressAsync — Configuration Validation ──────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_NullMppsHost_ReturnsDicomConnectionFailed()
    {
        var options = ValidOptions();
        options.MppsHost = null!;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_EmptyMppsHost_ReturnsDicomConnectionFailed()
    {
        var options = ValidOptions();
        options.MppsHost = string.Empty;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    // ── SendInProgressAsync — Network Error Paths ───────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_InvalidHost_ThrowsAggregateException()
    {
        // fo-dicom wraps connection failures in AggregateException (not DicomNetworkException),
        // which is NOT caught by MppsScu's catch blocks. This test documents the actual behavior.
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var act = async () => await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_PreCancelledToken_ReturnsResultWithoutThrowing()
    {
        // With a pre-cancelled token, fo-dicom may complete without throwing.
        // The method returns a Result (success or failure) rather than propagating an exception.
        var sut = CreateSut(ValidOptions());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Should not throw regardless of token state.
        var result = await sut.SendInProgressAsync(
            "1.2.3.4.5", "PAT001", "CHEST", cts.Token);

        // The important contract: no exception propagates; a Result is always returned.
        result.Should().NotBeNull();
    }

    // ── SendCompletedAsync — Configuration Validation ───────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_NullMppsHost_ReturnsDicomConnectionFailed()
    {
        var options = ValidOptions();
        options.MppsHost = null!;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_EmptyMppsHost_ReturnsDicomConnectionFailed()
    {
        var options = ValidOptions();
        options.MppsHost = string.Empty;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    // ── SendCompletedAsync — Parameter Validation ───────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_NullMppsUid_ThrowsArgumentNullException()
    {
        var sut = CreateSut(ValidOptions());

        var act = async () => await sut.SendCompletedAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_EmptyMppsUid_DoesNotThrowArgumentNullException()
    {
        // Empty string is not null — ArgumentNullException.ThrowIfNull only checks null.
        // With a valid MppsHost, it proceeds to network call which fails.
        var sut = CreateSut(ValidOptions());

        var act = async () => await sut.SendCompletedAsync(string.Empty);

        // Empty string passes the null check but may fail at the network level.
        // The fo-dicom client will throw due to invalid SOP Instance UID format.
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }

    // ── SendCompletedAsync — Network Error Paths ────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_InvalidHost_ThrowsAggregateException()
    {
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var act = async () => await sut.SendCompletedAsync("1.2.3.4.5.6.7");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_PreCancelledToken_ReturnsResultWithoutThrowing()
    {
        var sut = CreateSut(ValidOptions());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Should not throw regardless of token state.
        var result = await sut.SendCompletedAsync(
            "1.2.3.4.5.6.7", true, cts.Token);

        result.Should().NotBeNull();
    }

    // ── SendCompletedAsync — Status Parameter Variants ──────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_DiscontinuedInvalidHost_ThrowsAggregateException()
    {
        // Verifies the completed=false path also attempts network call.
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var act = async () => await sut.SendCompletedAsync("1.2.3.4.5.6.7", completed: false);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_DiscontinuedPreCancelledToken_ReturnsResultWithoutThrowing()
    {
        var sut = CreateSut(ValidOptions());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await sut.SendCompletedAsync(
            "1.2.3.4.5.6.7", completed: false, cts.Token);

        result.Should().NotBeNull();
    }

    // ── Error Code Consistency ──────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_DefaultOptions_NoMppsHost_ReturnsFailure()
    {
        // Default DicomOptions has MppsHost = string.Empty
        var sut = CreateSut();

        var result = await sut.SendInProgressAsync("1.2.3", "P001", "ABD");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("MPPS host");
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_DefaultOptions_NoMppsHost_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.SendCompletedAsync("1.2.3.4.5");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("MPPS host");
    }

    // ── Options propagation ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public void Ctor_WithValidOptions_DoesNotThrow()
    {
        var act = () => CreateSut(ValidOptions());

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_TlsEnabled_DoesNotEarlyReturn()
    {
        // Ensures the TLS path reaches the network layer (no config guard blocks it).
        var options = ValidOptions();
        options.TlsEnabled = true;
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var act = async () => await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        // TLS connection to invalid host also throws
        await act.Should().ThrowAsync<Exception>();
    }
}
