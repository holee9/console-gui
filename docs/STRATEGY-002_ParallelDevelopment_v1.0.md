# STRATEGY-002: 개발 작업 분류 — 단독 진행 vs 병렬 개발

**버전**: 1.0.0  
**작성일**: 2026-04-05  
**분류**: 내부 개발 전략  
**작성**: H&abyz SW개발팀

---

## 1. 분석 목적

Console-GUI 프로젝트의 남은 구현 작업을 두 가지 실행 모드로 분류한다:

- **단독 진행**: 공유 파일에 대한 쓰기 충돌 위험, 강한 순차 의존성, 또는 크로스커팅 통합 작업
- **병렬 개발 (Worktree Team)**: 안정된 인터페이스 경계를 기준으로 독립 모듈이 동시 진행 가능한 작업

Claude Code에서는 `Agent(isolation: "worktree")`를 사용해 각 Worktree가 독립 git 브랜치를 가지며 파일 충돌 없이 병렬 실행된다.

---

## 2. 현재 코드베이스 상태 (Phase 1d 완료 기준)

### 2.1 완료된 레이어

| 모듈 | 상태 | 비고 |
|------|------|------|
| HnVue.Common | ✅ 완료 | 17개 인터페이스, 전체 모델 레이어 — 변경 불필요 |
| HnVue.Data | ✅ 완료 | EF (Entity Framework) Core DbContext, 6개 DbSet (Patient, Study, Image, DoseRecord, User, AuditLog) |
| HnVue.Security | ✅ 완료 | JWT (JSON Web Token), RBAC (Role-Based Access Control), PasswordHasher, SecurityContext — 완전 구현 |
| HnVue.Workflow | ✅ 완료 | WorkflowEngine, WorkflowStateMachine, GeneratorSimulator (시뮬레이터) |
| HnVue.PatientManagement | ✅ 완료 | PatientService, WorklistService — 단, IWorklistRepository는 Null 스텁 |
| HnVue.Dose | ✅ 완료 | DoseService — 단, IDoseRepository는 Null 스텁 |
| HnVue.Incident | ✅ 완료 | IncidentResponseService — 단, IIncidentRepository는 Null 스텁 |
| HnVue.SystemAdmin | ✅ 완료 | SystemAdminService — 단, ISystemSettingsRepository는 Null 스텁 |
| HnVue.Update | ✅ 완료 | SWUpdateService, BackupService, CodeSignVerifier — 단, IUpdateRepository는 Null 스텁 |
| HnVue.CDBurning | ✅ 완료 | CDDVDBurnService, IMAPIComWrapper — 단, IStudyRepository는 Null 스텁 |
| HnVue.Dicom | ✅ 완료 | DicomFileIO (fo-dicom), DicomStoreScu (DICOM C-STORE SCU), DicomFindScu (DICOM C-FIND SCU) — 실제 구현 |
| HnVue.App | ✅ 완료 | DI (Dependency Injection) 컴포지션 루트 (13개 모듈 등록) — Wave B에서 스텁 교체 예정 |

### 2.2 미완성 모듈

| 모듈 | 상태 | 핵심 문제점 |
|------|------|------------|
| HnVue.Imaging | ⚠️ 스텁 | `Math.Sqrt(pixelCount)`로 크기 추정, DICOM (Digital Imaging and Communications in Medicine) 헤더 파싱 없음, Pan/Zoom 미구현 |
| HnVue.UI | ⚠️ 부분 구현 | LoginView만 완성. PatientListView, WorkflowView, ImageViewerView, DoseMonitorView 부재 |
| MainWindow.xaml | ⚠️ 껍데기 | TextBlock 플레이스홀더만 있음, 실제 View 없음 |

### 2.3 Null 스텁 → 실제 EF 구현 필요한 Repository

| 인터페이스 | 현재 위치 | 필요 DbSet | 비고 |
|-----------|----------|------------|------|
| IDoseRepository | App.xaml.cs 내 NullDoseRepository | DoseRecords (이미 존재) | DbContext 변경 불필요 |
| HnVue.CDBurning.IStudyRepository | App.xaml.cs 내 NullCdStudyRepository | Studies/Images (이미 존재) | DbContext 변경 불필요 |
| IWorklistRepository | App.xaml.cs 내 NullWorklistRepository | 없음 — DICOM MWL (Modality Worklist) 기반 | DB 불필요, DicomFindScu (DICOM C-FIND SCU) 연동 |
| IIncidentRepository | App.xaml.cs 내 NullIncidentRepository | IncidentEntity (신규) | DbContext에 DbSet 추가 필요 |
| ISystemSettingsRepository | App.xaml.cs 내 NullSystemSettingsRepository | SettingsEntity (신규) 또는 파일 기반 | DbContext 변경 또는 파일 스토리지 |
| IUpdateRepository | App.xaml.cs 내 NullUpdateRepository | 없음 — 파일 기반 패키지 | DB 불필요, 파일시스템 구현 |

