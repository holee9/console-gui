# DISPATCH Current Index — S13-R1

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Team A** | - | **MERGED** | S13-R1 COMPLETED — Tier 1 보안 통제 (23 files, 998 LOC) |
| **Team B** | - | **MERGED** | Print SCU + RDSR + PACS 비동기 + 선량 인터락 |
| **Coordinator** | - | **MERGED** | ViewModel TODO 해결 + 통합테스트 보강 + DI 등록 검증 |
| **Design** | - | **MERGED** | AddPatientProcedureView(Slide 8) + WorkflowView(Slides 9-11) 리디자인 |
| **QA** | - | **MERGED** | S13-R1 QA Gate PASS — 빌드/테스트/커버리지 검증 + 아키텍처 테스트 |
| **RA** | - | **MERGED** | S13-R1 COMPLETED — STRIDE + RTM v2.8 + CMP v2.4 |

**→ 전팀 MERGED / S13-R1 완료**

---

## [HARD] 세션 시작 절차 (모든 팀 필수)

```
Step 0: git pull origin main  ← [HARD] 이 파일 읽기 전 반드시 실행
Step 1: Read _CURRENT.md (이 파일)
Step 2: 자신의 팀 행(row)에서 파일명 확인
Step 3: 해당 파일만 읽기 (다른 팀 DISPATCH 절대 읽기 금지)
Step 4: 상태가 IDLE이면 → 즉시 IDLE 보고 (다른 작업 금지)
```

---

## [HARD] IDLE 상태 절대 규칙

```
_CURRENT.md에서 자신의 팀이 IDLE이면:
1. 즉시 IDLE 보고
2. DISPATCH 파일 검색 금지
3. 자율 작업 금지
4. CC 지시 대기
```

**위반 시 프로토콜 위반으로 간주합니다.**

---

## S12 자율주행 최적화 목표

### 팀별 모니터링 스케줄 (S11 데이터 기반 최적화)

| 역할 | 모니터링 주기 | 동기화 방식 | 비고 |
|------|------------|------------|------|
| **CC** | **15분** | git log + DISPATCH Status 확인 | S11 달성 기반, 전 라운드 목표 |
| **Team A (인프라)** | 15분 | DISPATCH Status 주기 업데이트 | 빌드 포함 적정 주기 |
| **Team B (의료영상)** | 15분 | DISPATCH Status 주기 업데이트 | 빌드 포함 적정 주기 |
| **Coordinator (통합)** | 15분 | DISPATCH Status 주기 업데이트 | 통합테스트 포함 |
| **Design (순수UI)** | 20분 | DISPATCH Status 주기 업데이트 | XAML 작업 특성 반영 |
| **QA (품질보증)** | 10분 | DISPATCH Status 주기 업데이트 | 분석/보고 위주, 신속 |
| **RA (규제)** | 10분 | DISPATCH Status 주기 업데이트 | 문서 작업, 신속 |

### S13 목표 메트릭

| 지표 | S12 기준 | S13 목표 |
|------|----------|----------|
| 라운드 소요 시간 | S12-R4 IDLE | 1시간 15분 |
| CC 모니터링 횟수 | 6회 이상/라운드 | 6회 이상/라운드 |
| 전팀 MERGED→발행 | 즉시 | 5분 이내 |
| 팀간 완료 편차 | 5분 이내 | 5분 이내 |

---

## [HARD] CC 모니터링 절차 (S13 기준 — Worktree 인식 추가)

**CC 모니터링**: 15분 간격

```
1. git fetch origin
2. git log --oneline origin/team/* --not main (6팀) — remote 커밋 확인
3. [S13 추가] .worktrees/{team}/ 로컬 상태 확인:
   - git status --short — 미커밋 변경 파일 존재 = 활발히 작업 중
   - git log --oneline -1 — 마지막 커밋 시간 파악
   - 미커밋 변경 있음 → "WORKING (uncommitted)" 상태로 인식
   - 미커밋 변경 없음 + remote 커밋 없음 → "IDLE/NOT_STARTED" 상태
4. DISPATCH 파일 Status 테이블 직접 읽기 확인
5. COMPLETED 감지 → 소유권 검증 → 머지 → _CURRENT.md 업데이트
6. 15분 후 다시 모니터링 (루프)
```

**팀 상태 판정 기준 (3-layer)**:

| 레이어 | 확인 방법 | 의미 |
|--------|-----------|------|
| Remote 커밋 | `git log origin/team/* --not main` | push된 작업 (확정) |
| Worktree 로컬 | `.worktrees/{team}/ git status` | 진행 중 작업 (미커밋) |
| DISPATCH Status | DISPATCH 파일 Status 테이블 | 팀 자가보고 상태 |

**자율 진행**: COMPLETED + 빌드 증거 + diff 범위 3개 통과 → 즉시 머지, 사용자 확인 불필요

**정지 조건**: 범위 위반 / 빌드 에러 / BLOCKED 5회 연속 / Safety-Critical 90% 미달 3회

---

## [HARD] DISPATCH Status 업데이트 의무

```
DISPATCH 읽기 직후    → NOT_STARTED → IN_PROGRESS (즉시 push)
작업 중 주기적으로    → Status 확인 + 필요시 메모 업데이트
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
| **Design** | UI Views, Styles, Themes, Components, Assets, DesignTime | tests.integration/ 수정 |
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
| 2026-04-19 | S12 R3 | ALL MERGED, QA PASS (100%) |
| 2026-04-19 | S12 R4 | ALL MERGED (IDLE CONFIRM) |
| **2026-04-19** | **S13 R1** | **ALL MERGED** — Coordinator + Design 최종 머지 |

---

## CC 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 → completed/ 이동
2. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
3. 이 표 업데이트 (전팀 ACTIVE or IDLE)
4. git add .moai/dispatches/ && git commit && git push origin main
5. 전팀 머지 후 즉시 다음 라운드 기획 (자율 진행)
```

Updated: 2026-04-19 (S13-R1: ACTIVE - Tier 1 보안 완성 + DICOM 프로토콜)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
