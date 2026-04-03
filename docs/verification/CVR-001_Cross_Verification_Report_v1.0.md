# HnVue Console SW
# 전체 문서 교차 검증 보고서 (Cross-Verification Audit Report)

---

| 항목 | 내용 |
|------|------|
| **보고서 ID** | CVR-XRAY-GUI-001 |
| **버전** | v1.0 |
| **작성일** | 2026년 3월 18일 |
| **검증 대상** | HnVue Console SW 규제 문서 전체 (13종) |
| **검증 기준** | IEC 62304:2006+AMD1:2015, ISO 14971:2019, FDA 21 CFR 820.30 |
| **작성자** | 품질보증팀 (Quality Assurance) |

---

## 목차

1. [Executive Summary (경영진 요약)](#1-executive-summary)
2. [검증 범위 및 방법론](#2-검증-범위-및-방법론)
3. [Check 1: ID 일관성 (ID Consistency)](#3-check-1-id-일관성)
4. [Check 2: 추적성 체인 완전성 (Traceability Chain)](#4-check-2-추적성-체인-완전성)
5. [Check 3: 규격 참조 일관성 (Standards References)](#5-check-3-규격-참조-일관성)
6. [Check 4: 문서 상호 참조 (Document Cross-References)](#6-check-4-문서-상호-참조)
7. [Check 5: 용어 일관성 (Terminology Consistency)](#7-check-5-용어-일관성)
8. [Check 6: 위험-요구사항 연결 (Risk-Requirement Linkage)](#8-check-6-위험-요구사항-연결)
9. [종합 준수 평가 (Overall Compliance Assessment)](#9-종합-준수-평가)
10. [개선 권고사항 (Recommendations)](#10-개선-권고사항)

---

## 1. Executive Summary

### 1.1 검증 결과 요약

총 6개 검증 카테고리에 걸쳐 13개 규제 문서의 교차 검증을 수행하였다. 발견된 불일치 사항 및 결함은 총 **21건**으로, 중요도(Severity)별로 분류하면 다음과 같다.

| 중요도 | 건수 | 내용 요약 |
|--------|------|-----------|
| 🔴 **Critical** | 3 | 문서 간 HAZ ID 체계 전면 불일치, SWR 번호 체계 불일치, PRD 참조 MRD 문서 ID 오류 |
| 🟠 **Major** | 8 | V&V Plan 참조 문서 ID 오류, Audit Log 보관 기간 상충, SWR 존재하지 않는 참조, PRD-FRS 간 SWR 매핑 불일치 등 |
| 🟡 **Minor** | 10 | IEC 62304 버전 표기 경미한 차이, Phase 개념 혼용, RC-32 타이핑 오류, MR-003 세분화 부재 등 |

### 1.2 카테고리별 통과/주의/실패 현황

| 검증 카테고리 | 상태 | 주요 발견 사항 수 |
|--------------|------|-----------------|
| Check 1: ID 일관성 | 🔴 **실패 (FAIL)** | 6건 (Critical 2 + Major 4) |
| Check 2: 추적성 체인 완전성 | 🟠 **주의 (WARNING)** | 4건 (Major 3 + Minor 1) |
| Check 3: 규격 참조 일관성 | 🟡 **통과 (PASS with Minor)** | 2건 (Minor 2) |
| Check 4: 문서 상호 참조 | 🟠 **주의 (WARNING)** | 4건 (Critical 1 + Major 2 + Minor 1) |
| Check 5: 용어 일관성 | 🟡 **통과 (PASS with Minor)** | 3건 (Minor 3) |
| Check 6: 위험-요구사항 연결 | 🔴 **실패 (FAIL)** | 4건 (Critical 1 + Major 2 + Minor 1) |

> **판정 기준**: 
> - 🔴 실패 (FAIL): Critical 또는 Major 불일치가 복수 존재하여 인허가 심사 시 문제 가능성 높음
> - 🟠 주의 (WARNING): Major 불일치가 존재하나 개별 문서 수준에서 수정 가능
> - 🟡 통과 (PASS with Minor): Minor 불일치만 존재하며 전반적 구조는 적합

---

## 2. 검증 범위 및 방법론

### 2.1 검증 대상 문서

| 번호 | 문서 ID | 문서명 | 버전 |
|------|---------|--------|------|
| 1 | MRD-XRAY-GUI-001 | Market Requirements Document (MRD) | v2.0 |
| 2 | PRD-XRAY-GUI-001 | Product Requirements Document (PRD) | v3.0 |
| 3 | FRS-XRAY-GUI-001 | Functional Requirements Specification (FRS) | v1.0 |
| 4 | WBS-001 | Work Breakdown Structure (WBS) | v4.0 |
| 5 | DMP-XRAY-GUI-001 | Document Management Plan (DMP) | v2.0 |
| 6 | DOC-RADI-GL-001 | Software Development Guideline | v1.0 |
| 7 | SDP-XRAY-GUI-001 | Software Development Plan (SDP) | v1.0 |
| 8 | VVP-XRAY-GUI-001 | Verification & Validation Plan (V&V Plan) | v1.0 |
| 9 | RMP-XRAY-GUI-001 | Risk Management Plan (RMP) | v1.0 |
| 10 | CMP-XRAY-GUI-001 | Cybersecurity Management Plan | v1.0 |
| 11 | CEP-XRAY-GUI-001 | Clinical Evaluation Plan (CEP) | v1.0 |
| 12 | QTP-XRAY-GUI-001 | QA Test Plan (QA Plan) | v1.0 |
| 13 | RTM-XRAY-GUI-001 | Requirements Traceability Matrix (RTM) | v1.0 |

### 2.2 검증 방법

- 각 문서의 ID 체계, 표준 참조, 상호 참조 항목을 텍스트 비교 분석
- MR-xxx, PR-xxx, SWR-xxx, HAZ-xxx, RC-xxx ID의 정의/참조 완전성 교차 확인
- 추적성 체인 (MR→PR→SWR→TC) 순방향 및 역방향 검증
- 표준 버전 문자열 정규화 비교 (IEC 62304, ISO 14971, IEC 62366 등)

---

## 3. Check 1: ID 일관성 (ID Consistency)

### 3.1 MR-xxx ID 일관성

**검증 결과: 🟡 주의**

- MRD에서 정의된 MR-001–MR-062는 PRD v3.0의 "출처 MR" 컬럼 및 RTM에 올바르게 참조됨
- 단, PRD v3.0의 표지 정보(§1.1)에 **"MRD 참조: MRD-XRAY-001 v1.2"** 라고 기재되어 있으나, 실제 MRD 문서 ID는 **MRD-XRAY-GUI-001 v2.0**임 → **문서 ID 불일치**
- RTM(§1.3 참조 문서)도 동일하게 **MRD-XRAY-001 v1.2** 로 잘못 기재됨

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-01 | 🔴 Critical | PRD §1.1, RTM §1.3 | PRD, RTM 모두 "MRD-XRAY-001 v1.2"를 참조하나 실제 MRD 문서 ID는 "MRD-XRAY-GUI-001 v2.0"임. 버전 및 문서 ID 동시 불일치. |

### 3.2 PR-xxx ID 일관성

**검증 결과: 🟡 경미한 불일치 (Minor)**

- PRD v3.0의 PR-PM-002 요구사항 테이블에는 파생 SWR로 **"SWR-PM-004, SWR-PM-005"** 가 명시됨
- 그러나 FRS에서 PR-PM-002에서 파생된 SWR은 **SWR-PM-010, SWR-PM-011, SWR-PM-012, SWR-PM-013** 으로 정의됨
- 또한, PRD에서 PR-PM-004의 파생 SWR로 **"SWR-PM-009, SWR-PM-010"** 이 기재되어 있으나, FRS에서 PR-PM-004는 SWR-PM-030–033에 매핑됨

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-02 | 🟠 Major | PRD §3(PR-PM-002 행), FRS §3(PR-PM-002 섹션) | PRD의 PR-PM-002 → SWR-PM-004, SWR-PM-005 매핑이 FRS의 실제 매핑(SWR-PM-010–013)과 불일치. 이전 버전(v2.0) SWR 번호가 미수정된 상태로 잔존. |
| F-03 | 🟠 Major | PRD §3(PR-PM-004 행), FRS §3(PR-PM-004 섹션) | PRD의 PR-PM-004 → SWR-PM-009, SWR-PM-010 매핑이 FRS의 실제 매핑(SWR-PM-030–033)과 불일치. |

### 3.3 SWR-xxx ID 일관성

**검증 결과: 🔴 Critical 불일치 발견**

FRS(v1.0)의 SWR 번호 체계와 QA Plan에서 참조하는 SWR 번호 체계가 전면 불일치한다.

| 도메인 | FRS SWR 번호 범위 | QA Plan SWR 번호 범위 | 비고 |
|--------|-------------------|----------------------|------|
| PM (Patient Management) | SWR-PM-001–053 | SWR-PM-001–020 | 일부 범위 불일치 |
| DM (Dose Management) | SWR-DM-040–055 | SWR-DM-001–010 | **전면 불일치** |
| NF (Non-Functional) | SWR-NF-CP-030–035 등 계층형 | SWR-NF-001–015 단순 번호 | **전면 불일치** |

- FRS는 PRD v3.0의 "v3.0 3단계 계층 ID 체계" 도입에 따라 SWR 번호가 40, 50, 70번대 등 점프된 번호를 사용
- QA Plan은 이전 버전(v2.0) 체계의 연속 번호(SWR-DM-001–010 등)를 그대로 사용

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-04 | 🔴 Critical | QA Plan 전체, FRS 전체 | QA Plan이 FRS와 완전히 다른 SWR ID 체계를 사용. QA Plan의 SWR-DM-001–010은 FRS에 존재하지 않음. FRS v3.0 번호 체계(SWR-DM-040–055)로 QA Plan 전면 개정 필요. |

### 3.4 HAZ-xxx ID 일관성

**검증 결과: 🔴 Critical 불일치 발견**

MRD, PRD의 HAZ ID 체계와 RMP, RTM의 HAZ ID 체계가 전면적으로 다르다.

| 문서 | HAZ ID 체계 | 예시 |
|------|------------|------|
| **MRD** | HAZ-카테고리-번호 (워크플로우/영상/선량/사이버 등) | HAZ-WF-001, HAZ-IP-001, HAZ-DM-001, HAZ-CS-001, HAZ-UX-001, HAZ-AI-001 등 |
| **RMP, RTM** | HAZ-카테고리-번호 (방사선/SW/데이터/보안 등) | HAZ-RAD-001, HAZ-SW-001, HAZ-DATA-001, HAZ-SEC-001, HAZ-UI-001 등 |
| **FRS, PRD** | HAZ 카테고리를 약어로 사용 (HAZ-RAD, HAZ-SW, HAZ-SEC, HAZ-DATA) | 구체 번호 없음, 카테고리 수준만 참조 |

MRD에서 정의된 HAZ-WF-001–005, HAZ-IP-001–002, HAZ-DM-001–003, HAZ-CS-001–006, HAZ-UX-001–003, HAZ-HW-001, HAZ-SF-001, HAZ-AI-001 등 22개 HAZ ID는 RMP나 RTM 어디에도 동일한 형태로 나타나지 않는다. RMP는 독자적인 HAZ-RAD-001–004, HAZ-SW-001–005, HAZ-DATA-001–004, HAZ-SEC-001–004, HAZ-UI-001–003, HAZ-NET-001–002 등 22개를 정의하였다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-05 | 🔴 Critical | MRD §10(위험 연결), RMP 전체, RTM §6 | MRD의 HAZ ID 체계(HAZ-WF/IP/DM/CS/UX/HW/SF/AI)와 RMP/RTM의 HAZ ID 체계(HAZ-RAD/SW/DATA/SEC/UI/NET)가 완전히 다른 분류 및 번호 체계를 사용함. 두 체계 간 공식 매핑 테이블 없음. |

### 3.5 RC-xxx ID 일관성

**검증 결과: 🟡 Minor 오류**

RMP와 RTM 모두 RC-001–RC-036을 정의하고 참조하며 전반적으로 일치하나, RMP 내에 **"RC-32"** 라는 형식 오류 표기가 발견된다(올바른 표기: RC-032).

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-06 | 🟡 Minor | RMP §7(위험 통제 요약 테이블), RTM | "RC-32" 타이핑 오류 — "RC-032"로 수정 필요. RTM에도 동일 오류 존재. |

---

## 4. Check 2: 추적성 체인 완전성 (Traceability Chain Completeness)

### 4.1 MR → PR 추적성

**검증 결과: 🟡 Minor 주의**

MRD에 정의된 MR-001–MR-062 (총 62개)는 PRD에서 PR-PM, PR-WF, PR-IP, PR-DM, PR-DC, PR-SA, PR-CS, PR-AI 등 다수의 PR로 파생된다. 전반적 매핑 구조는 적절하나 다음 사항이 발견된다.

- RTM §5의 Gap Analysis는 **"MR-003 단일 MR로 PR-IP-xxx 18개 항목이 파생"** 되는 과도한 세분화를 이미 인식하고 있으나, MRD 개정 없이 해결 미완료
- RTM이 MRD-XRAY-001 v1.2를 참조하나 실제 MRD는 v2.0 (F-01과 연계)

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-07 | 🟡 Minor | MRD §5(MR-003), PRD §3(PR-IP-020–037), RTM §5.3 | MR-003 단일 항목에서 18개 PR(PR-IP-020–037)이 파생되는 과도한 분해. IEC 62304 Design Control 심사 시 MR-PR 대응 관계 명확성 질의 받을 가능성 있음. RTM Gap Analysis에서 인식은 하였으나 MRD 세분화 조치 미완료. |

### 4.2 PR → SWR 추적성

**검증 결과: 🟠 Major 불일치**

PRD에서 일부 PR의 파생 SWR 번호가 FRS의 실제 정의와 불일치한다 (F-02, F-03). 추가로 다음 사항이 발견된다.

- PRD §3 테이블의 PR-PM-003 파생 SWR: **"SWR-PM-006, SWR-PM-007, SWR-PM-008"** — FRS에서 PR-PM-003은 실제 SWR-PM-020–024에 매핑됨
- PRD §3 테이블의 PR-PM-006 파생 SWR: **"SWR-PM-013, SWR-PM-014"** — FRS에서는 SWR-PM-050–053에 매핑됨

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-08 | 🟠 Major | PRD §3(PR-PM-003, PR-PM-006), FRS §3 | PRD에 명시된 파생 SWR(SWR-PM-006–008, SWR-PM-013–014)이 FRS 실제 SWR(SWR-PM-020–024, SWR-PM-050–053)과 불일치. PRD의 "파생 SWR" 컬럼 전반에 걸쳐 FRS v1.0 기준으로 재검토 필요. |

### 4.3 SWR → TC 추적성

**검증 결과: 🟠 Major 불일치**

QA Plan의 SWR 번호 체계 불일치(F-04)로 인해, QA Plan의 테스트 케이스와 FRS의 SWR 간 정상적인 TC 커버리지 확인이 불가능하다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-09 | 🟠 Major | QA Plan 전체 TC 테이블, FRS 전체 | QA Plan의 SWR 번호(SWR-DM-001–010, SWR-NF-001–015 등)가 FRS 정의 번호와 상이하여 TC→SWR 역추적 불가. FDA 21 CFR 820.30(f) Design Verification 심사 시 비준수 판정 위험. |

### 4.4 RTM 전체 체인 커버리지

**검증 결과: 🟡 주의**

RTM §5.3 Gap Analysis에서 스스로 인식한 Gap:
- SAD/SDS 미작성 (전체 88개 PR 해당)
- 테스트 케이스 미작성 (전체 88개 PR 해당)
- HAZ 미매핑 PR 다수 존재 (PR-IP-021–024 등)

이는 RTM이 Draft 상태임을 반영하나, 해결 일정 및 책임자가 "Phase 1 설계 단계에서 완성"으로 기재만 되어 있어 구체적 목표 일자 미설정이 문제이다.

---

## 5. Check 3: 규격 참조 일관성 (Standards References Consistency)

### 5.1 IEC 62304 버전 표기

**검증 결과: 🟡 Minor (표기 불일치)**

대부분 문서는 "IEC 62304:2006+AMD1:2015"를 사용하나, PRD, FRS, RTM에서는 "IEC 62304:2006+A1:2015" 라는 약식 표기를 사용한다. 내용상 동일한 규격이나 공식 표기 규칙("AMD" vs "A") 불통일.

| 문서 | IEC 62304 표기 방식 |
|------|-------------------|
| MRD, WBS, DMP, SDP, RMP, CyberPlan, DevGuideline, QAPlan | IEC 62304:2006+**AMD1**:2015 (공식 IEC 표기) |
| PRD, FRS, RTM | IEC 62304:2006+**A1**:2015 (약식 표기) |

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-10 | 🟡 Minor | PRD §표지, FRS §표지, RTM §표지 | IEC 62304 버전 표기가 "IEC 62304:2006+A1:2015"로 축약 사용. 공식 표기 "IEC 62304:2006+AMD1:2015"로 통일 권장. |

### 5.2 ISO 14971, ISO 13485, IEC 62366 버전 일관성

**검증 결과: 🟢 통과 (Pass)**

모든 문서에서 ISO 14971:2019, ISO 13485:2016, IEC 62366-1:2015+AMD1:2020으로 일관되게 참조됨. 충돌하는 버전 표기 없음.

### 5.3 FDA 규정 참조 일관성

**검증 결과: 🟢 통과 (Pass)**

FDA 21 CFR 820.30 (Design Controls), FDA Section 524B (Cybersecurity) 모두 일관되게 참조됨.

### 5.4 규격 표기 미세 차이

**검증 결과: 🟡 Minor**

- V&V Plan의 규격 표지에 IEC 62366 버전이 단순 "IEC 62366-1"로 표기되어 있고 AMD1:2020 버전이 누락된 경우 있음 (본문에는 올바르게 기재됨)

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-11 | 🟡 Minor | V&V Plan §표지 기준규격란 | "IEC 62366-1" 버전 표기가 "IEC 62366-1:2015+AMD1:2020"으로 완전히 기재되지 않음. 본문과 일치하도록 수정 필요. |

---

## 6. Check 4: 문서 상호 참조 (Document Cross-References)

### 6.1 V&V Plan의 참조 문서 ID 오류

**검증 결과: 🔴 Critical 오류**

V&V Plan(§1.3 참조 문서 테이블)에서 다음 문서 ID 및 버전이 실제 문서와 불일치한다.

| V&V Plan 기재 내용 | 실제 문서 |
|-------------------|----------|
| PRD-XRAY-**001** v3.0 | 실제 ID: **PRD-XRAY-GUI-001** v3.0 |
| FRS-XRAY-**001** v**2.1** | 실제 ID: **FRS-XRAY-GUI-001** v**1.0** |
| RMP-XRAY-**001** v**1.2** | 실제 ID: **RMP-XRAY-GUI-001** v**1.0** |

특히 FRS 버전이 **v2.1** 로 기재되어 있으나 실제 FRS는 **v1.0** 이다. 존재하지 않는 미래 버전을 참조하는 오류.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-12 | 🔴 Critical | V&V Plan §1.3 참조 문서 테이블 | FRS를 v2.1로 참조하나 실제 FRS는 v1.0. PRD, RMP도 문서 ID에 "GUI" 누락. FDA 인허가 심사 시 참조 문서 불일치로 제출 패키지 완결성 문제 발생 가능. |

### 6.2 DMP 문서 목록과 실제 문서 ID 정합성

**검증 결과: 🟡 Minor 주의**

DMP(v2.0) §4에 FRS가 **DOC-004** 로, SRS가 **DOC-005** 로 별도 문서로 관리되나, 실제 FRS(FRS-XRAY-GUI-001)는 SWR-xxx 체계를 직접 포함하여 FRS와 SRS를 통합한 문서로 운영 중이다. DMP의 "DOC-004 FRS" + "DOC-005 SRS" 이중 체계와 실제 단일 FRS 운영 간 구조 불일치.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-13 | 🟡 Minor | DMP §4(문서 목록 DOC-004, DOC-005) | DMP는 FRS(DOC-004)와 SRS(DOC-005)를 별도 문서로 계획하나, 실제 FRS(v1.0)가 IEC 62304 §5.2 SWR-xxx 체계를 통합하여 사실상 FRS+SRS 역할을 수행. DMP 또는 FRS 범위 정의 조정 필요. |

### 6.3 WBS와 DMP의 문서 목록 정합성

**검증 결과: 🟡 통과 (대체로 일치)**

WBS v4.0의 문서 계획(Gantt 및 DOC 참조)은 DMP v2.0의 문서 목록과 대체로 일치한다. WBS는 DOC-001–036 번호 체계를 참조하며 DMP와 동일 체계를 사용한다.

단, WBS 문서 참조 다이어그램에서 QA Plan (QTP-XRAY-GUI-001 / DOC-030)이 명시적으로 포함되지 않아 추적성 누락 가능성.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-14 | 🟡 Minor | WBS §추적성 다이어그램 | QA Plan(DOC-030)이 WBS 문서 연결 다이어그램에서 명시되지 않음. DOC-014(System Test Plan)와 구분하여 명시 필요. |

### 6.4 RTM 상위 문서 참조

**검증 결과: 🟠 Major**

RTM §1.1 및 §1.3에서 상위 문서를 **"MRD-XRAY-001 v1.2"** 로 기재하고 있으나 실제 MRD는 **"MRD-XRAY-GUI-001 v2.0"** 이다 (F-01과 동일 근원). RTM에서 역추적 기준점(Backward Traceability Anchor)으로 사용하는 MR ID들은 MRD v2.0 기준이지만, 참조 문서 정보가 v1.2로 남아 있어 법적 추적성 체계의 신뢰성을 저하시킨다.

---

## 7. Check 5: 용어 일관성 (Terminology Consistency)

### 7.1 제품명 (Product Name)

**검증 결과: 🟢 통과 (Pass)**

13개 문서 전체에서 **"HnVue Console SW"** 또는 **"HnVue"** 을 일관되게 사용. ™ 기호 포함 여부의 경미한 차이는 있으나 오해를 유발하는 다른 명칭 사용 없음.

### 7.2 SW 안전 등급 (SW Safety Class)

**검증 결과: 🟢 통과 (Pass)**

모든 13개 문서에서 **"IEC 62304 Class B"** 를 일관되게 사용.

### 7.3 Phase 정의 일관성

**검증 결과: 🟡 Minor 혼용**

두 가지 다른 "Phase" 개념이 문서 전체에 혼재한다.

| Phase 개념 | 사용 문서 | 정의 |
|-----------|---------|------|
| **제품 릴리스 Phase** | MRD, PRD, WBS, DMP | Phase 1(v1.0), Phase 1.5(v1.5), Phase 2(v2.0) — 제품 출시 단계 |
| **SW 개발 수명주기 Phase** | SDP | Phase 1(기획), Phase 2(요구사항 분석), Phase 3(설계), Phase 4(구현), Phase 5(검증), Phase 6(밸리데이션), Phase 7(릴리스) — IEC 62304 개발 활동 단계 |

SDP에서 "Phase 2: 요구사항 분석"이라는 표현은 PRD/WBS의 "Phase 2 = AI 통합 제품 버전"과 혼동될 수 있다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-15 | 🟡 Minor | SDP §5.3 (Phase 1–7 개발 활동), PRD §1.5 (Phase 1/1.5/2 릴리스 계획) | "Phase" 용어가 제품 릴리스 단계와 IEC 62304 개발 수명주기 단계 두 의미로 혼용됨. SDP의 개발 활동 단계를 "Stage" 또는 "Development Phase"로 구분하는 용어 정리 권장. |

### 7.4 HAZ 참조 표기 방식

**검증 결과: 🟡 Minor**

FRS와 PRD는 위험 참조 컬럼에 **"HAZ-RAD", "HAZ-SW", "HAZ-DATA", "HAZ-SEC"** 와 같이 카테고리 수준만 표기하나, RMP/RTM은 구체 번호(HAZ-RAD-001 등)까지 명시한다. 이는 FRS/PRD가 RMP의 HAZ 분류 체계를 차용하면서도 MRD의 상세 HAZ 번호를 무시하는 패턴으로 이어진다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-16 | 🟡 Minor | FRS 전체 "위험 참조" 컬럼, PRD 전체 "위험 참조" 컬럼 | FRS/PRD의 위험 참조가 카테고리 수준(HAZ-RAD 등)으로만 기재되어, 특정 HAZ ID(예: HAZ-RAD-001 vs HAZ-RAD-003)로의 정밀 역추적 불가. IEC 62304 §5.2와 ISO 14971 §6.2 연계 추적성을 위해 HAZ 번호 수준까지 명시 권장. |

---

## 8. Check 6: 위험-요구사항 연결 (Risk-Requirement Linkage)

### 8.1 HAZ → RC 연결 완전성

**검증 결과: 🟢 통과 (RMP 내부)**

RMP에서 정의된 모든 HAZ (HAZ-RAD-001–004, HAZ-SW-001–005, HAZ-DATA-001–004, HAZ-SEC-001–004, HAZ-UI-001–003, HAZ-NET-001–002, 총 22개)에 대해 RC-001–RC-036가 연결되어 있으며, 커버리지 100%이다.

### 8.2 RC → SWR 연결 — 존재하지 않는 SWR 참조

**검증 결과: 🔴 Critical**

RMP의 RC 테이블에서 연결된 SWR 중 **FRS에 정의되지 않은 SWR ID** 들이 다수 발견된다.

| RC ID | RMP 참조 SWR | FRS 존재 여부 | 비고 |
|-------|------------|--------------|------|
| RC-003 | SWR-UI-030 | ❌ 미존재 | FRS에 SWR-UI-xxx 도메인 없음 |
| RC-008 | SWR-PM-010 | ✅ 존재 | — |
| RC-009 | SWR-DC-015 | ❌ 미존재 | FRS의 DC 도메인은 SWR-DC-050번대 |
| RC-010 | SWR-UI-025 | ❌ 미존재 | SWR-UI 도메인 FRS에 없음 |
| RC-011 | SWR-IP-020 | ✅ 존재 | — |
| RC-012 | SWR-IP-025 | ✅ 존재 | — |
| RC-013 | SWR-DM-003 | ❌ 미존재 | FRS의 DM 도메인은 SWR-DM-040번대 |
| RC-014 | SWR-DM-005 | ❌ 미존재 | 동일 |
| RC-015 | SWR-WF-030 | ✅ 존재 | — |
| RC-016 | SWR-WF-031 | ✅ 존재 | — |
| RC-017 | SWR-SA-010 | ❌ 미존재 | FRS SA는 SWR-SA-060번대 |
| RC-018 | SWR-DC-010 | ❌ 미존재 | FRS DC는 SWR-DC-050번대 |
| RC-019 | SWR-CS-005 | ❌ 미존재 | FRS CS는 SWR-CS-070번대 |
| RC-020 | SWR-CS-010 | ❌ 미존재 | 동일 |
| RC-021 | SWR-SA-015 | ❌ 미존재 | FRS SA는 SWR-SA-060번대 |
| RC-022 | SWR-PM-020 | ✅ 존재 | — |
| RC-023 | SWR-CS-015 | ❌ 미존재 | — |
| RC-024 | SWR-CS-020 | ❌ 미존재 | — |
| RC-025 | SWR-CS-025 | ❌ 미존재 | — |
| RC-026 | SWR-CS-030 | ❌ 미존재 | — |
| RC-027 | SWR-CS-035 | ❌ 미존재 | — |
| RC-028 | SWR-CS-040 | ❌ 미존재 | — |
| RC-029 | SWR-UI-010 | ❌ 미존재 | SWR-UI 도메인 없음 |
| RC-030 | SWR-UI-015 | ❌ 미존재 | — |
| RC-031 | SWR-UI-040 | ❌ 미존재 | — |
| RC-032 | SWR-UI-041 | ❌ 미존재 | — |
| RC-033 | SWR-WF-040 | ❌ 미존재 | FRS WF 최대 SWR-WF-031 |
| RC-034 | SWR-UI-045 | ❌ 미존재 | — |
| RC-035 | SWR-WF-050 | ❌ 미존재 | FRS WF 최대 SWR-WF-031 |
| RC-036 | SWR-DC-020 | ❌ 미존재 | FRS DC는 SWR-DC-050번대 |
| RC-006 | SWR-SA-005 | ❌ 미존재 | FRS SA는 SWR-SA-060번대 |

**결론**: RC-001–036 중 FRS와 정확히 일치하는 SWR을 참조하는 RC는 소수에 불과하며, 대다수 RC가 FRS에 존재하지 않는 SWR을 참조한다. RMP가 이전 버전 SWR 체계를 기반으로 작성되어 FRS v1.0 체계로 업데이트되지 않은 것이 원인이다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-17 | 🔴 Critical | RMP §7(RC 테이블), FRS 전체 | RC의 70% 이상이 FRS에 존재하지 않는 SWR을 참조. SWR-UI-xxx, SWR-DC-010–020, SWR-CS-005–040, SWR-SA-005–015, SWR-DM-003–008, SWR-WF-040/050 모두 FRS에 미정의. ISO 14971 §6 위험 통제 효과성 검증 추적성 체인 단절. |

### 8.3 안전-Critical SWR의 TC 커버리지

**검증 결과: 🟠 Major**

SWR-PM-001–004, SWR-WF-015–017, SWR-WF-019–022 등 FRS에서 "Safety-related"로 분류된 SWR들에 대해 QA Plan이 올바른 SWR 번호로 TC를 매핑하지 못하고 있다(F-04의 연쇄 영향).

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-18 | 🟠 Major | QA Plan 전체, FRS Safety-related SWR | FRS의 Safety-related SWR에 대한 TC 커버리지가 SWR 번호 불일치로 인해 공식적으로 확인 불가. IEC 62304 §5.2와 ISO 14971 위험 통제 효과성 검증 연계 단절. |

### 8.4 MRD HAZ와 RMP HAZ 간 매핑 부재

**검증 결과: 🟠 Major**

MRD §10(위험 연결 매트릭스)에 정의된 HAZ-WF/IP/DM/CS 등과 RMP의 HAZ-RAD/SW/DATA/SEC 체계 간 공식 연결 매핑 테이블이 존재하지 않는다. ISO 14971에서 요구하는 위험 분석의 소급 추적성(Backward Traceability from RMP → Market Requirements)이 공식 문서화되지 않음.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-19 | 🟠 Major | MRD §10 위험연결, RMP 전체 | MRD HAZ 체계(HAZ-WF/IP/DM/CS 등)와 RMP HAZ 체계(HAZ-RAD/SW/DATA/SEC)를 연결하는 공식 매핑 테이블 없음. 두 체계가 실질적으로 동일한 위험을 다루나 공식 연결 부재. |

### 8.5 Audit Log 보관 기간 불일치

**검증 결과: 🟠 Major**

| 문서 | Audit Log 보관 기간 요구사항 |
|------|--------------------------|
| **MRD MR-035** | **최소 1년(12개월)** 이상 보관 |
| **PRD PR-SA-065** | 90일 이상 보관 |
| **PRD PR-NF-SC-045** | ≥90일 보관 |
| **FRS SWR-SA-073** | 90일 이상 보관 |

MRD의 시장 요구사항(MR-035)이 명시적으로 **"최소 1년"** 을 요구하고 있으나, 이를 구체화해야 하는 PRD 및 FRS는 **"90일"** 이라는 더 낮은 기준을 적용하고 있다. 이는 시장 요구사항 다운그레이드로, FDA Design Input → Design Output 검토 시 수용 기준 하향 문제로 지적될 수 있다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-20 | 🟠 Major | MRD MR-035, PRD PR-SA-065 및 PR-NF-SC-045, FRS SWR-SA-073 | MRD는 감사 로그 1년(12개월) 이상 보관을 요구하나, PRD/FRS는 90일 기준 적용. FDA Cybersecurity Guidance 및 HIPAA Audit Log 요건(최소 6년)과도 비교 검토 필요. PRD/FRS를 ≥1년으로 상향 조정하거나 MRD에 근거하여 90일 설정의 타당성 문서화 필요. |

### 8.6 SWR-PM-005 미정의

**검증 결과: 🟡 Minor**

PRD v3.0의 Appendix A(RTM)에서 MR-001 → PR-PM-002 → SWR-PM-004–005 매핑이 명시되어 있으나, FRS에서 SWR-PM-005는 정의되어 있지 않다 (SWR-PM-004 다음 SWR-PM-006부터 시작). PR-PM-002는 FRS에서 SWR-PM-010–013으로 분화되어 있다.

| 발견 ID | 심각도 | 발견 위치 | 내용 |
|---------|--------|----------|------|
| F-21 | 🟡 Minor | PRD Appendix A RTM, FRS §PR-PM-002 섹션 | PRD RTM에서 SWR-PM-005 참조하나 FRS에 SWR-PM-005 미정의. F-02 발견의 부수 영향. |

---

## 9. 종합 준수 평가 (Overall Compliance Assessment)

### 9.1 IEC 62304 준수 관점

| IEC 62304 조항 | 준수 상태 | 주요 결함 |
|--------------|---------|---------|
| §5.1 SW 개발 계획 | 🟢 적합 | SDP 및 DevGuideline이 §5.1 요건 충족 |
| §5.2 SW 요구사항 분석 | 🟠 부분 적합 | SWR ID 체계 불일치로 문서 간 추적성 단절 (F-04) |
| §5.7 SW 통합 테스트 | 🟠 부분 적합 | TC 번호 불일치로 SWR-TC 연결 확인 불가 (F-09) |
| §5.8 SW 시스템 테스트 | 🟠 부분 적합 | 동일 |

### 9.2 ISO 14971 준수 관점

| ISO 14971 조항 | 준수 상태 | 주요 결함 |
|--------------|---------|---------|
| §4 위험 관리 계획 | 🟢 적합 | RMP 체계 적절 |
| §5 위험 분석 | 🟡 주의 | MRD HAZ와 RMP HAZ 이중 체계 (F-05) |
| §6 위험 통제 | 🔴 불적합 | RC의 70%가 미정의 SWR 참조 (F-17) |
| §9 위험 통제 효과성 검증 | 🔴 불적합 | TC 추적성 단절로 효과성 확인 불가 (F-18) |

### 9.3 FDA 21 CFR 820.30 준수 관점

| FDA 820.30 조항 | 준수 상태 | 주요 결함 |
|--------------|---------|---------|
| §820.30(c) Design Input | 🟠 부분 적합 | MRD-PRD 참조 ID 불일치 (F-01) |
| §820.30(d) Design Output | 🟠 부분 적합 | PRD-FRS SWR 매핑 불일치 (F-02, F-03, F-08) |
| §820.30(f) Design Verification | 🔴 불적합 | TC-SWR 추적성 단절 (F-09) |
| §820.30(g) Design Validation | 🟡 부분 적합 | V&V Plan 참조 문서 ID 오류 (F-12) |

### 9.4 전체 발견 사항 요약

| 발견 ID | 심각도 | 카테고리 | 핵심 내용 |
|---------|--------|---------|---------|
| F-01 | 🔴 Critical | ID 일관성 | PRD/RTM에서 MRD 문서 ID 및 버전 오류 참조 |
| F-02 | 🟠 Major | ID 일관성 | PR-PM-002의 파생 SWR 번호 불일치 |
| F-03 | 🟠 Major | ID 일관성 | PR-PM-004의 파생 SWR 번호 불일치 |
| F-04 | 🔴 Critical | ID 일관성 / SWR | QA Plan과 FRS 간 SWR 번호 체계 전면 불일치 |
| F-05 | 🔴 Critical | ID 일관성 / HAZ | MRD와 RMP/RTM 간 HAZ ID 체계 전면 불일치 |
| F-06 | 🟡 Minor | ID 일관성 | RC-32 타이핑 오류 (→ RC-032) |
| F-07 | 🟡 Minor | 추적성 체인 | MR-003 단일 MR에서 18개 PR 과도한 파생 |
| F-08 | 🟠 Major | 추적성 체인 | PR-PM-003/006의 파생 SWR 불일치 |
| F-09 | 🟠 Major | 추적성 체인 | QA Plan TC → FRS SWR 역추적 불가 |
| F-10 | 🟡 Minor | 규격 참조 | IEC 62304 표기 "A1" vs "AMD1" 불통일 |
| F-11 | 🟡 Minor | 규격 참조 | V&V Plan IEC 62366 버전 미완전 기재 |
| F-12 | 🔴 Critical | 문서 상호 참조 | V&V Plan의 FRS 참조 버전 v2.1 (실제: v1.0) 및 ID 오류 |
| F-13 | 🟡 Minor | 문서 상호 참조 | DMP의 FRS+SRS 이중 체계 vs 실제 통합 FRS |
| F-14 | 🟡 Minor | 문서 상호 참조 | WBS 다이어그램에서 QA Plan 미포함 |
| F-15 | 🟡 Minor | 용어 일관성 | "Phase" 개념 혼용 (릴리스 단계 vs 개발 수명주기) |
| F-16 | 🟡 Minor | 용어 일관성 | FRS/PRD HAZ 참조가 카테고리 수준으로만 기재 |
| F-17 | 🔴 Critical | 위험-요구사항 | RMP RC의 70%가 FRS에 미정의 SWR 참조 |
| F-18 | 🟠 Major | 위험-요구사항 | Safety-related SWR TC 커버리지 확인 불가 |
| F-19 | 🟠 Major | 위험-요구사항 | MRD HAZ ↔ RMP HAZ 공식 매핑 테이블 부재 |
| F-20 | 🟠 Major | 위험-요구사항 | Audit Log 보관 기간 MRD(1년) vs PRD/FRS(90일) 불일치 |
| F-21 | 🟡 Minor | 위험-요구사항 | PRD RTM에서 SWR-PM-005 참조 — FRS 미정의 |

---

## 10. 개선 권고사항 (Recommendations)

### 우선순위 1 — Critical 항목 즉시 수정 필요

**권고 R-01**: PRD 및 RTM의 MRD 참조 수정
- PRD §1.1 "MRD 참조: MRD-XRAY-001 v1.2" → **"MRD-XRAY-GUI-001 v2.0"** 으로 수정
- RTM §1.3 참조 문서 테이블 동일하게 수정
- **근거**: F-01 / **담당**: 제품관리팀 / **기한**: 즉시

**권고 R-02**: V&V Plan 참조 문서 ID 전면 수정
- PRD 참조: "PRD-XRAY-001" → **"PRD-XRAY-GUI-001"** 로 수정
- FRS 참조: "FRS-XRAY-001 v2.1" → **"FRS-XRAY-GUI-001 v1.0"** 으로 수정
- RMP 참조: "RMP-XRAY-001 v1.2" → **"RMP-XRAY-GUI-001 v1.0"** 으로 수정
- **근거**: F-12 / **담당**: QA팀 / **기한**: 즉시

**권고 R-03**: FRS v1.0 기준으로 RMP 전체 SWR 참조 재수정
- RC 테이블의 SWR 참조를 FRS v1.0 번호 체계(SWR-DM-040번대, SWR-CS-070번대, SWR-SA-060번대 등)로 전면 업데이트
- 특히 FRS에 미정의된 SWR-UI-xxx 도메인 처리: FRS에 SWR-UI 섹션 신설하거나 해당 기능을 PR-WF/PR-UX 등에 통합
- **근거**: F-17 / **담당**: SE팀 + QA팀 / **기한**: 2주 이내

**권고 R-04**: QA Plan을 FRS v1.0 SWR 번호 체계로 전면 개정
- SWR-DM-001–010 → SWR-DM-040–055 번호 체계로 전면 교체
- SWR-NF-001–015 → SWR-NF-PF-001–008, SWR-NF-RL-010–015 등 계층형 번호로 교체
- 동일 작업을 SWR-CS, SWR-SA, SWR-WF, SWR-IP, SWR-DC, SWR-PM 전 도메인에 수행
- **근거**: F-04, F-09 / **담당**: QA팀 / **기한**: 2주 이내

**권고 R-05**: MRD HAZ와 RMP HAZ 간 공식 매핑 테이블 작성
- 두 HAZ 체계가 실질적으로 동일 위험을 다루고 있음을 공식 문서로 증명하는 "HAZ Cross-Reference Table" 신설
- 예시: HAZ-WF-001(환자 정보 오입력) = HAZ-SW-001(Wrong Patient) + HAZ-DATA-001(DICOM 데이터 손상) 부분 대응
- RMP 부록 또는 별도 MRD-RMP 위험 연결 문서로 작성
- **근거**: F-05, F-19 / **담당**: SE팀 + 위험관리팀 / **기한**: 1개월 이내

### 우선순위 2 — Major 항목 조속 수정

**권고 R-06**: PRD 파생 SWR 컬럼을 FRS v1.0 기준으로 전면 재검토
- PR-PM-002: SWR-PM-004,005 → **SWR-PM-010–013**
- PR-PM-003: SWR-PM-006–008 → **SWR-PM-020–024**
- PR-PM-004: SWR-PM-009,010 → **SWR-PM-030–033**
- PR-PM-006: SWR-PM-013,014 → **SWR-PM-050–053**
- 나머지 PR-WF, PR-IP, PR-DM, PR-DC, PR-SA, PR-CS 전 도메인의 파생 SWR 컬럼도 동일하게 재검토
- **근거**: F-02, F-03, F-08 / **담당**: 제품관리팀 + SE팀 / **기한**: 2주 이내

**권고 R-07**: Audit Log 보관 기간 요건 통일
- PRD PR-SA-065 및 PR-NF-SC-045의 "90일"을 MRD MR-035 요구사항인 **"1년(12개월) 이상"** 으로 상향 조정
- 또는 FDA Cybersecurity Guidance 및 HIPAA Security Rule (Audit Log 최소 6년 권고)을 검토하여 최적 기준 재설정 후 MRD-PRD-FRS 전 문서 일관 적용
- **근거**: F-20 / **담당**: RA파트 + QA팀 / **기한**: 1개월 이내

**권고 R-08**: MRD-RMP HAZ 이중 체계 해소 (또는 체계 선택 및 통일)
- 옵션 A: MRD의 HAZ-WF/IP/DM/CS 체계를 RMP/RTM의 HAZ-RAD/SW/DATA/SEC 체계로 통일하고 MRD 개정
- 옵션 B: 두 체계 중 하나를 인허가 주 체계로 선택하고 나머지 문서 일괄 갱신
- 옵션 C: 두 체계를 연결하는 공식 Cross-Reference 테이블 유지 (R-05와 연계)
- **근거**: F-05 / **기한**: MRD 차기 개정 시

### 우선순위 3 — Minor 항목 차기 개정 시 반영

**권고 R-09**: IEC 62304 표기 통일 — "AMD1" 사용 표준화
- PRD, FRS, RTM의 "IEC 62304:2006+A1:2015" → **"IEC 62304:2006+AMD1:2015"** 통일

**권고 R-10**: SDP의 Phase 용어 구분
- SDP 개발 활동 단계를 "Development Stage 1–7" 또는 "Lifecycle Phase"로 재명명하여 제품 릴리스 Phase 1/2와 혼동 방지

**권고 R-11**: RC-32 오타 수정
- RMP 및 RTM에서 "RC-32" → **"RC-032"** 로 수정

**권고 R-12**: MRD MR-003 세분화 검토
- PR-IP-xxx 18개로 분화되는 MR-003("영상 품질") 항목을 기능별로 세분화 (예: MR-003a 기본 뷰어, MR-003b 영상처리, MR-003c 측정 도구 등)

**권고 R-13**: FRS/PRD 위험 참조 컬럼에 구체 HAZ 번호 추가
- 현재 카테고리 수준(HAZ-RAD)에서 번호 수준(HAZ-RAD-001)으로 상세화

---

*본 보고서는 2026년 3월 18일 기준 제공된 13개 문서의 스냅샷을 기반으로 작성되었으며, 후속 문서 개정 시 재검토가 필요합니다.*

*문서 상태: 초안 (Draft) — RA파트 검토 후 최종 확정*
