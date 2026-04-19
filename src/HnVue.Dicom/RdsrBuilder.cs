// Copyright (c) H&abyz. All rights reserved.

using System.Globalization;
using FellowOakDicom;
using FellowOakDicom.IO.Buffer;
using HnVue.Common.Models;

namespace HnVue.Dicom;

/// <summary>
/// Builds a DICOM Radiation Dose Structured Report (RDSR) dataset per IEC 62494-1
/// using the Enhanced SR IOD (SOP Class 1.2.840.10008.5.1.4.1.1.88.22).
/// </summary>
/// <remarks>
/// The builder creates a valid DICOM SR document with:
/// <list type="bullet">
///   <item>Patient Module: PatientID, PatientName, PatientBirthDate, PatientSex</item>
///   <item>Study Module: StudyInstanceUID, StudyDate, StudyTime, AccessionNumber</item>
///   <item>SR Document General: SeriesInstanceUID, SOPInstanceUID, InstanceNumber</item>
///   <item>SR Document Content: CONTAINER root with TID 1001 Radiation Dose Report template</item>
///   <item>Dose Content: DAP, ESD, kVp, mAs, Exposure Time, Body Part, EI</item>
/// </list>
/// IEC 62304 Class B -- radiation dose regulatory reporting.
/// SWR-DC-060~065: RDSR generation and transmission.
/// </remarks>
public sealed class RdsrBuilder
{
    // ── DICOM SR SOP Class UIDs ──────────────────────────────────────────────
    // @MX:NOTE Enhanced SR SOP Class UID per DICOM PS3.4, used for RDSR per IEC 62494-1

    /// <summary>
    /// SOP Class UID for Enhanced SR (1.2.840.10008.5.1.4.1.1.88.22).
    /// This is the standard SOP class for Radiation Dose Structured Reports.
    /// </summary>
    public static readonly DicomUID EnhancedSrSopClassUid =
        DicomUID.Parse("1.2.840.10008.5.1.4.1.1.88.22");

    // ── DCM Code Values (DICOM Context Group 29) ─────────────────────────────
    private const string DcmScheme = "DCM";
    private const string CodeRdsrReport = "113812";
    private const string MeaningRdsrReport = "Radiation Dose Structured Report";
    private const string CodeIrradiationEvent = "113814";
    private const string MeaningIrradiationEvent = "Irradiation Event";
    private const string CodeDap = "113823";
    private const string MeaningDap = "Dose (RP) Total";
    private const string CodeEsd = "113824";
    private const string MeaningEsd = "Entrance Skin Dose";
    private const string CodeKvp = "113821";
    private const string MeaningKvp = "KVP";
    private const string CodeMas = "113822";
    private const string MeaningMas = "X Ray Tube Current";
    private const string CodeExposureTime = "113852";
    private const string MeaningExposureTime = "Exposure Time";
    private const string CodeBodyPart = "123009";
    private const string MeaningBodyPart = "Body Part";
    private const string CodeExposureIndex = "113840";
    private const string MeaningExposureIndex = "Exposure Index";

    // ── Input parameters ─────────────────────────────────────────────────────
    private readonly DoseRecord _doseRecord;
    private readonly RdsrPatientInfo _patientInfo;
    private readonly RdsrStudyInfo _studyInfo;
    private readonly RdsrExposureParams _exposureParams;
    private readonly DicomUID _sopInstanceUid;
    private readonly DicomUID _seriesInstanceUid;
    private readonly int _instanceNumber;

    /// <summary>
    /// Initializes a new instance of <see cref="RdsrBuilder"/> with the required data.
    /// </summary>
    /// <param name="doseRecord">The dose record containing exposure metrics.</param>
    /// <param name="patientInfo">Patient demographic information.</param>
    /// <param name="studyInfo">Study-level DICOM information.</param>
    /// <param name="exposureParams">Exposure technique parameters (kVp, mAs, exposure time). Null = not included.</param>
    /// <param name="sopInstanceUid">SOP Instance UID for this RDSR. Null = auto-generated.</param>
    /// <param name="seriesInstanceUid">Series Instance UID. Null = auto-generated.</param>
    /// <param name="instanceNumber">Instance number for this SR document.</param>
    public RdsrBuilder(
        DoseRecord doseRecord,
        RdsrPatientInfo patientInfo,
        RdsrStudyInfo studyInfo,
        RdsrExposureParams? exposureParams = null,
        DicomUID? sopInstanceUid = null,
        DicomUID? seriesInstanceUid = null,
        int instanceNumber = 1)
    {
        ArgumentNullException.ThrowIfNull(doseRecord);
        ArgumentNullException.ThrowIfNull(patientInfo);
        ArgumentNullException.ThrowIfNull(studyInfo);

        _doseRecord = doseRecord;
        _patientInfo = patientInfo;
        _studyInfo = studyInfo;
        _exposureParams = exposureParams ?? new RdsrExposureParams();
        _sopInstanceUid = sopInstanceUid ?? DicomUID.Generate();
        _seriesInstanceUid = seriesInstanceUid ?? DicomUID.Generate();
        _instanceNumber = instanceNumber;
    }

