# UISPEC-009: 시스템 관리(SystemAdmin) UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | v1.0 |
| 상태 | Draft |
| 작성일 | 2026-04-07 |
| PPT 참조 | 별도 슬라이드 없음 (관리자 전용 화면) |
| 구현 파일 | `src/HnVue.UI/Views/SystemAdminView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/SystemAdminViewModel.cs` |
| 관련 SPEC | SPEC-UI-001 |
| 준수율 | 40% (기본 구조만, 탭/필드 대부분 미구현) |

---

## 1. 화면 개요

시스템 관리 화면은 관리자(Admin) 전용 기능을 제공한다. 사용자 계정 관리, 역할 권한 설정, 감사 로그 조회, 라이선스 관리, 시스템 진단 등을 포함한다.

**핵심 특징:**
- `UserControl` 기반 — MainWindow 내에서 TabItem 또는 별도 Navigation으로 접근
- 관리자 권한 확인 — `IsAdminUser` 바인딩, 비관리자 시 접근 차단
- 탭 기반 네비게이션 — 5개 주요 섹션
- 보안 강조 — 감사 로그, 권한 매트릭스, 위험 구역 명확히 구분

**IEC 62366/보안 관련:**
- 모든 작업은 감사 로그에 기록 (MR-ADM-003)
- 파기 작업(삭제/초기화)은 명확한 확인 다이얼로그 후 실행
- 관리자 권한이 없는 사용자는 화면 진입 차단

---

## 2. 레이아웃 구조

### 2.1 전체 4행 그리드 레이아웃

```
┌────────────────────────────────────────────────────┐
│ Row 0: 타이틀 (Auto) — "System Administration"     │
├────────────────────────────────────────────────────┤
│ Row 1: 액션 버튼 행 (Auto) — Load/Save Settings    │
├────────────────────────────────────────────────────┤
│ Row 2: 상태 메시지 (Auto) — Status/Error          │
├────────────────────────────────────────────────────┤
│ Row 3: 콘텐츠 (*) — 탭 네비게이션 + 콘텐츠        │
└────────────────────────────────────────────────────┘
```

**현재 구현 상태:** 기본 Grid 구조만 있음, 탭/콘텐츠는 플레이스홀더

### 2.2 헤더 영역 (Row 0)

| 속성 | 값 |
|------|----|
| 높이 | Auto |
| 하단 마진 | 12px |
| 타이틀 | "System Administration" |
| FontSize | 18px |
| FontWeight | Bold |
| 전경색 | `#FFFFFF` (암시적) |

### 2.3 액션 버튼 행 (Row 1)

**구조:** `StackPanel(Orientation=Horizontal)`

| 버튼 | 텍스트 | Command | IsEnabled 바인딩 | Width |
|------|--------|---------|------------------|-------|
| Load Settings | "Load Settings" | `LoadSettingsCommand` | `{Binding IsAdminUser}` | 120px |
| Save Settings | "Save Settings" | `SaveSettingsCommand` | `{Binding IsAdminUser}` | 120px |
| IsBusy 인디케이터 | ProgressBar | — | `{Binding IsBusy}` | 80px |

**버튼 간격:** Margin `0,0,8,0` (좌)

**IsBusy 인디케이터:**
- `IsIndeterminate="True"`
- Height 20px
- Margin `12,0,0,0`
- Visibility: `{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}`

### 2.4 상태 메시지 행 (Row 2)

**구조:** `StackPanel`

| 메시지 타입 | TextBlock 바인딩 | 전경색 | 조건 |
|------------|------------------|--------|------|
| 성공 메시지 | `{Binding StatusMessage}` | Green | NullToVisibilityConverter |
| 에러 메시지 | `{Binding ErrorMessage}` | Red | NullToVisibilityConverter |

**공통 속성:**
- 하단 마진: 8px

### 2.5 콘텐츠 영역 (Row 3)

**현재 구현 상태:** 단일 GroupBox(DICOM Settings)만 존재, 탭 구조 미구현

**향후 설계:** 탭 기반 5섹션 구조

| 탭 ID | 탭명 | 설명 |
|-------|------|------|
| Users | 사용자 관리 | 계정 CRUD, 역할 할당 |
| Roles | 역할 관리 | 권한 매트릭스 |
| Audit | 감사 로그 | 필터링 가능한 이벤트 목록 |
| License | 라이선스 | 라이선스 키, 만료일 |
| Diagnostics | 진단 | 시스템 상태, 로그 내보내기 |

---

