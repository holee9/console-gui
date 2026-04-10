# PPT Runtime E2E Review

## 문서 정보

- 작성일: 2026-04-09
- 범위: `★HnVUE UI 변경 최종안_251118.pptx` 기준 런타임 GUI 구현 검증
- 워크트리 역할: Design Team
- 제약: 구현 모듈 수정 없이 런타임 검증과 Coordinator handoff 자료화만 수행

## 결론

현재 실행 가능한 `HnVue.App` 런타임은 PPT 최종안의 `변경 2안`과 구조적으로 일치하지 않는다.

불일치는 개별 컴포넌트 수준이 아니라 화면 구조 레벨에서 발생한다.

- PPT 최종안은 `Login`, `Worklist`, `Studylist`, `Image`, `Acquisition`를 독립 창 또는 독립 화면 단위로 다룬다.
- 현재 런타임은 로그인 이후에도 단일 `HnVue Console` 셸 내부에 `PatientList + ImageViewer + Workflow/Dose`를 3분할로 고정한다.
- 따라서 색상, 버튼, 텍스트 일부를 맞추는 수준으로는 최종안 일치 판정을 받을 수 없다.

판정: `Fail`

## 검증 근거

### 기준 문서

- PPT 원본: `docs/★HnVUE UI 변경 최종안_251118.pptx`
- 원본 해석 문서: `docs/design/PPT_ANALYSIS.md`
- 추출된 PPT 기준 이미지
  - Login 최종안: `temp_ppt_extract/ppt/media/image1.png`
  - Worklist 최종안: `temp_ppt_extract/ppt/media/image9.png`
  - Studylist 최종안: `temp_ppt_extract/ppt/media/image12.png`
  - Acquisition 최종안: `temp_ppt_extract/ppt/media/image20.png`

### 실제 런타임 증거

- 로그인 현재 화면 캡처: `C:\Users\drake.lee\AppData\Local\Temp\codex-shot-2026-04-09_21-13-14.png`
- 로그인 후 메인 셸 캡처: `C:\Users\drake.lee\AppData\Local\Temp\codex-shot-2026-04-09_21-21-13.png`
- 좌측/중앙 영역 캡처: `C:\Users\drake.lee\AppData\Local\Temp\codex-shot-2026-04-09_21-21-34.png`
- 우측 Acquisition 영역 캡처: `C:\Users\drake.lee\AppData\Local\Temp\codex-shot-2026-04-09_21-21-37.png`

### 실행 및 확인 내용

- `dotnet build HnVue.sln --configuration Debug --nologo` 성공
- `src/HnVue.App/bin/Debug/net8.0-windows/HnVue.App.exe` 실행
- UIAutomation으로 로그인 수행
- 로그인 후 top-level window 탐색 결과, HnVue 계열 윈도우는 `HnVue Console` 1개만 확인

## Findings

### 1. Critical: PPT 최종안의 독립 창 구조가 런타임에 존재하지 않는다

근거:

- `MainWindow`는 로그인 이후 고정 3열 셸을 표시한다.
- 좌측은 `PatientListViewModel`, 중앙은 `ImageViewerViewModel`, 우측은 `WorkflowViewModel + DoseDisplayViewModel`이다.
- 저장소 내 top-level XAML window도 `MainWindow.xaml` 1개뿐이다.

코드 근거:

- `src/HnVue.App/MainWindow.xaml`
  - 106행: `3-column main layout`
  - 109행: 좌측 `280`
  - 111행: 우측 `260`
  - 116행: `PatientListViewModel`
  - 121행: `ImageViewerViewModel`
  - 131-132행: `WorkflowViewModel`, `DoseDisplayViewModel`
- `src/HnVue.UI.ViewModels/ViewModels/MainViewModel.cs`
  - 26-35행: 셸 패널 ViewModel 속성 고정
  - 153-158행: 로그인 성공 시 `IsMainContentVisible = true`, `ActiveNavItem = "PatientList"`

영향:

- PPT 최종안의 `Worklist`, `Studylist`, `Image`, `Acquisition`는 독립 화면 기준인데, 현재 런타임은 처음부터 다른 정보 구조를 사용한다.
- 사용자가 PPT를 기준으로 보면 가장 먼저 구조적 위화감을 느끼게 된다.

