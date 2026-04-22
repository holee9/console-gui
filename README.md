# HnVue — 의료 영상 솔루션

의료 방사선 영상 획득/처리/관리 WPF (Windows Presentation Foundation) 데스크톱 애플리케이션

| 상태 | 값 |
|------|-----|
| **빌드** | `dotnet build HnVue.sln -c Release` 기준 0 errors |
| **테스트** | 2,539+ 통과 / 0 실패 / Flaky 0건 |
| **Safety-Critical** | Dose 100%, Incident 95.24%, Update 96%, Security 89.62% (90% 목표) |
| **아키텍처** | 11/11 규정 준수 (NetArchTest) |
| **인허가 분류** | IEC 62304 Class B |
| **Sprint** | S17-R1 ACTIVE (2026-04-22) |
| **운영 모델** | CC v2 (독립 worktree + PR-only + Gitea 이슈 추적) |

---

## 개발 진도 현황 (2026-04-22 기준)

> 상세: [개발 현황 상세](docs/management/development-status.md) | [WBS v4.0](docs/management/WBS-001_WBS_v3.0.md)

```
Phase 1: S12 / S24 (50.0%)    전체 프로젝트: S12 / S48 (25.0%)
Phase 1 남음: 50.0% (12 Sprint)    전체 남음: 75.0% (36 Sprint)
```

### Sprint S17-R1 진행 현황 — CC v2 자율주행

| Phase | 팀 | 작업 | 상태 |
|-------|-----|------|------|
| Phase 1 | Team A | Security 90%+ 달성 + SPEC-INFRA-002 REFACTOR | ACTIVE |
| Phase 1 | Team B | Incident branch 90%+ + Dicom 향상 | ACTIVE |
| Phase 2 | Coordinator | 6개 Repository 통합 검증 | ACTIVE |
| 독립 | Design | PatientListView 갭 + Studylist 분석 | MERGED |
| Phase 3 | QA | Safety-Critical 4/4 검증 | ACTIVE |
| Phase 4 | RA | 추적성 감사 + 문서 영향 평가 | ACTIVE |
| 상시 | CC | DISPATCH 관리 + PR 생성 + 이슈 추적 | ACTIVE |

### S16-R2 QA 판정 (CONDITIONAL PASS)

| 게이트 | 기준 | 결과 | 판정 |
|--------|------|------|------|
| 빌드 에러 | 0 | **0** | PASS |
| 테스트 | 0 실패 | **2,539P / 13F** | CONDITIONAL |
| Safety-Critical | 90%+ | Dose 100%, Incident 95.24%, Update 96%, Security 89.62% | 3/4 PASS |
| Architecture Tests | 전원 통과 | **11/11** | PASS |

> Security 89.62% < 90% 미달이 S17-R1에서 보강 목표

### Sprint S07~S17 주요 성과

