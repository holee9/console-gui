namespace HnVue.Security;

// @MX:NOTE JwtOptions - JWT config with SecretKey (32+ chars), 15-min expiry, issuer/audience claims
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
    /// Must be supplied via configuration (<c>Security:JwtSecretKey</c>) or
    /// environment variable (<c>HNVUE_JWT_SECRET</c>). No default is provided.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the token validity window in minutes. Defaults to 15 minutes.</summary>
    public int ExpiryMinutes { get; set; } = 15;

    /// <summary>Gets or sets the JWT issuer claim value.</summary>
    public string Issuer { get; set; } = "HnVue";

    /// <summary>Gets or sets the JWT audience claim value.</summary>
    public string Audience { get; set; } = "HnVue";

    /// <summary>
    /// Gets or sets the previous HMAC-SHA256 signing secret for key rotation.
    /// When set, tokens signed with this key are also accepted during the transition period.
    /// Set this to the old key when rotating <see cref="SecretKey"/> to a new value.
    /// </summary>
    public string? PreviousSecretKey { get; set; }
}
