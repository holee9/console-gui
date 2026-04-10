# Screen Specification: CD / Export Burning

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-CD-001 |
| **Priority** | High |
| **User Type** | Radiographers, Radiologists |
| **Purpose** | Export patient studies to CD/DVD or portable media with optional DICOM viewer |
| **Regulatory** | HRUS-009 (PHI confirmation before export) |

---

## Layout Specification

The CD/Export screen is a 4-step wizard. Each step occupies the full center panel. A step indicator bar is shown at the top throughout the wizard.

```
+------------------------------------------------------------+
| CD / Export                                                |
+------------------------------------------------------------+
| [Step 1: Select] -> [Step 2: Options] -> [Step 3: Confirm] -> [Step 4: Progress] |
+------------------------------------------------------------+
|                                                            |
|   [Step content area — see each step below]               |
|                                                            |
+------------------------------------------------------------+
| [Cancel]                   [Back]       [Next / Burn]      |
+------------------------------------------------------------+
```

### Step Indicator Bar

| Step | Label | Active Color | Completed Color | Pending Color |
|------|-------|-------------|-----------------|---------------|
| 1 | Study Selection | #00AEEF (accent) | #00C853 (safe) | #2E4A6E (border) |
| 2 | Export Options | — | — | — |
| 3 | PHI Confirmation | — | — | — |
| 4 | Progress | — | — | — |

