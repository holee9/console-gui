# Screen Specification: Studylist

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-STUDYLIST-001 |
| **Priority** | Critical |
| **User Type** | Radiographers, Radiologists |
| **Purpose** | Historical examination records and management |

---

## Layout Specification

### Header Layout

```
+----------------------------------------------------------+
| [<] [>]  Study List                    PACS: [LOCAL  ▼] |
+----------------------------------------------------------+
```

### Filter Buttons & Controls

```
+----------------------------------------------------------+
| [Today][3Days][1Week][All][1Month]     [Search________] |
+----------------------------------------------------------+
```

### Main Content Layout (Two-Column)

```
+------------+------------------------------------------+
| Sidebar    | Main Content Area                         |
|            |                                          |
| Filters    | DataGrid / Grid View                      |
|            |                                          |
| All (156)  | Patient ID | AccNo | Exam Date | Body  |
| Completed  |                                          |
| In Prog..  | Study Table / Grid View                  |
| Cancelled  |                                          |
| Reported   | Or Detail Panel (slide-out)              |
+------------+------------------------------------------+
```

### Panel Widths
- **Sidebar:** 250px
- **Main Content:** Flexible (1fr)

---

## Left Sidebar: Status Filters

### Filter Options
| Filter | Count | Color |
|--------|-------|-------|
| All Studies | Dynamic | #B0BEC5 (default text) |
| Completed | Dynamic | #00C853 (success) |
| In Progress | Dynamic | #00AEEF (info/accent) |
| Cancelled | Dynamic | #B0BEC5 (muted) |
| Reported | Dynamic | #7B68EE (purple) |

### Interaction
- Click filter: Update main table
- Active filter: #1B4F8A (`--primary-main`) background with white text
- Count badges: Rounded pills, color matches filter category

---

## Header Bar

### Navigation Controls
- **Previous Button:** [<] Navigate to previous set of studies or time period
- **Next Button:** [>] Navigate to next set of studies or time period
- **Screen Title:** "Study List"
- **PACS Server Selector:** ComboBox with available PACS servers (Default: "LOCAL")
  - Options: LOCAL, PACS1, PACS2, etc.
  - Selection changes queried study source

---

## Filter Bar

### Period Filter Buttons
- **Buttons:** Today, 3Days, 1Week, All, 1Month
- **Style:** Toggle buttons (only one active at a time)
- **Default:** Today
- **Action:** Updates table with studies matching selected period

### Search Input
- **Width:** 300px
- **Placeholder:** "Search by Patient ID, Accession #..."
- **Search Fields:** Patient ID, Name, Accession No, Study Date
- **Real-time:** Filters visible rows as user types

---

## Main Content Area

### Header Controls

#### Action Buttons
- **Export DICOM:** Export selected studies to DICOM
- **Burn to CD:** Create patient CD with viewer (bulk action — see CD Burning section)
- **Print:** Print study report

---

## Data Display Modes

### Table View (Default)

| Column | Width | Sortable | Notes |
|--------|-------|----------|-------|
| Select | 40px | No | Checkbox; shown in Export Mode only |
| Patient ID | 120px | Yes | Format: P20250001 |
| Accession No | 120px | Yes | Format: ACC20250001 |
| Exam Date | 120px | Yes | Date with time (YYYY-MM-DD HH:MM) |
| Body Part | 80px | Yes | Chest, Abdomen, Knee, etc. |
| Description | 150px | No | Study description or procedure name |
| Status | 100px | Yes | Color-coded badge |

**Status Badge Colors (CoreTokens):**
| Status | Background | Text |
|--------|-----------|------|
| Completed | #00C853 at 20% opacity | #00C853 |
| In Progress | #00AEEF at 20% opacity | #00AEEF |
| Cancelled | #2E4A6E | #B0BEC5 |
| Reported | #7B68EE at 20% opacity | #7B68EE |

### Grid View
- Card-based layout
- Each card shows:
  - Thumbnail (first image)
  - Patient name
  - Study date
  - Body part
  - Image count
- Grid: 4 columns (adjustable)

---

## Export Mode (Multi-Select for CD Burning)

### Activation
- Click "Burn to CD" button in header OR
- Click "Export" toggle button to enter Export Mode

### Export Mode Visual Changes
- Checkbox column appears as first column in table
- Header shows: "Export Mode — Select studies to burn"
- "Select All" checkbox in column header
- Count badge: "3 selected"
- Bulk action bar appears at bottom:
  ```
  +--------------------------------------------+
  |  3 studies selected                         |
  |  [Burn to CD] [Export DICOM] [Clear]        |
  +--------------------------------------------+
  ```

### Multi-Select Behavior
- Individual row checkbox: toggle single study
- Header checkbox: select/deselect all visible rows
- Count badge updates in real-time
- Keyboard: Space to toggle selected row, Ctrl+A to select all

