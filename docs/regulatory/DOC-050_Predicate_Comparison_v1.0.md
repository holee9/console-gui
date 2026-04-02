# Predicate Device 비교표 (Substantial Equivalence Comparison)
## HnVue Console SW (HnVue) — FDA 510(k) Premarket Notification

---

## 문서 메타데이터 (Document Metadata)

| 항목 | 내용 |
|------|------|
| **문서 번호** | DOC-050 |
| **버전** | v1.0 |
| **작성일** | 2026-03-31 |
| **작성자** | RA 팀 (Regulatory Affairs) |
| **검토자** | 임상/RA 선임, SW 아키텍트 |
| **승인자** | 의료기기 RA/QA 책임자 |
| **제품** | HnVue Console SW (HnVue) |
| **회사** | HnVue (가칭) |
| **분류** | 필수 — FDA 510(k) 심사에서 실질적 동등성(SE) 입증의 핵심 문서 |
| **적용 시장** | FDA 510(k) 전용 |
| **근거 규격** | 21 CFR §807.87(f), FDA 510(k) Substantial Equivalence Decision Making Process Guidance (2014), FDA eSTAR v6.1 Section 10, FDA Guidance "Factors to Consider When Making Benefit-Risk Determinations in Medical Device Premarket Approval and De Novo Classifications" |

### 개정 이력 (Revision History)

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| v1.0 | 2026-03-31 | 최초 작성 — Predicate 후보 식별 및 비교 프레임워크 수립 (개발 착수 전 계획서 수준) | RA 팀 |

