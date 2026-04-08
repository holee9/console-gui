<#
.SYNOPSIS
    Creates an issue directly on Gitea API with UTF-8 encoding.
.PARAMETER Title
    Issue title
.PARAMETER Body
    Issue body
.PARAMETER Labels
    Array of label IDs (integers)
.PARAMETER Assignee
    Assignee username
#>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Body,
    [int[]]$Labels = @(),
    [string]$Assignee
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$GiteaUrl = if ($env:GITEA_URL) { $env:GITEA_URL } else { "http://10.11.1.40:7001" }
$GiteaToken = if ($env:GITEA_TOKEN) { $env:GITEA_TOKEN } else { throw "GITEA_TOKEN environment variable required" }
$GiteaOwner = if ($env:GITEA_OWNER) { $env:GITEA_OWNER } else { "drake.lee" }
$GiteaRepo = if ($env:GITEA_REPO) { $env:GITEA_REPO } else { "Console-GUI" }

$headers = @{
    "Authorization" = "token $GiteaToken"
    "Content-Type"  = "application/json; charset=utf-8"
}

$payloadObj = @{ title = $Title; body = $Body }
if ($Labels.Count -gt 0) { $payloadObj.labels = $Labels }
if ($Assignee) { $payloadObj.assignee = $Assignee }

$jsonPayload = $payloadObj | ConvertTo-Json -Depth 3
$bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($jsonPayload)

$uri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/issues"

$response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $bodyBytes
Write-Host "Issue #$($response.number) created: $($response.html_url)" -ForegroundColor Green
return $response
