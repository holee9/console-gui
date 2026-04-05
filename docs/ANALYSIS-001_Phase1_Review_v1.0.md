# HnVue Console SW — Phase 1 현황 분석 보고서

| 항목 | 내용 |
|------|------|
| **문서 ID** | ANALYSIS-001 |
| **버전** | v1.0 |
| **작성일** | 2026-04-05 |
| **분석 대상** | HnVue Console SW (Phase 1 전체) |
| **분석 범위** | 소스코드 14개 모듈 + 규제문서 70개 + 기존 제품 문서 교차검증 |
| **결론** | **1차 릴리즈 불가 — 2차 업그레이드 작업 필요** |

---

## 목차

1. [핵심 결론](#1-핵심-결론)
2. [강점 분석](#2-강점-분석)
3. [Critical Blockers](#3-critical-blockers)
4. [종합 현황 매트릭스](#4-종합-현황-매트릭스)
5. [문서-코드 정합성 문제](#5-문서-코드-정합성-문제)
6. [2차 업그레이드 작업 계획](#6-2차-업그레이드-작업-계획)
7. [결론 및 권고사항](#7-결론-및-권고사항)

---

## 1. 핵심 결론

**1차 릴리즈 불가. 2차 업그레이드 작업 필수.**

현재 상태는 아키텍처와 규제 문서 프레임워크는 우수하나, 두 가지 근본적인 문제가 존재한다:

1. **핵심 기능 미완성**: 의료 영상 콘솔의 핵심인 Image Processing과 WPF UI 화면이 Stub 수준
2. **문서-코드 불일치**: 규제 문서(시험 보고서)가 실제 구현되지 않은 기능에 대해 Pass를 기재

> 참고: 이 레포지토리는 기존 HnVUE 제품(`docs/` 내 실제 매뉴얼, 성능시험보고서 존재)의 신규 버전을 개발하는 Greenfield 프로젝트다. 기존 제품과의 Gap 분석이 아직 수행되지 않았다.

---

## 2. 강점 분석

| 영역 | 세부 내용 | 평가 |
|------|----------|------|
| **Architecture** | Clean Architecture 6계층, 14개 모듈 명확한 의존성 그래프 | 우수 |
| **Security** | JWT HS256, bcrypt cost=12, RBAC 4계층, SQLCipher AES-256, HMAC-SHA256 Audit Chain | 우수 |
| **Unit Testing** | 523개 Tests 전체 통과, 안전임계 모듈 90%+ Coverage | 우수 |
| **Regulatory Framework** | 70개 문서, IEC 62304/ISO 14971/FDA 21 CFR 820/FDA §524B 매핑 완료 | 우수 |
| **Traceability** | MR→PR→SWR→TC 100% 양방향 추적 (CVR-002 검증 완료) | 우수 |
| **Code Documentation** | 257/257 Public Members XML Doc Comments 완비, 14개 모듈 README | 우수 |
| **Result<T> Pattern** | Railway-Oriented Programming 일관 적용, 에러 처리 체계화 | 우수 |

---

## 3. Critical Blockers

### 3-1. HnVue.Imaging — 핵심 기능 Stub 수준

`src/HnVue.Imaging/ImageProcessor.cs` 실제 구현 내용:

```csharp
// 실제 코드 (stub 수준)
var side = (int)Math.Sqrt(pixelCount);  // 파일 크기 제곱근으로 이미지 크기 추정
// "a production implementation would parse the DICOM/proprietary header" — 소스 주석
```

| 항목 | 현재 상태 | 요구 상태 |
|------|----------|----------|
| DICOM 이미지 파싱 | 파일 바이트 직접 처리 (Stub) | fo-dicom 기반 실제 DICOM Header 파싱 |
| 이미지 크기 계산 | 바이트 수의 제곱근 추정 | DICOM Tag (0028,0010/0011) 읽기 |
| Pan 연산 | 오프셋 없이 동일 이미지 반환 | 실제 픽셀 오프셋 적용 |
| 16-bit Grayscale | 미지원 | X-ray DR/CR 표준 Pixel Depth |
| GSDF LUT | 미구현 | DICOM Grayscale Standard Display Function |

### 3-2. WPF UI — 실제 화면 부재

```
src/HnVue.UI/Views/
  LoginView.xaml          ← 존재 (로그인만)

# 없는 화면들:
  PatientListView.xaml    ← 없음 (환자 목록)
  WorkflowView.xaml       ← 없음 (촬영 워크플로우)
  ImageViewerView.xaml    ← 없음 (영상 뷰어 — 핵심!)
  DoseMonitorView.xaml    ← 없음 (선량 모니터)
```

로그인 후 사용자가 볼 수 있는 화면이 없다. `MainViewModel.cs`가 존재하나 View와 연결되지 않은 상태.

### 3-3. 실제 하드웨어 연동 없음

| 구성요소 | 현재 상태 |
|---------|----------|
| X-ray Generator 통신 | `GeneratorSimulator.cs` (시뮬레이터만) |
| FPD (Flat Panel Detector) SDK | 연동 없음 |
| DICOM 네트워크 (C-STORE/C-FIND) | 구현됨 (실제 PACS 미검증) |

### 3-4. Production 보안 설정 미구현

다음 값들이 소스코드에 하드코딩된 상태로, 환경변수 교체 메커니즘이 미구현:

| 항목 | 파일 | 현재 값 (개발용) |
|------|------|----------------|
| JWT Secret | `JwtOptions.cs` | `"your-secret-key-at-least-32-characters"` |
| HMAC Key | `AuditService.cs` | `"default-hmac-key-for-development"` |
| DB Password | `appsettings.json` | `"dev-password-12345"` |

---

## 4. 종합 현황 매트릭스

| 영역 | 완성도 | 상태 | 비고 |
|------|:------:|------|------|
| Architecture / Clean Architecture | 90% | 우수 | 즉시 활용 가능 |
| Security (JWT, RBAC, Audit) | 85% | 양호 | Production 설정 교체 필요 |
| Data Layer (EF Core, Repository) | 80% | 양호 | |
| DICOM Communication | 75% | 보통 | 실제 PACS 환경 미검증 |
| Workflow Engine (State Machine) | 80% | 양호 | |
| **Image Processing (핵심)** | **15%** | **미완성** | Stub 수준 |
| **WPF UI Screens** | **20%** | **미완성** | Login만 존재 |
| Hardware Integration | 10% | 미완성 | Simulator만 |
| Regulatory Document Framework | 85% | 양호 | 서명 및 실제 데이터 필요 |
| **Test Reports (실제 데이터)** | **30%** | **불일치** | 아래 §5 참조 |
| Production Deployment | 20% | 미완성 | 환경변수, 설치 패키지 없음 |

**1차 릴리즈 준비도 종합: 약 40-45%**

---

## 5. 문서-코드 정합성 문제

### 5-1. 시험 보고서에 존재하지 않는 기능이 Pass로 기재됨

DOC-022 (Unit Test Report, v1.0) 기재 내용과 실제 코드 비교:

| 시험 보고서 기재 (Pass) | 실제 코드 상태 |
|----------------------|--------------|
| UT-IP-013: JPEG-LS Lossless 압축/해제 | `HnVue.Imaging`에 JPEG-LS 없음 |
| UT-PM-012: FHIR Patient 리소스 변환 | FHIR 구현 없음 |
| UT-WF-014: Generator 응답 Timeout | 실제 Generator 통신 없음 (Simulator만) |
| UT-WF-006: Generator CRC 검증 | 실제 Generator 프로토콜 없음 |
| UT-DC: MPPS N-SET | 코드에서 MPPS 확인 필요 |

> **규제 위험**: FDA/MFDS 심사 시 시험 보고서와 실제 코드가 불일치하면 즉각 AI(Additional Information) 요청 또는 거절 사유가 된다.

### 5-2. 시험 보고서 크기 문제

| 문서 | 파일 크기 | 실제 시험 보고서 예상 크기 |
|------|---------|------------------------|
| DOC-022 (Unit Test Report) | 6.2 KB | 최소 50-100 KB |
| DOC-023 (Integration Test Report) | 5.2 KB | 최소 30-50 KB |
| DOC-024 (System Test Report) | 3.5 KB | 최소 50-100 KB |
| DOC-026 (Cybersecurity Test Report) | 2.7 KB | 최소 30-50 KB |

시험 보고서 모두 Template 수준의 용량 — 실제 시험 수행 데이터(Log, Screenshot, 환경 정보)가 없음.

### 5-3. 기존 실제 제품 문서와의 관계

`docs/` 폴더에 이미 존재하는 실제 HnVUE 제품 문서들:

| 파일명 | 내용 | 작성일 |
|--------|------|--------|
| `★HnVUE UI 변경 최종안_251118.pptx` | 실제 UI 설계 최종안 | 2025-11-18 |
| `Instructions for Use(EN) HnVUE 250714(공식메뉴얼).docx` | 공식 영문 사용설명서 | 2025-07-14 |
| `3. [HnVUE] Performance Test Report (A-PTR-HNV).docx` | 실제 성능시험 보고서 | 미상 |
| `API_MANUAL_241206.pdf` | API 매뉴얼 | 2024-12-06 |

이 파일들은 이미 출하 중인 HnVUE 제품의 문서다. 신규 Console-GUI 프로젝트가 기존 제품의 어떤 기능을 재구현했는지, 무엇이 누락되었는지에 대한 **Gap 분석이 아직 없다**.

---

## 6. 2차 업그레이드 작업 계획

### Priority 1 — Release Blocker (착수 필수)

| # | 작업 | 담당 모듈 | 예상 규모 |
|---|------|----------|---------|
| 1 | **HnVue.Imaging 실제 구현** | `HnVue.Imaging` | 대 (3-4M) |
| | fo-dicom 기반 실제 DICOM Header 파싱 | | |
| | 16-bit Grayscale Pixel 렌더링 | | |
| | W/L, Pan, Zoom 실제 적용 | | |
| 2 | **WPF UI 화면 완성** | `HnVue.UI` | 대 (3-4M) |
| | PatientListView (환자 목록) | | |
| | WorkflowView (촬영 워크플로우) | | |
| | ImageViewerView (영상 뷰어) | | |
| | DoseMonitorView (선량 모니터) | | |
| 3 | **기존 HnVUE vs 신규 코드 Gap 분석** | 전체 | 소 (2W) |
| | 공식 매뉴얼 기반 기능 목록 추출 | | |
| | 신규 코드 구현 여부 매핑 | | |

### Priority 2 — 릴리즈 전 필수

| # | 작업 | 세부 내용 |
|---|------|----------|
| 4 | **시험 보고서 현실화** | 실제 523개 xUnit 테스트 기반으로 DOC-022~028 재작성 |
| 5 | **Hardware Integration** | FPD SDK 연동, Generator 실제 프로토콜 구현 |
| 6 | **Production 보안 설정** | 환경변수 기반 Secret 관리, `appsettings.Production.json` |
| 7 | **DICOM Network 실환경 검증** | 실제 PACS (DCM4CHEE 또는 고객 PACS)와 C-STORE/C-FIND 검증 |

### Priority 3 — 인허가 제출 전

| # | 작업 | 세부 내용 |
|---|------|----------|
| 8 | **DOC-044 Known Anomalies** | 실제 시험 후 결함 목록 작성 및 QA/RA 서명 |
| 9 | **DHF (DOC-035) 완성** | 설계 검토 기록, 실제 서명 |
| 10 | **KTL 사이버보안 모의침투** | IEC 81001-5-1 Independent Testing |
| 11 | **SBOM 최종 빌드 기준 갱신** | CycloneDX 자동 생성 파이프라인 구축 |

---

## 7. 결론 및 권고사항

### 현재 상태 한 문장 요약

> 의료기기 Software의 Architecture, Security, Testing Infrastructure는 매우 잘 구축되었으나, 정작 제품의 핵심인 Image Processing과 UI가 미완성이며, 규제 문서와 실제 코드 간 정합성이 확보되지 않아 어떤 형태의 릴리즈도 현재는 불가능하다.

### 권고사항

1. **전략적 재정의 우선**: 기존 HnVUE 제품이 동작 중이므로, 신규 Console-GUI 프로젝트의 목표와 범위를 명확히 재정의할 것
2. **Gap 분석 즉시 수행**: 공식 매뉴얼(`Instructions for Use(EN) HnVUE 250714`)을 기준으로 신규 코드와의 기능 Gap 매핑
3. **문서-코드 정합성 복원**: 시험 보고서를 실제 구현된 기능 기준으로 재작성 (현재 불일치는 규제 리스크)
4. **2명 개발자 현실적 일정**: Priority 1~2 작업만으로 최소 6~9개월 추가 필요

### 인허가 타임라인 재검토

DOC-034 (Release Document)에 명시된 릴리즈 예정일: **2026-09-01**

현재 진행 상황을 고려하면:
- Priority 1 작업 (Imaging + UI): 최소 6개월
- Priority 2 작업 (Hardware + 시험 재수행): 3개월
- Priority 3 작업 (인허가 문서 완성): 3개월

**현실적 릴리즈 예상일: 2027년 Q2~Q3**

---

*문서 끝 (End of Document)*

| 항목 | 내용 |
|------|------|
| 분석자 | abyz-lab |
| 분석일 | 2026-04-05 |
| 기반 데이터 | 소스코드 전수 검토 + 규제문서 교차검증 |
| 주의사항 | 본 분석은 AI 보조 분석으로, 최종 판단은 프로젝트 책임자가 수행해야 함 |
