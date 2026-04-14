# HnVue — 의료 영상 솔루션

의료 방사선 영상 획득/처리/관리 WPF (Windows Presentation Foundation) 데스크톱 애플리케이션

| 상태 | 값 |
|------|-----|
| **빌드** | `dotnet build HnVue.sln -c Release` 기준 0 errors |
| **테스트** | 2,539+ 통과 / 0 실패 / Flaky 0건, Safety-Critical 90%+ |
| **커버리지** | 전 모듈 85%+ 달성, 4 Safety-Critical 전원 90%+ |
| **아키텍처** | 11/11 규정 준수 (NetArchTest) |
| **인허가 분류** | IEC 62304 Class B |
| **Sprint** | S09 R1 완료 (2026-04-14) |

---

## 개발 진도 현황 (2026-04-14 기준)

> 상세: [개발 현황 상세](docs/management/development-status.md) | [정밀 분석 보고서](docs/management/PROGRESS-002_DetailedAnalysis_v1.0.md) | [WBS v3.0](docs/management/WBS-001_WBS_v3.0.md) | [WBS v2.0 (아카이브)](docs/archive/WBS-001_WBS_v2.0.md)

```
Sprint: S09 / S24 (37.5%)     누적 AS: ~55 Agent Sessions
기능:   ████████████████░░░░ 68%     병목: 인간 리뷰 + HW/벤더 조달
```

### Sprint S09 R1 완료 현황 (전팀 MERGED)

