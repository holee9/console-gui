---
id: SPEC-AUTODRIVE-GATE-001
version: 1.0.0
status: draft
priority: P0
team: meta
title: 자율주행 게이트 — Evidence-Based Completion 강화, 사망 나선 감지, Self-Verification 7항목
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
parent: SPEC-METHODOLOGY-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. SPEC-GOVERNANCE-001 REQ-GOV-001(커밋 의무), REQ-GOV-006(QA 보고서) 흡수 |

---

## 개요 (Overview)

자율주행 사이클의 게이트(통과 조건)를 정의한다. 핵심 목적:
1. **Evidence-Based Completion 강화** — COMPLETED 보고는 빌드/테스트 증거 필수.
2. **사망 나선 감지** — 5 라운드 연속 실질 커밋 0건 시 사용자 알림 (S15~S16-R1 재발 방지).
3. **Self-Verification 7항목** — 기존 6항목 + 1 추가("실질 커밋 발생 여부").
4. **QA 재검증 회귀 메커니즘** — QA가 COMPLETED를 거부할 시 IN_PROGRESS로 자동 회귀.

본 SPEC은 SPEC-CONSTITUTION-001과 SPEC-DISPATCH-V2-001을 전제로 하며, 게이트 통과 조건만 정의한다.

---

## 요구사항 (EARS 형식)

### [REQ-AUTOGATE-001] COMPLETED 보고 시 전체 솔루션 빌드 증거 필수

**유형**: Event-Driven

**WHEN** 구현 팀(Team A, Team B, Coordinator)이 DISPATCH Task 상태를 COMPLETED로 보고할 때,
**THE SYSTEM SHALL** DISPATCH Status 비고 열에 다음 증거를 모두 기재하도록 강제해야 한다:
1. **전체 솔루션 빌드 결과**: `dotnet build HnVue.sln -c Release`의 errors/warnings 수치
2. **자기 소유 테스트 결과**: 추가/수정한 테스트의 PASS/FAIL 개수
3. **변경 파일 목록**: `git diff --name-only` 출력 (소유권 확인용)

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-001 흡수 + quality-standards.md §4 격상.
S14-R2 사고 — 모듈 빌드만으로 COMPLETED 보고 → 다른 모듈 회귀 미감지.

---

### [REQ-AUTOGATE-002] Design/QA/RA 빌드 범위 분기

**유형**: State-Driven

**WHILE** 팀이 빌드 검증을 수행하는 동안,
**THE SYSTEM SHALL** 다음 빌드 범위 기준을 적용해야 한다:

| 팀 | 빌드 범위 |
|----|----------|
| Team A, Team B, Coordinator | **HnVue.sln 전체** (회귀 검증) |
| Design | HnVue.UI 프로젝트 |
| QA | HnVue.sln 전체 |
| RA | 빌드 불필요 (문서 작업) |

**근거**: quality-standards.md §3 격상. Design은 UI 독립적 검증으로 충분.

---

### [REQ-AUTOGATE-003] Self-Verification 7항목 체크리스트

**유형**: Ubiquitous

**THE SYSTEM SHALL** COMPLETED 보고 전에 모든 구현 팀이 다음 7항목을 검증하도록 강제해야 한다:

1. 모든 Task acceptance criteria 충족 여부?
2. 자기 빌드 범위(REQ-AUTOGATE-002 표 기준) 0 errors 확인?
3. 자기 소유 테스트 all passed 확인?
4. 변경 파일이 모두 소유권 범위 내인지 (`git diff --name-only` 확인)?
5. DISPATCH Status 테이블에 빌드 증거 기재?
6. 미완료 항목을 정직하게 PARTIAL로 표시?
7. **(NEW)** 본 라운드에서 자기 팀이 발생시킨 실질 제품 커밋(소스/테스트/문서)이 1건 이상 존재? (메타 작업만 있으면 No)

**금지**: 7번 항목이 No이면서 COMPLETED 보고 = REQ-AUTOGATE-005 사망 나선 감지 트리거.

**근거**: quality-standards.md §3 격상 + 신규 7번 항목으로 사망 나선 핵심 메커니즘 차단.

