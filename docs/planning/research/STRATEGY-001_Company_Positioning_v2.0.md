# STRATEGY-001: HnVue Console SW 전략 포지셔닝 문서

| 항목 | 내용 |
|------|------|
| **문서 번호** | STRATEGY-001 |
| **버전** | v2.0 |
| **작성일** | 2026-03-30 |
| **작성** | 전략마케팅본부 / 개발팀 |
| **분류** | 내부 전략 (Confidential) |

---

## 개정 이력

| 버전 | 날짜 | 개정 내용 |
|------|------|----------|
| v1.0 | 2026-03-30 | 최초 작성 |
| v2.0 | 2026-03-30 | 전면 재작성. CsI-SW 분리, 내재화 목표 명확화, 경쟁사 딥리서치 반영, Phase 1 범위 현실화 (30개→15개), 비현실적 KPI/타겟 삭제, 참조 제품(E-COM/피닉스비전/Samsung/경쟁 3사) 분석 추가 |

---

## 1. 전략의 출발점: 왜 Console SW를 만드는가

### 1.1 현재 상태 (As-Is)

자사 FPD 디텍터에 **2종의 외부 Console SW**를 운용 중:

| # | SW | 공급사 | 공급 방식 | 현재 상태 |
|---|------|--------|----------|----------|
| 1 | **feel-DRCS** | IMFOU(임포유), 6-8명 | OEM 라이선스 구매 | 현재 FPD 번들 판매용으로 사용 중 |
| 2 | **HnVue Console SW** | 외부 업체 (개발 의뢰) | 외주 개발 | **내부 평가 단계** — 기존 디텍터 번들 SW로 개발 진행 중 |

- feel-DRCS: FDA K110033 (2011), 14개+ 디텍터/9개+ Generator 호환 범용 콘솔
- HnVue: 자사 디텍터 전용으로 외주 개발 중이며, MRD/PRD/SRS/SAD/SDS 등 기획 문서가 이미 작성됨

**현재 문제점:**
- 2종 SW에 대한 라이선스/외주 비용이 FPD 마진을 압박
- 외주 개발(HnVue)의 기능/일정/품질 통제권 부족
- feel-DRCS: 범용 OEM이므로 자사 디텍터 최적화 한계, MPPS/Q/R/Commitment 옵션 추가비용
- HnVue 외주: 개발사 의존, 유지보수/기능추가 시 추가 비용 및 커뮤니케이션 오버헤드
- 장기적으로 SW 핵심역량이 사내에 축적되지 않음

### 1.2 목표 (To-Be)

- **HnVue Console SW를 자체 개발로 내재화** (소스코드/기술이전 없이 처음부터 자체 구축)
- **영상처리 엔진은 Phase 1에서 외부 SDK 구매 연동**, Phase 2+에서 자체 엔진 내재화
- **feel-DRCS OEM 의존도를 점진적으로 제거**, 자체 HnVue SW로 대체
- 최종 목표: **자체 개발 HnVue Console SW 하나로 FPD 번들 판매**

### 1.3 내재화 단계 (점진적 접근)

```
Phase 1: Console SW 프레임워크 자체 개발 + 영상처리 SDK 구매 연동
         ├── DICOM (fo-dicom) ── 자체
         ├── UI/워크플로우 (WPF) ── 자체
         ├── 디텍터 통합 ── 자체
         ├── Generator 연동 ── 자체
         └── 영상처리 엔진 ── HnVue 외주 SW 탑재 엔진을 SDK로 구매

Phase 2: 영상처리 엔진 내재화
         └── 외부 SDK 의존 → 자체 영상처리 파이프라인으로 교체

Phase 3: 완전 내재화
         └── 전 영역 자체 기술 (AI 포함)
```

### 1.3 목표가 아닌 것 (명시적 제외)

- Siemens/GE/Samsung과 기능 경쟁 ← **불가능하고 불필요**
- AI 플랫폼 구축 ← 2명으로 불가능
- 글로벌 SW 시장점유율 확보 ← SW는 FPD 판매 도구이지 독립 수익원이 아님
- CsI 기술의 SW 차별화 ← CsI는 HW/제조 영역 (아래 1.4 참조)

