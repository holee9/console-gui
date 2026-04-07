# gitea_api.ps1 - Gitea API helper (UTF-8 인코딩 보장)
# 사용법: . .\gitea_api.ps1; Invoke-GiteaApi -Method PATCH -Path "/issues/42" -Body @{title="제목"}

param()

$GiteaBase = "http://10.11.1.40:7001/api/v1/repos/DR_RnD/Console-GUI"
$GiteaToken = "a4cb79626194b34a2d52835de05fb770162af014"
$Utf8NoBOM = New-Object System.Text.UTF8Encoding $false

function Invoke-GiteaApi {
    param(
        [string]$Method = "GET",
        [string]$Path,
        [hashtable]$Body = $null
    )

    $uri = "$GiteaBase$Path"
    $headers = @{
        "Authorization" = "token $GiteaToken"
        "Content-Type"  = "application/json"
    }

    if ($Body) {
        $json = $Body | ConvertTo-Json -Depth 10 -Compress
        $bytes = $Utf8NoBOM.GetBytes($json)
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -Body $bytes
    } else {
        return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
    }
}

function New-GiteaIssue {
    param([string]$Title, [string]$Body, [string[]]$Labels = @())
    return Invoke-GiteaApi -Method "POST" -Path "/issues" -Body @{
        title = $Title
        body  = $Body
    }
}

function Close-GiteaIssue {
    param([int]$Number, [string]$Comment = $null)
    if ($Comment) {
        Invoke-GiteaApi -Method "POST" -Path "/issues/$Number/comments" -Body @{ body = $Comment } | Out-Null
    }
    return Invoke-GiteaApi -Method "PATCH" -Path "/issues/$Number" -Body @{ state = "closed" }
}

function Add-GiteaComment {
    param([int]$Number, [string]$Body)
    return Invoke-GiteaApi -Method "POST" -Path "/issues/$Number/comments" -Body @{ body = $Body }
}

Write-Host "Gitea API helper 로드됨. 사용 가능한 함수: Invoke-GiteaApi, New-GiteaIssue, Close-GiteaIssue, Add-GiteaComment"