---

### [REQ-AUTOGATE-004] QA 재검증 실패 시 IN_PROGRESS 자동 회귀

**유형**: Event-Driven

**WHEN** QA 팀이 COMPLETED 보고된 산출물에 대해 재검증을 수행하고 다음 중 하나에 해당하는 경우,
**THE SYSTEM SHALL** 해당 Task의 Status를 자동으로 COMPLETED → IN_PROGRESS로 회귀시키고 사유를 DISPATCH에 기록해야 한다:
- 빌드 증거가 누락 또는 위조 (실제 빌드 결과와 불일치)
- 자기 소유권 외 파일 변경 발견
- Safety-Critical 모듈 커버리지 90% 미달
- Self-Verification 7항목 중 1개 이상 미수행 흔적

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-006 (QA 보고서 파일 저장) + quality-standards.md §5 (QA 독립성) 통합.

---

### [REQ-AUTOGATE-005] 사망 나선 감지 (5 라운드 연속 무효)

**유형**: Event-Driven

**WHEN** 5 라운드 연속으로 7팀 합산 실질 제품 커밋이 0건인 상태가 감지될 때 (REQ-CONST-005 무효 라운드 정의 적용),
**THE SYSTEM SHALL** 다음 액션을 즉시 실행해야 한다:
1. CC가 사용자에게 "사망 나선 의심" 알림 (이슈 + AskUserQuestion 트리거 요청)
2. 다음 DISPATCH 발행 정지 (사용자 판단 대기)
3. 마지막 5 라운드 git log 요약 보고서 생성 (메타 키워드 분포 분석)

**근거**: S15~S16-R1 사망 나선 (3 Sprint, 실질 커밋 0건). 5 라운드 임계는 Sprint 구조와 일치(통상 1 Sprint = 4~6 라운드).

흡수: SPEC-CONSTITUTION-001 REQ-CONST-005 (헌법 조항)의 운영 게이트.

---

### [REQ-AUTOGATE-006] Safety-Critical 90% 커버리지 게이트

**유형**: State-Driven

**WHILE** Safety-Critical 모듈(Dose, Incident, Security, Update) 변경이 포함된 PR이 머지 대기 중일 때,
**THE SYSTEM SHALL** 해당 모듈의 라인 커버리지가 90% 이상임을 QA 보고서로 검증해야 한다.

**미달 시**: PR 블록 + 사용자 알림 + `priority-critical` 레이블 이슈 자동 생성.

**근거**: quality-standards.md §2 SSOT. IEC 62304 Class B 의료 영상 시스템 요구사항.

---

### [REQ-AUTOGATE-007] Safety-Adjacent 모듈 RA 검토 트리거

**유형**: Event-Driven

**WHEN** Safety-Adjacent 모듈(Imaging, Workflow) 변경이 포함된 PR이 생성될 때,
**THE SYSTEM SHALL** RA 팀에 해당 PR 리뷰 요청을 자동 통보해야 한다 (CODEOWNERS 또는 이슈 코멘트).

**근거**: quality-standards.md §2 (Safety-Adjacent 정의). 진단 해석/환자 안전 시퀀스 영향 가능성.

---

### [REQ-AUTOGATE-008] QA 보고서 파일 저장 의무

**유형**: Event-Driven

**WHEN** QA 에이전트가 분석/교차 리뷰/커버리지 측정/변이 테스트를 완료할 때,
**THE SYSTEM SHALL** `TestReports/{REPORT-TYPE}_{YYYY-MM-DD}.md` 형식으로 보고서 파일을 저장해야 한다.

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-006 흡수. V-003 사고 — QA 교차 리뷰가 콘솔 출력만으로 존재 → 추적 불가.

---

### [REQ-AUTOGATE-009] DISPATCH 완료 시 커밋 의무

**유형**: Event-Driven

