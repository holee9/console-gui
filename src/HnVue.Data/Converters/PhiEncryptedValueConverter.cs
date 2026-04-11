using System.Linq.Expressions;
using HnVue.Common.Abstractions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HnVue.Data.Converters;

/// <summary>
/// EF Core value converter that encrypts/decrypts string fields using AES-256-GCM.
/// REQ-PHI-003, SWR-CS-080: Column-level PHI encryption.
/// </summary>
public sealed class PhiEncryptedValueConverter(IPhiEncryptionService encryptionService)
    : ValueConverter<string, string>(
        v => encryptionService.Encrypt(v),
        v => encryptionService.Decrypt(v),
        new ConverterMappingHints(size: 1024))
{
}
