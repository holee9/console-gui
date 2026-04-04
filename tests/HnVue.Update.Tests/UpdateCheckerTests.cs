using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="UpdateChecker"/> using a mock <see cref="HttpMessageHandler"/>
/// to avoid real network calls.
/// </summary>
public sealed class UpdateCheckerTests
{
    private static UpdateOptions DefaultOptions(string currentVersion = "1.0.0") => new()
    {
        UpdateServerUrl = "https://update.hnvue.com/api/v1",
        CurrentVersion = currentVersion
    };

    private static HttpClient BuildHttpClient(HttpStatusCode statusCode, string? jsonBody)
    {
        var handler = new StubHttpMessageHandler(statusCode, jsonBody ?? string.Empty);
        return new HttpClient(handler);
    }

    private static string NewerVersionJson(string version = "1.1.0") => $$"""
        {
          "version": "{{version}}",
          "releaseNotes": "Bug fixes and improvements",
          "packageUrl": "https://cdn.hnvue.com/updates/1.1.0.zip",
          "sha256Hash": "abc123def456abc123def456abc123def456abc123def456abc123def456abc12345"
        }
        """;

    // ── CheckAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckAsync_NewerVersionAvailable_ReturnsUpdateInfo()
    {
        // Arrange
        var options = DefaultOptions("1.0.0");
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, NewerVersionJson("1.1.0"));
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.1.0");
        result.Value.PackageUrl.Should().Contain("1.1.0");
    }

    [Fact]
    public async Task CheckAsync_SameVersion_ReturnsNull()
    {
        // Arrange
        var options = DefaultOptions("1.1.0");
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, NewerVersionJson("1.1.0"));
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsSuccess.Should().BeTrue("same version means no update — still a success");
        result.Value.Should().BeNull("no update is available when versions match");
    }

    [Fact]
    public async Task CheckAsync_OlderVersionOnServer_ReturnsNull()
    {
        // Arrange
        var options = DefaultOptions("2.0.0");
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, NewerVersionJson("1.9.9"));
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull("server version is older than current — no update needed");
    }

    [Fact]
    public async Task CheckAsync_NetworkError_ReturnsFailure()
    {
        // Arrange
        var options = DefaultOptions();
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        using var httpClient = new HttpClient(handler);
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("Failed to contact update server");
    }

    [Fact]
    public async Task CheckAsync_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var options = DefaultOptions();
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, "{ invalid json !!!");
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task CheckAsync_NullResponse_ReturnsSuccessWithNull()
    {
        // Arrange
        var options = DefaultOptions();
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, "null");
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckAsync_Cancelled_ReturnsOperationCancelledFailure()
    {
        // Arrange
        var options = DefaultOptions();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var handler = new DelayedHttpMessageHandler(TimeSpan.FromSeconds(10));
        using var httpClient = new HttpClient(handler);
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync(cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    [Fact]
    public async Task CheckAsync_ReleaseNotesArePreserved()
    {
        // Arrange
        var options = DefaultOptions("1.0.0");
        using var httpClient = BuildHttpClient(HttpStatusCode.OK, NewerVersionJson("2.0.0"));
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<HnVue.Common.Models.UpdateInfo?> result = await sut.CheckAsync();

        // Assert
        result.Value!.ReleaseNotes.Should().Be("Bug fixes and improvements");
    }

    // ── Stub helpers ───────────────────────────────────────────────────────────

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception exception)
            => _exception = exception;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(_exception);
    }

    private sealed class DelayedHttpMessageHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;

        public DelayedHttpMessageHandler(TimeSpan delay)
            => _delay = delay;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
