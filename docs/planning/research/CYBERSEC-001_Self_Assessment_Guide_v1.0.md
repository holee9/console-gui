# CYBERSEC-001: HnVue Console SW (HnVue) 사이버보안 자체 검증 종합 가이드

**문서 번호**: CYBERSEC-001  
**버전**: 1.0  
**작성일**: 2026-03-31  
**적용 제품**: HnVue Console SW (HnVue)  
**시스템**: HnX-R1 (Detector + Console SW 번들)  
**분류**: 내부 기술 문서 (사이버보안 검증 가이드)

---

## 목차

1. [문서 개요 및 범위](#1-문서-개요-및-범위)
2. [규제 요건 매핑](#2-규제-요건-매핑)
3. [자체 침투 테스트 허용 조건 및 판단 기준](#3-자체-침투-테스트-허용-조건-및-판단-기준)
4. [보안 검증 도구 목록 (오픈소스/무료)](#4-보안-검증-도구-목록-오픈소스무료)
5. [단계별 자체 검증 파이프라인](#5-단계별-자체-검증-파이프라인)
6. [외부 vs 자체 수행 의사결정 매트릭스](#6-외부-vs-자체-수행-의사결정-매트릭스)
7. [규제 제출 산출물 목록](#7-규제-제출-산출물-목록)
8. [잔여 위험 평가 및 수용 기준](#8-잔여-위험-평가-및-수용-기준)
9. [취약점 관리 및 사후 처리](#9-취약점-관리-및-사후-처리)
10. [체크리스트 (인허가 제출 전)](#10-체크리스트-인허가-제출-전)
11. [참고 규격 및 문서](#11-참고-규격-및-문서)

---

## 1. 문서 개요 및 범위

### 1.1 목적

본 가이드는 HnVue Console SW (HnVue)의 FDA 510(k), EU MDR Class IIa, MFDS 2등급 인허가 신청을 위해 사이버보안 검증을 최소 비용으로 자체 수행하기 위한 실무 가이드다. IEC 62304 Class B, §524B cyber device 해당 제품으로서 규제 기관이 요구하는 수준의 증거(Evidence)를 생성하는 것을 목표로 한다.

### 1.2 대상 제품 정보

| 항목 | 내용 |
|------|------|
| 제품명 | HnVue Console SW (HnVue) |
| 시스템 | HnX-R1 (X-ray Detector + Console SW 번들) |
| 기술스택 | WPF .NET 8, fo-dicom 5.x, SQLite, Serilog |
| 프로토콜 | DICOM 3.0, PACS 연동 (DIMSE over TCP/IP) |
| 소프트웨어 안전등급 | IEC 62304 Class B |
| FDA 분류 | §524B Cyber Device |
| 인허가 대상 | FDA 510(k), EU MDR Class IIa, MFDS 2등급 |

### 1.3 사이버보안 위협 표면 요약

HnVue Console SW의 주요 공격 표면은 다음과 같다:

```
[환자/운영자]
      |
[WPF UI / .NET 8 Application Layer]
      |
[DICOM 3.0 Stack (fo-dicom 5.x)]  ←→  [PACS 서버]
      |
[SQLite 로컬 DB (환자 메타데이터)]
      |
[Serilog 감사 로그]
      |
[OS / 하드웨어 (HnX-R1 Console PC)]
      |
[네트워크 (병원 VLAN / WAN)]
```

**핵심 보안 우려사항:**
- DICOM C-STORE, C-FIND, C-MOVE 서비스의 비인증 접근
- fo-dicom 라이브러리의 파싱 취약점 (버퍼 오버플로우, 악의적 DICOM 파일)
- SQLite 주입 공격 (SQL Injection)
- 네트워크 평문 전송 (DICOM 기본 포트 104 — TLS 미적용 시)
- NuGet 패키지 종속성 취약점 (SBOM 미관리 시)
- 로컬 권한 상승 공격

---

## 2. 규제 요건 매핑

### 2.1 FDA §524B (Cyber Device) — 2025 Final Guidance

FDA는 2025년 6월 27일 "Cybersecurity in Medical Devices: Quality System Considerations and Content of Premarket Submissions" 최종 가이던스를 발표했다. §524B를 만족하는 cyber device(소프트웨어를 포함하는 모든 기기)에 대해 다음을 **의무적으로** 요구한다:

| 요구사항 | 근거 | 자체 검증 대응 방법 |
|----------|------|---------------------|
| Cybersecurity Management Plan 제출 | §524B(b)(1) | 취약점 모니터링 + CVD 절차 문서화 |
| Reasonable Assurance of Cybersecurity 증명 | §524B(b)(2) | SAST + DAST + 침투 테스트 + 잔여위험 평가 |
| SBOM 제출 (machine-readable) | §524B(b)(3) | Syft/Grype 또는 OWASP Dependency-Check |
| 약 12개 표준화 사이버보안 문서 (eSTAR) | 2025 Guidance | 본 가이드 §7 참조 |
| Secure Product Development Framework (SPDF) | Appendix 4 | IEC 62304 프로세스에 보안 활동 통합 |
| 침투 테스트 결과 (독립성·전문성 입증) | Guidance §V | 독립 인력 수행 + 자격증 증빙 |
| Threat Model (STRIDE 등) | Guidance §V.A | 아키텍처 기반 위협 모델링 문서 |
| Fuzz Testing (입력 오남용 케이스) | Guidance §V.A | DVTk, Radamsa, AFL++ 활용 |
| 취약점 공개 정책 (CVD) | §524B(b)(1) | 보안 취약점 신고 채널 수립 |

> **중요**: 2025 최종 가이던스는 소프트웨어를 포함하면 네트워크 연결 여부와 무관하게 모든 기기를 "cyber device"로 분류한다. HnVue이 PACS 네트워크에 연결되므로 고위험 cyber device에 해당한다.

### 2.2 EU MDR Class IIa — MDCG 2019-16 사이버보안 가이던스

| 요구사항 | 근거 | 대응 방법 |
|----------|------|-----------|
| General Safety and Performance Requirements (GSPR) Annex I §17 | EU MDR 2017/745 | 사이버보안 위험관리를 ISO 14971에 통합 |
| 사이버보안 위험관리 파일 | MDCG 2019-16 §3.3 | 위협 모델 + 취약점 테스트 결과 포함 |
| Fuzz Testing (V&V 항목) | MDCG 2019-16 §3.7 | DICOM 파서 퍼징 결과 제출 |
| SBOM 및 SOUP(Software of Unknown Provenance) 관리 | IEC 62304 §8 | OWASP Dependency-Check 결과 |
| Post-Market Surveillance (PMS) 계획 | EU MDR Art. 83~86 | 취약점 모니터링 + PSUR 계획 |
| 보안 업데이트 메커니즘 | MDCG 2019-16 §3.5 | 서명된 업데이트 + 무결성 검증 |

### 2.3 MFDS 2등급 — 사이버보안 허가·심사 가이드라인 (2025 개정)

MFDS는 2025년 1월 가이드라인을 개정하여 FDA/EU MDR/IMDRF와의 정합성을 강화했다.

| 요구사항 | 내용 | 대응 방법 |
|----------|------|-----------|
| 사이버보안 위험평가 보고서 | 위협 모델 + 취약점 분석 | STRIDE 기반 위협 모델링 |
| 보안 시험 결과 Evidence | 실제 테스트 수행 증거 제출 | SAST/DAST/침투 테스트 리포트 |
| SBOM 제출 | 상용·오픈소스 구성요소 목록 | Syft SBOM JSON/CycloneDX |
| 추적성 (Traceability) | 보안 요구사항 ↔ 테스트 ↔ 위험관리 | 요구사항-테스트 매트릭스 |
| 취약점 관리 체계 | 사후 취약점 대응 절차 | CVD 정책 + 패치 계획 |
| IEC 81001-5-1 기반 보안 제어 | 인증·암호화·로깅 등 | 기술적 보안 통제 구현 증명 |

> **MFDS 특이사항**: 2025 개정에서 "형식적 문서 제출" 중심에서 "증거 기반(Evidence-based) 검증"으로 전환되었다. 단순 체크리스트 대신 실제 시험 수행 결과와 위험관리 활동의 일관성 증빙이 핵심이다.

### 2.4 IEC 62304 Class B — 사이버보안 통합 요건

| IEC 62304 절차 | 사이버보안 통합 방법 |
|----------------|---------------------|
| §5.1 소프트웨어 개발 계획 | Secure SDLC 활동 포함 (SAST/DAST 자동화) |
| §5.2 소프트웨어 요구사항 | 인증·권한·암호화·로깅 보안 요구사항 명시 |
| §5.4 소프트웨어 상세 설계 | 위협 모델 기반 보안 아키텍처 설계 |
| §5.6 소프트웨어 통합 테스트 | 보안 통합 테스트 (경계값, 비인증 접근) |
| §5.7 소프트웨어 시스템 테스트 | DAST + 침투 테스트 수행 |
| §7 소프트웨어 위험관리 | ISO 14971과 연계한 사이버보안 위험 통제 |
| §8 소프트웨어 구성 관리 | SBOM 관리 + 종속성 취약점 모니터링 |
| §9 소프트웨어 문제 해결 | 취약점 발견 시 CAPA 프로세스 |

---

## 3. 자체 침투 테스트 허용 조건 및 판단 기준

### 3.1 FDA 입장 정리

FDA는 침투 테스트 시 다음을 요구한다 (UL Solutions, FDA CAP Scheme 기준):

1. **Independence and technical expertise of testers** (테스터의 독립성과 기술 전문성)
2. **Scope, duration, methods and results of testing** (범위·기간·방법·결과 문서화)
3. **Assessment of findings and rationales** (발견 사항 평가 및 완화 여부 근거)

FDA는 **제3자 외부 업체를 명시적으로 의무화하지 않는다**. Innolitics 및 ICS Advisory 등 FDA 전문 컨설턴트에 따르면:

> "Penetration testing can be done internally but it needs to be done by someone who has not worked on the design of the medical device. The person must have credentials and certifications for doing pen-testing. FDA will almost certainly not allow an engineer who helped develop the device to do the testing."

### 3.2 자체 침투 테스트 허용 조건 (체크리스트)

| 조건 | 요구사항 | 검증 방법 |
|------|----------|-----------|
| ✅ 독립성 | 개발 팀에 참여하지 않은 인력 수행 | 이력서·업무 이력으로 비참여 증명 |
| ✅ 기술 자격 | CEH, OSCP, GPEN, CompTIA PenTest+ 등 | 자격증 사본 문서화 |
| ✅ 방법론 | OWASP, PTES, NIST SP 800-115 등 공인 방법론 적용 | 방법론 문서 + 테스트 계획 작성 |
| ✅ 문서화 | 범위·기간·도구·결과·발견사항·완화 근거 | 최종 침투 테스트 리포트 |
| ✅ 의료기기 특화 | DICOM, PHI 처리 등 의료기기 맥락 이해 | DICOM 위협 시나리오 포함 |

### 3.3 자체 수행 가능 영역 vs 외부 권고 영역

| 검증 영역 | 자체 수행 가능 여부 | 근거 |
|-----------|-------------------|------|
| SAST (정적 분석) | ✅ 완전 자체 가능 | 개발팀 주도 CI/CD 통합 허용 |
| SCA (종속성 분석) | ✅ 완전 자체 가능 | 자동화 도구, 개발 중 상시 수행 |
| 네트워크 취약점 스캐닝 | ✅ 자체 가능 | OpenVAS/Nmap은 자동화 스캐닝 |
| DICOM 프로토콜 퍼징 | ✅ 자체 가능 | DVTk, Radamsa 조합, 내부 QA팀 |
| DAST (웹/API) | ✅ 자체 가능 | ZAP 자동 스캐닝 |
| 침투 테스트 | ⚠️ 조건부 자체 가능 | 독립 인력 + 자격증 필수 |
| 레드팀 고급 공격 | ❌ 외부 권장 | 고급 exploit 체인, APT 시뮬레이션 |
| 물리 보안 테스트 | ✅ 자체 가능 | 내부 보안팀 |

---

## 4. 보안 검증 도구 목록 (오픈소스/무료)

### 4.1 SAST — .NET 8 / C# / WPF 특화

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | FDA/MDR/MFDS 수용성 |
|------|-----|----------|------|-----------------|---------------------|
| **Security Code Scan** | https://security-code-scan.github.io | MIT | .NET/.NET Core Roslyn 기반 SAST, 인젝션·XXE·암호화 취약점 탐지 | `dotnet add package SecurityCodeScan.VS2019`, CI/CD (GitHub Actions) 통합, 빌드 시 자동 실행 | ✅ FDA SPDF의 SAST 요건 충족, MFDS 보안 시험 Evidence 제출 가능 |
| **Roslyn Security Guard** | https://dotnet-security-guard.github.io | LGPL-2.1 | Roslyn 분석기, 28종 취약점 패턴 68 시그니처 (SQLi, XSS, Path Traversal 등) | Visual Studio 확장 설치 + MSBuild 통합, 리포트 자동 생성 | ✅ C# SQLite 인젝션 탐지 — HnVue SQLite 쿼리 직접 스캔 |
| **DevSkim (Microsoft)** | https://github.com/microsoft/DevSkim | MIT | VS Code/Visual Studio 확장, C#/C++/Python 멀티언어, 하드코딩 시크릿 탐지 | VS 확장 설치 후 실시간 분석, CLI: `devskim analyze ./src` | ✅ 하드코딩 자격증명 탐지 — PACS 비밀번호 하드코딩 방지 |
| **SonarQube Community** | https://www.sonarsource.com/products/sonarqube | LGPL-3.0 | 다국어 SAST, C# 포함, 기술 부채·보안 취약점 통합 분석 | Docker: `docker run -p 9000:9000 sonarqube:community`, `sonar-scanner` CI 통합 | ✅ FDA Guidance SAST 요건, MFDS Evidence 보고서 생성 가능 |
| **Semgrep OSS** | https://semgrep.dev | LGPL-2.1 | 오픈소스 SAST, 커스텀 룰 지원, OWASP Top 10 룰셋 제공 | `semgrep --config=p/csharp src/`, fo-dicom 파싱 코드 커스텀 룰 작성 가능 | ✅ 커스텀 DICOM 처리 코드 취약점 패턴 정의 가능 |

### 4.2 SCA — NuGet 종속성 / SBOM

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | 규제 수용성 |
|------|-----|----------|------|-----------------|-------------|
| **OWASP Dependency-Check** | https://owasp.org/www-project-dependency-check | Apache 2.0 | NuGet/Maven/npm 종속성 CVE 스캐닝, NVD DB 연동 | `dependency-check.bat --project HnVue --scan . --format HTML,JSON` | ✅ FDA §524B SBOM 요건 부분 충족, MFDS SBOM 증빙 |
| **Syft (Anchore)** | https://github.com/anchore/syft | Apache 2.0 | SBOM 생성 (SPDX, CycloneDX, JSON 형식) | `syft dir:. -o cyclonedx-json > sbom.json` | ✅ FDA machine-readable SBOM 요건 완전 충족 (CycloneDX) |
| **Grype (Anchore)** | https://github.com/anchore/grype | Apache 2.0 | SBOM 기반 CVE 취약점 매칭 | `grype sbom:./sbom.json` | ✅ Syft SBOM과 연계하여 CVE 자동 매핑 |
| **Trivy (Aqua Security)** | https://github.com/aquasecurity/trivy | Apache 2.0 | 파일시스템/컨테이너/Git 리포 취약점 스캐닝 | `trivy fs --scanners vuln,secret,misconfig .` | ✅ Secret 탐지 포함 — 하드코딩 PACS 자격증명 스캔 |
| **OSV-Scanner (Google)** | https://github.com/google/osv-scanner | Apache 2.0 | 오픈소스 취약점 스캐너, OSV DB (NVD + GitHub Advisory) | `osv-scanner --lockfile packages.lock.json` | ✅ Google OSV DB 기반 최신 취약점 확인 |
| **dotnet-outdated** | https://github.com/dotnet-outdated/dotnet-outdated | MIT | .NET NuGet 패키지 업데이트 확인 | `dotnet outdated` — fo-dicom 5.x 최신 버전 확인 | 보조 도구 (CVE 직접 스캔 아님) |

### 4.3 네트워크 / 인프라 스캐닝

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | 규제 수용성 |
|------|-----|----------|------|-----------------|-------------|
| **Nmap** | https://nmap.org | GPL-2.0 | 포트 스캔, 서비스 탐지, NSE 스크립트 (취약점 스캔) | `nmap -sV -sC -p 104,2762,1433 <Console_IP>` | ✅ FDA 공격 표면 분석 요건, MFDS 네트워크 보안 시험 |
| **OpenVAS / Greenbone** | https://www.greenbone.net/en/community-edition | GPL-2.0 | 44,000+ 취약점 DB 기반 네트워크 취약점 스캐닝 | Docker: `docker run -d -p 9390:9390 greenbone/gvmd`, 콘솔 PC 대상 스캔 | ✅ FDA/MFDS 취약점 스캐닝 요건 |
| **Wireshark** | https://www.wireshark.org | GPL-2.0 | 네트워크 패킷 캡처/분석 | DICOM 통신 캡처: 필터 `dicom`, 평문 PHI 전송 여부 확인 | ✅ DICOM 평문 전송 탐지 — MDR §17 암호화 요건 증빙 |
| **testssl.sh** | https://testssl.sh | GPL-2.0 | TLS/SSL 설정 검증 (프로토콜 버전, 암호 스위트) | `./testssl.sh <PACS_IP>:2762` | ✅ DICOM TLS 설정 검증 — TLS 1.2+ 강제 여부 확인 |

### 4.4 DAST — 애플리케이션 동적 분석

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | 규제 수용성 |
|------|-----|----------|------|-----------------|-------------|
| **OWASP ZAP** | https://www.zaproxy.org | Apache 2.0 | 웹앱 프록시 기반 DAST, 자동 스캐닝 | HnVue가 REST API/웹 관리 콘솔 제공 시 적용. WPF 앱의 경우 HTTP 요청 프록시 캡처 분석 | ✅ FDA DAST 요건 |
| **Burp Suite Community** | https://portswigger.net/burp/communitydownload | Free (제한) | 웹앱 보안 테스트 프록시 | PACS 연동 HTTP API 테스트 (무료 버전 수동 테스트) | ✅ 수동 침투 테스트 보조 도구 |

### 4.5 Fuzzing — DICOM 프로토콜 특화

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | 규제 수용성 |
|------|-----|----------|------|-----------------|-------------|
| **DVTk (DICOM Validation Toolkit)** | https://dvtk.org | LGPL-3.0 | DICOM/HL7/IHE 프로토콜 검증, .NET 통합, 자동화 스크립트 지원 | DVTk DICOM Compare + Storage SCU/SCP 시뮬레이션, C-STORE 비정상 데이터 전송 테스트 | ✅ DICOM 프로토콜 특화 검증 — FDA DICOM 인터페이스 테스트 |
| **Radamsa** | https://gitlab.com/akihe/radamsa | MIT | 변이 기반 퍼저, DICOM 파일 퍼징에 IOActive가 사용 | `radamsa sample.dcm > fuzzed.dcm`, Python 스크립트로 dcmsend 연계 | ✅ FDA Fuzz Testing 요건, MDCG 2019-16 §3.7 |
| **DCMTK** | https://dicom.offis.de/dcmtk | BSD-like | DICOM 도구 모음 (dcmsend, findscu, echoscu), 보안 테스트에 활용 | `echoscu <IP> 104` (C-ECHO), `dcmsend <IP> 104 fuzzed.dcm` | ✅ DICOM 연결 테스트 및 퍼징 전송에 사용 |
| **AFL++** | https://github.com/AFLplusplus/AFLplusplus | Apache 2.0 | 커버리지 기반 퍼저, DICOM 파서 타겟 | fo-dicom DICOM 파서 타겟 바이너리 구성 후 AFL++ 퍼징 (고급 설정 필요) | ✅ FDA Fuzz Testing 요건 — 파서 레벨 취약점 발견 |
| **AFLNet** | https://github.com/aflnet/aflnet | Apache 2.0 | 네트워크 프로토콜 퍼징 (DICOM 테스트 사례 있음) | DICOM SCP 서버 프로세스 대상 네트워크 퍼징 (기술적 설정 복잡) | ✅ 네트워크 프로토콜 퍼징 — DICOM 포트 104/2762 대상 |

### 4.6 침투 테스트 프레임워크

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 | 규제 수용성 |
|------|-----|----------|------|-----------------|-------------|
| **Kali Linux** | https://www.kali.org | Free (OS) | 600+ 보안 도구 번들 OS, 침투 테스트 전용 환경 | 별도 VM 또는 부팅 USB로 구성, 테스트 환경 격리 운영 | ✅ FDA 침투 테스트 환경 구성 표준 |
| **Metasploit Framework** | https://www.metasploit.com/download | BSD-like | 익스플로잇 개발/실행, CVE 기반 공격 검증 | 알려진 CVE 익스플로잇 검증 (콘솔 PC OS, 네트워크 서비스 대상) | ✅ 독립 테스터가 수행 시 FDA 침투 테스트 요건 충족 |

### 4.7 기타 보조 도구

| 도구 | URL | 라이선스 | 용도 | HnVue 적용 방법 |
|------|-----|----------|------|-----------------|
| **CIS-CAT Lite** | https://www.cisecurity.org/cybersecurity-tools/cis-cat-pro/cis-catlite | Free | OS 보안 설정 벤치마크 (CIS Benchmarks) | 콘솔 PC OS (Windows 10/11) CIS Benchmark 적합성 평가 |
| **Lynis** | https://cisofy.com/lynis | GPL-3.0 | Linux 보안 감사 | Linux 기반 콘솔 PC 사용 시 적용 |
| **git-secrets** | https://github.com/awslabs/git-secrets | Apache 2.0 | Git 커밋 시 시크릿 자동 탐지 | `git secrets --install`, CI pre-commit hook 통합 |

---

## 5. 단계별 자체 검증 파이프라인

### 전체 타임라인

```
개발 단계          검증 단계         릴리스 전         인허가 제출
    │                 │                │                  │
Phase 1 ──────── Phase 2 ──────── Phase 3 ──────── Phase 4
(SAST+SCA)    (취약점스캔+퍼징)  (침투테스트)     (Retest+문서화)
  CI/CD           격리환경           독립인력          잔여위험
  자동화            수동+자동          수행              수용
```

---

### Phase 1: SAST + SCA (CI/CD 자동화, 비용 무료)

**목적**: 코드 수준 보안 취약점 및 종속성 CVE 조기 발견  
**수행 주체**: 개발팀 (CI/CD 파이프라인)  
**FDA/MDR 대응**: SPDF 요건, SBOM 생성

#### 1-1. GitHub Actions CI/CD 통합 예시 (`.github/workflows/security.yml`)

```yaml
name: Security Scan

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  sast-sca:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      # SAST: Security Code Scan
      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore HnVue.sln

      - name: Build with Security Code Scan
        run: dotnet build HnVue.sln /p:SecurityCodeScanEnabled=true
        # SecurityCodeScan은 NuGet 패키지로 빌드에 통합됨

      # SCA: OWASP Dependency-Check
      - name: Run OWASP Dependency-Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'HnVue-Console'
          path: '.'
          format: 'HTML,JSON,SARIF'
          args: >
            --enableRetired
            --nvdApiKey ${{ secrets.NVD_API_KEY }}

      - name: Upload Dependency-Check Report
        uses: actions/upload-artifact@v4
        with:
          name: dependency-check-report
          path: reports/

      # SBOM 생성: Syft
      - name: Generate SBOM (CycloneDX)
        uses: anchore/sbom-action@v0
        with:
          path: .
          format: cyclonedx-json
          output-file: sbom-cyclonedx.json

      # 취약점 매칭: Grype
      - name: Scan SBOM with Grype
        uses: anchore/scan-action@v3
        with:
          sbom: sbom-cyclonedx.json
          fail-build: true
          severity-cutoff: high  # HIGH 이상 CVE 시 빌드 실패

      # Secret 스캔: Trivy
      - name: Trivy Secret Scan
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          scan-ref: '.'
          scanners: 'vuln,secret,misconfig'
          format: 'sarif'
          output: 'trivy-results.sarif'
```

#### 1-2. SecurityCodeScan NuGet 패키지 추가

```xml
<!-- HnVue.csproj에 추가 -->
<ItemGroup>
  <PackageReference Include="SecurityCodeScan.VS2019" Version="5.6.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

#### 1-3. SBOM 생성 (로컬 실행)

```bash
# Syft 설치 (Windows)
winget install Anchore.Syft

# CycloneDX 형식 SBOM 생성 (FDA machine-readable SBOM 요건 충족)
syft dir:. -o cyclonedx-json > artifacts/sbom-hnvue-v1.0-cyclonedx.json
syft dir:. -o spdx-json > artifacts/sbom-hnvue-v1.0-spdx.json

# Grype로 CVE 스캔
grype sbom:./artifacts/sbom-hnvue-v1.0-cyclonedx.json --output table

# 결과 저장
grype sbom:./artifacts/sbom-hnvue-v1.0-cyclonedx.json --output json > artifacts/grype-cve-report.json
```

#### 1-4. Phase 1 산출물

- `artifacts/sbom-hnvue-v1.0-cyclonedx.json` — FDA §524B(b)(3) SBOM 제출용
- `artifacts/grype-cve-report.json` — CVE 목록 및 심각도
- `reports/dependency-check-report.html` — OWASP DC 리포트
- `reports/sast-findings.sarif` — SAST 발견사항 (SARIF 형식)

---

### Phase 2: 취약점 스캐닝 + DICOM 퍼징 (격리 환경, 비용 무료)

**목적**: 네트워크 공격 표면 및 DICOM 프로토콜 취약점 발견  
**수행 주체**: 내부 QA팀 또는 보안 담당자 (개발 참여 가능)  
**전제 조건**: 격리된 테스트 네트워크 환경 (프로덕션과 분리)

#### 2-1. 테스트 환경 구성

```
[테스트 네트워크 VLAN]
│
├── HnX-R1 Console PC (HnVue 설치)  ←── 스캔 대상
│     IP: 192.168.100.10
│
├── Orthanc DICOM 서버 (PACS 시뮬레이터)    ←── 시뮬레이션 PACS
│     IP: 192.168.100.20
│     Docker: docker run -p 4242:4242 -p 8042:8042 jodogne/orthanc
│
└── Kali Linux 공격 VM                      ←── 테스트 실행 머신
      IP: 192.168.100.50
```

#### 2-2. Nmap 포트/서비스 스캔

```bash
# 기본 포트 스캔 + 서비스 버전 탐지
nmap -sV -sC -p 104,2762,4242,8042,1433,445 192.168.100.10 \
  -oN reports/nmap-hnvue-console.txt \
  -oX reports/nmap-hnvue-console.xml

# DICOM C-ECHO 테스트 (NSE 스크립트)
nmap --script dicom-ping -p 104 192.168.100.10

# SMB 취약점 스캔 (EternalBlue 등)
nmap --script smb-vuln* -p 445 192.168.100.10

# 결과 해석:
# 포트 104 (DICOM) — 개방 시 인증 없이 C-FIND/C-STORE 가능 여부 확인 필요
# 포트 2762 (DICOM TLS) — TLS 설정 확인
```

#### 2-3. OpenVAS 취약점 스캐닝

```bash
# Docker Compose로 Greenbone Community Edition 실행
cat > docker-compose-gvm.yml << 'EOF'
version: '3'
services:
  gvm:
    image: greenbone/gvm:latest
    ports:
      - "9390:9390"
      - "9392:9392"
    volumes:
      - gvm_data:/var/lib/gvm
volumes:
  gvm_data:
EOF

docker-compose -f docker-compose-gvm.yml up -d

# 브라우저에서 https://localhost:9392 접속
# admin / admin으로 로그인 후 스캔 대상 192.168.100.10 등록
# "Full and Fast" 스캔 정책 적용
# 결과 XML 리포트 다운로드 → reports/openvas-report.xml
```

#### 2-4. DICOM C-ECHO / C-FIND 비인증 접근 테스트

```bash
# DCMTK 설치 (Windows)
# https://dicom.offis.de/dcmtk/dcmtk_support.html.en

# C-ECHO (DICOM 연결 확인) — 인증 없이 응답하면 취약
echoscu 192.168.100.10 104 -aet KALI_SCU -aec HNVUE_SCP

# C-FIND (환자 목록 비인증 쿼리) — 성공하면 PHI 노출
findscu -W -k "PatientName=*" -k "PatientID=*" \
  192.168.100.10 104 -aet KALI_SCU -aec HNVUE_SCP

# 결과: 응답이 오면 비인증 DICOM 접근 가능 → 보안 취약점 기록
```

#### 2-5. Wireshark DICOM 평문 전송 탐지

```bash
# Wireshark CLI (tshark) — DICOM 트래픽 캡처
tshark -i eth0 -f "port 104" -w captures/dicom-traffic.pcap

# HnVue에서 DICOM C-STORE 전송 수행 후 캡처 분석
tshark -r captures/dicom-traffic.pcap -Y "dicom" \
  -T fields -e dicom.PatientName -e dicom.PatientID

# 결과: PatientName, PatientID가 평문으로 표시되면 TLS 미적용 → 취약점 기록
```

#### 2-6. TLS 설정 검증

```bash
# DICOM TLS 포트 설정 검증
./testssl.sh 192.168.100.10:2762

# 확인 항목:
# - TLS 1.0/1.1 비활성화 여부 (취약 프로토콜)
# - 약한 암호 스위트 (RC4, DES, 3DES 등) 비활성화
# - 인증서 유효성 (자체 서명 여부, 만료 여부)
# - HSTS 헤더 (웹 관리 콘솔 있는 경우)

# 결과 저장
./testssl.sh --json 192.168.100.10:2762 > reports/testssl-report.json
```

#### 2-7. Radamsa + DCMTK DICOM 퍼징 (IOActive 방법론 적용)

```python
#!/usr/bin/env python3
# dicom_fuzzer.py — IOActive DICOM 퍼징 스크립트 기반 커스텀 구현
# 참고: https://www.ioactive.com/penetration-testing-of-the-dicom-protocol-real-world-attacks/

import subprocess
import os
import time
import logging
import shutil

# 설정
TARGET_IP = "192.168.100.10"
TARGET_PORT = "104"
TARGET_AET = "HNVUE_SCP"
SOURCE_AET = "FUZZ_SCU"
SAMPLE_DICOM = "samples/reference_ct.dcm"  # 정상 DICOM 파일
OUTPUT_DIR = "fuzz_output"
ITERATIONS = 500  # 퍼징 반복 횟수
LOG_FILE = "reports/dicom_fuzz_results.log"

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s %(levelname)s %(message)s',
    handlers=[logging.FileHandler(LOG_FILE), logging.StreamHandler()]
)

os.makedirs(OUTPUT_DIR, exist_ok=True)

def fuzz_dicom(iteration):
    """Radamsa로 DICOM 파일 변이 생성 후 dcmsend로 전송"""
    fuzzed_file = f"{OUTPUT_DIR}/fuzzed_{iteration:04d}.dcm"
    
    try:
        # Radamsa로 DICOM 파일 변이
        subprocess.run(
            ["radamsa", "-n", "1", "-o", fuzzed_file, SAMPLE_DICOM],
            timeout=10, check=True, capture_output=True
        )
        
        if not os.path.exists(fuzzed_file) or os.path.getsize(fuzzed_file) == 0:
            return "SKIP"
        
        # dcmsend로 전송
        result = subprocess.run(
            ["dcmsend", TARGET_IP, TARGET_PORT,
             "-aet", SOURCE_AET, "-aec", TARGET_AET,
             "--scan-files", fuzzed_file],
            timeout=30, capture_output=True, text=True
        )
        
        if result.returncode == 0:
            logging.info(f"[{iteration:04d}] SENT OK")
            return "OK"
        else:
            logging.warning(f"[{iteration:04d}] REJECTED: {result.stderr[:100]}")
            return "REJECTED"
            
    except subprocess.TimeoutExpired:
        logging.error(f"[{iteration:04d}] TIMEOUT — possible hang/crash detected!")
        shutil.copy(fuzzed_file, f"reports/crash_{iteration:04d}.dcm")
        return "TIMEOUT"
    except Exception as e:
        logging.error(f"[{iteration:04d}] ERROR: {e}")
        return "ERROR"
    finally:
        # 퍼징 파일 정리 (크래시 파일 제외)
        if os.path.exists(fuzzed_file) and f"crash_{iteration:04d}" not in fuzzed_file:
            os.remove(fuzzed_file)

# 퍼징 실행
results = {"OK": 0, "REJECTED": 0, "TIMEOUT": 0, "ERROR": 0, "SKIP": 0}

logging.info(f"=== DICOM Fuzzing 시작: {TARGET_IP}:{TARGET_PORT} ===")
logging.info(f"=== 반복 횟수: {ITERATIONS} ===")

for i in range(ITERATIONS):
    status = fuzz_dicom(i)
    results[status] += 1
    
    # 100회마다 대상 시스템 응답 확인 (C-ECHO)
    if i % 100 == 0 and i > 0:
        echo_result = subprocess.run(
            ["echoscu", TARGET_IP, TARGET_PORT,
             "-aet", SOURCE_AET, "-aec", TARGET_AET],
            timeout=10, capture_output=True
        )
        if echo_result.returncode != 0:
            logging.critical(f"[{i}] C-ECHO 실패 — 대상 서비스 크래시 가능성!")
        else:
            logging.info(f"[{i}] C-ECHO 확인: 서비스 정상")
    
    time.sleep(0.1)  # 과부하 방지

# 결과 요약
logging.info("=== 퍼징 완료 ===")
logging.info(f"결과 요약: {results}")
logging.info(f"크래시 파일: {len([f for f in os.listdir('reports') if 'crash_' in f])}개")
```

```bash
# 실행
python3 dicom_fuzzer.py

# fo-dicom 파일 파싱 퍼징 (네트워크 전송 없이 파서 직접 퍼징)
# 악의적인 DICOM 파일을 HnVue에서 직접 열기 테스트
for f in fuzz_output/fuzzed_*.dcm; do
  radamsa sample.dcm > "$f"
  # HnVue에서 파일 열기 자동화 (UI 테스트 도구 활용)
done
```

#### 2-8. Phase 2 산출물

- `reports/nmap-hnvue-console.txt` — 포트 스캔 결과
- `reports/openvas-report.xml` — 취약점 스캔 결과
- `captures/dicom-traffic.pcap` — 네트워크 트래픽 캡처
- `reports/testssl-report.json` — TLS 설정 검증 결과
- `reports/dicom_fuzz_results.log` — DICOM 퍼징 결과 (크래시 여부)
- `reports/crash_XXXX.dcm` — 크래시 유발 퍼징 파일 (발견 시)

---

### Phase 3: 침투 테스트 (독립 인력 수행)

**목적**: 복합 공격 시나리오 검증, FDA 침투 테스트 독립성 요건 충족  
**수행 주체**: 개발에 참여하지 않은 자격 있는 내부 인력 또는 소규모 외부 업체  
**비용**: 내부 독립 인력 수행 시 무료, 외부 위탁 시 별도 견적 (⚠️ 업체별 상이, 공개 가격 없음)

#### 3-1. 침투 테스트 계획 문서 (Pentest Plan) 필수 항목

```markdown
# HnVue Console SW 침투 테스트 계획서

## 1. 테스터 정보
- 이름: [독립 테스터명]
- 소속: [부서명] (개발팀 비소속 확인)
- 자격증: OSCP / CEH / GPEN (번호: XXXX)
- 개발 참여 여부: 없음 (서명 확인)

## 2. 테스트 범위 (Scope)
- 대상 시스템: HnVue Console SW v1.0 (HnVue)
- 대상 IP: 192.168.100.10
- 대상 포트: 104 (DICOM), 2762 (DICOM TLS), 1433 (SQLite N/A)
- 제외 범위: 외부 PACS 서버 (실제 병원 시스템)
- 테스트 환경: 격리된 테스트 네트워크

## 3. 방법론
- PTES (Penetration Testing Execution Standard)
- OWASP Testing Guide v4
- DICOM 특화: IOActive DICOM Penetration Testing 방법론

## 4. 테스트 기간
- 시작: YYYY-MM-DD
- 종료: YYYY-MM-DD (5~10 영업일)

## 5. 테스트 시나리오
[아래 §3-2 참조]
```

#### 3-2. HnVue 특화 침투 테스트 시나리오

| ID | 시나리오 | 공격 벡터 | 도구 | 예상 영향 |
|----|----------|-----------|------|-----------|
| PT-001 | 비인증 DICOM 접근 | C-FIND, C-STORE, C-MOVE without AE Title 검증 | DCMTK, dvtk | PHI 유출, 무단 이미지 전송 |
| PT-002 | DICOM 악의적 파일 처리 | 조작된 DICOM 파일 C-STORE | Radamsa + dcmsend | 서비스 크래시, 원격 코드 실행 |
| PT-003 | SQLite 인젝션 | DICOM 태그를 통한 SQLite 쿼리 인젝션 | 수동 + sqlmap | DB 조작, 환자 데이터 변조 |
| PT-004 | 네트워크 스니핑 | DICOM 평문 전송 PHI 가로채기 | Wireshark | HIPAA/개인정보보호법 위반 |
| PT-005 | AE Title 스푸핑 | 허가되지 않은 AE Title로 PACS 연결 | DCMTK | 무단 이미지 수신 |
| PT-006 | 권한 상승 | Windows 로컬 권한 상승 | Metasploit, WinPEAS | OS 관리자 권한 획득 |
| PT-007 | 구성 파일 노출 | PACS 자격증명 평문 저장 확인 | 파일 시스템 탐색 | 자격증명 유출 |
| PT-008 | 서비스 거부 (DoS) | DICOM C-STORE 대용량 연속 전송 | Python 스크립트 | 서비스 중단, 환자 진료 차질 |
| PT-009 | 업데이트 메커니즘 변조 | 소프트웨어 업데이트 무결성 미검증 | MitM + 파일 교체 | 악성코드 설치 |
| PT-010 | 세션 관리 취약점 | 로그인 세션 하이재킹 (UI 있는 경우) | Burp Suite | 권한 없는 기능 접근 |

#### 3-3. 크리티컬 시나리오 상세 수행 방법

**PT-003: SQLite 인젝션 테스트**
```bash
# fo-dicom이 DICOM 태그를 SQLite에 저장할 때 인젝션 가능한지 확인
# 예: PatientName 태그에 SQL 인젝션 페이로드 삽입

# 테스트용 DICOM 파일 생성 (Python + pydicom)
python3 << 'EOF'
import pydicom
from pydicom.dataset import Dataset, FileDataset
from pydicom.sequence import Sequence
import pydicom.uid

# 기본 DICOM 파일 복사
ds = pydicom.dcmread("samples/reference_ct.dcm")

# SQL 인젝션 페이로드를 PatientName에 삽입
ds.PatientName = "Robert'); DROP TABLE patients; --"
ds.PatientID = "' OR '1'='1"

ds.save_as("test_sqli_payload.dcm")
print("SQL 인젝션 테스트 DICOM 파일 생성 완료")
EOF

# HnVue에서 해당 파일 C-STORE로 전송
dcmsend 192.168.100.10 104 test_sqli_payload.dcm -aec HNVUE_SCP

# 결과: HnVue DB에서 쿼리 오류 또는 비정상 동작 확인
# SQLite DB 파일 직접 확인
sqlite3 /path/to/hnvue.db "SELECT * FROM patients WHERE name LIKE '%DROP TABLE%'"
```

**PT-007: PACS 자격증명 평문 저장 확인**
```bash
# 설정 파일, DB, 레지스트리에서 평문 자격증명 탐색
findstr /si "password\|passwd\|pacs\|aetitle\|secret" C:\HnVue\*.* C:\HnVue\*.xml C:\HnVue\*.json C:\HnVue\*.config

# SQLite DB에서 자격증명 저장 여부 확인
sqlite3 C:\HnVue\data\hnvue.db ".tables"
sqlite3 C:\HnVue\data\hnvue.db "SELECT * FROM settings WHERE key LIKE '%password%' OR key LIKE '%secret%'"

# 메모리 덤프에서 자격증명 탐색 (선택적)
procdump -ma HnVue.exe memory_dump.dmp
strings memory_dump.dmp | grep -i "password\|pacs"
```

#### 3-4. Phase 3 산출물

- `reports/pentest-plan-v1.0.pdf` — 테스트 계획서 (테스터 자격증 포함)
- `reports/pentest-report-v1.0.pdf` — 침투 테스트 최종 보고서
  - Executive Summary (위험 등급 요약)
  - 상세 발견사항 (취약점별 CVSS 점수, 재현 절차, 증거 스크린샷)
  - 완화 권고사항
  - 재검증(Retest) 결과 (수정 후)

---

### Phase 4: Retest + 잔여 위험 평가 및 문서화 (비용 무료)

**목적**: 발견된 취약점 수정 후 재검증, 인허가 제출용 최종 산출물 생성  
**수행 주체**: 내부 보안팀 + 개발팀

#### 4-1. 취약점 추적 및 재검증 프로세스

```
발견 → 분류 → 수정 → 재테스트 → 잔여위험 평가 → 수용/반려
  ↓        ↓        ↓        ↓           ↓
기록      CVSS    코드     통과/실패    ISO 14971
레지스터  점수    수정      리포트      위험평가
```

#### 4-2. 취약점 분류 기준 (CVSS v3.1 기반)

| CVSS 점수 | 심각도 | 대응 기한 | FDA/MDR 영향 |
|-----------|--------|-----------|--------------|
| 9.0~10.0 | Critical | 즉시 (출시 전 필수) | 인허가 차단 가능 |
| 7.0~8.9 | High | 출시 전 필수 | 심각한 우려사항 |
| 4.0~6.9 | Medium | 출시 전 권고 | 인허가 문서에 설명 필요 |
| 0.1~3.9 | Low | 다음 릴리스 | 잔여 위험 수용 가능 |

#### 4-3. 잔여 위험 수용 기준 (ISO 14971 연계)

각 취약점에 대해 다음 판단을 수행한다:

```
잔여 위험 = 완화 후 P(발생) × P(위해 도달) × 심각도

판단 기준 (ALARP 원칙):
- 잔여 위험 "As Low As Reasonably Practicable" 달성 여부
- 추가 완화 비용 > 위험 감소 편익 시 → 잔여 위험 수용 + 문서화
- 완화 불가능한 경우 → 사용자 교육·운영 제한 등 보완 조치
```

---

## 6. 외부 vs 자체 수행 의사결정 매트릭스

### 6.1 영역별 의사결정 가이드

| 검증 영역 | 자체 수행 | 외부 위탁 | 권장 방식 | 예상 비용 |
|-----------|-----------|-----------|-----------|-----------|
| **SAST** | 개발팀 주도 CI/CD 통합 | 외부 코드 리뷰 서비스 | ✅ **자체 (100%)** | 무료 |
| **SCA / SBOM** | Syft + Grype 자동화 | 외부 SCA 서비스 (Snyk 등) | ✅ **자체 (100%)** | 무료 |
| **네트워크 스캔** | Nmap + OpenVAS | Nessus Professional 등 | ✅ **자체 (100%)** | 무료 |
| **DICOM 퍼징** | DVTk + Radamsa | IOActive 등 전문 의료기기 보안 | ✅ **자체 (80%)** | 무료 |
| **기본 침투 테스트** | 내부 독립 인력 (자격증 보유) | 전문 보안 업체 | ⚠️ **하이브리드** | $0~$15K |
| **고급 익스플로잇 체인** | 제한적 (자격 인력 없는 경우) | 전문 보안 업체 필요 | ❌ **외부 권장** | $10K~$50K |
| **소셜 엔지니어링** | 내부 HR 협력 | 외부 레드팀 | 선택사항 | $5K~$20K |
| **물리 보안** | 내부 수행 가능 | 물리 보안 전문 업체 | ✅ **자체 가능** | 무료 |

### 6.2 비용-위험 의사결정 트리

```
침투 테스트 수행 방식 결정
│
├─ Q1: 개발에 참여하지 않은 보안 자격증 보유 내부 인력이 있는가?
│   │
│   ├─ YES → 자체 수행 (비용: 인력 시간만)
│   │         → FDA 제출 시 자격증 + 비참여 서약서 첨부
│   │
│   └─ NO
│       │
│       ├─ Q2: 프리랜서 OSCP 보유자 채용 가능한가?
│       │   │
│       │   ├─ YES → 단기 계약 ($3,000~$8,000)
│       │   │         → 의료기기 맥락 브리핑 제공 필수
│       │   │
│       │   └─ NO
│       │       │
│       │       └─ Q3: 예산이 $15,000 이상인가?
│       │           │
│       │           ├─ YES → 전문 의료기기 보안 업체 (ioXt, BishopFox 등)
│       │           │
│       │           └─ NO → Phase 1~2 자체 수행 후 최소 침투 테스트만
│       │                   소규모 프리랜서 위탁 ($5,000~$10,000)
│
└─ Q4: EU MDR Notified Body 또는 FDA가 독립성 추가 요구하는가?
    │
    ├─ YES → 외부 업체 필수 (비용 불가피)
    │
    └─ NO → 자체/하이브리드 수행으로 진행
```

### 6.3 자체 vs 외부 비용 비교표

| 방식 | 비용 | 소요 기간 | FDA 수용성 | EU MDR 수용성 | MFDS 수용성 |
|------|------|-----------|------------|---------------|-------------|
| **완전 자체** (독립 내부 인력, OSCP) | 내부 인건비만 | 2~4주 | ✅ 독립성+자격증 증빙 시 | ✅ | ✅ |
| **하이브리드** (자체 SAST/SCA + 외부 침투) | 별도 견적 (⚠️ 업체별 상이) | 3~6주 | ✅ 권장 방식 | ✅ | ✅ |
| **공인기관** (KTL 등 KOLAS 인정) | 별도 견적 (⚠️ 비공개) | 4~8주 | ✅ 가장 안전한 방식 | ✅ | ✅ |
| **외부 없이 자체만** (개발팀 직접) | 무료 | 1~2주 | ❌ FDA 거부 가능 | ❌ | ❌ |

> **권장**: 자체 SAST+SCA+네트워크스캔+DICOM 퍼징 수행 후, 침투 테스트만 공인기관 또는 독립 외부 전문가 위탁. 외부 비용은 별도 견적 필요 (⚠️ 공개 가격 없음).

---

## 7. 규제 제출 산출물 목록

### 7.1 FDA 510(k) eSTAR 사이버보안 문서 (~12개)

| # | 문서명 | 내용 | 생성 방법 |
|---|--------|------|-----------|
| 1 | Cybersecurity Management Plan | 취약점 모니터링·CVD·패치 계획 | 내부 작성 |
| 2 | Threat Model (STRIDE) | 자산·위협·완화 조치 | STRIDE 워크숍 + 문서화 |
| 3 | Software Bill of Materials (SBOM) | CycloneDX JSON, 전체 컴포넌트 | Syft 자동 생성 |
| 4 | Cybersecurity Risk Assessment | ISO 14971 + 사이버 위협 통합 | 내부 작성 (14971 프로세스) |
| 5 | SAST Report | SecurityCodeScan + SonarQube 결과 | CI/CD 자동 생성 |
| 6 | SCA / SBOM Vulnerability Report | Grype + OWASP DC CVE 목록 | 자동 생성 |
| 7 | Network Vulnerability Scan Report | OpenVAS + Nmap 결과 | 자동 생성 |
| 8 | Fuzz Testing Report | DICOM 퍼징 결과 (크래시 여부) | 내부 수행 후 작성 |
| 9 | Penetration Test Report | 독립 테스터 수행 최종 보고서 | 독립 인력 작성 |
| 10 | Vulnerability Remediation Tracker | 발견 취약점 수정 현황 | 내부 추적 시스템 |
| 11 | Residual Risk Assessment | 잔여 위험 수용 근거 | ISO 14971 연계 |
| 12 | Coordinated Vulnerability Disclosure (CVD) Policy | 보안 취약점 신고 채널·절차 | 내부 정책 문서 |

### 7.2 EU MDR Technical Documentation 사이버보안 파트

| 문서 | Technical Documentation 위치 | 내용 |
|------|------------------------------|------|
| GSPR Annex I §17 준수 선언 | General Safety/Performance 체크리스트 | 사이버보안 통제 목록 |
| 사이버보안 위험관리 요약 | Risk Management File | ISO 14971 위험관리 + 위협 모델 통합 |
| IEC 62304 사이버보안 활동 | Software Lifecycle 파일 | SAST/DAST/퍼징 V&V 추적 |
| SBOM (EU 형식) | Annex XIV Clinical/PMS | CycloneDX SBOM |
| 침투 테스트 리포트 요약 | V&V Summary | 발견사항·완화·잔여위험 요약 |
| PMS 사이버보안 계획 | Post-Market Surveillance Plan | 취약점 모니터링 프로세스 |

### 7.3 MFDS 제출 문서 (사이버보안 심사 자료)

| 문서 | MFDS 요구 기준 | 내용 |
|------|---------------|------|
| 사이버보안 위험관리 보고서 | 2025 개정 가이드라인 §3 | 위협 모델 + 위험평가 |
| 보안 시험 결과 Evidence | §4 (증거 기반) | SAST+DAST+침투 테스트 리포트 |
| SBOM | §5 | CycloneDX JSON |
| 추적성 매트릭스 | §6 | 보안요구사항 ↔ 테스트 ↔ 위험관리 |
| 취약점 관리 계획 | §7 | CVD 정책 + 패치 일정 |

---

## 8. 잔여 위험 평가 및 수용 기준

### 8.1 HnVue 특화 위협 시나리오별 위험 평가

| ID | 위협 시나리오 | 자산 | 발생 가능성 | 영향도 | 초기 위험 | 완화 조치 | 잔여 위험 |
|----|--------------|------|-------------|--------|-----------|-----------|-----------|
| T-001 | DICOM 비인증 접근으로 PHI 유출 | 환자 데이터 (DICOM) | 높음 | 높음 | **High** | AE Title 화이트리스트, TLS 적용 | Low |
| T-002 | 악의적 DICOM 파일로 서비스 크래시 | 서비스 가용성 | 중간 | 높음 | **High** | fo-dicom 입력 검증, 예외 처리 강화 | Low |
| T-003 | SQLite 인젝션으로 환자 데이터 변조 | 환자 메타데이터 DB | 낮음 | 높음 | **Medium** | Parameterized Query 강제 | Low |
| T-004 | PACS 자격증명 평문 저장 노출 | 시스템 접근 자격증명 | 중간 | 높음 | **High** | Windows DPAPI 암호화 저장 | Low |
| T-005 | 네트워크 스니핑으로 PHI 가로채기 | 전송 중 환자 데이터 | 중간 | 높음 | **High** | DICOM TLS (포트 2762) 강제 | Low |
| T-006 | NuGet 종속성 CVE 익스플로잇 | 애플리케이션 무결성 | 중간 | 중간 | **Medium** | 정기 SBOM 스캔 + 패치 | Low |
| T-007 | 로컬 권한 상승으로 OS 장악 | 시스템 무결성 | 낮음 | 높음 | **Medium** | OS 하드닝 (CIS Benchmark), 최소 권한 | Low |
| T-008 | DICOM DoS 공격으로 진료 차질 | 서비스 가용성 | 낮음 | 높음 | **Medium** | 연결 제한, 요청 속도 제한 | Low-Medium |

### 8.2 잔여 위험 수용 선언

위 완화 조치 적용 후 모든 위협의 잔여 위험이 "Low" 또는 "Low-Medium" 수준으로 감소한다. ISO 14971 ALARP 원칙에 따라:

- **Low** 잔여 위험: 수용 (추가 완화 불필요)
- **Low-Medium** 잔여 위험 (T-008 DoS): 병원 네트워크 방화벽 및 운영 절차로 보완. 운영 매뉴얼에 DICOM 포트 접근 제한 지침 포함

---

## 9. 취약점 관리 및 사후 처리

### 9.1 취약점 분류 및 대응 절차

```
외부 신고 / 내부 발견 / SBOM CVE 알림
            │
            ▼
     [취약점 접수 및 기록]
     Jira / GitHub Issues 티켓 생성
            │
            ▼
     [영향도 평가] (24시간 이내)
     CVSS v3.1 점수 산정
     HnVue 적용 가능성 분석
            │
     ┌──────┴──────┐
   High/Critical  Low/Medium
     │              │
     ▼              ▼
  [긴급 패치]   [일반 패치]
  30일 이내    다음 릴리스
     │              │
     └──────┬────────┘
            ▼
     [패치 개발 및 IEC 62304 유지보수]
     코드 수정 → SAST 재실행 → 회귀 테스트
            │
            ▼
     [패치 배포 및 고객 통보]
     업데이트 무결성 서명 확인
            │
            ▼
     [문서 업데이트]
     SBOM 업데이트, 위험관리 파일 갱신
```

### 9.2 Coordinated Vulnerability Disclosure (CVD) 정책

CVD 정책은 FDA §524B(b)(1) 필수 요건이다. 최소 내용:

```
## HnVue Console SW CVD 정책 요약

보안 취약점 신고 채널: security@[company].com
신고자 익명성 보장: 예
초기 응답 기한: 5 영업일 이내
최대 패치 기간: 90일 (고위험), 180일 (저위험)
공개 정책: 패치 배포 후 30일 이내 공개 가능
Safe Harbor 선언: 선의의 보안 연구 활동 법적 책임 면제
```

### 9.3 SBOM 지속적 모니터링

```bash
# 자동화 스크립트: 주간 CVE 모니터링 (cron/Task Scheduler)
# sbom_monitor.sh

#!/bin/bash
DATE=$(date +%Y%m%d)

# SBOM 재생성 (최신 패키지 반영)
syft dir:. -o cyclonedx-json > sbom-current.json

# CVE 스캔
grype sbom:sbom-current.json --output json > grype-results-${DATE}.json

# HIGH/CRITICAL CVE 필터링
python3 << 'EOF'
import json

with open(f"grype-results-{$(date +%Y%m%d)}.json") as f:
    data = json.load(f)

critical = [m for m in data.get('matches', []) 
            if m['vulnerability']['severity'] in ['Critical', 'High']]

if critical:
    print(f"[경고] {len(critical)}개 High/Critical CVE 발견!")
    for c in critical:
        print(f"  - {c['vulnerability']['id']}: {c['artifact']['name']} {c['artifact']['version']}")
else:
    print("CVE 없음 — 정상")
EOF
```

---

## 10. 체크리스트 (인허가 제출 전)

### 10.1 FDA 510(k) 제출 전 사이버보안 체크리스트

#### SBOM 및 종속성
- [ ] CycloneDX 또는 SPDX 형식 machine-readable SBOM 생성 완료
- [ ] fo-dicom 5.x, .NET 8 BCL, SQLite, Serilog 등 모든 오픈소스 컴포넌트 포함
- [ ] HIGH/CRITICAL CVE 전체 해결 또는 수용 근거 문서화
- [ ] SBOM 버전 관리 시스템에 등록

#### 위협 모델링
- [ ] STRIDE 또는 PASTA 방법론 기반 위협 모델 완성
- [ ] DICOM 인터페이스, PACS 연동, SQLite DB, 업데이트 메커니즘 포함
- [ ] 위협 모델과 완화 조치 간 추적성 확보
- [ ] ISO 14971 위험관리 파일과 연계

#### SAST
- [ ] SecurityCodeScan + SonarQube 스캔 완료 (현재 버전 기준)
- [ ] Critical/High SAST 발견사항 수정 완료
- [ ] SAST 리포트 날짜 및 도구 버전 명시

#### 취약점 스캐닝
- [ ] Nmap 포트 스캔 완료 — 불필요한 열린 포트 없음 확인
- [ ] OpenVAS 스캔 완료 — High/Critical 취약점 해결
- [ ] DICOM 비인증 접근 테스트 완료 (C-ECHO, C-FIND)

#### DICOM 퍼징
- [ ] Radamsa + dcmsend DICOM 파일 퍼징 완료 (최소 500회 반복)
- [ ] AFL++ fo-dicom 파서 퍼징 완료 (선택사항이나 권장)
- [ ] 크래시 발생 시 모두 분석 및 수정 완료

#### 침투 테스트
- [ ] 테스터 자격증 사본 보관 (OSCP/CEH/GPEN 등)
- [ ] 테스터 개발 비참여 확인서 서명
- [ ] 침투 테스트 계획서 (Scope, 방법론, 기간) 작성
- [ ] PT-001~PT-010 모든 시나리오 수행
- [ ] 최종 침투 테스트 리포트 (발견사항 + 완화 + 재검증) 완성

#### 문서화
- [ ] 12개 eSTAR 사이버보안 문서 전체 완성
- [ ] 취약점 추적 레지스터 최신화
- [ ] 잔여 위험 수용 선언서 작성
- [ ] CVD 정책 공개 URL 확보
- [ ] Cybersecurity Management Plan 작성

#### TLS / 암호화
- [ ] DICOM 통신 TLS 1.2+ 적용 확인
- [ ] testssl.sh 검증 통과
- [ ] PACS 자격증명 암호화 저장 확인
- [ ] 하드코딩 자격증명 없음 확인 (Trivy secret 스캔)

### 10.2 EU MDR / MFDS 추가 체크리스트

- [ ] GSPR Annex I §17 모든 항목 준수 확인
- [ ] 사이버보안 위험관리가 EU MDR Technical Documentation에 통합
- [ ] MFDS 추적성 매트릭스 (보안요구사항 ↔ 테스트 ↔ 위험관리) 완성
- [ ] IEC 62304 소프트웨어 유지보수 계획에 취약점 패치 절차 포함
- [ ] IEC 81001-5-1 보안 통제 항목 구현 및 증빙

---

## 11. 참고 규격 및 문서

### 규격 및 가이던스

| 문서 | 발행 기관 | 링크 |
|------|----------|------|
| Cybersecurity in Medical Devices: Quality System Considerations and Content of Premarket Submissions (2025 Final) | FDA | https://www.fda.gov/regulatory-information/search-fda-guidance-documents/cybersecurity-medical-devices-quality-system-considerations-and-content-premarket-submissions |
| Section 524B of the FD&C Act — Cyber Device Requirements | FDA | https://www.fda.gov/medical-devices/digital-health-center-excellence/cybersecurity-medical-devices-frequently-asked-questions-faqs |
| MDCG 2019-16: Guidance on Cybersecurity for Medical Devices | EU MDCG | https://health.ec.europa.eu/document/download/0d45d3b0-3c05-4e8b-88b7-62e41a33c15f_en |
| 의료기기 사이버보안 허가·심사 가이드라인 (2025 개정) | MFDS | https://www.mfds.go.kr/brd/m_1060/view.do?seq=15625 |
| IEC 62304:2015+AMD1:2015 — Medical Device Software Lifecycle | IEC | (유료 구매) |
| IEC 62443-4-1 — Security for Industrial Automation and Control Systems | IEC | (유료 구매) |
| IEC 81001-5-1 — Health Software and Health IT Security Controls | IEC | (유료 구매) |
| ISO 14971:2019 — Application of Risk Management to Medical Devices | ISO | (유료 구매) |
| IMDRF/CYBER WG/N60FINAL:2020 — Principles and Practices for Medical Device Cybersecurity | IMDRF | https://www.imdrf.org/documents/principles-and-practices-medical-device-cybersecurity |
| NIST SP 800-115 — Technical Guide to Information Security Testing | NIST | https://csrc.nist.gov/publications/detail/sp/800-115/final |
| PTES (Penetration Testing Execution Standard) | PTES | http://www.pentest-standard.org |
| DICOM Standard PS3.15 — Security and System Management Profiles | NEMA | https://dicom.nema.org/medical/dicom/current/output/chtml/part15/PS3.15.html |
| IOActive: Penetration Testing of the DICOM Protocol | IOActive | https://www.ioactive.com/penetration-testing-of-the-dicom-protocol-real-world-attacks/ |
| Innolitics: FDA Penetration Testing Independence | Innolitics | https://innolitics.com/articles/fda-penetration-testing/ |

### 도구 다운로드 요약

| 도구 | 빠른 설치 (Windows) |
|------|---------------------|
| Syft | `winget install Anchore.Syft` |
| Grype | `winget install Anchore.Grype` |
| Trivy | `winget install AquaSecurity.Trivy` |
| OSV-Scanner | `go install github.com/google/osv-scanner/cmd/osv-scanner@latest` |
| DCMTK | https://dicom.offis.de/dcmtk/dcmtk_support.html.en |
| DVTk | https://dvtk.org/download/ |
| Nmap | `winget install Insecure.Nmap` |
| Wireshark | `winget install WiresharkFoundation.Wireshark` |
| Kali Linux (VM) | https://www.kali.org/get-kali/#kali-virtual-machines |

---

*본 문서는 HnVue Console SW (HnVue) 사이버보안 자체 검증을 위한 내부 실무 가이드다. 인허가 제출 전 최신 FDA/EU MDR/MFDS 가이던스와 대조하여 업데이트할 것을 권장한다.*

*최종 업데이트: 2026-03-31*  
*다음 검토 예정: 인허가 제출 6개월 전 또는 주요 규제 변경 시*