## 3. 컴포넌트 디자인 명세

### 3.1 사용자 관리 탭 (Users)

**목표:** MR-ADM-001 Tier1 (보안) — 사용자 계정 관리

**레이아웃:**
- 상단: 신규 사용자 생성 폼
- 하단: 사용자 목록 DataGrid

**신규 사용자 생성 폼:**

| 필드 | 컨트롤 | 바인딩 | 필수여부 |
|------|--------|--------|---------|
| Account ID | TextBox | `NewAccountId` | Yes |
| Name | TextBox | `NewUserName` | Yes |
| Password | PasswordBox | `NewUserPassword` | Yes |
| Confirm Password | PasswordBox | `NewUserPasswordConfirm` | Yes |
| Role | ComboBox | `NewUserRole` | Yes |
| Department | TextBox | `NewUserDepartment` | No |
| Create 버튼 | Button | `CreateUserCommand` | — |

**입력 유효성 검증:**
- Account ID: 영문/숫자 조합, 최소 4자
- Password: 최소 8자, 영문+숫자+특수문자 조합
- Password 확인: 일치 여부
- 중복 확인: 동일 Account ID 존재 시 에러

**사용자 목록 DataGrid:**

| 컬럼 | 헤더 | 너비 | 정렬 |
|------|------|------|------|
| Account ID | "ID" | 120px | Left |
| Name | "이름" | 150px | Left |
| Role | "역할" | 100px | Center |
| Department | "부서" | 150px | Left |
| Last Login | "마지막 로그인" | 150px | Center |
| Status | "상태" | 80px | Center |
| Actions | "작업" | 120px | Center |

**행 높이:** 44px (IEC 62366 최소 터치 타겟)

**Actions 컬럼 버튼:**
- Edit (연필 아이콘) — 편집 다이얼로그 열기
- Delete (휴지통 아이콘) — 확인 후 삭제
- Reset Password (키 아이콘) — 임시 비밀번호 생성

**DataGrid 스타일:**

| 속성 | 값 |
|------|----|
| 배경 | `HnVue.Semantic.Surface.Card` (`#3B3B3B`) |
| 헤더 배경 | `HnVue.Semantic.Surface.Panel` (`#2A2A2A`) |
| 헤더 전경색 | `HnVue.Semantic.Text.Secondary` (`#B0BEC5`) |
| 행 호버 배경 | `rgba(0,174,239,0.08)` |
| 선택 행 배경 | `rgba(0,174,239,0.15)` |
| 그리드선 | 1px `HnVue.Semantic.Border.Default` (`#3B3B3B`) |
| AlternationCount | 2 |
| Alternating RowBackground | `rgba(255,255,255,0.03)` |

---

### 3.2 역할 관리 탭 (Roles)

**목표:** MR-ADM-002 Tier1 (보안) — 역할 기반 접근 제어(RBAC)

**레이아웃:** 좌우 2-패널

| 패널 | 내용 |
|------|------|
| 좌측 | 역할 목록 ListBox |
| 우측 | 선택된 역할의 권한 매트릭스 |

**좌측 역할 목록:**

| 항목 | 설명 |
|------|------|
| Admin | 모든 권한 |
| Technician | 촬영, 이미지 조작, 기본 설정 |
| Radiologist | 이미지 읽기, 측정, 주석 |
| Viewer | 읽기 전용 |

**우측 권한 매트릭스:**

**행(기능) / 열(역할) 구조:**

| 기능 카테고리 | Admin | Technician | Radiologist | Viewer |
|--------------|-------|------------|-------------|--------|
| **촬영 (Acquisition)** |
| Acquire (촬영) | ✅ | ✅ | ❌ | ❌ |
| Emergency Stop (긴급 정지) | ✅ | ✅ | ❌ | ❌ |
| Protocol Management (프로토콜 관리) | ✅ | ❌ | ❌ | ❌ |
| **이미지 (Imaging)** |
| View Images (이미지 보기) | ✅ | ✅ | ✅ | ✅ |
| Window/Level (W/L 조절) | ✅ | ✅ | ✅ | ✅ |
| Annotations (주석/측정) | ✅ | ✅ | ✅ | ❌ |
| Invert/Rotate (이미지 변환) | ✅ | ✅ | ✅ | ✅ |
| Export (내보내기) | ✅ | ✅ | ✅ | ❌ |
| **환자 (Patient)** |
| Create Patient (환자 등록) | ✅ | ✅ | ✅ | ❌ |
| Edit Patient (환자 편집) | ✅ | ✅ | ❌ | ❌ |
| Merge Study (검사 병합) | ✅ | ❌ | ❌ | ❌ |
| **설정 (Settings)** |
| System Settings (시스템 설정) | ✅ | ❌ | ❌ | ❌ |
| Network Settings (네트워크 설정) | ✅ | ❌ | ❌ | ❌ |
| Device Settings (장비 설정) | ✅ | ❌ | ❌ | ❌ |
| **관리 (Administration)** |
| User Management (사용자 관리) | ✅ | ❌ | ❌ | ❌ |
| Role Management (역할 관리) | ✅ | ❌ | ❌ | ❌ |
| Audit Log Access (감사 로그 조회) | ✅ | ❌ | ❌ | ❌ |
| License Management (라이선스 관리) | ✅ | ❌ | ❌ | ❌ |
| Database Backup (DB 백업) | ✅ | ❌ | ❌ | ❌ |

