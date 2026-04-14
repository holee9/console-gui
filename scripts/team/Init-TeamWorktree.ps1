<#
.SYNOPSIS
    Initializes team-specific CLAUDE.md and team-context.md files in each worktree.

.DESCRIPTION
    Creates CLAUDE.md (team-scoped project instructions) and .claude/rules/team-context.md
    (team identity context) for each team worktree under .worktrees/{team-name}/.
    Can target a single team or all teams.

.PARAMETER Team
    Optional. Name of a single team to initialize. If omitted, all teams are processed.

.EXAMPLE
    .\Init-TeamWorktree.ps1
    # Initializes all 6 team worktrees

.EXAMPLE
    .\Init-TeamWorktree.ps1 -Team team-a
    # Initializes only the team-a worktree
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("coordinator", "qa", "ra", "team-a", "team-b", "team-design")]
    [string]$Team
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
if (-not $ProjectRoot) {
    $ProjectRoot = (Get-Location).Path
}
$WorktreeRoot = Join-Path $ProjectRoot ".worktrees"

# Team definitions
$Teams = @{
    "coordinator" = @{
        DisplayName   = "Coordinator (Integration & UI Contracts)"
        Role          = "Integration gate, DI composition root, ViewModel composition, cross-module contract management"
        Modules       = @("HnVue.UI.Contracts", "HnVue.UI.ViewModels", "HnVue.App", "tests.integration/HnVue.IntegrationTests")
        FocusAreas    = @(
            "UI.Contracts interface integrity and backward compatibility",
            "DI registration completeness in App.xaml.cs",
            "ViewModel composition through constructor injection only",
            "Integration test coverage for all cross-module interactions",
            "PR review for any UI.Contracts, UI.ViewModels, or App changes"
        )
        FileOwnership = @(
            "src/HnVue.UI.Contracts/**",
            "src/HnVue.UI.ViewModels/**",
            "src/HnVue.App/**",
            "tests.integration/HnVue.IntegrationTests/**"
        )
    }
    "qa" = @{
        DisplayName   = "QA (Quality Assurance)"
        Role          = "CI/CD pipelines, coverage gates, mutation testing, OWASP scanning, release readiness"
        Modules       = @(".github/workflows/", "scripts/ci/", "scripts/qa/", "TestReports/")
        FocusAreas    = @(
            "SonarCloud quality gate enforcement (Bug=0, Vuln=0, Coverage>=85%)",
            "Stryker.NET mutation testing for safety-critical modules (>=70%)",
            "OWASP dependency scanning with CVSS>=7.0 build failure policy",
            "Release readiness report generation (DOC-034 aligned)",
            "Architecture rule enforcement via NetArchTest"
        )
        FileOwnership = @(
            ".github/workflows/**",
            "scripts/ci/**",
            "scripts/qa/**",
            "coverage.runsettings",
            "stryker-config.json",
            ".stylecop.json",
            "CODEOWNERS",
            ".github/pull_request_template.md",
            "TestReports/**"
        )
    }
    "ra" = @{
        DisplayName   = "RA (Regulatory Affairs)"
        Role          = "IEC 62304 document management, RTM tracing, SBOM maintenance, FDA/CE/KFDA compliance"
        Modules       = @("docs/regulatory/", "docs/planning/", "docs/risk/", "docs/verification/", "docs/management/", "scripts/ra/")
        FocusAreas    = @(
            "IEC 62304 document versioning and traceability (RTM DOC-032)",
            "SBOM management (CycloneDX 1.5, DOC-019) with 42 components",
            "Requirements hierarchy: MR -> PR -> SWR -> TC mapping",
            "DOC-042 CMP completion (priority: currently Draft)",
            "Implementation-to-document change mapping enforcement"
        )
        FileOwnership = @(
            "docs/regulatory/**",
            "docs/planning/**",
            "docs/risk/**",
            "docs/verification/**",
            "docs/management/**",
            "scripts/ra/**",
            "docfx.json",
            "CHANGELOG.md"
        )
    }
    "team-a" = @{
        DisplayName   = "Team A (Infrastructure & Foundation)"
        Role          = "Data layer, security, common libraries, system admin, update mechanisms"
        Modules       = @("HnVue.Common", "HnVue.Data", "HnVue.Security", "HnVue.SystemAdmin", "HnVue.Update")
        FocusAreas    = @(
            "EF Core migration standards (YYYYMMDD naming, Up/Down methods)",
            "SQLCipher AES-256 encryption and secure key management",
            "Repository pattern with async/CancellationToken support",
            "Security: bcrypt (12 rounds), JWT HS256, HMAC-SHA256 audit chain",
            "NuGet Central Package Management via Directory.Packages.props"
        )
        FileOwnership = @(
            "src/HnVue.Common/**",
            "src/HnVue.Data/**",
            "src/HnVue.Security/**",
            "src/HnVue.SystemAdmin/**",
            "src/HnVue.Update/**",
            "tests/HnVue.Common.Tests/**",
            "tests/HnVue.Data.Tests/**",
            "tests/HnVue.Security.Tests/**"
        )
    }
    "team-b" = @{
        DisplayName   = "Team B (Medical Imaging Pipeline)"
        Role          = "DICOM, detector SDK, imaging, dose safety, incident management, workflow engine, patient management"
        Modules       = @("HnVue.Dicom", "HnVue.Detector", "HnVue.Imaging", "HnVue.Dose", "HnVue.Incident", "HnVue.Workflow", "HnVue.PatientManagement", "HnVue.CDBurning")
        FocusAreas    = @(
            "Safety-critical modules (Dose, Incident): 90%+ branch coverage",
            "DICOM standards compliance with fo-dicom 5.1.3",
            "FPD detector SDK adapter pattern (IDetectorService abstraction)",
            "Workflow 9-state machine transition validation",
            "Dose interlock 4-level logic is invariant (RA risk assessment required for changes)"
        )
        FileOwnership = @(
            "src/HnVue.Dicom/**",
            "src/HnVue.Detector/**",
            "src/HnVue.Imaging/**",
            "src/HnVue.Dose/**",
            "src/HnVue.Incident/**",
            "src/HnVue.Workflow/**",
            "src/HnVue.PatientManagement/**",
            "src/HnVue.CDBurning/**",
            "tests/HnVue.Dicom.Tests/**",
            "tests/HnVue.Detector.Tests/**",
            "tests/HnVue.Imaging.Tests/**",
            "tests/HnVue.Dose.Tests/**",
            "tests/HnVue.Workflow.Tests/**"
        )
    }
    "team-design" = @{
        DisplayName   = "Team Design (Pure UI Design)"
        Role          = "WPF Views, styles, themes, components, converters, assets, DesignTime mocks"
        Modules       = @("HnVue.UI/Views", "HnVue.UI/Styles", "HnVue.UI/Themes", "HnVue.UI/Components", "HnVue.UI/Converters", "HnVue.UI/Assets", "HnVue.UI/DesignTime")
        FocusAreas    = @(
            "MahApps.Metro theme standards (Light, Dark, High Contrast)",
            "DESIGN_TO_XAML_WORKFLOW 5-phase compliance",
            "Accessibility: IEC 62366 / WCAG 2.1 AA (contrast>=4.5:1, touch>=44x44px)",
            "DesignTime mock pattern for VS2022 designer rendering",
            "No business logic in Views; data binding and commands only"
        )
        FileOwnership = @(
            "src/HnVue.UI/Views/**",
            "src/HnVue.UI/Styles/**",
            "src/HnVue.UI/Themes/**",
            "src/HnVue.UI/Components/**",
            "src/HnVue.UI/Converters/**",
            "src/HnVue.UI/Assets/**",
            "src/HnVue.UI/DesignTime/**"
        )
    }
}

