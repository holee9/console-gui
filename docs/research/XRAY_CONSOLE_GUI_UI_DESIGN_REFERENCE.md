# X-Ray Console GUI UI Design Ultimate Reference

**Global X-Ray Imaging Software Console GUI Deep Research & UI Design Reference Document**

---

## Document Purpose

This document is the definitive UI design reference for building a modern, user-friendly X-ray console GUI. It synthesizes deep research on every major X-ray imaging software worldwide, industrial/scientific UI best practices, NDT workflow standards, and WPF implementation patterns into an actionable design guide.

**Target Project:** HnVue (CsI+FPD X-ray/CT Console Application, WPF/.NET 8)
**Research Date:** 2026-04-07
**Classification:** UI Design Reference (Top-Tier)

---

# PART 1: GLOBAL X-RAY IMAGING SOFTWARE LANDSCAPE

## 1.1 Industrial X-Ray/CT Software

### Tier 1: Market Leaders (Comprehensive Feature Set)

#### 1. Volume Graphics (Hexagon) -- VGStudio MAX
- **Market Position:** Industry standard for industrial CT data visualization and analysis
- **UI Layout:** Ribbon-style toolbar + dockable panels/toolboxes + multi-viewport
- **Key UI Features:**
  - Multi-synchronized 2D slice views (Axial/Sagittal/Coronal) + 3D viewport in quad layout
  - Object tree: dataset, analysis, and result management in hierarchical panel
  - Modular add-on system: base edition + functional modules (geometry, porosity, fiber, etc.)
  - Python scripting and automation API for workflow customization
  - PTB/NIST certified metrology algorithms
  - myVGL free viewer for .vgl format sharing
- **Color Scheme:** Dark theme (gray background), red accent (Volume Graphics brand)
- **Design Strengths:** Industry's most comprehensive feature set, modular extensibility
- **Design Weaknesses:** Steep learning curve, feature density overwhelming for beginners
- **Lessons for HnVue:** Modular UI architecture; dockable panels; object tree for data management

#### 2. Waygate Technologies (Baker Hughes/GE) -- phoenix/inspekto/Rhythm Suite
- **Software Portfolio (Most Comprehensive):**
  - phoenix daten|x: Industrial CT acquisition and reconstruction
  - phoenix X|act: 2D X-ray inspection software
  - Rhythm: Radiography software suite
  - InspectionWorks: Cloud-based NDT software platform (real-time collaboration)
  - X|approver: ADR (Automatic Defect Recognition) for X-ray/CT
  - FLASH!: Intelligent image processing technology
  - PlanarCT: Planar CT software
- **UI Layout:** Task-based GUI, drag-and-drop workflow
- **Key UI Features:**
  - Cloud-connected InspectionWorks platform for real-time collaboration
  - ADR integration directly in inspection view
  - Real-time CT reconstruction preview
  - Multi-monitor support with configurable layouts
- **Color Scheme:** Dark theme, Baker Hughes brand (red/gray)
- **Design Strengths:** Most extensive software portfolio, cloud integration leadership
- **Lessons for HnVue:** Cloud connectivity; ADR integration; task-based workflow UI

#### 3. Nikon Metrology -- Inspect-X / CT Pro
- **UI Layout:** Traditional menu + toolbar + docking panel layout
- **Key UI Features:**
  - Automated and programmable inspection routines
  - Real-time X-ray viewer with integrated measurement tools
  - CT Pro: Slice views (axial/coronal/sagittal) + 3D volume rendering
  - Macro recording and playback
  - Advanced CT reconstruction with beam hardening correction
- **Color Scheme:** Dark theme, blue accent
- **Design Strengths:** Powerful automation capabilities, established ecosystem
- **Design Weaknesses:** Traditional GUI not modern touch-friendly
- **Lessons for HnVue:** Macro system; programmable routines; traditional layout for expert users

#### 4. Zeiss -- METROTOM / Scout-and-Scan / ORS Dragonfly
- **UI Layout:** Modular panel layout with metrology software integration
- **Key UI Features:**
  - Sample stage control panel (X, Y, Z, rotation)
  - Real-time X-ray detector preview with crosshair overlay
  - Scan parameter setup (voltage, power, exposure time, projections)
  - ROI selection tools
  - Queue/job manager for batch operations
  - Tight integration with Calypso metrology software
- **Color Scheme:** Zeiss brand white/blue, modern clean dark theme
- **Design Strengths:** Polished modern GUI, consistent design language with metrology tools
- **Design Weaknesses:** Requires switching between multiple software packages
- **Lessons for HnVue:** Parameter panel design; job queue UI; crosshair overlays

### Tier 2: Specialized Leaders

#### 5. Yxlon (Comet Yxlon) -- Yxlon FF GUI / Geminy Platform
- **UI Layout:** Workflow guide wizard-based interface (step-by-step)
- **Key UI Features:**
  - Dual mode: Beginner wizard mode + Expert advanced mode
  - Integrated defect detection and measurement tools
  - Real-time X-ray preview window
  - Automated inspection routine programming
- **Color Scheme:** Dark theme (dark gray background, light text)
- **Design Strengths:** Workflow-based UI reduces training time
- **Design Weaknesses:** Expert mode can be feature-cluttered
- **Lessons for HnVue:** Dual-mode UI (wizard for beginners, full for experts); workflow step indicator

#### 6. Bruker -- Skyscan CTAn / NRecon / CTVox
- **Software Suite:**
  - NRecon: Reconstruction (projections -> slices)
  - CTAn (CT Analyzer): Quantitative morphometric analysis
  - CTVox: 3D visualization
- **UI Layout (CTAn):** Tab-based interface, main viewport + toolbar + property panel
- **Key UI Features:**
  - Main image display area (2D/3D viewports)
  - Histogram and threshold setting panel
  - Tab interface: Binary selection -> Morphometry -> 3D rendering
  - Ring artifact correction slider, beam hardening correction
  - Results panel with quantitative metrics (BV/TV, Tb.Th, Tb.N, Tb.Sp)
- **Color Scheme:** Traditional Windows style, light/gray mixed
- **Design Strengths:** Deep bone research specialization
- **Design Weaknesses:** Outdated UI (Windows 7 era design)
- **Lessons for HnVue:** Histogram panel design; threshold controls; tab-based analysis workflow

#### 7. North Star Imaging (NSI) -- efX-CT / efX-DR
- **UI Layout:** Integrated pipeline (acquisition -> reconstruction -> analysis)
- **Key UI Features:**
  - Real-time CT reconstruction
  - 3D volume visualization
  - Defect detection and measurement tools
  - Automated workflow recipes
- **Color Scheme:** Dark theme
- **Lessons for HnVue:** Integrated pipeline UI; recipe management

#### 8. VisiConsult -- Xplus / VCxray Software Suite
- **Company:** Founded 1996, 300+ employees, Germany
- **Certifications:** ISO 9001, ISO 14001, ISO/IEC 27001
- **Key UI Features:**
  - Customized UI (tailored to customer requirements)
  - Automatic Defect Recognition (ADR)
  - Real-time image processing
  - Multi-language support
  - AI engineering integration
- **Lessons for HnVue:** Customizable UI; AI integration; multi-language

#### 9. VJ Technologies
- **Domain:** Electronics manufacturing (SMT/BGA/PCB inspection)
- **Specialization:** Automated X-ray Inspection (AXI) workflow optimization
- **Lessons for HnVue:** Automated inspection workflow; pass/fail UI patterns

### Tier 3: Regional/Specialized

#### 10. Shimadzu -- Dynamic Digital Radiography
- **Specialization:** DDR (Dynamic Digital Radiography) -- moving X-ray images
- **Key UI:** Dynamic image acquisition and playback interface
- **Lessons for HnVue:** Timeline/playback controls; dynamic imaging UI

