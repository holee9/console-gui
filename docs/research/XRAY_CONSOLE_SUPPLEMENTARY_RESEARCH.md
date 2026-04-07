# X-Ray Console GUI Supplementary Deep Research

**Supplementary research to fill gaps in the primary reference document (XRAY_CONSOLE_GUI_UI_DESIGN_REFERENCE.md)**

**Target Project:** HnVue (CsI+FPD X-ray/CT Console Application, WPF/.NET 8)
**Research Date:** 2026-04-07
**Classification:** Supplementary Research (Deep Web Analysis)

---

# PART 1: X-RAY CONSOLE GUI SCREENSHOTS AND DEMOS

## 1.1 Comet Yxlon (formerly YXLON International)

### Software: Geminy UI / Software Suite
- **Website:** https://www.yxlon.com (part of Comet Group)
- **UI Style:** Modern dark-themed ribbon + dockable panels + multi-viewport
- **Key observations from web research:**
  - Award-winning "Geminy" user interface with dual-mode: wizard for beginners, expert for advanced users
  - Software Suite includes Vista X Pro for advanced image processing
  - Multi-viewport layout with 2D X-ray view, 3D CT reconstruction, and analysis panels
  - Dragonfly 3D World integration for advanced data analytics (by ORS/Thermo Fisher)
  - System product lines: Cheetah EVO, Cougar EVO, FF series CT, UX series, CA20 for semiconductors
  - Website shows video demos with dark UI, real-time image preview, and CT slice viewers

### UI Patterns Discovered:
| Pattern | Description |
|---------|-------------|
| Wizard Mode | Step-by-step guided workflow for novice operators |
| Expert Mode | Full access to all controls and parameters for experienced users |
| Product Comparison | Built-in product comparison feature (stored in session cookies) |
| Multi-language | Supports EN, DE, JA, ZH (verified from website structure) |

## 1.2 Durr NDT -- D-Tect X Software

### Software: D-Tect X
- **Website:** https://www.duerr-ndt.com
- **UI Style:** Clean NDT-focused interface with DICONDE compliance
- **Key observations from web research:**
  - Product page shows X-ray inspection software for industrial NDT
  - Complementary products: instaNDT (PACS for X-ray image archiving), DRIVE (NDT management software)
  - DICONDE-compliant image format (ASTM standard for NDT digital imaging)
  - Flat panel detector product line: D-DR series (7, 1024, 1025B/1043B, 2329, 2430, 3643)
  - D-DR 3643 NDT: wireless portable FPD with high-resolution capabilities
  - Bendable detectors: D-DR 1025B/1043B NDT for curved surface inspection
  - Computed radiography: HD-CR 35 NDT scanner system
  - Major references: BP, Shell, Thales Alenia Space, Airbus, Rolls-Royce, TUV Sud

### Design Recommendations from Durr NDT:
- DICONDE compliance is essential for NDT market credibility
- PACS integration (instaNDT) is a separate product -- consider modular architecture
- NDT management software (DRIVE) handles order management, test reports, staff administration
- Modern web UI approach for PACS (instaNDT described as "modern web UI")

## 1.3 Other Industrial X-Ray Software (from research)

### VisiConsult -- Vcxray Control Software
- Web-based UI with dashboard elements
- Defect detection overlays with automated inspection workflows
- Used in security/EOD applications

### Waygate Technologies (formerly GE Inspection) -- phoenix datos / Rhythm
- Console UI with acquisition parameters, image processing filters, defect analysis results
- Rhythm software for radiography review with measurement and annotation tools
- InspectionWorks cloud platform for fleet-wide analytics

### Nikon Metrology -- Inspect-X
- Console-style interface with real-time X-ray imaging and CT reconstruction controls
- Multi-panel layout with 2D/3D viewports
- Measurement tools integrated directly in the viewer

### North Star Imaging (NSI) -- efX-CT
- Feature-rich console with scan queue management, reconstruction preview, analysis tools

### Carestream NDT -- HPX-1
- Touchscreen-friendly console interface
- Advanced image processing

---

# PART 2: WPF DOCKING PANEL IMPLEMENTATIONS

## 2.1 AvalonDock (Extended WPF Toolkit by Xceed)

