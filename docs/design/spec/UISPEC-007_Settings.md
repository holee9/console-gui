# UISPEC-007: 설정(Settings) 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | v1.0 |
| 상태 | Draft |
| 작성일 | 2026-04-07 |
| PPT 참조 | Slides 14–22 (Settings 화면 상세 명세, 9슬라이드) |
| HTML 목업 | `docs/ui_mockups/06-settings.html` |
| 구현 파일 | `src/HnVue.UI/Views/SettingsView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/SettingsViewModel.cs` |
| 관련 SPEC | SPEC-UI-001 |
| 준수율 | 80% (탭 구조 구현됨, 일부 콘텐츠 플레이스홀더 상태) |

---

## 1. 화면 개요

설정 화면은 HnVue 애플리케이션의 구성 허브다. 시스템 설정, 계정 관리, 장비 연결, 네트워크, DICOM 등 모든 운영 파라미터를 통합 관리한다.

**핵심 특징:**
- `UserControl` 기반 (팝업/Window가 아닌 MainWindow 내 콘텐츠 영역에 삽입)
- 상단 탭 네비게이션 (PPT 슬라이드 14–21 기준: 좌측 사이드바 구조 → 상단 탭으로 변경 확정)
- 탭: System / Account / Detector / Generator / Network / Display / Option / Database / DicomSet / RIS Code
- 하단 액션 바: Cancel / Save 버튼 고정
- MVVM 패턴: `ActiveTab` 바인딩으로 탭 전환

**IEC 62366 관련:**
- 설정 화면 자체는 직접적 안전 위험 경로가 아니나, 잘못된 DICOM/Generator 설정은 임상 위험 유발 가능
- 관리자(Admin) 전용 항목은 비관리자 세션에서 비활성화(IsEnabled=False) 처리 필수

---

## 2. 레이아웃 구조

### 2.1 전체 4행 그리드 레이아웃

```
┌─────────────────────────────────────────────────────────┐
│ Row 0: Header (48px) — Settings 제목 + 아이콘           │
├─────────────────────────────────────────────────────────┤
│ Row 1: Tab Row (Auto) — 상단 탭 네비게이션              │
├─────────────────────────────────────────────────────────┤
│ Row 2: Content Area (*) — 선택된 탭의 콘텐츠 (스크롤)  │
├─────────────────────────────────────────────────────────┤
│ Row 3: Action Bar (Auto) — Cancel / Save 버튼           │
└─────────────────────────────────────────────────────────┘
```

**XAML Grid RowDefinitions:**
```
Height="48"   <!-- Header -->
Height="Auto" <!-- Tab row -->
Height="*"    <!-- Content (ScrollViewer) -->
Height="Auto" <!-- Action bar -->
```

### 2.2 헤더 영역 (Row 0)

| 속성 | 값 |
|------|----|
| 높이 | 48px |
| 배경 | `HnVue.Semantic.Surface.Panel` (`#2A2A2A`) |
| 하단 테두리 | 1px `HnVue.Semantic.Border.Default` (`#3B3B3B`) |
| 패딩 | `16,0` (좌우) |
| 아이콘 | Segoe MDL2 Assets `&#xE713;` (Settings 기어), 16px, Accent(`#00AEEF`) |
| 아이콘-타이틀 간격 | 8px (Margin `0,0,8,0`) |
| 타이틀 | "Settings" — `HnVue.SectionHeader` 스타일 적용 |

### 2.3 탭 네비게이션 행 (Row 1)

**구조:** `ScrollViewer(HorizontalScrollBarVisibility=Auto)` 내 `StackPanel(Orientation=Horizontal)`

| 속성 | 값 |
|------|----|
| 높이 | Auto (내용에 맞춤, 실질적 36px) |
| 배경 | `HnVue.Semantic.Surface.Panel` (`#2A2A2A`) |
| 하단 테두리 | 1px `HnVue.Semantic.Border.Default` (`#3B3B3B`) |

**탭 버튼 목록 (순서 고정):**

