# DISPATCH Protocol [ALL TEAMS]

DISPATCH 파일 생성, 해석, 상태 관리, 이슈 추적에 대한 통합 규칙.
이 문서는 `team-common.md`에서 분리되었다 (2026-04-22 재정비).

### CC(DISPATCH 작성자) 참고

CC가 DISPATCH 파일을 생성하여 main에 push. 팀은 기존과 동일하게
git pull로 DISPATCH를 감지하여 작업 수행. DISPATCH 생성 권한은 CC에 위임.

---

## 1. DISPATCH Resolution [HARD — FIRST ACTION]

세션 시작 시 가장 먼저 실행. 다른 어떤 작업보다 우선.

```
Step 0: git pull origin main                               ← 구버전 오독 방지
Step 1: Read .moai/dispatches/active/_CURRENT.md           ← 인덱스 파일
Step 2: 자기 팀 행(row) 조회 → DISPATCH 파일명 확인
Step 3: 해당 DISPATCH 파일만 읽기 (다른 active/ 파일 무시)
Step 4: _CURRENT.md의 '팀 모니터링 설정' 테이블에서 ScheduleWakeup 값 획득
Step 5: 상태에 따라 분기
  - ACTIVE → 대기 없이 즉시 NOT_STARTED→IN_PROGRESS + 작업 시작
  - IDLE/MERGED/누락 → IDLE 보고 + ScheduleWakeup(읽은 값)
```

### HARD 규칙
- [HARD] Step 0(`git pull`) 생략 금지 — 미실행 시 구버전 DISPATCH 오독으로 IDLE 오보고 발생
- [HARD] `_CURRENT.md`에 자기 팀 행이 없으면 IDLE (임의로 다른 DISPATCH 파일 탐색 금지)
- [HARD] 루트의 `DISPATCH-*-2026-04-*.md` 파일은 아카이브 — 절대 읽지 않는다
- [HARD] DISPATCH 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
- [HARD] ScheduleWakeup 값은 하드코딩 금지 — 반드시 `_CURRENT.md`에서 읽기

### Phase 전환 시 강제 main 동기화 [S14-R2 교훈]

QUEUED → ACTIVE 전환 감지 시 반드시 실행:

```bash
# Step 1: 미커밋 작업 보호 (데이터 손실 방지)
if [ -n "$(git status --porcelain)" ]; then
    git stash push -m "phase-transition-backup-$(date +%Y%m%d%H%M%S)"
fi

# Step 2: 미푸시 커밋 보호
if [ -n "$(git log origin/team/{team}..HEAD --oneline)" ]; then
    echo "WARNING: Unpushed commits detected. Pushing before reset."
    git push origin team/{team}
fi

# Step 3: main 동기화
git fetch origin main
git reset --hard origin/main   # merge 대신 reset (merge commit 누적 방지)

# 이후에만 DISPATCH 읽기 + 작업 시작
```

- [HARD] `git reset --hard` 전에 반드시 stash로 미커밋 작업 보호
- [HARD] stash된 작업은 DISPATCH 작업 완료 후 `git stash pop`으로 복구 가능 (충돌 시 수동 해결)

### IDLE 보고 형식

```
[TIMESTAMP] 2026-04-22T14:30:00+09:00
State: IDLE
Reason: No active DISPATCH for this team
Last completed: [마지막 완료 작업 요약]
Awaiting: New DISPATCH
```

- [HARD] 첫 줄에 `[TIMESTAMP] ISO-8601+09:00` (KST) 필수

---

## 2. DISPATCH Status Update [HARD]

팀이 자체 DISPATCH Status를 업데이트한다.

### 상태 전환 규칙

| 전환 | 시점 | 필수 액션 |
|------|------|----------|
| `NOT_STARTED → IN_PROGRESS` | DISPATCH 읽기 직후 | Status 업데이트 + 타임스탬프 + push |
| `IN_PROGRESS → COMPLETED` | 작업 완료 + 자가검증 통과 | Status 업데이트 + 타임스탬프 + 빌드 증거 + push |
| `NOT_STARTED → BLOCKED` | 환경/의존성 문제 발생 | Status 업데이트 + 타임스탬프 + 사유 + push |
| `NOT_STARTED → TIMEOUT` | 사용자가 60분 미응답 후 선언 | 사용자만 업데이트 |

