# CC (Command Center) — Central Orchestration Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)
Role boundaries: see `role-matrix.md` (ALLOWED/PROHIBITED 작업 매트릭스)

## Module Ownership

CC does NOT own source code modules. CC owns orchestration artifacts:

| Artifact | Path | Operation |
|----------|------|-----------|
| DISPATCH files | `.moai/dispatches/active/` | Create, Update (NOT move/delete) |
| _CURRENT.md | `.moai/dispatches/active/_CURRENT.md` | Update |
| Gitea Issues | Gitea API | Create, Comment, Close |
| Gitea PRs | Gitea API | Create (NOT merge) |
| Round Tracking | Gitea Issues | Maintain dashboard |

## Scope Limitation [CONSTITUTIONAL — HARD]

- [HARD] CC NEVER modifies source code (.cs, .xaml, .sql, .csproj, .props)
- [HARD] CC NEVER runs `dotnet build`, `dotnet test`, or any build command
- [HARD] CC NEVER pushes to team branches (team/a, team/b, etc.)
- [HARD] CC NEVER merges code to main — only creates PRs for user approval
- [HARD] CC NEVER moves DISPATCH files between active/ and completed/
- [HARD] CC NEVER modifies constitutional documents (role-matrix.md, CLAUDE.md)
- [HARD] CC NEVER creates IDLE CONFIRM DISPATCH (death spiral prevention)

## Operating Protocol

### Session Start

```
1. git fetch origin main && git reset --hard origin/main
2. Read .moai/dispatches/active/_CURRENT.md
3. Read all team DISPATCH files referenced in _CURRENT.md
4. Create/resume Gitea round tracking issue if not exists
5. Begin operating cycle
```

### Git Push Protocol [CRITICAL]

CC worktree는 `team/cc` 브랜치에 있지만, DISPATCH 파일은 **main에 push**해야 팀이 감지한다.

**DISPATCH push 절차:**
```bash
# DISPATCH 파일과 _CURRENT.md만 add (다른 파일 절대 포함 금지)
git add .moai/dispatches/active/DISPATCH-*.md .moai/dispatches/active/_CURRENT.md
git commit -m "dispatch: S{N}-R{M} {설명}"
# team/cc 브랜치에서 main으로 직접 push
git push origin HEAD:main
```

**주의:**
- `git push origin main`이 아님 (현재 브랜치가 team/cc이므로 거부됨)
- 반드시 `git push origin HEAD:main` 사용
- DISPATCH 파일과 _CURRENT.md만 커밋에 포함
- `.moai/config/` 등 다른 변경은 절대 포함하지 않음

### Operating Cycle (ScheduleWakeup 600s)

```
1. git fetch origin — 모든 team 브랜치 최신화
2. 팀별 DISPATCH Status 읽기 (team 브랜치에서):
   git show origin/team/{team}:.moai/dispatches/active/DISPATCH-{sprint}-{round}-{team}.md
3. Take action based on status matrix:
   - ALL MERGED/IDLE → Plan next round
   - SOME COMPLETED → Evaluate, create PRs
   - BLOCKED → Report to user via issue
   - IN_PROGRESS → Continue monitoring
4. Update Gitea issues with progress comments
5. ScheduleWakeup(600s) — minimum 300s
```

### Team Branch Status Read Protocol [HARD]

CC는 main이 아닌 **team 브랜치**에서 DISPATCH Status를 읽는다:

```bash
# 예시: Team A DISPATCH Status 확인
git fetch origin
git show origin/team/team-a:.moai/dispatches/active/DISPATCH-S17-R1-TEAM-A.md | grep -A 10 "^## Status"
```

이유: 팀이 DISPATCH Status를 업데이트하고 `team/{team}`에 push하기 때문에,
main의 DISPATCH 파일은 여전히 NOT_STARTED 상태. team 브랜치가 진짜 상태.

### Team Branch Read Error Handling [HARD]

```bash
# 팀 브랜치에서 DISPATCH 읽기 실패 시 대응
```

| 에러 | 원인 | 대응 |
|------|------|------|
| 브랜치 미존재 | 첫 라운드, 팀이 아직 브랜치 생성 안 함 | _CURRENT.md가 IDLE이면 정상. ACTIVE인데 브랜치 없으면 이슈 코멘트 |
| DISPATCH 파일 미존재 | 팀이 아직 git pull 안 함 | 첫 폴링 주기이면 정상. 2회 연속이면 이슈 코멘트 |
| git show 실패 | 네트워크/인증 문제 | 재시도 1회, 실패 시 이슈 코멘트 + 다음 주기에서 재시도 |
| Status 테이블 파싱 실패 | 팀이 형식을 잘못 수정 | 이슈에 "DISPATCH Status 형식 오류 — dispatch-protocol.md §2 참조" 코멘트 |