---

## 3. 의존성 그래프

```
Layer 0: HnVue.Common [STABLE - 17 interfaces]
         │
Layer 1: HnVue.Data [EF Core완료, 스텁 Repo 교체 대기]
         │
Layer 2: HnVue.Security [COMPLETE]
         │
Layer 3: ┌──────────────────────────────────────────────────────┐
         │ HnVue.Imaging    HnVue.Workflow  HnVue.Dicom         │
         │ [STUB]           [완료+Simulator] [완료-real fo-dicom] │
         │ HnVue.Dose       HnVue.PatientMgmt  HnVue.Incident   │
         │ [서비스완료]       [서비스완료]         [서비스완료]       │
         └──────────────────────────────────────────────────────┘
         │
Layer 4: HnVue.Workflow [WorkflowEngine 완료]
         │
Layer 5: HnVue.UI [LoginView만 완성 → 4개 View 부재]
         │
Layer 6: HnVue.App [DI (Dependency Injection)완료, 스텁 교체 대기]
```

**병렬 개발 가능 조건**: Layer 0 (HnVue.Common) 인터페이스가 안정적이므로, 상위 레이어 모듈들은 인터페이스에 의존하여 독립 개발 가능.

---

## 4. 개발 작업 전체 분류표

| # | 작업 | 모드 | Wave | 담당 WT | 의존 작업 | 이유 |
|---|------|------|------|---------|----------|------|
| T-01 | HnVue.Imaging DICOM (Digital Imaging and Communications in Medicine) 헤더 파싱 | **병렬** | A | WT-1 | 없음 | IImageProcessor 인터페이스 안정, 단일 파일 소유 |
| T-02 | HnVue.Imaging Pan/Zoom 픽셀 처리 | **병렬** | A | WT-1 | T-01 내 | 동일 WT-1 내 순차 |
| T-03 | HnVue.Imaging Window/Level VOI LUT | **병렬** | A | WT-1 | T-01 내 | 동일 WT-1 내 순차 |
| T-04 | PatientListView.xaml + ViewModel | **병렬** | A | WT-2 | 없음 | IPatientService 안정, 신규 파일만 추가 |
| T-05 | WorkflowView.xaml + ViewModel | **병렬** | A | WT-2 | 없음 | IWorkflowEngine 안정, 신규 파일만 추가 |
| T-06 | DoseMonitorView.xaml + ViewModel | **병렬** | A | WT-2 | 없음 | IDoseService 안정, 신규 파일만 추가 |
| T-07 | ImageViewerView.xaml + ViewModel | **병렬** | A | WT-2 | 없음 | IImageProcessor 인터페이스로 개발, 실제 렌더링은 Wave B 통합시 |
| T-08 | DoseRepository EF (Entity Framework) 구현 | **병렬** | A | WT-3 | 없음 | DoseRecords DbSet 이미 존재, DbContext 변경 불필요 |
| T-09 | WorklistRepository DICOM MWL (Modality Worklist) 구현 | **병렬** | A | WT-3 | 없음 | DicomFindScu (DICOM C-FIND SCU) 연동, DB 불필요 |
| T-10 | IncidentRepository EF (Entity Framework) 구현 | **병렬** | A | WT-3 | 없음 | 단, IncidentEntity + DbSet 추가 필요 (WT-3 소유) |
| T-11 | SystemSettingsRepository 구현 | **병렬** | A | WT-3 | 없음 | 파일 기반 JSON (JavaScript Object Notation) 또는 SQLite, WT-3 소유 |
| T-12 | UpdateRepository 파일시스템 구현 | **병렬** | A | WT-3 | 없음 | 패키지 파일 스캔, DB 불필요 |
| T-13 | CDBurning.StudyRepository EF (Entity Framework) 구현 | **병렬** | A | WT-3 | 없음 | 기존 DbSet 활용 |
| T-14 | GeneratorSerialPort RS-232 구현 | **병렬** | A | WT-4 | 없음 | IGeneratorInterface 안정, 신규 파일, API_MANUAL 참조 |
| T-15 | MainWindow.xaml 패널 통합 | **단독** | B | 없음 | T-04~T-07 완료 | 4개 View 모두 필요, 공유 파일 |
| T-16 | MainViewModel 확장 (패널 내비게이션) | **단독** | B | 없음 | T-04~T-07 완료 | 전체 상태 조율, 크로스커팅 |
| T-17 | App.xaml.cs Null 스텁 교체 | **단독** | B | 없음 | T-08~T-13 완료 | DI (Dependency Injection) 컴포지션 루트, 단일 파일 |
| T-18 | GeneratorSimulator → SerialPort 교체 | **단독** | B | 없음 | T-14 완료 | App.xaml.cs 수정 |
| T-19 | 단위 테스트 (Imaging, UI (User Interface) ViewModels) | **병렬** | B | 선택적 WT | T-01~T-07 | 인터페이스 기반 모킹 가능 |
| T-20 | 통합 테스트 (End-to-End 워크플로) | **단독** | C | 없음 | T-15~T-18 완료 | 전체 파이프라인 검증 |
| T-21 | IEC (International Electrotechnical Commission) 62304 소프트웨어 검증 테스트 | **단독** | C | 없음 | T-20 완료 | 규제 필수, 단독 실행 |

