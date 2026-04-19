# HnVue Development Operations Guidelines

Version: 3.0.0
Last Updated: 2026-04-19
Classification: HARD (All teams + Commander Center must comply)

---

## 0. Project Philosophy [CONSTITUTIONAL]

> **"Quality over speed. Correct completion over fast completion."**

The #1 goal of this project is **maximum code quality and deliverable completeness**.

| Principle | Description |
|-----------|-------------|
| **Completeness first** | 3 tasks at 100% > 10 tasks at 80% |
| **Self-verification required** | Prove "0 errors" — don't assume |
| **No false reports** | Unverified COMPLETED = protocol violation |
| **Scope compliance** | Only do what DISPATCH instructs |
| **Evidence-based** | All completion claims include build logs and test results |

This philosophy applies equally to all teams and Commander Center.

### Quality Standards (All Teams)

| Metric | Minimum | Safety-Critical Modules |
|--------|---------|------------------------|
| Build | 0 errors | 0 errors, 0 warnings |
| Tests | All pass | All pass |
| Coverage | 85% | 90%+ |

---

## 0.5 Commander Center Role Boundaries [HARD]

CC is the **commander**. A commander does not fire the gun on the battlefield.

### CC Allowed Actions (DO)

| Action | Description |
|--------|-------------|
| Planning | Sprint analysis, SPEC review, priority decisions |
| DISPATCH writing | Write instructions → commit to main → push |
| Monitoring | git log/diff/status read-only |
| Verification | Check team build evidence, judge acceptance criteria |
| PR management | Create PR, review, merge |
| Result consolidation | Collect team reports, update project state |

### CC Forbidden Actions (NEVER) [HARD]

| Forbidden | Correct Response |
|-----------|-----------------|
| Direct code modification (.cs/.xaml/.csproj) | Issue correction DISPATCH |
| Agent invocation for implementation | Instruct team to implement |
| Running dotnet build/test directly | Check team build evidence |
| Fixing team errors directly | Issue DISPATCH with error details |

**Principle: If a team fails, re-dispatch — never do it for them.**

---

## 1. DISPATCH Lifecycle

### 1.1 Issuance Rules (Commander Center)

- Reference `.claude/rules/teams/{team}.md` when issuing DISPATCH
- Required sections: team role confirmation, Constraints, Git Completion Protocol
- Specify constraints derived from team rules
- Follow DISPATCH schema: `.claude/rules/moai/workflow/dispatch-schema.md`

### 1.2 Execution Rules (Each Team)

- Follow Task order (P0 -> P1 -> P2 -> P3)
- Update checkboxes only after verification
- Record only actual state in Status (no false reports)
- If cross-team file modification needed, create issue immediately

### 1.3 Completion Rules (Each Team) [HARD]

Before reporting COMPLETED:

1. Own module build success (`dotnet build` or MSBuild) -- **HARD**
2. Own tests all pass -- **HARD**
3. Full solution build attempt -- **SHOULD** (report other team errors in notes)
4. Copy last line of build output to Status -- **HARD**

COMPLETED without build evidence = treated as false report.

---

## 2. Git Completion Protocol [HARD]

### 2.1 Post-completion Procedure

1. `git add` (DISPATCH.md + changed files, exclude secrets)
2. `git commit` (conventional commit format)
3. `git push origin team/{team-name}`
4. **DO NOT create PR** — PR creation is Commander Center exclusive

### 2.2 PR Creation Rules [HARD]

- **Only Commander Center creates PRs** — team self-PR creation forbidden
- CC creates PR only after REVIEW (Phase 4) PASS judgment
- Push failure: report "PUSH_FAILED" status (no deadlock)
- Commit message derived from DISPATCH title

### 2.3 .gitignore Policy

Never commit development environment artifacts:

| Pattern | Description |
|---------|-------------|
| `temp_ppt_extract/` | PPT extraction temp files |
| `.dotnet-home/` | NuGet cache (2068+ files) |
| `tmp/` | Temp working directory |
| `_workspace*/` | Worktree working directories |
| `*.user` | VS user settings |
| `bin/`, `obj/` | Build output |

When introducing new tools, add their artifact patterns to `.gitignore` immediately.

---

## 3. Commander Center Integration Verification [HARD]

### 3.1 Distributed Verification + Central Collection

- Each team self-verifies build → records results in Status
- CC only checks Status (does NOT build directly)
- All PASS → decide merge order → sequential merge
- Any FAIL → issue correction DISPATCH to that team only

### 3.2 Merge Order

