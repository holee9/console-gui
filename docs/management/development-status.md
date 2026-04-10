# HnVue 개발 진행 현황 및 로드맵

> 원본: README.md "개발 진행 현황" + "개발 로드맵" 섹션에서 분리 (2026-04-09)
> 최종 업데이트: 2026-04-10 (PROGRESS-002 반영)

## 현재 진도 요약

> **현재 1.5M차 / 12M, 소진 3.0 MM / 24~36 MM** | 상세: [PROGRESS-002](PROGRESS-002_DetailedAnalysis_v1.0.md) | [WBS v3.0](WBS-001_WBS_v3.0.md)

### 계획 대비 진도

| 항목 | 계획 (WBS v2.0) | 현재 (1.5M차) | 판정 |
|------|-----------------|---------------|------|
| **경과 / 전체** | 12M | 1.5M (12.5%) | - |
| **소진 MM / 전체** | 24~36 MM | 3.0 MM (12.5%) | - |
| **기능 완성도** | 100% @12M차 | ~48% @1.5M차 | 선행 |
| **WBS 완료율** | 100% @12M차 | ~40% @1.5M차 | 선행 |
| **MM 효율** | 1.0배 (계획) | 3.8배 (AI 병행) | - |

### 마일스톤

| MS | Milestone | 목표 | 잔여M | 전망 | 핵심 차단 |
|----|-----------|------|-------|------|----------|
| **MS1** | 설계 완료 | 2026-05-15 (2.5M차) | 1.0M | **ON TRACK** | STRIDE 완성 (0.3 MM) |
| **MS2** | Tier 1 구현 | 2026-08-31 (6M차) | 4.5M | **AT RISK** | Generator RS-232 (HW), TLS 1.3 |
| **MS3** | Tier 2 구현 | 2026-10-31 (8M차) | 6.5M | **AT RISK** | FPD SDK (벤더), UI 리디자인 잔여 |
| **MS4** | 통합 테스트 | 2026-12-15 (9.5M차) | 8.0M | WATCH | M2/M3 완료에 종속 |
| **MS5** | 시스템 테스트 | 2027-01-15 (10.5M차) | 9.0M | WATCH | 외부 침투 테스트 일정 확보 필요 |
| **MS6** | 릴리스 | 2027-03-01 (12M차) | 10.5M | WATCH | 전체 파이프라인 정상 진행 시 달성 가능 |

### WBS 섹션별 MM 현황

| WBS 섹션 | 계획 기간 | 계획 MM | 소진 MM | 잔여 MM | 완료율 | 판정 |
|----------|----------|---------|---------|---------|--------|------|
| 0~4. 설계/문서 | 1~3M차 | 5.5~8.0 | 2.0 | 0.2~0.8 | 92% | 선행 |
| **5. Tier 1 구현** | **2~6M차** | **5.0~8.0** | **0.4** | **2.0~3.5** | **52%** | **정시** |
| **6. Tier 2 구현** | **2~8M차** | **5.0~7.0** | **0.3** | **2.5~3.5** | **50%** | **선행** |
| 7. 검증 | 3~10M차 | 4.0~6.0 | 0.3 | 2.5~3.8 | 25% | 선행 |
| 8~11. 시험/인허가/릴리스 | 9~12M차 | 4.5~7.5 | 0 | 4.5~7.5 | 5% | 정시 |
| **합계** | **1~12M차** | **24~36** | **3.0** | **21.0~33.0** | **~40%** | - |

### CRITICAL 차단 항목

| 항목 | 잔여 MM | 차단 MS | 상태 |
|------|---------|---------|------|
| **App.xaml.cs Null Stub 6개 -> 실 Repo 교체** | 0.1 MM | 전체 앱 | **SW 즉시 해결 가능 -- 최우선** |
| Generator RS-232/TCP | 0.5 MM | MS2 (6M차) | HW 프로토콜/장비 미확보 |
| FPD Detector SDK | 0.5 MM | MS2/MS3 | 벤더 SDK 미확보 |
| PHI AES-256-GCM 완성 | 0.15 MM | MS2 (6M차) | 기본 SQLCipher 설정 완료, GCM 미완 |
| TLS 1.3 네트워크 암호화 | 0.2 MM | MS2 (6M차) | 미착수 |

> **해소된 CRIT**: 영상처리 W/L/Zoom (Phase 4에서 13메서드 구현, 88.7% 커버리지)