    /// <summary>
    /// Builds the complete DICOM RDSR dataset ready for C-STORE transmission.
    /// </summary>
    /// <returns>A <see cref="DicomDataset"/> representing the RDSR Enhanced SR document.</returns>
    public DicomDataset Build()
    {
        var dataset = new DicomDataset
        {
            // ── SOP Common Module ─────────────────────────────────────────
            { DicomTag.SOPClassUID, EnhancedSrSopClassUid },
            { DicomTag.SOPInstanceUID, _sopInstanceUid },
            { DicomTag.SpecificCharacterSet, "ISO_IR 192" },

            // ── Patient Module ────────────────────────────────────────────
            { DicomTag.PatientID, _patientInfo.PatientId ?? string.Empty },
            { DicomTag.PatientName, _patientInfo.PatientName ?? string.Empty },
            { DicomTag.PatientBirthDate, _patientInfo.PatientBirthDate ?? string.Empty },
            { DicomTag.PatientSex, _patientInfo.PatientSex ?? string.Empty },

            // ── Study Module ──────────────────────────────────────────────
            { DicomTag.StudyInstanceUID, _studyInfo.StudyInstanceUid },
            { DicomTag.StudyDate, _studyInfo.StudyDate ?? string.Empty },
            { DicomTag.StudyTime, _studyInfo.StudyTime ?? string.Empty },
            { DicomTag.AccessionNumber, _studyInfo.AccessionNumber ?? string.Empty },
            { DicomTag.StudyID, _studyInfo.StudyId ?? string.Empty },
            { DicomTag.ReferringPhysicianName, _studyInfo.ReferringPhysicianName ?? string.Empty },

            // ── SR Document General Module ────────────────────────────────
            { DicomTag.SeriesInstanceUID, _seriesInstanceUid },
            { DicomTag.SeriesNumber, 1 },
            { DicomTag.InstanceNumber, _instanceNumber },
            { DicomTag.RetrieveAETitle, _studyInfo.RetrieveAeTitle ?? string.Empty },

            // ── SR Document Content Module (root) ─────────────────────────
            // Value Type = CONTAINER (root of the SR document tree)
            { DicomTag.ValueType, "CONTAINER" },
            { DicomTag.ContentDate, DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) },
            { DicomTag.ContentTime, DateTime.UtcNow.ToString("HHmmss.ffffff", CultureInfo.InvariantCulture) },
        };

