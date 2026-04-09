# HnVue.Workflow

> X-ray 촬영 워크플로우 엔진

## 목적

촬영 워크플로우 상태 머신(Idle → PatientSelected → ProtocolLoaded → ReadyToExpose → Exposing → ImageAcquiring → ImageProcessing → ImageReview → Completed)을 관리합니다.
IEC 62304 §5.3.6 안전 인터락 및 RBAC를 강제하며, RS-232 실 제너레이터 연동과 개발/테스트용 시뮬레이터를 모두 제공합니다.

---

## 주요 타입

| 타입 | 설명 |
|---|---|
| `WorkflowEngine` | `IWorkflowEngine` 구현체. 상태 머신 오케스트레이션, RBAC 강제, 선량 인터락, 안전 상태 관리, 감사 로그 |
| `WorkflowStateMachine` | 10-상태 전이표 기반 상태 머신 구현. `TryTransition`, `ForceError`, `ResetToIdle` 제공 |
| `ISerialPortAdapter` | `internal` 인터페이스. RS-232 통신 추상화 (IsOpen, Open, Close, Write, DataReceived) |
| `SerialPortAdapter` | `System.IO.Ports.SerialPort` 프로덕션 래퍼 |
| `GeneratorSerialPort` | `IGeneratorInterface` RS-232 실 구현체. Sedecal/CPI 제너레이터 연동 (SWR-WF-020~022). 테스트용 `ISerialPortAdapter` 주입 가능 |
| `GeneratorSimulator` | `IGeneratorInterface` 시뮬레이터. 개발/테스트 환경에서 하드웨어 없이 동작 |

---

## WorkflowEngine 상세

### 생성자 의존성

```
WorkflowEngine(IDoseService, IGeneratorInterface, ISecurityContext, IAuditService? = null, IDetectorInterface? = null)
```

- `IAuditService`는 optional 파라미터입니다. null 시 감사 로그 기록이 생략되지만 워크플로우 진행이 차단되지 않습니다 (fire-and-forget 방식).
- `IDetectorInterface`는 optional 파라미터입니다. null 시 검출기 ARM/Abort가 생략됩니다 (제너레이터 단독 운용 가능).

### 핵심 동작

#### `PrepareExposureAsync(ExposureParameters)` — SWR-WF-023~025

선량 인터락 4단계를 구현합니다. Issue #21.

| 검증 결과 | `DoseValidationLevel` | `SafeState` 전이 | 워크플로우 전이 | 반환 |
|---|---|---|---|---|
| 정상 | `Allow` | `Idle` 유지 | `ReadyToExpose` → `Exposing` + 검출기 ARM | `Result.Success(DoseValidationResult)` |
| 경고 | `Warn` | `Warning` 설정 | `ReadyToExpose` → `Exposing` + 검출기 ARM | `Result.Success(DoseValidationResult)` |
| 차단 | `Block` | `Blocked` 설정 | 전환 거부 | `Result.Failure(DoseInterlock, ...)` |
| 비상 | `Emergency` | `Emergency` 설정 | 전환 거부 | `Result.Failure(DoseInterlock, ...)` |

- WARN 시 `SafeState.Warning` → 호출자(UI)가 경고 알림 표시 후 진행 가능
- BLOCK/EMERGENCY 시 수동 개입 없이 복구 불가
- ALLOW/WARN 시 상태가 `Exposing`으로 전이된 직후 `IDetectorInterface?.ArmAsync(Sync)` 호출
  - 검출기 ARM 실패 시 강제 `Error` 상태 + `Result.Failure(DetectorNotReady)` 반환

#### `TransitionAsync(WorkflowState)` — SWR-IP-RBAC-001

`WorkflowState.Exposing` 전환 요청 시 RBAC 검증:
- 미인증 사용자 (`CurrentRole == null`) → `ErrorCode.AuthenticationFailed`
- `Admin` / `Service` 역할 → `RbacPolicy.Check` 실패 → 노출 차단
- `Radiographer` / `Radiologist` 역할만 노출 허용

#### `AbortAsync(reason)` — SWR-WF-031~033

모든 상태에서 즉시 호출 가능. 다음 동작을 순차 수행합니다:

1. `EXPOSURE_ABORT` 감사 로그 기록 (SWR-NF-SC-041)
2. `Exposing`, `ReadyToExpose`, `ImageAcquiring` 상태인 경우:
   - `IGeneratorInterface.AbortAsync()` 호출
   - `IDetectorInterface?.AbortAsync()` 호출 (주입된 경우)
3. 제너레이터 또는 검출기 중단 실패 시 `SafeState.Emergency`로 에스컬레이션
4. 워크플로우 상태를 `Error`로 강제 전이

⚠️ **버그 수정 (2026-04-06):** `capturedPatientId` 변수는 lock 획득 **전**에 로컬 변수로 캡처됩니다. 이전 코드에서 lock 내부에서만 캡처하면 경합 조건으로 인해 null이 될 수 있었습니다.

### 안전 상태 (`SafeState`)

