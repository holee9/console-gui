# HnVue Console SW -- 계획 대비 구현 현황 정밀 분석

| 항목 | 내용 |
|------|------|
| **문서 ID** | PROGRESS-002 |
| **버전** | v1.0 |
| **기준 시점** | 2026-04-10 (프로젝트 시작 후 약 1.5M차, 소진 약 3.0 MM) |
| **작성 근거** | WBS-001 v2.0, STRATEGY-002, DISPATCH 기록, Git 이력, 코드베이스 실측 |
| **목적** | 원본 계획(WBS v2.0) 대비 실제 진행 현황 정밀 분석, 선후관계 역전/꼬임 식별 |

---

## 1. 원본 계획 요약 (WBS v2.0 -- 변경 없음)

### 마일스톤 계획

| MS | 목표 | 목표 시기 | 핵심 기준 |
|----|------|----------|----------|
| M1 | 설계 완료 | 2026-05-15 | SWR 전체 SAD/SDS 반영, STRIDE 완성 |
| M2 | Tier 1 구현 | 2026-08-31 | RBAC/PHI/DICOM/IHE/인시던트/업데이트/STRIDE |
| M3 | Tier 2 구현 | 2026-10-31 | MWL/영상처리/선량/CD버닝/UI완성 |
| M4 | 통합 테스트 | 2026-12-15 | Tier 1+2 통합 테스트 전체 통과 |
| M5 | 시스템 테스트 | 2027-01-15 | E2E/침투/사용성 테스트 |
| M6 | 릴리스 | 2027-03-01 | DHF 완성, eSTAR 제출 |

### 계획상 실행 순서 (의도된 선후관계)

```
Phase 1d (설계): MRD/PRD/FRS/SRS/SAD/SDS  -->  M1
Phase 1e (Tier1 구현 SW1): RBAC -> PHI -> 감사로그 -> DICOM -> IHE SWF -> 인시던트/업데이트 -> STRIDE -> Generator RS-232  -->  M2
Phase 1f (Tier2 구현 SW2): MWL -> 영상처리 -> 선량 -> CD버닝 -> FPD -> UI MVVM  -->  M3
Phase 1g (검증): 단위테스트 -> 통합테스트 -> 보안테스트  -->  M4
Phase 1h (시스템테스트): E2E -> 침투 -> 사용성  -->  M5
Phase 1i (인허가): RTM -> DHF -> eSTAR  -->  M6
```

---

## 2. 실제 개발 진행 순서 (Git 이력 기반)

### 타임라인 (실제 발생 순서)

```
2026-03 초    MRD v1.0~v3.0, PRD, FRS, SRS 작성
2026-03 말    SAD v2.0, SDS v2.0 작성
2026-04 초    17개 모듈 프로젝트 구조 + 전체 인터페이스 설계 + 서비스 계층 구현
              - RBAC/bcrypt/JWT [Tier 1]
              - 감사로그 HMAC [Tier 1]
              - DICOM fo-dicom C-STORE/MWL [Tier 1]
              - Workflow 상태머신 + 시뮬레이터 [Tier 1]
              - DoseService/IncidentService/UpdateService [Tier 1]
              - PatientService/WorklistService [Tier 2]
              - CDBurnService/Imaging스텁 [Tier 2]
              - LoginView XAML [Tier 2]
              - DI 컴포지션 루트 (Null Stub 패턴) [통합]
              - 단위 테스트 ~900개
2026-04-05    STRATEGY-002 병렬개발전략 수립 (Wave A/B/C)
2026-04-06    MRD v4.0 (글로벌 리서치, MR-073~092)
2026-04-07    6팀 운영 인프라 구축 (Worktree, 하네스)
2026-04-08    Round 1 DISPATCH -- 전 팀 완료
              - QA: CI 인프라 로컬 전환, Roslyn 분석기
              - Team A: NuGet 취약성 해소 (9.0.0)
              - Team B: GlobalSuppressions, SCS0005 억제
              - Coordinator: 테스트 안정화, UI.Contracts
              - RA: DOC-042 CMP v1.1, SBOM v1.1, SOUP v1.1
              - Design: PPT 기반 LoginView/PatientListView 리디자인
2026-04-08~09 Round 2 DISPATCH
              - Team A: SPEC-INFRA-001 (12개 요구사항, 보안+데이터+업데이트)
              - Team B: 커버리지 Push (123개 신규 테스트, Detector 91.7%, Dose 99.5%)
              - Design: StudylistView PPT 리디자인, 이슈 #59 수정
2026-04-09    통합 리포트: 전체 1,258 테스트, 5개 팀 완료
2026-04-10    현재 시점
```

