# VEX 리포트 (Vulnerability Exploitability eXchange Report)

> **문서 번호**: DOC-045
> **버전**: 1.0 (계획서)
> **작성일**: 2026-03-31
> **작성자**: 사이버보안 팀 (Cybersecurity Team)
> **검토자**: SW 아키텍트, QA 팀장
> **승인자**: 의료기기 RA/QA 책임자
> **제품**: HnVue Console SW (RadiConsole™)
> **회사**: HnVue (가칭)
> **분류**: ⭐ 리스크 최소화 (FDA 강력 권장) | ⭐ EU MDR 권장
> **적용 시장**: FDA 510(k) (강력 권장), EU MDR Class IIa (권장), MFDS 2등급 (해당 없음)
> **근거 규격**: FDA Cybersecurity Guidance (Feb 2026), FDA Top Deficiency 대응, CISA VEX 가이드라인, CycloneDX VEX 1.5, OpenVEX v0.2.0
> **IEC 62304 클래스**: Class B

---

## 개정 이력 (Revision History)

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| 1.0 | 2026-03-31 | 최초 작성 — 개발 착수 전 계획 수준 초안 (DOC-019 SBOM 연계) | 사이버보안 팀 |

---

## 관련 문서 (Related Documents)

| 문서 ID | 문서명 | 관계 |
|---------|--------|------|
| DOC-016 | 사이버보안 관리 계획서 (Cybersecurity Management Plan) | 상위 사이버보안 전략 |
| DOC-017 | 위협 모델링 보고서 (Threat Model — STRIDE) | STRIDE 위협 분석 결과 참조 |
| DOC-019 | 소프트웨어 자재 명세서 (SBOM) | VEX 기준 SBOM (SBOM-XRAY-GUI-001) |
| DOC-046 | 사이버보안 핵심 통제 명세서 | 보안 통제 구현 현황 |
| DOC-048 | 취약점 관리 계획 (VMP) | VEX 업데이트 절차 |

---

## 1. 문서 정보

| 항목 | 내용 |
|---|---|
| 제품명 | HnVue Console SW (RadiConsole™) |
| 회사명 | HnVue (가칭) |
| SW 버전 | v1.0 (개발 예정) |
| 기준 SBOM 문서 | DOC-019 (SBOM-XRAY-GUI-001) v1.0 |
| 기준 SBOM 파일 | `hnvue-consolesw-v1.0-sbom.bom.json` (CycloneDX 1.5) |
| VEX 파일명 | `hnvue-consolesw-v1.0-vex.json` |
| VEX 파일 해시 (SHA-256) | [TBD - 개발 완료 후 작성] |
| CVE 기준일 (NVD 조회일) | [TBD - 개발 완료 후 작성] |
| 의존 문서 | DOC-019 (SBOM) |

> **⚠ 계획서 주의사항**: 본 문서는 개발 착수 전 계획 수준으로 작성된 문서이다. `[TBD - 개발 완료 후 작성]`으로 표시된 항목은 소프트웨어 개발 완료 후 실제 CVE 스캔 결과를 기반으로 작성한다.

---

## 2. VEX 개요

### 2.1 VEX란 무엇인가

VEX(Vulnerability Exploitability eXchange)는 SBOM(DOC-019)에 포함된 컴포넌트에 알려진 CVE(공개 취약점)가 존재할 때, 해당 취약점이 **이 제품(HnVue Console SW)에서 실제로 악용 가능한지 여부**를 명시하는 문서이다.

예를 들어 OpenSSL(SBOM-020)에 CVE가 발견되었더라도 HnVue Console SW가 해당 취약한 기능을 사용하지 않거나, 이미 패치된 버전을 사용 중이라면 "Not Affected" 또는 "Fixed"로 분류하여 무의미한 보안 경보 노이즈를 제거할 수 있다.

### 2.2 VEX 형식

| 항목 | 선택 |
|---|---|
| CycloneDX VEX 1.5 (OWASP) | ☑ 선택 (기본) |
| OpenVEX v0.2.0 | ☐ 선택 |
| CSAF VEX (OASIS) | ☐ 선택 |

