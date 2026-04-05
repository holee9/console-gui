# HnVue Console SW — 내재화 개발 컨텍스트 & Gap 분석 계획

| 항목 | 내용 |
|------|------|
| **문서 ID** | ANALYSIS-002 |
| **버전** | v1.0 |
| **작성일** | 2026-04-05 |
| **분석 대상** | Console-GUI 프로젝트 정체성 + 참고 문서 역할 + Gap 분석 계획 |
| **선행 문서** | [ANALYSIS-001](ANALYSIS-001_Phase1_Review_v1.0.md) — Phase 1 현황 분석 |
| **결론** | Gap 분석 미수행 — 즉시 착수 필요 |

---

## 목차

1. [문서 목적](#1-문서-목적)
2. [프로젝트 정체성 명확화](#2-프로젝트-정체성-명확화)
3. [기존 HnVue vs 신규 개발 관계](#3-기존-hnvue-vs-신규-개발-관계)
4. [참고 문서 역할 분류](#4-참고-문서-역할-분류)
5. [Gap 분석 필요성 및 현황](#5-gap-분석-필요성-및-현황)
6. [Gap 분석 실행 계획](#6-gap-분석-실행-계획)
7. [HnVue.Imaging 구현 계획](#7-hnvueimaging-구현-계획)
8. [현실적 개발 로드맵 재검토](#8-현실적-개발-로드맵-재검토)
9. [다음 단계 권고사항](#9-다음-단계-권고사항)

---

## 1. 문서 목적

ANALYSIS-001은 Phase 1 코드 품질과 Critical Blocker를 정확히 파악했으나, 한 가지 근본적인 컨텍스트가 누락되었다.

> **누락된 컨텍스트**: "이 프로젝트가 무엇을 대체하는가, 기존 제품과의 관계는 무엇인가, docs/ 안의 비코드 파일들은 왜 존재하는가"

본 문서는 이 컨텍스트를 명확히 정의하고, 이로부터 도출되는 Gap 분석 계획과 개발 우선순위를 수립한다.

---

## 2. 프로젝트 정체성 명확화

### Console-GUI = HnVue 내재화 개발 프로젝트

```
Console-GUI 레포지토리 (이 코드베이스)
    ↕ 동일 프로젝트
HnVue Console SW (제품명)
    ↕ 내재화 대상
기존 HnVue 제품 (현재 출하 중, 3rd-party 기반)
```

- **제조사**: H&abyz (에이치앤아비즈)
- **목표**: 기존 3rd-party Console SW를 자사 기술로 내재화 (Internalization)
- **대체 대상**: IMFOU feel-DRCS (FDA K110033) — 현재 출하 중인 제품에 탑재된 3rd-party SW
- **개발 인력**: 2명 (Software Engineers)
- **인허가 목표**: MFDS 2등급, FDA 510(k), CE MDR Class IIa

### 내재화 개발의 의미

내재화 개발은 Greenfield 신규 개발과 다르다.

| 항목 | Greenfield 신규 개발 | 내재화 개발 |
|------|---------------------|------------|
| 기능 정의 기준 | 시장 조사 + 기획 | **기존 제품 기능 + 개선 사항** |
| UI/UX 기준 | 새로 설계 | **기존 제품 UI 개선안 존재** |
| 성능 기준 | 목표 수치 설정 | **기존 제품 성능 시험 결과가 기준** |
| 인허가 전략 | 처음부터 구축 | **기존 Predicate Device 기반 510(k)** |

따라서 **기존 HnVue 제품 문서가 모든 개발 의사결정의 1차 기준**이 된다.

---

## 3. 기존 HnVue vs 신규 개발 관계

### 두 가지 HnVue의 명확한 구분

| 구분 | 기존 HnVue (출하 중) | 신규 HnVue (개발 중) |
|------|---------------------|---------------------|
| **위치** | 현장 설치·판매 중 | `Console-GUI` 레포지토리 |
| **기술 기반** | 3rd-party SW 기반 | 자사 개발 (C# / .NET 8 / WPF) |
| **문서 위치** | `docs/*.pptx`, `docs/*.docx`, `docs/*.pdf` | `docs/planning/`, `docs/management/` 등 |
| **상태** | 운영 중 (FDA K110033 predicate) | Phase 1 완료, 추가 개발 필요 |
| **버전 관계** | v1.x (기준선) | v2.0 목표 (내재화) |

### Predicate Device 전략

신규 HnVue의 FDA 510(k) 전략은 `DRTECH EConsole1 (K231225)`을 Predicate로 삼는 Substantial Equivalence 방식이다. 기존 HnVue 제품은 이미 `IMFOU feel-DRCS (K110033)` 기반으로 운영 중이므로 현장 운용 데이터와 기능 목록이 신규 개발의 기준이 된다.

---

## 4. 참고 문서 역할 분류

`docs/` 루트에 있는 5개 원본 제품 파일은 두 가지 역할을 수행한다.

### 역할 A: 계획서·사양서 작성의 기준 자료

기존 제품이 무엇을 하는지 파악하여 신규 개발의 기능 범위와 요구사항을 정의하는 데 사용한다.

| 파일 | 내용 | 활용 방식 |
|------|------|---------|
| `Instructions for Use(EN) HnVUE 250714(공식메뉴얼).docx` | 기존 제품 영문 IFU (사용설명서) | MRD/PRD/FRS 기능 목록 기준 — 신규 코드가 이 기능들을 커버하는지 매핑 |
| `★HnVUE UI 변경 최종안_251118.pptx` | 기존 UI 설계 최종안 (2025-11-18) | `HnVue.UI` WPF 화면 구현의 기준 — PatientListView, ImageViewerView 등 화면 설계 참조 |
| `3. [HnVUE] Performance Test Report (A-PTR-HNV).docx` | 기존 성능 시험 보고서 | DOC-027 (Performance Report) 작성 기준 — 신규 제품이 넘어야 할 성능 벤치마크 |
| `hnvue_abyz_plan.pptx` | 전략·계획 자료 | 제품 전략, 인허가 로드맵, 사업 계획 맥락 파악 |

### 역할 B: 영상엔진·하드웨어 연동 API 참고 자료

`HnVue.Imaging`과 `HnVue.Workflow`의 하드웨어 연동 구현 시 기술 참조 문서로 사용한다.

| 파일 | 내용 | 활용 방식 |
|------|------|---------|
| `API_MANUAL_241206.pdf` | FPD(Flat Panel Detector) SDK API 매뉴얼 (2024-12-06) | `HnVue.Imaging` Phase 1c 구현의 핵심 — 검출기로부터 영상 데이터를 받아오는 SDK 연동 |
| `GENERATOR-001_Communication_Protocol_Guide_v1.0.md` | Sedecal/CPI X-ray Generator 통신 프로토콜 | `GeneratorSimulator` 실 구현 시 RS-232/RS-422 프로토콜 참조 |
| `DICOM-001_Implementation_Guide_v1.0.md` | fo-dicom C-STORE/C-FIND/MWL 구현 가이드 | `HnVue.Dicom` + `HnVue.Imaging` DICOM Header 파싱 구현 참조 |

### 참고 자료 활용 시점

```
현재 단계: 역할 A 우선 (Gap 분석)
    → 공식 매뉴얼 기반 기능 목록 추출
    → 신규 코드와 매핑

다음 단계: 역할 B 활용 (구현)
    → API_MANUAL_241206.pdf 기반 FPD SDK 연동
    → GENERATOR-001 기반 실 Generator 프로토콜 구현
```

---

## 5. Gap 분석 필요성 및 현황

### 현재 상태: Gap 분석 미수행

ANALYSIS-001에서 지적한 바와 같이, 기존 HnVUE 제품과 신규 Console-GUI 코드 간의 Gap 분석이 아직 수행되지 않았다.

**이것이 문제인 이유:**

1. **기능 누락 리스크**: 기존 제품이 지원하는 기능 중 신규 코드에서 누락된 것이 있을 수 있다.
2. **사양서 부정확**: FRS/SRS가 기존 제품의 실제 기능을 완전히 반영하지 않을 가능성이 있다.
3. **시험 보고서 불일치**: DOC-022~028이 실제 구현된 기능 기준이 아닌 계획 기준으로 작성되어 있다.
4. **규제 위험**: FDA/MFDS 심사 시 IFU 기재 기능과 실제 소프트웨어 불일치 → 즉각 Additional Information 요청.

> 시험 보고서 구체 불일치 사례는 [ANALYSIS-001 §5](ANALYSIS-001_Phase1_Review_v1.0.md#5-문서-코드-정합성-문제)를 참조.

---

## 6. Gap 분석 실행 계획

### 6-1. Gap 분석 범위

기존 HnVUE 공식 매뉴얼(`Instructions for Use(EN) HnVUE 250714.docx`)과 신규 코드 간 매핑.

### 6-2. 분석 방법

```
Step 1: 기존 IFU 기능 목록 추출
    → Instructions for Use 각 챕터에서 기능 목록 추출
    → 표준화된 Feature ID 부여 (예: F-IMG-001, F-PAT-001 등)

Step 2: 신규 코드 구현 여부 확인
    → 각 Feature에 대해 src/ 코드에서 구현 위치 확인
    → 상태: Implemented / Stub / Missing / Not Required

Step 3: 사양서 정합성 검토
    → FRS/SRS 요구사항과 IFU 기능의 1:1 매핑 확인
    → 누락 요구사항 식별

Step 4: 시험 보고서 현실화
    → 실제 구현된 기능만 시험 보고서에 기재
    → Stub/Missing 항목은 "미구현 — 향후 구현 예정"으로 명기
```

### 6-3. 예상 결과물

| 산출물 | 형식 | 용도 |
|--------|------|------|
| Feature Gap Matrix | Markdown 표 | 개발 우선순위 결정 |
| 수정된 FRS/SRS | 기존 문서 업데이트 | 규제 문서 정합성 |
| 시험 보고서 현실화 계획 | 작업 목록 | DOC-022~028 재작성 기준 |

### 6-4. 예상 소요: 2주 (2명 기준)

---

## 7. HnVue.Imaging 구현 계획

> Stub 상태 및 DICOM/파일 처리 목표 상세는 [ANALYSIS-001 §3-1](ANALYSIS-001_Phase1_Review_v1.0.md#3-critical-blockers)을 참조.
> 작업 분류 및 Wave 구조는 [STRATEGY-002](STRATEGY-002_ParallelDevelopment_v1.0.md)를 참조.

### 구현 단계 (HnVue.Imaging 고유 설계)

```
Phase 1c-A: fo-dicom 기반 DICOM Header 파싱 (2-3주)
    → DICOM Tag 읽기 (0028 그룹: 이미지 크기, Pixel Depth, 광자 해석)
    → Pixel Data 추출 (7FE0,0010)
    → 16-bit Grayscale → 8-bit 렌더링 파이프라인

Phase 1c-B: 기본 영상 조작 (2-3주)
    → Window/Level 조절 (W=400, L=2000 초기값)
    → Pan (픽셀 오프셋)
    → Zoom (Bilinear Interpolation)

Phase 1c-C: FPD SDK 연동 (3-4주, API_MANUAL_241206.pdf 활용)
    → 검출기 초기화/연결
    → 영상 획득 트리거 (AcquiringImage → ImageAcquired 워크플로우 연동)
    → Raw 데이터 수신 + 전처리 파이프라인
```

---

## 8. 릴리즈 준비도 및 로드맵

> 종합 릴리즈 준비도 매트릭스와 개발 로드맵은 [ANALYSIS-001 §4, §7](ANALYSIS-001_Phase1_Review_v1.0.md#4-종합-현황-매트릭스)을 참조.
> 단독/병렬 개발 분류, Wave 구조, 파일 소유권 매트릭스는 [STRATEGY-002](STRATEGY-002_ParallelDevelopment_v1.0.md)를 참조.

**현재 1차 릴리즈 준비도 ~45%. 현실적 릴리즈 예상일: 2027년 Q2~Q3.**

---

## 9. 다음 단계

> 전체 권고사항은 [ANALYSIS-001 §7](ANALYSIS-001_Phase1_Review_v1.0.md#7-결론-및-권고사항)을 참조.

**본 문서(ANALYSIS-002) 고유 액션:**

1. **Phase 1.5 Gap 분석** — SRS/FRS 기반 Feature 목록 추출 후 코드 매핑. IFU `.docx` 파일 직접 분석은 바이너리 포맷 제약으로 사람이 수행 필요.
2. **참고 자료 역할 A/B 분류** 활용 — Wave A/B 구현 시 역할 A(UI 기준), 역할 B(API/프로토콜) 문서 참조.
3. **FPD SDK 연동 설계** — Phase 1c-C 착수 전 `API_MANUAL_241206.pdf` 기반 IFpdAcquisitionService 인터페이스 설계 선행.

---

*문서 끝 (End of Document)*

| 항목 | 내용 |
|------|------|
| 분석자 | abyz-lab |
| 분석일 | 2026-04-05 |
| 기반 데이터 | ANALYSIS-001 + 소스코드 전수 검토 + docs/ 참고 자료 분석 |
| 주의사항 | 본 분석은 AI 보조 분석으로, 최종 판단은 프로젝트 책임자가 수행해야 함 |