---

## 3. 계획 vs 실제 대비 매트릭스

### 3.1 WBS Tier 1 작업 (SW1 담당) -- 계획 대비 실제

| WBS ID | 작업 | 계획 시작 | 계획 완료 | 실제 상태 | 진척% | 판정 | 비고 |
|--------|------|----------|----------|----------|-------|------|------|
| 5.1.1~3 | RBAC/bcrypt/잠금 | 2026-04 | 2026-05 | **구현 완료** | 95% | DONE | 계획보다 선행 |
| 5.1.4 | PHI SQLCipher | 2026-04 | 2026-05 | **부분 구현** (AES-256 pragma 설정됨) | 60% | 진행중 | AES-256-GCM 미완 |
| 5.1.5 | TLS 1.3 | 2026-04 | 2026-05 | **미착수** | 20% | 지연 | 네트워크 계층 미구현 |
| 5.1.6 | 감사로그 HMAC | 2026-05 | 2026-05 | **구현 완료** (Serilog + HMAC) | 90% | 선행 | 계획보다 1M 선행 착수 |
| 5.1.7 | SBOM CycloneDX | 2026-07 | 2026-07 | **부분 완료** (CycloneDX 1.5, 45개) | 60% | 선행 | 2.5M 선행 착수 |
| 5.1.8~10 | fo-dicom | 2026-05 | 2026-07 | **구현 완료** (C-STORE/MWL/C-FIND) | 70% | 선행 | Print SCU 미완 |
| 5.1.11 | IHE SWF | 2026-06 | 2026-08 | **구현 완료** (9-state FSM) | 85% | 선행 | 2M 선행 착수 |
| 5.1.12~13 | 인시던트/CVD | 2026-06 | 2026-07 | **서비스 완료, Repo 완료** | 80% | 선행 | CVD 프로세스 미완 |
| 5.1.14~16 | SW 업데이트 | 2026-06 | 2026-08 | **서비스 완료, Repo 완료** | 75% | 선행 | 코드서명 검증 미테스트 |
| 5.1.17 | STRIDE | 2026-07 | 2026-07 | **위협모델 문서 존재** | 50% | 정시 | 구현 수준 미완 |
| **5.1.18** | **Generator RS-232** | **2026-07** | **2026-08** | **시뮬레이터만 존재** | **15%** | **CRIT** | 실제 RS-232 미구현 |
| **5.1.19** | **선량 인터락** | **2026-07** | **2026-08** | **DoseService 완료, 인터락 로직 존재** | **60%** | 수정 | PROGRESS-001 대비 상향 |
| 5.1.20 | JWT 세션 잠금 | 2026-04 | 2026-04 | **구현 완료** | 95% | DONE | |
| WP-T1-ERR | 에러 처리 매트릭스 | 2026-06 | 2026-06 | **부분 구현** | 50% | 정시 | |

### 3.2 WBS Tier 2 작업 (SW2 담당) -- 계획 대비 실제

