# Operations Protocol — DISPATCH Template & Checklists

For full operational rules, see: `docs/development/DEV-OPS-GUIDELINES.md` (Single Source of Truth)

---

## CC Pre-DISPATCH Checklist

Before writing a DISPATCH, CC verifies:
- [ ] Am I planning/instructing (allowed) or implementing (forbidden)?
- [ ] DISPATCH content aligns with SPEC acceptance criteria?
- [ ] Cross-team dependencies are specified?
- [ ] Acceptance criteria are measurable (numbers, commands, artifacts)?
- [ ] Source code files (.cs/.xaml/.csproj) are NOT being modified by CC?

## DISPATCH Template v2.0

```markdown
# DISPATCH: {Team} — S{NN} Round {N}

| Field | Value |
|-------|-------|
| Issued | YYYY-MM-DD |
| Issued By | Commander Center (CC) |
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
4. DO NOT create PR (CC exclusive)

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

## Monitoring Judgment Criteria (CC Reference)

| Situation | Judgment | CC Action |
|-----------|---------|-----------|
| Status=COMPLETED + uncommitted | DONE_UNCOMMITTED | Instruct commit+push |
| 20min+ no change | STALLED | Check status or re-dispatch |
| Acceptance unmet + COMPLETED | FALSE_REPORT | Issue correction DISPATCH |
| Status=BLOCKED | BLOCKED | Resolve dependency, re-dispatch |
