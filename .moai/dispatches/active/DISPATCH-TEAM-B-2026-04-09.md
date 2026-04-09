# DISPATCH: Team B — Safety-Critical Coverage Push (평균 갭 7.1pp)

Issued: 2026-04-09
Issued By: Main (MoAI Orchestrator)
Priority: P1-Critical
Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Execute tasks in priority order (Task 1-2 최우선: safety-critical)
3. Update Status section after each task
4. Run build verification as final step

## Context

QA Phase 1 확정 기준:
- Team B 평균 목표: **85%+** (현재 77.9%, 갭 7.1pp)
- Safety-critical hard gate: Dose/Incident **branch coverage 90%+**
- 모듈별 floor 미달 4개:
  - Detector: 42.6% → 85% (갭 **42.4pp** — 전체 최대 갭)
  - Dose: 67.6% → 90% (갭 **22.4pp**, safety-critical)
  - Dicom: 66.9% → 80% (갭 **13.1pp**)
  - PatientManagement: 72.7% → 80% (갭 **7.3pp**)
- 이미 목표 달성: Imaging 87.5%, Incident 94.2%, Workflow 91.4%, CDBurning 100%

## File Ownership

- HnVue.Dicom/**
- HnVue.Detector/**
- HnVue.Imaging/**
- HnVue.Dose/**
- HnVue.Incident/**
- HnVue.Workflow/**
- HnVue.PatientManagement/**
- HnVue.CDBurning/**

## Tasks

### Task 1: Detector 42.6% → 85.0% (갭 42.4pp, P1-Critical)

**0% 클래스 (3개)**:
- OwnDetectorAdapter (0%)
- OwnDetectorConfig (0%)
- VendorAdapterTemplate (0%)

**100% 클래스**: DetectorSimulator (100%)

**테스트 작성 대상**:
- OwnDetectorAdapter: Initialize/Connect/Configure/Acquire/Disconnect 라이프사이클
- OwnDetectorConfig: 설정 유효성 검증, 기본값, 범위 초과
- VendorAdapterTemplate: 추상 메서드 호출 검증
- 시뮬레이터 vs 실제 어댑터 인터페이스 일관성

**주의**: 하드웨어 의존 코드는 Mock/시뮬레이터로 테스트. IDetectorService 추상화 활용.

**기준**:
- xUnit + FluentAssertions
- [Trait("SWR", "SWR-xxx")] 어노테이션

**검증 기준**:
- [ ] Detector line coverage 85%+
- [ ] 0% 클래스 3개 모두 70%+ 달성
- [ ] 빌드 + 테스트 통과

### Task 2: Dose 67.6% → 90.0% (갭 22.4pp, P1-Critical, Safety-Critical)

**0% 클래스**:
- DoseRepository (0%) ← 핵심 갭

**100% 클래스**: DoseService (100%)

**테스트 작성 대상**:
- DoseRepository CRUD (In-Memory SQLite)
- Dose 인터록 4-level 모든 분기 경로 (branch coverage 목표)
- Dose 계산 경계값 (0, max, 음수, overflow)
- 인터록 해제/오버라이드 조건

**HARD GATE**: branch coverage **90%+** (DOC-012 안전성 기준)

**검증 기준**:
- [ ] Dose line coverage 90%+
- [ ] Dose branch coverage 90%+
- [ ] DoseRepository 0% → 80%+
- [ ] 인터록 4-level 전 경로 커버
- [ ] 빌드 + 테스트 통과

### Task 3: Dicom 66.9% → 80.0% (갭 13.1pp, P2-High)

**0% 클래스**:
- MppsScu (0%)

**저커버리지 클래스**:
- DicomOutbox (62.5%)
- DicomService (69.3%)

**테스트 작성 대상**:
- MppsScu: MPPS N-CREATE/N-SET 메시지 생성 테스트
- DicomOutbox: 큐 관리, 재시도 로직, 만료 처리
- DicomService: C-STORE/C-FIND 핸들러 경로

**검증 기준**:
- [ ] Dicom line coverage 80%+
- [ ] MppsScu 0% → 60%+
- [ ] 빌드 + 테스트 통과

### Task 4: PatientManagement 72.7% → 80.0% (갭 7.3pp, P2-High)

**0% 클래스**:
- WorklistRepository (0%)

**100% 클래스**: PatientService 100%, WorklistService 100%

**테스트 작성 대상**:
- WorklistRepository: MWL 쿼리 결과 매핑, 필터링, 페이징
- 환자 검색 시 빈 결과/특수문자 처리

**검증 기준**:
- [ ] PatientManagement line coverage 80%+
- [ ] WorklistRepository 0% → 70%+
- [ ] 빌드 + 테스트 통과

## Build Verification

```bash
dotnet build HnVue.sln --configuration Release
dotnet test HnVue.sln --configuration Release --no-build
```

## Status

- **State**: PENDING
- **Started**: -
- **Completed**: -
- **Results**: -