### Key Facts:
- **NuGet:** `Extended.Wpf.Toolkit` (free, open-source, MS-PL license)
- **GitHub:** https://github.com/xceedsoftware/ExtendedWPFToolkit (Note: repo structure may have changed)
- **MVVM Support:** Built-in support via `LayoutItemViewModel` base class
- **Features:**
  - `DockingManager` as root control with `DocumentsSource` and `AnchorablesSource`
  - `LayoutItemTemplateSelector` for different view templates per panel type
  - `XmlLayoutSerializer` for save/restore layout persistence
  - Document windows (tabbed, like VS code editor tabs) vs Anchorable windows (tool panels)
  - Support for floating windows, auto-hide, pinned/unpinned states

### MVVM Pattern with AvalonDock:
```
Architecture:
  MainViewModel
    -> Documents (ObservableCollection<IDocument>) -- image viewer tabs
    -> Tools (ObservableCollection<ITool>) -- property panels, histogram, etc.

  DockingManager
    -> DocumentsSource bound to Documents
    -> AnchorablesSource bound to Tools
    -> LayoutItemTemplateSelector for View resolution
```

### Design Recommendations:
- Use document windows for image viewer instances (multiple open images)
- Use anchorable windows for tool panels (properties, histogram, measurements, log)
- Implement `XmlLayoutSerializer` to persist user workspace layouts
- Each panel should have its own ViewModel implementing a common interface (ITool/IDocument)

### Scientific Application Patterns:
- Property panel (left): Image parameters, acquisition settings, detector info
- Output panel (bottom): Status messages, processing log, error output
- Data Explorer panel (right): File browser, study list, image series navigator
- Main area: Image viewer(s) in tabbed/MDI layout

## 2.2 Alternative Docking Libraries

| Library | License | Notes |
|---------|---------|-------|
| AvalonDock (Xceed) | MS-PL (free) | Most popular free option, well-documented |
| Actipro Docking & MDI | Commercial | Excellent MVVM, used in scientific apps |
| Syncfusion Docking Manager | Commercial | Rich feature set, good support |
| Infragistics XamDockManager | Commercial | Enterprise-grade |
| MahApps.Metro | Open source | Metro-style, pairs well with Fluent.Ribbon |

---

# PART 3: DICOM VIEWER UI PATTERNS FOR WPF

## 3.1 fo-dicom (Fellow Oak DICOM)

### Key Facts (verified from GitHub):
- **GitHub:** https://github.com/fo-dicom/fo-dicom
- **License:** Microsoft Public License (MS-PL)
- **Targets:** .NET Standard 2.0, .NET 8, .NET 9, .NET Framework 4.6.2
- **DICOM Dictionary Version:** 2025d (latest)
- **NuGet Packages:**
  - `fo-dicom` -- Core parser, services, tools
  - `fo-dicom.Imaging.Desktop` -- System.Drawing rendering (Bitmap)
  - `fo-dicom.Imaging.ImageSharp` -- Platform-independent rendering via ImageSharp
  - `fo-dicom.Imaging.SkiaSharp` -- Platform-independent rendering via SkiaSharp
  - `fo-dicom.Codecs` -- Cross-platform DICOM codecs (JPEG, JPEG-LS, JPEG2000, HTJPEG2000, RLE)
  - `fo-dicom.Instrumentation` -- OpenTelemetry instrumentation

### Key Features:
- High-performance fully asynchronous async/await API
- JPEG (including lossless), JPEG-LS, JPEG2000, HTJPEG2000, and RLE compression
- Supports very large datasets with content loading on demand
- JSON and XML export/import
- Anonymization support
- Full DICOM services (C-STORE, C-FIND, C-MOVE, C-ECHO, N-ACTION)
- DI container support for customization

### WPF Integration Pattern:
```
1. Install fo-dicom + fo-dicom.Imaging.Desktop NuGet packages
2. Configure via DicomSetupBuilder with DI
3. Load DICOM file: DicomFile.Open("file.dcm")
4. Render image: new DicomImage("file.dcm").RenderImage().As<Bitmap>()
5. Convert Bitmap to BitmapSource for WPF Image control
6. Implement Window/Level adjustment using DICOM windowing tags
```

