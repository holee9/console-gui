# UISPEC-004: Acquisition Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | Slides 9-11, Slide 11 우선 |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/WorkflowView.xaml` |

## 1. 목적

Acquisition은 촬영 제어와 viewer를 결합한 독립 작업 창이다. 이 화면은 exam control이 포함된 operational screen이며, Image window와는 구분한다.

## 2. 구조

### 상단

- 최상단 시스템 bar
- 환자 정보 banner
  - `ID`
  - 환자 이름
  - `BirthDate`
  - `Sex`

### 좌측 control rail

- 촬영 모드 탭 예: `Stand`, `Table`, `Portable`
- position / procedure tree
- generator mini control table
- 저장/모드/노출 관련 quick icon strip

### 중앙 viewer area

- large black canvas
- 촬영 중인 이미지 또는 대기 canvas 표시
- viewer surface grammar는 [UISPEC-008_ImageViewer.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/spec/UISPEC-008_ImageViewer.md)와 공유

### 우측 tool rail

- marker / annotation / zoom / rotate / print / save 계열 도구
- dark panel 위 white icon grammar 사용

## 3. 필수 요소

| 요소 | 요구사항 |
|------|----------|
| Patient banner | 상단 고정, 한눈에 식별 가능 |
| Procedure tree | 좌측 rail에 항상 존재 |
| Viewer canvas | 중앙 최대 면적 확보 |
| Tool rail | 우측 분리 영역 또는 overlay로 유지 |

## 4. 상호작용

- procedure 선택
- generator 값 조정
- viewer 조작
- annotation 및 marker 입력
- 저장/출력/보조 작업 실행

## 5. 분리 규칙

- Acquisition은 `촬영 제어 창`이다.
- Image는 `viewer 중심 창`이며 Acquisition과 동일하지 않다.
- 현재 구현이 여러 View로 분해되어 있어도 설계상 Acquisition은 하나의 top-level window다.

## 6. Coordinator handoff

- 현재 구현 분해는 `WorkflowView + ImageViewerView + DoseDisplayView`일 수 있으나, 설계 기준은 하나의 Acquisition 창이다.
- patient banner의 상단 배치와 viewer 중심 레이아웃은 필수다.
