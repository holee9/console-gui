# HnVue UI Component Library

## Design System Variables

> **NOTE:** All color values are sourced from `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` (authoritative).

### Colors

| Category | Variable | Value | Usage |
|----------|----------|-------|-------|
| **Background** | `--bg-page` | #1A1A2E | Page background |
| | `--bg-panel` | #16213E | Panel background |
| | `--bg-card` | #0F3460 | Card background |
| **Primary** | `--primary-main` | #1B4F8A | Primary actions, links |
| | `--primary-light` | #2E6DB4 | Hover states |
| | `--accent` | #00AEEF | Accent / focus / info |
| **Text** | `--text-primary` | #FFFFFF | Primary text |
| | `--text-secondary` | #B0BEC5 | Secondary / muted text |
| | `--text-disabled` | #546E7A | Disabled text |
| **Status (IEC 62366)** | `--status-safe` | #00C853 | Safe / success / completed |
| | `--status-warning` | #FFD600 | Warning |
| | `--status-blocked` | #FF6D00 | Blocked / attention |
| | `--status-emergency` | #D50000 | Emergency / danger / error |
| **Button** | `--btn-primary` | #1B4F8A | Primary button background |
| | `--btn-danger` | #D50000 | Danger button background |
| **Border** | `--border-default` | #2E4A6E | Default border |
| | `--border-focus` | #00AEEF | Focus ring border |

---

## Components

### Button

**Variants:** Primary, Secondary, Danger

**Sizes:**
- Medium: 36px height, 12px vertical padding
- Large: 44px height, 14px vertical padding
- Critical (for Emergency Stop and primary acquisition actions): 56px height minimum

**States:** Default, Hover, Active, Disabled

```html
<button class="btn btn-primary">Primary Button</button>
<button class="btn btn-secondary">Secondary Button</button>
<button class="btn btn-danger">Danger Button</button>
```

**Accessibility:**
- Minimum touch target for standard buttons: 44x44px
- Minimum touch target for critical buttons (Emergency Stop, Expose): 56px height, 100px width
- Visible focus indicator (2px solid `--border-focus`)
- ARIA labels for icon-only buttons

---

### EmergencyStopButton

Always-visible emergency stop control. Fixed position in window header on acquisition screen.

**Purpose:** Immediate halt of all generator activity. No confirmation dialog.

```
+------------------+
|   STOP           |
|   비상 정지       |
+------------------+
```

| Property | Value |
|----------|-------|
| Background | #D50000 (`--status-emergency`) |
| Text color | #FFFFFF |
| Minimum Width | 100px |
| Minimum Height | 56px |
| Border Radius | 4px |
| Keyboard | Escape key — always active regardless of focus |
| Position | Fixed, top-right of window header |
| Confirmation | None — immediate action |

**State Variants:**

| State | Appearance |
|-------|-----------|
| Normal (IDLE, PREP, COMPLETE) | Solid #D50000 background |
| Active (EXPOSING) | Pulsing animation (opacity 1.0 to 0.6, 0.8s cycle), 2px white border |

---

### DoseMonitoringPanel

Embedded panel displaying real-time radiation dose metrics. Used in the left panel of the Acquisition screen.

**Components within panel:**
- DAP value (real-time Gy·cm²)
- DRL percentage gauge bar with color thresholds
- EI and DI indicators with color coding
- Dose history link

**DRL Bar Color Thresholds:**

| Range | Color | Token |
|-------|-------|-------|
| 0–69% | #00C853 | `--status-safe` |
| 70–89% | #FFD600 | `--status-warning` |
| 90–99% | #FF6D00 | `--status-blocked` |
| 100%+ | #D50000 | `--status-emergency` |

**EI/DI Color Coding:**

| DI Range | Color |
|----------|-------|
| -2.0 to +2.0 | #00C853 |
| -3.0 to -2.0 or +2.0 to +3.0 | #FFD600 |
| beyond ±3.0 | #D50000 |