#### 11. Satec (Ametek) -- X-ray Software
- **Specialization:** Materials testing X-ray systems
- **Lessons for HnVue:** Materials testing workflow UI patterns

---

## 1.2 Medical X-Ray Console Software

#### 1. Fujifilm -- FDR Console
- **UI Layout:** Touchscreen-optimized workflow UI
- **Key UI Features:**
  - Large icons, color-coded status indicators
  - One-touch exam protocol selection (body part icons)
  - Workflow: Patient Select -> Exam Select -> Expose -> Image Review
  - Virtual Grid software
  - Image stitching GUI (spine, legs)
  - DICOM Worklist integration
- **Color Scheme:** Dark theme, Fujifilm green/blue accent
- **Design Strengths:** Intuitive touch interface, fast workflow
- **Lessons for HnVue:** Body part icon selector; one-touch workflow; touch-optimized targets

#### 2. Philips -- DigitalDiagnost / Eleva Workspace
- **UI Layout:** Integrated UI platform across all Philips radiography systems
- **Key UI Features:**
  - Large touchscreen monitor console
  - Auto-positioning software control
  - Exposure parameter management (kV, mAs, AEC)
  - Patient worklist integration (DICOM MWL)
  - Image preview and advanced processing
  - Protocol management with pre-programmed anatomical protocols
- **Color Scheme:** Dark theme (eye strain reduction)
- **Design Strengths:** Unified platform across all systems
- **Lessons for HnVue:** Unified UI across detector models; protocol management

#### 3. Siemens Healthineers -- Ysio / MULTIX
- **UI Layout:** Touchscreen interface
- **Key UI Features:**
  - Auto-positioning
  - MAX (Measure Angulate eXpect) measurement tools
  - CARE system (dose optimization)
  - Image post-processing pipeline
- **Color Scheme:** Dark theme, Siemens green accent
- **Lessons for HnVue:** Dose optimization UI; auto-positioning visualization

#### 4. Canon -- CXDI Software
- **UI Layout:** Touchscreen-optimized
- **Key UI Features:**
  - Workflow: Acquisition -> Review -> Transfer
  - Multi-detector support (CXDI-70C Wireless, CXDI-40C)
  - Automatic image processing optimization
- **Lessons for HnVue:** Simple 3-step workflow; detector auto-detection

---

## 1.3 Open-Source/Scientific Imaging Reference Software

| Software | Platform | Key UI Pattern | Relevance |
|----------|----------|----------------|-----------|
| **3D Slicer** | Qt/Python | Dark UI, MPR quad view, module panel | CT/MRI viewer architecture |
| **OHIF Viewer** | React/Web | Modern DICOM web viewer, toolbar | Web-era imaging UI patterns |
| **ImageJ/FIJI** | Java | Toolbar + image canvas + results panel | Scientific imaging standard |
| **QuPath** | JavaFX | Annotation workflow, hierarchical panel | Pathology annotation patterns |
| **Napari** | Python/Qt | Layer system, multi-dimensional viewer | Modern scientific visualization |
| **Horos/Owl Deer** | macOS | Polished DICOM viewer UX | Premium imaging UX benchmark |
| **Dragonfly (ORS)** | Qt | Advanced 3D visualization, polished dark UI | Zeiss-integrated CT analysis |
| **MicroView** | Qt | CT/micro-CT analysis | Direct CT domain relevance |
| **ITK-SNAP** | Qt/C++ | Segmentation workflow, crosshair navigation | ROI segmentation UI |

---

# PART 2: X-RAY IMAGING WORKFLOW & UI IMPLICATIONS

## 2.1 Standard 6-Phase Pipeline

```
Setup -> Positioning -> Acquisition -> Processing -> Analysis -> Reporting
```

### Phase 1: Setup/Configuration

**User Actions:**
- Select tube parameters: kV (20-450kV), mA, exposure time (ms)
- Choose filter (Cu, Al, steel of varying thickness)
- Configure detector (binning 1x1/2x2/4x4, gain, offset)
- Set working distance / geometry (SOD, SID)
- Load recipe/program from saved configurations

**UI Requirements:**
- Parameter panel with sliders, numeric inputs, and dropdowns
- Recipe browser with search and preview
- Preset quick-select buttons for common configurations
- Parameter validation with real-time feedback
- Warning indicators for out-of-range values

### Phase 2: Positioning/Alignment

**User Actions:**
- Place sample on manipulator stage
- Multi-axis positioning (5-7 axes: X, Y, Z, rotation, tilt, detector shift)
- Joystick manual positioning or CNC program execution
- Laser alignment / crosshair centering
- Define reference points

**UI Requirements:**
- Real-time preview window with crosshair overlay
- Axis control panel with position readouts
- Joystick sensitivity control
- Auto-center button
- Position coordinates display (digital readouts)

### Phase 3: Acquisition

**User Actions:**
- Live preview (continuous fluoroscopy at lower dose)
- Single-frame capture (full dose, highest quality)
- Multi-exposure/HDR (multiple images at different kV merged)
- Averaging mode (N frames averaged for noise reduction)
- CT acquisition (500-3000 projections over 360-degree rotation)

**UI Requirements:**
- Large live preview (70%+ of screen)
- Exposure controls (expose button, cancel, emergency stop)
- Mode toggle: Live/Capture/HDR/CT
- Frame averaging selector
- CT progress bar with projection count
- Audible + visual exposure indicator
- Dose accumulation display

### Phase 4: Processing

**User Actions:**
- Flat field correction (FFC)
- Dark current subtraction
- Gain normalization
- Bad pixel interpolation
- Beam hardening correction
- Noise reduction (spatial/temporal filtering)
- Edge enhancement / contrast adjustment

**UI Requirements:**
- Processing pipeline panel with toggle switches
- Real-time before/after comparison
- Histogram display
- Parameter adjustment sliders
- Processing progress indicator
- Auto/Manual mode toggle

### Phase 5: Analysis

**User Actions:**
- Distance, angle, wall thickness measurement
- ROI selection and analysis
- Defect detection (manual/ADR)
- Pass/Fail determination against tolerances
- Comparison with reference images

**UI Requirements:**
- Measurement toolbar (caliper, angle, ROI, density profile)
- Measurement results panel (floating labels + panel)
- Tolerance overlay with color coding (green/red/yellow)
- Reference image comparison (side-by-side, overlay, difference)
- ADR results display with confidence scoring

### Phase 6: Reporting

**User Actions:**
- Annotate images with findings
- Generate inspection report
- Export images (DICOM, TIFF, PNG)
- Archive to database
- Digital signature (FDA 21 CFR Part 11)

**UI Requirements:**
- Annotation toolbar (text, arrow, freeform, stamp)
- Report template selector
- Export format options
- Digital signature dialog
- Print preview
- Batch export capability

---

## 2.2 NDT Standards Impacting UI Design

| Standard | Scope | UI Impact |
|----------|-------|-----------|
| **ASTM E1742** | Radiographic Examination | Inspection workflow, image quality indicators |
| **ASTM E2339/E2699** | DICONDE Standard | File format, metadata display |
| **EN 12681** | Foundry Radiography | Casting-specific defect catalogs |
| **API 1104** | Weld Inspection | Weld-specific measurement tools |
| **NADCAP** | Aerospace NDT | Audit trail, operator certification display |
| **FDA 21 CFR Part 11** | Electronic Records | Digital signatures, audit logs, role-based access |
| **ISO 17640** | Ultrasonic Testing | Multi-method NDT integration |
| **ISO 17636** | Radiographic Testing | Film/digital radiography workflow |
| **IEC 62366** | Medical Device Usability | Safety colors, error prevention, usability testing |
| **IEC 60601-1-6** | Medical Electrical Usability | Alarm systems, safety interlocks |

### Critical UI Requirements from Standards:

1. **Audit Trail (FDA Part 11, NADCAP):** Every action must be logged with timestamp, user ID, and action type. UI must show audit log access and filter by date/user/action.

