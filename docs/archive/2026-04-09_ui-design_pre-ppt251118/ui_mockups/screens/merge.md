# Screen Specification: Sync Study

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-MERGE-001 |
| **Priority** | Medium |
| **User Type** | Radiologists |
| **Purpose** | Image merge and stitching functionality for comparative studies |

---

## Layout Specification

### Three-Column Layout (Patient A | Preview | Patient B)

```
+----------------------------------------------------------+
|  [🔀] Sync Study                                         |
+------------------+------------------+------------------+
| Patient A        |    Preview       | Patient B        |
| [Search_______]  |  +----+ +----+  | [Search_______]  |
| [Name / ID]      |  |ImgA| |ImgB|  | [Name / ID]      |
| [Name / ID]      |  +----+ +----+  | [Name / ID]      |
|                  | [Thumbnail strip]|                  |
+------------------+------------------+------------------+
| [Cancel]                          [Sync Study]         |
+----------------------------------------------------------+
```

### Panel Widths
- **Study Panels:** Flexible (1fr each)
- **Preview Center:** Flexible (1fr)

---

## Study Panels (Left & Right)

### Study Selector
- **Input:** Text field with autocomplete
- **Search By:** Patient ID, Accession No, Study Date
- **Dropdown:** Recent studies from same patient

### Preview Area
- **Size:** Minimum 300px height
- **Content:** Selected image from thumbnails
- **Overlay:** Study information (date, view, parameters)