### 1.4 CsI와 SW의 관계 정리

| 영역 | CsI 증착 | Console SW |
|------|---------|------------|
| **분류** | 물리/제조 기술 | 소프트웨어 |
| **경쟁력** | FPD 원가 절감, 품질 관리, 수직통합 | FPD 판매 시 번들 제공 |
| **차별화** | 글로벌 소수 업체만 보유 | 모든 경쟁사가 보유 (table stakes) |
| **SW에서의 역할** | 없음. SW는 디텍터의 디지털 출력을 처리 | 디텍터별 캘리브레이션 (모든 업체 동일) |

**CsI 수직통합은 HW/제조 경쟁력이다. SW에서 "CsI 최적화"는 사실상 "디텍터 캘리브레이션"이며, 이는 모든 디텍터 제조사가 수행하는 표준 작업이다.**

---

## 2. 회사 현황 분석

### 2.1 강점 (Strengths)

- CsI 증착 기술 내재화 → FPD 원가/품질 경쟁력
- FPD 설계/제조 역량 (HW 수직통합)
- 소규모 조직의 빠른 의사결정
- 현재 타사 SW로 FPD 판매 중 → 매출/고객 기반 존재

### 2.2 약점 (Weaknesses)

- **SW 인력 2명** (경쟁사: DRTECH ~20명, Rayence ~15명, Vieworks 20-30명 추정)
- 의료기기 SW 개발 경험 부재 (IEC 62304)
- UI/UX, QA 전담 인력 없음
- DICOM 전문 역량 미확인

### 2.3 핵심 현실 인식

**2명의 SW 인력으로 경쟁사 수준의 풀 Console을 자체 개발하는 것은 비현실적이다.** 그러나 현재 구매 중인 타사 SW의 핵심 기능을 대체하는 수준의 자체 SW는 12-18개월 내 가능하다.

---

## 3. Console SW 시장 참조 제품 분석

### 3.1 한국 디텍터 경쟁사 Console SW

| 항목 | DRTECH EConsole1 | Rayence Xmaru View V1 | Vieworks VXvue |
|------|:---:|:---:|:---:|
| **FDA 510(k)** | K152172 (2015) | K160579 | 2025.11 |
| **다국어** | 18개 | 18개 | 8개 (+사용자 추가) |
| **Auto Stitching** | 최대 4장 | 최대 3장 (Full/Semi/Manual) | 자동 |
| **Auto Label/Crop** | O | O | O |
| **DICOM MWL** | O | O | O |
| **DICOM C-STORE** | O | O | O |
| **DICOM Print** | O | O | O |
| **DICOM MPPS** | O | O | O |
| **Generator 연동** | O | O | O |
| **멀티 디텍터** | 16개+ 모델 | 최대 3대 동시 | VIVIX-S 전용 |
| **AI 기능** | DEPAI (CycleGAN), TRUVIEW ART | Clear ON, Virtual Grid | Noise-X, Bone-X |
| **EMR Bridge** | X | O (HIPAA) | X |
| **CD/DVD 굽기** | O | O | 미확인 |
| **터치 UI** | XConsole만 | X | Touch Skin |
| **자체 PACS** | X | Xmaru PACS | QXLink |
| **OEM 프로그램** | X | X | O (EXAMION 등) |
| **기술 플랫폼** | Windows, 스택 비공개 | 64-bit 네이티브 | .NET 4.8, Windows |

### 3.2 Console SW 전문 ISV

