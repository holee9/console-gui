---
id: SPEC-DISPATCH-V2-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
parent: SPEC-METHODOLOGY-001
---

## Definition of Done

- [ ] 12개 요구사항(REQ-DISPATCH-V2-001~012) 모두 EARS 형식
- [ ] SPEC-GOVERNANCE-001 REQ-GOV-003, 004, 005, 007 4건 흡수 매핑 명시
- [ ] 사고 케이스 4건(S07-R3, S09-R3, S14-R2, S15-R2) 본문 인용
- [ ] Phase Dependency 표 (Phase 1 상태별 후속 Phase 행동) 작성
- [ ] DISPATCH Status 권한 분리 표 작성 (REQ-DISPATCH-V2-009)
- [ ] Worktree 7팀 매핑 표 (REQ-DISPATCH-V2-012) 작성

---

## 시나리오 1: Phase 전환 시 미커밋 작업 stash 보호

**Given** Coordinator가 Phase 1 Team A 머지 직후 Phase 2 ACTIVE 전환을 감지하고, 미커밋 변경이 존재하는 상태에서
**When** REQ-DISPATCH-V2-002 절차가 실행되면
**Then** `git stash list`에 `phase-transition-backup-{TS}` 엔트리가 생성되고, `git reset --hard origin/main` 후 base가 최신 main과 일치해야 한다.

검증 방법:
- `git stash list | grep phase-transition-backup` ≥ 1 (미커밋 작업 보호 흔적)
- `git rev-parse HEAD` == `git rev-parse origin/main`

---

## 시나리오 2: Phase 1 BLOCKED 시 후속 Phase IDLE 대기

**Given** Team A가 BLOCKED 상태로 Status 업데이트한 상태에서
**When** Coordinator가 ScheduleWakeup으로 깨어나 _CURRENT.md를 읽으면
**Then** Coordinator는 ACTIVE로 전환되지 않고 IDLE 보고 + 사용자 판단 대기 상태에 머물러야 한다.

검증 방법:
- Coordinator team 브랜치의 DISPATCH Status: NOT_STARTED 유지
- IDLE 보고 형식 준수 (session-lifecycle.md §2 IDLE 보고 형식)

---

## 시나리오 3: QA 12회 Stall → 사용자 알림

**Given** QA 팀이 12회 연속 NOT_STARTED 상태로 폴링되는 상황에서 (S09-R3 재현)
**When** REQ-DISPATCH-V2-005 Stall Detection이 동작하면
**Then**:
- 3회차에 CC가 이슈에 "Stall 1차 경고" 코멘트 작성
- 5회차에 CC가 사용자에게 조치 요청 (priority-high 레이블)
- 12회차 시점까지 CC가 임의로 QA Status를 BLOCKED로 변경하지 않음

검증 방법:
- Gitea 이슈 코멘트 이력 확인
- QA DISPATCH Status는 사용자 또는 QA 본인에 의해서만 변경됨

---

## 시나리오 4: Design 무한 대기 → TIMEOUT

**Given** Design 팀이 DISPATCH 발행 후 60분 경과해도 NOT_STARTED인 상태에서 (S15-R2 재현)
**When** REQ-DISPATCH-V2-006이 발동하면
**Then**:
- CC가 사용자에게 TIMEOUT 선언 권한 위임 메시지 송부
- 사용자 승인 후 _CURRENT.md에 Design IDLE 마킹
- 다음 라운드 발행 가능 (Design은 페널티 없이 정상 포함)

검증 방법:
- TIMEOUT 마킹 시점에 사용자 승인 흔적 (이슈 코멘트 또는 명시적 메시지)
- 연속 3회 TIMEOUT 발생 시 워크트리 점검 요청 알림 발화

---

## 시나리오 5: 한글 안전 이슈 작성

**Given** CC가 한글 본문이 포함된 Gitea 이슈를 생성하려는 상황에서
**When** CC가 bash `curl` 인라인이 아닌 `gitea-api.sh issue-create` 또는 `New-GiteaIssue.ps1`을 사용하면
**Then** 이슈 본문에 U+FFFD(�) 깨짐 문자가 0건이어야 한다.

검증 방법:
- Gitea 이슈 API 응답 본문 → `grep -c $'�'` == 0
- 위반 시 BLOCKED, 이슈 재작성 필요

---

## 시나리오 6: DISPATCH Status 권한 분리

**Given** 팀이 DISPATCH 파일의 Tasks 섹션을 임의 수정하려는 상황에서
**When** REQ-DISPATCH-V2-009 권한 게이트가 동작하면
**Then** Tasks 섹션 변경은 거부되고, Status 테이블 변경만 허용되어야 한다.

검증 방법:
- PR diff에서 DISPATCH 파일의 Status 외 섹션 변경이 발견되면 PR 블록
- 위반 시 BLOCKED + 팀에게 dispatch-protocol.md §3 안내

---

## 시나리오 7: DISPATCH 파일 이동 차단

**Given** CC가 자동으로 active/ → completed/ 이동을 시도하는 상황에서
**When** REQ-DISPATCH-V2-010 게이트가 동작하면
**Then** 파일 이동이 거부되어야 하며, 사용자에게 "DISPATCH 파일 이동은 사용자 단독 권한" 메시지를 반환한다.

검증 방법:
- `git log --diff-filter=R -- '.moai/dispatches/active/'` — Author가 사용자가 아니면 BLOCKED.

---

## 엣지 케이스

- **stash 충돌**: REQ-DISPATCH-V2-002 stash pop 시 충돌 발생 → 수동 해결 안내 (자동 충돌 해결 금지).
- **Phase 1 PARTIAL + Phase 2 ACTIVE**: Phase 2 팀은 완료된 Task만 대상으로 작업. PARTIAL Task는 다음 라운드까지 대기.
- **TIMEOUT 팀이 다음 라운드에서 즉시 응답**: 페널티 없이 정상 라운드로 카운트.

---

## Quality Gate

- 12개 요구사항이 dispatch-protocol.md v2.5.0과 1:1 매핑 ✓
- SPEC-CONSTITUTION-001 헌법 조항 위배 없음 ✓
- 사고 케이스 4건 인용 ✓
- 시간 예측 사용 0회 ✓