---

## Tier 1 작업 상세 (SW1 담당)

| WBS ID | 작업 | 계획 시작 | 계획 완료 | 진척% | 판정 |
|--------|------|----------|----------|-------|------|
| 5.1.1~3 | RBAC/bcrypt/잠금 | 2026-04 | 2026-05 | 95% | **DONE** |
| 5.1.4 | PHI SQLCipher | 2026-04 | 2026-05 | 60% | 진행중 |
| 5.1.5 | TLS 1.3 | 2026-04 | 2026-05 | 20% | 지연 |
| 5.1.6 | 감사로그 HMAC | 2026-05 | 2026-05 | 90% | **선행** |
| 5.1.7 | SBOM CycloneDX | 2026-07 | 2026-07 | 60% | **선행** |
| 5.1.8~10 | fo-dicom C-STORE/MWL/Print | 2026-05 | 2026-07 | 70% | **선행** |
| 5.1.11 | IHE SWF 상태머신 | 2026-06 | 2026-08 | 85% | **선행** |
| 5.1.12~13 | 인시던트/CVD | 2026-06 | 2026-07 | 80% | **선행** |
| 5.1.14~16 | SW 업데이트 | 2026-06 | 2026-08 | 75% | **선행** |
| 5.1.17 | STRIDE | 2026-07 | 2026-07 | 50% | 정시 |
| **5.1.18** | **Generator RS-232** | **2026-07** | **2026-08** | **15%** | **CRIT** |
| 5.1.19 | 선량 인터락 | 2026-07 | 2026-08 | 60% | 정시 |
| 5.1.20 | JWT 세션 잠금 | 2026-04 | 2026-04 | 95% | **DONE** |

## Tier 2 작업 상세 (SW2 담당)

| WBS ID | 작업 | 계획 시작 | 계획 완료 | 진척% | 판정 |
|--------|------|----------|----------|-------|------|
| 5.2.1~2 | MWL/PACS 전송 | 2026-04 | 2026-06 | 55% | 진행중 |
| 5.2.3~5 | 영상 W/L/Zoom/회전 | 2026-04 | 2026-06 | 75% | **선행** |
| 5.2.6 | 시스템 설정 UI | 2026-07 | 2026-07 | 40% | **선행** |
| 5.2.7~8 | DAP/DRL 선량 | 2026-05 | 2026-07 | 50% | **선행** |
| **5.2.9** | **FPD SDK 통합** | **2026-06** | **2026-08** | **15%** | **CRIT** |
| 5.2.10~13 | CD/DVD 버닝 | 2026-06 | 2026-08 | 65% | **선행** |
| 5.2.14 | 촬영 프로토콜 | 2026-08 | 2026-08 | 5% | 정시 |
| 5.2.15 | 자동 잠금 | 2026-08 | 2026-08 | 80% | **선행** |
| 5.2.16 | WPF MVVM | 2026-08 | 2026-10 | 75% | **선행** |
| 5.2.17 | 환자 검색 | 2026-08 | 2026-08 | 50% | **선행** |
| 5.2.18 | DICOM RDSR | 2026-05 | 2026-06 | 5% | 정시 |

---

## 모듈 구현 완성도

| 모듈 | 서비스 | Repository | 테스트 | 커버리지 | DI 등록 | 상태 |
|------|--------|-----------|--------|---------|---------|------|
| Common | N/A | N/A | 있음 | 높음 | N/A | **완료** |
| Data | N/A | 3 (User,Study,Audit) | 있음 | 높음 | 정상 | **완료** |
| Security | AuthService | N/A | 있음 | 90%+ | 정상 | **완료** |
| Workflow | WorkflowEngine | N/A | 64 TC | 91.4% | 정상 | **완료** |
| Dose | DoseService | DoseRepository | 있음 | **99.5%** | **Null Stub** | 구현완료/DI미교체 |
| Incident | IncidentService | IncidentRepository | 있음 | 94.2% | **Null Stub** | 구현완료/DI미교체 |
| Update | SWUpdateService | UpdateRepository | 있음 | 높음 | **Null Stub** | 구현완료/DI미교체 |
| PatientMgmt | PatientService | WorklistRepository | 있음 | 높음 | **Null Stub** | 구현완료/DI미교체 |
| SystemAdmin | SystemAdminService | SystemSettingsRepo | 있음 | 높음 | **Null Stub** | 구현완료/DI미교체 |
| CDBurning | CDDVDBurnService | StudyRepository | 있음 | 100% | **Null Stub** | 구현완료/DI미교체 |
| Dicom | DicomService+SCU | N/A | 있음 | ~70% | 정상 | **완료** |
| Detector | DetectorSimulator | N/A | 있음 | 91.7% | 정상 | **Stub** (SDK 대기) |
| Imaging | ImageProcessor (13메서드) | N/A | 54 TC | 88.7% | 정상 | **완료** |
| UI | 12 Views | N/A | 있음 | ~60% | N/A | **부분** (PPT 6개 미적용) |
| UI.Contracts | 19 인터페이스 | N/A | N/A | N/A | N/A | **완료** |
| UI.ViewModels | 14 VMs | N/A | 있음 | 중간 | N/A | **대부분 완료** (TODO 9건) |
| App | DI Root | N/A | N/A | N/A | **Null Stub 6개 잔존** | **미완** |

