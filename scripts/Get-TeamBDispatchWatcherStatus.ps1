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
$statePath = Join-Path $stateDirectory "team-b-dispatch-watcher-state.json"
$signalPath = Join-Path $stateDirectory "team-b-dispatch-pending.json"
$logPath = Join-Path $stateDirectory "team-b-dispatch-watcher.log"

$pidInfo = $null
$isRunning = $false

if (Test-Path -LiteralPath $pidPath) {
    try {
        $pidInfo = Get-Content -LiteralPath $pidPath -Raw | ConvertFrom-Json
        $isRunning = $null -ne (Get-Process -Id $pidInfo.pid -ErrorAction SilentlyContinue)
    }
    catch {
        $isRunning = $false
    }
}

$stateInfo = $null
if (Test-Path -LiteralPath $statePath) {
    try {
        $stateInfo = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
    }
    catch {
        $stateInfo = $null
    }
}

$signalInfo = $null
if (Test-Path -LiteralPath $signalPath) {
    try {
        $signalInfo = Get-Content -LiteralPath $signalPath -Raw | ConvertFrom-Json
    }
    catch {
        $signalInfo = $null
    }
}

[pscustomobject]@{
    running = $isRunning
    pid = if ($pidInfo) { $pidInfo.pid } else { $null }
    statePath = $statePath
    signalPath = $signalPath
    logPath = $logPath
    latestWatchedFile = if ($stateInfo) { $stateInfo.latest } else { $null }
    latestPendingSignal = if ($signalInfo) { $signalInfo.latest } else { $null }
} | ConvertTo-Json -Depth 6