| Order | Team | Reason |
|-------|------|--------|
| 1 | QA/RA | No source changes, safe |
| 2 | Design | UI only, low cross-module dependency |
| 3 | Team A | Infrastructure (Common, Data, Security) |
| 4 | Team B | Medical (Dicom, Detector, Dose, etc.) |
| 5 | Coordinator | Integration (App, ViewModels, Contracts), last |

### 3.3 Pre-merge Checklist

- [ ] All 6 team DISPATCH Status confirmed COMPLETED
- [ ] Each team build evidence verified (build output in Status)
- [ ] PR mergeable state confirmed
- [ ] Conflicting teams rebased and re-pushed
- [ ] Post-merge full solution build verification

---

## 3.5 CC Monitoring Protocol [HARD]

CC monitors team progress via **read-only** git operations. CC does NOT build, test, or modify team code.

### Monitoring Interval: 10 minutes [HARD — S13-R2]

CC monitors every **10 minutes** (reduced from 15 min based on S13-R1 analysis).

### 3-Team Status Detection (3-Layer)

| Layer | Method | Meaning |
|-------|--------|---------|
| Remote commits | `git log origin/team/$team --not main` | Pushed work (confirmed) |
| Worktree local | `.worktrees/{team}/ git status` | In-progress work (uncommitted) |
| DISPATCH Status | DISPATCH file Status table | Team self-reported state |

---

## 3.6 Sequential Scheduling v1.0 [HARD — S13-R2]

> **S13-R1 Lesson**: All teams starting simultaneously ignores dependencies → Coordinator integrates without A/B code. Sequential scheduling aligns start times with dependency readiness.

### Phase Structure

```
Phase 1 (simultaneous):  Team A ──┐
                            Team B ──┤
                                     ↓
Phase 2 (after A+B):     Coordinator ──┐
                                          ↓
Phase 3 (after CO):          QA ──┐
                                     ↓
Phase 4 (after QA):              RA

Separate Track (parallel):  Design (independent, no dependencies)
```

### Phase Start Conditions [HARD]

| Phase | Team | Start Condition | CC Action |
|-------|------|----------------|-----------|
| Phase 1 | Team A, Team B | DISPATCH published | Immediate ACTIVE |
| Phase 2 | Coordinator | Team A **AND** Team B MERGED | CC sets DISPATCH Status → ACTIVE |
| Phase 3 | QA | Coordinator MERGED | CC sets DISPATCH Status → ACTIVE |
| Phase 4 | RA | QA MERGED | CC sets DISPATCH Status → ACTIVE |
| Separate | Design | DISPATCH published | Immediate ACTIVE, independent |

### Phase Transition [HARD]

When Phase N teams complete:

1. CC merges COMPLETED team(s)
2. CC edits next Phase team's DISPATCH: `NOT_STARTED` → `ACTIVE`
3. CC updates `_CURRENT.md`: `QUEUED` → `ACTIVE`
4. Single commit + push (merge + phase transition in one transaction)

### QUEUED Team Rules [HARD]

- QUEUED teams may read their DISPATCH but **must not start implementation**
- Only CC can transition QUEUED → ACTIVE
- Design is never QUEUED — always immediate ACTIVE

---

## 3.7 Team Reporting Protocol [HARD — S13-R2]

### 15-Minute Reporting Interval

Teams update DISPATCH Status every **15 minutes**:

| Event | Required Action |
|-------|----------------|
| DISPATCH read | NOT_STARTED → IN_PROGRESS (immediate push) |
| Work in progress | Progress notes update (optional) |
| Work completed | IN_PROGRESS → COMPLETED + build evidence (immediate push) |
| Work blocked | NOT_STARTED → BLOCKED + reason (immediate push) |

**No status update = communication breakdown = protocol violation**

| Target | Command | Detects |
|--------|---------|---------|
| New commits | `git log team/{team} --oneline -5` | Completed work |
| Uncommitted changes | `git -C .worktrees/{team} diff --name-only HEAD` | Work in progress |
| New files | `git -C .worktrees/{team} status --porcelain` | Untracked reports |
| DISPATCH status | Read `.worktrees/{team}/DISPATCH.md` Status section | Completion state |

### Anomaly Detection

| Situation | Judgment | CC Action |
|-----------|---------|-----------|
| Status=COMPLETED + uncommitted | DONE_UNCOMMITTED | Instruct commit+push |
| 20min+ no change (commit + uncommitted) | STALLED | Check status or re-dispatch |
| Acceptance unmet + COMPLETED | FALSE_REPORT | Issue correction DISPATCH |
| Status=BLOCKED | BLOCKED | Resolve dependency, re-dispatch |
| Report file exists + uncommitted | REPORT_UNCOMMITTED | Instruct commit+push |

