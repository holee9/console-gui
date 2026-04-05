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
│   ├── HnVue.Security           # ✅ Wave 1+REF 완료 — bcrypt/JWT/HMAC, PasswordHasher, RbacPolicy (안전 임계, 90%+)
│   ├── HnVue.UI                 # ✅ Wave 1 완료 — MahApps.Metro 테마 + LoginView/MainView
│   ├── HnVue.App                # ✅ Phase 1d 완료 — 전체 DI 등록, 13개 모듈 통합
│   ├── HnVue.Workflow           # ✅ REF 완료 — WorkflowEngine, WorkflowStateMachine(9-상태), GeneratorSimulator (안전 임계, 90%+)
│   ├── HnVue.Dose               # ✅ REF 완료 — DoseService 4단계 인터록 Allow/Warn/Block/Emergency (안전 임계, 90%+)
│   ├── HnVue.PatientManagement  # ✅ REF 완료 — PatientService(CRUD+중복), WorklistService(MWL+응급ID)
│   ├── HnVue.Dicom              # ✅ REF 완료 — DicomStoreScu, DicomFindScu, DicomFileIO, DicomFileWrapper
│   ├── HnVue.Incident           # ✅ REF 완료 — IncidentResponseService 4단계 심각도 (안전 임계, 90%+)
│   ├── HnVue.Update             # ✅ REF 완료 — SWUpdateService, CodeSignVerifier(SHA-256), BackupService (안전 임계, 85%+)
│   ├── HnVue.SystemAdmin        # ✅ REF 완료 — SystemAdminService(설정 검증+감사 CSV 내보내기)
│   ├── HnVue.CDBurning          # ✅ REF 완료 — CDDVDBurnService, IMAPIComWrapper(IMAPI2 시뮬)
│   └── HnVue.Imaging            # ⏳ 스텁 (Phase 1c — 외부 SDK 연동 대기)
├── tests/                       # 13개 테스트 프로젝트 (모듈별 1:1)
│   └── 475개 테스트 전체 통과 (IEC 62304 SWR Trait 추적성 포함)
└── tests.integration/           # 1개 통합 테스트 프로젝트 — 18개 테스트 전체 통과
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

#### REF (Review-Evaluate-Fix 10사이클) — 완료 ✅ (2026-04-05)

Wave 1 기반 위에서 계획서·사양서(SDS/SAD/SRS) 대비 누락 모듈을 10회 반복 평가·구현.

| 사이클 | 구현 내용 |
|:------:|---------|
| 1 | `PasswordHasher` (bcrypt cost=12), `RbacPolicy` (4역할 권한 매트릭스) — Security 보완 |
| 2 | `WorkflowStateMachine` (9-상태 전이표), `WorkflowEngine` (IWorkflowEngine) |
| 3 | `DoseService` (4단계 인터록), `DoseValidationLevel.Emergency` 추가 |
| 4 | `PatientService` (CRUD+중복검사), `WorklistService` (MWL+응급ID) |
| 5 | `GeneratorSimulator` (장애 주입 포함, IGeneratorInterface 구현) |
| 6 | `IncidentResponseService` (4단계 심각도+긴급 콜백) |
| 7 | `SystemAdminService` (설정 검증+감사 CSV 내보내기) |
| 8 | `SWUpdateService`, `CodeSignVerifier` (SHA-256), `BackupService` (타임스탬프 백업/복원) |
| 9 | `CDDVDBurnService`, `IMAPIComWrapper` (IMAPI2 시뮬), `DicomStoreScu`, `DicomFindScu`, `DicomFileIO` |
| 10 | `DoseValidationLevel` 변경으로 인한 Common.Tests 수정 및 전체 검증 |

**결과 (REF):** 0 errors / 0 warnings / **475 tests 전체 통과** (Imaging 20개 포함)

| 모듈 | 이전 | 이후 | 증가 |
|------|:----:|:----:|:----:|
| Security.Tests | 37 | 91 | +54 |
| Workflow.Tests | 0 | 64 | +64 |
| PatientManagement.Tests | 0 | 27 | +27 |
| Update.Tests | 0 | 25 | +25 |
| Dose.Tests | 0 | 17 | +17 |
| Incident.Tests | 0 | 13 | +13 |
| SystemAdmin.Tests | 0 | 13 | +13 |
| CDBurning.Tests | 0 | 12 | +12 |
| Dicom.Tests | 0 | 15 | +15 |
| Imaging.Tests | 0 | 20 | +20 |
| Common+Data+UI | 178 | 178 | 0 |
| **합계 (REF)** | **215** | **475** | **+260** |

