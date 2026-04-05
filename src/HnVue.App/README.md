# HnVue.App

> WPF 애플리케이션 진입점 (Composition Root)

## 목적

HnVue Console SW의 메인 실행 프로젝트입니다. WPF `App.xaml`과 `MainWindow.xaml`을 포함하며, 모든 모듈의 DI 등록과 애플리케이션 수명 주기를 관리합니다.

IEC 62304 §5.1.1 — 중앙집중형 Composition Root 패턴.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `App` | WPF Application 클래스 — `IHost` 구성, 13개 모듈 DI 등록, 수명 주기 관리 |
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

### Stubs/ 디렉토리 (Wave 3 통합 전 임시 구현체)

| 타입 | 대상 인터페이스 | 비고 |
|------|--------------|------|
| `StubImageProcessor` | `IImageProcessor` | `Rotate`, `Flip` 포함 — Wave 3 대체 예정 |
| `StubWorkflowEngine` | `IWorkflowEngine` | `PrepareExposureAsync` 포함 — Wave 3 대체 예정 |
| `StubPatientService` | `IPatientService` | 검색 빈 목록, 변이 실패 반환 |
| `StubCDDVDBurnService` | `ICDDVDBurnService` | 모든 작업 실패 반환 |
| `StubSystemAdminService` | `ISystemAdminService` | 설정 읽기 빈 값 반환 |
| `StubDoseService` | `IDoseService` | 모든 작업 실패 반환 |

## UI 레이아웃

### App.xaml — 전역 리소스

- MahApps.Metro `Dark.Steel` 테마 병합
- HnVue.UI `HnVueTheme.xaml` 병합
- **전역 컨버터 등록** (Issue #16):
  - `BooleanToVisibilityConverter` (WPF 기본)
  - `BoolToVisibilityConverter` (`HnVue.UI.Converters` — `xmlns:converters`)
  - `NullToVisibilityConverter` (`HnVue.UI.Converters`)

### MainWindow.xaml — 3컬럼 레이아웃

- **헤더 (Row 0, 48px):** 타이틀 + 우측에 사용자 정보 / 로그아웃 / **EMERGENCY 버튼** (빨간, `EmergencyCommand` 바인딩, SWR-NF-UX-026)
- **메인 컨텐츠 (Row 1):**
  - 왼쪽 (280px): `PatientListView`
  - 중앙 (`*`): `ImageViewerView`
  - 오른쪽 (260px): `WorkflowView` + `DoseDisplayView`
  - 로그인 오버레이: `LoginView` (`IsLoginVisible` 바인딩)
  - **TLS 비활성 황색 배너** (`IsTlsInactive` 바인딩, `BoolToVisibilityConverter`, SWR-CS-079)
  - **세션 카운트다운 배너** (`IsTimeoutWarningVisible` + `SessionTimeoutCountdown` 바인딩, SWR-CS-075)
- **푸터 (Row 2, 28px):** `StatusBar` + 세션 타임아웃 배너 (Panel.ZIndex=100)

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Dicom`
- `HnVue.Workflow`
- `HnVue.Imaging`
- `HnVue.Dose`
- `HnVue.PatientManagement`
- `HnVue.Incident`
- `HnVue.Update`
- `HnVue.CDBurning`
- `HnVue.SystemAdmin`
- `HnVue.UI`

### NuGet 패키지

- `Microsoft.Extensions.Hosting` (IHost 기반 DI)
- `MahApps.Metro` (MetroWindow)

## DI 등록

Composition Root — `App.BuildHost()` 에서 모든 모듈 서비스 등록:

| 모듈 | 등록 방법 |
|------|---------|
| `HnVue.Common` | `AddHnVueCommon()` |
| `HnVue.Data` | `AddHnVueData(connectionString)` |
| `HnVue.Security` | `AddHnVueSecurity(jwtOptions, auditOptions)` |
| `HnVue.Workflow` | `GeneratorSimulator`, `WorkflowEngine` 직접 등록 |
| `HnVue.Dose` | `NullDoseRepository` + `DoseService` 직접 등록 |
| `HnVue.PatientManagement` | `NullWorklistRepository` + `PatientService`, `WorklistService` 직접 등록 |
| `HnVue.Incident` | `NullIncidentRepository` + `IncidentResponseService` 직접 등록 |
| `HnVue.Update` | `NullUpdateRepository` + `BackupService`, `SWUpdateService` 직접 등록 |
| `HnVue.SystemAdmin` | `NullSystemSettingsRepository` + `SystemAdminService` 직접 등록 |
| `HnVue.CDBurning` | `IMAPIComWrapper`, `NullCdStudyRepository`, `CDDVDBurnService` 직접 등록 |
| `HnVue.Dicom` | `DicomNetworkConfig`, `DicomStoreScu`, `DicomFindScu`, `DicomFileIO` 직접 등록 |
| `HnVue.Imaging` | `ImageProcessor` 직접 등록 |
| `HnVue.UI` | ViewModel 6종 (Transient) + `MainWindow` (Singleton) |

## 비고

- `OutputType: WinExe` (WPF 애플리케이션)
- 모든 src 프로젝트를 참조하는 유일한 프로젝트
- JWT/HMAC 비밀키는 `appsettings.json`(Security 섹션) 또는 환경변수(`HNVUE_JWT_SECRET`, `HNVUE_AUDIT_HMAC_KEY`) 로드 — 미설정 시 시작 실패
- `appsettings.Development.json`: 개발용 플레이스홀더 (git 추적 제외 — 자격증명 보호)
- 테스트 프로젝트 없음 (Composition Root은 Integration Test 대상)
