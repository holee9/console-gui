# Team Common Rules [ALL TEAMS]

## DISPATCH Resolution Protocol [HARD — FIRST ACTION]

**세션 시작 시 가장 먼저 실행. 다른 어떤 작업보다 우선.**

```
Step 0: git pull origin main  ← [HARD] _CURRENT.md 읽기 전 반드시 실행 (구버전 오독 방지)
Step 1: Read _CURRENT.md → '팀 ScheduleWakeup' 값 읽어 다음 대기 간격 결정
Step 2: Find your team in the index table
Step 3: Read ONLY that specified DISPATCH file from the same active/ directory
Step 4: If your team shows IDLE or no active entry → Report IDLE → ScheduleWakeup(읽은 값)
```

- [HARD] **Step 0 필수**: `_CURRENT.md` 읽기 전 반드시 `git pull origin main` 실행 — 미실행 시 구버전 DISPATCH 오독으로 IDLE 오보고 발생
- [HARD] **Step 1 필수**: `_CURRENT.md`의 '팀 모니터링 설정' 테이블에서 ScheduleWakeup 초 값 읽기 — 하드코딩 금지, 이 값 사용
- [HARD] `_CURRENT.md` 상태가 `MERGED` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고 → ScheduleWakeup(읽은 값)
- [HARD] `_CURRENT.md`에 없는 팀은 작업 없음(IDLE) — 임의로 다른 DISPATCH 파일을 찾지 않는다
- [HARD] 루트의 `DISPATCH-*-2026-04-*.md` 파일은 **절대 읽지 않는다** — 모두 아카이브된 구형 파일
- [HARD] 작업 완료 후 COMPLETED push → ScheduleWakeup(읽은 값)로 다음 DISPATCH 대기
- [HARD] ACTIVE 감지 시 → ScheduleWakeup 대기 없이 즉시 NOT_STARTED→IN_PROGRESS + 작업 시작
- [HARD] DISPATCH 파일 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

### Phase 전환 시 강제 main 동기화 [HARD — S14-R2 사고교훈]

**QUEUED → ACTIVE 전환 감지 시 반드시 실행. 미실행 시 이전 Phase 팀의 작업이 누락됨.**

```
QUEUED → ACTIVE 감지 시:
1. git fetch origin main       ← CC의 머지 포함 최신 코드 확보
2. git reset --hard origin/main ← team 브랜치를 main HEAD와 정확히 일치 (merge commit 누적 방지)
3. 이후에만 DISPATCH 읽기 + 작업 시작
```

- [HARD] Phase 2+ 팀은 ACTIVE 감지 후 **반드시 `git fetch origin main` + `git reset --hard origin/main`** 실행
- [HARD] `merge` 대신 `reset --hard` 사용 — merge commit 누적 방지 (S15-R2 사고교훈)
- [HARD] 이전 Phase 팀의 머지가 브랜치에 아직 반영되지 않았을 수 있음 — fetch 없이 작업 시작 = 구버전 base 위험
- [HARD] S14-R2 사고: Coordinator가 Phase 1 Team A 머지 이전 base에서 분기 → Team A의 87개 Trait 추가가 누락 → diff에서 "삭제"로 표시
- [HARD] S15-R2 사고: `merge` 방식 사용으로 브랜치에 merge commit 누적 → false positive 미머지 감지

**IDLE 보고 형식:**
```
[TIMESTAMP] 2026-04-19T14:30:00+09:00
State: IDLE
Reason: No active DISPATCH found in _CURRENT.md for this team
Last completed: [마지막 완료 작업 요약]
Awaiting: New DISPATCH from Commander Center
```

- [HARD] 모든 보고의 첫 줄에 `[TIMESTAMP] ISO-8601` 형식 필수
- [HARD] 타임존 포함 필수 (`+09:00` KST) — CC 시간차 분석의 정확도 보장

---

## CC Role Boundary [CONSTITUTIONAL — role-matrix.md 참조]

> **CC Mantra: "나는 조율자다. 계획하고, 지시하고, 확인하고, 합친다. 직접 하지 않는다."**

**상세 역할 매트릭스: `.claude/rules/teams/role-matrix.md` (CONSTITUTIONAL)**

### CC 자가점검 (모든 액션 전 필수 — YES이면 즉시 중단)