See [dose-monitoring.md](screens/dose-monitoring.md) for complete panel specification.

---

### PatientConfirmationModal

Modal dialog requiring explicit patient identity verification before the first exposure of a study.

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
| Patient photo | 80×80px; gray placeholder icon if not available |
| Warning text color | #FFD600 (`--status-warning`) |
| Confirm button | Disabled until user reads; enabled immediately on display |
| Trigger | First exposure of each study |

---

### InterlockStatusModal

Modal displaying interlock validation results when expose conditions are not met.

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

| Check State | Icon | Text Color |
|-------------|------|-----------|
| Passed | Green checkmark | #00C853 |
| Failed | Empty box | #FF6D00 (`--status-blocked`) |

---

### DoseWarningBanner

Inline warning banner shown within the Dose Monitoring Panel when DRL thresholds are crossed.

**Variants:**

| Variant | Trigger | Background | Border | Text |
|---------|---------|-----------|--------|------|
| Warning | DRL 70–89% | rgba(#FFD600, 0.15) | 4px solid #FFD600 | #FFD600 |
| Blocked | DRL 90–99% | rgba(#FF6D00, 0.15) | 4px solid #FF6D00 | #FF6D00 |
| Emergency | DRL 100%+ | rgba(#D50000, 0.15) | 4px solid #D50000 | #D50000 |

- Not dismissible by user action
- Disappears when DRL drops below threshold or study closes

---

### MarkerSelector

Radio button group for anatomical marker selection. Mandatory before Expose is enabled.

```
Anatomical Marker (required):
( ) Left    ( ) Right    ( ) Bilateral    ( ) N/A
```

**Behavior:**
- Expose button disabled until one option is selected
- After selection, confirmation label shown:
  ```
  Marker: LEFT  [green checkmark]
  ```
- Confirmation label: text #00C853, checkmark icon

---

### StatusBar

Always-visible bar at the bottom of the Acquisition screen showing system status at a glance.

```
+------------------------------------------------------------------+
| [TLS] DICOM: Connected        Session: 04:23 remaining  [DET OK]|
+------------------------------------------------------------------+
```

| Element | Description | Colors |
|---------|-------------|--------|
| TLS indicator | Lock icon | Green (#00C853) = TLS active; Yellow (#FFD600) = no TLS |
| DICOM status | Connection state text | Connected: #00C853; Disconnected: #D50000 |
| Session timer | Remaining time | White normally; #FFD600 when < 5 minutes |
| Detector status | DET OK / DET ERROR | #00C853 / #D50000 |
| Height | 32px | — |
| Background | `--bg-card` (#0F3460) | — |

---

### Input

**Types:** Text, Password, Number, Date, Select

**States:** Default, Focus, Error, Disabled

```html
<input type="text" class="input" placeholder="Placeholder text">
```

**Accessibility:**
- Associated label required
- Error messages inline
- Validation on blur (not on type)

---

### Card

**Purpose:** Container for related content

**Padding:** 16px

**Radius:** 12px

**Shadow:** 0 4px 12px rgba(0, 0, 0, 0.3)

---

### Data Table

**Purpose:** Display tabular data

**Features:**
- Row hover effect
- Selected row indicator
- Sortable columns
- Status badges

---

### Status Badge

**Variants:** Waiting, In Progress, Completed, Emergency

| Status | Color | Token |
|--------|-------|-------|
| Waiting | #B0BEC5 | `--text-secondary` |
| In Progress | #00AEEF | `--accent` |
| Completed | #00C853 | `--status-safe` |
| Emergency | #D50000 | `--status-emergency` |

```html
<span class="status-badge status-waiting">Waiting</span>
<span class="status-badge status-progress">In Progress</span>
<span class="status-badge status-completed">Completed</span>
<span class="status-badge status-emergency">Emergency</span>
```

---

### Modal

**Sizes:** Medium (600px max-width), Large (900px max-width)

**Structure:**
- Header: 56px height
- Body: Scrollable content
- Footer: 64px height with actions

---

### Toast

**Variants:** Success, Error, Warning, Info

**Duration:**
- Success/Info: 3 seconds
- Warning: 5 seconds
- Error: Requires action

**Position:**
- Success/Info: Bottom-right
- Error/Warning: Bottom-center

---

## Interaction States

### Button States

| State | Background | Border | Text |
|-------|-----------|--------|------|
| Default (Primary) | `--btn-primary` (#1B4F8A) | — | #FFFFFF |
| Hover (Primary) | `--primary-light` (#2E6DB4) | — | #FFFFFF |
| Active (Primary) | Darker (#164080) | — | #FFFFFF |
| Disabled | `--bg-panel` (#16213E) | `--border-default` (#2E4A6E) | `--text-disabled` (#546E7A) |
| Default (Danger) | `--btn-danger` (#D50000) | — | #FFFFFF |
| Hover (Danger) | #B71C1C | — | #FFFFFF |

### Input States

| State | Border | Shadow | Background |
|-------|--------|--------|------------|
| Default | `--border-default` (#2E4A6E) | — | `--bg-panel` (#16213E) |
| Focus | `--border-focus` (#00AEEF) | 0 0 0 2px rgba(0, 174, 239, 0.2) | `--bg-panel` (#16213E) |
| Error | `--status-emergency` (#D50000) | — | `--bg-panel` (#16213E) |

---

## Accessibility Guidelines

### WCAG 2.2 Level AA Compliance

1. **Color Contrast:** Minimum 4.5:1 for normal text, 3:1 for large text
2. **Touch Targets:** Minimum 44x44px for standard interactive elements; 56px height for critical controls
3. **Keyboard Navigation:** All functions accessible via keyboard
4. **Focus Indicators:** Visible focus state (2px solid `--border-focus`) for all interactive elements
5. **Screen Reader:** Semantic HTML and ARIA labels

### Medical Device Considerations

- **Safety First:** Error prevention over error recovery
- **Clear Indicators:** Visual and auditory feedback for critical actions
- **Emergency Stop:** Always accessible, fixed position in window header, Escape key binding
- **Confirmation Dialogs:** For patient identity (HRUS-001), PHI export (HRUS-009), and high-dose operations
- **IEC 62366 Colors:** Status colors use the defined safety color system — do not use these colors for decorative purposes

---

## Responsive Breakpoints

| Breakpoint | Width | Usage |
|------------|-------|-------|
| Desktop | > 1024px | Full layout (primary target) |
| Large | > 1400px | Max-width container |

Note: HnVue is a desktop medical console application. Mobile and tablet layouts are not required.

---

## File Structure

```
docs/ui_mockups/
├── component_library.md       # This file
├── screens/
│   ├── login.md              # Login screen spec
│   ├── worklist.md           # Worklist screen spec
│   ├── studylist.md          # Studylist screen spec
│   ├── acquisition.md        # Acquisition screen spec (v2.0)
│   ├── dose-monitoring.md    # Dose monitoring panel spec
│   ├── cd-burning.md         # CD/Export burning spec
│   ├── merge.md              # Merge screen spec
│   ├── settings.md           # Settings screen spec
│   └── patient.md            # Patient registration spec
└── assets/
    ├── icons/                # SVG icons
    └── images/               # Sample images
```

---

**Version:** 2.0
**Date:** 2026-04-06
**Status:** Draft for Review
**Changes from v1.0:** Added EmergencyStopButton, DoseMonitoringPanel, PatientConfirmationModal, InterlockStatusModal, DoseWarningBanner, MarkerSelector, StatusBar components. Updated button minimum sizes for critical controls. Fixed Status Badge colors to CoreTokens values. Updated file structure.