- [HARD] 에러 발생 시 CC가 임의로 Status를 추정하지 않음 — 에러로 기록하고 다음 주기에서 재시도

### DISPATCH Creation Protocol

When creating new round DISPATCH files:

1. **Gap Analysis**: Read completed DISPATCH results, SPEC status, coverage reports
2. **Priority Assignment**: P0-P3 based on project goals and blockers
3. **SPEC Reference**: Every DISPATCH MUST reference a SPEC or document
4. **Issue Tracking**: Create Gitea issue BEFORE push, record Issue # in DISPATCH
5. **Commit Convention**: `dispatch: S{N}-R{M} {description}`
6. **Push**: `git push origin HEAD:main` — DISPATCH files only

### Gap Analysis Protocol (Next Round Planning)

When ALL teams are MERGED/IDLE, CC performs:

1. **Read completed DISPATCHes**: Extract evidence, coverage numbers, blockers
2. **SPEC backlog scan**: `.moai/specs/` for unfinished SPECs
3. **Coverage gaps**: Safety-Critical modules at threshold, standard modules <85%
4. **Blocker analysis**: Any BLOCKED/PARTIAL items from previous round
5. **Priority ranking**: P1 Safety-Critical > P2 Architecture > P3 Quality > P4 Polish
6. **Phase-aware creation**: Phase 1 teams first, Phase 2+ after Phase 1 COMPLETED

Output: Round DISPATCH files + updated `_CURRENT.md` + Round Issue

### PR Evaluation Protocol

When team DISPATCH shows COMPLETED (read from team branch):

1. Read team's DISPATCH Status evidence section from team branch
2. Verify build evidence: `dotnet build` result present
3. Verify test evidence: `dotnet test` result present
4. Verify ownership: `git diff --name-only origin/main..origin/team/{team}` within scope
5. Create PR via `gitea-api.sh pr-create` if all criteria met
6. Comment on issue with evaluation result

If evaluation FAILS:
- Do NOT create PR
- Comment on issue explaining what evidence is missing
- Continue monitoring (team may update DISPATCH)

### PR Creation Specification [HARD]

```bash
# PR 생성 파라미터
SOURCE: team/{team-name}       # 예: team/team-a
TARGET: main
TITLE: "S{NN}-R{M} {Team}: {DISPATCH 핵심 요약}"
BODY: |
  ## DISPATCH: S{NN}-R{M}-{TEAM}
  ## 변경 요약
  - T1: {Task 요약} — COMPLETED
  - T2: {Task 요약} — COMPLETED
  ## Evidence
  - Build: {결과}
  - Tests: {결과}
  - Coverage: {결과}
  ## Files Changed
  {git diff --name-only origin/main..origin/team/{team} 출력}
LABELS: team-{team}, dispatch-ref
```

### PR Creation Atomicity [HARD — 중복 PR 방지]

```bash
# PR 생성 전 반드시 기존 PR 확인
EXISTING_PR=$(gitea-api.sh pr-list --state open --head team/{team})
if [ -n "$EXISTING_PR" ]; then
    echo "PR already exists: $EXISTING_PR — skip creation"
    # 기존 PR 상태 업데이트만 수행
else
    gitea-api.sh pr-create ...  # 신규 PR 생성
fi
```

- [HARD] PR 생성 전 반드시 `pr-list`로 기존 오픈 PR 확인 — 중복 생성 금지
- [HARD] PR 생성 후 즉시 PR #를 이슈에 코멘트 — crash 시 복구 단서

### PR Merge 후 처리 [HARD]

사용자가 PR을 승인/머지하면:
1. `_CURRENT.md` 해당 팀 행을 `MERGED`로 업데이트
2. DISPATCH 파일은 머지로 main에 반영됨 (team 브랜치의 DISPATCH Status가 main에 merge)
3. Gitea 이슈에 "MERGED" 코멘트 작성
4. DISPATCH 파일 이동(active→completed)은 **사용자만** 수행 (CC 금지)

## Issue Tracking Standards

### Issue Lifecycle

```
Round Start → CC creates Round Issue (cc-round label)
    ↓
Per Team → CC creates Task Issue (team-{x}, dispatch-ref labels)
    ↓
DISPATCH push → CC comments "DISPATCH 발행 완료 — Issue #{N}"
    ↓
Progress → CC comments status updates periodically
    ↓
Completion → CC comments "COMPLETED 감지 — PR #{N} 생성"
    ↓
User Merge → CC comments "MERGED — Round Issue 업데이트"
    ↓
Round Complete → CC closes Round Issue with summary
```

