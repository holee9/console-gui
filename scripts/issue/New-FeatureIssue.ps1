<#
.SYNOPSIS
    Creates a feature implementation issue with structured template.
#>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Description,
    [Parameter(Mandatory)][ValidateSet('team-a','team-b','team-design','coordinator')]
    [string]$Team,
    [string]$Module
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$body = @"
## 구현 내용
$Description

## 관련 모듈
- $Module

## 완료 기준
- [ ] 단위 테스트 작성
- [ ] 커버리지 임계값 유지
- [ ] 코드 리뷰 완료

## 영향 범위
구현 시 영향받는 모듈 및 팀을 확인해주세요.

## 담당 팀
$Team
"@

$labels = @("feat", $Team)
& "$PSScriptRoot\New-Issue.ps1" -Title $Title -Body $body -Labels $labels
