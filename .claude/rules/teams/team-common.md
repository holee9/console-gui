# Team Common Rules [ALL TEAMS]

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

## Team Obligations [HARD]

- Read DISPATCH completely before starting work
- Verify acceptance criteria for each Task
- Stay within file ownership scope
- Report BLOCKED honestly when stuck (do not guess)
- PR creation is FORBIDDEN (Commander Center exclusive)