- **S17-R1 CC v2 도입**: 독립 worktree + PR-only + Gitea 이슈 추적, 코드/빌드/테스트 CONSTITUTIONAL PROHIBITION
- **S16-R2 실질 개발 재시작**: S14-R2 이후 첫 실질 제품 커밋, Security 보강 진행
- **S14-R2 QA CONDITIONAL PASS**: 87개 Trait 추가, 구버전 base 동기화 이슈 해결
- **role-matrix.md v5.0 CONSTITUTIONAL**: 7팀 역할 경계 최상위 규약, CC v2 포함, 사고 이력 12건
- **순차 스케줄링 v1.0**: A,B→CO→QA→RA 의존성 기반 Phase 구조
- **StudylistView XAML 구현** (PPT slides 5-7): Coordinator ViewModel + Design XAML 협업
- **디렉토리 단위 소유권 명확화**: DesignTime/ Design 단독 소유, 아키텍처 테스트로 검증
- **Flaky test 0건 달성**: S07-R1 일부 → S07-R5 완전 해결
- **RTM v2.6 동기화**: SWR→TC 매핑 업데이트
- **하드코딩 색상 토큰 교체**: EmergencyStop + 전체 UI 테마 일관성 확보

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
| [역할 매트릭스 v5.1](.claude/rules/teams/role-matrix.md) | 7팀 역할 경계 최상위 규약 (CONSTITUTIONAL FROZEN) |
| [CC 오케스트레이션 v2.1](.claude/rules/teams/cc.md) | CC v2: 독립 worktree, PR-only, team 브랜치 DISPATCH 모니터링 |
| [DISPATCH 프로토콜 v2.5](.claude/rules/teams/dispatch-protocol.md) | DISPATCH 생애주기, Phase 종속성, Status push 흐름 |
| [품질 기준 v1.3](.claude/rules/teams/quality-standards.md) | 품질 지표 SSOT, 빌드 범위 기준 (전체 솔루션 빌드 의무화) |
| [세션 관리 v1.2](.claude/rules/teams/session-lifecycle.md) | ScheduleWakeup, TIMEOUT, Stall Detection |
| [팀 공통 규칙 v3.2](.claude/rules/teams/team-common.md) | 규칙 파일 인덱스, HARD 규칙 요약 |
| [DISPATCH 템플릿 v1.2](.moai/dispatches/templates/STANDARD-DISPATCH.md) | DISPATCH 파일 표준 형식 (Issue # 필드 포함) |

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
| **프로젝트 팀** | 인간 2명 (리뷰/의사결정) + AI 에이전트 7팀 (CC + 6 구현팀) |
| **실행 모델** | AI 에이전트가 구현, 인간이 리뷰/의사결정 |

이 레포지토리는 H&abyz가 현재 판매 중인 HnVue 제품의 Console SW를 **자사 기술로 내재화하는 Greenfield 개발 프로젝트**입니다. 자세한 내용은 [ANALYSIS-002 -- 내재화 개발 컨텍스트](docs/ANALYSIS-002_InternalizationContext_v1.0.md)를 참조하세요.

### 팀 구성: 인간 2명 (리뷰어) + AI 에이전트 7팀 (실행자)

| 역할 | 인간 | AI 에이전트 | 인간이 하는 일 | AI가 하는 일 |
|------|------|-----------|--------------|------------|
| **CC** | 사용자 | hnvue-cc | 방향/우선순위 결정, PR 승인/머지 | DISPATCH 작성, PR 생성, 진도 추적 |
| **Team A** | - | hnvue-infra | 인프라 산출물 리뷰 | Common, Data, Security, SystemAdmin, Update 구현 |
| **Team B** | - | hnvue-medical | 의료 도메인 검증, Safety 리뷰 | Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning |
| **Coordinator** | - | hnvue-coordinator | 아키텍처 결정 | DI 통합, ViewModel, UI.Contracts, 통합테스트 |
| **Design** | - | hnvue-ui | PPT 디자인 제공, XAML 검수 | Views, Styles, Themes, Components |
| **QA** | - | hnvue-qa | 테스트 전략 승인, 릴리스 판정 | 빌드/테스트/커버리지/보안스캔 |
| **RA** | - | hnvue-ra | 규제 문서 승인, 인허가 제출 | RTM/SBOM/CMP/IEC 62304 문서 |

**운영 원칙:**
- **AI 에이전트가 구현, 인간이 리뷰/의사결정** (AI-first 실행 모델)
- **CC v2**: 독립 worktree + PR-only + 코드/빌드/테스트 CONSTITUTIONAL PROHIBITION
- **Phase 종속성**: Phase 1 (A,B) → Phase 2 (CO,Design) → Phase 3 (QA) → Phase 4 (RA)
- 병목: 인간 리뷰 속도 + 외부 의존성 (HW/벤더), 코딩 속도가 아님

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

**프로젝트 상태:** S17-R1 ACTIVE — CC v2 자율주행 (Safety-Critical 4/4 PASS 목표)
**현재 단계:** Phase 1 -- Tier 1+2 구현 (50.0% 완료)
**남음:** Phase 1 50.0% (12 Sprint), 전체 프로젝트 75.0% (36 Sprint)
**현실적 릴리즈:** 2027년 Q1~Q2 (Phase 1 완료 기준)

문서 최종 업데이트: 2026-04-22
