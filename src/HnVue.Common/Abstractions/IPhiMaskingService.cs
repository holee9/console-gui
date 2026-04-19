namespace HnVue.Common.Abstractions;

/// <summary>
/// Provides PHI (Protected Health Information) field masking for display purposes.
/// Implements STRIDE 'I' (Information Disclosure) control per WBS 5.1.17.
/// </summary>
public interface IPhiMaskingService
{
    /// <summary>Masks a patient name for non-privileged display (e.g., "김***").</summary>
    string MaskName(string name);

    /// <summary>Masks a national ID / registration number (e.g., "900101-*******").</summary>
    string MaskNationalId(string nationalId);

    /// <summary>Masks a phone number (e.g., "010-1234-****").</summary>
    string MaskPhone(string phone);

    /// <summary>Masks a date of birth to year-only (e.g., "1990-**-**").</summary>
    string MaskDateOfBirth(string dateOfBirth);

    /// <summary>Masks all PHI fields in a patient record string representation.</summary>
    string MaskPatientDisplay(string patientInfo);
}
