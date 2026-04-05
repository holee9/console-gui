# HnVue.SystemAdmin

> 시스템 관리 서비스

## 목적

시스템 설정 관리, 기기 설정(DICOM AE Title, Generator 파라미터 등), 감사 로그 내보내기 기능을 제공합니다.
IEC 62304 Class B.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SystemAdminService` | `ISystemAdminService` 구현체 — 설정 관리 + 감사 로그 내보내기 |
| `ISystemSettingsRepository` | 시스템 설정 리포지토리 인터페이스 |
| `SystemSettingsRepository` | `ISystemSettingsRepository` 구현체 |

## SystemAdminService 기능

| 메서드 | 설명 |
|--------|------|
| `GetSettingsAsync()` | 현재 시스템 설정 조회 |
| `UpdateSettingsAsync(settings)` | 설정 유효성 검사 후 저장 |
| `ExportAuditLogAsync(outputPath)` | 감사 로그를 CSV 파일로 내보내기 (tamper-evident) |

### 설정 유효성 검사 규칙

- `Dicom.PacsPort`: 1–65535 범위
- `Dicom.LocalAeTitle`: 비어있지 않을 것
- `Security.SessionTimeoutMinutes`: 1 이상
- `Security.MaxFailedLogins`: 1 이상

### 감사 로그 내보내기

`ExportAuditLogAsync(outputPath)` — CSV 형식:

```
EntryId,Timestamp,UserId,Action,Details,PreviousHash,CurrentHash
```

- RFC 3339 타임스탬프 (`O` 포맷)
- CSV 특수문자 이스케이프 처리
- 출력 디렉토리 자동 생성

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Update`

### NuGet 패키지

없음

## DI 등록

App에서 직접 등록:

```csharp
services.AddSingleton<ISystemSettingsRepository, NullSystemSettingsRepository>(); // Phase 1d
services.AddScoped<ISystemAdminService, SystemAdminService>();
```

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.SystemAdmin.Tests`
- 테스트 파일 및 메서드 수:
  - `SystemAdminServiceTests.cs`: 13개
  - **합계: 13개**

## 비고

- `SystemSettings`, `DicomSettings`, `GeneratorSettings`, `SecuritySettings` 모델은 `HnVue.Common`에 정의
- Admin 역할만 접근 가능 (RBAC — `HnVue.Security`)
- `IAuditRepository`를 직접 주입받아 감사 로그 조회 수행
