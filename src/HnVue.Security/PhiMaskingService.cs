using System.Text.RegularExpressions;
using HnVue.Common.Abstractions;

namespace HnVue.Security;

/// <summary>
/// Masks PHI fields for non-privileged display contexts.
/// STRIDE 'I' (Information Disclosure) control per WBS 5.1.17.
/// </summary>
public sealed class PhiMaskingService : IPhiMaskingService
{
    // Korean name pattern: 2-3 syllables + optional multi-syllable given name
    private static readonly Regex KoreanNameRegex = new(@"^([가-힣])([가-힣]+)$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex NationalIdRegex = new(@"^(\d{6})-?(\d{7})$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex PhoneRegex = new(@"^(\d{2,3})-?(\d{3,4})-?(\d{4})$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly Regex DateRegex = new(@"^(\d{4})-?(\d{2})-?(\d{2})$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));

    /// <inheritdoc/>
    public string MaskName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;

        var match = KoreanNameRegex.Match(name);
        if (match.Success)
            return $"{match.Groups[1].Value}{new string('*', match.Groups[2].Value.Length)}";

        // Non-Korean: show first character, mask rest
        return name.Length <= 1 ? "*" : $"{name[0]}{new string('*', name.Length - 1)}";
    }

    /// <inheritdoc/>
    public string MaskNationalId(string nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId)) return nationalId;

        var match = NationalIdRegex.Match(nationalId);
        if (match.Success)
            return $"{match.Groups[1].Value}-*******";

        // Fallback: mask all but first 6 chars
        return nationalId.Length <= 6 ? new string('*', nationalId.Length) : $"{nationalId[..6]}{new string('*', nationalId.Length - 6)}";
    }

    /// <inheritdoc/>
    public string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;

        var match = PhoneRegex.Match(phone);
        if (match.Success)
            return $"{match.Groups[1].Value}-{match.Groups[2].Value}-****";

        // Fallback: mask last 4 digits
        return phone.Length <= 4 ? new string('*', phone.Length) : $"{phone[..^4]}****";
    }

    /// <inheritdoc/>
    public string MaskDateOfBirth(string dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(dateOfBirth)) return dateOfBirth;

        var match = DateRegex.Match(dateOfBirth);
        if (match.Success)
            return $"{match.Groups[1].Value}-**-**";

        return dateOfBirth;
    }

    /// <inheritdoc/>
    public string MaskPatientDisplay(string patientInfo)
    {
        if (string.IsNullOrWhiteSpace(patientInfo)) return patientInfo;
        return patientInfo;
    }
}
