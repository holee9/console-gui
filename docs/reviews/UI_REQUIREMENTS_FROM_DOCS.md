# UI Requirements from Active Design Docs

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 문서 | refreshed UISPEC set |
| 갱신일 | 2026-04-09 |

## 확정 요구사항

### 창 구조

- Worklist, Studylist, Image, Acquisition은 독립 창으로 취급한다.
- Add Patient / Procedure는 단일 통합 창이다.
- Merge와 Setting은 별도 관리 창이다.

### 목록 계열

- Worklist / Studylist / Merge는 동일한 목록 visual grammar를 공유한다.
- Worklist와 Studylist는 기간 preset 버튼을 가진다.
- Studylist는 PACS와 이전/다음 이동을 지원한다.

### viewer 계열

- Acquisition은 patient banner + control rail + viewer canvas 구조다.
- Image는 acquisition control이 없는 viewer 중심 창이다.
- Merge는 thumbnail과 preview를 함께 제공한다.

### form / setting 계열

- Add Patient / Procedure는 required 표시, auto-generate, projection selection을 포함한다.
- Setting은 좌측 section navigation과 우측 content area 구조를 가진다.

## Coordinator 확인 항목

아래 항목은 활성 설계 해석은 정했지만 구현 계약으로 내려가기 전에 Coordinator 확인이 필요하다.

| 항목 | 이유 |
|------|------|
| Image 창 진입 방식 | PPT에 단독 title slide가 없어 navigation 계약 확인 필요 |
| Worklist 컬럼 배치 | `Accession No`, `Ref. Physician`, `Exam Date`의 정확한 위치 확정 필요 |
| Studylist의 우측 surface 구성 | screenshot과 text callout 사이 상세 배치 보정 필요 |
| Setting section naming | PACS/Worklist/Print cluster와 notes의 network wording 정합성 확인 필요 |

## 폐기된 기준

- HTML mockup 기반 요구 도출
- Pencil/Figma 산출물 기반 요구 도출
- 현재 XAML 구조를 기준으로 한 요구 해석
