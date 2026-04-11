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
/// factory. These tests cover configuration and failure translation paths
/// without requiring a reachable MPPS SCP.
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

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public void Ctor_WithValidOptions_DoesNotThrow()
    {
        var act = () => CreateSut(ValidOptions());

        act.Should().NotThrow();
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

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_DefaultOptions_NoMppsHost_ReturnsFailure()
    {
        // Default DicomOptions has MppsHost = string.Empty
        var sut = CreateSut();

        var result = await sut.SendInProgressAsync("1.2.3", "P001", "ABD");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host");
    }

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_WhitespaceMppsHost_ReturnsDicomConnectionFailed()
    {
        // Whitespace is not null or empty — it passes the guard and reaches network layer.
        var options = ValidOptions();
        options.MppsHost = "   ";
        var sut = CreateSut(options);

        // Will attempt network call and fail at the network level, not config guard.
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ── SendInProgressAsync — Network Error Paths ───────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_InvalidHost_ReturnsFailure()
    {
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_UnreachablePort_ReturnsNetworkError()
    {
        // Using a high unreachable port to exercise the network failure path.
        var options = ValidOptions();
        options.MppsHost = "127.0.0.1";
        options.MppsPort = 19999;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        // fo-dicom will fail to connect — typically throws AggregateException or similar.
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
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

    // ── SendInProgressAsync — TLS Configuration ─────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_TlsEnabled_ReturnsFailure()
    {
        // Ensures the TLS path reaches the network layer (no config guard blocks it).
        var options = ValidOptions();
        options.TlsEnabled = true;
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ── SendInProgressAsync — Parameter Variations ──────────────────────────

    [Theory]
    [InlineData("CHEST")]
    [InlineData("ABD")]
    [InlineData("EXTREMITY")]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_DifferentBodyParts_WithNoHost_ReturnsFailure(string bodyPart)
    {
        var options = ValidOptions();
        options.MppsHost = null!;
        var sut = CreateSut(options);

        var result = await sut.SendInProgressAsync("1.2.3.4.5", "PAT001", bodyPart);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
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

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_DefaultOptions_NoMppsHost_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.SendCompletedAsync("1.2.3.4.5");

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("MPPS host");
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
    public async Task SendCompletedAsync_InvalidHost_ReturnsFailure()
    {
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_UnreachablePort_ReturnsNetworkError()
    {
        var options = ValidOptions();
        options.MppsHost = "127.0.0.1";
        options.MppsPort = 19999;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
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
    public async Task SendCompletedAsync_CompletedTrue_InvalidHost_ReturnsFailure()
    {
        // Verifies the completed=true path attempts network call.
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7", completed: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_CompletedFalse_InvalidHost_ReturnsFailure()
    {
        // Verifies the completed=false path also attempts network call.
        var options = ValidOptions();
        options.MppsHost = "0.0.0.0";
        options.MppsPort = 1;
        var sut = CreateSut(options);

        var result = await sut.SendCompletedAsync("1.2.3.4.5.6.7", completed: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_CompletedFalse_PreCancelledToken_ReturnsResultWithoutThrowing()
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
    public async Task SendInProgressAsync_MisconfiguredHost_ReturnsConsistentErrorCode()
    {
        // Both null and empty host should produce the same error code.
        var options1 = ValidOptions();
        options1.MppsHost = null!;
        var sut1 = CreateSut(options1);

        var options2 = ValidOptions();
        options2.MppsHost = string.Empty;
        var sut2 = CreateSut(options2);

        var result1 = await sut1.SendInProgressAsync("1.2.3", "P001", "CHEST");
        var result2 = await sut2.SendInProgressAsync("1.2.3", "P001", "CHEST");

        result1.Error.Should().Be(result2.Error);
        result1.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-DC-056")]
    public async Task SendCompletedAsync_MisconfiguredHost_ReturnsConsistentErrorCode()
    {
        var options1 = ValidOptions();
        options1.MppsHost = null!;
        var sut1 = CreateSut(options1);

        var options2 = ValidOptions();
        options2.MppsHost = string.Empty;
        var sut2 = CreateSut(options2);

        var result1 = await sut1.SendCompletedAsync("1.2.3.4.5");
        var result2 = await sut2.SendCompletedAsync("1.2.3.4.5");

        result1.Error.Should().Be(result2.Error);
        result1.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ── Multiple Instances Independence ─────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DC-055")]
    public async Task SendInProgressAsync_DifferentSutInstances_ReturnIndependently()
    {
        // Two separate SUT instances with different configs should behave independently.
        var configuredOptions = ValidOptions();
        configuredOptions.MppsHost = null!;
        var configuredSut = CreateSut(configuredOptions);

        var defaultSut = CreateSut(); // Default has empty MppsHost

        var result1 = await configuredSut.SendInProgressAsync("1.2.3", "P001", "CHEST");
        var result2 = await defaultSut.SendInProgressAsync("1.2.3", "P001", "CHEST");

        result1.IsFailure.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
        result1.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result2.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }
}
