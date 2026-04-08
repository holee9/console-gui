<#
.SYNOPSIS
    Creates a bug report issue with structured template.
#>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Description,
    [Parameter(Mandatory)][ValidateSet('critical','high','medium','low')]
    [string]$Priority,
    [string]$Module
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$body = @"
## 버그 설명
$Description

## 관련 모듈
$Module

## 재현 단계
1.
2.
3.

## 예상 동작


## 실제 동작


## 환경
- OS: Windows 11
- .NET: 8.0
"@

$labels = @("bug", "priority-$Priority")
& "$PSScriptRoot\New-Issue.ps1" -Title $Title -Body $body -Labels $labels
