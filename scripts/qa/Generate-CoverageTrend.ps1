<#
.SYNOPSIS
    Weekly coverage trend report generator for HnVue project.

.DESCRIPTION
    Generates a Markdown table per team showing:
      Module | Current% | Target% | Gap | Trend

    Reads current coverage from a Cobertura XML report (if available),
    otherwise uses the hardcoded Phase 1 baseline (75.6% overall).

    Previous week values are loaded from TestReports/coverage-history.json
    (auto-created on first run).

    Outputs: TestReports/COVERAGE_TREND_{date}.md

.PARAMETER CoverageReportPath
    Path to Cobertura XML coverage report. If not found, uses baseline values.

.PARAMETER OutputDirectory
    Directory where the Markdown report is written. Default: TestReports

.EXAMPLE
    ./Generate-CoverageTrend.ps1
    ./Generate-CoverageTrend.ps1 -CoverageReportPath TestReports/coverage/Cobertura.xml
#>

[CmdletBinding()]
param(
    [string]$CoverageReportPath = "TestReports\coverage\Cobertura.xml",
    [string]$OutputDirectory = "TestReports"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

$DateStamp   = (Get-Date).ToString("yyyy-MM-dd")
$ReportPath  = Join-Path $OutputDirectory "COVERAGE_TREND_$DateStamp.md"
$HistoryPath = Join-Path $OutputDirectory "coverage-history.json"

# ── Phase 1 confirmed baseline (2026-04-09) ───────────────────────────────────
# Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md
$Baseline = @{
    'HnVue.Common'            = 94.1
    'HnVue.Data'              = 100.0
    'HnVue.Security'          = 86.2
    'HnVue.SystemAdmin'       = 66.6
    'HnVue.Update'            = 77.3
    'HnVue.Dicom'             = 66.9
    'HnVue.Detector'          = 42.6
    'HnVue.Imaging'           = 87.5
    'HnVue.Dose'              = 67.6
    'HnVue.Incident'          = 94.2
    'HnVue.Workflow'          = 91.4
    'HnVue.PatientManagement' = 72.7
    'HnVue.CDBurning'         = 100.0
    'HnVue.UI'                = 71.4
    'HnVue.UI.Contracts'      = 42.8
    'HnVue.UI.ViewModels'     = 42.0
}

# ── Target floors ─────────────────────────────────────────────────────────────
$Targets = @{
    'HnVue.Common'            = 90.0
    'HnVue.Data'              = 85.0
    'HnVue.Security'          = 90.0
    'HnVue.SystemAdmin'       = 80.0
    'HnVue.Update'            = 85.0
    'HnVue.Dicom'             = 80.0
    'HnVue.Detector'          = 85.0
    'HnVue.Imaging'           = 85.0
    'HnVue.Dose'              = 90.0
    'HnVue.Incident'          = 90.0
    'HnVue.Workflow'          = 90.0
    'HnVue.PatientManagement' = 80.0
    'HnVue.CDBurning'         = 80.0
    'HnVue.UI'                = 75.0
    'HnVue.UI.Contracts'      = 70.0
    'HnVue.UI.ViewModels'     = 75.0
}

# ── Team ownership mapping ────────────────────────────────────────────────────
$TeamModules = [ordered]@{
    'Team A'      = @('HnVue.Common', 'HnVue.Data', 'HnVue.Security', 'HnVue.SystemAdmin', 'HnVue.Update')
    'Team B'      = @('HnVue.Dicom', 'HnVue.Detector', 'HnVue.Imaging', 'HnVue.Dose', 'HnVue.Incident', 'HnVue.Workflow', 'HnVue.PatientManagement', 'HnVue.CDBurning')
    'Team Design' = @('HnVue.UI')
    'Coordinator' = @('HnVue.UI.Contracts', 'HnVue.UI.ViewModels')
}

# ── Load current coverage from Cobertura XML ──────────────────────────────────
$CurrentCoverage = @{}
$reportSource    = 'baseline'

if (Test-Path $CoverageReportPath) {
    Write-Host "Loading coverage from: $CoverageReportPath" -ForegroundColor Cyan
    try {
        [xml]$xml = Get-Content $CoverageReportPath -Raw
        foreach ($pkg in $xml.coverage.packages.package) {
            $name = $pkg.name -replace '\.dll$', ''
            $CurrentCoverage[$name] = [Math]::Round([double]$pkg.'line-rate' * 100.0, 1)
        }
        $reportSource = 'cobertura'
        Write-Host "  Loaded $($CurrentCoverage.Count) module(s) from report." -ForegroundColor Green
    } catch {
        Write-Host "  WARNING: Failed to parse XML — using baseline values. Error: $_" -ForegroundColor Yellow
    }
}

if ($CurrentCoverage.Count -eq 0) {
    Write-Host "Using Phase 1 hardcoded baseline values (2026-04-09)." -ForegroundColor Yellow
    $CurrentCoverage = $Baseline.Clone()
    $reportSource = 'baseline'
}

# ── Load previous week from history ───────────────────────────────────────────
$PrevCoverage = @{}
if (Test-Path $HistoryPath) {
    try {
        $history = Get-Content $HistoryPath -Raw | ConvertFrom-Json
        # Use the most recent entry that is NOT today
        $sortedKeys = ($history.PSObject.Properties.Name | Where-Object { $_ -lt $DateStamp } | Sort-Object -Descending)
        if ($sortedKeys.Count -gt 0) {
            $prevDate = $sortedKeys[0]
            $prevSnapshot = $history.$prevDate
            $prevSnapshot.PSObject.Properties | ForEach-Object { $PrevCoverage[$_.Name] = $_.Value }
            Write-Host "Loaded previous snapshot: $prevDate" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  WARNING: Could not load history — trend column will show N/A." -ForegroundColor Yellow
    }
}

# ── Save today's snapshot to history ─────────────────────────────────────────
$history = if (Test-Path $HistoryPath) {
    try { Get-Content $HistoryPath -Raw | ConvertFrom-Json } catch { [PSCustomObject]@{} }
} else {
    [PSCustomObject]@{}
}

$todaySnapshot = [PSCustomObject]@{}
foreach ($k in $CurrentCoverage.Keys) {
    $todaySnapshot | Add-Member -MemberType NoteProperty -Name $k -Value $CurrentCoverage[$k] -Force
}
$history | Add-Member -MemberType NoteProperty -Name $DateStamp -Value $todaySnapshot -Force

# Keep only last 8 weeks
$allDates = ($history.PSObject.Properties.Name | Sort-Object -Descending)
if ($allDates.Count -gt 8) {
    $toRemove = $allDates | Select-Object -Skip 8
    foreach ($d in $toRemove) {
        $history.PSObject.Properties.Remove($d)
    }
}

$history | ConvertTo-Json -Depth 5 | Out-File -FilePath $HistoryPath -Encoding UTF8 -Force

# ── Build report ──────────────────────────────────────────────────────────────
function Get-TrendArrow([double]$current, [double]$prev) {
    if ($prev -le 0) { return "N/A" }
    $delta = $current - $prev
    if ($delta -gt 0.5)  { return "+{0:F1}pp" -f $delta }
    if ($delta -lt -0.5) { return "{0:F1}pp" -f $delta }
    return "~0pp"
}

function Get-GapText([double]$current, [double]$target) {
    $gap = $target - $current
    if ($gap -le 0) { return "0.0pp (met)" }
    return "{0:F1}pp" -f $gap
}

# Compute overall
$allCurrentValues = $CurrentCoverage.Values | Where-Object { $_ -ge 0 }
$overallCurrent = if ($allCurrentValues.Count -gt 0) { [Math]::Round(($allCurrentValues | Measure-Object -Average).Average, 1) } else { 75.6 }
$allPrevValues  = $PrevCoverage.Values   | Where-Object { $_ -ge 0 }
$overallPrev    = if ($allPrevValues.Count -gt 0) { [Math]::Round(($allPrevValues | Measure-Object -Average).Average, 1) } else { 0 }

# ── Assemble Markdown ─────────────────────────────────────────────────────────
$sb = [System.Text.StringBuilder]::new()

$null = $sb.AppendLine("# HnVue Coverage Trend Report")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("**Date**: $DateStamp")
$null = $sb.AppendLine("**Data Source**: $reportSource")
$null = $sb.AppendLine("**Interim Gate**: 80%+ overall line coverage")
$null = $sb.AppendLine("**Release Gate**: 85%+ overall line coverage")
$null = $sb.AppendLine("**Safety-Critical Hard Gate**: Dose / Incident branch coverage 90%+")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("## Overall")
$null = $sb.AppendLine("")

$overallGap    = Get-GapText $overallCurrent 85.0
$overallTrend  = Get-TrendArrow $overallCurrent $overallPrev
$overallStatus = if ($overallCurrent -ge 80.0) { "interim PASS" } elseif ($overallCurrent -ge 85.0) { "release PASS" } else { "interim FAIL" }

$null = $sb.AppendLine("| Metric | Value |")
$null = $sb.AppendLine("|--------|------:|")
$null = $sb.AppendLine("| Current Line Coverage | $overallCurrent% |")
$null = $sb.AppendLine("| Trend vs Last Week | $overallTrend |")
$null = $sb.AppendLine("| Gap to 85% Release Gate | $overallGap |")
$null = $sb.AppendLine("| Status | $overallStatus |")
$null = $sb.AppendLine("")

foreach ($team in $TeamModules.Keys) {
    $null = $sb.AppendLine("## $team")
    $null = $sb.AppendLine("")
    $null = $sb.AppendLine("| Module | Current% | Target% | Gap | Trend |")
    $null = $sb.AppendLine("|--------|--------:|--------:|-----|-------|")

    foreach ($module in $TeamModules[$team]) {
        $cur    = if ($CurrentCoverage.ContainsKey($module)) { $CurrentCoverage[$module] } else { 0.0 }
        $prev   = if ($PrevCoverage.ContainsKey($module))    { $PrevCoverage[$module]    } else { 0.0 }
        $target = if ($Targets.ContainsKey($module))         { $Targets[$module]         } else { 85.0 }
        $gap    = Get-GapText $cur $target
        $trend  = Get-TrendArrow $cur $prev

        $null = $sb.AppendLine("| $module | $cur% | $target% | $gap | $trend |")
    }
    $null = $sb.AppendLine("")
}

$null = $sb.AppendLine("## History")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("Coverage history is stored in ``TestReports/coverage-history.json``.")
$null = $sb.AppendLine("Run this script weekly after CI coverage collection to build the trend series.")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("---")
$null = $sb.AppendLine("*Generated by scripts/qa/Generate-CoverageTrend.ps1*")

# ── Write output ──────────────────────────────────────────────────────────────
if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$sb.ToString() | Out-File -FilePath $ReportPath -Encoding UTF8 -Force
Write-Host ""
Write-Host "Coverage trend report written: $ReportPath" -ForegroundColor Cyan
Write-Host "Overall: $overallCurrent% (trend: $overallTrend)" -ForegroundColor White
Write-Host ""