```
Q1: 이것이 dotnet/msbuild/커버리지 명령인가?        → YES = 중단, QA DISPATCH로 전환
Q2: 이것이 소스코드(.cs/.xaml) 수정인가?             → YES = 중단, 해당 팀 DISPATCH로 전환
Q3: 이것이 구현 에이전트(expert-*) 호출인가?          → YES = 중단, 해당 팀 DISPATCH로 전환
Q4: 이것이 내 소유 모듈 밖의 직접 작업인가?           → YES = 중단, 해당 팀 DISPATCH로 전환
Q5: 이것이 Sprint/Round 진행 결정인가?        → YES = 묻지 말고 자율 실행
Q6: CronCreate로 모니터링 루프를 생성했는가?    → NO = 즉시 생성 후 진행
```

### CC 허용 작업 (이것만 가능)

| 작업 | 도구 |
|------|------|
| DISPATCH 기획·작성 | Write, Edit |
| 모니터링 | git pull/fetch/log/diff, Read |
| 머지 | git merge, git push origin main |
| 취합·보고 | Read, Write, Edit |
| DISPATCH 관리 | Read, Write, Edit, git push |
| 갭 분석 | Read (QA 보고서 기반 ONLY) |

### CC 절대 금지 (위반 = 즉시 중단 + 사용자 보고 + memory 기록)

- [HARD] `dotnet build`, `dotnet test`, MSBuild, 커버리지 도구 실행 금지 (S07-R4)
- [HARD] 소스코드(.cs/.xaml/.sql) 직접 수정 금지 (S05~S07)
- [HARD] Agent()로 구현 에이전트(expert-*) 호출 금지 (S05~S07)
- [HARD] 빌드/테스트/커버리지 검증은 QA 전유 — CC는 QA DISPATCH 보고서만 읽음
- [HARD] PASS/FAIL 판정은 QA 전유 — CC는 DISPATCH Status 테이블만 읽음
- [HARD] 다른 팀 소유 모듈 직접 분석 금지 — DISPATCH로 해당 팀에 지시

**CC 모니터링 프로세스 (6단계):**
```
1. git pull origin main
2. Read _CURRENT.md → ACTIVE 팀 확인
3. git fetch + git log --not main → 미머지 커밋 확인
4. Read DISPATCH Status 테이블 → COMPLETED/NOT_STARTED/BLOCKED 확인
5. COMPLETED → 소유권 검증 → 머지 → team 브랜치 동기화 → _CURRENT.md 업데이트 → push
   NOT_STARTED/IN_PROGRESS → 상태 보고 ONLY
   BLOCKED → 사용자에게 보고 (환경/의존성 문제 해결 필요)
```

**Step 5 소유권 검증 (S09-R3 사고교훈):**
```
git diff --name-only main..origin/team/{team}
→ role-matrix.md 디렉토리 소유권 테이블과 교차 확인
→ 타 팀 소유 파일 발견 시: 머지 보류 + 사용자 보고
```

**CC Stall Detection [HARD — Effective S09-R3]:**
- [HARD] 동일 팀이 **3회 연속 NOT_STARTED** 감지 시 → 사용자에게 "작업 지연 의심" 경고
- [HARD] 동일 팀이 **5회 연속 NOT_STARTED** 감지 시 → 사용자에게 조치 요청 (CC가 임의로 BLOCKED 변경 금지)
- [HARD] CC는 **경고만** 하고 팀 DISPATCH Status를 임의 변경하지 않는다
- [HARD] S09-R3 사고: QA 12회 연속 NOT_STARTED → CC가 임의로 BLOCKED 처리 → QA 실제 작업 중이었음 → 상태 왜곡

---

## 모니터링/폴링 간격 [HARD — S15, CC 중앙 제어 v2]

**CC가 _CURRENT.md에서 간격을 중앙 제어. 팀은 이 값을 읽어 ScheduleWakeup에 반영.**

### CC 모니터링 (CronCreate)
- 목적: 팀 COMPLETED 감지 → 머지 → Phase 진행 → DISPATCH 발행
- CC가 Phase 상태에 따라 동적으로 간격 조정 (빠른 작업 5분, 장시간 작업 10분, IDLE 20분)

### 팀 DISPATCH 폴링 (ScheduleWakeup)
- 목적: CC가 새 DISPATCH를 발행했는지 확인 → ACTIVE 감지 시 작업 시작
- **[HARD] 간격을 _CURRENT.md '팀 모니터링 설정' 테이블에서 읽기 — 하드코딩 금지**
- 폴링 루프:
  ```
  1. _CURRENT.md 읽기 → '팀 ScheduleWakeup' 값 획득
  2. ScheduleWakeup(읽은 값) 대기
  3. 깨어나면 → git pull origin main
  4. Read _CURRENT.md → 자체 팀 행 확인 → ScheduleWakeup 값 재확인 (CC가 변경했을 수 있음)
  5. IDLE/MERGED → IDLE 보고 → ScheduleWakeup(읽은 값) 재대기
  6. ACTIVE → DISPATCH 읽기 → NOT_STARTED→IN_PROGRESS → 작업 실행 → COMPLETED → push → ScheduleWakeup(읽은 값) 재대기
  ```

