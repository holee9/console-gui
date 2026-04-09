param(
    [ValidateSet('All', 'Build', 'Test', 'Coverage', 'Security')]
    [string]$Mode = 'All',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$OutputDirectory = 'TestReports'
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$solutionPath = Join-Path $repoRoot 'HnVue.sln'
$outputRoot = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
}
else {
    Join-Path $repoRoot $OutputDirectory
}

$testResultsDir = Join-Path $outputRoot 'test-results'
$coverageResultsDir = Join-Path $outputRoot 'coverage-results'
$coverageReportDir = Join-Path $outputRoot 'coverage'
$securityDir = Join-Path $outputRoot 'security'
$summaryPath = Join-Path $outputRoot 'local-analysis-summary.json'

$summary = [ordered]@{
    Mode          = $Mode
    Configuration = $Configuration
    TimestampUtc  = (Get-Date).ToUniversalTime().ToString('o')
    Build         = 'NotRun'
    Test          = 'NotRun'
    Coverage      = 'NotRun'
    Security      = 'NotRun'
    Failures      = @()
}

$toolsRestored = $false
$solutionRestored = $false

function New-OutputDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Invoke-Dotnet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Ensure-LocalTools {
    if ($script:toolsRestored) {
        return
    }

    Invoke-Dotnet -Arguments @('tool', 'restore')
    $script:toolsRestored = $true
}

function Ensure-SolutionRestore {
    if ($script:solutionRestored) {
        return
    }

    Invoke-Dotnet -Arguments @('restore', $solutionPath, '--nologo')
    $script:solutionRestored = $true
}

function Invoke-AnalysisStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SummaryKey,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    try {
        & $Action
        $summary[$SummaryKey] = 'Pass'
    }
    catch {
        $summary[$SummaryKey] = 'Fail'
        $summary.Failures += "${SummaryKey}: $($_.Exception.Message)"
        throw
    }
}

function Invoke-BuildAnalysis {
    Ensure-SolutionRestore

    Invoke-Dotnet -Arguments @(
        'build',
        $solutionPath,
        '--configuration', $Configuration,
        '--no-restore',
        '-p:ContinuousIntegrationBuild=true',
        '--nologo'
    )
}

function Invoke-TestAnalysis {
    Ensure-SolutionRestore
    New-OutputDirectory -Path $testResultsDir

    Invoke-Dotnet -Arguments @(
        'test',
        $solutionPath,
        '--configuration', $Configuration,
        '--no-restore',
        '--logger', 'trx;LogFileName=local-analysis.trx',
        '--results-directory', $testResultsDir,
        '--nologo'
    )
}

function Invoke-CoverageAnalysis {
    Ensure-LocalTools
    Ensure-SolutionRestore
    New-OutputDirectory -Path $coverageResultsDir
    New-OutputDirectory -Path $coverageReportDir

    $testArguments = @(
        'test',
        $solutionPath,
        '--configuration', $Configuration,
        '--no-restore',
        '--settings', (Join-Path $repoRoot 'coverage.runsettings'),
        '--collect:XPlat Code Coverage',
        '--results-directory', $coverageResultsDir,
        '--logger', 'trx;LogFileName=coverage.trx',
        '--nologo'
    )

    Write-Host "dotnet $($testArguments -join ' ')" -ForegroundColor Cyan
    & dotnet @testArguments
    $testExitCode = $LASTEXITCODE

    $reportPattern = Join-Path $coverageResultsDir '**\coverage.cobertura.xml'
    Invoke-Dotnet -Arguments @(
        'tool', 'run', 'reportgenerator', '--',
        "-reports:$reportPattern",
        "-targetdir:$coverageReportDir",
        '-reporttypes:Html;TextSummary'
    )

    if ($testExitCode -ne 0) {
        throw "dotnet test with coverage failed with exit code $testExitCode"
    }
}

function Invoke-SecurityAnalysis {
    Ensure-LocalTools
    New-OutputDirectory -Path $securityDir

    $outdatedReport = Join-Path $securityDir 'dotnet-outdated.txt'
    $vulnerabilityReport = Join-Path $securityDir 'vulnerable-packages.txt'

    Write-Host 'Running dotnet-outdated...' -ForegroundColor Cyan
    (& dotnet tool run dotnet-outdated -- $solutionPath -o $outdatedReport -of markdown -t -ifs 2>&1) |
        Tee-Object -FilePath $outdatedReport | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet-outdated reported a failure.'
    }

    Write-Host 'Running dotnet list package --vulnerable...' -ForegroundColor Cyan
    (& dotnet list $solutionPath package --vulnerable --include-transitive 2>&1) |
        Tee-Object -FilePath $vulnerabilityReport | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet list package --vulnerable reported a failure.'
    }
}

New-OutputDirectory -Path $outputRoot

try {
    switch ($Mode) {
        'All' {
            Invoke-AnalysisStep -SummaryKey 'Build' -Action { Invoke-BuildAnalysis }
            Invoke-AnalysisStep -SummaryKey 'Coverage' -Action { Invoke-CoverageAnalysis }
            $summary.Test = $summary.Coverage
            Invoke-AnalysisStep -SummaryKey 'Security' -Action { Invoke-SecurityAnalysis }
        }
        'Build' {
            Invoke-AnalysisStep -SummaryKey 'Build' -Action { Invoke-BuildAnalysis }
        }
        'Test' {
            Invoke-AnalysisStep -SummaryKey 'Test' -Action { Invoke-TestAnalysis }
        }
        'Coverage' {
            Invoke-AnalysisStep -SummaryKey 'Coverage' -Action { Invoke-CoverageAnalysis }
            $summary.Test = $summary.Coverage
        }
        'Security' {
            Invoke-AnalysisStep -SummaryKey 'Security' -Action { Invoke-SecurityAnalysis }
        }
    }
}
finally {
    $summary | ConvertTo-Json -Depth 4 | Out-File $summaryPath -Encoding utf8
}

Write-Host ''
Write-Host 'Local analysis summary' -ForegroundColor Green
Write-Host "  Build:    $($summary.Build)"
Write-Host "  Test:     $($summary.Test)"
Write-Host "  Coverage: $($summary.Coverage)"
Write-Host "  Security: $($summary.Security)"
Write-Host "  Summary:  $summaryPath"

if ($summary.Failures.Count -gt 0) {
    Write-Host ''
    Write-Host 'Failures:' -ForegroundColor Red
    foreach ($failure in $summary.Failures) {
        Write-Host "  - $failure" -ForegroundColor Red
    }
    exit 1
}
