---
id: SPEC-DISPATCH-V2-001
version: 1.0.0
status: draft
priority: P0
team: meta
title: DISPATCH 프로토콜 v2 — Phase Dependency 강제, Stall/TIMEOUT 해석, Worktree·이슈 정합성
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
parent: SPEC-METHODOLOGY-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. SPEC-GOVERNANCE-001의 REQ-GOV-003, REQ-GOV-004, REQ-GOV-005, REQ-GOV-007 흡수 |

---

## 개요 (Overview)

DISPATCH 프로토콜의 v2를 정의한다. 기존 dispatch-protocol.md v2.5.0의 주요 규칙을 헌법 수준으로 격상하고, S09-R3(QA Stall), S14-R2(Phase 2 main 동기화 누락), S15-R2(Design 무한 대기) 사고를 직접 차단하는 규칙을 추가한다.

본 SPEC은 SPEC-CONSTITUTION-001을 전제로 하며, 자체 헌법 조항을 추가하지 않고 운영 프로토콜만 정의한다.

---

## 요구사항 (EARS 형식)

### [REQ-DISPATCH-V2-001] DISPATCH Resolution FIRST ACTION 강제

**유형**: Event-Driven

**WHEN** 팀 Claude 세션이 시작되거나 ScheduleWakeup으로 깨어날 때,
**THE SYSTEM SHALL** 다른 어떤 작업보다 먼저 다음 순서를 실행해야 한다:
1. `git pull origin main`
2. `.moai/dispatches/active/_CURRENT.md` 읽기
3. 자기 팀 행 조회 → DISPATCH 파일명 확인
4. 해당 DISPATCH 파일만 읽기

**근거**: dispatch-protocol.md §1 [HARD — FIRST ACTION] 규칙 격상. Step 0 누락 시 구버전 DISPATCH 오독으로 IDLE 오보고 발생.

---

### [REQ-DISPATCH-V2-002] Phase 전환 시 main 강제 동기화 + stash 보호

**유형**: Event-Driven

**WHEN** 팀이 QUEUED → ACTIVE 전환을 감지할 때,
**THE SYSTEM SHALL** 다음 순서를 실행해야 한다:
1. 미커밋 작업 존재 시 `git stash push -m "phase-transition-backup-{TS}"` 실행
2. 미푸시 커밋 존재 시 `git push origin team/{team}` 실행
3. `git fetch origin main && git reset --hard origin/main` 실행
4. 이후에만 DISPATCH 작업 시작

**근거**: S14-R2 사고 — Coordinator가 Phase 1 Team A 머지 이전 base에서 분기하여 Trait 87건 누락. dispatch-protocol.md §1 "Phase 전환 시 강제 main 동기화" 규칙 격상.

---

### [REQ-DISPATCH-V2-003] Phase Dependency 강제 게이트

**유형**: State-Driven

**WHILE** Phase 1 팀(Team A, Team B) 중 1팀 이상이 BLOCKED 또는 TIMEOUT 상태인 동안,
**THE SYSTEM SHALL** Phase 2~4 후속 팀(Coordinator, Design, QA, RA)이 자동으로 IDLE 대기 상태에 머물도록 강제해야 한다.

**상세 규칙** (dispatch-protocol.md §6 격상):

| Phase 1 상태 | 후속 Phase 행동 |
|--------------|-----------------|
| 전원 COMPLETED | Phase 2 정상 시작 (main pull 후) |
| 1팀 BLOCKED | 후속 Phase 팀 IDLE 대기 — 사용자 판단까지 |
| 1팀 TIMEOUT | 후속 Phase 팀 직전 라운드 base에서 작업 — TIMEOUT 팀 변경 미반영 |
| 1팀 PARTIAL | 후속 Phase 팀은 완료된 Task만 대상으로 작업 |

**근거**: S14-R2 + S15-R2 통합 교훈. Phase 1 미완료 시 후속 Phase가 진행되면 base 불일치/소유권 침범 발생.

흡수: SPEC-GOVERNANCE-001 REQ-GOV-003 (Coordinator 통합 게이트).

---

### [REQ-DISPATCH-V2-004] DISPATCH Status 의무 업데이트

**유형**: Event-Driven

