# Operations Protocol — DISPATCH Template & Checklists

For full operational rules, see: `docs/development/DEV-OPS-GUIDELINES.md` (Single Source of Truth)

---

## Pre-DISPATCH Checklist (사용자)

DISPATCH 작성 전 확인:
- [ ] DISPATCH 내용이 SPEC Acceptance Criteria와 일치하는가?
- [ ] Cross-team 의존성이 명시되어 있는가?
- [ ] Acceptance Criteria가 측정 가능한가 (수치, 명령어, 산출물)?

## DISPATCH Template v2.1

```markdown
# DISPATCH: {Team} — S{NN} Round {N}

| Field | Value |
|-------|-------|
| Issued | YYYY-MM-DD |
| Issued By | 사용자 |
| Target | {Team} |
| Branch | team/{team} |
| Type | S{NN} Round {N} — {summary} |
| Prerequisites | {None or Team X completion} |
| SPEC Reference | SPEC-XXX |

## Project Philosophy [MUST READ]

This project prioritizes **quality and completeness over speed**.

- 3 tasks at 100% > 10 tasks at 80%
- Prove "0 errors" — don't assume
- If uncertain, report BLOCKED — don't guess
- If incomplete, report PARTIAL — don't lie

## How to Execute
1. git pull origin main
2. Read this entire document
3. Execute Tasks in order, self-verify each
4. Update Status with build evidence
5. git add -> commit -> push (NO PR creation)

## Team Rules
Reference: `.claude/rules/teams/{team}.md` + `team-common.md`

## Context
{Background description}

## File Ownership
{Allowed file patterns}

## Task {N} ({Priority}): {Title}

### Pre-check
{Commands to run before starting}

### Implementation
{Detailed instructions}

### Acceptance Criteria [HARD]
- [ ] {Measurable criterion 1}
- [ ] {Measurable criterion 2}

## Build Verification [HARD]
dotnet build {project} -> 0 errors required
dotnet test {test project} -> all pass required

## Git Completion Protocol [HARD]
1. git add {changed files}
2. git commit -m "{type}({team}): S{NN} R{N} {summary}"
3. git push origin team/{team}
4. DO NOT create PR (user manages directly)

## Status (Update after work)
- **State**: NOT_STARTED
- **Completed Tasks**: --
- **Build Evidence**: --
- **Test Evidence**: --
- **Coverage Evidence**: --
- **New/Modified Files**: --
- **Issues**: --
- **Blocked By**: --
```

## Monitoring Judgment Criteria

| Situation | Judgment | Action |
|-----------|---------|-----------|
| Status=COMPLETED + uncommitted | DONE_UNCOMMITTED | Instruct commit+push |
| 20min+ no change | STALLED | Check status or re-dispatch |
| Acceptance unmet + COMPLETED | FALSE_REPORT | Issue correction DISPATCH |
| Status=BLOCKED | BLOCKED | Resolve dependency, re-dispatch |