        // Concept Name Code Sequence: identifies this as RDSR (TID 1001)
        dataset.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence,
            BuildCodeItem(CodeRdsrReport, MeaningRdsrReport)));

        // Add the content tree (TID 1001 Radiation Dose Report children)
        var contentSequence = BuildContentSequence();
        dataset.Add(contentSequence);

        // ── Verification (optional, recommended) ──────────────────────────
        // Indicates the document is verified by the device
        dataset.Add(new DicomSequence(DicomTag.VerifyingObserverSequence,
            new DicomDataset
            {
                { DicomTag.VerifyingOrganization, "H&abyz" },
                { DicomTag.VerificationDateTime,
                    DateTime.UtcNow.ToString("yyyyMMddHHmmss.ffffff", CultureInfo.InvariantCulture) },
            }));

        return dataset;
    }

    /// <summary>
    /// Builds the content sequence containing all irradiation event data.
    /// TID 1001 (Radiation Dose Report) child content items.
    /// </summary>
    private DicomSequence BuildContentSequence()
    {
        var items = new List<DicomDataset>();

        // ── TID 1001 child: Irradiation Event (CONTAINER) ─────────────────
        var irradiationEvent = new DicomDataset
        {
            { DicomTag.ValueType, "CONTAINER" },
            { DicomTag.RelationshipType, "CONTAINS" },
        };

        irradiationEvent.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence,
            BuildCodeItem(CodeIrradiationEvent, MeaningIrradiationEvent)));

        // Children of Irradiation Event
        var eventChildren = new List<DicomDataset>();

        // DAP (Dose (RP) Total) -- mandatory
        eventChildren.Add(BuildNumericContent(
            CodeDap, MeaningDap,
            _doseRecord.DapMgyCm2 > 0.0 ? _doseRecord.DapMgyCm2 : _doseRecord.Dap,
            "mGy.cm2", "mGy.cm2", "[DT_2_001]", "UCUM"));

        // ESD (Entrance Skin Dose) -- optional, only if available
        if (_doseRecord.EsdMgy.HasValue)
        {
            eventChildren.Add(BuildNumericContent(
                CodeEsd, MeaningEsd,
                _doseRecord.EsdMgy.Value,
                "mGy", "mGy", "[DT_2_002]", "UCUM"));
        }

        // kVp (Peak Kilovoltage) -- optional, only if available
        if (_exposureParams.Kvp.HasValue)
        {
            eventChildren.Add(BuildNumericContent(
                CodeKvp, MeaningKvp,
                _exposureParams.Kvp.Value,
                "kV", "kV", "UCUM", "1.4"));
        }

        // mAs (X Ray Tube Current) -- optional, only if available
        if (_exposureParams.Mas.HasValue)
        {
            eventChildren.Add(BuildNumericContent(
                CodeMas, MeaningMas,
                _exposureParams.Mas.Value,
                "mA", "mA", "UCUM", "1.4"));
        }

        // Exposure Time -- optional, only if available
        if (_exposureParams.ExposureTimeMs.HasValue)
        {
            eventChildren.Add(BuildNumericContent(
                CodeExposureTime, MeaningExposureTime,
                _exposureParams.ExposureTimeMs.Value,
                "ms", "ms", "UCUM", "1.4"));
        }

        // Body Part Examined -- mandatory
        eventChildren.Add(BuildTextContent(
            CodeBodyPart, MeaningBodyPart,
            _doseRecord.BodyPart));

        // Exposure Index (EI) -- optional
        if (_doseRecord.Ei > 0.0)
        {
            eventChildren.Add(BuildNumericContent(
                CodeExposureIndex, MeaningExposureIndex,
                _doseRecord.Ei,
                "none", "none", "[DT_2_003]", "UCUM"));
        }

        irradiationEvent.Add(new DicomSequence(DicomTag.ContentSequence, eventChildren.ToArray()));

        items.Add(irradiationEvent);

        return new DicomSequence(DicomTag.ContentSequence, items.ToArray());
    }

    // ── Helper methods for SR content items ──────────────────────────────────

    /// <summary>
    /// Builds a NUM (numeric) content item with measurement unit code sequence.
    /// </summary>
    private static DicomDataset BuildNumericContent(
        string codeValue, string codeMeaning,
        double numericValue,
        string unitCodeValue, string unitCodeMeaning,
        string unitCodingSchemeDesignator, string unitCodingSchemeUid)
    {
        var item = new DicomDataset
        {
            { DicomTag.ValueType, "NUM" },
            { DicomTag.RelationshipType, "CONTAINS" },
        };

        item.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence,
            BuildCodeItem(codeValue, codeMeaning)));

        // Measured Value Sequence (mandatory for NUM items)
        var measuredValue = new DicomDataset
        {
            { DicomTag.NumericValue, numericValue.ToString("F4", CultureInfo.InvariantCulture) },
        };

        // Measurement Units Code Sequence
        measuredValue.Add(new DicomSequence(DicomTag.MeasurementUnitsCodeSequence,
            BuildCodeItem(unitCodeValue, unitCodeMeaning, unitCodingSchemeDesignator)));

        item.Add(new DicomSequence(DicomTag.MeasuredValueSequence, measuredValue));

        return item;
    }

    /// <summary>
    /// Builds a TEXT content item.
    /// </summary>
    private static DicomDataset BuildTextContent(
        string codeValue, string codeMeaning,
        string textValue)
    {
        var item = new DicomDataset
        {
            { DicomTag.ValueType, "TEXT" },
            { DicomTag.RelationshipType, "CONTAINS" },
        };

        item.Add(new DicomSequence(DicomTag.ConceptNameCodeSequence,
            BuildCodeItem(codeValue, codeMeaning)));

        item.AddOrUpdate(new DicomLongText(DicomTag.TextValue, textValue));

        return item;
    }

    /// <summary>
    /// Builds a Code Sequence item (Concept Name) with DCM scheme designator.
    /// </summary>
    private static DicomDataset BuildCodeItem(
        string codeValue, string codeMeaning,
        string schemeDesignator = DcmScheme)
    {
        return new DicomDataset
        {
            { DicomTag.CodeValue, codeValue },
            { DicomTag.CodingSchemeDesignator, schemeDesignator },
            { DicomTag.CodeMeaning, codeMeaning },
        };
    }
}
