namespace HnVue.Security;

/// <summary>
/// Configuration options for JWT token issuance and validation.
/// Bind from the "Jwt" section of application configuration.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>The configuration section name used for binding.</summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the HMAC-SHA256 signing secret. Minimum 32 characters required.
    /// WARNING: The default value is for development use only.
    /// In production, this MUST be overridden with a securely generated, environment-specific secret
    /// supplied via configuration (e.g., environment variable or secrets manager).
    /// </summary>
    public string SecretKey { get; set; } = "HnVue-Dev-Secret-Key-32CharMinimum!";

    /// <summary>Gets or sets the token validity window in minutes. Defaults to 15 minutes.</summary>
    public int ExpiryMinutes { get; set; } = 15;

    /// <summary>Gets or sets the JWT issuer claim value.</summary>
    public string Issuer { get; set; } = "HnVue";

    /// <summary>Gets or sets the JWT audience claim value.</summary>
    public string Audience { get; set; } = "HnVue";
}