| WBS ID | 작업 | 계획 시작 | 계획 완료 | 실제 상태 | 진척% | 판정 | 비고 |
|--------|------|----------|----------|----------|-------|------|------|
| 5.2.1~2 | MWL/PACS 전송 | 2026-04 | 2026-06 | **MWL C-FIND 구현, PACS C-STORE 구현** | 55% | 진행중 | 비동기 파이프라인 미완 |
| **5.2.3~5** | **영상 W/L/Zoom/회전** | **2026-04** | **2026-06** | **구현 완료** (13개 메서드, 88.7% 커버리지) | **75%** | 선행 | Math.Sqrt 일부 잔존하나 실 구현 상태 |
| 5.2.6 | 시스템 설정 UI | 2026-07 | 2026-07 | **SettingsView.xaml 존재, VM 존재** | 40% | 선행 | 2.5M 선행 착수 |
| 5.2.7~8 | DAP/DRL 선량 | 2026-05 | 2026-07 | **DoseService 완료, DoseDisplayView 존재** | 50% | 선행 | |
| **5.2.9** | **FPD SDK 통합** | **2026-06** | **2026-08** | **어댑터 패턴만, SDK 미확보** | **15%** | **CRIT** | Detector 테스트 91.7% |
| 5.2.10~13 | CD/DVD 버닝 | 2026-06 | 2026-08 | **서비스+IMAPI+Repo 완료** | 65% | 선행 | 2M 선행 착수 |
| 5.2.14 | 촬영 프로토콜 | 2026-08 | 2026-08 | **미착수** | 5% | 정시 | |
| 5.2.15 | 자동 잠금 | 2026-08 | 2026-08 | **QuickPinLockView+VM 존재** | 80% | 선행 | |
| 5.2.16 | WPF MVVM | 2026-08 | 2026-10 | **15개 VM, 12개 View 완성** | 75% | 선행 | 큰 선행 |
| 5.2.17 | 환자 검색 | 2026-08 | 2026-08 | **PatientService 필터링 구현** | 50% | 선행 | |
| 5.2.18 | DICOM RDSR | 2026-05 | 2026-06 | **미착수** | 5% | 정시 | |

### 3.3 STRATEGY-002 Wave 분류 대비 실제

| Task | 계획 Wave | 계획 모드 | 실제 상태 | 비고 |
|------|----------|----------|----------|------|
| T-01~03 Imaging DICOM파싱/Pan/Zoom/VOI | Wave A WT-1 | 병렬 | **구현 완료** (13메서드, 54테스트) | Phase 4에서 구현됨 |
| T-04 PatientListView | Wave A WT-2 | 병렬 | **완료** (PPT 리디자인) | |
| T-05 WorkflowView | Wave A WT-2 | 병렬 | **완료** (XAML+VM 존재) | |
| T-06 DoseMonitorView | Wave A WT-2 | 병렬 | **완료** (DoseDisplayView) | |
| T-07 ImageViewerView | Wave A WT-2 | 병렬 | **완료** (XAML+VM 존재) | |
| T-08 DoseRepository | Wave A WT-3 | 병렬 | **완료** (EF 구현) | |
| T-09 WorklistRepository | Wave A WT-3 | 병렬 | **완료** (MWL 연동) | |
| T-10 IncidentRepository | Wave A WT-3 | 병렬 | **완료** (EF 구현) | |
| T-11 SystemSettingsRepository | Wave A WT-3 | 병렬 | **완료** | |
| T-12 UpdateRepository | Wave A WT-3 | 병렬 | **완료** | |
| T-13 CDBurning StudyRepository | Wave A WT-3 | 병렬 | **완료** | |
| T-14 GeneratorSerialPort | Wave A WT-4 | 병렬 | **파일 존재, 실제 RS-232 미구현** | HW 의존 |
| T-15 MainWindow 패널 통합 | Wave B | 단독 | **부분 완료** (탭+View연결) | 선후역전: Wave A 미완 중 착수 |
| T-16 MainViewModel 확장 | Wave B | 단독 | **완료** (내비게이션 로직) | 선후역전: Wave A 미완 중 착수 |
| T-17 App.xaml.cs 스텁 교체 | Wave B | 단독 | **미완** (6개 Null Stub 잔존) | **핵심 미완** |
| T-18 Generator 시뮬레이터 교체 | Wave B | 단독 | **미완** | HW 의존 |
| T-19 단위 테스트 | Wave B | 병렬 | **선행 완료** (1,258개) | 계획과 역전 |
| T-20 E2E 통합 테스트 | Wave C | 단독 | **미착수** | |
| T-21 IEC 62304 검증 | Wave C | 단독 | **미착수** | |

