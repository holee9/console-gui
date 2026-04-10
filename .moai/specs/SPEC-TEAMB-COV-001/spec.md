---
id: SPEC-TEAMB-COV-001
version: "1.1"
status: draft
created: "2026-04-09"
updated: "2026-04-10"
author: MoAI (Team B)
priority: P1-Critical
issue_number: 0
---

# SPEC-TEAMB-COV-001: Team B 모듈 테스트 커버리지 목표 달성

## HISTORY

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-04-09 | 1.0 | MoAI | 최초 작성 |
| 2026-04-10 | 1.1 | MoAI | 교차검증: Round 2 허위 보고 반영, 빌드오류 P0 선행조건 추가, QA 검증 Dicom 1.8% 불일치 주석 |

## Overview

Team B 관할 8개 모듈 중 4개 모듈(Detector, Dose, Dicom, PatientManagement)의 테스트 커버리지가 목표치 미달. Safety-critical 모듈(Dose, Incident)은 DOC-012 기준 branch coverage 90%+ 필수. DISPATCH.md Task 1-4 기반으로 테스트 코드를 작성하여 전체 모듈 평균 85%+ 달성.

### 현재 상태

| Module | Current | Target | Gap | Priority |
|--------|---------|--------|-----|----------|
| Detector | 42.6% | 85% | 42.4pp | P1-Critical |
| Dose | 67.6% | 90% | 22.4pp | P1-Critical (Safety) |
| Dicom | 66.9% | 80% | 13.1pp | P2-High |
| PatientManagement | 72.7% | 80% | 7.3pp | P2-High |

## Requirements

### REQ-COV-001: Detector 모듈 커버리지 85% 달성

**EARS Format**: The system shall provide test coverage for all Detector adapter classes when the target classes (OwnDetectorAdapter, OwnDetectorConfig, VendorAdapterTemplate) currently have 0% coverage.

**상세 요구사항**:

#### REQ-COV-001-1: OwnDetectorAdapter 테스트 (0% → 70%+)

The system shall test the following OwnDetectorAdapter behaviors:

- **Constructor**: Null config → ArgumentNullException, valid config → DetectorState.Disconnected
- **ConnectAsync**: 성공 시 Idle 전환, 실패 시 Result.Failure, disposed 시 ObjectDisposedException
- **DisconnectAsync**: Disconnected 전환, 멱등성(idempotent), CancellationToken 지원
- **ArmAsync**: Idle → Armed → Acquiring → ImageReady → Idle 전환, 비-Idle 상태에서 실패, Sync/FreeRun 모드
- **AbortAsync**: Error 상태 전환, 멱등성
- **GetStatusAsync**: 상태별 올바른 DetectorStatus 반환
- **Events**: StateChanged, ImageAcquired 이벤트 발생 검증
- **Dispose**: 리소스 정리 후 메서드 호출 시 ObjectDisposedException

**0% 클래스 참조**:
- `src/HnVue.Detector/OwnDetector/OwnDetectorAdapter.cs` (Lines 32-236)
- `src/HnVue.Common/Abstractions/IDetectorInterface.cs`

#### REQ-COV-001-2: OwnDetectorConfig 테스트 (0% → 70%+)

- Null host → ArgumentNullException
- 유효한 파라미터 → 올바른 설정 생성
- 기본값 검증
- DetectorConfig 상속 동작

**참조**: `src/HnVue.Detector/OwnDetector/OwnDetectorConfig.cs` (Lines 16-24)

#### REQ-COV-001-3: VendorAdapterTemplate 테스트 (0% → 70%+)

- ConnectAsync/DisconnectAsync/ArmAsync/AbortAsync → 상태 전환 검증
- GetStatusAsync → 기본 DetectorStatus 반환
- StateChanged 이벤트 발생
- HandleImageReady 정적 콜백 동작

**참조**: `src/HnVue.Detector/ThirdParty/VendorAdapterTemplate.cs` (Lines 35-194)

### REQ-COV-002: Dose 모듈 커버리지 90% 달성 (Safety-Critical)

**EARS Format**: The system shall provide test coverage for DoseRepository when the module is classified as safety-critical per DOC-012, requiring branch coverage 90%+.

#### REQ-COV-002-1: DoseRepository CRUD 테스트 (0% → 80%+)

**SaveAsync** (Lines 28-48):
- 유효한 DoseRecord → Result.Success
- Null dose → ArgumentNullException
- DB 오류 → Result.Failure(ErrorCode.DatabaseError)
- CancellationToken 동작

**GetByStudyAsync** (Lines 52-76):
- 레코드 존재 → DoseRecord 반환
- 레코드 없음 → null 반환
- Null studyUid → ArgumentNullException
- DB 오류 → Result.Failure

**GetByPatientAsync** (Lines 83-131):
- 날짜 범위 필터링 (from/until)
- 환자 레코드 없음 → 빈 목록
- 다중 레코드 → RecordedAtTicks 기준 정렬
- Null patientId → ArgumentNullException
- DB 오류 → Result.Failure
- CancellationToken 동작

**참조**:
- `src/HnVue.Dose/DoseRepository.cs` (Lines 1-160)
- `src/HnVue.Dose/DoseService.cs` (Lines 1-276)
- `src/HnVue.Common/Models/DoseRecord.cs`

