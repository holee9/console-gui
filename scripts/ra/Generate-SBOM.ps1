<#
.SYNOPSIS
    Generates SBOM in CycloneDX 1.5 JSON format from NuGet packages.
.PARAMETER OutputPath
    Output path for the SBOM JSON file
#>
param(
    [string]$OutputPath = "docs/regulatory/sbom_latest.json"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

Write-Host "Generating SBOM (CycloneDX 1.5)..." -ForegroundColor Cyan

# Collect all NuGet packages
$packagesRaw = dotnet list "HnVue.sln" package --format json 2>&1
$packages = $packagesRaw | ConvertFrom-Json -ErrorAction SilentlyContinue

if (-not $packages) {
    Write-Warning "Failed to parse package list. Falling back to text parsing."
    $packagesText = dotnet list "HnVue.sln" package 2>&1
    Write-Host $packagesText
    exit 1
}

# Build CycloneDX structure
$components = @()
$seen = @{}

foreach ($project in $packages.projects) {
    foreach ($framework in $project.frameworks) {
        foreach ($pkg in $framework.topLevelPackages) {
            $key = "$($pkg.id)@$($pkg.resolvedVersion)"
            if (-not $seen.ContainsKey($key)) {
                $seen[$key] = $true
                $components += @{
                    type    = "library"
                    name    = $pkg.id
                    version = $pkg.resolvedVersion
                    purl    = "pkg:nuget/$($pkg.id)@$($pkg.resolvedVersion)"
                    scope   = "required"
                }
            }
        }
    }
}

$sbom = @{
    bomFormat   = "CycloneDX"
    specVersion = "1.5"
    version     = 1
    metadata    = @{
        timestamp = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        component = @{
            type    = "application"
            name    = "HnVue Console SW"
            version = "1.0.0"
        }
        tools     = @(@{ name = "Generate-SBOM.ps1"; version = "1.0.0" })
    }
    components  = $components
}

$sbomJson = $sbom | ConvertTo-Json -Depth 10
$sbomJson | Out-File $OutputPath -Encoding utf8

Write-Host "SBOM generated: $OutputPath ($($components.Count) components)" -ForegroundColor Green
