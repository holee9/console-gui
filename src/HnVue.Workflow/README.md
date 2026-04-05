# HnVue.Workflow

> X-ray 촬영 워크플로우 엔진

## 목적

촬영 워크플로우 상태 머신(Idle → PatientSelected → ProtocolSelected → ReadyToExpose → Exposing → ImageReview → Complete)을 관리합니다.  
IEC 62304 §5.3.6 안전 인터락 및 RBAC를 강제하며, RS-232 실 제너레이터 연동과 개발/테스트용 시뮬레이터를 모두 제공합니다.

---

## 주요 타입

| 타입 | 설명 |
|---|---|
| `WorkflowEngine` | `IWorkflowEngine` 구현체. 상태 머신 오케스트레이션, RBAC 강제, 선량 인터락, 안전 상태 관리 |
| `WorkflowStateMachine` | 9-상태 전이표 기반 상태 머신 구현. `TryTransition`, `ForceError` 제공 |
| `GeneratorSerialPort` | `IGeneratorInterface` RS-232 실 구현체. Sedecal/CPI 제너레이터 연동 (SWR-WF-020~022) |
| `GeneratorSimulator` | `IGeneratorInterface` 시뮬레이터. 개발/테스트 환경에서 하드웨어 없이 동작 |

---

## WorkflowEngine 상세

### 생성자 의존성

```
WorkflowEngine(IDoseService, IGeneratorInterface, ISecurityContext)
```

### 핵심 동작

#### `PrepareExposureAsync(ExposureParameters)` — SWR-WF-023~025

선량 인터락 4단계를 구현합니다. Issue #21.

| 검증 결과 | `DoseValidationLevel` | `SafeState` 전이 | 워크플로우 전이 | 반환 |
|---|---|---|---|---|
| 정상 | `Allow` | `Idle` 유지 | `ReadyToExpose` → `Exposing` | `Result.Success(DoseValidationResult)` |
| 경고 | `Warn` | `Warning` 설정 | `ReadyToExpose` → `Exposing` | `Result.Success(DoseValidationResult)` |
| 차단 | `Block` | `Blocked` 설정 | 전환 거부 | `Result.Failure(DoseInterlock, ...)` |
| 비상 | `Emergency` | `Emergency` 설정 | 전환 거부 | `Result.Failure(DoseInterlock, ...)` |

- WARN 시 `SafeState.Warning` → 호출자(UI)가 경고 알림 표시 후 진행 가능
- BLOCK/EMERGENCY 시 수동 개입 없이 복구 불가

#### `TransitionAsync(WorkflowState)` — SWR-IP-RBAC-001

`WorkflowState.Exposing` 전환 요청 시 RBAC 검증:
- 미인증 사용자 (`CurrentRole == null`) → `ErrorCode.AuthenticationFailed`
- `Admin` / `Service` 역할 → `RbacPolicy.Check` 실패 → 노출 차단
- `Radiographer` / `Radiologist` 역할만 노출 허용

#### `AbortAsync(reason)`

모든 상태에서 즉시 호출 가능. `Exposing` 또는 `ReadyToExpose` 상태에서는 `IGeneratorInterface.AbortAsync()`를 추가로 호출하며, 제너레이터 중단 실패 시 `SafeState.Emergency`로 에스컬레이션합니다.

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

## 테스트

| 파일 | 테스트 수 | 내용 |
|---|---|---|
| `WorkflowEngineTests.cs` | 27 | 선량 인터락 4단계, RBAC, 상태 전이, Abort 에스컬레이션 |
| `WorkflowStateMachineTests.cs` | 14 | 허용/거부 전이표 검증 |
| `GeneratorSimulatorTests.cs` | 16 | 시뮬레이터 상태 머신, 지연, 실패 주입 |
| `GeneratorSerialPortSmokeTests.cs` | 12 | RS-232 구현체 스모크 테스트 |

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 역할 |
|---|---|
| `HnVue.Common` | 인터페이스, 모델, Result 패턴 |
| `HnVue.Security` | `RbacPolicy`, `Permissions` 상수 |
| `HnVue.Dose` | `IDoseService` 구현체 |

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
