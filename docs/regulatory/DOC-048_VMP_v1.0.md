# 취약점 관리 계획 (Vulnerability Management Plan)

> **문서 번호**: DOC-048
> **버전**: 1.0 (계획서)
> **작성일**: 2026-03-31
> **작성자**: 사이버보안 팀 (Cybersecurity Team)
> **검토자**: SW 아키텍트, QA 팀장
> **승인자**: 의료기기 RA/QA 책임자
> **제품**: HnVue Console SW (HnVue)
> **회사**: HnVue (가칭)
> **분류**: ✅ 최소 필수
> **적용 시장**: FDA 510(k) (필수), EU MDR Class IIa (필수), MFDS 2등급 (권장)
> **근거 규격**: FD&C Act §524B(b)(1)(b)(4), FDA Cybersecurity Guidance (Feb 2026) §V (Post-Market), MDCG 2019-16, ISO/IEC 29147 (취약점 공개), ISO/IEC 30111 (취약점 처리), IEC 62304 §6.2
> **IEC 62304 클래스**: Class B

---

## 개정 이력 (Revision History)

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| 1.0 | 2026-03-31 | 최초 작성 — 개발 착수 전 계획 수준 초안 (DOC-016 VDP 정책 연계) | 사이버보안 팀 |

---

## 관련 문서 (Related Documents)

| 문서 ID | 문서명 | 관계 |
|---------|--------|------|
| DOC-016 | 사이버보안 관리 계획서 (Cybersecurity Management Plan) | 상위 사이버보안 전략 — VDP 정책, 모니터링 계획 연계 |
| DOC-019 | 소프트웨어 자재 명세서 (SBOM) | VMP 대상 컴포넌트 목록 (SBOM-001–038) |
| DOC-045 | VEX 리포트 | VMP 프로세스의 핵심 출력물 — 취약점 발견 시 VEX 업데이트 |
| DOC-046 | 사이버보안 핵심 통제 명세서 | 완화 조치 기준 참조 |
| DOC-047 | 보안 위험 평가 (Security Risk Assessment) | 취약점 대응 시 위험 재평가 기준 참조 |
| DOC-008 | 위험 관리 계획서 (RMP) | ISO 14971 위험 관리 통합 |

---

## 1. 문서 정보

| 항목 | 내용 |
|---|---|
| 제품명 | HnVue Console SW (HnVue) |
| 회사명 | HnVue (가칭) |
| 적용 SW 버전 범위 | HnVue Console SW **전 릴리즈 버전** (시판 중인 모든 버전) |
| 예상 제품 수명 | 약 10년 (시판 후 지원 기간) |
| 의존 문서 | DOC-019 (SBOM), DOC-045 (VEX) |
| 보안 신고 이메일 | security@hnvue.com |
| VMP 담당자 | [TBD - 담당자 지정 후 작성] |
| VMP 검토 주기 | 연 1회 이상 (또는 주요 사건 발생 시) |

> **⚠ 계획서 주의사항**: 본 문서는 개발 착수 전 계획 수준으로 작성된 문서이다. `[TBD - 개발 완료 후 작성]`으로 표시된 항목은 소프트웨어 출시 전 최종 확정한다. 본 VMP는 FDA 510(k) 제출 시 포함되며, 허가 후에도 계속 유효하게 유지·관리된다.

---

## 2. 범위 및 목적

### 2.1 목적

본 계획서는 HnVue Console SW (HnVue)의 **시판 후(Post-Market)** 단계에서 신규 취약점 발견 시:
1. 체계적으로 탐지(Detect)하고,
2. 신속하게 평가(Assess)하며,
3. 적시에 조치(Respond)하고,
4. 이해관계자(규제 기관, 고객)에게 투명하게 공개(Disclose)하며,
5. 패치를 안전하게 배포(Deploy)하는 전체 프로세스를 정의한다.

또한 본 VMP는 FD&C Act §524B(b)(1)에 따른 법적 의무를 이행하며, FDA Cybersecurity Guidance Feb 2026 §V(Post-Market) 요건을 충족한다.

### 2.2 적용 범위

| 범위 | 포함/제외 |
|---|---|
| HnVue Console SW 자체 코드 취약점 | ✅ 포함 |
| HnVue Console SW 의존 SOUP/OSS 컴포넌트 취약점 (DOC-019 SBOM 기반) | ✅ 포함 |
| HnVue Console SW 설치 환경 OS (Windows 10/11 IoT Enterprise) 취약점 | ☑ 부분 포함 (HnVue Console SW 운영에 직접 영향을 미치는 경우) |
| 병원 인프라 취약점 (PACS, RIS, 네트워크) | ❌ 제외 (병원 IT 책임) |

