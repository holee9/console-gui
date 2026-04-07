# UISPEC-008: 이미지 뷰어(ImageViewer) UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | v1.0 |
| 상태 | Draft |
| 작성일 | 2026-04-07 |
| PPT 참조 | Slide 10 (Acquisition 중앙 패널 이미지 뷰어) |
| HTML 목업 | `docs/ui_mockups/04-acquisition.html` (중앙 패널 부분) |
| 구현 파일 | `src/HnVue.UI/Views/ImageViewerView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/ImageViewerViewModel.cs` |
| 관련 SPEC | SPEC-UI-001 |
| 준수율 | 70% (기본 레이아웃 완료, 도구/오버레이 미구현) |

---

## 1. 화면 개요

이미지 뷰어는 촬영된 의료용 X선 이미지를 표시하고 조작하는 핵심 컴포넌트다. Acquisition 화면 중앙 패널에 임베드되며, 독립 팝업으로도 실행 가능하다.

**핵심 특징:**
- UserControl 기반 — 중앙 패널 또는 독립 윈도우에 배치
- 순수 검은 배경 (`#090909`) — CoreToken 아님, 이미지 대비 최적화
- 상단 툴바: Zoom In/Out, Reset W/L, 기타 이미지 조작 도구
- DICOM 오버레이: 환자정보(좌상단), 이미지정보(우상단)
- IEC 62366 준수: 의료 이미지 표시 품질 요구사항

**임베드 vs 독립 모드:**
- 임베드: Acquisition 화면 중앙 패널 `Grid.Row="1"`
- 독립: `Window` → `ContentControl` → `ImageViewerView`

---

## 2. 레이아웃 구조

### 2.1 전체 3행 그리드 레이아웃

```
┌──────────────────────────────────────────────────────┐
│ Row 0: Header (Auto) — "Image Viewer" 타이틀         │
├──────────────────────────────────────────────────────┤
│ Row 1: Toolbar (Auto) — Zoom In/Out, Reset W/L 등   │
├──────────────────────────────────────────────────────┤
│ Row 2: Image Display Area (*) — 이미지 + 오버레이    │
└──────────────────────────────────────────────────────┘
```

**XAML Grid RowDefinitions:**
```
Height="Auto"  <!-- Header -->
Height="Auto"  <!-- Toolbar -->
Height="*"     <!-- Image display area (Image + Border) -->
```

### 2.2 헤더 영역 (Row 0)

| 속성 | 값 |
|------|----|
| 높이 | Auto (TextBlock FontSize 16px 기준) |
| 하단 마진 | 8px |
| 타이틀 | "Image Viewer" |
| 폰트 | `HnVue.Core.FontFamily` |
| FontSize | 16px |
| FontWeight | Bold |
| 전경색 | `HnVue.Semantic.Text.Primary` (`#FFFFFF`) |

### 2.3 툴바 영역 (Row 1)

**구조:** `StackPanel(Orientation=Horizontal)`

| 속성 | 값 |
|------|----|
| 높이 | Auto (버튼 32px 기준) |
| 하단 마진 | 8px |
| Orientation | Horizontal |
| 자식 요소 간격 | 4px (Margin `0,0,4,0`) |

**툴바 버튼 공통 스타일:**

| 속성 | 값 |
|------|----|
| Style | `x:Null` (기본 스타일 재정의) |
| Background | Transparent |
| BorderBrush | `HnVue.Semantic.Text.Primary` (`#FFFFFF`) |
| BorderThickness | 1px |
| Foreground | `HnVue.Semantic.Text.Primary` (`#FFFFFF`) |
| Height | 32px |
| Padding | `12,6` |
| Cursor | Hand |

**버튼 내부 StackPanel:**
- Orientation: Horizontal
- Icon + Text 간격: 5px (Margin `0,0,5,0`)
- Icon FontFamily: Segoe MDL2 Assets
- Icon FontSize: 13px
- Text FontSize: 12px
- Text FontWeight: SemiBold

**현재 구현된 버튼:**

| 버튼 ID | 아이콘 | 텍스트 | Command |
|---------|--------|--------|---------|
| Zoom In | `&#xE8A3;` (돋보기 +) | "ZOOM IN" | `ZoomInCommand` |
| Zoom Out | `&#xE71F;` (돋보기 -) | "ZOOM OUT" | `ZoomOutCommand` |
| Reset W/L | `&#xE777;` (새로고침) | "RESET W/L" | `ResetWindowCommand` |

