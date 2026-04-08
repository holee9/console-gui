<#
.SYNOPSIS
    Sets up SonarCloud secrets for GitHub and Gitea.
.DESCRIPTION
    Prerequisites:
    1. Create SonarCloud account at https://sonarcloud.io
    2. Import GitHub repo (holee9/Console-GUI)
    3. Generate token at: https://sonarcloud.io/account/security
    4. Note your Organization Key and Project Key
.PARAMETER SonarToken
    SonarCloud authentication token
.PARAMETER SonarProjectKey
    SonarCloud project key (e.g., holee9_Console-GUI)
.PARAMETER SonarOrg
    SonarCloud organization key (e.g., holee9)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$SonarToken,

    [Parameter(Mandatory=$true)]
    [string]$SonarProjectKey,

    [Parameter(Mandatory=$true)]
    [string]$SonarOrg
)

$ErrorActionPreference = 'Stop'

Write-Host "=== SonarCloud Secret Setup ===" -ForegroundColor Cyan

# --- GitHub Secrets ---
Write-Host "`n--- GitHub Secrets ---" -ForegroundColor Yellow
$repo = "holee9/Console-GUI"

gh secret set SONAR_TOKEN --body $SonarToken --repo $repo
Write-Host "  SONAR_TOKEN: set" -ForegroundColor Green

gh secret set SONAR_PROJECT_KEY --body $SonarProjectKey --repo $repo
Write-Host "  SONAR_PROJECT_KEY: set" -ForegroundColor Green

gh secret set SONAR_ORG --body $SonarOrg --repo $repo
Write-Host "  SONAR_ORG: set" -ForegroundColor Green

# --- Gitea Secrets ---
Write-Host "`n--- Gitea Secrets ---" -ForegroundColor Yellow
$GiteaUrl = if ($env:GITEA_URL) { $env:GITEA_URL } else { "http://10.11.1.40:7001" }
$GiteaToken = $env:GITEA_TOKEN
$GiteaOwner = "drake.lee"
$GiteaRepo = "Console-GUI"

if ($GiteaToken) {
    $headers = @{
        "Authorization" = "token $GiteaToken"
        "Content-Type"  = "application/json"
    }

    $secrets = @{
        "SONAR_TOKEN"       = $SonarToken
        "SONAR_PROJECT_KEY" = $SonarProjectKey
        "SONAR_ORG"         = $SonarOrg
    }

    foreach ($key in $secrets.Keys) {
        $payload = @{ data = $secrets[$key] } | ConvertTo-Json
        $uri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/actions/secrets/$key"
        try {
            Invoke-RestMethod -Uri $uri -Method Put -Headers $headers -Body $payload | Out-Null
            Write-Host "  ${key}: set" -ForegroundColor Green
        }
        catch {
            Write-Host "  ${key}: error - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
else {
    Write-Warning "GITEA_TOKEN not set. Skipping Gitea secrets."
}

Write-Host "`n=== Setup complete ===" -ForegroundColor Cyan
Write-Host "Verify by triggering a PR to main with src/ changes." -ForegroundColor DarkGray
