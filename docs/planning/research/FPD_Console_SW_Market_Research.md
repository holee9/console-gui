# FPD Console Software Market Research
**Date:** 2026-03-27
**Purpose:** Research how small FPD companies develop and deliver console/acquisition software
**Context:** Company with <100 employees, 2 SW developers, manufactures CsI scintillators and FPDs

---

## 1. How Small FPD Companies Handle Console Software

### Korean FPD Companies (Direct Competitors)

**Rayence (Korea)**
- Develops its own acquisition software: **Xmaru View V1**
- Full-featured console: DICOM MWL, image stitching, DICOM printing, DVD/CD burning, patient history management
- Specialized variants: Xmaru VetView (veterinary), Xmaru PodView (podiatry)
- Rayence is the only Korean company with in-house capabilities for CMOS wafer design, TFT FPD development, AND scintillator technology
- Software is a key differentiator bundled with their detectors
- Sources: [Rayence Xmaru View](https://www.rayence.com/software_en/15549), [Rayence USA](https://radiologyimagingsolutions.com/product/xmaruview-v1-acquisition-software/)

**Vieworks (Korea)**
- Develops its own acquisition software: **VXvue**
- Award-winning software bundled FREE with VIVIX-S detectors (plug-and-play)
- Features: auto stitching, auto cropping, auto labeling, PureImpact post-processing, multi-patient management, multilingual, touch-oriented UI
- Also white-labels to OEM partners (e.g., EXAMION, Del Medical, Medlink Imaging)
- Sources: [VXvue Software](https://xrayimaging.vieworks.com/en/software/vxvue), [EXAMION VXvue](https://examion.us/site/vxvue-x-ray-acquisition-software/)

**DRTECH (Korea)**
- Global provider of FPD solutions since 2000
- Offers portable console PCs with imaging systems
- Products span General Radiography, Mammo, Dental, Veterinary, Dynamic, Industrial
- Sources: [DRTECH](https://www.drtech.co.kr/en/), [Komachine](https://www.komachine.com/en/companies/dr-tech)

**iRay Technology (China)**
- Provides bundled acquisition software: **iRay DR** and **RiasDR**
- Designed to work with iRay wireless and tethered detectors
- Automates image acquisition with advanced image processing
- Sources: [iRay DR](https://www.medicalexpo.com/prod/iray-technology/product-92939-1091603.html)

### Key Pattern: Most FPD companies of significant size develop their own console SW
- It is a competitive differentiator
- Bundled free with detector hardware
- Customized for their specific detector capabilities
- White-labeled to OEM system integrators

---

## 2. Buy vs Build Analysis for Medical Device Console SW

### Option A: BUILD In-House (Current Path)

**Pros:**
- Full control over features and roadmap
- Deep integration with own FPD hardware
- Competitive differentiation
- No licensing fees or vendor lock-in

**Cons:**
- With only 2 developers, extremely resource-constrained
- IEC 62304 compliance documentation burden
- Estimated cost: $30K-$500K+ depending on complexity
- Timeline: 1-3 years for Class II medical device software
- Ongoing maintenance burden

**Cost Estimates (Industry Data):**
- Basic medical device SW: $8K-$25K (3-8 months)
- Comprehensive healthcare platform: $30K-$500K+
- Full Class II device development (including regulatory): $2M-$30M total
- HIPAA compliance adds 15-25% to development costs ($15K-$75K)
- Sources: [Dev Technosys](https://devtechnosys.com/insights/cost-to-build-medical-device-software/), [Complizen](https://www.complizen.ai/post/medical-device-development-costs-2025-budget-guide)

### Option B: BUY / OEM White-Label

**OR Technology (Germany) - Best OEM Option Identified**
- Offers **dicomPACS DX-R** as white-label OEM solution
- OEM partners can brand it under their own name
- Complete integration: X-ray generators, stands, flat panels, CR systems, image processing, patient admin, PACS
- Reduces development time and costs significantly
- Manufacturer-independent PACS solution
- Supports all major modalities (X-ray, CT, MRI, US)
- DICOM and HL7 integration
- Includes mobile viewing (dicomPACS MobileView)
- Sources: [OR Technology OEM](https://www.or-technology.com/en/products/oem/oem-partnership-for-human-medicine.html), [AuntMinnie](https://www.auntminnie.com/imaging-informatics/enterprise-imaging/pacs-vna/article/15598590/or-technology-debuts-dicompacs-dx-r-20)

**Other OEM/SDK Options:**
- **Varex Imaging**: Provides Windows-based app and DLL library for OEM customers to build their own interface
- **DIRA (Toshiba)**: Total X-ray visualization solution for OEMs with complimentary software
- **Knoveltech**: UNICON Control Station Software + Context Vision Image Processing

### Option C: OPEN SOURCE Foundation + Custom Development

**iBEX (Best Open Source Option)**
- Modular, device-independent, open-source software for digital radiography
- Published in Journal of Digital Imaging (2020, PMC7256160)
- Features: device calibration, HIS integration, image acquisition, local storage, PACS send
- Extension mechanism for custom image processing plug-ins
- Hardware-agnostic: works with heterogeneous detector communication channels
- Suitable for preclinical and early clinical testing
- **Limitation:** Freeware for R&D; would need regulatory work for commercial deployment
- Sources: [iBEX PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC7256160/), [iBEX Springer](https://link.springer.com/article/10.1007/s10278-019-00304-1)

**Supporting Open Source Tools:**
- **Orthanc**: Free, lightweight DICOM server
- **Weasis**: Multipurpose DICOM viewer (standalone + web-based), FLOSS, cross-platform
- **fo-dicom**: DICOM toolkit in C# (.NET Standard 2.0)
- **DCMTK**: C++ DICOM toolkit (mature, widely used)
- **pydicom**: Python DICOM library
- Sources: [Orthanc](https://www.orthanc-server.com/), [Weasis](https://weasis.org/), [awesome-dicom](https://github.com/open-dicom/awesome-dicom)

**Commercial DICOM SDKs (for hybrid approach):**
- **Merge DICOM Toolkit** (Merative/IBM): Comprehensive, all modalities
- **LEADTOOLS**: Full DICOM dev tools for .NET, C/C++, Java, Web
- **MODALIZER-SDK** (HRZ): DICOM files, networking, Query/Retrieve, Print, MWL, MPPS
- **DicomObjects**: DICOM API/SDK toolkit
- **DCF (Imaging Solutions)**: Object-oriented DICOM v3.0 components
- Sources: [Merative](https://www.merative.com/merge-imaging/dicom-toolkit), [LEADTOOLS](https://www.leadtools.com/sdk/medical/dicom), [MODALIZER](https://www.hrzkit.com/products/modalizer-sdk/)

### Recommendation Matrix

| Criteria | Build | OEM (OR Tech) | Open Source + Custom |
|----------|-------|---------------|---------------------|
| Time to market | 1-3 years | 3-6 months | 6-12 months |
| Cost (initial) | $100K-$500K | License fee (negotiable) | $30K-$100K |
| Regulatory burden | Full IEC 62304 | Shared with OEM vendor | Full IEC 62304 + SOUP validation |
| Differentiation | High | Low (shared platform) | Medium |
| HW integration | Perfect | Good (standard APIs) | Good (customizable) |
| Ongoing cost | 2 FTE maintenance | License renewals | 1 FTE maintenance |
| Risk | High (2 devs) | Low | Medium |

---

## 3. IEC 62304 Compliance for Small Teams (2 Developers)

### Safety Classification Strategy

Console/acquisition software is typically **Class B** (injury possible but not serious) because:
- Software failure could lead to incorrect exposure parameters or image artifacts
- Does not directly control life-sustaining functions
- Diagnostic decisions are made by physicians, not software

### Class B Minimum Documentation Requirements

| IEC 62304 Clause | Class A | Class B | Class C |
|-------------------|---------|---------|---------|
| 5.1 Software Development Planning | Required | Required | Required |
| 5.2 Software Requirements Analysis | Required | Required | Required |
| 5.3 Software Architecture Design | -- | **Required** | Required |
| 5.4 Software Detailed Design | -- | Subdivision only (5.4.1) | Full detail required |
| 5.5 Software Unit Implementation | -- | **Required** | Required |
| 5.6 Software Integration Testing | -- | **Required** | Required |
| 5.7 Software System Testing | Required | Required | Required |
| 5.8 Software Release | Required | Required | Required |

**Key Class B Savings vs Class C:**
- Detailed design of individual units is NOT required (only subdivision into units)
- Less granular unit-level verification
- Bi-directional traceability still required (risk -> requirement -> implementation -> test)

### Free Resources for Small Teams

**OpenRegulatory (openregulatory.com)**
- Free, open-source templates for ISO 13485, IEC 62304, ISO 14971, IEC 62366
- Available as Word, PDF, Markdown, or HTML
- GitHub repository: [openregulatory/templates](https://github.com/openregulatory/templates)
- IEC 62304 mapping document included
- **Formwork**: Free QMS software for lean, founder-led medical device companies
- Sources: [OpenRegulatory Templates](https://openregulatory.com/templates/), [GitHub](https://github.com/openregulatory/templates)

### FDA 510(k) Software Documentation Strategy

- FDA uses risk-based approach: higher risk = more documentation
- At basic documentation level, Software Design Specification not required in premarket submission (keep in Device History File)
- Key submission components: device description, software architecture, risk analysis, V&V results, substantial equivalence comparison
- Consider hiring a SaMD consultant for first submission ($15K-$50K)
- Sources: [Promenade Software](https://www.promenadesoftware.com/blog/fda-iec62304-software-documentation), [I3CGlobal](https://www.i3cglobal.com/software-medical-device-documentation/)

### Practical Strategy for 2-Developer Team

1. **Classify as Class B** to minimize documentation overhead
2. **Use OpenRegulatory templates** to bootstrap QMS documentation
3. **Adopt SOUP strategy**: Use validated open-source components (fo-dicom, Orthanc) with proper SOUP documentation
4. **Outsource regulatory consulting** for first 510(k) submission
5. **Automate testing**: Invest in CI/CD with automated verification to reduce manual documentation burden

---

## 4. Minimum Viable Console Software Features

### Must-Have (MVP) Features

Based on WHO technical specifications, industry standards, and competitor analysis:

**Patient Management:**
- Patient registration (manual + DICOM Modality Worklist)
- Patient search and history
- Study/series management

**Image Acquisition:**
- Detector connection and control (specific to own FPD)
- Exposure parameter configuration
- Image preview and acceptance/rejection
- Organ-specific technique charts (APR - Anatomical Programmed Radiography)

**Image Processing:**
- Window/level adjustment
- Zoom, pan, rotate, flip
- Basic measurements (distance, angle)
- Organ-specific auto-processing algorithms
- Annotation tools

**DICOM Compliance:**
- DICOM Storage (send to PACS)
- DICOM Modality Worklist (receive patient/order info from RIS/HIS)
- DICOM Print
- DICOM Query/Retrieve (basic)
- DICOM CD/DVD export with viewer

**Data Management:**
- Local image storage/archive
- Patient data backup
- Export (DICOM, JPEG, PNG)

### Nice-to-Have (Phase 2) Features

- Image stitching (long-leg, full-spine)
- Auto-cropping and auto-labeling
- Multi-monitor support (acquisition + diagnostic)
- HL7 integration
- Web-based remote viewing
- AI-assisted quality checks
- Veterinary-specific modes
- Multi-language support
- Touch-screen optimized UI

### WHO Portable DR System Requirements (Reference)

The WHO specification covers: X-ray generator, generator stand, detector, detector stand, portable workstation/PC-console, software/hardware for data management and communication. CAD software is explicitly excluded from base requirements.
- Source: [WHO Publication](https://www.who.int/publications/i/item/9789240033818)

---

## 5. CsI Scintillator + FPD Competitive Landscape

### Companies with Both CsI Deposition AND Detector Assembly

| Company | CsI Capability | FPD Assembly | Console SW | HQ |
|---------|---------------|--------------|------------|-----|
| **Rayence** | In-house scintillator | In-house TFT + CMOS | Xmaru View (own) | Korea |
| **Hamamatsu** | In-house CsI deposition (GPXS series) | Scintillator plates + FOS | SDK/components only | Japan |
| **Trixell** | In-house CsI technology | Pixium detectors | No standalone SW | France (Thales JV) |
| **Varex Imaging** | Direct deposition CsI | Full FPD line (XRD, XRpad, PaxScan) | DLL library for OEM | USA |
| **RMD/Dynasil** | Vapor-grown columnar CsI:Tl films | Scintillator films only (no FPD) | N/A | USA |
| **Vieworks** | Uses CsI (VIVIX-S with cesium detector) | In-house FPD | VXvue (own, bundled free) | Korea |
| **DRTECH** | Undisclosed | In-house FPD line | Own console SW | Korea |
| **iRay Technology** | Uses CsI | In-house FPD | iRay DR / RiasDR | China |
| **CareRay** | Uses CsI | In-house FPD | Basic SW | China |

### Key Observations

1. **Rayence is the closest comparable**: Only Korean company with full vertical integration (CMOS wafer + TFT + scintillator + software)
2. **Hamamatsu** sells CsI scintillator plates as components; does not compete in finished FPD market directly
3. **Trixell** (Thales/Siemens/Philips JV) keeps CsI tech in-house for parent companies' systems
4. **Varex** is the largest pure-play FPD component supplier; provides SDK but not full console SW
5. **Chinese competitors** (iRay, CareRay) are aggressively expanding with bundled software, competing on price

### Vertical Integration Advantage

Companies that control CsI deposition + FPD assembly have a significant advantage:
- Optimized scintillator-to-detector coupling
- Better quality control across the imaging chain
- Ability to tune CsI thickness/structure for specific applications
- Cost advantage over companies purchasing scintillator plates

---

## 6. Actionable Recommendations for a <100 Employee, 2-Developer Company

### Immediate Priority (0-6 months)

1. **Evaluate OR Technology OEM partnership** for fastest time-to-market with white-labeled dicomPACS DX-R
2. **If building in-house**: Start with fo-dicom (.NET) + Orthanc as SOUP foundation
3. **Adopt OpenRegulatory templates** for IEC 62304 Class B documentation
4. **Hire a SaMD regulatory consultant** for 510(k) strategy ($15K-$50K one-time)

### Medium-Term Strategy (6-18 months)

1. **Hybrid approach recommended**: OEM software for initial market entry + parallel development of proprietary acquisition module optimized for own FPD
2. **Focus 2 developers on**: detector-specific integration layer, proprietary image processing algorithms, and hardware-specific calibration tools
3. **Outsource**: PACS integration, DICOM networking, UI/UX design

### Long-Term Differentiation (18+ months)

1. **Leverage CsI + FPD vertical integration** as primary competitive advantage (like Rayence)
2. **Develop proprietary image processing** optimized for own scintillator characteristics
3. **Consider acquiring/licensing VXvue-style bundled software model** to compete with Vieworks and Rayence
4. **AI integration**: Partner with AI companies for CAD features rather than building in-house

### Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| 2 developers leave | Medium | Critical | Document everything; use standard frameworks |
| Regulatory rejection | Low-Medium | High | Hire consultant; use OpenRegulatory templates |
| OEM vendor discontinues | Low | High | Maintain ability to switch; avoid deep lock-in |
| Chinese competitors undercut price | High | Medium | Differentiate on quality, service, integration |
| Timeline overrun | High | Medium | Start with OEM, build in parallel |

---

## Sources Summary

### Company & Product Pages
- [Rayence Xmaru View](https://www.rayence.com/software_en/15549)
- [Vieworks VXvue](https://xrayimaging.vieworks.com/en/software/vxvue)
- [DRTECH](https://www.drtech.co.kr/en/)
- [OR Technology OEM](https://www.or-technology.com/en/products/oem/oem-partnership-for-human-medicine.html)
- [Varex Imaging](https://www.vareximaging.com/solutions/4030dx/)
- [Hamamatsu CsI](https://www.hamamatsu.com/us/en/product/optical-sensors/x-ray-sensor/scintillator-plate/gpxs.html)
- [Trixell Technology](https://www.trixell.com/technology)
- [CareRay](https://careray.com/)

### Regulatory & Standards
- [OpenRegulatory IEC 62304 Templates](https://openregulatory.com/templates/)
- [OpenRegulatory GitHub](https://github.com/openregulatory/templates)
- [IEC 62304 Walkthrough](https://openregulatory.com/articles/iec-62304-walkthrough)
- [Johner Institute Safety Classes](https://blog.johner-institute.com/iec-62304-medical-software/safety-class-iec-62304/)
- [FDA IEC 62304 Documentation](https://www.promenadesoftware.com/blog/fda-iec62304-software-documentation)
- [Sequenex Build vs Buy](https://sequenex.com/build-vs-buy-regulatory-ready-medtech-platform)

### Open Source & SDKs
- [iBEX (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC7256160/)
- [Orthanc DICOM Server](https://www.orthanc-server.com/)
- [Weasis DICOM Viewer](https://weasis.org/)
- [awesome-dicom GitHub](https://github.com/open-dicom/awesome-dicom)
- [LEADTOOLS DICOM SDK](https://www.leadtools.com/sdk/medical/dicom)
- [MODALIZER-SDK](https://www.hrzkit.com/products/modalizer-sdk/)
- [Merge DICOM Toolkit](https://www.merative.com/merge-imaging/dicom-toolkit)

### Market & Cost Data
- [Medical Device Development Costs 2025](https://www.complizen.ai/post/medical-device-development-costs-2025-budget-guide)
- [Cost to Build Medical Device Software](https://devtechnosys.com/insights/cost-to-build-medical-device-software/)
- [WHO Portable DR Specifications](https://www.who.int/publications/i/item/9789240033818)
- [Top X-Ray Detector Companies](https://www.verifiedmarketresearch.com/blog/top-x-ray-detector-companies/)
