# Console-GUI

HnVue - Medical Diagnostic X-Ray Console Software

---

## Overview

HnVue Console SW는 H&abyz（에이치앤아비즈）가 자사 FPD（Flat Panel Detector）에 번들하여 판매하는 X-ray 촬영 콘솔 소프트웨어이다.

현재 외부 구매 중인 Console SW（IMFOU feel-DRCS OEM）를 **자체 개발로 내재화**하는 것이 본 프로젝트의 1차 목표이다.

| 항목 | 내용 |
|------|------|
| **제품명** | HnVue Console SW |
| **제조사** | H&abyz（에이치앤아비즈） |
| **프로젝트** | HnX-R1（Detector + Console SW 번들 retrofit） |
| **대체 대상** | IMFOU feel-DRCS（FDA K110033） |
| **FDA Predicate** | DRTECH EConsole1（[FDA K231225](https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf)） |
| **IEC 62304 분류** | Class B |
| **인허가 대상** | MFDS 2등급, FDA 510（k）, CE MDR Class IIa |
| **SW 인력** | 2명 |

---

## 기술 스택

| 계층 | 기술 | 비고 |
|------|------|------|
| UI Framework | WPF（.NET 8 LTS） | MVVM 패턴 |
| DICOM | fo-dicom 5.x（MIT） | C-STORE, MWL, Print SCU |
| 영상처리 | 외부 SDK（Phase 1） | Phase 2에서 자체 엔진 내재화 |
| DB | SQLite + EF Core | SQLCipher AES-256 암호화 |
| 로깅 | Serilog | SHA-256 해시 체인, 365일 보관 |
| 인증/보안 | bcrypt(cost=12), DPAPI, TLS 1.2+ | RBAC 4역할（Radiographer, Radiologist, Admin, Service） |
| 테스트 | xUnit + NSubstitute | 80%+ 커버리지 목표 |
| SBOM | CycloneDX for .NET | NVD 자동 매칭 |
| CI/CD | dotnet CLI + Code Signing | signtool.exe 디지털 서명 |

---

## 규제 표준

| 표준 | 적용 | 비고 |
|------|------|------|
| IEC 62304:2015+A1 | **Class B** | SW 수명주기 프로세스 |
| IEC 62366-1:2015+A1 | 필수 | 사용성 공학 |
| ISO 14971:2019 | 필수 | 위험 관리 |
| IEC 81001-5-1:2021 | 필수 | 사이버보안 수명주기 |
| FDA 21 CFR 820.30 | 필수 | Design Controls |
| FDA Section 524B | 필수 | SBOM + CVD + Patch/Update |
| ISO 13485:2016 | 필수 | QMS |
| DICOM 3.0 / IHE SWF | 필수 | 상호운용성 |
| MFDS 사이버보안 가이드라인 2024 | 필수 | 35개 항목, 7대 영역 |

---

## 소스코드 개발 현황

> **구현 방식**: 모듈 간 인터페이스 우선 설계(HnVue.Common.Abstractions) → Wave 단위 병렬 구현.
> 각 Wave는 이전 Wave가 `main` 머지 완료 후 분기.
> 안전 임계 모듈(Workflow, Security, Dose, Incident, Update)은 IEC 62304 §5.5 기준 90%+ 커버리지 적용.

---

### 솔루션 구조 (28개 프로젝트)

```
HnVue/
├── src/                         # 14개 소스 프로젝트
│   ├── HnVue.Common             # ✅ 구현 완료 — 17 인터페이스, Result<T>, 5 Enum
│   ├── HnVue.Data               # ✅ Wave 1 완료 — EF Core 8 + SQLCipher, 4 Repository
│   ├── HnVue.Security           # ✅ Wave 1 완료 — bcrypt/JWT/HMAC-SHA256 해시 체인 (안전 임계, 90%+)
│   ├── HnVue.UI                 # ✅ Wave 1 완료 — MahApps.Metro 테마 + LoginView skeleton
│   ├── HnVue.App                # ⏳ Wave 2  (DI 완전 연결)
│   ├── HnVue.Dicom              # ⏳ Wave 2
│   ├── HnVue.Incident           # ⏳ Wave 2  (안전 임계, 90%+)
│   ├── HnVue.Update             # ⏳ Wave 2  (안전 임계, 85%+)
│   ├── HnVue.Workflow           # ⏳ Wave 3  (안전 임계, 90%+)
│   ├── HnVue.Imaging            # 🔒 Wave 4  (Phase 1c)
│   ├── HnVue.Dose               # 🔒 Wave 4  (Phase 1c, DRL 수치표 필요)
│   ├── HnVue.PatientManagement  # 🔒 Wave 4  (Phase 1c)
│   ├── HnVue.CDBurning          # 🔒 Wave 4  (Phase 1c)
│   └── HnVue.SystemAdmin        # 🔒 Wave 4  (Phase 1c)
├── tests/                       # 13개 테스트 프로젝트 (모듈별 1:1)
└── tests.integration/           # 1개 통합 테스트 프로젝트
```

---

### 구현 단계별 상세

#### Pre-Wave — 완료 ✅ (2026-04-04, `v0.1.0-pre-wave`)

순차 실행. Wave 1 병렬 분기의 기준점(base commit).

