# HnVue.App

> WPF 애플리케이션 진입점 (Composition Root)

## 목적

HnVue Console SW의 메인 실행 프로젝트입니다. WPF `App.xaml`과 `MainWindow.xaml`을 포함하며, 모든 모듈의 DI 등록과 애플리케이션 수명 주기를 관리합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `App` | WPF Application 클래스 — DI 컨테이너 구성, 자식 ViewModel 등록 |
| `MainWindow` | 메인 윈도우 — 3컬럼 레이아웃 + 로그인 오버레이 (Wave B) |

## UI 레이아웃 (Wave B)

`MainWindow.xaml` 3컬럼 레이아웃:
- **왼쪽:** `PatientListView` — 환자 목록
- **중앙:** `ImageViewerView` — 영상 뷰어
- **오른쪽:** `WorkflowView` + `DoseDisplayView`
- **오버레이:** `LoginView` — 인증 전 전체화면 표시; `LoginSucceeded` 이벤트 후 메인 컨텐츠로 전환

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Dicom`
- `HnVue.Workflow`
- `HnVue.Imaging`
- `HnVue.Dose`
- `HnVue.PatientManagement`
- `HnVue.Incident`
- `HnVue.Update`
- `HnVue.CDBurning`
- `HnVue.SystemAdmin`
- `HnVue.UI`

### NuGet 패키지

없음

## DI 등록

Composition Root — `AddHnVueCommon()`, `AddHnVueData()`, `AddHnVueSecurity()` 등 모든 모듈 등록.

Wave B에서 추가된 자식 ViewModel DI 등록:
- `PatientListViewModel`
- `ImageViewerViewModel`
- `WorkflowViewModel`
- `DoseDisplayViewModel`

## 비고

- `OutputType: WinExe` (WPF 애플리케이션)
- 모든 src 프로젝트를 참조하는 유일한 프로젝트
- `appsettings.Development.json`: 개발용 플레이스홀더 (git 추적 제외 — 자격증명 보호)