| 순서 | 탭 ID | 표시명 | 설명 |
|------|-------|--------|------|
| 1 | `System` | System | 시스템 우선순위, Access Notice |
| 2 | `Account` | Account | 계정 생성/목록 관리 |
| 3 | `Detector` | Detector | FPD 검출기 설정 |
| 4 | `Generator` | Generator | X선 발생기 설정 |
| 5 | `Network` | Network | PACS + Worklist + Print 통합 |
| 6 | `Display` | Display | 화면 표시 설정 |
| 7 | `Option` | Option | 기타 옵션 |
| 8 | `Database` | Database | 로컬 DB 설정 |
| 9 | `DicomSet` | DicomSet | DICOM AE/타이틀 설정 |
| 10 | `RIS Code` | RIS Code | RIS 코드 매핑 (Matching/Un-Matched 서브탭) |

**SettingsTabButton 스타일 명세:**

| 상태 | 배경 | 전경색 | 하단 테두리 |
|------|------|--------|------------|
| 기본 | Transparent | `HnVue.Semantic.Text.Secondary` (`#B0BEC5`) | Transparent |
| Hover | Transparent | `HnVue.Semantic.Text.Primary` (`#FFFFFF`) | Transparent |
| IsChecked=True (활성) | Transparent | `HnVue.Semantic.Brand.Accent` (`#00AEEF`) | 2px `#00AEEF` |

```
Height: 36px
Padding: 14,0 (좌우)
FontSize: 12px
BorderThickness: 0,0,0,2 (하단만)
```

### 2.4 콘텐츠 영역 (Row 2)

| 속성 | 값 |
|------|----|
| 타입 | `ScrollViewer` |
| VerticalScrollBarVisibility | Auto |
| HorizontalScrollBarVisibility | Disabled |
| 내부 Grid Margin | `24,20,24,20` |

탭 전환은 각 `StackPanel`의 `Visibility` 바인딩으로 구현:
```xml
Visibility="{Binding ActiveTab, Converter={StaticResource ActiveTabToVisibility},
             ConverterParameter=[탭ID]}"
```

### 2.5 액션 바 (Row 3)

| 속성 | 값 |
|------|----|
| 배경 | `HnVue.Semantic.Surface.Panel` (`#2A2A2A`) |
| 상단 테두리 | 1px `HnVue.Semantic.Border.Default` (`#3B3B3B`) |
| 패딩 | `16,10` |
| 레이아웃 | 3-컬럼 Grid (에러메시지 * | Cancel Auto | Save Auto) |

**에러 메시지 영역:**
- `HnVue.Semantic.Status.Emergency` (`#D50000`) 전경색
- FontSize 12px
- `ErrorMessage` 바인딩, NullToVisibility 컨버터

**Cancel 버튼:**
- Style: `HnVue.OutlineButton`
- Width: 100px, Height: 36px
- Command: `CancelCommand`
- Margin: `0,0,8,0`

**Save 버튼:**
- Style: `HnVue.PrimaryButton`
- Width: 100px, Height: 36px
- Command: `SaveCommand`
- 배경: `HnVue.Semantic.Brand.Primary` (`#1B4F8A`)

---

## 3. 설정 탭별 상세 명세

### 3.1 System 탭

**섹션 1: NOTICE**

| 필드 | 컨트롤 | 바인딩 | 크기 |
|------|--------|--------|------|
| Access Notice | TextBox (멀티라인) | `AccessNoticeText` | Height=80px, TextWrapping=Wrap, AcceptsReturn=True |

- 라벨: "Access Notice" (InputLabel 스타일, Width=160px)
- 설명: 세션 시작 시 표시되는 알림 텍스트 (PPT: 구명칭 "Login Popup" → "Access Notice"로 변경)
- Padding: `8,6`

**섹션 구분선:** `Separator` (배경: `HnVue.Semantic.Border.Default`)

**섹션 2: SYSTEM**

| 필드 | 컨트롤 | 옵션 | 기본값 |
|------|--------|------|--------|
| Priority | ComboBox | Normal / High | Normal |

- Grid 2컬럼: Label(160px) + Control(*)

**SettingsSectionLabel 스타일:**
```
FontSize: 11px
FontWeight: SemiBold
Foreground: HnVue.Semantic.Text.Secondary (#B0BEC5)
Margin: 0,0,0,6
```

**InputLabel 스타일:**
```
FontSize: 12px
Foreground: HnVue.Semantic.Text.Primary (#FFFFFF)
VerticalAlignment: Center
Width: 160px
```

