// Copyright (c) H&abyz. All rights reserved.

using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="RdsrBuilder"/> covering RDSR dataset generation,
/// field validation, patient/study module population, and dose content encoding.
/// SWR-DC-060~065.
/// </summary>
public sealed class RdsrBuilderTests
{
    // ── Test data factories ──────────────────────────────────────────────────

    private static DoseRecord CreateTestDoseRecord(
        double dap = 5.0,
        double dapMgyCm2 = 5.0,
        double ei = 1200.0,
        double? esdMgy = 0.5,
        string bodyPart = "CHEST") =>
        new(
            DoseId: "DOSE-001",
            StudyInstanceUid: "1.2.3.4.5.6.7.8.9",
            Dap: dap,
            Ei: ei,
            EffectiveDose: 0.1,
            BodyPart: bodyPart,
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "PAT-001",
            DapMgyCm2: dapMgyCm2,
            FieldAreaCm2: 400.0,
            MeanPixelValue: 1800.0,
            EiTarget: 1500.0,
            EsdMgy: esdMgy);

    private static RdsrPatientInfo CreateTestPatientInfo() =>
        new(PatientId: "PAT-001", PatientName: "Doe^John", PatientBirthDate: "19800101", PatientSex: "M");

    private static RdsrStudyInfo CreateTestStudyInfo(string studyUid = "1.2.3.4.5.6.7.8.9") =>
        new(StudyInstanceUid: studyUid, StudyDate: "20260419", StudyTime: "120000",
            AccessionNumber: "ACC-001", StudyId: "ST-001",
            ReferringPhysicianName: "Dr^Smith", RetrieveAeTitle: "HNVUE");

    // ── Constructor validation ───────────────────────────────────────────────

    [Fact]
    public void Constructor_NullDoseRecord_ThrowsArgumentNullException()
    {
        var act = () => new RdsrBuilder(null!, CreateTestPatientInfo(), CreateTestStudyInfo());

        act.Should().Throw<ArgumentNullException>().WithParameterName("doseRecord");
    }

    [Fact]
    public void Constructor_NullPatientInfo_ThrowsArgumentNullException()
    {
        var act = () => new RdsrBuilder(CreateTestDoseRecord(), null!, CreateTestStudyInfo());

        act.Should().Throw<ArgumentNullException>().WithParameterName("patientInfo");
    }

    [Fact]
    public void Constructor_NullStudyInfo_ThrowsArgumentNullException()
    {
        var act = () => new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("studyInfo");
    }

    // ── SOP Common Module ────────────────────────────────────────────────────