---

## 3. 취약점 탐지 소스 (Monitoring Sources)

### 3.1 자동 모니터링 소스

DOC-019(SBOM) §2.1의 자동 SBOM 생성·스캔 파이프라인(OWASP Dependency-Check)을 기반으로, 시판 후 지속적 취약점 모니터링을 수행한다.

| 소스 | 내용 | 구독/연동 방법 | 담당자 | 검토 주기 |
|---|---|---|---|---|
| **NVD (National Vulnerability Database)** | SBOM(DOC-019) 38개 컴포넌트 기반 CVE 조회 | Grype/Trivy CI/CD 연동 또는 NVD API 구독 | [TBD] | 주 1회 자동 |
| **CISA KEV (Known Exploited Vulnerabilities)** | 실제 악용 중인 CVE 목록 — 최우선 대응 | https://www.cisa.gov/known-exploited-vulnerabilities-catalog RSS/API 구독 | [TBD] | 즉시 (실시간) |
| **GitHub Security Advisories** | GitHub 호스팅 오픈소스 의존성 취약점 (fo-dicom, OpenCV 등) | GitHub Dependabot 활성화 또는 GitHub Advisory Database API | [TBD] | 즉시 (실시간) |
| **NVD CPE/PURL 기반 알림** | SBOM의 CPE/PURL 기반 자동 매칭 | osv-scanner 또는 FOSSA 연동 | [TBD] | 주 1회 |

### 3.2 수동 모니터링 소스

| 소스 | 내용 | 담당자 | 검토 주기 |
|---|---|---|---|
| **CISA ICS-CERT 보안 권고문** | 의료기기 관련 사이버보안 경보 | [TBD] | 월 1회 검토 |
| **FDA MedWatch / MDR 보고 시스템** | 의료기기 관련 사이버보안 사고 보고 | [TBD] | 월 1회 검토 |
| **DCMTK / fo-dicom 공식 보안 공지** | DOC-019 핵심 SOUP 벤더 보안 공지 (SBOM-009, SBOM-010) | [TBD] | 월 1회 검토 |
| **OpenSSL 공식 보안 공지** | 암호화 라이브러리 보안 업데이트 (SBOM-020, SBOM-019) | [TBD] | 월 1회 검토 |
| **Microsoft Security Update Guide** | .NET Runtime, Windows 10/11 IoT 관련 보안 업데이트 (SBOM-001, SBOM-002) | [TBD] | 월 1회 검토 (Patch Tuesday 다음날) |
| **고객/사용자 신고** | security@hnvue.com 통해 접수 | [TBD] | 즉시 |
| **외부 연구자 신고 (CVD)** | 협조적 취약점 공개 (§8 참조) | [TBD] | 즉시 |
| **정기 내부 보안 스캔** | HnVue Console SW 설치 환경 정기 취약점 스캔 | [TBD] | 분기별 |

### 3.3 DOC-019 SBOM 기반 고위험 모니터링 대상

DOC-019 §6.2 및 DOC-017 §10의 고위험 모니터링 대상 컴포넌트에 대해 강화된 모니터링을 수행한다.

| 우선순위 | SBOM-ID | 구성요소 | 근거 |
|----------|---------|---------|------|
| 1 (최우선) | SBOM-020 | OpenSSL 3.2.1 | 암호화 핵심 — 과거 Critical CVE 빈번 (Heartbleed 등) |
| 1 (최우선) | SBOM-019 | BouncyCastle 2.3.0 | 인증/암호화 기능 — High CVE 이력 |
| 2 (높음) | SBOM-009 | fo-dicom 5.1.3 | DICOM 파서 — 네트워크 노출, DOC-017 TM-T-001 연계 |
| 2 (높음) | SBOM-010 | DCMTK 3.6.8 | DICOM 라이브러리 — 네트워크 노출 |
| 3 (보통) | SBOM-002 | .NET 6.0 Runtime | 플랫폼 전체 영향 — EOL 이후 업그레이드 모니터링 |
| 3 (보통) | SBOM-012/013 | OpenCvSharp4 / OpenCV | 영상 처리 파서 — 입력 취약점 가능성 |
| 4 (낮음) | 나머지 SBOM | WPF, Serilog 등 | 정기 스캔으로 충분 |

---

## 4. 취약점 심각도 분류 기준