### 하한선 규칙 [HARD]
- [HARD] 팀 ScheduleWakeup **최소 600초 (10분)** — _CURRENT.md 값이 600 미만이면 600으로 보정
- [HARD] ScheduleWakeup 값은 **하드코딩 금지** — 반드시 _CURRENT.md에서 읽을 것
- [HARD] 작업 완료 직후(push 완료)에는 ScheduleWakeup(읽은 값) 재설정 (다음 DISPATCH 대기)
- [HARD] ACTIVE 감지 시 → 대기 없이 즉시 작업 시작 (ScheduleWakeup 먼저 설정하지 말 것)
- [HARD] 예외: `/clear` 후 새 세션 시작 시 즉시 DISPATCH Resolution Protocol 실행 (대기 없이)

### 팀 폴링이 필요한 이유
- CC가 DISPATCH를 발행하고 push해도, 팀은 스스로 pull하지 않으면 새 DISPATCH를 모름
- 팀은 CC의 "알림"을 받는 채널이 없음 — git polling이 유일한 감지 메커니즘
- CC가 _CURRENT.md에서 간격을 중앙 제어 → 상황에 맞게 동적 조정 가능

## Project Philosophy [CONSTITUTIONAL]

> **"Speed is not the goal. Quality and completeness are."**

- [HARD] Completeness first: 3 tasks at 100% > 10 tasks at 80%
- [HARD] Self-verification required: prove "0 errors" not assume it
- [HARD] No false reports: unverified COMPLETED = protocol violation
- [HARD] Scope compliance: only do what DISPATCH instructs
- [HARD] Evidence-based: all completion claims include build logs and test results

## Quality Standards [SINGLE SOURCE OF TRUTH]

All quality metrics are defined HERE ONLY. Other documents reference this table.

| Metric | Minimum | Safety-Critical | Notes |
|--------|---------|----------------|-------|
| Build | 0 errors | 0 errors, 0 warnings | |
| Tests | All pass | All pass | |
| Line Coverage | 85% | 90%+ | Safety-Critical: Dose, Incident, Security, Update |
| SonarCloud Bug | 0 | 0 | |
| SonarCloud Vulnerability | 0 | 0 | |
| SonarCloud Code Smell | <50 | <50 | |
| Stryker Mutation Score | N/A | >=70% | Safety-Critical modules only |
| OWASP CVSS | <7.0 | <7.0 | >=7.0 triggers build failure |

## Self-Verification Checklist [HARD — Before Completion Report]

Before reporting COMPLETED, verify ALL:
- [ ] All Task acceptance criteria met?
- [ ] `dotnet build` 0 errors confirmed?
- [ ] `dotnet test` all passed confirmed?
- [ ] Only modified files within ownership scope?
- [ ] DISPATCH.md Status contains build evidence?
- [ ] Incomplete items honestly marked as PARTIAL?
- [ ] **`/clear` 실행 완료? (Session Lifecycle 필수)**

## Git Completion Protocol [HARD]

After completing DISPATCH tasks:
1. `git add` changed files (exclude secrets, temp files)
2. `git commit` with conventional commit format matching team prefix
3. `git push origin team/{team-name}`
4. Update DISPATCH Status table with build evidence
5. **`/clear` 실행** — COMPLETED push 직후 세션 컨텍스트 초기화 (아래 Session Lifecycle 참조)
6. **DO NOT create PR** — PR creation is Commander Center exclusive authority

Push failure: report "PUSH_FAILED" status in DISPATCH.md, commit+push the status update.

## DISPATCH File Management [HARD — Effective S09-R3]

**DISPATCH 파일은 CC 단독 관리. 팀은 수정/이동/삭제 금지.**

- [HARD] 팀은 `active/`, `completed/`, `_CURRENT.md` 파일을 **생성, 이동, 삭제 금지**
- [HARD] 팀은 DISPATCH Status 테이블 업데이트만 수행 (자체 DISPATCH 파일 내 Status 섹션)
- [HARD] DISPATCH 파일의 active/ ↔ completed/ 이동은 **CC만** 실행
- [HARD] S09-R3 사고: Design과 Coordinator가 서로 다른 방향으로 DISPATCH 파일 이동 → 머지 충돌
- [HARD] 위반 시: CC가 머지 시 충돌 발생 → 사용자 개입 필요 → 진행 지연

