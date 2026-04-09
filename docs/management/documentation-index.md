# HnVue 문서 체계

> 원본: README.md "문서 체계" + "개발 운영 전략" + "프로젝트 분석 문서" + "참고 자료 및 링크" 섹션에서 분리 (2026-04-09)

## 문서 분류

문서는 7개 주요 분류와 `docs/` 루트 기준 자료로 관리됩니다. 자동 동기화 스크립트 (`scripts/sync_docs.py`)로 버전 일관성을 유지합니다.

### 기획 문서 (`docs/planning/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| **DOC-001** | MRD (시장 요구사항) -- 36사 딥리서치 기반, 92개 MR | **v4.0** |
| DOC-001a | MR Detailed Spec -- Tier 1 (인허가 필수 13개) | v1.0 |
| DOC-001b | MR Detailed Spec -- Tier 2/3/4 (시장 진입 + 차별화) | v1.0 |
| **DOC-002** | PRD (제품 요구사항) -- MRD v4.0 연계, 17개 PR 추가 | **v3.0** |
| **DOC-004** | FRS (기능 요구사항) | v2.0 |
| **DOC-005** | SRS (소프트웨어 요구사항) | v2.0 |
| **DOC-006** | SAD (소프트웨어 아키텍처 설계) | v2.0 |
| **DOC-007** | SDS (소프트웨어 상세 설계) | v2.0 |

### 관리 문서 (`docs/management/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DMP-001 | DMP (개발 관리 계획) | v2.0 |
| DOC-003 | SW Development Guideline | v1.0 |
| DOC-003a | SDP (소프트웨어 개발 절차서) | v2.0 |
| DOC-016 | Cybersecurity Plan | v1.0 |
| DOC-041 | PM Plan | v1.0 |
| DOC-042 | CMP (형상관리 계획) -- Review 상태, 도구 확정 | v1.0 |
| DOC-043 | Build Environment (28개 프로젝트) | v1.0 |
| DOC-044 | Known Anomalies | v1.0 |
| WBS-001 | WBS (작업 분해도) | v2.0 |

### 위험 관리 (`docs/risk/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-008 | RMP (위험 관리 계획, ISO 14971) | v1.0 |
| DOC-009 | FMEA (고장 모드 영향 분석) | v1.0 |
| DOC-010 | RMR (위험 관리 보고서) | v1.0 |
| DOC-017 | STRIDE (위협 모델링) | v1.0 |
| DOC-047 | Security Risk Assessment | v1.0 |

### 검증 문서 (`docs/verification/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-032 | RTM (추적성 매트릭스) | v2.0 |
| DOC-011 | V&V Master Plan | v1.0 |
| DOC-015 | Validation Plan | v1.0 |
| DOC-025 | V&V Summary | v1.0 |
| DOC-029 | CER (임상 평가 보고서) | v1.0 |
| DOC-033 | SOUP Report | v1.0 |
| CVR-001 | Cross Verification Report | v1.0 |
| **CVR-002** | **MRD/PRD 교차검증 + 36사 딥리서치 갭 분석** | **v1.0** |

### 시험 문서 (`docs/testing/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-012 | Unit Test Plan | v2.0 |
| DOC-013 | Integration Test Plan | v2.0 |
| DOC-014 | System Test Plan | v2.0 |
| DOC-018 | Cybersecurity Test Plan | v2.0 |
| DOC-021 | Usability File | v2.0 |
| DOC-022 | UT Report | v1.0 |
| DOC-023 | IT Report | v1.0 |
| DOC-024 | ST Report | v1.0 |
| DOC-026 | Cyber Test Report | v1.0 |
| DOC-027 | Performance Report | v1.0 |
| DOC-028 | Usability Test Report | v1.0 |
| DOC-030 | QA Test Plan | v1.0 |
| DOC-031 | QA Verification | v1.0 |

### 인허가 문서 (`docs/regulatory/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-019 | SBOM (CycloneDX) | v1.0 |
| DOC-020 | Clinical Evaluation Plan | v1.0 |
| DOC-034 | Release Documentation | v1.0 |
| DOC-035 | DHF (Design History File) | v1.0 |
| DOC-036 | 510(k) eSTAR | v2.0 |
| DOC-037 | CE Technical Documentation | v1.0 |
| DOC-038 | DICOM Conformance Statement | v1.0 |
| DOC-039 | MFDS (식약처) 기술문서 | v1.0 |
| DOC-040 | IFU (사용 설명서) | v1.0 |
| DOC-045 | VEX Report (SBOM 취약점) | v1.0 |
| DOC-046 | Security Controls | v1.0 |
| DOC-048 | VMP (취약점 관리 계획) | v1.0 |
| DOC-049 | IEC 81001-5-1 Compliance | v1.0 |
| DOC-050 | Predicate Comparison | v1.0 |
| DOC-051 | PMS/PMCF Plan | v1.0 |
| DOC-052 | GSPR Checklist | v1.0 |

