# UISPEC-005: 환자/시술 추가 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 문서 ID | UISPEC-005 |
| 화면 ID | SCR-PATIENT-001 |
| 버전 | 1.0.0 |
| 작성일 | 2026-04-07 |
| 상태 | 초안 |
| PPT 참조 | Slide 8 (Add Patient / Procedure dialog) |
| HTML 목업 | `docs/ui_mockups/07-add-patient.html` |
| 구현 파일 | `src/HnVue.UI/Views/AddPatientProcedureView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/AddPatientProcedureViewModel.cs` |
| 안전 분류 | IEC 62366 일반 화면 (필수 입력 오류 방지 대상) |
| 관련 MRD | MR-PT-001, MR-PT-002, MR-PT-003, MR-PT-004 |

---

## 1. 화면 개요

"환자/시술 추가(Add Patient / Procedure)" 화면은 워크리스트에 새 환자를 수동으로 등록하고, 해당 환자에게 시술(Procedure)을 동시에 배정하는 다이얼로그형 UserControl이다. 이 화면은 모달 다이얼로그로 표시되며, 뒤 화면(WorklistView)은 비활성화된다.

**핵심 기능:**
- 환자 기본 정보 입력 (Patient ID, 이름, 생년월일, 성별)
- Patient ID 및 Accession Number 자동 생성 토글
- 시술 정보 입력 (Acc No, View Projection 다중 선택, Description 다중 선택, RIS Code)
- 필수 필드 유효성 검사 및 인라인 오류 표시
- 저장(Save) / 취소(Cancel) 액션

**설계 원칙:**
- 2-컬럼 레이아웃: 왼쪽 환자 정보 / 오른쪽 시술 정보
- 필수 필드는 레이블 옆 `(*)` 표시 (빨간색 `#D50000`)
- 저장 실패 시 전체 에러 배너를 Row 2에 표시
- 로딩 중 버튼 비활성화 + 로딩 인디케이터 표시

---

## 2. 레이아웃 구조

### 2.1 전체 레이아웃

설계 크기: **Width=700px, Height=650px** (DesignWidth/DesignHeight 기준)

```
UserControl (700×650, bg: HnVue.Semantic.Surface.Page = #242424)
├── Row 0 (48px):  타이틀 헤더 바
├── Row 1 (*):     2-컬럼 폼 바디 (16px 패딩)
│   ├── Col 0 (*): 환자 정보 패널 (bg: Surface.Panel, radius:6)
│   ├── Col 1 (12px): 구분 여백
│   └── Col 2 (*): 시술 정보 패널 (bg: Surface.Panel, radius:6)
├── Row 2 (Auto):  에러 메시지 배너 (조건부 표시)
└── Row 3 (56px):  액션 버튼 영역 (Cancel / Save)
```

**WPF Grid RowDefinitions:**
```
Row 0: Height="48"   → 헤더
Row 1: Height="*"    → 폼 바디
Row 2: Height="Auto" → 에러 배너
Row 3: Height="56"   → 액션 버튼
```

**WPF Grid ColumnDefinitions (Row 1 내부):**
```
Col 0: Width="*"   → 환자 정보
Col 1: Width="12"  → 구분 간격
Col 2: Width="*"   → 시술 정보
```

### 2.2 타이틀 헤더 (Row 0)

| 속성 | 값 |
|------|-----|
| Height | 48px |
| Background | `HnVue.Semantic.Surface.Panel` = `#2A2A2A` |
| BorderBrush | `HnVue.Semantic.Border.Default` = `#3B3B3B` |
| BorderThickness | `0,0,0,1` (하단 경계선만) |
| Padding | `16,0` |

**자식 요소:**
- 아이콘: Segoe MDL2 Assets `&#xE8FA;` (Person-Add), FontSize=16, 색상=`HnVue.Semantic.Brand.Accent` = `#00AEEF`, Margin=`0,0,10,0`
- 텍스트: `"Add Patient / Procedure"`, Style=`HnVue.SectionHeader`, FontSize=15

