# Screen Specification: Add Patient/Procedure

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-PATIENT-001 |
| **Priority** | High |
| **User Type** | Radiographers, Administrators |
| **Purpose** | New patient registration and procedure scheduling |

---

## Layout Specification

### Two-Column Layout

```
+----------------------+----------------------+
| Patient Information  | Procedure Selection  |
|                      |                      |
| [Photo] Patient ID[*]| Search Procedures    |
| Name [*]             | [Search MWL ->]      |
| DOB [*]  Age: 41y    | Available List       |
| Sex [*]              |                      |
| [Pediatric badge]    | Selected Procedures  |
| Accession No         |                      |
| Referring Physician  |                      |
| [Barcode scan input] |                      |
+----------------------+----------------------+
|  [Trauma/Emergency]  [Cancel] [Register Patient]  |
+----------------------------------------------------+
```

### Panel Widths
- Equal split (1fr each)
- Gap: 24px

---

## Left Panel: Patient Information

### Patient Photo Upload (Optional)
- **Position:** Top-left of patient information panel
- **Size:** 200×200px display area, 160×160px minimum
- **Label:** "Photo (optional)"
- **Interaction:**
  - Click area: opens file browser (JPG, PNG, max 5 MB)
  - Drag-and-drop supported
  - Shows placeholder icon when empty (person outline)
- **Cropping:** Crop tool shown after selection (square crop, centered)
- **ARIA:** `aria-label="Upload patient photo"`
- **Note:** Photo stored locally, not transmitted via DICOM

### Required Fields (*)

| Field | Label | Type | Validation | Default |
|-------|-------|------|------------|---------|
| patient_id | Patient ID | text | Required if auto-generate off | Auto |
| name | Name | text | Required, min 2 chars | - |
| dob | Date of Birth | date | Required, not future | - |
| sex | Sex | select | Required | - |
| accession_no | Accession No | text | Optional, unique | Auto |

### Auto-Calculated Age from DOB
- **Display:** "Age: {n} years" shown immediately to the right of the DOB field
- **Calculation:** `floor((today - DOB) / 365.25)` in years
- **Update:** Recalculated in real-time as DOB is typed
- **Format:**
  - >=2 years: "Age: 41 years"
  - 1–23 months: "Age: 14 months"
  - <1 month: "Age: {n} days"
- **Color:** #B0BEC5 (muted text, read-only indicator)
- **ARIA:** `aria-label="Calculated age"` aria-readonly="true"

### Pediatric Flag
- **Trigger:** Auto-calculated age < 18 years
- **Display:** Badge "Pediatric" shown below the name field on the patient card summary
- **Badge Style:**
  - Background: #FFD600 (warning)
  - Text: #16213E (dark, for contrast)
  - Border-radius: 4px
  - Padding: 2px 8px
  - Font: 12px bold
- **Behavior:** Badge appears/disappears dynamically as DOB changes
- **Warning on Protocol Selection:** If Pediatric patient has an adult protocol selected, show warning:
  ```
  +--------------------------------------------+
  |  WARNING: Adult Protocol Selected          |
  |  Patient age: 8 years (Pediatric)          |
  |  Selected protocol: Adult Chest PA         |
  |                                            |
  |  Recommended: Pediatric Chest PA           |
  |  [Use Recommended] [Keep Adult Protocol]   |
  +--------------------------------------------+
  ```
- **ARIA:** `aria-label="Pediatric patient"` role="status"

### Optional Fields

| Field | Label | Type | Validation |
|-------|-------|------|------------|
| referring_physician | Referring Physician | text with autocomplete | Must exist in system |

### Auto-Generate Toggle
- **Label:** "Auto-generate Patient ID"
- **Type:** Checkbox
- **Behavior:** When checked, disable manual input
- **Format:** P + YYYYMMDD + ### (e.g., P20260406001)