### Design Recommendation for HnVue:
- Use fo-dicom for DICOM export capability (even if primary format is proprietary)
- fo-dicom supports .NET 8 natively -- no compatibility issues
- For WPF rendering, use `fo-dicom.Imaging.Desktop` and convert to `BitmapSource`
- Consider `fo-dicom.Codecs` for JPEG2000 compression of X-ray images

## 3.2 OHIF Viewer (Web-Based Reference)

### Key Facts:
- **GitHub:** https://github.com/OHIF/Viewers
- **Architecture:** React.js + cornerstone.js
- **UI Components:** Modular @ohif/ui package
- **Key UI Features:**
  - Toolbar: Window/level, zoom, pan, scroll, annotation tools, layout selectors
  - Study Browser: Thumbnail-based study/series navigation
  - Measurement Panel: Tracked measurements and annotations
  - Side Panels: Customizable panels for AI results, reports
  - DICOM Tag Inspector: View metadata for loaded images
  - Hanging Protocols: Configurable display rules

### Patterns Applicable to WPF:
- Thumbnail-based series navigation (carousel or grid sidebar)
- Toolbar with tool groups (image manipulation, measurement, annotation)
- Measurement panel showing all active measurements in a table
- Side panel architecture with pluggable content areas
- Hanging protocol concept: pre-configured viewport layouts for different study types

## 3.3 ClearCanvas (Enterprise Open Source)
- **GitHub:** https://github.com/ClearCanvas/ClearCanvas
- Enterprise-grade DICOM/PACS viewer built with C# and WPF
- Full PACS server, viewer, and workstation
- Good reference for WPF medical imaging UI architecture

---

# PART 4: INDUSTRIAL SOFTWARE RIBBON UI

## 4.1 Fluent.Ribbon

### Key Facts (verified from GitHub):
- **GitHub:** https://github.com/fluentribbon/Fluent.Ribbon
- **License:** MIT License
- **Description:** Office-like Ribbon UI for WPF
- **Controls:** RibbonTabControl, Backstage, Gallery, QuickAccessToolbar, ScreenTip
- **Requires:** .NET SDK 10.0.100 or later for development
- **NuGet:** `Fluent.Ribbon`

### Dark Theme Support:
- Built-in dark themes: Dark.Steel, Dark.Emerald, Dark.Obsidian, Dark.Cobalt, and more
- Apply via XAML ResourceDictionary:
  ```xml
  <ResourceDictionary Source="pack://application:,,,/Fluent;component/Themes/Themes/Dark.Steel.xaml" />
  ```
- Or programmatically:
  ```csharp
  Fluent.ThemeManager.Current.ChangeTheme(this, "Dark.Steel");
  ```

### Industrial Application Patterns:
- **Ribbon Tabs:** File (Backstage), Acquisition, View, Image Processing, Measurement, Tools, Help
- **Ribbon Groups per Tab:**
  - Acquisition: Exposure (kV, mA, time), Detector, Sequence, Capture
  - View: Zoom, Pan, Window/Level, Layout, Overlay
  - Image Processing: Filters, Enhancement, Noise Reduction, Edge Detection
  - Measurement: Caliper, Angle, ROI, Profile Line, Distance
- **Backstage (File menu):** New Study, Open, Save, Export, Print, Settings, About
- **Quick Access Toolbar:** Frequently used actions (capture, save, undo/redo)
- **Gallery Controls:** For preset selection (window presets, filter presets)

### Design Recommendation:
- Use Fluent.Ribbon as the primary command UI framework
- Apply Dark.Steel theme as the base, customize with project-specific accent colors
- Combine with MahApps.Metro for additional controls (dialogs, toggles, etc.)
- Pair with AvalonDock for the docking panel system

## 4.2 WPF Dark Theme for Industrial/SCADA

### Color Palette Recommendations:
```
Background Primary:   #1E1E1E (VS Code dark)
Background Secondary: #2D2D2D (panel backgrounds)
Background Tertiary:  #383838 (tool windows)
Text Primary:         #E0E0E0
Text Secondary:       #A0A0A0
Accent Primary:       #007ACC (blue, for X-ray/medical)
Accent Warning:       #FFCC00 (yellow)
Accent Error:         #FF4444 (red)
Accent Success:       #44BB44 (green)
```

