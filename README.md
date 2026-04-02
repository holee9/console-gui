# Console-GUI

HnVue - Medical Diagnostic X-Ray Console Software

## Overview

HnVue Console SW는 자사 FPD(Flat Panel Detector)에 번들되는 X-ray 촬영 콘솔 소프트웨어이다.

현재 외부 구매/외주 중인 Console SW(IMFOU feel-DRCS OEM + HnVue 외주 개발)를 **자체 개발로 내재화**하는 것이 본 프로젝트의 목표이다.

### 내재화 단계

| Phase | 예상 공수 (SW 2명 기준) | 범위 | 영상처리 |
|-------|------------------------|------|---------|
| Phase 1 | ~24-36 MM | 콘솔 프레임워크 자체 개발, feel-DRCS 핵심 기능 대체 | 외부 SDK 구매 연동 |
| Phase 2 | ~18-24 MM (인력 보강 전제) | 업계 표준 달성 (Auto Stitching, 다국어, Reject Analysis 등) | 자체 엔진 내재화 |
| Phase 3 | TBD | AI/Cloud 등 고급 기능 | 자체 + AI 파트너십 |

### 기술 스택

| 계층 | 기술 |
|------|------|
| UI | WPF (.NET 8 LTS) |
| DICOM | fo-dicom 5.x (MIT) |
| 영상처리 | 외부 SDK (Phase 1) / 자체 (Phase 2) |
| DB | SQLite + EF Core |
| 로깅 | Serilog |
| 테스트 | xUnit + NSubstitute |

---

## Regulatory Standards

- IEC 62304 (Medical Device Software Lifecycle) - Class B
- IEC 62366-1 (Usability Engineering)
- ISO 14971 (Risk Management)
- FDA 21 CFR 820.30 (Design Controls)
- ISO 13485 (Quality Management)
- IEC 81001-5-1 (Cybersecurity Lifecycle)
- DICOM 3.0 / IHE SWF
- HIPAA / GDPR

---

## Documentation

### 인허가 산출물 커버리지 (software-templates 28종 기준)