### Status 테이블 표준 형식

```
| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | ... | IN_PROGRESS | Team A | P1 | 2026-04-22T20:00:00+09:00 | ... |
| T2 | ... | COMPLETED | Team A | P2 | 2026-04-22T20:30:00+09:00 | 빌드 증거 |
```

- [HARD] 타임스탬프 포맷: `YYYY-MM-DDTHH:MM:SS+09:00`
- [HARD] 타임스탬프 없는 Status 업데이트 = 프로토콜 위반
- [HARD] Status 업데이트 없이 대기 = 소통 단절 = 프로토콜 위반

---

## 3. DISPATCH File Management [HARD]

DISPATCH 파일 이동·삭제는 **사용자 직접 관리**.

- [HARD] 팀은 `active/`, `completed/`, `_CURRENT.md` 파일을 생성·이동·삭제 금지
- [HARD] 팀은 자체 DISPATCH 파일 **내부**의 Status 섹션만 수정
- [HARD] active/↔completed/ 이동은 사용자가 머지 완료 후 수행
- [HARD] MERGED DISPATCH가 active/에 잔존하면 팀이 반복 IDLE 보고 유발 (S07-R3 교훈)

### DISPATCH 파일 수정 구분 [HARD — 충돌 방지]

DISPATCH 파일은 CC와 팀이 각각 다른 섹션을 수정하여 충돌을 방지한다:

| 섹션 | 수정 권한 | 이유 |
|------|----------|------|
| 헤더 (Sprint, Team, Priority, SPEC) | **CC만** | CC가 생성 시 확정 |
| 배경 (Background) | **CC만** | CC가 컨텍스트 작성 |
| Tasks (체크리스트, 완료 조건) | **CC만** | CC가 기획 |
| **Status 테이블** | **팀만** | 팀이 진행 상태 업데이트 |
| Constraints | **CC만** | CC가 설정 |
| Evidence Required | **CC만** | CC가 설정 |
| 참고 문서 | **CC만** | CC가 설정 |

- [HARD] 팀이 Status 이외의 섹션을 수정하면 PR merge 시 충돌 발생 → 수정 금지
- [HARD] CC가 DISPATCH 내용 수정 시 team 브랜치와 충돌 가능 → 수정은 신규 라운드에서만

---

## 4. Git Completion Protocol [HARD]

DISPATCH 작업 완료 후:

```
1. DISPATCH Status 테이블 → COMPLETED + 타임스탬프 + 빌드 증거 기입
2. git add (소스코드 + DISPATCH 파일 변경)
3. git commit -m "conventional(team): ... (#이슈번호)"
4. git push origin team/{team-name}    ← 소스코드 + DISPATCH Status 함께 push
5. ScheduleWakeup(값) 재설정   ← 다음 DISPATCH 수신 채널
6. PR 생성 금지 — 사용자 또는 CC가 관리
```

### DISPATCH Status push 흐름 [HARD]

```
CC: main에 DISPATCH 파일 생성 (NOT_STARTED 상태)
         ↓
팀: git pull origin main → DISPATCH 파일 수신
         ↓
팀: 작업 수행 → DISPATCH Status를 IN_PROGRESS/COMPLETED로 업데이트
         ↓
팀: git commit + git push origin team/{team}  ← 소스코드 + DISPATCH Status 함께
         ↓
CC: git show origin/team/{team}:.moai/dispatches/active/DISPATCH-*.md → Status 읽기
         ↓
CC: COMPLETED 감지 → PR 생성 (team/{team} → main)
         ↓
사용자: PR 승인 → 머지 → DISPATCH Status가 main에 반영
```

Push 실패 시: Status에 `PUSH_FAILED` 표기 + 다시 push 시도.

---

## 5. Issue Tracking Protocol [HARD — S05-R2 시행]

### Pre-Work Issue 등록

