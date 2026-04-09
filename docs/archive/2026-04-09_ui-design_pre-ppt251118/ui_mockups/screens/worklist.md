# Screen Specification: Worklist

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-WORKLIST-001 |
| **Priority** | Critical |
| **User Type** | Radiographers, Radiologists |
| **Purpose** | Daily examination task list management |

---

## Layout Specification

### Structure

```
+----------------------------------------------------------+
| Worklist          [EMERGENCY]  [Search]  [Refresh]       |
|                   [#D50000]                              |
+----------------------------------------------------------+
| [Today] [3Days] [1Week] [1Month] [All]                   |
+----------------------------------------------------------+
| Acc #      | Patient ID | Name    | Date    | Proc |Status|
|------------+------------+---------+---------+------+------|
| ACC...001  | P20...001  | Kim,..  | 26-04-06| Chest| Blue |
| ACC...002  | P20...002  | Park,.. | 26-04-06| Abd.. | Gray |
| ...        | ...        | ...     | ...     | ...  | ...  |
+----------------------------------------------------------+
```

### 5-Click Workflow Navigation

The worklist is the primary entry point for the standard radiographer workflow. From worklist to image acquisition requires at most 5 interactions:

1. Select study row (click)
2. Confirm patient identity in patient confirmation modal (click)
3. Select anatomical marker (click)
4. Press PREP (Space key or button click)
5. Press Expose (Ctrl+E or button click)

Each step maps to a screen or modal. The worklist row double-click skips to Step 2 directly.

---

## Components

### Header Area
- **Title:** "Worklist"
- **Search Input:** 300px width, placeholder: "Search by Patient ID, Name, or Accession No..."
- **Action Buttons:** Emergency/Trauma, Refresh, Export

#### Emergency/Trauma Button (Fixed Position)

The Emergency/Trauma button is always visible in the worklist header. It allows immediate patient registration for trauma or walk-in emergency cases without waiting for a scheduled order.

| Property | Value |
|----------|-------|
| Position | Header bar, left of Search input |
| Label | "EMERGENCY" |
| Background | #D50000 (`--status-emergency`) |
| Text color | #FFFFFF |
| Minimum Height | 40px |
| Action | Opens Emergency Patient Registration modal |

**Emergency Registration Modal:**

```
+--------------------------------------------+
| Emergency / Trauma Registration            |
|                                            |
| Patient Name:  [________________]          |
| Patient ID:    [________________] (auto)   |
| DOB:           [____________]              |
| Sex:           (o) Male  ( ) Female        |
|                                            |
| Procedure:     [___Chest PA____________]   |
| Priority:      [o] Emergency  [ ] Urgent   |
|                                            |
|      [Cancel]          [Register & Open]   |
+--------------------------------------------+
```

- Patient ID auto-generated if left blank
- "Register & Open" immediately creates the study and opens the Acquisition screen
- Study appears at top of worklist with Emergency status badge

### Filter Bar
- **Buttons:** Today, 3 Days, 1 Week, 1 Month, All
- **Style:** Toggle buttons (only one active at a time)
- **Default:** Today

### Data Table

| Column | Width | Sortable | Filter |
|--------|-------|----------|--------|
| Accession No | 150px | Yes | Text match |
| Patient ID | 120px | Yes | Text match |
| Name | 150px | Yes | Text match |
| Exam Date | 100px | Yes | Date range |
| Procedure | 150px | Yes | Dropdown |
| Status | 100px | Yes | Status filter |

---

## Status Color Coding

> **CoreTokens source:** `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` (IEC 62366 compliant)

| Status | Color | CoreToken | Badge Style | Meaning |
|--------|-------|-----------|-------------|---------|
| Waiting | #B0BEC5 | `--text-secondary` | Gray | Not started |
| In Progress | #00AEEF | `--accent` | Blue | Currently being acquired |
| Completed | #00C853 | `--status-safe` | Green | Successfully completed |
| Emergency | #D50000 | `--status-emergency` | Red | Urgent/priority case |

