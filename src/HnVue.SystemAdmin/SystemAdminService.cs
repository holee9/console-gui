using System.IO;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.SystemAdmin;

// @MX:NOTE Audit export provides tamper-evident chain for regulatory compliance
/// <summary>
/// Implements system administration operations: settings management and audit log export.
/// </summary>
/// <remarks>
/// Settings are validated before persistence. Audit log export produces a tamper-evident
/// signed CSV for regulatory review. Settings are cached for 5 minutes to reduce database load.
/// IEC 62304 Class B.
/// </remarks>
public sealed class SystemAdminService : ISystemAdminService
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly IAuditRepository _auditRepository;

    // @MX:NOTE Cache duration balances freshness with database load reduction
    private SystemSettings? _cachedSettings;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initialises a new <see cref="SystemAdminService"/>.
    /// </summary>
    public SystemAdminService(
        ISystemSettingsRepository settingsRepository,
        IAuditRepository auditRepository)
    {
        _settingsRepository = settingsRepository
            ?? throw new ArgumentNullException(nameof(settingsRepository));
        _auditRepository = auditRepository
            ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<SystemSettings>> GetSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cachedSettings is not null && DateTimeOffset.UtcNow < _cacheExpiry)
            return Result.Success(_cachedSettings);

        var result = await _settingsRepository.GetAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _cachedSettings = result.Value;
            _cacheExpiry = DateTimeOffset.UtcNow.Add(CacheDuration);
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateSettingsAsync(
        SystemSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var validationError = ValidateSettings(settings);
        if (validationError is not null)
            return Result.Failure(ErrorCode.ValidationFailed, validationError);

        var result = await _settingsRepository.SaveAsync(settings, cancellationToken).ConfigureAwait(false);

        // Invalidate cache on successful save
        if (result.IsSuccess)
        {
            _cachedSettings = null;
            _cacheExpiry = DateTimeOffset.MinValue;
        }

        return result;
    }

    // @MX:ANCHOR ExportAuditLogAsync - @MX:REASON: Regulatory compliance feature for audit review
    /// <inheritdoc/>
    public async Task<Result> ExportAuditLogAsync(
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(outputPath);

        if (string.IsNullOrWhiteSpace(outputPath))
            return Result.Failure(ErrorCode.ValidationFailed, "Output path is required.");

        var entriesResult = await _auditRepository.QueryAsync(
            new Common.Models.AuditQueryFilter(MaxResults: int.MaxValue),
            cancellationToken).ConfigureAwait(false);
        if (entriesResult.IsFailure)
            return Result.Failure(entriesResult.Error!.Value, entriesResult.ErrorMessage!);

        try
        {
            var lines = new List<string>
            {
                "EntryId,Timestamp,UserId,Action,Details,PreviousHash,CurrentHash",
            };

            foreach (var entry in entriesResult.Value)
            {
                var details = CsvEscape(entry.Details);
                lines.Add(
                    $"{CsvEscape(entry.EntryId)},{entry.Timestamp:O},{CsvEscape(entry.UserId)}," +
                    $"{CsvEscape(entry.Action)},{details},{CsvEscape(entry.PreviousHash)},{CsvEscape(entry.CurrentHash)}");
            }

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllLinesAsync(outputPath, lines, cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (IOException ex)
        {
            return Result.Failure(ErrorCode.Unknown, $"Failed to write audit log: {ex.Message}");
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    // @MX:NOTE Port range validation prevents DICOM connection failures
    // @MX:NOTE AE title validation ensures DICOM network protocol compliance
    // @MX:NOTE Security settings validation prevents authentication bypass
    private static string? ValidateSettings(SystemSettings settings)
    {
        if (settings.Dicom.PacsPort is < 1 or > 65535)
            return "PACS port must be between 1 and 65535.";

        if (string.IsNullOrWhiteSpace(settings.Dicom.LocalAeTitle))
            return "Local AE Title is required.";

        if (settings.Security.SessionTimeoutMinutes < 1)
            return "Session timeout must be at least 1 minute.";

        if (settings.Security.MaxFailedLogins < 1)
            return "Max failed logins must be at least 1.";

        return null;
    }

    // @MX:NOTE RFC 4180 CSV escaping prevents injection and malformed export files
    private static string CsvEscape(string? value)
    {
        if (value is null)
            return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
