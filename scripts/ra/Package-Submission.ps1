<#
.SYNOPSIS
    Packages regulatory submission documents for FDA, CE, or KFDA.
.PARAMETER Target
    Regulatory target (FDA, CE, KFDA)
.PARAMETER Version
    Software version string
.PARAMETER OutputDir
    Output directory for the package
#>
param(
    [Parameter(Mandatory)]
    [ValidateSet('FDA','CE','KFDA')]
    [string]$Target,

    [string]$Version = "1.0.0",
    [string]$OutputDir = "submission-packages"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

# Required documents per regulatory target
$requiredDocs = @{
    FDA = @(
        "docs/regulatory/DOC-036_510k_eSTAR_v2.0.md"
        "docs/regulatory/DOC-035_DHF_v1.0.md"
        "docs/regulatory/DOC-034_ReleaseDoc_v1.0.md"
        "docs/regulatory/DOC-038_DICOM_Conformance_v1.0.md"
        "docs/regulatory/DOC-019_SBOM_v1.0.md"
        "docs/regulatory/DOC-040_IFU_v1.0.md"
        "docs/regulatory/DOC-046_Security_Controls_v1.0.md"
        "docs/regulatory/DOC-048_VMP_v1.0.md"
        "docs/risk/DOC-008_Risk_Management_Plan_v1.0.md"
        "docs/risk/DOC-009_FMEA_v1.0.md"
        "docs/risk/DOC-010_RMR_v1.0.md"
        "docs/verification/DOC-011_VV_Master_Plan_v1.0.md"
        "docs/verification/DOC-032_RTM_v2.0.md"
        "docs/planning/DOC-005_SRS_v2.0.md"
        "docs/planning/DOC-006_SAD_v2.0.md"
    )
    CE = @(
        "docs/regulatory/DOC-037_CE_TechDoc_v1.0.md"
        "docs/regulatory/DOC-052_GSPR_Checklist_v1.0.md"
        "docs/regulatory/DOC-051_PMS_PMCF_v1.0.md"
        "docs/regulatory/DOC-020_Clinical_Evaluation_Plan_v1.0.md"
        "docs/regulatory/DOC-035_DHF_v1.0.md"
        "docs/regulatory/DOC-019_SBOM_v1.0.md"
        "docs/regulatory/DOC-040_IFU_v1.0.md"
        "docs/risk/DOC-008_Risk_Management_Plan_v1.0.md"
        "docs/risk/DOC-009_FMEA_v1.0.md"
        "docs/verification/DOC-032_RTM_v2.0.md"
    )
    KFDA = @(
        "docs/regulatory/DOC-039_KFDA_v1.0.md"
        "docs/regulatory/DOC-049_IEC81001_Compliance_v1.0.md"
        "docs/regulatory/DOC-035_DHF_v1.0.md"
        "docs/regulatory/DOC-019_SBOM_v1.0.md"
        "docs/regulatory/DOC-040_IFU_v1.0.md"
        "docs/risk/DOC-008_Risk_Management_Plan_v1.0.md"
        "docs/risk/DOC-009_FMEA_v1.0.md"
        "docs/verification/DOC-032_RTM_v2.0.md"
        "docs/planning/DOC-005_SRS_v2.0.md"
    )
}

$targetDir = "$OutputDir/${Target}_v${Version}_$(Get-Date -Format 'yyyyMMdd')"
if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }

Write-Host "Packaging $Target submission (v$Version)..." -ForegroundColor Cyan

$docs = $requiredDocs[$Target]
$missing = @()
$copied = @()

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Copy-Item $doc -Destination $targetDir
        $copied += $doc
    }
    else {
        $missing += $doc
        Write-Warning "MISSING: $doc"
    }
}

# Generate manifest
$manifest = @{
    target     = $Target
    version    = $Version
    date       = (Get-Date -Format "yyyy-MM-dd")
    total      = $docs.Count
    included   = $copied.Count
    missing    = $missing.Count
    documents  = $copied
    missing_docs = $missing
}
$manifest | ConvertTo-Json -Depth 3 | Out-File "$targetDir/MANIFEST.json" -Encoding utf8

Write-Host "`nPackage complete: $targetDir" -ForegroundColor Green
Write-Host "  Included: $($copied.Count)/$($docs.Count)" -ForegroundColor White
if ($missing.Count -gt 0) {
    Write-Host "  MISSING: $($missing.Count) documents" -ForegroundColor Red
    exit 1
}
