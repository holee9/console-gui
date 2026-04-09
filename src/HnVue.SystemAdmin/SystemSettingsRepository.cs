using System.IO;
using System.Text.Json;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.SystemAdmin;

/// <summary>
/// File-based JSON implementation of <see cref="ISystemSettingsRepository"/>.
/// Settings are stored at <c>%AppData%\HnVue\settings.json</c>.
/// A missing file is treated as first-run and returns default settings.
/// </summary>
public class SystemSettingsRepository : ISystemSettingsRepository
{
    /// <summary>
    /// Gets the file path where settings are stored.
    /// Can be overridden in test subclasses to use temp directories.
    /// </summary>
    protected virtual string GetStorePath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HnVue",
            "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    /// <inheritdoc/>
    /// <remarks>SWR-DA-030: System settings must be loaded at application startup.</remarks>
    public async Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var storePath = GetStorePath();

            if (!File.Exists(storePath))
                return Result.Success(new SystemSettings());

            await using var stream = File.OpenRead(storePath);
            var settings = await JsonSerializer
                .DeserializeAsync<SystemSettings>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            return Result.Success(settings ?? new SystemSettings());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return Result.Failure<SystemSettings>(ErrorCode.FileOperationFailed,
                $"Failed to load system settings: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-031: Settings changes must be persisted atomically.</remarks>
    public async Task<Result> SaveAsync(SystemSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var storePath = GetStorePath();
            var dir = Path.GetDirectoryName(storePath)!;
            Directory.CreateDirectory(dir);

            // Write to temp file first, then replace to avoid partial writes.
            var tempPath = storePath + ".tmp";
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer
                    .SerializeAsync(stream, settings, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(tempPath, storePath, overwrite: true);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Result.Failure(ErrorCode.FileOperationFailed,
                $"Failed to save system settings: {ex.Message}");
        }
    }
}
