<#
.SYNOPSIS
    Creates a QA result issue from release readiness report.
#>
param(
    [string]$ReportPath = "TestReports/release_summary.json",
    [ValidateSet('green','yellow','red')]
    [string]$Status = 'green'
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$statusEmoji = switch ($Status) {
    'green'  { 'PASS' }
    'yellow' { 'WARNING' }
    'red'    { 'BLOCKED' }
}

$summary = ""
if (Test-Path $ReportPath) {
    $data = Get-Content $ReportPath -Raw -Encoding utf8 | ConvertFrom-Json
    $summary = @"
- 빌드: $($data.build_status)
- 테스트: $($data.tests_passed)/$($data.tests_total)
- 커버리지: $($data.coverage)%
- 보안 취약점 (Critical): $($data.critical_vulns)
"@
}
else {
    $summary = "(리포트 파일 없음: $ReportPath)"
}

$title = "QA 릴리즈 준비도 보고 [$statusEmoji] - $(Get-Date -Format 'yyyy-MM-dd')"
$body = @"
## QA 릴리즈 준비도 결과

### 판정: $statusEmoji

### 요약
$summary

### 리포트 경로
$ReportPath

### 날짜
$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
"@

& "$PSScriptRoot\New-Issue.ps1" -Title $title -Body $body -Labels @("qa-result") -GiteaOnly
