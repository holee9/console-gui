# Pencil/Figma → WPF XAML 변환 워크플로우

## 문서 정보

| 항목 | 내용 |
|------|------|
| 문서 ID | ARCH-DX-001 |
| 버전 | 1.1 |
| 작성일 | 2026-04-07 |
| 규격 기준 | SPEC-UI-001 §DT-005 |

---

## 1. 개요

이 워크플로우는 Pencil 또는 Figma에서 만든 UI 디자인을 WPF XAML 코드로 변환하는 표준 절차입니다.

**원칙**: 디자인 도구가 진실의 원천(Source of Truth). 승인된 디자인만 구현합니다.

---

## 2. 도구 설정

### Pencil (무료, 오픈소스 — 권장 기본 도구)

**다운로드**: https://pencil.evolus.vn/ (공식 사이트)

**프로젝트 파일 위치**:
```
docs/ui_mockups/
├── design_system.pen         ← 디자인 시스템 기준 파일
└── screens/
    ├── login.pen
    ├── worklist.pen
    ├── studylist.pen
    ├── acquisition.pen       ← CRITICAL: Emergency Stop 항상 표시
    ├── merge.pen
    ├── settings.pen
    └── add-patient.pen
```

**Pencil 사용 시 HnVue 색상 팔레트 설정**:
- Background Page: #1A1A2E
- Background Panel: #16213E
- Background Card: #0F3460
- Primary Blue: #1B4F8A
- Accent Blue: #00AEEF
- Emergency Red: #D50000
- Warning Amber: #FFD600
- Safe Green: #00C853
- Text Primary: #FFFFFF
- Text Secondary: #B0BEC5

### Figma (무료 Community 티어)

**무료 의료 UI 킷 검색 키워드**:
- "medical dashboard"
- "healthcare UI kit"
- "hospital design system"
- "radiology interface"
- "clinical dashboard"

**Figma → WPF 토큰 추출**:
1. Figma의 Local Variables / Styles에서 색상 추출
2. `CoreTokens.xaml`의 `HnVue.Core.Color.*` 키와 매핑
3. 차이가 있으면 CoreTokens.xaml을 기준으로 Figma 조정 (Figma가 종속)

---

## 3. 단계별 변환 워크플로우

### Phase 1: 디자인 생성 (Pencil/Figma)

**체크리스트**:
- [ ] HnVue 색상 팔레트 적용
- [ ] 다크 모드 기준으로 작업
- [ ] 폰트: Segoe UI, 크기: 10/11/13/16/20/28px
- [ ] 스페이싱: 4/8/12/16/24/32px 기준
- [ ] 모든 버튼/클릭 대상: 최소 44×44px
- [ ] Acquisition 화면: Emergency Stop 빨간 버튼 항상 표시
- [ ] 환자 ID: 28px Display 폰트, 눈에 띄는 위치

**산출물**: `.pen` 파일 또는 Figma 프레임 + PNG 익스포트

---

### Phase 2: 디자인 검토 (Review Gate)

**검토 항목**:

| 항목 | 검토자 | 확인 |
|------|--------|------|
| 색상 대비 ≥ 4.5:1 | 개발자 | [ ] |
| Safety 색상 IEC 62366 준수 | QA | [ ] |
| Emergency Stop 위치 (Acquisition) | QA/RA | [ ] |
| 환자 ID 표시 위치 | 방사선사 대표 | [ ] |
| MahApps.Metro 컨트롤 매핑 가능 여부 | 개발자 | [ ] |
| 44×44px 터치 타겟 | 개발자 | [ ] |

**승인 방식**: Gitea PR에 디자인 PNG 첨부 → 검토자 Approval

---

### Phase 3: 디자인 토큰 추출

**Pencil에서 추출할 정보**:
1. 사용된 모든 색상 → `CoreTokens.xaml`의 키와 1:1 매핑 확인
2. 폰트 사이즈, 굵기 → `HnVue.Core.FontSize.*` 키 매핑
3. 여백/패딩 값 → `HnVue.Core.Spacing.*` 키 매핑
4. 모서리 반경 → `HnVue.Core.CornerRadius.*` 키 매핑