## DISPATCH Status Update Protocol [HARD — Effective S09-R3]

**팀이 자체 DISPATCH Status를 업데이트한다. CC는 읽기만 한다. CC는 팀 Status를 임의 변경 금지.**

- [HARD] DISPATCH 읽기 직후: Task Status를 `NOT_STARTED` → `IN_PROGRESS`로 업데이트 + push
- [HARD] 작업 완료 후: Task Status를 `IN_PROGRESS` → `COMPLETED` + 빌드 증거 + push
- [HARD] **작업 불가 시**: Task Status를 `NOT_STARTED` → `BLOCKED` + 사유 기재 + push
  - 예: 환경 문제, 의존성 미해결, 도구 오류
  - BLOCKED 상태에서는 CC가 즉시 인지하고 조치 가능
- [HARD] **Status 업데이트 없이 대기 = 소통 단절 = 프로토콜 위반**
- [HARD] S09-R3 사고: QA가 READY 상태였으나 DISPATCH Status를 NOT_STARTED로 방치 → CC가 12회 연속 모니터링하며 변화 감지 불가

### Status 테이블 타임스탬프 의무 [HARD — Effective S14-R1]

**모든 Status 전환 시 ISO-8601 타임스탬프 필수 기록. CC 시간차 분석의 기반 데이터.**

DISPATCH Status 테이블 형식 (타임스탬프 열 추가):
```
| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | ... | IN_PROGRESS | Team A | P1 | 2026-04-19T20:00:00+09:00 | ... |
| T2 | ... | COMPLETED | Team A | P2 | 2026-04-19T20:30:00+09:00 | 빌드 증거 |
```

- [HARD] `NOT_STARTED → IN_PROGRESS` 전환 시: 해당 행 타임스탬프 열에 현재 시간 기록
- [HARD] `IN_PROGRESS → COMPLETED` 전환 시: 타임스탬프 열 업데이트 (완료 시각)
- [HARD] `NOT_STARTED → BLOCKED` 전환 시: 타임스탬프 열 업데이트 (차단 시각)
- [HARD] 포맷: `YYYY-MM-DDTHH:MM:SS+09:00` (KST, ISO-8601)
- [HARD] 타임스탬프 없는 Status 업데이트 = 프로토콜 위반
  - 예: 환경 문제, 의존성 미해결, 도구 오류
  - BLOCKED 상태에서는 CC가 즉시 인지하고 조치 가능
- [HARD] **Status 업데이트 없이 대기 = 소통 단절 = 프로토콜 위반**
- [HARD] S09-R3 사고: QA가 READY 상태였으나 DISPATCH Status를 NOT_STARTED로 방치 → CC가 12회 연속 모니터링하며 변화 감지 불가

### CC Status 변경 금지 [HARD — Effective S09-R3]

- [HARD] CC는 팀의 DISPATCH Status 테이블(IN_PROGRESS/BLOCKED/COMPLETED)을 **임의로 수정 금지**
- [HARD] Status 변경은 **팀 자체**가 수행. CC는 DISPATCH Status를 **읽기만** 한다
- [HARD] CC가 할 수 있는 것: _CURRENT.md의 팀 상태 행(IDLE/ACTIVE/MERGED) 관리 + DISPATCH 파일 active/↔completed/ 이동
- [HARD] S09-R3 사고: CC가 QA 확인 없이 BLOCKED로 임의 변경 → QA 실제로 작업 중이었음 → 상태 왜곡

## Session Lifecycle [HARD — Effective S09-R3]

**목표**: 완료 후 세션 컨텍스트를 정리하여 토큰 낭비 방지. Worktree는 유지.

**사고이력 (S09-R2)**: QA 에이전트가 context limit 도달 후 28회 연속 작업 불능 발생.
원인: 이전 라운드 컨텍스트가 누적되어 새 DISPATCH 수행 불가. `/clear` 미실행.

After completing ALL DISPATCH tasks and pushing:

1. Update DISPATCH Status → COMPLETED + build evidence
2. Push the status update to `team/{team-name}`
3. **`/clear` 실행 [HARD]**: COMPLETED push 직후 반드시 `/clear`로 세션 컨텍스트 초기화
   - [HARD] `/clear` 없이 다음 DISPATCH 대기 = context 누적 → 작업 불능 위험
   - [HARD] 새 DISPATCH 수신 시 **항상 깨끗한 세션**으로 시작
   - Worktree 디렉토리와 브랜치는 유지 (소스 관리)
   - DISPATCH 파일은 git에 push되므로 `/clear` 후에도 CC가 확인 가능
