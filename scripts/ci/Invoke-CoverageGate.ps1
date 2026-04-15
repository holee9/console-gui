param(
    [Parameter(Mandatory = $true)]
    [string]$ReportPath,

    [Parameter(Mandatory = $false)]
    [double]$MinCoverage = 85.0,

    [Parameter(Mandatory = $false)]
    [double]$SafetyCriticalCoverage = 90.0
)

$ErrorActionPreference = "Stop"

Write-Host "=== Coverage Gate Enforcement ===" -ForegroundColor Cyan
Write-Host "Minimum Overall Coverage: $MinCoverage%"
Write-Host "Safety-Critical Coverage: $SafetyCriticalCoverage%"
Write-Host ""

# Safety-Critical modules
$safetyCriticalModules = @(
    "HnVue.Dose",
    "HnVue.Incident",
    "HnVue.Update",
    "HnVue.Security"
)

$moduleCoverage = @{}
$totalCovered = 0
$totalValid = 0

# Find all coverage files
# Prefer ReportGenerator merged output (Cobertura.xml) over raw Coverlet files
# to avoid double-counting shared modules across test projects
$mergedFiles = Get-ChildItem -Path $ReportPath -Recurse -Filter "Cobertura.xml" -ErrorAction SilentlyContinue

if ($mergedFiles.Count -gt 0) {
    $coverageFiles = $mergedFiles
    Write-Host "Using $($mergedFiles.Count) merged ReportGenerator file(s)" -ForegroundColor Green
}
else {
    $coverageFiles = Get-ChildItem -Path $ReportPath -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue
    Write-Host "Using $($coverageFiles.Count) raw Coverlet file(s) - consider running ReportGenerator merge" -ForegroundColor Yellow
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "[ERROR] No coverage files found in $ReportPath" -ForegroundColor Red
    exit 1
}

Write-Host "Parsing $($coverageFiles.Count) coverage files..." -ForegroundColor Yellow

foreach ($file in $coverageFiles) {
    try {
        [xml]$xml = Get-Content -Path $file.FullName -Encoding UTF8
        $packages = $xml.coverage.packages.package
        if ($null -eq $packages) { continue }

        foreach ($pkg in $packages) {
            $name = $pkg.name -replace "\.Tests$", "" -replace "\.Test$", ""

            # Skip test assemblies and Architecture
            if ($name -match "\.Tests$" -or $name -match "Architecture") { continue }

            # Try lines-covered/lines-valid attributes first (Coverlet raw format)
            $linesCovered = 0
            $linesValid = 0

            if ($pkg.HasAttribute("lines-covered") -and $pkg.HasAttribute("lines-valid")) {
                $linesCovered = [int]$pkg.GetAttribute("lines-covered")
                $linesValid = [int]$pkg.GetAttribute("lines-valid")
            }
            else {
                # Fallback: count lines from classes/methods/lines elements
                $classes = $pkg.classes.class
                if ($null -ne $classes) {
                    foreach ($cls in $classes) {
                        $methods = $cls.methods.method
                        if ($null -ne $methods) {
                            foreach ($method in $methods) {
                                $lines = $method.lines.line
                                if ($null -ne $lines) {
                                    foreach ($line in $lines) {
                                        if ($line.branch -eq "False" -or $line.branch -eq "false") {
                                            $linesValid++
                                            if ([int]$line.hits -gt 0) {
                                                $linesCovered++
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ($linesValid -eq 0) { continue }

            if (-not $moduleCoverage.ContainsKey($name)) {
                $moduleCoverage[$name] = @{ Covered = 0; Valid = 0 }
            }
            $moduleCoverage[$name].Covered += $linesCovered
            $moduleCoverage[$name].Valid += $linesValid
            $totalCovered += $linesCovered
            $totalValid += $linesValid
        }
    } catch {
        Write-Host "[WARN] Failed to parse $($file.FullName): $_" -ForegroundColor Yellow
    }
}

if ($totalValid -eq 0) {
    Write-Host "[ERROR] No valid coverage data found" -ForegroundColor Red
    exit 1
}

$overallCoverage = ($totalCovered / $totalValid) * 100
Write-Host "Overall Coverage: $($overallCoverage.ToString('F2'))% ($totalCovered/$totalValid lines)" -ForegroundColor Cyan

# Check Safety-Critical modules
Write-Host ""
Write-Host "Safety-Critical Modules:" -ForegroundColor Yellow

$safetyCriticalFailed = $false
foreach ($module in $safetyCriticalModules) {
    if ($moduleCoverage.ContainsKey($module)) {
        $covered = $moduleCoverage[$module].Covered
        $valid = $moduleCoverage[$module].Valid
        $coverage = ($covered / $valid) * 100

        $status = if ($coverage -ge $SafetyCriticalCoverage) { "✅ PASS" } else { "❌ FAIL" }
        $color = if ($coverage -ge $SafetyCriticalCoverage) { "Green" } else { "Red" }

        Write-Host "  $module : $($coverage.ToString('F2'))% ($covered/$valid) $status" -ForegroundColor $color

        if ($coverage -lt $SafetyCriticalCoverage) {
            $safetyCriticalFailed = $true
        }
    } else {
        Write-Host "  $module : ⚠️  NOT FOUND" -ForegroundColor Yellow
    }
}

# Enforce gates
Write-Host ""
Write-Host "=== Coverage Gate Results ===" -ForegroundColor Cyan

$gatePassed = $true

if ($overallCoverage -lt $MinCoverage) {
    Write-Host "[FAIL] Overall coverage ($($overallCoverage.ToString('F2'))%) is below minimum ($MinCoverage%)" -ForegroundColor Red
    $gatePassed = $false
} else {
    Write-Host "[PASS] Overall coverage ($($overallCoverage.ToString('F2'))%) meets minimum ($MinCoverage%)" -ForegroundColor Green
}

if ($safetyCriticalFailed) {
    Write-Host "[FAIL] One or more Safety-Critical modules are below $SafetyCriticalCoverage%" -ForegroundColor Red
    $gatePassed = $false
} else {
    Write-Host "[PASS] All Safety-Critical modules meet $SafetyCriticalCoverage% threshold" -ForegroundColor Green
}

Write-Host ""
if ($gatePassed) {
    Write-Host "✅ Coverage gate PASSED" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ Coverage gate FAILED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Action Required:" -ForegroundColor Yellow
    if ($overallCoverage -lt $MinCoverage) {
        Write-Host "  - Add unit tests to achieve $MinCoverage% overall coverage" -ForegroundColor Yellow
    }
    if ($safetyCriticalFailed) {
        Write-Host "  - Improve Safety-Critical module coverage to $SafetyCriticalCoverage%" -ForegroundColor Yellow
    }
    exit 1
}