| 상태 | 값 | 선량 인터락 수준 | 설명 |
|---|---|---|---|
| `Idle` | 0 | ALLOW | 정상 동작. 모든 작업 허용 |
| `Warning` | 1 | WARN | DRL 초과지만 운영자 확인 후 진행 가능 (Issue #21) |
| `Degraded` | 2 | — | 비위험 서브시스템 오류. 제한된 기능 |
| `Blocked` | 3 | BLOCK | 안전 인터락 활성. 촬영 차단 |
| `Emergency` | 4 | EMERGENCY | 즉각 정지. 수동 개입 필요 |

`ErrorCode.DoseInterlock = 4005` — BLOCK/EMERGENCY 선량 인터락 반환 시 사용

---

## GeneratorSerialPort 상세

Sedecal 및 CPI 제너레이터와 RS-232 통신을 수행합니다.

### 상태 전이

```
Disconnected → Idle (ConnectAsync)
Idle → Preparing (PrepareAsync) → Ready (READY 응답 수신)
Ready → Exposing (TriggerExposureAsync) → Done (EXPOSURE_DONE) → Idle
Any → Error (ERROR 응답 또는 타임아웃)
Any → Disconnected (DisconnectAsync)
```

### 응답 토큰

`ACK`, `READY`, `EXPOSURE_DONE`, `AEC_TERMINATED`, `HEAT_UNITS`, `ERROR`, `BUSY`

### 예외 처리

`IOException`, `InvalidOperationException`, `TimeoutException`을 세분화하여 처리합니다.  
`AbortAsync`는 HAZ-RAD 안전 요구사항에 따라 silent failure 없이 동작합니다.

---

## GeneratorSimulator 상세

개발/테스트 환경에서 실 하드웨어 없이 동작합니다.

| 프로퍼티 | 기본값 | 설명 |
|---|---|---|
| `PrepareDelayMs` | 500 ms | 준비 시뮬레이션 지연 시간 |
| `ExposureDelayMs` | 200 ms | 촬영 시뮬레이션 지연 시간 |
| `FailNextConnectWith` | `null` | 설정 시 다음 Connect 강제 실패 |
| `FailNextExposureWith` | `null` | 설정 시 다음 TriggerExposure 강제 실패 |

---

## 감사 로그 이벤트

| 액션 | 발생 위치 | 설명 | SWR |
|------|---------|------|-----|
| `EXPOSURE_PREPARE` | `PrepareExposureAsync()` | 노출 준비 시작. DoseValidationLevel 기록 | SWR-NF-SC-041 |
| `EXPOSURE_ABORT` | `AbortAsync()` | 노출 중단. reason 파라미터 기록 | SWR-NF-SC-041 |
| `SAFESTATE_CHANGED` | `SetSafeState()` | 안전 상태 전이 (Idle → Warning → Blocked → Emergency). 이전/현재 상태 기록 | SWR-NF-SC-041 |

감사 로그는 `IAuditService.WriteAuditAsync()` 호출로 기록되며, 서비스 오류 시 워크플로우 진행이 차단되지 않습니다 (fire-and-forget).

---

## 테스트

| 파일 | 검증 범위 |
|---|---|
| `WorkflowEngineTests.cs` | 선량 인터락 4단계, RBAC, 상태 전이, Abort 에스컬레이션, 감사 로그 |
| `WorkflowEngineEmergencyTests.cs` | 응급 노출 fast-path, Error 상태 재진입, 안전 상태 에스컬레이션 |
| `WorkflowStateMachineTests.cs` | 허용/거부 전이표, `Completed`/`Error` reset 경로 |
| `GeneratorSimulatorTests.cs` | 시뮬레이터 상태 머신, 지연, 실패 주입 |
| `GeneratorSerialPortSmokeTests.cs` | RS-232 구현체 스모크 테스트 |
| `GeneratorSerialPortAdapterTests.cs` | `ISerialPortAdapter` FakeAdapter, DI 주입 시나리오 |

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 역할 |
|---|---|
| `HnVue.Common` | 인터페이스, 모델, Result 패턴, `IDetectorInterface` |
| `HnVue.Data` | 저장소 구현 및 EF 기반 데이터 접근 |
| `HnVue.Security` | `RbacPolicy`, `Permissions` 상수 |
| `HnVue.Dicom` | DICOM 서비스 연계 |
| `HnVue.Imaging` | 이미지 후처리 연계 |
| `HnVue.Dose` | `IDoseService` 구현체 |
| `HnVue.Incident` | 인시던트/안전 이벤트 연계 |

### NuGet 패키지

| 패키지 | 용도 |
|---|---|
| `System.IO.Ports` | RS-232 시리얼 통신 (`GeneratorSerialPort`) |

---

## DI 등록

없음 (HnVue.App에서 직접 등록)

---

## 비고

- IEC 62304 §5.3.6 traceability: WF-2xx 모듈 번호로 추적
- `GeneratorSimulator`로 하드웨어 없이 개발 및 CI/CD 가능
- `SafeState.Warning`은 Issue #21에서 추가 (WARN 인터락 수준 매핑)
- `ErrorCode.DoseInterlock = 4005`는 Issue #21에서 추가
