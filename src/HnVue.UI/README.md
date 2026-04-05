# HnVue.UI

> WPF UI 컴포넌트 (MVVM, ViewModel, View, 컨버터)

## 목적

WPF UI 인프라스트럭처를 제공합니다.  
CommunityToolkit.Mvvm 기반 MVVM 패턴, MahApps.Metro 테마, LiveCharts2 차트 컴포넌트, 전체 ViewModel/View 계층, 커스텀 컨버터를 포함합니다.

---

## ViewModel 목록

| ViewModel | 기반 클래스 | 설명 |
|---|---|---|
| `MainViewModel` | `ObservableObject`, `IDisposable` | 최상위 셸 ViewModel. 세션 타임아웃 타이머, 네비게이션, Emergency/Logout 커맨드 |
| `LoginViewModel` | `ObservableObject` | 로그인 처리. JWT 인증 + `LoginSucceeded` 이벤트 발행 |
| `PatientListViewModel` | `ObservableObject` | 환자 목록 조회, `SelectPatientCommand`, IsEmergency 배지, IsLoading 상태 |
| `ImageViewerViewModel` | `ObservableObject` | 영상 뷰어. `BitmapSource ImageSource`, `BuildBitmapSource()` (Gray8 WriteableBitmap) |
| `WorkflowViewModel` | `ObservableObject` | 워크플로우 상태 표시 및 제어 |
| `DoseDisplayViewModel` | `ObservableObject` | 방사선 선량 실시간 표시 |
| `DoseViewModel` | `ObservableObject` | 선량 통계 차트 ViewModel |
| `CDBurnViewModel` | `ObservableObject` | CD/DVD 소각 ViewModel |
| `SystemAdminViewModel` | `ObservableObject` | 시스템 관리 (설정, 감사 로그 내보내기) |
| `QuickPinLockViewModel` | `ObservableObject` | Quick PIN 잠금 오버레이. 3회 실패 시 ForceLogout (SWR-CS-076) |

### MainViewModel 상세

```csharp
// IDisposable: System.Timers.Timer 해제
// 자식 ViewModel: PatientListViewModel, ImageViewerViewModel, WorkflowViewModel,
//                 DoseDisplayViewModel, CDBurnViewModel, SystemAdminViewModel (Issue #17)
```

