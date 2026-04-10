# QA Module Role Analysis Report

## Metadata

- Audience: main branch user
- Author: `team/qa` worktree
- Date: 2026-04-08
- Snapshot: `team/qa` is in sync with `main` at commit `8d8295a`, so this report applies directly to the current `main` contents.
- Validation commands:
  - `dotnet restore HnVue.sln`
  - `dotnet test HnVue.sln --configuration Debug --no-restore`

## Executive Summary

The current `main` branch is best described as "module-complete and strongly test-backed for development and simulation, but not yet fully closed for production hardware integration and final release gating."

- Implemented source modules: 17
- Test result: 1,135 total, 1 failed, 0 skipped
- Overall QA view:
  - Strong: Common, Data, Security, Workflow, Dose, DICOM, Incident, Update, PatientManagement
  - Partially closed: UI runtime flows, App composition, detector hardware path
  - Release status: not production-hardware ready

## Role-Based Analysis

| Role Area | Modules | Current Role Fit | QA Assessment | Test Evidence |
|---|---|---|---|---|
| Core contracts and shared models | `HnVue.Common`, `HnVue.UI.Contracts` | Shared interfaces, result pattern, enums, events, and UI contracts are clearly separated | This is a solid foundation layer with good dependency direction | `HnVue.Common.Tests` 82/82 pass, `HnVue.Architecture.Tests` 4/4 pass |
| Persistence and repositories | `HnVue.Data` | Handles EF Core entities, mappings, and repositories | The repository role is implemented, but patient field encryption is still an explicit TODO | `HnVue.Data.Tests` 102/102 pass |
| Security and audit | `HnVue.Security` | Authentication, RBAC, audit chain, JWT, denylist | One of the strongest modules in the codebase | `HnVue.Security.Tests` 138/138 pass, auth and RBAC integration flow pass |
| Clinical workflow and dose safety | `HnVue.Workflow`, `HnVue.Dose` | Owns state machine, exposure control, dose validation, and interlock rules | The domain logic is strong, but App still wires simulators for device execution | `HnVue.Workflow.Tests` 115/115 pass, `HnVue.Dose.Tests` 53/53 pass, workflow and dose integration tests pass |
| Patient and worklist handling | `HnVue.PatientManagement` | Handles patient CRUD and MWL-based registration flow | Service logic is implemented, but App still uses a null worklist repository in Phase 1d wiring | `HnVue.PatientManagement.Tests` 43/43 pass, patient integration flow pass |
| Incident handling | `HnVue.Incident` | Handles incident recording, response, and notification | Service layer is present, but runtime integration is still Phase 1d in shape | `HnVue.Incident.Tests` 57/57 pass |
| DICOM interoperability and media export | `HnVue.Dicom`, `HnVue.CDBurning` | Handles DICOM transport, file IO, and CD/DVD export | Good failure-path handling and simulated integration, but not full real-environment validation | `HnVue.Dicom.Tests` 60/60 pass, `HnVue.CDBurning.Tests` 46/46 pass, integration flows pass |
| Imaging and detector abstraction | `HnVue.Imaging`, `HnVue.Detector` | Handles image processing and detector abstraction | Imaging is stable. Detector is simulation-ready, but real SDK integration is not complete | `HnVue.Imaging.Tests` 54/54 pass, `HnVue.Detector.Tests` 11/11 pass |
| Admin and update operations | `HnVue.SystemAdmin`, `HnVue.Update` | Handles settings, audit export, OTA, backup, signature validation | Service logic is implemented, but App runtime still depends on null repositories and development defaults | `HnVue.SystemAdmin.Tests` 13/13 pass, `HnVue.Update.Tests` 72/72 pass |
| Presentation layer | `HnVue.UI.ViewModels`, `HnVue.UI` | Handles WPF UI, ViewModels, components, converters, and UI QA checks | Broadly implemented, but some flows are still placeholder or TODO-backed | `HnVue.UI.Tests` 227/228 pass, `HnVue.UI.QA.Tests` 39/39 pass |
| Composition root and runtime assembly | `HnVue.App` | Owns full DI assembly and application startup | Good development composition root, but not yet final production wiring | No dedicated App test project, indirect coverage via `HnVue.IntegrationTests` 18/18 pass |

## What Is Solid on Main

### 1. The module split is real

- `HnVue.Common` acts as the actual shared foundation layer.
- Service modules are split into separate projects instead of being merged into one runtime assembly.
- UI is split into contracts, ViewModels, and Views, which keeps dependency direction relatively clean.

### 2. Validation breadth is already strong