**SettingsTextBox 스타일:**
```
Height: 30px
Padding: 8,0
FontSize: 12px
Background: HnVue.Semantic.Surface.Card (#3B3B3B)
Foreground: HnVue.Semantic.Text.Primary (#FFFFFF)
BorderBrush: HnVue.Semantic.Border.Default (#3B3B3B)
BorderThickness: 1
VerticalContentAlignment: Center
```

---

### 3.2 Account 탭

**섹션 1: NEW ACCOUNT**

| 필드 | 컨트롤 | 바인딩 | 비고 |
|------|--------|--------|------|
| Account ID | TextBox | `NewAccountId` | UpdateSourceTrigger=PropertyChanged |
| Role | ComboBox | `NewAccountRole` / `AvailableRoles` | 옵션: Admin / Technician / Radiologist |
| 추가 버튼 | Button | — | Style: HnVue.OutlineButton, "Add Account" |

- PPT 변경사항: Operator 필드 제거됨, Role이 드롭다운(ComboBox)으로 변경됨
- Add Account 버튼: Height=30px, Padding=`12,0`, FontSize=11px, 우측 정렬

**섹션 구분선**

**섹션 2: ACCOUNT LIST**

| 속성 | 값 |
|------|----|
| 타입 | Border (리스트 컨테이너) |
| MinHeight | 160px |
| 배경 | `HnVue.Semantic.Surface.Card` (`#3B3B3B`) |
| 테두리 | 1px `HnVue.Semantic.Border.Default` |

구현 예정: DataGrid 또는 ListView로 계정 목록 표시
- 컬럼: Account ID / Role / 마지막 로그인 / 상태 / 액션(편집/삭제)
- 행 높이: 44px (IEC 62366 최소 터치 타겟)

---

### 3.3 Detector 탭

**섹션: DETECTOR SETTINGS**

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록:

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| AE Title | TextBox | 검출기 DICOM AE 타이틀 |
| Model | ComboBox | 검출기 모델 선택 |
| Serial Number | TextBox (ReadOnly) | 읽기 전용 일련번호 |
| Connection Type | ComboBox | Wired / Wireless |
| IP Address | TextBox | 검출기 네트워크 주소 |
| Calibration Date | TextBox (ReadOnly) | 마지막 보정일 |
| Gain Mode | ComboBox | Low / Medium / High |
| Binning | ComboBox | 1x1 / 2x2 |

컨테이너: MinHeight=240px, Surface.Card 배경

---

### 3.4 Generator 탭

**섹션: GENERATOR SETTINGS**

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록:

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| Model | ComboBox | 발생기 모델 선택 |
| Connection Port | TextBox | COM 포트 또는 TCP 주소 |
| kVp Range | TextBox × 2 | 최소/최대 kVp |
| mAs Range | TextBox × 2 | 최소/최대 mAs |
| AEC Enabled | ToggleSwitch | 자동 노출 제어 활성화 |
| Communication Protocol | ComboBox | RS-232 / TCP/IP |

컨테이너: MinHeight=240px, Surface.Card 배경

---

### 3.5 Network 탭

PPT 변경사항: PACS + Worklist + Print를 단일 Network 탭으로 통합

**서브섹션 1: PACS**

| 필드 | 컨트롤 | 바인딩 | 크기 |
|------|--------|--------|------|
| Server Address | TextBox | `PacsServerAddress` | * (나머지 너비) |
| Port | TextBox | `PacsServerPort` | 64px 고정 |
| Add 버튼 | Button (OutlineButton) | — | Height=30px, Padding=`10,0` |
| Edit 버튼 | Button (OutlineButton) | — | Height=30px, Padding=`10,0` |

PACS 서버 목록:
- MinHeight=70px, Surface.Card 배경
- 구현 예정: DataGrid (AE Title / Host / Port / 연결상태)

5컬럼 Grid 레이아웃:
```
140px | * | 64px | Auto | Auto
Label | Address | Port | Add | Edit
```

**서브섹션 구분선**

**서브섹션 2: WORKLIST**

구조 동일 (PACS와 같은 레이아웃):
- 바인딩: `WorklistServerAddress`, `WorklistServerPort`
- Worklist 서버 목록 (MinHeight=70px)

