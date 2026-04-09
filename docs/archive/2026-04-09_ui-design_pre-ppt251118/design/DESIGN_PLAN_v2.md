# HnVue UI Design Plan v2.1

**Version**: 2.1 | **Date**: 2026-04-08 | **Status**: Approved Draft
**Author**: Team Design | **Approver**: Coordinator
**Supersedes**: DESIGN_PLAN_v1.0
**Changes**: P0/P1/P2 evaluator defects resolved — see Section 0 for change log

---

## 0. Change Log: v1.0 → v2.0

All evaluator-identified defects have been addressed. The table below maps each issue to its resolution location.

| Issue ID | Severity | Title | Resolution |
|----------|----------|-------|------------|
| P0-1 | Critical | Coordinator Team Ownership Violation | Section 1.5 + Section 11 (new) |
| P0-2 | Critical | Emergency Stop Height Compliance | Section 3.2 + Section 7.1 |
| P0-3 | Critical | Acquisition Parameter Buttons Safety Violation | Section 5 Phase 1 task P1-10 |
| P1-1 | High | "PROTYPER" Typo in Merge UISPEC | Section 4.6 note |
| P1-2 | High | Worklist/Studylist Navigation Contradiction | Section 2.1 + Section 2.2 |
| P1-3 | High | Acquisition Toolbar Button Duplication | Section 4.4 note |
| P1-4 | High | AddPatient as Modal vs NavigationToken | Section 1.5 + Section 2.3 |
| P2-1 | Important | Color Palette Decision for Merge | Section 6.2 |
| P2-2 | Important | Escape Key Conflict | Section 8.3 (new) |
| P2-3 | Important | WCAG Failure on Acquisition Screen | Section 7.3 + Section 5 Phase 2 |
| P2-4 | Important | Inter-Team Dependencies Table | Section 11 (new) |
| P2-5 | Important | CR/DR Modality Tokens | Section 6.1 note |

---

## 1. Architecture Decision: Navigation Model

### 1.1 Problem Statement

MainWindow.xaml currently implements a **fixed 3-column split panel** architecture:

```
MainWindow.xaml (CURRENT — WRONG)
├── Row 0: Header (48px)
├── Row 1: 3-Column Grid (always visible when authenticated)
│   ├── Col 0 (280px): PatientListView (ALWAYS visible)
│   ├── Col 1 (flex):   ImageViewerView (ALWAYS visible)
│   └── Col 2 (260px):  WorkflowView + DoseDisplayView (ALWAYS visible)
├── Login overlay (when not authenticated)
└── Row 2: StatusBar (28px)
```

This architecture violates the PPT design intent, which specifies each major screen (Worklist, Acquisition, Settings, Merge, etc.) as a **standalone full-screen view** with navigation between them. The current implementation shows all three panels simultaneously, making it impossible to display standalone screens like Settings, Merge, or Studylist.

Additionally, `INavigationService` is defined in `HnVue.UI.Contracts` but has **no implementation**. The `NavigationToken` enum exists with 9 tokens (Login, PatientList, Workflow, ImageViewer, DoseDisplay, CDBurn, SystemAdmin, QuickPinLock, Emergency) but is never used for actual view switching.

### 1.2 Decision: Single-Screen Navigation with ContentControl

**RECOMMENDED**: Replace the fixed 3-column grid with a single `ContentControl` that swaps its content via `INavigationService`.

**Rationale:**
- PPT design (22 slides) defines each screen as an independent full-screen layout
- Only the Acquisition screen (UISPEC-004) has its own internal 3-panel layout (left 230px + center flex + right 280px)
- Worklist (UISPEC-002) has its own 2-section layout (DataGrid + 230px Detail Panel)
- Settings (UISPEC-007) is a tabbed full-screen view
- Merge (UISPEC-006) is a 3-column comparison view
- Current architecture cannot support these varying layouts

### 1.3 MainWindow.xaml Required Changes

**Before (Current):**
```xml
<Grid Grid.Row="1">
    <!-- Fixed 3-column layout, always visible -->
    <Grid Visibility="{Binding IsMainContentVisible, ...}">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="280"/>  <!-- PatientList -->
            <ColumnDefinition Width="*"/>    <!-- ImageViewer -->
            <ColumnDefinition Width="260"/>  <!-- Workflow+Dose -->
        </Grid.ColumnDefinitions>
        ...
    </Grid>
    <!-- Login overlay -->
    <Border Visibility="{Binding IsLoginVisible, ...}">
        <views:LoginView .../>
    </Border>
</Grid>
```

**After (Proposed):**
```xml
<Grid Grid.Row="1">
    <!-- Single ContentControl swapped by NavigationService -->
    <ContentControl Content="{Binding CurrentView}"/>
</Grid>
```

The `MainWindowViewModel` manages `CurrentView` (type `object` or `UserControl`) which is set by the `NavigationService` implementation. Each `NavigationToken` maps to a view factory/resolver.

### 1.4 INavigationService Implementation Plan

The existing interface in `HnVue.UI.Contracts.Navigation.INavigationService` already defines the correct API:

```csharp
void NavigateTo(NavigationToken token);
void NavigateTo(NavigationToken token, object? parameter);
bool GoBack();
bool CanGoBack { get; }
event EventHandler<NavigationToken>? Navigated;
```

Implementation requirements:
- `NavigationService` class in `HnVue.App` (DI composition root) — **Coordinator team owns this file**
- View registry: `Dictionary<NavigationToken, Func<UserControl>>` for lazy view creation
- Navigation stack: `Stack<NavigationToken>` for GoBack() support
- Parameter passing: Navigation parameters (e.g., selected patient for Acquisition) stored per navigation
- `NavigationRequestedMessage` (already exists in UI.Contracts.Events) can be used for ViewModel-to-ViewModel communication

### 1.5 NavigationToken Additions Required

> **[P0-1 FIX] COORDINATOR OWNERSHIP NOTICE**: The `NavigationToken` enum resides in `HnVue.UI.Contracts`, which is **Coordinator-owned territory**. Team Design CANNOT modify this file unilaterally. All changes described in this section MUST follow the process defined in Section 11 (Cross-Team Dependencies).

Current `NavigationToken` enum is missing several screens. Required additions:

| Token | Screen | Status | Owner |
|-------|--------|--------|-------|
| Login | LoginView | EXISTS | Coordinator |
| PatientList | WorklistView (renamed) | EXISTS (name mismatch) | Coordinator |
| Workflow | AcquisitionView | EXISTS (name mismatch) | Coordinator |
| ImageViewer | ImageViewerView | EXISTS | Coordinator |
| DoseDisplay | DoseDisplayView | EXISTS (becomes sub-component) | Coordinator |
| CDBurn | CDBurnView | EXISTS | Coordinator |
| SystemAdmin | SystemAdminView | EXISTS | Coordinator |
| QuickPinLock | QuickPinLockView | EXISTS | Coordinator |
| Emergency | EmergencyView | EXISTS | Coordinator |
| **Studylist** | StudylistView | **NEW — requires Coordinator approval** | Coordinator |
| **Merge** | MergeView | **NEW — requires Coordinator approval** | Coordinator |
| **Settings** | SettingsView | **NEW — requires Coordinator approval** | Coordinator |
| **Acquisition** | AcquisitionView (replaces Workflow) | **RENAME — requires Coordinator approval** | Coordinator |
| **Worklist** | PatientListView (replaces PatientList) | **RENAME — requires Coordinator approval** | Coordinator |

> **[P1-4 FIX] AddPatient REMOVED**: AddPatient is a **modal dialog**, NOT a full navigation target. It does NOT belong in `NavigationToken` enum. AddPatient is launched via `IDialogService` (or `window.ShowDialog()`) as an overlay on top of the Worklist screen. See Section 2.3 for the modal vs navigation distinction.

