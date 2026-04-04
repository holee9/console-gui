<#
.SYNOPSIS
    Safely synchronize selected branches from Gitea (origin) to GitHub mirror.

.DESCRIPTION
    Pushes only the explicitly listed branches to the GitHub remote.
    Never uses --mirror or --prune, so branches that exist only on GitHub
    (e.g. feature/web-ui created from a secondary workstation) are preserved.

.PARAMETER Branches
    List of branch names to push.  Defaults to @('main').
    Add extra branches as needed:
        -Branches main, feature/web-ui

.PARAMETER Remote
    Name of the GitHub remote.  Defaults to 'github'.
    Configure once with:
        git remote add github https://github.com/holee9/console-gui.git

.PARAMETER DryRun
    Print the git push commands without executing them.

.EXAMPLE
    # main only (default)
    .\scripts\sync_to_github.ps1

    # main + feature/web-ui
    .\scripts\sync_to_github.ps1 -Branches main, feature/web-ui

    # preview without pushing
    .\scripts\sync_to_github.ps1 -DryRun
#>
param(
    [string[]] $Branches = @('main'),
    [string]   $Remote   = 'github',
    [switch]   $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# 1. Remote 설정 확인
# ---------------------------------------------------------------------------
$remoteUrl = git remote get-url $Remote 2>$null
if ($LASTEXITCODE -ne 0 -or -not $remoteUrl) {
    Write-Error @"
Remote '$Remote' is not configured.
Run once:
    git remote add $Remote https://github.com/holee9/console-gui.git
"@
    exit 1
}

Write-Host "Target remote : $Remote ($remoteUrl)"
Write-Host "Branches      : $($Branches -join ', ')"
if ($DryRun) { Write-Host "[DRY-RUN mode — no actual push]" -ForegroundColor Yellow }
Write-Host ""

# ---------------------------------------------------------------------------
# 2. 현재 로컬 브랜치 목록 취득
# ---------------------------------------------------------------------------
$localBranches = git branch --format='%(refname:short)' | ForEach-Object { $_.Trim() }

# ---------------------------------------------------------------------------
# 3. 브랜치별 push (--mirror / --prune 절대 사용 안 함)
# ---------------------------------------------------------------------------
$failed  = @()
$skipped = @()
$pushed  = @()

foreach ($branch in $Branches) {
    $ref = "refs/heads/${branch}:refs/heads/${branch}"

    # 로컬 브랜치 존재 여부 확인
    if ($branch -notin $localBranches) {
        Write-Warning "  SKIP  '$branch' — not found in local repository"
        $skipped += $branch
        continue
    }

    if ($DryRun) {
        Write-Host "  [DRY]  git push $Remote $ref" -ForegroundColor Cyan
        $pushed += $branch
        continue
    }

    Write-Host "  PUSH  $branch -> $Remote ..." -NoNewline
    git push $Remote $ref 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0) {
        Write-Host " OK" -ForegroundColor Green
        $pushed += $branch
    } else {
        Write-Host " FAILED" -ForegroundColor Red
        $failed += $branch
    }
}

# ---------------------------------------------------------------------------
# 4. 결과 요약
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "=== Sync Summary ==========================================="
Write-Host "  Pushed  : $($pushed  -join ', ')" -ForegroundColor Green
if ($skipped.Count -gt 0) {
    Write-Host "  Skipped : $($skipped -join ', ')" -ForegroundColor Yellow
}
if ($failed.Count -gt 0) {
    Write-Host "  Failed  : $($failed  -join ', ')" -ForegroundColor Red
}
Write-Host "  Remote branches NOT deleted (safe, no --mirror/--prune)"
Write-Host "============================================================"

if ($failed.Count -gt 0) { exit 1 }