### Why Dark Theme for X-Ray Console:
- Reduces eye strain during long inspection sessions
- X-ray images (typically grayscale) have better contrast against dark backgrounds
- Industry standard across all major X-ray console software
- Reduces monitor glare in darkened inspection rooms
- Matches medical imaging workstation conventions

---

# PART 5: MEASUREMENT TOOL UI IMPLEMENTATION

## 5.1 WPF Adorner-Based Approach

### Architecture:
```
Image control (showing WriteableBitmap or BitmapSource)
  -> AdornerLayer
    -> MeasurementAdorner (custom Adorner)
      -> Caliper lines (endpoints + crossbars)
      -> Distance text label
      -> Drag handles for adjustment
      -> Scale calibration info
```

### Implementation Pattern:
1. Display X-ray image in `<Image>` control bound to BitmapSource
2. Create custom `Adorner` that renders measurement graphics in `OnRender(DrawingContext)`
3. Handle mouse events on the adorner for interactive placement and dragging
4. Maintain pixel-to-physical-unit calibration (pixels/mm based on detector pixel pitch)
5. Store measurements as data objects bound to a measurement table panel

### Tool Types Needed for X-Ray Console:
| Tool | Description | WPF Implementation |
|------|-------------|-------------------|
| Caliper/Distance | Line between two points with distance readout | Adorner line + text |
| Angle | Three-point angle measurement | Adorner arc + lines + text |
| ROI Rectangle | Region of interest for statistics | Adorner rectangle + stats overlay |
| ROI Ellipse | Elliptical region for avg/std measurement | Adorner ellipse + stats |
| Profile Line | Intensity profile along a line | Adorner line + separate chart |
| Freehand Draw | Annotation freehand drawing | InkCanvas overlay |
| Text Annotation | Label with text on image | Adorner text block |
| Wall Thickness | Distance between two parallel edges | Adorner with parallel line detection |
| Porosity | Calculate void percentage in ROI | ROI stats with thresholding |

### Libraries and Resources:
- **WriteableBitmapEx** -- Extension methods for WriteableBitmap (line drawing, shapes)
- **OxyPlot** -- For profile line charts and histogram displays
- **LiveCharts** -- For real-time data visualization in measurement panels
- **SciChart** -- High-performance charting for industrial data (commercial)
- **LEADTOOLS** -- Commercial imaging SDK with annotation and measurement (expensive but comprehensive)

## 5.2 Measurement Workflow UI Pattern:

```
1. User selects measurement tool from ribbon or toolbar
2. Cursor changes to crosshair
3. User clicks/drags on image to place measurement
4. Measurement adorner appears with real-time value display
5. Measurement is added to measurement table panel (anchored right/bottom)
6. Each measurement row shows: ID, type, value, unit, color, delete button
7. Export measurements to report (PDF/CSV)
```

---

# PART 6: CsI+FPD SPECIFIC UI PATTERNS

## 6.1 Flat Panel Detector Console Software

### FPD Calibration Workflow:
1. **Offset/Dark Calibration:**
   - Captures dark images (no X-ray exposure)
   - Measures and subtracts electronic offset noise
   - UI: "Acquire Dark" button, progress bar, quality metrics display

2. **Gain/Flat Field Calibration:**
   - Uniform X-ray exposure to correct pixel-to-pixel gain variations
   - UI: "Acquire Flat" button, exposure parameter controls, preview of gain map

3. **Defective Pixel Mapping:**
   - Identifies and maps dead/bad pixels
   - UI: Defect map overlay (highlighted pixels), pixel count statistics, acceptance threshold

4. **Uniformity Correction:**
   - Ensures consistent image response across entire panel
   - UI: Uniformity map visualization, ROI-based uniformity statistics

### Calibration UI Components:
| Component | Description |
|-----------|-------------|
| Calibration Wizard | Step-by-step guide for first-time calibration |
| Live Preview | Real-time detector image during calibration |
| Gain/Offset Status | Status indicators (valid/invalid/expired) |
| Temperature Monitor | Detector temperature display and drift warnings |
| Calibration History | Timestamp, parameters, and quality metrics log |
| Multi-energy Support | Separate calibrations for different kV ranges |

