---
id: SPEC-TEAMB-FIX-001
version: "1.0"
status: approved
created: "2026-04-10"
updated: "2026-04-10"
author: MoAI (Team B)
priority: P2-High
issue_number: 0
---

# SPEC-TEAMB-FIX-001: Team B 모듈 기능개선 및 버그픽스

## HISTORY

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-04-10 | 1.0 | MoAI | Deep research 기반 SPEC 생성 |

## Overview

Team B 8개 모듈 중 Dicom 모듈의 테스트 커버리지가 43%로 가장 큰 갭을 보유. 이전 SPEC-TEAMB-COV-001에서 Detector(91.7%), Dose(99.5%), PatientManagement(100%)은 목표 달성했으나 Dicom은 MppsScu(0%), DicomStoreScu 미흡으로 저조.

본 SPEC은 Dicom 커버리지 향상을 최우선으로 하고, IncidentRepository/WorkflowEngine의 방어적 개선을 포함.

### 현재 상태

| Module | Coverage | Target | Status |
|--------|----------|--------|--------|
| Dicom | 43% | 80% | GAP (MppsScu 0%, DicomStoreScu 미흡) |
| Incident | 94.2% | 85% | PASS |
| Workflow | 91.4% | 85% | PASS |
| Detector | 91.7% | 85% | PASS |
| Dose | 99.5% | 90% | PASS |
| PatientManagement | 100% | 80% | PASS |
| Imaging | 87.5% | 85% | PASS |
| CDBurning | 100% | 80% | PASS |

## Requirements

### REQ-FIX-001: Dicom 모듈 커버리지 80% 달성

**EARS Format**: The system shall provide test coverage for all Dicom module classes when MppsScu currently has 0% coverage and DicomOutbox/DicomService coverage is below target.

#### REQ-FIX-001-1: MppsScu 테스트 (0% → 70%+)

The system shall test the following MppsScu behaviors:

- **SendInProgressAsync**: 유효한 파라미터 → MppsUid 반환, Host 누락 → Result.Failure, 네트워크 예외 처리, CancellationToken 동작
- **SendCompletedAsync**: Completed/Discontinued 상태 매핑, Null mppsUid → ArgumentNullException, 네트워크 예외 처리

**참조**: `src/HnVue.Dicom/MppsScu.cs`

#### REQ-FIX-001-2: DicomStoreScu 테스트 확장

The system shall test DicomStoreScu behaviors:
- **SendAsync**: 유효한 C-STORE → Result.Success, 연결 실패 → Result.Failure, CancellationToken 동작
- **체크섬 검증**: DICOM 파일 전송 전 데이터 무결성 검증 로직

**참조**: `src/HnVue.Dicom/DicomStoreScu.cs`

#### REQ-FIX-001-3: DicomOutbox/DicomService 테스트 보강

- DicomOutbox: Polly 재시도 정책 엣지 케이스, dead-letter 로깅, 동시성 처리
- DicomService: StoreAsync/QueryWorklistAsync/PrintAsync 경로 보강

### REQ-FIX-002: IncidentRepository 방어적 개선

**EARS Format**: The system shall guard against null record parameter in IncidentRepository.UpdateAsync when the method currently trusts caller to provide valid input.

#### REQ-FIX-002-1: UpdateAsync null guard 추가

- Null record → ArgumentNullException
- Null/empty IncidentId → ArgumentNullException

**참조**: `src/HnVue.Incident/IncidentRepository.cs:91-105`

### REQ-FIX-003: WorkflowEngine 방어적 개선

**EARS Format**: The system shall guard against null parameters in WorkflowEngine public methods when interacting with detector and dose services.

#### REQ-FIX-003-1: 공개 메서드 null guard

- PrepareExposureAsync: null parameters → ArgumentNullException
- CompleteExposureAsync: null parameters → ArgumentNullException

**참조**: `src/HnVue.Workflow/WorkflowEngine.cs`

## Files to Modify

### [NEW] 테스트 파일
- `tests/HnVue.Dicom.Tests/MppsScuTests.cs`
- `tests/HnVue.Dicom.Tests/DicomStoreScuTests.cs`

### [MODIFY] 기존 테스트 파일
- `tests/HnVue.Dicom.Tests/DicomOutboxTests.cs` — 엣지 케이스 추가
- `tests/HnVue.Dicom.Tests/DicomServiceTests.cs` — Store/Query/Print 경로 보강

### [MODIFY] 소스 파일 (방어적 개선)
- `src/HnVue.Incident/IncidentRepository.cs` — null guard 추가
- `src/HnVue.Workflow/WorkflowEngine.cs` — null guard 추가

### [EXISTING] 참조만 (변경 없음)
- `src/HnVue.Dicom/MppsScu.cs`
- `src/HnVue.Dicom/DicomStoreScu.cs`
- `src/HnVue.Dicom/DicomService.cs`
- `src/HnVue.Dicom/DicomOutbox.cs`

## Exclusions (What NOT to Build)

1. **프로덕션 SDK 교체 없음** — OwnDetectorAdapter의 TODO/NotImplementedException은 SDK 도착 후 작업
2. **IncidentRepository DB 영속화 없음** — Wave 4에서 처리 (README에 명시됨)
3. **Stub 서비스 구현 없음** — HnVue.App/Stubs는 개발용으로 의도적
4. **CDBurning IMAPI2 구현 없음** — 하드웨어 의존 기능, 별도 작업 필요
5. **UI Converter 구현 없음** — Team Design 관할
6. **알림 채널(Email/SMS) 구현 없음** — 외부 서비스 연동, 별도 SPEC 필요