| 항목 | **IMFOU feel-DRCS (현재 사용)** | E-COM DROC (중국) | OR Tech DX-R (독일) | 피닉스비전 DXVIEW (한국) |
|------|:---:|:---:|:---:|:---:|
| **비즈니스 모델** | B2B OEM 공급 | OEM 전문 (60개국+) | OEM + 직판 | OEM SDK 공급 |
| **멀티벤더 디텍터** | O (14개+ 제조사) | O | O (업계 최상) | O |
| **Generator 통합** | O (9개+ 제조사, APR/AEC) | O (OTC, U-arm) | O | 미확인 |
| **Auto Stitching** | 미확인 | O | O | 미확인 |
| **이미지 처리** | FS-MLW (자체) | Symphony, AI 적응형 | ADPC, AIAA, MFLA, ANF | FUMA SDK (저선량) |
| **Grid Line 제거** | O | O (자동감지) | O (GLI) | 미확인 |
| **DICOM 기본** | Storage, MWL, Print | 풀 지원 | 풀 지원 | 미확인 |
| **DICOM 옵션** | MPPS, Q/R, Commitment | - | - | - |
| **FDA** | K110033 (2011) | 미확인 | 미확인 | 미확인 |
| **설치 기반** | 20개국+ | 100개국+ | 7,000+ | 미확인 |
| **회사 규모** | 6-8명 | 중견 | 중견 | 4명 |

> **feel-DRCS가 현재 자사 FPD에 번들되어 판매 중인 타사 Console SW이다.** H&Abyz 디텍터가 feel-DRCS 호환 목록에 포함되어 있으며, 이 SW의 핵심 기능이 내재화 시 대체해야 할 기준선이다.

### 3.3 Samsung S-Vue (참조용, 직접 경쟁 대상 아님)

Samsung의 Console SW는 독립 제품이 아닌 하드웨어 번들이며, "S-View"는 존재하지 않음 (S-Vue가 영상처리 엔진 브랜드명).

**참고할 만한 기술:**
- S-Vue: FDA 인증 50% 선량 감소 (후처리 기반)
- SimGrid: CNN 기반 가상 산란격자 (물리 그리드 불필요)
- Lunit 통합: AI는 자체 개발이 아닌 파트너십

**교훈:** Samsung도 AI를 자체 개발하지 않고 Lunit에 위탁. 소규모 회사도 동일 전략 가능.

### 3.4 업계 표준 기능 (한국 경쟁사 전원 보유)

리서치 결과, 다음은 한국 디텍터 업체 **전원이 제공하는 표준 기능**이다:

1. 디텍터 이미지 획득 + Generator 연동
2. 체부위별 자동 영상 처리 프리셋
3. DICOM Storage (C-STORE) + MWL + MPPS + Print
4. Auto Stitching (최소 3장)
5. Auto Labeling + Auto Crop
6. 다국어 (최소 8개, DRTECH/Rayence 18개)
7. 기본 측정 도구 (길이, 각도)
8. 어노테이션
9. 멀티 디텍터 지원 (자사 라인업)
10. 환자 등록/관리

**이것이 "Console SW가 있다"고 말할 수 있는 최소 기준선이다.**

---

## 4. 내재화 전략

### 4.1 전략 원칙

1. **대체 우선**: 현재 구매 중인 타사 SW의 핵심 기능을 먼저 대체
2. **자사 디텍터 전용**: Phase 1은 자사 FPD만 지원 (멀티벤더는 Phase 2+)
3. **표준 기능 우선**: 업계 표준 기능부터. 차별화는 표준 달성 후
4. **오픈소스 극대화**: fo-dicom(MIT), SQLite, Serilog 등 검증된 오픈소스 활용
5. **외주 병행**: IEC 62304 문서화, UI/UX 디자인, QA는 외주

### 4.2 기술 스택

| 계층 | 기술 | 근거 |
|------|------|------|
| UI | WPF (.NET 8 LTS) | GPU 가속, 터치 지원, 의료영상 업계 실적 |
| DICOM | fo-dicom 5.x | MIT, .NET 네이티브, SCU/SCP 내장 |
| **영상처리 엔진** | **외부 SDK 구매** (Phase 1) | 현재 HnVue 외주 SW에 탑재된 영상처리 엔진을 SDK로 별도 구매하여 연동. Phase 2에서 자체 엔진 내재화 |
| 영상 렌더링 | SkiaSharp | 16비트 그레이스케일, 크로스플랫폼 |
| DB | SQLite + EF Core | 제로 설정, 단일 워크스테이션 최적 |
| 로깅 | Serilog | 구조화 로깅, 감사 추적 |
| 테스트 | xUnit + NSubstitute | .NET 표준 |
| 이미지 코덱 | OpenJPEG (binding) | BSD, JPEG2000 |

