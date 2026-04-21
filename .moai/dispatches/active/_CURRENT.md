# DISPATCH Current Index — S16-R1 ACTIVE

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 스케줄 Phase | 비고 |
|----|-------------------|------|-------------|------|
| **Team A** | DISPATCH-S16-R1-TEAM-A.md | **ACTIVE** | Phase 1 | IDLE CONFIRM |
| **Team B** | DISPATCH-S16-R1-TEAM-B.md | **ACTIVE** | Phase 1 | IDLE CONFIRM |
| **Coordinator** | DISPATCH-S16-R1-COORDINATOR.md | **ACTIVE** | Phase 1 | IDLE CONFIRM |
| **Design** | DISPATCH-S16-R1-DESIGN.md | **ACTIVE** | Phase 1 | ScheduleWakeup 최우선 (2라운드 연속 TIMEOUT 복구) |
| **QA** | DISPATCH-S16-R1-QA.md | **ACTIVE** | Phase 1 | node PATH 문제 해결 필요 |
| **RA** | DISPATCH-S16-R1-RA.md | **ACTIVE** | Phase 1 | IDLE CONFIRM |

**→ S16-R1 ACTIVE — 전팀 Phase 1 동시 시작**

---

## [HARD] 팀 모니터링 설정 (CC 중앙 제어)

| 설정 항목 | 값 | 비고 |
|----------|-----|------|
| **팀 ScheduleWakeup** | **300초 (5분)** | 폴링 간격 |
| **CC CronCreate** | **10분** | CC 모니터링 간격 |
| **ACTIVE 팀 즉시 시작** | 예 | ACTIVE 감지 시 즉시 작업 시작 |

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 상태 |
|------|--------|------|
| 2026-04-21 | S15 R1 | ALL MERGED — QA CONDITIONAL PASS (99.47%) |
| 2026-04-21 | S15 R2 | ALL MERGED — Design TIMEOUT (미응답) |
| 2026-04-21 | S15 R3 | 완료 — 4 MERGED + QA BLOCKED + Design TIMEOUT |
| **2026-04-21** | **S16 R1** | **ACTIVE — 전팀 IDLE CONFIRM + ScheduleWakeup 필수 설정** |

---

## S16-R1 특별 지시

**모든 팀**: /clear 후 세션 재시작 시 ScheduleWakeup이 소멸됩니다.
이번 라운드의 **최우선 작업**은 ScheduleWakeup(300초) 설정입니다.
ScheduleWakeup 없이 IDLE 보고 = 다음 DISPATCH 수신 불가 = TIMEOUT 반복.

---

Updated: 2026-04-21 (S16-R1 ACTIVE — 전팀 Phase 1)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
