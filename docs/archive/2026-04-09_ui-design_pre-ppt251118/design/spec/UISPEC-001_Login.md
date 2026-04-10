# UISPEC-001: 로그인 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | v1.0 |
| 상태 | Draft |
| 작성일 | 2026-04-07 |
| PPT 참조 | Slide 2 (원본 스크린샷 분석), Slide 3 (토큰 명세) |
| HTML 목업 | `docs/ui_mockups/01-login.html` |
| 구현 파일 | `src/HnVue.UI/Views/LoginView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/LoginViewModel.cs` |
| 관련 SPEC | SPEC-UI-001 |
| 준수율 | 95% (Slide 2 원본 기준) |

---

## 1. 화면 개요

로그인 화면은 HnVue 애플리케이션의 진입점이다. 사용자 인증(사용자명 + 비밀번호)을 처리하며, 인증 성공 시 메인 워크리스트 화면으로 전환된다.

**핵심 특징:**
- WindowStyle=None, AllowsTransparency=True — 타이틀바 없음, 크롬 없음
- 전체 화면을 단일 다크 네이비 배경 (#1c2333)으로 채우는 풀스크린 디자인
- 중앙 정렬된 컴팩트 폼 (Width=220px)
- 레이블 없음, 플레이스홀더 없음 — 극도의 미니멀 UI
- 아이콘 전용 버튼 (텍스트 없음)

---

## 2. 디자인 변경 이력 (원본 vs 재디자인)

### 2.1 원본 디자인 (Slide 2 — 실제 스크린샷 분석)

PPT Slide 2는 실제 HnVue 애플리케이션 스크린샷을 분석하여 작성된 XAML 명세다.

| 항목 | 원본 값 |
|------|---------|
| 창 스타일 | WindowStyle=None, AllowsTransparency=True |
| 창 크기 | 1280x800 (또는 전체화면) |
| 배경 | `#1c2333` 단색 전체 채움 |
| 컨테이너 | 카드/모달 없음 — 배경 위에 직접 배치 |
| 폼 너비 | 220px StackPanel |
| 사용자명 입력 | TextBox (드롭다운 아님), 레이블 없음 |
| 비밀번호 입력 | PasswordBox, 레이블 없음 |
| 버튼 | 아이콘 전용 (Path 체크마크 + X), 텍스트 없음 |
| 확인 버튼 크기 | 40x40px |
| 취소 버튼 크기 | 40x40px |
| 버튼 간격 | 24px (Margin=0,0,24,0) |
| 입력 배경 | `#ffffff` (순백색) |
| 입력 텍스트 | `#1c2333` |
| 포커스 테두리 | `#1a6fc7` 2px |
| 기본 테두리 | `#cccccc` 1px |

**슬라이드 2 버튼 Path 데이터:**
- 확인 (체크마크): `M4,10 L8,14 L16,6` — Stroke=White, StrokeThickness=2.5
- 취소 (X 마크): `M4,4 L16,16 M16,4 L4,16` — Stroke=White, StrokeThickness=2.5

### 2.2 재디자인 명세 (Slide 3 — 이전 설계 시도)

PPT Slide 3는 실제 스크린샷 이전에 작성된 추정 기반 설계안이다. 현재 코드베이스에는 **적용되어 있지 않다.**

| 항목 | 재디자인 값 (미적용) |
|------|---------------------|
| 컨테이너 | 320px 카드 + 드롭섀도 |
| 섹션 배지 | "로그인 창" 황색 배지 |
| 사용자명 | 레이블 "사용자 명" + ComboBox (드롭다운) |
| 비밀번호 | 레이블 "비밀번호" + PasswordBox |
| 버튼 | 텍스트 버튼 "확인" + "취소" |

### 2.3 디자인 결정 근거

Slide 3의 카드/레이블/ComboBox 설계는 실제 스크린샷 분석 결과(Slide 2)에 의해 **번복되었다.** Slide 2 NOTE-6에 명시적으로 기록됨:

> "이전 분석(슬라이드 1 PPT 이미지)에서 ComboBox, 섹션 레이블 등을 추정 작성했으나 실제 화면은 훨씬 심플한 미니멀 디자인임. 이 스펙이 실제 기준."

**현재 구현 방향: Slide 2 원본 명세 (미니멀 디자인)**

---

## 3. 레이아웃 명세

### 3.1 전체 구조

```
Window (WindowStyle=None, AllowsTransparency=True, 1280x800)
└── Grid Background=#1c2333
    └── StackPanel (VAlign=Center, HAlign=Center, Width=220)
        ├── [1] Image — HnVue 로고 (160x~80px)
        │       Margin=0,0,0,80
        ├── [2] TextBox — 사용자명
        │       Height=36, Margin=0,0,0,4
        ├── [3] PasswordBox — 비밀번호
        │       Height=36, Margin=0,0,0,16
        ├── [4] TextBlock — 오류 메시지 (Visibility=Collapsed 기본)
        │       Foreground=#e74c3c, FontSize=11
        ├── [5] StackPanel (HAlign=Center, Orientation=Horizontal)
        │       ├── Button 확인 (40x40, Margin=0,0,24,0, IsDefault=True)
        │       │   └── Path M4,10 L8,14 L16,6
        │       └── Button 취소 (40x40, IsCancel=True)
        │           └── Path M4,4 L16,16 M16,4 L4,16
        └── [6] ProgressBar (Height=2, IsIndeterminate, 로딩 중 표시)
```

### 3.2 컴포넌트별 명세

#### 로고 이미지

| 속성 | 값 |
|------|-----|
| Source | `/Resources/Images/hnvue_logo.png` |
| Width | 160px |
| Height | ~80px (2:1 비율) |
| HorizontalAlignment | Center |
| Margin | `0,0,0,80` |
| RenderOptions | BitmapScalingMode=HighQuality |
| 설명 | "Hn" 심볼 (세로 막대 2개 + 호) + "VUE" 텍스트, 3D 실버/흰색 계열 그래픽 이미지 |

#### 사용자명 TextBox

| 속성 | 값 |
|------|-----|
| Style | `HnVue.Login.TextBoxStyle` |
| Height | 36px |
| Margin | `0,0,0,4` |
| Padding | `8,0` |
| Background | `#ffffff` |
| Foreground | `#1c2333` |
| BorderBrush (기본) | `#cccccc` (1px) |
| BorderBrush (포커스) | `#1a6fc7` (2px) |
| FontFamily | Malgun Gothic |
| FontSize | 13 |
| CaretBrush | `#1c2333` |
| TabIndex | 0 |
| 레이블 | 없음 |
| 플레이스홀더 | 없음 |
| Binding | `{Binding Username, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}` |

#### 비밀번호 PasswordBox

| 속성 | 값 |
|------|-----|
| Style | `HnVue.Login.PasswordBoxStyle` |
| Height | 36px |
| Margin | `0,0,0,16` |
| Padding | `8,0` |
| Background | `#ffffff` |
| Foreground | `#1c2333` |
| BorderBrush (기본) | `#cccccc` (1px) |
| BorderBrush (포커스) | `#1a6fc7` (2px) |
| FontFamily | Malgun Gothic |
| FontSize | 13 |
| PasswordChar | 기본 불릿 (•) |
| TabIndex | 1 |
| 코드비하인드 | `PasswordChanged` 이벤트 → ViewModel 전달 |

#### 확인 버튼 (아이콘 전용)

| 속성 | 값 |
|------|-----|
| Style | `HnVue.Login.IconButtonStyle` |
| Width | 40px |
| Height | 40px |
| Margin | `0,0,24,0` |
| Background | Transparent |
| Border | 없음 |
| IsDefault | True (Enter 키 트리거) |
| TabIndex | 2 |
| 아이콘 Path | `M4,10 L8,14 L16,6` |
| 아이콘 Stroke | White, StrokeThickness=2.5 |
| 아이콘 크기 | 20x20px, Stretch=Uniform |
| Hover 배경 | `#33ffffff` (반투명 흰색 20%) |
| Pressed 배경 | `#55ffffff` (반투명 흰색 33%) |
| Command | `{Binding LoginCommand}` |

#### 취소 버튼 (아이콘 전용)

| 속성 | 값 |
|------|-----|
| Style | `HnVue.Login.IconButtonStyle` |
| Width | 40px |
| Height | 40px |
| IsCancel | True (Escape 키 트리거) |
| TabIndex | 3 |
| 아이콘 Path | `M4,4 L16,16 M16,4 L4,16` |
| 아이콘 Stroke | White, StrokeThickness=2.5 |
| 아이콘 크기 | 20x20px, Stretch=Uniform |
| Hover/Pressed | 확인 버튼과 동일 |
| Command | `{Binding CancelCommand}` |

#### 오류 메시지 TextBlock

| 속성 | 값 |
|------|-----|
| Foreground | `#e74c3c` |
| FontSize | 11 |
| FontFamily | Malgun Gothic |
| TextWrapping | Wrap |
| Margin | `0,0,0,8` |
| 기본 가시성 | Collapsed (ErrorMessage가 null/빈 문자열일 때) |
| Binding | `{Binding ErrorMessage}` |
| Visibility Converter | `NullToVisibilityConverter` |

#### 로딩 인디케이터 ProgressBar

| 속성 | 값 |
|------|-----|
| IsIndeterminate | True |
| Height | 2px |
| Margin | `0,8,0,0` |
| Background | Transparent |
| Foreground | `#1a6fc7` |
| Visibility | `{Binding IsLoading}` via `BoolToVisibilityConverter` |

---

## 4. 색상 토큰 매핑

### Slide 2/3 원시 토큰 → WPF 토큰 매핑

| 토큰 이름 (PPT) | Hex 값 | WPF 토큰 키 | 사용처 |
|----------------|--------|------------|--------|
| BgLogin | `#1c2333` | — (하드코딩) | 전체 창 배경 |
| InputBgLogin | `#ffffff` | `HnVue.Login.InputBg` | TextBox/PasswordBox 배경 |
| InputFgLogin | `#1c2333` | `HnVue.Login.InputFg` | 입력 텍스트 색 |
| InputBorderDefault | `#cccccc` | `HnVue.Login.InputBorder` | 기본 테두리 |
| InputBorderFocus | `#1a6fc7` | `HnVue.Login.InputFocusBorder` | 포커스 테두리 |
| BtnIconColor | `#ffffff` | — (Path Stroke=White) | 아이콘 색상 |
| BtnHoverBg | `#33ffffff` | — (Style Trigger) | 버튼 호버 배경 |
| BtnPressedBg | `#55ffffff` | — (Style Trigger) | 버튼 클릭 배경 |
| — | `#e74c3c` | `HnVue.Semantic.Status.Emergency` | 오류 메시지 |
| — | `#1a6fc7` | `HnVue.Semantic.Brand.Primary` | 로딩바 색상 |

### 현재 LoginView.xaml의 토큰 사용 현황

- `HnVue.Login.TextBoxStyle` — TextBox 스타일 (토큰 번들)
- `HnVue.Login.PasswordBoxStyle` — PasswordBox 스타일 (토큰 번들)
- `HnVue.Login.IconButtonStyle` — 아이콘 버튼 스타일 (토큰 번들)
- `NullToVisibilityConverter` — 오류 메시지 가시성 변환
- `BoolToVisibilityConverter` — 로딩 인디케이터 가시성 변환

---

## 5. 상태 디자인

### 5.1 기본 상태 (Default)

| 요소 | 외관 |
|------|------|
| 창 배경 | `#1c2333` 단색 |
| 사용자명 TextBox | 흰 배경, `#cccccc` 1px 테두리 |
| 비밀번호 PasswordBox | 흰 배경, `#cccccc` 1px 테두리 |
| 확인 버튼 | Transparent 배경, 흰 체크마크 아이콘 |
| 취소 버튼 | Transparent 배경, 흰 X 아이콘 |
| 오류 메시지 | Collapsed (숨김) |
| 로딩바 | Collapsed (숨김) |

### 5.2 포커스 상태 (Focused)

| 요소 | 외관 |
|------|------|
| 포커스된 입력 필드 | `#1a6fc7` 2px 테두리 (파란색) |
| 탭 순서 | Username(0) → Password(1) → Confirm(2) → Cancel(3) |
| 창 오픈 시 초기 포커스 | UsernameBox |

### 5.3 오류 상태 (Error)

| 조건 | 동작 |
|------|------|
| 빈 사용자명 또는 비밀번호 | ErrorMessage 설정, 오류 TextBlock 표시 |
| 인증 실패 | ErrorMessage 설정 (예: "사용자명 또는 비밀번호가 잘못되었습니다") |
| 오류 표시 색상 | `#e74c3c` (빨간색) |

### 5.4 로딩 상태 (Loading)

| 요소 | 외관 |
|------|------|
| 로딩바 | Visible, IsIndeterminate=True, `#1a6fc7` 색상 |
| 버튼 | 로딩 중 비활성화 처리 (IsEnabled 바인딩) |

### 5.5 인터랙션 흐름

| 트리거 | 동작 |
|--------|------|
| 창 열기 | UsernameBox.Focus() 호출 |
| Enter 키 | LoginCommand 실행 (IsDefault=True) |
| Escape 키 | CancelCommand 실행 → Application.Shutdown() |
| 마우스 드래그 | OnMouseLeftButtonDown → DragMove() (타이틀바 없어 전체 배경 드래그 가능) |
| 확인 클릭 | 유효성 검사 → AuthService.Login(user, pwd) |
| 취소 클릭 | Application.Current.Shutdown() |

---

## 6. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 | 티어 | 화면 요소 | 구현 상태 |
|--------|---------|------|----------|----------|
| MR-UI-001 | 시스템은 사용자 인증 인터페이스를 제공해야 한다 | Tier 1 | LoginView 전체 | 구현됨 |
| MR-UI-002 | 로그인 화면은 사용자 ID 드롭다운 선택을 지원해야 한다 | Tier 2 | 사용자명 입력 필드 | 미구현 (TextBox, ComboBox 아님) |
| MR-UI-003 | 로그인 화면은 비밀번호 입력을 지원해야 한다 | Tier 2 | PasswordBox | 구현됨 |
| MR-SEC-001 | 시스템은 접근 전 인증을 강제해야 한다 | Tier 1 | LoginCommand + AuthService | 구현됨 |

**주의:** MR-UI-002의 "드롭다운 선택"은 Slide 3 재디자인 명세에서 유래했으나, 실제 구현은 Slide 2 원본(TextBox)을 따른다. MR-UI-002 재검토 필요.

---

## 7. 구현 갭 분석

### 현재 구현 상태 (LoginView.xaml 기준)

| 기능 | PPT Slide 2 명세 | 현재 구현 | 갭 |
|------|-----------------|----------|----|
| 창 스타일 | WindowStyle=None | UserControl 내부 Grid (윈도우 스타일은 MainWindow에서 처리) | 확인 필요 |
| 배경 색상 | `#1c2333` | Grid Background="#1c2333" | 일치 |
| 폼 너비 | 220px | Width="220" StackPanel | 일치 |
| 로고 이미지 | 160x~80px, Margin-bottom 80px | Width=160, Margin=0,0,0,80 | 일치 |
| 사용자명 입력 | TextBox, Height=36 | TextBox Height=36 | 일치 |
| 비밀번호 입력 | PasswordBox, Height=36 | PasswordBox Height=36 | 일치 |
| 확인 버튼 | 아이콘 전용, 40x40, IsDefault | Path 체크마크, 40x40, IsDefault=True | 일치 |
| 취소 버튼 | 아이콘 전용, 40x40, IsCancel | Path X, 40x40, IsCancel=True | 일치 |
| 버튼 간격 | 24px | Margin=0,0,24,0 | 일치 |
| 포커스 테두리 | `#1a6fc7` 파란색 | 스타일에서 처리 (확인 필요) | 확인 필요 |
| 오류 메시지 | PPT에 없음 | TextBlock Foreground=#e74c3c 추가됨 | PPT 초과 (긍정적) |
| 로딩 인디케이터 | PPT에 없음 | ProgressBar 추가됨 | PPT 초과 (긍정적) |
| MVVM 바인딩 | PPT에 없음 (코드비하인드) | Command, Binding 완비 | PPT 초과 (긍정적) |

### 불일치 항목

| 항목 | PPT 명세 | 현재 구현 | 심각도 |
|------|---------|----------|--------|
| MR-UI-002 준수 | ComboBox (Slide 3) | TextBox (Slide 2 따름) | 낮음 (Slide 2가 정답) |
| 창 드래그 | DragMove() 필요 | UserControl이므로 MainWindow에서 처리 여부 확인 필요 | 중간 |

---

## 8. 개선 우선순위

### Phase 1 — 즉시 처리 (P1)

| 항목 | 작업 | 담당 |
|------|------|------|
| MainWindow 드래그 | MainWindow.MouseLeftButtonDown에서 DragMove() 확인 | 개발자 |
| LoginTextBoxStyle 포커스 테두리 | `#1a6fc7` 2px 적용 여부 검증 | 개발자 |
| LoginPasswordBoxStyle 포커스 테두리 | 동일 확인 | 개발자 |

### Phase 2 — 단기 (P2)

| 항목 | 작업 | 담당 |
|------|------|------|
| MR-UI-002 재검토 | TextBox vs ComboBox 최종 결정, MRD 업데이트 | PM + 개발자 |
| 입력 필드 플레이스홀더 여부 | 비즈니스 요구사항에 따라 추가 여부 결정 | PM |

### Phase 3 — 장기 (P3)

| 항목 | 작업 | 담당 |
|------|------|------|
| 토큰 완전 적용 | 하드코딩 `#1c2333` 을 CoreTokens로 이동 | 개발자 |
| 접근성 | 스크린리더를 위한 AutomationProperties.Name 추가 | 개발자 |

---

*이 문서는 PPT 슬라이드 2, 3 및 `src/HnVue.UI/Views/LoginView.xaml` 분석을 기반으로 작성됨.*
*최종 권위: Slide 2 원본 스크린샷 분석 (Slide 3 재디자인은 번복됨)*