**WHEN** 팀이 DISPATCH를 읽거나 작업 상태가 변경될 때,
**THE SYSTEM SHALL** DISPATCH 파일의 Status 테이블을 다음 시점에 즉시 업데이트해야 한다:
- DISPATCH 읽기 직후: NOT_STARTED → IN_PROGRESS (타임스탬프 + push)
- 작업 완료 + 자가검증 통과 시: IN_PROGRESS → COMPLETED (타임스탬프 + 빌드 증거 + push)
- 환경/의존성 문제 시: NOT_STARTED → BLOCKED (타임스탬프 + 사유 + push)

**타임스탬프 포맷**: `YYYY-MM-DDTHH:MM:SS+09:00` (KST)

**근거**: dispatch-protocol.md §2. S09-R3 사고 — QA가 작업 불가 상태를 BLOCKED로 업데이트하지 않고 12회 연속 NOT_STARTED 방치 → 12회 IDLE 대기 반복.

---

### [REQ-DISPATCH-V2-005] Stall Detection (3회/5회 임계)

**유형**: Event-Driven

**WHEN** 특정 팀이 N회 연속 NOT_STARTED 상태로 폴링되는 경우,
**THE SYSTEM SHALL** 다음 액션을 수행해야 한다:
- 3회 연속 → CC가 Gitea 이슈에 "Stall 1차 경고: 작업 지연 의심" 코멘트
- 5회 연속 → CC가 사용자에게 조치 요청 (이슈 코멘트 + `priority-high` 레이블)

**금지**: CC가 임의로 팀 Status를 변경하지 않는다 (S09-R3 교훈).

**근거**: dispatch-protocol.md §5 격상. 임의 Status 변경은 상태 왜곡 → 진실 신호 손실.

---

### [REQ-DISPATCH-V2-006] TIMEOUT Protocol (60분 + 연속 3회)

**유형**: Event-Driven

**WHEN** DISPATCH 발행 후 60분이 경과해도 해당 팀이 NOT_STARTED 상태인 경우,
**THE SYSTEM SHALL** 다음 절차를 수행해야 한다:
1. CC가 사용자에게 TIMEOUT 선언 권한 위임 (사용자만 TIMEOUT 마킹 가능)
2. 사용자 승인 후 `_CURRENT.md`에 해당 팀 IDLE + DISPATCH 파일에 TIMEOUT 기록
3. 다음 라운드 발행 가능 (TIMEOUT 팀은 정상 포함, 페널티 없음)
4. **연속 3회 TIMEOUT** 발생 시 사용자에게 워크트리/에이전트 구성 점검 요청

**근거**: S15-R2 사고 — Design 무한 대기로 전체 라운드 진행 불가. dispatch-protocol.md §6 (session-lifecycle.md §6) 통합.

---

### [REQ-DISPATCH-V2-007] Issue-First DISPATCH 발행

**유형**: Event-Driven

**WHEN** CC가 새 DISPATCH를 팀에 발행할 때,
**THE SYSTEM SHALL** 다음 순서를 강제해야 한다:
1. Gitea 이슈 생성 (`team-{team}` + 우선순위 레이블 필수)
2. DISPATCH 파일에 Issue # 기록
3. DISPATCH 파일을 main에 push
4. 발행 후 이슈에 "DISPATCH 발행 완료" 코멘트

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-004 흡수. 이슈 없는 DISPATCH = 추적 불가.

---

### [REQ-DISPATCH-V2-008] 한글 안전 이슈 작성

**유형**: Unwanted

**THE SYSTEM SHALL NOT** bash `curl` 인라인으로 한글 포함 이슈 본문을 전송하지 않는다 (U+FFFD 깨짐 발생).

**대신** 다음 중 하나를 사용해야 한다:
- `bash scripts/issue/gitea-api.sh issue-create "TITLE" "BODY" "labels"`
- `pwsh scripts/issue/New-GiteaIssue.ps1 -Title ... -Body ... -Labels @(ID)`

**근거**: dispatch-protocol.md §5 [CRITICAL] 격상. SPEC-GOVERNANCE-001 REQ-GOV-005 흡수 (한글 인코딩 정합성).

---

### [REQ-DISPATCH-V2-009] DISPATCH 파일 수정 권한 분리

**유형**: Ubiquitous

**THE SYSTEM SHALL** DISPATCH 파일 내 섹션별 수정 권한을 다음과 같이 분리해야 한다:

| 섹션 | 수정 권한 |
|------|----------|
| 헤더, 배경, Tasks, Constraints, Evidence Required, 참고 문서 | **CC만** |
| **Status 테이블** | **팀만** |

**근거**: dispatch-protocol.md §3 [HARD — 충돌 방지]. PR merge 시 충돌 차단.