function New-TeamClaudeMd {
    param(
        [string]$TeamName,
        [hashtable]$TeamDef
    )

    $modulesStr = ($TeamDef.Modules | ForEach-Object { "- $_" }) -join "`n"
    $focusStr = ($TeamDef.FocusAreas | ForEach-Object { "- $_" }) -join "`n"
    $ownershipStr = ($TeamDef.FileOwnership | ForEach-Object { "- ``$_``" }) -join "`n"

    $content = @"
# HnVue Console SW - $($TeamDef.DisplayName) Worktree

## Team Identity
- Team: $TeamName
- Role: $($TeamDef.Role)
- Module Ownership:
$modulesStr

## Team Rules
@.claude/rules/teams/$TeamName.md

## Shared Project Rules
@CLAUDE.md

## Focus Areas
$focusStr

## File Ownership (DO NOT modify files outside ownership)
$ownershipStr

## Cross-Team Coordination
- Interface changes affecting other teams: create issue + notify Coordinator
- Breaking changes: require `breaking-change` label and all affected team notification
- Shared module updates: coordinate through PR review process
"@

    return $content
}

function New-TeamContextMd {
    param(
        [string]$TeamName,
        [hashtable]$TeamDef
    )

    $modulesInline = $TeamDef.Modules -join ", "

    $content = @"
# Team Context

## [HARD] Session Start — FIRST ACTION

After /clear or new session, IMMEDIATELY execute DISPATCH Resolution Protocol:
1. `git pull origin main`
2. Read `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/_CURRENT.md`
3. Find $TeamName row -> read the specified DISPATCH file
4. If ACTIVE -> start working on tasks immediately (no user prompt needed)
5. If IDLE/MERGED -> report IDLE to Commander Center and wait

Do NOT wait for user input. This is automatic.

---

This worktree is configured for **$TeamName** ($($TeamDef.DisplayName)).

## Active Team
- Name: $TeamName
- Role: $($TeamDef.Role)
- Modules: $modulesInline

## Behavioral Rules
- You are operating as the **$TeamName** team
- Only modify files within your file ownership boundaries
- Follow the team-specific rules in .claude/rules/teams/$TeamName.md
- For changes outside your ownership, create an issue and notify the owning team
- All cross-team interface changes require Coordinator approval

## Team Rules Reference
The authoritative team rules are defined in:
- .claude/rules/teams/$TeamName.md (team-specific standards)
- CLAUDE.md (this worktree's scoped instructions)
"@

    return $content
}

function Initialize-TeamWorktree {
    param(
        [string]$TeamName
    )

    $teamDef = $Teams[$TeamName]
    if (-not $teamDef) {
        Write-Error "Unknown team: $TeamName"
        return
    }

    $worktreePath = Join-Path $WorktreeRoot $TeamName

    if (-not (Test-Path $worktreePath)) {
        Write-Warning "Worktree directory not found: $worktreePath"
        return
    }

    # Create CLAUDE.md
    $claudeMdPath = Join-Path $worktreePath "CLAUDE.md"
    $claudeMdContent = New-TeamClaudeMd -TeamName $TeamName -TeamDef $teamDef
    Set-Content -Path $claudeMdPath -Value $claudeMdContent -Encoding UTF8
    Write-Host "[OK] $TeamName/CLAUDE.md" -ForegroundColor Green

    # Create .claude/rules/team-context.md
    $rulesDir = Join-Path $worktreePath ".claude" "rules"
    if (-not (Test-Path $rulesDir)) {
        New-Item -Path $rulesDir -ItemType Directory -Force | Out-Null
    }
    $contextPath = Join-Path $rulesDir "team-context.md"
    $contextContent = New-TeamContextMd -TeamName $TeamName -TeamDef $teamDef
    Set-Content -Path $contextPath -Value $contextContent -Encoding UTF8
    Write-Host "[OK] $TeamName/.claude/rules/team-context.md" -ForegroundColor Green
}

# Main execution
Write-Host "=== Init-TeamWorktree ===" -ForegroundColor Cyan
Write-Host "Project root: $ProjectRoot"
Write-Host "Worktree root: $WorktreeRoot"
Write-Host ""

if ($Team) {
    Write-Host "Initializing team: $Team" -ForegroundColor Yellow
    Initialize-TeamWorktree -TeamName $Team
}
else {
    Write-Host "Initializing all teams..." -ForegroundColor Yellow
    foreach ($teamName in ($Teams.Keys | Sort-Object)) {
        Initialize-TeamWorktree -TeamName $teamName
    }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Cyan
