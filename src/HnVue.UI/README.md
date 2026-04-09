# HnVue.UI

> WPF View, 테마, 컨버터, 재사용 컴포넌트

## 목적

WPF 화면, 리소스 딕셔너리, 재사용 컴포넌트, 컨버터를 제공합니다.
현재 저장소 기준으로 **구체 ViewModel 구현은 `src/HnVue.UI.ViewModels/`**, **인터페이스 계약은 `src/HnVue.UI.Contracts/`**에 분리되어 있으며,
`src/HnVue.UI/`는 View 계층과 디자인 시스템 리소스에 집중합니다.

---

## 프로젝트 경계

| 프로젝트 | 현재 역할 |
|---|---|
| `HnVue.UI` | WPF View, Themes, Styles, Components, Converters |
| `HnVue.UI.Contracts` | ViewModel 인터페이스, 이벤트 메시지, 네비게이션, 테마 계약 |
| `HnVue.UI.ViewModels` | CommunityToolkit.Mvvm 기반 구체 ViewModel 구현 |

### `HnVue.UI.ViewModels`의 대표 구현

- `MainViewModel`, `LoginViewModel`, `PatientListViewModel`, `ImageViewerViewModel`
- `WorkflowViewModel`, `DoseDisplayViewModel`, `DoseViewModel`
- `CDBurnViewModel`, `SystemAdminViewModel`, `QuickPinLockViewModel`
- `AddPatientProcedureViewModel`, `MergeViewModel`, `SettingsViewModel`, `StudylistViewModel`

이 README는 `src/HnVue.UI/` 프로젝트 범위를 설명하며, 구체 ViewModel 동작은 별도 프로젝트(`src/HnVue.UI.ViewModels/`) 기준으로 해석해야 합니다.

---

## View 목록