| 팀 | 작업 | 상태 |
|----|------|------|
| Coordinator | Detector DI 조건부 등록 통합테스트 (#93) | MERGED |
| Design | 하드코딩 색상 토큰 교체 + EmergencyStop (#98, #103) | MERGED |
| RA | SBOM/SOUP 이미 등록 확인 | MERGED |
| Team A | IDLE CONFIRM | MERGED |
| Team B | IDLE CONFIRM | MERGED |
| QA | 품질게이트 대기 (전팀 MERGED 후 검증 예정) | IDLE |

### S08-R2 품질게이트 결과 (PASS)

| 게이트 | 기준 | 결과 | 판정 |
|--------|------|------|------|
| 빌드 에러 | 0 | **0** | PASS |
| 테스트 | 0 실패 | **2,539P / 0F** / Flaky 0건 | PASS |
| Safety-Critical 커버리지 | 90%+ | Dose, Incident, Update, Security 전원 90%+ | PASS |
| 전체 커버리지 | 85%+ | **전 모듈 85%+ 달성** | PASS |
| Architecture Tests | 전원 통과 | **11/11** | PASS |

### Sprint S07~S09 주요 성과

- **StudylistView XAML 구현** (PPT slides 5-7): Coordinator ViewModel + Design XAML 협업
- **role-matrix.md 도입** (CONSTITUTIONAL): 7팀 역할 경계 최상위 규약, CC 자가점검 4문
- **디렉토리 단위 소유권 명확화**: DesignTime/ Design 단독 소유, 아키텍처 테스트로 검증
- **Flaky test 0건 달성**: S07-R1 일부 → S07-R5 완전 해결
- **RTM v2.4 동기화**: SWR→TC 매핑 업데이트
- **하드코딩 색상 토큰 교체**: EmergencyStop + 전체 UI 테마 일관성 확보
- **Detector DI 조건부 등록**: 통합테스트로 검증

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
| ~~App.xaml.cs Null Stub 6개~~ -> 실 Repo DI 교체 | **완료** (S05-R1) | -- |
| ~~Flaky test~~ | **완료** (S07-R5) | -- |
| ~~Safety-Critical 커버리지 미달~~ | **완료** (S07-R5) | -- |
| Generator RS-232 실 구현 | 0.5 | HW 의존 |
| FPD Detector SDK 통합 | 0.5 | 벤더 의존 |
| PHI AES-256-GCM + TLS 1.3 | 0.35 | SW 가능 |

### 모듈 요약 (17개)

| 구분 | 모듈 | 상태 |
|------|------|------|
| **완료 (14)** | Common, Data, Security, Workflow, Dose, Incident, Update, PatientMgmt, SystemAdmin, CDBurning, Dicom, Imaging, UI.Contracts, App(DI 완료) | 서비스+테스트 완료 |
| **Stub (1)** | Detector | 시뮬레이터 완료, 벤더 SDK 대기 (91.7% 커버리지) |
| **부분 (2)** | UI (PPT 5/9 화면 구현), UI.ViewModels (구현 진행중) | 리디자인 진행중 |

---

## 개발운영 가이드

> **프로젝트 핵심 철학: "속도보다 품질. 빠른 완료보다 올바른 완성."**

| 문서 | 설명 |
|------|------|
| [개발운영 지침서 v2.0](docs/development/DEV-OPS-GUIDELINES.md) | 품질우선 철학, CC 역할경계, 6팀 워크트리 운영, DISPATCH 사이클, Git 절차 |
| [운영 프로토콜 v2.0](.moai/dispatches/templates/operations-protocol.md) | CC-팀 역할분담, 자기검증 체크리스트, 위반 대응, DISPATCH 템플릿 |
| [역할 매트릭스 v2.0](.claude/rules/teams/role-matrix.md) | 7팀 역할 경계 최상위 규약 (CONSTITUTIONAL), CC 자가점검 4문 |
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
| **프로젝트 팀** | 인간 7명 (리뷰/의사결정) + AI 에이전트 6팀 (구현 실행) |
| **실행 모델** | AI 에이전트가 구현, 인간이 리뷰/의사결정 |

이 레포지토리는 H&abyz가 현재 판매 중인 HnVue 제품의 Console SW를 **자사 기술로 내재화하는 Greenfield 개발 프로젝트**입니다. 자세한 내용은 [ANALYSIS-002 -- 내재화 개발 컨텍스트](docs/ANALYSIS-002_InternalizationContext_v1.0.md)를 참조하세요.

### 팀 구성: 인간 7명 (리뷰어) + AI 에이전트 6팀 (구현자)

| 인간 역할 | 인원 | AI 에이전트 | 인간이 하는 일 | AI가 하는 일 |
|-----------|------|-----------|--------------|------------|
| **CC (총괄)** | 1명 | MoAI | Sprint 계획 승인 | DISPATCH 작성, 진도 추적 |
| **SW 팀장** | 1명 | hnvue-coordinator + hnvue-infra | 아키텍처 결정, PR 리뷰/머지 | DI 통합, ViewModel, 인프라 구현 |
| **개발자 1** | 1명 | hnvue-medical | 의료 도메인 검증, Safety 리뷰 | 8개 의료모듈 구현 |
| **개발자 2** | 1명 | hnvue-infra (보조) | Team A 산출물 리뷰 | 인프라 구현 보조 |
| **디자이너** | 1명 | hnvue-ui | PPT 디자인 제공, XAML 검수 | PPT->XAML 코드화 |
| **QA** | 1명 | hnvue-qa | 테스트 전략 승인, 릴리스 판정 | CI/CD, 커버리지, 자동화 |
| **RA** | 1명 | hnvue-ra | 규제 문서 승인, 인허가 제출 | RTM/SBOM/CMP 생성 |

**운영 원칙:**
- **AI 에이전트가 구현, 인간이 리뷰/의사결정** (AI-first 실행 모델)
- SW 팀장: 아키텍처 결정 + PR 리뷰/머지 승인 (코딩은 AI 에이전트)
- 디자이너: PPT 디자인 제공 + XAML 결과물 검수 (코드화는 AI 에이전트)
- 병목: 인간 리뷰 속도 + 외부 의존성 (HW/벤더), 코딩 속도가 아님
- Commander Center: 계획/지시/검증만 수행, 직접 코드 구현 금지 ([운영 지침서](docs/development/DEV-OPS-GUIDELINES.md))

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

**17개 모듈**, GUI 교체 가능 아키텍처 (UI.Contracts 인터페이스 기반 분리)

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

**프로젝트 상태:** S09-R1 전팀 MERGED (2,539개 테스트, 0 실패, Safety-Critical 전원 90%+, 아키텍처 11/11)
**현재 단계:** Phase 2 -- UI 리디자인 + 기능 고도화
**현실적 릴리즈:** 2027년 Q2~Q3 ([ANALYSIS-002](docs/ANALYSIS-002_InternalizationContext_v1.0.md) 참조)

문서 최종 업데이트: 2026-04-14
