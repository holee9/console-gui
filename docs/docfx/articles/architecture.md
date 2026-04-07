# Architecture Overview

HnVue Console SW는 Clean Architecture 원칙에 따라 14개 모듈로 구성됩니다.

## 계층 구조

```
HnVue.App (Composition Root)
├── HnVue.UI (WPF (Windows Presentation Foundation)/MVVM)
├── HnVue.Workflow (Orchestration)
│   ├── HnVue.Imaging
│   ├── HnVue.Dose
│   ├── HnVue.Dicom
│   └── HnVue.Incident
├── HnVue.PatientManagement
├── HnVue.CDBurning
├── HnVue.SystemAdmin
├── HnVue.Update
├── HnVue.Security
├── HnVue.Data (EF Core (Entity Framework Core))
└── HnVue.Common (Core)
```

## DI (Dependency Injection) 등록

애플리케이션 시작 시 `App.xaml.cs`에서 세 개의 확장 메서드로 핵심 서비스를 등록합니다:

- `AddHnVueCommon()` — Options, Validation, Retry 정책
- `AddHnVueData()` — DbContext, Repository
- `AddHnVueSecurity()` — 인증, 인가, 감사

## 주요 설계 패턴

- **Result Pattern**: `Result<T>` — 예외 대신 명시적 오류 반환
- **Repository Pattern**: 데이터 접근 추상화
- **State Machine**: 워크플로우 상태 전이
- **Options Pattern**: `IOptions<HnVueOptions>` 설정 주입
- **Retry with Polly**: 네트워크 통신 재시도 정책
