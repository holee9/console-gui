# DISPATCH Current Index — S15-R3 진행 중

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 스케줄 Phase | 비고 |
|----|-------------------|------|-------------|------|
| **Team A** | - | **IDLE** | - | S15-R3 MERGED |
| **Team B** | - | **IDLE** | - | S15-R3 MERGED |
| **Coordinator** | - | **IDLE** | - | S15-R3 MERGED |
| **Design** | DISPATCH-S15-R3-DESIGN.md | **ACTIVE** | 별도 | 미응답 (S15-R2~R3 연속) |
| **QA** | - | **IDLE** | - | S15-R3 BLOCKED (Bash 권한 거부) |
| **RA** | - | **IDLE** | - | S15-R3 MERGED (직접 main push) |

**→ S15-R3: 5/6 처리 완료, Design만 대기 (TIMEOUT까지 ~15분)**

---

## [HARD] 팀 모니터링 설정 (CC 중앙 제어)

| 설정 항목 | 값 | 비고 |
|----------|-----|------|
| **팀 ScheduleWakeup** | **1200초 (20분)** | 폴링 간격 |
| **CC CronCreate** | **10분** | CC 모니터링 간격 |
| **ACTIVE 팀 즉시 시작** | 예 | ACTIVE 감지 시 즉시 작업 시작 |

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 상태 |
|------|--------|------|
| 2026-04-21 | S15 R1 | ALL MERGED — QA CONDITIONAL PASS (99.47%) |
| 2026-04-21 | S15 R2 | ALL MERGED — Design TIMEOUT (미응답) |
| **2026-04-21** | **S15 R3** | **5/6 완료 — 4MERGED + QA BLOCKED + Design 대기** |

---

Updated: 2026-04-21 (S15-R3 5/6 — QA BLOCKED 처리, Design TIMEOUT 대기)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