**줌 배율 표시:**

- "Zoom: " 라벨 (Text.Secondary `#B0BEC5`)
- 배율 값 (Text.Primary `#FFFFFF`, FontWeight SemiBold, `P0` 포맷)

### 2.4 이미지 표시 영역 (Row 2)

**구조:** `Border` + `Image` + `ProgressBar`(로딩) + `TextBlock`(에러/플레이스홀더)

**기본 Border 컨테이너:**

| 속성 | 값 |
|------|----|
| Background | `#090909` (순검정, 토큰 아님) |
| CornerRadius | 4px |
| ClipToBounds | True |
| Grid.Row | 2 |

**이유:** 의료 영상 표시 표준(AAPM)에 따른 최대 대비.

**Image 컨트롤 (이미지 로드 시):**

| 속성 | 값 |
|------|----|
| Grid.Row | 2 |
| Source | `{Binding ImageSource}` |
| Stretch | Uniform (비율 유지) |
| RenderOptions.BitmapScalingMode | HighQuality |
| Visibility | `{Binding IsImageLoaded, Converter={StaticResource BoolToVisibilityConverter}}` |

**ProgressBar (로딩 중):**

| 속성 | 값 |
|------|----|
| Grid.Row | 2 |
| IsIndeterminate | True |
| Height | 3px |
| VerticalAlignment | Top |
| Foreground | `HnVue.Semantic.Brand.Accent` (`#00AEEF`) |
| Panel.ZIndex | 10 |
| Visibility | `{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}` |

**에러 메시지 TextBlock:**

| 속성 | 값 |
|------|----|
| Grid.Row | 2 |
| Text | `{Binding ErrorMessage}` |
| Foreground | `HnVue.Semantic.Status.Emergency` (`#D50000`) |
| FontSize | Small |
| VerticalAlignment | Top |
| Margin | `0,8,0,0` |
| Panel.ZIndex | 10 |
| Visibility | `{Binding ErrorMessage, Converter={StaticResource NullToVisibilityConverter}}` |

**플레이스홀더 (이미지 없을 때):**

StackPanel 내부 (Border 내부, VerticalAlignment/HorizontalAlignment Center):

| 요소 | 값 |
|------|----|
| 아이콘 | Segoe MDL2 `&#xEB9F;`, 48px, Text.Disabled |
| 메시지 | "No image loaded. Select a patient and complete acquisition workflow." |
| 메시지 색상 | Text.Disabled (`#546E7A`) |
| TextAlignment | Center |
| TextWrapping | Wrap |
| Margin | 32px |
| Visibility | `{Binding IsImageLoaded, Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=Invert}` |

---

## 3. 이미지 도구 명세

### 3.1 현재 구현된 도구

| 도구 | ID | Command | 설명 |
|------|-----|---------|------|
| Zoom In | — | `ZoomInCommand` | 25% 단계 확대 |
| Zoom Out | — | `ZoomOutCommand` | 25% 단계 축소 |
| Reset W/L | — | `ResetWindowCommand` | 윈도우/레벨 기본값 복원 |

### 3.2 향후 구현 예정 도구

**조작 도구 (Manipulation):**

| 도구 | 아이콘 | Command | 설명 |
|------|--------|---------|------|
| Pan | `&#xE7F8;` (십자 화살표) | `PanCommand` | 드래그로 이미지 이동 |
| Rotate Left | `&#xE7AD;` (왼쪽 회전) | `RotateLeftCommand` | 90° 좌회전 |
| Rotate Right | `&#xE7AE;` (오른쪽 회전) | `RotateRightCommand` | 90° 우회전 |
| Flip Horizontal | `&#xE74E;` (좌우 반전) | `FlipHCommand` | 수평 반전 |
| Flip Vertical | `&#xE74F;` (상하 반전) | `FlipVCommand` | 수직 반전 |
| Invert | `&#xE793;` (색상 반전) | `InvertCommand` | 흑백 반전 (음화) |

**측정 도구 (Measurement):**

