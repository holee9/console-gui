[CmdletBinding()]
param(
    [string]$ProjectRoot = "",
    [int]$PollSeconds = 600,
    [switch]$PopupOnChange
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Join-Path $scriptRoot ".."
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$dispatchDirectory = Join-Path $resolvedProjectRoot ".moai\dispatches\active"
$stateDirectory = Join-Path $resolvedProjectRoot ".moai\state"
$logPath = Join-Path $stateDirectory "team-b-dispatch-watcher.log"
$statePath = Join-Path $stateDirectory "team-b-dispatch-watcher-state.json"
$pidPath = Join-Path $stateDirectory "team-b-dispatch-watcher.pid"
$signalPath = Join-Path $stateDirectory "team-b-dispatch-pending.json"
$stopPath = Join-Path $stateDirectory "team-b-dispatch-watcher.stop"

New-Item -ItemType Directory -Force -Path $stateDirectory | Out-Null

function Write-Log {
    param([string]$Message)

    $line = "{0} {1}" -f (Get-Date -Format o), $Message
    Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
}

function Save-Json {
    param(
        [string]$Path,
        [object]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 6
    Set-Content -LiteralPath $Path -Value $json -Encoding UTF8
}

function Read-Json {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    try {
        return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch {
        Write-Log ("Failed to parse JSON at {0}: {1}" -f $Path, $_.Exception.Message)
        return $null
    }
}

function Show-DispatchPopup {
    param([string]$Message)

    try {
        Add-Type -AssemblyName PresentationFramework
        [void][System.Windows.MessageBox]::Show(
            $Message,
            "Team B Dispatch Watcher",
            [System.Windows.MessageBoxButton]::OK,
            [System.Windows.MessageBoxImage]::Information)
    }
    catch {
        Write-Log ("Popup delivery failed: {0}" -f $_.Exception.Message)
    }
}

function Get-DispatchCandidates {
    $files = @()

    $rootDispatchPath = Join-Path $resolvedProjectRoot "DISPATCH.md"
    if (Test-Path -LiteralPath $rootDispatchPath) {
        $files += Get-Item -LiteralPath $rootDispatchPath
    }

    if (Test-Path -LiteralPath $dispatchDirectory) {
        $files += Get-ChildItem -LiteralPath $dispatchDirectory -File | Where-Object {
            $_.Name -like "*team-b*" -or $_.Name -like "DISPATCH-TEAM-B*"
        }
    }

    return $files | Sort-Object FullName -Unique
}

function Get-DispatchSnapshot {
    $files = Get-DispatchCandidates

    $items = foreach ($file in $files) {
        [ordered]@{
            name = $file.Name
            path = $file.FullName
            lastWriteTimeUtc = $file.LastWriteTimeUtc.ToString("o")
            length = $file.Length
        }
    }

    $latest = $null
    if ($items.Count -gt 0) {
        $latest = $items | Sort-Object lastWriteTimeUtc -Descending | Select-Object -First 1
    }

    return [ordered]@{
        capturedAtUtc = [DateTime]::UtcNow.ToString("o")
        latest = $latest
        files = @($items)
    }
}

function Get-SnapshotFingerprint {
    param([object]$Snapshot)

    if ($null -eq $Snapshot) {
        return ""
    }

    return ($Snapshot.files | ConvertTo-Json -Depth 6 -Compress)
}

$pidPayload = [ordered]@{
    pid = $PID
    startedAtUtc = [DateTime]::UtcNow.ToString("o")
    projectRoot = $resolvedProjectRoot
    pollSeconds = $PollSeconds
}
Save-Json -Path $pidPath -Value $pidPayload

$savedState = Read-Json -Path $statePath
$currentSnapshot = Get-DispatchSnapshot
$currentFingerprint = Get-SnapshotFingerprint -Snapshot $currentSnapshot

if ($null -eq $savedState) {
    Save-Json -Path $statePath -Value $currentSnapshot
    Write-Log "Watcher started and baseline state was recorded."
}
else {
    $savedFingerprint = Get-SnapshotFingerprint -Snapshot $savedState
    if ($savedFingerprint -ne $currentFingerprint) {
        Save-Json -Path $statePath -Value $currentSnapshot
        Save-Json -Path $signalPath -Value $currentSnapshot
        Write-Log "Watcher started and detected changes since last recorded state."
    }
}

while ($true) {
    if (Test-Path -LiteralPath $stopPath) {
        Remove-Item -LiteralPath $stopPath -Force
        Write-Log "Stop file detected. Watcher is shutting down."
        break
    }

    $latestSnapshot = Get-DispatchSnapshot
    $latestFingerprint = Get-SnapshotFingerprint -Snapshot $latestSnapshot

    if ($latestFingerprint -ne $currentFingerprint) {
        Save-Json -Path $statePath -Value $latestSnapshot
        Save-Json -Path $signalPath -Value $latestSnapshot

        if ($null -ne $latestSnapshot.latest) {
            $summary = "Dispatch change detected: {0}" -f $latestSnapshot.latest.name
            Write-Log $summary

            if ($PopupOnChange) {
                $popupMessage = @"
New Team B dispatch activity detected.

File: $($latestSnapshot.latest.name)
Path: $($latestSnapshot.latest.path)
Updated: $($latestSnapshot.latest.lastWriteTimeUtc)
"@
                Show-DispatchPopup -Message $popupMessage
            }
        }
        else {
            Write-Log "Dispatch change detected, but no Team B candidates were found."
        }

        $currentFingerprint = $latestFingerprint
    }

    Start-Sleep -Seconds $PollSeconds
}

Write-Log "Watcher process exited."