---

## 5. Wave 구조

### Wave A — 병렬 실행 (4개 Worktree 동시)

```
┌──────────────────────────────────────────────────────────────────┐
│                        Wave A (병렬)                              │
│                                                                  │
│  WT-1: Imaging        WT-2: UI Views      WT-3: EF Repos         │
│  ──────────────       ──────────────      ──────────────         │
│  T-01 DICOM파싱       T-04 PatientList    T-08 DoseRepo          │
│  T-02 Pan/Zoom        T-05 Workflow       T-09 MWL Worklist      │
│  T-03 VOI LUT         T-06 DoseMonitor    T-10 IncidentRepo      │
│                       T-07 ImageViewer    T-11 SettingsRepo      │
│                                           T-12 UpdateRepo        │
│  WT-4: Generator                          T-13 CDBurnRepo        │
│  ──────────────                                                  │
│  T-14 RS-232 실 구현                                              │
└──────────────────────────────────────────────────────────────────┘
           ↓ 모든 WT 완료 후
```

### Wave B — 단독 순차 실행

```
┌──────────────────────────────────────────────────────────────────┐
│                        Wave B (단독)                              │
│                                                                  │
│  Step 1: T-15 MainWindow.xaml 패널 통합                           │
│          (4개 View UserControl 연결, TextBlock 플레이스홀더 교체)    │
│                                                                  │
│  Step 2: T-16 MainViewModel 확장                                  │
│          (환자선택 → 워크플로 → 영상뷰어 상태 연결)                   │
│                                                                  │
│  Step 3: T-17 + T-18 App.xaml.cs 스텁 교체                       │
│          (NullDoseRepository → DoseRepository 등)                │
└──────────────────────────────────────────────────────────────────┘
           ↓
```

### Wave C — 통합 및 검증 (단독)

```
┌──────────────────────────────────────────────────────────────────┐
│                        Wave C (단독)                              │
│                                                                  │
│  T-20 End-to-End 통합 테스트                                      │
│  T-21 IEC 62304 소프트웨어 검증 테스트                              │
└──────────────────────────────────────────────────────────────────┘
```

---

## 6. 파일 소유권 매트릭스

병렬 개발 시 각 Worktree가 소유하는 파일 — 동일 파일을 두 WT가 쓰지 않도록 보장.

### WT-1: HnVue.Imaging

| 파일 | 작업 |
|------|------|
| `src/HnVue.Imaging/ImageProcessor.cs` | 전체 재구현 |

### WT-2: HnVue.UI (Views)

