# PPT to XAML Handoff Workflow

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 갱신일 | 2026-04-09 |
| 목적 | 설계팀 문서와 Coordinator handoff 경계 정리 |

## 1. 기본 원칙

이 워크플로우의 기준 원천은 `docs/★HnVUE UI 변경 최종안_251118.pptx`다. HTML mockup, Pencil/Figma 산출물, 현재 XAML layout은 더 이상 설계 기준이 아니다.

## 2. 활성 흐름

1. PPT 원천 확인
2. [PPT_ANALYSIS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/PPT_ANALYSIS.md) 갱신
3. [UI_DESIGN_MASTER_REFERENCE.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/UI_DESIGN_MASTER_REFERENCE.md) 정렬
4. UISPEC 개정
5. [UI_REQUIREMENTS_FROM_DOCS.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/reviews/UI_REQUIREMENTS_FROM_DOCS.md) 파생 리뷰 갱신
6. Coordinator에게 handoff

## 3. 설계팀이 하지 않는 일

- ViewModel 수정
- navigation 구현
- DI 수정
- XAML 직접 수정

## 4. 현재 구현 참조 매핑

아래 표는 구현 참고용이며 설계 truth가 아니다.

| Canonical Screen | Current XAML Reference | 해석 |
|------------------|------------------------|------|
| Login | `LoginView.xaml` | direct reference |
| Worklist | `PatientListView.xaml` | 명칭은 다르지만 Worklist 창으로 해석 |
| Studylist | `StudylistView.xaml` | direct reference |
| Add Patient / Procedure | `AddPatientProcedureView.xaml` | direct reference |
| Acquisition | `WorkflowView.xaml` | top-level acquisition control shell reference |
| Image | `ImageViewerView.xaml` | viewer surface reference |
| Acquisition support | `DoseDisplayView.xaml` | acquisition 보조 fragment reference |
| Merge | `MergeView.xaml` | direct reference |
| Setting | `SettingsView.xaml` | direct reference |

## 5. 분해 규칙

- 설계상 한 창이 구현상 여러 View로 나뉘어도 창 단위 해석을 유지한다.
- 특히 Acquisition은 `Workflow + ImageViewer + DoseDisplay`의 조합으로 구현될 수 있으나, 설계 기준은 하나의 창이다.
- Image는 별도 창으로 정의하므로 Acquisition 내부 패널로 다시 축소하지 않는다.

## 6. Coordinator handoff 패키지

Coordinator에게 전달하는 내용은 아래로 제한한다.

- 창 구조와 우선순위
- 필수 UI 요소
- PPT와 현재 구현 사이의 차이
- 미확정 포인트

구현 방식 선택은 Coordinator와 구현팀 책임이다.