---

## 4. 선후관계 역전/꼬임 분석

### 4.1 발견된 역전 사항

#### 역전 1: 단위 테스트가 구현보다 선행 (T-19 vs T-01~T-03)

- **계획**: Wave A 구현 (T-01~T-14) -> Wave B 테스트 (T-19)
- **실제**: 테스트(T-19) 1,258개 이미 완료, 그런데 Imaging(T-01~03)은 Stub
- **원인**: AI 에이전트가 서비스 계층 + 테스트를 동시에 생성했으나, 핵심 알고리즘(영상처리)은 Stub으로 남김
- **영향**: 테스트가 Stub 구현을 검증하고 있어 실질 커버리지 과대 평가 가능
- **판정**: **개발 진행순 문제** -- 테스트가 구현 완성 전에 작성됨

#### 역전 2: Wave B 작업이 Wave A 완료 전에 착수 (T-15, T-16)

- **계획**: Wave A 전체 완료 -> Wave B 순차 시작
- **실제**: MainWindow 탭 통합(T-15), MainViewModel 확장(T-16)이 이미 구현됨
- **원인**: STRATEGY-002는 Wave A 완료를 전제로 Wave B를 설계했으나, 실제로는 UI 프레임워크를 먼저 만들고 View를 채움
- **영향**: View가 Null Stub 서비스에 바인딩되어 실행 시 빈 데이터 표시
- **판정**: **기존 계획 문제** -- UI 프레임워크는 View와 동시에 만드는 것이 자연스러움. 계획이 과도하게 순차적

#### 역전 3: Null Stub 교체(T-17) 미완 + Repository 구현 완료

- **계획**: Wave A WT-3에서 Repository 구현 -> Wave B에서 App.xaml.cs Null Stub 교체
- **실제**: Repository 6개 모두 구현 완료, 그러나 App.xaml.cs에 여전히 Null Stub 6개 등록
- **원인**: 팀별 DISPATCH에서 Repository는 구현했지만, App.xaml.cs DI 등록 교체는 Coordinator 담당이라 미완
- **영향**: **앱 실행 시 실제 Repository가 아닌 Null Stub 사용됨** -- 가장 심각한 간극
- **판정**: **개발 진행순 문제** -- Repository 구현과 DI 교체를 동일 DISPATCH에서 처리했어야 함

#### 역전 4: 규제 문서가 구현보다 선행 (SBOM, SOUP, CMP)

- **계획**: 구현 -> 검증 -> 규제 문서 (인허가 섹션은 11~12M차)
- **실제**: SBOM v1.1(45개 컴포넌트), SOUP v1.1, CMP v1.1 이미 작성
- **영향**: 긍정적 -- 규제 문서 조기 정비는 후속 인허가 준비에 유리
- **판정**: **긍정적 역전** -- 의도적 선행 (RA 팀 DISPATCH 결과)

### 4.2 꼬인 의존성

