# HnVue.Dicom

> DICOM 네트워킹 및 파일 I/O (fo-dicom 5.x, IEC 62304 Class B)

## 목적

fo-dicom 라이브러리를 사용하여 DICOM C-STORE SCU, Modality Worklist C-FIND SCU,
MPPS SCU(N-CREATE/N-SET), Storage Commitment N-ACTION, DICOM Print SCU, 파일 읽기/쓰기 기능을 제공합니다.
`DicomOutbox`는 Polly 지수 백오프 재시도를 통해 신뢰성 있는 PACS 전송을 보장합니다.

---

## 주요 타입

| 타입 | 종류 | 설명 |
|------|------|------|
| `DicomService` | `sealed partial class` | `IDicomService` 구현체. C-STORE / C-FIND / Print / Storage Commitment 통합 서비스. 모든 DICOM 통신의 단일 엔트리 포인트 |
| `DicomOutbox` | `sealed partial class` | Polly 지수 백오프 재시도 인메모리 아웃박스 |
| `MppsScu` | `sealed class` | MPPS N-CREATE(SWR-DC-055) / N-SET(SWR-DC-056) SCU |
| `DicomStoreScu` | `sealed class` | C-STORE SCU (저수준, `IDicomNetworkConfig` 기반) |
| `DicomFindScu` | `sealed class` | ⚠️ **DEPRECATED** — Modality Worklist C-FIND SCU. `IDicomService.QueryWorklistAsync()` 사용 권장 (Issue #24, 2026-04-06) |
| `DicomFileIO` | `sealed class` (static API) | DICOM 파일 읽기(`ReadAsync`), 쓰기(`WriteAsync`), 태그 조회(`GetTagValueAsync`) |
| `DicomFileWrapper` | `sealed class` | fo-dicom `DicomFile` 래퍼 (SopInstanceUid, StudyInstanceUid, PatientName 노출) |
| `DicomOptions` | `sealed class` | DICOM 모듈 설정 바인딩 (`Dicom` 섹션) |
| `IDicomNetworkConfig` | `interface` | PACS/MWL 네트워크 설정 인터페이스 |
| `ServiceCollectionExtensions` | `static class` | `AddDicom()` 확장 메서드 |

---

## DicomService 메서드

| 메서드 | 반환 타입 | 설명 |
|--------|-----------|------|
| `StoreAsync(dicomFilePath, pacsAeTitle, CancellationToken)` | `Task<Result>` | C-STORE SCU. 파일 존재 확인 후 fo-dicom 비동기 클라이언트로 전송 |
| `QueryWorklistAsync(WorklistQuery, CancellationToken)` | `Task<Result<IReadOnlyList<WorklistItem>>>` | Modality Worklist C-FIND SCU. 날짜 범위 및 PatientId 필터 지원 |
| `PrintAsync(dicomFilePath, printerAeTitle, CancellationToken)` | `Task<Result>` | DICOM Print SCU. N-CREATE(Basic Film Session) → N-ACTION(Print) 순서 실행 |
| `RequestStorageCommitmentAsync(sopClassUid, sopInstanceUid, pacsAeTitle, CancellationToken)` | `Task<Result>` | Storage Commitment N-ACTION (SWR-DC-057/058, Issue #23). Action Type 1 전송 |

---

## MppsScu 메서드

SOP Class: `1.2.840.10008.3.1.2.3.3` (Modality Performed Procedure Step)

| 메서드 | 반환 타입 | SWR | 설명 |
|--------|-----------|-----|------|
| `SendInProgressAsync(studyInstanceUid, patientId, bodyPart, CancellationToken)` | `Task<Result<string>>` | SWR-DC-055 | N-CREATE 전송. 절차 상태를 `IN PROGRESS`로 마킹. 성공 시 MPPS SOP Instance UID 반환 |
| `SendCompletedAsync(mppsInstanceUid, bool completed, CancellationToken)` | `Task<Result>` | SWR-DC-056 | N-SET 전송. `completed=true` → `COMPLETED`, `false` → `DISCONTINUED` |

---

## DicomOutbox 재시도 정책

Polly 지수 백오프: 3회 재시도, 대기 시간 2s / 4s / 8s

재시도 대상 예외 (Issue #26):
- `InvalidOperationException` (StoreAsync 실패 래핑)
- `FellowOakDicom.Network.DicomNetworkException`
- `System.IO.IOException`
- `System.Net.Sockets.SocketException`

모든 재시도 소진 후에도 실패하면 항목은 Dead-letter 처리(폐기 및 로그 기록)됩니다.
취소 시에는 항목을 큐에 재삽입하고 처리를 중단합니다.

---

## DicomOptions 설정 항목

| 속성 | 기본값 | 설명 |
|------|--------|------|
| `LocalAeTitle` | `"HNVUE"` | 로컬 SCU AE 타이틀 |
| `PacsAeTitle` / `PacsHost` / `PacsPort` | `""` / `""` / `104` | C-STORE 대상 PACS |
| `MwlAeTitle` / `MwlHost` / `MwlPort` | `""` / `""` / `104` | Modality Worklist SCP |
| `PrinterAeTitle` / `PrinterHost` / `PrinterPort` | `""` / `""` / `104` | DICOM Print SCP |
| `MppsAeTitle` / `MppsHost` / `MppsPort` | `""` / `""` / `104` | MPPS SCP (SWR-DC-055/056) |
| `TlsEnabled` | `false` | 모든 DICOM 연결에 TLS 협상 여부 |

---

## DI 등록

```csharp
services.AddDicom(configuration);
```

`AddDicom()` 내부 동작:
- `DicomOptions` → `configuration.GetSection("Dicom")` 바인딩
- `IDicomService` → `DicomService` (Singleton)
- `DicomOutbox` (Singleton)

`MppsScu`는 DI 컨테이너에 자동 등록되지 않습니다. 호스트 애플리케이션에서 별도 등록하거나 직접 인스턴스화합니다.

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 제공 항목 |
|----------|-----------|
| `HnVue.Common` | `IDicomService`, `WorklistItem`, `WorklistQuery`, `Result<T>`, `ErrorCode` |
| `HnVue.Data` | 데이터 접근 레이어 |

### NuGet 패키지

| 패키지 | 용도 |
|--------|------|
| `fo-dicom` (5.x) | DICOM 프로토콜 처리 (C-STORE, C-FIND, N-CREATE, N-SET, N-ACTION) |
| `Polly` | DicomOutbox 지수 백오프 재시도 정책 |
| `Microsoft.Extensions.Hosting` | IOptions, ILogger 연동 |

---

## 테스트

| 항목 | 내용 |
|------|------|
| 테스트 프로젝트 | `tests/HnVue.Dicom.Tests/` |
| 테스트 파일 | `DicomFileIOTests.cs`, `DicomMappingTests.cs`, `DicomOutboxTests.cs`, `DicomServiceTests.cs`, `DicomStoreSCUTests.cs`, `ServiceCollectionExtensionsTests.cs`, `WorklistItemMappingTests.cs` |
| 테스트 케이스 수 | **60개** (`[Fact]` / `[Theory]`) |

---

## SWR 참조

| SWR ID | 대상 | 내용 |
|--------|------|------|
| SWR-DC-055 | `MppsScu.SendInProgressAsync` | MPPS N-CREATE — 절차 상태 IN PROGRESS |
| SWR-DC-056 | `MppsScu.SendCompletedAsync` | MPPS N-SET — 절차 상태 COMPLETED/DISCONTINUED |
| SWR-DC-057 | `DicomService.RequestStorageCommitmentAsync` | Storage Commitment N-ACTION 전송 |
| SWR-DC-058 | `DicomService.RequestStorageCommitmentAsync` | N-EVENT-REPORT 대기 (Phase 1: N-ACTION 응답 상태로 간소화) |
