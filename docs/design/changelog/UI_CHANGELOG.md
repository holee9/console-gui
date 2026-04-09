# HnVue UI Design Changelog

문서 ID: DESIGN-CL-001
작성일: 2026-04-07

---

## [2.1.0] — 2026-04-07 — PPT 1·2페이지 섹션 배지 및 색상 체계 반영

**참조**: `★HnVUE UI 변경 최종안_251118.pptx` 슬라이드 2, 3 (로그인), 5, 6 (Worklist 현재)
**작업 범위**: 디자인 전용 (모듈 코드 미변경)

### 추가됨 (Added)

| 항목 | 파일 | PPT 근거 |
|------|-----|---------|
| SectionBadge 색상 토큰 | CoreTokens.xaml | Slide 3: #f9e04b, #2a3a5c |
| SectionBadge 브러시 | ComponentTokens.xaml | Slide 3 & 6 디자인 토큰 표 |
| HnVue.SectionBadgeText 스타일 | HnVueTheme.xaml | Slide 3: 12px/700/#f9e04b |
| HnVue.DetailHeaderText 스타일 | HnVueTheme.xaml | Slide 6: 11px/700/#7bc8f5 |
| HnVue.UppercaseLabel 스타일 | HnVueTheme.xaml | Slide 3: 10px/600/#7090b0 |
| HnVue.CancelButton 스타일 | HnVueTheme.xaml | Slide 2: 취소 버튼 |
| 섹션 배지 "로그인 창" | LoginView.xaml | Slide 3 명세 |
| 취소 버튼 XAML | LoginView.xaml | Slide 2 HTML 코드 참조 |
| 섹션 배지 "Worklist" | PatientListView.xaml | Slide 6 명세 |

### 변경됨 (Changed)

| 항목 | 파일 | 변경 내용 |
|------|-----|---------|
| Username 레이블 | LoginView.xaml | "Username" → "사용자" (UppercaseLabel 스타일) |
| Password 레이블 | LoginView.xaml | "Password" → "비밀번호" (UppercaseLabel 스타일) |
| LOGIN 버튼 텍스트 | LoginView.xaml | "LOGIN" → "확인" (버튼 행 레이아웃 변경) |
| HTML 목업 색상 전체 | 01~07 및 design-system.html | #1A1A2E → #242424, #16213E → #2A2A2A, #0F3460 → #3B3B3B |
| PENCIL_DESIGN_SUMMARY.md | — | v3.2.0 토큰 테이블 업데이트 |

---

## [2.0.0] — 2026-04-07 — PPT 명세 기반 전체 구현

**참조 문서**: `docs/★HnVUE UI 변경 최종안_251118.pptx` (22슬라이드, 2025-11-18)
**분석 문서**: `docs/design/PPT_ANALYSIS.md`

### 추가됨 (Added)

| 화면 | 파일 | PPT 슬라이드 | 내용 |
|------|-----|------------|------|
| Studylist | StudylistView.xaml | 7 | 이전/다음 내비, PACS 드롭다운, 기간 필터 |
| 환자/Procedure 통합 | AddPatientProcedureView.xaml | 8 | 통합 창, (*) 필수, Auto-Generate, 칩 UI |
| Sync Study | MergeView.xaml | 13 | "Sync Study" 명칭, 3열 레이아웃, Preview |
| Settings | SettingsView.xaml | 14~21 | 상단 탭, Network 통합, Access Notice |

### 변경됨 (Changed)

| 항목 | 파일 | 변경 내용 | PPT 슬라이드 |
|------|-----|---------|------------|
| 배경색 | CoreTokens.xaml | #1A1A2E → **#242424** | 슬라이드 4 |
| 패널 배경 | CoreTokens.xaml | #16213E → **#2A2A2A** | 슬라이드 4 |
| 카드 배경 | CoreTokens.xaml | #0F3460 → **#3B3B3B** | 슬라이드 4 |
| 경계선 | CoreTokens.xaml | #2E4A6E → **#3B3B3B** | 슬라이드 4 |
| Login Username | LoginView.xaml | TextBox → **ComboBox** 드롭다운 | 슬라이드 1 |
| Worklist 필터 | PatientListView.xaml | 기간 필터 버튼 5개 추가 | 슬라이드 4 |

### 명칭 변경 (Renamed)

| 기존 명칭 | 신규 명칭 | 위치 |
|---------|--------|------|
| Same Studylist | **Sync Study** | MergeView 버튼/제목 |
| Login Popup | **Access Notice** | SettingsView System 탭 |
| Only No matching | **Un-Matched** | SettingsView RIS Code 서브탭 |

### 삭제됨 (Removed)

| 항목 | 위치 | PPT 근거 |
|------|-----|--------|
| Study Group 필드 | AddPatientProcedureView | 슬라이드 8 명시 삭제 |
| Account > Operator 항목 | SettingsView Account 탭 | 슬라이드 15 명시 삭제 |

### 보존됨 (Preserved)

| 항목 | 이유 |
|------|-----|
| Detector 설정 구조 | PPT 슬라이드 16: "현행 구조 유지" |
| Generator 설정 구조 | PPT 슬라이드 16: "현행 구조 유지" |
| MainWindow 3열 레이아웃 | 기존 사용자 연속성 |
| 응급 정지(STOP) 버튼 위치 | IEC 62366 안전 요구사항 |
| MahApps.Metro Dark.Steel 테마 | 다크 테마 의료기기 표준 |

---

## [1.1.0] — 2026-04-06 — CoreTokens 디자인 토큰 시스템 구축

### 추가됨

- CoreTokens.xaml — 3계층 디자인 토큰 시스템 (Core → Semantic → Component)
- SemanticTokens.xaml — 의미론적 색상/타이포그래피 토큰
- ComponentTokens.xaml — 컴포넌트별 토큰
- HnVueTheme.xaml — 커스텀 WPF 스타일 (HnVue.PrimaryButton, HnVue.OutlineButton, HnVue.EmergencyStopButton 등)
- InverseBoolConverter.cs — IsEnabled 바인딩용 컨버터

### 변경됨

- LoginView.xaml — CoreTokens 스타일 적용, Evaluator 검증 완료 (B/7)
- PatientListView.xaml — 아이콘(Segoe MDL2 Assets), 44px 행 높이
- WorkflowView.xaml — 260px 컴팩트 레이아웃
- DoseDisplayView.xaml — DrlPercentage 게이지 바
- MainWindow.xaml — 로고 배지, STOP 버튼 분리

---

## [1.0.0] — 2026-04-05 — WPF 뷰 초기 구현

### 추가됨

- LoginView.xaml (MahApps.Metro Dark.Steel 기반)
- PatientListView.xaml (좌측 280px 패널)
- ImageViewerView.xaml (중앙 패널)
- WorkflowView.xaml (우측 260px 패널)
- DoseDisplayView.xaml (우측 하단)
- MainWindow.xaml (3열 레이아웃: 280px | * | 260px)
