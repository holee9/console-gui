# HnVue Domain Map

## Architecture Overview

HnVue is a WPF desktop application for medical radiographic imaging using CsI flat panel detectors (FPD).

**Tech Stack**: .NET 8, WPF, MahApps.Metro, CommunityToolkit.Mvvm, EF Core, SQLCipher, fo-dicom 5.1.3

**Solution**: HnVue.sln (17 source projects + 14 test projects)

## Module Dependency Graph

```
Layer 4 (App):     HnVue.App (DI root)
                     |
Layer 3 (UI):      HnVue.UI ── HnVue.UI.ViewModels ── HnVue.UI.Contracts
                     |                |
Layer 2 (Domain):  HnVue.Workflow ── HnVue.Detector ── HnVue.Imaging
                   HnVue.Dicom ── HnVue.Dose ── HnVue.Incident
                   HnVue.PatientManagement ── HnVue.CDBurning
                   HnVue.SystemAdmin ── HnVue.Update
                     |
Layer 1 (Infra):   HnVue.Data ── HnVue.Security ── HnVue.Common
```

## Team Ownership Map

### Team A — Infrastructure (hnvue-infra)
| Module | Tests | Coverage Target |
|--------|-------|----------------|
| HnVue.Common | HnVue.Common.Tests | 85% |
| HnVue.Data | HnVue.Data.Tests | 85% |
| HnVue.Security | HnVue.Security.Tests | 90% (safety-critical) |
| HnVue.SystemAdmin | HnVue.SystemAdmin.Tests | 85% |
| HnVue.Update | HnVue.Update.Tests | 90% (safety-critical) |

### Team B — Medical Imaging (hnvue-medical)
| Module | Tests | Coverage Target | Safety-Critical |
|--------|-------|----------------|----------------|
| HnVue.Dicom | HnVue.Dicom.Tests | 85% | No |
| HnVue.Detector | HnVue.Detector.Tests | 85% | No |
| HnVue.Imaging | HnVue.Imaging.Tests | 85% | No |
| HnVue.Dose | HnVue.Dose.Tests | 90% | YES |
| HnVue.Incident | HnVue.Incident.Tests | 90% | YES |
| HnVue.Workflow | HnVue.Workflow.Tests | 85% | No |
| HnVue.PatientManagement | HnVue.PatientManagement.Tests | 85% | No |
| HnVue.CDBurning | HnVue.CDBurning.Tests | 85% | No |

### Team Design — UI (hnvue-ui)
| Module | Tests | Notes |
|--------|-------|-------|
| HnVue.UI | HnVue.UI.Tests | Views, Styles, Themes, Components |

### Coordinator (hnvue-coordinator)
| Module | Tests | Notes |
|--------|-------|-------|
| HnVue.UI.Contracts | - | Interface gate |
| HnVue.UI.ViewModels | - | Domain composition |
| HnVue.App | - | DI composition root |

## Safety-Critical Invariants

1. **Dose Interlock**: 4-level logic is INVARIANT. Any modification requires RA risk assessment (FMEA DOC-009)
2. **Audit Log Integrity**: HMAC-SHA256 hash chain. Tampering detection is mandatory
3. **Emergency Stop**: Always visible on Acquisition screen. Position changes require QA/RA review
4. **Workflow State Machine**: 9-state model. Invalid transitions throw InvalidOperationException

## Regulatory Documents (16 total)

Key documents for development impact:
- DOC-004 FRS: Workflow state additions/removals
- DOC-005 SRS: Interface contract changes
- DOC-006 SAD: Architecture changes
- DOC-007 SDS: Design changes
- DOC-009 FMEA: Safety-critical changes
- DOC-019 SBOM: NuGet package changes (42 components, CycloneDX 1.5)
- DOC-032 RTM: SWR traceability (100% coverage required)
- DOC-033 SOUP: Third-party software changes
- DOC-042 CMP: Configuration management (PRIORITY: complete from Draft)

## Build Configuration

- MSBuild: `D:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe`
- Target Framework: net8.0-windows
- Coverage Settings: coverage.runsettings
- Mutation Config: stryker-config.json
- Style Rules: .stylecop.json, .editorconfig