### 리서치 문서 (`docs/planning/research/`)

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| GENERATOR-001 | X-ray Generator Communication Protocol Guide | v1.0 |
| DICOM-001 | fo-dicom Implementation Guide | v1.0 |
| CYBERSEC-001~004 | 사이버보안 자가평가/외부시험 가이드 | v1.0~v1.1 |
| **UI-ARCH-001** | **GUI Replaceable Architecture Research Report** | **v1.0** |

---

## 기존 HnVUE 제품 원본 문서 (`docs/` 루트)

| 파일 | 역할 |
|------|------|
| `★HnVUE UI 변경 최종안_251118.pptx` | 기존 제품 UI 최종 설계안 -- WPF 화면 구현의 기준 |
| `Instructions for Use(EN) HnVUE 250714.docx` | 기존 제품 공식 IFU -- Gap 분석/사양서 작성의 기준 |
| `3. [HnVUE] Performance Test Report.docx` | 기존 제품 성능 시험 보고서 -- 신규 성능 기준 |
| `API_MANUAL_241206.pdf` | FPD SDK API 매뉴얼 -- `HnVue.Imaging` 하드웨어 연동 참조 |
| `hnvue_abyz_plan.pptx` | 전략/계획 자료 |

---

## 개발 운영 전략

팀 기반 Worktree 분리 개발 체계로 운영됩니다. 3개 구현팀 + Coordinator + QA + RA = 6개 워크트리.

| 에이전트 | 팀 | 담당 모듈 |
|---------|-----|----------|
| `hnvue-infra` | Team A | Common, Data, Security, SystemAdmin, Update |
| `hnvue-medical` | Team B | Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning |
| `hnvue-ui` | Design | UI Views, Styles, Themes, Components, DesignTime |
| `hnvue-coordinator` | Coordinator | UI.Contracts, UI.ViewModels, App (DI) |
| `hnvue-qa` | QA | Coverage, StyleCop, NetArchTest, Stryker, Release |
| `hnvue-ra` | RA | IEC 62304, SBOM, RTM, FMEA, FDA/CE/KFDA |

관련 문서:
- [운영 전략 가이드](../OPERATIONS.md)
- [QA 릴리즈 기준](../OPERATIONS.md#5-qa-게이트-및-릴리즈-기준)
- [RA 문서 절차](../OPERATIONS.md#6-ra-문서-유지보수-절차)

---

## 프로젝트 분석 문서

| 문서 | 내용 |
|------|------|
| **ANALYSIS-001** | Phase 1 현황 분석 -- 1차 릴리즈 준비도 평가, Critical Blockers |
| **ANALYSIS-002** | 내재화 개발 컨텍스트 -- 기존 HnVUE 관계 정의, Gap 분석 계획 |
| **STRATEGY-002** | 개발 작업 분류 -- 단독 vs 병렬 Worktree 개발, Wave 구조 |

---

## 참고 자료 및 링크

### 표준 및 규제

- [IEC 62304:2015+A1](https://www.iec.ch/webstore/publication/61997) -- 의료 소프트웨어 수명주기
- [FDA 21 CFR 820.30](https://www.ecfr.gov/current/title-21/section-820.30) -- Design Controls
- [DICOM Standard](https://www.dicomstandard.org/) -- 의료 영상 통신
- [OWASP Top 10](https://owasp.org/www-project-top-ten/) -- 보안 가이드

### 라이브러리 및 프레임워크

- [fo-dicom](https://github.com/fo-dicom/fo-dicom) -- C# DICOM 라이브러리
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) -- ORM
- [xUnit.net](https://xunit.net/) -- 테스트 프레임워크
- [MahApps.Metro](https://mahapps.com/) -- WPF 테마

### 저장소

- Gitea (사내): `http://10.11.1.40:7001/DR_RnD/Console-GUI`
- GitHub (미러): `https://github.com/holee9/console-gui`

---

문서 최종 업데이트: 2026-04-09
