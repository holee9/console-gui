# SPEC-TEAMB-COV-001 (Compact)

## Requirements

- **REQ-COV-001**: Detector 85% — OwnDetectorAdapter/Config/VendorAdapterTemplate 0%→70%+
- **REQ-COV-002**: Dose 90% (Safety) — DoseRepository 0%→80%+, branch 90%+
- **REQ-COV-003**: Dicom 80% — MppsScu 0%→60%+, Outbox 62.5%→80%+, Service 69.3%→80%+
- **REQ-COV-004**: PatientManagement 80% — WorklistRepository 0%→70%+

## Acceptance Criteria

- AC-001: Detector line coverage ≥ 85%
- AC-002: Dose line AND branch coverage ≥ 90% (HARD GATE: DOC-012)
- AC-003: Dicom line coverage ≥ 80%
- AC-004: PatientManagement line coverage ≥ 80%
- AC-005: Full solution build 0 errors + all tests pass

## Files to Create

- `tests/HnVue.Detector.Tests/OwnDetectorAdapterTests.cs`
- `tests/HnVue.Detector.Tests/OwnDetectorConfigTests.cs`
- `tests/HnVue.Detector.Tests/VendorAdapterTemplateTests.cs`
- `tests/HnVue.Dose.Tests/DoseRepositoryTests.cs`
- `tests/HnVue.Dicom.Tests/MppsScuTests.cs`
- `tests/HnVue.PatientManagement.Tests/WorklistRepositoryTests.cs`

## Files to Extend

- `tests/HnVue.Dicom.Tests/DicomOutboxTests.cs`
- `tests/HnVue.Dicom.Tests/DicomServiceTests.cs`

## Exclusions

1. No production code changes — test files only
2. No hardware-dependent tests — Mock/Simulator only
3. No external DICOM SCP integration tests
4. No performance/load tests
5. OwnDetectorNativeMethods (P/Invoke stubs) excluded
6. DicomFindScu (deprecated) excluded
