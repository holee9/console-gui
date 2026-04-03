# Console-GUI

HnVue - Medical Diagnostic X-Ray Console Software

## Overview

HnVue Console SW는 자사 FPD(Flat Panel Detector)에 번들되는 X-ray 촬영 콘솔 소프트웨어이다.

현재 외부 구매/외주 중인 Console SW(IMFOU feel-DRCS OEM + HnVue 외주 개발)를 **자체 개발로 내재화**하는 것이 본 프로젝트의 목표이다.

### 내재화 단계

| Phase | 예상 공수 (SW 2명 기준) | 범위 | 영상처리 |
|-------|------------------------|------|---------| 
| Phase 1 | ~24-36 MM | Tier 1 + Tier 2 (31개 MR), feel-DRCS 핵심 기능 대체 | 외부 SDK 구매 연동 |
| Phase 2 | ~18-24 MM (인력 보강 전제) | Tier 3 (25개 MR), 업계 표준 달성 | 자체 엔진 내재화 |
| Phase 3 | TBD | Tier 4 (12개 MR), AI/Cloud 등 고급 기능 | 자체 + AI 파트너십 |

### 기술 스택

| 계층 | 기술 |
|------|------|
| UI | WPF (.NET 8 LTS) |
| DICOM | fo-dicom 5.x (MIT) |
| 영상처리 | 외부 SDK (Phase 1) / 자체 (Phase 2) |
| DB | SQLite + EF Core (SQLCipher AES-256 암호화) |
| 로깅 | Serilog (SHA-256 해시 체인, 365일 보관) |
| 테스트 | xUnit + NSubstitute |
| SBOM | CycloneDX for .NET |

---

## Regulatory Standards

- IEC 62304 (Medical Device Software Lifecycle) - **Class B**
- IEC 62366-1 (Usability Engineering)
- ISO 14971 (Risk Management)
- IEC 81001-5-1 (Cybersecurity Lifecycle)
- FDA 21 CFR 820.30 (Design Controls)
- FDA §524B (Cybersecurity — SBOM, CVD, Patch/Update)
- ISO 13485 (Quality Management)
- DICOM 3.0 / IHE SWF
- MFDS 2024 사이버보안 가이드라인 (35개 항목)

---

## MRD v3.0 — 4-Tier 우선순위 체계

