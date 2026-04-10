# HnVUE UI Design Master Reference

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 | `docs/★HnVUE UI 변경 최종안_251118.pptx` |
| 갱신일 | 2026-04-09 |

## 목적

이 문서는 HnVUE UI의 활성 설계 기준을 한 곳에서 정리하는 마스터 레퍼런스다. 현재 구현 코드나 과거 HTML 목업이 아니라, PPT 최종안과 그 해석 결과를 기준으로 한다.

## 설계 원천 우선순위

1. 승인된 사용자 지시
2. `docs/★HnVUE UI 변경 최종안_251118.pptx`
3. [PPT_ANALYSIS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/PPT_ANALYSIS.md)
4. 개정된 UISPEC 문서
5. 현재 XAML 구현 구조

## 활성 창 인벤토리

| 창 | 활성 문서 | 원천 | 비고 |
|----|-----------|------|------|
| Login | UISPEC-001 | Slide 1 | note로 ID dropdown 요구 |
| Worklist | UISPEC-002 | Slides 2-4 | `2안` 최종 |
| Studylist | UISPEC-003 | Slides 5-7 | `2안` 최종 |
| Acquisition | UISPEC-004 | Slides 9-11 | `2안` 최종 |
| Add Patient/Procedure | UISPEC-005 | Slide 8 | 단일 통합 창 |
| Merge | UISPEC-006 | Slides 12-13 | `1안` 최종 |
| Setting | UISPEC-007 | Slides 14-21 | 독립 설정 창 |
| Image | UISPEC-008 | user clarification + viewer grammar | Acquisition과 분리된 top-level window |
| SystemAdmin | UISPEC-009 | 본 PPT 외 | active baseline 밖 |

## 전역 구조 규칙

### 1. 창 단위 규칙

- 대형 화면은 하나의 앱 셸 안 패널이 아니라 개별 창으로 다룬다.
- 각 창은 자체 title, toolbar, body, utility area를 가질 수 있다.
- 한 창 내부에서 좌우 분할이 있더라도 그것은 해당 창의 내부 레이아웃이지 앱 전역 셸이 아니다.

### 2. 목록 화면 규칙

Worklist, Studylist, Merge는 다음 속성을 공유한다.

- dark window background
- black data surface
- 밝은 헤더/그리드 구분선
- 상단 검색/필터/기간 제어
- list spacing, row density, selection highlight의 일관성

### 3. viewer 화면 규칙

Image와 Acquisition은 다음 속성을 공유한다.

- 중앙 black canvas
- white iconography
- dark control rail
- 환자/검사 정보의 상단 고정 표시

## 전역 visual grammar

### 확인된 색상

| 의미 | 값 | 근거 |
|------|----|------|
| Window Surface | `#242424` | Worklist 2안 callout |
| Viewer / Data Canvas | `#000000` | Worklist 2안 callout, viewer screenshots |
| Sub Surface | `#3B3B3B` | Worklist 2안 callout |
| Text / Icon Primary | White 계열 | 모든 screenshot |
| Accent Blue | 슬라이드 기반 파란 선택색/타이틀바 | Add Patient, Merge, Setting screenshots |
| Warning / Action Accent | Quick Start red 계열 | Worklist/Studylist screenshots |

정확한 파란색 토큰 값은 PPT 텍스트로 제공되지 않으므로 구현팀은 캡처 기반 샘플링 또는 Coordinator 합의 후 토큰화해야 한다.

### typography

- 대화면 title은 크고 무겁게 배치한다.
- 데이터 헤더와 본문은 compact density를 유지한다.
- 환자 식별 정보는 일반 본문보다 한 단계 크게 둔다.

### spacing

- 목록형 화면은 좁은 row density를 유지한다.
- modal/form 화면은 label-field 간격을 명확히 유지한다.
- viewer 화면은 툴 아이콘 간격보다 canvas 확보를 우선한다.

## 문서 운영 규칙

- HTML mockup은 참조하지 않는다.
- Pencil/Figma 산출물은 활성 기준이 아니다.
- 현재 XAML과 문서가 충돌할 때는 문서를 우선한다.
- 구현에 필요한 변경 요청은 Coordinator 전달 항목으로만 남긴다.

## archive 정책

이전 활성 문서와 mockup은 아래 스냅샷으로 이동되었다.

- `docs/archive/2026-04-09_ui-design_pre-ppt251118`
