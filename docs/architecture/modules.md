# HnVue 모듈 상세 설명 (15개)

> 원본: README.md "모듈 상세 설명" 섹션에서 분리 (2026-04-09)

## Layer 0: 공유 인터페이스

### HnVue.Common
기초 모델, 인터페이스, `Result<T>` Monad, Enum 정의. 모든 모듈이 참조합니다.

**핵심 항목:**
- `Result<T>` Monad / `Result.Success()`, `Result.Failure()`, `Result.SuccessNullable<T>()`

  > **`Result<T>` Monad란?**
  > 메서드가 성공 또는 실패를 명시적으로 반환하는 패턴이다. C#의 예외(Exception)를 던지는 대신, 성공이면 `Result.Success(value)`, 실패면 `Result.Failure(errorCode, message)`를 반환한다.
  > Railway-Oriented Programming 기법으로, 에러를 열차 선로처럼 두 갈래(성공/실패)로 분리해 처리한다. `throw`/`try-catch` 없이 에러 흐름을 컴파일 타임에 강제할 수 있어 의료기기 소프트웨어의 신뢰성에 적합하다.

- `ErrorCode` Enum (9개 도메인: Security, Workflow, DICOM, Dose, Incident, Update, PatientMgmt, System, CDBurning)
- `SafeState` Enum (장치 상태)
- `UserRole` Enum (Radiographer, Radiologist, Admin, Service)
- `WorkflowState` Enum (9-상태: Idle, PatientSelected, ProtocolLoaded, ReadyToExpose, Exposing, AcquiringImage, ImageAcquired, TransmittingToCACS, Complete)
- `GeneratorState` Enum (장치 제너레이터 상태)
- `DetectorState` Enum (Disconnected / Idle / Armed / Acquiring / ImageReady / Error)
- `DetectorTriggerMode` Enum (Sync / FreeRun)
- `IncidentSeverity` Enum (Critical, High, Medium, Low)
- 17개 서비스 인터페이스 (ISecurityService, IDataService 등)
- 17개 DTO (PatientDto, StudyDto 등)
- `ThreadLocalSecurityContext` (ReaderWriterLockSlim 기반 스레드 안전성)

**테스트:** 38개 (`HnVue.Common.Tests`)

---

## Layer 1: 데이터 접근

### HnVue.Data
EF Core 8 + SQLite + SQLCipher AES-256 암호화. Repository 패턴 구현.

**핵심 항목:**
- `HnVueDbContext` (6개 Entity: Patient, Study, Image, DoseRecord, User, AuditLog)
- `PatientRepository`, `StudyRepository`, `ImageRepository`, `UserRepository` (IAsyncRepository<T> 구현)
- `OperationCanceledException` 재발생 처리
- Connection string: `Data Source={appDataPath}/hnvue.db; Password={key};` (SQLCipher)

**테스트:** 69개 (`HnVue.Data.Tests`)
**커버리지:** 80%+

---

## Layer 2: 인증 및 보안

### HnVue.Security (안전 임계 90%+)
비밀번호 해싱, JWT 토큰, RBAC, 감사 로그 체인.

**핵심 항목:**
- `PasswordHasher` (bcrypt cost=12, ~300ms 해싱 시간)
- `JwtTokenService` (HS256, 15분 만료)
- `RbacPolicy` (4역할 권한 매트릭스: Radiographer < Radiologist < Admin < Service)
- `AuditOptions` (HMAC 키 IOptions 외부화)
- `AuditService` (HMAC-SHA256 해시 체인)
- `SecurityContext` (ReaderWriterLockSlim)

**테스트:** 67개 단위 (`HnVue.Security.Tests`)
**커버리지:** 90%+

---

## Layer 3a: DICOM 통신

### HnVue.Dicom
DICOM 3.0 표준 준수. C-STORE SCU (영상 전송), C-FIND SCU (Worklist 조회), 파일 I/O.

**핵심 항목:**
- `DicomStoreScu` (C-STORE Service Class User)
- `DicomFindScu` (C-FIND Service Class User)
- `DicomFileIO` (파일 I/O)
- `DicomFileWrapper` (래퍼 클래스)

**의존성:** fo-dicom 5.2.5 (MIT 라이선스)
**테스트:** 15개 (`HnVue.Dicom.Tests`)
**커버리지:** 80%+

---

## Layer 3b: 인시던트 대응 (안전 임계 90%+)

### HnVue.Incident
위험 이벤트 감지 및 대응. ISO 14971 위험 관리 준수.

**핵심 항목:**
- `IncidentResponseService` (4단계 심각도: Critical/High/Medium/Low)
- 자동 에스컬레이션 (24시간 미반응 시 상위 심각도)
- 긴급 콜백 (전화/SMS 알림, 향후 확장)

