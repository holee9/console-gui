# HnVue.UI

> WPF UI 컴포넌트 (MVVM, 컨버터)

## 목적

WPF UI 인프라스트럭처를 제공합니다. MVVM 패턴 지원(CommunityToolkit.Mvvm), MahApps.Metro 테마, LiveCharts 차트 컴포넌트를 포함합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `NullToVisibilityConverter` | null → Visibility 변환 컨버터 |
| `LoginSuccessEventArgs` | 로그인 성공 이벤트 인자 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

- `CommunityToolkit.Mvvm`
- `MahApps.Metro`
- `LiveChartsCore.SkiaSharpView.WPF`

## DI 등록

없음

## 비고

- CommunityToolkit.Mvvm — ObservableObject, RelayCommand
- MahApps.Metro — Modern WPF 테마
- LiveCharts2 — 선량 통계 차트