> 모든 테스트에 `[Trait("SWR", "SWR-XXX")]` IEC 62304 추적성 어노테이션 포함.

---

#### Wave 2 — 완료 ✅ (Phase 1b 핵심, REF 10-사이클 루프에서 구현)

| 구현 모듈 | 핵심 내용 | 테스트 | 커버리지 |
|---------|---------|:-----:|:-------:|
| **HnVue.Dicom** | `DicomStoreScu` (C-STORE SCU), `DicomFindScu` (C-FIND MWL), `DicomFileIO`, `DicomFileWrapper`, fo-dicom 5.1.3 | 15개 | 80%+ |
| **HnVue.Incident** | `IncidentResponseService` (4단계 심각도: Critical/High/Medium/Low, 긴급 콜백) | 13개 | **90%+** |
| **HnVue.Update** | `SWUpdateService`, `CodeSignVerifier` (SHA-256 해시), `BackupService` (타임스탬프 백업/복원) | 25개 | **85%+** |
| **HnVue.Security 보완** | `PasswordHasher` (bcrypt cost=12 정적 메서드), `RbacPolicy` (4역할 권한 상수 매트릭스) | +54개 (총 91개) | **90%+** |

---

#### Wave 3 — 완료 ✅ (Phase 1b 완성, REF 10-사이클 루프에서 구현)

| 구현 모듈 | 핵심 내용 | 테스트 | 커버리지 |
|---------|---------|:-----:|:-------:|
| **HnVue.Workflow** | `WorkflowStateMachine` (9-상태 검증 전이표), `WorkflowEngine` (IWorkflowEngine, Abort/StateChanged 이벤트), `GeneratorSimulator` (장애 주입 포함) | 64개 | **90%+** |

**M2 검증 기준:** 환자 선택 → 프로토콜 로드 → 촬영 준비 → 노출(시뮬레이터) → 영상 획득 → PACS 전송 전체 워크플로우

---

#### Wave 4 — 완료 ✅ (Phase 1c 핵심, REF 10-사이클 루프에서 구현)

| 구현 모듈 | 핵심 내용 | 테스트 | 비고 |
|---------|---------|:-----:|------|
| **HnVue.Dose** | `DoseService` (4단계 인터록: ALLOW/WARN/BLOCK/EMERGENCY), `DoseValidationLevel.Emergency` 추가 | 17개 | 안전 임계 90%+ |
| **HnVue.PatientManagement** | `PatientService` (CRUD+중복 체크), `WorklistService` (MWL Import+응급 ID 생성) | 27개 | 80%+ |
| **HnVue.SystemAdmin** | `SystemAdminService` (설정 검증 + 감사 로그 CSV 내보내기) | 13개 | 80%+ |
| **HnVue.CDBurning** | `CDDVDBurnService`, `IBurnSession`, `IMAPIComWrapper` (IMAPI2 시뮬레이션) | 12개 | 80%+ |
| **HnVue.Imaging** | 스텁 유지 (외부 SDK 연동 대기) | - | Phase 1c 잔여 |

---

#### Phase 1d — UI 통합 + 통합 테스트 ✅ (2026-04-05)

| 작업 | 내용 | 결과 |
|------|------|:----:|
| DI 완전 연결 | HnVue.App — 13개 모듈 전체 DI 등록 (Microsoft.Extensions.Hosting) | ✅ |
| 통합 테스트 — 인증 플로우 | SecurityService + RBAC + 감사 체인 E2E | ✅ |
| 통합 테스트 — 촬영 워크플로우 | WorkflowEngine 9-상태 전이, GeneratorSimulator 장애 주입 | ✅ |
| 통합 테스트 — DICOM 네트워크 | DicomStoreScu C-STORE, DicomFindScu C-FIND MWL | ✅ |
| 통합 테스트 — CD 굽기 | CDDVDBurnService 세션 관리 + IMAPIComWrapper 시뮬 | ✅ |
| REF 10-사이클 | Review-Evaluate-Fix 루프 — 경고 0, 전체 테스트 통과 | ✅ |