| View | 파일 | 설명 |
|---|---|---|
| `AddPatientProcedureView` | `AddPatientProcedureView.xaml` | 환자 추가/검사 절차 입력 화면 |
| `CDBurnView` | `CDBurnView.xaml` | CD/DVD 굽기 인터페이스 |
| `DoseDisplayView` | `DoseDisplayView.xaml` | 선량 실시간 표시 |
| `ImageViewerView` | `ImageViewerView.xaml` | 방사선 영상 뷰어 |
| `LoginView` | `LoginView.xaml` + `LoginView.xaml.cs` | 로그인 화면. `PasswordBox_PasswordChanged` code-behind, `IsLoading` ProgressBar (Issue #16) |
| `MergeView` | `MergeView.xaml` | 환자/스터디 병합 화면 |
| `PatientListView` | `PatientListView.xaml` | 환자 목록. `SelectPatientCommand` 바인딩, `IsEmergency` 배지, `IsLoading` ProgressBar (Issue #15, #16) |
| `QuickPinLockView` | `QuickPinLockView.xaml` + `QuickPinLockView.xaml.cs` | PIN 입력 오버레이. 워크플로우 세션 중 잠금화면 |
| `SettingsView` | `SettingsView.xaml` | 설정 탭 기반 관리 화면 |
| `StudylistView` | `StudylistView.xaml` | 스터디 목록 화면 |
| `SystemAdminView` | `SystemAdminView.xaml` | 시스템 관리 |
| `WorkflowView` | `WorkflowView.xaml` | 워크플로우 상태 및 노출 제어 |

### 재사용 컴포넌트 및 테마 리소스

- `Components/Common`: `Header.xaml`, `StatusBar.xaml`, `Modal.xaml`, `Toast.xaml`
- `Components/ImageViewer`: `AcquisitionPreview.xaml`, `PatientInfoCard.xaml`, `StudyThumbnail.xaml`
- `Themes`: `HnVueTheme.xaml`, `DesignSystem2026.xaml`, `ComponentLibrary.xaml`

---

## 컨버터 목록

### 전역 리소스 등록 (App.xaml)

| 키 | 클래스 | 설명 |
|---|---|---|
| `BooleanToVisibilityConverter` | WPF 내장 (`System.Windows.Controls`) | `true` → `Visible`, `false` → `Collapsed` |
| `BoolToVisibilityConverter` | `HnVue.UI.Converters.BoolToVisibilityConverter` | HnVue 커스텀. `IsInverted` 프로퍼티 및 `ConverterParameter="Invert"` 지원 |
| `NullToVisibilityConverter` | `HnVue.UI.Converters.NullToVisibilityConverter` | HnVue 커스텀. `null` → `Collapsed`, 비null → `Visible` |
| `InverseBoolConverter` | `HnVue.UI.Converters.InverseBoolConverter` | `bool` 값을 반전하여 `IsEnabled` 같은 바인딩에 사용 |
| `ActiveTabToVisibilityConverter` | `HnVue.UI.Converters.ActiveTabToVisibilityConverter` | Settings 탭 콘텐츠 토글 |
| `StringEqualityToBoolConverter` | `HnVue.UI.Converters.StringEqualityToBoolConverter` | Settings 토글 버튼 선택 상태 |

### 프로젝트 내 컨버터 구현

- `BoolToVisibilityConverter`
- `NullToVisibilityConverter`
- `InverseBoolConverter`
- `SafeStateToColorConverter`
- `InvertedBoolToVisibilityConverter`
- `NullToCollapsedConverter`
- `CountToVisibilityConverter`
- `StringToVisibilityConverter`
- `StatusToBrushConverter`
- `ActiveTabToVisibilityConverter`
- `StringEqualityToBoolConverter`
- `MultiBoolAndConverter`
- `MultiBoolOrConverter`

### BoolToVisibilityConverter 상세

```csharp
[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }
    // ConverterParameter="Invert" 로도 반전 가능
}
```

- `true` → `Visible`, `false` → `Collapsed` (기본)
- `IsInverted = true` 또는 `ConverterParameter="Invert"` → 반전
- 양방향 바인딩 미지원 (`ConvertBack` throws `NotSupportedException`)

### NullToVisibilityConverter 상세

```csharp
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityConverter : IValueConverter
```

- `null` → `Collapsed`, 비null → `Visible`
- 양방향 바인딩 미지원

---

## 테스트

- `tests/HnVue.UI.Tests`
- 검증 파일: `MainViewModelTests.cs`, `LoginViewModelTests.cs`, `PatientListViewModelTests.cs`, `ImageViewerViewModelTests.cs`, `WorkflowViewModelTests.cs`, `DoseDisplayViewModelTests.cs`, `DoseViewModelTests.cs`, `CDBurnViewModelTests.cs`, `SystemAdminViewModelTests.cs`, `QuickPinLockViewModelTests.cs`, `LoginSuccessEventArgsTests.cs`, `ComponentTests.cs`
- 보조 QA 스위트: `tests/UI`에서 접근성, 디자인 시스템, 성능, PPT 시안 정합성을 별도로 검증

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 역할 |
|---|---|
| `HnVue.Common` | 인터페이스, 모델, Result 패턴 |
| `HnVue.UI.Contracts` | ViewModel 인터페이스, 이벤트/네비게이션 계약 |

### NuGet 패키지

| 패키지 | 용도 |
|---|---|
| `CommunityToolkit.Mvvm` | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `MahApps.Metro` | Modern WPF 테마 및 컨트롤 |
| `LiveChartsCore.SkiaSharpView.WPF` | 선량 통계 차트 (LiveCharts2) |

---

## DI 등록

없음 (`HnVue.UI`는 View/Resource 프로젝트이며, DI는 `HnVue.App`에서 `HnVue.UI.Contracts`와 `HnVue.UI.ViewModels` 기준으로 수행)

---

## 비고

- `src/HnVue.UI/`에는 현재 구체 ViewModel 소스 파일이 없으며, UI 계층은 `HnVue.UI.ViewModels`에 직접 의존하지 않습니다.
- `WorkflowView.xaml`은 `SafeStateToColorConverter`를 로컬 리소스로 등록해 안전 상태 색상을 표시합니다.
- `SettingsView.xaml`은 탭 전환용 컨버터(`ActiveTabToVisibilityConverter`, `StringEqualityToBoolConverter`)를 사용합니다.