| 파일 | 작업 |
|------|------|
| `src/HnVue.UI/Views/PatientListView.xaml` | 신규 생성 |
| `src/HnVue.UI/Views/PatientListView.xaml.cs` | 신규 생성 |
| `src/HnVue.UI/ViewModels/PatientListViewModel.cs` | 신규 생성 |
| `src/HnVue.UI/Views/WorkflowView.xaml` | 신규 생성 |
| `src/HnVue.UI/Views/WorkflowView.xaml.cs` | 신규 생성 |
| `src/HnVue.UI/ViewModels/WorkflowViewModel.cs` | 신규 생성 |
| `src/HnVue.UI/Views/DoseMonitorView.xaml` | 신규 생성 |
| `src/HnVue.UI/Views/DoseMonitorView.xaml.cs` | 신규 생성 |
| `src/HnVue.UI/ViewModels/DoseViewModel.cs` | 신규 생성 |
| `src/HnVue.UI/Views/ImageViewerView.xaml` | 신규 생성 |
| `src/HnVue.UI/Views/ImageViewerView.xaml.cs` | 신규 생성 |
| `src/HnVue.UI/ViewModels/ImageViewerViewModel.cs` | 신규 생성 |

> **주의**: WT-2는 기존 파일(LoginViewModel.cs, MainViewModel.cs)을 수정하지 않는다. MainViewModel 확장은 Wave B 단독 작업.

### WT-3: Repository 구현

| 파일 | 작업 |
|------|------|
| `src/HnVue.Dose/DoseRepository.cs` | 신규 생성 (DoseRecords DbSet 활용) |
| `src/HnVue.PatientManagement/WorklistRepository.cs` | 신규 생성 (DicomFindScu 연동) |
| `src/HnVue.Incident/IncidentRepository.cs` | 신규 생성 |
| `src/HnVue.Incident/Entities/IncidentEntity.cs` | 신규 생성 |
| `src/HnVue.SystemAdmin/SystemSettingsRepository.cs` | 신규 생성 |
| `src/HnVue.Update/UpdateRepository.cs` | 신규 생성 |
| `src/HnVue.CDBurning/StudyRepository.cs` | 신규 생성 |
| `src/HnVue.Data/HnVueDbContext.cs` | IncidentEntity DbSet 추가 (WT-3 단독 소유) |

> **주의**: HnVueDbContext.cs는 WT-3만 수정. 다른 WT는 이 파일을 건드리지 않는다.

### WT-4: GeneratorSerialPort

| 파일 | 작업 |
|------|------|
| `src/HnVue.Workflow/GeneratorSerialPort.cs` | 신규 생성 (IGeneratorInterface 구현) |
| `src/HnVue.Workflow/GeneratorConfig.cs` | 신규 생성 (포트 설정값) |

> **참고**: `API_MANUAL_241206.pdf` Section 3 (Generator RS-232 Protocol) 참조 필수.

### Wave B 전용 파일 (단독 진행)

| 파일 | Wave B 작업 |
|------|------------|
| `src/HnVue.App/MainWindow.xaml` | 4개 View UserControl 통합 |
| `src/HnVue.UI/ViewModels/MainViewModel.cs` | 패널 내비게이션 상태 추가 |
| `src/HnVue.App/App.xaml.cs` | Null 스텁 → 실 구현체 교체 |

---

## 7. Claude Code 실행 가이드

### Wave A 병렬 실행 (4 에이전트 동시 Launch)

```
사용자 명령 예시:
"Wave A를 시작해줘. WT-1~WT-4를 병렬로 실행해"

MoAI Agent() 호출 구조:
- Agent(subagent_type="expert-frontend", isolation="worktree") → WT-2 (UI Views)
- Agent(subagent_type="expert-backend", isolation="worktree") → WT-1 (Imaging)
- Agent(subagent_type="expert-backend", isolation="worktree") → WT-3 (EF Repos)
- Agent(subagent_type="expert-backend", isolation="worktree") → WT-4 (GeneratorSerialPort)
```

**worktree isolation 필수 이유**: 4개 에이전트가 동일 프로젝트 디렉토리에서 파일을 동시에 쓰면 충돌 발생. `isolation: "worktree"`가 각 에이전트에게 독립 git worktree를 생성한다.

**프롬프트 경로 규칙**: Agent 프롬프트에서 파일 경로는 반드시 프로젝트 루트 상대경로 사용.
```
# 올바름 (상대 경로)
"src/HnVue.Imaging/ImageProcessor.cs 를 수정해서..."

# 잘못됨 (절대 경로 — worktree 격리를 우회함)
"D:/workspace-gitea/Console-GUI/src/HnVue.Imaging/ImageProcessor.cs 를..."
```

### Wave B 단독 실행

