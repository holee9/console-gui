# HnVue — 의료 영상 솔루션

의료 방사선 영상 획득/처리/관리 WPF (Windows Presentation Foundation) 데스크톱 애플리케이션

| 상태 | 값 |
|------|-----|
| **빌드** | `dotnet build HnVue.sln -c Release` 기준 0 errors |
| **테스트** | 2,043개 통과 / 14 실패 (1 flaky + 13 디자인준수), Safety-Critical 90%+ |
| **커버리지** | 14/16 모듈 PASS (Views/Migrations 제외 정책), 4 Safety-Critical 전원 PASS |
| **품질 점수** | 0.91/1.0 |
| **인허가 분류** | IEC 62304 Class B |
| **Gitea 이슈** | 58개 등록, 58개 해결 (100%) -- 2026-04-07 기준 |

---

## 개발 진도 현황 (2026-04-11 기준)

> 상세: [개발 현황 상세](docs/management/development-status.md) | [정밀 분석 보고서](docs/management/PROGRESS-002_DetailedAnalysis_v1.0.md) | [WBS v3.0](docs/management/WBS-001_WBS_v3.0.md) | [WBS v2.0 (아카이브)](docs/archive/WBS-001_WBS_v2.0.md)

```
경과:  ██░░░░░░░░░░░░░░░░░░ 1.5M / 12M (12.5%)     소진: 3.2 / 24~36 MM
기능:  ██████████░░░░░░░░░░ 48%                       효율: 3.8배 (AI 병행)
```

### Sprint S04 R1 진행 현황 (CONDITIONAL PASS)

| 게이트 | 기준 | 결과 | 판정 |
|--------|------|------|------|
| 빌드 에러 | 0 | **0** | PASS |
| 테스트 | 기능 0F | **2,043P / 14F** (1 flaky + 13 디자인준수) | CONDITIONAL |
| Safety-Critical 커버리지 | 90%+ | Dose 100%, Incident 94.4%, Security 92.3% | PASS |
| 전체 커버리지 (제외 후) | 80%+ | **14/16 모듈 PASS** | PASS |
| Architecture Tests | 전원 통과 | **4/4** | PASS |
| Integration Tests | 전원 통과 | **26/26** | PASS |

### 커버리지 FAIL 모듈 (S04 R1 이월)

| 모듈 | 측정치 | Floor | 소유 팀 |
|------|--------|-------|---------|
| Common | 88.8% | 90% | Team A |
| Detector | 80.3% | 85% | Team B |
| Update | 77.7% | 80% | Team B |

### 마일스톤

| MS | 목표 | 목표일 | 전망 | 핵심 차단 |
|----|------|--------|------|----------|
| M1 | 설계 완료 | 2026-05-15 | **ON TRACK** | STRIDE 완성 |
| M2 | Tier 1 구현 | 2026-08-31 | **AT RISK** | Generator RS-232 (HW), TLS 1.3 |
| M3 | Tier 2 구현 | 2026-10-31 | **AT RISK** | FPD SDK (벤더), UI 리디자인 잔여 |
| M4 | 통합 테스트 | 2026-12-15 | WATCH | M2/M3 종속 |
| M5 | 시스템 테스트 | 2027-01-15 | WATCH | 침투 테스트 일정 |
| M6 | 릴리스 | 2027-03-01 | WATCH | 전체 정상 진행 시 |

### CRITICAL 차단

| 항목 | 잔여 MM | SW 해결 가능? |
|------|---------|:------------:|
| App.xaml.cs Null Stub 6개 -> 실 Repo DI 교체 | 0.1 | **즉시 가능** |
| Generator RS-232 실 구현 | 0.5 | HW 의존 |
| FPD Detector SDK 통합 | 0.5 | 벤더 의존 |
| PHI AES-256-GCM + TLS 1.3 | 0.35 | SW 가능 |

### 모듈 요약 (17개)

| 구분 | 모듈 | 상태 |
|------|------|------|
| **완료 (14)** | Common, Data, Security, Workflow, Dose, Incident, Update, PatientMgmt, SystemAdmin, CDBurning, Dicom, Imaging, UI.Contracts, App(구조) | 서비스+테스트 완료 |
| **DI 미교체 (6)** | Dose, Incident, Update, PatientMgmt, SystemAdmin, CDBurning | Repo 구현 완료, App.xaml.cs Null Stub 잔존 |
| **Stub (1)** | Detector | 시뮬레이터 완료, 벤더 SDK 대기 (91.7% 커버리지) |
| **부분 (2)** | UI (PPT 3/9 완료), UI.ViewModels (TODO 9건) | 리디자인 진행중 |

---

## 개발운영 가이드

> **프로젝트 핵심 철학: "속도보다 품질. 빠른 완료보다 올바른 완성."**

