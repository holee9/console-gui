# HnVue.Dicom

> DICOM 네트워킹 및 파일 I/O (fo-dicom 5.x)

## 목적

fo-dicom 라이브러리를 사용하여 DICOM C-STORE SCU, C-FIND SCU, 파일 읽기/쓰기 기능을 제공합니다. PACS 서버와의 통신을 담당합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `DicomStoreScu` | DICOM C-STORE SCU — 이미지 전송 |
| `DicomFindScu` | DICOM C-FIND SCU — 워크리스트 조회 |
| `DicomFileIO` | DICOM 파일 읽기/쓰기 |
| `DicomFileWrapper` | DICOM 파일 래퍼 |
| `IDicomNetworkConfig` | DICOM 네트워크 설정 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`

### NuGet 패키지

- `fo-dicom`

## DI 등록

없음 (App에서 직접 등록)

## 비고

- fo-dicom 5.x (MIT 라이선스)
- MWL (Modality Worklist) C-FIND 지원
- 예외 처리: `catch(Exception ex) when (ex is not OutOfMemoryException)` 패턴 적용 (DicomStoreScu, DicomFindScu, DicomFileIO)
