# UISPEC-006: Merge Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slides 12-13, notesSlide3-4 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/MergeView.xaml` |

## 1. 목적

Merge는 두 환자 또는 두 study 집합을 비교하고 병합 대상을 선택하는 독립 창이다. 이 화면은 목록 비교와 preview를 동시에 수행할 수 있어야 한다.

## 2. 레이아웃

### 상단

- 파란 title bar
- `Merge`
- 우측 close

### 비교 본문

상하 또는 A/B lane 구조로 두 개의 동일한 비교 영역을 둔다.

각 lane은 아래 요소를 가진다.

- `Only WorkList` 계열 체크
- 검색 input
- patient list
- study list
- thumbnail strip
- preview panel

lane 사이에는 선택/정렬/Sync 제어를 둔다.

## 3. notes 기반 필수 규칙

- `Same Studylist` 명칭은 `Sync` 계열 명칭으로 정리한다.
- preview는 우측에서 더 크게 보여야 한다.
- 중앙에는 thumbnail 영역이 존재해야 한다.
- 목록 스타일은 Worklist/Studylist와 통일한다.

## 4. 상호작용

- patient A/B 검색
- 각 lane의 study 선택
- thumbnail 기반 비교
- sync on/off
- 병합 실행 전 preview 확인

## 5. 시각 규칙

- Worklist/Studylist와 같은 dark list surface
- 선택 row는 파란 highlight
- preview는 목록보다 더 높은 시각 우선순위를 가진다

## 6. Coordinator handoff

- 현재 구현이 단순 목록 두 개라면 thumbnail + preview 중심 구조를 보강 대상으로 본다.
- Merge는 Worklist 안 보조 패널이 아니라 별도 창이다.