### 2.3 환자 정보 패널 (Left Column)

| 속성 | 값 |
|------|-----|
| Background | `HnVue.Semantic.Surface.Panel` = `#2A2A2A` |
| CornerRadius | `6` |
| Padding | `14,12` |

**섹션 타이틀:**
- 아이콘: Segoe MDL2 Assets `&#xE77B;` (Person), FontSize=12, 색상=Accent `#00AEEF`, Margin=`0,0,6,0`
- 텍스트: `"Patient Info"`, Style=`HnVue.SectionHeader`, FontSize=13, Margin=`0,0,0,10`

### 2.4 시술 정보 패널 (Right Column)

| 속성 | 값 |
|------|-----|
| Background | `HnVue.Semantic.Surface.Panel` = `#2A2A2A` |
| CornerRadius | `6` |
| Padding | `14,12` |

**섹션 타이틀:**
- 아이콘: Segoe MDL2 Assets `&#xE9D9;` (Stethoscope), FontSize=12, 색상=Accent `#00AEEF`, Margin=`0,0,6,0`
- 텍스트: `"Procedure Info"`, Style=`HnVue.SectionHeader`, FontSize=13, Margin=`0,0,0,10`

### 2.5 에러 배너 (Row 2, 조건부)

| 속성 | 값 |
|------|-----|
| Margin | `16,4,16,0` |
| Padding | `12,6` |
| CornerRadius | `4` |
| Background | `HnVue.Semantic.Status.Emergency` = `#D50000` |
| Visibility | `ErrorMessage != null` 일 때만 Visible |

**자식 요소:**
- 경고 아이콘: Segoe MDL2 Assets `&#xE7BA;`, FontSize=13, Foreground=White, Margin=`0,0,8,0`
- 에러 텍스트: `{Binding ErrorMessage}`, Foreground=White, FontSize=12, TextWrapping=Wrap

### 2.6 액션 버튼 영역 (Row 3)

| 속성 | 값 |
|------|-----|
| Height | 56px |
| Background | `HnVue.Semantic.Surface.Panel` = `#2A2A2A` |
| BorderThickness | `0,1,0,0` (상단 경계선) |
| Padding | `16,0` |

**좌측 (로딩 인디케이터):**
- 아이콘: Segoe MDL2 Assets `&#xE72C;` (Sync), FontSize=14, 색상=Accent
- 텍스트: `"Saving..."`, FontSize=12
- Visibility: `IsLoading == true` 일 때만 표시

**우측 (버튼):**

| 버튼 | Content | Style | Width | Height | Margin |
|------|---------|-------|-------|--------|--------|
| 취소 | `"Cancel"` | `HnVue.OutlineButton` | 100px | 36px | `0,0,8,0` |
| 저장 | `"Save"` | `HnVue.PrimaryButton` | 100px | 36px | 없음 |

- 두 버튼 모두 `IsLoading == true`일 때 `IsEnabled=False`

---

## 3. 입력 필드 명세

### 3.1 공통 입력 스타일

| 속성 | 값 |
|------|-----|
| TextBox Height | 32px |
| Padding | `8,0` |
| VerticalContentAlignment | Center |
| Background | `HnVue.Semantic.Surface.Card` = `#3B3B3B` |
| Foreground | `HnVue.Semantic.Text.Primary` = `#FFFFFF` |
| BorderBrush (기본) | `HnVue.Semantic.Border.Default` = `#3B3B3B` |
| BorderBrush (포커스) | `HnVue.Semantic.Brand.Accent` = `#00AEEF` |
| BorderThickness | `1` |
| FontFamily | `HnVue.Core.FontFamily` (Segoe UI / Malgun Gothic) |
| FontSize | `HnVue.Core.FontSize.Normal` = 13px |