> 참조 템플릿: [software-templates](https://github.com/holee9/software-templates.git) — 의료기기 SW 인허가 28종 문서 템플릿

| 구분 | 수량 | 상태 |
|------|:----:|------|
| 완전 대응 | 28종 | 신규 10종 + 기존 18종 보완 완료 |
| 교차 검증 완료 | 3건 | SDP 통합, CyberTest Retest, GSPR 매핑 |

#### 템플릿 28종 매핑 현황

| Template ID | 산출물명 | Console-GUI 문서 | 상태 |
|:-----------:|----------|-----------------|:----:|
| A01 | SW Development Plan (SDP) | DOC-003a (SDP-RC-001) | ✅ 보완 |
| A02 | SW Requirements Specification | DOC-005_SRS_v1.0 | ✅ |
| A03 | SW Architecture Design | DOC-006_SAD_v1.0 | ✅ |
| A04 | SOUP List | DOC-033_SOUP_Report_v1.0 | ✅ |
| A05 | Configuration Management Plan | **DOC-042_CMP_v1.0** | 🆕 신규 |
| A06 | SW Release Record | DOC-034_ReleaseDoc_v1.0 | ✅ |
| A07 | Source Code & Build Environment | **DOC-043_Build_Environment_v1.0** | 🆕 신규 |
| A08 | Known Anomaly List | **DOC-044_Known_Anomalies_v1.0** | 🆕 신규 |
| B01 | Integration Test Report | DOC-023_ITReport_v1.0 | ✅ |
| B02 | System Test Report | DOC-024_STReport_v1.0 | ✅ |
| B03 | Requirements Traceability Matrix | DOC-032_RTM_v1.0 | ✅ |
| B04 | Usability Engineering Summary | DOC-021 + DOC-028 | ✅ |
| B05 | Clinical Evaluation / Equivalence | DOC-029_CER_v1.0 | ✅ |
| C01 | SBOM | DOC-019_SBOM_v1.0 | ✅ |
| C02 | VEX Report | **DOC-045_VEX_Report_v1.0** | 🆕 신규 |
| C03 | Cybersecurity Controls (8대 통제) | **DOC-046_Security_Controls_v1.0** | 🆕 신규 |
| C04 | Threat Model | DOC-017_ThreatModel_v1.0 | ✅ |
| C05 | Cybersecurity Risk Assessment | **DOC-047_Security_Risk_Assessment_v1.0** | 🆕 신규 |
| C06 | Penetration Test & Retest | DOC-026_CyberTestReport_v1.0 | ✅ 보완 |
| C07 | Vulnerability Management Plan | **DOC-048_VMP_v1.0** | 🆕 신규 |
| D01 | Risk Management File (ISO 14971) | DOC-008 + DOC-009 + DOC-010 | ✅ |
| D02 | IEC 81001-5-1 Compliance | **DOC-049_IEC81001_Compliance_v1.0** | 🆕 신규 |
| E01 | Predicate / SE Comparison | **DOC-050_Predicate_Comparison_v1.0** | 🆕 신규 |
| E02 | Labeling & IFU | DOC-040_IFU_v1.0 | ✅ |
| E03 | GSPR Checklist (EU MDR) | **DOC-052_GSPR_Checklist_v1.0** | 🆕 신규 |
| E04 | eSTAR v6.1 Submission Guide | DOC-036_510k_eSTAR_v1.0 | ✅ |
| F01 | Clinical Evaluation Report | DOC-029_CER_v1.0 | ✅ |
| F02 | PMS / PMCF Package | **DOC-051_PMS_PMCF_v1.0** | 🆕 신규 |

> 🆕 = 2026-03-31 신규 작성 | ✅ 보완 = 교차 검증 후 섹션 추가/수정

---

### 전체 문서 목록

#### Planning (기획)

| Doc ID | Document | Version | Path |
|--------|----------|---------|------|
| DOC-001 | MRD (Market Requirements) | **v3.0** | `docs/planning/DOC-001_MRD_v3.0.md` |
| DOC-002 | PRD (Product Requirements) | **v2.0** | `docs/planning/DOC-002_PRD_v2.0.md` |
| DOC-004 | FRS (Functional Requirements) | v1.0 | `docs/planning/DOC-004_FRS_v1.0.md` |
| DOC-005 | SRS (Software Requirements) | v1.0 | `docs/planning/DOC-005_SRS_v1.0.md` |
| DOC-006 | SAD (Software Architecture) | v1.0 | `docs/planning/DOC-006_SAD_v1.0.md` |
| DOC-007 | SDS (Software Design) | v1.0 | `docs/planning/DOC-007_SDS_v1.0.md` |
| STRATEGY-001 | Company Positioning | v2.0 | `docs/planning/research/STRATEGY-001_Company_Positioning_v2.0.md` |

#### Management (관리)

| Doc ID | Document | Version | Path |
|--------|----------|---------|------|
| DMP-001 | Document Master Plan | v1.0 | `docs/management/DMP-001_DMP_v1.0.md` |
| DOC-003 | SW Development Guideline | v1.0 | `docs/management/DOC-003_SW_Development_Guideline_v1.0.md` |
| DOC-003a | SW Development Procedure (SDP) | v1.1 | `docs/management/DOC-003a_SW_Development_Procedure_v1.0.md` |
| DOC-016 | Cybersecurity Management Plan | v1.0 | `docs/management/DOC-016_Cybersecurity_Plan_v1.0.md` |
| DOC-041 | PM Plan | v1.0 | `docs/management/DOC-041_PM_Plan_v1.0.md` |
| DOC-042 | Configuration Management Plan 🆕 | v1.0 | `docs/management/DOC-042_CMP_v1.0.md` |
| DOC-043 | Source Code & Build Environment 🆕 | v1.0 | `docs/management/DOC-043_Build_Environment_v1.0.md` |
| DOC-044 | Known Anomaly List 🆕 | v1.0 | `docs/management/DOC-044_Known_Anomalies_v1.0.md` |
| WBS-001 | Work Breakdown Structure | v1.0 | `docs/management/WBS-001_WBS_v1.0.md` |

#### Risk (위험관리)

| Doc ID | Document | Version | Path |
|--------|----------|---------|------|
| DOC-008 | Risk Management Plan | v1.0 | `docs/risk/DOC-008_Risk_Management_Plan_v1.0.md` |
| DOC-009 | FMEA | v1.0 | `docs/risk/DOC-009_FMEA_v1.0.md` |
| DOC-010 | Risk Management Report | v1.0 | `docs/risk/DOC-010_RMR_v1.0.md` |
| DOC-017 | Threat Model (STRIDE) | v1.0 | `docs/risk/DOC-017_ThreatModel_v1.0.md` |
| DOC-047 | Cybersecurity Risk Assessment 🆕 | v1.0 | `docs/risk/DOC-047_Security_Risk_Assessment_v1.0.md` |

#### Testing (시험)

| Doc ID | Document | Version | Path |
|--------|----------|---------|------|
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
|--------|----------|---------|------|
| DOC-011 | V&V Master Plan | v1.0 | `docs/verification/DOC-011_VV_Master_Plan_v1.0.md` |
| DOC-015 | Validation Plan | v1.0 | `docs/verification/DOC-015_ValidationPlan_v1.0.md` |
| DOC-025 | V&V Summary | v1.0 | `docs/verification/DOC-025_VVSummary_v1.0.md` |
| DOC-029 | Clinical Evaluation Report | v1.0 | `docs/verification/DOC-029_CER_v1.0.md` |
| DOC-032 | RTM (Traceability Matrix) | v1.0 | `docs/verification/DOC-032_RTM_v1.0.md` |
| DOC-033 | SOUP Report | v1.0 | `docs/verification/DOC-033_SOUP_Report_v1.0.md` |

#### Regulatory (인허가)

| Doc ID | Document | Version | Path |
|--------|----------|---------|------|
| DOC-019 | SBOM | v1.0 | `docs/regulatory/DOC-019_SBOM_v1.0.md` |
| DOC-020 | Clinical Evaluation Plan | v1.0 | `docs/regulatory/DOC-020_Clinical_Evaluation_Plan_v1.0.md` |
| DOC-034 | Release Document | v1.0 | `docs/regulatory/DOC-034_ReleaseDoc_v1.0.md` |
| DOC-035 | DHF (Design History File) | v1.0 | `docs/regulatory/DOC-035_DHF_v1.0.md` |
| DOC-036 | 510(k) eSTAR | v1.0 | `docs/regulatory/DOC-036_510k_eSTAR_v1.0.md` |
| DOC-037 | CE Technical Documentation | v1.0 | `docs/regulatory/DOC-037_CE_TechDoc_v1.0.md` |
| DOC-038 | DICOM Conformance | v1.0 | `docs/regulatory/DOC-038_DICOM_Conformance_v1.0.md` |
| DOC-039 | KFDA Submission | v1.0 | `docs/regulatory/DOC-039_KFDA_v1.0.md` |
| DOC-040 | IFU (Instructions for Use) | v1.0 | `docs/regulatory/DOC-040_IFU_v1.0.md` |
| DOC-045 | VEX Report 🆕 | v1.0 | `docs/regulatory/DOC-045_VEX_Report_v1.0.md` |
| DOC-046 | Security Controls (8대 통제) 🆕 | v1.0 | `docs/regulatory/DOC-046_Security_Controls_v1.0.md` |
| DOC-048 | Vulnerability Management Plan 🆕 | v1.0 | `docs/regulatory/DOC-048_VMP_v1.0.md` |
| DOC-049 | IEC 81001-5-1 Compliance 🆕 | v1.0 | `docs/regulatory/DOC-049_IEC81001_Compliance_v1.0.md` |
| DOC-050 | Predicate Comparison 🆕 | v1.0 | `docs/regulatory/DOC-050_Predicate_Comparison_v1.0.md` |
| DOC-051 | PMS/PMCF Package 🆕 | v1.0 | `docs/regulatory/DOC-051_PMS_PMCF_v1.0.md` |
| DOC-052 | GSPR Checklist (EU MDR) 🆕 | v1.0 | `docs/regulatory/DOC-052_GSPR_Checklist_v1.0.md` |

---

### 2026-03-31 산출물 정비 요약

#### 신규 작성 (10종)

| 문서 | 템플릿 매핑 | 필수 여부 | 설명 |
|------|:-----------:|:---------:|------|
| DOC-042 CMP | A05 | ✅ FDA/EU/MFDS | 형상관리 계획서 — Git Flow, CI 목록, 변경 통제, 베이스라인, 감사 |
| DOC-043 Build | A07 | ✅ FDA/MFDS | 빌드 환경 기록 — .NET 8 SDK, WiX, NuGet locked-mode, Docker 보존 |
| DOC-044 Known Anomalies | A08 | ✅ FDA/EU/MFDS | 알려진 결함 목록 — 심각도 분류, SOUP 이슈 모니터링, No Known 선언란 |
| DOC-045 VEX | C02 | ⭐ FDA/EU | VEX 리포트 — SBOM 38종 연계, CVE 분석 우선순위, CycloneDX 1.5 |
| DOC-046 Security Controls | C03 | ⭐ FDA/EU | 8대 보안 통제 명세 — FDA Appendix 1 매핑, IEC 81001-5-1 연계 |
| DOC-047 Security Risk Assessment | C05 | ✅ FDA/EU | 보안 위험 평가 — STRIDE 28개 위협 전수 매핑, ISO 14971 통합 |
| DOC-048 VMP | C07 | ✅ FDA/EU | 취약점 관리 계획 — §524B 준수, CVD 절차, 3개 시장 보고 기준 |
| DOC-049 IEC 81001 | D02 | ⭐ FDA/EU | IEC 81001-5-1 조항별 준수 매핑 — 사이버보안 전 생명주기 |
| DOC-050 Predicate | E01 | ✅ FDA | Predicate 비교표 — SE 비교 프레임워크, Intended Use, 기술 특성 16항목 |
| DOC-051 PMS/PMCF | F02 | ✅ EU | 시판후 감시/임상 추적 — PMS KPI, PMCF 질문 6개, PSUR 템플릿 |
| DOC-052 GSPR | E03 | ✅ EU | GSPR 체크리스트 — Annex I 전체 23개 조항 매핑, §21 사이버보안 포함 |

#### 기존 문서 보완 (2건)

| 문서 | 보완 내용 |
|------|----------|
| DOC-003a SDP | v1.0→v1.1: IEC 62304 §5.1 ID 병기, 마일스톤 일정표 추가, 개발 도구 버전 기재 |
| DOC-026 CyberTestReport | v1.0→v1.1: Retest 기록 양식, Production-Equivalent 환경 명세, Low 취약점 수용 근거, 8대 통제 검증 매트릭스, 잔여 위험 선언 추가 |

#### 교차 검증 결과

| 검증 항목 | 결론 | 조치 |
|-----------|------|------|
| SDP 통합 필요 여부 | DOC-003a 단독으로 충분 | 마일스톤/도구 버전 보완 (완료) |
| CyberTest Retest | 30% 커버리지 → 보완 필요 | Retest 5개 섹션 추가 (완료) |
| GSPR 매핑 | 8/23 조항만 커버 → 별도 문서 필요 | DOC-052 신규 작성 (완료) |

---

### 개발 전 Placeholder 안내

신규 작성 문서들은 **개발 착수 전 계획서/사양서 수준**으로 작성되어 있다. 개발 완료 후 다음 항목들을 실제 값으로 채워야 한다:

- `[TBD - 개발 완료 후 작성]` — 빌드 해시, 테스트 결과, 릴리즈 버전 등
- `[작성 필요]` — 실제 데이터 수집 후 기입
- `[TBD]` — Predicate 510(k) 번호, NB 지정, 인증서 번호 등

---

### MRD v3.0 Priority Summary (4-Tier)

| Tier | Count | 의미 | Phase |
|------|:-----:|------|-------|
| Tier 1 (인허가 필수) | 13 | MFDS/FDA/IEC 필수 | Phase 1 |
| Tier 2 (시장 진입 필수) | 18 | feel-DRCS 동등 + 고객 최소 기대 | Phase 1 |
| Tier 3 (있으면 좋고) | 25 | 경쟁 차별화, EConsole1 미포함 | Phase 2+ |
| Tier 4 (비현실적) | 12 | 2명 조직 불가, AI/Cloud 등 | Phase 3+ |
| 제외 | 4 | v2.0에서 이미 제외 | - |
| **합계** | **72** | **Phase 1 = Tier 1+2 = 31개** | |

---

### Research Documents

#### Strategy

| Document | Description |
|----------|-------------|
| STRATEGY-001 v2.0 | HnVue Console SW internalization strategy |
| STRATEGY-001 v1.0 | Initial strategy (superseded) |
| FPD Console SW Market Research | FPD console SW buy/build/open-source analysis |
| Market Research (Console) | X-ray console software competitive landscape |
| Market Research (Imaging) | X-ray imaging software market data |

#### Cybersecurity Research (사이버보안 딥리서치 시리즈)

| Doc ID | Document | Version | Path | Description |
|--------|----------|---------|------|-------------|
| CYBERSEC-001 | 사이버보안 자체 검증 가이드 | v1.0 | `docs/planning/research/CYBERSEC-001_Self_Assessment_Guide_v1.0.md` | 29개 무료 도구, 4단계 파이프라인, 의사결정 매트릭스 (1,079줄) |
| CYBERSEC-002 | 침투 테스트 독립성 & 기술 전문성 충족 가이드 | v1.0 | `docs/planning/research/CYBERSEC-002_Independence_Expertise_Guide_v1.0.md` | FDA 원문 팩트 기반, IEC 62443-4-1 독립성 3등급, 자격증 16종, 무료 교육 13종, 리포트 템플릿 |
| CYBERSEC-003 | 한국 내 최소비용 침투 테스트 위탁 가이드 | v1.1 | `docs/planning/research/CYBERSEC-003_Korea_Pentest_Outsourcing_Guide_v1.1.md` | 공인기관 4곳(KTL/KTC/KTR/넴코), 전문업체 18곳, 정부 무료 프로그램 4종, 견적 비교 |
| CYBERSEC-004 | 공인기관 의뢰 전 내부 자체평가 가이드 | v1.0 | `docs/planning/research/CYBERSEC-004_Internal_PreAssessment_Guide_v1.0.md` | 31개 무료 도구, 8주 Phase별 실행 계획, 31개 자체점검 체크리스트, MFDS/IG-NB/FDA 대응 |

> 사이버보안 리서치는 FDA §524B, IEC 81001-5-1, MFDS 2024 개정 가이드라인을 기준으로 팩트 검증된 내용입니다.
>
> **권장 전략 A (공인기관 위탁 — 추천):**
> CYBERSEC-004(자체평가 8주) → CYBERSEC-003(KTL 공인시험 의뢰) → 공인 성적서로 독립성/전문성 자동 충족
>
> **권장 전략 B (자체/프리랜서 수행 — 비용 절감):**
> CYBERSEC-004(자체평가 8주) → CYBERSEC-002(독립성/전문성 입증 방법 참조) → 자체 리포트 작성
>
> 공인기관 시험 시 CYBERSEC-002는 참고용이며, 별도 독립성 입증 절차는 불필요합니다.

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
