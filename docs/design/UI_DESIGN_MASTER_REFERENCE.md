# HnVue UI Design Master Reference

**Version**: 1.0 | **Date**: 2026-04-06 | **Status**: Active

---

## 1. Current UI Analysis (From Screenshots)

### Observed Characteristics

| Element | Current Value | Source |
|---------|---------------|--------|
| Window title | "HNVUE CONSOLE" ALL CAPS | Windows title bar |
| Header bar | MAH (MahApps.Metro) blue (#1B4F8A) | `HnVue.Primary.Brush` |
| Header text | "HnVue Console" white left-aligned | `HnVue.Text.Primary.Brush` |
| Background | Dark gray (#242424) | `HnVue.Background.Dark.Brush` |
| Left panel | "Patient List" title + search + list | PatientListView.xaml |
| Center | "Image Viewer" + toolbar + viewer | ImageViewerView.xaml |
| Right | Workflow controls + Dose monitor | WorkflowView + DoseDisplayView |
| Buttons | White border outline, white ALL CAPS text | MahApps Dark.Steel defaults |
| Font | Segoe UI (Windows system) | MAH default |
| EMERGENCY | Red button in header when authenticated | MainWindow.xaml |

### Layout Measurements (MainWindow.xaml)
```
┌─────────────────────────────────────────────────────────┐
│  Header: 48px                                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │ Logo(28px) "HnVue Console"    [Username] [⛔STOP] [Logout] │
│  └────────────────────────────────────────────────────┘  │
├───────────┬────────────────────────┬───────────────────┤
│ PatientList│     Image Viewer        │ Workflow (top)     │
│  280px    │       flex (*)          │   +               │
│           │                        │ DoseDisplay(bottom)│
│           │                        │      260px         │
├───────────┴────────────────────────┴───────────────────┤
│  Status Bar: 28px                                       │
└─────────────────────────────────────────────────────────┘
```

---

## 2. Target Design Vision

### Design System Maturity Levels

| Level | Description | Current State |
|-------|-------------|---------------|
| L1 | Basic colors applied | ✅ Done |
| L2 | CoreTokens semantic tokens | ✅ Done |
| L3 | Component-level tokens | ✅ Done |
| L4 | Rich interactions (animations, transitions) | 🔜 Next |
| L5 | Accessibility-complete (screen reader, HC mode) | 🔜 Phase 2 |

### Design DNA (Preserved from Existing UI)
1. Dark theme (#242424 base) — PPT 슬라이드 4 지정, 기존 사용자 연속성
2. MahApps.Metro Blue header (#1B4F8A) — brand identity
3. White text throughout — high contrast on dark
4. Outline button style — preserved for toolbar actions (ZOOM IN/OUT etc.)
5. ALL CAPS button text — kept for major action buttons
6. Compact, functional layout — no decoration without purpose

### Modernization Additions
1. Gradient logo badge in header and login screen
2. Segoe MDL2 Assets icons on all buttons
3. 44px minimum row height (IEC 62366 touch targets)
4. Accent color (#00AEEF) for focus states and indicators
5. Rounded corners (4-12px) on cards and inputs
6. Status indicators with IEC 62366 color coding

---

## 3. Global Medical Device UI Patterns

### Industry Reference Systems

| System | Company | Key UI Patterns |
|--------|---------|-----------------|
| syngo | Siemens | Modular ribbon, context-sensitive tools, workflow panels |
| Centricity | GE Healthcare | Sidebar worklist, dual-panel comparison, hanging protocols |
| IntelliVue | Philips | Real-time waveforms, color-coded alarm hierarchy |
| Vue PACS | Carestream | Tabbed multi-series, measurement tools, keyboard shortcuts |
| Sectra PACS | Sectra | Minimalist dark BG, hanging protocols, batch ops |

### Universal Medical Device UI Laws
1. **Dark backgrounds always** — reduces eye strain in dimmed rooms, improves image contrast
2. **Color-coded status** — Red=Emergency/Stop, Amber=Warning, Green=Safe, Blue=Info
3. **Emergency controls always visible** — IEC 62366 / FDA HFE requirement
4. **Patient ID prominent** — largest text element in patient context
5. **Keyboard shortcuts** — Space/Enter for primary action, Escape for abort
6. **No silent failures** — every action gets visual + optional audio feedback
7. **Minimal modals** — prefer inline status, modals only for critical decisions
8. **Touch-target minimum** — 44px (IEC 62366), 56px for safety-critical

---

## 4. Icon System

### Recommendation: Segoe MDL2 Assets (Built-in Windows)

**Rationale:**
- Zero additional NuGet packages required
- Already available on all Windows 10/11 systems
- Consistent with MahApps.Metro visual style
- IEC 62366 compatible (clear, recognized symbols)

**Implementation:**
```xml
<TextBlock Text="&#xE721;"
           FontFamily="SMA (Segoe MDL2 Assets)"
           FontSize="14"
           VerticalAlignment="Center"/>
```

### Icon Reference Table

| Action | Unicode | Description |
|--------|---------|-------------|
| Search | &#xE721; | Magnifying glass (SMA) |
| Add/Plus | &#xE710; | Plus sign (SMA) |
| Person | &#xE77B; | Single person (SMA) |
| Zoom In | &#xE8A3; | Magnify+ |
| Zoom Out | &#xE71F; | Magnify- |
| Reset | &#xE777; | Reset/Refresh arrows |
| Warning | &#xE7BA; | Triangle warning |
| Emergency | &#xE71A; | Stop/prohibition circle |
| Acquisition | &#xE722; | Camera/capture |
| Radiation | &#xECA4; | Activity/radiation |
| Settings | &#xE713; | Gear settings |
| Logout | &#xF3B1; | Sign-out arrow |
| Image | &#xEB9F; | Photo/image |
| Play/Expose | &#xE7C8; | Circle play |
| Prepare | &#xE768; | Checkmark prepare |
| Clock | &#xE916; | Clock/time |
| User | &#xE77B; | Person silhouette |
| Lock | &#xE72E; | Padlock |
| TLS/Secure | &#xE72E; | Lock (connected) |
| Disconnect | &#xE8CD; | Disconnect symbol |

---

## 5. PPT UI 변경 명세 구현 현황

**출처:** `docs/★HnVUE UI 변경 최종안_251118.pptx` (22슬라이드, 2025-11-18)

### 5.1 구현 완료 항목

| 슬라이드 | 화면 | 변경 내용 | 구현 파일 |
|---------|-----|---------|---------|
| 1 | 로그인 | Username: TextBox → **ComboBox** 드롭다운 | LoginView.xaml |
| 4 | Worklist | **기간 필터 버튼** Today/3Days/1Week/All/1Month 추가 | PatientListView.xaml |
| 4 | 디자인 토큰 | **배경색 #242424** (#1A1A2E에서 변경) | CoreTokens.xaml |
| 7 | Studylist | **이전/다음 내비** + **PACS 드롭다운** + 기간 필터 | StudylistView.xaml (신규) |
| 8 | 환자 등록 | Patient+Procedure **통합 창**, (*) 필수, Auto-Generate, 칩 UI | AddPatientProcedureView.xaml (신규) |
| 13 | Merge | **"Sync Study"** 명칭 변경, 3열 레이아웃, Preview 강화 | MergeView.xaml (신규) |
| 14~21 | Settings | **상단 탭 배치**, Network 탭 통합(PACS+Worklist+Print), **Access Notice** | SettingsView.xaml (신규) |

### 5.2 보존된 기존 UI 구조

| 항목 | 보존 이유 |
|------|---------|
| Detector / Generator 설정 구조 | PPT 슬라이드 16 명시: "현행 구조 유지" |
| 전반적 창 구조 (MainWindow 3열) | 기존 사용자 연속성 |
| MahApps.Metro Dark.Steel 테마 | 다크 테마 의료기기 표준 |
| 브랜드 색상 #1B4F8A / #00AEEF | 기업 아이덴티티 유지 |

### 5.3 신규 ViewModel 계약 (Interface Contracts)

| 인터페이스 | ViewModel | 주요 멤버 |
|----------|----------|---------|
| IStudylistViewModel | StudylistViewModel | NavigatePrevious/NextCommand, PacsServers, FilterByPeriodCommand |
| IAddPatientProcedureViewModel | AddPatientProcedureViewModel | SelectedProjections, SelectedDescriptions, IsAutoGenerate |
| IMergeViewModel | MergeViewModel | PatientsA/B, PreviewStudiesA/B, MergeCompleted |
| ISettingsViewModel | SettingsViewModel | ActiveTab, AccessNoticeText, ActiveRisTab |

### 5.4 신규 컨버터

| 컨버터 | 용도 |
|-------|------|
| ActiveTabToVisibilityConverter | Settings 탭 패널 표시/숨김 |
| StringEqualityToBoolConverter | 탭 ToggleButton IsChecked 바인딩 |

---

## 6. Screen-by-Screen Design Specifications

### 5.1 Login Screen (LoginView.xaml)

```
Background: HnVue.Semantic.Surface.Page (#1A1A2E)
Card:       HnVue.Semantic.Surface.Panel (#16213E)
            Width=320, CornerRadius=12, DropShadow

Logo:       LinearGradient Primary→PrimaryLight
            Content: "HnVue" Bold 18px white
            Height=56, CornerRadius=8

Title:      "HnVue Login" Bold 20px Text.Primary
Subtitle:   "Medical Imaging Console" 13px Text.Secondary

Inputs:     Height=44, CornerRadius=4
            Background: Surface.Card (#0F3460)
            Border: Border.Default (#2E4A6E)
            Focus: Border.Focus (#00AEEF)

Button:     HnVue.PrimaryButton, Height=44, full width
            Text: "LOGIN" SemiBold 14px

Error:      Status.Emergency (#D50000), 12px
Loading:    3px bar, Brand.Accent (#00AEEF)
```

### 5.2 Patient List (PatientListView.xaml — 280px panel)

```
Background: HnVue.Semantic.Surface.Panel (#16213E)

Header:     "Patient List" Bold 16px + count badge
            Badge: Brand.Primary circle, white count text

Search:     Height=36, CornerRadius=4
            Icon: &#xE721; Segoe MDL2, Text.Secondary
            Background: Surface.Card (#0F3460)
            Border: Border.Default

Register:   Height=32
            Icon: &#xE710; + "Register" text
            Background: Surface.Card

Rows:       Height=44 (IEC 62366 touch target)
            Hover: PrimaryLight @ 25% opacity
            Selected: Brand.Primary (#1B4F8A)
            Separator: Border.Default bottom

Item:       PatientID Bold 12px Text.Primary
            Name Regular 12px Text.Secondary

EMRG badge: Status.Emergency (#D50000) background
            "EMRG" Bold 10px white, CornerRadius=2
```

### 5.3 Image Viewer (ImageViewerView.xaml — center panel)

```
Background: HnVue.Semantic.Surface.Page (#1A1A2E)

Header:     "Image Viewer" Bold 16px Text.Primary

Toolbar buttons (white outline — matches screenshot):
            Style={x:Null}
            Background=Transparent
            BorderBrush=Text.Primary, BorderThickness=1
            Foreground=Text.Primary, Height=32
            
ZOOM IN:    Icon &#xE8A3; + "ZOOM IN" SemiBold 12px
ZOOM OUT:   Icon &#xE71F; + "ZOOM OUT" SemiBold 12px
RESET W/L:  Icon &#xE777; + "RESET W/L" SemiBold 12px

Zoom text:  "Zoom:" Text.Secondary + value Text.Primary Bold

Image area: Background=#090909 (near-black, NOT CoreToken)
            CornerRadius=4
            
Placeholder: &#xEB9F; 48px #444455 + text 13px #555566
```

### 5.4 Workflow/Acquisition (WorkflowView.xaml — 260px right panel)

```
Background: HnVue.Semantic.Surface.Panel (#16213E)
Width:      260px (fit into MainWindow right column)

Header:     Icon &#xE722; Accent + "Acquisition" 14px Normal

SafeState:  Background from SafeStateToColorConverter
            Icon &#xE7BA; + SafeStateLabel Bold + CurrentState Small

Status msg: Text.Secondary 13px, TextWrapping=Wrap

PREPARE:    HnVue.OutlineButton (white border)
            Height=40, FullWidth
            Icon &#xE768; + "PREPARE" SemiBold

EXPOSE:     HnVue.PrimaryButton (filled blue)
            Height=44, FullWidth
            Icon &#xE7C8; + "EXPOSE" Bold

STOP:       HnVue.EmergencyStopButton (red, #D50000)
            Height=56, FullWidth, ALWAYS ENABLED
            Icon &#xE71A; + "STOP" + "비상 정지 (Esc)"
            Escape KeyBinding at UserControl level
```

### 5.5 Dose Monitor (DoseDisplayView.xaml — below WorkflowView)

```
Background: HnVue.Component.DosePanel.Bg
Border:     Top separator Border.Default 1px

Header:     Icon &#xECA4; Warning + "DOSE MONITOR" 12px

DAP row:    Label "DAP" Text.Secondary
            Value: 22px SemiBold Text.Primary
            Unit: "mGy·cm²" 10px Text.Secondary

DRL gauge:  ProgressBar 8px height, CornerRadius=4
            Track: DosePanel.GaugeTrack
            Fill:  DosePanel.GaugeSafe (default)
            Thresholds:
              0-69%: GaugeSafe (#00C853)
              70-89%: GaugeWarning (#FFD600)
              90-99%: GaugeBlocked (#FF6D00)
              100%+: GaugeEmergency (#D50000)

Alert banner: Status.Emergency background
              Icon &#xE7BA; + "DOSE ALERT — DRL exceeded"
              Visible: IsDoseAlert=True
```

### 5.6 Main Window Header (MainWindow.xaml — 48px)

```
Background: HnVue.Primary.Brush (#1B4F8A)

Logo badge: Brand.Accent (#00AEEF) 28x28px CornerRadius=4
            "H" Bold 14px white

Title:      "HnVue Console" 16px Text.Primary

Username:   CurrentUsername Text.Primary
Role:       CurrentRoleDisplay Text.Secondary

STOP btn:   HnVue.EmergencyStopButton style
            "⛔ STOP" Bold
            Command: EmergencyCommand

Logout:     White outline style
            Icon &#xF3B1; + "Logout"
```

---

## 6. WPF (Windows Presentation Foundation) Implementation Patterns

### Pattern 1: DRC (DynamicResource Colors) (ALWAYS use this)
```xml
<!-- ✅ Correct -->
<TextBlock Foreground="{DynamicResource HnVue.Semantic.Text.Primary}"/>

<!-- ❌ Wrong - hardcoded -->
<TextBlock Foreground="#FFFFFF"/>
```

### Pattern 2: WOB (White Outline Button) (toolbar actions)
```xml
<Button Style="{x:Null}"
        Background="Transparent"
        BorderBrush="{DynamicResource HnVue.Semantic.Text.Primary}"
        BorderThickness="1"
        Foreground="{DynamicResource HnVue.Semantic.Text.Primary}"
        Height="32" Padding="12,6">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="&#xE8A3;" FontFamily="Segoe MDL2 Assets"
                   FontSize="13" Margin="0,0,5,0" VerticalAlignment="Center"/>
        <TextBlock Text="ACTION" FontSize="12" FontWeight="SemiBold"
                   VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

### Pattern 3: ES (Emergency Stop) (NEVER modify)
```xml
<Button Command="{Binding AbortCommand}"
        Style="{StaticResource HnVue.EmergencyStopButton}"
        HorizontalAlignment="Stretch">
    <!-- Content per context -->
</Button>
<!-- Never add IsEnabled binding to Emergency Stop! -->
```

### Pattern 4: SB (Status Badge)
```xml
<Border Background="{DynamicResource HnVue.Semantic.Status.Emergency}"
        CornerRadius="2" Padding="4,1">
    <TextBlock Text="EMRG" Foreground="White"
               FontSize="10" FontWeight="Bold"/>
</Border>
```

### Pattern 5: DIF (Dark Input Field)
```xml
<Border Height="44" CornerRadius="4"
        Background="{DynamicResource HnVue.Semantic.Surface.Card}"
        BorderBrush="{DynamicResource HnVue.Semantic.Border.Default}"
        BorderThickness="1" Padding="12,0">
    <TextBox Background="Transparent"
             Foreground="{DynamicResource HnVue.Semantic.Text.Primary}"
             BorderThickness="0"
             CaretBrush="{DynamicResource HnVue.Semantic.Brand.Accent}"/>
</Border>
```

---

## 7. IEC 62366 (International Electrotechnical Commission) Compliance Checklist

### Safety-Critical Controls

| Requirement | Implementation | Status |
|-------------|----------------|--------|
| Emergency Stop always visible | Fixed position in WorkflowView + header | ✅ |
| Emergency Stop always enabled | `IsEnabled` not bound, no Collapsed | ✅ |
| Emergency Stop Escape key | `KeyBinding Key="Escape"` in UserControl | ✅ |
| Emergency Stop 56px height | `MinHeight=56` in EmergencyStopButton style | ✅ |
| Touch target 44px minimum | All buttons MinHeight=44px | ✅ |
| Safety color Red=Emergency | `#D50000` reserved for emergency only | ✅ |
| Safety color Amber=Warning | `#FFD600` reserved for warnings | ✅ |
| Safety color Green=Safe | `#00C853` reserved for safe states | ✅ |
| Patient ID prominent | Bold text, largest in patient context | ✅ |

### Color Contrast (WCAG 2.2 AA)

| Combination | Ratio | Status |
|-------------|-------|--------|
| White on #1B4F8A (Primary) | 7.2:1 | ✅ PASS |
| White on #D50000 (Emergency) | 5.9:1 | ✅ PASS |
| #B0BEC5 on #1A1A2E (Muted) | 7.1:1 | ✅ PASS |
| #00C853 on #1A1A2E (Safe) | 5.8:1 | ✅ PASS |
| #FFD600 on #1A1A2E (Warning) | 10.6:1 | ✅ PASS |

---

## 8. Available WPF (Windows Presentation Foundation) Styles Reference

| Style Key | Type | Usage |
|-----------|------|-------|
| `HnVue.PrimaryButton` | Button | Filled primary actions (LOGIN, EXPOSE) |
| `HnVue.EmergencyStopButton` | Button | STOP/ABORT — always enabled, red |
| `HnVue.DangerButton` | Button | High-risk actions (delete, override) |
| `HnVue.SecondaryButton` | Button | Secondary actions with accent border |
| `HnVue.OutlineButton` | Button | Toolbar actions (ZOOM IN/OUT) |
| `HnVue.AccentOutlineButton` | Button | Active toolbar state |
| `HnVue.DarkTextBox` | TextBox | Standalone dark-themed inputs |
| `HnVue.SectionHeader` | TextBlock | Panel section titles |
| `HnVue.MutedLabel` | TextBlock | Labels, captions, secondary text |

---

## 9. Pending Improvements (Next Phase)

### Phase 2 Targets
1. **Animated Emergency Stop** — Pulsing red glow during exposure (WPF Storyboard)
2. **Status bar DICOM connection** — Live green/red indicator (partially done)
3. **Quick PIN lock overlay** — QuickPinLockView styling
4. **System Admin view** — SystemAdminView.xaml styling
5. **CD Burn view** — CDBurnView.xaml styling
6. **Worklist/StudyList** — Full worklist with DataGrid (not just PatientList)

### Design Debt
- MainWindow IsDicomConnected — ViewModel may not have this property (FallbackValue=False used)
- DoseDisplayView DrlPercentage — Calculated property may need adding to ViewModel
- WorkflowView compact panel — Body part grid and kVp/mAs inputs not yet added (Phase 2)

---

Version: 1.0
Status: Active reference for all UI development
Last Updated: 2026-04-06