> **핵심 간극**: Repository 6개 구현 완료인데 App.xaml.cs DI에 Null Stub 잔존 -- 앱 실행 시 빈 데이터

---

## UI 화면별 상태

| View | XAML | ViewModel | PPT 디자인 적용 | PPT 슬라이드 | 상태 |
|------|:----:|:---------:|:--------------:|:----------:|------|
| LoginView | O | O | **완료** | 1 | **완료** |
| PatientListView | O | O | **완료** | 2-4 | **완료** |
| StudylistView | O | O | **완료** | 5-7 | **완료** |
| AddPatientProcedureView | O | O | 미완 | 8 | 부분 |
| WorkflowView | O | O | 미완 | 9-11 | 부분 |
| ImageViewerView | O | O | 미완 | 10-11 | 부분 |
| MergeView | O | O | 미완 | 12-13 | 부분 |
| SettingsView | O | O | 미완 | 14-21 | 부분 |
| DoseDisplayView | O | O | N/A | - | 부분 |
| CDBurnView | O | O | N/A | - | 부분 |
| QuickPinLockView | O | O | N/A | - | **완료** |
| SystemAdminView | O | O | PPT 범위 외 | - | 부분 |
| MainWindow | O | O | 탭 통합 완료 | - | 부분 |

> PPT 디자인 적용 완료: 3/9 화면 (Login, PatientList, Studylist)

---

## 선후관계 역전/꼬임 (PROGRESS-002 분석 결과)

| 역전 사항 | 원인 | 심각도 |
|----------|------|--------|
| **Null Stub 미교체** -- Repo 6개 완료인데 DI 미등록 | 팀간 담당 경계 누락 (진행순 문제) | **CRITICAL** |
| **테스트 > 구현 선행** -- 1,258개 테스트가 Stub 검증 | AI가 Stub+테스트 동시 생성 (진행순 문제) | HIGH |
| **Wave B가 Wave A 미완 중 착수** -- MainWindow 이미 통합 | UI 프레임워크는 View와 동시 개발이 자연 (계획 문제) | MEDIUM |
| **규제 문서 선행** -- SBOM/SOUP/CMP 이미 작성 | RA 팀 의도적 선행 (긍정적) | 없음 |

> 상세 분석: [PROGRESS-002](PROGRESS-002_DetailedAnalysis_v1.0.md)

---

## 개발 단계별 요약

### Pre-Wave (완료)
- 빌드 인프라 구성 (.NET 8.0.419, global.json, Directory.Build.props)
- 솔루션 스캐폴딩 (28개 프로젝트, 의존성 그래프)
- HnVue.Common 구현 (Result<T> Monad, 17개 인터페이스, Enum)

### Wave 1 (완료)
병렬 개발: 3개 worktree에서 동시 구현
- WT-1: HnVue.Data (EF Core + SQLCipher)
- WT-2: HnVue.Security (bcrypt + JWT + RBAC + 감사 로그)
- WT-3: HnVue.UI (MahApps.Metro 테마, LoginView, MainWindow)

### REF 루프 (완료)
Review-Evaluate-Fix 10-사이클. Wave 1 기반에서 누락 모듈 구현.

### Phase 1d (완료)
UI 통합 + 통합 테스트 (DI 완전 연결, 4가지 시나리오)