---

## Interaction Specifications

### Row Selection
- **Click:** Select row (show border indicator)
- **Double-click:** Open study in acquisition or viewer
- **Hover:** Highlight row background

### Keyboard Navigation
- **Arrow Up/Down:** Navigate rows
- **Enter:** Open selected study
- **Ctrl+F:** Focus search input
- **F5:** Refresh list

### Context Menu (Right-click)
- Open Study
- View Patient Details
- Mark as Completed
- Cancel Study
- Export to DICOM

---

## Real-time Updates

### Auto-refresh
- **Interval:** 30 seconds (configurable in Settings)
- **Trigger:** Also triggered by DICOM MWL (Modality Worklist) push events
- **Indicator:** Top-right notification badge with count of new/changed studies
- **Behavior:** Preserves current selection, scroll position, and active filter

### Update Indicators
- **New Items:** Brief 2s highlight animation (background flash in `--accent` at 15% opacity)
- **Status Changes:** Badge color transitions with 300ms CSS transition
- **Removed Items:** Fade out animation (500ms)
- **Emergency Arrival:** New Emergency-status studies trigger an audible alert and scroll to top

### Real-time Update Specification

| Event | Visual Response | Audio |
|-------|----------------|-------|
| New study scheduled | Row added at sorted position with highlight | None |
| Study status changed | Badge color transitions | None |
| Emergency study registered | Row added at top, red Emergency badge | Alert beep |
| Study completed | Badge changes to green Completed | None |
| Study cancelled | Row fades out and removed | None |

---

## Data Loading States

### Initial Load
- Show skeleton loading state
- Display 8-10 placeholder rows

### Empty State
```
+------------------------------------------+
|        No studies found for              |
|           the selected date              |
|                                          |
|         [Change Date Filter]             |
+------------------------------------------+
```

### Error State
- Show error message
- Provide retry button
- Display last successful data if available

---

## Export Functionality

### Export Options
- **CSV Format:** For spreadsheet analysis
- **PDF Format:** For printing/reporting
- **DICOM Export:** For patient CD burning

### Export Dialog
- Date range selection
- Status filter
- Column selection
- Include images option

---

## Accessibility

### Table Navigation
- Proper `<table>` markup with `thead` and `tbody`
- `scope="col"` on header cells
- `aria-sort` attribute on sortable columns
- Row selection via keyboard (Space)

### Screen Reader Support
- Table caption: "Worklist showing 5 studies"
- Row count announcement
- Status announced as "Waiting", "In Progress", etc.

---

## Performance Targets

| Metric | Target |
|--------|--------|
| Initial Load | < 500ms |
| Search Response | < 200ms |
| Filter Change | < 100ms |
| Real-time Update | < 50ms |

---

## Edge Cases

1. **Large Dataset:** Show first 100, load more on scroll
2. **Duplicate Patients:** Highlight with warning icon
3. **Conflicting Appointments:** Show time overlap indicator
4. **Emergency Cases:** Always appear at top when sorted by priority

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Filter button active state |
| `--bg-surface (BackgroundPanel)` | #2A2A2A (변경: #16213E→#2A2A2A) | Page background |
| `--bg-card (BackgroundCard)` | #3B3B3B (변경: #0F3460→#3B3B3B) | Header card background |
| BackgroundPage | #242424 | 주 배경 (변경: #1A1A2E→#242424) |
| `--status-emergency` | #D50000 | Emergency button background |

---

## Related Documents

- [Studylist Screen](studylist.md)
- [Acquisition Screen](acquisition.md)
- [CD/Export Screen](cd-burning.md)
- [Component Library](../component_library.md)

---

**Version:** 1.1
**Last Updated:** 2026-04-07
**Status:** Active
**Changes v1.1:** PPT 슬라이드 1 — 색상 토큰 업데이트 (#242424, #3B3B3B, #2A2A2A)