```
문제: App.xaml.cs Null Stub 병목
     ┌──────────────────────────────────────────────────────┐
     │  Repository 6개 (완료)   -->  App.xaml.cs (미교체)    │
     │  DoseRepository           ┌─> NullDoseRepository     │ <-- 여기가 꼬임
     │  WorklistRepository       ├─> NullWorklistRepository │
     │  IncidentRepository       ├─> NullIncidentRepository │
     │  UpdateRepository         ├─> NullUpdateRepository   │
     │  SystemSettingsRepository ├─> NullSystemSettings...  │
     │  CDBurning.StudyRepo      └─> NullCdStudyRepository  │
     └──────────────────────────────────────────────────────┘
     --> 결과: 앱 실행 시 모든 서비스가 Null 동작
```

---

## 5. 모듈별 구현 완성도 실측

### 5.1 백엔드 모듈 (src/)

| 모듈 | .cs 파일 | 서비스 | Repository | 테스트 | 테스트 커버리지 | Null Stub? |
|------|---------|--------|-----------|--------|--------------|-----------|
| HnVue.Common | 17개 인터페이스 | N/A | N/A | 있음 | 높음 | N/A |
| HnVue.Data | DbContext+3 Repo | N/A | 3 (User,Study,Audit) | 있음 | 높음 | N/A |
| HnVue.Security | 완료 | AuthService | N/A | 있음 | 높음 | N/A |
| HnVue.Workflow | 완료 | WorkflowEngine+FSM | N/A | 있음 | 91.4% | N/A |
| HnVue.Dose | 완료 | DoseService | DoseRepository | 있음 | **99.5%** | **잔존** |
| HnVue.Incident | 완료 | IncidentService | IncidentRepository | 있음 | 94.2% | **잔존** |
| HnVue.Update | 완료 | SWUpdateService | UpdateRepository | 있음 | 높음 | **잔존** |
| HnVue.PatientMgmt | 완료 | PatientService+Worklist | WorklistRepository | 있음 | 높음 | **잔존** |
| HnVue.SystemAdmin | 완료 | SystemAdminService | SystemSettingsRepo | 있음 | 높음 | **잔존** |
| HnVue.CDBurning | 완료 | CDDVDBurnService | StudyRepository | 있음 | 100% | **잔존** |
| HnVue.Dicom | 완료 | DicomService+SCU | N/A | 있음 | ~66.9%->향상중 | N/A |
| HnVue.Detector | 어댑터 패턴 | DetectorSimulator | N/A | 있음 | **91.7%** | N/A |
| HnVue.Imaging | 완료 | ImageProcessor (13메서드) | N/A | 있음 (54 TC) | 88.7% | N/A |

### 5.2 UI 모듈

| View | XAML | ViewModel | PPT 디자인 적용 | 상태 |
|------|------|-----------|----------------|------|
| LoginView | 있음 | LoginViewModel | 완료 (Slide 1) | **완료** |
| PatientListView | 있음 | PatientListViewModel | 완료 (Slides 2-4) | **완료** |
| StudylistView | 있음 | StudylistViewModel | 완료 (Slides 5-7) | **완료** |
| AddPatientProcedureView | 있음 | AddPatientProcedureVM | 미완 (Slide 8) | 부분 |
| WorkflowView | 있음 | WorkflowViewModel | 미완 (Slides 9-11) | 부분 |
| ImageViewerView | 있음 | ImageViewerViewModel | 미완 (Slides 10-11) | **Stub** |
| MergeView | 있음 | MergeViewModel | 미완 (Slides 12-13) | 부분 |
| SettingsView | 있음 | SettingsViewModel | 미완 (Slides 14-21) | 부분 |
| DoseDisplayView | 있음 | DoseDisplayViewModel | N/A | 부분 |
| CDBurnView | 있음 | CDBurnViewModel | N/A | 부분 |
| QuickPinLockView | 있음 | QuickPinLockViewModel | N/A | 완료 |
| SystemAdminView | 있음 | SystemAdminViewModel | PPT 범위 외 | 부분 |
| MainWindow | 있음 | MainViewModel | 탭 통합 완료 | **부분** |

### 5.3 테스트 현황

