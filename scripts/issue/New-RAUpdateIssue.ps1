<#
.SYNOPSIS
    Creates an RA document update issue.
#>
param(
    [Parameter(Mandatory)][string]$DocumentId,
    [Parameter(Mandatory)][string]$Reason,
    [ValidateSet('high','medium')]
    [string]$Priority = 'medium'
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$title = "규제 문서 업데이트: $DocumentId"
$body = @"
## 업데이트 대상 문서
$DocumentId

## 변경 사유
$Reason

## 영향 분석
이 문서 변경이 다른 규제 문서에 미치는 영향을 확인해주세요.

## 완료 기준
- [ ] 문서 업데이트 완료
- [ ] 버전 번호 갱신
- [ ] 관련 RTM 추적 확인

## 날짜
$(Get-Date -Format 'yyyy-MM-dd')
"@

& "$PSScriptRoot\New-Issue.ps1" -Title $title -Body $body -Labels @("ra-update", "priority-$Priority") -GiteaOnly