**구현 방식:** CheckBox Grid, IsChecked={Binding Permissions[Feature][Role]}

---

### 3.3 감사 로그 탭 (Audit)

**목표:** MR-ADM-003 Tier1 (규제) — 모든 중요 작업 감사

**레이아웃:**

| 영역 | 컨트롤 |
|------|--------|
| 상단 필터 바 | DatePicker × 2 (From/To) + Event Type ComboBox + Export Button |
| 중간 | DataGrid (감사 로그 목록) |
| 하단 페이지 네이비게이션 | First / Prev / Page Number / Next / Last |

**필터 바:**

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| From Date | DatePicker | 시작일 (기본: 오늘 - 7일) |
| To Date | DatePicker | 종료일 (기본: 오늘) |
| Event Type | ComboBox | All / Login / Logout / Acquisition / Patient Management / Settings Change / Admin Action |
| Export | Button | "Export to CSV" |

**감사 로그 DataGrid 컬럼:**

| 컬럼 | 헤더 | 너비 | 설명 |
|------|------|------|------|
| Timestamp | "시간" | 150px | ISO 8601 포맷 |
| User | "사용자" | 120px | Account ID |
| Role | "역할" | 100px | Admin/Technician/Radiologist |
| Event Type | "이벤트 유형" | 150px | Login, Acquisition, Settings Change 등 |
| Description | "설명" | * | 상세 설명 (예: "Changed PACS server address") |
| Result | "결과" | 80px | Success / Failure |
| IP Address | "IP 주소" | 120px | 접속 IP (보안) |

**행 높이:** 36px

**색상 코딩:**
- Success: `#00C853` (Safe)
- Failure: `#D50000` (Emergency)

**내보내기 형식:** CSV (UTF-8 with BOM)

---

### 3.4 라이선스 탭 (License)

**목표:** MR-ADM-004 Tier2 — 라이선스 관리

**레이아웃:** 2개 GroupBox

**GroupBox 1: 라이선스 정보**

| 필드 | 컨트롤 | 바인딩 |
|------|--------|--------|
| License Key | TextBox (ReadOnly) | `LicenseKey` |
| Product | TextBox (ReadOnly) | "HnVue Medical Imaging Console" |
| Version | TextBox (ReadOnly) | `CurrentVersion` |
| License Type | TextBox (ReadOnly) | "Trial" / "Standard" / "Professional" |
| Expiration Date | TextBox (ReadOnly) | `ExpirationDate` |
| Days Remaining | TextBlock | `DaysRemaining` (남은 일수, 만료 30일 전 경고) |
| Seat Count | TextBox (ReadOnly) | `MaxConcurrentUsers` |
| Active Seats | TextBox (ReadOnly) | `CurrentActiveUsers` |

**GroupBox 2: 라이선스 갱신**

| 필드 | 컨트롤 | 설명 |
|------|--------|------|
| New License Key | TextBox | 라이선스 키 입력 |
| Activate 버튼 | Button | 서버 활성화 요청 |
| Deactivate 버튼 | Button (Danger) | 라이선스 해제 |

**만료 경고:**
- 30일 미만: `#FFD600` (Warning) 뱃지
- 7일 미만: `#D50000` (Emergency) 뱃지
- 만료됨: 비활성화 + "License Expired" 메시지

---

### 3.5 진단 탭 (Diagnostics)

**목표:** 시스템 상태 모니터링, 문제 해결 지원

**레이아웃:** 3개 GroupBox

**GroupBox 1: 시스템 상태**

