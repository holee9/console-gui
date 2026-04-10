using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;

namespace HnVue.Update;

/// <summary>
/// Queries the configured update server to determine whether a newer software version is available.
/// </summary>
internal sealed class UpdateChecker
{
    private readonly UpdateOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdateChecker>? _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="UpdateChecker"/>.
    /// </summary>
    /// <param name="options">Update configuration options.</param>
    /// <param name="httpClient">HTTP client used to contact the update server.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when UpdateServerUrl in options is null, empty, or uses HTTP instead of HTTPS.
    /// </exception>
    public UpdateChecker(UpdateOptions options, HttpClient httpClient, ILogger<UpdateChecker>? logger = null)
    {
        _options = options;
        _httpClient = httpClient;
        _logger = logger;

        // Validate URL scheme in constructor
        options.Validate();
    }

    /// <summary>
    /// Queries the update server for the latest available version.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing an <see cref="UpdateInfo"/> when a newer version
    /// is available, or <see langword="null"/> when the current version is already up to date.
    /// Returns a failure result when the server is unreachable or returns invalid data.
    /// </returns>
    public async Task<Result<UpdateInfo?>> CheckAsync(CancellationToken ct = default)
    {
        string url = $"{_options.UpdateServerUrl.TrimEnd('/')}/updates/latest";
        _logger?.LogInformation("Checking for updates at {Url}", url);

        try
        {
            UpdateServerResponse? response = await _httpClient
                .GetFromJsonAsync<UpdateServerResponse>(url, ct)
                .ConfigureAwait(false);

            if (response is null)
            {
                _logger?.LogWarning("Update server returned null response");
                return Result.SuccessNullable<UpdateInfo?>(null);
            }

            if (!IsNewerVersion(response.Version, _options.CurrentVersion))
            {
                _logger?.LogInformation(
                    "No update available. Current={Current}, Available={Available}",
                    _options.CurrentVersion, response.Version);
                return Result.SuccessNullable<UpdateInfo?>(null);
            }

            var updateInfo = new UpdateInfo(
                response.Version,
                response.ReleaseNotes,
                response.PackageUrl,
                response.Sha256Hash);

            _logger?.LogInformation("Update available: {Version}", response.Version);
            return Result.SuccessNullable<UpdateInfo?>(updateInfo);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<UpdateInfo?>(ErrorCode.OperationCancelled, "Update check was cancelled.");
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error checking for updates");
            return Result.Failure<UpdateInfo?>(ErrorCode.ValidationFailed,
                $"Failed to contact update server: {ex.Message}");
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidOperationException)
        {
            _logger?.LogError(ex, "Invalid JSON response from update server");
            return Result.Failure<UpdateInfo?>(ErrorCode.ValidationFailed,
                $"Update server returned invalid data: {ex.Message}");
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="available"/> is strictly newer than <paramref name="current"/>.
    /// Uses <see cref="Version"/> for semantic comparison; falls back to ordinal string comparison.
    /// </summary>
    private static bool IsNewerVersion(string available, string current)
    {
        if (Version.TryParse(available, out Version? availableVer) &&
            Version.TryParse(current, out Version? currentVer))
        {
            return availableVer > currentVer;
        }

        // Fallback: ordinal string comparison (covers pre-release labels not parseable by Version)
        return string.Compare(available, current, StringComparison.Ordinal) > 0;
    }

    // ── DTO ───────────────────────────────────────────────────────────────────

    /// <summary>JSON response model returned by the update server.</summary>
    private sealed class UpdateServerResponse
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("releaseNotes")]
        public string? ReleaseNotes { get; set; }

        [JsonPropertyName("packageUrl")]
        public string PackageUrl { get; set; } = string.Empty;

        [JsonPropertyName("sha256Hash")]
        public string Sha256Hash { get; set; } = string.Empty;
    }
}
