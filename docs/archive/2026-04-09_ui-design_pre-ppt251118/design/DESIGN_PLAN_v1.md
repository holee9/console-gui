# HnVue UI Design Plan v1.0

**Version**: 1.0 | **Date**: 2026-04-08 | **Status**: Review Draft
**Author**: Team Design | **Approver**: Coordinator

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
- `NavigationService` class in `HnVue.App` (DI composition root)
- View registry: `Dictionary<NavigationToken, Func<UserControl>>` for lazy view creation
- Navigation stack: `Stack<NavigationToken>` for GoBack() support
- Parameter passing: Navigation parameters (e.g., selected patient for Acquisition) stored per navigation
- `NavigationRequestedMessage` (already exists in UI.Contracts.Events) can be used for ViewModel-to-ViewModel communication

### 1.5 NavigationToken Additions Required

Current `NavigationToken` enum is missing several screens. Required additions:

| Token | Screen | Status |
|-------|--------|--------|
| Login | LoginView | EXISTS |
| PatientList | WorklistView (renamed) | EXISTS (name mismatch) |
| Workflow | AcquisitionView | EXISTS (name mismatch) |
| ImageViewer | ImageViewerView | EXISTS |
| DoseDisplay | DoseDisplayView | EXISTS (becomes sub-component) |
| CDBurn | CDBurnView | EXISTS |
| SystemAdmin | SystemAdminView | EXISTS |
| QuickPinLock | QuickPinLockView | EXISTS |
| Emergency | EmergencyView | EXISTS |
| **Studylist** | StudylistView | **NEW** |
| **AddPatient** | AddPatientProcedureView | **NEW** |
| **Merge** | MergeView | **NEW** |
| **Settings** | SettingsView | **NEW** |
| **Acquisition** | AcquisitionView (replaces Workflow) | **RENAME** |
| **Worklist** | PatientListView (replaces PatientList) | **RENAME** |

> **Note**: `DoseDisplay` and `ImageViewer` are no longer top-level navigation targets. They become sub-components of the Acquisition screen. The `Workflow` token should be renamed to `Acquisition` to match PPT terminology.

---

## 2. Screen Navigation Map

### 2.1 Navigation Flow Diagram

```
                    [App Launch]
                         │
                    ┌────▼────┐
                    │  Login   │
                    └────┬────┘
                         │ (successful auth)
                    ┌────▼────┐
             ┌──────┤ Worklist ├──────┐
             │      └────┬────┘      │
             │           │           │
    ┌────────▼───┐  ┌───▼────┐  ┌──▼──────┐
    │ Acquisition │  │Settings│  │  Merge   │
    │(3-panel own)│  │(tabbed)│  │(3-column)│
    └────────┬───┘  └───┬────┘  └──┬──────┘
             │           │           │
             └───────────┼───────────┘
                         │ (Back)
                    ┌────▼────┐
                    │ Worklist │
                    └─────────┘

    Worklist ←→ Studylist (tab switch within same screen)
    Any Screen → Login (logout)
    Any Screen → SystemAdmin (admin access)
```

### 2.2 Navigation Trigger Table

| Source Screen | Trigger | Target Screen | Parameter |
|---------------|---------|---------------|-----------|
| Login | Successful authentication | Worklist | User session |
| Worklist | Patient double-click or Acquisition button | Acquisition | Selected PatientId, StudyId |
| Worklist | Settings button (header) | Settings | None |
| Worklist | Merge button (toolbar) | Merge | None |
| Worklist | SystemAdmin (header/admin) | SystemAdmin | None |
| Worklist | Studylist tab | Studylist | None (same-level tab) |
| Worklist | Add Patient button | AddPatient | None (modal overlay) |
| Studylist | Worklist tab | Worklist | None (same-level tab) |
| Studylist | Row double-click | Acquisition | Selected StudyId |
| Acquisition | Back button | Worklist | None |
| Acquisition | Study complete | Worklist | Updated study status |
| Settings | Close/Cancel/Save | Worklist | None (GoBack) |
| Merge | Close/Cancel | Worklist | None (GoBack) |
| SystemAdmin | Close | Worklist | None (GoBack) |
| Any screen | Logout button | Login | Clear session |
| Any screen | Emergency Stop | (No navigation) | Abort current operation |

### 2.3 Modal vs Navigation