| 도구 | 아이콘 | Command | 설명 |
|------|--------|---------|------|
| Length | `&#xE74F;` (자) | `MeasureLengthCommand` | 직선 거리 측정 |
| Angle | `&#xE794;` (각도) | `MeasureAngleCommand` | 각도 측정 |
| ROI | `&#xE74C;` (사각형) | `MeasureRoiCommand` | ROI 밝기/면적 |
| Clear | `&#xE74A;` (지우개) | `ClearAnnotationsCommand` | 모든 측정/주석 삭제 |

**디스플레이 도구 (Display):**

| 도구 | 아이콘 | Command | 설명 |
|------|--------|---------|------|
| Grid On/Off | `&#xE893;` (격자) | `ToggleGridCommand` | 격자 오버레이 |
| Magnifier | `&#xE8A3;` (돋보기) | `MagnifierCommand` | 확대경 (2x/4x) |
| Split | `&#xE74D;` (분할) | `ToggleSplitCommand` | 2x2/1x1 전환 |

**Window/Level 조절:**

| 도구 | 컨트롤 | 설명 |
|------|--------|------|
| W/L Preset | ComboBox | Chest / Bone / Soft Tissue 등 |
| W Adjust | Slider 또는 키패드 +/- | Window 너비 조절 |
| L Adjust | Slider 또는 키패드 [ ] | Level 중간값 조절 |

---

## 4. 색상 토큰 매핑

### 4.1 사용된 토큰

| 역할 | Semantic 토큰 | Hex 값 |
|------|--------------|--------|
| 툴바 버튼 텍스트 | `HnVue.Semantic.Text.Primary` | `#FFFFFF` |
| 줌 배율 라벨 | `HnVue.Semantic.Text.Secondary` | `#B0BEC5` |
| 툴바 버튼 테두리 | `HnVue.Semantic.Text.Primary` | `#FFFFFF` |
| 로딩 인디케이터 | `HnVue.Semantic.Brand.Accent` | `#00AEEF` |
| 에러 메시지 | `HnVue.Semantic.Status.Emergency` | `#D50000` |
| 플레이스홀더 아이콘/텍스트 | `HnVue.Semantic.Text.Disabled` | `#546E7A` |

### 4.2 토큰이 아닌 고정값

| 요소 | 값 | 이유 |
|------|----|----|
| 이미지 표시 영역 배경 | `#090909` | 의료 영상 대비 최적화 (AAPM 표준) |

---

## 5. 상태 디자인

### 5.1 이미지 로드 상태

| 상태 | 시각적 표현 |
|------|------------|
| 로딩 중 | 상단 3px 높이 ProgressBar(Accent 색) + 기존 이미지 흐리게 처리 |
| 로딩 성공 | Image 컨트롤 표시 (Stretch=Uniform) |
| 로딩 실패 | 빨간색 에러 메시지 + 빈 Border 배경 |
| 이미지 없음 | 플레이스홀더 아이콘(48px) + 안내 텍스트 |

### 5.2 툴 활성화 상태

| 도구 | 활성 상태 | 비활성 상태 |
|------|----------|------------|
| Zoom In/Out | Border 1px White + White Text | Border 1px `#546E7A` + `#546E7A` Text |
| Pan | 선택 시 하이라이트(하단 2px Accent) | 기본 상태 |
| Measure | 선택 시 하이라이트 + 커서 변경(십자) | 기본 상태 |
| Invert | 활성 시 버튼 배경 Accent | 기본 상태 |

### 5.3 줌 상태

| 배율 | 버튼 상태 | 표시 |
|------|----------|------|
| 50% | Zoom In만 활성 | "Zoom: 50%" |
| 100% | 둘 다 활성 | "Zoom: 100%" |
| 400% | Zoom Out만 활성 | "Zoom: 400%" |

### 5.4 DICOM 오버레이 상태 (향후 구현)

| 요소 | 위치 | 내용 | 스타일 |
|------|------|------|-------|
| 환자 정보 | 좌상단 (10,10) | 환자명 / ID / 생년월일 | FontSize 11px, SemiBold, `#FFFFFF`, 반투명 배경 `rgba(0,0,0,0.6)` |
| 검사 정보 | 우상단 (Top=10, Right=10) | 검사일 / Modality / Body Part | FontSize 11px, 우측 정렬, `#FFFFFF`, 반투명 배경 |
| W/L 값 | 좌하단 | W: [width] / L: [level] | FontSize 12px, Bold, `#00AEEF` |
| 줌 배율 | 우하단 | [zoom]% | FontSize 12px, `#FFFFFF` |
| 측정값 | 측정 위치 근처 | Length: [mm] / Angle: [deg] | FontSize 10px, `#F9E04B` (황색), 테두리 1px `#F9E04B` |

