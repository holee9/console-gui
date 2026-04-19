// Copyright (c) H&abyz. All rights reserved.

namespace HnVue.Common.Models;

/// <summary>
/// Patient demographic information for RDSR (Radiation Dose Structured Report) generation.
/// SWR-DC-060: RDSR Patient Module data.
/// </summary>
/// <param name="PatientId">Patient identifier (DICOM Tag 0010,0020).</param>
/// <param name="PatientName">Patient full name in DICOM PN format (Last^First).</param>
/// <param name="PatientBirthDate">Birth date in DICOM DA format (YYYYMMDD).</param>
/// <param name="PatientSex">Sex code: M, F, O (other), or empty.</param>
public sealed record RdsrPatientInfo(
    string? PatientId = null,
    string? PatientName = null,
    string? PatientBirthDate = null,
    string? PatientSex = null);

/// <summary>
/// Study-level DICOM information for RDSR (Radiation Dose Structured Report) generation.
/// SWR-DC-061: RDSR Study Module data.
/// </summary>
/// <param name="StudyInstanceUid">DICOM Study Instance UID.</param>
/// <param name="StudyDate">Study date in DICOM DA format (YYYYMMDD).</param>
/// <param name="StudyTime">Study time in DICOM TM format (HHMMSS).</param>
/// <param name="AccessionNumber">Accession number for the study.</param>
/// <param name="StudyId">Study ID.</param>
/// <param name="ReferringPhysicianName">Referring physician name in DICOM PN format.</param>
/// <param name="RetrieveAeTitle">AE title of the device creating this RDSR.</param>
public sealed record RdsrStudyInfo(
    string StudyInstanceUid,
    string? StudyDate = null,
    string? StudyTime = null,
    string? AccessionNumber = null,
    string? StudyId = null,
    string? ReferringPhysicianName = null,
    string? RetrieveAeTitle = null);

/// <summary>
/// Exposure technique parameters for RDSR (Radiation Dose Structured Report) generation.
/// Provides kVp, mAs, and exposure time for encoding into the irradiation event content.
/// SWR-DC-063: RDSR exposure parameters encoding.
/// </summary>
/// <param name="Kvp">Peak kilovoltage (kVp). Null when not recorded.</param>
/// <param name="Mas">Milliampere-seconds (mAs). Null when not recorded.</param>
/// <param name="ExposureTimeMs">Exposure time in milliseconds. Null when not recorded.</param>
public sealed record RdsrExposureParams(
    double? Kvp = null,
    double? Mas = null,
    double? ExposureTimeMs = null);
