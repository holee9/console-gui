# DISPATCH S18-R1 — Coordinator (Integration & UI Contracts)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: Coordinator
## Priority: P1-Critical
## 근거 SPEC/문서: SPEC-COORDINATOR-001 + SPEC-METHODOLOGY-001

---

## 배경

S17-R1 freeze 시점 carryover. 6개 Repository 통합테스트 12+ PASS 잔여.
신 방법론에서 Phase 2 게이트(Phase 1 완료 의존) 적용.

---

## Tasks

### T1: 6 Repository 통합테스트 12+ PASS [P1]
- 설명: tests.integration/에서 6개 Repository 통합 시나리오 검증
- 체크리스트:
  - [ ] 누락된 통합테스트 식별
  - [ ] in-memory SQLite + 실제 서비스로 12+ 시나리오 작성
  - [ ] 모두 PASS 확인
- 완료 조건: 12+ integration tests passing

### T2: UI.Contracts 인터페이스 일관성 점검 [P2]
- 설명: 신 ViewModel 추가 시 발생한 인터페이스 변경 영향 분석
- 완료 조건: 충돌 0건

### T3: Phase 2 진입 게이트 검증 [P1, 신규]
- 설명: SPEC-DISPATCH-V2-001 REQ-DISPATCH-V2-003 Phase 종속성 첫 적용
- 체크리스트:
  - [ ] Phase 1 (Team A, Team B) COMPLETED 확인 후 작업 시작
  - [ ] main 동기화 강제 (git fetch + reset --hard origin/main + stash 보호)
  - [ ] Phase 1 미완료 시 IDLE 보고

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 6 Repo 통합테스트 12+ | NOT_STARTED | CO | P1 | - | S17 carryover |
| T2 | UI.Contracts 점검 | NOT_STARTED | CO | P2 | - | - |
| T3 | Phase 2 게이트 검증 | NOT_STARTED | CO | P1 | - | 신규 (DISPATCH-V2) |

---

## Constraints

- [HARD] 소유 모듈만: HnVue.UI.Contracts, HnVue.UI.ViewModels, HnVue.App, tests.integration/
- [HARD] DesignTime/ 침범 금지 (S08-R1 교훈)
- [HARD] Phase 1 미완료 시 자동 IDLE
- [HARD] 빌드/테스트 검증 없이 COMPLETED 금지
- [HARD] ScheduleWakeup(960초) — Phase 2

## Evidence Required

1. `dotnet build HnVue.sln` 결과
2. `dotnet test tests.integration/ --logger "console;verbosity=detailed"` PASS 수
3. Phase 1 완료 확인 evidence
4. 실질 커밋 SHA

---

## 참고 문서

- `.moai/specs/SPEC-COORDINATOR-001/`, `SPEC-DISPATCH-V2-001/`
- `.claude/rules/teams/coordinator.md`, `dispatch-protocol.md` §6