---

### [REQ-DISPATCH-V2-010] DISPATCH 파일 이동은 사용자만

**유형**: Unwanted

**THE SYSTEM SHALL NOT** 팀 또는 CC가 `.moai/dispatches/active/` ↔ `.moai/dispatches/completed/` 간 파일 이동을 수행하지 않는다.

**예외**: 사용자가 직접 수행.

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-007 흡수. S07-R3 교훈 — MERGED DISPATCH가 active/에 잔존하면 팀이 반복 IDLE 보고 유발.

---

### [REQ-DISPATCH-V2-011] DISPATCH Status push는 team 브랜치로

**유형**: Event-Driven

**WHEN** 팀이 DISPATCH Status를 업데이트할 때,
**THE SYSTEM SHALL** 다음 흐름을 따라야 한다:
1. 팀이 작업 수행 + Status 업데이트 + `git commit` + `git push origin team/{team}`
2. CC는 `git show origin/team/{team}:.moai/dispatches/active/DISPATCH-*.md`로 Status 읽기
3. CC는 main의 DISPATCH 파일을 직접 읽지 않음 (NOT_STARTED 잔존 상태이므로)

**근거**: dispatch-protocol.md §4 [HARD] + cc.md "Team Branch Status Read Protocol" 격상.

---

### [REQ-DISPATCH-V2-012] Worktree 7팀 매핑 유지

**유형**: Ubiquitous

**THE SYSTEM SHALL** 다음 7팀 worktree 매핑을 변경 없이 유지해야 한다:

| 팀 | 워크트리 브랜치 |
|----|----------------|
| CC | `team/cc` |
| Team A | `team/team-a` |
| Team B | `team/team-b` |
| Coordinator | `team/coordinator` |
| Design | `team/team-design` |
| QA | `team/qa` |
| RA | `team/ra` |

**근거**: SPEC-METHODOLOGY-001 NG-2. 본 SPEC군은 운영 정합성 강화이며 구조 변경이 아님.

---

## TRUST 5 매핑

- **Tested**: acceptance.md의 Given/When/Then 시나리오 + Stall Detection 자동화 검증.
- **Readable**: 12개 요구사항을 단일 운영 사이클(폴링→Phase→Status→TIMEOUT→파일관리)로 그룹핑.
- **Unified**: dispatch-protocol.md v2.5.0과 1:1 매칭. 본 SPEC은 SPEC 형태로 격상한 것.
- **Secured**: REQ-DISPATCH-V2-008(한글 안전), REQ-DISPATCH-V2-007(이슈 추적)으로 보안 추적성 확보.
- **Trackable**: 사고 케이스 4건(S07-R3, S09-R3, S14-R2, S15-R2) 인용 + GOVERNANCE-001 4개 요구사항 흡수.

---

## 사고 케이스 인용

- **S07-R3**: REQ-DISPATCH-V2-010 — DISPATCH 파일 이동 차단
- **S09-R3**: REQ-DISPATCH-V2-004, REQ-DISPATCH-V2-005 — QA Stall 12회 + 임의 Status 변경 차단
- **S14-R2**: REQ-DISPATCH-V2-002, REQ-DISPATCH-V2-003 — Phase 2 main 동기화 + Phase Dependency
- **S15-R2**: REQ-DISPATCH-V2-006 — TIMEOUT 60분 도입

---

## SPEC-CONSTITUTION-001 참조

- REQ-CONST-001 (7팀 안정 보존) → REQ-DISPATCH-V2-012 (Worktree 매핑 유지)
- REQ-CONST-002 #4 (DISPATCH 파일 이동 금지) → REQ-DISPATCH-V2-010

---

## 제외 범위 (What NOT to Build)

- Stall Detection 자동화 도구 작성 (오케스트레이터/CC 운영 책임)
- TIMEOUT 자동 트리거 (사용자 명시 승인 필요)
- 새 워크트리 추가/제거
- DISPATCH 파일 이동 자동화 (사용자만)
- gitea-api.sh / New-GiteaIssue.ps1 자체 수정 (별도 SPEC)

---

## 참고

- `.claude/rules/teams/dispatch-protocol.md` v2.5.0
- `.claude/rules/teams/session-lifecycle.md` v1.2.0
- `.claude/rules/teams/cc.md` Operating Cycle 섹션
- `SPEC-CONSTITUTION-001` (parent reference)
- `SPEC-METHODOLOGY-001` (parent)
