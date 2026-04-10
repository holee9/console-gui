# UISPEC-005: Add Patient / Procedure Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slide 8, notesSlide2 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/AddPatientProcedureView.xaml` |

## 1. 목적

Add Patient와 Procedure 선택을 하나의 통합 창으로 제공한다. 이 창은 환자 등록과 검사 구성 입력을 한 흐름에서 끝내도록 설계한다.

## 2. 레이아웃

### title bar

- 파란 title bar
- 중앙 또는 상단에 `New Patient`
- 우측 close

### form 영역

- `Accession No`
- `Patient ID`
- `Patient Name`
- `BirthDate`
- `Age`
- `Sex`
- `Description`

### procedure 영역

- 인체 body guide
- body part list
- projection list
- 선택된 procedure / step list

### action 영역

- `Start Exam`
- `Cancel`

## 3. notes 기반 필수 규칙

- Add Patient와 Procedure는 분리 창이 아니라 하나의 창이어야 한다.
- required field는 `(*)`로 표시한다.
- `Accession No`, `Patient ID`는 auto-generate 옵션을 지원해야 한다.
- `View projection`을 지원해야 한다.
- `Description`은 dropdown 선택과 수동 입력을 함께 수용한다.
- MWL이 없어도 manual RIS Code 생성 흐름을 허용한다.

## 4. 상호작용

- body part 선택 -> projection 선택 -> selected procedure 구성
- auto-generate on/off
- manual description 입력 또는 predefined description 선택
- 입력 완료 후 `Start Exam`

## 5. 시각 규칙

- title bar는 settings/merge와 같은 파란 window grammar를 사용
- body는 dark form surface
- 리스트 3열은 명확히 분리

## 6. Coordinator handoff

- 현재 구현에서 Add Patient와 Procedure가 내부적으로 분리되어 있어도, 활성 설계 문서는 단일 창을 기준으로 유지한다.