### Flat Field Correction Formula:
```
Corrected = (Raw - Offset) / (Gain - Offset) x Reference
```

### FPD Console Communication:
- Common interfaces: GigE, USB 3.0, Camera Link, IEEE 1394 (legacy)
- SDK typically provided by detector manufacturer (C/C++ DLL, sometimes .NET wrapper)
- Serial communication for tube control (kV, mA)
- Software triggers vs hardware triggers for exposure synchronization

## 6.2 CsI Scintillator-Specific Considerations

### CsI:Tl (Thallium-doped Cesium Iodide) Characteristics:
- High X-ray absorption efficiency
- Good spatial resolution (columnar growth structure)
- Slight afterglow (affects rapid fluoroscopy sequences)
- Hygroscopic -- requires hermetic sealing
- Typical FPD pixel pitch: 100um, 139um, 200um for medical; down to 50um for industrial

### UI Implications:
- Afterglow correction settings may need UI controls for fluoroscopy mode
- Pixel pitch displayed in image info panel (affects measurement calibration)
- Binning mode selection (1x1, 2x2, 4x4) affects resolution and frame rate
- Frame rate display (fps) for live mode
- Integration time control for different exposure scenarios

---

# PART 7: MEDICAL DEVICE UI REGULATIONS

## 7.1 IEC 62366-1:2015+AMD1:2020

### Standard Overview:
- Full title: "Application of usability engineering to medical devices"
- Harmonized under EU MDR (Medical Device Regulation)
- FDA-recognized consensus standard
- Defines a usability engineering process (UEP) throughout device lifecycle

### Key UI/Design Requirements:

#### Use Specification:
- Define intended users (radiographers, technicians, physicists, physicians)
- Define use environments (hospital radiology dept, mobile unit, industrial lab)
- Define user interface elements (software GUI, hardware controls, documentation)

#### Critical Tasks:
- Identify tasks that, if performed incorrectly, could cause serious harm
- X-ray console critical tasks:
  - Setting exposure parameters (kV, mA, time)
  - Patient/part identification
  - Dose management
  - Image quality verification
  - Emergency stop procedures

#### Formative Evaluation:
- Iterative usability testing during development
- Methods: cognitive walkthroughs, heuristic evaluations, user testing
- Purpose: identify usability issues early and fix before final design

#### Summative Evaluation (Validation):
- Final validation with representative users
- Must use simulated or actual use environment
- Users perform critical tasks
- Measures: task completion, errors, time, satisfaction
- Results documented in Human Factors Validation Report

#### Documentation:
- Usability Engineering File (UEF) must be maintained
- Contains: use specification, UI evaluation plan, formative results, summative results, risk analysis

### Design Recommendations for HnVue:
- Document all usability decisions from the start
- Plan formative evaluations at each major UI milestone
- Create use specification early (user profiles, use scenarios, use environments)
- Track critical tasks and their UI implementations explicitly
- Consider summative evaluation before regulatory submission

## 7.2 FDA Human Factors Guidance

### Key Document:
- "Applying Human Factors and Usability Engineering to Medical Devices" (Feb 2016)
- FDA CDRH and CBER guidance
- Recognizes IEC 62366-1 as consensus standard

### When Human Factors Data is Expected:
- PMA (Premarket Approval) submissions
- De Novo requests
- 510(k) with significant UI changes
- Device classified as Software as Medical Device (SaMD)

### Key Expectations:
1. Use-related risk analysis for software interface
2. Formative evaluations during iterative design
3. Summative testing with representative users in simulated conditions
4. Human factors validation report in premarket submissions

### Related Standards:
- **IEC 62366-2** -- Technical report with application guidance
- **ISO 14971** -- Risk management for medical devices
- **ANSI/AAMI HE75:2009/(R)2018** -- Human factors engineering design of medical devices
- **IEC 61223** -- X-ray equipment acceptance testing

## 7.3 IEC 60073 -- Indicator Colors for Industrial UI

### Color Standards:
| Color | Meaning | X-Ray Console Application |
|-------|---------|--------------------------|
| Red | Danger/Emergency | X-ray ON indicator, dose exceeded, interlock open |
| Yellow | Warning/Caution | Calibration needed, approaching dose limit |
| Green | Normal/Safe | System ready, calibration valid, exposure complete |
| Blue | Mandatory action | Please confirm, required input |
| White/Gray | No specific meaning | General status information |