**서브섹션 구분선**

**서브섹션 3: PRINT**

| 필드 | 컨트롤 | 옵션 |
|------|--------|------|
| Printer | ComboBox | "Default Printer" 등 시스템 프린터 목록 |

---

### 3.6 Display 탭

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록:

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| Language | ComboBox | 한국어 / English |
| Date Format | ComboBox | YYYY-MM-DD / MM/DD/YYYY |
| Time Format | ComboBox | 24시간 / 12시간 |
| Font Size | ComboBox | Small / Normal / Large |
| Image Flip Default | CheckBox | 기본 이미지 좌우반전 |
| Window/Level Preset | 그룹 | 기본 W/L 프리셋 테이블 |

---

### 3.7 Option 탭

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록:

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| Auto-Send to PACS | ToggleSwitch | 촬영 후 자동 전송 |
| Print After Acquire | ToggleSwitch | 촬영 후 자동 인쇄 |
| Thumbnail Quality | ComboBox | Low / Medium / High |
| Session Timeout | ComboBox | 10분 / 30분 / 1시간 / 없음 |
| Backup Path | PathPicker | 로컬 백업 경로 |

---

### 3.8 Database 탭

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록:

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| DB Type | ComboBox | SQLite / MSSQL |
| Connection String | TextBox | DB 연결 문자열 |
| Test Connection | Button | 연결 테스트 |
| Auto Backup | ToggleSwitch | 자동 백업 |
| Retention Days | NumericInput | 데이터 보관 일수 |
| DB Size | TextBlock (ReadOnly) | 현재 DB 크기 표시 |

---

### 3.9 DicomSet 탭

현재 상태: 플레이스홀더 (구현 예정)

구현 예정 필드 목록 (MR-CFG-002 Tier1 — 규제 요구사항):

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| Station AE Title | TextBox | 로컬 스테이션 DICOM AE 타이틀 |
| Station Name | TextBox | DICOM Station Name (0008,1010) |
| Institution Name | TextBox | DICOM Institution Name (0008,0080) |
| Modality | ComboBox | DX / CR / RF |
| Manufacturer | TextBox | 제조사명 (DICOM 0008,0070) |
| Model Name | TextBox | 장비 모델명 (DICOM 0008,1090) |

---

### 3.10 RIS Code 탭

**서브탭 구조:**

| 서브탭 | ID | 설명 |
|--------|-----|------|
| Matching | `Matching` | RIS 코드 ↔ 프로토콜 매핑 테이블 |
| Un-Matched | `Un-Matched` | 매칭되지 않은 RIS 코드 목록 (PPT 구명칭: "Only No matching") |

**RisSubTabButton 스타일 (SettingsTabButton 기반):**
```
Height: 30px
Padding: 10,0
FontSize: 11px
```

서브탭 행:
- Border: Surface.Panel 배경, 하단 1px Border.Default 테두리
- StackPanel Orientation=Horizontal
- Margin: `0,2,0,0` (Padding)

**Matching 서브탭 콘텐츠:**
- MinHeight=200px, Surface.Card 배경
- 구현 예정: RIS 코드 ↔ HnVue 프로토콜 매핑 테이블

**Un-Matched 서브탭 콘텐츠:**
- MinHeight=200px, Surface.Card 배경
- 매칭되지 않은 RIS 코드 표시
- Export(내보내기) 버튼

---

## 4. 색상 토큰 매핑

### 4.1 CoreTokens.xaml 기준값