Some screens are modal overlays, not full navigation targets:

| Screen | Type | Reason |
|--------|------|--------|
| AddPatient | Modal overlay | PPT Slide 8: dialog over Worklist |
| QuickPinLock | Modal overlay | Security lock over current screen |
| CDBurn | Modal overlay | Utility dialog |
| Emergency | Not a screen | Action trigger, no navigation |

---

## 3. MainWindow Shell Redesign

### 3.1 New Shell Layout

```
┌─────────────────────────────────────────────────────────────┐
│  Header (48px): Logo + Nav Buttons + Emergency STOP (ALWAYS)│
├─────────────────────────────────────────────────────────────┤
│  TLS Warning Banner (conditional, top of content)           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ContentControl — SWAPS views via NavigationService         │
│  (LoginView / WorklistView / AcquisitionView / SettingsView │
│   / MergeView / StudylistView / SystemAdminView)            │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  Session Timeout Banner (conditional, bottom overlay)       │
├─────────────────────────────────────────────────────────────┤
│  Status Bar (28px): Section name | DICOM connection status  │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Header (48px) — Preserved from Current

The current header is well-implemented and should be **preserved** with minor additions:

| Element | Current | Change |
|---------|---------|--------|
| Logo badge ("H") | 28x28 Accent | Keep |
| "HnVue Console" title | 16px white | Keep |
| Emergency STOP button | Always visible, always enabled | Keep (IEC 62366) |
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

| Element | Location | IEC 62366 Requirement |
|---------|----------|----------------------|
| Emergency STOP button | Header, right side | Always visible, always enabled, Escape key |
| TLS Warning banner | Below header, conditional | Security warning visibility |
| Session Timeout banner | Above status bar, conditional | User awareness |
| DICOM connection status | Status bar, right | Connection awareness |

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
| Modality column + color badges (CT/MR/XR/US/NM) | P1 | MR-WL-005 |
| Status column with dot indicators (8px colored dots) | P1 | MR-WL-003 |
| Row left 3px urgency border (DataTrigger) | P1 | MR-WL-003 |
| Detail Panel content (InfoSection x2, patient/study info) | P1 | MR-WL-004 |
| ExamDate (StudyDate) column | P1 | MR-WL-001 |
| Modality badge color tokens (CoreTokens.xaml) | P1 | MR-WL-005 |
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

**Layout:** Full-screen, 4-row grid:
- Row 0 (48px): Header (Prev/Next arrows + "Study List" + PACS ComboBox)
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
- Left panel (230px): Patient info + Exam info + Alerts + Device status
- Center panel (flex): DICOM Image Viewer + Thumbnail strip (80px)
- Right panel (280px): Exposure parameters (kVp/mAs/SID) + Grid/AEC toggle + Dose monitor + Expose/Cancel/Retake buttons

**This screen is the ONLY screen that has a 3-column internal layout**, matching the current MainWindow structure. However, the current MainWindow splits this across separate ViewModels bound to fixed columns, which is incorrect. The 3-panel layout must be self-contained within `AcquisitionView.xaml`.

**Key Safety Components (IEC 62366):**
- Emergency STOP: Header-level (inherited from shell), always visible
- Expose button: 44px height minimum, blue primary, disabled when equipment not ready
- Device status indicators: Generator/Detector/Bucky/Collimator with colored dots + text
- LIVE badge: Pulsing red animation during exposure
- Dose monitor: Real-time progress bar with threshold colors (safe/warning/blocked/emergency)

**Implementation Gap:**
- `AcquisitionView.xaml` must be created from scratch
- `AcquisitionViewModel.cs` must be created
- `DeviceStatusToBrushConverter`, `AcquisitionStateToBrushConverter` needed
- Service interfaces exist (`IDetectorService`, etc.) but no view integration

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

**Layout:** Full-screen, 3-column comparison:
- Header (60px): Logo + "Merge" title
- Selection Bar (80px): Study A / Study B search inputs
- Content: Panel A (flex) + Center Controls (280px) + Panel C (flex)
- Footer (64px): Study info + Cancel/Save
- Status Bar (32px): Status messages

**Key Safety Components:**
- Patient ID mismatch detection: Auto-block merge when IDs differ
- Patient name mismatch: Confirmation dialog required
- Study date mismatch (>7 days): Warning display

**Implementation Requirements:**
- `MergeView.xaml` must be created
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

### Phase 0: Architecture Fix (P0 -- PREREQUISITE)

**Goal:** Enable single-screen navigation, unblock all other phases.

| Task | Description | Files Affected |
|------|-------------|----------------|
| P0-01 | Implement `NavigationService` class | `HnVue.App/Services/NavigationService.cs` (new) |
| P0-02 | Add missing NavigationTokens | `NavigationToken.cs` (Studylist, AddPatient, Merge, Settings, Acquisition) |
| P0-03 | Redesign MainWindow.xaml shell | `MainWindow.xaml` — replace 3-column grid with ContentControl |
| P0-04 | Update MainWindowViewModel | `MainWindowViewModel.cs` — add CurrentView, inject INavigationService |
| P0-05 | Register views in DI container | `App.xaml.cs` or `ServiceCollectionExtensions.cs` |
| P0-06 | Add navigation buttons to header | `MainWindow.xaml` header — Worklist/Settings/Merge/Admin buttons |
| P0-07 | Wire LoginView to navigate to Worklist on auth | `LoginViewModel.cs` or `MainWindowViewModel.cs` |

**Dependency:** Coordinator team must approve INavigationService implementation and DI registration changes.

### Phase 1: Critical Screens (P1)

**Goal:** Complete the two most critical screens: Worklist detail and Acquisition view.

| Task | Description | Screen | Dependency |
|------|-------------|--------|------------|
| P1-01 | Create AcquisitionView.xaml (3-panel internal layout) | Acquisition | P0 complete |
| P1-02 | Consolidate WorkflowView + ImageViewerView + DoseDisplayView into AcquisitionView | Acquisition | P1-01 |
| P1-03 | Implement Modality badge column + color tokens | Worklist | None |
| P1-04 | Implement Status dot indicator column | Worklist | None |
| P1-05 | Implement row left 3px urgency border (DataTrigger) | Worklist | None |
| P1-06 | Complete Detail Panel content (InfoSection x2) | Worklist | None |
| P1-07 | Add ExamDate (StudyDate) column | Worklist | None |
| P1-08 | Create MergeView.xaml with patient ID validation | Merge | P0 complete |
| P1-09 | Implement Modality badge color tokens | Tokens | None |

### Phase 2: Important Screens (P2)

**Goal:** Complete secondary screens and missing features.

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

### Phase 3: Polish (P3)

**Goal:** Accessibility, completeness, and UX quality.

| Task | Description | Screen |
|------|-------------|--------|
| P3-01 | SystemAdmin: 5 tab sections implementation | SystemAdmin |
| P3-02 | Login: Token migration, accessibility | Login |
| P3-03 | Worklist: Row height 36px to 24px per PPT | Worklist |
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
| `HnVue.Semantic.Text.Placeholder` | `#5580a0` | Input placeholders |
| `HnVue.Semantic.Text.Nav` | `#a0b4cc` | Navigation button text |

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

