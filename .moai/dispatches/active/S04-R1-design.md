# DISPATCH: Team Design — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | Team Design |
| **브랜치** | team/team-design |
| **유형** | S04 Round 1 — 이월 수정 + UI 구현 |
| **우선순위** | P0 (UI.QA 실패 수정), P1 (StudylistView), P2 (AddPatientView) |
| **SPEC 참조** | SPEC-UI-001 |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 0, Task 1은 P0 (즉시 착수, 병렬 가능)
- Task 2는 Task 0 완료 후 착수
- Task 3은 Task 2 완료 후 착수
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

S03 완료 후 S04 이월 항목 및 추가 UI 구현 DISPATCH.

**S04 이월 항목 (P0 — 즉시 수정 필요)**:
- UI.QA 13개 테스트 실패 (PptPage2WorklistDesignTests + DesignSystemTests)
- RelayCommand value type flaky 테스트 1개

**S04 신규 항목**:
- StudylistView PPT 리디자인 (UISPEC-003, 63%→100% 준수율)
- AddPatientProcedureView 리디자인 착수 (UISPEC-005)

---

## 파일 소유권

```
HnVue.UI/Views/
HnVue.UI/Styles/
HnVue.UI/Themes/
HnVue.UI/Components/
HnVue.UI/Converters/
HnVue.UI/Assets/
HnVue.UI/DesignTime/
tests/HnVue.UI.Tests/
tests/HnVue.UI.QA.Tests/
```

---

## Task 0 (P0): UI.QA 13개 테스트 실패 수정

### 배경

PptPage2WorklistDesignTests 및 DesignSystemTests 13개가 S03에서 실패한 채 이월됨.
디자인 토큰 누락 및 PatientListView 구조 미적용이 원인.

### 수정 항목 1: CoreTokens.xaml — 누락 토큰 추가

**파일**: `HnVue.UI/Themes/CoreTokens.xaml`

추가할 토큰 (ResourceDictionary 내 적절한 위치에 삽입):

```xml
<!-- Worklist 섹션 배지 토큰 -->
<Color x:Key="SectionBadgeBg">#2A3A5C</Color>
<Color x:Key="SectionBadgeText">#F9E04B</Color>
<Color x:Key="DetailHeaderText">#7BC8F5</Color>
<Color x:Key="LabelMuted">#7090B0</Color>
```

**수용 기준**: CoreTokens.xaml에 위 4개 Color 리소스 존재, 빌드 성공

---

### 수정 항목 2: SemanticTokens.xaml — WorklistStatus 색상 브러시 추가

**파일**: `HnVue.UI/Themes/SemanticTokens.xaml`

추가할 브러시 (Worklist 상태 색상):

```xml
<!-- Worklist 상태 색상 브러시 -->
<SolidColorBrush x:Key="WorklistStatusNew" Color="#2196F3"/>
<SolidColorBrush x:Key="WorklistStatusInProgress" Color="#FF9800"/>
<SolidColorBrush x:Key="WorklistStatusCompleted" Color="#4CAF50"/>
<SolidColorBrush x:Key="WorklistStatusCancelled" Color="#9E9E9E"/>
```

**수용 기준**: SemanticTokens.xaml에 위 4개 SolidColorBrush 존재

---

### 수정 항목 3: ComponentTokens.xaml — WorklistSectionBadge 브러시 추가

**파일**: `HnVue.UI/Themes/ComponentTokens.xaml`

추가할 브러시:

```xml
<!-- Worklist 섹션 배지 컴포넌트 토큰 -->
<SolidColorBrush x:Key="WorklistSectionBadgeBg" Color="{StaticResource SectionBadgeBg}"/>
<SolidColorBrush x:Key="WorklistSectionBadgeText" Color="{StaticResource SectionBadgeText}"/>
```

**수용 기준**: ComponentTokens.xaml에 위 2개 브러시 존재

---

### 수정 항목 4: HnVueTheme — DetailHeaderTextStyle 추가

**파일**: `HnVue.UI/Themes/HnVueTheme.xaml` (또는 동등한 테마 파일)

추가할 스타일:

```xml
<!-- Detail Header Text Style -->
<Style x:Key="DetailHeaderTextStyle" TargetType="TextBlock">
    <Setter Property="Foreground" Value="{StaticResource WorklistSectionBadgeText}"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
</Style>
```

**수용 기준**: HnVueTheme에 DetailHeaderTextStyle 존재

---

### 수정 항목 5: PatientListView.xaml — 누락 UI 요소 추가

**파일**: `HnVue.UI/Views/PatientListView.xaml`

추가/수정 항목:
1. **SectionBadge 요소**: `x:Name="SectionBadge"` 속성 포함한 Border/TextBlock 구조
2. **PeriodFilterButtons**: 기간 필터 버튼 그룹 (오늘/이번주/이번달 등)
3. **ListBoxItem 높이 44px**: `<Setter Property="Height" Value="44"/>` 또는 `MinHeight="44"` 적용
4. **EmergencyBadge**: 응급 환자 표시 배지 요소

PPT 지정 페이지 기준(슬라이드 2-4)만 구현. 타 화면 요소 삽입 절대 금지.

**수용 기준**:
- PatientListView.xaml에 SectionBadge, PeriodFilterButtons, EmergencyBadge 요소 존재
- ListBoxItem 높이 44px 이상
- PPT 슬라이드 2-4 범위 외 요소 미삽입

---

### 수정 항목 6: DesignSystemTests 배경색 토큰 확인

**파일**: 테스트 확인 후 해당 CoreTokens 또는 SemanticTokens 수정

DesignSystemTests가 요구하는 값:
- `BackgroundPage`: `#242424`
- `BackgroundPanel`: `#2A2A2A`