Wave A의 4개 Worktree 브랜치를 main에 순서대로 머지한 후, 단독으로:
```
1. WT-3 머지 먼저 (DbContext 변경 포함)
2. WT-1 머지 (Imaging — 충돌 없음)
3. WT-2 머지 (UI Views — 신규 파일만, 충돌 없음)
4. WT-4 머지 (Generator — 신규 파일, 충돌 없음)
5. Wave B 작업 (MainWindow.xaml, MainViewModel, App.xaml.cs) 단독 진행
```

---

## 8. 머지 충돌 위험 분석

| 파일 | Wave A 접근 WT | 충돌 위험 | 해결 방안 |
|------|--------------|---------|---------|
| `HnVue.Data/HnVueDbContext.cs` | WT-3만 | **없음** | WT-3 단독 소유 |
| `HnVue.UI (User Interface)/ViewModels/MainViewModel.cs` | **없음** | 없음 | Wave B 단독 수정 |
| `HnVue.App/App.xaml.cs` | **없음** | 없음 | Wave B 단독 수정 |
| `HnVue.App/MainWindow.xaml` | **없음** | 없음 | Wave B 단독 수정 |
| WT-2 신규 파일들 | WT-2만 | 없음 | 신규 파일 — git add만 |
| WT-1 ImageProcessor.cs | WT-1만 | 없음 | WT-1 단독 소유 |

**결론**: 파일 소유권 매트릭스를 준수하면 Wave A 머지 시 충돌 없음.

---

## 9. 진행 단계별 체크리스트

### Wave A 시작 전

- [ ] HnVue.Common 인터페이스 확정 (변경 없음 원칙)
- [ ] API_MANUAL_241206.pdf Generator RS-232 프로토콜 섹션 확인 (WT-4용)
- [ ] IEC 62304 SWR 추적성 확인 (각 WT 구현이 어떤 SWR을 만족하는지)

### Wave A 완료 기준

- [ ] WT-1: `ImageProcessor` 단위 테스트 통과, 실제 DICOM (Digital Imaging and Communications in Medicine) 파일로 픽셀 데이터 정상 추출
- [ ] WT-2: 4개 View가 DI (Dependency Injection) 컨테이너에서 인스턴스화 가능, 기본 데이터 바인딩 동작
- [ ] WT-3: 모든 Null 스텁 Repository 실제 구현체로 교체 완료, 마이그레이션 적용
- [ ] WT-4: `GeneratorSerialPort` 시뮬레이터와 동일한 상태 전환 동작

### Wave B 완료 기준

- [ ] MainWindow.xaml 3패널 레이아웃에 실제 View 연결
- [ ] 로그인 후 PatientList → WorkflowView 내비게이션 동작
- [ ] ImageViewerView에 ProcessedImage 표시 (비록 단순한 gray scale이라도)
- [ ] 빌드 에러 0, dotnet build 성공

### Wave C 완료 기준 (릴리즈 전)

- [ ] 전체 End-to-End 시나리오 수동 테스트 통과
- [ ] IEC (International Electrotechnical Commission) 62304 §5.7 소프트웨어 통합 테스트 레코드 작성
- [ ] 커버리지 85% 이상 (단위 + 통합 통합)

---

## 10. 2인 팀 현실적 적용 방안

팀 규모가 2명(SW개발자)인 점을 감안할 때:

### 옵션 A: Claude Code Worktree Team 완전 활용
- Wave A의 4개 WT를 Claude Code Agent로 동시 실행
- 2명 개발자는 각 WT 코드 리뷰 및 머지에 집중
- **장점**: 최대 병렬화, Wave A 완료 속도 극대화
- **주의**: 각 에이전트 프롬프트에 충분한 컨텍스트 제공 필요

### 옵션 B: 인간 + AI 역할 분리
- 개발자 1: WT-1 (Imaging) + WT-4 (Generator) — 하드웨어 도메인 지식 필요
- 개발자 2: WT-2 (UI Views) — WPF MVVM 집중
- Claude Code Agent: WT-3 (EF Repos) — 반복적이고 패턴화된 작업
- **장점**: 도메인 전문성 활용, 코드 품질 직접 통제

### 권장: 옵션 A (단, 개발자 상시 리뷰)
Wave A는 모두 인터페이스 기반이라 에이전트가 정확하게 구현할 수 있는 범위. Wave B와 C는 크로스커팅 통합 판단이 필요하므로 개발자 직접 진행.

---

## 11. 위험 요소 및 완화 방안