> **선택 근거**: DOC-019 SBOM이 CycloneDX 1.5 형식으로 작성되어 있으므로, VEX도 동일 생태계(CycloneDX VEX 1.5)를 사용하여 SBOM과 기계 판독 방식으로 연동한다.

### 2.3 스캔 도구 계획

| 항목 | 내용 |
|---|---|
| CVE 스캔 도구 (계획) | Grype v0.x 또는 Trivy v0.x (CI/CD 파이프라인 통합) |
| 보조 스캔 도구 | OWASP Dependency-Check (DOC-019 §2.1 참조) |
| 스캔 실행 명령어 (참고) | `grype sbom:hnvue-consolesw-v1.0-sbom.bom.json -o json > cve-scan.json` |
| NVD API 연동 여부 | ☑ 예 (계획) |

---

## 3. VEX 상태 정의

| 상태 | 의미 | FDA 해석 |
|---|---|---|
| **Not Affected** | 해당 CVE의 취약한 코드 경로가 HnVue Console SW에서 도달 불가 또는 비활성화 | 추가 조치 불필요 — 근거 반드시 명시 |
| **Not Exploitable** | 취약점이 존재하나 HnVue Console SW 아키텍처상 실질적 악용이 불가 | 추가 조치 불필요 — 기술적 근거 필수 |
| **Affected** | HnVue Console SW에서 해당 취약점이 악용 가능한 상태 | 패치 또는 완화 조치 필요 |
| **Fixed** | 해당 CVE가 현재 버전에서 이미 해결됨 | 패치 버전 및 방법 명시 |
| **Under Investigation** | 악용 가능성 조사 진행 중 | 조사 완료 기한 명시 필요 |

---

## 4. CVE 분석 결과 — 상세 테이블

> **계획서 작성 지침**:
> 이하 테이블은 DOC-019 SBOM(SBOM-XRAY-GUI-001)에 등재된 38개 배포 구성요소를 기준으로, 개발 완료 후 CVE 스캔 도구(Grype/Trivy)로 스캔하여 채울 예정이다.
> 계획서 단계에서는 DOC-017 위협 모델의 SBOM 연계 섹션(§10)에서 식별한 고위험 모니터링 대상 컴포넌트에 대한 분석 구조를 사전 정의한다.

### 4.1 Critical (CVSS 9.0 이상)