- 전체 테스트: **~1,258개** (xUnit, 2026-04-09 기준)
- 전체 통과율: **100%** (0 실패)
- 안전임계 커버리지: Dose **99.5%**, Incident **94.2%** (90% 게이트 통과)
- 아키텍처 테스트: HnVue.Architecture.Tests 존재 (Clean Architecture 검증)

---

## 6. 핵심 미완 항목 (잔여 작업 목록)

### Priority CRITICAL (앱 기능 차단)

| # | 항목 | 잔여 공수 | 차단 대상 | 비고 |
|---|------|----------|----------|------|
| C-1 | **App.xaml.cs Null Stub 6개 -> 실제 Repository 교체** | 0.1 MM | 전체 앱 | 가장 긴급 -- Repository는 구현 완료인데 DI 미교체 |
| ~~C-2~~ | ~~Imaging DICOM 헤더 파싱~~ | ~~0.3 MM~~ | -- | **해소됨**: 13개 메서드 구현, 88.7% 커버리지 |
| ~~C-3~~ | ~~Imaging Pan/Zoom/W-L~~ | ~~0.4 MM~~ | -- | **해소됨**: W/L, Zoom, Pan, Rotate, Flip, CLAHE 등 구현 |
| C-4 | **Generator RS-232 실 구현** (T-14) | 0.5 MM | 촬영 워크플로우 | HW 프로토콜 필요 |
| C-5 | **FPD SDK 통합** (5.2.9) | 0.5 MM | 영상 수신 | 벤더 SDK 필요 |

### Priority HIGH

| # | 항목 | 잔여 공수 | 비고 |
|---|------|----------|------|
| H-1 | PHI AES-256-GCM 완성 | 0.15 MM | SQLCipher 기본 설정은 됨 |
| H-2 | TLS 1.3 네트워크 암호화 | 0.2 MM | 미착수 |
| H-3 | fo-dicom Print SCU | 0.2 MM | C-STORE/MWL은 완료 |
| H-4 | DICOM RDSR 생성/전송 | 0.2 MM | 미착수 |
| H-5 | PPT 디자인 미적용 View 6개 | 0.3 MM | Slides 8~21 |
| H-6 | E2E 통합 테스트 | 0.5 MM | Wave C 미착수 |
| H-7 | STRIDE 구현 완성 | 0.3 MM | 위협모델 문서만 존재 |

### Priority MEDIUM

| # | 항목 | 잔여 공수 | 비고 |
|---|------|----------|------|
| M-1 | PACS 비동기 전송 파이프라인 | 0.2 MM | 30초 이내 목표 |
| M-2 | CVD 프로세스/CVE 모니터링 | 0.15 MM | 인시던트 부속 |
| M-3 | 촬영 프로토콜 관리 | 0.2 MM | 미착수 |
| M-4 | 에러 처리 매트릭스 완성 | 0.15 MM | 부분 구현 |

---

## 7. 계획 문제 vs 진행순 문제 판별

| 역전 사항 | 원인 | 판별 | 해결 방안 |
|----------|------|------|----------|
| 테스트가 구현보다 선행 | AI 에이전트가 Stub+테스트 동시 생성 | **진행순 문제** | Stub을 실제 구현으로 교체 후 테스트 재검증 |
| Wave B가 Wave A 완료 전 착수 | UI 프레임워크는 View와 동시 개발이 자연스러움 | **계획 문제** | Wave A/B 경계를 "서비스 구현 -> DI 통합"으로 재정의 |
| Null Stub 미교체 | 팀간 담당 경계에서 누락 | **진행순 문제** | Coordinator DISPATCH에서 T-17 즉시 처리 |
| 규제 문서 선행 | 의도적 선행 (RA 팀) | **긍정적 역전** | 유지 -- 인허가 준비에 유리 |
| Imaging Stub 잔존 | HW 의존 + 알고리즘 복잡도 | **양쪽 모두** | fo-dicom 헤더 파싱은 SW만으로 가능, 즉시 착수 |

