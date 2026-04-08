<#
.SYNOPSIS
    Scans test files for [Trait("SWR", "SWR-xxx")] annotations and verifies RTM traceability.
#>
param(
    [string]$TestsDir = "tests",
    [string]$OutputPath = "TestReports/rtm_trace_report.json"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "Scanning test files for SWR traceability..." -ForegroundColor Cyan

$traitPattern = '\[Trait\("SWR",\s*"(SWR-[^"]+)"\)\]'
$traceMap = @{}

# Scan all test .cs files
$testFiles = Get-ChildItem -Path $TestsDir -Filter "*.cs" -Recurse
foreach ($file in $testFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($content) {
        $matches = [regex]::Matches($content, $traitPattern)
        foreach ($match in $matches) {
            $swrId = $match.Groups[1].Value
            if (-not $traceMap.ContainsKey($swrId)) {
                $traceMap[$swrId] = @()
            }
            $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "").Replace("\", "/")
            $traceMap[$swrId] += $relativePath
        }
    }
}

# Output report
$report = @{
    timestamp      = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    total_swrs     = $traceMap.Count
    traced_swrs    = ($traceMap.GetEnumerator() | Where-Object { $_.Value.Count -gt 0 }).Count
    trace_details  = $traceMap
}

$outputDir = Split-Path $OutputPath
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir -Force | Out-Null }

$report | ConvertTo-Json -Depth 5 | Out-File $OutputPath -Encoding utf8

Write-Host "RTM trace report: $OutputPath" -ForegroundColor Green
Write-Host "  Total SWR references found: $($traceMap.Count)" -ForegroundColor White

# Check for gaps (SWRs with no tests)
$gaps = $traceMap.GetEnumerator() | Where-Object { $_.Value.Count -eq 0 }
if ($gaps.Count -gt 0) {
    Write-Warning "RTM traceability gaps found: $($gaps.Count) SWRs with no test coverage"
    foreach ($gap in $gaps) {
        Write-Warning "  - $($gap.Key)"
    }
}
