<#
.SYNOPSIS
    Generates coverage report with exclusion policy applied (SPEC-GOVERNANCE-001 REQ-GOV-003).
.DESCRIPTION
    Runs dotnet test with XPlat Code Coverage, excludes Views/Migrations/DesignTime,
    reports per-module coverage vs targets, and validates Safety-Critical modules >= 90%.
    Outputs JSON + HTML summary to TestReports/.

    Exit codes:
        0 = All gates passed
        1 = One or more coverage gates FAILED
        2 = Conditional pass (non-critical gate missed)

.PARAMETER SolutionPath
    Path to the .sln file. Defaults to HnVue.sln in the repo root.

.PARAMETER OutputDir
    Directory for coverage output. Defaults to TestReports/coverage.

.PARAMETER SkipBuild
    If set, skips dotnet restore and build (assume already built).

.EXAMPLE
    pwsh -File scripts/qa/Generate-CoverageReport.ps1
    pwsh -File scripts/qa/Generate-CoverageReport.ps1 -SkipBuild
#>

param(
    [string]$SolutionPath = "HnVue.sln",
    [string]$OutputDir = "TestReports/coverage",
    [switch]$SkipBuild
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Continue'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = (Get-Item $scriptDir).Parent.Parent.FullName

$solutionFile = Join-Path $repoRoot $SolutionPath
$outputPath = Join-Path $repoRoot $OutputDir

Write-Host "=== HnVue Coverage Report Generator ==="
Write-Host "SPEC-GOVERNANCE-001 REQ-GOV-003"
Write-Host "Repo root: $repoRoot"
Write-Host "Solution:  $solutionFile"
Write-Host "Output:    $outputPath"
Write-Host ""

# -----------------------------------------------------------------------
# Exclusion policy (per docs/testing/coverage-exclusion-policy.md)
# -----------------------------------------------------------------------
$excludeByFile = @(
    "**/Views/*.xaml.cs",
    "**/Components/**/*.xaml.cs",
    "**/DesignTime/**/*.cs",
    "**/Migrations/**/*.cs",
    "**/*Snapshot.cs"
)
$excludeByAttr = @(
    "ExcludeFromCodeCoverage",
    "GeneratedCodeAttribute",
    "CompilerGeneratedAttribute"
)
$excludeFileArg  = $excludeByFile -join ","
$excludeAttrArg  = $excludeByAttr -join ","

# -----------------------------------------------------------------------
# Module coverage targets (per team-common.md Quality Standards)
# -----------------------------------------------------------------------
$coverageTargets = @{
    "HnVue.Dose"               = @{ Target = 0.90; Category = "Safety-Critical" }
    "HnVue.Incident"           = @{ Target = 0.90; Category = "Safety-Critical" }
    "HnVue.Security"           = @{ Target = 0.90; Category = "Safety-Critical" }
    "HnVue.Update"             = @{ Target = 0.90; Category = "Safety-Critical" }
    "HnVue.Imaging"            = @{ Target = 0.85; Category = "Safety-Adjacent" }
    "HnVue.Workflow"           = @{ Target = 0.85; Category = "Safety-Adjacent" }
    "HnVue.Dicom"              = @{ Target = 0.85; Category = "Standard" }
    "HnVue.Detector"           = @{ Target = 0.85; Category = "Standard" }
    "HnVue.PatientManagement"  = @{ Target = 0.85; Category = "Standard" }
    "HnVue.CDBurning"          = @{ Target = 0.85; Category = "Standard" }
    "HnVue.Common"             = @{ Target = 0.85; Category = "Standard" }
    "HnVue.Data"               = @{ Target = 0.85; Category = "Standard" }
    "HnVue.SystemAdmin"        = @{ Target = 0.85; Category = "Standard" }
    "HnVue.UI.ViewModels"      = @{ Target = 0.85; Category = "Standard" }
    "HnVue.UI.Contracts"       = @{ Target = 0.85; Category = "Standard" }
}

# -----------------------------------------------------------------------
# Step 1: Run dotnet test with coverage collection
# -----------------------------------------------------------------------
Write-Host "--- Step 1: Running tests with coverage collection ---"

if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
}

$testResultsDir = Join-Path $outputPath "raw"
if (-not (Test-Path $testResultsDir)) {
    New-Item -ItemType Directory -Path $testResultsDir | Out-Null
}

$runSettings = @"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <ExcludeByFile>$excludeFileArg</ExcludeByFile>
          <ExcludeByAttribute>$excludeAttrArg</ExcludeByAttribute>
          <IncludeTestAssembly>false</IncludeTestAssembly>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
"@

$runSettingsPath = Join-Path $outputPath "coverage-run-settings.xml"
$runSettings | Set-Content -Path $runSettingsPath -Encoding UTF8

$buildFlag = if ($SkipBuild) { "--no-build" } else { "" }

$testArgs = @(
    "test", $solutionFile,
    "--settings", $runSettingsPath,
    "--results-directory", $testResultsDir,
    "--logger", "trx",
    "--verbosity", "minimal"
)
if ($SkipBuild) { $testArgs += "--no-build" }

