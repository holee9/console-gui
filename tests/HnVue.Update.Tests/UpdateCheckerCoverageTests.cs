using System.Net;
using System.Net.Http;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Coverage tests for <see cref="UpdateChecker"/>.
/// Targets CheckAsync response handling, version comparison, and error paths.
/// </summary>
public sealed class UpdateCheckerCoverageTests
{
    private static UpdateOptions CreateOptions(string currentVersion = "1.0.0")
    {
        return new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            CurrentVersion = currentVersion,
            RequireAuthenticodeSignature = false,
        };
    }

    private static UpdateChecker CreateSut(
        UpdateOptions? options = null,
        HttpClient? httpClient = null)
    {
        return new UpdateChecker(
            options ?? CreateOptions(),
            httpClient ?? new HttpClient(),
            NullLogger<UpdateChecker>.Instance);
    }

    // ── Constructor Validation ───────────────────────────────────────────────

    [Fact]
    public void Constructor_EmptyUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = string.Empty, RequireAuthenticodeSignature = false };
        var act = () => new UpdateChecker(options, new HttpClient(), null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_HttpUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = "http://update.example.com", RequireAuthenticodeSignature = false };
        var act = () => new UpdateChecker(options, new HttpClient(), null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTPS*");
    }

    [Fact]
    public void Constructor_InvalidScheme_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.example.com", RequireAuthenticodeSignature = false };
        var act = () => new UpdateChecker(options, new HttpClient(), null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*valid HTTPS*");
    }

    // ── CheckAsync Error Paths ──────────────────────────────────────────────

    [Fact]
    public async Task CheckAsync_UnreachableServer_ReturnsFailure()
    {
        var sut = CreateSut(CreateOptions());
        var result = await sut.CheckAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CheckAsync_CancelledToken_ReturnsCancelled()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await sut.CheckAsync(cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }
}
