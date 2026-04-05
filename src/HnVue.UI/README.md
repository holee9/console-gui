# HnVue.UI

> WPF UI 컴포넌트 (MVVM, ViewModel, 컨버터)

## 목적

WPF UI 인프라스트럭처를 제공합니다. MVVM 패턴 지원(CommunityToolkit.Mvvm), MahApps.Metro 테마, LiveCharts 차트 컴포넌트, 전체 ViewModel 계층을 포함합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `MainViewModel` | 최상위 ViewModel — 자식 ViewModel 오케스트레이션 |
| `LoginViewModel` | 로그인 뷰 — JWT 인증 + `LoginSucceeded` 이벤트 발행 |
| `PatientListViewModel` | 환자 목록 뷰 ViewModel |
| `ImageViewerViewModel` | 영상 뷰어 ViewModel |
| `WorkflowViewModel` | 워크플로우 상태 표시 ViewModel |
| `DoseDisplayViewModel` | 방사선 선량 표시 ViewModel |
| `DoseViewModel` | 선량 통계 차트 ViewModel |
| `SystemAdminViewModel` | 시스템 관리 ViewModel |
| `CDBurnViewModel` | CD/DVD 소각 ViewModel |
| `NullToVisibilityConverter` | null → Visibility 변환 컨버터 |
| `LoginSuccessEventArgs` | 로그인 성공 이벤트 인자 |

## Wave B UI 통합

`MainWindow`에 연결된 View 구성:
- **오버레이:** `LoginView` — 인증 전 전체화면, 인증 성공 시 `LoginSucceeded` 이벤트로 해제
- **왼쪽 패널:** `PatientListView`
- **중앙 패널:** `ImageViewerView`
- **오른쪽 패널:** `WorkflowView` + `DoseDisplayView`

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

- `CommunityToolkit.Mvvm`
- `MahApps.Metro`
- `LiveChartsCore.SkiaSharpView.WPF`

## DI 등록

없음 (자식 ViewModel들은 HnVue.App에서 DI 등록)

## 비고

- CommunityToolkit.Mvvm — ObservableObject, RelayCommand
- MahApps.Metro — Modern WPF 테마
- LiveCharts2 — 선량 통계 차트
