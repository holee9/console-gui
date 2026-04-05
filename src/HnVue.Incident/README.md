# HnVue.Incident

> 인시던트 대응 및 기록 서비스

## 목적

시스템 인시던트(장비 오류, 소프트웨어 장애 등)를 기록하고 대응 절차를 관리합니다. FDA 21 CFR Part 803 MDR 보고 요구사항을 지원합니다.
IEC 62304 §5.8 — 인시던트 조사 및 이상 관리.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `IncidentService` | `IIncidentService` 구현체 — 인시던트 CRUD + 감사 연동 + 알림 |
| `IncidentResponseService` | 인시던트 대응 및 에스컬레이션 서비스 (Critical 콜백 등록 지원) |
| `IIncidentService` | 인시던트 서비스 인터페이스 |
| `IIncidentRepository` | 인시던트 데이터 리포지토리 인터페이스 |
| `IncidentRepository` | 메모리 `ConcurrentDictionary` 기반 리포지토리 구현체 |
| `NotificationService` | 인시던트 발생 시 알림 처리 |
| `IncidentRecord` (Models/) | 인시던트 기록 불변 레코드 모델 |

## 인시던트 처리 흐름

### IncidentService.ReportAsync()

1. `IncidentRecord` 생성 (GUID ID, UTC 타임스탬프)
2. `IncidentRepository.AddAsync()` — 저장
3. `IAuditService.WriteAuditAsync()` — 감사 항목 기록 (Critical 시 `CRITICAL_INCIDENT` 마커 포함)
4. `NotificationService.Notify()` — 알림 발송

### IncidentResponseService.RecordAsync()

1. 유효성 검사 (category, description 필수)
2. `IIncidentRepository.SaveAsync()` — 저장
3. Critical 심각도 시 `EscalateAsync()` 호출 — 등록된 콜백 순차 실행

## 에스컬레이션

`IncidentResponseService.OnCritical(Func<IncidentRecord, Task>)` 로 콜백 등록:
- Critical 인시던트 발생 즉시 등록된 모든 콜백 순차 실행
- 콜백 예외는 `catch(Exception ex) when (ex is not OutOfMemoryException)` 로 격리 — 인시던트 레코드 저장에 영향 없음

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`

### NuGet 패키지

- `Microsoft.Extensions.Logging.Abstractions` (고성능 `[LoggerMessage]` 델리게이트)

## DI 등록

`AddIncident()` 확장 메서드:

```csharp
services.AddIncident(); // IncidentRepository, NotificationService, IIncidentService 등록
```

또는 App에서 `IncidentResponseService` 직접 등록:

```csharp
services.AddSingleton<IIncidentRepository, NullIncidentRepository>(); // Phase 1d
services.AddScoped<IncidentResponseService>();
```

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.Incident.Tests`
- 테스트 파일 및 메서드 수:
  - `IncidentServiceTests.cs`: 21개
  - `IncidentResponseServiceTests.cs`: 13개
  - `IncidentRepositoryTests.cs`: 11개
  - `NotificationServiceTests.cs`: 6개
  - `ServiceCollectionExtensionsTests.cs`: 3개
  - **합계: 54개**

## SWR 참조

- FDA 21 CFR Part 803 (MDR 보고)
- IEC 62304 §5.8 (소프트웨어 이상 조사)

## 비고

- `IncidentSeverity` 열거형은 `HnVue.Common`에 정의
- 인시던트 레코드는 Append-only — 생성 후 삭제/수정 불가
- `IncidentRecord`는 `Models/` 디렉토리에 단일 소스로 관리 (루트 중복 파일 제거됨)
- 감사 로그와 연동하여 인시던트 추적 (`VerifyAuditIntegrityAsync()` 지원)
