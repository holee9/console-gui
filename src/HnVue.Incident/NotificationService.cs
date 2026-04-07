using HnVue.Common.Enums;
using HnVue.Incident.Models;
using Microsoft.Extensions.Logging;

namespace HnVue.Incident;

// @MX:TODO Wave 4 requires external notification channels (email, SMS, PACS alert)
// @MX:NOTE LoggerMessage delegates provide high-performance structured logging (CA1848)
/// <summary>
/// Provides severity-appropriate logging notifications for incident records.
/// Wave 2 implementation: external notification channels (email, SMS, PACS alert) are deferred.
/// </summary>
internal sealed partial class NotificationService(ILogger<NotificationService> logger)
{
    private readonly ILogger<NotificationService> _logger = logger;

    // ── High-performance LoggerMessage delegates (CA1848) ─────────────────────

    [LoggerMessage(Level = LogLevel.Error,
        Message = "CRITICAL INCIDENT [{IncidentId}] reported by {UserId} — Category: {Category} — {Description}")]
    private partial void LogCritical(string incidentId, string userId, string category, string description);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "HIGH INCIDENT [{IncidentId}] reported by {UserId} — Category: {Category} — {Description}")]
    private partial void LogHigh(string incidentId, string userId, string category, string description);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "MEDIUM INCIDENT [{IncidentId}] reported by {UserId} — Category: {Category} — {Description}")]
    private partial void LogMedium(string incidentId, string userId, string category, string description);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "LOW INCIDENT [{IncidentId}] reported by {UserId} — Category: {Category} — {Description}")]
    private partial void LogLow(string incidentId, string userId, string category, string description);

    // @MX:ANCHOR Notify - @MX:REASON: Called by ReportAsync for all incidents
    /// <summary>
    /// Emits a log notification appropriate to the incident's severity level.
    /// Critical and High incidents are logged at error/warning level to ensure
    /// they surface in production log aggregators and monitoring dashboards.
    /// </summary>
    /// <param name="incident">The incident record to notify about.</param>
    public void Notify(IncidentRecord incident)
    {
        switch (incident.Severity)
        {
            case IncidentSeverity.Critical:
                LogCritical(incident.IncidentId, incident.ReportedByUserId, incident.Category, incident.Description);
                break;

            case IncidentSeverity.High:
                LogHigh(incident.IncidentId, incident.ReportedByUserId, incident.Category, incident.Description);
                break;

            case IncidentSeverity.Medium:
                LogMedium(incident.IncidentId, incident.ReportedByUserId, incident.Category, incident.Description);
                break;

            default:
                LogLow(incident.IncidentId, incident.ReportedByUserId, incident.Category, incident.Description);
                break;
        }
    }
}