### Flashing Rates:
- Slow flash (0.4-1 Hz): Attention required
- Fast flash (1-2 Hz): Urgent attention required
- Steady: Normal state

---

# PART 8: KOREAN/ASIAN X-RAY SOFTWARE MARKET

## 8.1 Vieworks

### Company Profile:
- **Website:** https://www.vieworks.com
- **Headquarters:** South Korea
- **Description:** "Imaging Expert" -- leader in X-ray detector, industrial camera, and in vivo imaging
- **Key Products:**
  - X-ray flat panel detectors (FPDs)
  - Industrial cameras (machine vision)
  - In vivo imaging systems (bio-imaging)

### X-Ray Detector Products:
- **Vivix** series: portable and fixed FPDs for general radiography
- Various sizes: 14x17", 17x17" and more
- Both wired and wireless models
- Applications: medical radiography, fluoroscopy

### Console Software:
- Detector control and image acquisition software
- Calibration utilities for FPD setup
- DICOM-compatible image management
- SDK available for OEM integration

### Key Personnel:
- Managing Director: Hyungtai Kim
- Contact: +82-70-7011-6166, privacy@vieworks.com

### Website Features:
- Multi-language: Korean, English, Japanese, Chinese
- Product catalogs and user manuals available
- Privacy policy compliant with Korean law (Personal Information Protection Act)

## 8.2 Rayence

### Company Profile:
- **Website:** https://www.rayence.com
- **Headquarters:** South Korea
- **Tagline:** "Global X-ray Imaging parts and materials specialist" (글로벌 X-ray Imaging 부품, 소재 전문기업)
- **Parent/Affiliate:** Vatech (major dental/medical imaging company)

### Key Products (verified from website):
- **WCE Series:** 3rd Generation Medical X-ray Imaging Solution
- **In-house CsI Technology:** "Centre of Scintillator for Medical, Dental, NDT"
- Applications: Medical, Industrial (NDT), Dental, Veterinary
- X-ray software solutions

### Technology:
- **CsI (Cesium Iodide) scintillator** -- in-house manufacturing capability
- Both a-Si (amorphous silicon) and CMOS-based detectors
- TFT detectors and CMOS detectors
- "Imaging the First, Imaging the Next"

### Market Position:
- Key Korean player in global X-ray FPD market
- Competes with Trixell (France), Canon (Japan), Hamamatsu (Japan), Vieworks (Korea)
- Strong in dental CBCT detectors and portable DR systems
- SDK available for OEM system integrators

### Website Structure:
- Korean language primary (www.rayence.com)
- Product categories: Medical, Industrial, Dental, Veterinary, Software
- CsI scintillator technology is a core differentiator

## 8.3 DRTECH

### Company Profile:
- South Korean company developing and manufacturing X-ray flat panel detectors
- Competes in medical and industrial X-ray detector market
- Also known as a Korean-Chinese joint venture in some references

### Market Context:
- Part of the Korean FPD manufacturer cluster alongside Rayence and Vieworks
- Korean FPD manufacturers have gained significant global market share
- Competitive pricing and advancing CMOS detector technology

## 8.4 Korean X-Ray Market Analysis

### Key Players Summary:
| Company | Specialty | Key Differentiator |
|---------|-----------|-------------------|
| Vieworks | FPD + Industrial Camera | Vivix series, multi-segment |
| Rayence | FPD + CsI Scintillator | In-house CsI, Vatech affiliate |
| DRTECH | FPD | Competitive pricing |

### Korean Market Advantages:
- Strong CsI scintillator technology (Rayence in-house manufacturing)
- Competitive pricing vs Japanese/European competitors
- Growing OEM supply to global DR system manufacturers
- Government support for medical device industry (KIMES trade show)

### Software Implications for HnVue:
- Korean FPD vendors typically provide SDK with C/C++ DLL interface
- Some offer .NET wrappers or sample code
- Console software is often bundled with the detector purchase
- OEM console software is the integration point for detector + tube + software
- Korean language support is essential for domestic market

---

# PART 9: SYNTHESIZED DESIGN RECOMMENDATIONS

