# HnVue.Workflow

> X-ray 촬영 워크플로우 엔진

## 목적

촬영 워크플로우 상태 머신(Login → PatientSelect → ProtocolSelect → Positioning → Exposure → Review)을 관리합니다. 제너레이터 시뮬레이터 및 RS-232 시리얼 통신 모듈을 포함합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `WorkflowEngine` | IWorkflowEngine 구현체 — 메인 워크플로우 오케스트레이션 + RBAC 강제 |
| `WorkflowStateMachine` | 상태 머신 구현 (9-상태 전이표) |
| `GeneratorSerialPort` | RS-232 시리얼 통신 — 실 제너레이터 하드웨어 연동 |
| `GeneratorSimulator` | IGeneratorInterface 구현체 — 개발/테스트용 시뮬레이터 |

## RBAC 강제 (SWR-IP-RBAC-001)

`WorkflowEngine.TransitionAsync(WorkflowState.Exposing)` 호출 시:
- 미인증 사용자 (`CurrentRole == null`) → `ErrorCode.AuthenticationFailed` 반환
- Admin / Service 역할 → `RbacPolicy.Check` 실패 → 노출 차단
- Radiographer / Radiologist 역할만 노출 허용

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Dicom`
- `HnVue.Imaging`
- `HnVue.Dose`
- `HnVue.Incident`

### NuGet 패키지

- `System.IO.Ports`

## DI 등록

없음 (App에서 직접 등록)

## 비고

- System.IO.Ports — RS-232 시리얼 통신 (실 제너레이터 연동)
- `GeneratorSerialPort`: `IOException`/`InvalidOperationException`/`TimeoutException` 세분화 예외 처리
- Safe State 전이 로직 포함 (IEC 62304 요구사항)
- `GeneratorSimulator`로 하드웨어 없이 개발 가능
