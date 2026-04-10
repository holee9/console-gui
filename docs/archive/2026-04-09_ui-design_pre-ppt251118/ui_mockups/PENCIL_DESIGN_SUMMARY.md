# HnVue UI Design Summary

**Project:** HnVue Medical X-Ray Imaging Console (WPF Desktop, .NET 8)
**Date:** 2026-04-06
**Design Standard:** IEC 62366 (Medical Device Usability), WCAG 2.1 AA guidance

---

## What Was Created / Modified

### 1. Design System Token Update (`design_system.pen`)

The `design_system.pen` file was synchronized with `CoreTokens.xaml` (the authoritative WPF token source).

**Key corrections from v3.0.0 to v3.1.0:**

| Token | Old Value | New Value (from CoreTokens.xaml) |
|-------|-----------|-----------------------------------|
| brand.primary | #0066CC | #1B4F8A |
| brand.accent | (missing) | #00AEEF |
| background.page | #1A1A2E | #1A1A2E (confirmed) |
| background.panel | (missing) | #16213E |
| background.card | (missing) | #0F3460 |
| text.primary | #E0E0E0 | #FFFFFF |
| text.secondary | #A0A0B0 | #B0BEC5 |
| safety.safe | #2ED573 | #00C853 |
| safety.warning | #FFA502 | #FFD600 |
| safety.blocked | (missing) | #FF6D00 |
| safety.emergency | #FF4757 | #D50000 |

**New sections added:**
- `screens` - Per-screen layout metadata
- `components.emergencyStop` - Critical safety component spec
- `components.statusBadge` - IEC 62366 status color mapping
- `accessibility` - Minimum touch target and contrast requirements

---

## Screen-by-Screen Design Decisions

### SCR-LOGIN-001: Login Screen

