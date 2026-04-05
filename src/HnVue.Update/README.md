# HnVue.Update

> 소프트웨어 업데이트 및 백업 서비스

## 목적

OTA(Over-The-Air) 소프트웨어 업데이트, 코드 서명 검증, 업데이트 전 자동 백업 기능을 제공합니다.
IEC 62304 §6.2.5 — 업데이트 패키지 적용 전 Authenticode 서명 및 SHA-256 해시 검증 필수.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SWUpdateService` | `ISWUpdateService` 구현체 — 업데이트 확인/적용/롤백 |
| `UpdateOptions` | 업데이트 설정 POCO — `appsettings.json` "SWUpdate" 섹션 바인딩 |
| `UpdateChecker` | HTTP 기반 업데이트 서버 조회 |
| `BackupManager` | 업데이트 전 애플리케이션 디렉토리 백업 |
| `BackupService` | 백업 수명 주기 관리 |
| `CodeSignVerifier` | Authenticode 코드 서명 검증 |
| `SignatureVerifier` | SHA-256 해시 검증 |
| `IUpdateRepository` | 업데이트 메타데이터 리포지토리 인터페이스 |
| `UpdateRepository` | `IUpdateRepository` 구현체 |

## 업데이트 적용 워크플로우

`SWUpdateService.ApplyUpdateAsync(packagePath)` 실행 순서:

1. `.sha256` 사이드카 파일 존재 시 SHA-256 해시 검증 (`SignatureVerifier.VerifyHash`)
2. `UpdateOptions.RequireAuthenticodeSignature` 활성화 시 Authenticode 서명 검증 (`SignatureVerifier.VerifyAuthenticode`)
3. `BackupManager.CreateBackupAsync()` — 현재 애플리케이션 디렉토리 백업
4. `pending_update.txt` 마커 파일 기록 (재시작 후 설치 완료)
5. 감사 로그 기록 (`UPDATE_STAGED`)

### Wave 2 스테이징 방식

바이너리 교체는 재시작 시 수행 (프로세스 잠금 문제 회피). Wave 3에서 라이브 교체 구현 예정.

## UpdateOptions 설정 항목

| 속성 | 기본값 | 설명 |
|------|--------|------|
| `UpdateServerUrl` | "" | 업데이트 서버 REST API URL |
| `CurrentVersion` | "1.0.0" | 현재 설치 버전 (Semantic Versioning) |
| `BackupDirectory` | `%APPDATA%\HnVue\backup` | 백업 저장 디렉토리 |
| `ApplicationDirectory` | 현재 프로세스 디렉토리 | 업데이트 대상 디렉토리 |
| `RequireAuthenticodeSignature` | `true` | Authenticode 서명 검증 필수 여부 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Security`

### NuGet 패키지

- `Microsoft.Extensions.Options` (UpdateOptions 바인딩)
- `Microsoft.Extensions.Http` (IHttpClientFactory)
- `Microsoft.Extensions.Logging.Abstractions`

## DI 등록

`AddSWUpdate(configuration)` 확장 메서드:

```csharp
services.AddSWUpdate(configuration); // UpdateOptions 바인딩 + HttpClient + ISWUpdateService 등록
```

또는 App에서 직접 등록 (Phase 1d):

```csharp
services.AddSingleton<IUpdateRepository, NullUpdateRepository>();
services.AddSingleton(new BackupService(applicationDirectory, backupBaseDirectory));
services.AddScoped<ISWUpdateService, SWUpdateService>();
```

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.Update.Tests`
- 테스트 파일 및 메서드 수:
  - `SWUpdateServiceTests.cs`: 9개
  - `BackupManagerTests.cs`: 12개
  - `BackupServiceTests.cs`: 11개
  - `CodeSignVerifierTests.cs`: 13개
  - `SignatureVerifierTests.cs`: 8개
  - `UpdateCheckerTests.cs`: 6개
  - `ServiceCollectionExtensionsTests.cs`: 4개
  - **합계: 63개**

## SWR 참조

- IEC 62304 §6.2.5 (소프트웨어 변경 관리)
- FDA §524B (소프트웨어 무결성 요구사항)

## 비고

- 코드 서명 검증 필수 (FDA §524B 무결성 요구사항)
- `UpdateInfo` 모델은 `HnVue.Common`에 정의
- 롤백 지원: `RollbackAsync()` — 최신 백업에서 복원 + 감사 로그 기록 (`UPDATE_ROLLED_BACK`)
- 예외 처리: `catch(Exception ex) when (ex is not OutOfMemoryException)` 패턴 적용 (BackupManager, BackupService, CodeSignVerifier, SWUpdateService)
- `IAuditService`는 선택적 의존성 (`null` 시 감사 기록 생략)