위 값이 테마 파일에 없다면 추가.

**수용 기준**: DesignSystemTests 전원 통과

---

### Task 0 검증

```
dotnet build HnVue.UI.QA.Tests
dotnet test HnVue.UI.QA.Tests --filter "PptPage2WorklistDesignTests|DesignSystemTests"
```

**수용 기준**: 13개 테스트 전원 PASS

---

## Task 1 (P0): RelayCommand flaky 테스트 수정

### 배경

`HnVue.UI.Tests`에서 RelayCommand의 값 타입(value type) 처리 관련 1개 테스트가
불규칙적으로 실패 (pre-existing). S04에서 근본 원인 수정.

### 파일

`tests/HnVue.UI.Tests/` 내 RelayCommand 관련 테스트 파일

### 접근 방법

1. 실패 테스트 특정 — `dotnet test --filter "RelayCommand"` 실행
2. 실패 원인 분석 — 값 타입 박싱/언박싱 또는 Equals 비교 이슈
3. 테스트 코드 또는 RelayCommand 구현 수정

**수용 기준**: 
- RelayCommand 관련 테스트 10회 연속 실행 시 전원 PASS
- `dotnet test --filter "RelayCommand"` PASS

---

## Task 2 (P1): StudylistView PPT 리디자인 (UISPEC-003)

### 배경

StudylistView (검사 목록 화면)를 PPT 슬라이드 5-7 기준으로 리디자인.
현재 PPT 준수율 63%, 목표 100%.

### 파일

**수정 대상만 (PPT 슬라이드 5-7 범위)**:
- `HnVue.UI/Views/StudylistView.xaml`
- `HnVue.UI/DesignTime/DesignTimeStudylistViewModel.cs` (필요 시 신규)

### PPT 슬라이드 5-7 구현 항목

슬라이드 확인 후 다음 항목 구현:
1. 헤더 섹션 (환자 정보 + 검사 날짜)
2. 검사 목록 (ListBox/DataGrid + 44px 높이)
3. 필터 영역 (날짜 범위, 상태 필터)
4. 액션 버튼 (선택, 취소 등)

**PPT 범위 외 절대 구현 금지** — 슬라이드 9-11(영상뷰어/촬영) 요소 삽입 금지.

**수용 기준**:
- StudylistView.xaml PPT 슬라이드 5-7 요소 1:1 매핑
- 빌드 성공
- DesignTime VM으로 VS2022 디자이너 렌더링 확인

---

## Task 3 (P2): AddPatientProcedureView 리디자인 착수 (UISPEC-005)

### 배경

PPT 슬라이드 8 기준 AddPatientProcedureView 리디자인 착수.

### 파일

**수정 대상만 (PPT 슬라이드 8 범위)**:
- `HnVue.UI/Views/AddPatientProcedureView.xaml`

### 구현 항목

슬라이드 8 확인 후 기본 레이아웃 구조 구현 (완전 구현은 S05로 이월 가능).

**수용 기준**:
- AddPatientProcedureView.xaml 슬라이드 8 기본 레이아웃 반영
- 빌드 성공

---

## Git 완료 프로토콜 [HARD]

모든 Task 완료 후 순서대로 실행:

```bash
# 1. 스테이징 (비밀키, .tmp 제외)
git add HnVue.UI/Themes/CoreTokens.xaml
git add HnVue.UI/Themes/SemanticTokens.xaml
git add HnVue.UI/Themes/ComponentTokens.xaml
git add HnVue.UI/Themes/HnVueTheme.xaml
git add HnVue.UI/Views/PatientListView.xaml
git add HnVue.UI/Views/StudylistView.xaml
git add HnVue.UI/Views/AddPatientProcedureView.xaml
git add tests/HnVue.UI.Tests/
# (변경된 파일 모두 명시적으로 추가)

# 2. 커밋
git commit -m "fix(design): fix 13 UI.QA failures and RelayCommand flaky test for S04"

# 3. 푸시
git push origin team/team-design

# 4. PR 생성 (Gitea API)
# 기존 열린 PR 확인 후 없으면 신규 생성
curl -X POST "http://10.11.1.40:7001/api/v1/repos/drake.lee/Console-GUI/pulls" \
  -H "Authorization: token a4cb79626194b34a2d52835de05fb770162af014" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "[S04-R1-Design] UI.QA 13개 실패 수정 + StudylistView 리디자인",
    "body": "S04 Round 1 Design Team DISPATCH 완료\n\n## 변경 사항\n- CoreTokens.xaml: 4개 누락 토큰 추가\n- SemanticTokens.xaml: WorklistStatus 브러시 추가\n- ComponentTokens.xaml: WorklistSectionBadge 브러시 추가\n- HnVueTheme: DetailHeaderTextStyle 추가\n- PatientListView.xaml: SectionBadge, PeriodFilter, EmergencyBadge 추가\n- RelayCommand flaky 수정\n- StudylistView PPT 슬라이드 5-7 리디자인\n\n## 검증\n- UI.QA 13개 테스트 전원 PASS\n- 전체 솔루션 빌드 0 에러",
    "head": "team/team-design",
    "base": "main"
  }'
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 0: UI.QA 13 실패 수정 | NOT_STARTED | -- | -- |
| Task 1: RelayCommand flaky 수정 | NOT_STARTED | -- | -- |
| Task 2: StudylistView PPT 리디자인 | NOT_STARTED | -- | -- |
| Task 3: AddPatientView 착수 | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 빌드 검증 결과

```
# 여기에 실제 빌드/테스트 결과 기록
dotnet build: ?
dotnet test HnVue.UI.QA.Tests: ?
전체 테스트: ?
```
