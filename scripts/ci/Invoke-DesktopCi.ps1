param(
  [ValidateSet("Restore", "Build", "Test", "All")]
  [string]$Mode = "All",
  [string]$Configuration = "Release",
  [string]$ResultsDirectory = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$projectRoots = @(
  (Join-Path $repoRoot "src"),
  (Join-Path $repoRoot "tests"),
  (Join-Path $repoRoot "tests.integration")
) | Where-Object { Test-Path $_ }

$allProjects = foreach ($root in $projectRoots) {
  Get-ChildItem -Path $root -Filter *.csproj -Recurse
}

$allProjects = $allProjects | Sort-Object FullName -Unique
$testProjects = $allProjects | Where-Object { $_.Name -match '\.(Tests|IntegrationTests)\.csproj$' }
$sourceProjects = $allProjects | Where-Object { $_.Name -notmatch '\.(Tests|IntegrationTests)\.csproj$' }

if (-not $allProjects) {
  throw "No .csproj files were found under src/, tests/, or tests.integration/."
}

Write-Host "Repository root: $repoRoot"
Write-Host "Source projects discovered: $($sourceProjects.Count)"
Write-Host "Test projects discovered: $($testProjects.Count)"

if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
  $ResultsDirectory = Join-Path $repoRoot "artifacts\test-results"
}

function Invoke-Dotnet {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments
  )

  Write-Host "dotnet $($Arguments -join ' ')"
  & dotnet @Arguments

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet command failed with exit code $LASTEXITCODE"
  }
}

if ($Mode -in @("Restore", "All")) {
  foreach ($project in $allProjects) {
    Invoke-Dotnet -Arguments @(
      "restore",
      $project.FullName,
      "--nologo"
    )
  }
}

if ($Mode -in @("Build", "All")) {
  foreach ($project in $sourceProjects) {
    Invoke-Dotnet -Arguments @(
      "build",
      $project.FullName,
      "--configuration", $Configuration,
      "--no-restore",
      "-p:ContinuousIntegrationBuild=true",
      "--nologo"
    )
  }
}

if ($Mode -in @("Test", "All")) {
  New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null

  foreach ($project in $testProjects) {
    $trxFileName = "{0}.trx" -f [System.IO.Path]::GetFileNameWithoutExtension($project.Name)

    Invoke-Dotnet -Arguments @(
      "test",
      $project.FullName,
      "--configuration", $Configuration,
      "--no-restore",
      "-p:ContinuousIntegrationBuild=true",
      "--logger", "trx;LogFileName=$trxFileName",
      "--results-directory", $ResultsDirectory,
      "--nologo"
    )
  }
}
