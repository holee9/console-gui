<#
.SYNOPSIS
    Generates a team-specific CLAUDE.md for a worktree.
.PARAMETER TeamName
    The team identifier
.DESCRIPTION
    Creates a lean CLAUDE.md at the current directory root that loads only relevant
    skills and rules for the specified team. This reduces context token usage by ~60%.
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet('team-a','team-b','design','coordinator','qa','ra')]
    [string]$TeamName
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$templates = @{
    'team-a' = @{
        title = "Team A — Infrastructure & Foundation"
        modules = "HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update"
        rules = @("@.claude/rules/moai/languages/csharp.md", "@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/team-a.md")
        skills = "moai-lang-csharp, moai-domain-database, moai-platform-auth"
        focus = "EF Core migrations, SQLCipher encryption, Repository pattern, bcrypt/JWT security, NuGet central management"
    }
    'team-b' = @{
        title = "Team B — Medical Imaging Pipeline"
        modules = "HnVue.Dicom, HnVue.Detector, HnVue.Imaging, HnVue.Dose, HnVue.Incident, HnVue.Workflow, HnVue.PatientManagement, HnVue.CDBurning"
        rules = @("@.claude/rules/moai/languages/csharp.md", "@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/team-b.md")
        skills = "moai-lang-csharp, moai-foundation-quality"
        focus = "DICOM fo-dicom 5.1.3, FPD Detector SDK, Workflow 9-state machine, Dose 4-level interlock, Safety-critical 90%+ coverage"
    }
    'design' = @{
        title = "Team Design — Pure UI Design"
        modules = "HnVue.UI (Views, Styles, Themes, Components, Converters, Assets, DesignTime)"
        rules = @("@.claude/rules/moai/languages/csharp.md", "@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/team-design.md")
        skills = "moai-domain-uiux, moai-domain-frontend, moai-lang-csharp"
        focus = "WPF XAML, MahApps.Metro themes, DesignTime Mock ViewModels, WCAG 2.1 AA accessibility, IEC 62366 usability"
    }
    'coordinator' = @{
        title = "Coordinator — Integration & UI Contracts"
        modules = "HnVue.UI.Contracts, HnVue.UI.ViewModels, HnVue.App (DI root), tests.integration"
        rules = @("@.claude/rules/moai/languages/csharp.md", "@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/coordinator.md")
        skills = "moai-lang-csharp, moai-ref-api-patterns"
        focus = "UI.Contracts interface management, DI registration, ViewModel composition, Integration testing, Cross-team PR review"
    }
    'qa' = @{
        title = "QA Team — Quality Assurance"
        modules = ".github/workflows, scripts/ci, scripts/qa, TestReports, docs/testing"
        rules = @("@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/qa.md")
        skills = "moai-foundation-quality, moai-workflow-testing, moai-ref-owasp-checklist"
        focus = "SonarCloud, OWASP Dependency-Check, Stryker.NET mutation testing, Coverage gates (85%/90%), Release readiness DOC-034"
    }
    'ra' = @{
        title = "RA Team — Regulatory Affairs"
        modules = "docs/regulatory, docs/planning, docs/risk, docs/verification, docs/management, scripts/ra"
        rules = @("@.claude/rules/moai/core/moai-constitution.md", "@.claude/rules/teams/ra.md")
        skills = "moai-workflow-jit-docs"
        focus = "IEC 62304, FDA 510(k), CE MDR, KFDA, SBOM CycloneDX, RTM traceability, DOC-042 CMP completion"
    }
}

$t = $templates[$TeamName]
$rulesImports = ($t.rules | ForEach-Object { $_ }) -join "`n"

$claudeContent = @"
# $($t.title)

$rulesImports

@.moai/config/sections/language.yaml

## Owned Modules
$($t.modules)

## Loaded Skills
$($t.skills)

## Focus Areas
$($t.focus)

## Key Rules
- All user-facing responses in Korean (conversation_language: ko)
- Code comments in English
- Conventional commits with scope required
- Create Gitea/GitHub issues for all work (Korean UTF-8 safe)
- See docs/OPERATIONS.md for full team operations guide
"@

$outputPath = Join-Path (Get-Location) "CLAUDE.md"
$claudeContent | Out-File $outputPath -Encoding utf8
Write-Host "Team CLAUDE.md created: $outputPath" -ForegroundColor Green
Write-Host "Team: $($t.title)" -ForegroundColor Cyan
