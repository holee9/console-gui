# DISPATCH Schema

Standard schema for team DISPATCH.md documents in worktree-based development.

## Required Sections [HARD]

Every DISPATCH.md MUST contain these sections:

### Header

```markdown
# DISPATCH: {Team} -- {Description}
- Issued: {YYYY-MM-DD}
- Issued By: Commander Center
- Priority: P0 | P1 | P2 | P3
- Supersedes: {previous DISPATCH ref or "none"}
```

### Team Role Confirmation [HARD]

```markdown
## {Team} Role Confirmation (.claude/rules/teams/{team}.md)
- Owned modules: {list from team rule file}
- Key constraints: {derived from team rule file}
- Forbidden actions: {derived from team rule file}
```

- MUST reference the team's rule file at `.claude/rules/teams/{team}.md`
- MUST list: owned modules, key constraints, forbidden actions
- Omitting this section is a schema violation

### How to Execute

```markdown
## How to Execute
1. Read this DISPATCH completely
2. Execute tasks in priority order (P0 first)
3. Update checkboxes only after verification
4. Complete Git Completion Protocol
5. Update Status section
```

### Tasks

```markdown
## Tasks

### Task 1: {Title} [P0]
{Description}

Verification:
- [ ] {Objectively verifiable criterion with measurable output}
- [ ] {Build output, test count, coverage %, or file existence}
```

- Each task: title, priority tag, description, verification criteria with checkboxes
- Verification criteria MUST be objectively verifiable (build output, test count, coverage %)
- Subjective criteria ("looks good", "works correctly") are not acceptable

### Constraints [HARD]

```markdown
## Constraints [HARD]
- File ownership: Only modify files in {owned modules}
- Cross-team notification: {required notifications from team rules}
- Safety-critical: {restrictions if applicable}
```

- Derived from team rules file
- File ownership boundaries
- Cross-team notification requirements
- Safety-critical restrictions (if applicable)

### Git Completion Protocol [HARD]

```markdown
## Git Completion Protocol [HARD]
1. git add (DISPATCH.md + changed files, exclude sensitive files)
2. git commit (conventional commit format)
3. git push origin team/{team}
4. Create PR via Gitea API (check for existing open PR first)
5. Record PR URL in Status
```

This section MUST be included verbatim in every DISPATCH. Teams that skip git push or PR creation have not completed their work.

### Final Verification [HARD]

```markdown
## Final Verification [HARD -- DO NOT report COMPLETED without this]
1. Own module build: `dotnet build` or MSBuild -> 0 errors
2. Own module tests: `dotnet test {team-tests}` -> all pass
3. Full solution build: `dotnet build HnVue.sln` -> record result (SHOULD)
4. Copy build output summary to Status
```

This section prevents false completion reports. Build evidence is mandatory.

### Status

```markdown
## Status
- **State**: NOT_STARTED | IN_PROGRESS | COMPLETED | BLOCKED
- **Build Evidence**: {paste last line of build output}
- **PR**: {PR URL or "not created"}
- **Results**: Task 1->{status}, Task 2->{status}, ...
```

## Optional Sections

### Dependencies

```markdown
## Dependencies
- Task 2 BLOCKED by Team A Task 1 (interface change)
```

Cross-team task dependencies with BLOCKED status explanation.

### File Ownership

```markdown
## File Ownership
- src/HnVue.{Module}/**
- tests/HnVue.{Module}.Tests/**
```

Explicit file patterns this team may modify. Useful when ownership is ambiguous.

## Validation Rules

- State transition: NOT_STARTED -> IN_PROGRESS -> COMPLETED (no skipping)
- COMPLETED requires: all HARD checkboxes checked + Build Evidence present + PR URL present
- BLOCKED requires: blocker description + which team/task is blocking
- IN_PROGRESS: at least one task started, Status updated with partial results

## Anti-Patterns

| Anti-Pattern | Why It Fails | Prevention |
|-------------|-------------|------------|
| Setting COMPLETED with unchecked checkboxes | False reporting, hides real issues | Final Verification section enforces evidence |
| Missing Team Role Confirmation section | Team operates without boundary awareness | Schema validation rejects DISPATCH without it |
| Missing Git Completion Protocol section | Work stays local, never reaches main | Schema requires verbatim inclusion |
| Build evidence showing errors but State = COMPLETED | Contradictory status | Commander Center cross-checks evidence vs state |
| Modifying files outside team ownership boundaries | Cross-team conflicts, broken builds | Constraints section + team rules enforcement |
| Skipping PR creation after push | Work is invisible to Commander Center | Git Protocol step 4 is HARD requirement |
