# DISPATCH Current Index — S13-R2

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 스케줄 Phase | 비고 |
|----|-------------------|------|-------------|------|
| **Team A** | - | **MERGED** | Phase 1 | 78 tests (Update 40 + SystemAdmin 38) |
| **Team B** | - | **MERGED** | Phase 1 | 208 tests (Imaging 143 + CDBurning 65) |
| **Coordinator** | DISPATCH-S13-R2-COORDINATOR.md | **ACTIVE** | Phase 2 | Phase 1 완료 → Phase 2 오픈 |
| **Design** | - | **MERGED** | 별도 | StudylistView 접근성 + DoseDisplayView 개선 |
| **QA** | DISPATCH-S13-R2-QA.md | **QUEUED** | Phase 3 | Coordinator 완료 후 시작 |
| **RA** | DISPATCH-S13-R2-RA.md | **MERGED** | Phase 4 | 이미 MERGED |

**→ Phase 1 완료 / Phase 2(Coordinator) 오픈 / Design+RA MERGED**

---

## [HARD] 순차 스케줄링 (S13-R2 신규)

> **S13-R1 분석 기반 프로세스 개선: 의존성 기반 순차 시작**

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

### Phase 시작 조건 [HARD]

| Phase | 팀 | 시작 조건 | CC 액션 |
|-------|-----|----------|---------|
| Phase 1 | Team A, Team B | DISPATCH 발행 즉시 | DISPATCH push 후 바로 시작 |
| Phase 2 | Coordinator | Team A **AND** Team B MERGED | CC가 머지 후 Coordinator DISPATCH Status → ACTIVE 전환 |
| Phase 3 | QA | Coordinator MERGED | CC가 머지 후 QA DISPATCH Status → ACTIVE 전환 |
| Phase 4 | RA | QA MERGED | CC가 머지 후 RA DISPATCH Status → ACTIVE 전환 |
| 별도 | Design | DISPATCH 발행 즉시 | Phase 1과 동시 시작, 독립 진행 |

### QUEUED 팀 행동 규칙 [HARD]

- [HARD] QUEUED 상태의 팀은 DISPATCH 파일을 읽되 **구현을 시작하지 않는다**
- [HARD] CC가 DISPATCH Status를 NOT_STARTED → ACTIVE로 전환할 때까지 대기
- [HARD] ACTIVE 전환 후에만 NOT_STARTED → IN_PROGRESS 업데이트 + 작업 시작
- [HARD] Design은 QUEUED 없이 즉시 ACTIVE → 즉시 작업 시작

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

## [HARD] IDLE / QUEUED 상태 절대 규칙

```
_CURRENT.md에서 자신의 팀이:
- IDLE이면: 즉시 IDLE 보고, DISPATCH 검색 금지, 자율 작업 금지
- QUEUED이면: DISPATCH 읽기만 허용, 구현 대기, CC ACTIVE 전환 시까지 대기
- ACTIVE이면: 즉시 NOT_STARTED → IN_PROGRESS 업데이트 + 작업 시작
```

---

## CC 모니터링 프로토콜 v2.0 [HARD — S13-R2 개선]

### CC 모니터링: 10분 간격

```
1. git fetch origin
2. git log --oneline origin/team/* --not main — 미머지 커밋 확인
3. DISPATCH Status 테이블 직접 읽기
4. COMPLETED 감지 → 소유권 검증 → 머지 → Phase 진행:
   a. Phase 1 팀(Team A/B) COMPLETED → 머지 → 양쪽 모두 MERGED 시 Phase 2 오픈
   b. Phase 2 팀(Coordinator) COMPLETED → 머지 → Phase 3 오픈
   c. Phase 3 팀(QA) COMPLETED → 머지 → Phase 4 오픈
   d. Phase 4 팀(RA) COMPLETED → 머지 → 라운드 종료
   e. Design(별도) COMPLETED → 머지 (Phase 독립)
5. Phase 오픈: 해당 팀 DISPATCH Status NOT_STARTED → ACTIVE 전환 + push
6. 10분 후 다시 모니터링 (루프)
```

### Phase 오픈 액션 [HARD]

```
CC가 Phase 진행 조건 충족 시:
1. DISPATCH 파일 내 Status NOT_STARTED → ACTIVE로 Edit
2. _CURRENT.md 해당 팀 상태 QUEUED → ACTIVE로 Edit
3. git add .moai/dispatches/ && git commit && git push origin main
4. "Phase {N} 오픈: {team} ACTIVE 전환" 보고
```

### 팀 상태 보고: 15분 간격 [HARD]

