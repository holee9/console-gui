# HnVue.Common

> 공유 추상화, 모델, 인터페이스 (Core Layer)

## 목적

모든 HnVue 모듈이 공통으로 참조하는 최하위 도메인 계층입니다.  
서비스 인터페이스(`Abstractions/`), 도메인 모델(`Models/`), 열거형(`Enums/`), Result 패턴(`Results/`), 설정 클래스(`Configuration/`)를 정의합니다.  
외부 프로젝트 참조가 없으며, 다른 모든 모듈이 이 패키지를 단방향으로 참조합니다.

---

## Abstractions (인터페이스 목록)

### 서비스 인터페이스

| 인터페이스 | 설명 | 주요 메서드 |
|---|---|---|
| `IWorkflowEngine` | 촬영 워크플로우 상태 머신 제어 | `StartAsync`, `TransitionAsync`, `PrepareExposureAsync`, `AbortAsync` |
| `IDoseService` | 선량 검증 및 기록 | `ValidateExposureAsync`, `RecordDoseAsync`, `GetDoseByStudyAsync` |
| `IGeneratorInterface` | X-ray 제너레이터 하드웨어 추상화 | `ConnectAsync`, `PrepareAsync`, `TriggerExposureAsync`, `AbortAsync`, `GetStatusAsync` |
| `IDicomService` | DICOM 네트워킹 (C-STORE, C-FIND, 인쇄, 보관 확인) | `StoreAsync`, `QueryWorklistAsync`, `PrintAsync`, `RequestStorageCommitmentAsync` |
| `IImageProcessor` | 영상 처리 및 뷰어 조작 | `ProcessAsync`, `ApplyWindowLevel`, `Zoom`, `Pan`, `Rotate`, `Flip` |
| `IPatientService` | 환자 등록/조회/수정/삭제 | `RegisterAsync`, `SearchAsync`, `UpdateAsync`, `GetByIdAsync`, `DeleteAsync` |
| `ISecurityService` | 인증/인가/잠금/비밀번호 변경 | `AuthenticateAsync`, `CheckAuthorizationAsync`, `LockAccountAsync`, `ChangePasswordAsync` |
| `ISecurityContext` | 현재 사용자 컨텍스트 (읽기 전용) | `IsAuthenticated`, `CurrentUsername`, `CurrentRole`, `HasRole` |
| `IAuditService` | 감사 로그 기록 및 조회 | `WriteAuditAsync`, `VerifyChainIntegrityAsync`, `GetAuditLogsAsync` |
| `IAuditRepository` | 감사 로그 영속 계층 | `AppendAsync`, `GetLastHashAsync`, `QueryAsync` |
| `IPatientRepository` | 환자 데이터 영속 계층 | `AddAsync`, `FindByIdAsync`, `SearchAsync`, `UpdateAsync`, `DeleteAsync` |
| `IStudyRepository` | 스터디 데이터 영속 계층 | `AddAsync`, `GetByPatientAsync`, `GetByUidAsync`, `UpdateAsync` |
| `IUserRepository` | 사용자 계정 영속 계층 | `GetByUsernameAsync`, `GetByIdAsync`, `UpdateFailedLoginCountAsync`, `SetLockedAsync`, `UpdatePasswordHashAsync`, `GetAllAsync` |
| `IWorklistService` | MWL(Modality Worklist) 조회 | `PollAsync`, `ImportFromMwlAsync` |
| `ISystemAdminService` | 시스템 설정 관리 및 감사 로그 내보내기 | `GetSettingsAsync`, `UpdateSettingsAsync`, `ExportAuditLogAsync` |
| `ISWUpdateService` | 소프트웨어 업데이트 관리 | `CheckUpdateAsync`, `ApplyUpdateAsync`, `RollbackAsync` |
| `ICDDVDBurnService` | CD/DVD 소각 서비스 | `BurnStudyAsync`, `VerifyDiscAsync` |
| `IRetryPolicyFactory` | Polly 재시도 정책 팩토리 | (Polly `IResiliencePipeline` 반환) |

### 주요 인터페이스 상세

