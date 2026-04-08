<#
.SYNOPSIS
    Runs OWASP Dependency-Check for NuGet vulnerability scanning.
#>
param(
    [string]$OutputDir = "TestReports/security-scan"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Continue'

if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null }

# Check for dependency-check CLI
$dcPath = Get-Command "dependency-check" -ErrorAction SilentlyContinue
if (-not $dcPath) {
    Write-Warning "OWASP Dependency-Check CLI not found in PATH."
    Write-Host "Install from: https://owasp.org/www-project-dependency-check/"
    Write-Host "Or use: winget install OWASP.DependencyCheck"
    exit 1
}

Write-Host "Running OWASP Dependency-Check..." -ForegroundColor Cyan

dependency-check `
    --project "HnVue" `
    --scan "." `
    --format "JSON" `
    --format "HTML" `
    --out $OutputDir `
    --suppression "dependency-check-suppressions.xml" `
    --failOnCVSS 7 `
    --enableExperimental 2>&1

$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Host "CRITICAL: Vulnerabilities with CVSS >= 7.0 found!" -ForegroundColor Red
    Write-Host "Review report at: $OutputDir" -ForegroundColor Yellow

    # Auto-create RA issue for SBOM update
    $scriptRoot = $PSScriptRoot
    $issueScript = Join-Path (Split-Path $scriptRoot) "issue/New-RAUpdateIssue.ps1"
    if (Test-Path $issueScript) {
        & $issueScript -DocumentId "DOC-019" -Reason "OWASP Dependency-Check found CVSS >= 7.0 vulnerabilities" -Priority high
    }
}
else {
    Write-Host "No critical vulnerabilities found." -ForegroundColor Green
}

Write-Host "Security scan complete. Reports at: $OutputDir" -ForegroundColor Cyan
exit $exitCode