```
팀은 15분마다 자체 DISPATCH Status 확인 + 업데이트:
- 작업 시작 시: NOT_STARTED → IN_PROGRESS + push
- 작업 진행 중: 진행 상황 메모 업데이트 (선택)
- 작업 완료 시: IN_PROGRESS → COMPLETED + 빌드 증거 + push
- 작업 불가 시: NOT_STARTED → BLOCKED + 사유 기재 + push
```

### 팀 상태 판정 기준 (3-layer)

| 레이어 | 확인 방법 | 의미 |
|--------|-----------|------|
| Remote 커밋 | `git log origin/team/* --not main` | push된 작업 (확정) |
| Worktree 로컬 | `.worktrees/{team}/ git status` | 진행 중 작업 (미커밋) |
| DISPATCH Status | DISPATCH 파일 Status 테이블 | 팀 자가보고 상태 |

**자율 진행**: COMPLETED + 빌드 증거 + diff 범위 3개 통과 → 즉시 머지

**정지 조건**: 범위 위반 / 빌드 에러 / BLOCKED 5회 연속 / Safety-Critical 90% 미달 3회

---

## S13-R2 스케줄링 목표

| 지표 | S13-R1 기준 | S13-R2 목표 | 개선 포인트 |
|------|------------|------------|------------|
| CC 모니터링 주기 | 15분 | **10분** | 감지 속도 33% 향상 |
| 팀 보고 주기 | 15분 | **15분** | 유지 |
| Phase 진행 | 전팀 동시 | **순차** | 의존성 정렬 |
| 라운드 소요 | ~2시간 | **1시간 30분** | 병목 감소 |
| 전팀 MERGED→발행 | 즉시 | **5분 이내** | 유지 |

---

## [HARD] DISPATCH Status 업데이트 의무

```
DISPATCH 읽기 직후    → NOT_STARTED → IN_PROGRESS (즉시 push)
작업 중 15분마다      → Status 확인 + 필요시 메모 업데이트
작업 완료 후          → IN_PROGRESS → COMPLETED + 빌드 증거 (즉시 push)
작업 불가 시          → NOT_STARTED → BLOCKED + 사유 기재 (즉시 push)
```

**Status 업데이트 없이 대기 = 소통 단절 = 프로토콜 위반**

---

## 팀별 역할 경계 (퀵 레퍼런스)

| 팀 | 소유 모듈 | 핵심 금지 사항 |
|----|-----------|---------------|
| **CC** | DISPATCH 관리만 | 소스코드 수정, dotnet 실행, 구현 에이전트 호출 |
| **Team A** | Common, Data, Security, SystemAdmin, Update | 다른 팀 모듈 수정 |
| **Team B** | Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning | 다른 팀 모듈 수정 |
| **Coordinator** | UI.Contracts, UI.ViewModels, App, tests.integration | DesignTime/ 수정 |
| **Design** | UI Views, Styles, Themes, Components, Assets, DesignTime | tests.integration/ 수정, _CURRENT.md/타팀DISPATCH 수정 |
| **QA** | .github/workflows, scripts/ci, scripts/qa | 소스코드 수정 |
| **RA** | docs/regulatory, docs/planning, docs/risk, docs/verification | 소스코드 수정 |

> **상세**: `.claude/rules/teams/role-matrix.md` (CONSTITUTIONAL)

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
| **2026-04-19** | **S13 R2** | **ACTIVE — 순차 스케줄링 v1.0** |

---

## CC 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 → completed/ 이동
2. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
3. 이 표 업데이트 (Phase/QUEUED/ACTIVE 지정)
4. Phase 1 팀 + Design만 ACTIVE, 나머지 QUEUED
5. git add .moai/dispatches/ && git commit && git push origin main
6. CC 모니터링 루프 즉시 시작 (10분 간격)
```

---

## S13-R1 운영 분석 기반 개선 사항

### 발견된 문제
1. **전팀 동시 시작**: 의존성 무시 → Coordinator가 A/B 코드 없이 통합테스트 작성
2. **Design DISPATCH 관리 위반**: Design이 _CURRENT.md + QA DISPATCH 파일 수정 (S13-R1)
3. **CC 모니터링 지연**: 15분 간격으로 COMPLETED 감지 지연

### 개선 조치
1. **순차 스케줄링**: 의존성 기반 Phase 구조 (A,B → CO → QA → RA)
2. **Design 독립 트랙**: 의존성 없으므로 Phase 1과 병렬 진행
3. **CC 모니터링 10분**: 감지 속도 향상
4. **DISPATCH 관리 강화**: Design 팀 역할 경계에 명시적 금지 항목 추가

Updated: 2026-04-19 (S13-R2: 순차 스케줄링 v1.0 + CC 10분 모니터링)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
