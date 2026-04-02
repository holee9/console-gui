# CYBERSEC-004 사이버보안 자체 사전평가 가이드

---

| 항목 | 내용 |
|------|------|
| **문서 ID** | CYBERSEC-004 |
| **버전** | v1.0 |
| **작성일** | 2026-03-31 |
| **프로젝트** | HnX-R1 HnVue |
| **SW 분류** | X-ray 촬영 콘솔 SW (WPF .NET 8, fo-dicom 5.x, SQLite) |
| **규격 등급** | IEC 62304 Class B |
| **작성자** | 내부 보안 담당 |
| **상태** | 초안 (Draft) |

---

## 목차

1. [개요](#1-개요)
2. [자체평가 영역 구분: "내부 가능" vs "외부 필수"](#2-자체평가-영역-구분-내부-가능-vs-외부-필수)
3. [Phase별 자체평가 실행 계획](#3-phase별-자체평가-실행-계획)
4. [무료 도구 전체 목록 (HnVue .NET 8 기준)](#4-무료-도구-전체-목록-hnvuetm-net-8-기준)
5. [자체평가 체크리스트 — 공인기관 의뢰 전 최종 점검](#5-자체평가-체크리스트--공인기관-의뢰-전-최종-점검)
6. [공인기관 시험 시 기대 효과](#6-공인기관-시험-시-기대-효과)
7. [참고 자료](#7-참고-자료)

---

## 1. 개요

### 1.1 목적

본 가이드는 HnX-R1 HnVue의 공인기관(KTL 등) 사이버보안 시험 의뢰 전에, 내부에서 **무료 또는 최소 비용**으로 자체 사전평가를 수행하여 시험 불합격 리스크를 최소화하는 것을 목적으로 한다.

**핵심 원칙**: 공인기관 시험 전에 자체적으로 발견하고 수정할 수 있는 것은 모두 미리 처리하여 재시험 리스크를 줄인다.

> **참고**: SAST/SCA/SBOM 취약점, 문서 누락, 설정 오류 등 자체적으로 발견 가능한 항목을 사전에 제거하면 공인기관 시험에서 불필요한 반려/재시험을 줄일 수 있다. 구체적인 통과율 향상 수치는 공인기관 사전 상담 시 확인 필요.

### 1.2 적용 범위

- **제품**: HnX-R1 HnVue (X-ray 촬영 콘솔 소프트웨어)
- **기술 스택**: WPF .NET 8, fo-dicom 5.x, SQLite
- **규격 등급**: IEC 62304 Class B
- **적용 규격**: IEC 81001-5-1:2021 (의료기기 소프트웨어 사이버보안)

### 1.3 IEC 81001-5-1 Clause별 자체 수행 가능성 판단

IEC 81001-5-1의 9개 Clause (Clause 4~9) 중 자체 수행 가능한 항목과 공인기관에 맡겨야 하는 항목을 다음과 같이 구분한다.

| Clause | 내용 | 자체 수행 가능 | 비고 |
|--------|------|:------------:|------|
| Clause 4 | 조직 및 관리 (보안 정책, 책임) | **O** | 내부 절차로 수립 가능 |
| Clause 5 | 보안 위험 관리 (위협 모델링, 위험평가) | **O** | OWASP Threat Dragon 등 활용 |
| Clause 6 | 보안 요구사항 (아키텍처, 설계) | **O** | 내부 설계 검토로 수행 |
| Clause 7 | 보안 구현 (SAST, SCA, 코드 리뷰) | **O** | 무료 도구 활용 |
| Clause 8 | 보안 검증 (동적 분석, Fuzzing) | **O** | 일부 복잡한 프로토콜 제외 |
| Clause 9 | 보안 출시 (SBOM, 잔여 위험) | **O** | CycloneDX 등 활용 |
| 침투 테스트 | 독립성+전문성 요구 | **△** | 내부 예비 수행 후 외부 필수 |

---

## 2. 자체평가 영역 구분: "내부 가능" vs "외부 필수"

| 영역 | 내부 자체 수행 | 외부 필수 | 비고 |
|------|:------------:|:--------:|------|
| SAST (정적 분석) | O | - | Semgrep, Roslyn Analyzers, PVS-Studio(무료 .NET) |
| SCA (SW 구성 분석) | O | - | OWASP Dependency-Check, Trivy, NuVet |
| SBOM 생성 | O | - | CycloneDX for .NET, Syft |
| SBOM 취약점 매칭 | O | - | Grype, OWASP Dependency-Track |
| 위협 모델링 (STRIDE) | O | - | OWASP Threat Dragon, Microsoft TMT, MITRE Playbook |
| 보안 요구사항 검증 | O | - | 자체 테스트 케이스 |
| Fuzz Testing | O | △ (복잡한 프로토콜) | AFL++, DICOM fuzzer |
| DAST (동적 분석) | O (웹 인터페이스) | - | OWASP ZAP (웹 있을 경우) |
| 취약점 스캐닝 | O | - | OpenVAS, Nmap |
| 침투 테스트 | △ (예비) | **O (필수)** | 독립성+전문성 요건 충족 필요 |
| 보안 위험 평가 | O | - | ISO 14971 연동 |
| 코드 리뷰 (보안) | O | - | 자체 + 자동화 도구 |
| 문서화/추적성 | O | - | 위험관리파일, 요구사항 매트릭스 |

> **주의**: 침투 테스트는 IEC 81001-5-1 및 규제기관 요구사항에 따라 **독립성과 전문성**을 갖춘 외부 기관이 수행해야 한다. 내부 예비 침투 테스트는 사전 발견 목적에 한하며, 공인 시험을 대체하지 않는다.

---

## 3. Phase별 자체평가 실행 계획

### Phase 1: 기반 구축 (Week 1-2)

**비용: 무료** | **기간: 2주**

#### 3.1.1 보안 요구사항 수립

IEC 81001-5-1 Clause 5.1을 기반으로 HnVue의 보안 요구사항을 수립한다.

**주요 작업**:
- 보안 요구사항 목록 작성 (기밀성, 무결성, 가용성 기준)
- 각 요구사항에 대한 시험 케이스 정의
- 요구사항 추적 매트릭스(RTM) 작성

#### 3.1.2 위협 모델링 수행 (STRIDE 기반)

**사용 도구**:
- **OWASP Threat Dragon** (무료): https://owasp.org/www-project-threat-dragon/
- **Microsoft Threat Modeling Tool** (무료): https://www.microsoft.com/en-us/securityengineering/sdl/threatmodeling
- **MITRE Medical Device Threat Modeling Playbook** (참조 문서): https://www.mitre.org/sites/default/files/2021-11/Playbook-for-Threat-Modeling-Medical-Devices.pdf

**HnVue 특화 위협 모델링 대상**:

| 영역 | 식별 대상 |
|------|-----------|
| DICOM 통신 경로 | fo-dicom을 통한 DICOM 메시지 송수신 경로 |
| PACS 연동 인터페이스 | 외부 PACS 서버와의 연결 trust boundary |
| 병원 네트워크 | 내부망/외부망 경계, 인증 메커니즘 |
| 로컬 SQLite DB | 환자 데이터 저장소 접근 통제 |
| USB/외부 미디어 | 물리적 접근 위협 |
| WPF UI 인터페이스 | 사용자 입력 검증, 세션 관리 |

**STRIDE 위협 카테고리별 식별**:

| 위협 유형 | 설명 | HnVue 적용 예시 |
|-----------|------|------------------------|
| **S**poofing (스푸핑) | 신원 위조 | DICOM SCU/SCP 인증 우회 |
| **T**ampering (변조) | 데이터 무결성 침해 | DICOM 이미지 전송 중 변조 |
| **R**epudiation (부인) | 행위 부인 | 촬영 로그 삭제/위변조 |
| **I**nformation Disclosure (정보 노출) | 기밀 정보 유출 | 환자 PHI 평문 전송 |
| **D**enial of Service (서비스 거부) | 가용성 침해 | DICOM 포트 DoS 공격 |
| **E**levation of Privilege (권한 상승) | 권한 탈취 | 일반 사용자 → 관리자 권한 획득 |

#### 3.1.3 SBOM 생성 파이프라인 구축

**사용 도구**:
- **CycloneDX for .NET** (무료): `dotnet tool install --global CycloneDX`
- **Syft** (무료): 파일시스템 + NuGet 자동 감지

**출력 형식**: CycloneDX JSON (FDA 권장 형식)

```bash
# CycloneDX for .NET 사용 예시
dotnet CycloneDX [project.csproj] -o ./sbom -j

# Syft 사용 예시
syft packages dir:./src --output cyclonedx-json > sbom.json
```

---

### Phase 2: 코드 보안 분석 (Week 3-4)

**비용: 무료** | **기간: 2주**

#### 3.2.1 SAST (정적 애플리케이션 보안 테스트) 실행

| 도구 | URL | 특징 |
|------|-----|------|
| **Semgrep** | https://semgrep.dev | C# 지원, 3000+ 커뮤니티 규칙 |
| **.NET Code Analysis (Roslyn Analyzers)** | https://docs.microsoft.com | Visual Studio 내장, 추가 설정 불필요 |
| **SonarQube CE** | https://www.sonarqube.org | C# 지원, 코드 품질 + 보안, CI/CD 연동 |
| **Security Code Scan** | https://security-code-scan.github.io | .NET 전용 보안 분석기 |
| **PVS-Studio** | https://pvs-studio.com | C# 무료 (오픈소스 프로젝트용) |

**우선 실행 순서**: Roslyn Analyzers (즉시) → Semgrep → SonarQube CE → Security Code Scan

**주요 점검 항목**:
- SQL 인젝션 취약점 (SQLite 쿼리)
- 하드코딩된 자격증명/비밀키
- 안전하지 않은 직렬화
- 버퍼 오버플로우 가능성
- 입력값 검증 누락
- 암호화 알고리즘 취약점 (MD5, SHA1 등)

#### 3.2.2 SCA (소프트웨어 구성 분석) 실행

| 도구 | URL | 특징 |
|------|-----|------|
| **OWASP Dependency-Check** | https://owasp.org/www-project-dependency-check/ | .NET NuGet 지원, NVD 기반 매칭 |
| **Trivy** | https://trivy.dev | NuGet 포함 30+ 패키지 에코시스템 |
| **NuVet** | https://github.com/wesleyscholl/NuVet | .NET 전용 NuGet 취약점 스캐너 |
| **dotnet list package --vulnerable** | https://docs.microsoft.com | 내장 명령, 즉시 사용 가능 |

```bash
# 내장 명령으로 즉시 취약 패키지 확인
dotnet list package --vulnerable

# Trivy로 NuGet 스캔
trivy fs --security-checks vuln ./
```

#### 3.2.3 SBOM 취약점 매칭

| 도구 | URL | 특징 |
|------|-----|------|
| **Grype** | https://github.com/anchore/grype | Syft SBOM 입력 → NVD/GHSA 매칭 |
| **OWASP Dependency-Track** | https://dependencytrack.org | CycloneDX SBOM 업로드 → 지속 모니터링 |
| **OSV-Scanner** | https://osv.dev | Google의 무료 취약점 스캐너 |

```bash
# Grype로 SBOM 취약점 매칭
grype sbom:./sbom.json

# OSV-Scanner 실행
osv-scanner --sbom sbom.json
```

---

### Phase 3: 동적 보안 테스트 (Week 5-6)

**비용: 무료** | **기간: 2주**

#### 3.3.1 취약점 스캐닝

**Nmap (네트워크 포트/서비스 스캔)**:
```bash
# 기본 포트 스캔
nmap -sV -sC [target_ip]

# 모든 포트 스캔
nmap -p- [target_ip]
```

**점검 사항**:
- 불필요하게 열려 있는 포트 식별
- DICOM 포트(104/2762) 외 불필요한 서비스 확인
- 서비스 버전 노출 여부 확인

**OpenVAS (GVM) (종합 취약점 스캔)**:
- GVM Community Edition 설치 후 전체 시스템 취약점 스캔
- Critical/High 취약점 식별 및 수정
- 스캔 결과 보고서 저장 (XML/PDF)

#### 3.3.2 DAST (동적 애플리케이션 보안 테스트)

**OWASP ZAP** (해당 시): 웹 인터페이스가 있는 경우에만 적용
- URL: https://www.zaproxy.org
- HnVue의 관리 웹 인터페이스(있는 경우) 스캔

#### 3.3.3 Fuzz Testing

| 도구 | 용도 | URL |
|------|------|-----|
| **AFL++** | 범용 퍼저, 바이너리/소스 코드 퍼징 | https://aflplus.plus |
| **CI Fuzz** | 의료기기 특화 퍼징 (OSS 버전) | https://code-intelligence.com |

**DICOM 퍼징 전략**:
- fo-dicom 통신에 대한 malformed DICOM 메시지 테스트
- 비정상적인 태그값, 잘못된 VR 형식, 과도한 크기의 데이터 등
- C-STORE, C-FIND, C-MOVE, C-ECHO 각 서비스 별 퍼징
- **KISA 무료 SW 개발보안 진단** 활용 가능 (https://www.kisa.or.kr)

**Fuzz Testing 중점 대상**:

| 인터페이스 | 퍼징 대상 | 기대 발견 취약점 |
|-----------|-----------|----------------|
| DICOM 수신 포트 | malformed DICOM 패킷 | 파싱 오류, 크래시, 버퍼 오버플로우 |
| 파일 입력 처리 | 비정상 이미지 파일 | 파일 파서 취약점 |
| UI 입력 필드 | 경계값, 특수문자, 긴 문자열 | 입력 검증 미흡 |
| SQLite 쿼리 | SQL 인젝션 페이로드 | 쿼리 삽입 취약점 |

#### 3.3.4 자체 침투 테스트 (예비)

> **주의**: 이 단계는 공인 침투 테스트를 **대체하지 않는다**. 공인기관 시험 전 내부 사전 발견 목적에만 한정한다.

**수행 주체**: 개발팀이 아닌 내부 인력(QA 또는 별도 담당자)

**사용 도구**:
- **Metasploit Framework**: exploit 검증 (https://www.metasploit.com)
- **Burp Suite CE**: 웹 프록시/인터셉터 (https://portswigger.net)

**점검 항목**:
- 인증 우회 시도
- 세션 관리 취약점
- 권한 상승 시도
- 네트워크 트래픽 도청 가능성

---

### Phase 4: 문서화 & 검증 (Week 7-8)

**비용: 무료** | **기간: 2주**

#### 3.4.1 MFDS 사이버보안 체크리스트 자체 점검

식품의약품안전처(MFDS) 사이버보안 가이드라인 **2024년 개정판** 기준으로 자체 점검을 수행한다.

**2024년 개정 내용**:
- 기존 15개 항목 → **35개 항목**으로 확대
- **6개 핵심 영역** 재편
- 7개 구분: 보안통신, 데이터 보호, 기기 무결성, 사용자 인증, SW 유지보수, 물리적 접근, 신뢰성 및 가용성

출처: [보안뉴스 MFDS 체크리스트 해설](https://m.boannews.com/html/detail.html?tab_type=1&idx=117564)

**7대 영역별 점검 포인트**:

| 영역 | 주요 점검 항목 |
|------|--------------|
| 보안통신 | TLS 적용, 암호화 알고리즘 강도, 인증서 유효성 |
| 데이터 보호 | 환자 PHI 암호화 (저장/전송), 개인정보 최소화 |
| 기기 무결성 | SW 서명, 부트 무결성, 코드 변조 감지 |
| 사용자 인증 | 강력한 비밀번호 정책, MFA, 세션 타임아웃 |
| SW 유지보수 | 안전한 업데이트 메커니즘, 패치 관리 |
| 물리적 접근 | USB 포트 제어, 물리적 잠금 메커니즘 |
| 신뢰성 및 가용성 | 장애 시 안전 상태(fail-safe) 전환, 백업 |

#### 3.4.2 IG-NB 체크리스트 자체 점검 (EU MDR 대비)

**OpenRegulatory 무료 템플릿** 활용:
- URL: https://openregulatory.com/document_templates/questionnaire-cybersecurity-for-medical-devices-technical-documentation

**2023년 German IG-NB 작성, 5개 섹션**:

| 섹션 | 내용 |
|------|------|
| 1. System Description | 소프트웨어 아키텍처, 인터페이스, 운영 환경 기술 |
| 2. Security Risk Management | 위협 모델링, 위험 수용 기준, 잔여 위험 |
| 3. Accompanying Documentation | 사용자 매뉴얼의 사이버보안 지침 |
| 4. Secure Implementation | SAST/SCA 결과, 보안 코딩 표준 준수 |
| 5. Secure Maintenance | 취약점 공개 정책(CVDP), 패치 프로세스 |

#### 3.4.3 FDA eSTAR 사이버보안 섹션 사전 작성

추적성 확보 순서:
1. **위협 모델** → 식별된 위협 목록
2. **보안 요구사항** → 각 위협에 대응하는 보안 통제
3. **시험 결과** → SAST/SCA/Fuzzing/침투 테스트 결과
4. **잔여 위험** → 수용된 잔여 위험 및 근거

#### 3.4.4 보안 시험 보고서 초안 작성

**보고서 구성 요소**:

| 섹션 | 내용 |
|------|------|
| 시험 범위 | 대상 시스템, 버전, 시험 환경 |
| 사용 도구 목록 | 각 도구별 버전, 설정, 실행 명령 |
| SAST 결과 | 발견 취약점 목록, 심각도, 수정 상태 |
| SCA 결과 | 취약 구성요소 목록, CVE 번호, 수정 버전 |
| Fuzzing 결과 | 크래시/오류 발생 여부, 수정 내용 |
| 취약점 스캔 결과 | OpenVAS/Nmap 결과, 열린 포트 목록 |
| 잔여 위험 | 미수정 항목 및 수용 근거 |
| IEC 81001-5-1 매핑 | 결과 → Clause 매핑 표 |

**Before/After 증빙 요건**:
- 발견된 취약점의 수정 완료 증빙 (코드 변경 전후 캡처)
- 재테스트 결과로 수정 확인
- 미수정 항목에 대한 잔여 위험 수용 근거 (ISO 14971 연동)

---

## 4. 무료 도구 전체 목록 (HnVue .NET 8 기준)

| 카테고리 | 도구명 | 비용 | .NET 지원 | 용도 | URL |
|---------|--------|:----:|:---------:|------|-----|
| SAST | Semgrep | 무료 | O (C#) | 패턴 기반 보안 분석, 3000+ 규칙 | https://semgrep.dev |
| SAST | SonarQube CE | 무료 | O (C#) | 코드 품질+보안, CI/CD 연동 | https://www.sonarqube.org |
| SAST | Roslyn Analyzers | 무료 | O (.NET 내장) | .NET 코드 분석 | https://docs.microsoft.com |
| SAST | Security Code Scan | 무료 | O (C#/VB.NET) | .NET 전용 보안 분석 | https://security-code-scan.github.io |
| SAST | CodeQL | 무료 (GitHub) | O (C#) | 심층 데이터 흐름 분석 | https://github.com/github/codeql |
| SCA | OWASP Dependency-Check | 무료 | O (NuGet) | NVD 기반 취약점 매칭 | https://owasp.org/www-project-dependency-check/ |
| SCA | Trivy | 무료 | O (NuGet) | 컨테이너+파일시스템 스캔 | https://trivy.dev |
| SCA | NuVet | 무료 | O (.NET 전용) | NuGet 패키지 취약점 | https://github.com/wesleyscholl/NuVet |
| SCA | dotnet list --vulnerable | 무료 | O (내장) | NuGet 취약 패키지 목록 | https://docs.microsoft.com |
| SBOM | CycloneDX for .NET | 무료 | O (.NET 전용) | CycloneDX SBOM 생성 | https://cyclonedx.org |
| SBOM | Syft | 무료 | O (NuGet 감지) | 범용 SBOM 생성 | https://github.com/anchore/syft |
| SBOM 검증 | Grype | 무료 | - | SBOM→취약점 매칭 | https://github.com/anchore/grype |
| SBOM 검증 | OWASP Dependency-Track | 무료 | - | SBOM 지속 모니터링 | https://dependencytrack.org |
| SBOM 검증 | OSV-Scanner | 무료 | - | Google 취약점 스캐너 | https://osv.dev |
| SBOM 검증 | Bomber | 무료 | - | SBOM→취약점 경량 스캐너 | https://github.com/devops-kung-fu/bomber |
| 위협 모델링 | OWASP Threat Dragon | 무료 | - | STRIDE/LINDDUN 위협 모델링 | https://owasp.org/www-project-threat-dragon/ |
| 위협 모델링 | Microsoft TMT | 무료 | - | DFD 기반 위협 모델링 | https://www.microsoft.com |
| 위협 모델링 | Aristiun | 무료 | - | STRIDE 자동 위협 식별 | https://threat-modeling.com |
| 위협 모델링 | IriusRisk CE | 무료 | - | 자동화 위협 모델링 | https://iriusrisk.com/community |
| 취약점 스캔 | Nmap | 무료 | - | 네트워크 포트/서비스 스캔 | https://nmap.org |
| 취약점 스캔 | OpenVAS (GVM) | 무료 | - | 종합 취약점 스캐너 | https://www.openvas.org |
| DAST | OWASP ZAP | 무료 | - | 웹 앱 동적 분석 | https://www.zaproxy.org |
| Fuzzing | AFL++ | 무료 | - | 범용 퍼저 | https://aflplus.plus |
| Fuzzing | CI Fuzz | 무료(OSS) | - | 의료기기 특화 퍼징 | https://code-intelligence.com |
| 침투 도구 | Metasploit Framework | 무료 | - | exploit 검증 | https://www.metasploit.com |
| 침투 도구 | Burp Suite CE | 무료 | - | 웹 프록시/인터셉터 | https://portswigger.net |
| 체크리스트 | OpenRegulatory IG-NB | 무료 | - | EU MDR 사이버보안 체크리스트 | https://openregulatory.com |
| 체크리스트 | Greenlight Guru | 무료 | - | 사이버보안 GAP 평가 체크리스트 | https://www.greenlight.guru |
| 참조 문서 | MITRE Medical Device TM | 무료 PDF | - | 의료기기 위협 모델링 플레이북 | https://www.mitre.org |
| 참조 문서 | OWASP Testing Guide v4 | 무료 | - | 보안 테스트 방법론 | https://owasp.org |
| 참조 문서 | NIST SP 800-115 | 무료 | - | 기술 보안 테스트 가이드 | https://www.nist.gov |

---

## 5. 자체평가 체크리스트 — 공인기관 의뢰 전 최종 점검

공인기관(KTL 등)에 사이버보안 시험을 의뢰하기 전, 아래 체크리스트를 모두 완료하였는지 확인한다.

> **기준**: 모든 항목이 체크된 상태에서 공인기관 시험을 의뢰하는 것을 원칙으로 한다.  
> **미완료 항목**: 각 미완료 항목에 대해 사유와 대응 계획을 별도 기재한다.

---

### 5.1 SBOM

- [ ] CycloneDX 또는 SPDX 형식 SBOM 생성 완료
- [ ] SBOM에 모든 NuGet 패키지, SOUP(기성 소프트웨어) 포함
- [ ] Grype/Dependency-Track으로 알려진 취약점 스캔 완료
- [ ] Critical/High 취약점 모두 수정 또는 수용 근거 문서화
- [ ] SBOM 인간 판독 가능 요약서 작성 완료

---

### 5.2 위협 모델링

- [ ] STRIDE 기반 위협 모델 작성 완료
- [ ] DICOM 통신 경로에 대한 trust boundary 식별
- [ ] 모든 외부 인터페이스에 대한 위협 식별 (네트워크, USB, 파일)
- [ ] 위협 → 보안 요구사항 → 시험 케이스 추적성 확보

---

### 5.3 SAST/SCA

- [ ] Semgrep 또는 동등 SAST 도구 실행, 결과 보고서 생성
- [ ] OWASP Dependency-Check 또는 Trivy SCA 실행 완료
- [ ] 하드코딩된 자격증명/비밀키 없음 확인
- [ ] Critical/High 발견 사항 모두 수정 완료
- [ ] 발견 사항 → IEC 81001-5-1 요구사항 매핑 완료

---

### 5.4 동적 테스트

- [ ] Nmap 포트 스캔: 불필요한 열린 포트 없음 확인
- [ ] OpenVAS 취약점 스캔 완료, Critical 없음 확인
- [ ] Fuzz 테스트: DICOM 인터페이스 malformed 입력 테스트 완료
- [ ] 인증/세션 관리 기능 수동 점검 완료

---

### 5.5 MFDS 체크리스트 (7대 영역)

- [ ] **보안통신**: 암호화 통신 적용 확인 (TLS 1.2 이상)
- [ ] **데이터 보호**: 환자 정보 암호화 확인 (저장 및 전송 시)
- [ ] **기기 무결성**: SW 무결성 검증 메커니즘 확인 (디지털 서명 등)
- [ ] **사용자 인증**: 인증/권한 메커니즘 확인 (최소 권한 원칙)
- [ ] **SW 유지보수**: 업데이트 메커니즘 보안 확인 (서명된 업데이트)
- [ ] **물리적 접근**: 물리적 보안 조치 확인 (USB 포트 통제 등)
- [ ] **신뢰성 및 가용성**: 장애 시 안전 상태(fail-safe) 전환 확인

---

### 5.6 문서 완성도

- [ ] 보안 위험관리 계획서 작성 완료
- [ ] 보안 위험평가 보고서 작성 완료
- [ ] 보안 시험 계획서/보고서 초안 작성 완료
- [ ] SAST/SCA/Fuzzing 결과 → 위험관리 파일 연동 완료
- [ ] IFU(사용설명서)에 사이버보안 관련 사용자 안내 포함
- [ ] FDA eSTAR 사이버보안 섹션 초안 완료
- [ ] CVDP (취약점 공개 정책, Coordinated Vulnerability Disclosure Policy) 초안 수립

---

## 6. 공인기관 시험 시 기대 효과

### 6.1 기대 효과 (⚠️ 아래는 일반적 기대이며, 구체적 수치는 공인기관 사전 상담으로 확인 필요)

| 구분 | 자체 사전평가 미수행 시 | 자체 사전평가 수행 시 |
|------|----------------------|-------------------|
| 재시험 리스크 | SAST/SCA/SBOM 취약점 잔존 시 반려 가능성 | 사전 제거로 리스크 감소 |
| 재시험 횟수 | 증가 가능성 | 감소 가능성 |
| 추가 비용 | 재시험 비용 발생 (금액은 기관별 상이) | 절감 가능 |
| 일정 지연 | 지연 위험 있음 | 지연 리스크 감소 |

### 6.2 정성적 효과

- **심사관 신뢰도 향상**: 체계적인 보안 관리 역량을 시험 결과물로 입증
- **내부 보안 역량 축적**: 반복 수행을 통한 조직 내 보안 전문성 강화
- **규제 대응 준비**: MFDS, FDA, EU MDR 등 다중 규제 동시 대응 기반 마련
- **제품 보안 품질 향상**: 출시 전 실질적인 보안 수준 개선
- **CVDP 운영 기반**: 출시 후 취약점 신고 및 대응 프로세스 사전 구축

### 6.3 비용 구조

| 항목 | 비용 | 비고 |
|------|------|------|
| 자체 사전평가 도구 | **무료** (오픈소스/무료 도구) | 팩트 |
| 자체 사전평가 인력 | 내부 인건비 | 팀 규모/역량에 따라 다름 |
| 공인기관 시험 비용 | 별도 견적 | KTL 등 기관에 직접 확인 필요 |
| 재시험 비용 | 기관별 상이 | 사전평가로 감소 가능성 있으나 보장은 아님 |

---

## 7. 참고 자료

| 번호 | 문서/출처 | URL |
|------|-----------|-----|
| 1 | IEC 81001-5-1:2021 원문 (Clause 4~9) | https://www.iec.ch/homepage |
| 2 | MITRE Medical Device Threat Modeling Playbook | https://www.mitre.org/sites/default/files/2021-11/Playbook-for-Threat-Modeling-Medical-Devices.pdf |
| 3 | IG-NB Cybersecurity Questionnaire (OpenRegulatory) | https://openregulatory.com/document_templates/questionnaire-cybersecurity-for-medical-devices-technical-documentation |
| 4 | Greenlight Guru Cybersecurity GAP Assessment Checklist | https://www.greenlight.guru/downloads/cybersecurity-gap-assessment-checklist |
| 5 | MFDS 사이버보안 가이드라인 2024 개정 (기존 15개→35개 항목) | https://www.mfds.go.kr |
| 6 | 보안뉴스 MFDS 체크리스트 해설 | https://m.boannews.com/html/detail.html?tab_type=1&idx=117564 |
| 7 | Johner Institute IEC 81001-5-1 해설 | https://blog.johner-institute.com/iec-62304-medical-software/iec-81001-5-1/ |
| 8 | IEC 81001-5-1 실무 가이드 (cm-dm.com) | https://blog.cm-dm.com/post/2024/02/23/IEC-81001-5-1-Right-Here-Right-Now |
| 9 | Sekurno EU MDR/IVDR Self-Assessment Guide | https://www.sekurno.com/post/eu-mdr-ivdr-cybersecurity-compliance-guide |
| 10 | AppSec Santa SAST/SCA 도구 비교 | https://appsecsanta.com |
| 11 | Innolitics SBOM Best Practices & FAQs | https://innolitics.com/articles/sbom-best-practices-faqs-examples/ |
| 12 | OWASP Threat Dragon | https://owasp.org/www-project-threat-dragon/ |
| 13 | OWASP Dependency-Check | https://owasp.org/www-project-dependency-check/ |
| 14 | CycloneDX 공식 사이트 | https://cyclonedx.org |
| 15 | Semgrep 공식 사이트 | https://semgrep.dev |

---

*본 문서는 HnX-R1 HnVue 프로젝트의 내부 사용을 위해 작성되었습니다.*  
*공인기관 시험 결과 및 규제 요구사항 변경 시 본 가이드를 업데이트하여야 합니다.*  
*버전 이력 관리는 프로젝트 문서 관리 시스템에 따라 수행합니다.*