    [Fact]
    public void Build_SetsEnhancedSrSopClassUid()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var sopClassUid = dataset.GetSingleValue<DicomUID>(DicomTag.SOPClassUID);
        sopClassUid.Should().Be(RdsrBuilder.EnhancedSrSopClassUid);
    }

    [Fact]
    public void Build_EnhancedSrSopClassUid_HasCorrectValue()
    {
        RdsrBuilder.EnhancedSrSopClassUid.UID.Should().Be("1.2.840.10008.5.1.4.1.1.88.22");
    }

    [Fact]
    public void Build_SetsSpecificCharacterSet()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var charset = dataset.GetSingleValue<string>(DicomTag.SpecificCharacterSet);
        charset.Should().Be("ISO_IR 192");
    }

    // ── Patient Module ───────────────────────────────────────────────────────

    [Fact]
    public void Build_PopulatesPatientId()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var patientId = dataset.GetSingleValue<string>(DicomTag.PatientID);
        patientId.Should().Be("PAT-001");
    }

    [Fact]
    public void Build_PopulatesPatientName()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var patientName = dataset.GetSingleValue<string>(DicomTag.PatientName);
        patientName.Should().Be("Doe^John");
    }

    [Fact]
    public void Build_PopulatesPatientBirthDate()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var birthDate = dataset.GetSingleValue<string>(DicomTag.PatientBirthDate);
        birthDate.Should().Be("19800101");
    }

    [Fact]
    public void Build_PopulatesPatientSex()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var sex = dataset.GetSingleValue<string>(DicomTag.PatientSex);
        sex.Should().Be("M");
    }

    [Fact]
    public void Build_NullPatientFields_DefaultsToEmpty()
    {
        var patientInfo = new RdsrPatientInfo();
        var builder = new RdsrBuilder(CreateTestDoseRecord(), patientInfo, CreateTestStudyInfo());
        var dataset = builder.Build();

        // fo-dicom represents empty strings as zero-length values; use GetOrDefault
        dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty).Should().BeEmpty();
        dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty).Should().BeEmpty();
        dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty).Should().BeEmpty();
        dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty).Should().BeEmpty();
    }

    // ── Study Module ─────────────────────────────────────────────────────────

    [Fact]
    public void Build_PopulatesStudyInstanceUid()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var studyUid = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID);
        studyUid.Should().Be("1.2.3.4.5.6.7.8.9");
    }

    [Fact]
    public void Build_PopulatesStudyDateAndTime()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        dataset.GetSingleValue<string>(DicomTag.StudyDate).Should().Be("20260419");
        dataset.GetSingleValue<string>(DicomTag.StudyTime).Should().Be("120000");
    }

    [Fact]
    public void Build_PopulatesAccessionNumber()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        dataset.GetSingleValue<string>(DicomTag.AccessionNumber).Should().Be("ACC-001");
    }

    [Fact]
    public void Build_PopulatesReferringPhysicianName()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        dataset.GetSingleValue<string>(DicomTag.ReferringPhysicianName).Should().Be("Dr^Smith");
    }

    // ── SR Document General Module ───────────────────────────────────────────

    [Fact]
    public void Build_AutoGeneratesSopInstanceUid()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var sopInstanceUid = dataset.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID);
        sopInstanceUid.Should().NotBeNull();
        sopInstanceUid.UID.Should().NotBeEmpty();
    }

    [Fact]
    public void Build_UsesProvidedSopInstanceUid()
    {
        var expectedUid = DicomUID.Generate();
        var builder = new RdsrBuilder(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            sopInstanceUid: expectedUid);
        var dataset = builder.Build();

        var sopInstanceUid = dataset.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID);
        sopInstanceUid.Should().Be(expectedUid);
    }

    [Fact]
    public void Build_SetsInstanceNumber()
    {
        var builder = new RdsrBuilder(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            instanceNumber: 42);
        var dataset = builder.Build();

        var instanceNumber = dataset.GetSingleValue<int>(DicomTag.InstanceNumber);
        instanceNumber.Should().Be(42);
    }

    [Fact]
    public void Build_SetsSeriesNumberToOne()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var seriesNumber = dataset.GetSingleValue<int>(DicomTag.SeriesNumber);
        seriesNumber.Should().Be(1);
    }

    // ── SR Document Content Module ───────────────────────────────────────────

    [Fact]
    public void Build_RootValueTypeIsContainer()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var valueType = dataset.GetSingleValue<string>(DicomTag.ValueType);
        valueType.Should().Be("CONTAINER");
    }

    [Fact]
    public void Build_ConceptNameCodeSequence_IsRdsrReport()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var conceptSeq = dataset.GetSequence(DicomTag.ConceptNameCodeSequence);
        ((int)conceptSeq.Items.Count).Should().Be(1);

        var codeItem = conceptSeq.Items[0];
        codeItem.GetSingleValue<string>(DicomTag.CodeValue).Should().Be("113812");
        codeItem.GetSingleValue<string>(DicomTag.CodingSchemeDesignator).Should().Be("DCM");
        codeItem.GetSingleValue<string>(DicomTag.CodeMeaning).Should().Be("Radiation Dose Structured Report");
    }

    [Fact]
    public void Build_ContentSequence_ContainsIrradiationEvent()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        contentSeq.Items.Should().NotBeEmpty();

        var irradiationEvent = contentSeq.Items[0];
        irradiationEvent.GetSingleValue<string>(DicomTag.ValueType).Should().Be("CONTAINER");
        irradiationEvent.GetSingleValue<string>(DicomTag.RelationshipType).Should().Be("CONTAINS");

        var conceptSeq = irradiationEvent.GetSequence(DicomTag.ConceptNameCodeSequence);
        conceptSeq.Items[0].GetSingleValue<string>(DicomTag.CodeValue).Should().Be("113814");
    }

    // ── Dose Content Encoding ────────────────────────────────────────────────

    [Fact]
    public void Build_DapContent_UsesDapMgyCm2_WhenAvailable()
    {
        var dose = CreateTestDoseRecord(dap: 3.0, dapMgyCm2: 5.5);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        // Find DAP item
        var dapItem = FindContentItemByCode(eventChildren, "113823");
        dapItem.Should().NotBeNull();

        var measuredSeq = dapItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("5.5000");
    }

    [Fact]
    public void Build_DapContent_FallsBackToDap_WhenDapMgyCm2IsZero()
    {
        var dose = CreateTestDoseRecord(dap: 3.0, dapMgyCm2: 0.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var dapItem = FindContentItemByCode(eventChildren, "113823");
        dapItem.Should().NotBeNull();

        var measuredSeq = dapItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("3.0000");
    }

    [Fact]
    public void Build_EsdContent_Included_WhenEsdMgyHasValue()
    {
        var dose = CreateTestDoseRecord(esdMgy: 0.75);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var esdItem = FindContentItemByCode(eventChildren, "113824");
        esdItem.Should().NotBeNull();

        var measuredSeq = esdItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("0.7500");
    }

    [Fact]
    public void Build_EsdContent_Excluded_WhenEsdMgyIsNull()
    {
        var dose = CreateTestDoseRecord(esdMgy: null);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var esdItem = FindContentItemByCode(eventChildren, "113824");
        esdItem.Should().BeNull("ESD should not be included when EsdMgy is null");
    }

    [Fact]
    public void Build_BodyPart_EncodedAsTextContent()
    {
        var dose = CreateTestDoseRecord(bodyPart: "ABDOMEN");
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var bodyPartItem = FindContentItemByCode(eventChildren, "123009");
        bodyPartItem.Should().NotBeNull();
        bodyPartItem!.GetSingleValue<string>(DicomTag.ValueType).Should().Be("TEXT");

        var textValue = bodyPartItem.GetSingleValueOrDefault(DicomTag.TextValue, string.Empty);
        textValue.Should().Be("ABDOMEN");
    }

    [Fact]
    public void Build_ExposureIndex_Included_WhenEiIsPositive()
    {
        var dose = CreateTestDoseRecord(ei: 1200.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var eiItem = FindContentItemByCode(eventChildren, "113840");
        eiItem.Should().NotBeNull();

        var measuredSeq = eiItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("1200.0000");
    }

    [Fact]
    public void Build_ExposureIndex_Excluded_WhenEiIsZero()
    {
        var dose = CreateTestDoseRecord(ei: 0.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var eiItem = FindContentItemByCode(eventChildren, "113840");
        eiItem.Should().BeNull("EI should not be included when value is 0.0");
    }

    // ── Content Date/Time ────────────────────────────────────────────────────

    [Fact]
    public void Build_SetsContentDateAndTime()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentDate = dataset.GetSingleValue<string>(DicomTag.ContentDate);
        var contentTime = dataset.GetSingleValue<string>(DicomTag.ContentTime);

        contentDate.Should().MatchRegex(@"^\d{8}$");
        contentTime.Should().MatchRegex(@"^\d{6}");
    }

    // ── Verification ─────────────────────────────────────────────────────────

    [Fact]
    public void Build_IncludesVerifyingObserverSequence()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var verifySeq = dataset.GetSequence(DicomTag.VerifyingObserverSequence);
        ((int)verifySeq.Items.Count).Should().Be(1);

        var observer = verifySeq.Items[0];
        observer.GetSingleValue<string>(DicomTag.VerifyingOrganization).Should().Be("H&abyz");
    }

    // ── Measurement Units ────────────────────────────────────────────────────

    [Fact]
    public void Build_DapMeasurementUnit_IsMgyCm2()
    {
        var builder = new RdsrBuilder(CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var dapItem = FindContentItemByCode(eventChildren, "113823");
        var measuredSeq = dapItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var unitSeq = measuredSeq.Items[0].GetSequence(DicomTag.MeasurementUnitsCodeSequence);

        unitSeq.Items[0].GetSingleValue<string>(DicomTag.CodeValue).Should().Be("mGy.cm2");
    }

    // ── Exposure Parameters (kVp, mAs, Exposure Time) ─────────────────────────

    [Fact]
    public void Build_KvpContent_Included_WhenExposureParamsProvided()
    {
        var dose = CreateTestDoseRecord();
        var exposureParams = new RdsrExposureParams(Kvp: 80.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo(), exposureParams);
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var kvpItem = FindContentItemByCode(eventChildren, "113821");
        kvpItem.Should().NotBeNull();
        kvpItem!.GetSingleValue<string>(DicomTag.ValueType).Should().Be("NUM");

        var measuredSeq = kvpItem.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("80.0000");
    }

    [Fact]
    public void Build_KvpContent_Excluded_WhenExposureParamsNull()
    {
        var dose = CreateTestDoseRecord();
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var kvpItem = FindContentItemByCode(eventChildren, "113821");
        kvpItem.Should().BeNull("kVp should not be included when exposure params are not provided");
    }

    [Fact]
    public void Build_MasContent_Included_WhenExposureParamsProvided()
    {
        var dose = CreateTestDoseRecord();
        var exposureParams = new RdsrExposureParams(Mas: 200.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo(), exposureParams);
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var masItem = FindContentItemByCode(eventChildren, "113822");
        masItem.Should().NotBeNull();
        masItem!.GetSingleValue<string>(DicomTag.ValueType).Should().Be("NUM");

        var measuredSeq = masItem.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("200.0000");
    }

    [Fact]
    public void Build_MasContent_Excluded_WhenExposureParamsNull()
    {
        var dose = CreateTestDoseRecord();
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var masItem = FindContentItemByCode(eventChildren, "113822");
        masItem.Should().BeNull("mAs should not be included when exposure params are not provided");
    }

    [Fact]
    public void Build_ExposureTimeContent_Included_WhenExposureParamsProvided()
    {
        var dose = CreateTestDoseRecord();
        var exposureParams = new RdsrExposureParams(ExposureTimeMs: 150.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo(), exposureParams);
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var timeItem = FindContentItemByCode(eventChildren, "113852");
        timeItem.Should().NotBeNull();
        timeItem!.GetSingleValue<string>(DicomTag.ValueType).Should().Be("NUM");

        var measuredSeq = timeItem.GetSequence(DicomTag.MeasuredValueSequence);
        var numericValue = measuredSeq.Items[0].GetSingleValue<string>(DicomTag.NumericValue);
        numericValue.Should().Be("150.0000");
    }

    [Fact]
    public void Build_ExposureTimeContent_Excluded_WhenExposureParamsNull()
    {
        var dose = CreateTestDoseRecord();
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo());
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var timeItem = FindContentItemByCode(eventChildren, "113852");
        timeItem.Should().BeNull("Exposure time should not be included when exposure params are not provided");
    }

    [Fact]
    public void Build_AllExposureParams_IncludedTogether()
    {
        var dose = CreateTestDoseRecord();
        var exposureParams = new RdsrExposureParams(Kvp: 70.0, Mas: 100.0, ExposureTimeMs: 50.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo(), exposureParams);
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        FindContentItemByCode(eventChildren, "113821").Should().NotBeNull("kVp");
        FindContentItemByCode(eventChildren, "113822").Should().NotBeNull("mAs");
        FindContentItemByCode(eventChildren, "113852").Should().NotBeNull("Exposure Time");
    }

    [Fact]
    public void Build_KvpMeasurementUnit_IsKv()
    {
        var dose = CreateTestDoseRecord();
        var exposureParams = new RdsrExposureParams(Kvp: 80.0);
        var builder = new RdsrBuilder(dose, CreateTestPatientInfo(), CreateTestStudyInfo(), exposureParams);
        var dataset = builder.Build();

        var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
        var irradiationEvent = contentSeq.Items[0];
        var eventChildren = irradiationEvent.GetSequence(DicomTag.ContentSequence);

        var kvpItem = FindContentItemByCode(eventChildren, "113821");
        var measuredSeq = kvpItem!.GetSequence(DicomTag.MeasuredValueSequence);
        var unitSeq = measuredSeq.Items[0].GetSequence(DicomTag.MeasurementUnitsCodeSequence);

        unitSeq.Items[0].GetSingleValue<string>(DicomTag.CodeValue).Should().Be("kV");
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds a content item within a DICOM sequence by its concept name code value.
    /// </summary>
    private static DicomDataset? FindContentItemByCode(DicomSequence sequence, string codeValue)
    {
        foreach (var item in sequence.Items)
        {
            if (item.TryGetSequence(DicomTag.ConceptNameCodeSequence, out var conceptSeq)
                && conceptSeq.Items.Count > 0
                && conceptSeq.Items[0].TryGetSingleValue(DicomTag.CodeValue, out string code)
                && code == codeValue)
            {
                return item;
            }
        }

        return null;
    }
}
