namespace HnVue.Security;

/// <summary>
/// Configuration options for the tamper-evident audit logging service.
/// The HMAC key must be supplied externally via configuration or environment variable.
/// </summary>
public sealed class AuditOptions
{
    /// <summary>The configuration section name used for binding.</summary>
    public const string SectionName = "Security";

    /// <summary>
    /// Gets or sets the HMAC-SHA256 signing key for audit hash chain computation.
    /// Minimum 32 characters required. Must be supplied via configuration or
    /// environment variable (<c>HNVUE_AUDIT_HMAC_KEY</c>).
    /// </summary>
    public string HmacKey { get; set; } = string.Empty;
}