| 항목 | 컨트롤 | 바인딩 |
|------|--------|--------|
| Application Version | TextBlock | `AppVersion` |
| .NET Runtime | TextBlock | `DotNetRuntimeVersion` |
| OS Version | TextBlock | `OsVersion` |
| Uptime | TextBlock | `Uptime` (형식: "d일 h시간 m분") |
| Memory Usage | ProgressBar + TextBlock | 사용량 / 전체 (형식: "1.2 GB / 16 GB") |
| CPU Usage | ProgressBar + TextBlock | 0~100% |
| Disk Space | ProgressBar + TextBlock | 사용량 / 전체 |

**GroupBox 2: 장비 연결 상태**

| 장비 | 상태 | 마지막 통신 |
|------|------|-----------|
| Detector | Connected / Disconnected / Error | 2026-04-07 10:30:15 |
| Generator | Connected / Disconnected / Error | 2026-04-07 10:29:45 |
| PACS Server | Connected / Disconnected / Error | 2026-04-07 10:28:00 |
| Worklist Server | Connected / Disconnected / Error | 2026-04-07 10:27:30 |

**상태 색상:**
- Connected: `#00C853` (Safe)
- Disconnected: `#546E7A` (Disabled)
- Error: `#D50000` (Emergency)

**GroupBox 3: 로그 관리**

| 버튼 | Command | 설명 |
|------|---------|------|
| View Application Log | — | 텍스트 에디터로 로그 파일 열기 |
| Export Diagnostics Bundle | — | ZIP(시스템정보+로그+설정) 생성 |
| Clear Logs | `ClearLogsCommand` | 확인 후 로그 파일 삭제 |
| Restart Application | `RestartAppCommand` | 애플리케이션 재시작 (이중 확인) |

---

## 4. 색상 토큰 매핑

### 4.1 사용된 토큰

| 역할 | Semantic 토큰 | Hex 값 |
|------|--------------|--------|
| 배경 | `HnVue.Semantic.Surface.Page` | `#242424` |
| 패널 배경 | `HnVue.Semantic.Surface.Panel` | `#2A2A2A` |
| 카드/DataGrid 배경 | `HnVue.Semantic.Surface.Card` | `#3B3B3B` |
| 테두리 | `HnVue.Semantic.Border.Default` | `#3B3B3B` |
| 주 텍스트 | `HnVue.Semantic.Text.Primary` | `#FFFFFF` |
| 보조 텍스트 | `HnVue.Semantic.Text.Secondary` | `#B0BEC5` |
| 성공 상태 | `HnVue.Semantic.Status.Safe` | `#00C853` |
| 경고 상태 | `HnVue.Semantic.Status.Warning` | `#FFD600` |
| 에러 상태 | `HnVue.Semantic.Status.Emergency` | `#D50000` |
| 위험 작업 배경 | `HnVue.Semantic.Status.Emergency` | `#D50000` |

### 4.2 위험 구역(Danger Zone) 색상

**삭제/초기화 작업:** 명확히 구분

| 요소 | 색상 |
|------|------|
| 배경 | `#D50000` (Emergency) 20% 투명도 |
| 테두리 | `#D50000` 2px |
| 버튼 배경 | `#D50000` |
| 버튼 텍스트 | `#FFFFFF` |
| 호버 배경 | `#B71C1C` (더 어두운 빨간색) |

---

## 5. 상태 디자인

### 5.1 권한 부족 상태

**진입 차단:**
- `IsAdminUser=False` 시 전체 화면 비활성화
- 잠금 배너: "이 화면은 관리자 전용입니다. 현재 역할: [CurrentRole]"
- 잠금 아이콘: Segoe MDL2 `&#xE72E;`, 48px, `#546E7A`

### 5.2 로딩 상태

| 작업 | 표현 |
|------|------|
| Load Settings | Load Settings 버튼 비활성화 + ProgressBar 인디케이터 |
| Save Settings | Save Settings 버튼 비활성화 + ProgressBar 인디케이터 |
| Export Audit Log | Export 버튼 텍스트 → "Exporting..." + ProgressBar |

### 5.3 성공/실패 피드백

| 작업 | 성공 | 실패 |
|------|------|------|
| Load Settings | "Settings loaded successfully." (Green) | "Failed to load settings: [reason]" (Red) |
| Save Settings | "Settings saved successfully." (Green) | "Failed to save settings: [reason]" (Red) |
| Create User | "User [ID] created successfully." (Green) | "Failed to create user: [reason]" (Red) |
| Delete User | "User [ID] deleted successfully." (Green) | "Failed to delete user: [reason]" (Red) |