### 4.1 CVSS v3.1 기반 분류

| 분류 | CVSS v3.1 범위 | 설명 |
|---|---|---|
| **Critical** | 9.0 ~ 10.0 | 즉시 대응 필요. 실제 악용 가능성 매우 높음. |
| **High** | 7.0 ~ 8.9 | 신속 대응 필요. 의미 있는 피해 발생 가능. |
| **Medium** | 4.0 ~ 6.9 | 정기 릴리즈 내 대응. |
| **Low** | 0.1 ~ 3.9 | 영향 적음. 차기 릴리즈 또는 수용 검토. |

> **의료기기 특수 조건**: AAMI TIR57 부속서 C에 따라 환자 안전에 직접 영향을 미치는 취약점은 CVSS Base Score에 관계없이 한 단계 상향하여 처리한다. (예: 방사선 파라미터 관련 High CVE → Critical 처리)

### 4.2 통제 가능성 분류

| 분류 | 설명 | 처리 방법 |
|---|---|---|
| **통제 가능 (Controllable)** | HnVue가 패치 배포로 직접 해결 가능 | 패치 개발 → 릴리즈 → 고객 배포 |
| **통제 불가 — SOUP 벤더 의존** | 제3자 라이브러리 (DCMTK, OpenSSL, fo-dicom 등) 취약점 — 벤더 패치 대기 | DOC-045 VEX 업데이트(Under Investigation → Affected), 벤더 패치 모니터링, 임시 완화 조치 제공 |
| **통제 불가 — 환경 의존** | 병원 OS(Windows), 네트워크 인프라 취약점 | IFU 권고 사항 업데이트, 고객 통보, 병원 IT 협력 요청 |
| **수용 (Accept)** | 임상 위험이 낮아 즉시 해결보다 수용이 합리적 | DOC-045 VEX 상태 Not Exploitable 또는 수용 판정 기록, 향후 릴리즈 계획 |

---

## 5. 대응 SLA (Service Level Agreement)

DOC-016(Cybersecurity Management Plan) 및 DOC-045(VEX 리포트) §9의 SLA와 완전히 일치한다.

| 심각도 | 탐지 후 분류 완료 | VEX 업데이트 기한 | 패치 개발 완료 기한 | 패치 배포 (고객 알림) 기한 | FDA/MFDS 보고 |
|---|---|---|---|---|---|
| **Critical** | 24시간 이내 | 48시간 이내 | 30일 이내 | 30일 이내 | 환자 위해 시 즉시 / 잠재 위해 시 검토 (§7 참조) |
| **High** | 72시간 이내 | 7일 이내 | 90일 이내 | 90일 이내 | 검토 후 결정 |
| **Medium** | 7일 이내 | 30일 이내 | 다음 정기 릴리즈 | 다음 정기 릴리즈 | 해당 없음 |
| **Low** | 30일 이내 | 60일 이내 | 판단 후 결정 | 판단 후 결정 | 해당 없음 |

> **CISA KEV 등재 취약점 특별 처리**: CVSS 점수와 무관하게, CISA KEV에 등재된 취약점은 Critical로 처리하여 상기 Critical SLA를 적용한다.

---

## 6. 취약점 대응 프로세스

### 6.1 프로세스 흐름도

```
[탐지]
  NVD 자동 스캔 / CISA KEV 알림 / 고객 신고 / 외부 연구자 신고
        ↓
[접수 및 1차 분류]
  담당자: VMP 담당자 [TBD]
  - CVSS 점수 확인 및 심각도 분류 (§4)
  - HnVue Console SW 적용 가능성 1차 판단
  - 취약점 티켓 생성 (내부 이슈 트래커)
  - CISA KEV 등재 여부 확인 → Critical 강제 처리
        ↓
[영향도 심층 분석]
  담당자: 개발팀 + 사이버보안 담당자
  - DOC-019 SBOM 연계: 영향 컴포넌트 SBOM-ID 확인
  - VEX 분석: Not Affected / Affected / Fixed 판정 (DOC-017 STRIDE 위협 연계)
  - 환자 피해 가능성 평가 (DOC-047 Security Risk Assessment 기준 적용)
  - DOC-045 VEX 문서 업데이트
        ↓
     ┌──────────────────────────────────┐
     │ Affected 판정                    │  Not Affected / Not Exploitable
     ↓                                  ↓
[패치 개발]                   [DOC-045 VEX 업데이트 완료 → 모니터링 계속]
  - 자체 코드: 직접 수정
  - SOUP 벤더 의존: 업그레이드 또는 임시 완화 조치
  - 단위/통합 시험 (회귀 시험 포함)
  - 침투 테스트 재확인 (High 이상)
        ↓
[릴리즈 준비]
  - DOC-019 SBOM 재생성 (업데이트된 컴포넌트 반영)
  - DOC-045 VEX 업데이트 (Fixed 상태로 변경)
  - 릴리즈 노트 작성 (보안 패치 내용 포함)
  - Microsoft Authenticode 코드 서명 확인 (DOC-046 통제8)
        ↓
[고객 배포 및 알림]
  - 패치 버전 서명 및 SHA-256 체크섬 확인
  - 고객 이메일/포털 알림 (§9 참조)
  - 패치 미적용 고객 임시 완화 조치 안내
        ↓
[완료 처리]
  - 취약점 티켓 종결
  - VMP 이력 기록 (§11)
  - 규제 기관 보고 여부 결정 (§7)
```