### 3.2 환자 정보 필드 목록

#### Patient ID

| 속성 | 값 |
|------|-----|
| 필드명 | Patient ID |
| 필수 여부 | 선택 (자동 생성 시 자동 채움) |
| 레이블 스타일 | `HnVue.MutedLabel` (text-muted `#B0BEC5`, FontSize 12) |
| 입력 타입 | TextBox |
| Binding | `PatientId` |
| 비활성화 조건 | `IsPatientIdAutoGenerate == true` → `IsEnabled=False`, Opacity=0.55 |
| 자동 생성 버튼 | Content=`"Auto-Generate"`, Height=32, Padding=`10,0`, FontSize=11 |
| 자동 생성 버튼 활성 상태 | Background=`HnVue.Semantic.Brand.Accent` (#00AEEF), Foreground=White |
| 자동 생성 버튼 비활성 상태 | Style=`HnVue.OutlineButton` (기본 outline) |
| 레이아웃 | Grid 2-컬럼: TextBox(flex) + Gap(8px) + Button(Auto) |
| Margin 하단 | 8px |

#### Patient Name (필수)

| 속성 | 값 |
|------|-----|
| 필드명 | Patient Name |
| 필수 여부 | 필수 (*) |
| 필수 마커 | 레이블 뒤 `" (*)"`, Foreground=`HnVue.Semantic.Status.Emergency` (#D50000), FontSize=11 |
| 입력 타입 | TextBox |
| Binding | `PatientName`, UpdateSourceTrigger=PropertyChanged |
| 에러 상태 | BorderBrush=`#D50000`, 에러 메시지는 Row 2 배너에 표시 |
| Margin 하단 | 8px |

#### Birth Date (필수)

| 속성 | 값 |
|------|-----|
| 필드명 | Birth Date |
| 필수 여부 | 필수 (*) |
| 필수 마커 | 레이블 뒤 `" (*)"`, Foreground=`#D50000`, FontSize=11 |
| 입력 타입 | TextBox (형식: yyyy-MM-dd) |
| Binding | `BirthDate`, UpdateSourceTrigger=PropertyChanged |
| ToolTip | `"Format: yyyy-MM-dd"` |
| 구현 갭 | HTML 목업은 `<input type="date">` 사용 — WPF는 TextBox + 포맷 검증으로 구현. DatePicker 위젯 도입 권장 |
| Margin 하단 | 8px |

#### Gender (필수)

| 속성 | 값 |
|------|-----|
| 필드명 | Gender |
| 필수 여부 | 필수 (*) |
| 필수 마커 | 레이블 뒤 `" (*)"`, Foreground=`#D50000`, FontSize=11 |
| 입력 타입 | ComboBox |
| Binding | `Gender`, UpdateSourceTrigger=PropertyChanged |
| 항목 | M / F / Other |
| Height | 32px |
| Margin 하단 | 8px |

> **HTML 목업 차이:** HTML은 라디오 버튼 그룹 (남/여 2개) 사용. XAML 구현은 ComboBox(M/F/Other 3개 항목)로 변경됨. HTML 프로토타입의 "선택 시 배경 채움" 시각 피드백은 ComboBox 선택 하이라이트(`HnVue.Semantic.Brand.Primary` = `#1B4F8A`)로 대체.

### 3.3 시술 정보 필드 목록

#### Accession Number (필수)

| 속성 | 값 |
|------|-----|
| 필드명 | Acc No |
| 필수 여부 | 필수 (*) |
| 필수 마커 | 레이블 뒤 `" (*)"`, Foreground=`#D50000`, FontSize=11 |
| 입력 타입 | TextBox + Auto-Generate 버튼 |
| Binding | `AccessionNumber`, UpdateSourceTrigger=PropertyChanged |
| 비활성화 조건 | `IsAccNoAutoGenerate == true` → Opacity=0.55 |
| 자동 생성 버튼 | Content=`"Auto-Generate"`, Height=32, FontSize=11 |
| 자동 생성 버튼 활성 상태 | Background=Accent `#00AEEF`, Foreground=White |
| 레이아웃 | Grid 2-컬럼: TextBox(flex) + Gap(8px) + Button(Auto) |
| Margin 하단 | 8px |

#### View Projection (필수, 다중 선택)

| 속성 | 값 |
|------|-----|
| 필드명 | View Projection |
| 필수 여부 | 필수 (*) |
| 선택 방식 | ComboBox에서 선택 후 `+ Add` 버튼으로 칩 추가 |
| 칩 컨테이너 | ItemsControl + WrapPanel (가로 줄바꿈) |
| 칩 배경 | `HnVue.Semantic.Brand.Primary` = `#1B4F8A` |
| 칩 텍스트 | Foreground=White, FontSize=11 |
| 칩 모양 | CornerRadius=12, Padding=`8,3`, Margin=`0,0,4,4` |
| 칩 삭제 버튼 | Segoe MDL2 `&#xE711;` (Cancel/X), Foreground=White, FontSize=9 |
| Binding | `SelectedProjections` (ObservableCollection) |
| 드롭다운 | `AvailableProjections` (예: PA, AP, LAT, OBL 등) |
| 추가 Command | `AddProjectionCommand`, CommandParameter=ComboBox.SelectedItem |
| 제거 Command | `RemoveProjectionCommand`, CommandParameter=칩 항목 |
| Margin 하단 | 8px |

#### Description (선택, 다중 선택 + 직접 입력)

| 속성 | 값 |
|------|-----|
| 필드명 | Description |
| 필수 여부 | 선택 |
| 선택 방식 | 편집 가능 ComboBox + `+ Add` 버튼, 직접 입력 가능 |
| 칩 컨테이너 | ItemsControl + WrapPanel |
| 칩 배경 | `HnVue.Semantic.Surface.Card` = `#3B3B3B` |
| 칩 테두리 | `HnVue.Semantic.Brand.Accent` = `#00AEEF`, BorderThickness=1 |
| 칩 텍스트 | Foreground=`HnVue.Semantic.Text.Primary` = `#FFFFFF`, FontSize=11 |
| 칩 모양 | CornerRadius=12, Padding=`8,3`, Margin=`0,0,4,4` |
| ComboBox 속성 | IsEditable=True, Text 바인딩=`DescriptionInput` |
| Binding | `SelectedDescriptions` (ObservableCollection) |
| 추가 Command | `AddDescriptionCommand`, CommandParameter=ComboBox.Text |
| 제거 Command | `RemoveDescriptionCommand` |
| Margin 하단 | 8px |

> **칩 디자인 차이:** View Projection 칩은 Primary Blue 채움 (선택된 항목 강조), Description 칩은 Card 배경 + Accent 테두리 (보조 항목 구분).

#### RIS Code (선택)

| 속성 | 값 |
|------|-----|
| 필드명 | RIS Code |
| 필수 여부 | 선택 |
| 입력 타입 | TextBox |
| Binding | `RisCode`, UpdateSourceTrigger=PropertyChanged |
| ToolTip | `"Enter RIS code to link with external RIS system"` |
| Margin 하단 | 없음 (마지막 필드) |

---

## 4. 색상 토큰 매핑

| UI 요소 | 토큰 이름 | 실제 색상값 | 비고 |
|---------|-----------|------------|------|
| 배경 (전체) | `HnVue.Semantic.Surface.Page` | `#242424` | UserControl 루트 |
| 헤더/버튼 영역 배경 | `HnVue.Semantic.Surface.Panel` | `#2A2A2A` | Row 0, Row 3, 좌우 패널 |
| 입력 필드 배경 | `HnVue.Semantic.Surface.Card` | `#3B3B3B` | TextBox, ComboBox bg |
| 기본 테두리 | `HnVue.Semantic.Border.Default` | `#3B3B3B` | 비포커스 상태 |
| 기본 텍스트 | `HnVue.Semantic.Text.Primary` | `#FFFFFF` | 입력값, 버튼 레이블 |
| 뮤트 레이블 | `HnVue.Semantic.Text.Secondary` | `#B0BEC5` | 필드 레이블 (`HnVue.MutedLabel`) |
| 브랜드 액센트 | `HnVue.Semantic.Brand.Accent` | `#00AEEF` | 포커스 테두리, 아이콘, Auto-Generate 활성 |
| 프라이머리 버튼 bg | `HnVue.Semantic.Brand.Primary` | `#1B4F8A` | Save 버튼, View Projection 칩 |
| 필수 표시자 (*) | `HnVue.Semantic.Status.Emergency` | `#D50000` | 필수 필드 마커 |
| 에러 배너 배경 | `HnVue.Semantic.Status.Emergency` | `#D50000` | Row 2 에러 배너 |

**3-티어 토큰 체계 경로:**
```
CoreTokens.xaml (색상 원시값)
  → SemanticTokens.xaml (HnVue.Semantic.Surface.*, HnVue.Semantic.Brand.*, HnVue.Semantic.Status.*)
    → ComponentTokens.xaml (HnVue.PrimaryButton, HnVue.OutlineButton, HnVue.SectionHeader, HnVue.MutedLabel)
```

---

## 5. 상태 디자인

### 5.1 입력 필드 상태

| 상태 | BorderBrush | Background | Foreground | 기타 |
|------|------------|------------|------------|------|
| 기본 (Default) | `#3B3B3B` | `#3B3B3B` | `#FFFFFF` | — |
| 포커스 (Focus) | `#00AEEF` | `#3B3B3B` | `#FFFFFF` | 포커스 링 없음 (border color 변경으로 표시) |
| 비활성 (Disabled) | `#3B3B3B` | `#3B3B3B` | `#FFFFFF` | Opacity=0.55 |
| 에러 (Error) | `#D50000` | `#3B3B3B` | `#FFFFFF` | Row 2 에러 배너 표시 |

### 5.2 Auto-Generate 버튼 상태

| 상태 | 조건 | Background | Foreground | BorderBrush |
|------|------|------------|------------|-------------|
| 비활성 (OFF) | `IsAutoGenerate=False` | Transparent | `#B0BEC5` | `#3B3B3B` |
| 활성 (ON) | `IsAutoGenerate=True` | `#00AEEF` | `#FFFFFF` | — |

### 5.3 Save 버튼 상태

| 상태 | 조건 | Background | IsEnabled |
|------|------|------------|-----------|
| 기본 | `IsLoading=False` | `#1B4F8A` | True |
| 호버 | Mouse over | `#2E6DB4` | True |
| 로딩 중 | `IsLoading=True` | `#1B4F8A` | False (비활성화) |

### 5.4 에러 배너 상태

| 상태 | 조건 | Visibility | 내용 |
|------|------|------------|------|
| 숨김 | `ErrorMessage == null` | Collapsed | — |
| 표시 | `ErrorMessage != null` | Visible | `#D50000` 배경 + 흰색 경고 텍스트 |

### 5.5 칩(Chip) 상태

| 칩 유형 | 기본 | 호버 | X 버튼 호버 |
|---------|------|------|-------------|
| View Projection | `#1B4F8A` bg, White text | 별도 호버 없음 | X 아이콘 강조 (opacity 1.0) |
| Description | `#3B3B3B` bg + `#00AEEF` border | 별도 호버 없음 | X 아이콘 강조 |

---

## 6. IEC 62366 접근성 고려사항

### 6.1 터치 타겟 크기

| 요소 | 현재 구현 크기 | 권장 최소 크기 | 준수 여부 |
|------|-------------|-------------|---------|
| TextBox | 32px × flex-width | 32px × 44px | 경고 (높이 32px < 44px 권장) |
| ComboBox | 32px × flex-width | 44px | 경고 (높이 32px < 44px 권장) |
| Auto-Generate 버튼 | 32px × Auto | 44px | 경고 |
| Cancel 버튼 | 36px × 100px | 44px | 경고 (높이 36px < 44px 권장) |
| Save 버튼 | 36px × 100px | 44px | 경고 (높이 36px < 44px 권장) |
| 칩 삭제(X) 버튼 | ~20px × ~20px | 44px × 44px | 부적합 — 개선 필요 |

> **권고:** 의료 환경에서는 장갑을 착용한 작동을 고려하여 모든 인터랙티브 요소를 최소 44×44px로 설정하는 것이 IEC 62366 Part 1 기준에 부합한다. 현재 32~36px 구현은 운영 위험 분석(Operational Risk Analysis) 결과에 따라 허용 가능 여부를 문서화해야 한다.

### 6.2 필수 필드 식별

- 필수 필드는 `(*)` 마커로 시각적으로 구분됨 (색상: `#D50000`)
- 색맹 사용자를 위해 색상 외에 `(*)` 텍스트 기호를 병용 — IEC 62366 이중 코딩 요건 충족

### 6.3 오류 피드백

- 유효성 검사 실패 시 Row 2 에러 배너가 전체 너비로 표시됨
- 배너는 빨간 배경 + 경고 아이콘 + 텍스트 조합으로 주목도 확보
- 에러 메시지는 구체적인 필드명과 요구 조건을 포함해야 함 (예: `"Patient Name is required."`)

### 6.4 키보드 접근성

| 단축키 | 동작 |
|--------|------|
| Tab | 필드 간 이동 (선언적 TabIndex 필요) |
| Enter (TextBox) | 다음 필드 이동 또는 저장 동작 |
| Escape | Cancel 버튼과 동일한 동작 (닫기) |
| Ctrl+S | Save 단축키 (HTML 목업에서 정의됨, WPF InputBinding 추가 권장) |

### 6.5 진단 데이터 보호

- 환자 이름, 생년월일, 성별은 DICOM 필수 식별자에 해당
- 이 화면에서 입력된 데이터는 DICOM Study/Patient Module과 직접 매핑됨
- 자동 생성 ID(`PatientId`, `AccessionNumber`)의 고유성은 시스템 수준에서 보장되어야 함

---

## 7. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 설명 | 우선순위 | 구현 상태 | 관련 XAML 요소 |
|--------|-------------|---------|---------|--------------|
| MR-PT-001 | 시스템은 환자 등록을 지원해야 한다 | Tier 1 | 구현됨 | `AddPatientProcedureView.xaml` 전체 |
| MR-PT-002 | 시스템은 필수 환자 필드를 검증해야 한다 | Tier 2 | 부분 구현 | `ErrorMessage` Binding + Row 2 배너 |
| MR-PT-003 | 시스템은 응급 환자 등록을 지원해야 한다 | Tier 1 | 미구현 | HTML 목업에 없음, WPF에 없음 |
| MR-PT-004 | 시스템은 등록 시 시술 배정을 지원해야 한다 | Tier 2 | 구현됨 | Right Column (시술 정보 패널) |

> **MR-PT-003 갭:** HTML 목업과 WPF 구현 모두 응급(Emergency) 토글 UI가 없다. Priority/응급 플래그 필드 추가가 필요하다.

---

## 8. 구현 갭 분석

### 8.1 HTML 목업 vs WPF 구현 비교

| 기능 | HTML 목업 (`07-add-patient.html`) | WPF 구현 (`AddPatientProcedureView.xaml`) | 갭 설명 |
|------|-----------------------------------|------------------------------------------|---------|
| 환자 ID 자동 생성 | 체크박스 + AUTO 배지 | 버튼 토글 (AutoGenButton 스타일) | 상이한 UX 패턴, WPF 방식이 더 명확함 |
| 성별 입력 | 라디오 버튼 그룹 (남/여) | ComboBox (M/F/Other 3항목) | 항목 수 차이, WPF에 Other 추가 |
| 생년월일 | `<input type="date">` | TextBox (수동 형식 입력) | DatePicker 도입 필요 |
| 검사 선택 | 검색 가능 리스트 + 체크박스 (다중) | View Projection + Description 칩 UI | 완전히 다른 UX 모델. HTML은 "검사 코드" 선택, WPF는 "View Projection + Description" 조합 |
| 의뢰 의사 | `<input list>` (datalist) | 미구현 | WPF에 없음 |
| 임상 증상 | 텍스트 입력 | 미구현 | WPF에 없음 |
| 연락처 / 비상연락처 | 전화번호 입력 2개 | 미구현 | WPF에 없음 |
| 주소 | 텍스트 입력 | 미구현 | WPF에 없음 |
| 특이사항 | Textarea | 미구현 | WPF에 없음 |
| 중복 환자 경고 | 노란 경고 박스 | 미구현 | 이름+생년월일 중복 감지 기능 없음 |
| 응급 토글 | 미구현 (HTML에도 없음) | 미구현 | MR-PT-003 미충족 |
| Accession No | 텍스트 입력 (별도 Auto 없음) | Auto-Generate 버튼 | WPF가 더 많은 기능 구현 |
| RIS Code | 미구현 | TextBox + ToolTip | WPF가 추가 기능 구현 |

### 8.2 PPT 명세 vs WPF 구현

**PPT Slide 8 명세 (PENCIL_DESIGN_SUMMARY.md SCR-PATIENT-001):**
- 폼 카드: `#16213E` 배경, radius 8px (명세 기준)
- **실제 WPF:** `HnVue.Semantic.Surface.Panel` = `#2A2A2A`, CornerRadius=6

> 배경색이 명세(`#16213E`)와 구현(`#2A2A2A`) 간에 차이가 있다. CoreTokens.xaml v3.2.0에서 `background.panel`이 `#2A2A2A`로 변경되었으므로 WPF 구현이 최신 토큰을 따르고 있음. PPT 명세 업데이트 필요.

---

## 9. 개선 우선순위

| 우선순위 | 개선 항목 | MRD 연결 | 작업 규모 |
|---------|---------|---------|---------|
| P1 (즉시) | 응급 환자 등록 토글/플래그 필드 추가 | MR-PT-003 | 중간 |
| P1 (즉시) | 중복 환자 감지 경고 UI 구현 | MR-PT-001 | 중간 |
| P2 (단기) | 생년월일 DatePicker 컨트롤로 교체 | MR-PT-002 | 소규모 |
| P2 (단기) | 칩 삭제 X 버튼 터치 타겟 44px로 확대 | IEC 62366 | 소규모 |
| P2 (단기) | Cancel/Save 버튼 높이 36px → 44px 상향 | IEC 62366 | 소규모 |
| P3 (중기) | 의뢰 의사(Referring Physician) 필드 추가 | MR-PT-004 | 소규모 |
| P3 (중기) | 임상 증상(Clinical Info) 필드 추가 | MR-PT-004 | 소규모 |
| P3 (중기) | 연락처 / 비상연락처 필드 추가 | MR-PT-001 | 소규모 |
| P4 (장기) | 주소 / 특이사항 필드 추가 | MR-PT-001 | 소규모 |
| P4 (장기) | Ctrl+S 단축키 InputBinding 추가 | 접근성 | 극소규모 |

---

*문서 버전: 1.0.0 | 최종 업데이트: 2026-04-07 | 작성 근거: 07-add-patient.html, AddPatientProcedureView.xaml, PENCIL_DESIGN_SUMMARY.md*
