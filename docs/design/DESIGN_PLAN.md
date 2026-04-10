# HnVUE UI Design Plan

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 | PPT 최종안 refresh |
| 갱신일 | 2026-04-09 |

## 문제 정의

기존 설계 문서는 다음 세 가지 이유로 활성 기준으로 유지하기 어려웠다.

- PPT의 `변경 2안`보다 현재 구현 구조를 우선 해석했다.
- Worklist, Studylist, Image, Acquisition을 독립 창이 아닌 한 화면의 분해 패널처럼 기술한 문서가 있었다.
- HTML/Pencil mockup을 보조 자료가 아니라 사실상 기준처럼 참조하는 문서가 남아 있었다.

## 목표

이번 개정의 목표는 설계 기준을 단순하게 재정렬하는 것이다.

- PPT 최종안 중심
- 화면별 독립 창 기준
- mockup 없는 문서 운영
- Coordinator handoff 친화적인 사양 구성

## 설계 구조

### 독립 창 기준

활성 설계는 다음 순서의 창 전환을 허용한다.

`Login -> Worklist / Studylist / Image / Acquisition / Add Patient / Merge / Setting`

여기서 각 창은 top-level design unit이다. 현재 코드가 내부적으로 여러 View로 쪼개져 있어도 설계 기준은 변하지 않는다.

### 목록 계열

- Worklist
- Studylist
- Merge

이 세 화면은 목록의 tone, grid density, selection 방식, 보조 정보 표현을 동일 계열로 유지한다.

### viewer 계열

- Image
- Acquisition

이 두 화면은 viewer canvas grammar를 공유하되, Acquisition은 exam control이 포함되고 Image는 viewer 중심이라는 차이를 둔다.

### 설정 계열

- Add Patient / Procedure
- Setting

이 두 화면은 파란 title bar, form 중심 body, 명시적 action button row를 공유한다.

## 문서 세트

- 기준 해석: [PPT_ANALYSIS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/PPT_ANALYSIS.md)
- 마스터 기준: [UI_DESIGN_MASTER_REFERENCE.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/UI_DESIGN_MASTER_REFERENCE.md)
- 세부 사양: `UISPEC-001` ~ `UISPEC-009`
- handoff 기준: [DESIGN_TO_XAML_WORKFLOW.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/architecture/DESIGN_TO_XAML_WORKFLOW.md)
- 파생 리뷰: [UI_REQUIREMENTS_FROM_DOCS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/reviews/UI_REQUIREMENTS_FROM_DOCS.md)

## 구현팀 전달 경계

설계팀 문서는 아래 내용을 구현팀에 전달할 수 있다.

- 창 구조
- 시각 우선순위
- 필수 UI 요소
- 명칭 변경
- 미확정 포인트

설계팀 문서가 직접 바꾸지 않는 항목은 아래와 같다.

- ViewModel 계약
- navigation service
- DI 등록
- 실제 XAML 편집
- 테스트 코드

## 이번 refresh의 완료 조건

1. 모든 활성 UISPEC이 PPT 최종안 기준으로 다시 작성될 것
2. 구 문서와 HTML mockup이 archive로 이동될 것
3. `MainWindow 3열 셸` 전제가 활성 문서에서 제거될 것
4. 파생 리뷰 문서가 새 UISPEC과 동기화될 것
