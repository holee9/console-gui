---
name: hnvue-dev
description: >
  HnVue 의료영상 WPF 데스크톱 애플리케이션 개발 오케스트레이터.
  17개 모듈(Common, Data, Security, Dicom, Detector, Dose, Workflow, UI 등)을 6팀(Infra, Medical, UI, Coordinator, QA, RA) 전문 에이전트로 라우팅.
  HnVue 개발, 구현, 빌드, 테스트, 커버리지, 리팩토링, 모듈 작업 요청 시 반드시 이 스킬 사용.
  후속 작업: 결과 수정, 부분 재실행, 업데이트, 보완, 다시 실행, 이전 결과 개선 요청 시에도 사용.
  HnVue, hnvue, 의료영상, 디텍터, DICOM, 워크플로우, 환자관리, 선량, UI, 로그인, 테마 관련 작업 시 트리거.
user-invocable: true
metadata:
  version: "1.0.0"
  category: "workflow"
  status: "active"
  updated: "2026-04-09"
---

# HnVue Development Orchestrator

HnVue 의료영상 WPF 데스크톱 애플리케이션의 개발 작업을 적절한 팀 전문 에이전트로 라우팅하는 오케스트레이터.

## Execution Mode: Sub-agent (Expert Pool)

## Module-to-Agent Routing Map

| Agent | Modules | Trigger Keywords |
|-------|---------|-----------------|
| hnvue-infra | Common, Data, Security, SystemAdmin, Update | DB, migration, repository, authentication, JWT, bcrypt, NuGet, security, audit, common, data, systemadmin, update |
| hnvue-medical | Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning | DICOM, detector, FPD, imaging, dose, incident, workflow, patient, MWL, C-STORE, state machine, acquisition, CD burning |
| hnvue-ui | UI Views, Styles, Themes, Components, Converters, DesignTime | XAML, view, style, theme, MahApps, component, button, converter, accessibility, design, PPT, 화면, 뷰, 스타일, 테마 |
| hnvue-coordinator | UI.Contracts, UI.ViewModels, App | interface, DI, ViewModel, composition, registration, contract, integration, App.xaml, MainWindow |
| hnvue-qa | Coverage, Analysis, Architecture Tests, Release | coverage, test, quality, architecture, mutation, StyleCop, SonarCloud, release, 커버리지, 빌드, 테스트 |
| hnvue-ra | Regulatory Documents, SBOM, RTM, Risk | IEC 62304, FDA, SBOM, RTM, SOUP, FMEA, regulatory, compliance, DOC-, 규제, 문서 |

## Workflow

### Phase 0: Context Check

1. Check if `_workspace/` exists in project root
2. Determine execution mode:
   - `_workspace/` absent -> Initial execution (Phase 1)
   - `_workspace/` exists + user requests partial fix -> Re-invoke specific agent only
   - `_workspace/` exists + new input -> Move to `_workspace_{timestamp}/`, start Phase 1

### Phase 1: Request Analysis & Routing

1. Analyze user request to identify affected modules
2. Map modules to responsible agent(s) using routing table above
3. Determine execution strategy:
   - **Single team**: Direct delegation to one agent
   - **Multi-team**: Sequential or parallel delegation based on dependencies
   - **Cross-module**: Coordinator agent required for interface-touching changes

### Phase 2: Agent Dispatch

For each identified agent, invoke with:

```
Agent(
  description: "{task summary}",
  subagent_type: "hnvue-{team}",
  model: "opus",
  prompt: "{detailed task description with module paths, constraints, and expected output}"
)
```

**Parallel dispatch rules**:
- Independent modules (no shared interfaces) -> parallel `run_in_background: true`
- Interface-dependent changes -> sequential (infrastructure first, then consumers)
- Cross-module integration -> Coordinator last (after all teams complete)

**Write operations**: Always `run_in_background: false` for agents that modify files.

### Phase 3: Integration & Verification

After agent(s) complete:

1. If multi-team: verify cross-module consistency
2. If interface changes: invoke hnvue-coordinator for contract verification
3. Run solution build: `dotnet build HnVue.sln -c Release` or MSBuild
4. Run relevant tests: `dotnet test` for affected test projects
5. Report results to user

### Phase 4: Quality Check (if applicable)

For non-trivial changes:
1. Invoke hnvue-qa for coverage analysis on affected modules
2. If safety-critical modules touched: verify 90%+ coverage
3. If NuGet changes: flag for RA SBOM update

## Cross-Team Dependency Rules

```
HnVue.Common (Team A) <- used by ALL modules
HnVue.Data (Team A) <- used by Medical, Coordinator
HnVue.UI.Contracts (Coordinator) <- used by UI, ViewModels
HnVue.Security (Team A) <- used by App, SystemAdmin
```

**Execution order for cross-module changes**:
1. Team A (infrastructure changes)
2. Team B (medical domain changes) — parallel with Team Design (UI changes)
3. Coordinator (integration, DI registration)
4. QA (verification)
5. RA (regulatory document updates, if triggered)

## Build Commands

- Solution build: `"D:/Program Files/Microsoft Visual Studio/2022/Professional/MSBuild/Current/Bin/MSBuild.exe" HnVue.sln /p:Configuration=Release /restore`
- Dotnet build: `dotnet build HnVue.sln -c Release`
- Run tests: `dotnet test HnVue.sln -c Release --no-build`
- Specific test: `dotnet test tests/HnVue.{Module}.Tests/ -c Release`

## Error Handling

| Situation | Strategy |
|-----------|----------|
| Agent fails | 1 retry with simplified scope. If still fails, report partial result |
| Build fails after changes | Invoke hnvue-qa to diagnose, then re-invoke responsible agent |
| Cross-module conflict | Invoke hnvue-coordinator to resolve interface mismatch |
| Coverage drops below threshold | Invoke hnvue-qa for gap analysis, then responsible team for test additions |

## Test Scenarios

### Normal Flow
1. User requests "Detector 모듈에 연결 타임아웃 설정 추가"
2. Phase 1: Detector -> hnvue-medical
3. Phase 2: Dispatch hnvue-medical with task details
4. Phase 3: Build HnVue.sln, run Detector tests
5. Result: Implementation + passing tests

### Cross-Module Flow
1. User requests "환자 데이터 모델에 새 필드 추가"
2. Phase 1: PatientManagement (medical) + Data (infra) + possible UI changes
3. Phase 2: hnvue-infra first (Data layer), then hnvue-medical (PM), then hnvue-coordinator (integration)
4. Phase 3: Full solution build + integration tests
5. Phase 4: hnvue-qa for coverage check

### Error Flow
1. Phase 2: hnvue-medical agent fails on Dose module
2. Retry once with narrower scope
3. If retry fails: report to user with error details, suggest manual intervention
4. Continue with non-Dose portions if possible

## Domain Reference

For detailed module architecture, safety requirements, and team-specific standards, read `${CLAUDE_SKILL_DIR}/references/domain-map.md`.
