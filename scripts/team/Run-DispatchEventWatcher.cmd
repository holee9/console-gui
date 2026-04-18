@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
set "DEBOUNCE=5"
if not "%~1"=="" set "DEBOUNCE=%~1"
powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File "%SCRIPT_DIR%Start-DispatchEventWatcher.ps1" -DebounceSeconds %DEBOUNCE%