### [HARD] Monitor both committed AND uncommitted changes

S04 R1 lesson: Team completed work and updated DISPATCH.md but did NOT commit.
Git log monitoring missed it. Always check `git diff` and `git status` alongside `git log`.

---

## 4. Team Role Boundaries [HARD]

### 4.1 Ownership Rules

- Each team modifies only files within their owned modules
- Cross-team file modification: create issue + notify target team
- UI.Contracts interfaces: Coordinator is SOLE modifier

Module ownership:

| Team | Owned Modules |
|------|--------------|
| Team A | Common, Data, Security, SystemAdmin, Update |
| Team B | Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning |
| Design | UI/Views, Styles, Themes, Components, Converters, Assets, DesignTime |
| Coordinator | UI.Contracts, UI.ViewModels, App |
| QA | .github/workflows, scripts/ci, scripts/qa, TestReports |
| RA | docs/regulatory, docs/planning, docs/risk, docs/verification, scripts/ra |

### 4.2 Cross-dependency Protocol

| Change Type | Required Action |
|-------------|----------------|
| Common interface change | `breaking-change` issue + notify Coordinator |
| NuGet package change | `soup-update` issue + notify RA |
| Safety-critical source modification | Characterization test first + RA risk assessment |
| Workflow state change | RA RTM update issue |
| UI.Contracts interface addition | Coordinator only, impact analysis required |
| DB schema change | Notify Team A + Coordinator |

---

## 5. Incident History and Lessons

### 5.1 Round 1 Incident (2026-04-10)

| Team | Violation | Root Cause | Prevention |
|------|-----------|-----------|------------|
| Team B | False COMPLETED report (0/16 checkboxes) | State change without build verification | Completion Rule 1.3 HARD |
| Design | 14MB polluted commit (temp_ppt_extract/ + .dotnet-home/) | Missing .gitignore | .gitignore Policy 2.3 |
| Team A | Constructor mismatch undetected | Module-only build, no full solution build | Full solution build SHOULD |
| 4 teams | No push, no PR created | Missing git completion procedure | Git Protocol 2.1 HARD |
| Commander | DISPATCH issued without team rules | No DISPATCH schema | Issuance Rule 1.1 + schema |
| Commander | Accepted merge without build verification | Trust-based acceptance | Integration Verification 3.1 HARD |

### 5.2 Commander Center Self-verification Failure (2026-04-10)

| Claim | Reality |
|-------|---------|
| "15/15 PASS" | Design DISPATCH contaminated with Team B content, 5 teams missing required sections |
| Root cause: `grep -c` keyword existence check only | Did not actually read file contents |

Same pattern as Team B false report: completion claim without verification.

### 5.3 Commander Center Self-verification Rules [HARD]

Before CC reports "PASS/complete/verified":

1. **Read verification**: Read actual file content, not just grep count -- HARD
2. **Title match**: Verify each team DISPATCH title matches target team via head -1 -- HARD
3. **Section existence**: Confirm required sections via grep -n (position, not just presence) -- HARD
4. **Cross-reference**: 1:1 comparison of schema-required fields against actual DISPATCH -- HARD
5. **Self-doubt**: "All PASS" is a warning signal. Read at least 2 files in full -- HARD

On violation: Report "unverified items exist" to user. Only report PASS for verified items.

### 5.4 Lessons Summary

1. **COMPLETED without build evidence = false report** — most frequent and critical violation
2. **.gitignore must be updated when introducing tools** — post-hoc cleanup is expensive
3. **Full solution build is the only way to detect cross-dependency errors**
4. **Git push + PR is the final completion act** ��� commit alone is incomplete
5. **Commander Center is a verifier, not a truster** — evidence-based acceptance

---

## 6. Reference Documents

| Document | Path |
|----------|------|
| Team rules | `.claude/rules/teams/{team}.md` |
| DISPATCH schema | `.claude/rules/moai/workflow/dispatch-schema.md` |
| Agent definitions | `.claude/agents/hnvue/` |
| CI/CD | `.github/workflows/desktop-ci.yml` |
| Operations Protocol v2.0 | `.moai/dispatches/templates/operations-protocol.md` |
| Operations Strategy | `docs/OPERATIONS.md` |
| Git Workflow | `docs/development/git-workflow.md` |
