# DISPATCH: RA — Regulatory Affairs

Issued: 2026-04-08
Issued By: Main (MoAI Orchestrator)
Priority: P3-Medium

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Set Status to IN_PROGRESS
3. Execute each task in order
4. After each task, update its checkbox and add result notes
5. Set Status to COMPLETE with summary

## Context

QA team completed migration from SonarCloud (cloud-based) to local Roslyn analyzer infrastructure. This impacts three regulatory documents:

1. **DOC-042 CMP** (Configuration Management Plan): Tool section references SonarCloud — needs update to reflect local analyzers
2. **DOC-019 SBOM**: Three new NuGet analyzer packages added to the project
3. **DOC-033 SOUP**: Three new dotnet tools added to local manifest

New packages added:
- StyleCop.Analyzers 1.2.0-beta.556 (development analyzer, PrivateAssets=all)
- Roslynator.Analyzers 4.12.9 (development analyzer, PrivateAssets=all)
- SecurityCodeScan.VS2019 5.6.7 (security analyzer, PrivateAssets=all)

New tools added (.config/dotnet-tools.json):
- dotnet-reportgenerator-globaltool (coverage reporting)
- dotnet-stryker (mutation testing)
- dotnet-outdated-tool (dependency freshness check)

Note: All analyzer packages are PrivateAssets=all (build-only, not deployed to production). This affects their SBOM classification.

## Tasks

### Task 1: Update DOC-042 CMP Tool Section
- **Target files**: `docs/management/DOC-042*` (Configuration Management Plan)
- **Action**: 
  - Find the section describing static analysis tools
  - Replace SonarCloud references with local Roslyn analyzer description
  - Document the three analyzers: StyleCop.Analyzers (code style), Roslynator (code quality), SecurityCodeScan (security)
  - Document the local analysis script: `scripts/qa/Invoke-LocalAnalysis.ps1`
  - Document CI integration: `desktop-ci.yml` coverage artifact upload
  - Update version to v1.1 (minor update — tool change, not process change)
- **Acceptance criteria**: DOC-042 no longer references SonarCloud. Local analyzer toolchain is documented.
- **Constraints**: Do NOT change the CMP process sections. Only update tool descriptions.

### Task 2: Update DOC-019 SBOM
- **Target files**: `docs/verification/DOC-019*` (SBOM) OR run `scripts/ra/Generate-SBOM.ps1`
- **Action**:
  - Add three new analyzer packages to SBOM component list
  - Classification: development-only (PrivateAssets=all), not deployed
  - If Generate-SBOM.ps1 exists and works, run it to auto-generate
  - If manual: add entries with component name, version, license, and "development" scope
  - Update component count (was 42, now 45)
  - Update version to reflect the addition
- **Acceptance criteria**: SBOM includes all three analyzer packages with correct metadata.
- **Constraints**: Do NOT remove existing SBOM entries. Do NOT change CycloneDX format version.

### Task 3: Update DOC-033 SOUP List
- **Target files**: `docs/verification/DOC-033*` (SOUP List)
- **Action**:
  - Add three new dotnet tools to SOUP list: reportgenerator, stryker, dotnet-outdated
  - Add three analyzer packages: StyleCop.Analyzers, Roslynator.Analyzers, SecurityCodeScan.VS2019
  - Classification: development/QA tooling (not deployed to production)
  - Include version, purpose, and risk classification per IEC 62304
  - SOUP items for development-only tools typically get Class A (no direct patient safety impact)
- **Acceptance criteria**: SOUP list includes all 6 new components with IEC 62304 classification.
- **Constraints**: Do NOT change existing SOUP entries. Follow existing SOUP list format.

## Constraints

- DO NOT modify files outside RA ownership (docs/regulatory/, docs/planning/, docs/risk/, docs/verification/, docs/management/)
- DO NOT modify source code
- Follow IEC 62304 document version policy (minor version for non-substantive updates)
- Use existing document formatting conventions

## Status

- **State**: COMPLETE
- **Started**: 2026-04-08
- **Completed**: 2026-04-08
- **Results**:
  - Task 1 (DOC-042 CMP): DONE — `docs/management/DOC-042_CMP_v1.0.md` 업데이트 완료. v1.0 → v1.1. 정적 분석 도구 항목에서 "SonarCloud (계획)" 문구를 제거하고 StyleCop.Analyzers 1.2.0-beta.556, Roslynator.Analyzers 4.12.9, SecurityCodeScan.VS2019 5.6.7 로컬 Roslyn 분석기 인프라로 교체. `scripts/qa/Invoke-LocalAnalysis.ps1` 및 `desktop-ci.yml` 통합 명시. CI/CD 파이프라인 다이어그램 내 SonarCloud 참조도 수정.
  - Task 2 (DOC-019 SBOM): DONE — `docs/regulatory/DOC-019_SBOM_v1.0.md` 업데이트 완료. v1.0 → v1.1. 새 섹션 4.12 추가: 개발 전용 분석기 SBOM-043 (StyleCop.Analyzers), SBOM-044 (Roslynator.Analyzers), SBOM-045 (SecurityCodeScan.VS2019). 모두 PrivateAssets=all / scope: devOnly. 구성요소 합계 42 → 45 업데이트. 기존 항목 미변경.
  - Task 3 (DOC-033 SOUP): DONE — `docs/verification/DOC-033_SOUP_Report_v1.0.md` 업데이트 완료. v1.0 → v1.1. 6개 신규 항목 추가 (SOUP-039~044): dotnet 도구 3종 (reportgenerator, stryker, dotnet-outdated) + Roslyn 분석기 3종 (StyleCop.Analyzers, Roslynator.Analyzers, SecurityCodeScan.VS2019). 모두 Class A (환자 안전 영향 없음), 배포 미포함. 결론 섹션 및 범위 항목 업데이트. 기존 SOUP 항목 미변경.