| 문서 | 설명 |
|------|------|
| [개발운영 지침서 v2.0](docs/development/DEV-OPS-GUIDELINES.md) | 품질우선 철학, CC 역할경계, 6팀 워크트리 운영, DISPATCH 사이클, Git 절차 |
| [운영 프로토콜 v2.0](.moai/dispatches/templates/operations-protocol.md) | CC-팀 역할분담, 자기검증 체크리스트, 위반 대응, DISPATCH 템플릿 |
| [DISPATCH 스키마](.claude/rules/moai/workflow/dispatch-schema.md) | DISPATCH.md 필수 섹션 및 검증 규칙 |
| [팀 규칙](.claude/rules/teams/) | 팀별 역할, 품질 철학, 소유권, 자기검증 체크리스트 |

---

## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| **제품명** | HnVue Console SW |
| **제조사** | H&abyz |
| **대체 대상** | IMFOU feel-DRCS (FDA K110033) |
| **FDA Predicate** | DRTECH EConsole1 ([FDA K231225](https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf)) |
| **IEC 62304 분류** | Class B |
| **인허가 대상** | MFDS 2등급, FDA 510(k), CE MDR Class IIa |
| **개발 인력** | 6명 (SW 개발 5명 + 총괄 1명) |

이 레포지토리는 H&abyz가 현재 판매 중인 HnVue 제품의 Console SW를 **자사 기술로 내재화하는 Greenfield 개발 프로젝트**입니다. 자세한 내용은 [ANALYSIS-002 -- 내재화 개발 컨텍스트](docs/ANALYSIS-002_InternalizationContext_v1.0.md)를 참조하세요.

### 팀 구성 (6명)

| 역할 | 인원 | 담당 워크트리 팀 | 핵심 업무 |
|------|------|----------------|-----------|
| **Commander Center (총괄)** | 1명 | CC (main) | DISPATCH 발행, 스프린트 계획, PR 관리, 통합 검증 |
| **SW 개발팀장** | 1명 | Coordinator + Team A | UI.Contracts/ViewModels/App DI + Common/Data/Security 인프라 |
| **개발자 1** | 1명 | Team B | 의료 영상 파이프라인 (Dicom, Detector, Dose, Workflow 등 8모듈) |
| **개발자 2** | 1명 | Team Design + Team A 보조 | PPT→XAML 코드화 (기능구현 없음) + 인프라 보조 |
| **QA** | 1명 | QA | CI/CD, 커버리지 분석, 릴리스 준비도 보고, 코드 리뷰 |
| **RA** | 1명 | RA | IEC 62304 문서, SBOM, RTM, FDA 510(k) 제출 준비 |

**운영 원칙:**
- SW 개발팀장은 Coordinator(통합) + Team A(인프라)를 겸임하며, 코드 리뷰 최종 승인자
- 개발자 2는 Design Team으로서 XAML 코드화만 수행, C# 기능구현은 팀장/개발자 1이 분담
- AI 에이전트(MoAI)가 각 워크트리 팀의 실행을 보조하여 인력 효율 3.8배 달성
- Commander Center는 계획/지시/검증만 수행, 직접 코드 구현 금지 ([운영 지침서](docs/development/DEV-OPS-GUIDELINES.md))

### 핵심 기능

- **환자 관리**: 의료진 및 환자 정보 등록/조회/관리
- **촬영 워크플로우**: 환자 선택 -> 프로토콜 -> 촬영 -> 영상 획득 -> PACS 전송
- **DICOM 상호운용성**: C-STORE SCU, C-FIND SCU (Worklist), DICOM 3.0 파일 I/O
- **방사선 선량 관리**: IEC 60601-1-3 준수 (4단계 인터록: ALLOW/WARN/BLOCK/EMERGENCY)
- **보안 및 인증**: JWT 인증, RBAC 4역할, HMAC-SHA256 감사 로그 체인
- **소프트웨어 업데이트**: SHA-256 무결성 검증, 백업/복원, 코드 서명
- **인시던트 대응**: 4단계 심각도 분류, 긴급 콜백
- **미디어 처리**: CD/DVD 영상 배포 (IMAPI2)

---

## 기술 스택

| 항목 | 기술 |
|------|------|
| **UI Framework** | WPF + .NET 8 LTS (MVVM, MahApps.Metro) |
| **데이터베이스** | SQLite + EF Core 8 (SQLCipher AES-256) |
| **DICOM** | fo-dicom 5.2.5 (MIT) |
| **보안** | bcrypt (cost=12), JWT HS256, HMAC-SHA256 |
| **테스트** | xUnit + NSubstitute + FluentAssertions |
| **패키지 관리** | NuGet Central Package Management |

### 규제 표준

IEC 62304, IEC 62366-1, ISO 14971, IEC 81001-5-1, FDA 21 CFR 820.30, FDA Section 524B, ISO 13485, DICOM 3.0 / IHE SWF, MFDS 사이버보안 가이드라인 2024

---

## 아키텍처 개요