> **[P0-1 FIX] SUMMARY**: Phase 0 CANNOT start until the Coordinator team:
> 1. Approves the 6 new/renamed NavigationToken additions (Studylist, Merge, Settings, Acquisition rename, Worklist rename, and removal of AddPatient)
> 2. Merges the `NavigationToken.cs` change and files the `interface-contract` issue
> 3. Creates the `NavigationService.cs` implementation in `HnVue.App`
>
> Team Design prepares the implementation draft for the NavigationService, but Coordinator approves and commits it.

> **Note**: `DoseDisplay` and `ImageViewer` are no longer top-level navigation targets. They become sub-components of the Acquisition screen. The `Workflow` token should be renamed to `Acquisition` to match PPT terminology.

---

## 2. Screen Navigation Map

### 2.1 Navigation Flow Diagram

> **[P1-2 FIX] NAVIGATION DECISION**: Worklist and Studylist are **SEPARATE full-screen views** (confirmed by PPT slides 2-4 for Worklist and slides 5-7 for Studylist showing each as a standalone screen). The Worklist screen's tab header contains "Worklist" and "Studylist" radio buttons that navigate to their respective full-screen views via `INavigationService`. There is NO "tab switch within same screen" behavior.

```
                    [App Launch]
                         |
                    +----v----+
                    |  Login   |
                    +----+----+
                         |
                         | (successful auth)
                         |
                    +----v----+
             +------+ Worklist +------+
             |      +----+----+      |
             |           |           |
  +----------v--+    +---v----+  +---v------+
  | Acquisition |    |Settings|  |  Merge   |
  | (own 3-pnl) |    |(tabbed)|  |(3-column)|
  +----------+--+    +---+----+  +---+------+
             |           |           |
             +-----------+-----------+
                         |
                         | (Back / GoBack)
                    +----v----+
                    | Worklist |
                    +---------+

    Worklist header radio button "Studylist" --NavigateTo(Studylist)--> StudylistView (full screen)
    StudylistView header radio button "Worklist" --NavigateTo(Worklist)--> WorklistView (full screen)

    Any Screen --> Login (logout)
    Any Screen --> SystemAdmin (admin access)
```

### 2.2 Navigation Trigger Table

| Source Screen | Trigger | Target Screen | Parameter | Navigation Type |
|---------------|---------|---------------|-----------|-----------------|
| Login | Successful authentication | Worklist | User session | NavigateTo |
| Worklist | Patient double-click or Acquisition button | Acquisition | Selected PatientId, StudyId | NavigateTo |
| Worklist | Settings button (header) | Settings | None | NavigateTo |
| Worklist | Merge button (toolbar) | Merge | None | NavigateTo |
| Worklist | SystemAdmin (header/admin) | SystemAdmin | None | NavigateTo |
| **Worklist** | **"Studylist" radio button in header** | **Studylist (full screen)** | **None** | **NavigateTo** |
| **Studylist** | **"Worklist" radio button in header** | **Worklist (full screen)** | **None** | **NavigateTo** |
| Worklist | Add Patient button | AddPatient | None | **IDialogService (modal)** |
| Studylist | Row double-click | Acquisition | Selected StudyId | NavigateTo |
| Acquisition | Back button | Worklist | None | GoBack |
| Acquisition | Study complete | Worklist | Updated study status | NavigateTo |
| Settings | Close/Cancel/Save | Worklist | None | GoBack |
| Merge | Close/Cancel | Worklist | None | GoBack |
| SystemAdmin | Close | Worklist | None | GoBack |
| Any screen | Logout button | Login | Clear session | NavigateTo |
| Any screen | Emergency Stop | (No navigation) | Abort current operation | Command (no nav) |

### 2.3 Modal vs Navigation

Some screens are modal overlays, not full navigation targets. **These screens must NOT be added to NavigationToken.**

| Screen | Type | Launch Mechanism | Reason |
|--------|------|-----------------|--------|
| AddPatient | Modal dialog | `IDialogService.ShowDialog()` | PPT Slide 8: dialog over Worklist. NOT a NavigationToken. |
| QuickPinLock | Modal overlay | `IDialogService.ShowDialog()` | Security lock over current screen |
| CDBurn | Modal overlay | `IDialogService.ShowDialog()` | Utility dialog |
| Emergency | Not a screen | Command execution | Action trigger, no navigation |

---

## 3. MainWindow Shell Redesign

### 3.1 New Shell Layout

```
+-------------------------------------------------------------+
|  Header (60px): Logo + Nav Buttons + Emergency STOP (ALWAYS)|
+-------------------------------------------------------------+
|  TLS Warning Banner (conditional, top of content)           |
+-------------------------------------------------------------+
|                                                             |
|  ContentControl -- SWAPS views via NavigationService        |
|  (LoginView / WorklistView / AcquisitionView / SettingsView |
|   / MergeView / StudylistView / SystemAdminView)            |
|                                                             |
+-------------------------------------------------------------+
|  Session Timeout Banner (conditional, bottom overlay)       |
+-------------------------------------------------------------+
|  Status Bar (28px): Section name | DICOM connection status  |
+-------------------------------------------------------------+
```

### 3.2 Header — Height Increased to 60px

> **[P0-2 FIX] EMERGENCY STOP HEIGHT DECISION**:
>
> **DECISION: Option A — Increase app header from 48px to 60px.**
>
> The current 48px header restricts the Emergency Stop button to approximately 32px effective touch height, which falls below the IEC 62366 required minimum of 44px. This is a **safety compliance failure**.
>
> Chosen resolution: Increase the app header height from 48px to **60px**. The Emergency Stop button fills the header height with 6px top/bottom padding, yielding a **48px touch target** — compliant with IEC 62366 44px minimum and exceeding the recommended 44px by 4px.
>
> **Phase 0 task P0-00 added**: "Increase app header height from 48px to 60px and verify Emergency Stop button touch target reaches 48px."

| Element | Current | Change |
|---------|---------|--------|
| Header height | 48px | **60px** (P0-2 fix) |
| Logo badge ("H") | 28x28 Accent | Keep |
| "HnVue Console" title | 16px white | Keep |
| Emergency STOP button | ~32px effective height | **48px** (6px top/bottom padding in 60px header) |
| Username + Role | Visible when authenticated | Keep |
| Logout button | Outline style | Keep |
| **Navigation buttons** | Not present | **ADD**: Worklist / Settings / Merge / Admin buttons (visible when authenticated) |

Navigation buttons appear only when `IsAuthenticated == true`. They replace the current implicit "you're always on the same 3-panel screen" assumption.

### 3.3 Content Area — Single ContentControl

The entire Row 1 becomes a single `ContentControl`:

```xml
<ContentControl Grid.Row="1" Content="{Binding CurrentView}"/>
```

Each view is a self-contained `UserControl` that manages its own internal layout. The shell has no knowledge of individual view layouts.

### 3.4 Status Bar (28px) — Preserved

Current status bar implementation is correct and should be preserved:
- Left: Active section name (`{Binding ActiveNavItem}`)
- Right: DICOM connection status indicator (green/red dot + text)

### 3.5 Persistent Elements (Always Visible)

These elements remain visible regardless of which screen is active:

| Element | Location | IEC 62366 Requirement | Compliance |
|---------|----------|----------------------|------------|
| Emergency STOP button | Header, right side, 48px height | Always visible, always enabled, Escape key | **PASS** (after 60px header fix) |
| TLS Warning banner | Below header, conditional | Security warning visibility | PASS |
| Session Timeout banner | Above status bar, conditional | User awareness | PASS |
| DICOM connection status | Status bar, right | Connection awareness | PASS |

---

## 4. Per-Screen Design Specifications