### Barcode / QR Scanner Input
- **Position:** Below Accession No field
- **Label:** "Scan Barcode / QR"
- **Visual Indicator:**
  - Icon: Barcode scan icon (left side of input)
  - Input field: Read-only text showing scanned value
  - Status indicator: "Ready to scan" (gray) / "Scanning..." (blue pulse) / "Scanned: {value}" (green)
- **Behavior:**
  - When scanner peripheral fires, input receives focus automatically
  - Scanned string auto-populates Patient ID or Accession No (based on format detection)
  - Confirmation toast: "Scanned: {value} — Patient ID populated"
- **Manual entry:** Also accepts keyboard input as fallback
- **ARIA:** `aria-label="Barcode or QR code scanner input"`

---

## Right Panel: Procedure Selection

### Search Input
- **Placeholder:** "Type to search procedures..."
- **Filter By:** Modality, Body Part, Procedure name
- **Debounce:** 300ms

### MWL Query Integration
- **Button:** "Search MWL ->" positioned next to search input
- **Action:** Opens MWL search overlay — queries Modality Worklist for scheduled procedures
- **MWL Result Display:**
  ```
  +------------------------------------------+
  | Modality Worklist Results                |
  |                                          |
  | [ACC-2026-001] Chest PA — Kim, Min-jun  |
  | Scheduled: 2026-04-06 14:00             |
  | Requested by: Dr. Park                  |
  | [Select]                                |
  |                                          |
  | [ACC-2026-002] Knee AP/LAT              |
  | Scheduled: 2026-04-06 15:30             |
  | [Select]                                |
  +------------------------------------------+
  ```
- **On Select:** Populates patient fields and procedure from MWL entry
- **ARIA:** `aria-label="Search Modality Worklist"`

### Available Procedures List
- **Height:** 300px max with scroll
- **Style:** Checkboxes with labels
- **Selection:** Multiple allowed
- **Highlight:** Selected items in #1B4F8A (primary color)

### Selected Procedures Display
- **Purpose:** Show selected count and names
- **Style:** Chip/tag display
- **Behavior:** Remove individual items by clicking x

---

## Trauma / Emergency Quick Registration

### Emergency Button
- **Label:** "Trauma/Emergency"
- **Position:** Bottom-left of form, next to Cancel
- **Style:**
  - Background: #D50000
  - Text: White, bold
  - Icon: Lightning bolt / emergency icon
  - Height: 44px
- **Behavior on Click:**
  ```
  +------------------------------------------+
  |  Trauma/Emergency Registration           |
  |                                          |
  |  Patient ID auto-generated:              |
  |  TRAUMA-20260406-001                     |
  |                                          |
  |  Name: (optional — enter if known)       |
  |  [ Unknown Patient            ]         |
  |                                          |
  |  Sex: [Male] [Female] [Unknown]          |
  |                                          |
  |  [Begin Trauma Protocol]                |
  +------------------------------------------+
  ```
- **Temp ID Format:** `TRAUMA-YYYYMMDD-###`
- **Required:** Sex selection only (or "Unknown")
- **Name:** Optional — defaults to "Unknown Patient"
- **Post-Registration:** Directly opens acquisition screen with emergency protocol pre-selected
- **Merge Reminder:** Yellow banner after acquisition: "Remember to link trauma case to actual patient record"
- **ARIA:** `aria-label="Start trauma or emergency quick registration"`
- **Keyboard:** No accidental trigger — requires click or Space/Enter when focused

---

## Procedure Data Structure

### Each Procedure Contains
```json
{
  "id": "PROC001",
  "name": "Chest PA",
  "modality": "CR",
  "body_part": "Chest",
  "projection": "PA",
  "default_kv": 110,
  "default_mas": 2.5,
  "requires_contrast": false,
  "pediatric_variant": "PROC001-PED"
}
```

---

## Validation Rules

### Patient ID
- If manual: 3-20 alphanumeric characters
- Must be unique in system
- Auto-generated format enforced