### 2차 품질 검증 (완료, 2026-04-05)
IMAPIComWrapper 테스트 추가, CDBurning 커버리지 53% -> 95.6%

### Wave A + B 병렬 개발 (완료, 2026-04-05)
3개 worktree 병렬 개발 + UI 통합 + 보안 강화 (테스트: 812개)

### Phase 2 품질 강화 (완료, 2026-04-06)
Gitea 이슈 23개 해결 + 전체 모듈 README 업데이트

### Phase 3 품질 강화 (완료, 2026-04-06)
이슈 #27~#40 해결, 영상 파이프라인 완성

### Phase 4 완료 (완료, 2026-04-07)
JWT Denylist, RDSR, FPD 검출기 추상화, GUI 교체 가능 아키텍처, **Imaging 13메서드 구현** (테스트: 1,135개)

### 6팀 운영 인프라 구축 (완료, 2026-04-08)
Worktree 기반 팀 분리 개발, QA/RA 자동화, UISPEC 문서 체계, DOC-042 CMP v1.0 발행

### Round 1 DISPATCH (완료, 2026-04-08~09)
- QA: CI 로컬 전환, Roslyn 분석기 통합
- Team A: NuGet 취약성 해소 (9.0.0), SPEC-INFRA-001 (12개 요구사항)
- Team B: 123개 신규 테스트 (Detector 91.7%, Dose 99.5%)
- Coordinator: 테스트 안정화, UI.Contracts
- RA: DOC-042 v1.1, SBOM v1.1 (45개), SOUP v1.1
- Design: PPT LoginView/PatientListView/StudylistView 리디자인

### 현재 단계 (2026-04-10)
- **즉시 필요**: App.xaml.cs Null Stub 6개 -> 실 Repository DI 교체
- **진행 예정**: PPT 디자인 미적용 View 6개 리디자인
- **M1 준비**: STRIDE 상세 구현 완성
- **외부 대기**: Generator RS-232 프로토콜 문서, FPD 벤더 SDK

---

## 로드맵

### Phase 1: 기초 인프라 완성 (완료)
Tier 1+2 총 31개 MR 코드 기반 구축 완료.

> Phase 1은 코드 기반 완성을 의미하며, 제품 릴리즈 준비 완료가 아님.

### Phase 1.5: Gap 분석 & 정합성 복원 (현재 단계)

| 작업 | 상태 | 비고 |
|------|:----:|------|
| **App.xaml.cs Null Stub DI 교체** | **미완** | **최우선 -- Repository 구현 완료인데 DI 미연결** |
| PPT 디자인 미적용 View 6개 리디자인 | 진행중 | Slides 8~21 |
| PHI AES-256-GCM + TLS 1.3 완성 | 미착수 | M1 전 필수 |
| STRIDE 구현 완성 | 진행중 | M1 Gate 항목 |
| 기존 HnVUE IFU 기반 Feature Gap 매핑 | 미착수 | IFU 기준 |
| 시험 보고서 현실화 (DOC-022~028) | 미착수 | 실 구현 기능 기준 |

### Phase 2: 핵심 기능 구현 (다음 단계)

| 작업 | 참조 자료 |
|------|---------|
| **Generator 실 프로토콜 구현** (RS-232/RS-422) | `API_MANUAL_241206.pdf` |
| **자사 FPD SDK 연동** (OwnDetectorAdapter) | 벤더 SDK 확보 후 |
| **fo-dicom Print SCU + RDSR 전송** | DICOM-001 |
| **PACS 비동기 전송 파이프라인** (30초 이내) | SWR-WF-* |

### Phase 3: 품질 완성 & 인허가 준비

- Production 보안 설정 (환경변수 기반 Secret 관리)
- DICOM 실환경 검증 (DCM4CHEE 또는 고객 PACS)
- 시험 재수행 (UT/IT/ST 실 데이터)
- KTL 사이버보안 모의침투 (IEC 81001-5-1)
- DHF/510(k)/CE 문서 최종화 및 인허가 제출

### Phase 4: 경쟁 차별화 (인허가 취득 후)

- 영상 처리 자체 엔진 고도화 (AI 기반 자동 분석)
- 클라우드 연동, 웹 UI 추가, 성능 최적화

**현실적 1차 릴리즈: 2027년 Q2~Q3**

---

문서 최종 업데이트: 2026-04-10
