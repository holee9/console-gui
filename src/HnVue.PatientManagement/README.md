# HnVue.PatientManagement

> 환자 관리 및 워크리스트 서비스

## 목적

환자 등록, 검색, 수정과 DICOM MWL(Modality Worklist) 기반 워크리스트 관리를 제공합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `PatientService` | IPatientService 구현체 — 환자 CRUD |
| `WorklistService` | IWorklistService 구현체 — 워크리스트 관리 |
| `IWorklistRepository` | 워크리스트 데이터 리포지토리 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- DICOM MWL C-FIND 결과를 로컬 워크리스트로 변환
- PatientRecord, WorklistItem 모델은 HnVue.Common에 정의
