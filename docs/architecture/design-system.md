# HnVue 디자인 시스템 및 UI 화면 구성

> 원본: README.md "디자인 시스템" + "UI 화면 구성" 섹션에서 분리 (2026-04-09)

## 디자인 원칙

HnVue는 **의료기기 전용 디자인 시스템**을 기반으로 UI/UX를 설계합니다. IEC 62366 사용성 공학 기준을 준수하며, 다크 테마 최적화되어 있습니다.

| 원칙 | 설명 |
|------|------|
| **환자 안전 최우선** | 환자 ID 오병합 방지, 긴급정지 버튼 항상 표시 |
| **IEC 62366 준수** | 44px 터치 타겟, 색상 대비비 4.5:1 이상 |
| **다크 테마 기본** | 방사선실 환경 최적화 (눈부심 방지) |
| **PPT 기반 구현** | HnVUE UI 변경 최종안_251118.pptx 22슬라이드 |

## 색상 토큰

| 토큰 | 색상 | 용도 |
|------|------|------|
| BackgroundPage | #242424 | 주 배경 |
| BackgroundPanel | #2A2A2A | 패널/사이드바 |
| BackgroundCard | #3B3B3B | 카드/행 |
| Primary | #1B4F8A | 브랜드 기본 (MahApps.Metro Blue) |
| Accent | #00AEEF | 포커스, 강조 |
| Safe/Success | #00C853 | 안전 상태 |
| Warning | #FFD600 | 경고 상태 |
| Emergency/Error | #D50000 | 비상 정지, 오류 |

## 안전 색상 (IEC 62366) — 3개 테마 모두 보장

| 상태 | Dark | Light | High-Contrast |
|------|------|-------|---------------|
| Safe/Idle | `#00C853` | `#2E7D32` | `#00FF00` |
| Warning | `#FFD600` | `#F57F17` | `#FFFF00` |
| Blocked | `#FF6D00` | `#E65100` | `#FF8800` |
| Emergency | `#D50000` | `#C62828` | `#FF0000` |

## UISPEC 문서 체계

PPT 디자인과 XAML 구현 사이의 브릿지 역할을 하는 9개 UISPEC 문서가 `docs/design/spec/`에 정의되어 있습니다.

| UISPEC | 화면 | PPT 슬라이드 | 상태 |
|--------|------|------------|------|
| **UISPEC-001_Login** | 로그인 | 1-3 | 95% 준수 |
| **UISPEC-002_Worklist** | 워크리스트 | 2-4 | 44% 준수 |
| **UISPEC-003_Studylist** | 스터디리스트 | 5-7 | 63% 준수 |
| **UISPEC-004_Acquisition** | 촬영 | 9-11 | 0% (안전 중요) |
| **UISPEC-005_AddPatient** | 환자/시술 추가 | 8 | 0% |
| **UISPEC-006_Merge** | 영상 병합 | 12-13 | 0% |
| **UISPEC-007_Settings** | 설정 | 14-22 | 0% |
| **UISPEC-008_ImageViewer** | 이미지 뷰어 | - | 0% |
| **UISPEC-009_SystemAdmin** | 시스템 관리 | - | 0% |

## 디자인 참고 문서

| 문서 | 경로 | 설명 |
|------|------|------|
| **UI_DESIGN_MASTER_REFERENCE** | `docs/design/` | WPF 구현 패턴, IEC 62366 체크리스트 |
| **UI_CHANGELOG** | `docs/design/` | 디자인 시스템 변경 이력 |
| **PPT 디자인 소스** | `docs/★HnVUE UI 변경 최종안_251118.pptx` | 22슬라이드 원본 디자인 |

> **참고**: HTML mockup 파일은 더 이상 사용되지 않습니다. UISPEC 문서와 PPT 디자인 소스를 직접 참조하여 XAML을 구현하세요.

---

## UI 화면 구성

### 구현된 WPF 화면 목록 (PPT 기준)

| 화면 ID | 뷰 파일 | PPT 슬라이드 | 주요 변경사항 | 상태 |
|---------|---------|------------|------------|------|
| SCR-LOGIN | LoginView.xaml | 슬라이드 1 | Username -> ComboBox 드롭다운 | 완료 |
| SCR-WORKLIST | PatientListView.xaml | 슬라이드 4 | 기간 필터 버튼 (Today/3Days/1Week/All/1Month) | 완료 |
| SCR-STUDYLIST | StudylistView.xaml | 슬라이드 7 | 이전/다음 내비, PACS 서버 선택, 기간 필터 | 완료 |
| SCR-ACQUISITION | WorkflowView.xaml | 슬라이드 9~11 | 환자 정보 표시 개선 (우측 패널 260px) | 완료 |
| SCR-ADD-PT | AddPatientProcedureView.xaml | 슬라이드 8 | Patient+Procedure 통합, (*) 필수, Auto-Generate, 칩 UI | 완료 |
| SCR-SYNC | MergeView.xaml | 슬라이드 13 | "Sync Study" 명칭, 3열 레이아웃, Preview | 완료 |
| SCR-SETTINGS | SettingsView.xaml | 슬라이드 14~21 | 상단 탭, Network 통합, Access Notice | 완료 |

### PPT 명칭 변경 사항

| 기존 | 신규 | 위치 |
|------|------|------|
| Same Studylist | **Sync Study** | MergeView 버튼/제목 |
| Login Popup | **Access Notice** | SettingsView System 탭 |
| Only No matching | **Un-Matched** | SettingsView RIS Code 서브탭 |

---

문서 최종 업데이트: 2026-04-09