**테스트:** 13개 (`HnVue.Incident.Tests`)
**커버리지:** 90%+

---

## Layer 3c: 소프트웨어 업데이트 (안전 임계 85%+)

### HnVue.Update
무결성 검증, 백업/복원, 코드 서명.

**핵심 항목:**
- `SWUpdateService` (업데이트 관리: 다운로드, SHA-256 검증, 백업, 설치)
- `CodeSignVerifier` (SHA-256 코드 서명 검증)
- `BackupService` (Timestamp 기반 백업/복원)

**테스트:** 25개 (`HnVue.Update.Tests`)
**커버리지:** 85%+

---

## Layer 3d: 방사선 선량 관리 (안전 임계 90%+)

### HnVue.Dose
IEC 60601-1-3 준수. 4단계 인터록 시스템.

**핵심 항목:**
- `DoseService` (ALLOW/WARN/BLOCK/EMERGENCY 4단계 인터록)
- `DoseValidationLevel` Enum
- 환자별 누적 선량 추적, 프로토콜별 기준값 적용, 실시간 모니터링

**테스트:** 17개 (`HnVue.Dose.Tests`)
**커버리지:** 90%+

---

## Layer 3e: 영상 처리

### HnVue.Imaging
영상 처리 파이프라인 (외부 SDK 연동 대기).

**현재 상태:** 스텁
**향후 구현:** Phase 1c — 외부 SDK 또는 자체 엔진 선택 예정

**테스트:** 20개 스텁 (`HnVue.Imaging.Tests`)

---

## Layer 3f: 환자 관리

### HnVue.PatientManagement
환자 등록, 조회, DICOM Worklist 통합.

**핵심 항목:**
- `PatientService` (CRUD: 환자 등록, 중복 검사, 조회/수정/삭제)
- `WorklistService` (MWL 통합: Worklist 서버 조회, 응급 ID 기반 긴급 환자 추가, 5분 간격 자동 동기화)

**테스트:** 27개 (`HnVue.PatientManagement.Tests`)
**커버리지:** 80%+

---

## Layer 3g: 시스템 관리

### HnVue.SystemAdmin
시스템 설정, 감시, 감사.

**핵심 항목:**
- `SystemAdminService` (설정 검증, 시스템 상태 모니터링, 감사 로그 CSV 내보내기, 365일 자동 정리)

**테스트:** 13개 (`HnVue.SystemAdmin.Tests`)
**커버리지:** 80%+

---

## Layer 3h: CD/DVD 소각

### HnVue.CDBurning
의료 영상 미디어 배포. IMAPI2 시뮬레이션.

**핵심 항목:**
- `CDDVDBurnService` (미디어 소각: 드라이브 감지, 영상 기록, 진행률 추적)
- `IBurnSession` (세션 관리: 활성 세션 추적, 취소 지원)
- `IMAPIComWrapper` (IMAPI2 COM 래퍼: 프로덕션=실 COM, 테스트=시뮬레이션)

**테스트:** 12개 (`HnVue.CDBurning.Tests`)
**커버리지:** 80%+

---

## Layer 3.5: 검출기 인터페이스

### HnVue.Detector
FPD (Flat Panel Detector) 검출기 통신 추상화 및 어댑터. 자사 CsI FPD SDK 연동 준비 완료.

**핵심 항목:**
- `IDetectorInterface` (ConnectAsync, DisconnectAsync, ArmAsync, AbortAsync, GetStatusAsync + 2 이벤트)
- `DetectorSimulator` — 하드웨어 없이 전체 영상 획득 흐름 검증 (12-bit 노이즈 영상 생성)
- `OwnDetectorAdapter` — 자사 프로덕션 어댑터 (SDK 도착 후 NotImplementedException 교체)
- `VendorAdapterTemplate` — 타사 SDK 어댑터 복사/수정 기준 (5가지 통합 패턴)
- `DetectorTriggerMode` — Sync (HW X-ray 트리거) / FreeRun (SW 트리거)

**SDK 배치 위치:** `sdk/own-detector/` (자사), `sdk/third-party/` (타사)
**테스트:** 11개 (`HnVue.Detector.Tests`, `SWR-WF-030`)
**커버리지:** 85%+

---

## Layer 4: 워크플로우 엔진 (안전 임계 90%+)

### HnVue.Workflow
촬영 워크플로우의 중추. 상태 머신 + 이벤트 기반.

