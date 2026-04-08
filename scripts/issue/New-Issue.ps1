<#
.SYNOPSIS
    Creates an issue on both Gitea and GitHub with UTF-8 safe Korean support.
.PARAMETER Title
    Issue title (Korean safe)
.PARAMETER Body
    Issue body content (Korean safe)
.PARAMETER Labels
    Array of label names
.PARAMETER GiteaOnly
    Create issue only on Gitea
.PARAMETER GitHubOnly
    Create issue only on GitHub
#>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Body,
    [string[]]$Labels = @(),
    [switch]$GiteaOnly,
    [switch]$GitHubOnly
)

# UTF-8 encoding setup (Korean character safety)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
$OutputEncoding = [System.Text.Encoding]::UTF8

$ErrorActionPreference = 'Stop'

# Configuration from environment or defaults
$GiteaUrl = if ($env:GITEA_URL) { $env:GITEA_URL } else { "http://10.11.1.40:7001" }
$GiteaToken = $env:GITEA_TOKEN
$GiteaOwner = if ($env:GITEA_OWNER) { $env:GITEA_OWNER } else { "drake.lee" }
$GiteaRepo = if ($env:GITEA_REPO) { $env:GITEA_REPO } else { "Console-GUI" }
$GitHubRepo = if ($env:GITHUB_REPO) { $env:GITHUB_REPO } else { "holee9/Console-GUI" }

function New-GiteaIssueInternal {
    param($IssueTitle, $IssueBody, $IssueLabels)

    if (-not $GiteaToken) {
        Write-Warning "GITEA_TOKEN not set. Skipping Gitea issue creation."
        return $null
    }

    $headers = @{
        "Authorization" = "token $GiteaToken"
        "Content-Type"  = "application/json; charset=utf-8"
    }

    $payload = @{
        title = $IssueTitle
        body  = $IssueBody
    } | ConvertTo-Json -Depth 3

    $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
    $uri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/issues"

    try {
        $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $bodyBytes
        Write-Host "[Gitea] Issue #$($response.number) created: $($response.html_url)" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Error "[Gitea] Failed to create issue: $_"
        return $null
    }
}

function New-GitHubIssueInternal {
    param($IssueTitle, $IssueBody, $IssueLabels)

    $tmpFile = [System.IO.Path]::GetTempFileName()
    try {
        $IssueBody | Out-File -FilePath $tmpFile -Encoding utf8NoBOM

        $labelArgs = @()
        foreach ($label in $IssueLabels) {
            $labelArgs += "--label"
            $labelArgs += $label
        }

        $result = & gh issue create --title $IssueTitle --body-file $tmpFile @labelArgs --repo $GitHubRepo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[GitHub] Issue created: $result" -ForegroundColor Green
            return $result
        }
        else {
            Write-Warning "[GitHub] Failed: $result"
            return $null
        }
    }
    finally {
        Remove-Item $tmpFile -ErrorAction SilentlyContinue
    }
}

# Main execution
$results = @{}

if (-not $GitHubOnly) {
    $results.Gitea = New-GiteaIssueInternal -IssueTitle $Title -IssueBody $Body -IssueLabels $Labels
}

if (-not $GiteaOnly) {
    $results.GitHub = New-GitHubIssueInternal -IssueTitle $Title -IssueBody $Body -IssueLabels $Labels
}

return $results
