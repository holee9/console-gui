# Screen Specification: Acquisition

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-ACQ-001 |
| **Priority** | Critical (CRITICAL PATH) |
| **User Type** | Radiographers |
| **Purpose** | Image acquisition control - Primary workflow |
| **Regulatory** | IEC 62366-1, PR-UX-026, FR-DM-001, FR-DM-015, HRUS-001 to HRUS-004, SWR-WF-045, FR-WF-020 to FR-WF-024 |

---

## PPT 명세 (슬라이드 9~11)

### 슬라이드 10 (1안) - 환자 정보 표시
헤더 영역에 환자 정보를 명확히 표시:
- `ID: 20251013002`
- `홍길동`
- `1988-01-01  M`

### 슬라이드 11 (2안/최종) - 환자 정보 재배치
- 환자 ID / 이름 / 생년월일 / 성별 정보를 상단에 간결하게 표시
- WorkflowView (260px 우측 패널) 상단에 환자 정보 배치

### 현재 구현 상태
WorkflowView.xaml은 260px 우측 패널로 구현됨. 환자 정보 표시 영역 추가는 다음 단계에서 WorkflowViewModel과 연계 예정.

**보존 사항:** 응급 정지(STOP) 버튼은 항상 보여야 함 (IEC 62366: Emergency Stop accessible at all times).

---

## Layout Specification

### Full Window Layout

```
+------------------------------------------------------------------+
| [Window Header]  [Title: Acquisition]   [STOP | 비상 정지  ]    |
|                                         [  #D50000 / always  ]  |
+--------+--------------------------+---------+--------------------+
| Patient|     Image Preview        | Control |                    |
| Info   |                          | Panel   |                    |
| 280px  |   [LIVE IMAGE AREA]      | 320px   |                    |
|        |                          |         |                    |
|        |   [Exposure Indicator]   |         |                    |
|--------|                          |---------|                    |
| Dose   |                          |         |                    |
| Monitor|                          |         |                    |
| Panel  |                          |         |                    |
+--------+--------------------------+---------+--------------------+
|              Image Strip (Thumbnails)                           |
+------------------------------------------------------------------+
| [Status Bar: DICOM status | TLS | Session Timer | Detector]     |
+------------------------------------------------------------------+
```

### Panel Widths
- **Left (Patient Info + Dose Monitor):** 280px
- **Center (Preview):** Flexible (1fr)
- **Right (Controls):** 320px
- **Bottom (Strip):** Full width, 100px height
- **Status Bar:** Full width, 32px height

---

## A. Emergency Stop Button (CRITICAL — IEC 62366 + PR-UX-026)

### Position and Appearance

The Emergency Stop button MUST be fixed in the top-right corner of the window header bar. It is NOT inside any panel. It remains visible and accessible on ALL acquisition screen states without exception.

```
+------------------------------------------------------------------+
| HnVue Acquisition                        [ STOP       ]         |
|                                          [ 비상 정지  ]         |
+------------------------------------------------------------------+
```

### Specification

| Property | Value |
|----------|-------|
| Position | Top-right corner of window header, fixed |
| Background | #D50000 (`--status-emergency`) |
| Text (line 1) | "STOP" — bold, 16px, #FFFFFF |
| Text (line 2) | "비상 정지" — regular, 11px, #FFFFFF |
| Minimum Width | 100px |
| Minimum Height | 56px |
| Border Radius | 4px |
| Keyboard Trigger | Escape — active in ALL states, regardless of focus |
| Confirmation | None — immediate action on click or Escape |

### State Variations

| Screen State | Button Appearance |
|-------------|-------------------|
| IDLE | Normal: solid #D50000 background |
| PREP | Normal: solid #D50000 background |
| EXPOSING | Pulsing animation (opacity 1.0 to 0.6 at 0.8s cycle), highlighted white border 2px |
| PROCESSING | Normal: solid #D50000 background |
| COMPLETE | Normal: solid #D50000 background |

### Behavior
- Clicking the button or pressing Escape immediately halts the generator
- No confirmation dialog is shown
- System transitions to IDLE state after emergency stop
- Audit log entry created automatically

---

## B. Left Panel: Patient Information

### Display Fields

| Field | Label | Format |
|-------|-------|--------|
| Patient ID | "Patient ID" | P20...001 |
| Name | "Name" | Last, First |
| DOB/Age | "DOB / Age" | YYYY-MM-DD / XXy |
| Sex | "Sex" | Male/Female |
| Accession No | "Accession No" | ACC... |
| Procedure | "Procedure" | Exam name |

