# Implementation Plan — SPEC-TEAMB-COV-001

## Task Decomposition

### Phase 1: Safety-Critical 우선 (Dose + Detector)

#### Task 1.1: DoseRepository 테스트 (P1-Critical, Safety)
- **File**: `tests/HnVue.Dose.Tests/DoseRepositoryTests.cs`
- **Pattern**: NSubstitute mock + In-Memory SQLite (필요시)
- **Scope**: SaveAsync, GetByStudyAsync, GetByPatientAsync CRUD 시나리오
- **Test Count**: ~15-20개
- **Dependencies**: DoseRecordEntity, HnVueDbContext

#### Task 1.2: OwnDetectorAdapter 테스트 (P1-Critical)
- **File**: `tests/HnVue.Detector.Tests/OwnDetectorAdapterTests.cs`
- **Pattern**: IDetectorInterface 구현체 직접 테스트 (SDK는 placeholder이므로 실제 호출)
- **Scope**: 상태머신 6-state 전이, 이벤트 발생, Dispose
- **Test Count**: ~20-25개
- **Dependencies**: OwnDetectorConfig, DetectorState enum

#### Task 1.3: OwnDetectorConfig 테스트
- **File**: `tests/HnVue.Detector.Tests/OwnDetectorConfigTests.cs`
- **Pattern**: Record 유효성 검증
- **Scope**: Null 파라미터, 기본값, 상속
- **Test Count**: ~5개

#### Task 1.4: VendorAdapterTemplate 테스트
- **File**: `tests/HnVue.Detector.Tests/VendorAdapterTemplateTests.cs`
- **Pattern**: Template 메서드 검증
- **Scope**: Connect/Disconnect/Arm/Abort/GetStatus, StateChanged
- **Test Count**: ~10-12개

### Phase 2: High Priority (Dicom + PatientManagement)

#### Task 2.1: MppsScu 테스트 (P2-High)
- **File**: `tests/HnVue.Dicom.Tests/MppsScuTests.cs`
- **Pattern**: DicomClient mock (NSubstitute) 또는 fo-dicom 테스트 패턴
- **Scope**: SendInProgressAsync, SendCompletedAsync
- **Test Count**: ~10-12개
- **Note**: DicomClient는 정적 팩토리이므로 래퍼 인터페이스 고려 필요

#### Task 2.2: DicomOutbox 확장 테스트
- **File**: `tests/HnVue.Dicom.Tests/DicomOutboxTests.cs` (기존 확장)
- **Pattern**: Polly 지수 백오프 검증
- **Scope**: 성공 Enqueue, retry 정책, dead-letter, 동시 처리
- **Test Count**: ~6-8개 추가

#### Task 2.3: DicomService 확장 테스트
- **File**: `tests/HnVue.Dicom.Tests/DicomServiceTests.cs` (기존 확장)
- **Pattern**: DicomClient mock
- **Scope**: StoreAsync, QueryWorklistAsync, PrintAsync, 내부 메서드
- **Test Count**: ~12-15개 추가
- **Note**: DicomClientFactory 정적 메서드 → 래핑 필요 가능성

#### Task 2.4: WorklistRepository 테스트 (P2-High)
- **File**: `tests/HnVue.PatientManagement.Tests/WorklistRepositoryTests.cs`
- **Pattern**: IDicomService mock (NSubstitute)
- **Scope**: QueryTodayAsync 성공/실패/빈결과
- **Test Count**: ~8-10개

### Phase 3: Build Verification

- `dotnet build HnVue.sln --configuration Release`
- `dotnet test HnVue.sln --configuration Release --no-build`
- 커버리지 리포트 확인

## Technology Stack

- **Framework**: xUnit 2.x
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Coverage**: coverlet.collector
- **Target**: .NET 8

## Risk Analysis

| Risk | Impact | Mitigation |
|------|--------|------------|
| DicomClient 정적 팩토리 mock 불가 | MppsScu/DicomService 테스트 제한 | 인터페이스 래핑 또는 내부 메서드 간접 테스트 |
| OwnDetectorAdapter SDK placeholder | NotImplementedException 발생 가능 | 예외 처리 테스트로 커버리지 확보 |
| EF Core In-Memory DB 미지원 | DoseRepository CRUD 테스트 제한 | NSubstitute로 DbContext mock |
| fo-dicom 복잡한 응답 시뮬레이션 | DicomService 테스트 어려움 | Dataset 직접 생성으로 간접 테스트 |

## MX Tag Plan

### Priority 1: @MX:ANCHOR (fan_in >= 3)
- `IDetectorInterface.ConnectAsync` — 3+ 호출자 (OwnDetector, Simulator, Vendor)
- `IDoseService.ValidateExposureAsync` — safety-critical 핵심 경로
- `IDicomService.StoreAsync` — 다중 소비자

### Priority 2: @MX:WARN
- `DoseService.ClassifyDose` — 4-level interlock 안전 로직
- `MppsScu.SendInProgressAsync` — 네트워크 의존 + 예외 경로

### Priority 3: @MX:NOTE
- `OwnDetectorAdapter.TransitionState` — 상태 전환 불변식
- `WorklistRepository.QueryTodayAsync` — advisory MWL 패턴
