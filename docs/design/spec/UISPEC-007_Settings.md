# UISPEC-007: Setting Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slides 14-21 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/SettingsView.xaml` |

## 1. 목적

Setting은 시스템 구성 정보를 관리하는 독립 창이다. 좌측 섹션 네비게이션과 우측 편집 영역을 갖는 관리형 창으로 정의한다.

## 2. 기본 구조

### 상단

- 파란 title bar
- `Setting`
- 우측 close

### 좌측 navigation

슬라이드군에서 확인되는 top-level section은 아래 계열이다.

- `System`
- `Account`
- `Detector`
- `Generator`
- `PACS`
- `Worklist`
- `Print`
- `Option`
- `Display`
- `DicomSet`
- `RIS Code`
- `AI`

### 우측 content

- 선택 section의 table / form / edit surface
- dark background와 밝은 table header

### 하단 action bar

- `Add`
- `Edit`
- `Delete`
- 기타 section-specific action
- 우측 하단 `Save`

## 3. 세부 규칙

- 좌측 navigation은 항상 고정되어야 한다.
- RIS Code와 Procedure Step은 별도 편집 subview를 지원해야 한다.
- slides 15-21의 notes는 section 내부 항목 재배치와 명칭 정리를 요구한다.
- network 성격의 설정은 PACS / Worklist / Print cluster 또는 그 하위 편집 흐름으로 해석한다.

## 4. 상호작용

- section 전환
- table row 선택
- add / edit / delete
- 저장
- 세부 편집 창 진입

## 5. 시각 규칙

- Add Patient, Merge와 동일한 파란 title bar grammar
- 좌측 navigation은 dark tab block
- 우측 content는 dense data-entry surface

## 6. Coordinator handoff

- 본 refresh에서는 `Setting`만 활성 범위다.
- `SystemAdmin`이 필요한 기능은 별도 source 없이는 이 UISPEC으로 흡수하지 않는다.
