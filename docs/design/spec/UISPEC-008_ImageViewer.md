# UISPEC-008: Image Window

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | user clarification, Slides 10-11 viewer grammar, Slide 13 preview grammar |
| 갱신일 | 2026-04-09 |
| 구현 참조 | `src/HnVue.UI/Views/ImageViewerView.xaml` |

## 1. 목적

Image는 viewer 중심의 독립 창이다. Acquisition과 시각 문법을 공유하지만, 촬영 제어를 포함하지 않고 이미지 확인과 조작에 집중한다.

## 2. 설계 해석 기준

- 사용자의 최신 해석에 따라 `Image`는 top-level window다.
- PPT에는 `Image` 단독 title slide가 없으므로 viewer grammar는 Acquisition 1안/2안의 중앙 viewer와 Merge preview에서 역추적한다.
- 따라서 이 문서는 `viewer 표면의 최종 문법`을 정의하는 역할을 한다.

## 3. 레이아웃

- 상단 patient/study 정보 banner
- 중앙 large black canvas
- 우측 또는 하단 tool strip
- 필요 시 thumbnail strip

## 4. 필수 도구

- zoom
- rotate
- fit / reset
- annotation
- marker
- print / export

## 5. out of scope

다음 요소는 Image 창에 포함하지 않는다.

- 촬영 모드 선택
- generator 값 조정
- exam control rail
- acquisition-specific quick control

## 6. Coordinator handoff

- 현재 구현이 `ImageViewerView`를 Acquisition 내부 패널처럼 취급하더라도, 활성 설계는 별도 창으로 유지한다.
- 별도 navigation token 또는 화면 진입 계약이 필요하면 Coordinator가 처리한다.