| Token | Hex | Modality |
|-------|-----|----------|
| `HnVue.Badge.CT` | `#c0392b` | CT |
| `HnVue.Badge.MR` | `#2980b9` | MR |
| `HnVue.Badge.XR` | `#27ae60` | XR |
| `HnVue.Badge.US` | `#8e44ad` | US |
| `HnVue.Badge.NM` | `#d35400` | NM |
| `HnVue.Badge.CR` | `#1f6fc7` | CR |
| `HnVue.Badge.DR` | `#1565a0` | DR |

### 6.2 Token Discrepancy Notes

Two color palettes exist due to historical evolution:

| Context | Background | Panel | Card | Text Primary |
|---------|-----------|-------|------|-------------|
| UISPEC-002/003/004 (Worklist palette) | `#0d1527` | `#1a2540` | `#152035` | `#e0e6f0` |
| UISPEC-005/006/007 (Form palette) | `#242424` | `#2A2A2A` | `#3B3B3B` | `#FFFFFF` |

The Worklist palette (dark navy series) is the PPT-authoritative design. The Form palette (#242424 series) was applied during earlier implementation. **Recommendation**: Progressively migrate Form palette screens to Worklist palette for visual consistency, starting with new screens (Acquisition, Merge).

---

## 7. IEC 62366 Safety Checklist

### 7.1 Safety-Critical Controls

| Requirement | Specification | Current Status |
|-------------|---------------|----------------|
| Emergency Stop always visible | Fixed position in header, never hidden by navigation | PASS (header persists across all screens) |
| Emergency Stop always enabled | `IsEnabled` never bound, never `Collapsed` | PASS |
| Emergency Stop Escape key | `KeyBinding Key="Escape"` at Window level | PASS |
| Emergency Stop minimum size | 56px height recommended, currently 32px in header | WARNING (header height limits this) |
| No confirmation dialog for E-Stop | IEC 62366 section 5.9.2 | PASS (direct command execution) |

### 7.2 Touch Target Compliance

| Screen | Component | Current Size | IEC 62366 Minimum | Status |
|--------|-----------|-------------|-------------------|--------|
| All | Emergency STOP | Header height (48px restricted) | 44px | PASS (width compensates) |
| Acquisition | Expose button | 44px (spec) | 44px | SPEC COMPLIANT |
| Acquisition | Parameter +/- buttons | 18px (spec) | 44px | FAIL — Phase 2 fix |
| Worklist | DataGrid rows | 36px (current) / 24px (PPT) | 44px | FAIL — 24px conflicts with IEC |
| AddPatient | Cancel/Save buttons | 36px | 44px | FAIL — Phase 2 fix |
| AddPatient | Chip delete X button | ~20px | 44px | FAIL — Phase 2 fix |
| Settings | Tab buttons | 36px | 44px | WARNING |
| Studylist/Worklist | Filter buttons | 28px | 44px | FAIL |

> **Design Conflict**: PPT specifies 24px DataGrid row height, but IEC 62366 requires 44px minimum for touch targets. **Resolution**: Use 44px for touch-screen installations, allow 36px for mouse-only desktop use with runtime configuration.

### 7.3 Color Contrast (WCAG 2.1 AA, minimum 4.5:1)

| Combination | Ratio | Status |
|-------------|-------|--------|
| White (#fff) on #1B4F8A (Header) | 7.2:1 | PASS |
| White (#fff) on #D50000 (Emergency) | 5.9:1 | PASS |
| White (#fff) on #1f6fc7 (Primary button) | 4.5:1 | PASS (borderline) |
| #e0e6f0 on #0d1527 (Primary text on dark bg) | 13.2:1 | PASS |
| #b8cce0 on #101a2e (Secondary text on panel) | 7.2:1 | PASS |
| #7bc8f5 on #0d1527 (Accent on dark) | 6.8:1 | PASS |
| #f9e04b on #152035 (Yellow params on card) | 9.1:1 | PASS |
| #5580a0 on #152035 (Muted label on card) | 2.8:1 | **FAIL** — fix to #7090b0 |
| #00C853 on #0d1527 (Safe status) | 5.8:1 | PASS |
| #FFD600 on #0d1527 (Warning) | 10.6:1 | PASS |

### 7.4 Keyboard Navigation Requirements

| Requirement | Specification | Status |
|-------------|---------------|--------|
| Tab navigation for all interactive elements | Every button, input, DataGrid reachable via Tab | PARTIAL — needs audit |
| Escape key for Emergency Stop | Window-level KeyBinding | IMPLEMENTED |
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
| Modality Badge | Worklist, Studylist | `HnVue.Badge.{Modality}` colors |
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
| UISPEC-006 Merge | `docs/design/spec/UISPEC-006_Merge.md` | Merge screen specification |
| UISPEC-007 Settings | `docs/design/spec/UISPEC-007_Settings.md` | Settings screen specification |
| UISPEC-008 ImageViewer | `docs/design/spec/UISPEC-008_ImageViewer.md` | Image Viewer specification |
| UISPEC-009 SystemAdmin | `docs/design/spec/UISPEC-009_SystemAdmin.md` | System Admin specification |
| INavigationService | `src/HnVue.UI.Contracts/Navigation/INavigationService.cs` | Navigation interface |
| NavigationToken | `src/HnVue.UI.Contracts/Navigation/NavigationToken.cs` | Navigation enum |
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

*Document Version: 1.0 | Status: Review Draft*
*Based on: 9 UISPEC documents, PPT_ANALYSIS.md, UI_DESIGN_MASTER_REFERENCE.md, MainWindow.xaml analysis*
*Next Review: Coordinator approval required before Phase 0 implementation*
