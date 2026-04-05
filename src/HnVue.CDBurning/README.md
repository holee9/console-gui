# HnVue.CDBurning

> CD/DVD 굽기 서비스 (DICOM 스터디 내보내기)

## 목적

환자 스터디 데이터를 CD/DVD로 내보내는 기능을 제공합니다. Windows IMAPI COM API를 래핑하여 DICOM 파일 번들을 광학 미디어에 기록합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `CDDVDBurnService` | CD/DVD 굽기 서비스 구현체 |
| `BurnFileEntry` | 굽기 대상 파일 엔트리 모델 |
| `IMAPIComWrapper` | Windows IMAPI COM API 래퍼 |
| `IBurnSession` | 굽기 세션 추상화 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Dicom`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- Windows 전용 — IMAPI COM Interop 사용
- IEC 62304 Class B 소프트웨어 아이템