### 6.2 긴급 패치 프로세스 (Critical 취약점)

| 단계 | 내용 | 책임자 | 기한 |
|---|---|---|---|
| 긴급 알림 | 경영진 및 개발팀 즉시 통보 | VMP 담당자 [TBD] | 탐지 후 24시간 |
| 긴급 패치 개발 착수 | 정규 릴리즈 스케줄과 별도 진행 (Hotfix 프로세스) | 개발팀 리드 [TBD] | 탐지 후 48시간 |
| 임시 완화 조치 배포 | 패치 개발 중 즉시 적용 가능한 완화 조치 고객 안내 | VMP 담당자 [TBD] | 탐지 후 72시간 |
| 패치 릴리즈 | 긴급 릴리즈 (Hotfix) — Authenticode 서명 + SHA-256 포함 | 개발팀 [TBD] | 탐지 후 30일 이내 |
| 규제 기관 보고 검토 | FDA MDR/MFDS 보고 여부 결정 (§7 참조) | QA/RA 담당자 [TBD] | 패치 완료 시 |

---

## 7. 규제 기관 보고 기준 (Mandatory Reporting)

### 7.1 FDA 보고 기준

| 상황 | 보고 유형 | 기한 | 보고 방법 |
|---|---|---|---|
| 사이버보안 사고로 환자 사망 또는 심각한 부상 | FDA MDR (Medical Device Report) — 30일 보고 | 사고 인지 후 30일 | FDA MedWatch 포털 |
| 사이버보안 사고로 환자 부상 임박 (즉각 위협) | FDA MDR — 5일 보고 | 사고 인지 후 5일 | FDA MedWatch 포털 |
| Critical 취약점 + 실제 악용 사례 확인 | FDA CISA 조율 공개 (CVD) 권장 | 즉시 연락 | FDA Digital Health Center of Excellence |
| 자진 사이버보안 소통 (선제적 고객 알림) | FDA 권장 (필수 아님) | 패치 릴리즈 시 | 고객 직접 통보 |

### 7.2 MFDS 보고 기준

| 상황 | 보고 유형 | 기한 |
|---|---|---|
| 사이버보안 원인의 이상사례 (환자 위해) | MFDS 이상사례 보고 | 인지 후 30일 이내 |
| 중대 이상사례 (사망·중증) | MFDS 이상사례 보고 (긴급) | 인지 후 15일 이내 |

### 7.3 EU MDR 보고 기준

| 상황 | 보고 유형 | 기한 |
|---|---|---|
| 사이버보안 원인의 심각한 사고 | MDR Article 87 — Serious Incident 보고 | 인지 후 15일 이내 |
| 사망 또는 예상치 못한 심각한 건강 악화 | MDR Article 87 — 즉시 보고 | 즉시 |
| FSCA (Field Safety Corrective Action) 개시 | EUDAMED 등록 | 즉시 |

---

## 8. 협조적 취약점 공개 (CVD — Coordinated Vulnerability Disclosure) 절차

### 8.1 CVD 개요

HnVue는 ISO/IEC 29147 (취약점 공개) 및 ISO/IEC 30111 (취약점 처리)에 따른 CVD 프로세스를 운영한다. 외부 연구자 또는 고객이 HnVue Console SW의 취약점을 발견한 경우, 책임 있는 방식으로 신고하고 조율하여 공개한다.

DOC-016(Cybersecurity Management Plan)의 VDP(Vulnerability Disclosure Policy) 정책과 완전히 일치한다.

### 8.2 취약점 신고 채널

