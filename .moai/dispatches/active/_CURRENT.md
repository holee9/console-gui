# DISPATCH Current Index — S12-R1

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Team A** | S12-R1-team-a.md | **ACTIVE** | Update 테스트 수정 + 커버리지 85%+ |
| **Team B** | S12-R1-team-b.md | **ACTIVE** | Detector TODO 정리 + Dicom 안정성 |
| **Coordinator** | S12-R1-coordinator.md | **ACTIVE** | UI ViewModels 커버리지 개선 |
| **Design** | S12-R1-team-design.md | **ACTIVE** | UI Tests 커버리지 + DesignTime TODO |
| **QA** | S12-R1-qa.md | **ACTIVE** | 전체 테스트 PASS 판정 |
| **RA** | S12-R1-ra.md | **ACTIVE** | RTM 업데이트 + 릴리즈 준비 |

**→ S12-R1: 6/6 ACTIVE (2026-04-18)**

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

### S12 목표 메트릭

| 지표 | S10 기준 | S11 목표 | S12 목표 |
|------|----------|----------|----------|
| 라운드 소요 시간 | 2시간 1분 | 1시간 30분 | **1시간 15분** |
| CC 모니터링 횟수 | 0회 (실패) | 6회/라운드 | **6회 이상/라운드** |
| 전팀 MERGED→발행 | 11시간 53분 | 10분 이내 | **5분 이내** |
| 팀간 완료 편차 | 18분 | 10분 | **5분** |

---

## [HARD] CC 모니터링 절차 (S12 기준)

**CC 모니터링**: 15분 간격

```
1. git fetch origin
2. git log --oneline origin/team/* --not main (6팀)
3. DISPATCH 파일 Status 테이블 직접 읽기 확인
4. COMPLETED 감지 → 소유권 검증 → 머지 → _CURRENT.md 업데이트
5. 15분 후 다시 모니터링 (루프)
```

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

## [HARD] 완료 프로세스 (Git + Session Lifecycle)

```
1. 작업 완료
2. git add → git commit → git push origin team/{team-name}
3. DISPATCH Status COMPLETED + 빌드 증거 업데이트 → push
4. /clear 실행 [HARD] ← 누락 시 context 누적 → 다음 라운드 작업 불능
5. CC 자동 감지 → 머지 → _CURRENT.md 업데이트
```

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
| **2026-04-18** | **S12 R1** | **ACTIVE** |

---

## CC 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 → completed/ 이동
2. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
3. 이 표 업데이트 (전팀 ACTIVE or IDLE)
4. git add .moai/dispatches/ && git commit && git push origin main
5. 전팀 머지 후 즉시 다음 라운드 기획 (자율 진행)
```

Updated: 2026-04-18 (S12-R1: 6/6 ACTIVE)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
