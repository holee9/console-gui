[CmdletBinding()]
param(
    [string]$ProjectRoot = "",
    [int]$PollSeconds = 5
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
$watcherScriptPath = Join-Path $scriptRoot "Watch-TeamBDispatch.ps1"

New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null

if (Test-Path -LiteralPath $pidPath) {
    try {
        $pidInfo = Get-Content -LiteralPath $pidPath -Raw | ConvertFrom-Json
        $runningProcess = Get-Process -Id $pidInfo.pid -ErrorAction SilentlyContinue
        if ($null -ne $runningProcess) {
            [pscustomobject]@{
                status = "already_running"
                pid = $runningProcess.Id
                projectRoot = $resolvedProjectRoot
            } | ConvertTo-Json
            exit 0
        }
    }
    catch {
    }

    Remove-Item -LiteralPath $pidPath -Force -ErrorAction SilentlyContinue
}

$process = Start-Process -FilePath "powershell.exe" `
    -ArgumentList @(
        "-NoLogo",
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File",
        $watcherScriptPath,
        "-ProjectRoot",
        $resolvedProjectRoot,
        "-PollSeconds",
        $PollSeconds
    ) `
    -WorkingDirectory $resolvedProjectRoot `
    -WindowStyle Hidden `
    -PassThru

Start-Sleep -Seconds 2

[pscustomobject]@{
    status = "started"
    pid = $process.Id
    projectRoot = $resolvedProjectRoot
    watcherScript = $watcherScriptPath
} | ConvertTo-Json
