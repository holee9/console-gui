# HnVue.App

> WPF 애플리케이션 진입점 (Composition Root)

## 목적

HnVue Console SW의 메인 실행 프로젝트입니다. WPF `App.xaml`과 `MainWindow.xaml`을 포함하며, 모든 모듈의 DI 등록과 애플리케이션 수명 주기를 관리합니다.

IEC 62304 §5.1.1 — 중앙집중형 Composition Root 패턴.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `App` | WPF Application 클래스 — `IHost` 구성, 각 모듈 및 UI 계약/ViewModel DI 등록, 수명 주기 관리 |
| `MainWindow` | 메인 윈도우 — `MetroWindow` 기반, `MainViewModel` DataContext 주입 및 로그인 이벤트 연결 |

### Phase 1d 인라인 Null 리포지토리 (App.xaml.cs 내부 sealed class)

| 타입 | 대상 인터페이스 | 설명 |
|------|--------------|------|
| `NullDoseRepository` | `IDoseRepository` | EF 구현 전 임시 스텁 |
| `NullWorklistRepository` | `IWorklistRepository` | 빈 목록 반환 |
| `NullIncidentRepository` | `IIncidentRepository` | 저장 성공 반환 |
| `NullUpdateRepository` | `IUpdateRepository` | 업데이트 없음 반환 |
| `NullSystemSettingsRepository` | `ISystemSettingsRepository` | 메모리 in-process 저장 |
| `NullCdStudyRepository` | `HnVue.CDBurning.IStudyRepository` | 빈 파일 목록 반환 |

### App.xaml.cs 직접 등록 대체 구현 (Wave 3 통합 전)

| 타입 | 대상 인터페이스 | 비고 |
|------|--------------|------|
| `GeneratorSimulator` | `IGeneratorInterface` | 기본 제너레이터 시뮬레이터 |
| `DetectorSimulator` | `IDetectorInterface` | 기본 검출기 시뮬레이터 |
| `IMAPIComWrapper` | `IBurnSession` | Phase 1d CD 굽기 세션 래퍼 |

## UI 레이아웃

### App.xaml — 전역 리소스

- MahApps.Metro `Dark.Steel` 테마 병합
- HnVue.UI `HnVueTheme.xaml` 병합
- `DataTemplates/ViewMappings.xaml` 병합
- **전역 컨버터 등록**:
  - `BooleanToVisibilityConverter` (WPF 기본)
  - `BoolToVisibilityConverter` (`HnVue.UI.Converters` — `xmlns:converters`)
  - `NullToVisibilityConverter` (`HnVue.UI.Converters`)
  - `InverseBoolConverter` (`HnVue.UI.Converters`)
  - `ActiveTabToVisibilityConverter`, `StringEqualityToBoolConverter`

### MainWindow.xaml — 현재 셸 레이아웃

- **헤더 (Row 0, 30px):** 좌측 브랜딩, 중앙 네비게이션, 우측 사용자/세션 액션
- **메인 컨텐츠 (Row 1):**
  - 기본 컬럼 구성은 `* / 0 / 0`
  - 좌측 가시 영역: `PatientListView`
  - 가운데/우측 상세 패널은 현재 width `0`으로 시작하며 `ImageViewerView`, `WorkflowView`, `DoseDisplayView` 호스팅용으로 유지
  - 로그인 오버레이: `LoginView` (`IsLoginVisible` 바인딩)
  - TLS 비활성 경고 배너 (`IsTlsInactive`)
  - 세션 만료 경고 배너 (`IsTimeoutWarningVisible`, `SessionTimeoutCountdown`)
- **푸터 (Row 2, 28px):** `StatusBar`

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Dicom`
- `HnVue.Workflow`
- `HnVue.Detector`
- `HnVue.Imaging`
- `HnVue.Dose`
- `HnVue.PatientManagement`
- `HnVue.Incident`
- `HnVue.Update`
- `HnVue.CDBurning`
- `HnVue.SystemAdmin`
- `HnVue.UI`
- `HnVue.UI.Contracts`
- `HnVue.UI.ViewModels`

### NuGet 패키지

- `Microsoft.Extensions.Hosting` (IHost 기반 DI)
- `Microsoft.Extensions.Configuration` (구성 로드)

## DI 등록

Composition Root — `App.BuildHost()` 에서 모든 모듈 서비스 등록:

| 모듈 | 등록 방법 |
|------|---------|
| `HnVue.Common` | `AddHnVueCommon()` |
| `HnVue.Data` | `AddHnVueData(connectionString)` |
| `HnVue.Security` | `AddHnVueSecurity(jwtOptions, auditOptions)` |
| `HnVue.Workflow` | `GeneratorSimulator`, `DetectorSimulator`, `WorkflowEngine` 직접 등록 |
| `HnVue.Dose` | `NullDoseRepository` + `DoseService` 직접 등록 |
| `HnVue.PatientManagement` | `NullWorklistRepository` + `PatientService`, `WorklistService` 직접 등록 |
| `HnVue.Incident` | `NullIncidentRepository` + `IncidentResponseService` 직접 등록 |
| `HnVue.Update` | `NullUpdateRepository` + `BackupService`, `SWUpdateService` 직접 등록 |
| `HnVue.SystemAdmin` | `NullSystemSettingsRepository` + `SystemAdminService` 직접 등록 |
| `HnVue.CDBurning` | `IMAPIComWrapper`, `NullCdStudyRepository`, `CDDVDBurnService` 직접 등록 |
| `HnVue.Dicom` | `DicomNetworkConfig`, `DicomStoreScu`, `DicomFileIO` 직접 등록 |
| `HnVue.Imaging` | `ImageProcessor` 직접 등록 |
| `HnVue.UI` | `ILoginViewModel`, `IPatientListViewModel`, `IImageViewerViewModel`, `IWorkflowViewModel`, `IDoseDisplayViewModel`, `IDoseViewModel`, `ICDBurnViewModel`, `ISystemAdminViewModel`, `IQuickPinLockViewModel`, `IMainViewModel` + 일부 concrete ViewModel + `MainWindow` 등록 |

## 비고

- `OutputType: WinExe` (WPF 애플리케이션)
- 모든 src 프로젝트를 참조하는 유일한 프로젝트
- JWT/HMAC 비밀키는 `appsettings.json`(Security 섹션) 또는 환경변수(`HNVUE_JWT_SECRET`, `HNVUE_AUDIT_HMAC_KEY`) 로드 — 미설정 시 시작 실패
- `src/HnVue.App/appsettings.Development.json`: 개발용 플레이스홀더 (현재 저장소에 추적되며, 프로덕션 자격증명 저장 금지)
- 테스트 프로젝트 없음 (Composition Root은 Integration Test 대상)
