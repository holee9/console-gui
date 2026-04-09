# UISPEC-002: Worklist Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slides 2-4, Slide 4 우선 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/PatientListView.xaml` |

## 1. 목적

Worklist는 환자/검사 대기 목록을 빠르게 검색하고 연관 목록을 확인하는 독립 창이다. 앱 전체의 좌측 패널이 아니라 하나의 완결된 작업 창으로 정의한다.

## 2. 레이아웃

### 상단 chrome

- 시스템 상태 아이콘과 `HnVUE` wordmark가 있는 최상단 bar
- 좌측 제목 `Worklist >>`
- 우측 시간/상태/닫기 영역

### 작업 bar

- 좌측: modality/filter dropdown, patient search input, 검색/액션 아이콘군
- 우측: 시작일/종료일 입력, 기간 preset 버튼
- 강한 액션 버튼 `Quick Start`를 우측 상단에 배치

### 본문

- 좌측 대형 목록: patient master list
- 중앙 분리선
- 우측 대형 목록: 연관 study / detail list
- 두 목록 모두 black data surface와 밝은 header line grammar를 사용

### 하단 utility

- 좌측 `Suspend`, `Hide`
- 중앙 `Total`
- 우측 도구 아이콘군

## 3. 필수 데이터 요소

왼쪽 기본 목록은 아래 컬럼 계열을 포함해야 한다.

- `No`
- `Patient ID`
- `Patient Name`
- `Sex`
- `Age`
- `BirthDate`

Slide 4 callout text에 의해 아래 필드도 최종 grid 구성에서 지원되어야 한다.

- `Accession No`
- `Ref. Physician`
- `Exam Date`

이 세 항목은 화면 폭에 따라 primary column 또는 연관 목록/보조 컬럼으로 배치될 수 있다.

## 4. 상호작용

- patient 검색어 입력
- 기간 preset 선택: `Today`, `3Days`, `1Week`, `All`, `1Month`
- 좌측 선택과 우측 목록 동기화
- Quick Start 진입

## 5. 시각 규칙

- window surface는 `#242424` 계열
- data canvas는 `#000000`
- sub surface는 `#3B3B3B`
- selection과 주요 action은 파란 accent를 사용

## 6. 설계 해석 메모

- Worklist는 독립 창이다.
- 창 내부에 좌우 분할이 있어도 `Studylist` 창과 동일한 것으로 합치지 않는다.
- 목록 표면 문법은 Studylist, Merge와 통일한다.

## 7. Coordinator handoff

- 현재 구현이 `PatientListView`라는 이름이어도 활성 설계 명칭은 `Worklist Window`다.
- 현재 구현이 단일 목록 또는 detail panel 중심이면 dual-list grammar에 맞게 재해석해야 한다.
