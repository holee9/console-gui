# DISPATCH: Coordinator — 빌드오류 수정 + ViewModel/Contracts 커버리지 + Integration 보강

Issued: 2026-04-10 (v2 교차검증 보강)
Issued By: Main (MoAI Commander Center)
Priority: **P0-Blocker** (빌드 오류 7건) + P1-Critical (커버리지 갭 30.1pp)
Supersedes: 이전 DISPATCH (IN_PROGRESS, 체크 0/10)

## Coordinator 역할 재확인 (.claude/rules/teams/coordinator.md)

- **소유 모듈**: UI.Contracts (인터페이스 게이트), UI.ViewModels (도메인 합성), App (DI 루트), IntegrationTests
- **SOLE modifier**: UI.Contracts 인터페이스 유일한 수정 권한자
- **DI 기준**: Singleton(무상태)/Scoped(요청별)/Transient(경량), 미등록=앱 크래시
- **ViewModel**: 생성자 주입, 인프라 직접 참조 금지
- **통합 테스트**: 실서비스 + in-memory SQLite, Mock 금지

## How to Execute

1. **Task 0 (P0-Blocker) 최우선** 수행
2. Task 1-3 순서대로 수행
3. 체크박스 + Status 업데이트

## Task 0: IntegrationTests 빌드 오류 수정 (P0-Blocker)

**오류 7건** (2026-04-10 빌드 검증 결과):
- CS1061 `IUserRepository.AddAsync` 미정의 (x6건): TeamAIntegrationTests.cs:80,118,164,213,245,282
- CS7036 `AuditEntry` 생성자 매개변수 누락 (x1건): TeamAIntegrationTests.cs:184

**원인**: Team A의 SPEC-INFRA-001 구현에서 IUserRepository 인터페이스와 AuditEntry 생성자가 변경되었으나, IntegrationTests가 미갱신

**수행**:
1. `IUserRepository` 현재 인터페이스 확인 (AddAsync → 올바른 메서드명 확인)
2. `AuditEntry` 생성자 시그니처 확인
3. `TeamAIntegrationTests.cs` 호출부 수정

**검증 기준**:
- [x] HnVue.IntegrationTests 빌드 오류 0건
- [x] 기존 IntegrationTests 전부 통과 (26/26)
- [x] 빌드 + 테스트 통과

## Task 1: UI.ViewModels 42.0% → 75.0% (P1-Critical)

**0% 클래스**: AddPatientProcedureViewModel, MergeViewModel, SettingsViewModel, StudylistViewModel
**50% 미만**: MainViewModel(42.6%), CDBurnViewModel(42.8%)
**테스트 대상**: Command(CanExecute+Execute), INotifyPropertyChanged, 생성자 DI, 예외

**검증 기준**:
- [ ] UI.ViewModels line coverage 75%+
- [ ] 0% 클래스 4개 모두 50%+
- [ ] 빌드 + 테스트 통과

## Task 2: UI.Contracts 42.8% → 70.0% (P1-Critical)

**0% 클래스**: NavigationRequestedMessage, PatientSelectedMessage, SessionTimeoutMessage
**테스트**: 메시지 생성/직렬화, Contracts 모델 유효성

**검증 기준**:
- [ ] UI.Contracts line coverage 70%+
- [ ] 0% 클래스 모두 커버
- [ ] 빌드 + 테스트 통과

## Task 3: Integration Scenario 보강 (P2-High)

**규칙**: 실서비스 + in-memory SQLite, Mock 금지, {Module}_{Scenario}_{ExpectedResult}

**검증 기준**:
- [ ] 신규 integration scenario 6개+
- [ ] IntegrationTests 전체 green
- [ ] 빌드 + 테스트 통과

## Constraints

- Coordinator 소유 파일만 수정
- UI.Contracts 인터페이스 변경 시 영향 분석 + `interface-contract` 이슈
- ViewModel 인프라 직접 참조 금지


## Final Verification [HARD — 이 섹션 미완료 시 COMPLETED 보고 금지]

1. 자기 모듈 빌드: `dotnet build` → 오류 0건
2. 자기 테스트: `dotnet test {소유 테스트}` → 전원 통과
3. 전체 솔루션 빌드: `dotnet build HnVue.sln -c Release` → 결과 기록
4. 빌드 출력 요약을 Status에 복사

## Git Completion Protocol [HARD]

1. git add (DISPATCH.md + 변경 파일)
2. git commit (conventional commit 형식)
3. git push origin team/coordinator
4. PR 생성 (기존 open PR 확인 후 중복 방지)
5. PR URL을 Status에 기록

## Status

- **State**: IN_PROGRESS
- **Build Evidence**: IntegrationTests 빌드 0 errors 0 warnings, 테스트 26/26 passed
- **PR**: http://10.11.1.40:7001/DR_RnD/Console-GUI/pulls/76
- **Commit**: 305f83b
- **Results**: Task 0→COMPLETED (7/7 errors fixed), Task 1→PENDING, Task 2→PENDING, Task 3→PENDING