Write-Host "dotnet $($testArgs -join ' ')"
Push-Location $repoRoot
try {
    & dotnet @testArgs
    $testExitCode = $LASTEXITCODE
} finally {
    Pop-Location
}

Write-Host ""
if ($testExitCode -ne 0) {
    Write-Host "[WARN] Some tests failed (exit code: $testExitCode). Coverage data may be incomplete." -ForegroundColor Yellow
}

# -----------------------------------------------------------------------
# Step 2: Parse coverage XML files
# -----------------------------------------------------------------------
Write-Host "--- Step 2: Parsing coverage results ---"

$coverageFiles = Get-ChildItem -Path $testResultsDir -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Host "[ERROR] No coverage.cobertura.xml files found in $testResultsDir" -ForegroundColor Red
    Write-Host "Ensure coverlet.collector is referenced in test projects."
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage file(s)"

# Aggregate per-module coverage from all cobertura files
$moduleCoverage = @{}
$totalCoveredLines = 0
$totalValidLines = 0

foreach ($file in $coverageFiles) {
    try {
        [xml]$xml = Get-Content -Path $file.FullName -Encoding UTF8
        $packages = $xml.coverage.packages.package
        if ($null -eq $packages) { continue }

        foreach ($pkg in $packages) {
            $name = $pkg.name -replace "\.Tests$", "" -replace "\.Test$", ""
            # Skip test assemblies
            if ($name -match "\.Tests$" -or $name -match "Architecture") { continue }

            $coveredLines = [int]($pkg."lines-covered" ?? 0)
            $validLines   = [int]($pkg."lines-valid" ?? 0)

            if (-not $moduleCoverage.ContainsKey($name)) {
                $moduleCoverage[$name] = @{ Covered = 0; Valid = 0 }
            }
            $moduleCoverage[$name].Covered += $coveredLines
            $moduleCoverage[$name].Valid   += $validLines
            $totalCoveredLines += $coveredLines
            $totalValidLines   += $validLines
        }
    } catch {
        Write-Host "[WARN] Failed to parse $($file.FullName): $_" -ForegroundColor Yellow
    }
}

# -----------------------------------------------------------------------
# Step 3: Report per-module results
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "--- Step 3: Per-module coverage report ---"
Write-Host ("{0,-35} {1,-20} {2,10} {3,10} {4,10}" -f "Module", "Category", "Target", "Actual", "Status")
Write-Host ("-" * 90)

$failedModules = @()
$warningModules = @()
$results = @()

foreach ($module in ($coverageTargets.Keys | Sort-Object)) {
    $targetInfo = $coverageTargets[$module]
    $target     = $targetInfo.Target
    $category   = $targetInfo.Category

    if ($moduleCoverage.ContainsKey($module)) {
        $covered = $moduleCoverage[$module].Covered
        $valid   = $moduleCoverage[$module].Valid
        $actual  = if ($valid -gt 0) { $covered / $valid } else { 0 }
        $pct     = [math]::Round($actual * 100, 1)
    } else {
        $pct    = 0.0
        $actual = 0.0
        $covered = 0
        $valid   = 0
    }

    $targetPct = [math]::Round($target * 100, 1)
    $passed    = $actual -ge $target
    $status    = if ($passed) { "PASS" } else { "FAIL" }
    $color     = if ($passed) { "Green" } else { "Red" }

    Write-Host ("{0,-35} {1,-20} {2,9}% {3,9}% {4,10}" -f $module, $category, $targetPct, $pct, $status) -ForegroundColor $color

    if (-not $passed) {
        if ($category -eq "Safety-Critical") {
            $failedModules += "$module ($pct% < $targetPct% required)"
        } else {
            $warningModules += "$module ($pct% < $targetPct% required)"
        }
    }

    $results += @{
        module   = $module
        category = $category
        target   = $targetPct
        actual   = $pct
        covered  = $covered
        valid    = $valid
        passed   = $passed
    }
}

# -----------------------------------------------------------------------
# Step 4: Overall effective coverage
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "--- Step 4: Overall effective coverage ---"

$effectiveCoverage = if ($totalValidLines -gt 0) {
    [math]::Round($totalCoveredLines / $totalValidLines * 100, 1)
} else {
    0.0
}

$overallTarget = 85.0
$overallPassed = $effectiveCoverage -ge $overallTarget
$overallStatus = if ($overallPassed) { "PASS" } else { "FAIL" }
$overallColor  = if ($overallPassed) { "Green" } else { "Red" }

Write-Host "Total covered lines:  $totalCoveredLines"
Write-Host "Total valid lines:    $totalValidLines"
Write-Host ("Effective coverage:   {0}% (target: {1}%) [{2}]" -f $effectiveCoverage, $overallTarget, $overallStatus) -ForegroundColor $overallColor
Write-Host "(Exclusions applied: Views/*.xaml.cs, Migrations/, DesignTime/)"