| 역할 | Semantic 토큰 | Core 토큰 | Hex 값 |
|------|--------------|-----------|--------|
| 페이지 배경 | `HnVue.Semantic.Surface.Page` | `HnVue.Core.Color.BackgroundPage` | `#242424` |
| 패널/사이드바 배경 | `HnVue.Semantic.Surface.Panel` | `HnVue.Core.Color.BackgroundPanel` | `#2A2A2A` |
| 카드/입력 배경 | `HnVue.Semantic.Surface.Card` | `HnVue.Core.Color.BackgroundCard` | `#3B3B3B` |
| 기본 테두리 | `HnVue.Semantic.Border.Default` | `HnVue.Core.Color.Border` | `#3B3B3B` |
| 포커스 테두리 | `HnVue.Semantic.Border.Focus` | `HnVue.Core.Color.BorderFocus` | `#00AEEF` |
| 주 텍스트 | `HnVue.Semantic.Text.Primary` | `HnVue.Core.Color.TextPrimary` | `#FFFFFF` |
| 보조 텍스트 | `HnVue.Semantic.Text.Secondary` | `HnVue.Core.Color.TextSecondary` | `#B0BEC5` |
| 비활성 텍스트 | `HnVue.Semantic.Text.Disabled` | `HnVue.Core.Color.TextDisabled` | `#546E7A` |
| 브랜드 Primary | `HnVue.Semantic.Brand.Primary` | `HnVue.Core.Color.Primary` | `#1B4F8A` |
| 브랜드 Accent | `HnVue.Semantic.Brand.Accent` | `HnVue.Core.Color.Accent` | `#00AEEF` |
| 에러/긴급 | `HnVue.Semantic.Status.Emergency` | `HnVue.Core.Color.StatusEmergency` | `#D50000` |
| 경고 | `HnVue.Semantic.Status.Warning` | `HnVue.Core.Color.StatusWarning` | `#FFD600` |
| 안전/정상 | `HnVue.Semantic.Status.Safe` | `HnVue.Core.Color.StatusSafe` | `#00C853` |

### 4.2 설정 화면 전용 컨트롤 색상

| 컨트롤 요소 | 상태 | 색상 |
|------------|------|------|
| Tab 버튼 기본 | Default | `#B0BEC5` (Text.Secondary) |
| Tab 버튼 활성 | IsChecked=True | `#00AEEF` (Brand.Accent) |
| Tab 하단 인디케이터 | IsChecked=True | 2px solid `#00AEEF` |
| 섹션 레이블 | — | `#B0BEC5` (Text.Secondary) |
| 입력 필드 배경 | Default | `#3B3B3B` (Surface.Card) |
| 입력 필드 테두리 | Default | `#3B3B3B` (Border.Default) |
| 입력 필드 테두리 | Focus | `#00AEEF` (Border.Focus) |
| 에러 메시지 | — | `#D50000` (Status.Emergency) |
| Save 버튼 배경 | Default | `#1B4F8A` (Brand.Primary) |
| Save 버튼 배경 | Hover | `#2E6DB4` (Brand.PrimaryLight) |
| Cancel 버튼 | — | OutlineButton 스타일 |

---

## 5. 상태 디자인

### 5.1 탭 전환 상태

| 상태 | 시각적 표현 |
|------|------------|
| 탭 비활성 | 텍스트 `#B0BEC5`, 하단 테두리 없음 |
| 탭 활성 | 텍스트 `#00AEEF`, 하단 2px solid `#00AEEF` |
| 탭 호버 | 텍스트 `#FFFFFF`, 하단 테두리 없음 |

### 5.2 입력 필드 상태

| 상태 | 테두리 | 배경 | 텍스트 |
|------|--------|------|--------|
| Default | 1px `#3B3B3B` | `#3B3B3B` | `#FFFFFF` |
| Focus | 1px `#00AEEF` + 2px 외곽선 `rgba(0,174,239,0.2)` | `#3B3B3B` | `#FFFFFF` |
| Error | 1px `#D50000` | `#3B3B3B` | `#FFFFFF` |
| Disabled | 1px `#3B3B3B` | `#242424` (더 어둡게) | `#546E7A` |
| ReadOnly | 1px `#3B3B3B` | `#2A2A2A` | `#B0BEC5` |

### 5.3 저장 상태 피드백

| 상태 | 표현 방식 |
|------|----------|
| 저장 성공 | 하단 StatusMessage 녹색 텍스트 (일시적 표시 후 3초 후 자동 숨김) |
| 저장 실패 | 하단 ErrorMessage 빨간색 텍스트 + 원인 설명 |
| 저장 중 | Save 버튼 비활성화 + IsBusy 인디케이터 표시 |
| 미저장 변경 | (향후 구현) 탭 제목에 asterisk(*) 표시 |

### 5.4 연결 테스트 상태 (Network 탭)

