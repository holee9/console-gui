# MRD v2.0 → v3.0 우선순위 재조정 제안서

---

| 항목 | 내용 |
|------|------|
| **문서 ID** | MRD-PRIORITY-REASSESS-001 |
| **버전** | v1.0 |
| **작성일** | 2026-04-02 |
| **프로젝트** | HnX-R1 RadiConsole™ |
| **작성자** | 전략마케팅본부 |
| **적용 대상** | MRD v2.0 → v3.0 우선순위 체계 전환 |
| **분류** | 대외비 (Confidential) |

---

## 개정 이력

| 버전 | 날짜 | 개정 내용 |
|------|------|---------|
| v1.0 | 2026-04-02 | 최초 작성. FDA K231225 및 feel-DRCS 실사 기반 4-Tier 재분류 제안 |

---

## 목차

1. [재조정 근거 요약](#1-재조정-근거-요약)
2. [4-Tier 분류 체계](#2-4-tier-분류-체계)
3. [전체 71개 MR 재분류표](#3-전체-71개-mr-재분류표)
4. [변경 요약 통계](#4-변경-요약-통계)
5. [Phase 1 최소 기능 범위 제안](#5-phase-1-최소-기능-범위-제안)
6. [출처](#6-출처)

---

## 1. 재조정 근거 요약

### 1.1 회사 현실: 솔직한 진단

우리는 현재 **SW 인력 2명**인 영세 소기업이다. 경쟁사인 DRTECH는 SW 인력 약 20명, Rayence는 약 15명, Vieworks는 20~30명이다. 이 격차를 무시하고 작성된 요구사항은 일정 지연과 리소스 낭비의 직접적 원인이 된다.

현재 MRD v2.0은 P1~P4 체계로 분류되어 있으나, **P1/P2 항목이 실질적으로 너무 많고, 그 안에 인허가와 무관한 항목들이 섞여 있다.** 이 제안서는 "없으면 법적으로 팔 수 없는 것"과 "없으면 고객이 안 사는 것"과 "없으면 좋은 것"을 명확히 분리하는 것을 목적으로 한다.

### 1.2 핵심 벤치마크 #1: DRTECH EConsole1 (FDA K231225)

국내 최대 경쟁사인 DRTECH(SW 인력 ~20명)의 EConsole1이 **2023년 FDA 510(k) 클리어런스(K231225)**를 받았다. 이때 사용한 **primary predicate가 바로 feel-DRCS(K110033, IMFOU)**이다. 즉, DRTECH는 우리가 현재 구매 중인 SW를 기준선으로 삼아 FDA를 통과했다.

**EConsole1이 FDA 510(k)에서 클리어한 최소 기능 범위:**
- 디텍터 제어 및 인터페이스
- X-ray Generator 촬영 설정 제어
- 이미지 획득 및 저장
- 데이터 관리 (환자 DB)
- 이미지 처리 (W/L, Zoom, Rotate, Measure 등)
- DICOM 3.0 (Storage SCU, MWL SCU, Print SCU)
- Image Stitch
- Cybersecurity (FDA 2014 guidance 기반)

**EConsole1에 없는 것 (팩트):** AI, Cloud, Analytics Dashboard, 원격 제어 앱, HL7 FHIR, RESTful API, RDSR, DRL 경고 등. 이 항목들은 FDA가 510(k)에서 요구하지 않은 것들이다.

> **시사점**: DRTECH보다 인력이 10분의 1 수준인 우리가 DRTECH보다 많은 기능을 Phase 1에 넣겠다는 것은 비현실적이다. EConsole1 수준이 우리의 Phase 1 상한선이다.

### 1.3 핵심 벤치마크 #2: feel-DRCS 실제 기능 (IMFOU 공식 확인)

우리가 대체하려는 feel-DRCS의 **실제 기본 기능**은 다음과 같다 (IMFOU 공식 사이트 및 사용자 매뉴얼 기준):

**기본 포함 (추가 비용 없음):**
- DICOM C-Store SCU, MWL SCU (자동/수동), Print SCU, DIR
- 디텍터 제어 (14개+ 제조사 호환)
- Generator 제어 (APR, AEC, Ready/Exposure)
- 영상처리 (FS-MLW 알고리즘): Edge Enhancement, Contrast Enhancement, Latitude, Noise Reduction
- W/L, Zoom, Pan, Flip, Rotate
- Annotation (R, L, Mark, Text)
- Calibration (Offset, Gain)
- Patient DB 관리
- CD Backup with viewer

**옵션 (추가 비용 필요):**
- DICOM MPPS SCU
- DICOM Storage Commitment SCU
- DICOM Query/Retrieve

**없는 것 (팩트):** RDSR, DRL 경고, AEC 모니터링 UI, Reject Analysis, RBAC, 감사 로그, SBOM, CVD, Code Signing, PHI 암호화(AES-256), SSO/AD 연동, 네트워크 격리 모드, AI 기능 일체.

> **시사점**: 우리의 1차 목표는 feel-DRCS를 대체하는 것이다. feel-DRCS에 없는 기능을 Phase 1에 필수로 넣는 것은 대체 기준선을 스스로 높이는 것이다.

### 1.4 MFDS 인허가 최소 필수 요건 (2등급 의료기기 SW)

식약처 2025년 가이드라인 기반 실제 인허가 차단 요건은 다음과 같다:
1. IEC 62304 수명주기 문서
2. IEC 62366 사용성 공학 문서 (Use Specification, Summative Evaluation)
3. SW 적합성 확인보고서 (안전성 등급, 위험관리)
4. SW 검증 및 유효성확인 보고서
5. 기술문서 (모양·구조, 성능, 사용목적, 사용방법, 시험규격)
6. 사이버보안: MFDS 2024 개정 가이드라인 35개 항목
7. DICOM Conformance Statement

> **중요**: MFDS 인허가는 RDSR, RBAC, PHI 암호화, 감사 로그 등을 **강제하지 않는다.** 이 항목들은 병원 구매 조건이거나 FDA/EU 요건이지, MFDS 2등급 인허가 차단 요건이 아니다 (추정 — MFDS 가이드라인 원문 최종 확인 필요).

### 1.5 재조정의 핵심 논리

MRD v2.0의 문제점: **P1(인허가 필수)과 P2(안전 필수)에 feel-DRCS에도 없고 EConsole1에도 없는 항목들이 포함되어 있다.** 이는 기준선을 자의적으로 높인 결과다.

재조정 후 기대 효과:
- Phase 1 범위 현실화 → 2명이 24~36 MM으로 완성 가능한 수준
- 인허가 필수 항목 명확화 → 규제 리스크 관리
- 과도한 요구사항 제거 → 개발 집중도 향상

---

## 2. 4-Tier 분류 체계

v2.0의 P1~P4 체계를 다음 4-Tier로 대체한다. 이 체계는 "비즈니스 임팩트" 기준으로 설계되었다.

### Tier 1: "없으면 인허가 불가" (Must Have — Regulatory Gate)

**정의**: MFDS 2등급 의료기기 소프트웨어 인허가를 차단하는 항목. 이것이 없으면 제품을 법적으로 팔 수 없다. 협상 불가.

| 구성 요소 | 근거 |
|---------|------|
| IEC 62304 프로세스 준수 | MFDS SW 적합성 확인보고서 필수 |
| IEC 62366 사용성 공학 | MFDS 인허가 필수 문서 |
| DICOM 기본 서비스 (C-Store, MWL) | MFDS 기술문서 인터페이스 기재 필수 |
| DICOM Conformance Statement | MFDS 인허가 제출 필수 |
| 기본 사이버보안 (MFDS 2024 가이드라인) | MFDS 사이버보안 35개 항목 |
| SW 무결성 검증 | MFDS 사이버보안 항목 내 포함 |
| 기본 사용자 인증 | MFDS 사이버보안 항목 내 포함 |

### Tier 2: "없으면 팔 수 없다" (Need to Have — Market Gate)

**정의**: 인허가는 통과할 수 있지만, 고객(병원)이 구매하지 않는 수준. **feel-DRCS의 기본 기능과 동등 이상**이어야 고객이 대체를 수용한다. 이것이 우리 제품의 시장 진입 최소 기준선이다.

| 구성 요소 | feel-DRCS 동등성 |
|---------|----------------|
| 자사 FPD 디텍터 완전 통합 | 자사 제품 필수 (번들 판매 목적) |
| Generator 연동 (APR/AEC Ready/Exposure) | feel-DRCS 기본 기능 |
| 영상처리 (외부 SDK) | feel-DRCS FS-MLW 동등 |
| W/L, Zoom, Pan, Flip, Rotate | feel-DRCS 기본 기능 |
| Annotation (R/L Mark, Text) | feel-DRCS 기본 기능 |
| Patient DB 관리 | feel-DRCS 기본 기능 |
| Worklist 자동 + 수동 | feel-DRCS 기본 기능 |
| PACS 전송 (C-Store) | feel-DRCS 기본 기능 |
| 프로토콜 프리셋 (촬영 부위별) | feel-DRCS 기본 기능 |
| DICOM Print | feel-DRCS 기본 기능 |

### Tier 3: "있으면 좋고" (Nice to Have — Competitive Enhancement)

**정의**: Phase 2 이후에 해도 된다. 경쟁력을 높이지만, Phase 1에서 구현하지 않아도 feel-DRCS를 대체할 수 있다. EConsole1(K231225)도 대부분 포함하지 않은 항목들이다.

- MPPS, Storage Commitment, Q/R (feel-DRCS에서도 옵션, 추가 비용)
- RDSR (EConsole1 미포함, feel-DRCS 미포함)
- DRL 경고 (EConsole1 미포함)
- Reject Analysis
- Image Stitching (EConsole1 포함 → Phase 2 목표)
- SSO/AD 연동
- 멀티벤더 디텍터
- 다국어 (한/영 외)
- 터치 최적화 UI
- EMR Bridge
- Auto Labeling, Auto Crop (파라미터 설정으로 유사 구현 가능)
- 포지셔닝 가이드

### Tier 4: "허황된/과도한 — 영구 제거 또는 Phase 3+" (Won't Have)

**정의**: 2명 조직에서 비현실적이거나, 현재 비즈니스 모델(FPD 번들 판매)에 맞지 않는 것. 넣어봤자 일정만 늦어진다.

- AI 노이즈 캔슬레이션 (자체 개발 불가, 외부 SDK도 고비용)
- AI Auto Collimation
- AI 포지셔닝 보조
- AI 플러그인 아키텍처
- Cloud SaaS 배포
- Analytics API
- 원격 제어 앱
- HL7 FHIR
- RESTful API
- 가상 산란 격자 (Virtual Grid) — 전용 알고리즘 개발 필요
- 골 억제 영상 (Bone Suppression) — AI 기반
- 레트로핏 호환 모드 — 타사 디텍터 지원 필요

---

## 3. 전체 71개 MR 재분류표

**변경 기호**: ↑ 상향, ↓ 하향, ↔ 유지 (Tier 기준), ✕ 제거(영구)

> **범례**:
> - 근거 A = "MFDS 인허가 필수"
> - 근거 B = "feel-DRCS 기본 기능"
> - 근거 C = "EConsole1 FDA K231225 미포함"
> - 근거 D = "2명 조직 비현실적"
> - 근거 E = "feel-DRCS 옵션 (추가 비용)"
> - 근거 F = "Phase 2+ 로드맵"
> - 근거 G = "비즈니스 모델 불일치"

### 카테고리 1: 워크플로우 (Workflow Efficiency)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-001 | MWL 자동 조회 (HIS/RIS→환자 목록 자동 수신, 수동 입력 최소화) | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능. 단, MFDS 인허가 차단 항목은 아님 |
| MR-002 | 촬영→PACS 전송 30초 이내 (성능 요건) | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 동등 성능 기준. 고객 대체 수용의 최소 조건 |
| MR-003 | 촬영 워크플로우 5 클릭 이내 (UX 요건) | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 동등 UX 기준. 고객 대체 수용의 최소 조건 |
| MR-004 | 50개 이상 촬영 프로토콜 사전 정의 | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 기본 기능 (프로토콜 프리셋). Phase 1에서 50개는 현실적 |
| MR-005 | 즐겨찾기 + 사용자 맞춤 프로토콜 설정 | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 기본 기능 (APR 맞춤 설정). 고객 수용 필수 |
| MR-006 | Prior Image Comparison (이전 영상 나란히 비교) | P3 | **Tier 3** | ↔ | 근거 C: EConsole1 미포함. Phase 2 목표. feel-DRCS 미포함 |
| MR-007 | STAT (응급) 우선순위 처리 | P3 | **Tier 3** | ↔ | 근거 C: EConsole1 미포함. Phase 2 목표. 긴급하지 않음 |
| MR-008 | 이동형 X-Ray 무선 연결 워크플로우 | P4 | **Tier 3** | ↑ | 근거 F: Phase 2. 이동형 FPD 번들 추가 시 필요 |
| MR-009 | MPPS 자동 보고 (RIS/HIS) | P3 | **Tier 3** | ↓ | 근거 E: feel-DRCS에서도 옵션(추가 비용). EConsole1 K231225 미포함 |
| MR-010 | 다중 디텍터 상태 표시 및 선택 | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 기본 기능 (14개+ 디텍터 관리). 자사 FPD 번들 필수 |
| MR-065 | ~~3D 카메라 기반 환자 감지~~ | **제외** | **✕ 영구 제거** | ✕ | 근거 D: HW+SW 통합 복잡도. 2명 개발팀으로 감당 불가. 제외 유지 |
| MR-066 | 실시간 영상 품질 QA 알림 (회전, FOV 클리핑 자동 검출) | P3 | **Tier 3** | ↔ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. Phase 2 목표 |
| MR-070 | Auto Labeling (L/R 마커 + 부위명 자동 배치) | P3 | **Tier 3** | ↓ | 근거: feel-DRCS는 파라미터 설정으로 유사 구현 가능. 별도 자동화는 Phase 2 |
| MR-071 | Auto Crop (유효 영역 자동 감지 크롭) | P3 | **Tier 3** | ↓ | 근거: feel-DRCS는 파라미터 설정으로 유사 구현 가능. 별도 자동화는 Phase 2 |

### 카테고리 2: 영상 품질 (Image Quality)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-011 | 자동 이미지 최적화 (Phase 1: 외부 영상처리 SDK 연동) | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능 (FS-MLW 알고리즘). 외부 SDK 연동으로 충족 가능 |
| MR-012 | W/L, Zoom, Pan 등 기본 영상 조작 | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능. 모든 경쟁사 보유. 고객 대체 수용 최소 조건 |
| MR-013 | Edge Enhancement, Noise Reduction, Grid Line 제거, Preset 저장 | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS FS-MLW 기본 기능 (Edge Enhancement, Latitude, NR 포함). 동등 필수 |
| MR-014 | AI 노이즈 캔슬레이션 (Smart Noise Cancellation) | P4 | **Tier 4** | ↓ | 근거 D: AI 자체 개발 불가. 외부 SDK도 고비용/인허가 이슈. EConsole1 미포함 |
| MR-015 | Image Stitching (최소 2장 이상) | P4 | **Tier 3** | ↑ | 근거: EConsole1 포함 (FDA 클리어). 경쟁사 표준. Phase 2 우선 목표로 격상 |
| MR-016 | Reject Analysis (재촬영 사유 기록 + 보고서) | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. MFDS 인허가 비차단. Phase 2 목표 |
| MR-017 | Scatter Correction (산란 방사선 보정) | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 기본 기능에서 명시 없음. Phase 2 목표 |
| MR-018 | 영상 품질 지표 자동 측정 (SNR, MTF) + 기준치 미달 경고 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. MFDS 인허가 비차단. Phase 2 목표 |
| MR-063 | 가상 산란 격자 (Virtual Grid / Gridless Imaging) | P3 | **Tier 4** | ↓ | 근거 D: 전용 알고리즘 개발 필요. 2명 개발팀 비현실적. EConsole1 미포함 |
| MR-064 | 골 억제 영상 (Bone Suppression) | P4 | **Tier 4** | ↔ | 근거 D: AI 기반 고급 기능. 2명 개발팀 비현실적. EConsole1 미포함 |

### 카테고리 3: 통합/연동성 (Integration & Interoperability)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-019 | DICOM 3.0 기본 서비스 (C-STORE, MWL, MPPS, Storage Commitment, Q/R) | P1 | **Tier 1 + Tier 3** | ↓ (분리) | **중요 재분류**: C-STORE + MWL → Tier 1 (MFDS 인허가 필수 A). MPPS·Commitment·Q/R → Tier 3 (feel-DRCS에서도 옵션 E) |
| MR-020 | IHE Scheduled Workflow (SWF) 프로파일 준수 | P1 | **Tier 1** | ↔ | 근거 A: MFDS 기술문서 인터페이스 기재 필수. 실질적 시장 진입 요건 |
| MR-021 | 주요 PACS 상호운용성 검증 (최소 3개 벤더) | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 기본 기능 (20개국+ 설치 실적). 병원 구매 결정 요인 |
| MR-022 | HL7 FHIR 기반 EMR/HIS 연동 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. 2명 비현실적. Phase 3+ |
| MR-023 | 자사 FPD 디텍터 완전 통합 (Phase 1 자사 전용) | P4 | **Tier 2** | ↑ | 근거 B: 자사 FPD 번들 판매의 핵심 전제. 이게 없으면 제품 자체가 성립 안 됨 |
| MR-024 | DICOM Print Management (필름 출력) | P3 | **Tier 2** | ↑ | 근거 B: feel-DRCS 기본 기능 (DICOM Print SCU). 일부 시장 필수 |
| MR-025 | DICOM MWL을 통한 RIS 검사 일정 자동 수신 | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능 (MWL 자동/수동). MR-001과 중복이나 구체화된 버전 |
| MR-026 | RESTful API 외부 시스템 연동 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. Cloud/AI 확장용. Phase 3+ |
| MR-067 | ~~OEM 화이트라벨 SDK~~ | **제외** | **✕ 영구 제거** | ✕ | 근거 G: 자사 비즈니스 모델과 불일치. 제외 유지 |

### 카테고리 4: 선량 관리 (Safety & Dose Management)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-027 | RDSR 생성 + PACS/DRL 전송 | P1 | **Tier 3** | ↓ | **핵심 재분류**: feel-DRCS 미포함. EConsole1(K231225) 미포함. MFDS 인허가 차단 항목 아님 (추정). Phase 2 목표 |
| MR-028 | 환자 선량 이력 기록 + DRL 초과 경고 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. Phase 2 목표. MFDS 인허가 비차단 |
| MR-029 | 촬영 전 예상 선량 화면 표시 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. Phase 2 목표 |
| MR-030 | 소아 환자 별도 선량 프로토콜 및 DRL 기준 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS에 소아 전용 선량 UI 없음. Phase 2 목표 |
| MR-031 | AEC 파라미터 콘솔 모니터링 및 설정 | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능 (AEC 제어 포함). Generator 연동 핵심 구성요소 |
| MR-032 | 선량 트렌드 분석 + 기관별 선량 통계 보고서 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. Analytics 성격. Phase 2 목표 |

### 카테고리 5: 사이버보안 (Cybersecurity)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-033 | RBAC (Role-Based Access Control) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 2024 사이버보안 가이드라인 35개 항목 내 포함 (접근통제). feel-DRCS 미포함이지만 MFDS 요건 |
| MR-034 | PHI 암호화 (AES-256, 전송+저장) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 사이버보안 가이드라인 (데이터 보호 항목). feel-DRCS 미포함이지만 MFDS 요건 |
| MR-035 | 감사 로그 (1년 이상 보관) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 사이버보안 가이드라인 (감사 및 책임추적성). feel-DRCS 미포함이지만 MFDS 요건 |
| MR-036 | SBOM (소프트웨어 구성요소 목록) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 사이버보안 2024 개정 가이드라인 (SW 투명성). 미국 FDA 524B도 동일 요건 |
| MR-037 | CVD (취약점 공개 정책) 프로세스 | P1 | **Tier 1** | ↔ | 근거 A: MFDS 사이버보안 가이드라인 (사후 관리 요건). 문서/프로세스 수준으로 구현 가능 |
| MR-038 | SSO/AD (Active Directory/LDAP) 연동 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. 병원 IT 편의 기능. Phase 2 목표 |
| MR-039 | SW 무결성 검증 (Code Signing, Integrity Check) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 사이버보안 가이드라인 (SW 무결성). feel-DRCS 미포함이지만 MFDS 요건 |
| MR-040 | 네트워크 격리 환경 핵심 기능 동작 | P2 | **Tier 3** | ↓ | 근거 C: EConsole1 명시 미포함. feel-DRCS 미포함. Phase 2 목표. 병원 보안 정책 대응 |

> **사이버보안 섹션 주석**: MR-033~039의 Tier 1 분류는 MFDS 2024 개정 가이드라인 기반 추정이다. MFDS 가이드라인 35개 항목의 실제 내용에 따라 일부 항목이 Tier 1 → Tier 2로 이동할 수 있다. **RA 파트 확인 필수.**

### 카테고리 6: UX/UI (사용성)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-041 | 터치스크린 + 마우스/키보드 병행 지원 | P3 | **Tier 3** | ↔ | 근거: feel-DRCS 기본 기능이나, 터치 전용 최적화는 Phase 2. 기본 마우스/키보드로 Phase 1 충분 |
| MR-042 | Nielsen 휴리스틱 75점 이상 (사용성 평가) | P3 | **Tier 2** | ↑ | 근거 A: IEC 62366 사용성 공학 → MFDS 인허가 연계. Summative Evaluation에서 측정됨 |
| MR-043 | 방사선사 4시간 이내 독립 수행 가능 | P3 | **Tier 2** | ↑ | 근거 A: IEC 62366 사용성 공학 목표. MFDS 인허가 연계 |
| MR-044 | 시스템 상태 색상+아이콘 즉각 인지 (연결/준비/오류) | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능. 촬영 오류 방지 필수 UI |
| MR-045 | 한/영 2개 언어 지원 (Phase 1) | P4 | **Tier 3** | ↑ | 근거: 국내 출시(한국어) + 수출용(영어). 한국어는 Tier 2 수준이나, 영어는 Phase 2에서도 가능 |
| MR-046 | 촬영 부위별 멀티미디어 포지셔닝 가이드 | P3 | **Tier 3** | ↔ | 근거 C: EConsole1 미포함. feel-DRCS 미포함. Phase 2 목표 |
| MR-047 | 화면 구성 사용자/역할별 커스터마이징 | P4 | **Tier 4** | ↔ | 근거 D: 2명 개발팀에서 Phase 1에 불필요한 UX 고도화. Phase 3+ |
| MR-048 | 오류 메시지 원인+해결방법 명확 안내 | P2 | **Tier 2** | ↔ | 근거 B: feel-DRCS 기본 기능 수준. IEC 62366 사용 오류 방지 요건 |
| MR-049 | 스마트폰/태블릿 원격 제어 앱 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. 2명 개발팀 비현실적. Phase 3+ |
| MR-068 | EMR Bridge 직접 연동 | P3 | **Tier 3** | ↔ | 근거 F: Rayence가 보유한 차별화 기능. Phase 2 목표. feel-DRCS 미포함 |

### 카테고리 7: 규제 준수 (Regulatory Compliance)

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-050 | IEC 62304 Class B 수명주기 프로세스 준수 | P1 | **Tier 1** | ↔ | 근거 A: MFDS 인허가 필수. SW 적합성 확인보고서의 핵심 |
| MR-051 | IEC 62366-1 사용성 공학 (Use Specification, Summative Evaluation) | P1 | **Tier 1** | ↔ | 근거 A: MFDS 인허가 필수. 2등급 의료기기 SW 필수 문서 |
| MR-052 | ISO 13485/FDA 21 CFR 820 품질 관리 시스템 | P1 | **Tier 1** | ↔ | 근거 A: MFDS GMP 심사 요건 (QMS 문서 필수) |
| MR-053 | MFDS/FDA/CE 규제 승인 획득 | P1 | **Tier 1** | ↔ | 근거 A: 법적 판매 필수. 단, FDA/CE는 Phase 1 목표에서 제외하고 MFDS만 Phase 1 |
| MR-054 | DICOM Conformance Statement 작성/공개 | P1 | **Tier 1** | ↔ | 근거 A: MFDS 인허가 제출 필수. 병원 구매 기본 요구사항 |
| MR-055 | GDPR/HIPAA 개인정보 보호 기능 (익명화, 삭제권, 접근 로그) | P1 | **Tier 3** | ↓ | **핵심 재분류**: 국내 MFDS 인허가에는 GDPR/HIPAA 불필요. Phase 1 = 국내 출시. 개인정보보호법(PIPA) 준수로 충분 |

### 카테고리 8: 확장성 및 AI-Readiness

| MR ID | 요구사항 (요약) | v2.0 | 제안 | 변경 | 근거 |
|-------|--------------|------|------|------|------|
| MR-056 | AI 플러그인 아키텍처 (3rd party FDA 승인 AI 모듈 통합) | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. 2명 비현실적. Phase 3+ |
| MR-057 | AI Auto Collimation 향후 통합 가능 아키텍처 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. "아키텍처 준비"도 실질적 공수 필요. Phase 3+ |
| MR-058 | AI 포지셔닝 보조 향후 통합 가능 인터페이스 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. Phase 3+ |
| MR-059 | ~~마이크로서비스 아키텍처~~ | **제외** | **✕ 영구 제거** | ✕ | 근거 D: WPF 데스크톱 앱에 마이크로서비스는 과도한 엔지니어링. 제외 유지 |
| MR-060 | Cloud SaaS/Hybrid 배포 | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. 2명 비현실적. Phase 3+ |
| MR-061 | Analytics API (운영 데이터 외부 플랫폼 제공) | P4 | **Tier 4** | ↔ | 근거 D: feel-DRCS 미포함. EConsole1 미포함. 2명 비현실적. Phase 3+ |
| MR-062 | ~~중앙 집중식 설정 관리 (멀티사이트)~~ | **제외** | **✕ 영구 제거** | ✕ | 근거 G: 단일 워크스테이션 기반. 비즈니스 모델 불일치. 제외 유지 |
| MR-069 | 레트로핏 호환 모드 (CR→DR 전환 시장) | P3 | **Tier 4** | ↓ | 근거 G: 자사는 자사 FPD 번들 판매 목적. 타사 디텍터 레트로핏은 비즈니스 모델 불일치. 2명 비현실적 |

---

## 4. 변경 요약 통계

### 4.1 Tier별 분류 결과

| Tier | 명칭 | MR 개수 | 주요 포함 항목 |
|------|------|---------|-------------|
| **Tier 1** | Must Have (인허가 필수) | **12개** | MR-019(C-Store+MWL 부분), MR-020, MR-033~037, MR-039, MR-050~054 |
| **Tier 2** | Need to Have (시장 진입 필수) | **17개** | MR-001~005, MR-010~013, MR-021, MR-023~025, MR-031, MR-042~044, MR-048 |
| **Tier 3** | Nice to Have (Phase 2+ 목표) | **23개** | MR-006~009, MR-015~018, MR-027~030, MR-032, MR-038, MR-040~041, MR-045~046, MR-055, MR-066~068, MR-070~071 |
| **Tier 4** | Won't Have (영구 제거/Phase 3+) | **11개** | MR-014, MR-022, MR-026, MR-047, MR-049, MR-056~058, MR-060~061, MR-063~064, MR-069 |
| **영구 제거** | 제외 (v2.0에서 이미 제외) | **4개** | MR-059, MR-062, MR-065, MR-067 |
| **합계** | | **67개 활성 + 4개 제외 = 71개** | |

> **참고**: MR-019는 C-Store/MWL은 Tier 1, MPPS/Commitment/Q/R은 Tier 3으로 분리 처리. 표에서는 MR-019를 Tier 1 카운트에 포함.

### 4.2 v2.0 대비 변경 현황

| 구분 | v2.0 개수 | v3.0 제안 개수 | 변화 |
|------|---------|-------------|-----|
| P1 → Tier 1 (인허가 필수) | 13개 (P1) | 12개 (Tier 1) | -1 (MR-055 하향) |
| P2 → Tier 2 (시장 필수) | 18개 (P2) | 17개 (Tier 2) | -1 (일부 재조정) |
| P3 → Tier 3 (Phase 2+) | 28개 (P3) | 23개 (Tier 3) | -5 (일부 Tier 2 상향, 일부 Tier 4 하향) |
| P4 → Tier 4 (Phase 3+/제거) | 8개 (P4) | 11개 (Tier 4) | +3 (하향 조정) |
| 제외 (영구) | 4개 | 4개 | 변동 없음 |

### 4.3 주요 변경 하이라이트

**하향 조정 (과도했던 항목):**
- MR-027 (RDSR): P1 → Tier 3. feel-DRCS에도, EConsole1에도 없음. MFDS 인허가 비차단.
- MR-028~030 (선량 관리 고급): P2 → Tier 3. EConsole1 미포함.
- MR-032 (선량 통계 보고서): P2 → Tier 3. Analytics 성격.
- MR-016~018 (Reject Analysis, Scatter Correction, SNR 측정): P2 → Tier 3. EConsole1 미포함.
- MR-055 (GDPR/HIPAA): P1 → Tier 3. 국내 MFDS Phase 1에 불필요.
- MR-038 (SSO/AD): P2 → Tier 3. feel-DRCS/EConsole1 미포함.
- MR-040 (네트워크 격리): P2 → Tier 3. EConsole1 미포함.
- MR-009 (MPPS): P3 → Tier 3. feel-DRCS 옵션(추가 비용).
- MR-069 (레트로핏): P3 → Tier 4. 비즈니스 모델 불일치.

**상향 조정 (실제로 중요한 항목):**
- MR-002 (30초 전송): P3 → Tier 2. 고객 대체 수용 최소 조건.
- MR-003 (5 클릭): P3 → Tier 2. 고객 대체 수용 최소 조건.
- MR-004, MR-005 (프로토콜 프리셋): P3 → Tier 2. feel-DRCS 기본 기능.
- MR-013 (후처리 알고리즘+Preset): P3 → Tier 2. feel-DRCS FS-MLW 동등.
- MR-021 (PACS 상호운용성 3개 벤더): P3 → Tier 2. 병원 구매 필수.
- MR-023 (자사 FPD 통합): P4 → Tier 2. 번들 판매 전제.
- MR-024 (DICOM Print): P3 → Tier 2. feel-DRCS 기본 기능.
- MR-042, MR-043 (사용성 평가): P3 → Tier 2. IEC 62366 연계.
- MR-015 (Image Stitching): P4 → Tier 3. EConsole1 포함, Phase 2 우선화.

---

## 5. Phase 1 최소 기능 범위 제안

**Phase 1 = Tier 1 + Tier 2 전부**

이것만 하면: (1) MFDS 인허가 가능, (2) feel-DRCS 대체 가능, (3) SW 인력 2명이 24~36 MM으로 완성 가능 (추정).

### 5.1 Tier 1: 인허가 필수 항목 (12개 MR)

이 항목들은 개발이 완료되어야 하는 것이 아니라, **문서와 프로세스가 준수되어야 하는 것**이 대부분이다. 즉, 개발 공수보다 RA 공수가 더 많이 드는 항목들이다.

| MR ID | 항목 | Phase 1 구현 방식 |
|-------|-----|----------------|
| MR-019 (부분) | DICOM C-Store SCU + MWL SCU | 기능 구현 |
| MR-020 | IHE SWF 프로파일 준수 | DICOM Conformance Statement에 반영 |
| MR-033 | RBAC (기본 사용자 권한 관리) | 관리자/방사선사 2개 역할로 단순 구현 |
| MR-034 | PHI 암호화 (AES-256) | 저장 DB 암호화 + TLS 전송 |
| MR-035 | 감사 로그 (1년 보관) | 로그 파일 + 보관 정책 |
| MR-036 | SBOM | 개발 툴체인에서 자동 생성 (Syft 등 무료 툴 활용) |
| MR-037 | CVD 프로세스 | SOP 문서 작성 (실제 코딩 불필요) |
| MR-039 | SW 무결성 검증 (Code Signing) | 인증서 구매 + 빌드 파이프라인 설정 |
| MR-050 | IEC 62304 프로세스 준수 | 개발 프로세스 문서화 (개발하면서 병행) |
| MR-051 | IEC 62366 사용성 공학 | Use Specification 작성 + Summative Evaluation (출시 전) |
| MR-052 | ISO 13485/QMS | QMS SOP 문서 작성 |
| MR-053 | MFDS 인허가 획득 | Phase 1 완성 후 신청 (6~12개월 소요) |
| MR-054 | DICOM Conformance Statement | 문서 작성 (DICOM 구현 완료 후) |

**현실적 공수 추정 (Tier 1)**: 코딩 약 3 MM + RA 문서 약 6~9 MM = 총 약 9~12 MM

### 5.2 Tier 2: 시장 진입 필수 항목 (17개 MR)

| MR ID | 항목 | feel-DRCS 동등성 |
|-------|-----|----------------|
| MR-001 | MWL 자동 조회 | feel-DRCS 기본 기능 ✓ |
| MR-002 | 30초 이내 전송 성능 | feel-DRCS 동등 ✓ |
| MR-003 | 5 클릭 이내 워크플로우 | feel-DRCS 동등 ✓ |
| MR-004 | 50개 촬영 프로토콜 | feel-DRCS 기본 기능 ✓ |
| MR-005 | 즐겨찾기 + 맞춤 프로토콜 | feel-DRCS 기본 기능 ✓ |
| MR-010 | 디텍터 상태 표시 및 선택 | feel-DRCS 기본 기능 ✓ |
| MR-011 | 자동 영상 처리 (외부 SDK) | feel-DRCS FS-MLW 동등 ✓ |
| MR-012 | W/L, Zoom, Pan, Flip, Rotate | feel-DRCS 기본 기능 ✓ |
| MR-013 | 후처리 알고리즘 + Preset | feel-DRCS FS-MLW 동등 ✓ |
| MR-021 | PACS 상호운용성 (3개 벤더) | feel-DRCS 실적 ✓ |
| MR-023 | 자사 FPD 디텍터 완전 통합 | feel-DRCS 기본 기능 ✓ |
| MR-024 | DICOM Print | feel-DRCS 기본 기능 ✓ |
| MR-025 | MWL 자동 수신 (RIS) | feel-DRCS 기본 기능 ✓ |
| MR-031 | AEC 파라미터 모니터링+설정 | feel-DRCS 기본 기능 ✓ |
| MR-042 | 사용성 평가 75점 이상 | IEC 62366 연계 |
| MR-043 | 4시간 이내 독립 수행 | IEC 62366 연계 |
| MR-044 | 시스템 상태 색상+아이콘 | feel-DRCS 기본 기능 ✓ |
| MR-048 | 오류 메시지 원인+해결방법 | feel-DRCS 기본 기능 ✓ |

**현실적 공수 추정 (Tier 2)**: 코딩 약 12~18 MM = 총 약 12~18 MM

### 5.3 Phase 1 총계

| 구분 | 공수 추정 |
|------|---------|
| Tier 1 (규제/문서) | 9~12 MM |
| Tier 2 (기능 개발) | 12~18 MM |
| 버퍼 (테스트, 버그픽스, QA) | 3~6 MM |
| **합계** | **24~36 MM** |

**결론**: Tier 1 + Tier 2만 하면 SW 인력 2명 기준 24~36 MM 이내에 (1) MFDS 인허가 신청 가능한 수준, (2) feel-DRCS 대체 가능한 기능 수준 달성이 가능하다. **Tier 3/4 항목은 단 하나도 Phase 1에 포함하지 않는다.**

### 5.4 Phase 2 목표 (인력 보강 전제)

Phase 2에서 달성해야 할 경쟁력 향상 항목:
- **Image Stitching** (MR-015) — EConsole1 포함, 경쟁사 표준
- **Reject Analysis** (MR-016) — 품질 관리 필수화 추세
- **RDSR + DRL 경고** (MR-027, MR-028) — 선량 관리 규제 강화 대비
- **멀티벤더 디텍터** (MR-023 확장) — 시장 확대
- **SSO/AD 연동** (MR-038) — 대형 병원 진입
- **한/영 다국어** (MR-045) — 수출 대비
- **EMR Bridge** (MR-068) — Rayence 동등 수준
- **MPPS** (MR-009, MR-019 부분) — IHE SWF 완전 준수
- **FDA 510(k) / CE 마킹** (MR-053) — 수출 시장 진입

---

## 6. 출처

| # | 출처 | URL / 비고 |
|---|------|----------|
| 1 | FDA 510(k) K231225 — DRTECH EConsole1 | https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf |
| 2 | FDA 510(k) K110033 — IMFOU feel-DRCS (predicate device) | FDA CDRH 데이터베이스 |
| 3 | feel-DRCS 공식 제품 페이지 (IMFOU) | https://www.imfou.com/bbs/board.php?bo_table=product2&wr_id=8 |
| 4 | MFDS 의료기기 소프트웨어 허가심사 가이드라인 2025 | 식약처 공식 고시 |
| 5 | MFDS 디지털의료기기SW 허가심사 가이드라인 2025 | 식약처 공식 고시 |
| 6 | MFDS 의료기기 사이버보안 가이드라인 2024 개정 | 식약처 공식 고시 |
| 7 | Vieworks VXvue FDA 510(k) K122866 | FDA CDRH 데이터베이스 |
| 8 | STRATEGY-001 v2.0 — HnX-R1 회사 전략 포지셔닝 | /docs/planning/research/STRATEGY-001_Company_Positioning_v2.0.md |
| 9 | MRD v2.0 — 시장 요구사항 문서 (기준 문서) | /docs/planning/DOC-001_MRD_v2.0.md |

---

## 부록: 재분류 결정 트리 (Quick Reference)

어떤 항목을 어느 Tier에 넣을지 빠르게 판단하는 기준:

```
Q1. MFDS 2등급 SW 인허가 심사에서 이 항목이 없으면 심사 통과가 불가능한가?
  → YES: Tier 1
  → NO: Q2로

Q2. feel-DRCS 기본 기능(무료 포함)에 있는가?
  (즉, 우리가 대체하려는 SW가 기본으로 제공하는 기능인가?)
  → YES: Tier 2
  → NO: Q3으로

Q3. DRTECH EConsole1(K231225)에 포함되어 FDA 510(k) 클리어런스를 받았는가?
  → YES: Tier 3 (Phase 2 우선 목표)
  → NO: Q4로

Q4. SW 인력 2명이 24~36 MM 안에 구현 가능한가?
  또는 자사 비즈니스 모델(FPD 번들 판매)에 적합한가?
  → YES: Tier 3 (Phase 2+ 목표)
  → NO: Tier 4 (Phase 3+ 또는 영구 제거)
```

---

*이 문서는 MRD v3.0 작성 시 우선순위 체계 전환의 기준 자료로 활용한다. v3.0 확정 전 RA 파트 및 개발팀 검토가 필요하다. 특히 사이버보안(MR-033~037, MR-039) 항목의 Tier 1 분류는 MFDS 2024 가이드라인 35개 항목 원문 대조 후 최종 확정한다.*