| 프로퍼티 / 커맨드 | 타입 | 설명 |
|---|---|---|
| `IsTlsInactive` | `bool` | TLS 비활성 경고 배너 표시 여부 (SWR-CS-079, Issue #13) |
| `IsTimeoutWarningVisible` | `bool` | 세션 타임아웃 3분 전 경고 배너 표시 (SWR-CS-075, Issue #14) |
| `SessionTimeoutCountdown` | `int` | 남은 초 수. 1초 주기 `System.Timers.Timer` 업데이트 (SWR-CS-075) |
| `EmergencyCommand` | `IRelayCommand` | 응급 환자 등록 워크플로우 진입 (SWR-NF-UX-026, Issue #11) |
| `LogoutCommand` | `IRelayCommand` | 세션 종료 및 로그인 화면 복귀 |
| `NavigateCommand` | `IRelayCommand<string>` | 네비게이션 항목 이동 |

### ImageViewerViewModel 상세

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `ImageSource` | `BitmapSource?` | WPF Image 컨트롤에 바인딩되는 이미지 소스 (SWR-IP-020, Issue #10) |
| `WindowLevel` | `double` | DICOM 윈도우 센터. 변경 시 `ApplyWindowLevel()` 자동 호출 |
| `WindowWidth` | `double` | DICOM 윈도우 너비. 변경 시 `ApplyWindowLevel()` 자동 호출 |
| `ZoomFactor` | `double` | 현재 줌 배율 (1.0 = 100%) |
| `IsBusy` | `bool` | 이미지 로딩/처리 중 상태 |

```
BuildBitmapSource(ProcessedImage image) → BitmapSource
// PixelData를 8비트 그레이스케일 WriteableBitmap으로 변환
// PixelFormats.Gray8, stride = width × 1, Freeze() for cross-thread access
```

### QuickPinLockViewModel 상세 (SWR-CS-076, Issue #12)

| 프로퍼티 / 이벤트 | 타입 | 설명 |
|---|---|---|
| `Pin` | `string` | 입력된 PIN (4~6자리) |
| `RemainingAttempts` | `int` | 남은 시도 횟수 (최대 3회) |
| `IsVerifying` | `bool` | PIN 검증 진행 중 상태 |
| `LockedUsername` | `string?` | 잠금된 사용자명 |
| `SessionResumed` | `event EventHandler?` | PIN 인증 성공 시 발생 |
| `ForceLogout` | `event EventHandler?` | 3회 실패 시 발생 → 전체 로그아웃 |

---

## View 목록

| View | 파일 | 설명 |
|---|---|---|
| `LoginView` | `LoginView.xaml` + `LoginView.xaml.cs` | 로그인 화면. `PasswordBox_PasswordChanged` code-behind, `IsLoading` ProgressBar (Issue #16) |
| `PatientListView` | `PatientListView.xaml` | 환자 목록. `SelectPatientCommand` 바인딩, `IsEmergency` 배지, `IsLoading` ProgressBar (Issue #15, #16) |
| `WorkflowView` | `WorkflowView.xaml` | 워크플로우 상태 및 노출 제어 |
| `ImageViewerView` | `ImageViewerView.xaml` | 방사선 영상 뷰어 |
| `DoseDisplayView` | `DoseDisplayView.xaml` | 선량 실시간 표시 |
| `CDBurnView` | `CDBurnView.xaml` | CD/DVD 소각 인터페이스 |
| `SystemAdminView` | `SystemAdminView.xaml` | 시스템 관리 |
| `QuickPinLockView` | `QuickPinLockView.xaml` + `QuickPinLockView.xaml.cs` | PIN 입력 오버레이. 워크플로우 세션 중 잠금화면 |

---

## 컨버터 목록

### 전역 리소스 등록 (App.xaml)

| 키 | 클래스 | 설명 |
|---|---|---|
| `BooleanToVisibilityConverter` | WPF 내장 (`System.Windows.Controls`) | `true` → `Visible`, `false` → `Collapsed` |
| `BoolToVisibilityConverter` | `HnVue.UI.Converters.BoolToVisibilityConverter` | HnVue 커스텀. `IsInverted` 프로퍼티 및 `ConverterParameter="Invert"` 지원 |
| `NullToVisibilityConverter` | `HnVue.UI.Converters.NullToVisibilityConverter` | HnVue 커스텀. `null` → `Collapsed`, 비null → `Visible` |

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

| 파일 | 테스트 수 | 내용 |
|---|---|---|
| `MainViewModelTests.cs` | 8 | 세션 타임아웃, 네비게이션, Logout, IDisposable |
| `LoginViewModelTests.cs` | 14 | 인증 성공/실패, 잠금 계정, 이벤트 발행 |
| `PatientListViewModelTests.cs` | 7 | 환자 검색, SelectPatient, IsLoading |
| `ImageViewerViewModelTests.cs` | 8 | 이미지 로드, Zoom, Window/Level, BitmapSource |
| `WorkflowViewModelTests.cs` | 9 | 상태 전이, 노출 커맨드 |
| `DoseDisplayViewModelTests.cs` | 8 | 선량 표시, 실시간 업데이트 |
| `DoseViewModelTests.cs` | 12 | 선량 통계, 차트 데이터 |
| `CDBurnViewModelTests.cs` | 10 | 소각 시작/완료/오류 |
| `SystemAdminViewModelTests.cs` | 8 | 설정 조회/저장, 감사 로그 내보내기 |
| `LoginSuccessEventArgsTests.cs` | 2 | 이벤트 인자 속성 |

**전체 테스트: 86개**

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 역할 |
|---|---|
| `HnVue.Common` | 인터페이스, 모델, Result 패턴 |

### NuGet 패키지

| 패키지 | 용도 |
|---|---|
| `CommunityToolkit.Mvvm` | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` |
| `MahApps.Metro` | Modern WPF 테마 및 컨트롤 |
| `LiveChartsCore.SkiaSharpView.WPF` | 선량 통계 차트 (LiveCharts2) |

---

## DI 등록

없음 (모든 ViewModel은 `HnVue.App`에서 DI 등록)

---

## 비고

- `MainViewModel`이 `IDisposable`을 구현하여 `System.Timers.Timer` 정리
- `ImageViewerViewModel.BuildBitmapSource()`는 `BitmapSource.Freeze()`를 호출하여 크로스 스레드 접근 안전
- `QuickPinLockView` + `QuickPinLockViewModel`은 Issue #12에서 추가 (SWR-CS-076)
- `CDBurnViewModel`, `SystemAdminViewModel`은 Issue #17에서 `MainViewModel` 네비게이션 그래프에 추가