### Burn to CD Action
- Requires 1+ studies selected
- Opens CD Burning Wizard (4-step, see below)

---

## CD Burning Wizard (P1-005)

### Step 1: Review Selection
```
+------------------------------------------+
| Burn to CD — Step 1 of 4                |
| Review Studies                           |
|                                          |
| 3 studies selected:                      |
| [x] Kim, Min-jun  Chest PA  2026-04-06  |
| [x] Kim, Min-jun  Knee AP   2026-03-15  |
| [x] Lee, Su-jin   Hand PA   2026-04-01  |
|                                          |
| Total images: 12  Estimated size: 45 MB  |
|                                          |
| [Cancel]              [Next: PHI Check]  |
+------------------------------------------+
```

### Step 2: PHI Confirmation
```
+------------------------------------------+
| Burn to CD — Step 2 of 4                |
| Confirm Patient Health Information       |
|                                          |
| The following PHI will be burned to CD:  |
|                                          |
| Patient Name: Kim, Min-jun              |
| Patient ID:   P20260315001              |
| DOB:          1985-03-15               |
| Study Dates:  2026-04-06, 2026-03-15    |
|                                          |
| [ ] I confirm this CD is for the        |
|     correct patient / authorized use    |
|                                          |
| [Back]                [Next: Burn Options]|
+------------------------------------------+
```

### Step 3: Burn Options
```
+------------------------------------------+
| Burn to CD — Step 3 of 4               |
| Burn Options                             |
|                                          |
| Include DICOM Viewer: [x] Yes            |
| Disc Label: Kim_MinJun_20260406          |
| [Edit label...]                          |
|                                          |
| Drive: [D: HL-DT-ST DVD-RAM  v]         |
| Disc: Insert disc... [Detected: CD-R]    |
| Available Space: 700 MB (need 45 MB)     |
|                                          |
| [Back]                    [Start Burning]|
+------------------------------------------+
```

### Step 4: Burning Progress
```
+------------------------------------------+
| Burn to CD — Step 4 of 4               |
| Burning in Progress                      |
|                                          |
| Writing images...                        |
| [============================   ] 87%    |
|                                          |
| Do not remove disc during burning        |
|                                          |
| -- On completion --                      |
| [x] Burn complete. Verifying...          |
| [x] Verification passed.                 |
| Disc is ready.                           |
|                                          |
|                              [Done]      |
+------------------------------------------+
```

---

## Right-Click Context Menu

### Actions Available
```
+----------------------------------+
| Open in Viewer                   |
| Export DICOM...                  |
| Burn to CD...                    |
| Print Report                     |
| -------------------------------- |
| Stitch Selected Images           |
| -------------------------------- |
| Mark as Rejected...              |
| -------------------------------- |
| Study Properties                 |
+----------------------------------+
```

### Image Stitching (Context Menu)
- **Trigger:** Right-click on study with multiple images -> "Stitch Selected Images"
- **Prerequisite:** Study must have 2+ images
- **Action:** Opens Merge/Stitch screen (SCR-MERGE-001) with study pre-loaded
- **ARIA:** Menu item `role="menuitem"` aria-label="Stitch selected images for this study"

### Rejection Marking (Context Menu)
- **Trigger:** Right-click -> "Mark as Rejected..."
- **Dialog:**
  ```
  +------------------------------------------+
  |  Mark Study as Rejected                  |
  |                                          |
  |  Reason (required):                      |
  |  [Select reason               v]         |
  |  - Patient motion             |
  |  - Positioning error          |
  |  - Exposure error             |
  |  - Equipment artifact         |
  |  - Patient repeat requested   |
  |  - Other (specify below)      |
  |                                          |
  |  Notes (optional):                       |
  |  [                            ]          |
  |                                          |
  |  [Cancel]          [Mark as Rejected]    |
  +------------------------------------------+
  ```
