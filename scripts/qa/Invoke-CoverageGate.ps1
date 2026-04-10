<#
.SYNOPSIS
    CI Coverage Gate enforcement script for HnVue project.

.DESCRIPTION
    Parses Cobertura XML coverage report and enforces module-specific and overall
    coverage floors. Returns exit code 0 on PASS, 1 on FAIL.

.PARAMETER CoverageReportPath
    Path to Cobertura XML coverage report (e.g., TestReports/coverage/Cobertura.xml).

.PARAMETER Gate
    Coverage gate mode: 'interim' (80%) or 'release' (85%). Default: interim.

.PARAMETER Strict
    If set, safety-critical branch coverage check is enforced (Dose/Incident 90%+).

.EXAMPLE
    ./Invoke-CoverageGate.ps1 -CoverageReportPath TestReports/coverage/Cobertura.xml -Gate release -Strict
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$CoverageReportPath = "TestReports\coverage\Cobertura.xml",

    [Parameter(Mandatory = $false)]
    [ValidateSet('interim', 'release')]
    [string]$Gate = 'interim',

    [switch]$Strict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Gate thresholds ────────────────────────────────────────────────────────────
$OverallThreshold = if ($Gate -eq 'release') { 85.0 } else { 80.0 }

# Module-specific floors (confirmed targets from PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md)
$ModuleFloors = @{
    # Team A
    'HnVue.Common'           = 90.0
    'HnVue.Data'             = 85.0
    'HnVue.Security'         = 90.0
    'HnVue.SystemAdmin'      = 80.0
    'HnVue.Update'           = 85.0
    # Team B
    'HnVue.Dicom'            = 80.0
    'HnVue.Detector'         = 85.0
    'HnVue.Imaging'          = 85.0
    'HnVue.Dose'             = 90.0
    'HnVue.Incident'         = 90.0
    'HnVue.Workflow'         = 90.0
    'HnVue.PatientManagement'= 80.0
    'HnVue.CDBurning'        = 80.0
    # Team Design
    'HnVue.UI'               = 75.0
    # Coordinator
    'HnVue.UI.Contracts'     = 70.0
    'HnVue.UI.ViewModels'    = 75.0
}

# Safety-critical modules requiring branch coverage 90%+ (hard gate)
$SafetyCriticalModules = @('HnVue.Dose', 'HnVue.Incident')

# ── Helper functions ───────────────────────────────────────────────────────────
function Format-Pct([double]$value) {
    return "{0:F1}%" -f $value
}

function Get-CoveragePercent([xml.XmlElement]$packageNode, [string]$attr) {
    $val = $packageNode.GetAttribute($attr)
    if ([string]::IsNullOrEmpty($val)) { return 0.0 }
    return [double]$val * 100.0
}

function Write-Result([string]$label, [string]$status, [string]$detail) {
    $color = if ($status -eq 'PASS') { 'Green' } else { 'Red' }
    Write-Host ("  [{0,-4}] {1,-40} {2}" -f $status, $label, $detail) -ForegroundColor $color
}

# ── Parse report ──────────────────────────────────────────────────────────────
if (-not (Test-Path $CoverageReportPath)) {
    Write-Host "ERROR: Coverage report not found: $CoverageReportPath" -ForegroundColor Red
    Write-Host "       Run tests with coverlet first:  dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura"
    exit 1
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  HnVue Coverage Gate Check  |  Mode: $($Gate.ToUpper())  |  Threshold: $(Format-Pct $OverallThreshold)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

[xml]$report = Get-Content $CoverageReportPath -Raw

# Overall coverage from root <coverage> element
$rootCoverage = $report.coverage
$overallLine   = [double]$rootCoverage.'line-rate'   * 100.0
$overallBranch = [double]$rootCoverage.'branch-rate' * 100.0

Write-Host "Overall Coverage:" -ForegroundColor White
Write-Host ("  Line:   {0}" -f (Format-Pct $overallLine))
Write-Host ("  Branch: {0}" -f (Format-Pct $overallBranch))
Write-Host ""

# ── Overall gate check ────────────────────────────────────────────────────────
$overallPass = $overallLine -ge $OverallThreshold
$gateResults = [System.Collections.Generic.List[hashtable]]::new()

$gateResults.Add(@{
    Label  = "Overall line coverage ($Gate gate)"
    Pass   = $overallPass
    Detail = "$(Format-Pct $overallLine) (required: $(Format-Pct $OverallThreshold))"
})

Write-Host "Gate Results:" -ForegroundColor White

# ── Per-module floor check ─────────────────────────────────────────────────────
$packages = $report.coverage.packages.package
if ($null -eq $packages) {
    Write-Host "  WARNING: No <package> elements found in report. Skipping module checks." -ForegroundColor Yellow
} else {
    # Build lookup: assembly name → coverage node
    $moduleCoverage = @{}
    foreach ($pkg in $packages) {
        # Cobertura package name is typically the assembly name
        $name = $pkg.name -replace '\.dll$', ''
        $moduleCoverage[$name] = $pkg
    }

    foreach ($module in ($ModuleFloors.Keys | Sort-Object)) {
        $floor = $ModuleFloors[$module]
        if ($moduleCoverage.ContainsKey($module)) {
            $linePct = Get-CoveragePercent $moduleCoverage[$module] 'line-rate'
            $pass    = $linePct -ge $floor
            $gateResults.Add(@{
                Label  = $module
                Pass   = $pass
                Detail = "$(Format-Pct $linePct) (floor: $(Format-Pct $floor))"
            })
        } else {
            # Module not in report — treat as 0%
            $gateResults.Add(@{
                Label  = $module
                Pass   = ($floor -le 0.0)
                Detail = "N/A in report (floor: $(Format-Pct $floor)) — assumed 0%"
            })
        }
    }
}

# ── Safety-critical branch gate ────────────────────────────────────────────────
if ($Strict) {
    Write-Host ""
    Write-Host "Safety-Critical Branch Gate (90%+ required):" -ForegroundColor Yellow
    foreach ($module in $SafetyCriticalModules) {
        if ($null -ne $packages) {
            $moduleCoverage = @{}
            foreach ($pkg in $packages) {
                $name = $pkg.name -replace '\.dll$', ''
                $moduleCoverage[$name] = $pkg
            }
            if ($moduleCoverage.ContainsKey($module)) {
                $branchPct = Get-CoveragePercent $moduleCoverage[$module] 'branch-rate'
                $pass      = $branchPct -ge 90.0
                $gateResults.Add(@{
                    Label  = "$module (branch)"
                    Pass   = $pass
                    Detail = "$(Format-Pct $branchPct) (hard gate: 90.0%)"
                })
            } else {
                $gateResults.Add(@{
                    Label  = "$module (branch)"
                    Pass   = $false
                    Detail = "N/A in report — assumed 0%"
                })
            }
        }
    }
}

# ── Print results table ────────────────────────────────────────────────────────
$failCount = 0
foreach ($r in $gateResults) {
    $status = if ($r.Pass) { 'PASS' } else { 'FAIL'; $failCount++ }
    Write-Result $r.Label $status $r.Detail
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($failCount -eq 0) {
    Write-Host "  RESULT: PASS — All coverage gates satisfied" -ForegroundColor Green
} else {
    Write-Host ("  RESULT: FAIL — {0} gate(s) failed" -f $failCount) -ForegroundColor Red
}

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

exit ($failCount -gt 0 ? 1 : 0)
