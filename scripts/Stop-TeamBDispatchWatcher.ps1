[CmdletBinding()]
param(
    [string]$ProjectRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Join-Path $scriptRoot ".."
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$stateDirectory = Join-Path $resolvedProjectRoot ".moai\state"
$pidPath = Join-Path $stateDirectory "team-b-dispatch-watcher.pid"
$stopPath = Join-Path $stateDirectory "team-b-dispatch-watcher.stop"

New-Item -ItemType File -Force -Path $stopPath | Out-Null

$stoppedPid = $null
if (Test-Path -LiteralPath $pidPath) {
    try {
        $pidInfo = Get-Content -LiteralPath $pidPath -Raw | ConvertFrom-Json
        $stoppedPid = $pidInfo.pid
    }
    catch {
    }
}

[pscustomobject]@{
    status = "stop_requested"
    pid = $stoppedPid
} | ConvertTo-Json
