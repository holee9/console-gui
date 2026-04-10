# SPEC-UI-001: HnVue UI Redesign — Module Independence, Usability Evaluation, Modern Medical Design

---

## Metadata

| Field | Value |
|-------|-------|
| SPEC ID | SPEC-UI-001 |
| Title | HnVue UI Redesign — Module Independence, Usability Evaluation & Modern Medical Design |
| Status | Draft |
| Created | 2026-04-06 |
| Author | MoAI Strategic Orchestrator (ultrathink) |
| Regulatory References | IEC 62366-1:2015+AMD1:2020, FDA HFE Guidance, WCAG 2.2 AA |
| Baseline | SUS 82.3 (DOC-028 Usability Test Report v1.0) |
| Priority | High |

---

## 0. UISPEC Reference (UI Design Specifications)

> This SPEC implements the UI design specifications documented in UISPEC-001 through UISPEC-009. These documents serve as the bridge between PPT designs and code implementation.

| UISPEC ID | Document Title | PPT Reference | Implementation File | Status |
|-----------|---------------|---------------|---------------------|--------|
| **UISPEC-001** | 로그인 화면 UI 디자인 명세서 | Slide 1-3 | LoginView.xaml | 95% compliant |
| **UISPEC-002** | 워크리스트 화면 UI 디자인 명세서 | Slide 2-4 | PatientListView.xaml | 44% compliant |
| **UISPEC-003** | 스터디리스트 화면 UI 디자인 명세서 | Slide 5-7 | StudylistView.xaml | 63% compliant |
| **UISPEC-004** | 촬영(Acquisition) 화면 UI 디자인 명세서 | Slide 9-11 | WorkflowView.xaml, ImageViewerView.xaml | New |
| **UISPEC-005** | 환자/시술 추가 화면 UI 디자인 명세서 | Slide 8 | AddPatientProcedureView.xaml | New |
| **UISPEC-006** | 영상 병합(Merge) 화면 UI 디자인 명세서 | Slide 12-13 | MergeView.xaml | New |
| **UISPEC-007** | 설정(Settings) 화면 UI 디자인 명세서 | Slide 14-22 | SettingsView.xaml | New |
| **UISPEC-008** | 이미지 뷰어(ImageViewer) UI 디자인 명세서 | (Embedded) | ImageViewerView.xaml | New |
| **UISPEC-009** | 시스템 관리(SystemAdmin) UI 디자인 명세서 | Slide 14-22 | SystemAdminView.xaml | New |

**Location**: `docs/design/spec/`

**Purpose**: UISPEC documents provide:
- Screen-level layout specifications with pixel values
- Component design specifications
- Color token mappings (CoreTokens.xaml → SemanticTokens.xaml)
- State designs (default, focus, error, disabled)
- IEC 62366 usability considerations
- MRD/PRD traceability tables
- Implementation gap analysis

**Relationship to this SPEC**:
- This SPEC (SPEC-UI-001) defines **implementation requirements** (architecture, testing, CI/CD)
- UISPEC documents define **design specifications** (layout, colors, components, states)
- Together they form Layer 2 (UISPEC) and Layer 3 (SPEC) of the 3-layer document architecture

---

## 1. Background & Gap Analysis

### 1.1 What Was Already Accomplished

Previous sessions produced thorough analysis documents:

| Document | Content | Status |
|----------|---------|--------|
| HNVue_ARCHITECTURE_ANALYSIS.md | Module independence verified (9.6/10) | ✅ Done |
| UI_REDESIGN_STRATEGY.md | Pencil/Figma workflow proposed | ✅ Done |
| USABILITY_EVALUATION_FRAMEWORK.md | Heuristic + SUS + accessibility | ✅ Done |
| ui_design_plan_2026.md | Design tokens, 7-screen plan | ✅ Done |
| DOC-028 Usability Test Report | SUS 82.3 baseline established | ✅ Done |

