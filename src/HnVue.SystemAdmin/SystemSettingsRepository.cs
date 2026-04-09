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
public sealed class SystemSettingsRepository : ISystemSettingsRepository
{
    private static readonly string DefaultStorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HnVue",
        "settings.json");

    private readonly string _storePath;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Initialises a new instance using the default AppData storage path.
    /// </summary>
    public SystemSettingsRepository()
        : this(DefaultStorePath)
    {
    }

    /// <summary>
    /// Initialises a new instance with an explicit storage path. Used for testing.
    /// </summary>
    /// <param name="storePath">Full path to the settings JSON file.</param>
    internal SystemSettingsRepository(string storePath)
    {
        ArgumentNullException.ThrowIfNull(storePath);
        _storePath = storePath;
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-030: System settings must be loaded at application startup.</remarks>
    public async Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_storePath))
                return Result.Success(new SystemSettings());

            await using var stream = File.OpenRead(_storePath);
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
            return Result.Failure<SystemSettings>(ErrorCode.DatabaseError,
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
            var dir = Path.GetDirectoryName(_storePath)!;
            Directory.CreateDirectory(dir);

            // Write to temp file first, then replace to avoid partial writes.
            var tempPath = _storePath + ".tmp";
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer
                    .SerializeAsync(stream, settings, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(tempPath, _storePath, overwrite: true);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Result.Failure(ErrorCode.DatabaseError,
                $"Failed to save system settings: {ex.Message}");
        }
    }
}
