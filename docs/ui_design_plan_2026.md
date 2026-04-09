# HnVUE UI Design Plan 2026

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | `docs/★HnVUE UI 변경 최종안_251118.pptx` |
| 갱신일 | 2026-04-09 |
| 작성 범위 | 디자인 계획, 화면 사양, Coordinator handoff |

## 목적

이 문서는 `변경 2안`이 있는 화면은 해당 `2안`을 최종안으로, `2안`이 없는 화면은 PPT 최종 슬라이드와 notes를 최종안으로 간주하여 HnVUE UI의 활성 설계 기준을 재정의한다.

이번 개정의 핵심은 다음 네 가지다.

- `MainWindow 3열 셸` 해석을 폐기하고 화면별 독립 창 구조를 기준으로 삼는다.
- HTML/Pencil/Figma 목업을 활성 설계 자산에서 제외한다.
- 현재 XAML 구조는 참고 정보로만 남기고 설계 기준으로 사용하지 않는다.
- 구현 변경은 하지 않고 Coordinator 전달용 설계 문서만 정비한다.

## 활성 기준 문서

- [PPT_ANALYSIS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/PPT_ANALYSIS.md)
- [UI_DESIGN_MASTER_REFERENCE.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/UI_DESIGN_MASTER_REFERENCE.md)
- [DESIGN_PLAN.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/DESIGN_PLAN.md)
- [DESIGN_TO_XAML_WORKFLOW.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/architecture/DESIGN_TO_XAML_WORKFLOW.md)
- [UI_REQUIREMENTS_FROM_DOCS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/reviews/UI_REQUIREMENTS_FROM_DOCS.md)

## 활성 화면 범위

| 화면 | 상태 | 근거 |
|------|------|------|
| Login | Active | Slide 1, notesSlide1 |
| Worklist | Active | Slides 2-4, `2안` 우선 |
| Studylist | Active | Slides 5-7, `2안` 우선 |
| Add Patient/Procedure | Active | Slide 8, notesSlide2 |
| Acquisition | Active | Slides 9-11, `2안` 우선 |
| Image | Active | 사용자 해석 고정 + Slides 10-11 viewer grammar |
| Merge | Active | Slides 12-13, `1안` 최종 |
| Setting | Active | Slides 14-21 |
| SystemAdmin | Out of scope | 본 PPT 최종안 범위 외 |

## 2026 실행 항목

1. 설계 기준 문서 유지
   - PPT 해석, 마스터 레퍼런스, UISPEC, 파생 리뷰를 동기화 상태로 유지한다.
2. Coordinator handoff 정리
   - 화면별 설계 의도, 창 단위 해석, 미확정 포인트만 전달한다.
3. 구현팀 분리 유지
   - `src/` 하위 구현 변경은 이 워크트리에서 수행하지 않는다.
4. 설계 변경 이력 관리
   - 설계 변경은 [UI_CHANGELOG.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/UI_CHANGELOG.md)에 기록한다.

## 금지 사항

- HTML mockup을 새 활성 기준으로 삼지 않는다.
- 현재 XAML 배치를 PPT보다 우선 해석하지 않는다.
- ViewModel, 인터페이스, DI, navigation contract를 이 워크트리에서 수정하지 않는다.

## 산출물 패키지

- 설계 기준 문서 4종
- UISPEC 001~008 개정본
- UISPEC-009 범위외 안내본
- 파생 리뷰 문서 1종
- mockup archive 안내 문서 1종
