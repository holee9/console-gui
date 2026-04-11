---
name: hnvue-skill-qa
description: >
  HnVue QA engineering skill. Encodes coverage analysis (Cobertura, 85%/90% gates), Stryker.NET
  mutation testing for safety-critical modules, NetArchTest architecture boundary enforcement,
  FlaUI UI automation, CI/CD pipeline management, and release readiness reporting.
  Loaded by hnvue-qa agent. Triggers on: coverage, test, quality, architecture, mutation, release, CI/CD, build.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, qa, coverage, mutation, architecture, flaui, ci-cd, release"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["coverage", "test", "quality", "architecture", "mutation", "stryker", "stylecop", "sonarcloud", "release", "ci", "build", "flaui"]
  agents: ["hnvue-qa"]
---

# HnVue QA Engineering Skill

Senior-level domain knowledge for HnVue quality assurance infrastructure.

## 1. Quality Standards (Single Source of Truth)

From team-common.md — ALL metrics defined here only:

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

## 2. Coverage Analysis

**Tool:** Coverlet (Cobertura + JSON output)
**Config:** coverage.runsettings
**Exclusions:** DesignTime/, Migrations/, generated code

**Script:** `scripts/qa/Invoke-LocalAnalysis.ps1`
- Modes: Build, Test, Coverage, Security
- Output: Cobertura XML + JSON summary

**Safety-critical modules requiring 90%+:**
- HnVue.Dose (dose interlock logic)
- HnVue.Incident (incident reporting)
- HnVue.Security (authentication, audit chain)
- HnVue.Update (staged update, signature verification)

**Standard modules requiring 85%+:**
- All other modules

## 3. Mutation Testing (Stryker.NET)

**Config:** stryker-config.json
**Targets:** Dose, Incident, Security, Update (safety-critical only)
**Thresholds:** high=80, low=70, break=60

**Script:** `scripts/qa/Invoke-MutationTest.ps1`

**DesignTime/ directory excluded from mutation testing.**

**Interpretation:**
- Mutation score < 70%: issue with `qa-result` + `priority-medium` labels
- Surviving mutants indicate weak assertions or missing edge case tests

## 4. Architecture Tests (NetArchTest)

**Project:** tests/HnVue.Architecture.Tests/
**File:** UILayerArchitectureTests.cs

**Three validation strategies:**
1. Assembly scanning via NetArchTest.Rules (UI.Contracts, UI.ViewModels)
2. csproj XML parsing for HnVue.UI (avoids XAML compilation issues)
3. Dependency validation: UI forbidden from referencing business modules

**Boundary rules enforced:**
- HnVue.UI must NOT reference: Dose, Incident, Dicom, Workflow, PatientManagement, Security, Data
- HnVue.UI must use interfaces from HnVue.UI.Contracts only
- Architecture rule violations = PR blocked

## 5. UI Automation (FlaUI)

**Project:** tests/UI/HnVue.UI.QA.Tests.csproj
**Framework:** FlaUI v4.0 (UIA3)

**Test files:**
- AccessibilityTests.cs: WCAG 2.2 AA (contrast, keyboard nav)
- DesignSystemTests.cs: Visual design consistency
- PerformanceTests.cs: Performance benchmarks
- PptPage1LoginDesignTests.cs: PPT slide 1 validation
- PptPage2WorklistDesignTests.cs: PPT slide 2 validation
- VisualRegressionTests.cs + ScreenshotCapture.cs

## 6. CI/CD Pipelines

**desktop-ci.yml (main pipeline):**
restore -> build -> unit tests -> integration tests -> coverage -> artifact upload (45min timeout)

**qa-issue-reporter.yml:** Creates Gitea issues on CI failure
**security-scan.yml:** OWASP Dependency-Check on Directory.Packages.props changes (CVSS >= 7.0 fails)

**Script:** `scripts/ci/Invoke-DesktopCi.ps1`
- Orchestrates restore/build/test on Windows
- Outputs .trx results

## 7. Static Analysis

**Tools (from Directory.Build.props):**
- StyleCop Analyzers: code style enforcement (.stylecop.json)
- Roslynator: code quality
- SecurityCodeScan: SAST for common vulnerabilities
- .editorconfig: naming conventions (authoritative)

## 8. Release Readiness Report

**Script:** `scripts/qa/Generate-ReleaseReport.ps1`
**Output:** TestReports/RELEASE_READY_{date}.html
**Aligned with:** DOC-034 release checklist (10 items)

**ALL blocking items must Pass for release authorization.**
**4-signature gate (DOC-034 section 5):** SW Dev Lead -> QA Lead -> RA/QA Manager -> PM

## 9. Code Review Policy

- QA team is REQUIRED reviewer for ALL PRs (via CODEOWNERS)
- PR checklist must be completed before merge
- Architecture rule violations = PR blocked

## 10. Issue Protocol

- Coverage < 85%: issue with `qa-result` + `priority-high`
- Mutation score < 70%: issue with `qa-result` + `priority-medium`
- Release report: issue with `qa-result`, post summary
- Security vulnerability: Gitea-only issue with `security` label (private)
- OWASP results feed into RA SBOM update pipeline

## 11. Test Project Inventory (15 unit + 1 integration + 1 UI)

Unit: Common, Data, Security, Dicom, Detector, Dose, Imaging, Incident, Workflow, PatientManagement, CDBurning, SystemAdmin, Update, UI, Architecture
Integration: HnVue.IntegrationTests
UI Automation: HnVue.UI.QA.Tests

## 12. Quality Enforcement Protocol [HARD]

Before running any analysis, read `${CLAUDE_SKILL_DIR}/references/qa-patterns.md` for:
- Coverage analysis step-by-step procedure (4 steps)
- Mutation test result interpretation guide (surviving mutant categories)
- Issue creation templates (exact format for coverage/mutation/security)
- Release readiness 10-item checklist with pass criteria

**Analysis flow:**
1. Read references/qa-patterns.md Pre-Analysis Checklist
2. Run analysis in correct order (build first, then tests, then coverage)
3. Evaluate results against thresholds from Section 1 above
4. Create issues using templates from references for any violations
5. Only report COMPLETED with all analysis results documented
