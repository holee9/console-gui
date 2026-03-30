# CE 기술 문서 (CE Technical Documentation)
## HnVue Console SW

---

## 문서 메타데이터 (Document Metadata)

| 항목 | 내용 |
|------|------|
| **문서 ID** | CE-XRAY-GUI-001 |
| **문서명** | HnVue Console SW CE 기술 문서 |
| **버전** | v1.0 |
| **작성일** | 2026-03-18 |
| **작성자** | RA 팀 |
| **승인자** | 의료기기 RA/QA 책임자 |
| **상태** | 초안 (Draft) |
| **기준 규격** | EU MDR 2017/745, Annex II (기술 문서), Annex III (기술 문서 갱신) |

---

## 1. 제출 개요

| 항목 | 내용 |
|------|------|
| **규정** | EU MDR 2017/745 (Medical Device Regulation) |
| **분류** | Class IIa (Rule 11 — 진단 목적 SW) |
| **Notified Body** | [NB 지정 예정] |
| **적합성 경로** | Annex IX (Quality Management System) + Annex XI (Product Conformity Assessment) |

---

## 2. Annex II 기술 문서 구성 (Technical Documentation per Annex II)

### 2.1 Section 1: 기기 설명 및 사양

| 항목 | 참조 문서 |
|------|----------|
| 제품 설명 및 의도된 용도 | MRD, PRD, IFU |
| 기본 UDI-DI | [발급 예정 — EUDAMED 등록] |
| 기기 분류 근거 | Rule 11 분석 문서 |
| 적합성 선언서 (DoC) | [별도 작성 예정] |

### 2.2 Section 2: 제조자 정보 및 QMS

| 항목 | 참조 문서 |
|------|----------|
| ISO 13485 인증서 | QMS 인증서 사본 |
| 제조 공정 설명 | SDP, SDG |
| 공급자 관리 | SOUP Report |

### 2.3 Section 3: 설계 및 제조 정보

| 항목 | 참조 문서 |
|------|----------|
| 설계 사양 | SRS, SAD, SDS |
| IEC 62304 프로세스 준수 | SDP, V&V 보고서 |
| 소프트웨어 검증 & 밸리데이션 | VVP, UTR, ITR, STR, VVSR, VAL |

### 2.4 Section 4: 일반 안전성 및 성능 요구사항 (GSPR)

**EU MDR Annex I GSPR 체크리스트 (주요 항목)**:

| GSPR # | 요구사항 | 적합 방법 | 참조 문서 |
|--------|---------|----------|----------|
| 1 | 안전하고 효과적인 성능 | 설계 관리, V&V | DHF 전체 |
| 5 | 반복 사용 시 안전성 | 72시간 안정성 테스트 | PTR |
| 14 | 의도된 용도에 적합한 설계 | 사용적합성 공학 | UEF, USTR |
| 17.1 | SW 수명주기 프로세스 | IEC 62304 준수 | SDP, V&V |
| 17.2 | SW 검증 및 밸리데이션 | IEC 62304 §5.5-5.8 | VVSR |
| 17.4 | 반복 가능한 결과 | 성능 테스트 | PTR |
| 20 | IVD 관련 | N/A | — |
| 23 | 라벨링 | EU MDR Annex I §23 | IFU |

### 2.5 Section 5: 이득-위험 분석

| 항목 | 참조 문서 |
|------|----------|
| 위험 관리 계획 | RMP |
| FMEA/FTA | FMEA |
| 위험 관리 보고서 | RMR |
| 이득-위험 결론 | CER §5 |

### 2.6 Section 6: 임상 평가

| 항목 | 참조 문서 |
|------|----------|
| 임상 평가 계획 | CEP |
| 임상 평가 보고서 | CER |
| PMCF 계획 | CER §6 |

### 2.7 Section 7: 시판 후 감시

| 항목 | 참조 문서 |
|------|----------|
| PMS 계획 | PMS-XRAY-GUI-001 |
| PSUR 계획 | PMS §3.2 |
| Vigilance 절차 | PMS §3.1 |

---

## 3. 적합 표준 목록 (Harmonised Standards)

| 표준 | 제목 | 적합 근거 |
|------|------|----------|
| EN ISO 14971:2019 | 위험 관리 | RMP, FMEA, RMR |
| EN 62304:2006+A1:2015 | SW 수명주기 | SDP, V&V 전체 |
| EN 62366-1:2015+A1:2020 | 사용적합성 공학 | UEF, USTR |
| EN ISO 13485:2016 | QMS | QMS 인증 |

---

## 4. CE 인증 일정

| 단계 | 목표일 |
|------|--------|
| 기술 문서 최종 완성 | 2026-10-01 |
| Notified Body 제출 | 2026-11-01 |
| NB 검토 (6-12개월) | 2027-05-01 ~ 2027-11-01 |
| CE 마킹 획득 (예상) | 2027-H2 |

---

*문서 끝 (End of Document)*
