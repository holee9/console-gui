# HnVue Console SW — 모듈 구현 교차검증 보고서

| 항목 | 내용 |
|------|------|
| **문서 ID** | ANALYSIS-003 |
| **버전** | v1.0 |
| **작성일** | 2026-04-07 |
| **분석 대상** | HnVue Console SW 전체 17개 모듈 |
| **분석 범위** | SAD v1.0, SDS v1.0, SRS v1.0, FRS v1.0, WBS v2.0, 테스트 계획서 대비 구현 상태 교차검증 |
| **참조 문서 제외** | UI 디자인 문서 (`docs/design/`, `docs/ui_mockups/`) |
| **결론** | **구현 완성도 ~45% — 아키텍처 우수, 하드웨어 어댑터·보안 핵심 기능 미구현** |

---

## 개정 이력

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| v1.0 | 2026-04-07 | 최초 작성 — 모듈별 완성도, Gap 분석, 교정 계획 | MoAI |

---

## 목차

1. [분석 방법 및 범위](#1-분석-방법-및-범위)
2. [아키텍처 품질 평가](#2-아키텍처-품질-평가)
3. [모듈별 완성도 매트릭스](#3-모듈별-완성도-매트릭스)
4. [인터페이스 계약 구현 현황](#4-인터페이스-계약-구현-현황)
5. [스펙 대비 핵심 Gap 분석](#5-스펙-대비-핵심-gap-분석)
6. [테스트 커버리지 현황](#6-테스트-커버리지-현황)
7. [교정 계획](#7-교정-계획)
8. [종합 진단](#8-종합-진단)

---

## 1. 분석 방법 및 범위

### 1.1 분석 대상 문서

| 문서 ID | 문서명 | 분석 목적 |
|---------|--------|----------|
| SAD-XRAY-GUI-001 v1.0 | Software Architecture Design | 모듈 설계 vs 구현 비교 |
| SDS-XRAY-GUI-001 v1.0 | Software Design Specification | 인터페이스 계약 이행 검증 |
| SRS-XRAY-GUI-001 v1.0 | Software Requirements Specification | 기능 요건 충족률 산출 |
| FRS-XRAY-GUI-001 v1.0 | Functional Requirements Specification | ~180개 SWR 이행 여부 |
| WBS-001 v2.0 | Work Breakdown Structure | 작업 완료율 추정 |
| DOC-012 v2.0 | Unit Test Plan | 계획 대비 구현된 테스트 케이스 비율 |
| DOC-013 v2.0 | Integration Test Plan | 통합 테스트 시나리오 이행률 |
| ANALYSIS-001 v1.0 | Phase 1 현황 분석 보고서 | 이전 분석과의 비교 |

### 1.2 분석 도구 및 방법

- 소스코드 정적 분석 (331개 .cs 파일, 17개 모듈)
- .csproj 의존성 그래프 빌드 (전체 ProjectReference 추출)
- SAD §6 인터페이스 계약과 구현 파일 1:1 매핑
- FRS SWR 항목별 구현 파일 존재 여부 확인
- 테스트 파일 수 / 소스 파일 수 비율 산출

---

## 2. 아키텍처 품질 평가

### 2.1 레이어 구조

```
HnVue.App  (Composition Root / WinExe)
    │
    ├── HnVue.UI + HnVue.UI.ViewModels  [Presentation Layer]
    │       └── HnVue.UI.Contracts 만 참조 (비즈니스 모듈 직접 참조 없음)
    │
    ├── HnVue.Workflow  [Orchestration — 7개 의존성]
    │       └── Common, Data, Security, Dicom, Imaging, Dose, Incident
    │
    ├── Domain Services
    │       ├── HnVue.Dose, HnVue.Imaging, HnVue.Dicom
    │       ├── HnVue.PatientManagement, HnVue.Incident
    │       ├── HnVue.CDBurning, HnVue.SystemAdmin
    │       └── HnVue.Detector, HnVue.Update
    │
    ├── Infrastructure
    │       ├── HnVue.Data  (EF Core + SQLite)
    │       └── HnVue.Security  (JWT, RBAC, Audit)
    │
    └── HnVue.Common  (Foundation — 16개 모듈 의존)
```

### 2.2 품질 지표

| 항목 | 결과 | 세부 내용 |
|------|------|----------|
| **순환 의존성** | ✅ 없음 | 단방향 레이어 구조 완벽 준수 |
| **UI → Business 직접 참조** | ✅ 없음 | NetArchTest 자동화 검증 3개 테스트 통과 |
| **God Class (500줄 초과)** | ✅ 없음 | 최대 296줄 (WPF 보일러플레이트 정당) |
| **ViewModel 인터페이스 주입** | ✅ 완전 | 21개 ViewModel 전부 생성자 주입 |
| **레이어 위반** | ✅ 없음 | 하위 → 상위 호출 전무 |
| **Fan-Out 과다 (5+ 의존성)** | ⚠ HnVue.Workflow | 7개 의존 — 오케스트레이터로서 적절 |
| **HnVue.Common Fan-In** | ⚠ 모니터링 필요 | 16개 모듈 전체 의존 — 단일 실패점 |

> **결론**: 스파게티 아키텍처 없음. Clean Architecture 및 MVVM 패턴이 정확히 적용되어 있음.

---

## 3. 모듈별 완성도 매트릭스

| 모듈 | 소스 파일 | 테스트 파일 | 기능 완성도 | 상태 | 주요 미구현 사항 |
|------|----------|------------|------------|------|----------------|
| **HnVue.Common** | 60 | 0 | ~80% | ⚠ | Result\<T\> 모나드 단위 테스트 없음 (600+ 호출처) |
| **HnVue.Data** | 19 | 4 | ~55% | ⚠ | WAL 설정, 스키마 마이그레이션 정책 불명확 |
| **HnVue.Security** | 17 | 6 | ~40% | ❌ | PHI AES-256-GCM 암호화 미구현, IHE ATNA 감사 추적 미완 |
| **HnVue.Dicom** | 16 | 7 | ~50% | ⚠ | fo-dicom 래퍼 존재, 멀티벤더 PACS 미검증, TLS 미구현 |
| **HnVue.Incident** | 15 | 5 | ~65% | ✅ | 비교적 완성도 높음 |
| **HnVue.Update** | 17 | 8 | ~65% | ✅ | 디지털 서명, 백업/복원 테스트 충실 |
| **HnVue.App** | 18 | 0 | ~60% | ⚠ | DI 구성(BuildHost) 통합 테스트 없음 |
| **HnVue.UI.Contracts** | 32 | 0 | ~70% | ✅ | 인터페이스 정의 충실, 인터페이스 전용 모듈 |
| **HnVue.UI.ViewModels** | 21 | 0 | ~55% | ⚠ | HnVue.UI.Tests에서 간접 검증 |
| **HnVue.UI** | 175 | 13 | ~50% | ⚠ | MVVM 프레임워크 완비, 일부 화면 미완 |
| **HnVue.Workflow** | 14 | 7 | ~30% | ❌ | Generator/Detector 하드웨어 어댑터 스텁만 존재 |
| **HnVue.PatientManagement** | 10 | 3 | ~50% | ⚠ | MWL C-FIND, HL7 ADT/ORM 파싱 미구현 |
| **HnVue.Dose** | 9 | 1 | ~50% | ❌ | DRL 한도 강제, EI/DI 경보 UI 미완 |
| **HnVue.Imaging** | 7 | 1 | ~35% | ❌ | Gain/Offset 보정, 노이즈 감소 Workflow 통합 불명확 |
| **HnVue.Detector** | 9 | 1 | ~20% | ❌ | 벤더 SDK 스텁만 존재, GigE Vision/USB3 드라이버 없음 |
| **HnVue.SystemAdmin** | 9 | 1 | ~25% | ❌ | 인터페이스 정의만, 실제 구현체 거의 없음 |
| **HnVue.CDBurning** | 11 | 3 | ~55% | ⚠ | 뷰어 번들 통합 불명확 |
| **전체** | **331** | **59** | **~45%** | — | WBS 30~35% 완료 추정 |

---

## 4. 인터페이스 계약 구현 현황

SAD §6에 정의된 24개 내부 인터페이스 이행률:

| 구분 | 개수 | 비율 |
|------|------|------|
| 구현 완료 | 4 | 17% |
| 부분 구현 | 5 | 21% |
| 미구현 | 15 | 62% |

### 4.1 미구현 핵심 인터페이스

| 인터페이스 ID | 소스 모듈 | 대상 모듈 | 미구현 이유 |
|-------------|---------|---------|-----------|
| IF-WF-002 | WorkflowEngine | Generator Adapter | 하드웨어 어댑터 미구현 |
| IF-WF-003 | WorkflowEngine | Detector Adapter | 하드웨어 어댑터 미구현 |
| IF-WF-004 | WorkflowEngine | DoseManagement | 도즈 인터락 API 없음 |
| IF-WF-005 | WorkflowEngine | ImageProcessing | 이미지 파이프라인 트리거 없음 |
| IF-WF-006 | WorkflowEngine | DICOMCommunication | MPPS 업데이트 없음 |
| IF-WF-007 | WorkflowEngine | DataPersistence | 검사 레코드 저장 없음 |
| IF-DM-001 | DoseManagement | WorkflowEngine | 도즈 인터락 역방향 API 없음 |
| IF-DM-004 | DoseManagement | DICOMCommunication | RDSR 전송 없음 |
| IF-CS-001 | SecurityModule | 전체 모듈 | 인가 체크 API 미적용 |
| IF-SA-002 | SystemAdmin | SecurityModule | RBAC 강제 적용 없음 |

---

## 5. 스펙 대비 핵심 Gap 분석

### 5.1 CRITICAL — 제품 완성 차단

| Gap ID | SWR | 내용 | 영향 |
|--------|-----|------|------|
| **GAP-WF-001** | SWR-WF-018/019 | Generator RS-232/TCP 프로토콜 어댑터 미구현 | 실제 X선 촬영 불가 |
| **GAP-WF-002** | SWR-WF-024 | Detector GigE Vision/USB3 드라이버 미구현 | FPD (Flat Panel Detector) 이미지 수집 불가 |
| **GAP-CS-001** | SWR-CS-080 | PHI (Protected Health Information) AES (Advanced Encryption Standard)-256-GCM 암호화 미구현 | HIPAA (Health Insurance Portability and Accountability Act)/GDPR (General Data Protection Regulation) 위반 |
| **GAP-CS-002** | SWR-SA-072/073 | IHE (Integrating the Healthcare Enterprise) ATNA (Audit Trail and Node Authentication) 감사 추적 + Merkle 체인 미완 | FDA (Food and Drug Administration) 21 CFR 820 위반 |
| **GAP-DM-001** | SWR-DM-042/048 | 도즈 인터락 강제 + DRL (Diagnostic Reference Level) 한도 UI 미완 | 환자 안전 위해 가능성 |

### 5.2 HIGH — 기능 불완전

| Gap ID | SWR | 내용 |
|--------|-----|------|
| **GAP-PM-001** | IF-PM-003 | MWL (Modality Worklist) C-FIND + HL7 (Health Level 7) ADT (Admission Discharge Transfer)/ORM (Order Message) 파싱 미구현 |
| **GAP-SA-001** | IF-SA-002 | RBAC (Role-Based Access Control) 강제 적용 로직 거의 없음 |
| **GAP-IP-001** | SWR-IP-039 | Gain/Offset 보정 → Workflow 파이프라인 통합 불명확 |
| **GAP-DC-001** | IF-DC-003 | DICOM (Digital Imaging and Communications in Medicine) TLS (Transport Layer Security) 인증서 관리 미구현 |
| **GAP-SA-002** | SWR-SA-067/068 | 보정(Calibration) 만료 강제 검사 없음 |
| **GAP-CY-001** | SWR-CS-082/084 | SBOM (Software Bill of Materials, CycloneDX) + EV (Extended Validation) 코드 서명 없음 — FDA §524B 미충족 |

### 5.3 MEDIUM — 테스트/검증 부족

| Gap ID | 내용 |
|--------|------|
| **GAP-TEST-001** | 테스트 파일 비율 18% — 의료기기 기준 40~60% 필요 (IEC 62304 §5.5.4) |
| **GAP-TEST-002** | 시스템 테스트 0%, 수용 테스트 0% |
| **GAP-TEST-003** | HnVue.Common Result\<T\> 모나드 — 단위 테스트 없음 (600+ 호출처) |
| **GAP-TEST-004** | HnVue.App DI 그래프 통합 테스트 없음 |
| **GAP-IF-001** | SAD §6 인터페이스 24개 중 17%만 구현, 62% 미구현 |

---

## 6. 테스트 커버리지 현황

### 6.1 모듈별 테스트 수준

| 수준 | 모듈 | 상태 |
|------|------|------|
| **충실 (3+ 테스트 파일)** | Update(8), UI(13), Workflow(7), Dicom(7), Security(6), Incident(5), Data(4) | ✅ |
| **최소 (1~3 테스트 파일)** | CDBurning(3), PatientManagement(3), Dose(1), Imaging(1), Detector(1), SystemAdmin(1) | ⚠ |
| **없음** | Common(0), App(0), UI.Contracts(0), UI.ViewModels(0) | ❌ |

### 6.2 테스트 계획 대비 이행률

| 테스트 레벨 | 계획 TC | 구현 TC | 이행률 |
|-----------|--------|--------|--------|
| 단위 테스트 | ~100 | ~32 | 32% |
| 통합 테스트 | ~30 | ~5 | 17% |
| 시스템 테스트 | ~25 | 0 | 0% |
| 수용 테스트 | ~20 | 0 | 0% |
| **합계** | **~175** | **~37** | **~21%** |

---

## 7. 교정 계획

### Phase A — 즉시 착수 (제품 완성 차단 해소, ~6주)

| 우선순위 | Gap ID | 작업 | 대상 파일 |
|---------|--------|------|---------|
| P1 | GAP-WF-001 | GeneratorAdapter RS-232/TCP 구현 | `src/HnVue.Workflow/Adapters/GeneratorAdapter.cs` (신규) |
| P1 | GAP-WF-002 | DetectorHardwareAdapter GigE Vision 구현 | `src/HnVue.Detector/DetectorHardwareAdapter.cs` (신규) |
| P1 | GAP-CS-001 | PhiEncryptionService AES-256-GCM 구현 | `src/HnVue.Security/Encryption/PhiEncryptionService.cs` (신규) |
| P1 | GAP-CS-002 | MerkleAuditRepository IHE ATNA 완성 | `src/HnVue.Security/Audit/MerkleAuditRepository.cs` (신규) |
| P1 | GAP-DM-001 | 도즈 인터락 UI 경보 + DRL 한도 강제 | `src/HnVue.Dose/`, `src/HnVue.UI/` |

### Phase B — 단기 (기능 완성, 4~6주)

| Gap ID | 작업 |
|--------|------|
| GAP-PM-001 | MWL C-FIND 쿼리 + HL7 ADT/ORM 파서 구현 |
| GAP-SA-001 | SystemAdmin RBAC 실제 적용 (IF-SA-002) |
| GAP-IP-001 | Imaging 파이프라인 Workflow 통합 (IF-WF-005) |
| GAP-DC-001 | DICOM TLS 인증서 관리 UI |
| GAP-SA-002 | Calibration 만료 검사 로직 |

### Phase C — 중기 (품질/인허가 준비, 6~10주)

| 작업 | 목표 |
|------|------|
| HnVue.Common Result\<T\> 단위 테스트 | 커버리지 90% |
| HnVue.Dose 알고리즘 경계값 테스트 | IEC 62304 §5.5.4 충족 |
| HnVue.App DI 통합 테스트 | BuildHost() 검증 |
| 통합 테스트: Workflow ↔ Generator 시뮬레이터 | IT-WF-Generator 5개 시나리오 |
| 통합 테스트: PACS C-STORE/C-FIND 멀티벤더 | IT-DC-PACS 완성 |
| SBOM CycloneDX 생성 자동화 + EV 코드 서명 | GAP-CY-001 |
| 시스템 테스트 (성능/신뢰성/보안) 실행 | ST-Performance, ST-Security |
| DHF (Design History File) 완성 — V&V 결과, RTM 100% | 인허가 제출 준비 |

---

## 8. 종합 진단

| 평가 항목 | 등급 | 점수 |
|----------|------|------|
| **아키텍처 설계 품질** | 우수 — Clean Architecture 완벽 준수, 스파게티 없음 | A |
| **코드 구조 / 의존성** | 우수 — 순환 의존성 없음, God Class 없음, DI 완전 적용 | A |
| **MVVM 패턴 적용** | 우수 — CommunityToolkit.Mvvm, UI Contracts 분리 | A |
| **기능 구현 완성도** | 미흡 — ~45% (하드웨어 어댑터가 가장 큰 공백) | D |
| **테스트 커버리지** | 미흡 — 18% (의료기기 기준 40~60% 필요) | D |
| **규정 준수 준비도** | 미흡 — PHI 암호화, 감사 추적 미완 | D |
| **인터페이스 계약 이행** | 미흡 — 24개 중 17%만 구현 | D |

> **핵심 결론**: 코드 아키텍처는 탄탄하다. 그러나 **실제 제품 완성도는 약 45% 수준**이다.  
> Generator/Detector 하드웨어 어댑터 미구현이 가장 큰 차단 요소이며, PHI 암호화와 IHE ATNA 감사 추적은 규정 준수를 위해 Phase A에서 반드시 선행되어야 한다.  
> 테스트 커버리지(현재 18%)는 IEC 62304 Class B 인허가 기준에 크게 미달하며, Phase C에서 집중 개선이 필요하다.

---

*이 문서는 자동화된 정적 분석(MoAI Agent)을 통해 생성되었으며, 인허가 목적의 공식 검증 보고서가 아닙니다.*
