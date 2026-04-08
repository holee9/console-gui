<#
.SYNOPSIS
    Synchronizes issue labels between Gitea and GitHub.
#>

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Continue'

$GiteaUrl = if ($env:GITEA_URL) { $env:GITEA_URL } else { "http://10.11.1.40:7001" }
$GiteaToken = $env:GITEA_TOKEN
$GiteaOwner = if ($env:GITEA_OWNER) { $env:GITEA_OWNER } else { "drake.lee" }
$GiteaRepo = if ($env:GITEA_REPO) { $env:GITEA_REPO } else { "Console-GUI" }
$GitHubRepo = if ($env:GITHUB_REPO) { $env:GITHUB_REPO } else { "holee9/Console-GUI" }

$labels = @(
    @{ name = "team-a";              color = "#0075ca"; description = "Team A - Infrastructure" }
    @{ name = "team-b";              color = "#008672"; description = "Team B - Medical Domain" }
    @{ name = "team-design";         color = "#a2eeef"; description = "Design Team - UI" }
    @{ name = "coordinator";         color = "#7057ff"; description = "Coordinator - Integration" }
    @{ name = "team-qa";             color = "#e4e669"; description = "QA Team - Quality" }
    @{ name = "team-ra";             color = "#d876e3"; description = "RA Team - Regulatory" }
    @{ name = "priority-critical";   color = "#b60205"; description = "Priority: Critical" }
    @{ name = "priority-high";       color = "#d93f0b"; description = "Priority: High" }
    @{ name = "priority-medium";     color = "#fbca04"; description = "Priority: Medium" }
    @{ name = "priority-low";        color = "#0e8a16"; description = "Priority: Low" }
    @{ name = "feat";                color = "#1d76db"; description = "Feature implementation" }
    @{ name = "bug";                 color = "#d73a4a"; description = "Bug report" }
    @{ name = "docs";                color = "#0075ca"; description = "Documentation" }
    @{ name = "qa-result";           color = "#e4e669"; description = "QA analysis result" }
    @{ name = "ra-update";           color = "#d876e3"; description = "RA document update needed" }
    @{ name = "breaking-change";     color = "#b60205"; description = "Breaking interface change" }
    @{ name = "interface-contract";  color = "#7057ff"; description = "Interface contract change" }
    @{ name = "soup-update";         color = "#d876e3"; description = "SOUP/SBOM update needed" }
    @{ name = "security";            color = "#b60205"; description = "Security vulnerability" }
)

Write-Host "=== Syncing labels ===" -ForegroundColor Cyan

# GitHub labels
Write-Host "`n--- GitHub Labels ---" -ForegroundColor Yellow
foreach ($label in $labels) {
    $result = gh label create $label.name --color $label.color.TrimStart('#') --description $label.description --force --repo $GitHubRepo 2>&1
    Write-Host "  $($label.name): $result"
}

# Gitea labels
if ($GiteaToken) {
    Write-Host "`n--- Gitea Labels ---" -ForegroundColor Yellow
    $headers = @{
        "Authorization" = "token $GiteaToken"
        "Content-Type"  = "application/json; charset=utf-8"
    }

    foreach ($label in $labels) {
        $payload = @{
            name        = $label.name
            color       = $label.color
            description = $label.description
        } | ConvertTo-Json -Depth 3
        $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
        $uri = "$GiteaUrl/api/v1/repos/$GiteaOwner/$GiteaRepo/labels"

        try {
            Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $bodyBytes | Out-Null
            Write-Host "  $($label.name): created" -ForegroundColor Green
        }
        catch {
            Write-Host "  $($label.name): already exists or error" -ForegroundColor DarkGray
        }
    }
}
else {
    Write-Warning "GITEA_TOKEN not set. Skipping Gitea labels."
}

Write-Host "`n=== Label sync complete ===" -ForegroundColor Cyan