| 채널 | 정보 |
|---|---|
| **보안 신고 이메일** | security@hnvue.com |
| **PGP 공개키 (선택 사항)** | [TBD - 보안 민감 취약점 암호화 신고용 공개키 발행 예정] |
| **공개 채널 게시** | https://www.hnvue.com/security (웹사이트 VDP 페이지 — 출시 전 준비 예정) |
| IFU 명시 여부 | ✅ IFU §X에 security@hnvue.com 명시 예정 |

### 8.3 CVD 프로세스 흐름

```
[외부 연구자/고객 신고]
  security@hnvue.com 수신
        ↓
[접수 확인 (72시간 이내)]
  - 신고자에게 접수 확인 이메일 발송
  - 예상 처리 기간 안내
        ↓
[취약점 검증 및 분류]
  - 재현 확인 (개발 환경)
  - CVSS v3.1 점수 산정
  - DOC-019 SBOM 연계 영향 범위 평가
  - DOC-047 위험 평가 기준 적용 (환자 피해 가능성)
        ↓
[패치 개발 및 배포]
  - §6 대응 프로세스 따름
  - 신고자에게 진행 상황 정기 업데이트 (최소 30일마다)
        ↓
[공개 (Disclosure)]
  - 패치 배포 후 신고자와 공개 일정 협의
  - 공개 내용: CVE 번호 신청, 패치 정보, 신고자 크레딧(동의 시)
  - 최대 비공개 기간: 90일 (Critical: 30일, 패치 지연 시 연장 협의)
        ↓
[DOC-045 VEX 업데이트 및 완료 처리]
```

### 8.4 CVD SLA

| 항목 | 목표 기간 |
|---|---|
| 접수 확인 | 72시간 이내 |
| 취약점 검증 완료 | 14일 이내 |
| 패치 개발 완료 | §5 SLA 준수 (심각도별) |
| 최대 비공개 기간 | 90일 (Critical: 30일) |
| 신고자 진행 상황 업데이트 | 최소 30일마다 |

---

## 9. 고객 통보 프로세스

### 9.1 보안 패치 알림 방법

| 상황 | 알림 방법 | 알림 내용 | 기한 |
|---|---|---|---|
| Critical/High 취약점 패치 배포 | 이메일 (등록 고객 전체) + 고객 포털 공지 | 취약점 설명(기술 세부 정보 제한), 패치 버전, 설치 방법, 미패치 임시 완화 조치 | 패치 배포와 동시 |
| Medium 취약점 패치 배포 | 릴리즈 노트 + 고객 포털 | 패치 내용 요약 | 릴리즈 시 |
| Low 취약점 패치 배포 | 릴리즈 노트 | 변경 이력 포함 | 릴리즈 시 |
| 패치 대기 중 임시 완화 조치 | 이메일 + 포털 보안 권고문 | 완화 조치 방법, 위험 수준 설명, 패치 예정일 | 탐지 후 72시간 이내 (Critical) |

### 9.2 고객 알림 템플릿

```
제목: [보안 알림] HnVue Console SW (HnVue) v[패치 버전] 보안 업데이트 안내

안녕하십니까, HnVue 기술 지원팀입니다.

HnVue Console SW (HnVue)의 보안 취약점 패치를 포함한 v[버전] 업데이트를 배포합니다.

■ 보안 업데이트 요약
  - 취약점 심각도: [Critical / High / Medium]
  - 취약점 영향: [간략 설명 — 기술적 세부 정보 제한]
  - 환자 안전 영향: [없음 / 낮음 / 있음: 설명]

■ 권고 조치
  - 가능한 빠른 시일 내 v[버전]으로 업데이트 설치를 권장합니다.
  - 업데이트 방법: IFU §X 참조 또는 첨부 업데이트 가이드 참조
  - 패치 적용 전 임시 완화 조치: [해당 시 기재]

■ 다운로드
  - 패치 버전 다운로드: [고객 포털 URL]
  - SHA-256 체크섬: [TBD - 릴리즈 시 작성] (무결성 확인 필수)
  - Authenticode 서명 확인 방법: 설치 패키지 우클릭 → 속성 → 디지털 서명

■ 문의
  - 기술 지원: support@hnvue.com
  - 보안 문의: security@hnvue.com

감사합니다.
HnVue 기술 지원팀
```

### 9.3 패치 미적용 고객 임시 완화 조치 예시