4. Report completion to Commander Center (DISPATCH 상태 업데이트로 대체)

### 완료 프로세스 흐름 (개정)

```
DISPATCH 작업 완료
    ↓
git add → git commit → git push origin team/{team-name}
    ↓
DISPATCH Status COMPLETED 업데이트 → push
    ↓
[HARD] /clear 실행 ← 이 단계 누락 = context 누적 = 다음 라운드 작업 불능
    ↓
CC가 DISPATCH Status 확인 → 머지 → _CURRENT.md 업데이트
    ↓
새 DISPATCH 발행 → 팀은 깨끗한 세션으로 수행
```

## CC Merge Protocol [HARD — 자율 주행 원칙 v2]

**CC는 DISPATCH Status COMPLETED + 빌드 증거 확인 후 자율적으로 머지 판단.**

### 감지 방식 (이중 체크) [HARD]
- [HARD] **방법 1**: `git log --oneline origin/team/{team} --not main` — team 브랜치 미머지 커밋 확인
- [HARD] **방법 2**: DISPATCH 파일 Status 테이블 직접 읽기 — COMPLETED 여부 확인
- [HARD] 방법 1 또는 방법 2 중 하나라도 COMPLETED 감지 → 머지/처리 진행

### 머지 규칙
- [HARD] DISPATCH Status가 COMPLETED + 빌드 증거 있음 → CC가 자율 검토 후 머지 실행
- [HARD] DISPATCH Status가 NOT_STARTED 또는 IN_PROGRESS → 머지 금지, 대기
- [HARD] 커밋이 push되었어도 Status가 COMPLETED가 아니면 → 머지 금지, 팀에 상태 업데이트 요청
- [HARD] diff에 문제가 있으면 (빌드 에러, 범위 외 수정) → 머지 보류, 사용자에게 보고
- [HARD] 머지 완료 후 _CURRENT.md 업데이트 + DISPATCH 파일 completed/ 이동 + push → 결과 보고

### 머지 후 정리 [HARD — Effective S07-R3]
- [HARD] 머지 완료 즉시 해당 팀 DISPATCH 파일을 `active/` → `completed/` 로 이동
- [HARD] _CURRENT.md 해당 팀 행을 `IDLE`로 업데이트 (파일명 `-` 로 표시)
- [HARD] 정리 후 반드시 `git add .moai/dispatches/ && git commit && git push origin main` 실행
- [HARD] **MERGED DISPATCH 파일이 active/에 잔존하면 팀이 세션 재시작 시 계속 IDLE 보고 반복** — 이것이 S07-R3 사고 원인

### 머지 후 team 브랜치 동기화 [HARD — Effective S15-R2]
- [HARD] CC가 머지 완료 후 **워크트리 디렉토리**에서 `git reset --hard origin/main && git push origin team/{team} --force` 실행
- [HARD] `merge` 대신 **`reset --hard`** 사용 — merge commit 누적 방지 (S15-R2 사고교훈)
- [HARD] 명령: `cd .worktrees/{team-name} && git reset --hard origin/main && git push origin team/{team} --force`
- [HARD] 워크트리가 브랜치를 잡고 있으므로 `git checkout` 불가 → 반드시 워크트리 디렉토리에서 직접 실행
- [HARD] 미동기화 시 `git log origin/team/{team} --not main`이 **이미 머지된 커밋을 false positive**로 보고
- [HARD] S09-R3 사고: Coordinator 머지 완료했으나 team/coordinator 브랜치 미동기화 → 다음 모니터링에서 동일 커밋 재감지
- [HARD] S15-R2 사고: `merge` 방식 사용으로 Team B에 5개, Design에 19개 merge commit 누적 → `reset --hard`로 전환

### 머지 전 소유권 교차 검증 [HARD — Effective S09-R3]
- [HARD] CC가 머지 전 `git diff --name-only main..origin/team/{team}` 로 변경 파일 목록 확인
- [HARD] role-matrix.md 디렉토리 단위 소유권 테이블과 교차 검증
- [HARD] **타 팀 소유 파일 포함 시**: 머지 보류 + 사용자 보고 + 해당 팀에 범위 위반 통지
- [HARD] S09-R3 사고: Coordinator가 Design 소유(Converters, DesignTime) 수정 + Design이 Coordinator 소유(tests.integration) 수정

### 직접 main push 감지 [S07-R1 사고교훈]
- [HARD] team/{team} 브랜치에 미머지 커밋이 없는데 DISPATCH Status가 COMPLETED → main 직접 push 케이스
- [HARD] 이 경우 머지는 불필요, _CURRENT.md MERGED 업데이트만 실행