### 4.3 Phase 1: 타사 SW 대체 (12-18개월, SW 2명)

**목표: 외주 개발 중인 HnVue SW의 개발을 내재화하고, feel-DRCS를 대체**

**내재화 기준선:** 현재 HnVue 외주 SW가 구현 중인 기능 + feel-DRCS 기본 기능의 합집합

#### Phase 1 핵심 기능 (15개)

| # | 기능 | 관련 MR | 담당 | 난이도 |
|---|------|---------|------|--------|
| 1 | 자사 디텍터 이미지 획득 | MR-010 | Dev A | 상 |
| 2 | Generator 연동 (프로토콜 의존) | MR-031 | Dev A | 상 |
| 3 | 체부위별 자동 영상 처리 프리셋 (50개) — **HnVue 탑재 영상엔진 SDK 연동** | MR-011, MR-013 | Dev A | 중 |
| 4 | 기본 영상 조작 (W/L, Zoom, Pan, Rotate, Flip) | MR-012 | Dev B | 중 |
| 5 | Auto Labeling | - | Dev B | 중 |
| 6 | Auto Crop | - | Dev B | 중 |
| 7 | 기본 어노테이션 + 측정 (길이, 각도) | MR-012 | Dev B | 중 |
| 8 | 환자 등록 (수동 + DICOM MWL) | MR-001, MR-025 | Dev B | 중 |
| 9 | DICOM C-STORE (PACS 전송) | MR-019 | Dev A | 중 |
| 10 | DICOM MWL 조회 | MR-019 | Dev A | 중 |
| 11 | DICOM MPPS | MR-009 | Dev A | 중 |
| 12 | 로컬 스터디 관리 (SQLite) | - | Dev B | 중 |
| 13 | Exposure Index 표시 + 기본 선량 기록 | MR-027 | Dev A | 하 |
| 14 | 한국어/영어 UI | MR-045 축소 | Dev B | 하 |
| 15 | RBAC (관리자/사용자 2단계) | MR-033 | Dev B | 하 |

#### MFDS 규제 필수 (외주 + 자체 병행)

| # | 항목 | 방식 |
|---|------|------|
| 16 | IEC 62304 Class B 문서 패키지 | 외주 (규제 컨설팅) |
| 17 | PHI 암호화 (AES-256) | 자체 (.NET Cryptography API) |
| 18 | 감사 로그 | 자체 (Serilog) |
| 19 | SBOM 생성 | 자체 (도구 활용) |
| 20 | 코드 서명 | 자체 |
| 21 | DICOM Conformance Statement | 자체 |

#### Phase 1에서 명시적으로 제외하는 것

| 기능 | 제외 근거 |
|------|----------|
| Auto Stitching | 알고리즘 복잡도 높음. 업계 표준이나 Phase 1 범위 초과. Phase 2 우선 |
| AI 영상처리 (노이즈 캔슬링 등) | ML 모델 학습 인프라 필요. 2명 불가 |
| Virtual Grid (가상 산란격자) | CNN 기반, 대규모 학습 데이터 필요 |
| DICOM Print | 사용 빈도 급감 중. Phase 2 |
| CD/DVD 굽기 | 사용 빈도 급감 중. Phase 2 |
| 멀티벤더 디텍터 | 자사 디텍터 우선. Phase 2 |
| 다국어 8개+ | 한/영 2개로 시작. Phase 2 확장 |
| EMR Bridge | 미국 시장 진출 시. Phase 2 |
| 터치 전용 UI | WPF 기본 터치 지원으로 충분. 전용 스킨은 Phase 2 |
| 모바일/원격 앱 | 별도 플랫폼. Phase 3 |
| Cloud 배포 | 인프라 운영 역량 부재. Phase 3 |
| OEM 화이트라벨 SDK | 우리가 OEM을 받는 입장. 영구 제외 |
| 마이크로서비스 | 오버엔지니어링. 영구 제외 |
| 멀티사이트 관리 | 우리 규모에 부적합. 영구 제외 |
| 3D 카메라 환자 감지 | HW+SW 통합 과도. 영구 제외 |

