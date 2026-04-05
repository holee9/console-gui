# HnVue.Workflow

> X-ray 촬영 워크플로우 엔진

## 목적

촬영 워크플로우 상태 머신(Login → PatientSelect → ProtocolSelect → Positioning → Exposure → Review)을 관리합니다. 제너레이터 시뮬레이터를 포함합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `WorkflowEngine` | IWorkflowEngine 구현체 — 메인 워크플로우 오케스트레이션 |
| `WorkflowStateMachine` | 상태 머신 구현 (State 패턴) |
| `GeneratorSimulator` | IGeneratorInterface 구현체 — 개발/테스트용 시뮬레이터 |

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
- Safe State 전이 로직 포함 (IEC 62304 요구사항)
- GeneratorSimulator로 하드웨어 없이 개발 가능
