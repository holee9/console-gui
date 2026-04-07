# X-Ray Inspection & Imaging Workflow Patterns -- Research Report

Comprehensive research on X-ray inspection and imaging workflow patterns and their UI implications, covering workflow stages, NDT standards, real-time imaging, CT reconstruction, defect analysis, measurement tools, and file management.

---

## 1. X-Ray Inspection Workflow Stages

### Standard 6-Phase Pipeline

Based on the product architectures of Waygate Technologies (Phoenix line), Volume Graphics (VGSTUDIO MAX), VisiConsult (vestaXpro), and Nordson DAGE (ezVision), the canonical X-ray inspection workflow follows these phases:

### Phase 1: Setup/Configuration

- Tube parameter selection: kV (typically 20-450kV), mA, exposure time (ms)
- Filter selection (Cu, Al, steel filters of varying thickness)
- Detector configuration (binning mode 1x1/2x2/4x4, gain, offset)
- Working distance / geometry setup (SOD = Source-to-Object Distance, SID = Source-to-Image Distance)
- Recipe/program loading from saved configurations
- Magnification calculation based on geometry

### Phase 2: Positioning/Alignment

- Sample placement on manipulator stage
- Multi-axis positioning (typically 5-7 axes: X, Y, Z, rotation, tilt, detector shift)
- Joystick-based manual positioning or CNC program execution
- Laser alignment or crosshair centering
- Reference point definition for repeatable inspections
- Auto-center routines

### Phase 3: Acquisition

- Live preview mode (continuous fluoroscopy at lower dose)
- Single-frame capture (full dose, highest quality)
- Multi-exposure/HDR acquisition (multiple images at different kV/exposure merged for high dynamic range)
- Averaging mode (N frames averaged for noise reduction, typical N=4,8,16,32)
- CT acquisition (series of projections over 360-degree rotation, typical 500-3000 projections)
- Snapshot vs continuous mode toggle

### Phase 4: Processing

- Flat field correction (FFC): `(raw - dark) / (flat - dark) * mean(flat - dark)`
- Dark current subtraction
- Gain normalization
- Bad pixel interpolation (from detector defect map)
- Beam hardening correction
- Noise reduction (spatial/temporal filtering, median, Gaussian)
- Edge enhancement / contrast adjustment (unsharp mask, histogram equalization)
- Geometric distortion correction (barrel/pincushion)

### Phase 5: Analysis

- Measurement (distance, angle, wall thickness, density/profile)
- Defect detection (manual, semi-automated, fully automated ADR)
- Comparison against reference/acceptance criteria
- Pass/Fail determination
- Statistical analysis

### Phase 6: Reporting

- Image annotation (arrows, text, measurement overlays)
- Automatic report generation (PDF, CSV, XML)
- DICONDE-compliant data storage
- Export to MES (Manufacturing Execution System)/SPC (Statistical Process Control) systems
- Digital signature for approval

---

## 2. NDT (Non-Destructive Testing) Software Standards & Compliance

### Key Standards Affecting UI Design

| Standard | Scope | UI Implications |
|----------|-------|-----------------|
| ASTM E1742 | Radiographic Examination | IQI visibility tools, density measurement, technique qualification display |
| ASTM E2033 | Computed Radiography | Imaging plate handling, scanner calibration UI, image quality metrics |
| ASTM E2339 (DICONDE (Digital Imaging Communication for Non-Destructive Evaluation)) | Digital Imaging Communication | DICONDE file I/O, metadata management, PACS-like workflow, interoperability |
| ASTM E2699 | DICONDE Long-Term Archiving | Archive management UI, data integrity verification, retention policies |
| ASTM E2934 | DICONDE Radiographic Testing | RT-specific metadata fields, image quality validation |
| EN 12681 | Foundry Radiography | Casting defect classification (per ASTM (American Society for Testing and Materials) E155/E446), acceptance level selection |
| API 1104 | Weld Inspection | Weld-specific workflow: joint ID, defect code selection (crack, porosity, slag, incomplete penetration) |
| NADCAP | Aerospace Accreditation | Traceability: operator certification display, calibration status, audit trail, equipment ID |
| EN 12681 | Foundry Radiography | Casting defect classification (per ASTM E155/E446), acceptance level selection |
| API 1104 | Weld Inspection | Weld-specific workflow: joint ID, defect code selection (crack, porosity, slag, incomplete penetration) |
| NADCAP | Aerospace Accreditation | Traceability: operator certification display, calibration status, audit trail, equipment ID |
| FDA 21 CFR Part 11 | Electronic Records/Signatures | User authentication, electronic signatures, audit trail, role-based access, record integrity |

