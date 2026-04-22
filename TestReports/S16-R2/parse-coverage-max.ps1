#Requires -Version 5.1
<#
.SYNOPSIS
S16-R2 module coverage — take MAX(LinesCovered, LinesValid) per module across all cobertura files.

.DESCRIPTION
Each test project produces one cobertura that primarily covers its own assembly under test.
We take the max (most complete instrumentation) per module instead of summing.
#>

[CmdletBinding()]
param(
    [string]$CoverageRoot = "TestReports/S16-R2/coverage"
)

$ErrorActionPreference = 'Stop'
$safetyCritical = @('HnVue.Dose','HnVue.Incident','HnVue.Security','HnVue.Update')
$targetModules = @(
    'HnVue.Common','HnVue.Data','HnVue.Security','HnVue.Dicom','HnVue.Detector',
    'HnVue.Imaging','HnVue.Dose','HnVue.Incident','HnVue.Workflow',
    'HnVue.PatientManagement','HnVue.CDBurning','HnVue.SystemAdmin','HnVue.Update',
    'HnVue.UI','HnVue.UI.Contracts','HnVue.UI.ViewModels'
)

$best = @{}
foreach ($m in $targetModules) {
    $best[$m] = [pscustomobject]@{
        Module        = $m
        LinesCovered  = 0
        LinesValid    = 0
        SourceFile    = ''
    }
}

$files = Get-ChildItem -Path $CoverageRoot -Recurse -Filter coverage.cobertura.xml -File
Write-Host "Scanning $($files.Count) cobertura files (max per module)" -ForegroundColor Cyan

foreach ($file in $files) {
    try { [xml]$xml = Get-Content -LiteralPath $file.FullName -Raw } catch { continue }

    foreach ($pkg in $xml.coverage.packages.package) {
        $pkgName = [string]$pkg.name
        $matched = $null
        foreach ($t in $targetModules) { if ($pkgName -eq $t) { $matched = $t; break } }
        if (-not $matched) { continue }

        $linesCovered = 0
        $linesValid   = 0
        foreach ($cls in $pkg.classes.class) {
            foreach ($ln in $cls.lines.line) {
                $linesValid++
                if ([int]$ln.hits -gt 0) { $linesCovered++ }
            }
        }

        # Take the file where this module has MAX lines-valid (most complete instrumentation)
        # AND among those, highest lines-covered
        $cur = $best[$matched]
        if (($linesValid -gt $cur.LinesValid) -or
            ($linesValid -eq $cur.LinesValid -and $linesCovered -gt $cur.LinesCovered)) {
            $best[$matched] = [pscustomobject]@{
                Module        = $matched
                LinesCovered  = $linesCovered
                LinesValid    = $linesValid
                SourceFile    = $file.FullName
            }
        }
    }
}

Write-Host ""
Write-Host "=== S16-R2 Module Line Coverage (max per module) ===" -ForegroundColor Yellow
$results = @()
foreach ($m in $targetModules) {
    $c = $best[$m]
    if ($c.LinesValid -eq 0) {
        "     {0,-24} NO_DATA" -f $m | Write-Host
        $results += [pscustomobject]@{
            Module=$m; LinesCovered=0; LinesValid=0; Coverage=0; TargetPct=0; SafetyCritical=$false; Status='NO_DATA'
        }
        continue
    }
    $pct = [math]::Round(($c.LinesCovered / $c.LinesValid) * 100, 2)
    $isSc = $safetyCritical -contains $m
    $target = if ($isSc) { 90.0 } else { 85.0 }
    $status = if ($pct -ge $target) { 'PASS' } else { 'FAIL' }
    $tag = if ($isSc) { '[SC]' } else { '    ' }
    $results += [pscustomobject]@{
        Module=$m; LinesCovered=$c.LinesCovered; LinesValid=$c.LinesValid
        Coverage=$pct; TargetPct=$target; SafetyCritical=$isSc; Status=$status
    }
    "{0} {1,-24} {2,10} / {3,-10} = {4,7:F2}%   target {5}%   {6}" -f $tag, $m, $c.LinesCovered, $c.LinesValid, $pct, $target, $status | Write-Host
}

$results | Export-Csv -Path "TestReports/S16-R2/coverage-summary-max.csv" -NoTypeInformation -Encoding UTF8
$results | ConvertTo-Json -Depth 3 | Set-Content -Path "TestReports/S16-R2/coverage-summary-max.json" -Encoding UTF8

Write-Host ""
Write-Host "=== Safety-Critical Gate ===" -ForegroundColor Yellow
$scResults = $results | Where-Object { $_.SafetyCritical }
foreach ($r in $scResults) {
    "  {0}  {1,-20} {2,7:F2}% (target 90%)" -f $r.Status, $r.Module, $r.Coverage | Write-Host
}