벤치마크: [FDA K231225 (DRTECH EConsole1)](https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf), [feel-DRCS (K110033)](https://www.imfou.com/bbs/board.php?bo_table=product2&wr_id=8)

| Tier | Count | 의미 | Phase |
|------|:-----:|------|-------|
| Tier 1 (인허가 필수) | 13 | MFDS/FDA/IEC 필수 | Phase 1 |
| Tier 2 (시장 진입 필수) | 18 | feel-DRCS 동등 + 고객 최소 기대 | Phase 1 |
| Tier 3 (있으면 좋고) | 25 | 경쟁 차별화, EConsole1 미포함 | Phase 2+ |
| Tier 4 (비현실적) | 12 | 2명 조직 불가, AI/Cloud 등 | Phase 3+ |
| 제외 | 4 | v2.0에서 이미 제외 | - |
| **합계** | **72** | **Phase 1 = Tier 1+2 = 31개** | |

### 추적성 체인

```
MR (72개) → PR (65개) → SWR (176+개) → TC → HAZ
   MRD v3.0    PRD v2.0    FRS/SRS v2.0   RTM v2.0
```

### 완성도 (2026-04-03 기준)

| 항목 | 상태 |
|------|:---:|
| MR → PR 추적성 (30/30 Tier 1+2) | 100% |
| 28종 인허가 템플릿 커버리지 (27/28) | 96% |
| Mermaid 차트 classDef 통일 (231/231) | 100% |
| P1~P4 구 표기 완전 제거 | 100% |
| 용어 통일 (RadiConsole → HnVue) | 100% |

> 미비 1건: C06_PenTest (침투 테스트) — 외부 위탁 대기 (KTL 견적 요청 예정)

---

## Documentation

### 현행 문서 (Active)

아래 문서가 현재 유효한 최신 버전입니다.

#### Planning (기획)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DOC-001 | MRD (Market Requirements) | **v3.0** | `docs/planning/DOC-001_MRD_v3.0.md` |
| DOC-001a | MR 상세 설명서 — Tier 1 | v1.0 | `docs/planning/DOC-001a_MR_Detailed_Spec_Tier1.md` |
| DOC-001b | MR 상세 설명서 — Tier 2/3/4 | v1.0 | `docs/planning/DOC-001b_MR_Detailed_Spec_Tier2_3_4.md` |
| DOC-002 | PRD (Product Requirements) | **v2.0** | `docs/planning/DOC-002_PRD_v2.0.md` |
| DOC-004 | FRS (Functional Requirements) | **v2.0** | `docs/planning/DOC-004_FRS_v2.0.md` |
| DOC-005 | SRS (Software Requirements) | **v2.0** | `docs/planning/DOC-005_SRS_v2.0.md` |
| DOC-006 | SAD (Software Architecture) | v1.0 | `docs/planning/DOC-006_SAD_v1.0.md` |
| DOC-007 | SDS (Software Design) | v1.0 | `docs/planning/DOC-007_SDS_v1.0.md` |

#### Management (관리)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DMP-001 | Document Master Plan | v1.0 | `docs/management/DMP-001_DMP_v1.0.md` |
| DOC-003 | SW Development Guideline | v1.0 | `docs/management/DOC-003_SW_Development_Guideline_v1.0.md` |
| DOC-003a | SW Development Procedure (SDP) | v1.1 | `docs/management/DOC-003a_SW_Development_Procedure_v1.0.md` |
| DOC-016 | Cybersecurity Management Plan | v1.0 | `docs/management/DOC-016_Cybersecurity_Plan_v1.0.md` |
| DOC-041 | PM Plan | v1.0 | `docs/management/DOC-041_PM_Plan_v1.0.md` |
| DOC-042 | Configuration Management Plan | v1.0 | `docs/management/DOC-042_CMP_v1.0.md` |
| DOC-043 | Source Code & Build Environment | v1.0 | `docs/management/DOC-043_Build_Environment_v1.0.md` |
| DOC-044 | Known Anomaly List | v1.0 | `docs/management/DOC-044_Known_Anomalies_v1.0.md` |
| WBS-001 | Work Breakdown Structure | v1.0 | `docs/management/WBS-001_WBS_v1.0.md` |

#### Risk (위험관리)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DOC-008 | Risk Management Plan | v1.0 | `docs/risk/DOC-008_Risk_Management_Plan_v1.0.md` |
| DOC-009 | FMEA | v1.0 | `docs/risk/DOC-009_FMEA_v1.0.md` |
| DOC-010 | Risk Management Report | v1.0 | `docs/risk/DOC-010_RMR_v1.0.md` |
| DOC-017 | Threat Model (STRIDE) | v1.0 | `docs/risk/DOC-017_ThreatModel_v1.0.md` |
| DOC-047 | Cybersecurity Risk Assessment | v1.0 | `docs/risk/DOC-047_Security_Risk_Assessment_v1.0.md` |

#### Testing (시험)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DOC-012 | Unit Test Plan | v1.0 | `docs/testing/DOC-012_UnitTestPlan_v1.0.md` |
| DOC-013 | Integration Test Plan | v1.0 | `docs/testing/DOC-013_IntegTestPlan_v1.0.md` |
| DOC-014 | System Test Plan | v1.0 | `docs/testing/DOC-014_SystemTestPlan_v1.0.md` |
| DOC-018 | Cybersecurity Test Plan | v1.0 | `docs/testing/DOC-018_CyberTestPlan_v1.0.md` |
| DOC-021 | Usability Engineering File | v1.0 | `docs/testing/DOC-021_UsabilityFile_v1.0.md` |
| DOC-022 | Unit Test Report | v1.0 | `docs/testing/DOC-022_UTReport_v1.0.md` |
| DOC-023 | Integration Test Report | v1.0 | `docs/testing/DOC-023_ITReport_v1.0.md` |
| DOC-024 | System Test Report | v1.0 | `docs/testing/DOC-024_STReport_v1.0.md` |
| DOC-026 | Cybersecurity Test Report | v1.1 | `docs/testing/DOC-026_CyberTestReport_v1.0.md` |
| DOC-027 | Performance Test Report | v1.0 | `docs/testing/DOC-027_PerfReport_v1.0.md` |
| DOC-028 | Usability Test Report | v1.0 | `docs/testing/DOC-028_UsabilityTestReport_v1.0.md` |
| DOC-030 | QA Test Plan | v1.0 | `docs/testing/DOC-030_QA_Test_Plan_v1.0.md` |
| DOC-031 | QA Verification | v1.0 | `docs/testing/DOC-031_QAVerification_v1.0.md` |

#### Verification (검증)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DOC-011 | V&V Master Plan | v1.0 | `docs/verification/DOC-011_VV_Master_Plan_v1.0.md` |
| DOC-015 | Validation Plan | v1.0 | `docs/verification/DOC-015_ValidationPlan_v1.0.md` |
| DOC-025 | V&V Summary | v1.0 | `docs/verification/DOC-025_VVSummary_v1.0.md` |
| DOC-029 | Clinical Evaluation Report | v1.0 | `docs/verification/DOC-029_CER_v1.0.md` |
| DOC-032 | RTM (Traceability Matrix) | **v2.0** | `docs/verification/DOC-032_RTM_v2.0.md` |
| DOC-033 | SOUP Report | v1.0 | `docs/verification/DOC-033_SOUP_Report_v1.0.md` |

#### Regulatory (인허가)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| DOC-019 | SBOM | v1.0 | `docs/regulatory/DOC-019_SBOM_v1.0.md` |
| DOC-020 | Clinical Evaluation Plan | v1.0 | `docs/regulatory/DOC-020_Clinical_Evaluation_Plan_v1.0.md` |
| DOC-034 | Release Document | v1.0 | `docs/regulatory/DOC-034_ReleaseDoc_v1.0.md` |
| DOC-035 | DHF (Design History File) | v1.0 | `docs/regulatory/DOC-035_DHF_v1.0.md` |
| DOC-036 | 510(k) eSTAR | **v2.0** | `docs/regulatory/DOC-036_510k_eSTAR_v2.0.md` |
| DOC-037 | CE Technical Documentation | v1.0 | `docs/regulatory/DOC-037_CE_TechDoc_v1.0.md` |
| DOC-038 | DICOM Conformance | v1.0 | `docs/regulatory/DOC-038_DICOM_Conformance_v1.0.md` |
| DOC-039 | MFDS Submission | v1.0 | `docs/regulatory/DOC-039_KFDA_v1.0.md` |
| DOC-040 | IFU (Instructions for Use) | v1.0 | `docs/regulatory/DOC-040_IFU_v1.0.md` |
| DOC-045 | VEX Report | v1.0 | `docs/regulatory/DOC-045_VEX_Report_v1.0.md` |
| DOC-046 | Security Controls (8대 통제) | v1.1 | `docs/regulatory/DOC-046_Security_Controls_v1.0.md` |
| DOC-048 | Vulnerability Management Plan | v1.0 | `docs/regulatory/DOC-048_VMP_v1.0.md` |
| DOC-049 | IEC 81001-5-1 Compliance | v1.0 | `docs/regulatory/DOC-049_IEC81001_Compliance_v1.0.md` |
| DOC-050 | Predicate Comparison | v1.1 | `docs/regulatory/DOC-050_Predicate_Comparison_v1.0.md` |
| DOC-051 | PMS/PMCF Package | v1.0 | `docs/regulatory/DOC-051_PMS_PMCF_v1.0.md` |
| DOC-052 | GSPR Checklist (EU MDR) | v1.0 | `docs/regulatory/DOC-052_GSPR_Checklist_v1.0.md` |

---

### 28종 인허가 템플릿 매핑

> 참조 템플릿: [software-templates](https://github.com/holee9/software-templates.git) — 의료기기 SW 인허가 28종 문서 템플릿

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
| C06 | Penetration Test | 외부 위탁 대기 | 🔄 |
| C07 | Vulnerability Management Plan | DOC-048 | ✅ |
| D01 | Risk Management File | DOC-008 + 009 + 010 | ✅ |
| D02 | IEC 81001-5-1 Compliance | DOC-049 | ✅ |
| E01 | Predicate / SE Comparison | DOC-050 v1.1 | ✅ |
| E02 | Labeling & IFU | DOC-040 | ✅ |
| E03 | GSPR Checklist (EU MDR) | DOC-052 | ✅ |
| E04 | eSTAR v6.1 Submission Guide | DOC-036 v2.0 | ✅ |
| F01 | Clinical Evaluation Report | DOC-029 | ✅ |
| F02 | PMS / PMCF Package | DOC-051 | ✅ |

---

### Research Documents

#### Strategy

| Document | Version | Path | Description |
|----------|:-------:|------|-------------|
| Company Positioning | **v2.0** | `docs/planning/research/STRATEGY-001_Company_Positioning_v2.0.md` | HnVue 내재화 전략 (현행) |
| MRD 우선순위 재조정 제안서 | v1.0 | `docs/planning/research/MRD_Priority_Reassessment_Proposal.md` | FDA K231225 + feel-DRCS 벤치마크 기반 71개 MR 재분류 |
| FPD Console SW Market Research | - | `docs/planning/research/FPD_Console_SW_Market_Research.md` | FPD 콘솔 SW buy/build/open-source 분석 |
| Market Research (Console) | - | `docs/planning/research/market-research-xray-console-software.md` | X-ray 콘솔 SW 경쟁 환경 |
| Market Research (Imaging) | - | `docs/planning/research/market-research-xray-imaging-software.md` | X-ray 영상 SW 시장 데이터 |

#### Cybersecurity Research (사이버보안 딥리서치 시리즈)

| Doc ID | Document | Version | Path |
|--------|----------|:-------:|------|
| CYBERSEC-001 | 사이버보안 자체 검증 가이드 | v1.0 | `docs/planning/research/CYBERSEC-001_Self_Assessment_Guide_v1.0.md` |
| CYBERSEC-002 | 침투 테스트 독립성 & 기술 전문성 가이드 | v1.0 | `docs/planning/research/CYBERSEC-002_Independence_Expertise_Guide_v1.0.md` |
| CYBERSEC-003 | 한국 내 최소비용 위탁 가이드 | v1.1 | `docs/planning/research/CYBERSEC-003_Korea_Pentest_Outsourcing_Guide_v1.1.md` |
| CYBERSEC-004 | 공인기관 의뢰 전 자체평가 가이드 | v1.0 | `docs/planning/research/CYBERSEC-004_Internal_PreAssessment_Guide_v1.0.md` |

> **사이버보안 권장 전략 A (공인기관 위탁 — 추천):**
> CYBERSEC-004(자체평가 8주) → CYBERSEC-003(KTL 공인시험 의뢰) → 공인 성적서로 독립성/전문성 자동 충족
>
> **권장 전략 B (자체/프리랜서 수행 — 비용 절감):**
> CYBERSEC-004(자체평가 8주) → CYBERSEC-002(독립성/전문성 입증 방법 참조) → 자체 리포트 작성

---

### 구 버전 / 미사용 파일 (Archive)

아래 파일들은 **신 버전이 존재하여 더 이상 현행 문서가 아닙니다.** 이력 보존 목적으로 유지합니다.

| 파일 | 대체 문서 | 비고 |
|------|----------|------|
| `docs/archive/DOC-001_MRD_v1.0.md` | MRD v3.0 | 초기 버전 |
| `docs/archive/DOC-001_MRD_v2.0.md` | MRD v3.0 | P1~P4 체계 (폐기) |
| `docs/archive/DOC-002_PRD_v1.0.md` | PRD v2.0 | MR 추적성 없음 |
| `docs/archive/DOC-004_FRS_v1.0.md` | FRS v2.0 | Tier 미반영 |
| `docs/archive/DOC-005_SRS_v1.0.md` | SRS v2.0 | Tier 미반영 |
| `docs/archive/STRATEGY-001_Company_Positioning_v1.0.md` | STRATEGY-001 v2.0 | 초기 전략 |
| `docs/archive/DOC-036_510k_eSTAR_v1.0.md` | eSTAR v2.0 | 인시던트 대응 미포함 |
| `docs/archive/DOC-032_RTM_v1.0.md` | RTM v2.0 | P1~P4 체계, MR-072 없음 |
| `docs/archive/CVR-002_Final_CrossVerification.md` | CVR-002 v1.0 | 중복 파일 (파일명 불일치) |
| `docs/archive/medical-device-cybersecurity-template.md` | DOC-046, DOC-048 등 | 초기 참조 템플릿, 개별 문서로 분리 완료 |

---

### 개발 전 Placeholder 안내

문서들은 **개발 착수 전 계획서/사양서 수준**으로 작성되어 있다. 개발 완료 후 다음 항목들을 실제 값으로 채워야 한다:

- `[TBD - 개발 완료 후 작성]` — 빌드 해시, 테스트 결과, 릴리즈 버전 등
- `[작성 필요]` — 실제 데이터 수집 후 기입
- `[TBD]` — Predicate 510(k) 번호, NB 지정, 인증서 번호 등

---

## Sync from GitHub Mirror

Perplexity Computer에서 GitHub 미러에 작업한 내용을 사내 로컬 Git에 반영할 때 아래 명령어를 실행한다.

```bash
# 최초 1회: GitHub remote 추가 (이미 설정했으면 생략)
git remote add github https://github.com/holee9/console-gui.git

# 작업 반영 시 매번 실행
git fetch github
git merge github/main
git push origin main
```
