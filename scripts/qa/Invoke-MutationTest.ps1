<#
.SYNOPSIS
    Runs Stryker.NET mutation testing for safety-critical modules.
.PARAMETER Module
    Target module (Dose, Incident, Security, Update, or All)
#>
param(
    [ValidateSet('Dose','Incident','Security','Update','All')]
    [string]$Module = 'All'
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$configFile = Join-Path $PSScriptRoot "../../stryker-config.json"
if (-not (Test-Path $configFile)) {
    Write-Error "stryker-config.json not found at project root"
    exit 1
}

# Check if stryker is installed
$strykerInstalled = dotnet tool list -g | Select-String "dotnet-stryker"
if (-not $strykerInstalled) {
    Write-Host "Installing dotnet-stryker..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-stryker
}

$outputDir = "TestReports/mutation"
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir -Force | Out-Null }

if ($Module -eq 'All') {
    Write-Host "Running mutation tests for all safety-critical modules..." -ForegroundColor Cyan
    dotnet stryker --config-file stryker-config.json --output $outputDir
}
else {
    Write-Host "Running mutation tests for HnVue.$Module..." -ForegroundColor Cyan
    dotnet stryker --project "src/HnVue.$Module/HnVue.$Module.csproj" --output "$outputDir/$Module"
}

Write-Host "Mutation test complete. Reports at: $outputDir" -ForegroundColor Green
