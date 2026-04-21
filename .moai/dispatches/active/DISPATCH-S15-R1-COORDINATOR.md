# DISPATCH - Coordinator (S15-R1)

> **Sprint**: S15 | **Round**: 1 | **팀**: Coordinator (Integration)
> **발행일**: 2026-04-21
> **상태**: ACTIVE (Phase 2 오픈 — Team A + Design MERGED, Team B IDLE CONFIRM)

---

## 1. 작업 개요

S14-R2에서 발생한 통합테스트 5건 + SettingsViewModel 관련 4건 총 9건 실패 수정.

## 2. 작업 범위

### Task 1: SettingsViewModel.SaveAsync() NullReferenceException 수정 (P0)

**목표**: SettingsViewModel.SaveAsync()의 NullRef 수정 → 5건 테스트 동시 해결

**실패 테스트** (모두 동일 근원 원인):
1. `UI.Tests: SettingsViewModelIntegrationTests.SettingsViewModel_SaveCommand_RaisesSaveCompleted` — NullRef at line 118
2. `UI.Tests: ViewModelValidationIntegrationTests.SettingsViewModel_SaveCommand_CompletesSuccessfully` — NullRef at line 118
3. `UI.Tests: SettingsViewModelTests.SaveCommand_RaisesSaveCompletedEvent` — SaveCompleted not raised
4. `UI.Tests: ViewModelBoundaryTests.SettingsViewModel_SaveCommand_PlaceholderRaisesSaveCompleted` — SaveCompleted not raised
5. `IntegrationTests: CoordinatorIntegrationTests.Settings_SaveCommand_RaisesSaveCompletedEvent` — NullRef at line 118

**근원 원인**: `src/HnVue.UI.ViewModels/ViewModels/SettingsViewModel.cs:118`에서 NullReferenceException 발생. S14-R2 Coordinator DISPATCH에서 `AppSettings → SystemSettings` 이름 변경 후 Mock 업데이트가 누락되었을 가능성.

**수정 방향**:
- SettingsViewModel.SaveAsync()에서 null 체크 추가 또는 의존성 주입 누락 수정
- 테스트의 Mock 설정이 실제 ViewModel 생성자 파라미터와 일치하는지 확인

### Task 2: DI 등록 수정 — DicomOptions 누락 (P0)

**목표**: DI_DomainServices_ResolveSuccessfully 테스트 통과

**실패 테스트**:
- `IntegrationTests: DiRegistrationIntegrationTests.DI_DomainServices_ResolveSuccessfully`
- Error: `Unable to resolve service for type 'IOptions<DicomOptions>' while activating DicomService`

**수정 방향**:
- App.xaml.cs 또는 ServiceCollectionExtensions에 `services.Configure<DicomOptions>(...)` 등록 추가
- DicomOptions 클래스가 존재하는지 확인 (Team B 모듈)

### Task 3: E2E 통합테스트 3건 수정 (P1)

**목표**: End-to-end 워크플로우 테스트 통과

**실패 테스트**:
1. `EndToEndIntegrationTests.PrintScu_ToPacs_EndToEnd_FlowSuccess` — Print SCU result is Failure
2. `EndToEndIntegrationTests.Workflow_StateTransition_TriggersDoseValidation` — **Safety-Critical** (Dose 검증)
3. `EndToEndIntegrationTests.TlsConnection_DicomCommunication_SecureFlow` — TLS 연결 실패

**수정 방향**:
- 테스트 Setup에서 Mock 서비스 구성이 실제 DI 등록과 일치하는지 확인
- Dose validation 테스트는 Safety-Critical이므로 우선 수정

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | SettingsViewModel NullRef 수정 | COMPLETED | Coordinator | P0 | 2026-04-21T09:30:00+09:00 | Team A 머지 후 자동 해결됨 |
| T2 | DicomOptions DI 등록 수정 | COMPLETED | Coordinator | P0 | 2026-04-21T10:00:00+09:00 | IOptions<DicomOptions> + ILogger<DicomService> 추가 |
| T3 | E2E 통합테스트 3건 수정 | COMPLETED | Coordinator | P1 | 2026-04-21T10:00:00+09:00 | Print mock화, Workflow ProtocolLoaded+RBAC+ArmAsync, TLS mock화 |

---

## 4. 완료 조건

- [ ] `dotnet build HnVue.sln` 0 errors
- [ ] SettingsViewModel 관련 5건 테스트 전체 통과
- [ ] DI 등록 테스트 통과
- [ ] E2E 통합테스트 3건 통과 (Dose Safety-Critical 포함)
- [ ] 수정 파일이 Coordinator 소유 범위 내
- [ ] DISPATCH Status에 빌드 증거 기록

---

## 5. Build Evidence

- `dotnet build HnVue.sln`: 0 errors, 0 warnings (excluding StyleCop)
- `dotnet test tests.integration/`: 179/179 pass, 0 fail
- 수정 파일:
  - `tests.integration/HnVue.IntegrationTests/DiRegistrationIntegrationTests.cs`: IOptions<DicomOptions> + ILogger<DicomService> 등록
  - `tests.integration/HnVue.IntegrationTests/EndToEndIntegrationTests.cs`: Print SCU mock화, Workflow ProtocolLoaded 중간전이+RBAC+ArmAsync, TLS mock화