**자율 판단 기준**: DISPATCH Status + 빌드 증거 + diff 범위 검토. 이 3개가 통과되면 묻지 않고 실행.

## CC Time Analysis Protocol [HARD — Effective S14-R1]

**CC는 팀 타임스탬프 데이터를 수집하여 운영 방식을 점진적으로 진화시킨다.**

### 수집 메트릭

| 메트릭 | 계산 방식 | 활용 목적 |
|--------|-----------|-----------|
| **DISPATCH 수신→시작** | IN_PROGRESS 타임스탬프 - _CURRENT.md 발행 시각 | 팀 반응 속도, DISPATCH 전달 지연 분석 |
| **시작→완료** | COMPLETED 타임스탬프 - IN_PROGRESS 타임스탬프 | 팀별 작업 소요 시간, 예측 정확도 |
| **팀간 완료 편차** | Phase 내 팀간 COMPLETED 타임스탬프 표준편차 | 병목 팀 식별, Phase 재설계 |
| **CC 머지→Phase 오픈** | Phase 오픈 시각 - 마지막 COMPLETED 타임스탬프 | CC 반응 속도 자가 측정 |
| **전체 라운드 소요** | 마지막 팀 COMPLETED - DISPATCH 발행 시각 | Sprint/Round 효율성 추이 |
| **BLOCKED 지속** | BLOCKED 타임스탬프 - 해결 시각 | 환경/의존성 문제 패턴 분석 |

### 분석 주기

- **라운드 종료 시**: 해당 라운드 메트릭 계산 → _CURRENT.md DISPATCH 라운드 이력에 기록
- **Sprint 종료 시**: Sprint 전체 라운드 평균/추이 분석 → 진화 메트릭 테이블 업데이트
- **3 Sprint 누적 시**: 운영 파라미터 재조정 (모니터링 주기, Phase 구조, 팀 분배)

### 진화 대상 파라미터

| 파라미터 | 현재 값 | 조정 기준 |
|----------|---------|-----------|
| CC 모니터링 주기 | 20분 | 단일 통일 주기 (2026-04-20~) |
| Phase 구조 | A+B→CO→QA→RA | 팀간 의존성 변화 시 재설계 |
| 팀 분배 | 고정 6팀 | 메트릭 기반 병목 해소 |

### 타임스탬프 기록 위치

1. **팀 DISPATCH Status 테이블**: 각 Task 행의 `타임스탬프` 열
2. **_CURRENT.md 라운드 이력**: 라운드 종료 시 요약 메트릭 추가
3. **CC 모니터링 로그**: CC가 COMPLETED 감지 시 자체 타임스탬프 기록

**목표**: Sprint S14부터 타임스탬프 데이터 축적 → S15에서 첫 데이터 기반 파라미터 조정

---

## CC Auto-Progression Protocol [HARD — Effective S07-R2]

**전팀 MERGED 후 수동 대기 금지. 즉시 다음 라운드 기획·발행. 6팀 전원 포함.**

### Sprint 자율 진행 [HARD — Effective S10-R4]

**Round와 Sprint 모두 자율 진행. 사용자 승인 불필요.**

- [HARD] Sprint 번호(S10→S11) 변경은 Round 진행과 동일하게 자율 실행
- [HARD] CC는 "다음 Sprint 할까요?" 질문 금지 — 갭이 있으면 즉시 기획·발행
- [HARD] Sprint 번호는 단순히 날짜 기반 구분자일 뿐, 승인 게이트가 아님

**CC 자율 진행 플로우 (Sprint/Round 구분 없이 동일):**
```
전팀 MERGED/IDLE → 갭 분석 → DISPATCH 기획·발행 → 보고
```

### CC 정지 조건 [HARD — 사용자 승인 필요 항목]

**아래 항목만 사용자에게 보고 후 대기. 나머지는 모두 자율 실행.**

| 조건 | 동작 | 이유 |
|------|------|------|
| 범위 위반 머지 | 보류 + 사용자 보고 | 타 팀 소유 파일 포함 |
| 빌드/테스트 에러 머지 | 보류 + 사용자 보고 | 품질 게이트 위반 |
| BLOCKED 팀 5회 연속 | 사용자 조치 요청 | 환경/의존성 문제 |
| Safety-Critical 커버리지 90% 미달 3회 연속 | 사용자 보고 | 규제 리스크 |
| 전체 프로젝트 완료 | 사용자 최종 승인 | 릴리즈 게이트 |