**매핑 테이블 예시**:

| Pencil 색상 | CoreTokens 키 | XAML Brush 키 |
|------------|--------------|--------------|
| #1B4F8A (버튼) | HnVue.Core.Color.ButtonPrimary | HnVue.Core.Brush.ButtonPrimary |
| #D50000 (Emergency) | HnVue.Core.Color.StatusEmergency | HnVue.Core.Brush.StatusEmergency |
| #00C853 (Safe) | HnVue.Core.Color.StatusSafe | HnVue.Core.Brush.StatusSafe |

---

### Phase 4: WPF XAML 구현

**View 파일 위치**: `src/HnVue.UI/Views/{ScreenName}View.xaml`

**XAML 구조 템플릿**:

```xml
<UserControl x:Class="HnVue.UI.Views.WorklistView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mah="http://metro.mahapps.com/winfx/xaml/controls">

    <!-- ✅ 디자인 토큰만 사용 -->
    <Grid Background="{StaticResource HnVue.Core.Brush.BackgroundPage}">

        <!-- Header -->
        <Border Background="{StaticResource HnVue.Core.Brush.BackgroundPanel}"
                Padding="{StaticResource HnVue.Core.Padding.Normal}">
            <TextBlock Text="{Binding Title}"
                       FontSize="{StaticResource HnVue.Core.FontSize.Header}"
                       Foreground="{StaticResource HnVue.Core.Brush.TextPrimary}" />
        </Border>

        <!-- 환자 ID 표시 (항상 눈에 띄게) -->
        <TextBlock Text="{Binding SelectedPatientId}"
                   FontSize="{StaticResource HnVue.Core.FontSize.Display}"
                   Foreground="{StaticResource HnVue.Core.Brush.TextPrimary}" />

    </Grid>
</UserControl>
```

**Acquisition 화면 Emergency Stop 필수 패턴**:

```xml
<!-- ✅ Emergency Stop — 항상 표시, 절대 숨기지 않음 -->
<Button Content="비상 정지 / STOP"
        Background="{StaticResource HnVue.Core.Brush.StatusEmergency}"
        Foreground="White"
        FontSize="{StaticResource HnVue.Core.FontSize.Large}"
        FontWeight="Bold"
        MinWidth="88" MinHeight="44"
        Padding="16,8"
        Visibility="Visible"
        Command="{Binding EmergencyStopCommand}"
        AutomationProperties.Name="Emergency Stop"
        AutomationProperties.HelpText="Immediately stop radiation exposure" />
```

---

### Phase 5: 구현 검증

**자동 검사** (`dotnet test`):
- [ ] 아키텍처 테스트 통과 (NetArchTest)
- [ ] 단위 테스트 통과

**수동 검사**:
- [ ] High Contrast 테마 전환 후 시각적 확인
- [ ] Tab 키만으로 모든 인터랙티브 요소 접근 가능 확인
- [ ] 화면 리더기(내레이터) 레이블 확인
- [ ] 디자인 파일과 XAML 구현 일치 여부 비교

**색상 대비 검사 도구**:
- Windows 내장: 색 대비 분석기 (접근성 도구)
- Colour Contrast Analyser (무료): https://www.tpgi.com/color-contrast-checker/

---

## 4. WPF ↔ Pencil 컴포넌트 매핑

| Pencil 요소 | WPF/MahApps 대응 컴포넌트 |
|------------|--------------------------|
| Button (Primary) | `<Button Style="{StaticResource MahApps.Styles.Button.Accent}">` |
| Button (Danger) | `<Button Background="{StaticResource HnVue.Core.Brush.StatusEmergency}">` |
| Text Input | `<mah:MetroTextBox>` |
| Search Box | `<mah:MetroTextBox>` + 검색 아이콘 |
| DataGrid (Table) | `<DataGrid>` with RowStyle |
| Card/Panel | `<Border CornerRadius="...">` |
| Dialog/Modal | `<mah:MetroWindow>` 또는 `<mah:MetroDialog>` |
| Toast Notification | `<mah:Snackbar>` 또는 커스텀 Toast.xaml |
| Toggle Switch | `<mah:ToggleSwitch>` |
| Progress Bar | `<ProgressBar>` or `<mah:MetroProgressBar>` |
| Slider | `<Slider>` |
| Dropdown | `<ComboBox>` |
| Tab Control | `<TabControl Style="{StaticResource MahApps.Styles.TabControl}">` |