**핵심 항목:**
- `WorkflowStateMachine` (9-상태 전이표: Idle → PatientSelected → ... → Complete)
- `WorkflowEngine` (IWorkflowEngine 구현, RBAC 강제, 검출기 ARM/Abort 통합)
- `GeneratorSerialPort` (RS-232 시리얼 통신)
- `GeneratorSimulator` (장애 주입 테스트용)

**테스트:** 64개 (`HnVue.Workflow.Tests`)
**커버리지:** 90%+

---

## Layer 5: UI (프레젠테이션) — GUI 교체 가능 아키텍처

HnVue UI 레이어는 **GUI 교체 가능 아키텍처**로 설계되어, 기능 모듈 코드 변경 없이 전체 UI를 교체할 수 있습니다.

### 3-프로젝트 분리 구조

```
HnVue.UI.Contracts  <- 인터페이스 계약 (Navigation, Dialog, Theme, ViewModel 계약)
       | (참조)
HnVue.UI.ViewModels <- ViewModel 구현 (11개 ViewModel, 인터페이스 구현)
       | (DI 등록)
HnVue.UI            <- View 전용 (XAML + code-behind, 교체 대상)
       | (참조)
HnVue.UI.Contracts  <- Views는 Contracts만 참조 (ViewModels 직접 참조 없음)
```

| 프로젝트 | 역할 | 참조 대상 |
|---------|------|----------|
| **HnVue.UI.Contracts** | UI-모듈 간 인터페이스 계약 | HnVue.Common만 |
| **HnVue.UI.ViewModels** | ViewModel 구현 (11개) | HnVue.Common + UI.Contracts |
| **HnVue.UI** | View 전용 (XAML, Converter, Theme) | HnVue.Common + UI.Contracts (**ViewModels 참조 없음**) |

### GUI 교체 방법

1. `HnVue.UI` 프로젝트를 복제하여 `HnVue.UI.V2` 생성
2. `HnVue.UI.Contracts` 인터페이스만 참조하여 새 Views 구현
3. `HnVue.App`에서 프로젝트 참조 교체 + `ViewMappings.xaml` 업데이트
4. **기능 모듈 코드 변경 0건**

### 인터페이스 계약 (HnVue.UI.Contracts)

| 카테고리 | 계약 | 설명 |
|---------|------|------|
| Navigation | `INavigationService`, `INavigationAware`, `NavigationToken` | Shell 네비게이션 |
| Dialog | `IDialogService` | 모달 다이얼로그 추상화 |
| Theming | `IThemeService`, `ThemeInfo` | 런타임 테마 전환 |
| ViewModel | `IViewModelBase` + 10개 per-feature 인터페이스 | View-ViewModel 바인딩 계약 |
| Events | `NavigationRequestedMessage`, `SessionTimeoutMessage`, `PatientSelectedMessage` | Messenger 패턴 |

### Design Token 3-Level 구조

```
src/HnVue.UI/Themes/
  tokens/
    CoreTokens.xaml          <- Level 1: Color, Font, Spacing (원시값)
    SemanticTokens.xaml      <- Level 2: Surface, Text, Status (의미 있는 이름)
    ComponentTokens.xaml     <- Level 3: DataGrid, Chart 등 (컴포넌트별)
  dark/DarkTheme.xaml        <- Dark 테마 (기본)
  light/LightTheme.xaml      <- Light 테마
  high-contrast/HighContrastTheme.xaml  <- 임상용 고대비 (IEC 62366)
  HnVueTheme.xaml            <- MergedDictionaries + 하위 호환 별칭
```

### HnVue.UI (Views)

| View | 설명 |
|------|------|
| `LoginView` | JWT 로그인 (PasswordBox code-behind) |
| `PatientListView` | 환자 검색, 선택, 등록 |
| `ImageViewerView` | 영상 표시, W/L, Zoom |
| `WorkflowView` | 촬영 워크플로우 제어, SafeState 인디케이터 |
| `DoseDisplayView` | 실시간 선량 모니터링, DRL 알림 |
| `CDBurnView` | CD/DVD 굽기 진행률 |
| `SystemAdminView` | 시스템 설정 (Admin/Service 전용) |
| `QuickPinLockView` | Quick PIN 잠금 해제 (SWR-CS-076) |

**테스트:** 93개 단위 (`HnVue.UI.Tests`)
**커버리지:** 60%+

---

## Layer 6: 애플리케이션 진입점

### HnVue.App
DI 컴포지션 루트. 모든 모듈 통합.

**핵심 항목:**
- `Program.cs` (Main 진입점: HostBuilder, Serilog, DI 컨테이너)
- `App.xaml.cs` (WPF 애플리케이션: 전역 예외 처리, DI 등록)
- `appsettings.Development.json`: 개발용 설정 (git 추적 제외)

---

문서 최종 업데이트: 2026-04-09