### 4.4 Phase 2: 업계 표준 달성 (Phase 1 완료 후 12개월, 인력 보강 전제)

**전제: SW 팀 3-4명으로 확대**

| 기능 | 우선순위 | 근거 |
|------|----------|------|
| **영상처리 엔진 내재화** | **필수** | Phase 1 외부 SDK 의존 제거. 자체 영상처리 파이프라인 구축 |
| Auto Stitching (최소 3장) | 필수 | 한국 경쟁사 전원 보유. 이것 없이는 "표준 미달" |
| DICOM Print | 높음 | 일부 병원 아직 요구 |
| 다국어 확장 (4-8개) | 높음 | 수출 필수 |
| Reject Analysis | 높음 | 품질 관리 기본. Rayence ASR 참조 |
| 이전 영상 비교 | 중간 | Q/R SCU 필요 |
| 터치 전용 UI 스킨 | 중간 | 포터블 시스템 대응 |
| 멀티벤더 디텍터 지원 | 중간 | 시장 확장 (OR Tech 모델 참조) |
| FDA 510(k) / CE 준비 | 높음 | 해외 시장 진출 |
| 기본 선량 관리 (DRL, 소아) | 높음 | 규제 요구 강화 추세 |
| EMR Bridge | 중간 | 미국 시장 (Rayence 참조) |
| CD/DVD 굽기 + 뷰어 | 낮음 | 수요 감소 중이나 일부 요구 |

### 4.5 Phase 3: 차별화 (24개월+)

| 기능 | 구현 방식 |
|------|----------|
| AI 노이즈 캔슬링 | 외부 파트너 (Lunit, VUNO 등) — Samsung 사례 참조 |
| Scatter Correction / Virtual Grid | 학술 논문 기반 구현 또는 라이선싱 |
| AI 플러그인 아키텍처 | 자체 |
| Cloud 하이브리드 | 외주 + 자체 |
| 고급 선량 관리 대시보드 | 자체 (Samsung SMART Center 참조) |
| 원격 제어 앱 | 외주 |

---

## 5. 인력 계획

### 5.1 현 인력 배치 (SW 2명)

| 역할 | 담당 | 핵심 기술 |
|------|------|----------|
| Dev A (시니어) | 아키텍처, DICOM(fo-dicom), 디텍터 통신, Generator 연동, 영상 처리 | C#/.NET, DICOM, 시리얼/이더넷 |
| Dev B | WPF UI, 워크플로우, DB(SQLite/EF Core), 사용자 관리, 다국어 | C#/WPF, XAML, SQL |

**가용 코딩 시간: 60-70%** (IEC 62304 문서화, 회의, 기타 업무 제외)

### 5.2 외주 필수 영역

| 영역 | 시기 | 예상 비용 |
|------|------|----------|
| IEC 62304 문서화/컨설팅 | M1-18 | 30-50M KRW |
| UI/UX 디자인 | M3-6 | 10-20M KRW |
| QA/테스트 (V&V) | M10-18 | 15-25M KRW |
| MFDS 인허가 대행 | M14-20 | 20-30M KRW |
| PACS 호환 테스트 | M12-14 | 5-10M KRW |
| **소계** | | **80-135M KRW** |

### 5.3 채용 권장

| 역할 | 시기 | 우선순위 | 근거 |
|------|------|----------|------|
| SW 개발자 1명 추가 | Phase 1 M3 이전 | **필수** | 2명은 SPOF. 1명 이탈 시 프로젝트 중단 |
| QA 엔지니어 | Phase 1 M8 | 높음 | IEC 62304 V&V 내부 수행 |

---

## 6. 일정 로드맵

### 6.1 Phase 1 마일스톤 (12-18개월)