| 작업 | 내용 | 결과 |
|------|------|:----:|
| 빌드 인프라 | `global.json` (.NET 8.0.419 LTS 고정), `Directory.Build.props` (Nullable/TreatWarningsAsErrors/Deterministic), `Directory.Packages.props` (CPM), `nuget.config`, `.editorconfig` | ✅ |
| 솔루션 스캐폴딩 | `HnVue.sln` + 28개 빈 `.csproj`, 의존성 그래프 기반 `ProjectReference` 연결, `dotnet build` 성공 | ✅ |
| HnVue.Common | `Result<T>` 모나드, `ErrorCode` (9개 도메인), `SafeState`/`UserRole`/`WorkflowState`/`GeneratorState`/`IncidentSeverity` Enum, 17개 서비스 인터페이스, 17개 DTO, `ThreadLocalSecurityContext` (ReaderWriterLockSlim) | ✅ |
| HnVue.Common.Tests | 38개 테스트 — Result 모나드, Enum 검증, SecurityContext 스레드 안전성 | ✅ 38/38 |
| Evaluator 평가 | 0.766 PASS — Critical 2건(IGeneratorInterface 추가, 동시성 수정) + High/Medium 3건 수정 | ✅ |

**모듈 의존성 그래프:**
```
HnVue.Common (Layer 0)
  └─ HnVue.Data (Layer 1)
       └─ HnVue.Security (Layer 2)
            ├─ HnVue.Dicom    (Layer 3)
            ├─ HnVue.Incident (Layer 3, 안전 임계)
            ├─ HnVue.Update   (Layer 3, 안전 임계)
            ├─ HnVue.Imaging  (Layer 3)
            └─ HnVue.Workflow (Layer 4, 안전 임계, Dicom+Imaging+Dose+Incident 통합)
                  └─ HnVue.UI (Layer 5, Common 인터페이스만 참조)
                        └─ HnVue.App (Layer 6, DI 컴포지션 루트)
```

---

#### Wave 1 — 완료 ✅ (2026-04-04, `main` 머지 완료)

Pre-Wave commit을 base로 3개 브랜치 동시 분기.

| Worktree | 브랜치 | 구현 모듈 | 핵심 내용 | 커버리지 |
|----------|--------|---------|---------|:-------:|
| WT-1 | `feat/wave1-data` | **HnVue.Data** | EF Core 8 + SQLCipher AES-256, 6개 Entity (Patients/Studies/Images/DoseRecords/Users/AuditLogs), Repository 구현, `Result.SuccessNullable<T>()` API | 80%+ |
| WT-2 | `feat/wave1-security` | **HnVue.Security** | bcrypt cost=12 (~300ms), JWT HS256 15분 만료, HMAC-SHA256 해시 체인, RBAC 4역할, 계정 잠금(5회) | **90%+** |
| WT-3 | `feat/wave1-ui-skeleton` | **HnVue.UI skeleton** | MahApps.Metro 테마 토큰 (Colors/Typography/Spacing/ButtonStyles), MainWindow 5-패널, LoginView+LoginViewModel | 60%+ |

**결과:** 0 errors / 0 warnings / 215 tests 통과 (Common 82 + Data 69 + Security 37 + UI 27)

**M1 검증 기준:** 로그인 → 인증 → RBAC → 감사 로그 해시 체인 E2E 동작

---

#### Wave 2 — 대기 ⏳ (4개 worktree 병렬, Phase 1b 핵심)

Wave 1 전체 `main` 머지 후 분기. 최대 10 agents 동시 (각 팀 2~3 agents).

| Worktree | 브랜치 | 구현 모듈 | 핵심 내용 | 커버리지 |
|----------|--------|---------|---------|:-------:|
| WT-4 | `feat/wave2-dicom` | **HnVue.Dicom** | C-STORE SCU, C-FIND MWL, Print SCU, TLS 1.2/1.3, OutboxQueue (Polly 재시도), fo-dicom 5.1.3 | 80%+ |
| WT-5 | `feat/wave2-incident` | **HnVue.Incident** | 4단계 심각도 분류, CVE 조회, 알림 체계 | **90%+** |
| WT-6 | `feat/wave2-update` | **HnVue.Update** | Authenticode 서명 검증, SHA-256, 백업/롤백 | **85%+** |
| WT-7 | `feat/wave2-app-ui` | **HnVue.App** + **HnVue.UI 완성** | DI 컴포지션 루트 + 나머지 6개 ViewModel (PatientList/Workflow/ImageViewer/DoseDisplay/SystemAdmin/CDBurn) | 70%+ |

---

#### Wave 3 — 대기 ⏳ (1개 worktree, Phase 1b 완성)

Wave 2 전체 `main` 머지 후 분기. Workflow는 모든 모듈의 통합점이므로 단독 Wave.

| Worktree | 브랜치 | 구현 모듈 | 핵심 내용 | 커버리지 |
|----------|--------|---------|---------|:-------:|
| WT-8 | `feat/wave3-workflow` | **HnVue.Workflow** | 9-상태 머신 (IDLE→COMPLETED/ERROR), GeneratorSerialPort (RS-232: STX+ETX 프레임, SET_KVP/PREP/EXPOSE/ABORT), GeneratorSimulator, FpdSdkWrapper, DetectorSimulator | **90%+** |

**M2 검증 기준:** 환자 선택 → 프로토콜 로드 → 촬영 준비 → 노출(시뮬레이터) → 영상 획득 → PACS 전송 전체 워크플로우

---

#### Wave 4 — 대기 🔒 (Phase 1c, DRL 수치표 수령 후)

| Worktree | 구현 모듈 | 블로커 |
|----------|---------|--------|
| WT-9 | **HnVue.Dose** | DRL 신체 부위별 수치 테이블 미확보 (사용자 제공 필요) |
| WT-10 | **HnVue.Imaging** | Wave 3 완료 후 |
| WT-11 | **HnVue.PatientManagement** | Wave 3 완료 후 |
| WT-12 | **HnVue.CDBurning** | Wave 3 완료 후 |
| WT-13 | **HnVue.SystemAdmin** | Wave 3 완료 후 |

---

#### Phase 1d — UI 통합 + 통합 테스트

Wave 4 완료 후.