---

## 6. 보안/접근 제어 UI

### 6.1 관리자 권한 확인

**진입 전 확인:**
```csharp
if (!IsAdminUser) {
    ShowAccessDeniedDialog();
    return;
}
```

**ShowAccessDeniedDialog():**
- 타이틀: "접근 거부"
- 메시지: "시스템 관리 화면은 관리자 권한이 필요합니다.\n\n현재 역할: [CurrentRole]\n필요 권한: Admin"
- 버튼: "확인"만

### 6.2 감사 로그 자동 기록

**기록 대상 작업 (MR-ADM-003):**
- 사용자 생성/삭제/수정
- 역할 권한 변경
- 설정 변경 (System, Network, Device, DICOM)
- 라이선스 활성화/비활성화
- 로그 내보내기
- 데이터베이스 백업/복원

**감사 로그 항목:**
```csharp
new AuditLogEntry {
    Timestamp = DateTime.UtcNow,
    UserId = CurrentUser.AccountId,
    UserRole = CurrentUser.Role,
    EventType = "UserCreated", // 예: "UserCreated", "SettingsChanged", "LicenseActivated"
    Description = "Created user [AccountId] with role [Role]",
    Result = "Success",
    IpAddress = GetClientIpAddress()
}
```

### 6.3 위험 작업 이중 확인

**삭제/초기화 작업:**

```
다이얼로그 타이틀: "작업 확인"
메시지: "정말로 [작업명]을(를) 실행하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다."
확인 텍스트: "[작업명] 실행" (예: "사용자 삭제")
취소 텍스트: "취소"
```

**취소 시:** 작업 취소, 기록 안 함
**확인 시:** 작업 실행, 감사 로그 기록

---

## 7. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 설명 | 우선순위 | 관련 탭 | 구현 상태 |
|--------|--------------|---------|---------|----------|
| MR-ADM-001 | 사용자 계정 관리 | Tier1 (보안) | Users | 미구현 |
| MR-ADM-002 | 역할 기반 접근 제어(RBAC) | Tier1 (보안) | Roles | 미구현 |
| MR-ADM-003 | 감사 로그 유지 | Tier1 (규제) | Audit | 미구현 |
| MR-ADM-004 | 라이선스 관리 | Tier2 | License | 미구현 |

---

## 8. 구현 갭 분석

| 갭 항목 | 현재 상태 | 설계 목표 | 우선순위 |
|---------|----------|----------|---------|
| 탭 네비게이션 | 미구현 (단일 GroupBox만) | 5개 탭(Users/Roles/Audit/License/Diagnostics) | P1 |
| Users 탭 콘텐츠 | 미구현 | 계정 생성 폼 + DataGrid | P1 |
| Roles 탭 콘텐츠 | 미구현 | 권한 매트릭스 CheckBox Grid | P1 |
| Audit 탭 콘텐츠 | 미구현 | 필터 + DataGrid + CSV 내보내기 | P1 |
| License 탭 콘텐츠 | 미구현 | 라이선스 정보 + 갱신 폼 | P2 |
| Diagnostics 탭 콘텐츠 | 미구현 | 시스템 상태 + 장비 연결 + 로그 관리 | P2 |
| Admin 권한 확인 | 부분 구현 (IsAdminUser) | 진입 전 다이얼로그 + 잠금 배너 | P1 |
| 위험 작업 이중 확인 | 미구현 | 삭제/초기화 전 확인 다이얼로그 | P1 |
| 감사 로그 자동 기록 | 미구현 | 모든 중요 작업 로깅 | P1 |

---

## 9. 개선 우선순위

**P1 (릴리즈 블로커 — 보안/규제):**
1. 탭 네비게이션 구현 — 5개 탭 분리
2. Users 탭 구현 — MR-ADM-001 Tier1
3. Roles 탭 구현 — MR-ADM-002 Tier1
4. Audit 탭 구현 — MR-ADM-003 Tier1
5. Admin 권한 확인 다이얼로그
6. 위험 작업 이중 확인 다이얼로그
7. 감사 로그 자동 기록

**P2 (다음 릴리즈):**
1. License 탭 구현 — MR-ADM-004 Tier2
2. Diagnostics 탭 구현
3. 장비 연결 상태 모니터링

**P3 (백로그):**
1. 감사 로그 검색 기능 (자유 텍스트 검색)
2. 사용자 활동 요약 대시보드
3. 자동 백업 스케줄링
4. 원격 지원 로그 전송
