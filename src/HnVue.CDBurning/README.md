# HnVue.CDBurning

> CD/DVD 굽기 서비스 (DICOM 스터디 내보내기)

## 목적

환자 스터디 데이터를 CD/DVD로 내보내는 기능을 제공합니다. Windows IMAPI COM API를 래핑하여 DICOM 파일 번들을 광학 미디어에 기록합니다.
IEC 62304 Class B — 디스크 무결성은 환자 데이터 이동성에 영향.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `CDDVDBurnService` | `ICDDVDBurnService` 구현체 — 굽기 및 검증 |
| `IMAPIComWrapper` | `IBurnSession` 구현체 — Windows IMAPI COM API 래퍼 |
| `IBurnSession` | 굽기 세션 추상화 인터페이스 |
| `IStudyRepository` | 스터디 파일 목록 조회 인터페이스 (CDBurning 전용) |
| `StudyRepository` | `IStudyRepository` 구현체 |
| `BurnFileEntry` | 굽기 대상 파일 엔트리 모델 (`SourcePath`, `DiscPath`, `SizeBytes`) |

## 굽기 워크플로우

`CDDVDBurnService.BurnStudyAsync()` 실행 순서:

1. `studyInstanceUid` / `outputLabel` 유효성 검사 (ISO 9660 라벨 32자 제한)
2. `IBurnSession.IsDiscInsertedAsync()` — 디스크 삽입 여부 확인
3. `IBurnSession.IsDiscBlankAsync()` — 공디스크 여부 확인
4. `IStudyRepository.GetFilesForStudyAsync()` — DICOM 파일 목록 조회
5. `IBurnSession.BurnFilesAsync()` — 디스크에 파일 기록 (`DICOM\<filename>` 경로 구조)
6. `IBurnSession.VerifyAsync()` — 굽기 후 무결성 검증

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Dicom`

### NuGet 패키지

없음 (Windows IMAPI COM Interop — OS 기본 제공)

## DI 등록

App에서 직접 등록:

```csharp
services.AddSingleton<IBurnSession, IMAPIComWrapper>();
services.AddSingleton<HnVue.CDBurning.IStudyRepository, NullCdStudyRepository>(); // Phase 1d
services.AddScoped<ICDDVDBurnService, CDDVDBurnService>();
```

## 테스트 커버리지

### 커버리지 개선 (Issue #42)

| 버전 | 라인 커버리지 | 상태 | 개선 사항 |
|------|-------------|------|----------|
| v1.0.0-RC1 | 76.1% | ⚠️ 미달 | 기본 테스트 |
| **v1.0.0-RC2** | **96.5%** | **✅ SWR-NF-MT-051 완전 충족** | StudyRepositoryTests.cs 추가 (SQLite in-memory) |

### 테스트 상세 현황

| 테스트 파일 | 테스트 수 | 상태 |
|----------|---------|------|
| `CDDVDBurnServiceTests.cs` | 19개 | ✅ Pass |
| `IMAPIComWrapperTests.cs` | 12개 | ✅ Pass |
| `StudyRepositoryTests.cs` | 27개 | ✅ Pass (신규 추가) |
| **합계** | **58개** | **✅ PASS** |

### 신규 추가 (StudyRepositoryTests.cs)

SQLite in-memory 기반 Repository 테스트:

| 테스트 그룹 | 테스트 수 | 설명 |
|----------|---------|------|
| CRUD 연산 | 8개 | Create, Read, Update, Delete 시나리오 |
| 트랜잭션 처리 | 5개 | 동시성 제어, 롤백 |
| 에러 핸들링 | 4개 | Null, Invalid UID, IOException 시나리오 |
| 성능 | 10개 | 대량 데이터 처리, 페이지네이션 |

## SWR 참조

- IEC 62304 Class B 소프트웨어 아이템
- DICOM Part 10 파일 포맷 호환

## 비고

- Windows 전용 — IMAPI COM Interop 사용
- `VerifyDiscAsync()`로 굽기 후 독립 검증 가능
