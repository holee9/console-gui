# Team Common Rules [ALL TEAMS]

## DISPATCH Resolution Protocol [HARD — FIRST ACTION]

**세션 시작 시 가장 먼저 실행. 다른 어떤 작업보다 우선.**

```
Step 0: git pull origin main  ← [HARD] _CURRENT.md 읽기 전 반드시 실행 (구버전 오독 방지)
Step 1: Read D:/workspace-gitea/Console-GUI/.moai/dispatches/active/_CURRENT.md
Step 2: Find your team in the index table
Step 3: Read ONLY that specified DISPATCH file from the same active/ directory
Step 4: If your team shows IDLE or no active entry → Report IDLE to Commander Center
```

- [HARD] **Step 0 필수**: `_CURRENT.md` 읽기 전 반드시 `git pull origin main` 실행 — 미실행 시 구버전 DISPATCH 오독으로 IDLE 오보고 발생
- [HARD] `_CURRENT.md` 상태가 `MERGED` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고
- [HARD] `_CURRENT.md`에 없는 팀은 작업 없음(IDLE) — 임의로 다른 DISPATCH 파일을 찾지 않는다
- [HARD] 루트의 `DISPATCH-*-2026-04-*.md` 파일은 **절대 읽지 않는다** — 모두 아카이브된 구형 파일. `DISPATCH.md`(CC 전용)도 읽지 않는다
- [HARD] 날짜가 오래된 DISPATCH 파일(DISPATCH-TEAM-*-2026-04-XX.md 형식)은 모두 아카이브됨 — 읽지 않는다
- [HARD] 작업이 완료된 후 새 DISPATCH가 없으면 Commander Center에 IDLE 보고하고 대기한다
- [HARD] DISPATCH 파일 경로는 항상 Main 프로젝트 절대경로 기준: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

**IDLE 보고 형식:**
```
State: IDLE
Reason: No active DISPATCH found in _CURRENT.md for this team
Last completed: [마지막 완료 작업 요약]
Awaiting: New DISPATCH from Commander Center
```

---

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

## Git Completion Protocol [HARD]

After completing DISPATCH tasks:
1. `git add` changed files (exclude secrets, temp files)
2. `git commit` with conventional commit format matching team prefix
3. `git push origin team/{team-name}`
4. **DO NOT create PR** — PR creation is Commander Center exclusive authority

Push failure: report "PUSH_FAILED" status in DISPATCH.md, commit+push the status update.

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