---

## 6. IEC 62366 의료 이미지 표시 요구사항

### 6.1 영상 품질 (Medical Image Quality)

| 요구사항 | 구현 방식 |
|---------|----------|
| 최대 대비 | `#090909` 배경 (토큰 아님, 하드코딩) |
| 보간 품질 | `RenderOptions.BitmapScalingMode="HighQuality"` |
| 축/회전 시 손실 방지 | ViewModel에서 원본 Source 유지, Transform만 적용 |
| 모니터 보정 | (향후 구현) DICOM GSDF 적용 |

### 6.2 인체공학 (Ergonomics)

| 요구사항 | 구현 방식 |
|---------|----------|
| 터치 타겟 최소 크기 | 버튼 32px 이상 (현재 32px 준수) |
| 색상 대비 | White(`#FFFFFF`) on Black(`#090909`) = 21:1 (AAA) |
| 포커스 표시 | 키보드 탭 이동 시 선택된 도구 하이라이트 |

### 6.3 안전 (Safety)

| 요구사항 | 구현 방식 |
|---------|----------|
| 잘못된 이미지 방지 | 이미지 없을 때 플레이스홀더 표시 |
| 로딩 상태 피드백 | ProgressBar + IsBusy 바인딩 |
| 에러 표시 | 빨간색(`#D50000`) 에러 메시지 |
| 중요 작업 취소 가능 | ESC 키로 Pan/Measure 도구 종료 |

---

## 7. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 설명 | 우선순위 | 구현 상태 |
|--------|--------------|---------|----------|
| MR-IV-001 | 촬영 X선 이미지 표시 | Tier1 | 완료 (Image 컨트롤) |
| MR-IV-002 | Window/Level 조절 지원 | Tier1 | 부분 (Reset만 구현, 슬라이더 미구현) |
| MR-IV-003 | Zoom/Pan 지원 | Tier2 | 부분 (Zoom만 구현, Pan 미구현) |
| MR-IV-004 | DICOM 메타데이터 오버레이 | Tier1 (규제) | 미구현 |
| MR-IV-005 | 이미지 주석 도구 | Tier3 | 미구현 |

---

## 8. 구현 갭 분석

| 갭 항목 | 현재 상태 | 설계 목표 | 우선순위 |
|---------|----------|----------|---------|
| Pan 도구 | 미구현 | 드래그로 이미지 이동 | P1 |
| Rotate/Flip 도구 | 미구현 | 90° 회전, 수평/수직 반전 | P2 |
| Invert 도구 | 미구현 | 흑백 반전 (음화) | P2 |
| Window/Level 슬라이더 | 미구현 | W/L 실시간 조절 (MR-IV-002) | P1 |
| 측정 도구 | 미구현 | Length/Angle/ROI | P3 |
| DICOM 오버레이 | 미구현 | 환자정보/검사정보 (MR-IV-004) | P1 |
| 격자 오버레이 | 미구현 | Grid 토글 | P3 |
| 확대경 | 미구현 | Magnifier 2x/4x | P3 |
| 2x2/1x1 전환 | 미구현 | 다중 이미지 레이아웃 | P2 |
| 주석 저장/로드 | 미구현 | DICOM PR-TAG 적용 | P3 |

---

## 9. 개선 우선순위

**P1 (릴리즈 블로커):**
1. Window/Level 슬라이더 구현 — MR-IV-002 Tier1
2. Pan 도구 구현 — 기본 이미지 이동
3. DICOM 오버레이 구현 — MR-IV-004 Tier1 규제

**P2 (다음 릴리즈):**
1. Rotate/Flip 도구
2. 2x2/1x1 레이아웃 전환
3. W/L 프리셋 (Chest/Bone/Soft Tissue)

**P3 (백로그):**
1. 측정 도구 (Length/Angle/ROI) — MR-IV-005 Tier3
2. 확대경
3. 주석 저장 (DICOM PR-TAG)