**WHEN** 팀이 DISPATCH Task 상태를 COMPLETED로 변경할 때,
**THE SYSTEM SHALL** 해당 팀 브랜치(`team/{team}`)에 최소 1개의 git commit이 포함되어 있고, 해당 커밋이 origin에 push된 상태임을 확인한 후에만 Status 변경을 허용해야 한다.

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-001 흡수. V-001 사고 — 5개 팀 dispatch가 미커밋 상태로 COMPLETE 보고됨 → 작업 소실 위험.

---

## 운영 흐름 (Operational Flow)

```
팀: 작업 수행
    ↓
팀: REQ-AUTOGATE-003 7항목 자가검증
    ↓ (7번 항목 No이면 차단)
팀: git commit + git push (REQ-AUTOGATE-009)
    ↓
팀: DISPATCH Status 업데이트 (REQ-AUTOGATE-001 증거 + REQ-AUTOGATE-002 빌드 범위)
    ↓
CC: COMPLETED 감지 → PR 생성
    ↓
QA: PR 재검증 (REQ-AUTOGATE-004)
    ↓ (실패 시 IN_PROGRESS 회귀)
QA: TestReports/ 보고서 저장 (REQ-AUTOGATE-008)
    ↓
사용자: PR 머지
    ↓
CC: 라운드 종료 후 사망 나선 감지 (REQ-AUTOGATE-005)
```

---

## TRUST 5 매핑

- **Tested**: acceptance.md 시나리오에서 7항목 체크리스트 자동 검증 + 사망 나선 감지 회귀 테스트.
- **Readable**: 9개 요구사항을 운영 흐름도로 시각화. 각 Gate가 단일 책임.
- **Unified**: quality-standards.md §3 Self-Verification 6항목과의 호환성 유지(7번 항목 신규 추가).
- **Secured**: REQ-AUTOGATE-006/007/008로 Safety-Critical/Adjacent 보호 + QA 추적성 확보.
- **Trackable**: 사고 케이스 3건(V-001/V-003 GOVERNANCE 흡수, S14-R2, S15~S16-R1) 인용.

---

## 사고 케이스 인용

- **S14-R2**: REQ-AUTOGATE-001, REQ-AUTOGATE-002 — 모듈 빌드만으로 COMPLETED → 회귀 미감지
- **S15~S16-R1**: REQ-AUTOGATE-003 #7, REQ-AUTOGATE-005 — 사망 나선 핵심 메커니즘 차단
- **V-001 (GOVERNANCE-001)**: REQ-AUTOGATE-009 — 미커밋 COMPLETE 차단
- **V-003 (GOVERNANCE-001)**: REQ-AUTOGATE-008 — QA 보고서 파일 저장
- **S07-R4**: REQ-AUTOGATE-004 — QA 독립성 강화 (CC가 빌드/테스트 직접 실행 차단은 헌법 SPEC에서)

---

## SPEC 참조

- SPEC-CONSTITUTION-001 REQ-CONST-005 (사망 나선 헌법 조항) → 본 SPEC REQ-AUTOGATE-005에서 운영화
- SPEC-DISPATCH-V2-001 REQ-DISPATCH-V2-004 (Status 의무 업데이트) → REQ-AUTOGATE-001 증거 기재 의존
- SPEC-DISPATCH-V2-001 REQ-DISPATCH-V2-009 (DISPATCH 권한 분리) → REQ-AUTOGATE-001은 Status 섹션만 기재

---

## 제외 범위 (What NOT to Build)

- 자동 빌드 트리거 도구 (CI 시스템 책임)
- 사망 나선 감지 자동화 스크립트 작성 (CC가 라운드 종료 시 수동 실행)
- QA 재검증 자동화 도구 (별도 SPEC)
- Safety-Critical 모듈의 실제 커버리지 보강 (SPEC-TEAMB-COV-001 등 별도 SPEC)
- 새 evaluator 에이전트 도입

---

## 참고

- `.claude/rules/teams/quality-standards.md` v1.3.0 (SSOT)
- `.claude/rules/teams/cc.md` PR Evaluation Protocol
- `SPEC-CONSTITUTION-001` (헌법 전제)
- `SPEC-DISPATCH-V2-001` (프로토콜 전제)
- `SPEC-METHODOLOGY-001` (parent)