| 마일스톤 | 기간 | 산출물 | 핵심 지표 |
|----------|------|--------|----------|
| **MS-01 기반 구축** | M1-M5 | DICOM 인프라(fo-dicom), 기본 이미지 표시, DB 스키마, 아키텍처 문서 | DICOM Echo 성공, 이미지 로드/표시 |
| **MS-02 촬영 통합** | M6-M10 | 디텍터+Generator 연동, 촬영 워크플로우, MWL, C-STORE | 자사 FPD로 촬영→PACS 전송 성공 |
| **MS-03 임상 기능** | M11-M14 | 영상 처리 프리셋, 어노테이션, 스터디 관리, 선량 기록 | 임상 워크플로우 end-to-end 완료 |
| **MS-04 규제/출시** | M15-M18 | IEC 62304 문서, V&V, MFDS 신청 | MFDS 접수 |

### 6.2 상세 타임라인

```
Month:  1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18
        |===MS-01 기반 구축======|===MS-02 촬영 통합======|===MS-03 임상====|==MS-04 규제===|
Dev A:  [DICOM/아키텍처--------][디텍터+Generator 통합--][영상처리프리셋-][V&V/버그수정--]
Dev B:  [DB/UI 프레임워크------][워크플로우 UI---------][스터디관리/UX--][V&V/버그수정--]
외주:   [IEC 62304 컨설팅 시작================================계속==========][MFDS 인허가--]
        [                      ][UI/UX 디자인---]       [         ][QA 테스트====]
채용:   [SW 3번째 개발자 채용!!]
```

---

## 7. 리스크 매트릭스

| 리스크 | 확률 | 영향 | 등급 | 대응 |
|--------|:----:|:----:|:----:|------|
| SW 인력 1명 이탈 | 중 | 치명적 | **극심** | 즉시 채용, 코드 문서화, 지식 공유 |
| IEC 62304 경험 부족 | 높음 | 높음 | **높음** | M1에 외부 컨설팅 착수 |
| 일정 지연 (18개월 초과) | 높음 | 중 | **높음** | Phase 1 범위 추가 축소 (15→12개 기능) |
| DICOM 호환성 이슈 | 중 | 중 | 중 | fo-dicom 커뮤니티 + 조기 PACS 테스트 |
| 디텍터 SDK/프로토콜 문서 부족 | 중 | 높음 | **높음** | HW팀과 조기 협의, 프로토콜 문서화 |
| 3번째 SW 채용 실패 | 중 | 높음 | **높음** | 처우 경쟁력, 원격근무 허용 |
| 타사 SW 라이선스 계약 종료 압박 | 낮음 | 높음 | 중 | 자체 SW 완성까지 라이선스 유지 |

---

## 8. 경쟁사 대비 현실적 포지셔닝

### 8.1 기능 비교 (현실 기준)

| 기능 | DRTECH | Rayence | Vieworks | **HnVue Phase 1** | **HnVue Phase 2** |
|------|:---:|:---:|:---:|:---:|:---:|
| 이미지 획득 + 처리 | ◎ | ◎ | ◎ | **○** | **○** |
| DICOM 핵심 (C-STORE/MWL/MPPS) | ◎ | ◎ | ◎ | **○** | **◎** |
| Auto Stitching | ◎ | ◎ | ◎ | **X** | **○** |
| Auto Label/Crop | ◎ | ◎ | ◎ | **○** | **◎** |
| Generator 연동 | ◎ | ◎ | ◎ | **○** | **○** |
| 다국어 | ◎ (18개) | ◎ (18개) | ○ (8개) | **△ (2개)** | **○ (4-8개)** |
| AI 영상처리 | ◎ (DEPAI) | ○ (Clear ON) | ◎ (Noise-X) | **X** | **X** |
| Virtual Grid | X | ◎ | △ | **X** | **X** |
| EMR Bridge | X | ◎ | X | **X** | **○** |
| Touch UI | △ | X | ○ | **△** | **○** |
| DICOM Print | ◎ | ◎ | ◎ | **X** | **○** |
| 멀티벤더 디텍터 | ◎ (16+) | ○ (3대) | X (자사만) | **X (자사만)** | **○** |
| Reject Analysis | X | ◎ (ASR) | X | **X** | **○** |