- Active step: Filled circle with number, `--accent` (#00AEEF) background
- Completed step: Check icon, `--status-safe` (#00C853) background
- Pending step: Number only, `--border-default` (#2E4A6E) background

---

## Step 1: Study Selection

### Purpose
Allow the user to select one or more patient studies to include in the export.

### Layout

```
+------------------------------------------------------------+
| Select Studies to Export                                   |
+------------------------------------------------------------+
| [ ] Select All                                  [Filter]  |
+------------------------------------------------------------+
| [v] | Patient ID    | Name        | Date       | Modality |
|-----|---------------|-------------|------------|----------|
| [v] | P20260406001  | Kim, Min-jun| 2026-04-06 | CR       |
| [ ] | P20260406002  | Park, Ji-su | 2026-04-05 | CR       |
| [v] | P20260406003  | Lee, Hyun   | 2026-04-06 | DX       |
+------------------------------------------------------------+
| Selected: 2 studies (3 images)                            |
+------------------------------------------------------------+
```

### Column Specification

| Column | Width | Description |
|--------|-------|-------------|
| Checkbox | 40px | Multi-select; Select All at top |
| Patient ID | 140px | Unique patient identifier |
| Name | 160px | Last, First format |
| Date | 110px | YYYY-MM-DD |
| Modality | 90px | CR, DX, MG, etc. |

### Interaction
- Checkbox: Selects/deselects individual study
- Select All: Toggles all visible studies
- Row click: Toggles checkbox
- Filter button: Opens filter popover (date range, modality, status)

### Selection Summary
- Shown at bottom: "Selected: N studies (M images)"
- Color: `--text-secondary` (#B0BEC5)
- If no studies selected, Next button is disabled

### Status Indicators
- Studies with status "In Progress" shown with amber indicator: not selectable
- Studies with "Completed" status shown with green indicator: selectable
- Studies with "Error" status shown with red indicator: shown with warning icon, selectable but flagged

---

## Step 2: Export Options

### Purpose
Configure what is included in the export and how the data is handled.

### Layout

```
+------------------------------------------------------------+
| Export Options                                             |
+------------------------------------------------------------+
|                                                            |
| Include DICOM Viewer                                       |
| [ ] Include HnView viewer application on disc             |
|     (Adds ~45MB to disc; enables viewing on any PC)       |
|                                                            |
| Anonymization                                              |
| [ ] Anonymize patient data before export                  |
|     WARNING: Patient identity will be removed. This       |
|     action cannot be reversed after burning.              |
|                                                            |
| Report                                                     |
| [ ] Include radiology report (if available)               |
|     PDF format, appended to DICOM header                  |
|                                                            |
| Media                                                      |
| (o) CD (700 MB max)                                        |
| ( ) DVD (4.7 GB max)                                       |
| ( ) USB / Folder export                                    |
|                                                            |
| Estimated size: 128 MB of 700 MB available                |
+------------------------------------------------------------+
```

### Option Details

| Option | Default | Notes |
|--------|---------|-------|
| Include viewer | Unchecked | HnView DICOM viewer embedded on disc |
| Anonymize | Unchecked | Removes: Name, DOB, Patient ID from DICOM tags |
| Include report | Unchecked | Only shown if report exists for selected studies |
| Media type | CD selected | DVD and USB options depend on system hardware |

### Estimated Size Bar
- Shows current selected content size vs. media capacity
- Bar: `--status-safe` when under capacity, `--status-warning` at 80%, `--status-emergency` at 100%+
- If size exceeds media capacity: Next button disabled, error shown: "Content exceeds disc capacity. Reduce studies or choose larger media."

### Anonymization Warning
- When anonymize checkbox is ticked, an inline warning panel appears:

```
+----------------------------------------------+
|  Anonymization cannot be undone.             |
|  Patient identity will be permanently        |
|  removed from the exported files.            |
|  Original data in HnVue is not affected.     |
+----------------------------------------------+
```
- Background: rgba(#FFD600, 0.12), left border 4px #FFD600

---

## Step 3: PHI Confirmation (HRUS-009)

### Purpose
Require explicit confirmation that the user has verified the patient data before exporting protected health information.

### Layout

```
+------------------------------------------------------------+
| Confirm Export — PHI Verification                          |
+------------------------------------------------------------+
|                                                            |
|  You are about to export studies containing Protected      |
|  Health Information (PHI).                                 |
|                                                            |
|  Please verify the following before proceeding:           |
|                                                            |
|  Studies selected for export:                             |
|  +--------------------------------------------------+     |
|  | Kim, Min-jun   P20260406001   2026-04-06   CR    |     |
|  | Lee, Hyun      P20260406003   2026-04-06   DX    |     |
|  +--------------------------------------------------+     |
|                                                            |
|  [ ] I confirm these studies belong to the correct        |
|      patient and that this export is authorized.          |
|                                                            |
|  Exporting to: CD (estimated 128 MB)                      |
|  Viewer included: No                                      |
|  Anonymized: No                                           |
|                                                            |
|                          [Cancel]  [Proceed to Burn]      |
+------------------------------------------------------------+
```

### Confirmation Requirements
- Checkbox must be explicitly checked by user — "Proceed to Burn" is disabled until checked
- The studies table is read-only; user must go Back to Step 1 to change selection
- Summary of chosen options shown (media, viewer, anonymization)

### Anonymized Export Variant
When anonymize was selected in Step 2, PHI summary shows anonymized placeholder values:

```
| [Anonymous]  [Anonymized ID]  2026-04-06   CR    |
```

With note: "Patient identifiers will be removed during burn."

---

## Step 4: Progress and Completion

### Layout During Burning

```
+------------------------------------------------------------+
| Burning to CD...                                           |
+------------------------------------------------------------+
|                                                            |
|  [===================-------]  65%                        |
|                                                            |
|  Writing: Kim_Min-jun_20260406.dcm                        |
|  Files written: 12 of 18                                  |
|  Elapsed time: 00:01:24                                   |
|                                                            |
|  [Cancel Burn]                                            |
|                                                            |
+------------------------------------------------------------+
```

| Property | Value |
|----------|-------|
| Progress bar fill | `--accent` (#00AEEF) |
| Progress bar background | `--bg-card` (#0F3460) |
| Percentage text | #FFFFFF, right-aligned |
| Current file | Scrolling filename below bar |
| Cancel button | Active during burn; requires confirmation |

### Cancel During Burn Confirmation

```
+--------------------------------------------+
| Cancel Burn?                               |
|                                            |
| Cancelling now will result in an           |
| incomplete disc which may not be readable. |
|                                            |
|     [Continue Burning]   [Cancel Burn]     |
+--------------------------------------------+
```

- Button: Continue Burning — resumes burn
- Button: Cancel Burn — aborts, logs partial burn event

### Layout on Completion (Success)

```
+------------------------------------------------------------+
| Export Complete                                            |
+------------------------------------------------------------+
|                                                            |
|  [green check icon]                                       |
|  CD burned successfully.                                  |
|                                                            |
|  Studies exported: 2                                      |
|  Images exported: 18                                      |
|  Media: CD (128 MB used)                                  |
|  Time: 00:02:41                                           |
|                                                            |
|  [Eject Disc]            [Export Another]   [Close]       |
+------------------------------------------------------------+
```

- Check icon color: #00C853 (`--status-safe`)
- Button: Eject Disc — triggers OS/hardware eject command
- Button: Export Another — restarts wizard at Step 1
- Button: Close — closes the CD/Export screen and returns to previous screen

---

## Error Handling

| Error | Display | Recovery Option |
|-------|---------|-----------------|
| No disc inserted | Modal: "Please insert a disc" | [Retry] |
| Disc full / capacity exceeded | Step 2 warning, Next disabled | Reduce studies or change media |
| Write error during burn | Modal: "Write error. Disc may be damaged." | [Retry] or [Cancel] |
| Disc not writable (ROM) | Modal: "This disc cannot be written to." | [Cancel], insert blank disc |
| Viewer copy failed | Toast warning (non-blocking) | Export continues without viewer |
| PHI confirmation not checked | "Proceed to Burn" button remains disabled | User must check the checkbox |

### Error Modal Template

```
+--------------------------------------------+
| Export Error                               |
|                                            |
| [Error description]                        |
|                                            |
| Suggestion: [Recovery instruction]         |
|                                            |
|              [Cancel]        [Retry]       |
+--------------------------------------------+
```

- Header background: rgba(#D50000, 0.15)
- Header border-left: 4px solid #D50000

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Escape | Cancel wizard (confirmation modal shown) |
| Enter | Proceed to next step (if enabled) |
| Alt+B | Back to previous step |
| F1 | Help |

---

## Accessibility

- All checkboxes have associated labels
- Step indicator uses aria-current="step" on active step
- Progress bar uses role="progressbar" with aria-valuenow, aria-valuemin, aria-valuemax
- Error messages read by screen readers via aria-live="assertive"

---

## Related Documents

- [Worklist Screen](worklist.md)
- [Acquisition Screen](acquisition.md)
- [Component Library](../component_library.md)

---

**Version:** 1.0
**Last Updated:** 2026-04-06
**Status:** Draft