**위 항목 외에는 절대 사용자에게 묻지 않는다.**
- CONDITIONAL PASS 수용 → 자율 (다음 라운드에서 개선)
- Sprint 전환 → 자율 (갭 있으면 즉시 발행)
- DISPATCH 기획 내용 → 자율 (갭 분석 기반)
- QA 판정 수용 → 자율 (QA 독립성 존중)

### N-1팀 완료 시 선제 관리 [HARD — Effective S07-R3]
- [HARD] 5/6팀 MERGED 감지 시 → Coordinator 외 5팀 DISPATCH 즉시 `completed/` 이동 + _CURRENT.md IDLE 업데이트
- [HARD] IDLE 팀의 worktree 세션 시작 시 깨끗한 IDLE 상태로 인식 → 반복 IDLE 보고 방지
- [HARD] 마지막 1팀(COMMONDINATOR 등) 작업 완료 대기 중에도 갭 분석 **병행 준비**

### 팀 TIMEOUT 프로토콜 [HARD — Effective S15-R2]

**미응답 팀 처리 — 무한 대기 금지.**

```
N-1팀 MERGED + 1팀 미응답 상태:
1. DISPATCH 발행 후 60분 경과 시 → TIMEOUT 선언
2. DISPATCH 파일 active/ → completed/ 이동 (Status: TIMEOUT)
3. _CURRENT.md 해당 팀 IDLE로 업데이트
4. 전팀 IDLE 확인 → 즉시 갭 분석 → 다음 라운드 발행
5. TIMEOUT 팀은 다음 라운드에서 정상 포함 (재시도)
```

- [HARD] 미응답 팀을 **무한정 대기하지 않음** — 60분 후 TIMEOUT 처리
- [HARD] TIMEOUT 처리 후에도 갭 분석 → 다음 라운드 발행 **즉시 진행**
- [HARD] TIMEOUT 팀은 페널티 없이 다음 라운드에 포함 (워크트리/에이전트 미실행이 원인일 수 있음)
- [HARD] 연속 3회 TIMEOUT → 사용자 보고 (워크트리/에이전트 구성 문제 의심)
- [HARD] S15-R2 사고: Design이 응답하지 않아 S15-R2가 무기한 대기 상태 방치
- [HARD] 절대 "사용자가 모니터링 지시할 때까지 대기" 금지 — CC가 자율 판단하여 실행

### 전팀 완료 감지
- [HARD] CC 모니터링에서 **전팀 MERGED/IDLE** 감지 시 → 즉시 갭 분석 실행
- [HARD] "전팀" = Team A, Team B, Coordinator, Design, QA, RA 6팀 전원

### 갭 분석 (자동)
- [HARD] 커버리지 < 85% 모듈 식별
- [HARD] 통합테스트 누락 항목
- [HARD] 문서 동기화 갭 (RA)
- [HARD] Coordinator 대기작업 (DI 등록, ViewModel, 통합테스트)
- [HARD] Design 대기작업 (PPT 미구현 화면)

### DISPATCH 발행 (6팀 전원)
- [HARD] **모든 라운드에 6팀 전원 DISPATCH 발행** — 팀 제외 금지
- [HARD] 할 일이 없는 팀은 "IDLE CONFIRM" DISPATCH 발행 (상태 유지만 확인)
- [HARD] 각 팀의 소유 모듈/문서 기준으로 실질적 작업 할당
- [HARD] 사용자에게 "전팀 완료 → S{N}-R{M} 발행" 보고 (대기 보고 금지)

**자동 기획 프로세스:**
```
1. 전팀 MERGED/IDLE 감지
2. 갭 분석 (커버리지, 통합테스트, 문서, Coordinator, Design)
3. 6팀별 DISPATCH 생성 (Team A, Team B, Coordinator, Design, QA, RA)
4. _CURRENT.md 업데이트 (전팀 ACTIVE)
5. commit + push
6. "S{N}-R{M} 발행 완료 (6팀)" 보고
```

### CC 모니터링 주기 [HARD — Effective S15 CronCreate 의무화]

**20분 단일 주기 + CronCreate 자동화 (2026-04-21~)**

- [HARD] CC 세션 시작 시 ACTIVE 팀 있으면 → **즉시 CronCreate**로 20분 모니터링 루프 생성
- [HARD] CronCreate 없이 "모니터링 중" 보고 = 프로토콜 위반
- [HARD] 팀 동기화 주기: **20분** (CC와 동일)
- [HARD] 차등 주기(10/15/20분) 폐지 — 단일 20분으로 통일