### ASTM E1742 -- Radiographic Examination (UI Implications)

- IQI (Image Quality Indicator) visibility verification tools
- Image density measurement (pixel value to optical density conversion)
- Technique card display (exposure parameters, source-to-film distance)
- penetrameter sensitivity verification
- Exposure technique qualification records
- Image density measurement (pixel value to optical density conversion)
- Technique card display (exposure parameters, source-to-film distance)
- penetrameter sensitivity verification
- Exposure technique qualification records

### EN 12681 -- Foundry Radiography (Workflow Patterns)

- Casting defect classification per severity classes (Class A through D)
- Reference radiograph comparison (ASTM E155 for aluminum, E446 for steel castings)
- Defect type catalog: gas porosity, sand inclusions, shrinkage, hot tears, cracks
- Acceptance level selection per customer specification
- Zone-based evaluation (critical zones vs non-critical zones on the casting)

### API 1104 -- Weld Inspection (Workflow Patterns)

- Weld joint identification and tracking
- Defect code selection from standard code list
- Indication length and spacing measurement
- Acceptance criteria lookup per weld type and service class
- Repair tracking workflow (detect, mark, repair, re-inspect)

### FDA (Food and Drug Administration) 21 CFR (Code of Federal Regulations) Part 11 -- Electronic Records/Signatures (UI Requirements)

- **Audit Trail**: Time-stamped, computer-generated log of all create/modify/delete actions on electronic records. Must be visible to users and not editable.
- **Electronic Signatures**: Two-component authentication (user ID + password), signature manifestation (name, date/time, meaning such as "reviewed by" or "approved by"), linked to specific records.
- **Access Controls**: Role-based UI (User Interface) where operators, reviewers, and administrators see different functionality.
- **Record Integrity**: Original images must be stored unaltered; any processing creates a new version with metadata tracking.
- **Validation**: System must be validated (IQ/OQ/PQ protocols).

### NADCAP-Specific UI Patterns

- Operator certification status indicator (Level I/II/III with expiry date)
- Equipment calibration expiry warnings (visual alert when approaching due date)
- Procedure reference display (governing work instruction / technique card number)
- Traceability fields: part number, batch, work order, inspector ID, date/time
- Non-conformance report (NCR) linking

---

## 3. Real-Time Imaging UI (User Interface) Patterns

### Live Preview Window Design

