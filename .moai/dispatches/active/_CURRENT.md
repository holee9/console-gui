# DISPATCH Current Index — S15-R2 진행 중

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 스케줄 Phase | 비고 |
|----|-------------------|------|-------------|------|
| **Team A** | DISPATCH-S15-R2-TEAM-A.md | **ACTIVE** | Phase 1 | 동작 확인 |
| **Team B** | DISPATCH-S15-R2-TEAM-B.md | **MERGED** | Phase 1 | IDLE CONFIRM 완료 |
| **Coordinator** | DISPATCH-S15-R2-COORDINATOR.md | **ACTIVE** | Phase 2 | 동작 확인 |
| **Design** | DISPATCH-S15-R2-DESIGN.md | **ACTIVE** | 별도 | 동작 확인 |
| **QA** | DISPATCH-S15-R2-QA.md | **ACTIVE** | Phase 3 | 동작 확인 |
| **RA** | DISPATCH-S15-R2-RA.md | **ACTIVE** | Phase 4 | 동작 확인 |

**→ S15-R2 전팀 ACTIVE — 동작 확인용 IDLE CONFIRM 요청**

---

## [HARD] 팀 모니터링 설정 (CC 중앙 제어)

| 설정 항목 | 값 | 비고 |
|----------|-----|------|
| **팀 ScheduleWakeup** | **1200초 (20분)** | IDLE/QUEUED 팀의 폴링 간격 |
| **CC CronCreate** | **10분** | CC 모니터링 간격 |
| **ACTIVE 팀 즉시 시작** | 예 | ACTIVE 감지 시 즉시 작업 시작 |
| **준수 점검** | **매 틱** | 소유권/DISPATCH 범위/커밋 접두사 |

### 팀 ScheduleWakeup 규칙 [HARD]

```
1. DISPATCH Resolution 시 _CURRENT.md의 '팀 ScheduleWakeup' 값 읽기
2. IDLE/QUEUED → ScheduleWakeup(1200)로 대기
3. ACTIVE 감지 → 즉시 NOT_STARTED→IN_PROGRESS + 작업 시작
4. 작업 완료 후 COMPLETED push → ScheduleWakeup(1200)로 다음 DISPATCH 대기
```

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 상태 |
|------|--------|------|
| 2026-04-21 | S15 R1 | ALL MERGED — QA CONDITIONAL PASS (99.47%) |
| **2026-04-21** | **S15 R2** | **진행 중 — 전팀 동작 확인** |

---

Updated: 2026-04-21 (S15-R2 전팀 ACTIVE 재설정)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
