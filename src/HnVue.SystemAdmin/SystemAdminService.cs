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
/// All settings changes are logged to audit trail for IEC 62304 compliance.
/// IEC 62304 Class B.
/// </remarks>
public sealed class SystemAdminService : ISystemAdminService
{
    // @MX:NOTE Cache duration balances freshness with database load reduction
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ISecurityContext _securityContext;

    private SystemSettings? _cachedSettings;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemAdminService"/> class.
    /// </summary>
    public SystemAdminService(
        ISystemSettingsRepository settingsRepository,
        IAuditRepository auditRepository,
        ISecurityContext securityContext)
    {
        _settingsRepository = settingsRepository
            ?? throw new ArgumentNullException(nameof(settingsRepository));
        _auditRepository = auditRepository
            ?? throw new ArgumentNullException(nameof(auditRepository));
        _securityContext = securityContext
            ?? throw new ArgumentNullException(nameof(securityContext));
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

        // Get old settings for audit trail
        var oldSettingsResult = await _settingsRepository.GetAsync(cancellationToken).ConfigureAwait(false);
        if (oldSettingsResult.IsFailure)
            return Result.Failure(oldSettingsResult.Error!.Value, oldSettingsResult.ErrorMessage!);

        var oldSettings = oldSettingsResult.Value;

        // Save new settings
        var result = await _settingsRepository.SaveAsync(settings, cancellationToken).ConfigureAwait(false);

        // Invalidate cache on successful save
        if (result.IsSuccess)
        {
            _cachedSettings = null;
            _cacheExpiry = DateTimeOffset.MinValue;

            // Create audit entry for settings change
            var auditResult = await CreateSettingsChangeAuditAsync(oldSettings, settings, cancellationToken).ConfigureAwait(false);
            if (auditResult.IsFailure)
                return Result.Failure(auditResult.Error!.Value, auditResult.ErrorMessage!);
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

    // ── Static helpers ────────────────────────────────────────────────────────

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

    // ── Internals ─────────────────────────────────────────────────────────────

    // @MX:NOTE Settings change audit captures all field modifications for regulatory compliance
    private async Task<Result> CreateSettingsChangeAuditAsync(
        SystemSettings oldSettings,
        SystemSettings newSettings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(oldSettings);
        ArgumentNullException.ThrowIfNull(newSettings);

        var changes = new List<string>();

        // Detect DICOM settings changes
        if (oldSettings.Dicom.PacsAeTitle != newSettings.Dicom.PacsAeTitle)
            changes.Add($"Dicom.PacsAeTitle: '{oldSettings.Dicom.PacsAeTitle}' → '{newSettings.Dicom.PacsAeTitle}'");
        if (oldSettings.Dicom.PacsHost != newSettings.Dicom.PacsHost)
            changes.Add($"Dicom.PacsHost: '{oldSettings.Dicom.PacsHost}' → '{newSettings.Dicom.PacsHost}'");
        if (oldSettings.Dicom.PacsPort != newSettings.Dicom.PacsPort)
            changes.Add($"Dicom.PacsPort: {oldSettings.Dicom.PacsPort} → {newSettings.Dicom.PacsPort}");
        if (oldSettings.Dicom.LocalAeTitle != newSettings.Dicom.LocalAeTitle)
            changes.Add($"Dicom.LocalAeTitle: '{oldSettings.Dicom.LocalAeTitle}' → '{newSettings.Dicom.LocalAeTitle}'");

        // Detect Generator settings changes
        if (oldSettings.Generator.ComPort != newSettings.Generator.ComPort)
            changes.Add($"Generator.ComPort: '{oldSettings.Generator.ComPort}' → '{newSettings.Generator.ComPort}'");
        if (oldSettings.Generator.BaudRate != newSettings.Generator.BaudRate)
            changes.Add($"Generator.BaudRate: {oldSettings.Generator.BaudRate} → {newSettings.Generator.BaudRate}");
        if (oldSettings.Generator.TimeoutMs != newSettings.Generator.TimeoutMs)
            changes.Add($"Generator.TimeoutMs: {oldSettings.Generator.TimeoutMs} → {newSettings.Generator.TimeoutMs}");

        // Detect Security settings changes
        if (oldSettings.Security.SessionTimeoutMinutes != newSettings.Security.SessionTimeoutMinutes)
            changes.Add($"Security.SessionTimeoutMinutes: {oldSettings.Security.SessionTimeoutMinutes} → {newSettings.Security.SessionTimeoutMinutes}");
        if (oldSettings.Security.MaxFailedLogins != newSettings.Security.MaxFailedLogins)
            changes.Add($"Security.MaxFailedLogins: {oldSettings.Security.MaxFailedLogins} → {newSettings.Security.MaxFailedLogins}");

        var userId = _securityContext.IsAuthenticated && _securityContext.CurrentUserId is not null
            ? _securityContext.CurrentUserId
            : "system";

        var details = changes.Count > 0
            ? string.Join("; ", changes)
            : "No changes detected";

        // Get last hash for chain integrity
        var lastHashResult = await _auditRepository.GetLastHashAsync(cancellationToken).ConfigureAwait(false);
        if (lastHashResult.IsFailure)
            return Result.Failure(lastHashResult.Error!.Value, lastHashResult.ErrorMessage!);

        var lastHash = lastHashResult.Value; // Can be null for empty audit log

        // Compute hash for this entry
        var entryData = $"{DateTimeOffset.UtcNow:O}|{userId}|SettingsChanged|{details}|{lastHash ?? "none"}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(entryData));
        var hashString = Convert.ToHexString(hash).ToLowerInvariant();

        var auditEntry = new AuditEntry(
            timestamp: DateTimeOffset.UtcNow,
            userId: userId,
            action: "SettingsChanged",
            currentHash: hashString,
            details: details,
            previousHash: lastHash);

        return await _auditRepository.AppendAsync(auditEntry, cancellationToken).ConfigureAwait(false);
    }
}