**Layout**: Large central viewport with dark background (typically #1a1a1a to #000000). The viewport occupies 60-70% of screen real estate.

**Overlays** (rendered on top of the live image, semi-transparent):
- Crosshair (center + extended lines to edges, typically green or cyan, ~1px width)
- Scale bar (adaptive to zoom level, showing mm or inch units)
- Orientation labels (based on detector/source position)
- ROI rectangles/ellipses (color-coded, numbered)
- Measurement lines (caliper-style with distance readout)
- Detector edges / active area boundary

**Status Bar** (below or surrounding viewport):
- Current kV, mA, exposure time (ms)
- Frame rate (fps) for live mode
- Detector temperature
- Accumulated dose / tube hours
- Zoom level and pixel size at current magnification (e.g., "0.025 mm/pixel")
- Image dimensions (width x height in pixels)

### Histogram Display Patterns

- Live histogram (256-bin or 1024-bin) showing pixel intensity distribution
- Linear/Logarithmic scale toggle
- Window/Level adjustment handles (two draggable markers on histogram)
- Auto-window button (automatic contrast optimization)
- ROI-specific histogram mode (histogram of selected region only)
- Color-coded: gray for full image, colored (red/green/blue) for ROI regions
- Clipping indicators (overexposed = red markers, underexposed = blue markers)

### ROI (Region of Interest) Selection UI (User Interface)

- Rectangle, ellipse, polygon, and freehand ROI tools
- Draggable resize handles at corners/edges (8 handles for rectangle)
- Multiple simultaneous ROIs with different colors (ROI-1=red, ROI-2=green, etc.)
- ROI statistics panel: mean, min, max, std deviation, area (mm2), pixel count
- Named/labeled ROIs for reference in reports
- ROI copy/paste between images
- ROI from measurement tool (convert caliper endpoints to ROI region)

### Snapshot vs Continuous Mode Toggle

- Toggle button (prominent, in toolbar): "Single Frame" / "Continuous"
- Continuous mode: live fluoroscopy at reduced dose (lower mA)
- Single frame: full-dose high-quality capture
- Averaging mode: capture N frames and average (spinner control for N)
- HDR mode: capture sequence at different exposures, auto-merge
- Keyboard shortcut: Space bar for snapshot, Enter for continuous toggle

### Exposure Parameter Adjustment During Live View

- Real-time sliders for kV, mA, exposure time
- Immediate visual feedback (image updates in <100ms)
- Parameter preset buttons (Low kV / High kV / Penetrating)
- Auto-exposure button (system optimizes parameters for current sample)
- Fine adjustment with mouse wheel on slider
- Numeric input field alongside slider for precise entry
- Safety interlocks: kV/mA limits based on tube rating chart

---

## 4. CT (Computed Tomography) Reconstruction UI (User Interface)

### Slice Viewer (MPR (Multi-Planar Reconstruction) -- Multi-Planar Reconstruction)

**Classic 2x2 Quad Layout**:
```
+------------------+------------------+
|   Axial (XY)     |  Sagittal (YZ)  |
|   Z-slice index  |  X-slice index   |
+------------------+------------------+
|   Coronal (XZ)   |  3D Volume      |
|   Y-slice index  |  Rendering      |
+------------------+------------------+
```

**Slice Navigation**:
- Vertical slider per viewport for slice index (0 to N)
- Mouse wheel scrolling through slices
- Keyboard arrows (up/down for slice, left/right for fine step)
- Click in one view repositions crosshair in other two views (linked cursors)
- Slice number display (e.g., "Slice 127 / 512")
- Jump to slice: direct numeric input
- Cine mode (auto-scroll animation, configurable speed)

**Window/Level Controls**:
- Preset W/L values (Bone, Soft Tissue, Metal, Custom)
- Right-click drag: Window (horizontal) / Level (vertical)
- Direct numeric entry for Window Width and Window Center
- Auto W/L button

### 3D Volume Rendering Controls

- Transfer function editor: opacity and color curves as function of voxel value
- Preset rendering modes: MIP (Maximum Intensity Projection), MinIP (Minimum Intensity Projection), Composite, Iso-surface
- Clipping planes (6-DOF adjustable: position + normal direction)
- Rotation: trackball or arcball interaction (left-click drag)
- Zoom: scroll wheel or pinch gesture
- Pan: right-click drag or middle-click drag
- Volume clipping box: adjustable bounds on all 3 axes
- Opacity slider for overall volume transparency

### Reconstruction Progress UI

- Progress bar with percentage (0-100%)
- Estimated time remaining (dynamic calculation)
- Current projection number / total (e.g., "Projection 523 / 1500")
- Current stage indicator:
  1. Loading projections
  2. Pre-processing (dark/flat correction)
  3. Filtering (Ram-Lak, Shepp-Logan)
  4. Back-projection (FBP or iterative)
  5. Post-processing (ring artifact removal, beam hardening)
  6. Volume assembly complete
- Cancel button
- Preview mode: low-resolution preview during reconstruction (every Nth slice)
- Log/output panel showing reconstruction parameters and warnings

### ROI and Segmentation Tools in CT

- Threshold-based segmentation (slider for min/max gray value)
- Region growing (click seed point, expand to connected similar voxels)
- Manual painting/erasing on individual slices (brush tool with adjustable radius)
- 3D bounding box crop
- Morphological operations (dilate, erode, open, close)
- Segmentation result: colored overlay on slices, 3D surface mesh in volume view
- Export segmentation as STL or OBJ mesh

### Reference: VGSTUDIO MAX Module Structure (Volume Graphics/Hexagon)

From volumegraphics.com product page:

| Module | Purpose |
|--------|---------|
| Basic Edition | Visualization, measurement instruments, reporting, presentation |
| CT Reconstruction Module | Reconstruct 3D volume from CT projections |
| Coordinate Measurement Module | GD&T measurements (PTB/NIST certified algorithms) |
| CAD Import Module | Import native CAD files (CATIA V5, Creo) |
| CAD Import with PMI | Import Product and Manufacturing Information |
| Nominal/Actual Comparison | Compare manufactured parts to CAD/mesh/voxel data |
| Wall Thickness Analysis | Localize insufficient/excessive wall thickness or gap width |
| Manufacturing Geometry Correction | Correct tools for injection molding, casting, 3D printing |
| Reverse Engineering | Convert CT scans to usable CAD models |
| Porosity/Inclusion Analysis | Non-destructive defect discovery, pore cut prediction |
| Extended Porosity/Inclusion | Defect analysis per specifications P201 and P202 |
| Fiber Composite Material Analysis | Fiber orientation and parameters in composites |
| Foam/Powder Analysis | Cell structures in porous foams and filters |
| Digital Volume Correlation | Displacement quantification between initial/deformed volumes |
| Volume Meshing | Convert CT data to simulation meshes |
| Structural Mechanics Simulation | Stress simulation directly on CT data |
| Transport Phenomena Simulation | Fluid/electrical/thermal flow and diffusion simulation |

---

## 5. Defect Analysis UI (User Interface) Patterns

### Automatic Defect Recognition (ADR (Automatic Defect Recognition)) UI (User Interface)

**Reference**: Waygate Technologies X|approver; VisiConsult AI-based ADR; VGinLINE (Volume Graphics)

**ADR (Automatic Defect Recognition) Workflow UI (User Interface)**:

**Reference**: Waygate Technologies X|approver; VisiConsult AI-based ADR; VGinLINE (Volume Graphics)

**ADR Workflow UI**:
1. **Configuration**: Select inspection type, load trained model or rule set
2. **Processing**: Automatic scan of image with progress indicator
3. **Results Display**: Defects highlighted with bounding boxes/contours overlaid on image
4. **Review**: Operator reviews each detection, confirms or rejects
5. **Reporting**: Auto-generated pass/fail report

**ADR Result Display Patterns**:
- Color-coded severity: Red (critical), Yellow (warning), Green (acceptable)
- Confidence percentage per detection (e.g., "Crack: 94% confidence")
- Defect classification labels (crack, porosity, void, inclusion, lack of fusion, etc.)
- Defect list panel with sortable columns: type, size, location, severity, confidence
- Click-to-inspect: clicking a defect in the list centers it in the viewport with zoom
- Bounding box toggle (show/hide all detections)
- Heat map overlay mode (defect density visualization)

### Manual Annotation Tools

- Arrow/pointer annotations (selectable head/tail styles)
- Freehand drawing (pencil tool with adjustable width)
- Text labels with configurable font/size/color
- Circle/rectangle/ellipse markup
- Measurement annotations (distance, angle auto-displayed on annotation)
- Highlighter tool (semi-transparent color overlay)
- Stamp tool (predefined symbols: crack, void, inclusion)
- Annotation layer separate from image data (non-destructive, toggleable)
- Undo/redo for annotations

### Pass/Fail Indication Patterns

- Prominent, unmissable indicator (large colored bar at top or side of viewport)
- PASS: Green background with checkmark icon
- FAIL: Red background with X icon, optional audible alert
- Conditional: Yellow/amber for marginal results (within tolerance but borderline)
- Result summary text: "N defects found, M critical, K acceptable per criteria"
- Large, bold font for factory floor visibility
- Status LED simulation (green/red circle)

### Statistical Analysis Dashboards

- Defect count by type (bar chart)
- Defect size distribution (histogram)
- Trend chart: defect rate over time / batch number
- Pareto chart of defect types
- Yield rate (pass/total as percentage)
- Cp/Cpk process capability indices
- SPC control charts (X-bar, R chart)
- Filterable by date range, part number, operator, batch

### Reference Image Comparison

- **Side-by-side**: Two viewports showing current and reference simultaneously, linked zoom/pan
- **Overlay**: Semi-transparent blend of reference over current (opacity slider 0-100%)
- **Difference**: Subtraction image highlighting deviations (current - reference), auto-scaled
- **Flicker/blink**: Rapid alternating display between current and reference (toggle speed)
- **Registration/alignment tools**: Manual or auto alignment for overlay mode
- Tolerance envelope: show acceptable deviation range as colored band around reference

---

## 6. Measurement Tools UI

### Caliper / Distance Measurement

- Click two points on image, line drawn between them with endpoints marked
- Distance displayed as floating label near the midpoint of the line
- Units: mm, inch, pixels (selectable, with conversion factor from pixel size)
- Sub-pixel accuracy via edge detection algorithms (Canny, Sobel)
- Snap-to-edge option (caliper snaps to nearest high-contrast edge)
- Perpendicular distance (shortest distance from point to a line)
- Multi-segment polyline measurement (chain of connected distances)

### Angle Measurement

- Three-point angle tool (vertex + two ray endpoints)
- Angle value displayed as floating label at vertex
- Arc visualization showing the measured angle sector
- Units: degrees or radians (selectable)
- Supplementary angle display option

### Wall Thickness Measurement

- **Automatic**: Click on a surface point, system finds nearest opposite surface using ray-tracing
- **Profile line**: Draw a line across the wall, intensity profile shows both edges, auto-measure distance between edges
- **Color-mapped wall thickness**: entire part colored by thickness (gradient from thin=red to thick=blue/green), legend bar
- Minimum/maximum/average thickness readout in panel
- Tolerance overlay: green within spec, red outside spec, yellow borderline
- Wall thickness along a path (draw a curve, get thickness profile along it)

### Density / Profile Measurement

- Line profile tool: draw a line, see gray-value/intensity plot in a separate panel below the image
- ROI statistics: mean, min, max, std deviation, median of pixel values within ROI
- Density calibration: map gray values to material density (g/cm3) using calibration standards
- Point measurement: click to get exact pixel coordinates and gray value
- Cross-section profile: extract intensity values along arbitrary path

### Tolerance Overlay Patterns

- Nominal dimension with +/- tolerance displayed (e.g., "2.50 +/- 0.10 mm")
- Color coding: green (within tolerance), red (out of tolerance), yellow (warning zone)
- Deviation value shown (e.g., "+0.05mm" in green, "-0.12mm" in red)
- Tolerance zones visualized as shaded regions on the measurement graphic
- GD&T symbols display (flatness, cylindricity, position, etc.)

### Measurement Result Display

- Floating labels attached to measurement geometry (follow on zoom/pan)
- Measurement results panel (dockable sidebar): tabular list of all measurements
- Export measurements to CSV (Comma-Separated Values)/Excel
- Pin measurements to image for report generation
- Measurement grouping (group related measurements by feature)
- Statistical summary of all measurements (min, max, mean, std dev)

---

## 7. File Management and Batch Processing

### Image Gallery and Browser

- Thumbnail grid view of all acquired images (adjustable thumbnail size)
- List view with metadata columns (date, part ID, kV, mA, pass/fail, operator)
- Preview pane (single-click shows preview, double-click opens full viewer)
- Sort by date, part number, result, operator, file size
- Filter by date range, result (pass/fail), part type, defect type
- Drag-and-drop to folders/collections

### Batch Inspection Workflow

1. Load batch program/recipe (or auto-select via barcode scan)
2. Scan barcode/QR for part identification (auto-selects recipe)
3. System acquires image automatically (pre-programmed parameters)
4. ADR processes image (automatic defect detection)
5. Pass/Fail result displayed immediately (large, prominent indicator)
6. Operator reviews flagged items (if any)
7. Confirm and advance to next part
8. Batch summary at end: yield rate, defect distribution, SPC data, report generation

### Recipe/Program Management

- Recipe browser: saved programs organized by part number / customer / inspection type
- Recipe editor: define ROIs, acceptance criteria, kV/mA settings, measurement points, ADR rules
- Recipe version control with approval workflow (draft -> reviewed -> approved -> released)
- Import/export recipes (XML/JSON format for sharing between systems)
- Recipe parameters include: X-ray settings, manipulator positions, processing steps, ADR rules, measurement definitions, acceptance criteria
- Recipe simulation/preview (show expected image quality without actual exposure)

### Export and Archiving

- DICONDE-compliant export (ASTM E2339 format for NDT interoperability)
- Image export: TIFF (Tagged Image File Format) (lossless, 16-bit), JPEG (Joint Photographic Experts Group) (lossy), PNG (Portable Network Graphics) (lossless, 8-bit), BMP (Bitmap), RAW (Raw Image Data)
- Report export: PDF (Portable Document Format) (formatted report), CSV (Comma-Separated Values) (tabular data), XML (eXtensible Markup Language) (structured data)
- Batch export with naming conventions (configurable templates with variables: partnumber, date, sequence)
- Archive to network storage / PACS (Picture Archiving and Communication System) system
- Archive verification (checksum validation, MD5 (Message Digest Algorithm 5)/SHA-256 (Secure Hash Algorithm 256-bit))
- Retention policy management (auto-delete after N years per policy)

### Search and Filter Capabilities

- Full-text search across metadata fields
- Filter by: date range, part number, batch ID, operator, result (pass/fail), defect type, severity
- Advanced query builder (AND/OR conditions on multiple fields)
- Saved search profiles (reusable filter combinations)
- Tag-based organization (user-defined tags on images/batches)
- Recently accessed files quick-access list

---

## 8. Key Software Platform Reference

### Major Vendors and Their Software Products

| Vendor | Software Products | Primary Focus |
|--------|-------------------|---------------|
| **Volume Graphics** (Hexagon) | VGSTUDIO MAX, VGinLINE, myVGL | CT data analysis, metrology, defect detection, simulation |
| **Waygate Technologies** (Baker Hughes) | Phoenix Datos|x, X|act, Rhythm, X|approver, InspectionWorks, PlanarCT, FLASH! | CT acquisition/reconstruction, 2D X-ray, DR workflow, ADR, cloud platform |
| **VisiConsult** | vestaXpro | Digital radiography NDT workflow, flat panel integration, AI-based ADR |
| **Nordson DAGE** | ezVision | Electronics X-ray inspection, BGA/solder ADR |
| **Comet Yxlon** | FF CT/DR software | Industrial CT and DR acquisition and analysis |
| **Omron** | VT-X series software | Inline AXI for electronics SMT production |
| **Saki Corporation** | 3D X-ray inspection software | Inline AXI, electronics manufacturing |
| **Durr NDT** | HD-CR, Xplus | Computed radiography, DICONDE workflow |
| **Carestream NDT** | Image Suite | Digital radiography, measurement, annotation |
| **ZEISS** | ZEISS METROTOM software | Industrial CT, dimensional metrology |

### Waygate Technologies NDT Software Suite (from waygate-tech.com)

- **InspectionWorks**: Cloud-based secure, scalable NDT software platform with real-time collaboration
- **FLASH!**: Intelligent image processing technology for real-time enhancement
- **Phoenix Datos|x**: Industrial CT acquisition and reconstruction software
- **Phoenix X|act**: 2D X-ray inspection software
- **Rhythm**: Radiography software suite for CR/DR workflow
- **PlanarCT**: CT reconstruction for planar (laminography) scanning
- **X|approver**: ADR (Automated Defect Recognition) for X-ray and CT

### VGSTUDIO MAX Architecture (from volumegraphics.com)

- Modular architecture: Base edition + add-on modules
- Handles CT (Computed Tomography) data and other 3D formats (point cloud, mesh, CAD (Computer-Aided Design))
- PTB (Physikalisch-Technische Bundesanstalt)/NIST (National Institute of Standards and Technology) certified metrology algorithms
- Subvoxel accuracy surface determination
- Automation via scripting/macro capabilities
- Free viewer (myVGL) for sharing results
- Supports .vgl format for 3D data exchange

---

## 9. CsI+FPD System-Specific Considerations

For a CsI (Cesium Iodide scintillator) + FPD (Flat Panel Detector) based system:

### Detector Calibration UI (User Interface)

- **Dark field acquisition**: Capture image with X-ray off (records detector offset/noise)
- **Flat field acquisition**: Capture image with X-ray on, no sample (records detector gain variation and beam profile)
- **Bad pixel map**: Identify and map dead/hot pixels for interpolation
- Calibration schedule: frequency settings (every N hours, on startup, manual trigger)
- Calibration quality indicator: SNR (Signal-to-Noise Ratio), uniformity metrics after calibration

### Image Quality Parameters

- **MTF** (Modulation Transfer Function): Display spatial resolution capability
- **SNR** (Signal-to-Noise Ratio): Per-image quality metric
- **CNTR** (Contrast-to-Noise Ratio): For defect detectability assessment
- **Spatial resolution**: lp/mm (line pairs per millimeter) display
- **Dynamic range**: Detector bit depth utilization (e.g., "14-bit, using 85% of range")

### CsI-Specific Processing

- Lag correction (CsI scintillator afterglow compensation)
- Veiling glare correction (light scatter within CsI layer)
- Scintillator uniformity correction
- DQE (Detective Quantum Efficiency) optimization

---

## Sources

- [Volume Graphics - VGSTUDIO MAX](https://www.volumegraphics.com/en/products/vgstudio-max.html)
- [Waygate Technologies - NDT Software](https://www.waygate-tech.com/products/industrial-radiography-and-ct/ndt-software)
- [Waygate Technologies - Industrial X-ray CT Systems](https://www.waygate-tech.com/industrial-x-ray-ct-systems)
- [VisiConsult - X-ray Systems & Solutions](https://www.visiconsult.de/en/products/software/)
- [Comet Technologies](https://www.comet.tech)
- Domain knowledge from industry standards: ASTM E1742, E2033, E2339, E2699, EN 12681, API 1104, NADCAP, FDA 21 CFR Part 11