### 2. High: Login 화면이 PPT 최종안과 전혀 다른 레이아웃으로 구현돼 있다

PPT 기준:

- 중앙 로고
- 입력 2개만 있는 미니멀 화면
- 체크/취소 아이콘 버튼
- notes 기준 `ID dropdown list`

현재 런타임:

- 메인 셸 배경 위에 카드형 로그인 오버레이
- `HnVue Login`, `Medical Imaging Console` 제목/부제 노출
- 큰 `LOGIN` 버튼
- 헤더와 Emergency Stop이 로그인 상태에서도 노출

코드 근거:

- `src/HnVue.UI/Views/LoginView.xaml`
  - 11행: `Keep centered form layout matching existing screenshot`
  - 17행: 카드형 centered layout
  - 53행: `HnVue Login`
  - 62행: `Medical Imaging Console`
  - 84-86행: editable ComboBox
  - 131-132행: `LOGIN` 버튼
- `src/HnVue.App/MainWindow.xaml`
  - 46-55행: 로그인 전에도 헤더 우측 Emergency Stop 노출
  - 137행 이후: 로그인은 별도 창이 아니라 overlay

영향:

- PPT Login 최종안과 가장 눈에 띄는 첫인상 차이가 발생한다.
- ID dropdown 요구는 일부 반영됐지만, 화면 문법은 최종안과 다르다.

### 3. High: Worklist가 독립 창이 아니라 좌측 패널로 축소되어 있으며, 표 구조도 다르다

PPT Worklist 2안 기준:

- Worklist 자체가 독립 창
- 좌측 환자 목록 + 우측 연관 목록의 dual-list 구조
- 상단 기간 필터와 검색/작업 도구
- 중앙/우측이 다른 화면에 잠식되지 않음

현재 런타임:

- Worklist는 `PatientListView`의 좌측 280px 패널 일부
- 중앙은 항상 Image Viewer가 차지하고, 우측은 Acquisition 패널이 차지한다
- 즉, Worklist가 하나의 전체 창으로 존재하지 않는다

코드 근거:

- `src/HnVue.UI/Views/PatientListView.xaml`
  - 202-209행: `WorklistTab`, `StudylistTab`
  - 383-384행: 내부에서 `StudylistViewModel.Studies`까지 같이 들고 있음
  - 633-637행: 좌측 패널 내부 `Quick Start`
- `src/HnVue.App/MainWindow.xaml`
  - 114-121행: 좌측 패널과 중앙 viewer가 동시에 강제 배치됨

영향:

- PPT의 Worklist 창과 달리 현재 구현은 셸 일부다.
- 화면 단위 인지, 정보 우선순위, 밀도, 작업 흐름이 달라진다.

### 4. High: Studylist는 독립 창으로 구현되지 않았고, 런타임 진입도 연결되지 않았다

확인 결과:

- `StudylistView.xaml`은 별도 UserControl로 존재한다.
- 하지만 top-level window가 아니며, `MainWindow`에 별도 영역으로 연결돼 있지 않다.
- `PatientListView`의 `Worklist/Studylist` 탭은 스타일만 존재하고, 실제 표시 영역 전환 바인딩/트리거가 없다.

코드 근거:

- `src/HnVue.UI/Views/StudylistView.xaml`
  - 10-14행: PPT slide 7 기반 독립 screen으로 설명
  - 117-123행: header + PACS + DataGrid 구조
  - 196-221행: 기간 버튼 구현
- `src/HnVue.UI/Views/PatientListView.xaml`
  - 202-209행: `WorklistTab`, `StudylistTab` 선언
  - 해당 탭에 연결된 `Visibility`, `DataTrigger`, content switching 없음

영향:

- 문서상 준비된 `StudylistView`가 있어도 실제 사용자 런타임에서는 PPT의 별도 Studylist 창 경험이 나오지 않는다.
- 사용자가 탭을 눌러도 기대한 창 전환이 발생하지 않는다.

### 5. High: Acquisition이 PPT 최종안의 viewer 중심 독립 창이 아니라 우측 260px 세로 패널이다

PPT Acquisition 2안 기준:

- 상단 환자 정보
- 좌측 control rail
- 중앙 대형 black viewer surface
- 우측 도구 패널

현재 런타임:

- `Acquisition`은 우측 좁은 패널
- `PREPARE`, `EXPOSE`, `STOP` 중심의 compact control panel
- 환자 정보 상단 배치, viewer 중심 창 구조가 없다

