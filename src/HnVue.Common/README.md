# HnVue.Common

> 공유 추상화, 모델, 인터페이스 (Core Layer)

## 목적

모든 모듈이 의존하는 핵심 계층입니다. 도메인 모델, 서비스 인터페이스(`Abstractions/`), 열거형(`Enums/`), 설정 클래스(`Configuration/`), 그리고 `Result<T>` 패턴을 정의합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `IGeneratorInterface` | X-ray 제너레이터 통신 추상화 |
| `IWorkflowEngine` | 촬영 워크플로우 엔진 인터페이스 |
| `ISecurityService` | 인증/인가 서비스 인터페이스 |
| `IDicomService` | DICOM 네트워킹 서비스 인터페이스 |
| `Result / Result<T>` | 오류 처리용 Result 패턴 |
| `HnVueOptions` | 전역 설정 (Security, DICOM, Generator 포함) |
| `PatientRecord` | 환자 도메인 모델 |
| `StudyRecord` | 스터디 도메인 모델 |
| `GeneratorState (enum)` | 제너레이터 상태 (Idle → Ready → Exposing → …) |
| `WorkflowState (enum)` | 워크플로우 상태 (Login → PatientSelect → Exposure → …) |

## 의존성

### 프로젝트 참조

없음 (최하위 계층)

### NuGet 패키지

- `Microsoft.Extensions.DependencyInjection`
- `Polly`
- `FluentValidation`

## DI 등록

`AddHnVueCommon()` — Options, Validation, Retry 정책 등록

## 비고

- 외부 프로젝트 참조 없음 (순수 도메인 계층)
- 18개 서비스 인터페이스 정의
- 6개 열거형 (GeneratorState, WorkflowState, UserRole 등)