#### REQ-COV-002-2: 인터록 4-level 분기 커버 (이미 DoseService 100%)

DoseService.ClassifyDose()의 4-level 분기는 이미 100% 테스트됨.
DoseRepository 추가 시 DoseService + Repository 통합 시나리오 검증 필요:

- Allow: DAP ≤ DRL (e.g., CHEST: ≤10.0)
- Warn: DRL < DAP ≤ 2×DRL
- Block: 2×DRL < DAP ≤ 5×DRL
- Emergency: DAP > 5×DRL
- Unknown body part → DefaultDrl = 20.0

### REQ-COV-003: Dicom 모듈 커버리지 80% 달성

**EARS Format**: The system shall provide test coverage for MppsScu and improve coverage for DicomOutbox and DicomService when current coverage is below the 80% target.

#### REQ-COV-003-1: MppsScu 테스트 (0% → 60%+)

**SendInProgressAsync** (Lines 42-102):
- 유효한 파라미터 → MppsUid 반환
- Host 누락 → Result.Failure(ErrorCode.DicomConnectionFailed)
- 네트워크 예외 → ErrorCode.DicomConnectionFailed
- N-CREATE 비성공 응답 → Result.Failure
- CancellationToken → 취소 동작

**SendCompletedAsync** (Lines 115-169):
- 유효한 파라미터 → Result.Success
- Completed/Discontinued 상태 매핑
- Null mppsUid → ArgumentNullException
- 네트워크 예외 → ErrorCode.DicomConnectionFailed
- CancellationToken → 취소 동작

**참조**: `src/HnVue.Dicom/MppsScu.cs` (Lines 1-170)

#### REQ-COV-003-2: DicomOutbox 추가 테스트 (62.5% → 80%+)

- EnqueueAsync 성공 시나리오
- Polly 지수 백오프 검증
- Dead-letter 로깅 동작
- 다중 아이템 순차 처리

**참조**: `src/HnVue.Dicom/DicomOutbox.cs` (Lines 1-126)

#### REQ-COV-003-3: DicomService 추가 테스트 (69.3% → 80%+)

- StoreAsync 성공/실패 C-STORE
- QueryWorklistAsync 성공 C-FIND + 다중 응답
- PrintAsync N-CREATE/N-ACTION 워크플로우
- BuildWorklistRequest 날짜 범위/환자ID 필터
- MapToWorklistItem 복합 데이터셋/누락 태그

**참조**: `src/HnVue.Dicom/DicomService.cs` (Lines 1-408)

### REQ-COV-004: PatientManagement 모듈 커버리지 80% 달성

**EARS Format**: The system shall provide test coverage for WorklistRepository when the class currently has 0% coverage and the module target is 80%.

#### REQ-COV-004-1: WorklistRepository 테스트 (0% → 70%+)

**QueryTodayAsync** (Lines 36-59):
- MWL 쿼리 성공 → WorklistItem 목록 반환
- DICOM 서비스 실패 → 빈 목록 반환 (advisory nature)
- Null 설정 → ArgumentNullException
- 빈 워크리스트 → 빈 목록
- CancellationToken → 취소 동작
- 다중 WorklistItem 반환

**참조**: `src/HnVue.PatientManagement/WorklistRepository.cs` (Lines 1-61)

## Files to Modify

### [NEW] 테스트 파일 (신규 생성)
- `tests/HnVue.Detector.Tests/OwnDetectorAdapterTests.cs`
- `tests/HnVue.Detector.Tests/OwnDetectorConfigTests.cs`
- `tests/HnVue.Detector.Tests/VendorAdapterTemplateTests.cs`
- `tests/HnVue.Dose.Tests/DoseRepositoryTests.cs`
- `tests/HnVue.Dicom.Tests/MppsScuTests.cs`
- `tests/HnVue.PatientManagement.Tests/WorklistRepositoryTests.cs`

### [MODIFY] 기존 테스트 파일 (확장)
- `tests/HnVue.Dicom.Tests/DicomOutboxTests.cs` — 추가 시나리오
- `tests/HnVue.Dicom.Tests/DicomServiceTests.cs` — Store/Query/Print 경로 추가

### [EXISTING] 소스 파일 (변경 없음, 참조만)
- `src/HnVue.Detector/**` (읽기 전용)
- `src/HnVue.Dose/**` (읽기 전용)
- `src/HnVue.Dicom/**` (읽기 전용)
- `src/HnVue.PatientManagement/**` (읽기 전용)

## Exclusions (What NOT to Build)

1. **프로덕션 코드 수정 없음** — 소스 코드 변경 불필요, 테스트 코드만 작성
2. **하드웨어 의존 테스트 없음** — 실제 FPD/SDK 하드웨어 연동 테스트 제외
3. **통합 테스트 범위 제한** — 외부 DICOM SCP 서버 연동 테스트 제외
4. **OwnDetectorNativeMethods 테스트 제외** — P/Invoke 조건부 컴파일 스텁은 테스트 불필요
5. **성능/부하 테스트 제외** — 대규모 데이터셋 처리 성능 테스트는 범위 외
6. **Deprecated 클래스 테스트 제외** — DicomFindScu(deprecated) 테스트 제외