> ◎ 우수 | ○ 표준 | △ 기본/제한 | X 미지원

### 8.2 포지셔닝 요약

**Phase 1 (내재화 완료 시점):** 자사 FPD 전용, 핵심 워크플로우 동작, MFDS 인허가 수준
- 경쟁사 대비 기능 열위 (Auto Stitching 없음, 다국어 2개, AI 없음)
- 그러나 **타사 SW 라이선스 비용 제거 + 자사 통제권 확보**라는 목표 달성

**Phase 2 (업계 표준 달성):** Auto Stitching, 다국어, Reject Analysis 등 추가
- 경쟁사 대비 기능 동등 수준 접근
- FDA/CE 인증으로 해외 시장 진출 기반 확보

---

## 9. 의사결정 사항

### 9.1 즉시 결정 필요

| # | 의사결정 | 옵션 | 권장 | 근거 |
|---|---------|------|------|------|
| D-01 | 3번째 SW 개발자 채용 | 즉시/M3/미채용 | **즉시** | 2명은 SPOF |
| D-02 | IEC 62304 컨설팅 계약 | 즉시/M6 | **즉시** | 개발 초기부터 병행 필수 |
| D-03 | feel-DRCS 라이선스 유지 기간 | 12개월/18개월/24개월 | **18-24개월** | 자체 HnVue SW 안정화 전까지 판매용 유지 |
| D-04 | HnVue 내재화 착수 시점 | 즉시/평가 완료 후 | **평가 완료 후** | 현재 내부 평가 결과를 반영하여 내재화 범위 확정 |
| D-05 | 영상처리 SDK 구매 조건 | 라이선스 범위/가격/업데이트 | **확인 필요** | HnVue 외주 SW에 탑재된 영상엔진을 SDK로 별도 구매. Phase 2에서 자체 내재화 |

### 9.2 Phase 1 중 결정

| # | 의사결정 | 시점 | 판단 기준 |
|---|---------|------|----------|
| D-06 | Phase 1 범위 조정 여부 | M8 | 일정 지연 시 15→12개 기능으로 축소 |
| D-07 | QA 내부화 vs 외주 | M10 | V&V 볼륨에 따라 |
| D-08 | MFDS 우선 vs FDA 우선 | M6 | MFDS가 비용/기간 효율적 |

---

## 부록 A: 참조 제품 상세

### A.0 IMFOU feel-DRCS — 현재 사용 중인 타사 Console SW (내재화 기준선)

- **회사명**: 주식회사 임포유 (imfoU Co., Ltd.)
- **설립**: 2008년, 서울 구로구
- **직원**: 6-8명, 매출 ~40억원
- **제품**: feel-DRCS v2.1 (Human/Vet 에디션)
- **FDA**: K110033 (2011.09 승인), Product Code LLZ
- **핵심 기술**: FS-MLW (Faster Specialized Multi Layered Wavelet) 영상처리
- **호환 디텍터**: TRIXELL, VAREX, CETD(TOSHIBA), DRTECH, RAYENCE, VIEWORKS, PIXXGEN, IRAY, CARERAY, PZMEDICAL, CANON, RADISSEN, **H&Abyz(당사)**
- **호환 Generator**: CPI, SEDECAL, EMD, DRGEM, POSKOM, ECORAY, TECHNIX, SMAM, SPELLMAN
- **DICOM 기본**: Storage SCU, MWL SCU, Print SCU, DICOM DIR
- **DICOM 옵션 (추가 비용)**: MPPS, Q/R, Storage Commitment
- **영상 처리**: Fixed/Flexible LUT, 콘트라스트 균등화, 자동 윈도우잉, 노이즈 제거, Grid Line 제거, Edge enhancement, Latitude enhancement, Smoothing, Frequency filtering
- **워크플로우**: Worklist, Browser, Review, Configuration 모듈
- **적용**: General, Orthopedic, Chiropractic, Podiatry, Mammography, Veterinary, NDT
- **웹사이트**: imfou.com