---

## 8. 수정 권고사항

### 즉시 실행 (0.5M 이내)

1. **C-1**: App.xaml.cs Null Stub 6개 -> 실제 Repository DI 교체 (Coordinator 담당)
2. **C-2/C-3**: Imaging DICOM 헤더 파싱 + W/L/Zoom 기본 구현 (HW 불필요, SW만으로 가능)
3. **H-5**: PPT 디자인 미적용 View 6개 순차 리디자인 (Design 팀)

### 1M 이내

4. **H-1/H-2**: PHI AES-256-GCM + TLS 1.3 완성 (M1 전 필수)
5. **H-3/H-4**: fo-dicom Print SCU + RDSR
6. **H-7**: STRIDE 구현 (M1 게이트 항목)

### HW 의존 (외부 차단)

7. **C-4**: Generator RS-232 -- 프로토콜 문서/장비 확보 후 착수
8. **C-5**: FPD SDK -- 벤더 SDK 확보 후 착수

---

## 9. 마일스톤 전망 업데이트

| MS | 목표 시기 | 현재 전망 | 잔여 핵심 차단 |
|----|----------|----------|---------------|
| **M1 설계 완료** | 2026-05-15 | **ON TRACK** | STRIDE 완성 (0.3 MM) |
| **M2 Tier 1 구현** | 2026-08-31 | **AT RISK** | Generator RS-232 (HW), TLS 1.3, STRIDE 구현 |
| **M3 Tier 2 구현** | 2026-10-31 | **AT RISK** | Imaging 실 구현, FPD SDK, UI 리디자인 잔여 |
| **M4 통합 테스트** | 2026-12-15 | **WATCH** | M2/M3 완료에 종속 |
| **M5 시스템 테스트** | 2027-01-15 | **WATCH** | 외부 침투 테스트 일정 확보 필요 |
| **M6 릴리스** | 2027-03-01 | **WATCH** | 전체 파이프라인 정상 진행 시 달성 가능 |

---

## 10. 요약 대시보드

```
현재: 2026-04-10 (1.5M차)  소진: ~3.0 MM / 24~36 MM

경과:  ███░░░░░░░░░░░░░░░░░ 1.5M / 12M (12.5%)
MM:    ███░░░░░░░░░░░░░░░░░ 3.0 / 24~36 MM (12.5%)

계획 대비 실제:
설계/문서:  ██████████████████░░ 92%  [DONE -- M1 전 STRIDE만 잔여]
Tier1 구현: ██████████░░░░░░░░░░ 52%  [서비스 완료, HW/네트워크 미완]
Tier2 구현: ██████████░░░░░░░░░░ 50%  [서비스+Imaging 완료, UI 부분]
검증:       █████░░░░░░░░░░░░░░░ 25%  [1,258 테스트, E2E 미착수]
인허가:     ████░░░░░░░░░░░░░░░░ 20%  [SBOM/SOUP/CMP 선행 완료]

CRITICAL 차단: 3건 (5건 중 2건 해소)
  C-1 DI Stub 교체 (SW 즉시 가능) -- 가장 긴급
  C-2 해소: Imaging 13메서드 구현 완료 (88.7%)
  C-3 해소: W/L/Zoom/Pan/Rotate/Flip/CLAHE 구현 완료
  C-4 Generator RS-232 (HW 의존)
  C-5 FPD SDK (벤더 의존)

생산성: 3.0 MM으로 ~48% 작업 완료 (계획 대비 3.8배)
HW 의존 차단: ~1.0 MM (Generator + FPD) -- 프로젝트 최대 리스크
```

---

*이 문서는 WBS-001 v2.0의 계획 MM과 마일스톤을 변경하지 않으며, 2026-04-10 시점의 실제 진행 현황을 분석합니다.*