---

## 5. Figma Community 의료 UI 킷 활용

**활용 목적**: 디자인 영감과 검증된 의료 UI 패턴 참조 (복사 금지 — 참조만)

**조사된 리소스**: `docs/ui_mockups/FIGMA_RESOURCES.md` 참조

**활용 방법**:
1. Figma Community 킷에서 레이아웃 패턴 참조
2. 동일한 디자인 원칙을 HnVue CoreTokens를 사용하여 Pencil로 재현
3. Figma 코드 익스포트(CSS/HTML)는 참조용 — XAML로 수동 변환

---

---

## X. 구현된 뷰 현황 (2026-04-07 기준)

### PPT 명세 기반 구현 뷰 목록

| 뷰 파일 | 클래스 | ViewModel 계약 | PPT 출처 | 상태 |
|---------|------|--------------|---------|------|
| LoginView.xaml | LoginView | ILoginViewModel | 슬라이드 1 | ✅ Active |
| PatientListView.xaml | PatientListView | IPatientListViewModel | 슬라이드 4 | ✅ Active |
| StudylistView.xaml | StudylistView | IStudylistViewModel | 슬라이드 7 | ✅ Active |
| WorkflowView.xaml | WorkflowView | IWorkflowViewModel | 슬라이드 9~11 | ✅ Active |
| DoseDisplayView.xaml | DoseDisplayView | IDoseDisplayViewModel | — | ✅ Active |
| AddPatientProcedureView.xaml | AddPatientProcedureView | IAddPatientProcedureViewModel | 슬라이드 8 | ✅ Active |
| MergeView.xaml | MergeView | IMergeViewModel | 슬라이드 13 | ✅ Active |
| SettingsView.xaml | SettingsView | ISettingsViewModel | 슬라이드 14~21 | ✅ Active |
| ImageViewerView.xaml | ImageViewerView | IImageViewerViewModel | — | ✅ Active |
| CDBurnView.xaml | CDBurnView | ICDBurnViewModel | — | ✅ Active |
| SystemAdminView.xaml | SystemAdminView | ISystemAdminViewModel | — | ✅ Active |
| QuickPinLockView.xaml | QuickPinLockView | IQuickPinLockViewModel | — | ✅ Active |

### ViewMappings.xaml 등록 현황

`src/HnVue.App/DataTemplates/ViewMappings.xaml`에 모든 뷰가 DataTemplate으로 등록됨.

### 공통 컨버터 (App.xaml)

| 컨버터 키 | 클래스 | 용도 |
|---------|------|-----|
| BoolToVisibilityConverter | BoolToVisibilityConverter | bool → Visibility |
| NullToVisibilityConverter | NullToVisibilityConverter | null 체크 → Visibility |
| InverseBoolConverter | InverseBoolConverter | bool 반전 |
| ActiveTabToVisibilityConverter | ActiveTabToVisibilityConverter | Settings 탭 전환 |
| StringEqualityToBoolConverter | StringEqualityToBoolConverter | 탭 IsChecked 바인딩 |

### CoreTokens 색상 기준 (PPT 슬라이드 4)

| 토큰 | 색상 | 변경 이력 |
|------|------|---------|
| BackgroundPage | #242424 | 변경: #1A1A2E (2026-04-07) |
| BackgroundPanel | #2A2A2A | 변경: #16213E (2026-04-07) |
| BackgroundCard | #3B3B3B | 변경: #0F3460 (2026-04-07) |
| Border | #3B3B3B | 변경: #2E4A6E (2026-04-07) |

---

버전: 1.1 | 2026-04-07
