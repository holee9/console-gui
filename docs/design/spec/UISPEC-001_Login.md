# UISPEC-001: Login Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slide 1, notesSlide1 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/LoginView.xaml` |

## 1. 목적

HnVUE 진입 화면을 단순하고 빠른 인증 창으로 정의한다. 이 화면은 dark full-screen 위에 최소 요소만 배치하며, 긴 설명이나 카드형 장식을 두지 않는다.

## 2. 화면 구조

- 전체 배경은 어두운 단색 화면이다.
- 중앙에는 로고가 먼저 오고, 그 아래에 입력 2개가 수직으로 놓인다.
- 가장 아래에는 텍스트가 없는 icon confirm / cancel 버튼이 놓인다.

## 3. 필수 요소

| 요소 | 요구사항 |
|------|----------|
| Logo | 중앙 정렬, 다른 정보보다 먼저 보이도록 배치 |
| ID 입력 | notes 기준으로 dropdown list 사용 |
| Password 입력 | ID 아래 한 줄 입력 |
| Confirm | 체크 아이콘 버튼 |
| Cancel | X 아이콘 버튼 |

## 4. 상호작용

- `ID`는 자유 텍스트가 아니라 선택 가능한 사용자 목록이어야 한다.
- password 입력 후 confirm으로 인증을 시도한다.
- cancel은 화면 종료 또는 이전 상태 복귀 동작을 가진다.
- 오류 메시지는 필요 시 입력 영역 근처에만 최소한으로 표시한다.

## 5. 시각 규칙

- 배경은 very dark navy 계열 screenshot grammar를 따른다.
- 입력 필드는 밝은 면으로 배치하고, 주변 장식은 최소화한다.
- 버튼은 텍스트가 아니라 아이콘으로만 의미를 전달한다.

## 6. Coordinator handoff

- 현재 구현이 text input 중심이면 dropdown 기반 ID 선택으로 해석을 맞춰야 한다.
- Login은 독립 창이며 다른 화면의 일부 패널처럼 취급하지 않는다.
