# QA Implementation Quality Guide

QA agent reads this file when running coverage analysis, architecture tests, mutation testing, or release readiness checks.

## Pre-Analysis Checklist

Before running any QA analysis:

1. Ensure solution builds cleanly: `dotnet build HnVue.sln -c Release`
2. Verify coverage.runsettings exists and excludes DesignTime/Migrations
3. Check stryker-config.json targets only safety-critical modules
4. Confirm all test projects are referenced in solution

## Coverage Analysis Procedure

### Step 1: Run tests with coverage

```bash
dotnet test HnVue.sln --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory TestReports/coverage -c Release --verbosity normal
```

### Step 2: Generate report

```bash
# If reportgenerator is installed:
reportgenerator -reports:"TestReports/coverage/**/coverage.cobertura.xml" -targetdir:"TestReports/coveragereport" -reporttypes:"Html;TextSummary"
```

### Step 3: Evaluate against thresholds

| Module | Minimum | Classification |
|--------|---------|---------------|
| HnVue.Dose | 90% | Safety-Critical |
| HnVue.Incident | 90% | Safety-Critical |
| HnVue.Security | 90% | Safety-Critical |
| HnVue.Update | 90% | Safety-Critical |
| All others | 85% | Standard |

### Step 4: Identify gaps

For each module below threshold:
1. Find uncovered methods: look for 0% branch coverage in Cobertura XML
2. Categorize gaps: missing happy path, missing error path, missing edge case
3. Create issue with `qa-result` label and specific file:line references

## Architecture Test Validation

### Running architecture tests

```bash
dotnet test tests/HnVue.Architecture.Tests/ --verbosity normal
```

### What architecture tests verify:

1. **UI dependency boundaries**: HnVue.UI must not reference business modules
2. **UI.Contracts independence**: must not reference Data/Security internals
3. **ViewModel composition**: must use UI.Contracts interfaces only

### When architecture tests fail:

1. Identify the violating reference (assembly or namespace)
2. Determine if it's a new dependency or a refactoring regression
3. Report to responsible team with exact violation
4. PR BLOCKED until resolved

## Mutation Testing (Safety-Critical Only)

### Running Stryker.NET

```bash
# For specific module
dotnet stryker --config-file stryker-config.json

# Modules covered by stryker-config.json:
# - HnVue.Dose (dose interlock logic)
# - HnVue.Incident (incident reporting)
# - HnVue.Security (authentication, audit)
# - HnVue.Update (staged updates)
```

### Interpreting Results

**Mutation score thresholds:**
- high >= 80: Good — most mutants killed
- low >= 70: Acceptable minimum
- break >= 60: Build failure threshold

**Surviving mutant categories:**

| Mutant Type | What It Means | Action |
|-------------|--------------|--------|
| Boundary mutation survived | <= changed to < not detected | Add boundary value test |
| Conditional mutation survived | if(x) changed to if(!x) not detected | Add inverse condition test |
| Return value mutation survived | Success changed to Failure not detected | Add explicit return assertion |
| Arithmetic mutation survived | + changed to - not detected | Add calculation verification test |

### Common False Positives

- Logging-only mutations (changing log message doesn't affect behavior)
- Exception message mutations (cosmetic, not functional)
- DesignTime code mutations (excluded in config)

## Release Readiness Assessment

### 10-Item Checklist (DOC-034)

1. [ ] All unit tests pass
2. [ ] All integration tests pass
3. [ ] Coverage >= 85% overall, >= 90% safety-critical
4. [ ] Architecture tests pass (no dependency violations)
5. [ ] Stryker mutation score >= 70% for safety-critical
6. [ ] SonarCloud: 0 bugs, 0 vulnerabilities, < 50 code smells
7. [ ] OWASP: no CVSS >= 7.0 vulnerabilities
8. [ ] SBOM current (DOC-019)
9. [ ] RTM complete (DOC-032)
10. [ ] Known Anomalies documented (DOC-044)

### Generating Release Report

```bash
powershell -File scripts/qa/Generate-ReleaseReport.ps1
```

Output: `TestReports/RELEASE_READY_{date}.html`

### 4-Signature Gate

ALL blocking items must Pass. Report goes through:
1. SW Dev Lead sign-off
2. QA Lead sign-off
3. RA/QA Manager sign-off
4. PM final approval

## CI Pipeline Validation

### desktop-ci.yml verification

When CI fails:
1. Check which step failed (restore/build/test/coverage)
2. For build failures: identify breaking commit
3. For test failures: check if new tests or regression
4. For coverage drops: identify which module dropped
5. Create Gitea issue via qa-issue-reporter.yml

### Security scan (security-scan.yml)

Triggers on Directory.Packages.props changes:
1. OWASP Dependency-Check runs
2. CVSS >= 7.0 triggers build failure
3. Results feed into RA SBOM update pipeline

## Issue Creation Protocol

### Templates

**Coverage gap:**
```
Title: [QA] {Module} coverage below {target}%: current {actual}%
Labels: qa-result, priority-high
Body: 
- Module: {module name}
- Current coverage: {actual}%
- Target: {target}%
- Gap areas: {list uncovered methods with file:line}
- Recommended tests: {specific test descriptions}
```

**Mutation score gap:**
```
Title: [QA] {Module} mutation score below 70%: current {actual}%
Labels: qa-result, priority-medium
Body:
- Module: {module name}
- Current score: {actual}%
- Surviving mutants: {count}
- Top surviving categories: {boundary/conditional/return}
- Recommended improvements: {specific test additions}
```

**Security vulnerability:**
```
Title: [SECURITY] {component} CVSS {score}: {CVE-ID}
Labels: security, priority-critical
Body: (Gitea-only, not public)
- Component: {name} {version}
- CVE: {ID}
- CVSS: {score}
- Impact: {description}
- Remediation: {upgrade path or mitigation}
```

## Post-Analysis Verification

Before reporting QA analysis as COMPLETED:

1. All analysis scripts ran without errors
2. Results are saved in TestReports/
3. All modules below threshold have issues created
4. Architecture test results documented
5. DISPATCH.md Status updated with evidence