| 취약점 유형 | 임시 완화 조치 |
|---|---|
| 네트워크 취약점 (DICOM 포트 EP-001) | 병원 방화벽에서 비인가 IP의 DICOM 포트(104/11112) 차단 설정 요청 |
| 인증 취약점 | 비밀번호 복잡도 강화 및 불필요한 계정 비활성화 요청 |
| 데이터 노출 취약점 | HnVue Console SW 사용 후 즉시 로그아웃, 자동 잠금(15분) 활성화 확인 요청 |
| SW 무결성 취약점 (DLL 관련) | 촬영실 PC에 대한 물리적 접근 통제 강화 요청 (USB 포트 잠금 등) |
| HL7/FHIR 인터페이스 취약점 | 소스 IP 화이트리스트 재확인 요청 |

---

## 10. 연간 VMP 검토

| 항목 | 내용 |
|---|---|
| 검토 주기 | 연 1회 이상 (4분기 정기 보안 리뷰와 통합 가능) |
| 검토 트리거 | 주요 CVE 발생, 규제 요건 변경, 보안 사고 발생, 연간 정기 |
| 검토 내용 | VMP 유효성 확인, SLA 달성률 검토, 미해결 취약점 현황, DOC-019 SBOM/DOC-045 VEX 최신화 여부, DOC-017 위협 모델 재검토 필요성 판단 |
| 검토 결과물 | VMP 개정판 또는 "변경 없음" 확인 기록 |
| 담당자 | [TBD] |

> **ISO 13485 연간 관리 검토 통합**: VMP 연간 검토는 ISO 13485 연간 관리 검토(Management Review) 및 EU MDR Article 83–86 시판 후 감시(PMS) 연간 검토와 통합하여 관리 부담을 최소화한다.

---

## 11. 취약점 이력 관리

취약점 발견부터 해결까지의 전체 이력을 관리한다. 이력 기록은 FDA MDR 조사 대응 및 EU MDR 기술 파일 최신화에 활용된다.

| 항목 | 내용 |
|---|---|
| 이력 기록 도구 | 내부 이슈 트래커 [TBD - 도구 선정 후 작성] |
| 이력 보존 기간 | 제품 수명 (약 10년) + 5년 = 총 15년 |
| 기록 항목 | CVE ID, SBOM 연계 컴포넌트, 탐지일, 분류일, VEX 상태, 패치 릴리즈일, 고객 알림일, 규제 기관 보고 여부 |
| 접근 권한 | 사이버보안 팀, QA/RA 담당자 |

### 11.1 취약점 이력 테이블 (계획 — 시판 후 채울 예정)

| 발생일 | CVE ID | SBOM-ID | 컴포넌트 | CVSS | VEX 상태 | 패치 릴리즈 버전 | 고객 알림일 | FDA 보고 여부 |
|--------|--------|---------|---------|------|---------|---------------|------------|--------------|
| [TBD - 시판 후 작성] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |

---

## 12. EU MDR PMS(Post-Market Surveillance) 연계

본 VMP는 EU MDR Article 83–86에서 요구하는 시판 후 감시(PMS) 계획의 사이버보안 특화 절차서로 기능한다. EU MDR PMS Plan과 다음과 같이 연계된다.

| EU MDR PMS 요건 | VMP 대응 절차 |
|---|---|
| Article 83 — 능동적 PMS 시스템 | §3 자동/수동 취약점 모니터링 소스 |
| Article 84 — PMS 계획 | 본 VMP 전체 (특히 §5 SLA, §6 대응 프로세스) |
| Article 85 — PMSR/PSUR | §11 취약점 이력 → 연간 보안 요약 보고 |
| Article 86 — PMS 데이터 활용 | §10 연간 VMP 검토 → DOC-047 위험 평가 업데이트 |
| Article 87 — Serious Incident 보고 | §7.3 EU MDR 보고 기준 |

---

## 13. 변경 이력

| 버전 | 변경일 | 변경자 | 변경 내용 |
|---|---|---|---|
| 1.0 | 2026-03-31 | 사이버보안 팀 | 최초 작성 — 개발 착수 전 계획 수준 초안 |

---

## 14. 승인

| 역할 | 이름 | 서명 | 일자 |
|---|---|---|---|
| 작성자 (사이버보안 담당) | [TBD] | [TBD] | 2026-03-31 |
| 검토자 (개발팀 리드) | [TBD] | [TBD] | [TBD] |
| 검토자 (QA/RA 담당) | [TBD] | [TBD] | [TBD] |
| 승인자 (QA/RA 책임자) | [TBD] | [TBD] | [TBD] |

---

*문서 끝 (End of Document)*