- The repository contains unit, integration, UI, UI-QA, and architecture tests.
- 16 of 17 test assemblies are green in the current run.
- Integration tests cover real cross-module scenarios, not only smoke checks.

### 3. Safety-oriented modules are not placeholders

- `Security`, `Workflow`, `Dose`, `Incident`, and `Update` have real responsibility and real logic.
- `Workflow` and `Dose` contain rule-driven behavior, not just CRUD.
- `Update` includes backup and signature validation responsibilities.

## Gaps the Main User Should Know

### 1. Hardware integration is still simulator-first

- `HnVue.App` currently wires `IGeneratorInterface` to `GeneratorSimulator`.
- `HnVue.App` currently wires `IDetectorInterface` to `DetectorSimulator`.
- `OwnDetectorAdapter` still throws `NotImplementedException` in the core hardware methods:
  - `ConnectAsync`
  - `DisconnectAsync`
  - `ArmAsync`
  - `AbortAsync`
  - `GetStatusAsync`

### 2. App composition still includes Phase 1d null or stub repositories

The current App runtime still wires placeholder repositories for:

- `IDoseRepository`
- `IWorklistRepository`
- `IIncidentRepository`
- `IUpdateRepository`
- `ISystemSettingsRepository`
- `HnVue.CDBurning.IStudyRepository`

This means the module code and the runtime wiring are not equally mature.

### 3. Some UI flows are not fully connected

- `MainViewModel` emergency navigation is still a TODO-backed path.
- `SettingsViewModel.SaveAsync()` is still a placeholder async delay instead of a real persistence call.
- Several ViewModel comments still mark unfinished runtime service connections.

### 4. Data protection is not fully closed

- `PatientEntity` still contains an explicit TODO for AES-256-GCM encryption of patient name, date of birth, and creator fields.
- This is not just normal technical debt. It is a direct security and compliance gap.

### 5. The UI test suite is not green today

- Current failure:
  - `HnVue.UI.Tests.UI.PerformanceTests.ResponseTime_ShouldMeetTarget(targetMs: 50, operation: "HoverEffect")`
  - Actual result: 51ms
  - Threshold: 50ms
- Additional warning:
  - `AccessibilityTests.TouchTarget_ShouldMeetMinimumSize` has duplicate `[InlineData(44, 44)]`, which causes a duplicate test case warning.

## Validation Result Summary

| Test Assembly | Result |
|---|---|
| `HnVue.Common.Tests` | 82/82 pass |
| `HnVue.Architecture.Tests` | 4/4 pass |
| `HnVue.PatientManagement.Tests` | 43/43 pass |
| `HnVue.Imaging.Tests` | 54/54 pass |
| `HnVue.CDBurning.Tests` | 46/46 pass |
| `HnVue.Data.Tests` | 102/102 pass |
| `HnVue.UI.QA.Tests` | 39/39 pass |
| `HnVue.Security.Tests` | 138/138 pass |
| `HnVue.IntegrationTests` | 18/18 pass |
| `HnVue.Dicom.Tests` | 60/60 pass |
| `HnVue.Dose.Tests` | 53/53 pass |
| `HnVue.Update.Tests` | 72/72 pass |
| `HnVue.UI.Tests` | 227/228 pass, 1 fail |
| `HnVue.Incident.Tests` | 57/57 pass |
| `HnVue.SystemAdmin.Tests` | 13/13 pass |
| `HnVue.Detector.Tests` | 11/11 pass |
| `HnVue.Workflow.Tests` | 115/115 pass |

## Release-Oriented QA Opinion

Current `main` is suitable for:

- architecture review
- parallel development baseline
- simulation-centered validation
- service and domain integration work

Current `main` is not yet suitable for:

- real detector or generator release
- production deployment with strong data-protection requirements
- a fully green release gate driven by the current UI test suite

## Recommended Actions for Main

1. Fix the single failing UI performance test and remove the duplicate theory data warning.
2. Replace detector SDK skeleton code and App simulator wiring together, not separately.
3. Replace null repository wiring in `HnVue.App` with real persistence or integration implementations.
4. Close the `PatientEntity` field encryption gap as a security and compliance task, not as a low-priority cleanup.
5. Close the remaining placeholder UI flows such as emergency registration and settings persistence.

## Final Conclusion

The current `main` branch has real modular structure, strong automated validation, and meaningful domain logic. From a QA perspective, it is a credible development baseline and a strong simulation-ready codebase. It is not yet a production-ready hardware-integrated release baseline. The key difference is not missing module count, but the remaining gap between implemented service modules and final runtime wiring.
