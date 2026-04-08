<#
.SYNOPSIS
    Closes an issue with a Korean-safe comment on Gitea and/or GitHub.
#>
param(
    [Parameter(Mandatory)][int]$IssueNumber,
    [Parameter(Mandatory)][string]$Comment,
    [ValidateSet('gitea','github','both')]
    [string]$Platform = 'both'
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$GiteaUrl = if ($env:GITEA_URL) { $env:GITEA_URL } else { "http://10.11.1.40:7001" }
$GiteaToken = $env:GITEA_TOKEN
$GiteaOwner = if ($env:GITEA_OWNER) { $env:GITEA_OWNER } else { "drake.lee" }
$GiteaRepo = if ($env:GITEA_REPO) { $env:GITEA_REPO } else { "Console-GUI" }
$GitHubRepo = if ($env:GITHUB_REPO) { $env:GITHUB_REPO } else { "holee9/Console-GUI" }

if ($Platform -in @('gitea', 'both')) {
    if ($GiteaToken) {
        $headers = @{
            "Authorization" = "token $GiteaToken"
            "Content-Type"  = "application/json; charset=utf-8"
        }
        # Add comment
        $commentPayload = @{ body = $Comment } | ConvertTo-Json -Depth 3
        $commentBytes = [System.Text.Encoding]::UTF8.GetBytes($commentPayload)
        $commentUri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/issues/$IssueNumber/comments"
        Invoke-RestMethod -Uri $commentUri -Method Post -Headers $headers -Body $commentBytes | Out-Null

        # Close issue
        $closePayload = @{ state = "closed" } | ConvertTo-Json
        $closeBytes = [System.Text.Encoding]::UTF8.GetBytes($closePayload)
        $closeUri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/issues/$IssueNumber"
        Invoke-RestMethod -Uri $closeUri -Method Patch -Headers $headers -Body $closeBytes | Out-Null
        Write-Host "[Gitea] Issue #$IssueNumber closed with comment" -ForegroundColor Green
    }
}

if ($Platform -in @('github', 'both')) {
    $tmpFile = [System.IO.Path]::GetTempFileName()
    $Comment | Out-File -FilePath $tmpFile -Encoding utf8NoBOM
    gh issue comment $IssueNumber --body-file $tmpFile --repo $GitHubRepo 2>&1 | Out-Null
    gh issue close $IssueNumber --repo $GitHubRepo 2>&1 | Out-Null
    Remove-Item $tmpFile -ErrorAction SilentlyContinue
    Write-Host "[GitHub] Issue #$IssueNumber closed with comment" -ForegroundColor Green
}