#### `IWorkflowEngine`
```
PrepareExposureAsync(ExposureParameters) → Result<DoseValidationResult>
```
- SWR-WF-023~025: ALLOW / WARN / BLOCK / EMERGENCY 4단계 선량 인터락 구현
- WARN → `SafeState.Warning` 설정 후 Exposing 전환 허용
- BLOCK → `SafeState.Blocked` 설정, 전환 거부
- EMERGENCY → `SafeState.Emergency` 설정, 전환 거부, `ErrorCode.DoseInterlock` 반환

#### `IDicomService`
```
RequestStorageCommitmentAsync(sopClassUid, sopInstanceUid, pacsAeTitle) → Result
```
- SWR-DC-057 (N-ACTION SCU) / SWR-DC-058 (N-EVENT-REPORT)
- C-STORE 성공 후 PACS 영구 보관 확인 (Issue #23)

#### `IImageProcessor`
```
Rotate(ProcessedImage image, int degrees) → Result<ProcessedImage>   // SWR-IP-027
Flip(ProcessedImage image, bool horizontal) → Result<ProcessedImage>  // SWR-IP-027
```
- `Rotate`: 90, 180, 270도 시계방향 회전 (Bilinear 보간)
- `Flip`: `horizontal=true` → 좌우 반전, `false` → 상하 반전

---

## Models (도메인 모델 목록)

| 모델 | 타입 | 설명 |
|---|---|---|
| `ProcessedImage` | `sealed class` | 처리된 방사선 영상. `Width`, `Height`, `BitsPerPixel`, `PixelData(byte[])`, `WindowCenter`, `WindowWidth`, `FilePath?`, `PanOffsetX`, `PanOffsetY`, `RawPixelData16(ushort[]?, init-only)` |
| `ProcessingParameters` | `sealed record` | 영상 처리 파라미터. `WindowCenter?`, `WindowWidth?`, `AutoWindow`, `RawImageWidth?(int?)`, `RawImageHeight?(int?)` |
| `ExposureParameters` | `sealed record` | 촬영 기술 파라미터 (kV, mAs 등) |
| `PatientRecord` | `sealed record` | 환자 도메인 모델 |
| `StudyRecord` | `sealed record` | 스터디 도메인 모델 |
| `UserRecord` | `sealed record` | 사용자 계정 도메인 모델 |
| `AuthenticatedUser` | `sealed record` | 인증 성공 후 사용자 정보 |
| `AuthenticationToken` | `sealed record` | JWT 인증 토큰 정보 |
| `DoseRecord` | `sealed record` | 촬영 선량 기록 |
| `DoseValidationResult` | `sealed record` | 선량 검증 결과 (`Level: DoseValidationLevel`, `Message`) |
| `GeneratorStatus` | `sealed record` | 제너레이터 현재 상태 스냅샷 |
| `GeneratorStateChangedEventArgs` | `sealed class : EventArgs` | 제너레이터 상태 변경 이벤트 인자 |
| `WorkflowStateChangedEventArgs` | `sealed class : EventArgs` | 워크플로우 상태 변경 이벤트 인자 |
| `AuditEntry` | `sealed record` | 감사 로그 항목 |
| `AuditQueryFilter` | `sealed record` | 감사 로그 조회 필터 |
| `SystemSettings` | `sealed class` | 전역 시스템 설정 (`DicomSettings`, `GeneratorSettings`, `SecuritySettings` 포함) |
| `UpdateInfo` | `sealed record` | 소프트웨어 업데이트 정보 |
| `WorklistItem` | `sealed record` | MWL 워크리스트 항목 |
| `WorklistQuery` | `sealed record` | MWL 쿼리 조건 |

### ProcessedImage 주요 속성 변경 이력

| 속성 | 타입 | 설명 | SWR |
|---|---|---|---|
| `PanOffsetX` | `int` | 누적 수평 팬 오프셋 (생성자 파라미터) | SWR-IP-026 |
| `PanOffsetY` | `int` | 누적 수직 팬 오프셋 (생성자 파라미터) | SWR-IP-026 |
| `RawPixelData16` | `ushort[]?` | 16비트 원본 픽셀 버퍼 (`init` 전용 설정자) | SWR-IP-036, SWR-DC-055 |

### ProcessingParameters 주요 속성 변경 이력

| 속성 | 타입 | 기본값 | 설명 | Issue |
|---|---|---|---|---|
| `RawImageWidth` | `int?` | `null` | Raw 이미지 명시적 픽셀 너비 | #7 |
| `RawImageHeight` | `int?` | `null` | Raw 이미지 명시적 픽셀 높이 | #7 |

---

## Enums (열거형 목록)

| 열거형 | 값 | 설명 |
|---|---|---|
| `SafeState` | `Idle`, `Warning`, `Degraded`, `Blocked`, `Emergency` | 시스템 안전 상태 (5종). `Warning`은 WARN 인터락 수준에 매핑 (SWR-WF-023, Issue #21) |
| `DoseValidationLevel` | `Allow`, `Warn`, `Block`, `Emergency` | 선량 검증 후 권장 조치 (4종) |
| `WorkflowState` | `Idle`, `PatientSelected`, `ProtocolSelected`, `ReadyToExpose`, `Exposing`, `ImageReview`, `Complete`, `Error` | 촬영 워크플로우 상태 |
| `GeneratorState` | `Disconnected`, `Idle`, `Preparing`, `Ready`, `Exposing`, `Error` | 제너레이터 하드웨어 상태 |
| `UserRole` | `Radiographer`, `Radiologist`, `Admin`, `Service` | 사용자 역할 (RBAC) |
| `IncidentSeverity` | `Critical`, `High`, `Medium`, `Low` | 인시던트 심각도 (IEC 62304) |

---

## Results (Result 패턴)

| 타입 | 설명 |
|---|---|
| `Result` | 값 없는 연산 결과. `Result.Success()` / `Result.Failure(ErrorCode, string)` |
| `Result<T>` | 값 있는 연산 결과. `.Value`, `.Error`, `.ErrorMessage`, `.Map<TOut>()`, `.Bind<TOut>()`, `.Match<TOut>()` |
| `ErrorCode` | 도메인별 오류 코드 열거형 |

### ErrorCode 범위

| 범위 | 도메인 | 주요 값 |
|---|---|---|
| 0xxx | General | `Unknown(0)`, `ValidationFailed(1000)`, `NotFound(1001)`, `AlreadyExists(1002)`, `OperationCancelled(1003)` |
| 2xxx | Security | `AuthenticationFailed(2000)`, `AccountLocked(2001)`, `TokenExpired(2002)`, `TokenInvalid(2003)`, `InsufficientPermission(2004)`, `PasswordPolicyViolation(2005)` |
| 3xxx | Data | `DatabaseError(3000)`, `MigrationFailed(3001)`, `EncryptionFailed(3002)` |
| 4xxx | Workflow | `InvalidStateTransition(4000)`, `GeneratorNotReady(4001)`, `DetectorNotReady(4002)`, `ExposureAborted(4003)`, `DoseLimitExceeded(4004)`, `DoseInterlock(4005)` |
| 5xxx | DICOM | `DicomConnectionFailed(5000)`, `DicomStoreFailed(5001)`, `DicomQueryFailed(5002)`, `DicomPrintFailed(5003)` |
| 6xxx | Incident | `IncidentLogFailed(6000)` |
| 7xxx | Update | `SignatureVerificationFailed(7000)`, `UpdatePackageCorrupt(7001)`, `RollbackFailed(7002)` |
| 8xxx | CD Burning | `BurnFailed(8000)`, `DiscVerificationFailed(8001)` |
| 9xxx | Imaging | `ImageProcessingFailed(9000)`, `UnsupportedImageFormat(9001)` |

> `DoseInterlock = 4005` — SWR-WF-023~025 선량 인터락으로 노출 차단/중단 시 사용

---

## 의존성

### 프로젝트 참조

없음 (최하위 계층, 단방향 참조 구조의 루트)

### NuGet 패키지

| 패키지 | 용도 |
|---|---|
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI 인터페이스 |
| `Polly` | `IRetryPolicyFactory` 구현용 재시도 정책 |
| `FluentValidation` | 입력 유효성 검증 |

---

## DI 등록

`AddHnVueCommon()` — Options, Validation, Retry 정책 등록

---

## 비고

- 외부 프로젝트 참조 없음 (순수 도메인 계층)
- IEC 62304 §5.3.6 traceability: 모든 에러 코드와 안전 상태가 SWR 번호로 추적됨
- 18개 서비스/리포지터리 인터페이스 정의
- 6개 열거형 (`SafeState` 포함, Warning 상태 추가로 5종)
