using HnVue.Common.Abstractions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HnVue.Data.Converters;

/// <summary>
/// EF Core value converter that encrypts/decrypts string PHI fields at rest.
/// REQ-PHI-003: Column-level encryption for protected health information.
/// </summary>
public sealed class PhiEncryptedValueConverter : ValueConverter<string, string>
{
    public PhiEncryptedValueConverter(IPhiEncryptionService encryptionService)
        : base(
            v => encryptionService.Encrypt(v),
            v => encryptionService.Decrypt(v),
            convertsNulls: false)
    {
    }
}

/// <summary>
/// EF Core value converter that encrypts/decrypts nullable DateOnly? PHI fields at rest.
/// Stores encrypted value as string; null values pass through unchanged.
/// </summary>
public sealed class NullablePhiEncryptedValueConverter : ValueConverter<DateOnly?, string?>
{
    public NullablePhiEncryptedValueConverter(IPhiEncryptionService encryptionService)
        : base(
            v => v.HasValue ? encryptionService.Encrypt(v.Value.ToString("yyyyMMdd")) : null,
            v => v != null ? DateOnly.ParseExact(encryptionService.Decrypt(v), "yyyyMMdd") : null,
            convertsNulls: true)
    {
    }
}