| CVE ID | 영향 컴포넌트 (SBOM-ID) | 버전 | CVSS | VEX 상태 | 착취 가능 경로 / 근거 | 완화 조치 / 패치 | 분석자 | 검토일 |
|---|---|---|---|---|---|---|---|---|
| [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |

> **고위험 모니터링 대상**: DOC-017 §10(SBOM 연계) 및 DOC-019 §6.2에서 식별된 우선순위 1~2 컴포넌트(OpenSSL 3.x / SBOM-020, BouncyCastle 2.3.0 / SBOM-019)는 Critical CVE 발생 가능성이 높아 개발 완료 후 최우선으로 분석한다.

### 4.2 High (CVSS 7.0 ~ 8.9)

| CVE ID | 영향 컴포넌트 (SBOM-ID) | 버전 | CVSS | VEX 상태 | 착취 가능 경로 / 근거 | 완화 조치 / 패치 | 분석자 | 검토일 |
|---|---|---|---|---|---|---|---|---|
| [TBD - 개발 완료 후 작성] | OpenSSL (SBOM-020) | 3.2.1 | [TBD] | [TBD] | [TBD — 코드 검토 후 HnVue Console SW의 해당 기능 사용 여부 확인 필요] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | fo-dicom (SBOM-009) | 5.1.3 | [TBD] | [TBD] | [TBD — DICOM 파서 취약 함수 호출 경로 코드 검토 필요] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | DCMTK (SBOM-010) | 3.6.8 | [TBD] | [TBD] | [TBD — TM-T-001 위협과 연계 검토 필요 (DOC-017 §5.3 참조)] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | BouncyCastle (SBOM-019) | 2.3.0 | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | Entity Framework Core (SBOM-018) | 8.0.2 | [TBD] | [TBD] | [TBD — TM-E-002 SQL Injection 위협과 연계 검토 필요 (DOC-017 §5.7 참조)] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | .NET 6.0 Runtime (SBOM-002) | 6.0.36 | [TBD] | [TBD] | [TBD — TM-T-004 업데이트 변조 위협과 연계 검토 필요 (DOC-017 §5.3 참조)] | [TBD] | [TBD] | [TBD] |

### 4.3 Medium (CVSS 4.0 ~ 6.9)

| CVE ID | 영향 컴포넌트 (SBOM-ID) | 버전 | CVSS | VEX 상태 | 착취 가능 경로 / 근거 | 완화 조치 / 패치 | 분석자 | 검토일 |
|---|---|---|---|---|---|---|---|---|
| [TBD - 개발 완료 후 작성] | OpenCV (SBOM-013) | 4.9.0 | [TBD] | [TBD] | [TBD — 영상 처리 입력 파서 취약 경로 검토 필요] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | NHapi HL7 (SBOM-024) | 3.2.0 | [TBD] | [TBD] | [TBD — TM-S-005 HL7 위협과 연계 검토 필요 (DOC-017 §5.2 참조)] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | ITK.NET Wrapper (SBOM-014) | 5.3.0 | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| [TBD - 개발 완료 후 작성] | zlib (SBOM-035) | 1.3.1 | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |

### 4.4 Low (CVSS 0.1 ~ 3.9)

| CVE ID | 영향 컴포넌트 (SBOM-ID) | 버전 | CVSS | VEX 상태 | 착취 가능 경로 / 근거 | 완화 조치 / 패치 | 분석자 | 검토일 |
|---|---|---|---|---|---|---|---|---|
| [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |

---

## 5. VEX 요약 통계

> **계획서 주의**: 이하 수치는 개발 완료 후 실제 CVE 스캔 결과로 대체된다.

| 상태 | Critical | High | Medium | Low | 합계 |
|---|---|---|---|---|---|
| Not Affected | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| Not Exploitable | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| Fixed | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| Affected (조치 필요) | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| Under Investigation | [TBD] | [TBD] | [TBD] | [TBD] | [TBD] |
| **총 CVE 수** | **[TBD]** | **[TBD]** | **[TBD]** | **[TBD]** | **[TBD]** |

---

## 6. SBOM 컴포넌트별 분석 우선순위 계획

DOC-019(SBOM-XRAY-GUI-001) §6.2 고위험 모니터링 대상 및 DOC-017(위협 모델) §10 SBOM 연계를 기반으로 CVE 분석 우선순위를 사전 정의한다.

| 우선순위 | SBOM-ID | 구성요소명 | 버전 | 근거 (DOC-017 연계 위협) | 분석 계획 |
|----------|---------|-----------|------|--------------------------|----------|
| 1 (최우선) | SBOM-020 | OpenSSL (Native) | 3.2.1 | TM-I-001 (네트워크 스니핑, PHI 유출) — 암호화 핵심 | 개발 완료 즉시 Grype 스캔 + 코드 경로 분석 |
| 1 (최우선) | SBOM-019 | BouncyCastle.Cryptography | 2.3.0 | 인증/암호화 기능 — 과거 Critical CVE 이력 존재 | 개발 완료 즉시 분석 |
| 2 (높음) | SBOM-009 | fo-dicom | 5.1.3 | TM-T-001 (DICOM 영상 변조), TM-D-001 (DICOM Flooding) | DICOM 파서 취약 경로 코드 레벨 검토 |
| 2 (높음) | SBOM-010 | DCMTK | 3.6.8 | TM-T-001 (DICOM 영상 변조) | DCMTK 3.6.8 패치 이력 확인 |
| 3 (보통) | SBOM-012 | OpenCvSharp4 | 4.9.0 | 영상 처리 입력 파서 취약점 가능성 | OpenCV Native 연계 분석 |
| 3 (보통) | SBOM-013 | OpenCV (Native) | 4.9.0 | 영상 파일 파서 취약점 — CVE-2023-xxxx 패치 이력 확인 필요 | 패치 상태 및 사용 함수 검토 |
| 3 (보통) | SBOM-002 | .NET 6.0 Runtime | 6.0.36 | TM-T-004 (SW 업데이트 변조) — 런타임 전체 영향 | .NET 6.0 EOL(2024-11) 후 업그레이드 계획 검토 |
| 4 (낮음) | SBOM-018 | Entity Framework Core | 8.0.2 | TM-E-002 (SQL Injection) | EF Core 파라미터화 쿼리 사용 여부 코드 검토 |
| 4 (낮음) | SBOM-024 | NHapi HL7 | 3.2.0 | TM-S-005 (HL7 위조) | HL7 파서 입력 검증 코드 검토 |
| 5 (모니터링) | SBOM-035 | zlib (Native) | 1.3.1 | 간접 영향 (압축 라이브러리) | 정기 스캔으로 모니터링 |
| 5 (모니터링) | 나머지 SBOM | WPF, MahApps 등 UI | — | 낮은 CVE 위험 — CVE 없음 확인 | 정기 스캔으로 충분 |

> **참고**: .NET 6.0 Runtime (SBOM-002)은 EOL(End of Life, 2024년 11월)로, .NET 8.0 LTS 또는 .NET 9.0으로의 업그레이드 계획을 검토해야 한다. 개발 완료 전 업그레이드 여부를 결정하고, VEX 작성 시 해당 버전을 반영한다.

---

## 7. Affected 항목 해결 계획

> 개발 완료 후 §4절 CVE 분석에서 VEX 상태가 **Affected**로 판정된 CVE에 대한 구체적 해결 계획을 기재한다.

| CVE ID | 컴포넌트 (SBOM-ID) | CVSS | 해결 방법 | 목표 릴리즈 | 담당자 | 완료 예정일 |
|---|---|---|---|---|---|---|
| [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | [TBD] | v[TBD] | [TBD] | [TBD] |

---

## 8. VEX 파일 첨부

> 개발 완료 후 실제 스캔 결과에 따라 채운다.

| # | 파일명 | 형식 | 해시 (SHA-256) | 설명 |
|---|---|---|---|---|
| 1 | `hnvue-consolesw-v1.0-vex.json` | CycloneDX VEX 1.5 | [TBD - 개발 완료 후 작성] | 기계 판독 VEX 파일 (FDA 제출용) |
| 2 | `hnvue-consolesw-v1.0-cve-scan-raw.json` | Grype/Trivy 출력 | [TBD - 개발 완료 후 작성] | CVE 스캔 원시 데이터 |

---

## 9. 취약점 모니터링 지속 프로세스

VEX는 FDA 510(k) 제출 시점 한 번으로 완료되지 않는다. 시판 후에도 신규 CVE 발생 시 DOC-048(VMP)에 따라 지속 업데이트한다.

| 모니터링 소스 | 주기 | 담당자 | 알림 방법 |
|---|---|---|---|
| NVD (National Vulnerability Database) | 주 1회 자동 조회 | [TBD] | Grype/Trivy CI/CD 알림 |
| CISA KEV (Known Exploited Vulnerabilities) | 즉시 (실시간) | [TBD] | RSS 구독 또는 이메일 알림 |
| GitHub Security Advisories | 의존성 변경 시 | [TBD] | GitHub Dependabot 알림 |
| CISA ICS-CERT 권고문 | 월 1회 검토 | [TBD] | 이메일 구독 |
| 고객/외부 연구자 신고 | 즉시 | [TBD] | security@hnvue.com |
| Microsoft Security Update Guide | 월 1회 (Patch Tuesday) | [TBD] | 이메일 구독 |
| DCMTK / fo-dicom 공식 보안 공지 | 월 1회 | [TBD] | 벤더 보안 채널 구독 |

### 대응 SLA (DOC-048 VMP와 일치)

| 심각도 | 탐지 후 VEX 업데이트 기한 | 패치 목표 기한 |
|---|---|---|
| Critical (CVSS 9.0+) | 48시간 이내 | 30일 이내 |
| High (CVSS 7.0~8.9) | 7일 이내 | 90일 이내 |
| Medium (CVSS 4.0~6.9) | 30일 이내 | 다음 정기 릴리즈 |
| Low (CVSS 0.1~3.9) | 60일 이내 | 판단 후 결정 |

> **CISA KEV 특별 처리**: CVSS 점수와 무관하게 CISA KEV에 등재된 취약점은 Critical 수준으로 처리한다.

---

## 10. DOC-017 위협 모델과의 연계 (STRIDE ↔ VEX)

DOC-017 위협 모델의 STRIDE 분석 결과와 VEX 분석을 연계하여, 위협 시나리오에서 식별된 SOUP 컴포넌트의 CVE를 우선 분석한다.

| DOC-017 위협 ID | 위협 설명 | 관련 SBOM 컴포넌트 | VEX 분석 요구 수준 |
|-----------------|----------|-------------------|--------------------|
| TM-T-001 | DICOM 영상 전송 중 변조 | fo-dicom (SBOM-009), DCMTK (SBOM-010) | High — 코드 경로 분석 필수 |
| TM-T-004 | SW 업데이트 바이너리 변조 | .NET 6.0 Runtime (SBOM-002) | High — 코드 서명 검증 로직 연계 |
| TM-I-001 | 네트워크 스니핑으로 PHI 유출 | OpenSSL (SBOM-020) | Critical — TLS 구현 취약점 최우선 검토 |
| TM-E-002 | SQL Injection | Entity Framework Core (SBOM-018), SQLite (SBOM-016) | High — 파라미터화 쿼리 사용 확인 |
| TM-D-004 | 촬영 워크플로우 크래시 | Grpc.Net.Client (SBOM-022), RestSharp (SBOM-023) | Medium — DoS 취약점 검토 |
| TM-E-003 | DLL Injection | .NET 6.0 (SBOM-002) | High — DLL 경로 고정 및 서명 검증 연계 |
| TM-S-005 | HL7 발신지 위조 | NHapi HL7 (SBOM-024) | Medium — 파서 입력 검증 검토 |

---

## 11. 결론

> **[TBD - 개발 완료 후 작성]**
>
> 작성 지침: VEX 분석 완료 후 아래 항목을 포함하여 기술한다.
> - 총 CVE 수 및 상태별 요약 (Not Affected / Fixed / Affected / Under Investigation)
> - Affected 항목 중 v1.0 릴리즈 전 해결 여부
> - 잔여 위험 수용 선언 (Medium/Low Affected 항목에 대한 수용 근거 — ISO 14971 ALARP 원칙 적용)
> - 시판 후 VEX 업데이트 책임자 및 주기 (DOC-048 VMP와 일치)
>
> **계획서 단계 판단**: SBOM(DOC-019) §4.12에 따르면 38개 배포 구성요소 중 "알려진 CVE 없음" 상태로 초기 작성된 구성요소가 다수이나, 개발 기간 중 신규 CVE 발생 가능성이 있으므로 개발 완료 시점의 실제 스캔 결과를 기반으로 최종 VEX를 작성한다.

---

## 12. 변경 이력

| 버전 | 변경일 | 변경자 | 변경 내용 |
|---|---|---|---|
| 1.0 | 2026-03-31 | 사이버보안 팀 | 최초 작성 — 개발 착수 전 계획 수준 초안 |

---

## 13. 승인

| 역할 | 이름 | 서명 | 일자 |
|---|---|---|---|
| 작성자 (사이버보안 담당) | [TBD] | [TBD] | 2026-03-31 |
| 검토자 (개발팀 리드) | [TBD] | [TBD] | [TBD] |
| 검토자 (사이버보안 담당) | [TBD] | [TBD] | [TBD] |
| 승인자 (QA/RA 책임자) | [TBD] | [TBD] | [TBD] |

---

*문서 끝 (End of Document)*