The codebase already has:
- `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` — 3-tier design token system
- Dark/Light/HighContrast theme separation
- IEC 62366 safety colors (Safe=#00C853, Warning=#FFD600, Emergency=#D50000)
- `docs/ui_mockups/design_system.pen` — Pencil file exists

### 1.2 Critical Gaps (Why This SPEC Exists)

Three mandatory items were **not implemented** in previous sessions:

**GAP-1: Architecture independence is declared but NOT enforced**
- No compile-time/test-time boundary enforcement
- A developer can add a direct business module reference without detection
- NetArchTest or ArchUnitNET test suite is missing

**GAP-2: Design specifications not separated from implementation**
- UI design details were embedded in implementation specs
- No dedicated UISPEC layer to bridge PPT designs and code
- Design token mappings scattered across multiple documents

**GAP-3: Usability evaluation framework exists as document but has no execution pipeline**
- No integration with CI/CD or development workflow
- No formal change-management process connecting evaluation results to UI change decisions
- Rollback mechanism not implemented at code level

> **Note**: As of Phase C completion (2026-04-07), UISPEC-001~009 have been created to address GAP-2. This SPEC now references UISPEC documents for design details.

---

## 2. Objectives

1. **Enforce** UI module independence via automated architecture tests
2. **Separate** design specifications from implementation specs via UISPEC documents
3. **Implement** usability evaluation → change management pipeline at process and code level
4. **Deliver** modern medical device WPF XAML UI that passes IEC 62366 requirements

---

## 3. Scope

### In Scope
- `src/HnVue.UI/` — Views, Components, Styles, Themes
- `src/HnVue.UI.ViewModels/` — ViewModels
- `src/HnVue.UI.Contracts/` — Interface contracts
- `tests/HnVue.Architecture.Tests/` — New: architecture boundary tests
- `docs/design/spec/` — UISPEC-001~009 (UI design specifications) — **NEW**
- `docs/ui_mockups/` — HTML mockup reference files
- `docs/architecture/` — Architecture independence policy

### Out of Scope
- Business module logic (HnVue.Security, HnVue.Workflow, etc.)
- Database schema changes
- DICOM protocol changes

---

## 4. Functional Requirements (EARS Format)

### 4.1 Architecture Independence Requirements

**AR-001** (Ubiquitous)
The `HnVue.UI` assembly SHALL NOT contain any compile-time reference to the following business assemblies: HnVue.Data, HnVue.Security, HnVue.Workflow, HnVue.Imaging, HnVue.Dicom, HnVue.Dose, HnVue.PatientManagement, HnVue.Incident, HnVue.Update, HnVue.SystemAdmin, HnVue.CDBurning.

**AR-002** (Ubiquitous)
The `HnVue.UI.Contracts` assembly SHALL NOT contain any compile-time reference to implementation assemblies, accepting only HnVue.Common model types.

**AR-003** (Ubiquitous)
The `HnVue.UI.ViewModels` assembly SHALL depend only on `HnVue.UI.Contracts` (interfaces) and `HnVue.Common` (shared models), with no direct service implementation references.

**AR-004** (Ubiquitous)
All dependency injection registration for UI → Business module bindings SHALL be located exclusively in `HnVue.App/Program.cs` or the application composition root.

**AR-005** (Event-driven)
When a developer adds a direct business module reference to `HnVue.UI`, `HnVue.UI.ViewModels`, or `HnVue.UI.Contracts`, the architecture test suite SHALL fail with a descriptive error message identifying the violating type.

**AR-006** (Ubiquitous)
An architecture test project (`tests/HnVue.Architecture.Tests/`) using `NetArchTest.Fluent` SHALL be created and integrated into the solution build pipeline, running on every `dotnet test` invocation.

**AR-007** (Ubiquitous)
The file `docs/architecture/UI_INDEPENDENCE_POLICY.md` SHALL document: allowed dependencies per layer, forbidden dependencies, how to add new UI components, and how to register new business service bindings.

### 4.2 Design Specification Requirements (UISPEC)

**DT-001** (Ubiquitous)
UI design specifications SHALL be documented in separate UISPEC documents located in `docs/design/spec/`, following the 3-layer document architecture:
- Layer 1: MRD/PRD (business requirements)
- Layer 2: UISPEC (screen-level UI design specifications)
- Layer 3: SPEC/Code (implementation)

**DT-002** (Ubiquitous)
All UI design specifications SHALL reference the appropriate UISPEC document:
- Login screen → UISPEC-001
- Worklist screen → UISPEC-002
- Studylist screen → UISPEC-003
- Acquisition screen → UISPEC-004
- Add Patient screen → UISPEC-005
- Merge screen → UISPEC-006
- Settings screen → UISPEC-007
- Image Viewer → UISPEC-008
- System Admin → UISPEC-009

**DT-003** (Ubiquitous)
Each UISPEC document SHALL contain:
- Screen layout specifications with pixel values
- Component design specifications
- Color token mappings (CoreTokens.xaml → SemanticTokens.xaml)
- State designs (default, focus, error, disabled)
- IEC 62366 usability considerations
- MRD/PRD traceability tables
- Implementation gap analysis

**DT-004** (Safety-critical)
The Acquisition screen design (UISPEC-004) SHALL specify:
- Emergency Stop button: always visible, minimum 44×44px, color #D50000, labeled "STOP" or "비상 정지"
- Real-time radiation indicator (audible + visual)
- Patient ID display (28px minimum)
- Body part diagram selector (44×44px minimum touch targets)

**DT-005** (Ubiquitous)
WPF XAML Views SHALL use only design tokens from the `HnVue.Core.*` resource key namespace. Hardcoded color values in XAML are FORBIDDEN. Design token values SHALL be sourced from UISPEC documents.

**DT-006** (Process-driven)
A UISPEC-to-XAML implementation workflow SHALL be documented in `docs/architecture/DESIGN_TO_XAML_WORKFLOW.md`, covering:
1. UISPEC document review and approval
2. Design token extraction from UISPEC
3. WPF XAML implementation from UISPEC specifications
4. Design-implementation consistency validation
5. Gap analysis updates in UISPEC

### 4.3 Usability Evaluation → Change Management Pipeline

**UE-001** (Ubiquitous)
A formal Usability Evaluation Change Process SHALL be documented in `docs/usability/CHANGE_MANAGEMENT_PROCESS.md` defining the complete lifecycle from usability finding to UI change deployment.

**UE-002** (Event-driven)
When a usability issue is identified (from heuristic evaluation, task timing, or SUS survey), the issue SHALL be logged as a GitHub/Gitea issue with labels: `usability`, `ui-change`, and severity (`critical`/`high`/`medium`/`low`).

**UE-003** (Conditional)
Where a proposed UI change affects CRITICAL PATH screens (Acquisition, Login, Worklist patient selection), the change SHALL require:
1. Heuristic evaluation score ≥ 75/100 (baseline maintained or improved)
2. Task completion time within ±10% of baseline (current: 68 seconds average)
3. Zero increase in critical task error rate
4. Written sign-off from the responsible QA/RA engineer

**UE-004** (Conditional)
Where a proposed UI change affects non-critical screens (Merge, Settings, AddPatient), the change SHALL require:
1. Heuristic evaluation score ≥ 70/100
2. SUS score maintained above 78 (current baseline: 82.3)
3. Peer review approval in Gitea PR

**UE-005** (Event-driven)
When SUS score drops below 78 after a UI change is deployed, an automatic rollback SHALL be triggered by reverting the resource dictionary files (`DesignSystem2026.xaml` or screen-specific XAML) to the previous Git commit.

**UE-006** (Ubiquitous)
A rollback mechanism SHALL be implemented using WPF's `ResourceDictionary` hot-swap capability:
- Current design version stored as `Themes/HnVueTheme.xaml`
- Previous version retained as `Themes/HnVueTheme.previous.xaml`
- Rollback command: replace current with previous via `Application.Current.Resources.MergedDictionaries`

**UE-007** (Ubiquitous)
Usability metrics SHALL be tracked in `docs/usability/METRICS_HISTORY.md`:
- SUS score per version (baseline: 82.3, target: ≥ 85)
- Task completion time per workflow (baseline from DOC-028)
- Heuristic evaluation scores per screen
- Error counts per severity category

**UE-008** (Ubiquitous)
An A/B testing capability SHALL be designed (not necessarily implemented in v1) where a feature flag in `HnVueOptions.cs` can switch between two UI themes for selected users. This enables data-driven UI decisions.

### 4.4 Modern Medical Device UI Implementation (WPF XAML)

**UI-001** (Ubiquitous)
All 9 primary screens SHALL have WPF XAML Views implemented in `src/HnVue.UI/Views/` using the naming convention `{ScreenName}View.xaml`, following the design specifications in their corresponding UISPEC documents:
- LoginView.xaml → UISPEC-001
- PatientListView.xaml → UISPEC-002
- StudylistView.xaml → UISPEC-003
- WorkflowView.xaml, ImageViewerView.xaml → UISPEC-004
- AddPatientProcedureView.xaml → UISPEC-005
- MergeView.xaml → UISPEC-006
- SettingsView.xaml, SystemAdminView.xaml → UISPEC-007, UISPEC-009

**UI-002** (Ubiquitous)
All Views SHALL be built using:
- MahApps.Metro controls (already integrated)
- CommunityToolkit.Mvvm for MVVM bindings
- DesignSystem2026 / CoreTokens resources exclusively
- Material Design Symbols (Outlined) icon font for all icons

**UI-003** (Safety-critical)
The Acquisition View SHALL implement safety-critical UI elements as specified in UISPEC-004:
- Emergency Stop button (refer to UISPEC-004 Section 3.1)
- Real-time radiation indicator (refer to UISPEC-004 Section 3.4)
- Patient ID display (refer to UISPEC-004 Section 2.2)

**UI-004** (Ubiquitous)
All interactive elements SHALL meet minimum touch target requirements (44×44px) as specified in each UISPEC's "상태 디자인" section, per IEC 62366 and WCAG 2.2 requirements.

**UI-005** (Ubiquitous)
All Views SHALL support keyboard-only navigation with visible focus indicators, using the focus style defined in UISPEC documents (#00AEEF 2px outline).

**UI-006** (Ubiquitous)
The application SHALL support High Contrast Mode via the existing `Themes/high-contrast/HighContrastTheme.xaml`. All new components SHALL be tested against this theme.

**UI-007** (Ubiquitous)
Error messages and status indicators SHALL use both color AND text/icon (no color-only signaling) to ensure accessibility for color-blind users, as specified in each UISPEC's "상태 디자인" section.

**UI-008** (Event-driven)
Emergency/Blocked state visual indicators SHALL use the color #D50000 (Emergency) as specified in UISPEC-004 and UISPEC-007, with pulsing animation and audible alert, disabling all non-emergency UI interactions.

---

## 5. Non-Functional Requirements

### 5.1 Performance

| Metric | Requirement | Baseline |
|--------|-------------|---------|
| Screen load time | < 1 second | — |
| Search results | < 500ms | — |
| Image preview render | < 200ms | — |
| Button response | < 100ms | — |
| Emergency Stop response | < 50ms | — |

### 5.2 Regulatory Compliance

| Standard | Requirement |
|----------|-------------|
| IEC 62366-1:2015 | Usability engineering process compliance |
| FDA HFE Guidance | Safety-critical task analysis complete |
| WCAG 2.2 AA | All color contrasts ≥ 4.5:1 (normal text), ≥ 3:1 (large text) |
| AAMI HE75 | Touch target ≥ 44×44px |

### 5.3 Architecture Quality

| Metric | Requirement |
|--------|-------------|
| Architecture test coverage | 100% of boundary rules enforced |
| UI direct business deps | 0 (zero) |
| Design token hardcode violations | 0 (zero) |

---

## 6. Architecture Design

### 6.1 Module Dependency Map

```
┌─────────────────────────────────────────────────────────────────┐
│                       HnVue.App                                │
│  (Composition Root — ONLY place with full dependency access)    │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  DI Registration: ILoginViewModel → LoginViewModel        │ │
│  │                   ISecurityService → SecurityService       │ │
│  │                   (all interface → implementation)        │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────────┘
           ┌───────────────┼──────────────────┐
           ▼               ▼                  ▼
   ┌───────────────┐ ┌──────────────┐ ┌──────────────────┐
   │  HnVue.UI     │ │ HnVue.UI.VM  │ │ HnVue.UI.Contracts│
   │  (XAML Views) │ │ (ViewModels) │ │ (Pure Interfaces) │
   │  MahApps.Metro│ │ CToolkit.Mvvm│ │ No deps allowed   │
   └───────────────┘ └──────┬───────┘ └──────────────────┘
                            │  via interfaces only
           ┌────────────────┼─────────────────┐
           ▼                ▼                  ▼
   ┌──────────────┐ ┌──────────────┐ ┌──────────────────┐
   │HnVue.Security│ │HnVue.Workflow│ │  HnVue.Imaging   │
   │(impl only)   │ │(impl only)   │ │  (impl only)     │
   └──────────────┘ └──────────────┘ └──────────────────┘

   ENFORCED BY: NetArchTest.Fluent in HnVue.Architecture.Tests
```

### 6.2 Usability Evaluation Change Pipeline

```
User/QA identifies usability issue
          │
          ▼
  [LOG] Gitea issue (usability label + severity)
          │
          ▼
  [EVALUATE] Heuristic evaluation + SUS pre-change score
          │
          ├─── Critical screen? ──── YES ──→ Full evaluation required (UE-003)
          │                                   QA/RA sign-off required
          │
          └─── Non-critical? ──── YES ──→ Standard evaluation (UE-004)
                                           Peer review PR approval
          │
          ▼
  [DESIGN] Update UISPEC document (docs/design/spec/UISPEC-XXX.md)
          │
          ▼
  [REVIEW] Design review gate (architecture team)
          │
          ▼
  [IMPLEMENT] XAML implementation using UISPEC specifications
          │
          ▼
  [TEST] Heuristic re-evaluation + SUS post-change score
          │
          ├─── Score improved? ──── YES ──→ Deploy
          │
          └─── Score dropped? ──── YES ──→ Auto-rollback (UE-005/UE-006)
                                           Root cause analysis
```

### 6.3 UISPEC to WPF XAML Conversion Workflow

```
1. DESIGN PHASE (UISPEC)
   ├── Create/update UISPEC document (docs/design/spec/UISPEC-XXX.md)
   ├── Reference PPT designs (★HnVUE UI 변경 최종안_251118.pptx)
   ├── Apply CoreTokens color palette mappings
   ├── Document layout specifications (pixel values, components)
   └── Export HTML mockups for review (docs/ui_mockups/)

2. REVIEW PHASE
   ├── Architecture team review
   ├── QA/RA review (safety-critical screens: Acquisition, Login)
   ├── Design approval recorded in Gitea PR
   └── Update UISPEC "상태" field (Draft → Approved)

3. CONVERSION PHASE
   ├── Extract design tokens from UISPEC (verify match with CoreTokens.xaml)
   ├── Identify new components needed
   ├── Map to MahApps.Metro control equivalents
   └── Create component implementation plan

4. IMPLEMENTATION PHASE
   ├── Create/update View XAML file per UISPEC specifications
   ├── Use ONLY HnVue.Core.* resource keys (no hardcoded values)
   ├── Apply MahApps.Metro styles
   ├── Wire ViewModel bindings via interface
   └── Update UISPEC "구현 현황" section

5. VALIDATION PHASE
   ├── Verify implementation matches UISPEC specifications
   ├── High Contrast Mode test
   ├── Keyboard navigation test
   ├── Color contrast check (≥4.5:1)
   ├── Touch target size verification (≥44×44px)
   └── Update UISPEC "Gap 분석" section
```

---

## 7. Acceptance Criteria

### AC-1: Architecture Independence (AR-001 ~ AR-007)
- [ ] `tests/HnVue.Architecture.Tests/UILayerArchitectureTests.cs` exists and passes
- [ ] `dotnet test` includes architecture tests in the run
- [ ] Test fails when a forbidden dependency is manually added (verified by intentional test)
- [ ] `docs/architecture/UI_INDEPENDENCE_POLICY.md` documents all boundary rules

### AC-2: UISPEC Documents (DT-001 ~ DT-006)
- [ ] All 9 UISPEC documents exist in `docs/design/spec/`
- [ ] UISPEC-001 (Login) references LoginView.xaml implementation status
- [ ] UISPEC-004 (Acquisition) specifies Emergency Stop button design
- [ ] Each UISPEC contains MRD/PRD traceability tables
- [ ] Each UISPEC contains implementation gap analysis
- [ ] `docs/architecture/DESIGN_TO_XAML_WORKFLOW.md` documents UISPEC-to-XAML process

### AC-3: Usability Evaluation Pipeline (UE-001 ~ UE-008)
- [ ] `docs/usability/CHANGE_MANAGEMENT_PROCESS.md` documents full lifecycle
- [ ] `docs/usability/METRICS_HISTORY.md` contains current baseline (SUS 82.3)
- [ ] Rollback mechanism documented and prototype code provided
- [ ] Critical vs non-critical screen evaluation thresholds documented

### AC-4: WPF XAML Implementation (UI-001 ~ UI-008)
- [ ] All 9 Views exist in `src/HnVue.UI/Views/`
- [ ] Each View implements the design specified in its corresponding UISPEC
- [ ] AcquisitionView.xaml implements UISPEC-004 safety specifications
- [ ] All Views use ONLY HnVue.Core.* resource keys (no hardcoded values)
- [ ] All resource keys use `HnVue.Core.*` namespace (no hardcoded colors)
- [ ] High Contrast theme tested on all new Views
- [ ] Keyboard navigation verified for all interactive screens

---

## 8. Implementation Plan

### Phase 1: Architecture Enforcement (Priority: High)
1. Create `tests/HnVue.Architecture.Tests/` project
2. Add NetArchTest.Fluent package
3. Write architecture boundary tests
4. Create `docs/architecture/UI_INDEPENDENCE_POLICY.md`

### Phase 2: UISPEC Design Specifications (Priority: High)
1. Create 9 UISPEC documents in `docs/design/spec/`
2. Document PPT design specifications (Slide 1-22)
3. Extract design tokens from CoreTokens.xaml
4. Create MRD/PRD traceability tables
5. Document implementation gap analysis
6. Write `docs/architecture/DESIGN_TO_XAML_WORKFLOW.md`

### Phase 3: Usability Pipeline (Priority: Medium)
1. Create `docs/usability/CHANGE_MANAGEMENT_PROCESS.md`
2. Create `docs/usability/METRICS_HISTORY.md` (with baseline data)
3. Prototype rollback code in `src/HnVue.UI/Services/ThemeRollbackService.cs`
4. Define Gitea issue templates for usability findings

### Phase 4: WPF XAML Views (Priority: High)
1. Implement all 9 Views using UISPEC specifications + design tokens
2. Integrate with existing ViewModels
3. Apply MahApps.Metro styles
4. Validate High Contrast + keyboard navigation
5. Update UISPEC "구현 현황" sections

---

## 9. Risk Analysis

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| UISPEC documents incomplete | Medium | Low | Reference existing HTML mockups; document gaps explicitly |
| PPT design ambiguity | Medium | Medium | Document assumptions in UISPEC; get design sign-off |
| Architecture tests break existing build | Medium | Low | Run in separate test project; fix violations first |
| SUS score drops after new UI | High | Low | Incremental rollout; automatic rollback trigger |
| WCAG contrast failure | High | Low | Verify each color pair with contrast ratio tool pre-deployment |
| Design token mapping errors | Medium | Low | Automated tests to verify no hardcoded values in XAML |

---

## 10. References

| Document | Location |
|----------|---------|
| Architecture Analysis | `docs/HNVue_ARCHITECTURE_ANALYSIS.md` |
| UI Redesign Strategy | `docs/UI_REDESIGN_STRATEGY.md` |
| Usability Evaluation Framework | `docs/USABILITY_EVALUATION_FRAMEWORK.md` |
| UI Design Plan 2026 | `docs/ui_design_plan_2026.md` |
| Usability Test Report (Baseline) | `docs/testing/DOC-028_UsabilityTestReport_v1.0.md` |
| Core Design Tokens | `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` |
| **UISPEC Documents (NEW)** | **`docs/design/spec/UISPEC-001~009.md`** |
| HTML Mockup Reference | `docs/ui_mockups/` |
| MRD (v4.0) | `docs/planning/DOC-001_MRD_v3.0.md` (with Appendix F: UISPEC Traceability) |
| PRD (v2.0) | `docs/planning/DOC-002_PRD_v2.0.md` (with Appendix D: UISPEC Traceability) |
| IEC 62366 Usability File | `docs/testing/DOC-021_UsabilityFile_v2.0.md` |
| PPT Design Source | `docs/★HnVUE UI 변경 최종안_251118.pptx` |

---

Version: 1.0
Status: Draft — Approved for Implementation
