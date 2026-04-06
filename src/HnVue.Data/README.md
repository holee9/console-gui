# HnVue.Data

> 데이터 접근 계층 (EF Core + SQLite/SQLCipher)

## 목적

EF Core를 사용한 데이터 영속성 계층입니다. SQLCipher 암호화 DB에 환자, 스터디, 사용자, 감사 로그, 선량 기록을 저장합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `HnVueDbContext` | EF Core DbContext — 전체 데이터베이스 스키마 |
| `PatientEntity` | 환자 DB 엔티티 |
| `StudyEntity` | 스터디 DB 엔티티 |
| `UserEntity` | 사용자 DB 엔티티 (QuickPinHash, QuickPinFailedCount, QuickPinLockedUntilTicks 필드 추가) |
| `AuditLogEntity` | 감사 로그 엔티티 |
| `DoseRecordEntity` | 선량 기록 엔티티 |
| `ImageEntity` | 이미지 엔티티 |
| `EntityMapper` | 엔티티 ↔ 도메인 모델 매핑 |
| `PatientRepository` | `IPatientRepository` 구현체 |
| `StudyRepository` | `IStudyRepository` 구현체 |
| `UserRepository` | `IUserRepository` 구현체 |
| `AuditRepository` | `IAuditRepository` 구현체 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `SQLitePCLRaw.bundle_e_sqlcipher`

## DI 등록

`AddHnVueData(connectionString)` — DbContext, 리포지토리 일괄 등록:

```csharp
services.AddHnVueData("Data Source=hnvue.db");
```

## EF Core Cascade Delete 정책 (IEC 62304 §5.5)

| 관계 | 이전 | 현재 | 사유 |
|------|------|------|------|
| Patient → Study | `DeleteBehavior.Cascade` | `DeleteBehavior.Restrict` | 환자 삭제 시 자동 삭제 금지 — 선량 기록 보호 (DICOM 스터디 참조 유지) |
| Study → DoseRecord | `DeleteBehavior.Cascade` | `DeleteBehavior.Restrict` | 스터디 삭제 시 자동 삭제 금지 — 감사 기록 보호 (FDA §524B) |

Restrict 정책 적용으로 인해 **Parent 레코드 삭제는 Child 존재 시 실패**합니다. 관리자가 의도적으로 수동 삭제해야 합니다 (감사 로그 남김).

---

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.Data.Tests`
- 테스트 파일 및 메서드 수:
  - `HnVueDbContextTests.cs`: 2개
  - `ResultExtensionsTests.cs`: 4개
  - `EntityInstantiationTests.cs`: 4개
  - `ServiceCollectionExtensionsTests.cs`: 2개
  - `EntityMapperTests.cs`: 14개
  - `AuditRepositoryTests.cs`: 12개
  - `PatientRepositoryTests.cs`: 13개
  - `StudyRepositoryTests.cs`: 9개
  - `UserRepositoryTests.cs`: 9개
  - `CascadeDeleteTests.cs`: 2개 (Patient-Study, Study-DoseRecord Restrict 검증)
  - **합계: 71개** (+2개)

## SWR 참조

- FDA §524B — 저장 데이터 암호화 (SQLCipher)

## 비고

- SQLCipher를 통한 저장 데이터 암호화 (FDA §524B 요구사항)
- `InternalsVisibleTo`로 테스트 프로젝트에 내부 타입 노출
- 예외 처리: `catch(Exception ex) when (ex is not OutOfMemoryException)` 패턴 적용 (AuditRepository, UserRepository, PatientRepository, StudyRepository)