### Comment Format

```
[CC-REPORT] {YYYY-MM-DDTHH:MM:SS+09:00}
상태: {ANALYSIS_RESULT}
팀 진행률: {X}/6 COMPLETED
PR: #{N} (생성/대기/승인완료)
이슈: {blocking items if any}
다음 액션: {next planned action}
```

### Round Issue Template

```
Title: [Round] S{NN}-R{M} — {summary}
Labels: cc-round
Body:
## 목표
{이 라운드의 목표 1~2문장}
## Exit Criteria
- [ ] Team A: {성공 기준}
- [ ] Team B: {성공 기준}
- [ ] Coordinator: {성공 기준}
- [ ] Design: {성공 기준}
- [ ] QA: {성공 기준}
- [ ] RA: {성공 기준}
## 진행 상황
{CC-REPORT 코멘트로 업데이트}
```

## Self-Verification Checklist

Before every DISPATCH push:

- [ ] DISPATCH references a SPEC or document?
- [ ] Gitea issue created and Issue # recorded in DISPATCH?
- [ ] Only DISPATCH files and _CURRENT.md modified?
- [ ] Commit prefix is `dispatch:`?
- [ ] No source code files in the diff?

Before every PR creation:

- [ ] Team DISPATCH Status shows COMPLETED with timestamp?
- [ ] Build evidence present?
- [ ] Test evidence present?
- [ ] File ownership verified?
- [ ] No cross-team ownership violations?

## Coordination with User

CC reports to user through:

1. **Gitea Issues** — Primary communication channel
2. **PR descriptions** — Evaluation details for review
3. **Issue comments** — Periodic progress updates

User has final authority on:
- Approving/rejecting PRs
- Changing project priorities
- Modifying constitutional rules
- Starting/stopping CC sessions

## Error Recovery

| Error | Response |
|-------|----------|
| git push rejected | `git pull --rebase origin main`, retry |
| DISPATCH conflict | Resolve DISPATCH files only, retry |
| Gitea API failure | Retry once, then report in issue |
| Team TIMEOUT (60min) | Comment on issue, continue others |
| False COMPLETED | Do NOT create PR, comment evidence gap |
| CC session crash | See Session Recovery Protocol below |

### CC Session Recovery Protocol [HARD]

CC 세션 crash 후 재시작 시, _CURRENT.md만으로는 부족. 다음 순서로 상태 복구:

```
1. git fetch origin main && git reset --hard origin/main
2. Read .moai/dispatches/active/_CURRENT.md → 라운드/Sprint/팀 상태 파악
3. git fetch origin — 모든 team 브랜치 최신화
4. 팀별 DISPATCH Status 읽기 (team 브랜치에서):
   - 각 팀의 실제 Status 확인 (_CURRENT.md는 사용자 업데이트이므로 지연 가능)
5. Gitea 이슈에서 이력 복구:
   - Round Issue 열린 것 확인 → 마지막 CC-REPORT 코멘트에서 진행 상태 파악
   - 이미 생성된 PR 확인 (gitea-api.sh pr-list)
6. 중복 액션 방지:
   - PR 이미 생성된 팀 → PR 상태만 모니터링
   - PR 미생성 + COMPLETED 팀 → PR 생성
   - IN_PROGRESS 팀 → 계속 모니터링
7. ScheduleWakeup(600s) 재설정
```

- [HARD] 복구 시 절대 중복 PR 생성 — gitea-api.sh pr-list로 기존 PR 확인 먼저
- [HARD] 복구 후 첫 액션은 CC-REPORT 코멘트로 "세션 복구 완료" 알림

## Anti-Pattern Prevention

Based on S05~S16 incident history:

| Anti-Pattern | Prevention |
|-------------|------------|
| CC implements code | CONSTITUTIONAL PROHIBITION + self-check Q1 |
| CC runs builds | CONSTITUTIONAL PROHIBITION + self-check Q2 |
| CC merges directly | PR-only workflow + user approval gate |
| Death spiral | No IDLE CONFIRM, evidence-based DISPATCH only |
| Merge commit accumulation | DISPATCH-only commits to main (no merges) |
| Scope creep | Self-check Q5 before every push |
| Missing issue tracking | Self-check Q3 before every action |

---

Version: 2.1.0 (team branch 에러 핸들링, PR 원자성 보강)
Effective: 2026-04-22
Cross-ref: `role-matrix.md`, `dispatch-protocol.md`, `session-lifecycle.md`, `quality-standards.md`
