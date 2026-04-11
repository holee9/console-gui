# Team B — Medical Imaging Pipeline Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)

## Module Ownership
- HnVue.Dicom, HnVue.Detector, HnVue.Imaging, HnVue.Dose, HnVue.Incident
- HnVue.Workflow, HnVue.PatientManagement, HnVue.CDBurning

## Test Ownership
- tests/HnVue.Dicom.Tests/
- tests/HnVue.Detector.Tests/
- tests/HnVue.Imaging.Tests/
- tests/HnVue.Dose.Tests/
- tests/HnVue.Incident.Tests/
- tests/HnVue.Workflow.Tests/
- tests/HnVue.PatientManagement.Tests/
- tests/HnVue.CDBurning.Tests/

## Safety-Critical Module Standards
- Safety-Critical (90%+ coverage): Dose, Incident
- Safety-Adjacent (85%+ coverage, RA review recommended): Imaging, Workflow
  - Imaging: rendering errors may affect diagnostic interpretation
  - Workflow: state machine errors may affect patient safety sequence
- Standard (85%+ coverage): Dicom, Detector, PatientManagement, CDBurning
- All safety-critical changes require characterization tests before modification
- Dose interlock 4-level logic is invariant — changes require RA risk assessment

## DICOM Standards (per DICOM-001 Guide)
- Use fo-dicom 5.1.3 API conventions
- DICOM tag references use standard group,element notation
- C-STORE SCP/SCU must handle association negotiation properly
- MWL (Modality Worklist) queries follow IHE Radiology Technical Framework

## FPD Detector SDK Adapter Pattern
- All detector interactions through IDetectorService abstraction
- Simulator adapter for testing (no hardware dependency)
- SDK adapter wraps vendor-specific API
- Connection lifecycle: Initialize -> Connect -> Configure -> Acquire -> Disconnect

## Workflow State Machine Rules
- 9-state transition model is authoritative
- State transitions must be validated against allowed transitions table
- Invalid transitions throw InvalidOperationException
- State change events must be published for UI notification

## Interface Change Protocol
- IDetectorService or IWorkflowEngine changes: Coordinator approval required
- Workflow state transition changes: create RA issue for RTM (DOC-032) update
- Patient data model changes: coordinate with Team A (Data layer)

## Design Team 기능구현 분담 (Team B 담당분)

Design Team은 코더가 없으므로, 아래 도메인 UI 코드는 Team B가 구현:

| 항목 | 파일 위치 | 설명 |
|------|-----------|------|
| 도메인 Converter | HnVue.UI/Converters/SafeStateToColorConverter.cs | SafeState enum 의존, 도메인 변경 시 Team B 수정 |
| 도메인 Converter | HnVue.UI/Converters/AgeFromBirthDateConverter.cs | 환자 데이터 모델 의존 |
| 의료 컨트롤 C# 로직 | HnVue.UI/Components/Medical/AcquisitionPreview.cs | DoseLevel, ExposureInfo 등 의료 도메인 속성 |
| 의료 컨트롤 C# 로직 | HnVue.UI/Components/Medical/PatientInfoCard.xaml.cs | 환자 데이터 바인딩 |
| 의료 컨트롤 C# 로직 | HnVue.UI/Components/Medical/StudyThumbnail.xaml.cs | 스터디 데이터 바인딩 |

**공동 작업 패턴:**
- Team B: C# 코드 (DependencyProperty, 도메인 로직, Converter)
- Design Team: XAML 템플릿 (레이아웃, 색상, 스타일)
- 동일 파일 수정 시: Team B가 먼저 C# 구현 → Design이 XAML 적용

## Issue Protocol
- Safety-critical changes: create issue with `team-b` + `priority-high` labels
- Workflow state changes: create issue + notify RA team for RTM update