# -----------------------------------------------------------------------
# Step 5: Safety-Critical gate
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "--- Step 5: Safety-Critical gate (>= 90%) ---"

if ($failedModules.Count -eq 0) {
    Write-Host "All Safety-Critical modules passed." -ForegroundColor Green
} else {
    Write-Host "FAILED Safety-Critical modules:" -ForegroundColor Red
    foreach ($m in $failedModules) {
        Write-Host "  - $m" -ForegroundColor Red
    }
}

# -----------------------------------------------------------------------
# Step 6: JSON summary output
# -----------------------------------------------------------------------
$date = Get-Date -Format "yyyy-MM-dd"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$summary = @{
    generated_at           = $timestamp
    effective_coverage_pct = $effectiveCoverage
    overall_target_pct     = $overallTarget
    overall_passed         = $overallPassed
    total_covered_lines    = $totalCoveredLines
    total_valid_lines      = $totalValidLines
    safety_critical_passed = ($failedModules.Count -eq 0)
    failed_critical        = $failedModules
    warning_standard       = $warningModules
    modules                = $results
    exclusions             = @(
        "Views/*.xaml.cs",
        "Components/**/*.xaml.cs",
        "DesignTime/**/*.cs",
        "Migrations/**/*.cs"
    )
}

$jsonPath = Join-Path $outputPath "coverage-summary-$date.json"
$summary | ConvertTo-Json -Depth 10 | Set-Content -Path $jsonPath -Encoding UTF8
Write-Host ""
Write-Host "JSON summary: $jsonPath"

# -----------------------------------------------------------------------
# Step 7: HTML report
# -----------------------------------------------------------------------
$htmlRows = foreach ($r in ($results | Sort-Object { $_.module })) {
    $rowClass = if ($r.passed) { "pass" } else { "fail" }
    "<tr class='$rowClass'><td>$($r.module)</td><td>$($r.category)</td><td>$($r.target)%</td><td>$($r.actual)%</td><td>$(if ($r.passed) { 'PASS' } else { 'FAIL' })</td></tr>"
}

$htmlContent = @"
<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="UTF-8">
<title>HnVue Coverage Report - $date</title>
<style>
  body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }
  h1 { color: #333; }
  .summary { background: white; padding: 15px; border-radius: 8px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
  .metric { display: inline-block; margin: 10px; padding: 10px 20px; border-radius: 6px; text-align: center; }
  .metric.pass { background: #d4edda; color: #155724; }
  .metric.fail { background: #f8d7da; color: #721c24; }
  .metric .value { font-size: 28px; font-weight: bold; }
  .metric .label { font-size: 12px; }
  table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
  th { background: #343a40; color: white; padding: 10px; text-align: left; }
  td { padding: 8px 10px; border-bottom: 1px solid #dee2e6; }
  tr.pass td:last-child { color: #155724; font-weight: bold; }
  tr.fail td:last-child { color: #721c24; font-weight: bold; }
  tr.fail { background: #fff5f5; }
  .exclusion-note { font-size: 12px; color: #666; margin-top: 10px; }
</style>
</head>
<body>
<h1>HnVue Coverage Report</h1>
<p>Generated: $timestamp | SPEC-GOVERNANCE-001 REQ-GOV-003</p>

<div class="summary">
  <div class="metric $(if ($overallPassed) { 'pass' } else { 'fail' })">
    <div class="value">$effectiveCoverage%</div>
    <div class="label">Effective Coverage (target: $overallTarget%)</div>
  </div>
  <div class="metric $(if ($failedModules.Count -eq 0) { 'pass' } else { 'fail' })">
    <div class="value">$(if ($failedModules.Count -eq 0) { 'PASS' } else { 'FAIL' })</div>
    <div class="label">Safety-Critical Gate (>= 90%)</div>
  </div>
  <p class="exclusion-note">Exclusions: Views/*.xaml.cs, Components/**/*.xaml.cs, DesignTime/**/*.cs, Migrations/**/*.cs</p>
</div>

<table>
  <thead><tr><th>Module</th><th>Category</th><th>Target</th><th>Actual</th><th>Status</th></tr></thead>
  <tbody>$($htmlRows -join "`n  ")</tbody>
</table>
</body>
</html>
"@

$htmlPath = Join-Path $outputPath "coverage-report-$date.html"
$htmlContent | Set-Content -Path $htmlPath -Encoding UTF8
Write-Host "HTML report:  $htmlPath"

# -----------------------------------------------------------------------
# Final exit code
# -----------------------------------------------------------------------
Write-Host ""
if ($failedModules.Count -gt 0) {
    Write-Host "=== RESULT: FAILED (Safety-Critical gate) ===" -ForegroundColor Red
    exit 1
} elseif (-not $overallPassed -or $warningModules.Count -gt 0) {
    Write-Host "=== RESULT: CONDITIONAL PASS (Standard modules below target) ===" -ForegroundColor Yellow
    exit 2
} else {
    Write-Host "=== RESULT: PASSED ===" -ForegroundColor Green
    exit 0
}