### Styling
- Background: `--bg-panel` (#16213E)
- Padding: 16px
- Each field on separate row
- Label: `--text-secondary` (#B0BEC5)
- Value: Bold, `--text-primary` (#FFFFFF)

### Pediatric Warning Banner (HRUS-002)

When the registered patient's age is less than 18, display an amber warning banner immediately below the patient information fields, above the dose monitoring panel.

```
+----------------------------------------------+
| WARNING: Pediatric patient (Age: 8).         |
| Adult protocol selected.                     |
| [Use Recommended Protocol]  [Override]       |
+----------------------------------------------+
```

| Property | Value |
|----------|-------|
| Background | #FFD600 (`--status-warning`) at 20% opacity |
| Border-left | 4px solid #FFD600 |
| Text | #FFD600 (warning color) |
| Icon | Warning triangle before text |
| Trigger | Patient age < 18 years |
| Button: Use Recommended | Switches to age-appropriate protocol automatically |
| Button: Override | Allows adult protocol with acknowledgment logged |

---

## C. Dose Monitoring Panel (CRITICAL — FR-DM-001, FR-DM-015)

Position: Below the patient information section in the left panel.

### Layout

```
+-------------------------------+
| Dose Monitoring               |
+-------------------------------+
| DAP: 2.34 Gy·cm²             |
|                               |
| DRL: 72%                      |
| [||||||||--] 72%              |
|                               |
| EI: 320  [green]              |
| DI: +0.8  [green]             |
+-------------------------------+
```

### DAP (Dose Area Product)
- Label: "DAP:"
- Value: Real-time, updates during and after each exposure
- Unit: Gy·cm²
- Format: X.XX Gy·cm²
- Color: White normally; transitions with DRL threshold

### DRL Progress Bar (Diagnostic Reference Level)

| DRL Range | Bar Color | Text Color | Action |
|-----------|-----------|------------|--------|
| 0–69% | #00C853 (`--status-safe`) | #00C853 | None |
| 70–89% | #FFD600 (`--status-warning`) | #FFD600 | Amber warning banner shown |
| 90–99% | #FF6D00 (`--status-blocked`) | #FF6D00 | Modal confirmation required before next exposure |
| 100%+ | #D50000 (`--status-emergency`) | #D50000 | Physician PIN required before any further exposure |

Bar visual: Filled rectangle proportional to percentage, color changes at thresholds.

### DRL Warning Banners

**70–89% (Warning):**

```
+----------------------------------------------+
| Dose approaching DRL limit (72%).            |
| Review exposure parameters.                  |
+----------------------------------------------+
```
- Background: #FFD600 at 15% opacity, left border 4px #FFD600

**90–99% (Blocked — Modal required):**

```
+----------------------------------------------+
|  Dose near DRL limit (93%).                  |
|  Confirm to proceed with next exposure.      |
|                                              |
|        [Cancel]     [Confirm Proceed]        |
+----------------------------------------------+
```
- Modal must be dismissed before expose button re-enables

**100%+ (Emergency — Physician PIN required):**

```
+----------------------------------------------+
|  DRL EXCEEDED (102%).                        |
|  Physician authorization required.           |
|                                              |
|  Physician PIN: [________]                   |
|                                              |
|        [Cancel]     [Authorize]              |
+----------------------------------------------+
```

### EI and DI Indicators

| Indicator | Description | Color Thresholds |
|-----------|-------------|-----------------|
| EI (Exposure Index) | Numeric value | Green if within ±2 DI; amber if ±2 to ±3; red if beyond ±3 |
| DI (Deviation Index) | +/- numeric value | Green: -2 to +2; Amber: -3 to -2 or +2 to +3; Red: beyond ±3 |

Display format:
```
EI:  320   [  green dot  ]
DI:  +0.8  [  green dot  ]
```

### Dose History Link
- Text link at bottom of panel: "View Dose History"
- Opens dose history for current patient study

---

## D. Center Panel: Image Preview

### Preview Area
- **Size:** Minimum 400px height, flexible width
- **Background:** #0a0a1a (darker than main background)
- **Border Radius:** 12px

### Exposure Indicator
- **Position:** Top right corner of preview area
- **Style:** Rounded pill with animated dot
- **States:** (see 5-State Flow section below)

### Image Placeholder
When no image is available:
- Centered SVG icon
- Text: "Image Preview Area"
- Subtext: "1600 x 1200 pixels"

---

## E. Right Panel: Acquisition Controls

### Anatomical Marker Selector (HRUS-004 — mandatory before expose)

Displayed at the top of the right control panel. The Expose button remains disabled until a marker is selected.

```
Anatomical Marker (required):
( ) Left    ( ) Right    ( ) Bilateral    ( ) N/A
```

- Radio buttons — one must be selected
- Selected state shown with filled radio and green confirmation label:
  ```
  Marker: LEFT  [green checkmark]
  ```
- If user attempts to expose without marker selected, the interlock modal appears (see section F)

### Body Part Selection

```
[Chest] [Abdomen] [Skull]
[Spine] [Pelvis] [Extremity]
```
- Grid: 3 columns
- Only one selectable at a time
- Visual feedback on selection

### Projection Selection

```
[PA] [AP] [LAT] [OBL]
```
- Grid: 4 columns
- Mutually exclusive

### Exposure Settings

| Control | Type | Range | Default |
|---------|------|-------|---------|
| kVp | Number input | 40–150 | 110 |
| mAs | Number input | 0.1–630 | 2.5 |
| AEC | Toggle | — | Off |

### Action Buttons

#### PREP Button (IDLE state only)
- **Text:** "Prepare"
- **Shortcut:** Space (in IDLE state)
- **Style:** Primary (#1B4F8A), large (56px height)
- **Becomes hidden once generator is ready (transitions to Expose)**

#### Expose Button (PREP complete state only)
- **Text:** "Expose"
- **Shortcut:** Ctrl+E (only in PREP complete state)
- **Style:** Primary (#1B4F8A), large (56px height)
- **Disabled when:** Marker not selected, patient not confirmed, interlock conditions unmet
- **Disabled appearance:** Background #16213E, border #2E4A6E, text #546E7A

#### Emergency Stop Button
- See Section A for full specification
- Also shown as secondary reference in control panel, but primary is the header button

---

## F. Patient Confirmation Modal (HRUS-001 mitigation)

This modal MUST appear before the first exposure of any study. It requires explicit confirmation before the expose action can proceed.

```
+--------------------------------------------+
| Confirm Patient Selection                  |
|                                            |
| [Photo   ] Name:       Kim, Min-jun        |
| [80x80px ] Patient ID: P20260406001        |
|            DOB:        1985-03-15 (41)     |
|            Sex:        Male                |
|                                            |
| Verify patient identity before exposing   |
|                                            |
| [Select Different Patient]  [Confirm  ]   |
+--------------------------------------------+
```

| Property | Value |
|----------|-------|
| When shown | Before first exposure of the current study |
| Patient photo | 80×80px; placeholder icon if no photo available |
| Background | `--bg-card` (#0F3460) |
| Warning text color | #FFD600 (`--status-warning`) |
| Button: Select Different Patient | Navigates back to worklist |
| Button: Confirm | Allows expose to proceed; logs confirmation with timestamp |
| Two-click minimum | Patient must first be selected from worklist, then confirmed here |

---

## G. Interlock Validation Modal (SWR-WF-045)

Shown when user attempts to expose but one or more interlock conditions are unmet.

```
+--------------------------------------------+
| Cannot Expose - Check Status               |
|                                            |
| [v] Patient Registered: P20260406001       |
| [ ] Generator Ready: Warming up (~2 min)   |
| [v] Detector Ready                         |
| [ ] Protocol Applied: Select protocol      |
| [v] Marker Set: LEFT                       |
|                                            |
|              [Retry]       [Cancel]        |
+--------------------------------------------+
```

### Interlock Conditions

| Condition | Check | Failure Message |
|-----------|-------|-----------------|
| Patient Registered | Patient loaded and confirmed | "No patient selected" |
| Generator Ready | Generator warm-up complete | "Warming up (~2 min)" |
| Detector Ready | Detector communication OK | "Detector not responding" |
| Protocol Applied | kVp and mAs values set | "Select protocol first" |
| Marker Set | Anatomical marker selected | "Select anatomical marker" |

- Checked items: Green checkmark, text in #FFFFFF
- Unchecked items: Empty box, text in #FF6D00 (`--status-blocked`)
- Button: Retry — re-evaluates all conditions
- Button: Cancel — closes modal, returns to IDLE state

---

## H. 5-State Exposure Flow (FR-WF-020 to FR-WF-024)

### State Machine

```
IDLE --> [PREP] --> EXPOSING --> PROCESSING --> COMPLETE
  ^                                                 |
  |_________________________________________________|
  (auto-reset or manual new acquisition)

Any state --> IDLE (via Emergency Stop)
```

### State Definitions

#### State 1: IDLE
- Exposure indicator: Green dot, label "READY"
- PREP button: Enabled
- Expose button: Hidden
- Emergency Stop: Normal appearance (#D50000, no animation)
- All controls: Enabled

#### State 2: PREP
- Triggered by: Space key or PREP button
- Exposure indicator: Blue spinning dot, label "Generator preparing..."
- PREP button: Hidden (replaced by spinner)
- Expose button: Enabled only after PREP ACK received from generator
- Controls: kVp, mAs disabled during preparation
- Emergency Stop: Normal appearance

#### State 3: EXPOSING
- Triggered by: Ctrl+E or Expose button (PREP complete)
- Exposure indicator: Red pulsing dot, label "EXPOSING" in #D50000
- All controls: Disabled EXCEPT Emergency Stop
- Expose button: Disabled
- PREP button: Hidden
- Overlay: Semi-transparent red border on preview area
- Emergency Stop: Pulsing animation, white border highlight
- Audio: Exposure tone active

#### State 4: PROCESSING
- Triggered by: Exposure complete, image transfer begins
- Exposure indicator: Yellow dot, label "Processing image..."
- Progress bar: Visible in center panel, shows percentage (e.g., "65%")
- Color: #FFD600 (`--status-warning`)
- All controls: Disabled
- Emergency Stop: Normal appearance

#### State 5: COMPLETE
- Triggered by: Image processing finished
- Exposure indicator: Green dot, label "Image acquired"
- Image: Auto-displays in center preview panel
- Thumbnail: Added to bottom strip
- Confirmation text: "Image acquired" in #00C853
- Controls: Re-enabled for next acquisition
- Emergency Stop: Normal appearance

---

## I. Bottom Panel: Image Strip

### Thumbnail Display
- **Size:** 100×100px each
- **Gap:** 8px
- **Border:** 2px solid `--border-default` (#2E4A6E)
- **Selected:** 2px solid `--accent` (#00AEEF)
- **Overflow:** Horizontal scroll

### Thumbnail Content
- Image preview
- Number badge (e.g., "1/3")
- Thumbnail click updates main preview

---

## J. Status Bar (always-visible, bottom of window)

```
+------------------------------------------------------------------+
| [TLS] DICOM: Connected        Session: 04:23 remaining  [DET OK]|
+------------------------------------------------------------------+
```

| Element | Description |
|---------|-------------|
| TLS indicator | Lock icon (green = TLS active, yellow = no TLS) |
| DICOM status | "Connected" in #00C853 or "Disconnected" in #D50000 |
| Session timer | Remaining session time; amber when < 5 minutes |
| Detector status | "DET OK" in #00C853 or "DET ERROR" in #D50000 |
| Height | 32px |
| Background | #0F3460 (`--bg-card`) |

---

## K. Keyboard Shortcuts

| Shortcut | Action | Available In |
|----------|--------|--------------|
| Escape | Emergency Stop (immediate) | ALL states |
| Space | PREP | IDLE state only |
| Ctrl+E | Expose | PREP complete state only |
| F1 | Help | Always |
| F5 | Refresh preview | IDLE state |
| Ctrl+S | Save current image | COMPLETE state |
| Ctrl+Z | Undo last acquire | Multiple images acquired |

---

## L. Audio Feedback

### Beep Patterns
- **Single Beep:** Successful acquisition (COMPLETE state)
- **Double Beep:** Warning (e.g., high dose, DRL threshold)
- **Continuous Tone:** Error during exposure
- **No Sound:** Emergency stop (visual only)
- **Exposure Tone:** Audible during EXPOSING state

### Volume Control
- Adjustable in Settings
- Mute option available
- Default: 50% volume

---

## M. Error Handling

| Error | Display | Recovery |
|-------|---------|----------|
| Detector Not Ready | Interlock modal with condition listed | Wait for ready, then Retry |
| Generator Fault | Toast + interlock modal | Contact service |
| Exposure Timeout | Modal error | Retry or Cancel |
| Image Corrupted | Toast + Retry button | Re-acquire |
| Equipment Fault | Full screen error overlay | Contact service |
| DRL 90–99% | Modal confirmation required | Confirm or Cancel |
| DRL 100%+ | Physician PIN modal | Authorize with PIN or Cancel |

---

## Related Documents

- [Worklist Screen](worklist.md)
- [Dose Monitoring Panel](dose-monitoring.md)
- [CD/Export Screen](cd-burning.md)
- [Settings Screen - Devices](settings.md)
- [Safety Requirements](../../regulatory/IEC62366.md)
- [Component Library](../component_library.md)

---

**Version:** 2.1
**Last Updated:** 2026-04-07
**Status:** Draft - Pending Regulatory Review
**Changes from v2.0:** PPT 슬라이드 9~11 참조 정보 추가, 환자 정보 재배치 계획 명시.
**Changes from v1.0:** Added Emergency Stop fixed positioning, Dose Monitoring Panel, Patient Confirmation Modal, Interlock Modal, Marker Selector, Pediatric Warning, 5-State flow expansion, Status Bar, keyboard shortcuts update.