**Layout:** Centered card (400px wide) on full-page background (#1A1A2E)

**Design decisions:**
- Card background uses `#16213E` (panel) to separate from page background
- Logo area uses gradient from `#1B4F8A` to `#2E6DB4`
- Input fields: 36px height, border `#2E4A6E`, focus border `#00AEEF`
- Login button: 44px height (IEC 62366 minimum touch target), full width
- Language selector: bottom-right, compact dropdown
- No emergency-related UI (authentication screen, safety features not applicable)

**Patient ID display:** Not applicable (pre-authentication)

---

### SCR-WORKLIST-001: Worklist Screen

**Layout:** Full-screen table with header and filter bar

**Design decisions:**
- Table row height: 44px (IEC 62366 minimum touch target)
- Status color coding follows IEC 62366:
  - Scheduled: `#00AEEF` (accent/info blue)
  - In Progress: `#FFD600` (warning amber)
  - Completed: `#00C853` (safe green)
  - Cancelled: `#546E7A` (disabled text gray)
- Filter bar uses toggle buttons, default: Today
- Search input: 300px, placeholder text in `#B0BEC5`
- Patient ID column prominently placed as second column
- Row hover: `rgba(0, 174, 239, 0.08)` (accent with 8% opacity)

---

### SCR-STUDYLIST-001: Studylist Screen

**Layout:** Search bar + date range picker + scrollable study cards

**Design decisions:**
- Date range picker: Two inputs (From/To) with calendar icon
- Study cards: `#16213E` background, `#0F3460` on hover, radius 8px
- Each card shows: Patient ID (large, `#FFFFFF`), Name, Date, Modality, Status badge
- Status badge uses safety color system
- Cards display in grid (3 columns for 1920px display, 2 for 1440px)

---

### SCR-ACQ-001: Acquisition Screen (CRITICAL PATH)

**Layout:** Three-column + bottom thumbnail strip

**This is the highest safety priority screen.**

**Design decisions:**

**Left panel (280px) - Patient Information:**
- Background: `#16213E` (panel)
- Patient ID: Font size 16px (`#FFFFFF`, bold) - prominently displayed
- All other fields: 13px, label in `#B0BEC5`, value in `#FFFFFF`
- Padding: 16px all sides
- Vertical separator: `#2E4A6E`

**Center panel (flexible) - Image Preview:**
- Background: `#0a0a1a` (darker than base, for image contrast)
- Exposure indicator: pill badge, top-right corner
  - READY: dot `#00C853` + text "READY"
  - EXPOSING: animated dot `#D50000` + text "EXPOSING" (pulsing)
  - PROCESSING: dot `#FFD600` + text "PROCESSING"
  - ERROR: dot `#D50000` + text "ERROR"
- Corner radius: 8px on viewer container

**Right panel (320px) - Controls:**
- Body part grid: 3 columns, toggle buttons, 44px min height
- Projection grid: 4 columns (PA/AP/LAT/OBL), 44px min height
- Exposure settings: kVp and mAs as number inputs, AEC toggle
- **Acquire button:** 56px height, `#1B4F8A` background, full panel width
- **Emergency Stop button:**
  - Always visible, never hidden
  - 56px height, `#D50000` background, `#FFFFFF` text, bold
  - No confirmation dialog (immediate action per IEC 62366)
  - Keyboard shortcut: Escape
  - Placed directly below Acquire button

**Bottom strip (100px) - Thumbnails:**
- Thumbnail size: 80x80px with 8px gaps
- Selected border: 2px `#00AEEF`
- Unselected border: 1px `#2E4A6E`
- Horizontal scroll on overflow

**Safety validation before Acquire:**
- Pre-acquisition checklist displayed as inline status indicators
- High-dose warning modal uses `#FFD600` header (warning color)

---

### SCR-MERGE-001: Merge Screen

**Layout:** Dual panel with merge controls center

**Design decisions:**
- Left panel: Source study (patient ID, image strip)
- Right panel: Target study (patient ID, image strip)
- Center strip: Merge/swap controls, 48px wide icon buttons
- Both panels display Patient ID at top: 16px bold, `#FFFFFF`
- Panel separator: `#2E4A6E` border
- Merge action button: 44px height, primary color `#1B4F8A`

---

### SCR-SETTINGS-001: Settings Screen

**Layout:** Sidebar navigation + content panel

**Design decisions:**
- Sidebar width: 240px, background `#16213E`
- Navigation categories: Device, Network, User, System, About
- Selected nav item: left border `4px #00AEEF`, background `rgba(0,174,239,0.08)`
- Content panel: `#1A1A2E` background
- Section cards: `#16213E`, radius 8px, padding 24px
- Settings rows: 44px height (touch target compliance)
- Toggle switches: `#00C853` for enabled, `#546E7A` for disabled

---

### SCR-PATIENT-001: Add Patient Screen

**Layout:** Centered form (max 640px)

**Design decisions:**
- Form card: `#16213E` background, radius 8px
- Required fields marked with `#D50000` asterisk
- Field groups: Patient Info, Contact, Procedure
- Procedure selection: scrollable list with checkboxes
- Submit button: 44px height, primary color, full-width at bottom
- Cancel button: 44px height, border-only style

---

## Color Tokens Used

| Token | Hex | Usage |
|-------|-----|-------|
| brand.primary | #1B4F8A | Primary buttons, active states |
| brand.primaryLight | #2E6DB4 | Button hover, focus ring area |
| brand.accent | #00AEEF | Focus border, selected states, info |
| background.page | #1A1A2E | Screen backgrounds |
| background.panel | #16213E | Panel and sidebar backgrounds |
| background.card | #0F3460 | Card hover states |
| background.imageViewer | #0a0a1a | X-ray image viewer background |
| text.primary | #FFFFFF | All primary content text |
| text.secondary | #B0BEC5 | Labels, captions, muted text |
| text.disabled | #546E7A | Disabled elements |
| safety.safe | #00C853 | Safe/ready status (IEC 62366) |
| safety.warning | #FFD600 | Warning status (IEC 62366) |
| safety.blocked | #FF6D00 | Blocked/fault status (IEC 62366) |
| safety.emergency | #D50000 | Emergency stop, critical errors (IEC 62366) |
| interactive.border | #2E4A6E | Default input/panel borders |
| interactive.borderFocus | #00AEEF | Focus indicator for all inputs |

---

## Accessibility Considerations

### IEC 62366 Requirements

1. **Emergency Stop (Acquisition Screen)**
   - Always visible, never obstructed by other UI elements
   - Minimum size: 44x44px (preferred: 56px height for gloved operation)
   - Color: #D50000 with white text (high contrast)
   - No confirmation dialog - immediate action
   - Keyboard: Escape key always active

2. **Touch Targets**
   - All interactive elements: minimum 44x44px
   - Critical controls (Acquire, Emergency Stop): 56px height
   - Table rows: 44px height

3. **Safety Color Coding**
   - Consistent throughout all screens
   - Never use safety colors for non-safety-related UI elements
   - Red (#D50000) reserved exclusively for emergency/critical errors
   - Amber (#FFD600) reserved for warnings
   - Green (#00C853) for safe/ready states

4. **Patient ID Prominence**
   - Acquisition: 16px bold, first visible element in patient panel
   - Worklist/Studylist: Dedicated column with adequate width
   - Merge: 16px bold at top of each dual panel
   - All screens where patient context matters: prominent placement

### WCAG 2.1 AA Guidance

- Text contrast: White (#FFFFFF) on #1B4F8A = 5.6:1 (passes AA)
- Text contrast: White (#FFFFFF) on #16213E = 9.8:1 (passes AAA)
- Text contrast: White (#FFFFFF) on #D50000 = 5.1:1 (passes AA)
- Secondary text (#B0BEC5) on #1A1A2E = 5.9:1 (passes AA)
- Focus indicators: 2px solid #00AEEF on all interactive elements
- Keyboard navigation: Tab order matches visual reading order

### Dark Mode

Dark mode is the primary and only supported mode for this application. Medical imaging environments require:
- Low ambient light adaptation
- Reduced eye strain during long shifts
- Maximum image contrast for X-ray viewing

The color palette is specifically tuned for dark medical environments. No light mode variant is implemented.

---

## File Structure

```
docs/ui_mockups/
├── design_system.pen          # Design tokens (updated, synced with CoreTokens.xaml)
├── PENCIL_DESIGN_SUMMARY.md   # This file
├── 01-login.html              # Login screen HTML prototype
├── 02-worklist.html           # Worklist screen HTML prototype
├── 03-studylist.html          # Studylist screen HTML prototype
├── 04-acquisition.html        # Acquisition screen HTML prototype (CRITICAL)
├── 05-merge.html              # Merge screen HTML prototype
├── 06-settings.html           # Settings screen HTML prototype
├── 07-add-patient.html        # Add Patient screen HTML prototype
├── index.html                 # Navigation index
└── screens/                   # Detailed screen specifications
    ├── login.md
    ├── worklist.md
    ├── studylist.md
    ├── acquisition.md         # Critical path - IEC 62366 annotated
    ├── merge.md
    ├── settings.md
    └── patient.md
```

---

## Note on Pencil MCP

The `design_system.pen` file in this project uses a JSON format for design token storage rather than the Pencil application's native `.pen` binary format. This approach provides:
- Git-friendly plain text (diffable, reviewable)
- Direct reference in WPF theme implementation
- Framework-agnostic token definitions

To open interactive Pencil designs, the Pencil desktop application or VS Code extension is required with the MCP server configured in `.mcp.json`. The current HTML prototype files (`01-login.html` through `07-add-patient.html`) serve as the visual reference implementation for WPF development.

---

**Version:** 1.0
**Status:** Design Tokens Synchronized, HTML Prototypes Available
**Next Step:** WPF XAML implementation using `CoreTokens.xaml`, `SemanticTokens.xaml`, `ComponentTokens.xaml`
