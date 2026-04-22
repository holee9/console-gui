# DISPATCH S16-R2 — Coordinator (Integration)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: Coordinator
## Priority: HIGH (P0-Blocker SPEC 실행)
## 근거 SPEC: SPEC-COORDINATOR-001 (Null Repository Stub 교체 — EF Core 실제 구현)

---

## 배경

SPEC-COORDINATOR-001은 P0-Blocker로 2026-04-11 승인되었으나 spec.md만 존재.
DI 컨테이너의 6개 NullRepository stub으로 인해 Dose 측정, Worklist 조회, Incident 보고,
Update 처리, SystemSettings 저장, CD Burning 스터디 조회가 실제 DB와 연동되지 않는 상태.
이번 라운드에서 planning 산출물 완성 + 6개 중 1개 Repository 실구현.

---

## Tasks

### T1: SPEC-COORDINATOR-001 Planning 산출물 작성 [P1]
- **설명**: `.moai/specs/SPEC-COORDINATOR-001/`에 research/plan/acceptance/tasks 작성
- **체크리스트**:
  - [ ] research.md — 현재 App.xaml.cs에서 NullRepository 등록 지점 6개 식별 + 기존 DbContext 구조 조사
  - [ ] plan.md — EF Core Repository 6개 구현 순서 (의존성 순으로) + DI 등록 교체 전략
  - [ ] acceptance.md — 통합테스트 6개 수용 기준 명시
  - [ ] tasks.md — Repository 단위로 Task 분할 (EfDose, EfWorklist, EfIncident, EfUpdate, EfSystemSettings, EfCdStudy)
- **완료 조건**: 4개 파일 생성 + commit + push

### T2: EfDoseRepository 또는 EfWorklistRepository 1개 실구현 + 통합테스트 [P2]
- **설명**: 6개 중 1개를 선택하여 EF Core 기반 실구현 완료
- **체크리스트**:
  - [ ] 선택한 Repository (IDoseRepository 또는 IWorklistRepository) 실구현 작성
  - [ ] `HnVue.App/App.xaml.cs`에서 Null → EF 구현체로 교체
  - [ ] `tests.integration/HnVue.IntegrationTests/`에 통합테스트 1개 이상 추가 (in-memory SQLite)
  - [ ] `dotnet test tests.integration/` 통과 확인
- **완료 조건**: 1개 Repository 실구현 + 통합테스트 1개 PASS

### T3: DISPATCH Status 실시간 업데이트
- **설명**: Planning 산출물 생성 후 IN_PROGRESS, 구현 완료 시 COMPLETED
- **완료 조건**: 타임스탬프 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | SPEC-COORDINATOR-001 Planning 산출물 | NOT_STARTED | Coordinator | P1 | - | research/plan/acceptance/tasks |
| T2 | Repository 1개 실구현 + 통합테스트 | NOT_STARTED | Coordinator | P2 | - | EfDose 또는 EfWorklist |
| T3 | DISPATCH Status 업데이트 | NOT_STARTED | Coordinator | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.UI.Contracts`, `HnVue.UI.ViewModels`, `HnVue.App`, `tests.integration/`
- [HARD] `HnVue.Data` 내부 Repository 클래스 신규 추가는 **Team A에 요청** (DISPATCH에 `NEEDS_TEAM_A` 태그)
- [HARD] Repository 인터페이스 변경 금지 (SPEC 범위 외)
- [HARD] DesignTime/ 수정 금지 (Design 단독 소유)
- [HARD] 통합테스트는 in-memory SQLite 사용 (실 DB 의존성 없음)
- [HARD] ScheduleWakeup(300초 이상) 유지 — 작업 완료 push 직후 재설정 (session-lifecycle.md §2)

## Evidence Required

완료 보고 시:
1. `dotnet build HnVue.sln` 0 errors
2. `dotnet test tests.integration/` 결과 (추가된 테스트 PASS 확인)
3. App.xaml.cs DI 등록 diff
4. 생성된 plan/acceptance/tasks 파일 경로

---

## Team A 협업 포인트

T2에서 선택한 Repository의 실구현(예: `EfDoseRepository.cs`)은 `HnVue.Data/Repositories/`에 생성되므로
Team A의 소유 모듈에 속함. Coordinator는 **인터페이스 설계 + DI 교체 + 통합테스트**를 담당하고,
실제 EF 코드는 Team A DISPATCH로 위임 가능. 이번 라운드에서는 Coordinator가 초안을 작성하고 S17에서 Team A가 품질 보강하는 분담 권장.

---

## 참고 문서

- `.moai/specs/SPEC-COORDINATOR-001/spec.md` — REQ-COORD-001 ~ 006
- `src/HnVue.App/App.xaml.cs` — 현재 NullRepository 등록 지점
- `.claude/rules/teams/coordinator.md` — DI Registration Standards