### Thumbnails
- **Size:** 100x100px
- **Selection:** Highlight active thumbnail (border: 2px solid #00AEEF)
- **Click:** Update main preview

---

## Center Preview Panel

### Preview Display
- **Content:** Merged result display with side-by-side comparison
- **Overlay:** Alignment indicators and sync status
- **Toggle Modes:** Side by Side / Overlay / Difference
- **Live Sync:** Real-time image alignment preview

---

## Action Buttons

### Bottom Button Bar
- **Cancel Button:** Return to previous screen without saving
  - Style: Secondary button
  - Action: Discard all changes
  
- **Sync Study Button:** Complete synchronization and merge
  - Style: Primary button (#1B4F8A background)
  - Action: Save as new merged study or update existing study
  - Confirmation: Save dialog (see Output Options section)

---

## Merge Modes

### Side-by-Side Mode
```
| Study A Image | Study B Image |
```
- Left: Study A
- Right: Study B
- Slider: Adjustable split position

### Overlay Mode
```
| Combined Image with Opacity Slider |
```
- Opacity slider: 0-100%
- Toggle: A on top / B on top

### Difference Mode
```
| Heatmap of differences |
```
- Color coding: Red (#D50000) significant diff, Green (#00C853) similar
- Sensitivity slider

---

## Image Stitching Sub-Workflow

Image stitching combines multiple images from a single study into a single continuous image (e.g., full-spine or full-leg from multiple exposures).

### Activation
- From Studylist: right-click study -> "Stitch Selected Images"
- From this screen: select a study with multiple images, click "Stitch Mode" tab

### Stitching Layout

```
+------------------------------------------+
| [Stitch Mode] [Merge Mode]               |
|                                          |
| Images to Stitch (drag to reorder):      |
| [img1][img2][img3]  ->  [Stitch Result] |
|                                          |
| Overlap: [====|====] 40%                 |
| Orientation: [Vertical v]                |
+------------------------------------------+
| Auto-Match Algorithm:                    |
| [Matching...] [=======     ] 65%         |
|                                          |
| Manual Alignment Controls (if needed):   |
| X Offset: [----|---] 0px                 |
| Y Offset: [----|---] 0px                 |
| Rotation: [----|---] 0.0 deg             |
|                                          |
| [Reset Alignment]                        |
+------------------------------------------+
| [Before] / [After] toggle                |
| [Accept Stitch] [Reject / Redo]          |
+------------------------------------------+
```

### Auto-Match Algorithm Status
- **Display:** Inline below image panels
- **States:**
  - Idle: "Ready — click Stitch to begin"
  - Running: "Matching... {n}%" with animated progress bar
    - Color: #00AEEF
    - Bar fills left-to-right as matching progresses
  - Success: "Match complete — confidence: High (94%)" in #00C853
  - Low confidence: "Match complete — confidence: Low (42%). Manual alignment recommended." in #FFD600
  - Failed: "Auto-match failed. Please use manual alignment." in #D50000
- **ARIA:** `aria-live="polite"` on status container

### Manual Alignment Sliders

| Control | Range | Default | Step | Unit |
|---------|-------|---------|------|------|
| X Offset | -200 to +200 | 0 | 1 | px |
| Y Offset | -500 to +500 | 0 | 1 | px |
| Rotation | -10.0 to +10.0 | 0.0 | 0.1 | degrees |

- **Slider styling:**
  - Track: #2E4A6E
  - Thumb: #1B4F8A (primary), 20px circle
  - Active thumb: #00AEEF with glow
  - Negative value region: subtle red tint
  - Positive value region: subtle blue tint
- **Numeric input:** Adjacent to each slider for precise entry
- **Reset individual:** "Reset" button beside each slider
- **Reset All:** "Reset Alignment" button resets all three to 0
- **Live preview:** Image updates in real-time as sliders move
- **ARIA:** `role="slider"` with `aria-valuenow`, `aria-valuemin`, `aria-valuemax`, `aria-label`

### Before/After Comparison Toggle
- **Position:** Below alignment controls, above accept/reject buttons
- **Display:**
  - "Before" shows original unstitched images side-by-side
  - "After" shows stitched result
- **Toggle style:**
  - Two-button toggle: [Before] [After]
  - Active button: #1B4F8A background, white text
  - Inactive: #2E4A6E border, #B0BEC5 text
- **Keyboard:** Left/Right arrow keys cycle when toggle has focus
- **ARIA:** `role="radiogroup"` with `role="radio"` buttons

### Accept / Reject Stitch

#### Accept
- **Label:** "Accept Stitch"
- **Style:** Primary button, #1B4F8A
- **Action:** Saves stitched image as new image in the study
- **Confirmation dialog:**
  ```
  +----------------------------------------+
  | Save Stitched Image                    |
  |                                        |
  | Save as:                               |
  | ( ) New image in existing study        |
  | ( ) New separate study                 |
  |                                        |
  | Preserve source images: [x] Yes        |
  |                                        |
  | [Cancel]           [Save Stitched]     |
  +----------------------------------------+
  ```
- **On save:** Toast "Stitched image saved." Navigate to viewer showing result.

#### Reject / Redo
- **Label:** "Reject / Redo"
- **Style:** Secondary button with warning icon
- **Action:** Discards stitch result, returns to image selection
- **No confirmation required** (non-destructive — source images unchanged)

---

## Adjustment Dialog

### Available Adjustments
| Adjustment | Range | Default |
|------------|-------|---------|
| Brightness | -100 to +100 | 0 |
| Contrast | -100 to +100 | 0 |
| Pan X/Y | -500 to +500 px | 0 |
| Zoom | 50% to 200% | 100% |
| Rotation | -180 to +180 deg | 0 |

### Controls
- Slider for each adjustment
- Reset button for each
- Reset All button

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S | Sync studies |
| Ctrl+P | Preview result |
| Ctrl+Z | Undo last adjustment |
| Ctrl+R | Reset all adjustments |
| Ctrl+Enter | Save merge / Accept stitch |
| Escape | Cancel/close |
| Left/Right | Toggle Before/After (when toggle focused) |

---

## Validation

### Pre-Merge Checks
1. Verify both studies have images
2. Check image compatibility (same modality, similar view)
3. Validate patient match (warn if different)
4. Confirm merge options

### Pre-Stitch Checks
1. Minimum 2 images required in study
2. Images should have same modality (warn if different)
3. Auto-match runs before showing manual controls

### Warnings

#### Different Patients
```
+--------------------------------------+
|  WARNING: Different Patients        |
|  Study A: Kim, Min-jun              |
|  Study B: Park, Su-jin              |
|                                      |
|  Continue anyway?                    |
|  [Cancel] [Continue]                |
+--------------------------------------+
```

#### Incompatible Views
```
+--------------------------------------+
|  WARNING: Different Views            |
|  Study A: Chest PA                   |
|  Study B: Abdomen AP                 |
|                                      |
|  Merge may not be meaningful         |
|  [Cancel] [Continue]                |
+--------------------------------------+
```

#### Low Auto-Match Confidence
```
+--------------------------------------+
|  Auto-Match: Low Confidence (42%)    |
|  Manual alignment recommended        |
|  before accepting stitch.            |
|                                      |
|  [Continue with Auto] [Adjust Manually] |
+--------------------------------------+
```

---

## Output Options

### Save as New Study
- Create new accession number
- Copy patient info
- Include source study references

### Update Existing Study
- Add merged images to Study B
- Preserve original images
- Add merge tag to metadata

### Export Only
- Export merged image to file
- No database modification
- Formats: PNG, JPEG, DICOM secondary capture

---

## 명칭 변경 이력

| 기존 | 신규 | PPT 슬라이드 |
|-----|-----|------------|
| Same Studylist | **Sync Study** | 슬라이드 13 |
| Merge Button | **Sync Study Button** | 슬라이드 13 |

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Sync button, Accept button, active toggle |
| `--primary-light` | #00AEEF | Thumbnail selection border, matching progress bar |
| `--bg-surface` | #16213E | Page background |
| `--bg-card` | #0F3460 | Panel backgrounds |
| `--border-default` | #2E4A6E | Panel borders, slider track |
| `--border-focus` | #00AEEF | Focus rings |
| `--text-primary` | #FFFFFF | Primary text |
| `--text-muted` | #B0BEC5 | Inactive toggle text, hints |
| `--error` | #D50000 | Auto-match failed, difference mode heatmap |
| `--warning` | #FFD600 | Low-confidence match warning |
| `--success` | #00C853 | High-confidence match status, difference similar |

---

## ViewModel Reference

**IMergeViewModel:**
- `PatientAStudy` — Selected study A data binding
- `PatientBStudy` — Selected study B data binding
- `SyncStudyCommand` — Execute merge/sync operation
- `CancelCommand` — Cancel without saving
- `PreviewMode` — Current preview mode (SideBySide, Overlay, Difference)
- `AlignmentStatus` — Auto-match algorithm status

---

## Related Documents

- [Studylist Screen](studylist.md)
- [Component Library](../component_library.md)

---

**Version:** 1.2
**Last Updated:** 2026-04-07
**Status:** Active
**Changes v1.2:** PPT 슬라이드 13 — "Same Studylist" → "Sync Study" 명칭 변경, 3열 레이아웃 (Patient A | Preview | Patient B) 강조, 버튼 레이블 업데이트, ViewModel 참고 추가