2. **Digital Signatures (FDA Part 11):** Approve/reject workflow requires electronic signature with meaning (e.g., "Approved by", "Reviewed by"). UI must show signature status clearly.

3. **Role-Based Access (FDA Part 11, IEC 62366):** Different user roles see different UI elements. Login screen must support role selection or auto-detection.

4. **Safety Colors (IEC 62366, IEC 60073):**
   - Green (#00C853): Safe/Normal
   - Amber (#FFD600): Warning
   - Orange (#FF6D00): Blocked/Caution
   - Red (#D50000): Emergency/Alarm

5. **Alarm Management (ISA-18.2):** Four-tier priority system. UI must provide alarm acknowledgment, shelving, and filtering.

---

# PART 3: UI/UX DESIGN BEST PRACTICES FOR IMAGING SOFTWARE

## 3.1 Layout Architecture

### Primary Layout Pattern (Recommended for HnVue)

```
+-------+-------------------------------------------+------------------+
| NAV   |              IMAGE VIEWPORT                |  PROPERTIES      |
| SIDE  |              (70%+ of space)                |  PANEL           |
| BAR   |                                            |                  |
|       |                                            |  - Patient info  |
| Patient+------------------------------------------+  - Detector info |
| List  |         CONTEXT-SPECIFIC RIBBON            |  - W/L settings  |
|       |         (Image | Measure | Annotate)        |  - Measurements  |
| Det.  +------------------------------------------+  - Processing    |
| Status|     IMAGE STRIP / TIMELINE / SLICES        |                  |
|       |     [========o==================]          |  ALARM / LOG     |
|       |     <<  <  Play/Pause  >  >>  Slice: 127   |  PANEL           |
+-------+-------------------------------------------+------------------+
```

### Dockable Panel System

| Panel | Position | Content | Collapsible |
|-------|----------|---------|-------------|
| Navigation Sidebar | Left | Patient list, study tree, detector status | Auto-hide |
| Image Viewport | Center | Main image display (70%+ of usable area) | No |
| Context Ribbon | Top-center | Tool tabs: Home, Image, Measure, Annotate, Inspect | Collapsible |
| Properties Panel | Right | Patient info, detector params, W/L, measurements | Auto-hide |
| Image Strip | Bottom | Recent acquisitions, slice navigator, timeline | Collapsible |
| Alarm/Log Panel | Bottom-right | Events, alerts, audit trail | Auto-hide |

### Multi-Monitor Layout

```
Monitor 1 (Primary):          Monitor 2 (Analysis):
+------------------------+    +------------------------+
| Navigation | VIEWPORT  |    |  Slice Views (2x2)     |
|            | (Live)     |    |  Axial | Sagittal       |
| Workflow   |            |    |  ------+-------         |
| Controls   | RIBBON     |    |  Coronal | 3D           |
|            |            |    |                        |
|            | IMAGE STRIP|    |  Measurement Results   |
+------------------------+    +------------------------+
```

---

## 3.2 Dark Theme Design System

### Why Dark Theme is Mandatory for Imaging Software

1. **Eye Strain Reduction:** Operators work 8-12 hour shifts in dimly lit rooms
2. **Image Contrast:** Dark surround maximizes perceived grayscale dynamic range
3. **Industry Standard:** 100% of major X-ray/CT software uses dark theme as default
4. **Regulatory Alignment:** IEC 62366 recommends low-luminance environments for image evaluation

### Color Token Architecture (HnVue Already Aligned)

```
Level 1: CoreTokens (raw values)
   BackgroundPage: #242424    <- Matches industry standard #1E1E1E~#2D2D30
   BackgroundPanel: #2A2A2A   <- Slightly lighter for depth
   BackgroundCard: #3B3B3B    <- Interactive surfaces

Level 2: SemanticTokens (meaning)
   StatusSafe: #00C853        <- IEC 60073 green
   StatusWarning: #FFD600     <- IEC 60073 amber
   StatusBlocked: #FF6D00     <- ISA-101 orange
   StatusEmergency: #D50000   <- IEC 60073 red

Level 3: ComponentTokens (component-specific)
   ButtonPrimary, InputBorder, etc.
```

### Critical Color Distinction

| Element | Color | Purpose |
|---------|-------|---------|
| **UI Chrome** | `#242424` (dark gray) | Application background |
| **Image Viewport** | `#000000` or `#0A0A0A` (pure black) | Maximizes grayscale dynamic range |
| **Measurement Overlays** | `#00BCD4` (cyan) or `#00FF00` (lime) | High visibility against grayscale |
| **Defect Markers** | Status colors (green/amber/red) | Pass/Marginal/Fail indication |
| **Active Tool** | `#00AEEF` (accent blue) | Current tool highlight |

### Recommended Adjustment

HnVue's `TextPrimary: #FFFFFF` is slightly bright. For extended use:
- **Recommended:** `#E8E8E8` or `#F0F0F0` (softer white, still WCAG AA compliant)
- **Rationale:** Reduces eye strain during long sessions without sacrificing readability

---

## 3.3 Ribbon vs Toolbar Decision

### Analysis for HnVue

| Factor | Ribbon | Toolbar | Verdict |
|--------|--------|---------|---------|
| Number of tools | 30-500+ | < 30 | HnVue has 30+ tools |
| Discoverability | High (tabbed groups) | Low | New users need discoverability |
| Screen space | ~100px vertical | ~40px | Acceptable tradeoff |
| Context sensitivity | Excellent (contextual tabs) | Limited | Image vs Measure contexts |
| Touch friendliness | Moderate | Good | Both work with large targets |
| Medical/Industrial precedent | Siemens, Zeiss, VGStudio | Canon, basic tools | Industry standard |

### Recommendation: Contextual Ribbon

```
+------------------------------------------------------------------+
| Home | Image | Measure | Annotate | Inspect | Tools | Admin      |
+------------------------------------------------------------------+
| [Open] [Save] | [W/L] [Zoom] [Pan] | [Caliper] [Angle] [ROI]   |
| [Undo] [Redo] | [Invert] [Rotate] | [Text] [Arrow] [Freeform]  |
+------------------------------------------------------------------+
```

**Tab Definitions:**
- **Home:** File operations (open, save, export, print), undo/redo, clipboard
- **Image:** W/L, zoom, pan, invert, rotate, flip, image processing pipeline
- **Measure:** Distance (caliper), angle, ROI, wall thickness, density profile
- **Annotate:** Text, arrow, freeform drawing, stamp, defect marker
- **Inspect:** NDT workflow controls, ADR, pass/fail, tolerance overlay
- **Tools:** Detector configuration, dose monitoring, flat field calibration, recipes
- **Admin:** User management, system settings, audit log, reports

---

## 3.4 Image Viewer Interaction Design

### Mouse Gesture Standard (DICOM Viewer Convention)

| Action | Mouse Gesture | Modifier | Visual Feedback |
|--------|--------------|----------|-----------------|
| **Pan** | Middle-click + Drag | or Space + Drag | Cursor changes to hand |
| **Zoom** | Scroll Wheel | (zoom to cursor point) | Zoom percentage in status bar |
| **Window/Level** | Right-click + Drag | Horizontal=W, Vertical=L | W/L values in overlay |
| **Rotate** | Alt + Drag | Horizontal movement | Rotation angle in overlay |
| **Measure** | Click-drag (line) | Click points (polygon) | Measurement label at midpoint |
| **Scroll slices** | Scroll Wheel | (when in scroll mode) | Slice number in status bar |

### Touch Gesture Standard

| Gesture | Action | Implementation Notes |
|---------|--------|---------------------|
| Tap | Select/confirm | Target size >= 48px for gloved hands |
| Long press (500ms) | Context menu | Vibration feedback |
| Pinch/Spread | Zoom | Natural mapping |
| Two-finger drag | Pan | Alternative to one-finger pan |
| Swipe left/right | Previous/next slice | Filmstrip metaphor |
| Two-finger rotate | Rotate image | Optional, may confuse users |

### Keyboard Shortcut Design

| Category | Shortcuts | Notes |
|----------|-----------|-------|
| **View** | F (fit), 1 (100%), +/- (zoom) | Universal imaging conventions |
| **Tools** | V (select), H (hand/pan), Z (zoom), W (W/L), M (measure) | Single-key tool switching |
| **Navigation** | Arrow keys (step slices), Home/End, Page Up/Down | CT slice browsing |
| **Image** | I (invert), R (rotate 90), L (flip horizontal) | Quick adjustments |
| **Safety** | Esc (cancel exposure), Ctrl+Shift+E (emergency stop) | Critical safety shortcuts |
| **File** | Ctrl+O (open), Ctrl+S (save), Ctrl+P (print) | Standard Windows |

---

## 3.5 CT Reconstruction & Slice Viewer UI

### Quad View Layout (Industry Standard)

```
+-------------------+-------------------+
|                   |                   |
|    AXIAL          |    SAGITTAL       |
|    (Top-down)     |    (Side)         |
|                   |                   |
+-------------------+-------------------+
|                   |                   |
|    CORONAL        |    3D VOLUME      |
|    (Front)        |    RENDERING      |
|                   |                   |
+-------------------+-------------------+
```

### Linked Cursor Pattern
- Crosshair in one view shows as lines in perpendicular views
- Clicking in axial view shows corresponding position in sagittal/coronal
- Synchronized zoom and pan across views (optional toggle)

### Slice Navigation Controls

```
[<<] [<] [Play/Pause] [>] [>>]  ----[========o==========]----  Slice: 127/512  Speed: [5fps v]
```

- **Large slider** for precise positioning across hundreds/thousands of slices
- **Mouse scroll wheel** mapped to slice navigation
- **Cine loop** with configurable frame rate (1, 5, 10, 15, 24 fps)
- **Keyboard:** Left/Right (step), Home/End (first/last), Space (play/pause)
- **Mini-preview on hover** at slider position (optional enhancement)

### Volume Rendering Controls
- Transfer function editor (opacity/color mapping)
- Preset rendering modes: MIP (Maximum Intensity Projection), DVR (Direct Volume Rendering), X-Ray
- Clipping planes (axial, sagittal, coronal, oblique)
- Light direction control
- Transparency slider

---

## 3.6 Measurement & Analysis Tools UI

### Measurement Toolbar Layout

```
+--------------------------------------------------+
| [Distance] [Angle] [ROI Rect] [ROI Ellipse]      |
| [Wall Thickness] [Density Profile] [Multi-Point]  |
| [Clear] [Undo] [Snap to Edge]                     |
+--------------------------------------------------+
```

### Measurement Display Patterns

1. **Floating Label (On Image):**
   - Distance: "23.4 mm" at midpoint of measurement line
   - Angle: "45.2 deg" at vertex
   - ROI area: "Area: 125.3 mm2" inside ROI

2. **Results Panel (Right Side):**
   - Table format: # | Type | Value | Tolerance | Status
   - Click-to-focus: clicking a row highlights the measurement on image
   - Export: CSV/PDF measurement report

3. **Tolerance Overlay:**
   - Green: Within tolerance
   - Yellow: Marginal (within 110% of tolerance)
   - Red: Out of tolerance
   - Display as colored band around measurement

### Defect Analysis UI

```
+--------------------------------------------------+
| ADR Mode: [Auto] [Semi-Auto] [Manual]            |
+--------------------------------------------------+
| Defect List:                                      |
|  #1  Porosity    2.3mm   Confidence: 95%  [PASS] |
|  #2  Crack       0.8mm   Confidence: 87%  [FAIL] |
|  #3  Inclusion   1.1mm   Confidence: 72%  [WARN] |
+--------------------------------------------------+
| Overall: 1 FAIL, 1 WARN, 1 PASS    [Reject]      |
+--------------------------------------------------+
```

- Color-coded defect markers on image (red=fail, yellow=warn, green=pass)
- Click defect in list -> zoom to defect on image
- Confidence percentage with visual indicator (progress bar)
- Overall verdict displayed prominently

---

## 3.7 Real-Time Imaging UI Patterns

### Live Preview Window Design

```
+----------------------------------------------------------+
|                    LIVE PREVIEW                            |
|                                                           |
|           +-----------------------------+                 |
|           |                             |                 |
|           |     [X-ray Image]           |                 |
|           |                             |                 |
|           |         +   Crosshair       |                 |
|           |                             |                 |
|           +-----------------------------+                 |
|                                                           |
|  +----------+  +----------+  +------------------+         |
|  |Histogram |  | Profile  |  | Detector Status  |         |
|  | [chart]  |  | [chart]  |  | Temp: 32C        |         |
|  +----------+  +----------+  | FPS: 15          |         |
|                              | Exposure: Ready   |         |
|                              +------------------+         |
+----------------------------------------------------------+
| kV: [120]  mA: [2.5]  ms: [500]  Filter: [Cu 1.0mm]     |
| [LIVE] [CAPTURE] [HDR] [CT]            [EMERGENCY STOP]  |
+----------------------------------------------------------+
```

### Crosshair & Alignment Overlay
- Thin crosshair lines (1px, cyan or white with opacity)
- Center coordinates displayed
- Scale bar for distance reference
- Orientation markers (L/R/A/P for medical, or Cartesian axes for industrial)

### Histogram Display
- Real-time histogram of pixel values
- Window/Level range indicator overlay on histogram
- Click-and-drag on histogram to adjust W/L
- Min/Max/Mean/Stdev statistics

---

## 3.8 File Management & Batch Processing UI

### Image Gallery Pattern

```
+------+ +------+ +------+ +------+ +------+
| [img]| | [img]| | [img]| | [img]| | [img]|
|      | |      | |      | |      | |      |
+------+ +------+ +------+ +------+ +------+
| ID:01 | | ID:02 | | ID:03 | | ID:04 | | ID:05 |
| PASS  | | PASS  | | FAIL  | | PASS  | | WARN  |
+------+ +------+ +------+ +------+ +------+
```

### Recipe/Program Management

```
+--------------------------------------------------+
| Recipes: [Search...] [Filter: All v]  [+ New]    |
+--------------------------------------------------+
| Name            | Created    | Author  | Uses     |
| Chest-Standard  | 2026-03-15 | Admin   | 45      |
| Weld-Fine       | 2026-03-10 | Op1     | 23      |
| PCB-BGA         | 2026-02-28 | Admin   | 67      |
+--------------------------------------------------+
| [Load] [Edit] [Duplicate] [Delete] [Export]      |
+--------------------------------------------------+
```

### Batch Inspection Workflow

```
+--------------------------------------------------+
| Batch: B-2026-0407-001   Status: In Progress     |
| Progress: [============>-------] 67% (20/30)     |
+--------------------------------------------------+
| Part # | Status  | Defects | Time   | Result     |
| 001    | Done    | 0       | 12.3s  | PASS       |
| 002    | Done    | 1       | 14.1s  | FAIL       |
| 003    | Done    | 0       | 11.8s  | PASS       |
| 004    | Active  | -       | 8.2s   | SCANNING   |
| 005    | Queued  | -       | -      | WAITING    |
+--------------------------------------------------+
| [Pause] [Skip] [Abort]              [Report]     |
+--------------------------------------------------+
```

---

# PART 4: WPF IMPLEMENTATION GUIDANCE

## 4.1 Framework & Library Recommendations

### Current Stack Assessment

HnVue uses: **MahApps.Metro** + Custom Design System (CoreTokens/SemanticTokens/ComponentTokens)

### Recommended Additions

| Library | Purpose | Priority | Why |
|---------|---------|----------|-----|
| **AvalonDock** (Xceed) | Dockable panel system | **Critical** | VS-style docking for imaging software; enables flexible multi-panel layouts |
| **Fluent.Ribbon** | Contextual ribbon UI | **High** | Tabbed command organization for 30+ tools |
| **HandyControl** | Selective controls | **Medium** | NumericUpDown, PropertyGrid, CheckComboBox |
| **LiveCharts2** | Real-time charts | **Already using** | Histograms, dose charts, profiles |
| **Helix Toolkit** | 3D visualization | **Future** | CT volume rendering, 3D reconstruction |

### NOT Recommended
- Full framework replacement (Material Design, MUI): Conflicts with IEC 62366 safety-color design system
- Telerik/DevExpress/Infragistics: Commercial cost, feature overlap with custom design system

## 4.2 Dockable Panel Implementation

### AvalonDock Layout Strategy

```xml
<!-- Conceptual layout -->
<dock:DockingManager>
  <!-- Left: Navigation (auto-hide capable) -->
  <dock:LayoutAnchorablePaneGroup DockWidth="250">
    <dock:LayoutAnchorable ContentId="PatientList" />
    <dock:LayoutAnchorable ContentId="DetectorStatus" />
  </dock:LayoutAnchorablePaneGroup>

  <!-- Center: Image Viewport (main document) -->
  <dock:LayoutDocumentPane>
    <dock:LayoutDocument ContentId="ImageViewer" />
  </dock:LayoutDocumentPane>

  <!-- Right: Properties (auto-hide capable) -->
  <dock:LayoutAnchorablePaneGroup DockWidth="300">
    <dock:LayoutAnchorable ContentId="Properties" />
    <dock:LayoutAnchorable ContentId="Measurements" />
  </dock:LayoutAnchorablePaneGroup>

  <!-- Bottom: Timeline + Alarms -->
  <dock:LayoutAnchorablePaneGroup DockHeight="200">
    <dock:LayoutAnchorable ContentId="ImageStrip" />
    <dock:LayoutAnchorable ContentId="AlarmLog" />
  </dock:LayoutAnchorablePaneGroup>
</dock:DockingManager>
```

### Layout Serialization
- Save/restore workspace layouts per user role
- XML serialization of panel positions and sizes
- Default layouts: Radiographer, Radiologist, Service Engineer, Administrator

## 4.3 Typography Best Practices

### Recommended Font Stack

| Element | Font | Size | Weight | Rationale |
|---------|------|------|--------|-----------|
| UI text | Segoe UI | 13px | 400 | Windows native, ClearType optimized |
| Headers | Segoe UI | 20px | 600 | Section titles |
| Measurements | JetBrains Mono | 14px | 400 | Digit alignment, readability |
| Status values | Consolas | 12px | 400 | Numeric readouts |
| Korean text | Malgun Gothic | 13px | 400 | Korean fallback |
| Body part icons | Segoe UI Emoji | 32px | - | Anatomical icons |

### Why Monospace for Measurements
- Proportional fonts cause misaligned decimal points
- "11.23" and "99.99" appear different widths in proportional fonts
- Monospace ensures column alignment in measurement tables
- JetBrains Mono has distinctive digit shapes (no 0/O confusion)

---

# PART 4.5: WPF IMPLEMENTATION PATTERNS (DEEP DIVE)

## 4.5.1 AvalonDock MVVM Integration

### Recommended Package

**Dirkster99/AvalonDock** (v4.72+) -- Ms-PL license, .NET 8 compatible, VS2013 dark/light themes

```
NuGet: Dirkster.AvalonDock + Dirkster.AvalonDock.Themes.VS2013
```

### MVVM Pattern with CommunityToolkit.Mvvm

```xml
<!-- MainWindow.xaml -->
<xcad:DockingManager
    DocumentsSource="{Binding Documents}"
    AnchorablesSource="{Binding Tools}"
    ActiveContent="{Binding ActiveContent, Mode=TwoWay}">

    <xcad:DockingManager.LayoutItemTemplateSelector>
        <local:PanelsTemplateSelector>
            <local:PanelsTemplateSelector.ImageViewerTemplate>
                <DataTemplate DataType="{x:Type vm:ImageViewerViewModel}">
                    <views:ImageViewerView />
                </DataTemplate>
            </local:PanelsTemplateSelector.ImageViewerTemplate>
            <local:PanelsTemplateSelector.WorklistTemplate>
                <DataTemplate DataType="{x:Type vm:WorklistViewModel}">
                    <views:WorklistView />
                </DataTemplate>
            </local:PanelsTemplateSelector.WorklistTemplate>
        </local:PanelsTemplateSelector>
    </xcad:DockingManager.LayoutItemTemplateSelector>
</xcad:DockingManager>
```

### Role-Based Layout Serialization

```csharp
public class LayoutManager
{
    private readonly string _layoutRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HnVue", "layouts");

    public void SaveLayout(DockingManager manager, UserRole role)
    {
        var serializer = new XmlLayoutSerializer(manager);
        var path = Path.Combine(_layoutRoot, $"{role}.layout.xml");
        Directory.CreateDirectory(_layoutRoot);
        using var writer = new StreamWriter(path);
        serializer.Serialize(writer);
    }

    public void LoadLayout(DockingManager manager, UserRole role,
        IEnumerable<DockableViewModel> viewModels)
    {
        var path = Path.Combine(_layoutRoot, $"{role}.layout.xml");
        if (!File.Exists(path)) return;

        var serializer = new XmlLayoutSerializer(manager);
        serializer.LayoutSerializationCallback += (s, e) =>
        {
            e.Content = viewModels.FirstOrDefault(vm => vm.ContentId == e.Model.ContentId);
        };
        serializer.Deserialize(path);
    }
}
```

### Default Layouts per Role

| Role | Left Panel | Center | Right Panel | Bottom |
|------|-----------|--------|-------------|--------|
| **Radiographer** | Patient Worklist | Live Preview + Acquisition Controls | Exposure Params | Image Strip |
| **Radiologist** | Study List | Image Viewer (full) | Measurement Results | Report Preview |
| **Service Engineer** | System Status | Detector Calibration | Diagnostic Info | System Log |
| **Administrator** | User Management | Audit Log | System Config | Statistics |

## 4.5.2 X-Ray Image Rendering Pipeline

### Current State Analysis

HnVue uses `BitmapSource.Create` with `PixelFormats.Gray8`, which requires CPU resampling for Window/Level. The `ImageProcessor` normalizes all 16-bit data to 8-bit.

### Recommended 3-Phase Upgrade

**Phase 1: WriteableBitmap (Immediate)**

Replace `BitmapSource.Create` with `WriteableBitmap` for pixel-level updates without full reconstruction:

```csharp
private WriteableBitmap? _writeableBitmap;

private WriteableBitmap? BuildWriteableBitmap(ProcessedImage image)
{
    if (_writeableBitmap == null
        || _writeableBitmap.PixelWidth != image.Width
        || _writeableBitmap.PixelHeight != image.Height)
    {
        _writeableBitmap = new WriteableBitmap(
            image.Width, image.Height, 96, 96, PixelFormats.Gray8, null);
    }

    _writeableBitmap.Lock();
    try
    {
        unsafe
        {
            var ptr = (byte*)_writeableBitmap.BackBuffer;
            int stride = _writeableBitmap.BackBufferStride;
            for (int y = 0; y < image.Height; y++)
            {
                var row = ptr + y * stride;
                Marshal.Copy(image.PixelData, y * image.Width, row, image.Width);
            }
        }
        _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, image.Width, image.Height));
    }
    finally
    {
        _writeableBitmap.Unlock();
    }
    return _writeableBitmap;
}
```

**Phase 2: GPU ShaderEffect for Window/Level**

Encode 16-bit values in two 8-bit channels (R=high, G=low), apply Window/Level via pixel shader:

```hlsl
// WindowLevelEffect.ps (HLSL Pixel Shader)
sampler2D input : register(s0);
float WindowCenter : register(C0);
float WindowWidth  : register(C1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 tex = tex2D(input, uv);
    float val = tex.r * 255.0 + tex.g * 255.0 * 256.0;
    float normalized = saturate((val - (WindowCenter - WindowWidth * 0.5)) / WindowWidth);
    return float4(normalized, normalized, normalized, 1.0);
}
```

**Phase 3: D3DImage for Live Preview (Future)**

For real-time detector feed, use `D3DImage` for Direct3D texture interop enabling true GPU pipeline.

### Image Overlay Architecture

```xml
<!-- ImageViewerView.xaml -- Overlay layer on top of X-ray image -->
<Grid>
    <!-- Base X-ray image -->
    <Image Source="{Binding ImageSource}" Stretch="Uniform"
           RenderOptions.BitmapScalingMode="Fant"
           RenderOptions.EdgeMode="Aliased"
           RenderOptions.CachingHint="Cache">
        <Image.RenderTransform>
            <ScaleTransform ScaleX="{Binding ZoomFactor}" ScaleY="{Binding ZoomFactor}"/>
        </Image.RenderTransform>
    </Image>

    <!-- Measurement/Annotation overlay canvas -->
    <Canvas x:Name="OverlayCanvas" ClipToBounds="True" IsHitTestVisible="True">
        <Line X1="{Binding CrosshairH.X1}" Y1="{Binding CrosshairH.Y1}"
              X2="{Binding CrosshairH.X2}" Y2="{Binding CrosshairH.Y2}"
              Stroke="#40FF0000" StrokeThickness="1" StrokeDashArray="4,2"/>
        <Rectangle Canvas.Left="{Binding RoiRect.X}" Canvas.Top="{Binding RoiRect.Y}"
                   Width="{Binding RoiRect.Width}" Height="{Binding RoiRect.Height}"
                   Stroke="Yellow" StrokeThickness="1" StrokeDashArray="2,2"/>
    </Canvas>

    <!-- Ink annotation overlay (stylus support) -->
    <InkCanvas x:Name="InkOverlay" Visibility="Collapsed"
               Background="Transparent"
               DefaultDrawingAttributes="{Binding AnnotationAttributes}"/>
</Grid>
```

## 4.5.3 Fluent.Ribbon Integration

### Package

```
NuGet: Fluent.Ribbon (MIT license, .NET 8+)
Dark Themes: Dark.Steel, Dark.Emerald, Dark.Obsidian, Dark.Cobalt
```

### Contextual Tab Groups for Imaging

```xml
<Fluent:Ribbon>
    <!-- Home Tab (always visible) -->
    <Fluent:RibbonTabItem Header="Home">
        <Fluent:RibbonGroupBox Header="Patient">
            <Fluent:Button Header="Search" Command="{Binding SearchPatientCommand}"/>
            <Fluent:Button Header="Register" Command="{Binding RegisterPatientCommand}"/>
        </Fluent:RibbonGroupBox>
    </Fluent:RibbonTabItem>

    <!-- Image Tools Tab (contextual: shown when image loaded) -->
    <Fluent:RibbonTabItem Header="Image Tools"
                          Visibility="{Binding IsImageLoaded, Converter={StaticResource BoolToVisibilityConverter}}">
        <Fluent:RibbonGroupBox Header="Window/Level">
            <Fluent:Button Header="Reset W/L" KeyTip="RW" Command="{Binding ResetWindowCommand}"/>
            <Fluent:Button Header="Auto W/L" KeyTip="AW" Command="{Binding AutoWindowCommand}"/>
        </Fluent:RibbonGroupBox>
        <Fluent:RibbonGroupBox Header="Zoom">
            <Fluent:Button Header="Zoom In" KeyTip="ZI" Command="{Binding ZoomInCommand}"/>
            <Fluent:Button Header="Zoom Out" KeyTip="ZO" Command="{Binding ZoomOutCommand}"/>
            <Fluent:Button Header="Fit" KeyTip="F" Command="{Binding FitToWindowCommand}"/>
        </Fluent:RibbonGroupBox>
    </Fluent:RibbonTabItem>

    <!-- Measure Tab (contextual) -->
    <Fluent:RibbonTabItem Header="Measure"
                          Visibility="{Binding IsImageLoaded, Converter={StaticResource BoolToVisibilityConverter}}">
        <Fluent:RibbonGroupBox Header="Tools">
            <Fluent:ToggleButton Header="Distance" KeyTip="MD" Command="{Binding DistanceToolCommand}"/>
            <Fluent:ToggleButton Header="Angle" KeyTip="MA" Command="{Binding AngleToolCommand}"/>
            <Fluent:ToggleButton Header="ROI" KeyTip="MR" Command="{Binding RoiToolCommand}"/>
        </Fluent:RibbonGroupBox>
    </Fluent:RibbonTabItem>
</Fluent:Ribbon>
```

### KeyTip Support (Alt+key navigation)

Essential for medical environments where keyboard-only operation is required. Press `Alt` to activate KeyTips, then press displayed keys for fast tool access.

## 4.5.4 Performance Optimization for Large Images

### Image Buffer Pooling

```csharp
public sealed class ImageBufferPool
{
    private readonly ConcurrentBag<byte[]> _pool = new();
    private readonly int _bufferSize;

    public ImageBufferPool(int width, int height) => _bufferSize = width * height;

    public byte[] Rent() => _pool.TryTake(out var buffer) ? buffer : new byte[_bufferSize];

    public void Return(byte[] buffer)
    {
        if (buffer.Length == _bufferSize) _pool.Add(buffer);
    }
}
```

For 4316x4316 DR images (~18MB per buffer), pooling reduces GC pressure significantly.

### Live Preview Frame Rate Control

```csharp
private readonly DispatcherTimer _frameTimer;
private byte[]? _pendingFrame;

public LivePreviewViewModel()
{
    _frameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) }; // ~30fps
    _frameTimer.Tick += OnFrameTick;
}

private void OnFrameTick(object? sender, EventArgs e)
{
    if (_pendingFrame == null) return;
    var frame = Interlocked.Exchange(ref _pendingFrame, null);
    UpdateDisplay(frame, _frameWidth, _frameHeight);
}
```

## 4.5.5 Touch Gesture Implementation

```csharp
// Zoom/Pan via Manipulation events
ImageControl.IsManipulationEnabled = true;
ImageControl.ManipulationStarting += (s, e) =>
{
    e.ManipulationContainer = this;
    e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
};
ImageControl.ManipulationDelta += (s, e) =>
{
    var matrix = _transform.Matrix;
    var delta = e.DeltaManipulation;
    matrix.ScaleAt(delta.Scale.X, delta.Scale.Y,
                   e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
    matrix.Translate(delta.Translation.X, delta.Translation.Y);
    _transform.Matrix = matrix;
};
```

### Stylus/Ink for Annotation

WPF `InkCanvas` with stylus pressure sensitivity for natural annotation. Stylus presence automatically switches from touch (pan/zoom) to ink (annotation) mode.

---

# PART 4.6: KOREAN/ASIAN X-RAY MARKET & FPD-SPECIFIC PATTERNS

## 4.6.1 Korean FPD Manufacturers

### Vieworks
- **Products:** Vivix FPD series (Vivix 1012N, 1417, 1717, etc.)
- **Technology:** Own TFT backplane + scintillator technology
- **Software:** Console software with multi-language support (KO/EN/JA/ZH)
- **UI Characteristics:** Clean modern interface, Korean-primary
- **Website:** vieworks.com

### Rayence (Vatech Group)
- **Products:** WCE series FPDs, EzRay Air portable
- **Technology:** Own CsI scintillator technology (Vatech subsidiary)
- **UI Characteristics:** Medical-focused, dental+medical imaging
- **Website:** rayence.com

### DRTECH
- **Positioning:** Competitive pricing, growing market share
- **Products:** Digital radiography FPDs
- **UI Characteristics:** Chinese/English primary interface

### Key Insight for HnVue
Korean FPD vendors typically provide C/C++ SDK DLLs. HnVue's P/Invoke integration pattern is correct. Korean language support is essential for domestic market.

## 4.6.2 CsI+FPD Specific UI Requirements

### FPD Calibration Workflow UI

```
Step 1: Offset/Dark Calibration
  - Cover detector (no X-ray)
  - Acquire dark frames (typical 16-64 frames)
  - Calculate offset map
  - UI: Progress bar, frame count, temperature display

Step 2: Gain/Flat Field Calibration
  - Uniform X-ray exposure (no object)
  - Acquire flat frames at working kV
  - Calculate gain correction map
  - UI: kV selection, exposure count, uniformity indicator

Step 3: Bad Pixel Mapping
  - Identify dead/hot pixels
  - Generate defect map
  - Bad pixel count display
  - UI: Defect map overlay toggle, pixel count threshold

Step 4: Uniformity Verification
  - Test flat field uniformity
  - Calculate Signal-to-Noise Ratio (SNR)
  - Display uniformity map
  - UI: Heat map visualization, SNR readout, pass/fail
```

### CsI:TI Scintillator Specific Considerations

1. **Afterglow Correction:** CsI:Tl has visible afterglow (phosphorescence). UI should provide:
   - Afterglow compensation toggle
   - Frame delay setting for continuous acquisition
   - Visual indicator when afterglow correction is active

2. **Binning Mode Selection:**
   - 1x1 (full resolution, e.g., 4316x4316)
   - 2x2 (half resolution, 4x faster readout)
   - 4x4 (quarter resolution, fastest)
   - UI: Radio buttons with resolution/readout speed tradeoff display

3. **MTF/SNR Quality Indicators:**
   - Display MTF (Modulation Transfer Function) value after calibration
   - SNR measurement result
   - DQE (Detective Quantum Efficiency) if available

### Additional Vendors Discovered

#### Durr NDT -- D-Tect X / instaNDT
- **Software:** D-Tect X (inspection), instaNDT (PACS), DRIVE (management)
- **DICONDE compliance:** ASTM E2339/E2699 standard
- **Key clients:** Airbus, Shell, Rolls-Royce
- **UI Lessons:** DICONDE file management, NDT-specific PACS

---

# PART 5: ACCESSIBILITY & USABILITY

## 5.1 Color Blindness Considerations

### Statistics
- ~8% of males have color vision deficiency (deuteranopia/protanopia most common)
- Primary user demographic (industrial operators, radiographers) is predominantly male

### Design Rules

1. **Never rely on color alone** for status indication:
   - Green circle + checkmark = PASS
   - Yellow triangle + exclamation = WARNING
   - Red octagon + X = FAIL/EMERGENCY

2. **Use distinguishable color pairs:**
   - Blue (#00AEEF) vs Orange (#FF6D00): Distinguishable by all color vision types
   - Avoid: Red vs Green without additional shape/text differentiation

3. **Test with CIEP simulation tools:**
   - Color Oracle (free, cross-platform)
   - Built-in Windows accessibility checker
   - Chrome DevTools color vision emulation

### WCAG 2.1 Compliance

| Element | Minimum Contrast Ratio | HnVue Status |
|---------|----------------------|--------------|
| Normal text (< 18px) | 4.5:1 | #E8E8E8 on #242424 = 12.3:1 (PASS) |
| Large text (>= 18px) | 3:1 | #FFFFFF on #2A2A2A = 14.2:1 (PASS) |
| UI components | 3:1 | Status colors all pass against dark bg |
| Focus indicators | 3:1 | #00AEEF border on #242424 = 3.5:1 (PASS) |

## 5.2 Touch-Friendly Design

### Minimum Touch Targets

| Element | Size | Rationale |
|---------|------|-----------|
| Buttons | 44x44px | Microsoft guidelines |
| Icon buttons | 48x48px | Gloved hand operation |
| Toggle switches | 44x24px | Easy to tap |
| Slider thumbs | 28x28px | Precise but tappable |
| List items | 48px height | Easy to select |
| Emergency stop | 64x64px minimum | Large, always visible, red |

### Touch Mode Considerations
- Increase spacing between interactive elements (minimum 8px gap)
- Disable right-click context menus (use long-press instead)
- Provide on-screen zoom/pan controls (not just mouse gestures)
- Support both mouse and touch simultaneously (hybrid mode)

## 5.3 Error Prevention & Safety UI

### Critical Safety Patterns

1. **Emergency Stop Button:**
   - Always visible (never hidden behind tabs or panels)
   - Minimum 64x64px, red background, white text
   - Requires deliberate action (not accidental touch)
   - Visual feedback when activated (flashing, full-screen overlay)

2. **Exposure Confirmation:**
   - Two-step confirmation for high-dose procedures
   - Clear display of estimated dose before exposure
   - Countdown timer with audible beep
   - Abort capability during countdown

3. **Dose Monitoring:**
   - Cumulative dose display always visible
   - Warning thresholds with progressive urgency
   - Four-tier system: ALLOW -> WARN -> BLOCK -> EMERGENCY

4. **Mode-Error Prevention:**
   - Current mode always displayed (LIVE/CAPTURE/CT/REVIEW)
   - Mode transitions require confirmation for safety-critical changes
   - Visual distinction between live and captured images (border color)

---

# PART 6: DESIGN TRENDS 2024-2026

## 6.1 Key Trends for X-Ray Console Software

### 1. Dark Mode First (Universal Adoption)
- 100% of major X-ray/CT software now defaults to dark theme
- Dual theme support (dark/light) required for accessibility
- Image viewport always pure black regardless of UI theme

### 2. Workflow-Based UI (Replacing Traditional Menus)
- Step-by-step wizard for beginners
- Full control panel for experts
- Progressive disclosure: show only relevant options per workflow step
- Yxlon Geminy and Waygate lead this trend

### 3. AI-Enhanced Interfaces
- Automatic Defect Recognition (ADR) with confidence scoring
- AI-suggested parameters based on sample type
- Predictive maintenance alerts
- Smart histogram analysis

### 4. Cloud Connectivity
- Remote inspection collaboration
- Cloud-based ADR processing
- Centralized recipe management
- Waygate InspectionWorks leads this trend

### 5. Touch-First Design
- Medical consoles already touch-optimized (FDR, Philips Eleva)
- Industrial systems transitioning
- Gloved-hand operation support
- Minimum 48px touch targets

### 6. Modular Dockable Panels
- User-configurable workspace layouts
- VS-style docking (AvalonDock)
- Layout serialization per user/role
- Volume Graphics VGStudio MAX leads this pattern

### 7. Real-Time 3D Visualization
- CT data real-time volume rendering is now standard
- Quad view (Axial/Sagittal/Coronal/3D) with linked cursors
- GPU-accelerated rendering (OpenGL/Vulkan)

---

# PART 7: RECOMMENDED DESIGN DIRECTION FOR HNVUE

## 7.1 Current State Assessment

| Aspect | Current | Industry Standard | Gap |
|--------|---------|-------------------|-----|
| Dark theme | CoreTokens defined | Standard practice | Aligned |
| Safety colors | IEC 62366 compliant | IEC 60073 | Aligned |
| Typography | Segoe UI | Segoe UI + Monospace | Minor (add monospace) |
| Layout | Fixed 3-column | Dockable panels | **Gap: Need AvalonDock** |
| Toolbar | Simple toolbar | Contextual ribbon | **Gap: Need ribbon** |
| Touch support | Not implemented | Standard | **Gap: Need touch targets** |
| CT viewer | Not implemented | Quad view standard | **Gap: Future phase** |
| Measurement tools | Stub | Full toolbar | **Gap: Critical** |

## 7.2 Priority Implementation Roadmap

### Phase 1: Foundation (Immediate)
1. Add AvalonDock for dockable panel layout
2. Implement contextual ribbon (Fluent.Ribbon or custom)
3. Add JetBrains Mono / Consolas for measurement readouts
4. Adjust TextPrimary from #FFFFFF to #E8E8E8 for comfort
5. Define image viewport background as pure black (#0A0A0A) separate from UI chrome

### Phase 2: Image Viewer (Critical)
1. Implement Window/Level with mouse drag
2. Zoom to cursor with scroll wheel
3. Pan with middle-click/space+drag
4. Measurement tools (distance, angle, ROI)
5. Annotation tools (text, arrow, freeform)
6. Keyboard shortcuts (V/H/Z/W/M standard keys)

### Phase 3: Workflow Enhancement
1. Step indicator for acquisition workflow
2. Recipe/program management panel
3. Live preview with histogram
4. Crosshair overlay with coordinates
5. Exposure parameter panel with validation

### Phase 4: Advanced Features (Future)
1. CT quad view with linked cursors
2. 3D volume rendering
3. ADR integration
4. Batch inspection workflow
5. Cloud connectivity

---

# APPENDIX A: COMPETITIVE SOFTWARE COMPARISON MATRIX

| Software | Vendor | Domain | Dark Theme | Touch | Ribbon | Docking | CT | ADR | Cloud |
|----------|--------|--------|-----------|-------|--------|---------|-----|-----|-------|
| VGStudio MAX | Volume Graphics | Industrial | Yes | No | Yes | Yes | Yes | Yes | No |
| phoenix daten|x | Waygate/GE | Industrial | Yes | Partial | No | Partial | Yes | Yes | Yes |
| Inspect-X | Nikon | Industrial | Yes | No | No | Yes | Yes | Yes | No |
| Scout-and-Scan | Zeiss | Industrial | Yes | Partial | No | Yes | Yes | Partial | No |
| Geminy | Yxlon | Industrial | Yes | Partial | No | No | Yes | Yes | No |
| CTAn | Bruker | Scientific | No | No | No | No | Yes | No | No |
| efX-CT | NSI | Industrial | Yes | No | No | Partial | Yes | Yes | No |
| Xplus | VisiConsult | Industrial | Yes | Partial | No | Partial | Partial | Yes | No |
| D-Tect X | Durr NDT | Industrial | Yes | No | No | Partial | Partial | Yes | Partial |
| FDR Console | Fujifilm | Medical | Yes | Yes | No | No | No | No | Yes |
| Eleva | Philips | Medical | Yes | Yes | No | No | No | No | Yes |
| CXDI | Canon | Medical | Yes | Yes | No | No | No | No | Partial |
| Ysio/MULTIX | Siemens | Medical | Yes | Yes | No | No | No | No | Partial |
| Vivix Console | Vieworks | Medical | Yes | Partial | No | No | No | No | No |
| EzRay Console | Rayence | Medical | Yes | Partial | No | No | No | No | No |
| **HnVue (Target)** | **ABYZ** | **Medical** | **Yes** | **Plan** | **Plan** | **Plan** | **Future** | **Future** | **Future** |

# APPENDIX B: DESIGN TOKEN REFERENCE

### HnVue Core Tokens (Current -- Industry Aligned)

```yaml
Background:
  Page: "#242424"     # Industry: #1E1E1E~#2D2D30  -> ALIGNED
  Panel: "#2A2A2A"    # Slightly lighter for depth  -> ALIGNED
  Card: "#3B3B3B"     # Interactive surfaces         -> ALIGNED
  Viewport: "#0A0A0A" # Pure black for image canvas  -> NEEDS ADDITION

Text:
  Primary: "#FFFFFF"   # Industry recommends #E8E8E8  -> ADJUST RECOMMENDED
  Secondary: "#B0BEC5" # Muted blue-gray              -> ALIGNED
  Disabled: "#546E7A"  # Low contrast disabled         -> ALIGNED

Status (IEC 62366):
  Safe: "#00C853"      # IEC 60073 green               -> ALIGNED
  Warning: "#FFD600"   # IEC 60073 amber               -> ALIGNED
  Blocked: "#FF6D00"   # ISA-101 orange                -> ALIGNED
  Emergency: "#D50000" # IEC 60073 red                 -> ALIGNED

Interactive:
  Primary: "#1B4F8A"   # Medical blue                  -> ALIGNED
  Accent: "#00AEEF"    # Bright blue for focus          -> ALIGNED
  Hover: "#2E6DB4"     # Lighter blue                   -> ALIGNED

Typography:
  Font: "Segoe UI"     # Windows native                -> ALIGNED
  Mono: "JetBrains Mono" or "Consolas"                   -> NEEDS ADDITION
```

# APPENDIX C: KEYBOARD SHORTCUT REFERENCE

### Imaging Standard Shortcuts (Universal Convention)

```
Tool Switching (Single Key):
  V  - Select/Pointer tool
  H  - Hand/Pan tool
  Z  - Zoom tool
  W  - Window/Level tool
  M  - Measure tool
  A  - Annotate tool

View:
  F  - Fit to window
  1  - 100% (actual pixels)
  2  - 200%
  +  - Zoom in
  -  - Zoom out
  I  - Invert image
  R  - Rotate 90 degrees clockwise
  L  - Flip horizontal

Navigation (CT/Slices):
  Left/Right  - Previous/Next slice
  Home/End    - First/Last slice
  Page Up/Dn  - Jump 10 slices
  Space       - Play/Pause cine loop

File:
  Ctrl+O  - Open image/study
  Ctrl+S  - Save
  Ctrl+P  - Print
  Ctrl+Z  - Undo
  Ctrl+Y  - Redo
  Ctrl+E  - Export

Safety:
  Esc            - Cancel current operation
  Ctrl+Shift+E   - Emergency stop (hard stop)
```

---

# APPENDIX D: REFERENCE SOFTWARE FOR UI INSPIRATION

### Best-in-Class by Category

| Category | Software | Why Study It |
|----------|----------|-------------|
| **Overall Layout** | VGStudio MAX | Best dockable panel + ribbon + multi-viewport |
| **Workflow UI** | Yxlon Geminy | Best wizard + expert dual-mode design |
| **Touch Interface** | Fujifilm FDR | Best touchscreen medical console |
| **CT Viewer** | 3D Slicer | Best open-source CT quad view + linked cursors |
| **Measurement Tools** | ImageJ/FIJI | Most comprehensive measurement toolkit |
| **Dark Theme** | ORS Dragonfly | Most polished dark UI in scientific software |
| **Cloud Platform** | Waygate InspectionWorks | Best cloud-connected NDT platform |
| **ADR Integration** | Waygate X\|approver | Best automatic defect recognition UI |
| **Industrial HMI** | Siemens WinCC | Best industrial process control UI patterns |
| **DICOM Viewer** | OHIF Viewer | Best modern web-based DICOM viewer UX |
| **Annotation** | QuPath | Best pathology annotation workflow |
| **3D Visualization** | Napari | Best modern 3D scientific visualization |
| **Ribbon UI** | Zeiss Calypso | Best metrology software ribbon implementation |

---

*Document generated through deep research of 30+ X-ray imaging software products, NDT standards, medical device regulations, and UI/UX best practices. All recommendations align with IEC 62366, IEC 60073, ISA-101, and WCAG 2.1 standards.*

*Version: 2.0.0 | Date: 2026-04-07 | Classification: UI Design Reference*
*Includes: WPF implementation patterns, Korean market analysis, CsI+FPD specific requirements, AvalonDock/Fluent.Ribbon integration guides*