코드 근거:

- `src/HnVue.UI/Views/WorkflowView.xaml`
  - 3-5행: `Right panel of MainWindow (260px wide)`
  - 18행: `d:DesignWidth="260"`
  - 59행: `Acquisition` 섹션 라벨
  - 111-141행: `PREPARE`, `EXPOSE`
  - 152-156행: 하단 Emergency Stop
- `src/HnVue.App/MainWindow.xaml`
  - 111행: 우측 열이 `260`

영향:

- PPT의 Acquisition 2안과 완전히 다른 정보 구조다.
- 현재 구현은 `control subpanel`에 가깝고, 독립 Acquisition 창이라고 보기 어렵다.

### 6. High: Image가 별도 창이 아니라 메인 셸 중앙 패널로 고정되어 있다

확인 결과:

- `ImageViewerView` 자체 설명이 `center panel X-ray image viewer`다.
- 런타임에서도 로그인 직후 중앙 패널에 항상 고정된다.

코드 근거:

- `src/HnVue.UI/Views/ImageViewerView.xaml`
  - 9행: `center panel X-ray image viewer`
  - 22행: `Image Viewer`
  - 49행: `ZOOM IN`
  - 99행: `RESET W/L`
  - 161행: `No image loaded...`
- `src/HnVue.App/MainWindow.xaml`
  - 119-121행: 중앙 열에 고정 배치

영향:

- PPT 및 개정 설계 기준의 `Image` 독립 화면 개념이 런타임에 없다.

### 7. Medium: Login 자동화 과정에서 Username ComboBox와 PasswordBox가 불안정하게 동작해 E2E 안정성이 낮다

확인 결과:

- `Username`은 editable ComboBox에 `SelectedValue` 바인딩
- Password는 `PasswordChanged` code-behind로 ViewModel에 전달
- 단순 자동 입력만으로는 `LoginCommand` 활성 조건이 자주 충족되지 않았다
- 실제 검증에서도 `ValuePattern.SetValue('admin')` + PasswordBox 직접 타이핑 + 마우스 클릭 조합이 필요했다

코드 근거:

- `src/HnVue.UI/Views/LoginView.xaml`
  - 84행: `ComboBox SelectedValue="{Binding Username ...}"`
  - 110-118행: `PasswordBox`, `PasswordChanged`
- `src/HnVue.UI.ViewModels/ViewModels/LoginViewModel.cs`
  - `CanLogin`: Username/Password가 모두 비어 있지 않아야 함

영향:

- E2E 자동화와 회귀 검증 재현성이 떨어진다.
- 런타임 사용성 문제로 번질 가능성도 있다.

## 화면별 평가

| 화면 | PPT 기대 | 런타임 상태 | 판정 |
|------|----------|-------------|------|
| Login | 미니멀 독립 로그인 화면 | 셸 위 카드형 overlay | Fail |
| Worklist | 독립 dual-list 창 | 좌측 패널 일부 | Fail |
| Studylist | 독립 창 | 별도 UserControl 존재하나 런타임 연결 부재 | Fail |
| Image | 독립 창 | 중앙 패널 고정 | Fail |
| Acquisition | viewer 중심 독립 창 | 우측 260px control panel | Fail |

## Coordinator 전달 필요 사항

1. `MainWindow`의 3열 셸 전제를 버리고, PPT 최종안 기준 화면 경계를 다시 정의해야 한다.
2. `StudylistView`는 이미 별도 View가 있으므로, runtime surface 연결 방식부터 재설계해야 한다.
3. `WorkflowView`는 독립 Acquisition 창이 아니라 `right-side subpanel`로 설계되어 있어 역할 재정의가 필요하다.
4. `ImageViewerView`의 중앙 패널 전제를 제거하지 않으면 `Image` 독립 화면 요구를 충족할 수 없다.
5. Login은 `ID dropdown` 일부만 맞고 전체 레이아웃 문법은 다시 설계해야 한다.

## 이번 턴에서 하지 않은 것

- 구현 모듈 수정
- ViewModel 계약 변경
- UI 리팩터링
- 테스트 코드 추가

Design Team 워크트리 제약에 따라 이번 작업은 런타임 검증과 증거 수집, Coordinator handoff 문서화까지만 수행했다.