**결과:** 0 errors / 0 warnings / **493 tests 전체 통과** (단위 475 + 통합 18)

---

## 변경 이력 (Changelog)

### 2026-04-05 — 코드 품질 검증 및 보안 취약점 수정

**영역:** 보안, 인프라, 테스트

**주요 수정사항:**

1. **SecurityService.cs** — 역할 계층 비교 버그 수정
   - `HasRoleOrHigher()` 메서드: 역할 계층 비교 로직 수정 (Radiographer < Radiologist < Admin)
   
2. **JwtTokenService.cs** — JWT 검증 메서드 추가
   - `Validate()` 메서드 신규 구현: JWT 서명, 만료시간, 클레임 검증
   - `SecurityTokenMalformedException` 예외 처리 개선
   
3. **Repository 모듈들** (AuditRepository, PatientRepository, StudyRepository, UserRepository)
   - `OperationCanceledException` 재발생 처리 추가 (취소 요청 시 즉시 예외 전파)
   
4. **JwtOptions.cs, AuditService.cs** — 프로덕션 배포 경고 주석 추가
   - 본번 배포 전 설정값 검토 필수 주석 추가
   
5. **단위 테스트 신규 추가** (8개)
   - JwtTokenServiceTests: `Validate()` 메서드 4개 시나리오 테스트
   - SecurityServiceTests: `HasRoleOrHigher()` 역할 비교 2개 테스트
   - 통합 테스트 포함 총 테스트 증가: **491 → 499** 

**최종 상태:**
- 품질 점수: **0.82/1.0** (PASS)
- 테스트 커버리지: **안전 임계 모듈 90%+** 유지
- 빌드: 0 errors, 0 warnings

---

### 진행 요약

```
Pre-Wave  ████████████████████  완료  ✅  v0.1.0-pre-wave
Wave 1    ████████████████████  완료  ✅  Data + Security + UI skeleton (215 tests)
REF Loop  ████████████████████  완료  ✅  10-사이클 Review-Evaluate-Fix (475 tests)
Wave 2    ████████████████████  완료  ✅  Dicom + Incident + Update + Security 보완
Wave 3    ████████████████████  완료  ✅  Workflow (9-상태 머신 + Generator 시뮬)
Wave 4    ████████████████████  완료  ✅  Dose + PatientMgmt + SystemAdmin + CDBurning + Imaging
Phase 1d  ████████████████████  완료  ✅  DI 통합 + 통합 테스트 4개 시나리오 (493 tests total)
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

## Git Repository

### 저장소 구성

| 저장소 | 역할 | 주소 |
|--------|------|------|
| **Gitea**（origin） | 사내 주 저장소 | `http://10.11.1.40:7001/DR_RnD/Console-GUI.git` |
| **GitHub**（github） | 외부 미러 | `https://github.com/holee9/console-gui.git` |

- 자동 동기화: **Gitea → GitHub**（Push Mirror, 10분 간격）
- Gitea가 기준. GitHub는 미러이므로 Gitea에 없는 브랜치는 GitHub에서 자동 삭제됨.

### 브랜치

| 브랜치 | 용도 |
|--------|------|
| `main` | 릴리스 기준선 |
| `feat/wave*-*` | Wave별 WPF 구현 |
| `feature/web-ui` | 웹 UI 검증 |

### GitHub → Gitea 동기화

Perplexity 세션 등에서 GitHub에 push한 내용을 사내 Gitea에 반영할 때 사용한다.

최초 1회:

```bash
git remote add github https://github.com/holee9/console-gui.git
```

매번 `main` 반영:

```bash
git fetch github
git checkout main
git merge github/main
git push origin main
```

매번 `feature/web-ui` 반영:

```bash
git fetch github
git checkout feature/web-ui
git merge github/feature/web-ui
git push origin feature/web-ui
```

### CI（GitHub Actions）

| Workflow | 환경 | 내용 |
|----------|------|------|
| `desktop-ci` | Windows | .NET 8 restore, build, test |
| `web-ui-ci` | Node | `package.json` 존재 시 활성화 |