- [HARD] DISPATCH 작업 시작 **전**에 Gitea 이슈 생성
- [HARD] 이슈 없이 작업 = 프로토콜 위반
- [HARD] 모든 이슈는 팀 라벨(`team-a|team-b|team-design|coordinator|qa|ra`) + 우선순위 라벨(`priority-critical|high|medium`) 필수

### Issue Lifecycle

```
DISPATCH 읽기 → 이슈 생성 → DISPATCH Status에 이슈 번호 기록 →
작업 → 커밋 메시지 `(#N)` 참조 → 완료 코멘트 작성 → 이슈 Close
```

### Korean-Safe Issue Creation [CRITICAL]

- [HARD] bash `curl` 인라인 한글 금지 — U+FFFD 깨짐 발생
- [HARD] 한글 포함 시 반드시 아래 중 하나 사용:
  1. `bash scripts/issue/gitea-api.sh issue-create "TITLE" "BODY" "labels"`
  2. `pwsh scripts/issue/New-GiteaIssue.ps1 -Title ... -Body ... -Labels @(ID)`
  3. git 커밋 메시지는 한글 OK (git은 UTF-8 native)

---

## 6. Phase Dependency Protocol [S17+ 도입]

### Phase 구조

| Phase | 팀 | ScheduleWakeup |
|-------|-----|---------------|
| Phase 1 | Team A, Team B | 기준값 |
| Phase 2 | Coordinator, Design | 기준값 + 60초 |
| Phase 3 | QA | 기준값 + 120초 |
| Phase 4 | RA | 기준값 + 180초 |
| 상시 | CC | 600초 |

### Phase 실패 시 행동 규칙

| 상황 | 후속 Phase 팀 행동 |
|------|-------------------|
| Phase 1 팀 COMPLETED | Phase 2 정상 시작 (main pull 후) |
| Phase 1 팀 BLOCKED | 후속 Phase 팀은 **IDLE 대기** — 사용자 판단까지 대기 |
| Phase 1 팀 TIMEOUT | 후속 Phase 팀은 **직전 라운드 기준으로 작업** — TIMEOUT 팀의 변경 미반영 |
| Phase 1 팀 PARTIAL | 후속 Phase 팀은 **완료된 Task만 대상으로 작업** |

- [HARD] Phase 1이 전원 COMPLETED가 아니면 Phase 2는 main pull 후 **변경 사항 확인** 후 작업 시작
- [HARD] Phase 실패 시 후속 팀이 자의적으로 Phase 1 작업 대체 금지 (role-matrix 위반)

### QA (Phase 3) 시작 조건 [HARD]

QA는 Phase 1+2 완료 상태에 따라 검증 범위를 결정:

| Phase 1+2 상태 | QA 행동 |
|----------------|---------|
| 전원 COMPLETED | 전체 모듈 빌드/테스트/커버리지 측정 |
| Phase 1 COMPLETED, Phase 2 PARTIAL | Phase 1 모듈만 검증 + Phase 2 완료 모듈만 추가 검증. PARTIAL 모듈은 제외 |
| Phase 1 COMPLETED, Phase 2 BLOCKED | Phase 1 모듈만 검증. Phase 2 BLOCKED는 이슈 보고 |
| Phase 1 PARTIAL | 완료된 Phase 1 모듈만 검증. 나머지는 PARTIAL로 보고 |

- [HARD] QA는 검증 불가한 모듈을 임의로 PASS 처리 금지 — 검증 안 된 모듈은 "UNVERIFIED"로 명시
- [HARD] QA DISPATCH의 최종 판정은 **검증 완료된 모듈만 근거**로 판정

---

## 7. Team Obligations [HARD]

- DISPATCH Resolution 준수 (§1)
- Acceptance Criteria 검증
- 파일 소유권 범위 준수 (role-matrix.md §2)
- BLOCKED는 정직하게 보고 (추측 금지)
- IDLE은 정직하게 보고 (가공 업무 금지)
- PR 생성 금지 — 사용자가 직접 관리

---

Version: 2.5.0 (Phase 전환 미커밋 작업 보호, DISPATCH 수정 구분 명시)
Effective: 2026-04-22
Cross-ref: `session-lifecycle.md`, `quality-standards.md`, `role-matrix.md`, `cc.md`, `_CURRENT.md`
