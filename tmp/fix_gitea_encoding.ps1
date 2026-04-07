# Fix Gitea issue encoding - Korean text corrupted due to CP949 encoding in curl
# This script uses PowerShell which properly handles Unicode strings

$baseUrl = "http://10.11.1.40:7001/api/v1/repos/DR_RnD/Console-GUI"
$token = "a4cb79626194b34a2d52835de05fb770162af014"
$headers = @{
    "Authorization" = "token $token"
    "Content-Type"  = "application/json; charset=utf-8"
}

function Invoke-GiteaApi {
    param($method, $path, $body = $null)
    $uri = "$baseUrl$path"
    if ($body) {
        $json = $body | ConvertTo-Json -Depth 10
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
        return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers -Body $bytes
    } else {
        return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers
    }
}

Write-Host "=== Issue #41 수정 ===" -ForegroundColor Cyan

$issue41Title = "[Dose] SWR-DM-040-052: DAP/ESD/RDSR 계산 및 기록 구현 기능 미구현"
$issue41Body = @"
## 문제
DoseService.cs에 인터페이스는 정의되어 있으나 DAP(Dose Area Product), ESD(Entrance Surface Dose), RDSR(Radiation Dose Structured Report) 계산 기능이 미구현.

## SWR 참조
- SWR-DM-040/041: DAP 표시 및 계산
- SWR-DM-042/043: ESD 계산
- SWR-DM-044/046: RDSR 생성
- SWR-DM-051/052: 선량 X선 이력

## 구현 내용
DoseService.cs:
1. CalculateDapAsync() — kVp/mAs/FOV 기반 DAP 계산
2. CalculateEsdAsync() — ESD = DAP × BSF / (FOV² / 4π)
3. GenerateRdsrSummaryAsync() — fo-dicom DICOM SR 생성
4. GetDoseHistoryAsync() — SQLite 기반 선량 이력 조회

## 완료 기준
- 각 계산 메서드 단위 테스트 (최소 10개)
- RDSR 구조화 데이터 반환 기능 확인
"@

Invoke-GiteaApi -method "PATCH" -path "/issues/41" -body @{
    title = $issue41Title
    body  = $issue41Body
} | Select-Object number, title | Format-Table

Write-Host "=== Issue #42 수정 ===" -ForegroundColor Cyan

$issue42Title = "[Testing] SWR-NF-MT-051: 전체 테스트 커버리지 85% 목표 계획"
$issue42Body = @"
## 현황
현재 테스트 커버리지: ~2% (812개 테스트 중 대부분 미사용)
RTM SWR-NF-MT-051 요구사항: 최소 80% 코드 커버리지

## 미테스트 모듈 (우선순위 순)
1. HnVue.Security: SecurityService, JwtTokenService 전체 검증
2. HnVue.Workflow: WorkflowEngine 상태 전환 전체
3. HnVue.Dicom: DicomService, MppsScu, WorklistService
4. HnVue.Dose: DoseService 계산 검증
5. HnVue.Imaging: ImageProcessor 각 처리 함수

## 완료 기준
- 각 모듈 별도 테스트 커버리지 최소 85%
- IEC 62304 제5.5 단위 테스트 요구사항 충족
"@

Invoke-GiteaApi -method "PATCH" -path "/issues/42" -body @{
    title = $issue42Title
    body  = $issue42Body
} | Select-Object number, title | Format-Table

Write-Host "=== 댓글 #702 (Issue #42) 수정 ===" -ForegroundColor Cyan

$comment702Body = @"
## 테스트 커버리지 목표 달성 ✅

SWR-NF-MT-051 요구사항(≥85%) 전체 충족

| 모듈 | Before | After | 상태 |
|------|--------|-------|------|
| HnVue.Security | 91.53% | 91.53% | ✅ |
| HnVue.Workflow | 96.34% | 96.34% | ✅ |
| HnVue.Dose | ≥85% | ≥85% | ✅ |
| HnVue.Imaging | 80.4% | **88.66%** | ✅ |
| HnVue.Data | 71.78% | **85.64%** | ✅ |
| HnVue.CDBurning | 76.1% | **96.46%** | ✅ |

**총 테스트: 1124개, 실패: 0**
"@

Invoke-GiteaApi -method "PATCH" -path "/issues/42/comments/702" -body @{
    body = $comment702Body
} | Select-Object id | Format-Table

Write-Host "=== 전체 이슈 깨짐 검사 ===" -ForegroundColor Cyan

# 모든 닫힌 이슈 가져와서 깨짐 여부 체크
$page1 = Invoke-GiteaApi -method "GET" -path "/issues?state=closed&type=issues&limit=50&page=1"
$page2 = Invoke-GiteaApi -method "GET" -path "/issues?state=closed&type=issues&limit=50&page=2"
$allIssues = $page1 + $page2

foreach ($issue in $allIssues) {
    $title = $issue.title
    # 깨진 텍스트는 ? 치환 문자가 포함됨
    $hasCorruption = $title -match '\?' -and $title -notmatch '(?i)null|none'
    if ($hasCorruption) {
        Write-Host "⚠️  Issue #$($issue.number) 깨짐 의심: $title" -ForegroundColor Yellow
    }
}

Write-Host "`n=== 완료 ===" -ForegroundColor Green
Write-Host "수정된 이슈: #41, #42 (제목+본문), 댓글 #702"