> **중요**: 본 문서는 개발 착수 전 계획서 수준으로 작성됨. Predicate Device의 실제 510(k) 번호 및 허가 세부 정보는 FDA 510(k) 데이터베이스(https://www.accessdata.fda.gov/scripts/cdrh/cfdocs/cfpmn/pmn.cfm) 조회를 통해 확인 후 [TBD] 항목을 채워야 한다.

---

## 1. 문서 정보

### 1.1 문서 목적

FDA eSTAR 510(k) 제출을 위해 HnVue Console SW (Subject Device)와 Predicate Device 후보군의 의도된 사용 및 기술 특성 비교를 정형화된 표 형식으로 제공하여, FDA 심사관이 실질적 동등성(Substantial Equivalence, SE)을 신속하게 확인할 수 있도록 한다.

본 문서는 DOC-036 (FDA 510(k) eSTAR_v1.0.md)의 eSTAR v6.1 Section 10 (Substantial Equivalence Comparison)에 직접 삽입·참조되는 핵심 문서이다.

### 1.2 실질적 동등성 판단 기준 (21 CFR §807.87(f))

FDA는 다음 두 조건을 모두 충족할 때 SE를 인정한다:

1. Subject Device와 Predicate Device가 **동일한 의도된 사용(Intended Use)**을 가짐
2. Subject Device와 Predicate Device가 **동일하거나 다른 기술 특성**을 가지되, 다른 경우:
   - ① 새로운 안전성/유효성 문제를 제기하지 않고
   - ② 적어도 Predicate Device만큼 안전하고 효과적임을 데이터로 지지

### 1.3 Split Predicate 전략 (해당 시 적용)

| 전략 유형 | 적용 여부 | 설명 |
|----------|----------|------|
| Single Predicate | **주 전략** | EConsole1 (DRTECH Corp., K231225)를 단일 Predicate로 활용하여 의도된 사용 및 기술 특성 모두 비교 |
| Split Predicate | **보완 전략** | 단일 Predicate로 기술 특성 일부를 설명하기 어려울 경우, VXvue를 보조 Predicate로 추가 |

> Split Predicate 전략 사용 시 FDA Pre-Submission(Pre-Sub) 미팅을 통해 사전 확인 권장. (참조: FDA Pre-Sub Program Guidance, 2019)

---

## 2. Subject Device (제출 제품) 요약

| 항목 | 내용 |
|------|------|
| **제품명** | HnVue Console SW (HnVue) |
| **제조사** | HnVue (가칭) |
| **제조 국가** | 대한민국 |
| **제품 코드** | LLZ (Picture Archiving and Communications System) |
| **분류 규정** | 21 CFR §892.2050 |
| **분류 등급** | Class II |
| **제출 유형** | 510(k) Premarket Notification |
| **SW 안전 분류** | IEC 62304 Class B / FDA Moderate Level of Concern (DOC-036 §3 참조) |
| **SW 버전** | [TBD - 개발 완료 후 작성] |
| **의도된 사용 (Intended Use)** | HnVue Console SW (HnVue)는 면허를 보유한 의료 전문가(방사선사, 영상의학과 전문의)가 디지털 방사선(Digital Radiography, DR) 검출기(FPD: Flat Panel Detector)로 취득한 진단용 X-ray 영상을 획득, 표시, 처리(후처리 기능), 저장 및 PACS로 전송하기 위한 처방용(Rx Only) 의료기기 소프트웨어이다. |
| **의도된 사용 (Indications for Use)** | 일반 진단 X-ray 촬영 (흉부, 복부, 사지, 척추 등); 성인 및 소아 환자; 병원 촬영실(촬영용) 및 영상 판독실(판독용) 환경 |
| **연계 하드웨어** | HnVue FPD (Flat Panel Detector) 번들; X-ray Generator 제어 인터페이스 |

---

## 3. Predicate Device 후보군 요약

### 3.1 Predicate 선정 기준

Predicate Device는 다음 기준을 충족하는 제품으로 선정한다:

1. **의도된 사용 동일성**: DR X-ray 콘솔 소프트웨어로서 영상 획득/표시/처리/저장/전송 기능 포함
2. **제품 코드 동일**: LLZ (또는 유사 분류 코드)
3. **분류 규정 동일**: 21 CFR §892.2050
4. **허가 연도**: 가능한 한 최근 허가 제품을 주 Predicate로 선택 (현재 규제 기준 반영)
5. **제조 환경 유사성**: 한국 제조 DR 소프트웨어 우선 고려 (유사 시장 환경)

### 3.2 Predicate 후보 목록 및 전략

| 항목 | Predicate 1 (주) | Predicate 2 (보조) | Predicate 3 (참조) |
|------|-----------------|-------------------|-------------------|
| **제품명** | **EConsole1** | **[TBD - Predicate 2 제품명]** | **[TBD - Predicate 3 제품명]** |
| **제조사** | DRTECH Corp. | [TBD] | [TBD] |
| **510(k) 번호** | **K231225** | **[TBD]** | **[TBD]** |
| **허가 일자** | [TBD - FDA DB 확인] | [TBD] | [TBD] |
| **제품 코드** | LLZ | LLZ | LLZ |
| **분류 규정** | 21 CFR §892.2050 | 21 CFR §892.2050 | 21 CFR §892.2050 |
| **제조 국가** | 대한민국 | [TBD] | [TBD] |
| **활용 전략** | 주 Predicate (의도된 사용 + 기술 특성 전체) | 보조 Predicate (Split Predicate 시 활용) | 기술 특성 보완 참조 |

> **Predicate 선정 작업 지침 (RA 팀)**:
> 1. FDA 510(k) 데이터베이스 (https://www.accessdata.fda.gov/scripts/cdrh/cfdocs/cfpmn/pmn.cfm) 에서 Product Code "LLZ" 또는 "QKQ" 검색
> 2. "Picture Archiving and Communications System" 또는 "Digital Radiography" 관련 최근 3년 이내 허가 제품 검색
> 3. 한국 제조 DR 콘솔 SW (DRTECH, Vieworks, Rayence 등) 가운데 FDA 510(k) 허가 취득 제품 우선 검토
> 4. 선정된 Predicate의 Decision Summary 및 Summary of Safety and Effectiveness Data 원문 확인 필수
> 5. 위 표의 [TBD] 항목을 실제 데이터로 업데이트 후 본 문서 개정 (v1.1 이상)

#### 3.2.1 Predicate 후보 참조 리스트 (검토 대상 — 최종 선정 전)

아래는 FDA 510(k) 데이터베이스에서 유사 제품으로 확인된 후보군이다. **실제 Predicate 선정 전 원문 확인 필수**:

| 후보 제품명 | 제조사 | 예상 제품 코드 | 비고 |
|-----------|-------|--------------|------|
| EConsole1 (디지털 방사선 콘솔) | DRTECH Corp. | LLZ | 한국 제조; **K231225** (주 Predicate 선정) |
| VXvue (DR 영상 SW) | Vieworks Co., Ltd. | LLZ | 한국 제조; 최근 허가 이력 확인 필요 |
| XmaruView (DR 워크스테이션 SW) | Rayence Co., Ltd. | LLZ | 한국 제조; K160579 등 다수 취득 이력 |
| 기타 DR 콘솔 SW | [FDA 데이터베이스 검색] | LLZ | [TBD - 검색 후 추가] |

---

## 4. 의도된 사용 비교

### 4.1 의도된 사용 비교 테이블

| 비교 항목 | HnVue Console SW | Predicate 1 (EConsole1, K231225) | Predicate 2 ([TBD]) | SE 판정 |
|----------|-----------------|----------------------------------|---------------------|--------|
| **의도된 사용 핵심 문구** | DR X-ray 영상 획득·표시·처리·저장·PACS 전송 | DR X-ray 영상 획득·표시·처리·저장·PACS 전송 | [TBD] | ✅ 동등 예상 |
| **처방 목적** | 진단 보조 (진단 결정은 의료인 담당) | 진단 보조 | 진단 보조 | ✅ 동등 예상 |
| **영상 모달리티** | Digital Radiography (DR), X-ray | DR, X-ray | DR, X-ray | ✅ 동등 예상 |
| **적응증 (해부학적 부위)** | 전신 — 흉부, 복부, 사지, 척추 등 성인/소아 일반 진단 방사선 촬영 | 전신 | 전신 | ✅ 동등 예상 |
| **대상 환자** | 성인 및 소아 | [TBD] | [TBD] | [TBD] |
| **대상 사용자** | 면허를 보유한 방사선사(Radiologic Technologist), 영상의학과 전문의(Radiologist) | 의료 전문가 | 의료 전문가 | ✅ 동등 예상 |
| **처방용/OTC** | Rx Only (처방용) | Rx Only | Rx Only | ✅ 동등 예상 |
| **사용 환경** | 병원 촬영실 (영상 획득), 영상 판독실 (판독) | 병원/클리닉 | 병원/클리닉 | ✅ 동등 예상 |
| **연계 하드웨어** | HnVue FPD (DR 검출기); 전용 번들 또는 호환 DR 시스템 | [TBD] | [TBD] | [TBD] |

**의도된 사용 비교 결론**: [TBD - Predicate 510(k) 번호 확인 및 원문 비교 후 기재. 예상: HnVue Console SW는 Predicate Device와 의도된 사용이 동일함.]

### 4.2 의도된 사용 원문 (Subject Device)

FDA eSTAR 제출용 공식 Intended Use 문구 (영문):

> *"HnVue Console SW (HnVue) is a prescription-use-only medical device software intended for use by licensed healthcare professionals (radiologic technologists and radiologists) to acquire, display, process (including post-processing functions), store, and transmit diagnostic digital radiography (DR) X-ray images to a Picture Archiving and Communication System (PACS). The device is intended for use in hospital radiology departments for general diagnostic X-ray imaging of the chest, abdomen, extremities, and spine in adult and pediatric patients."*

FDA eSTAR 제출용 공식 Indications for Use 문구 (영문):

> *"HnVue Console SW (HnVue) is indicated for use as a digital radiography acquisition and display console software, designed to interface with flat panel detector (FPD)-based X-ray imaging systems. The software acquires, displays, processes, archives, and transmits diagnostic-quality X-ray images. It is intended for use by licensed radiologic technologists and radiologists for general diagnostic radiographic examinations in adults and pediatric patients in a hospital or clinical environment."*

---

## 5. 기술 특성 비교

### 5.1 기술 특성 비교 테이블 (주 Predicate 비교)

| 기술 특성 항목 | HnVue Console SW | Predicate 1 ([TBD]) | 차이 여부 | SE 판정 |
|--------------|-----------------|---------------------|---------|--------|
| **SW 유형** | 독립형 SW (Stand-alone Software, SaMD) | [TBD] | [TBD] | [TBD] |
| **운영 플랫폼** | Windows 10/11 IoT Enterprise (64-bit) | [TBD] | [TBD] | [TBD] |
| **DICOM 표준 지원** | DICOM 3.0 — C-STORE, C-FIND, C-MOVE, C-GET, Modality Worklist, MPPS | [TBD] | [TBD] | [TBD] |
| **영상 취득 (Acquisition)** | DR FPD 검출기 연동 영상 취득 지원 (AED, Triggered, Manual mode) | [TBD] | [TBD] | [TBD] |
| **영상 표시 (Display)** | 고해상도 DICOM 영상 표시 (최대 [TBD] MP 해상도 지원) | [TBD] | [TBD] | [TBD] |
| **영상 후처리 기능** | 윈도우/레벨(W/L) 조정, 줌/패닝, 회전/반전, 샤프닝 필터, 노이즈 감소 필터 | [TBD] | [TBD] | [TBD] |
| **측정 기능** | 길이 측정, 각도 측정, 면적 측정, ROI 통계 | [TBD] | [TBD] | [TBD] |
| **환자/검사 관리** | Modality Worklist (MWL) 기반 환자 정보 수신, 검사 관리, 영상 이력 조회 | [TBD] | [TBD] | [TBD] |
| **PACS 전송** | DICOM C-STORE via TCP/IP Ethernet (TLS 1.2+) | [TBD] | [TBD] | [TBD] |
| **촬영 프로토콜 관리** | 해부학적 부위별 촬영 프로토콜 (kVp, mAs, 부위 코드) | [TBD] | [TBD] | [TBD] |
| **선량 정보 관리** | DICOM Radiation Dose Structured Report (RDSR) 지원, DRL 경고 | [TBD] | [TBD] | [TBD] |
| **AI/ML 기능** | 없음 (No AI/ML Component — Phase 1) | [TBD] | [TBD] | [TBD] |
| **클라우드 기능** | 없음 (On-premise 전용 — Phase 1) | [TBD] | [TBD] | [TBD] |
| **UDI 지원** | UDI-DI / UDI-PI 지원 (EUDAMED 및 FDA GUDID 등록 예정) | [TBD] | [TBD] | [TBD] |
| **IEC 62304 분류** | Class B / FDA Moderate Level of Concern | [TBD] | [TBD] | [TBD] |
| **IEC 62366 사용적합성** | IEC 62366-1 적용 (Usability Engineering) | [TBD] | [TBD] | [TBD] |

### 5.2 사이버보안 기술 특성 상세 비교

| 사이버보안 항목 | HnVue Console SW | Predicate 1 ([TBD]) | 차이점 분석 |
|--------------|-----------------|---------------------|-----------|
| **암호화 통신** | TLS 1.2 이상 (DICOM TLS); TLS 1.3 지원 | [TBD] | HnVue는 TLS 1.3 추가 지원 — 차이 있으나 보안 강화 방향; 새로운 안전성 문제 없음 |
| **인증 방식** | RBAC (역할 기반 접근 통제) + 패스워드 정책 (8자+, 복잡성, 90일 만료) | [TBD] | [TBD] |
| **감사 로그** | 모든 임상 동작·접근 이벤트 감사 로그 기록 (ISO/IEC 27001 기준) | [TBD] | [TBD] |
| **SBOM 제공** | CycloneDX 형식 SBOM 제공 | [TBD] | FDA §524B 요건 충족; Predicate 미제공 시에도 추가된 기능으로 새로운 위험 없음 |
| **VEX 제공** | VEX (Vulnerability Exploitability eXchange) 제공 | [TBD] | FDA §524B 요건 충족 |
| **보안 패치 정책** | Critical ≤ 14일, High ≤ 30일, Medium ≤ 90일 SLA | [TBD] | [TBD] |
| **VDP (취약점 공시)** | 공개 VDP: security@hnvue.com, 90일 공시 유예 | [TBD] | [TBD] |
| **SW 무결성** | 배포 패키지 디지털 서명, 실행 시 무결성 검증 | [TBD] | [TBD] |

---

## 6. 차이점 분석 (Difference Analysis)

### 6.1 식별된 기술 특성 차이점 (예비 분석 — Predicate 확정 후 업데이트 필요)

아래는 예상 차이점이며, Predicate 510(k) 원문 확인 후 실제 차이점으로 업데이트한다.

| 차이점 ID | 차이점 항목 | HnVue Console SW | Predicate 1 (EConsole1, K231225) | 새로운 위험 여부 | 분석 |
|---------|-----------|-----------------|----------------------------------|--------------|------|
| DIFF-001 | TLS 버전 | TLS 1.2/1.3 양쪽 지원 | TLS 1.2 지원 (추정; K231225 내용 확인 필요) | ❌ 새로운 위험 없음 | TLS 1.3은 TLS 1.2 대비 보안 강화 버전. 하위 호환성 지원으로 기존 DICOM 인프라와 호환 가능. 성능 또는 안전성 저하 없음. |
| DIFF-002 | SBOM 제공 | CycloneDX 형식 SBOM 제공 | [TBD - K231225 SBOM 지원 여부 확인] | ❌ 새로운 위험 없음 | SBOM은 소프트웨어 구성 요소 투명성 향상 도구. 임상 위험 증가 요소 없음. FDA §524B 요건 충족. |
| DIFF-003 | VEX 제공 | VEX 파일 제공 | [TBD - K231225 VEX 지원 여부 확인] | ❌ 새로운 위험 없음 | VEX는 취약점 익스플로잇 가능성 평가 정보 제공. 투명성 향상이며 임상 위험 증가 없음. |
| DIFF-004 | 선량 정보 관리 | DICOM RDSR + DRL 경고 기능 | [TBD - K231225 DRL 지원 여부 확인] | [TBD - 확인 필요] | DRL 경고는 과다 피폭 방지 안전 기능. 새로운 위험 없음 (오히려 추가 안전장치). |
| DIFF-005 | CD/DVD Burning | CD/DVD Burning with DICOM Viewer 지원 (MR-072, Tier 2) | [TBD - K231225 지원 여부 확인] | ❌ 새로운 위험 없음 | CD/DVD Burning은 환자 배포용 이동식 미디어 기능. feel-DRCS 기본 기능 동등 수준. |
| DIFF-00X | [TBD - Predicate K231225 원문 확인 후 추가] | | | | |

### 6.2 차이점 분석 결론 (예비)

> "[TBD - Predicate 510(k) 번호 확인 및 Decision Summary 원문 비교 후 최종 기재.]
>
> **예비 결론**: 상기 식별된 기술 특성 차이점(DIFF-001~004)은 모두 HnVue Console SW의 보안 강화 또는 기능 추가에 해당하며, 새로운 안전성 또는 유효성 문제를 제기하지 않는다. 각 차이점은 Predicate Device 대비 동등하거나 더 안전한 성능을 제공한다. 특히 TLS 1.3 지원 및 SBOM 제공은 FDA §524B 사이버보안 요건을 충족하는 방향의 기능 강화이다."

---

## 7. 실질적 동등성 결론

### 7.1 SE 판정 요약 (예비 — Predicate 확정 후 최종 기재)

| SE 판정 기준 | 판정 결과 (예비) |
|------------|----------------|
| 동일한 의도된 사용 | [TBD - 동일 예상] |
| 동일하거나 안전성 문제 없는 기술 특성 | [TBD - 동일 또는 더 안전 예상] |
| 새로운 안전성/유효성 문제 없음 | [TBD - 없음 예상] |
| **최종 SE 판정** | **[TBD - 실질적으로 동등(Substantially Equivalent) 예상]** |

### 7.2 SE 결론 선언 (Predicate 확정 후 최종 기재)

> "[TBD - Predicate 510(k) 번호 및 제품명 확인 후 기재]
>
> **영문 (eSTAR 제출용)**:
>
> *"HnVue Console SW (HnVue) has the same intended use as [Predicate 1 제품명] ([Predicate 1 K번호]) [and [Predicate 2 제품명] ([Predicate 2 K번호])]. The differences in technological characteristics do not raise new questions of safety and effectiveness, as supported by the comparative analysis above. The technological differences represent enhancements in cybersecurity (TLS 1.3 support, SBOM provision) and do not adversely affect the safety or effectiveness of the device. Therefore, HnVue Console SW (HnVue) is Substantially Equivalent to the predicate device(s)."*"

---

## 8. Predicate 510(k) 요약 정보

*FDA 510(k) 데이터베이스 (https://www.accessdata.fda.gov/scripts/cdrh/cfdocs/cfpmn/pmn.cfm) 조회 후 작성*

| 항목 | Predicate 1 (EConsole1) | Predicate 2 ([TBD]) |
|------|------------------------|---------------------|
| **510(k) 번호** | **K231225** | [TBD] |
| **제품명** | EConsole1 | [TBD] |
| **제조사** | DRTECH Corp. | [TBD] |
| **Decision Date** | [TBD - FDA DB 확인] | [TBD] |
| **Decision** | Substantially Equivalent | Substantially Equivalent |
| **Primary Predicate (of Predicate)** | [TBD - K번호] | [TBD - K번환] |
| **Review Time (일수)** | [TBD] | [TBD] |
| **Decision Summary URL** | https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf | [TBD] |
| **Intended Use (원문 요약)** | DR X-ray 콘솔 SW — 영상 획득·표시·저장·PACS 전송 (한국 제조 DR 콘솔) | [TBD] |
| **Product Code** | LLZ | LLZ |
| **Regulation Number** | 21 CFR §892.2050 | 21 CFR §892.2050 |

---

## 9. 성능 데이터 비교 (해당 시)

### 9.1 영상 품질 관련 성능 비교

| 성능 항목 | HnVue Console SW | Predicate ([TBD]) | 비고 |
|---------|-----------------|------------------|------|
| 영상 표시 해상도 지원 | 최대 [TBD] MP | [TBD] | [TBD - 개발 완료 후 작성] |
| DICOM 영상 로딩 시간 | [TBD] 초 이내 (10 MP 기준) | [TBD] | [TBD - 검증 완료 후 작성] |
| 동시 접속 지원 (워크스테이션) | 단일 워크스테이션 (독립형) | [TBD] | [TBD] |
| 영상 취득→표시 지연 | [TBD] ms 이내 | [TBD] | [TBD - 검증 완료 후 작성] |

> 본 섹션은 개발 완료 후 성능 검증(V&V) 결과를 기반으로 채워야 한다. **[TBD - 개발 완료 후 작성]**

---

## 10. eSTAR v6.1 Section 10 매핑

FDA eSTAR v6.1 Section 10 (Substantial Equivalence Comparison)에 본 문서가 다음과 같이 활용된다:

| eSTAR Section 10 항목 | 본 문서 참조 섹션 | 비고 |
|---------------------|----------------|------|
| 10.1 Predicate Device Identification | 섹션 3 (Predicate 요약) | K번호 및 허가 일자 기재 |
| 10.2 Intended Use Comparison | 섹션 4 (의도된 사용 비교) | 표 형식으로 직접 삽입 |
| 10.3 Technological Characteristics Comparison | 섹션 5 (기술 특성 비교) | 표 형식으로 직접 삽입 |
| 10.4 Differences Analysis | 섹션 6 (차이점 분석) | DIFF-00X 표 포함 |
| 10.5 Substantial Equivalence Conclusion | 섹션 7 (SE 결론) | 영문 선언문 포함 |

---

## 11. 참조 문서

| 문서 ID | 문서명 | 역할 |
|--------|--------|------|
| DOC-036 | FDA 510(k) eSTAR_v1.0 | 제출 패키지 — Section 10에 본 문서 참조 |
| DOC-005 | SRS (SRS-XRAY-GUI-001) | SW 기능 및 보안 요구사항 (기술 특성 근거) |
| DOC-006 | SAD (SAD-XRAY-GUI-001) | 아키텍처 — 기술 특성 상세 근거 |
| DOC-029 | CER (CER-XRAY-GUI-001) | 임상 평가 — 동등 기기 임상 비교 (섹션 3 참조) |
| DOC-016 | CMP (CMP-XRAY-GUI-001) | 사이버보안 기술 특성 근거 |
| DOC-049 | IEC 81001-5-1 준수 문서 | 사이버보안 규제 매핑 |
| DOC-040 | IFU (IFU-XRAY-GUI-001) | 의도된 사용 원문 및 금기 사항 |

---

## 12. 변경 이력

| 버전 | 변경 일자 | 변경 내용 | 변경자 |
|------|---------|----------|--------|
| v1.0 | 2026-03-31 | 최초 작성 — Predicate 후보 식별 및 비교 프레임워크 수립 (개발 착수 전 계획서 수준; Predicate K번호는 FDA DB 조회 후 기재) | RA 팀 |
| v1.1 | 2026-04-02 | EConsole1 (DRTECH Corp., K231225) 주 Predicate 확정, DIFF-005 CD/DVD Burning 추가, MRD v3.0 Tier 2 기준 반영 | RA 팀 |

---

## 13. 승인

| 역할 | 성명 | 서명 | 일자 |
|------|------|------|------|
| 작성자 (RA 팀) | [TBD] | | 2026-03-31 |
| 검토자 (임상/RA 선임) | [TBD] | | [TBD] |
| 검토자 (SW 아키텍트) | [TBD] | | [TBD] |
| 승인자 (RA/QA 책임자) | [TBD] | | [TBD] |

---

## 부록 A: Predicate 선정 작업 체크리스트

RA 팀이 FDA 510(k) 제출 전 수행해야 할 Predicate 선정 및 본 문서 완성 체크리스트:

| # | 작업 항목 | 담당 | 완료 여부 |
|---|----------|------|---------|
| 1 | FDA 510(k) 데이터베이스에서 LLZ 코드 제품 검색 (최근 5년) | RA 팀 | ☐ |
| 2 | 후보 Predicate Decision Summary 다운로드 및 검토 | RA 팀 | ☐ |
| 3 | 주 Predicate 확정 및 본 문서 §3.2 [TBD] 업데이트 | RA 팀 | ☐ |
| 4 | 주 Predicate 의도된 사용 원문 확인 및 §4.1 표 업데이트 | RA 팀 | ☐ |
| 5 | 주 Predicate 기술 특성 확인 및 §5.1 표 업데이트 | RA 팀 + SW 팀 | ☐ |
| 6 | 실제 차이점 분석 및 §6.1 DIFF 표 업데이트 | RA 팀 | ☐ |
| 7 | SE 결론 선언문 §7.2 최종 기재 (영문) | RA 팀 | ☐ |
| 8 | §8 Predicate 요약 정보 전체 업데이트 | RA 팀 | ☐ |
| 9 | §9 성능 데이터 (V&V 결과 기반) 업데이트 | RA 팀 + QA 팀 | ☐ |
| 10 | 본 문서 최종 승인 및 eSTAR Section 10 삽입 | RA/QA 책임자 | ☐ |

---

## 부록 B: 관련 FDA 가이던스 및 규정

| 문서명 | 출처 | 적용 섹션 |
|-------|------|---------|
| 21 CFR §807.87(f) — Substantial Equivalence Information | FDA Code of Federal Regulations | 비교 의무 근거 |
| Guidance for Industry: The 510(k) Program: Evaluating Substantial Equivalence in Premarket Notifications (2014) | FDA | SE 판단 프레임워크 |
| FDA eSTAR Template v6.1 — Section 10 | FDA | eSTAR 양식 참조 |
| Guidance for Industry and FDA Staff: The Least Burdensome Provisions (2019) | FDA | 심사 효율화 |
| Pre-Submission Program and Meetings with FDA Staff (2019) | FDA | Pre-Sub 활용 방법 |
| Guidance: Cybersecurity in Medical Devices: Quality System Considerations and Content of Premarket Submissions (2023) | FDA | 사이버보안 기술 특성 비교 참조 |

---

*문서 끝 (End of Document)*
*DOC-050 | HnVue Console SW (HnVue) | Predicate Device Comparison | v1.0 | 2026-03-31*
