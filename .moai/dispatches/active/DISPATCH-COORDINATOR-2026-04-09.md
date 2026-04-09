# DISPATCH: Coordinator — Coverage Push (가장 큰 갭 30.1pp)

Issued: 2026-04-09
Issued By: Main (MoAI Orchestrator)
Priority: P1-Critical
Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Execute tasks in order (Task 1 must complete before others)
3. Update Status section after each task
4. Run build verification as final step

## Context

QA Phase 1 확정 기준:
- Coordinator 평균 목표: **72.5%+** (현재 42.4%, 갭 30.1pp — 전 팀 중 최대)
- UI.Contracts: 42.8% → 70.0% (갭 27.2pp)
- UI.ViewModels: 42.0% → 75.0% (갭 33.0pp)
- HnVue.App: integration scenario 6개 이상
- 전체 interim gate: 80%+

## File Ownership

- HnVue.UI.Contracts/**
- HnVue.UI.ViewModels/**
- HnVue.App/**
- tests.integration/HnVue.IntegrationTests/**

## Tasks

### Task 1: UI.ViewModels 커버리지 42.0% → 75.0% (갭 33.0pp, P1-Critical)

**0% 클래스 (최우선)**:
- AddPatientProcedureViewModel (0%)
- MergeViewModel (0%)
- SettingsViewModel (0%)
- StudylistViewModel (0%)

**50% 미만 클래스**:
- MainViewModel (42.6%)
- CDBurnViewModel (42.8%)

**테스트 작성 대상**:
- 각 ViewModel의 Command 실행 (CanExecute + Execute)
- Property 변경 알림 (INotifyPropertyChanged)
- 생성자 의존성 주입 검증
- 예외 경로 (null 인자, 무효 상태)

**기준**:
- xUnit + FluentAssertions
- [Trait("SWR", "SWR-xxx")] 어노테이션
- Mock 사용 (Moq — domain service 인터페이스)

**검증 기준**:
- [ ] UI.ViewModels line coverage 75%+
- [ ] 0% 클래스 4개 모두 50%+ 달성
- [ ] 빌드 + 테스트 통과

### Task 2: UI.Contracts 커버리지 42.8% → 70.0% (갭 27.2pp, P1-Critical)

**0% 클래스**:
- NavigationRequestedMessage (0%)
- PatientSelectedMessage (0%)
- SessionTimeoutMessage (0%)

**테스트 작성 대상**:
- 이벤트 메시지 생성/직렬화 테스트
- 인터페이스 기본 구현 테스트 (있는 경우)
- Contracts 모델 유효성 검증

**검증 기준**:
- [ ] UI.Contracts line coverage 70%+
- [ ] 0% 클래스 모두 커버
- [ ] 빌드 + 테스트 통과

### Task 3: HnVue.App Integration Scenario 보강 (P2-High)

**목적**: DI 컨테이너 완전성 + 모듈 간 상호작용 검증

**시나리오 6개 이상**:
1. 전체 서비스 DI 해석 (resolve all registered services)
2. ViewModel → Service 주입 검증
3. Login → Navigation 연동
4. Patient 선택 → Workflow 시작 연동
5. Security → Audit 로깅 연동
6. Settings 변경 → 서비스 재구성 연동

**검증 기준**:
- [ ] 신규 integration scenario 6개+
- [ ] HnVue.IntegrationTests 전체 green
- [ ] 빌드 + 테스트 통과

### Task 4: BUG-001 Touch Target 기본값 수정 (P2-High)

**현재**: CoreTokens.xaml 버튼 기본 높이 36px (Medium)
**목표**: 기본 버튼 높이 44px (IEC 62366 의료기기 기준)

**수정 대상**:
- `src/HnVue.UI/Styles/CoreTokens.xaml` — ButtonHeightMedium 또는 DefaultButtonHeight 44로 변경

**검증 기준**:
- [ ] 모든 대화형 버튼 최소 44x44px
- [ ] 빌드 통과

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