| 작업 | 내용 |
|------|------|
| DI 완전 연결 | HnVue.App에서 전체 13개 모듈 DI 등록 |
| 통합 테스트 4개 시나리오 | 촬영 워크플로우 / DICOM 네트워크 / 인증 플로우 / CD 굽기 |
| M3/M4 게이트 | 커버리지: 안전 임계 90%+, 기타 80%+ |

---

### 진행 요약

```
Pre-Wave  ████████████████████  완료  ✅  v0.1.0-pre-wave
Wave 1    ████████████████████  완료  ✅  (Data + Security + UI skeleton, 215 tests)
Wave 2    ░░░░░░░░░░░░░░░░░░░░  대기  ⏳  (4 worktree 병렬)
Wave 3    ░░░░░░░░░░░░░░░░░░░░  대기  ⏳  (Workflow 단독)
Wave 4    ░░░░░░░░░░░░░░░░░░░░  대기  🔒  (DRL 수치표 필요)
Phase 1d  ░░░░░░░░░░░░░░░░░░░░  대기  🔒
```

---

## 개발 로드맵

### Phase 구성

| Phase | 범위 | 예상 공수 | 영상처리 |
|:-----:|------|:--------:|---------|
| **Phase 1** | Tier 1 + Tier 2（31개 MR） | 24–36 MM | 외부 SDK |
| Phase 2 | Tier 3（25개 MR） | 18–24 MM（인력 보강） | 자체 엔진 |
| Phase 3 | Tier 4（12개 MR） | TBD | 자체 + AI |

### MRD v3.0 — 4-Tier 우선순위 체계