- **After Marking:**
  - Status badge changes to "Rejected" (#D50000 at 20% opacity, text #D50000)
  - Rejection reason saved to study metadata
  - Audit log entry created
- **ARIA:** Dialog `role="dialog"` aria-labelledby="rejection-title"

---

## DICOM Export — Progress Indicator

### Export Flow
- **Trigger:** Header "Export DICOM" button, or right-click -> "Export DICOM..."
- **Progress Display:**
  ```
  +------------------------------------------+
  | Exporting DICOM                          |
  |                                          |
  | Study: Kim, Min-jun — Chest PA          |
  | Destination: E:\Export\                  |
  |                                          |
  | [======================        ] 65%     |
  | Sending image 7 of 12...                 |
  |                                          |
  | [Cancel]                                 |
  +------------------------------------------+
  ```
- **On Completion:** Toast "Export complete. 12 images exported."
- **On Error:** Error banner with details + [Retry] button
- **Background export:** Modal can be minimized; export continues; notification on completion

---

## Detail Panel (Slide-out)

### Trigger
- Click row in table
- Click card in grid

### Content

```
+------------------------------------------+
| Patient Information                      |
| Patient ID: P20260315001                 |
| Name: Kim, Min-jun                       |
| DOB: 1985-03-15 (41y, Male)              |
+------------------------------------------+
| Study Details                            |
| Study Date: 2026-04-06 14:30             |
| Modality: CR                             |
| Body Part: Chest                         |
| View: PA                                 |
| Images: 2                                |
+------------------------------------------+
| Procedure: Chest PA                      |
| Referring: Dr. Park - Radiology          |
| Status: Completed                        |
+------------------------------------------+
| Thumbnails                               |
| [img1] [img2]                            |
+------------------------------------------+
| [Open in Viewer] [Export] [Print]        |
+------------------------------------------+
```

---

## Export Functionality

### DICOM Export
- **Format:** Standard DICOM DIR
- **Include:** Images, metadata, optionally report
- **Destination:** Configurable path
- **Progress:** Inline progress indicator (see above)

### CD Burning
- **Content:** DICOM images + standalone viewer
- **Label:** Patient info, study date, facility name
- **Verification:** Verify burned disc
- **Workflow:** 4-step wizard (see CD Burning Wizard above)

### PDF Export
- **Content:** Study report with key images
- **Format:** A4 portrait
- **Include:** Hospital logo, patient demographics

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Focus search |
| Ctrl+E | Export selected |
| Ctrl+B | Burn selected to CD |
| Ctrl+P | Print selected |
| Arrow Keys | Navigate rows |
| Enter | Open selected study |
| Space | Toggle checkbox in Export Mode |
| Ctrl+A | Select all (in Export Mode) |
| Escape | Close detail panel / Exit Export Mode |

---

## Accessibility

### Table/Grid Toggle
- Button with current state indication
- Keyboard accessible
- Screen reader announcement

### Detail Panel
- `role="dialog"` attribute
- Focus trap when open
- Escape to close

### Status Announcements
- Row count: "Showing 25 of 156 studies"
- Filter change: "Filtered to 142 completed studies"
- Export Mode: "Export mode active. 0 studies selected."
- Selection change: "{n} studies selected"

### Context Menu
- `role="menu"` on container
- `role="menuitem"` on each item
- Keyboard navigation: Arrow keys, Enter to activate, Escape to close

---

## Performance Targets

| Metric | Target |
|--------|--------|
| Initial Load | < 1s |
| Filter Change | < 300ms |
| Search Response | < 200ms |
| Detail Panel Open | < 100ms |
| Export Start | < 500ms |

---

## Edge Cases

1. **Large Result Sets:** Paginate after 500 items
2. **No Images:** Show placeholder in thumbnail
3. **Corrupted Data:** Show warning icon, allow view attempt
4. **Merge Candidates:** Highlight related studies for same patient
5. **Disc Full:** Show error in CD Burning Step 3 if disc capacity insufficient
6. **CD Drive Not Available:** Burn to CD button grayed out with tooltip "No disc drive detected"

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Active filter, selected row |
| `--primary-light` | #00AEEF | In Progress badge, focus rings |
| `--bg-surface (BackgroundPanel)` | #2A2A2A (변경: #16213E→#2A2A2A) | Page background |
| `--bg-card (BackgroundCard)` | #3B3B3B (변경: #0F3460→#3B3B3B) | Panel backgrounds |
| BackgroundPage | #242424 | 주 배경 (변경: #1A1A2E→#242424) |
| `--border-default` | #2E4A6E | Table borders |
| `--text-primary` | #FFFFFF | Primary text |
| `--text-muted` | #B0BEC5 | Cancelled status, default filter |
| `--error` | #D50000 | Rejected badge |
| `--warning` | #FFD600 | Merge candidate highlight |
| `--success` | #00C853 | Completed badge |

---

## ViewModel Reference

**IStudylistViewModel:**
- `NavigatePreviousCommand` — Navigate to previous studies/time period
- `NavigateNextCommand` — Navigate to next studies/time period
- `PacsServers` — List of available PACS servers
- `SelectedPacsServer` — Currently selected PACS server
- `FilterByPeriodCommand` — Apply period filter (Today, 3Days, 1Week, All, 1Month)
- `ActivePeriodFilter` — Current active period filter
- `SearchText` — Real-time search input binding
- `StudyList` — ObservableCollection<Study> for data binding

---

## Related Documents

- [Worklist Screen](worklist.md)
- [Merge Screen](merge.md)
- [Component Library](../component_library.md)

---

**Version:** 2.0
**Last Updated:** 2026-04-07
**Status:** Active
**Changes v2.0:** PPT 슬라이드 2-4 — 레이아웃 변경 (헤더 네비게이션 및 PACS 선택기 추가), DataGrid 컬럼 재정의 (PatientID, AccNo, ExamDate, BodyPart, Description), 색상 토큰 업데이트