**CC 세션 시작 절차 (CronCreate 포함):**
```
Step 0: git pull origin main
Step 1: Read _CURRENT.md → ACTIVE 팀 확인
Step 2: ACTIVE 팀 있으면 → CronCreate("*/20 * * * *", "CC 20분 모니터링") 즉시 실행
Step 3: ACTIVE 팀 없고 전팀 MERGED/IDLE → 갭 분석 → DISPATCH 발행 → CronCreate
```

**모니터링 루프 (CronCreate로 자동 실행):**
```
1. git fetch origin
2. git log --oneline origin/team/* --not main (6팀)
3. DISPATCH 파일 Status 테이블 확인
4. COMPLETED → 소유권 검증 → 머지 → _CURRENT.md 업데이트
5. 전팀 MERGED → CronDelete → 갭 분석 → DISPATCH 발행 → 새 CronCreate
```

**팀 동기화 규칙:**
```
1. DISPATCH 읽기 직후: Status NOT_STARTED → IN_PROGRESS 업데이트 + push
2. 작업 진행 중: 20분마다 Status 확인 + 필요 시 업데이트
3. 작업 완료 시: Status IN_PROGRESS → COMPLETED + 빌드 증거 + push
4. 작업 불가 시: Status NOT_STARTED → BLOCKED + 사유 기재 + push
```

**비상 규칙:**
- [HARD] 전팀 MERGED 후 **1시간 이내** 다음 라운드 발행 미준비 = 사용자 즉시 보고
- [HARD] S10-R4→S11-R1 사례(12시간 지연) 재발 방지
- [HARD] CC 모니터링 0회 = 프로토콜 위반 (S11-R1 교훈)

**진화 메트릭 (데이터 기반 최적화 목표):**
| 지표 | S10 기준 | S11 목표 | S12 목표 |
|------|----------|----------|----------|
| 라운드 소요 시간 | 2시간 1분 | 1시간 30분 | 1시간 15분 |
| CC 모니터링 횟수 | 0회 (실패) | 6회/라운드 | 4회/라운드 |
| 전팀 MERGED→발행 | 11시간 53분 | 10분 이내 | 5분 이내 |
| 팀간 완료 편차 | 18분 | 10분 | 5분 |

**단일 통일 주기 (2026-04-20~):**
```
모든 모니터링/동기화: 20분 단일 주기
차등 주기(10/15/20분) 폐지
```

---

## Issue Tracking Protocol [HARD — Effective S05-R2]

### Pre-Work Issue Registration (Mandatory)

- [HARD] Create a Gitea issue BEFORE starting any DISPATCH task
- [HARD] Working without an issue = protocol violation
- [HARD] Every issue MUST have team label (`team-a`, `team-b`, `team-design`, `coordinator`, `qa`, `ra`)
- [HARD] Every issue MUST have priority label (`priority-critical`, `priority-high`, `priority-medium`)

### Korean-Safe Issue Creation

```bash
# NEVER use curl with inline Korean — causes U+FFFD corruption
# Always use file-based API helper:
bash scripts/issue/gitea-api.sh issue-create "TITLE" "BODY" "team-a,priority-high"

# Or PowerShell (UTF-8 safe):
pwsh scripts/issue/New-GiteaIssue.ps1 -Title "Title" -Body "Body" -Labels @(42)
```

### Issue Lifecycle

- [HARD] After task completion, post a **completion comment** on the issue (build result, test count, PR number)
- [HARD] After comment, **Close** the issue
- [HARD] Git commit message must reference issue number: `feat(team-a): ... (#issue-number)`

### Issue-PR Workflow

```
Flow:
1. Read DISPATCH -> Create issue (with labels)
2. Record issue number in DISPATCH Status
3. Complete work -> git commit references issue number
4. Post completion comment on issue -> Close issue
5. Update DISPATCH Status
```

### Korean Encoding Rules [CRITICAL]

- [HARD] NEVER use Korean in bash curl inline — causes U+FFFD corruption
- [HARD] For Korean text in API calls, use ONE of:
  1. `scripts/issue/gitea-api.sh` (bash, file-based transfer)
  2. `scripts/issue/New-GiteaIssue.ps1` (PowerShell, UTF-8 safe)
  3. Git commit messages: Korean OK (git handles UTF-8 natively)
- [HARD] English-only content: curl direct usage is fine

---

## Team Obligations [HARD]

- **DISPATCH Resolution**: Read `_CURRENT.md` then specified DISPATCH file (see Resolution Protocol above)
- Verify acceptance criteria for each Task
- Stay within file ownership scope
- Report BLOCKED honestly when stuck (do not guess)
- Report IDLE when no DISPATCH found — do not fabricate work
- PR creation is FORBIDDEN (Commander Center exclusive)
