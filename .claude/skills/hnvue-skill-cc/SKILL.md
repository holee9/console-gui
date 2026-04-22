---
name: hnvue-skill-cc
description: "Command Center orchestration skill for HnVue team coordination. DISPATCH lifecycle management, Gitea PR/issue automation, team progress monitoring, gap analysis. Loaded by hnvue-cc agent. Triggers on: DISPATCH, orchestration, PR, issue, monitoring, coordination."
---

# CC Orchestration Skill

Command Center orchestration expertise for HnVue team coordination.

## DISPATCH Lifecycle Management

### Creation Checklist

Before creating any DISPATCH file:

1. Read `_CURRENT.md` to understand current round status
2. Verify all teams from previous round are MERGED/IDLE
3. Identify gaps: coverage, SPEC progress, blocked items
4. Map gaps to team ownership (role-matrix.md)
5. Create Gitea issue for the round (label: `cc-round`)
6. Create DISPATCH files per team using STANDARD-DISPATCH.md template
7. Update `_CURRENT.md` with new round entries
8. Git commit with prefix `dispatch:` and push to main

### DISPATCH File Naming

```
DISPATCH-S{Sprint}-R{Round}-{TEAM}.md
Example: DISPATCH-S17-R1-TEAM-A.md
```

### Round Progression

```
Round N completion (all MERGED/IDLE)
    ↓
CC: Gap analysis (read specs, coverage, previous DISPATCH results)
    ↓
CC: Plan Round N+1 tasks per team
    ↓
CC: Create DISPATCH files + _CURRENT.md update
    ↓
CC: git push origin main (dispatch: commit prefix)
    ↓
CC: Create/update Gitea issues
    ↓
Teams: ScheduleWakeup detects new round → work begins
```

## Gap Analysis Framework

### Sources for Gap Identification

| Source | Location | What to Check |
|--------|----------|---------------|
| SPEC Status | `.moai/specs/*/` | Incomplete specs, TODO tasks |
| Coverage Report | QA DISPATCH evidence | Modules below 85%/90% threshold |
| DISPATCH History | `.moai/dispatches/completed/` | BLOCKED items, PARTIAL completions |
| RTM Gaps | docs/verification/ | Missing SWR→TC mappings |
| SBOM Updates | docs/regulatory/ | New/changed dependencies |
| FMEA Items | docs/risk/ | Unmitigated risks |

### Priority Assignment

| Priority | Criteria | Label |
|----------|----------|-------|
| P0-Blocker | Build fails, safety-critical regression | `priority-critical` |
| P1-High | Coverage below gate, SPEC blocker | `priority-high` |
| P2-Medium | Feature implementation, documentation | `priority-medium` |
| P3-Low | Cleanup, minor improvements | `priority-low` |

## PR Evaluation Criteria

### Pre-PR Checklist

Before creating a PR for team completion:

```bash
# 1. Verify team branch has commits beyond main
git log origin/main..origin/team/{team} --oneline

# 2. Check file ownership
git diff --name-only origin/main..origin/team/{team}

# 3. Verify against team ownership (role-matrix.md §2)
# Team A: HnVue.Common, Data, Security, SystemAdmin, Update
# Team B: Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning
# Coordinator: UI.Contracts, UI.ViewModels, App
# Design: UI/Views, Styles, Themes, Components, Converters, Assets
# QA: CI/CD scripts, coverage config, quality reports
# RA: docs/ regulatory, planning, risk, verification, management
```

### PR Creation Command

```bash
bash scripts/issue/gitea-api.sh pr-create \
  "S{Sprint}-R{Round} {Team}: {summary}" \
  "## Summary
{bullet points}

## Evidence
- Build: {result}
- Tests: {result}
- Files changed: {count}

## Issue Tracking
- Closes #{issue_number}

CC Evaluation: {PASS/CONDITIONAL PASS/FAIL}" \
  "team/{team}" \
  "main"
```

## Issue Management

### Round Issue Template

```
Title: [S{N}-R{M}] Round Coordination
Labels: cc-round, sprint-{N}
Body:
## Round S{N}-R{M} Status

| Team | DISPATCH | Status | Issue |
|------|----------|--------|-------|
| Team A | DISPATCH-S{N}-R{M}-TEAM-A | {status} | #{issue} |
| ... | ... | ... | ... |

## Exit Criteria
{from _CURRENT.md}

## Progress Log
{CC adds comments as teams progress}
```

### Team Task Issue Template

```
Title: [S{N}-R{M}] {Team}: {task title}
Labels: team-{x}, dispatch-ref
Body:
## DISPATCH Reference
DISPATCH-S{N}-R{M}-{TEAM}.md

## Tasks
{from DISPATCH Tasks section}

## Acceptance Criteria
{from DISPATCH completion conditions}

## Comments
{Team and CC add progress comments}
```

## Monitoring Dashboard

CC maintains a mental dashboard updated each cycle:

```
┌──────────────────────────────────────────┐
│ Round S{N}-R{M} Dashboard                │
├──────────┬──────────┬────────────────────┤
│ Team     │ Status   │ Last Update        │
├──────────┼──────────┼────────────────────┤
│ Team A   │ ■ DONE   │ HH:MM  │
│ Team B   │ ▶ ACTIVE │ HH:MM  │
│ Coord    │ ◻ IDLE   │ -      │
│ Design   │ ▶ ACTIVE │ HH:MM  │
│ QA       │ ◻ IDLE   │ -      │
│ RA       │ ◻ IDLE   │ -      │
├──────────┼──────────┼────────────────────┤
│ Progress │ 2/6 (33%)│ PR: 1 pending      │
└──────────┴──────────┴────────────────────┘
```

## Error Recovery Procedures

### DISPATCH Push Conflict

```
git pull --rebase origin main
# If conflict in DISPATCH files only:
#   Resolve conflict (keep both changes if different teams)
#   git add, git rebase --continue
# If conflict in other files:
#   STOP — report to user via issue
```

### Team TIMEOUT Handling

```
# After 60 minutes of no DISPATCH status update:
1. Comment on round tracking issue: "Team {X} TIMEOUT — no response for 60 minutes"
2. Do NOT create PR for this team
3. Continue monitoring other teams
4. Report to user: "3회 연속 TIMEOUT 시 환경 점검 필요"
```

### False Completion Detection

```
# Signs of false COMPLETED:
# - No build evidence in DISPATCH Status
# - Missing timestamp in status table
# - git diff shows files outside team ownership
# - Test count is 0 or suspiciously low

Response:
1. Do NOT create PR
2. Comment on issue: "COMPLETED 검증 실패 — {reason}"
3. Wait for team to update with valid evidence
```

## ScheduleWakeup Management

- CC polling interval: 600 seconds (10 minutes)
- Minimum: 300 seconds (5 minutes)
- After DISPATCH push: continue monitoring (no sleep)
- After all teams MERGED + PRs created: gap analysis → next round
- Hardcoded delays PROHIBITED — read from configuration