**feel-DRCS는 현재 사용 중인 2종 외부 SW 중 하나이며, 이 기능 세트 + HnVue 외주 SW 기능의 합집합이 "최소 대체 기준"이다. 양쪽 SW 모두에 없는 기능(Auto Stitching, AI 등)은 Phase 1 필수가 아니다.**

### A.1 E-COM Technology (중국, 1997년 설립)
- **DROC**: Windows 기반 DR 콘솔, OEM 전문 (60개국+)
- Multi-vendor 디텍터 지원, Symphony 영상처리 엔진
- Mini-PACS 내장, Grid Line 자동 제거
- AI 적응형 이미지 획득
- DICOM 3.0 풀 지원 (Storage, MWL, MPPS, RDSR, Q/R)
- 웹사이트: e-comtech.com

### A.2 피닉스비전 (한국, 2005년 설립, 4명)
- **DXVIEW**: 일반 DR 콘솔 소프트웨어
- **FUMA SDK**: AI(퍼지) 기반 저선량 영상 처리 엔진 (0.4-1.0초/3Kx3K)
- 지원 모달리티: DR, Mammo, C-Arm, RF, CBCT, Tomo
- OEM SDK 공급 모델 (디텍터 제조사에 SW 공급)
- 웹사이트: phenixvision.com

### A.3 Samsung S-Vue (참조용)
- S-Vue: 영상처리 엔진 브랜드 (독립 콘솔 SW 제품이 아님)
- FDA K172229: 성인 흉부 50% 선량 감소 인증
- SimGrid: CNN 기반 가상 산란격자 (18.7% 추가 선량 감소)
- CXR Assist: Lunit INSIGHT CXR 통합 (흉부 이상 자동 검출)
- Vision Assist: AI 카메라 (GC85A Vision+ 전용, 89가지 기능)
- SMART Center: 엔터프라이즈 선량/품질/생산성 중앙 관리

### A.4 OR Technology dicomPACS DX-R (독일, 1991년 설립)
- OEM 화이트라벨 전문, 7,000+ 설치 기반
- 멀티벤더 호환 업계 최상, 400+ 촬영 부위 프로토콜
- GLI (가상 산란격자), 스마트폰 원격 제어 앱
- OR Dose Inspector 통합 선량 관리
- 다국어 GUI + 멀티미디어 포지셔닝 가이드

### A.5 medical ECONET meX+ (독일, 1997년 설립)
- 모바일 올인원 콘솔, 노트북 기반
- 장기별 영상 처리, 멀티미디어 포지셔닝 가이드
- 수의학 파생 제품 (meX+100 VET, meX+ dentX)
- ISO 13485:2016 인증

---

## 부록 B: 용어 정리

| 용어 | 설명 |
|------|------|
| Console SW | X-ray 촬영 워크스테이션 소프트웨어 |
| FPD | Flat Panel Detector. 디지털 X-ray 검출기 |
| CsI | Cesium Iodide. X-ray→가시광 변환 scintillator (HW 영역) |
| DICOM | 의료영상 국제 표준 |
| MWL | Modality Worklist. HIS/RIS에서 환자 정보 조회 |
| MPPS | Modality Performed Procedure Step. 검사 수행 보고 |
| C-STORE | DICOM Storage 서비스. 영상을 PACS로 전송 |
| PACS | 의료영상 저장/전송 시스템 |
| RDSR | Radiation Dose Structured Report. 선량 보고 |
| IEC 62304 | 의료기기 SW 라이프사이클 국제 표준 |
| MFDS | 식품의약품안전처 |
| fo-dicom | .NET 오픈소스 DICOM 라이브러리 (MIT) |
| ISV | Independent Software Vendor. 독립 SW 공급사 |
| OEM | Original Equipment Manufacturer |
| SPOF | Single Point of Failure. 단일 장애점 |
| EI | Exposure Index. 노출 지표 |

---

**문서 끝**

> 이 문서는 내부 전략 문서입니다. 외부 유출을 금지합니다.
> v2.0: CsI-SW 분리, 내재화 목표 중심 재작성, 경쟁사 딥리서치 반영
> 다음 업데이트: Phase 1 착수 후 Month 3 (D-01~D-03 의사결정 반영)