| 위험 | 확률 | 영향 | 완화 방안 |
|------|------|------|---------|
| WT-3이 HnVueDbContext.cs를 잘못 수정 | 낮음 | 중간 | PR 리뷰 필수, IncidentEntity DbSet만 추가 |
| WT-2 ViewModels이 안정 인터페이스 아닌 구현체에 직접 의존 | 중간 | 중간 | 에이전트 프롬프트에 "인터페이스만 사용" 명시 |
| HnVue.Imaging이 실제 H&abyz FPD 독점 포맷 미지원 | 중간 | 높음 | API_MANUAL에서 포맷 스펙 확인 필수, Phase 1.5 갭 분석 우선 |
| Wave A 완료 후 MainViewModel 통합 작업 과도하게 복잡 | 낮음 | 낮음 | WT-2 설계 시 MainViewModel 확장 포인트 최소화 |
| IEC 62304 추적성 누락 | 중간 | 높음 | 각 구현 파일에 SWR 태그 주석 유지 |

---

---

## 12. Phase 1.5 선행 작업 (Wave A 전)

Wave A 코드 구현 전에 처리할 작업들. Wave A와 의존성 없이 병렬 진행 가능.

| # | 작업 | 유형 | 비고 |
|---|------|------|------|
| P1 | ANALYSIS-002 중복 섹션 정리 | 단독 | §5/§7/§8/§9 중복 → 참조 링크로 교체 |
| P2 | STRATEGY-002 Phase 1.5 섹션 추가 | 단독 | 이 작업 |
| P3 | Pre-flight 빌드 에러 수정 | 단독 | CS1574, CS1591 (XML 주석) |
| P4 | Gap 분석 (SRS 기반) | 단독 | IFU .docx는 바이너리 — 사람이 수행 필요. SRS 기반 부분 분석만 가능 |
| P5 | 시험 보고서 현실화 메모 | 단독 | DOC-022~028 재작성 작업 목록 정리 |

---

## 13. 자율 실행 아키텍처 (Claude Code Agent 기반)

완전 자율 실행 시 파이프라인:

```
Pre-flight fixes (solo, direct)
    │  CS1574, CS1591 수정 → dotnet build = 0 errors
    ↓
Phase 1.5 docs (solo, direct)
    │  ANALYSIS-002 정리, STRATEGY-002 업데이트
    ↓
Wave A (4 agents parallel, isolation: worktree)
    ├── WT-1: Imaging DICOM 구현  ──────────────┐
    ├── WT-2: UI 4개 View 구현    ──────────────┤ 모두 완료될 때까지 대기
    ├── WT-3: EF Repos 구현       ──────────────┤
    └── WT-4: GeneratorSerialPort ──────────────┘
    ↓
Wave A Merge (solo)
    │  순서: WT-3 first (DbContext) → WT-1 → WT-4 → WT-2
    ↓
Wave B (solo, sequential)
    │  MainWindow.xaml → MainViewModel → App.xaml.cs DI swap
    ↓
Wave C: Build + Test loop (solo)
    │  "/c/Program Files/dotnet/dotnet.exe" test --no-restore
    │  실패 시 자동 수정 → 재실행
    ↓
Review/Quality loop (evaluator-active, max 5 iterations)
    │  품질 점수 ≥ 0.75 달성 시 종료
    ↓
Final push + 완료 보고
```

### 자율 실행 하드 한계 (구현 불가 — 인터페이스 stub 처리)

| 한계 | 이유 | 처리 방식 |
|------|------|---------|
| FPD SDK 라이브 획득 | 하드웨어 SDK DLL 없음 | `IFpdAcquisitionService` stub + TODO 주석 |
| IFU .docx Gap 분석 | 바이너리 파일 읽기 불가 | 사람이 수행. SRS 기반 부분 분석으로 대체 |
| UI .pptx 디자인 스펙 | 바이너리 파일 읽기 불가 | SRS + HnVueTheme.xaml + 3패널 레이아웃 기반 구현 |
| 하드웨어 통합 테스트 | 물리 장비 없음 | GeneratorSimulator 기반 단위 테스트로 대체 |
| 시각적 UI 품질 | 앱 실행 불가 | 컴파일 타임 검증 + 코드 리뷰로 대체 |

---

버전: 1.1.0 (Phase 1.5 + 자율 실행 아키텍처 추가)
다음 검토: Wave A 완료 후 (실제 구현 결과에 따라 업데이트)
