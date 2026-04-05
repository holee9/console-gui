# HnVue.App

> WPF 애플리케이션 진입점 (Composition Root)

## 목적

HnVue Console SW의 메인 실행 프로젝트입니다. WPF `App.xaml`과 `MainWindow.xaml`을 포함하며, 모든 모듈의 DI 등록과 애플리케이션 수명 주기를 관리합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `App` | WPF Application 클래스 — DI 컨테이너 구성 및 startup |
| `MainWindow` | 메인 윈도우 — 전체 UI 레이아웃 호스트 |

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

Composition Root — `AddHnVueCommon()`, `AddHnVueData()`, `AddHnVueSecurity()` 등 모든 모듈을 등록합니다.

## 비고

- `OutputType: WinExe` (WPF 애플리케이션)
- 모든 src 프로젝트를 참조하는 유일한 프로젝트
