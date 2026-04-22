#Requires -Version 5.1
<#
.SYNOPSIS
S16-R2 Safety-Critical coverage parser.

.DESCRIPTION
Aggregates all Cobertura XML files in TestReports/S16-R2/coverage,
then reports module-level Line Coverage for Safety-Critical modules.
#>

[CmdletBinding()]
param(
    [string]$CoverageRoot = "TestReports/S16-R2/coverage"
)

$ErrorActionPreference = 'Stop'
$safetyCritical = @(
    'HnVue.Dose'
    'HnVue.Incident'
    'HnVue.Security'
    'HnVue.Update'
)

$targetModules = @(
    'HnVue.Common'
    'HnVue.Data'
    'HnVue.Security'
    'HnVue.Dicom'
    'HnVue.Detector'
    'HnVue.Imaging'
    'HnVue.Dose'
    'HnVue.Incident'
    'HnVue.Workflow'
    'HnVue.PatientManagement'
    'HnVue.CDBurning'
    'HnVue.SystemAdmin'
    'HnVue.Update'
    'HnVue.UI'
    'HnVue.UI.Contracts'
    'HnVue.UI.ViewModels'
)

$moduleAgg = @{}
foreach ($m in $targetModules) {
    $moduleAgg[$m] = [pscustomobject]@{
        Module        = $m
        LinesCovered  = 0
        LinesValid    = 0
        BranchCovered = 0
        BranchValid   = 0
    }
}

$files = Get-ChildItem -Path $CoverageRoot -Recurse -Filter coverage.cobertura.xml -File
Write-Host "Found $($files.Count) cobertura files" -ForegroundColor Cyan

foreach ($file in $files) {
    try {
        [xml]$xml = Get-Content -LiteralPath $file.FullName -Raw
    } catch {
        Write-Warning "Parse failed: $($file.FullName): $_"
        continue
    }

    foreach ($pkg in $xml.coverage.packages.package) {
        $pkgName = [string]$pkg.name
        # Match module: remove assembly suffix if any
        $matched = $null
        foreach ($t in $targetModules) {
            if ($pkgName -eq $t) { $matched = $t; break }
        }
        if (-not $matched) { continue }

        # Count lines
        $linesCovered = 0
        $linesValid   = 0
        foreach ($cls in $pkg.classes.class) {
            foreach ($ln in $cls.lines.line) {
                $hits = [int]$ln.hits
                $linesValid++
                if ($hits -gt 0) { $linesCovered++ }
            }
        }
        $moduleAgg[$matched].LinesCovered += $linesCovered
        $moduleAgg[$matched].LinesValid   += $linesValid
    }
}

Write-Host ""
Write-Host "=== S16-R2 Module Line Coverage (aggregated) ===" -ForegroundColor Yellow
$results = @()
foreach ($m in $targetModules) {
    $agg = $moduleAgg[$m]
    if ($agg.LinesValid -eq 0) { continue }
    $pct = [math]::Round(($agg.LinesCovered / $agg.LinesValid) * 100, 2)
    $isSc = $safetyCritical -contains $m
    $target = if ($isSc) { 90.0 } else { 85.0 }
    $status = if ($pct -ge $target) { 'PASS' } else { 'FAIL' }
    $tag = if ($isSc) { '[SC]' } else { '    ' }

    $results += [pscustomobject]@{
        Module        = $m
        LinesCovered  = $agg.LinesCovered
        LinesValid    = $agg.LinesValid
        Coverage      = $pct
        TargetPct     = $target
        SafetyCritical = $isSc
        Status        = $status
    }

    "{0} {1,-24} {2,10} / {3,-10} = {4,7:F2}%   target {5}%   {6}" -f $tag, $m, $agg.LinesCovered, $agg.LinesValid, $pct, $target, $status | Write-Host
}

$results | Export-Csv -Path "TestReports/S16-R2/coverage-summary.csv" -NoTypeInformation -Encoding UTF8
$results | ConvertTo-Json -Depth 3 | Set-Content -Path "TestReports/S16-R2/coverage-summary.json" -Encoding UTF8

Write-Host ""
Write-Host "=== Safety-Critical Gate ===" -ForegroundColor Yellow
$scResults = $results | Where-Object { $_.SafetyCritical }
foreach ($r in $scResults) {
    $icon = if ($r.Status -eq 'PASS') { 'PASS' } else { 'FAIL' }
    "  {0}  {1,-20} {2,7:F2}% (target 90%)" -f $icon, $r.Module, $r.Coverage | Write-Host
}

$failed = $scResults | Where-Object { $_.Status -eq 'FAIL' }
if ($failed.Count -eq 0) {
    Write-Host ""
    Write-Host "RESULT: Safety-Critical gate PASS — all 4 modules >= 90%" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "RESULT: Safety-Critical gate FAIL — $($failed.Count) module(s) below 90%" -ForegroundColor Red
    foreach ($f in $failed) {
        Write-Host "  - $($f.Module): $($f.Coverage)%" -ForegroundColor Red
    }
    exit 1
}
