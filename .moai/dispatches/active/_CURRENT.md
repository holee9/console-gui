# DISPATCH Current Index — S15-R2 진행 중

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 스케줄 Phase | 비고 |
|----|-------------------|------|-------------|------|
| **Team A** | DISPATCH-S15-R2-TEAM-A.md | **ACTIVE** | Phase 1 | AssemblyResolve 리뷰 |
| **Team B** | DISPATCH-S15-R2-TEAM-B.md | **ACTIVE** | Phase 1 | IDLE CONFIRM |
| **Coordinator** | DISPATCH-S15-R2-COORDINATOR.md | **QUEUED** | Phase 2 | Team A/B 완료 후 |
| **Design** | DISPATCH-S15-R2-DESIGN.md | **ACTIVE** | 별도 | IDLE CONFIRM |
| **QA** | DISPATCH-S15-R2-QA.md | **QUEUED** | Phase 3 | CO 완료 후 — 19건 실패 분석 |
| **RA** | DISPATCH-S15-R2-RA.md | **QUEUED** | Phase 4 | QA 완료 후 |

**→ S15-R2 발행 — Phase 1 (Team A, Team B) + Design ACTIVE**

---

## [HARD] 팀 모니터링 설정 (CC 중앙 제어)

> **CC가 이 값을 동적으로 변경. 모든 팀은 DISPATCH Resolution 시 이 값을 읽어 ScheduleWakeup에 반영.**

| 설정 항목 | 값 | 비고 |
|----------|-----|------|
| **팀 ScheduleWakeup** | **1200초 (20분)** | IDLE/QUEUED 팀의 폴링 간격 |
| **CC CronCreate** | **10분** | CC 모니터링 간격 |
| **ACTIVE 팀 즉시 시작** | 예 | ACTIVE 감지 시 ScheduleWakeup 대기 없이 즉시 작업 시작 |
| **준수 점검** | **매 틱** | 소유권/DISPATCH 범위/커밋 접두사 검증 |

### 팀 ScheduleWakeup 규칙 [HARD]

```
1. DISPATCH Resolution 시 _CURRENT.md의 '팀 ScheduleWakeup' 값 읽기
2. IDLE/QUEUED → ScheduleWakeup(1200)로 대기 (값을 하드코딩하지 말고 이 표에서 읽을 것)
3. ACTIVE 감지 → ScheduleWakeup 대기 없이 즉시 NOT_STARTED→IN_PROGRESS + 작업 시작
4. 작업 완료 후 COMPLETED push → ScheduleWakeup(1200)로 다음 DISPATCH 대기
5. CC가 이 값을 변경하면 다음 폴링 주기부터 자동 적용
```

---

## [HARD] 순차 스케줄링

### Phase 구조

```
Phase 1 (동시 시작):  Team A ──┐
                            Team B ──┤
                                     ↓
Phase 2 (A+B 완료 후):     Coordinator ──┐
                                          ↓
Phase 3 (CO 완료 후):          QA ──┐
                                     ↓
Phase 4 (QA 완료 후):              RA

별도 트랙 (병렬):           Design
```

---

## [HARD] 세션 시작 절차 (모든 팀 필수)

```
Step 0: git pull origin main  ← [HARD] 이 파일 읽기 전 반드시 실행
Step 1: Read _CURRENT.md (이 파일)
Step 2: 자신의 팀 행(row)에서 파일명 + 스케줄 Phase 확인
Step 3: 해당 DISPATCH 파일만 읽기 (다른 팀 DISPATCH 절대 읽기 금지)
Step 4: 상태가 QUEUED이면 → 대기 (CC가 ACTIVE 전환 시까지)
Step 5: 상태가 IDLE이면 → 즉시 IDLE 보고 (다른 작업 금지)
```

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 상태 |
|------|--------|------|
| 2026-04-08~11 | Phase 0 / S04 | `completed/` 아카이브 |
| 2026-04-12~13 | S05~S06 R1~R2 | ALL MERGED |
| 2026-04-14 | S07 R1~R5 | ALL MERGED |
| 2026-04-14 | S08 R1~R2 | ALL MERGED |
| 2026-04-14~15 | S09 R1~R3 | ALL MERGED, QA PASS |
| 2026-04-15~16 | S10 R1~R4 | ALL MERGED, QA CONDITIONAL PASS (81.3%) |
| 2026-04-16~17 | S11 R1~R2 | ALL MERGED, QA CONDITIONAL PASS (99.97%) |
| 2026-04-18~19 | S12 R1~R2 | ALL MERGED, QA PASS (100%) |
| 2026-04-19 | S12 R3~R4 | ALL MERGED (IDLE CONFIRM) |
| 2026-04-19 | S13 R1 | ALL MERGED — Coordinator + Design |
| 2026-04-20 | S13 R2 | ALL MERGED — QA COMPLETED |
| 2026-04-20 | S14 R1 | ALL MERGED — QA CONDITIONAL PASS, RA COMPLETED |
| 2026-04-20 | S14 R2 | ALL MERGED — QA CONDITIONAL PASS (99.59%), RA CMP v2.5 |
| 2026-04-21 | S15 R1 | ALL MERGED — QA CONDITIONAL PASS (99.47%, 19건 실패) |
| **2026-04-21** | **S15 R2** | **진행 중 — AssemblyResolve 리뷰 + 19건 실패 분석** |

---

Updated: 2026-04-21 (S15-R2 발행)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