## 9.1 Technology Stack Confirmation

Based on all research, the recommended WPF technology stack for HnVue:

| Component | Library | Rationale |
|-----------|---------|-----------|
| Ribbon UI | Fluent.Ribbon (MIT) | Office-like ribbon, dark theme, MIT license |
| Docking Panels | AvalonDock/Xceed (MS-PL) | Free, MVVM-friendly, layout serialization |
| DICOM I/O | fo-dicom (MS-PL) | .NET 8 native, MS-PL, active maintenance |
| Charting | OxyPlot (MIT) or LiveCharts | Measurement profiles, histograms |
| Image Rendering | WriteableBitmap + Adorner | Native WPF, no external dependency |
| MVVM Framework | CommunityToolkit.Mvvm | Microsoft-supported, lightweight |
| Dark Theme | Fluent.Ribbon Dark.Steel + custom | Professional dark UI, industry standard |

## 9.2 UI Layout Architecture

```
+----------------------------------------------------------+
| Fluent.Ribbon (Tabs: Acquisition | View | Process | Measure) |
+----------------------------------------------------------+
| Quick Access | Search | System Status | kV | mA | fps    |
+----------+-------------------------------+---------------+
|          |                               |               |
| Tool     |   Main Image Viewport        | Properties    |
| Panel    |   (WriteableBitmap +          | Panel         |
| (Left)   |    Adorner Overlay)           | (Right)       |
|          |                               |               |
| - Series |                               | - Image Info  |
| - Measure|                               | - Acquisition |
| - History|                               | - Detector    |
|          |                               | - Annotations |
+----------+-------------------------------+---------------+
| Status Bar | Histogram | Measurement Table | Log Output   |
+----------------------------------------------------------+
```

## 9.3 Critical UI Design Principles (from all research)

1. **Dark theme is mandatory** -- every X-ray console uses dark UI for image contrast
2. **Ribbon + Docking is the industry standard** -- Volume Graphics, Zeiss Calypso, Geminy
3. **Dual-mode UI** (wizard/expert) -- Yxlon Geminy's award-winning approach
4. **Measurement tools via Adorner overlay** -- standard WPF pattern for image annotation
5. **DICONDE compliance** -- critical for NDT market (Durr NDT, Waygate)
6. **DICOM export** -- fo-dicom for medical market compatibility
7. **Calibration workflow UI** -- step-by-step wizard for FPD calibration
8. **Real-time status indicators** -- temperature, calibration state, connection status
9. **IEC 62366 usability process** -- document all UI decisions for regulatory compliance
10. **Korean language support** -- essential for domestic market with Rayence/Vieworks/DRTECH

## 9.4 Gap Analysis vs Primary Reference

| Topic | Primary Reference Coverage | This Supplement Fills |
|-------|---------------------------|----------------------|
| Yxlon Geminy UI | Listed but no detail | Added wizard/expert mode, product details |
| Durr NDT D-Tect X | Not covered | Full product analysis, DICONDE, PACS |
| AvalonDock MVVM | Listed but no code pattern | Added MVVM architecture, serialization |
| fo-dicom WPF | Listed libraries | Verified .NET 8 support, NuGet packages, rendering |
| Fluent.Ribbon dark theme | Mentioned | Verified GitHub, themes list, MIT license |
| WPF Adorner measurement | General mention | Detailed tool types, implementation pattern |
| FPD calibration UI | Not covered | Full calibration workflow, UI components |
| IEC 62366 details | Referenced | Expanded requirements, documentation needs |
| FDA Human Factors | Referenced | Added guidance details, submission expectations |
| Vieworks | Mentioned | Verified website, Vivix series, contact info |
| Rayence | Mentioned | Verified CsI technology, WCE series, Vatech link |
| DRTECH | Not covered | Added company profile, market position |
| Korean market analysis | Not covered | Added competitive landscape, SDK implications |

---

*This supplementary research document was generated through deep web research across 30+ search queries, direct website analysis of yxlon.com, duerr-ndt.com, vieworks.com, rayence.com, and github.com repositories. All findings are current as of 2026-04-07.*

*Version: 1.0.0 | Date: 2026-04-07 | Classification: Supplementary Research*
