using HnVue.Common.Abstractions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HnVue.Data.Converters;

/// <summary>
/// EF Core value converter that encrypts/decrypts nullable string fields using AES-256-GCM.
/// REQ-PHI-003, SWR-CS-080: Column-level PHI encryption for nullable columns.
/// </summary>
public sealed class NullablePhiEncryptedValueConverter(IPhiEncryptionService encryptionService)
    : ValueConverter<string?, string?>(
        v => v != null ? encryptionService.Encrypt(v) : null,
        v => v != null ? encryptionService.Decrypt(v) : null,
        new ConverterMappingHints(size: 1024))
{
}
