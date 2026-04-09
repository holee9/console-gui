# UISPEC-003: Studylist Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slides 5-7, Slide 7 우선 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/StudylistView.xaml` |

## 1. 목적

Studylist는 저장된 study를 탐색하고 PACS 기준으로 필터링하는 독립 창이다. Worklist와 시각 문법을 공유하지만, 화면 목적은 `study 중심 조회`에 맞춘다.

## 2. 레이아웃

### 상단 chrome

- 최상단 상태 bar
- 좌측 제목 `Studylist >>`
- 우측 시간/상태 영역

### 작업 bar

- 좌측 검색/액션 아이콘군
- `PACS` 선택 또는 표시 영역
- 이전/다음 이동 버튼 `<` `>`
- 시작일/종료일 입력
- 기간 preset 버튼: `Today`, `3Days`, `1Week`, `All`, `1Month`

### 본문

- 좌측 목록 surface
- 우측 목록 또는 study-linked surface
- Worklist/Merge와 같은 table header, row density, divider 사용

### 하단 utility

- Worklist와 동일 계열의 utility strip

## 3. 필수 데이터 요소

- `Accession No`
- `StudyDate`
- `StudyDescription`
- patient 식별 기본 정보
- PACS 기준 정보

## 4. 상호작용

- PACS 전환 또는 선택
- 이전/다음 묶음 이동
- 날짜 preset 필터
- 선택 study의 연관 내용 탐색

## 5. 통일 규칙

notes 기준으로 아래 항목은 Worklist / Studylist / Merge 사이에 통일해야 한다.

- 목록 헤더 톤
- 선택 강조 방식
- row density
- 검색/필터 bar 톤

## 6. Coordinator handoff

- Studylist는 Worklist의 우측 패널이 아니라 독립 창이다.
- 현재 구현에 PACS selector와 이전/다음 네비게이션이 없다면 설계 차이로 기록해야 한다.