```
+----------------------------------------------------------+
| Layer 6: HnVue.App (DI 컴포지션 루트)                       |
+----------------------------------------------------------+
| Layer 5: UI.Contracts + UI.ViewModels + UI                |
+----------------------------------------------------------+
| Layer 4: HnVue.Workflow (상태 머신, 워크플로우 엔진)          |
+----------------------------------------------------------+
| Layer 3.5: HnVue.Detector (FPD 검출기 추상화)               |
+----------------------------------------------------------+
| Layer 3: Dose, Incident, Update, Dicom, Imaging,          |
|          PatientManagement, SystemAdmin, CDBurning         |
+----------------------------------------------------------+
| Layer 2: HnVue.Security (인증, RBAC, 암호화)                |
+----------------------------------------------------------+
| Layer 1: HnVue.Data (EF Core, Repository)                 |
+----------------------------------------------------------+
| Layer 0: HnVue.Common (모델, 인터페이스, Enum)               |
+----------------------------------------------------------+
```

**15개 모듈**, GUI 교체 가능 아키텍처 (UI.Contracts 인터페이스 기반 분리)

---

## 빠른 시작

```bash
# 빌드
dotnet build HnVue.sln -c Debug

# 테스트
dotnet test HnVue.sln --configuration Debug

# Release 빌드
dotnet build HnVue.sln -c Release
```

**시스템 요구사항:** Windows 10/11, .NET 8.0.419 LTS SDK, Visual Studio 2022

---

## 문서 인덱스

### 아키텍처

| 문서 | 설명 |
|------|------|
| [모듈 상세 설명](docs/architecture/modules.md) | 15개 모듈 레이어별 상세 |
| [디자인 시스템 및 UI](docs/architecture/design-system.md) | 색상 토큰, UISPEC, WPF 화면 목록 |
| [DESIGN_TO_XAML_WORKFLOW](docs/architecture/DESIGN_TO_XAML_WORKFLOW.md) | 디자인-코드 변환 워크플로우 |

### 개발

| 문서 | 설명 |
|------|------|
| [코드 품질 표준](docs/development/code-quality.md) | 문서화 체계, TRUST 5, DocFX |
| [보안 설정](docs/development/security.md) | 암호화 표준, RBAC, 배포 체크리스트 |
| [Git 워크플로우](docs/development/git-workflow.md) | 저장소 정보, 브랜치 전략 |
| [FAQ / 문제 해결](docs/development/troubleshooting.md) | 빌드/테스트/DICOM 문제 해결 |

### 관리

| 문서 | 설명 |
|------|------|
| [개발 진행 현황](docs/management/development-status.md) | WBS, 마일스톤, 로드맵 |
| [WBS v3.0](docs/management/WBS-001_WBS_v3.0.md) | Sprint/MM 기반 작업 분해 구조 |
| [Sprint 구현계획](docs/management/SPRINT-001_Implementation_Plan_v1.0.md) | 6팀 Sprint별 구현계획 + 마일스톤 게이트 |
| [문서 체계 인덱스](docs/management/documentation-index.md) | 50+ 규제/기획/검증 문서 전체 목록 |
| [MRD/PRD 교차검증](docs/management/mrd-prd-validation.md) | 36사 딥리서치, 92개 MR |
| [운영 전략 가이드](docs/OPERATIONS.md) | 6팀 Worktree 분리 개발 |

### 배포

| 문서 | 설명 |
|------|------|
| [배포 파일 매니페스트](docs/deployment/deployment-manifest.md) | 필수 파일, NuGet 의존성, 폴더 구조 |

### 변경 이력

| 문서 | 설명 |
|------|------|
| [CHANGELOG](CHANGELOG.md) | 버전별 변경 상세 |

### 분석 보고서

| 문서 | 설명 |
|------|------|
| [ANALYSIS-001](docs/ANALYSIS-001_Phase1_Review_v1.0.md) | Phase 1 현황 분석, 릴리즈 준비도 |
| [ANALYSIS-002](docs/ANALYSIS-002_InternalizationContext_v1.0.md) | 내재화 컨텍스트, Gap 분석 계획 |
| [STRATEGY-002](docs/STRATEGY-002_ParallelDevelopment_v1.0.md) | 병렬 개발 전략, Wave 구조 |

---

## 라이선스

HnVue는 H&abyz의 소유 소프트웨어입니다. 상용 의료기기로 판매되는 제품입니다.

**주요 타사 라이선스:** fo-dicom (MIT), MahApps.Metro (MIT), Serilog (Apache 2.0), FluentAssertions (Apache 2.0), NSubstitute (BSD 2-Clause)

전체 목록: [DOC-019 SBOM](docs/regulatory/DOC-019_SBOM_v1.0.md)

---

**프로젝트 상태:** S04 R1 CONDITIONAL PASS (2,043개 테스트, Safety-Critical 90%+, 14/16 모듈 PASS)
**현재 단계:** Phase 1.5 -- Gap 분석 & DI 통합 + UI 리디자인
**즉시 필요:** App.xaml.cs Null Stub 6개 -> 실 Repository DI 교체
**현실적 릴리즈:** 2027년 Q2~Q3 ([ANALYSIS-002](docs/ANALYSIS-002_InternalizationContext_v1.0.md) 참조)

문서 최종 업데이트: 2026-04-11