### Name
- Required: minimum 2 characters
- Format: "Last, First" or "First Last"
- Special characters allowed: hyphen, apostrophe, space

### Date of Birth
- Required
- Cannot be future date
- Reasonable range: 1900-current date
- Age auto-calculated on valid entry

### Sex
- Required
- Options: Male, Female, Other

### Accession Number
- Optional
- Must be unique if provided
- Auto-generated if empty: ACC + YYYYMMDD + ###

---

## Duplicate Patient Check

### Trigger
On Patient ID blur or Name + DOB entry

### Check Logic
```sql
SELECT COUNT(*) FROM patients
WHERE patient_id = ? OR (name = ? AND dob = ?)
```

### If Duplicate Found
```
+----------------------------------------+
|  WARNING: Possible Duplicate Patient   |
|                                        |
|  Existing: P20250315001                |
|  Name: Kim, Min-jun                    |
|  DOB: 1985-03-15                       |
|                                        |
|  Use existing patient?                 |
|  [Create New] [Use Existing]           |
+----------------------------------------+
```

---

## Form Actions

### Register Button
- **Style:** Primary button, background #1B4F8A
- **Enabled When:** All required fields valid + at least 1 procedure
- **Action:** Create patient record + schedule procedures
- **Success:** Navigate to Worklist with new study highlighted

### Cancel Button
- **Style:** Secondary button
- **Action:** Return to previous screen
- **Confirmation:** "Discard new patient registration?"

### Trauma/Emergency Button
- **Style:** Danger button, background #D50000
- **Action:** Open emergency registration modal (minimal fields)

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S | Save/Register (when valid) |
| Ctrl+N | Clear form / Start new |
| Escape | Cancel / Close |
| Tab | Next field |
| Shift+Tab | Previous field |

---

## Accessibility

### Required Field Indicators
- Asterisk (*) in red (#D50000) after label
- ARIA `required` attribute
- Screen reader: "Patient ID, required"

### Age Display
- `aria-live="polite"` — updates announced when DOB changes
- `aria-readonly="true"` on age field

### Pediatric Badge
- `role="status"` with `aria-label="Pediatric patient"`
- Announced when badge appears/disappears

### Error Display
- Inline below field
- Red text (#D50000) with icon
- ARIA `aria-invalid="true"` on error
- ARIA `aria-describedby` linking to error message

### Focus Management
- Auto-focus first field on load
- Return focus to field after validation error
- Focus trap in modal dialogs

---

## Post-Registration Flow

```
Register Successful
         |
         v
Create Patient Record
         |
         v
Create Procedure(s)
         |
         v
Generate Accession Number
         |
         v
Navigate to Worklist
         |
         v
Highlight New Study (In Progress)
```

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Register button, selection highlight |
| `--primary-light` | #00AEEF | Focus rings, MWL button accent |
| `--bg-surface` | #16213E | Page background |
| `--bg-card` | #0F3460 | Panel backgrounds |
| `--border-default` | #2E4A6E | Input borders |
| `--border-focus` | #00AEEF | Focused input outline |
| `--text-primary` | #FFFFFF | Primary text |
| `--text-muted` | #B0BEC5 | Age auto-calc, hints |
| `--error` | #D50000 | Required field asterisks, errors |
| `--warning` | #FFD600 | Pediatric badge, duplicate warning |
| `--success` | #00C853 | Barcode scanned confirmation |

---

## Related Documents

- [Worklist Screen](worklist.md)
- [Acquisition Screen](acquisition.md)
- [Component Library](../component_library.md)

---

**Version:** 1.1
**Last Updated:** 2026-04-06
**Status:** Draft
**Changes v1.1:** Added auto-calculated age from DOB, patient photo upload, trauma/emergency quick registration, barcode/QR scanner indicator, pediatric flag with auto-detection and protocol warning, MWL query integration, CoreTokens color table