### 4.1 Login (UISPEC-001)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-001_Login.md |
| Implementation File | `Views/LoginView.xaml` |
| Compliance | **95%** (Slide 2 original basis) |
| Priority | **P3** (nearly complete) |

**Layout:** Full-screen dark navy (#1c2333), centered 220px form, minimal design (no labels, no placeholders, icon-only buttons).

**Key Components:**
- Logo image (160x~80px)
- Username TextBox (Height=36)
- PasswordBox (Height=36)
- Confirm button (40x40, checkmark icon, IsDefault=True)
- Cancel button (40x40, X icon, IsCancel=True)
- Error message TextBlock (#e74c3c)
- Loading ProgressBar (2px)

**Current Compliance (95%):**
- All core elements implemented per Slide 2 original screenshot
- MVVM bindings complete (LoginCommand, CancelCommand, ErrorMessage, IsLoading)
- Tab order defined (0-3)

**Missing (5%):**
- Focus border color (#1a6fc7 2px) verification needed in custom styles
- MainWindow DragMove() verification for borderless window
- MR-UI-002 (ComboBox dropdown) decision pending (currently TextBox per Slide 2)

**Phase 3 Items:**
- Hardcoded #1c2333 should migrate to CoreTokens
- AutomationProperties.Name for screen reader support

---

### 4.2 Worklist (UISPEC-002)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-002_Worklist.md |
| Implementation File | `Views/PatientListView.xaml` |
| Compliance | **44%** (PPT design basis) |
| Priority | **P1** (main work screen, critical gaps) |

**Layout:** Full-screen, 2-section horizontal split:
- Left (flex:1): App header + Toolbar + Filter bar + DataGrid (12 columns)
- Right (230px): Detail Panel (Study info + Viewer preview + Action buttons)

**Header Tab Navigation (Worklist/Studylist radio buttons):**
- The Worklist screen header contains radio-style tab buttons: "Worklist" (selected) and "Studylist"
- Clicking "Studylist" calls `NavigateTo(NavigationToken.Studylist)` — navigates to the full StudylistView
- Studylist header similarly has "Worklist" and "Studylist" buttons; clicking "Worklist" navigates back

**Key Components:**
- App Header (30px): Logo "HnVUE" + tab navigation (Worklist/Study/Report/Admin/Statistics)
- Worklist Toolbar (32px): Section badge + action buttons (Register/Current/Reported/Delete)
- Filter Bar (28px): Date range + Modality dropdown + Status dropdown + Keyword search
- DataGrid: 12 columns including Modality badges, Status dots, urgency left borders
- Detail Panel (230px): Patient info + Study info + Viewer preview + Action buttons

**What's Implemented (44%):**
- Section badge + count badge
- Search box
- Period filter buttons (Today/3Days/1Week/All/1Month)
- Register/Current/Reported/Delete buttons (buttons exist, some non-functional)
- DataGrid with 8 columns (PatientId, Name, Sex, DOB, CreatedAt, Status badge, CreatedBy)
- Detail panel 230px width allocated

**Critical Gaps (56% missing):**

| Gap | Priority | MRD |
|-----|----------|-----|
| Modality column + color badges (CT/MR/XR/US/NM/CR/DR) | P1 | MR-WL-005 |
| Status column with dot indicators (8px colored dots) | P1 | MR-WL-003 |
| Row left 3px urgency border (DataTrigger) | P1 | MR-WL-003 |
| Detail Panel content (InfoSection x2, patient/study info) | P1 | MR-WL-004 |
| ExamDate (StudyDate) column | P1 | MR-WL-001 |
| Modality badge color tokens (CoreTokens.xaml) — all 7 including CR/DR | P1 | MR-WL-005 |
| Worklist/Studylist radio button navigation in header | P1 | Navigation |
| App header tab navigation | P2 | MR-WL-006 |
| Modality/Status dropdown filters | P2 | MR-WL-002 |
| From/To DatePicker filters | P2 | MR-WL-002 |
| Detail Panel ViewerPreview area | P2 | MR-WL-004 |
| Detail Panel ActionButtons (6 buttons) | P2 | MR-WL-004 |
| StudyDescription column | P2 | — |
| Scrollbar custom style (5px, #2e4070) | P4 | — |

---

### 4.3 Studylist (UISPEC-003)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-003_Studylist.md |
| Implementation File | `Views/StudylistView.xaml` |
| Compliance | **63%** (core structure done) |
| Priority | **P2** (important but not blocking) |

**Layout:** Full-screen standalone view (PPT slides 5-7):
- Row 0 (48px): Header (Prev/Next arrows + "Study List" + PACS ComboBox + Worklist/Studylist radio navigation)
- Row 1 (Auto): Filter bar (period buttons + search input)
- Row 2 (flex): DataGrid (9 columns with Modality/Status/Priority)
- Row 3 (Auto): Status bar

**What's Implemented (63%):**
- Prev/Next navigation arrows
- PACS server ComboBox
- Period filter buttons
- Search input box
- DataGrid (PatientId, AccNo, StudyDate, BodyPart, Description)
- Status bar (error message + count)

**Missing (37%):**

| Gap | Priority | MRD |
|-----|----------|-----|
| Modality badge column | P1 | MR-SL-001 |
| Status indicator column (dots) | P1 | MR-SL-001 |
| PatientName column | P1 | MR-SL-001 |
| Worklist/Studylist radio navigation buttons in header | P1 | Navigation |
| Priority indicator | P2 | MR-SL-003 |
| PACS connection status display (dots in ComboBox) | P2 | MR-SL-001 |
| Urgent row left border | P3 | — |
| Modality side filter panel | P3 | MR-SL-002 |
| Row double-click detail modal | P3 | MR-SL-001 |

---

### 4.4 Acquisition (UISPEC-004) -- CRITICAL

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-004_Acquisition.md |
| Implementation File | `Views/AcquisitionView.xaml` (**does not exist**) |
| Compliance | **0% XAML** (no view file) / ~30% conceptual (WorkflowView + ImageViewerView + DoseDisplayView exist as fragments) |
| Priority | **P0** (safety-critical, IEC 62366) |

**Layout:** Full-screen, own internal 3-panel layout:
- App Header (**60px**, inherited from shell after P0-00 fix, contains Emergency STOP): "HnVUE" + navigation tabs + Emergency STOP
- Section Toolbar (32px): Section badge + Protocol dropdown + Description + action buttons
- Left panel (230px): Patient info + Exam info + Alerts + Device status
- Center panel (flex): DICOM Image Viewer + Thumbnail strip (80px)
- Right panel (280px): Exposure parameters (kVp/mAs/SID) + Grid/AEC toggle + Dose monitor + Expose/Cancel/Retake/Delete buttons
- Status Bar (44px): Connection status + completion count

> **[P1-3 FIX] TOOLBAR BUTTON CLARIFICATION**: The Section Toolbar (32px row) at the top of the Acquisition screen shows the protocol dropdown and study description. The "촬영 시작" (Expose), "촬영 취소" (Cancel), "재촬영" (Retake) action buttons that appear in the toolbar area per the UISPEC layout diagram are **informational section identifiers only and must NOT be duplicated in the toolbar**. All acquisition action buttons (Expose, Cancel, Retake, Delete) belong **exclusively in the right control panel** (280px right column). The toolbar "촬영 시작/취소/재촬영" entries in the UISPEC-004 layout diagram represent the section name badge, not interactive buttons. This prevents accidental activation.

**This screen is the ONLY screen that has a 3-column internal layout**, matching the current MainWindow structure. However, the current MainWindow splits this across separate ViewModels bound to fixed columns, which is incorrect. The 3-panel layout must be self-contained within `AcquisitionView.xaml`.

**Key Safety Components (IEC 62366):**
- Emergency STOP: Header-level (inherited from shell), always visible
- Expose button: 44px height minimum, blue primary, disabled when equipment not ready
- Device status indicators: Generator/Detector/Bucky/Collimator with colored dots + text
- LIVE badge: Pulsing red animation during exposure
- Dose monitor: Real-time progress bar with threshold colors (safe/warning/blocked/emergency)

**Acquisition parameter ±buttons — IEC 62366 compliance timeline:**
- Phase 1: Minimum height 32px (up from 18px in UISPEC-004 spec)
- Phase 2: Target height 44px (full IEC 62366 touch target compliance)

**Implementation Gap:**
- `AcquisitionView.xaml` must be created from scratch
- `AcquisitionViewModel.cs` must be created
- `DeviceStatusToBrushConverter`, `AcquisitionStateToBrushConverter` needed
- Service interfaces exist (`IDetectorService`, `IGeneratorService`, `IDicomImageService`) but no view integration (Team B provides these — see Section 11)

**Existing fragments to merge:**
- `WorkflowView.xaml` — Exposure controls (PREPARE/EXPOSE/STOP buttons)
- `ImageViewerView.xaml` — DICOM viewer with toolbar
- `DoseDisplayView.xaml` — Dose gauge display

These must be consolidated into a single `AcquisitionView.xaml` with proper internal layout.

---

### 4.5 AddPatient (UISPEC-005)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-005_AddPatient.md |
| Implementation File | `Views/AddPatientProcedureView.xaml` |
| Compliance | **75%** (form structure done, some fields missing) |
| Priority | **P2** (functional, needs polish) |
| **Launch Type** | **Modal dialog via IDialogService — NOT a NavigationToken** |

**Layout:** Modal dialog (700x650), 2-column form:
- Left: Patient Info (PatientID + AutoGenerate, Name*, DOB*, Gender*)
- Right: Procedure Info (AccNo + AutoGenerate, View Projection chips, Description chips, RIS Code)
- Bottom: Error banner + Cancel/Save buttons

**What's Implemented:**
- 2-column panel layout with section icons
- Patient ID + Auto-Generate toggle
- Patient Name, Birth Date, Gender inputs
- Accession Number + Auto-Generate
- View Projection chip UI (ComboBox + Add button + chip list)
- Description chip UI (editable ComboBox + chip list)
- RIS Code input
- Error banner (Row 2, conditional)
- Cancel/Save buttons

**Missing:**

| Gap | Priority | MRD |
|-----|----------|-----|
| Emergency patient toggle/flag | P1 | MR-PT-003 |
| Duplicate patient detection warning | P1 | MR-PT-001 |
| Birth Date: TextBox to DatePicker upgrade | P2 | MR-PT-002 |
| Chip delete X button touch target (20px to 44px) | P2 | IEC 62366 |
| Cancel/Save button height (36px to 44px) | P2 | IEC 62366 |
| Referring Physician field | P3 | MR-PT-004 |
| Clinical Info field | P3 | MR-PT-004 |

---

### 4.6 Merge (UISPEC-006) -- CRITICAL

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-006_Merge.md |
| Implementation File | `Views/MergeView.xaml` (**does not exist**) |
| Compliance | **0%** (XAML not created) |
| Priority | **P1** (safety: patient data merge requires ID validation) |

> **[P1-1 FIX] UISPEC TYPO**: UISPEC-006 line 79 (Section 2.2 Header spec) contains "PROTYPER" in the logo specification. This is a **copy-paste error from a prototype document**. **All Merge screen implementations MUST use "HnVUE" branding**, consistent with all other screens. The "PROTYPER" text must never appear in the production implementation. The UISPEC-006 document should be corrected in a separate RA/doc update.

> **[P2-1 FIX] COLOR PALETTE DECISION**: The Merge screen MUST use the **Worklist navy palette** (`#0d1527` series), NOT the gray palette (`#2A2A2A` series) specified in UISPEC-006's color token table. UISPEC-006 was authored with the older gray palette. The authoritative PPT design and all new screen implementations use the Worklist navy palette. This decision is binding for all Merge screen implementation work.

**Authoritative Color Palette for Merge Screen:**

| Element | Use Worklist Navy | Do NOT use UISPEC-006 Gray |
|---------|------------------|---------------------------|
| Page background | `#0d1527` | ~~`#242424`~~ |
| Panel background | `#1a2540` | ~~`#2A2A2A`~~ |
| Card/input background | `#152035` | ~~`#3B3B3B`~~ |
| Primary text | `#e0e6f0` | ~~`#FFFFFF`~~ |
| Secondary/muted text | `#7090b0` | ~~`#B0BEC5`~~ |
| Default border | `#2e4070` | ~~`#3B3B3B`~~ |

**Layout:** Full-screen, 3-column comparison:
- Header (60px): Logo "HnVUE" + "Merge" title (NOT "PROTYPER")
- Selection Bar (80px): Study A / Study B search inputs
- Content: Panel A (flex) + Center Controls (280px) + Panel C (flex)
- Footer (64px): Study info + Cancel/Save
- Status Bar (32px): Status messages

**Key Safety Components:**
- Patient ID mismatch detection: Auto-block merge when IDs differ
- Patient name mismatch: Confirmation dialog required
- Study date mismatch (>7 days): Warning display

**Implementation Requirements:**
- `MergeView.xaml` must be created using Worklist navy color tokens
- `MergeViewModel.cs` must be created
- 4 merge modes: Horizontal, Vertical, Overlay, Compare
- Drag-and-drop image strip support (Phase 2)
- Image preview panel

---

### 4.7 Settings (UISPEC-007)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-007_Settings.md |
| Implementation File | `Views/SettingsView.xaml` |
| Compliance | **80%** (tab structure done, content placeholders) |
| Priority | **P2** (structure good, content needs filling) |

**Layout:** Full-screen, 4-row grid:
- Row 0 (48px): Header with gear icon + "Settings"
- Row 1 (Auto): Horizontal scrolling tab row (10 tabs)
- Row 2 (flex): Tab content area (ScrollViewer)
- Row 3 (Auto): Cancel/Save action bar

**10 Tabs (per PPT Slides 14-21):**
1. System — Priority settings, Access Notice
2. Account — User management, permissions (all dropdowns per PPT)
3. Detector — FPD settings (preserve current structure per PPT Slide 16)
4. Generator — X-ray generator (preserve current structure per PPT Slide 16)
5. Network — PACS + Worklist + Print unified (PPT Slide 17 consolidation)
6. Display — Marker, Annotation, Overlay settings
7. Option — Image storage, delete options, import
8. Database — Capacity/cleanup settings
9. DicomSet — DICOM tag management
10. RIS Code — RIS Code matching (Matching/Un-Matched sub-tabs)

**What's Implemented (80%):**
- Tab navigation structure with ToggleButton row
- All 10 tabs defined
- MVVM ActiveTab binding
- Cancel/Save action bar

**Missing (20%):**
- Most tab content panels are placeholder text only
- Network tab: PACS+Worklist+Print consolidation not implemented
- RIS Code: Matching/Un-Matched sub-tab switching
- Account: Dropdown conversion (currently some text inputs)
- Conditional UI (Detector mapfile, Generator port) not implemented

---

### 4.8 ImageViewer (UISPEC-008)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-008_ImageViewer.md |
| Implementation File | `Views/ImageViewerView.xaml` |
| Compliance | **70%** (basic layout done, tools/overlay missing) |
| Priority | **P2** (becomes sub-component of Acquisition) |

**Layout:** 3-row grid:
- Row 0 (Auto): "Image Viewer" header
- Row 1 (Auto): Toolbar (Zoom In/Out, Reset W/L)
- Row 2 (flex): Image display area (#090909 background)

**What's Implemented:**
- Header with title
- Toolbar with outline-style buttons (ZOOM IN/OUT, RESET W/L)
- Image placeholder area with zoom text display
- White outline button style matching screenshots

**Missing:**
- DICOM overlay information (patient name, kV/mAs, W/L values)
- Additional tools (Pan, Measure, Rotate, Flip)
- Measurement annotations
- Multi-frame support
- Standalone popup window mode

**Role Change:** With navigation redesign, ImageViewerView becomes an embedded component inside AcquisitionView (center panel), not a top-level navigation target. Standalone popup mode is a separate feature.

---

### 4.9 SystemAdmin (UISPEC-009)

| Attribute | Value |
|-----------|-------|
| UISPEC Reference | UISPEC-009_SystemAdmin.md |
| Implementation File | `Views/SystemAdminView.xaml` |
| Compliance | **40%** (basic grid structure only) |
| Priority | **P3** (admin-only, lower user impact) |

**Layout:** 4-row grid:
- Row 0 (Auto): "System Administration" title
- Row 1 (Auto): Load/Save Settings buttons
- Row 2 (Auto): Status message
- Row 3 (flex): Tab content (5 planned sections)

**Planned Tabs:**
1. User Management — Account CRUD, role assignment
2. Role & Permissions — Permission matrix
3. Audit Log — System activity log viewer
4. License — License management
5. System Diagnostics — Health checks, version info

**What's Implemented (40%):**
- Basic Grid structure
- Load/Save Settings buttons with IsAdminUser binding
- IsBusy indicator
- Status message TextBlock

**Missing (60%):**
- Tab navigation (5 tabs not implemented)
- User management DataGrid
- Role & permission matrix
- Audit log viewer
- License management panel
- System diagnostics panel

---

## 5. Implementation Priority Roadmap

### Phase 0: Architecture Fix (P0 — PREREQUISITE)

**Goal:** Enable single-screen navigation, unblock all other phases.

**PREREQUISITE: Coordinator approval of NavigationToken changes (see Section 11) must be completed before Phase 0 tasks P0-01 and P0-02 can begin. Other P0 tasks (P0-00, P0-03 through P0-07) can start in parallel with Coordinator review.**

| Task | Description | Files Affected | Blocker |
|------|-------------|----------------|---------|
| **P0-00** | **Increase app header height from 48px to 60px** (P0-2 fix) | `MainWindow.xaml` | None |
| P0-01 | Implement `NavigationService` class | `HnVue.App/Services/NavigationService.cs` (new) | **Coordinator approval required** |
| P0-02 | Add/rename NavigationTokens (Studylist, Merge, Settings, Acquisition, Worklist) | `NavigationToken.cs` | **Coordinator team files interface-contract issue** |
| P0-03 | Redesign MainWindow.xaml shell | `MainWindow.xaml` — replace 3-column grid with ContentControl | P0-00 complete |
| P0-04 | Update MainWindowViewModel | `MainWindowViewModel.cs` — add CurrentView, inject INavigationService | P0-01 complete |
| P0-05 | Register views in DI container | `App.xaml.cs` or `ServiceCollectionExtensions.cs` | **Coordinator owns this file** |
| P0-06 | Add navigation buttons to header | `MainWindow.xaml` header — Worklist/Settings/Merge/Admin buttons | P0-03 complete |
| P0-07 | Wire LoginView to navigate to Worklist on auth | `LoginViewModel.cs` or `MainWindowViewModel.cs` | P0-04 complete |

### Phase 1: Critical Screens (P1)

**Goal:** Complete the two most critical screens: Worklist detail and Acquisition view. Fix P0-3 safety violation for parameter buttons.

| Task | Description | Screen | Dependency |
|------|-------------|--------|------------|
| P1-01 | Create AcquisitionView.xaml (3-panel internal layout) | Acquisition | P0 complete |
| P1-02 | Consolidate WorkflowView + ImageViewerView + DoseDisplayView into AcquisitionView | Acquisition | P1-01 |
| P1-03 | Implement Modality badge column + color tokens (all 7: CT/MR/XR/US/NM/CR/DR) | Worklist | None |
| P1-04 | Implement Status dot indicator column | Worklist | None |
| P1-05 | Implement row left 3px urgency border (DataTrigger) | Worklist | None |
| P1-06 | Complete Detail Panel content (InfoSection x2) | Worklist | None |
| P1-07 | Add ExamDate (StudyDate) column | Worklist | None |
| P1-08 | Create MergeView.xaml with patient ID validation (using Worklist navy palette) | Merge | P0 complete |
| P1-09 | Implement Modality badge color tokens in CoreTokens.xaml (all 7 modalities) | Tokens | None |
| **P1-10** | **Acquisition parameter ±buttons: increase from 18px to minimum 32px height** (P0-3 fix) | Acquisition | P1-01 |
| P1-11 | Add Worklist/Studylist navigation radio buttons to Worklist header | Worklist | P0 complete |
| P1-12 | Add Worklist/Studylist navigation radio buttons to Studylist header | Studylist | P0 complete |

### Phase 2: Important Screens (P2)

**Goal:** Complete secondary screens, missing features, and remaining safety compliance items.

| Task | Description | Screen |
|------|-------------|--------|
| P2-01 | Worklist filter bar: Modality/Status dropdowns + DatePicker | Worklist |
| P2-02 | Worklist Detail Panel: ViewerPreview + ActionButtons | Worklist |
| P2-03 | Studylist: Modality badge + Status + PatientName columns | Studylist |
| P2-04 | Studylist: PACS connection status display | Studylist |
| P2-05 | AddPatient: Emergency toggle + Duplicate detection | AddPatient |
| P2-06 | Settings: Fill remaining tab content panels | Settings |
| P2-07 | Settings: Network tab consolidation (PACS+Worklist+Print) | Settings |
| P2-08 | ImageViewer: DICOM overlay information | ImageViewer |
| P2-09 | App header tab navigation (Worklist/Study/Report/Admin) | Shell |
| **P2-10** | **Acquisition placeholder color: fix `#5580a0` to `#7090b0` (WCAG AA compliance)** (P2-3 fix) | Acquisition |
| **P2-11** | **Acquisition parameter ±buttons: increase to full 44px IEC 62366 target** (P0-3 final) | Acquisition |

### Phase 3: Polish (P3)

**Goal:** Accessibility, completeness, and UX quality.

| Task | Description | Screen |
|------|-------------|--------|
| P3-01 | SystemAdmin: 5 tab sections implementation | SystemAdmin |
| P3-02 | Login: Token migration, accessibility | Login |
| P3-03 | Worklist: Row height 36px to 24px per PPT (desktop only, see IEC note) | Worklist |
| P3-04 | AddPatient: DatePicker, touch target expansion | AddPatient |
| P3-05 | Merge: Drag-and-drop, additional merge modes | Merge |
| P3-06 | AutomationProperties.Name across all views | All |
| P3-07 | Custom scrollbar style (5px, #2e4070) | All |
| P3-08 | Keyboard navigation audit (Tab order, shortcuts) | All |

---

## 6. Design Tokens and Style Compliance

### 6.1 Color Palette (Authoritative Source: UISPEC-002 Slide 6 + UISPEC-004)

#### Background Tokens

| Token | Hex | Usage |
|-------|-----|-------|
| `HnVue.Core.Color.BgApp` | `#0d1527` | Full app background (darkest) |
| `HnVue.Semantic.Surface.Panel` | `#1a2540` | General panel surfaces |
| `HnVue.Semantic.Surface.Card` | `#0F3460` / `#152035` | Input fields, cards |
| `HnVue.Component.DetailPanel.Bg` | `#101a2e` | Detail panels, side panels |
| `HnVue.Component.Toolbar.Bg` | `#152035` | Toolbars |
| `HnVue.Component.FilterBar.Bg` | `#1a2a44` | Filter bars |
| `HnVue.Component.DataGrid.HeaderBg` | `#152645` | DataGrid column headers |
| `HnVue.Semantic.Surface.DicomCanvas` | `#050d18` | DICOM image viewing area |

#### Text Tokens

| Token | Hex | Usage |
|-------|-----|-------|
| `HnVue.Semantic.Text.Primary` | `#e0e6f0` | Primary text |
| `HnVue.Semantic.Text.Secondary` | `#b8cce0` / `#7090b0` | Secondary/muted labels |
| `HnVue.Semantic.Text.Muted` | `#7090b0` | Filter labels, info keys |
| `HnVue.Semantic.Text.Placeholder` | `#5580a0` | Input placeholders — **see WCAG note below** |
| `HnVue.Semantic.Text.Nav` | `#a0b4cc` | Navigation button text |

> **[P2-3 FIX] WCAG PLACEHOLDER TOKEN**: `#5580a0` on `#152035` yields a contrast ratio of 2.8:1, which **fails WCAG 2.1 AA** (requires 4.5:1). This fix is moved to **Phase 2** because the Acquisition screen is safety-critical.
>
> **Correct Fix Values** (updated per 2nd evaluator review — #7090b0 on #152035 = 4.2:1 is ALSO a FAIL):
> - On `#152035` background (Acquisition card bg): use **`#8090a8`** (5.0:1 — WCAG AA PASS)
> - On `#0d1527` background (main dark bg): use **`#7090b0`** (4.8:1 — WCAG AA PASS)
> - `#7090b0` MUST NOT be used on `#152035` backgrounds — contrast is only 4.2:1 (FAIL)

#### Brand Tokens

| Token | Hex | Usage |
|-------|-----|-------|
| `HnVue.Primary.Brush` | `#1B4F8A` | Header background, primary brand |
| `HnVue.Semantic.Brand.Primary` | `#1f6fc7` | Primary buttons, active filters |
| `HnVue.Semantic.Brand.Accent` | `#00AEEF` / `#7bc8f5` | Focus borders, PatientID highlight, panel titles |
| `HnVue.Semantic.Accent.Yellow` | `#f9e04b` | Section badges, parameter values |

#### Status Tokens (IEC 62366 Color Coding)

| Token | Hex | Usage |
|-------|-----|-------|
| `HnVue.Semantic.Status.Emergency` | `#e74c3c` / `#D50000` | Emergency stop, error states, urgent indicators |
| `HnVue.Semantic.Status.Warning` | `#f39c12` / `#FFD600` | Warnings, pending states |
| `HnVue.Semantic.Status.Safe` | `#2ecc71` / `#00C853` | Safe/ready/completed states |
| `HnVue.Semantic.Status.Info` | `#3498db` | In-progress states |
| `HnVue.Semantic.Text.Disabled` | `#546e7a` | Disabled/cancelled |

#### Border Tokens

| Token | Hex | Usage |
|-------|-----|-------|
| `HnVue.Semantic.Border.Default` | `#2e4070` / `#1e2d4a` | General borders |
| `HnVue.Semantic.Border.Input` | `#3a5080` | Input field borders |
| `HnVue.Component.DataGrid.GridLine` | `#2a3a58` | DataGrid header lines |
| `HnVue.Component.DataGrid.RowLine` | `#1e2e48` | Row separators |

#### Modality Badge Tokens

> **[P2-5 FIX] ALL 7 MODALITIES REQUIRED**: UISPEC-002 (Worklist) defines CT/MR/XR/US/NM modalities. UISPEC-003 (Studylist) adds CR and DR modalities. The `CoreTokens.xaml` file MUST define all 7 modality color tokens for consistency across both screens. Missing CR/DR tokens would cause rendering failures on Studylist.

| Token | Hex | Modality |
|-------|-----|----------|
| `HnVue.Badge.CT` | `#c0392b` | CT |
| `HnVue.Badge.MR` | `#2980b9` | MR |
| `HnVue.Badge.XR` | `#27ae60` | XR |
| `HnVue.Badge.US` | `#8e44ad` | US |
| `HnVue.Badge.NM` | `#d35400` | NM |
| `HnVue.Badge.CR` | `#1f6fc7` | CR — **required by UISPEC-003** |
| `HnVue.Badge.DR` | `#1565a0` | DR — **required by UISPEC-003** |

### 6.2 Color Palette Decision by Screen

> **[P2-1 FIX] DEFINITIVE PALETTE ASSIGNMENT**: The two legacy palettes are hereby formally assigned per screen. This decision is binding and not subject to per-screen UISPEC overrides.

| Screen | Authoritative Palette | Background | Panel | Card |
|--------|----------------------|------------|-------|------|
| Login | Worklist Navy | `#0d1527` | `#1a2540` | `#152035` |
| Worklist | Worklist Navy | `#0d1527` | `#1a2540` | `#152035` |
| Studylist | Worklist Navy | `#0d1527` | `#1a2540` | `#152035` |
| Acquisition | Worklist Navy | `#0d1527` | `#101a2e` | `#152035` |
| **Merge** | **Worklist Navy** | **`#0d1527`** | **`#1a2540`** | **`#152035`** |
| Settings | Worklist Navy (migrate) | `#0d1527` | `#1a2540` | `#152035` |
| AddPatient | Worklist Navy (migrate) | `#0d1527` | `#1a2540` | `#152035` |
| SystemAdmin | Worklist Navy (migrate) | `#0d1527` | `#1a2540` | `#152035` |

The gray palette (`#242424` / `#2A2A2A` / `#3B3B3B`) is **deprecated**. Legacy screens (AddPatient, Settings, SystemAdmin) use gray palette today and will migrate to Worklist Navy progressively. New screens (Merge) launch with Worklist Navy directly.

---

## 7. IEC 62366 Safety Checklist

### 7.1 Safety-Critical Controls

| Requirement | Specification | Current Status |
|-------------|---------------|----------------|
| Emergency Stop always visible | Fixed position in header, never hidden by navigation | PASS (header persists across all screens) |
| Emergency Stop always enabled | `IsEnabled` never bound, never `Collapsed` | PASS |
| Emergency Stop Escape key | `KeyBinding Key="Escape"` at Window level | PASS — see Escape conflict resolution in Section 8.3 |
| Emergency Stop minimum touch target | **48px** (header height 60px minus 6px top+bottom padding) | **PASS** (after P0-00 fix, was FAIL at 48px header) |
| No confirmation dialog for E-Stop | IEC 62366 section 5.9.2 | PASS (direct command execution) |

### 7.2 Touch Target Compliance

| Screen | Component | Current Size | IEC 62366 Minimum | Phase 1 Fix | Phase 2 Target | Status |
|--------|-----------|-------------|-------------------|-------------|---------------|--------|
| All | Emergency STOP | 48px (after 60px header fix) | 44px | Complete at P0-00 | — | **PASS** |
| Acquisition | Expose button | 44px (spec) | 44px | — | — | SPEC COMPLIANT |
| Acquisition | Parameter +/- buttons | 18px (spec) | 44px | **32px minimum (P1-10)** | 44px (P2-11) | IN PROGRESS |
| Worklist | DataGrid rows | 36px (current) / 24px (PPT) | 44px | Keep 36px | Desktop config option | WARNING (see note) |
| AddPatient | Cancel/Save buttons | 36px | 44px | — | Phase 2 fix | FAIL — Phase 2 fix |
| AddPatient | Chip delete X button | ~20px | 44px | — | Phase 2 fix | FAIL — Phase 2 fix |
| Settings | Tab buttons | 36px | 44px | — | Phase 2 | WARNING |
| Studylist/Worklist | Filter buttons | 28px | 44px | — | Phase 2 | FAIL |

> **Design Conflict Note**: PPT specifies 24px DataGrid row height, but IEC 62366 requires 44px minimum for touch targets. **Resolution**: Use 44px for touch-screen installations, allow 36px for mouse-only desktop use with runtime configuration via `AppSettings.UseDesktopRowHeight`.

### 7.3 Color Contrast (WCAG 2.1 AA, minimum 4.5:1)

| Combination | Ratio | Status | Action |
|-------------|-------|--------|--------|
| White (#fff) on #1B4F8A (Header) | 7.2:1 | PASS | None |
| White (#fff) on #D50000 (Emergency) | 5.9:1 | PASS | None |
| White (#fff) on #1f6fc7 (Primary button) | 4.5:1 | PASS (borderline) | None |
| #e0e6f0 on #0d1527 (Primary text on dark bg) | 13.2:1 | PASS | None |
| #b8cce0 on #101a2e (Secondary text on panel) | 7.2:1 | PASS | None |
| #7bc8f5 on #0d1527 (Accent on dark) | 6.8:1 | PASS | None |
| #f9e04b on #152035 (Yellow params on card) | 9.1:1 | PASS | None |
| #5580a0 on #152035 (Placeholder/param label) | 2.8:1 | **FAIL** | Phase 2: fix to #8090a8 on Acquisition, #7090b0 elsewhere |
| #00C853 on #0d1527 (Safe status) | 5.8:1 | PASS | None |
| #FFD600 on #0d1527 (Warning) | 10.6:1 | PASS | None |

### 7.4 Keyboard Navigation Requirements

| Requirement | Specification | Status |
|-------------|---------------|--------|
| Tab navigation for all interactive elements | Every button, input, DataGrid reachable via Tab | PARTIAL — needs audit |
| Escape key for Emergency Stop | Window-level KeyBinding | IMPLEMENTED — see Section 8.3 for conflict resolution |
| Enter for primary actions | IsDefault=True on primary buttons | IMPLEMENTED (Login) |
| Arrow keys for DataGrid navigation | Built-in WPF DataGrid support | BUILT-IN |
| Space for Expose (Acquisition) | KeyBinding when no input focused | NOT IMPLEMENTED |
| F5 for Retake (Acquisition) | KeyBinding after capture complete | NOT IMPLEMENTED |

### 7.5 Dual Coding (Color-Blind Accessibility)

All status information must use color + text/icon combination:

| State | Color | Secondary Indicator | Status |
|-------|-------|---------------------|--------|
| Emergency/Error | Red #e74c3c | "STOP" / "Error" text | SPECIFIED |
| Warning/Pending | Yellow #f39c12 | "Warning" / "Pending" text | SPECIFIED |
| Safe/Ready | Green #2ecc71 | "Ready" / "Normal" text | SPECIFIED |
| In Progress | Blue #3498db | "In Progress" text + LIVE badge | SPECIFIED |
| Disabled/Cancelled | Gray #546e7a | "Offline" / "Cancelled" text | SPECIFIED |

---

## 8. Cross-Screen Consistency Requirements

### 8.1 Shared Components

These components must be visually identical across all screens:

| Component | Used In | Token/Style |
|-----------|---------|-------------|
| Section Badge | Worklist, Studylist, Acquisition | `HnVue.Component.SectionBadge.Bg` + yellow text |
| Modality Badge | Worklist, Studylist | `HnVue.Badge.{Modality}` colors (all 7) |
| Status Dot Indicator | Worklist, Studylist, Acquisition | 8px Ellipse + status colors |
| Primary Button | All screens | `HnVue.PrimaryButton` style |
| Outline Button | All screens | `HnVue.OutlineButton` style |
| Emergency Stop Button | Header (all screens) | `HnVue.EmergencyStopButton` style |
| Search Input | Worklist, Studylist, Merge | Dark background + magnifying glass icon |

### 8.2 List Style Consistency (PPT Section 4.1)

PPT explicitly requires: **Worklist list = Studylist list = Merge list** must have unified visual style.

This means:
- Same DataGrid row height
- Same column header style (#152645 background, #90b0d0/#b8cce0 text)
- Same row alternation colors (#0d1527 / #111f3a)
- Same hover/selection colors (#1e3558 hover, #1f4878 selected)
- Same urgency left border pattern (3px colored)

### 8.3 Escape Key Routing Rules (NEW — P2-2 Fix)

> **[P2-2 FIX] ESCAPE KEY CONFLICT RESOLUTION**:
>
> **Problem**: The Emergency Stop command is bound to the Escape key at Window level. The Merge screen Cancel action also uses Escape (per UISPEC-006 Section 6.4). This creates a routing conflict: pressing Escape on the Merge screen would trigger Emergency Stop instead of canceling the merge operation.
>
> **Resolution — Escape Key Priority Rules**:
>
> 1. **Window Level (lowest priority)**: Emergency Stop Escape binding is registered at MainWindow level with `e.Handled = false` — it fires only if no child element handles the event first.
>
> 2. **Screen Level (higher priority)**: Any screen that needs to handle Escape for a local action (Merge Cancel, dialog dismiss, etc.) registers a `KeyBinding` with `e.Handled = true` in its own UserControl or dialog. This prevents bubbling to the Window-level Emergency Stop binding.
>
> 3. **Implementation rule**: The Merge screen MUST register `<KeyBinding Key="Escape" Command="{Binding CancelCommand}"/>` in `MergeView.xaml` with the command handler setting `e.Handled = true`. This ensures Escape cancels the merge workflow without triggering Emergency Stop.
>
> 4. **Safety invariant**: If NO screen-level handler claims the Escape key (i.e., no modal dialog or screen Cancel is active), the Window-level Emergency Stop binding fires. This is the correct fallback behavior.

**Escape Key Priority Table:**

| Context | Escape Behavior | Handler Level |
|---------|----------------|---------------|
| Normal screen (no active input) | Emergency Stop fires | Window level |
| Merge screen (Cancel) | Cancel merge, e.Handled = true | MergeView.xaml KeyBinding |
| Any modal dialog | Dismiss dialog, e.Handled = true | Dialog UserControl KeyBinding |
| Text input focused | Default (deselect/close suggestion) | Built-in WPF behavior, Handled = true |
| Acquisition screen (state-dependent — see matrix below) | See Acquisition Escape Matrix | AcquisitionView.xaml KeyBinding (state check) |

**Acquisition Screen Escape Key State Matrix** (IEC 62366 — state-based behavior is required):

| Acquisition State | Escape Behavior | Rationale |
|-------------------|----------------|-----------|
| `Idle` (no patient loaded) | Emergency Stop fires | No active workflow to cancel; allow safety stop |
| `PatientLoaded` (ready, not exposing) | Navigate back to Worklist (e.Handled=true) | Escape = cancel/exit acquisition without triggering stop |
| `Exposing` (radiation active) | **Cancel exposure only** (e.Handled=true); Emergency Stop NOT fired | Cancelling exposure is the primary user intent; Emergency Stop is reserved for hardware emergency |
| `ExposureComplete` (image received) | Navigate back to Worklist (e.Handled=true) | Study done; Escape = exit screen |
| `Error` state | Emergency Stop fires | Error state requires hardware-level intervention |

> **Implementation Note**: AcquisitionView.xaml must register a Window.KeyDown handler with priority above the Window-level Emergency Stop binding, checking `WorkflowViewModel.CurrentState` and setting `e.Handled = true` for `PatientLoaded`, `Exposing`, and `ExposureComplete` states.

---

## 9. Document References

| Document | Path | Purpose |
|----------|------|---------|
| PPT Analysis | `docs/design/PPT_ANALYSIS.md` | Original PPT slide breakdown |
| UI Master Reference | `docs/design/UI_DESIGN_MASTER_REFERENCE.md` | Current UI patterns and styles |
| UISPEC-001 Login | `docs/design/spec/UISPEC-001_Login.md` | Login screen specification |
| UISPEC-002 Worklist | `docs/design/spec/UISPEC-002_Worklist.md` | Worklist screen specification |
| UISPEC-003 Studylist | `docs/design/spec/UISPEC-003_Studylist.md` | Studylist screen specification |
| UISPEC-004 Acquisition | `docs/design/spec/UISPEC-004_Acquisition.md` | Acquisition screen specification |
| UISPEC-005 AddPatient | `docs/design/spec/UISPEC-005_AddPatient.md` | Add Patient specification |
| UISPEC-006 Merge | `docs/design/spec/UISPEC-006_Merge.md` | Merge screen specification (contains "PROTYPER" typo — ignore, use "HnVUE") |
| UISPEC-007 Settings | `docs/design/spec/UISPEC-007_Settings.md` | Settings screen specification |
| UISPEC-008 ImageViewer | `docs/design/spec/UISPEC-008_ImageViewer.md` | Image Viewer specification |
| UISPEC-009 SystemAdmin | `docs/design/spec/UISPEC-009_SystemAdmin.md` | System Admin specification |
| INavigationService | `src/HnVue.UI.Contracts/Navigation/INavigationService.cs` | Navigation interface |
| NavigationToken | `src/HnVue.UI.Contracts/Navigation/NavigationToken.cs` | Navigation enum (Coordinator-owned) |
| MainWindow.xaml | `src/HnVue.App/MainWindow.xaml` | Current shell implementation |

---

## 10. Compliance Summary Matrix

| Screen | UISPEC | Compliance % | Priority | Phase | Safety-Critical |
|--------|--------|-------------|----------|-------|----------------|
| Navigation Architecture | — | 0% | P0 | 0 | Yes (enables all) |
| Login | UISPEC-001 | 95% | P3 | 3 | No |
| Worklist | UISPEC-002 | 44% | P1 | 1-2 | No |
| Studylist | UISPEC-003 | 63% | P2 | 2 | No |
| Acquisition | UISPEC-004 | 0% (XAML) | P0 | 1 | **Yes** (IEC 62366) |
| AddPatient | UISPEC-005 | 75% | P2 | 2 | No |
| Merge | UISPEC-006 | 0% (XAML) | P1 | 1 | **Yes** (data safety) |
| Settings | UISPEC-007 | 80% | P2 | 2 | No |
| ImageViewer | UISPEC-008 | 70% | P2 | 2 | No |
| SystemAdmin | UISPEC-009 | 40% | P3 | 3 | No |

---

## 11. Cross-Team Dependencies (NEW — P0-1 Fix + P2-4 Fix)

This section is the authoritative reference for all inter-team dependencies that affect Team Design's ability to implement UI work. Team Design CANNOT unilaterally implement items owned by other teams.

### 11.1 Inter-Team Dependency Table

| What Team Design Needs | Providing Team | File / Interface | Needed For | Blocker For |
|------------------------|---------------|-----------------|------------|-------------|
| `NavigationToken` enum update (6 tokens) | **Coordinator** | `HnVue.UI.Contracts/Navigation/NavigationToken.cs` | Phase 0 navigation model | Phase 0 start |
| `NavigationService.cs` implementation | **Coordinator** | `HnVue.App/Services/NavigationService.cs` | Shell navigation wiring | Phase 0 P0-01 |
| DI registration of NavigationService + Views | **Coordinator** | `HnVue.App/App.xaml.cs` | App startup | Phase 0 P0-05 |
| `interface-contract` issue filed | **Coordinator** | Gitea issue tracker | Process compliance | Phase 0 P0-02 |
| `IGeneratorService` interface | **Team B** | `HnVue.UI.Contracts` or `HnVue.Detector` | Acquisition ViewModel | Phase 1 P1-01 |
| `IDetectorService` interface | **Team B** | `HnVue.UI.Contracts` or `HnVue.Detector` | Acquisition ViewModel | Phase 1 P1-01 |
| `IDicomImageService` interface | **Team B** | `HnVue.UI.Contracts` or `HnVue.Imaging` | Acquisition image viewer | Phase 1 P1-02 |

### 11.2 Coordinator Team Approval Process

**Process for NavigationToken changes (mandated by Coordinator team rules):**

1. Team Design prepares a draft `NavigationToken.cs` with the proposed additions/renames
2. Team Design creates a Gitea issue with label `interface-contract` describing all 6 token changes
3. Coordinator reviews the issue — verifies impact on all consumers (ViewModels, NavigationService, DI registration)
4. Coordinator approves and commits the `NavigationToken.cs` change
5. Coordinator creates the `NavigationService.cs` implementation file or approves Team Design's draft
6. Coordinator registers the NavigationService in `App.xaml.cs`
7. Team Design receives notification of completion and can proceed with Phase 0

**Estimated unblock sequence (priority order, not time-based):**
1. Team Design files `interface-contract` issue with full token change list
2. Coordinator reviews and approves token changes
3. Coordinator completes NavigationService.cs and DI registration
4. Phase 0 implementation begins (unblocked)

### 11.3 Team B Interface Requirements

Team B must provide the following service interfaces for the Acquisition screen ViewModel to function. Team Design CANNOT implement the Acquisition screen's business logic without these interfaces being defined and available in a referenced assembly.

| Interface | Package/Assembly | Methods Team Design Needs |
|-----------|-----------------|--------------------------|
| `IGeneratorService` | TBD by Team B | `GetStatus()`, `SendExposureParameters(kVp, mAs)`, `TriggerEmergencyStop()` |
| `IDetectorService` | TBD by Team B | `GetStatus()`, `GetLatestImage()`, `StartAcquisition()`, `StopAcquisition()` |
| `IDicomImageService` | TBD by Team B | `ConvertToDicom(image)`, `GetThumbnails()`, `SendToPacs()` |

Team Design will create stub/mock implementations in `HnVue.UI/DesignTime/` to enable XAML designer preview and unit testing while Team B's actual implementations are in progress.

---

## 12. Known UISPEC Issues to Address in Future Revisions

The following issues were identified in source UISPEC documents. These are documentation errors that need to be corrected in separate document update tasks. They do NOT block implementation (this design plan takes precedence).

| Issue | UISPEC | Location | Correct Value |
|-------|--------|----------|---------------|
| "PROTYPER" logo typo | UISPEC-006 | Section 2.2, line ~79 | "HnVUE" |
| Gray palette used for Merge | UISPEC-006 | Section 4 Color Tokens | Worklist Navy palette |
| Acquisition parameter ±buttons listed as 18px | UISPEC-004 | Section 3.3 Adjustment Buttons | Phase 1: 32px, Phase 2: 44px |
| WCAG failure for #5580a0 listed as Phase 3 fix | UISPEC-004 | Section 9 Phase 3 item 10 | Moved to Phase 2 (safety screen) |
| CR/DR modality tokens missing from UISPEC-002 | UISPEC-002 | Color tokens section | Add CR (#1f6fc7) and DR (#1565a0) |
| Emergency Stop height listed as 30px (app header height) | UISPEC-004 | Section 3.2 Line ~308 | Design Plan v2 decision: header = 60px, Emergency Stop = 48px (after P0-00) |
| Acquisition toolbar shows 촬영시작/취소/재촬영 as interactive buttons | UISPEC-004 | Section 2.1 + Section 3.1 | Design Plan v2 decision: action buttons are in right control panel ONLY; toolbar entries are section name badges |

---

*Document Version: 2.1 | Status: Approved Draft*
*Supersedes: DESIGN_PLAN_v1.0*
*Based on: 9 UISPEC documents, PPT_ANALYSIS.md, UI_DESIGN_MASTER_REFERENCE.md, MainWindow.xaml analysis, Evaluator defect report*
*Next Review: After Coordinator approval of NavigationToken changes (Phase 0 unblock)*
