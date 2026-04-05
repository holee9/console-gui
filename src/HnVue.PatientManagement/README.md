# HnVue.PatientManagement

> 환자 관리 및 워크리스트 서비스

## 목적

환자 등록, 검색, 수정과 DICOM MWL(Modality Worklist) 기반 워크리스트 관리를 제공합니다.
IEC 62304 Class B — 환자 데이터 무결성은 선량 귀속의 안전 관련 요소.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `PatientService` | `IPatientService` 구현체 — 환자 CRUD (등록/검색/수정/삭제) |
| `WorklistService` | `IWorklistService` 구현체 — 워크리스트 관리 |
| `IWorklistRepository` | 워크리스트 데이터 리포지토리 인터페이스 |
| `WorklistRepository` | `IWorklistRepository` 구현체 |

## PatientService 기능

`IPatientService` 인터페이스:

| 메서드 | 설명 |
|--------|------|
| `RegisterAsync(patient)` | 환자 등록 — PatientId·Name·DateOfBirth 필수 필드 검증 |
| `SearchAsync(query)` | 이름/ID 기반 검색 |
| `GetByIdAsync(patientId)` | ID로 환자 조회 |
| `UpdateAsync(patient)` | 환자 정보 수정 |
| `DeleteAsync(patientId)` | 환자 삭제 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`

### NuGet 패키지

없음

## DI 등록

App에서 직접 등록:

```csharp
services.AddSingleton<IWorklistRepository, NullWorklistRepository>(); // Phase 1d
services.AddScoped<IPatientService, PatientService>();
services.AddScoped<IWorklistService, WorklistService>();
```

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.PatientManagement.Tests`
- 테스트 파일 및 메서드 수:
  - `PatientServiceTests.cs`: 10개
  - `WorklistServiceTests.cs`: 17개
  - **합계: 27개**

## 비고

- DICOM MWL C-FIND 결과를 로컬 워크리스트로 변환
- `PatientRecord`, `WorklistItem` 모델은 `HnVue.Common`에 정의