| 상태 | 뱃지 | 색상 |
|------|------|------|
| 연결됨 | "Connected" | `#00C853` |
| 연결 안됨 | "Disconnected" | `#D50000` |
| 테스트 중 | "Testing..." | `#FFD600` + 스피너 |
| 미테스트 | "Unknown" | `#546E7A` |

---

## 6. 접근 권한 UI (Admin 전용 항목)

### 6.1 기본 원칙 (MR-CFG-003)

비관리자(Technician, Radiologist) 세션에서 Admin 전용 항목:
- `IsEnabled="False"` — 컨트롤 비활성화
- 잠금 아이콘 (Segoe MDL2 `&#xE72E;`) + 툴팁: "관리자 권한이 필요합니다"
- Opacity: 0.5 (시각적 비활성화 표시)

### 6.2 탭별 권한 요구사항

| 탭 | 권한 요구사항 |
|----|--------------|
| System | Admin 전용 (Access Notice, Priority) |
| Account | Admin 전용 (계정 생성/삭제) |
| Detector | Admin 전용 |
| Generator | Admin 전용 |
| Network | Admin 전용 |
| Display | 모든 사용자 가능 |
| Option | 모든 사용자 가능 |
| Database | Admin 전용 |
| DicomSet | Admin 전용 (규제 요구사항: MR-CFG-002) |
| RIS Code | Admin 전용 |

### 6.3 비관리자 접근 시 UX

```
[탭 헤더] Network  →  활성화되나 콘텐츠는 비활성화 처리
[잠금 배너] 이 섹션은 관리자 전용입니다. 현재 세션: [역할명]
```

---

## 7. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 설명 | 우선순위 | 관련 탭 | 구현 상태 |
|--------|--------------|---------|---------|----------|
| MR-CFG-001 | 시스템 구성 지원 | Tier2 | System, Option | 부분 구현 |
| MR-CFG-002 | DICOM 구성 지원 | Tier1 (규제) | DicomSet, Network | 플레이스홀더 |
| MR-CFG-003 | 민감 설정 Admin 제한 | Tier1 (보안) | 전체 탭 | 부분 구현 (`IsAdminUser`) |
| MR-CFG-004 | 프로토콜 관리 지원 | Tier2 | RIS Code | 플레이스홀더 |
| MR-CFG-005 | 감사 로그 접근 | Tier1 (규제) | (별도 SystemAdmin 화면) | 미구현 |

---

## 8. 구현 갭 분석

| 갭 항목 | 현재 상태 | 설계 목표 | 우선순위 |
|---------|----------|----------|---------|
| Detector 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 | P1 |
| Generator 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 | P1 |
| Display 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 | P2 |
| Option 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 | P2 |
| Database 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 | P2 |
| DicomSet 탭 콘텐츠 | 플레이스홀더 | 전체 필드 구현 (MR-CFG-002) | P1 |
| RIS Code 서브탭 콘텐츠 | 플레이스홀더 | Matching/Un-Matched 테이블 | P2 |
| Account 목록 DataGrid | 플레이스홀더 텍스트 | 실제 DataGrid 바인딩 | P1 |
| Admin 잠금 UI | 부분 구현 (IsAdminUser) | 잠금 아이콘 + 배너 | P2 |
| 설정 변경 미저장 경고 | 미구현 | 탭 asterisk + 이탈 경고 다이얼로그 | P3 |
| Network 연결 테스트 | 미구현 | Ping/C-ECHO 테스트 버튼 | P1 |

---

## 9. 개선 우선순위

**P1 (릴리즈 블로커):**
1. DicomSet 탭 필드 구현 — DICOM 규제 요구사항 (MR-CFG-002)
2. Detector/Generator 탭 — 장비 연결 설정 없으면 촬영 불가
3. Account 목록 DataGrid — 현재 "Account list will be populated here." 텍스트만 있음
4. Network 탭 PACS 연결 테스트 (C-ECHO)

**P2 (다음 릴리즈):**
1. Admin 권한 잠금 UI (배너 + 잠금 아이콘) — MR-CFG-003
2. RIS Code 매핑 테이블 구현
3. Display / Option 탭 콘텐츠

**P3 (백로그):**
1. 미저장 변경 추적 (탭 asterisk, 이탈 경고)
2. 설정 내보내기/가져오기 기능
3. 설정 검색 (HTML 목업에 검색박스 존재, XAML 미구현)