벤치마크: [DRTECH EConsole1（K231225）](https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf), [feel-DRCS（K110033）](https://www.imfou.com/bbs/board.php?bo_table=product2&wr_id=8)

| Tier | 개수 | 의미 | Phase |
|:----:|:----:|------|:-----:|
| Tier 1（인허가 필수） | 13 | MFDS/FDA/IEC 규제 필수 | Phase 1 |
| Tier 2（시장 진입 필수） | 18 | feel-DRCS 동등 + 고객 최소 기대 | Phase 1 |
| Tier 3（있으면 좋고） | 25 | 경쟁 차별화, EConsole1 미포함 | Phase 2+ |
| Tier 4（비현실적） | 12 | 2명 조직 비현실적, AI/Cloud | Phase 3+ |
| 제외 | 4 | v2.0에서 제외 | - |
| **합계** | **72** | **Phase 1 = 31개** | |

### 추적성 체인

```
MR（72개）→ PR（65개）→ SWR（176+개）→ TC → HAZ
  MRD v3.0    PRD v2.0    FRS/SRS v2.0    RTM v2.0
```

### 문서 정합성 현황（2026-04-04, 평가 PASS 87.3/100）

| 구분 | 상태 | 개수 | 설명 |
|------|:----:|:----:|------|
| 핵심 체인（MRD/PRD/FRS/SRS/RTM） | ✅ 정합 | 5 | 4-Tier, 추적성 100%, RTM SAD/SDS 매핑 90행 완료 |
| 설계+관리+테스트 | ✅ 정합 | 13 | v2.0 개정 완료（SAD/SDS/DMP/SDP/WBS + 테스트 5개 + eSTAR） |
| 위험/보안/검증 | ✅ v2.0 | 3 | RMP v2.0（4HAZ+4RC）, STRIDE v2.0（38위협）, V&V Plan v2.0（13테스트 프로젝트） |
| 규제 문서 정합 | ✅ 반영 | 4 | VEX/보안통제/VMP/검증계획 — .NET 8 반영 완료 |
| RBAC 통일 | ✅ 완료 | 6 | PRD/FRS/SRS/SDS/RTM/DOC-003 — Radiographer/Radiologist/Admin/Service 4역할 |
| Phase별 개정 대기 | ⏳ 대기 | 12 | 검증 완료/인허가 시 개정 |
| 28종 인허가 템플릿 | 27/28 | - | C06 PenTest만 외부 위탁 대기 |

**교차검증 평가 이력:** Round 1（63.8）→ Round 2（79.9）→ Round 3（80.0）→ **Round 4（87.3 PASS）**

---

## 문서 체계

### 핵심 문서（v2.0+ 개정 완료）

개발 착수에 필요한 핵심 추적성 체인. **현재 정합 상태.**

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-001 | MRD（시장 요구사항） | **v3.0** | `docs/planning/DOC-001_MRD_v3.0.md` |
| DOC-001a | MR 상세 설명서 — Tier 1 | v1.0 | `docs/planning/DOC-001a_MR_Detailed_Spec_Tier1.md` |
| DOC-001b | MR 상세 설명서 — Tier 2/3/4 | v1.0 | `docs/planning/DOC-001b_MR_Detailed_Spec_Tier2_3_4.md` |
| DOC-002 | PRD（제품 요구사항） | **v2.0** | `docs/planning/DOC-002_PRD_v2.0.md` |
| DOC-004 | FRS（기능 요구사항） | **v2.0** | `docs/planning/DOC-004_FRS_v2.0.md` |
| DOC-005 | SRS（SW 요구사항） | **v2.0** | `docs/planning/DOC-005_SRS_v2.0.md` |
| DOC-032 | RTM（추적성 매트릭스） | **v2.0** | `docs/verification/DOC-032_RTM_v2.0.md` |
| DOC-036 | 510（k） eSTAR | **v2.0** | `docs/regulatory/DOC-036_510k_eSTAR_v2.0.md` |

### 설계 문서（Phase 1 착수 시 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-006 | SAD（SW 아키텍처 설계） | **v2.0** | `docs/planning/DOC-006_SAD_v2.0.md` |
| DOC-007 | SDS（SW 상세 설계） | **v2.0** | `docs/planning/DOC-007_SDS_v2.0.md` |

### 관리 문서（Phase 1 착수 시 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DMP-001 | 문서 마스터 플랜 | **v2.0** | `docs/management/DMP-001_DMP_v2.0.md` |
| DOC-003 | SW 개발 지침서 | v1.0 | `docs/management/DOC-003_SW_Development_Guideline_v1.0.md` |
| DOC-003a | SW 개발 절차서（SDP） | **v2.0** | `docs/management/DOC-003a_SW_Development_Procedure_v2.0.md` |
| DOC-016 | 사이버보안 관리 계획 | v1.0 | `docs/management/DOC-016_Cybersecurity_Plan_v1.0.md` |
| DOC-041 | PM 계획서 | v1.0 | `docs/management/DOC-041_PM_Plan_v1.0.md` |
| DOC-042 | 형상관리 계획 | v1.0 | `docs/management/DOC-042_CMP_v1.0.md` |
| DOC-043 | 소스코드 및 빌드 환경 | v1.0 | `docs/management/DOC-043_Build_Environment_v1.0.md` |
| DOC-044 | 알려진 결함 목록 | v1.0 | `docs/management/DOC-044_Known_Anomalies_v1.0.md` |
| WBS-001 | WBS | **v2.0** | `docs/management/WBS-001_WBS_v2.0.md` |

### 위험관리（Phase 1 착수 시 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-008 | 위험 관리 계획서 | **v2.0** | `docs/risk/DOC-008_Risk_Management_Plan_v1.0.md` |
| DOC-009 | FMEA | v1.0 | `docs/risk/DOC-009_FMEA_v1.0.md` |
| DOC-010 | 위험 관리 보고서 | v1.0 | `docs/risk/DOC-010_RMR_v1.0.md` |
| DOC-017 | 위협 모델링（STRIDE） | **v2.0** | `docs/risk/DOC-017_ThreatModel_v1.0.md` |
| DOC-047 | 사이버보안 위험 평가 | v1.0 | `docs/risk/DOC-047_Security_Risk_Assessment_v1.0.md` |

### 시험（Phase 1 구현 완료 시 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-012 | 단위 테스트 계획 | **v2.0** | `docs/testing/DOC-012_UnitTestPlan_v2.0.md` |
| DOC-013 | 통합 테스트 계획 | **v2.0** | `docs/testing/DOC-013_IntegTestPlan_v2.0.md` |
| DOC-014 | 시스템 테스트 계획 | **v2.0** | `docs/testing/DOC-014_SystemTestPlan_v2.0.md` |
| DOC-018 | 사이버보안 테스트 계획 | **v2.0** | `docs/testing/DOC-018_CyberTestPlan_v2.0.md` |
| DOC-021 | 사용성 공학 파일 | **v2.0** | `docs/testing/DOC-021_UsabilityFile_v2.0.md` |
| DOC-022 | 단위 테스트 보고서 | v1.0 | `docs/testing/DOC-022_UTReport_v1.0.md` |
| DOC-023 | 통합 테스트 보고서 | v1.0 | `docs/testing/DOC-023_ITReport_v1.0.md` |
| DOC-024 | 시스템 테스트 보고서 | v1.0 | `docs/testing/DOC-024_STReport_v1.0.md` |
| DOC-026 | 사이버보안 테스트 보고서 | v1.1 | `docs/testing/DOC-026_CyberTestReport_v1.0.md` |
| DOC-027 | 성능 테스트 보고서 | v1.0 | `docs/testing/DOC-027_PerfReport_v1.0.md` |
| DOC-028 | 사용성 테스트 보고서 | v1.0 | `docs/testing/DOC-028_UsabilityTestReport_v1.0.md` |
| DOC-030 | QA 테스트 계획 | v1.0 | `docs/testing/DOC-030_QA_Test_Plan_v1.0.md` |
| DOC-031 | QA 검증 보고서 | v1.0 | `docs/testing/DOC-031_QAVerification_v1.0.md` |

### 검증（Phase 1 검증 완료 시 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-011 | V&V 마스터 플랜 | **v2.0** | `docs/verification/DOC-011_VV_Master_Plan_v1.0.md` |
| DOC-015 | 밸리데이션 계획 | v1.0 | `docs/verification/DOC-015_ValidationPlan_v1.0.md` |
| DOC-025 | V&V 요약 보고서 | v1.0 | `docs/verification/DOC-025_VVSummary_v1.0.md` |
| DOC-029 | 임상 평가 보고서 | v1.0 | `docs/verification/DOC-029_CER_v1.0.md` |
| DOC-033 | SOUP 보고서 | v1.0 | `docs/verification/DOC-033_SOUP_Report_v1.0.md` |

### 인허가（인허가 제출 전 최종 개정）

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| DOC-019 | SBOM | v1.0 | `docs/regulatory/DOC-019_SBOM_v1.0.md` |
| DOC-020 | 임상 평가 계획 | v1.0 | `docs/regulatory/DOC-020_Clinical_Evaluation_Plan_v1.0.md` |
| DOC-034 | 릴리스 문서 | v1.0 | `docs/regulatory/DOC-034_ReleaseDoc_v1.0.md` |
| DOC-035 | DHF（설계 이력 파일） | v1.0 | `docs/regulatory/DOC-035_DHF_v1.0.md` |
| DOC-037 | CE 기술 문서 | v1.0 | `docs/regulatory/DOC-037_CE_TechDoc_v1.0.md` |
| DOC-038 | DICOM Conformance Statement | v1.0 | `docs/regulatory/DOC-038_DICOM_Conformance_v1.0.md` |
| DOC-039 | MFDS 제출 문서 | v1.0 | `docs/regulatory/DOC-039_KFDA_v1.0.md` |
| DOC-040 | IFU（사용설명서） | v1.0 | `docs/regulatory/DOC-040_IFU_v1.0.md` |
| DOC-045 | VEX 리포트 | v1.0 | `docs/regulatory/DOC-045_VEX_Report_v1.0.md` |
| DOC-046 | 보안 통제 | v1.1 | `docs/regulatory/DOC-046_Security_Controls_v1.0.md` |
| DOC-048 | 취약점 관리 계획 | v1.0 | `docs/regulatory/DOC-048_VMP_v1.0.md` |
| DOC-049 | IEC 81001-5-1 적합성 | v1.0 | `docs/regulatory/DOC-049_IEC81001_Compliance_v1.0.md` |
| DOC-050 | Predicate 비교 | v1.1 | `docs/regulatory/DOC-050_Predicate_Comparison_v1.0.md` |
| DOC-051 | PMS/PMCF | v1.0 | `docs/regulatory/DOC-051_PMS_PMCF_v1.0.md` |
| DOC-052 | GSPR 체크리스트 | v1.0 | `docs/regulatory/DOC-052_GSPR_Checklist_v1.0.md` |

---

## 28종 인허가 템플릿 매핑

> 참조: [software-templates](https://github.com/holee9/software-templates.git) — 의료기기 SW 인허가 28종 문서 템플릿

| Template | 산출물명 | 현행 문서 | 상태 |
|:--------:|----------|----------|:----:|
| A01 | SW Development Plan | DOC-003a | ✅ |
| A02 | SW Requirements Specification | DOC-005 v2.0 | ✅ |
| A03 | SW Architecture Design | DOC-006 | ✅ |
| A04 | SOUP List | DOC-033 | ✅ |
| A05 | Configuration Management Plan | DOC-042 | ✅ |
| A06 | SW Release Record | DOC-034 | ✅ |
| A07 | Source Code & Build Environment | DOC-043 | ✅ |
| A08 | Known Anomaly List | DOC-044 | ✅ |
| B01 | Integration Test Report | DOC-023 | ✅ |
| B02 | System Test Report | DOC-024 | ✅ |
| B03 | Requirements Traceability Matrix | DOC-032 v2.0 | ✅ |
| B04 | Usability Engineering Summary | DOC-021 + DOC-028 | ✅ |
| B05 | Clinical Evaluation / Equivalence | DOC-029 | ✅ |
| C01 | SBOM | DOC-019 | ✅ |
| C02 | VEX Report | DOC-045 | ✅ |
| C03 | Cybersecurity Controls | DOC-046 v1.1 | ✅ |
| C04 | Threat Model | DOC-017 | ✅ |
| C05 | Cybersecurity Risk Assessment | DOC-047 | ✅ |
| **C06** | **Penetration Test** | **외부 위탁 대기** | **🔄** |
| C07 | Vulnerability Management Plan | DOC-048 | ✅ |
| D01 | Risk Management File | DOC-008 + 009 + 010 | ✅ |
| D02 | IEC 81001-5-1 Compliance | DOC-049 | ✅ |
| E01 | Predicate / SE Comparison | DOC-050 v1.1 | ✅ |
| E02 | Labeling & IFU | DOC-040 | ✅ |
| E03 | GSPR Checklist | DOC-052 | ✅ |
| E04 | eSTAR Submission | DOC-036 v2.0 | ✅ |
| F01 | Clinical Evaluation Report | DOC-029 | ✅ |
| F02 | PMS / PMCF Package | DOC-051 | ✅ |

> C06 PenTest: KTL（한국산업기술시험원）공인 시험 위탁 예정. 상세 계획은 CYBERSEC-003 참조.

---

## 리서치 문서

### 전략

| 문서 | 버전 | 경로 |
|------|:----:|------|
| 회사 포지셔닝 전략 | v2.0 | `docs/planning/research/STRATEGY-001_Company_Positioning_v2.0.md` |
| MRD 우선순위 재조정 제안서 | v1.0 | `docs/planning/research/MRD_Priority_Reassessment_Proposal.md` |
| FPD 콘솔 SW 시장 조사 | - | `docs/planning/research/FPD_Console_SW_Market_Research.md` |
| X-ray 콘솔 SW 경쟁 분석 | - | `docs/planning/research/market-research-xray-console-software.md` |
| X-ray 영상 SW 시장 데이터 | - | `docs/planning/research/market-research-xray-imaging-software.md` |

### 사이버보안 딥리서치

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| CYBERSEC-001 | 사이버보안 자체 검증 가이드 | v1.0 | `docs/planning/research/CYBERSEC-001_Self_Assessment_Guide_v1.0.md` |
| CYBERSEC-002 | 침투 테스트 독립성/전문성 가이드 | v1.0 | `docs/planning/research/CYBERSEC-002_Independence_Expertise_Guide_v1.0.md` |
| CYBERSEC-003 | 한국 내 최소비용 위탁 가이드 | v1.1 | `docs/planning/research/CYBERSEC-003_Korea_Pentest_Outsourcing_Guide_v1.1.md` |
| CYBERSEC-004 | 공인기관 의뢰 전 자체평가 가이드 | v1.0 | `docs/planning/research/CYBERSEC-004_Internal_PreAssessment_Guide_v1.0.md` |

**사이버보안 권장 전략:**
- **A（추천）**: CYBERSEC-004 자체평가 → CYBERSEC-003 KTL 공인시험 → 공인 성적서로 독립성 자동 충족
- **B（비용 절감）**: CYBERSEC-004 자체평가 → CYBERSEC-002 참조 → 자체 리포트 작성

---

## Archive

`docs/archive/` 에 이동된 구 버전 파일. 이력 보존 목적.

| 파일 | 대체 문서 | 사유 |
|------|----------|------|
| DOC-001_MRD_v1.0.md | MRD v3.0 | 초기 버전 |
| DOC-001_MRD_v2.0.md | MRD v3.0 | P1–P4 체계 폐기 |
| DOC-002_PRD_v1.0.md | PRD v2.0 | MR 추적성 없음 |
| DOC-004_FRS_v1.0.md | FRS v2.0 | Tier 미반영 |
| DOC-005_SRS_v1.0.md | SRS v2.0 | Tier 미반영 |
| STRATEGY-001 v1.0 | v2.0 | 초기 전략 |
| DOC-036_eSTAR_v1.0.md | eSTAR v2.0 | 인시던트 대응 미포함 |
| DOC-032_RTM_v1.0.md | RTM v2.0 | P1–P4, MR-072 없음 |
| CVR-002 중복 파일 | CVR-002 v1.0 | 파일명 불일치 |
| cybersecurity-template.md | DOC-046/048 | 개별 문서 분리 완료 |

---

## Document Sync & Revision

### 자동 동기화 스크립트

MRD/PRD 개정 시 전체 문서의 버전 참조, 제품명, Mermaid 오류를 자동 동기화합니다.

```bash
# 검증만（수정 안 함）
python scripts/sync_docs.py --check --verbose

# 실행（자동 수정）
python scripts/sync_docs.py
```

동기화 대상:
- 구 버전 참조 → 현행 버전으로 교체
- RadiConsole → HnVue 제품명 통일
- HnVue HnVue 중복 제거
- Mermaid 비flowchart classDef 제거

> 현행 버전은 `scripts/sync_docs.py` 의 `CURRENT_VERSIONS`에 정의.
> MRD/PRD 개정 시 이 딕셔너리만 업데이트 후 스크립트 실행.

### Phase별 문서 개정 로드맵

| 시점 | 대상 문서 | 개정 내용 |
|------|----------|----------|
| **완료** | MRD, PRD, FRS, SRS, RTM, eSTAR, SAD, SDS, DMP, SDP, WBS, UTP, ITP, STP, CyberTest, Usability | 4-Tier, MR-072, 추적성, 보완 3건, Phase 1 착수 준비 |
| **완료（v2.0 교차검증, 평가 87.3 PASS）** | SRS/FRS/PRD(RBAC 4역할+bcrypt), SDS(디자인토큰+MahApps), SBOM(.NET8+NSubstitute), DOC-043(28프로젝트), RTM(SAD/SDS 90행), RMP(4HAZ+4RC), STRIDE(38위협), V&V(13테스트), VEX/보안통제/VMP/검증계획(.NET8), SOUP(NSubstitute 제외주석), DOC-003(bcrypt) | 15개 문서 개정, 4회 평가 반복 |
| **Phase 1 착수 시** | ~~SAD, SDS, DMP, SDP, WBS~~ | ~~Tier 반영 + 실제 설계~~ **완료** |
| **Phase 1 구현 완료 시** | ~~UTP, ITP, STP, CyberTest, Usability~~ | ~~테스트 계획~~ **완료**, 구현 후 실제 TC 기입 |
| **Phase 1 검증 완료 시** | UTR, ITR, STR, V&V Summary, QA | 실제 테스트 결과 |
| **인허가 제출 전** | eSTAR, DHF, DICOM Conf, IFU, GSPR | 최종 확정 |
| **시판 후** | PMS, PMCF, CER | 실제 시판 후 데이터 |

### Placeholder 안내

문서들은 개발 착수 전 계획서/사양서 수준으로 작성되어 있다. 다음 항목들은 개발 완료 후 실제 값으로 채워야 한다:

- `[TBD - 개발 완료 후 작성]` — 빌드 해시, 테스트 결과, 릴리즈 버전
- `[작성 필요]` — 실제 데이터 수집 후 기입
- `[TBD]` — Predicate 510（k） 번호, NB 지정, 인증서 번호

---

## Branch Strategy for Parallel UI Development

웹 UI 사용성 검증을 별도 PC에서 병행할 때는 `main`을 통합 기준선으로 유지하고, 실험성 UI는 전용 브랜치에서만 진행한다.

| 목적 | 권장 브랜치 | 운영 원칙 |
|------|-------------|----------|
| 통합 기준선 | `main` | 문서 정리, 검증 완료된 모듈, 병합 완료된 Wave 결과만 반영 |
| 데스크톱/WPF 구현 | `feat/wave*-*` | 현재 Wave 범위의 WPF, App, Core 모듈 구현 전용 |
| 웹 UI 사용성 검증 | `feature/web-ui` | 웹 전용 화면, mock data, API contract, 사용성 테스트 산출물만 반영 |

### 운영 규칙

- `main`은 데스크톱 릴리스 기준선으로 유지한다. 웹 UI 실험 코드는 직접 섞지 않는다.
- 웹 UI는 별도 PC에서 병행 개발 가능하며, 가급적 `feature/web-ui` 한 브랜치에만 모은다.
- 웹 브랜치에서는 WPF 화면과 기존 App wiring을 직접 수정하지 말고, 웹 전용 디렉터리와 계약 문서 중심으로 작업한다.
- 공용 변경이 필요하면 `HnVue.Common` 인터페이스 또는 별도 contract 문서에 먼저 반영하고, 데스크톱과 웹이 같은 의미의 DTO와 서비스 계약을 공유하도록 맞춘다.
- 사용성 검증이 끝난 후에는 웹 브랜치 전체를 합치기보다, `main`에 필요한 산출물과 검증된 계약 변경만 선택적으로 반영한다.
- **새 작업 브랜치는 반드시 Gitea(`origin`)에도 push해야 한다.** Gitea Push Mirror가 10분 간격으로 GitHub를 Gitea 상태로 덮어쓰므로, Gitea에 없는 브랜치는 GitHub에서 자동 삭제된다.

> 권장 흐름: `main` pull → `feature/web-ui`에서 UI 검증 → 결과 정리 → 필요한 공용 계약만 `main`으로 병합

### Mirror Sync 동작 방식

Gitea에는 **Push Mirror**가 설정되어 있다(`interval: 10m, sync_on_commit: true`). 이 미러가 Gitea → GitHub 자동 동기화의 주체이며, Gitea 상태를 기준으로 GitHub를 덮어쓴다.

- Gitea에 없는 브랜치는 Push Mirror 실행 시 GitHub에서 자동 삭제된다.
- `feature/web-ui` 등 작업 브랜치는 반드시 **Gitea에도 push**해야 GitHub에서 보존된다.
- `scripts/sync_to_github.ps1`은 비상 수동 동기화용이며, 평상시 자동 동기화는 Push Mirror가 담당한다.
- Push Mirror가 있는 한 `git push --mirror`, `git push --prune` 같은 별도 명령은 불필요하며 사용하지 않는다.

---

## Remote CI for Secondary Workstation

보조 작업 PC에 `dotnet SDK`가 없어도 GitHub Actions로 원격 빌드 검증을 수행할 수 있다.

- `desktop-ci`: `windows-latest`에서 WPF/.NET 8 restore, build, test 수행
- `web-ui-ci`: `web-ui/` 또는 `src/HnVue.Web/` 아래 `package.json`이 생기면 Node 기반 lint, test, build 수행
- 이 PC에서는 코드 수정과 push에 집중하고, 실제 WPF 실행, 장치 연동, 현장 배포 검증은 원래 구현 PC에서 계속 진행한다.

> 권장 흐름: 보조 PC에서 수정 및 push → GitHub Actions로 원격 검증 → 원래 구현 PC에서 실제 실행/하드웨어 확인

---

## Sync to GitHub Mirror (Gitea → GitHub)

### 수동 동기화 스크립트 (비상용)

Gitea Push Mirror 장애 또는 즉시 동기화가 필요할 때만 사용한다. `--mirror`/`--prune` 없이 지정한 브랜치만 push한다.

```powershell
# 최초 1회 — GitHub remote 등록
git remote add github https://github.com/holee9/console-gui.git

# main + feature/web-ui 동기화
.\scripts\sync_to_github.ps1 -Branches main, feature/web-ui

# 실행 전 확인 (dry-run)
.\scripts\sync_to_github.ps1 -DryRun
```

> 스크립트 위치: `scripts/sync_to_github.ps1`
> 평상시 자동 동기화는 Gitea Push Mirror(10m)가 담당한다. 이 스크립트는 보조 수단이다.

### 새 브랜치 생성 시 체크리스트

GitHub(보조 PC)에서 새 브랜치를 만든 경우 반드시 아래 절차를 따른다.

```bash
# 1) GitHub 브랜치 로컬에 가져오기
git fetch github <branch>

# 2) Gitea에 push (Push Mirror 보존을 위해 필수)
git push origin github/<branch>:refs/heads/<branch>
```

브랜치가 Gitea에 없으면 다음 Push Mirror 실행(최대 10분) 때 GitHub에서 삭제된다.

---

## Mirror Sync Fix Status

> **이 섹션은 GitHub 미러 측에서 동기화 방식 전환 여부를 확인하기 위한 체크포인트입니다.**

### 현재 상태: ✅ 해결 완료 (2026-04-04)

| # | 확인 항목 | 기대 결과 | 상태 |
|---|-----------|-----------|------|
| 1 | `scripts/sync_to_github.ps1` 파일 존재 | 파일 존재 | ✅ |
| 2 | `feature/web-ui` — Gitea(origin) 존재 | `refs/heads/feature/web-ui` 존재 | ✅ |
| 3 | `feature/web-ui` — GitHub 존재 | `refs/heads/feature/web-ui` 존재 | ✅ |
| 4 | Gitea ↔ GitHub commit 일치 | 양쪽 `7926ba1` 동일 | ✅ |

### 근본 원인 (확인 완료)

Gitea 저장소에 **Push Mirror**가 설정되어 있었다. 이 미러는 10분 간격과 커밋 즉시 실행 모드로 GitHub에 push하며, 내부적으로 `--mirror` 동작을 사용한다.

`feature/web-ui`가 **GitHub에만 존재하고 Gitea에는 없는** 상태였기 때문에, Push Mirror가 실행될 때마다 Gitea 상태를 기준으로 GitHub를 덮어써 `feature/web-ui`를 반복 삭제했다.

```
Gitea Push Mirror 설정 (조회 결과)
  remote : https://github.com/holee9/console-gui.git
  interval: 10m (+ sync_on_commit: true)
```

### 해결 방법 (적용 완료)

`feature/web-ui`를 GitHub에서 Gitea에도 동일하게 push하여, 양쪽 저장소 상태를 일치시켰다.

```bash
# 실행한 명령
git fetch github
git push origin github/feature/web-ui:refs/heads/feature/web-ui
```

이후 Gitea Push Mirror가 실행되면 `feature/web-ui`가 Gitea에도 존재하므로 GitHub에서 삭제되지 않는다.

### 브랜치 삭제 재발 시 대응

GitHub에서 `feature/web-ui`가 다시 사라진 경우, Gitea에서 해당 브랜치도 사라졌는지 먼저 확인한다.

```bash
# Gitea 브랜치 목록 확인
git ls-remote origin

# Gitea에 없다면 GitHub → Gitea 복구 후 양쪽 동기화
git fetch github
git push origin github/feature/web-ui:refs/heads/feature/web-ui
```

> Gitea에 없는 브랜치는 Push Mirror가 실행될 때마다 GitHub에서도 삭제된다.
> 작업 브랜치는 **반드시 Gitea와 GitHub 양쪽에 존재**해야 한다.

### GitHub에 새 커밋이 안 보이거나 예전 커밋으로 되돌아갈 때

증상:

- 이 PC에서 `git push origin feature/web-ui` 는 성공한 것처럼 보임
- 하지만 GitHub 브랜치 화면에는 방금 커밋이 안 보임
- 또는 잠시 보였다가 Gitea에 있던 예전 커밋으로 다시 돌아감

원인:

- GitHub `feature/web-ui`만 먼저 앞으로 갔고
- Gitea `feature/web-ui`는 아직 예전 커밋인 상태에서
- Push Mirror가 다시 Gitea 상태를 기준으로 GitHub를 덮어씀

해결 원칙:

- **GitHub에 새 커밋을 올린 직후, Gitea의 `feature/web-ui`도 즉시 같은 커밋으로 fast-forward 해야 한다.**
- `git fetch github <branch>` 만 단독으로 쓰지 말고, **반드시 `refs/remotes/github/<branch>` 를 명시 갱신**해야 한다.
- 이유: plain `git fetch github feature/web-ui` 또는 `git fetch github main` 은 `FETCH_HEAD` 만 갱신되고, 이후 `git merge github/...` 가 **stale remote-tracking ref** 를 써서 예전 커밋을 다시 push 할 수 있다.

#### Gitea 작업 PC 복붙용 명령

주의:

- 아래 3줄은 **`feature/web-ui` 전용**이다.
- 이 3줄을 실행해도 **Gitea `main` 은 바뀌지 않는다.**
- Gitea 웹 화면에서 기본으로 보이는 README는 보통 `main` 이므로, README 변경 확인은 아래 `main 동기화` 블록을 따로 실행해야 한다.

아래 3줄을 `feature/web-ui` 반영용 **표준 명령**으로 사용한다.

```bash
git fetch github feature/web-ui:refs/remotes/github/feature/web-ui
git checkout -B feature/web-ui github/feature/web-ui
git push origin feature/web-ui
```

#### 왜 3줄 표준 명령만 쓰는가

- `git fetch github feature/web-ui:refs/remotes/github/feature/web-ui`
  GitHub 최신 브랜치를 **로컬 remote-tracking ref** 에 강제로 반영한다.
- `git checkout -B feature/web-ui github/feature/web-ui`
  로컬 `feature/web-ui` 를 GitHub 최신 커밋으로 정확히 맞춘다.
- `git push origin feature/web-ui`
  Gitea 원본 저장소를 GitHub와 동일 커밋으로 맞춘다.

#### 금지: 예전 4줄 fast-forward 블록

아래 패턴은 **사용 금지**:

```bash
git fetch github feature/web-ui
git checkout feature/web-ui
git merge --ff-only github/feature/web-ui
git push origin feature/web-ui
```

이 4줄은 `github/feature/web-ui` ref 가 stale 인 상태에서 예전 커밋을 다시 Gitea 원본으로 push 할 수 있다. 실제 운영 중 **README가 다시 예전 상태로 돌아간 원인**으로 확인되었다.

#### 그 다음 GitHub 미러 동기화까지 다시 맞추려면

PowerShell에서 아래를 실행:

```powershell
.\scripts\sync_to_github.ps1 -Branches main, feature/web-ui
```

#### 운영 팁

- 가장 안전한 순서는 `GitHub push -> 즉시 Gitea fast-forward -> 필요 시 sync_to_github.ps1 실행` 이다.
- `feature/web-ui` 작업 중에는 `Gitea feature/web-ui` 와 `GitHub feature/web-ui` 의 헤드 커밋이 다르면 안 된다.
- `merge --ff-only` 가 실패하면, Gitea `feature/web-ui` 에서 별도 커밋이 생긴 상태일 가능성이 있으므로 수동 정리가 필요하다.

### 이력

| 날짜 | 변경 내용 |
|------|-----------|
| 2026-04-04 | `scripts/sync_to_github.ps1` 추가, README Mirror Sync 섹션 개선 |
| 2026-04-04 22:23 KST | `feature/web-ui` 재생성 후에도 삭제 재발 — Gitea Push Mirror가 원인으로 특정 |
| 2026-04-04 | Gitea Push Mirror 설정 확인 (10m interval, sync_on_commit) |
| 2026-04-04 | `feature/web-ui`를 Gitea에 push → Gitea ↔ GitHub 완전 일치, 해결 완료 |

---

## Sync from GitHub Mirror (GitHub → Gitea)

Perplexity Computer에서 GitHub 미러 작업 내용을 사내 Gitea에 반영할 때도 **plain fetch + merge 패턴은 사용하지 않는다**.

#### Gitea `main` README 를 GitHub 최신 상태로 맞추는 복붙용 명령

아래 4줄은 **`main` 전용**이다. GitHub `main` 의 README 변경을 Gitea 기본 화면에도 보이게 하려면 이 블록을 실행해야 한다.

```bash
# 최초 1회
git remote add github https://github.com/holee9/console-gui.git

# 권장: main만 동기화
git fetch github main:refs/remotes/github/main
git checkout main
git merge --ff-only github/main
git push origin main
```

#### GitHub 작업 브랜치 `feature/web-ui` 를 Gitea에도 반영하는 복붙용 명령

```bash
# GitHub 작업 브랜치를 Gitea에도 반영해야 할 때
git fetch github feature/web-ui:refs/remotes/github/feature/web-ui
git checkout -B feature/web-ui github/feature/web-ui
git push origin feature/web-ui
```

즉:

- `Gitea main README 를 바꾸고 싶다` -> `main` 4줄 블록 실행
- `Gitea feature/web-ui 브랜치를 맞추고 싶다` -> `feature/web-ui` 3줄 블록 실행
