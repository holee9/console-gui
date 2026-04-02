# CYBERSEC-002: 침투 테스트 독립성 & 기술 전문성 충족/입증 가이드

| 항목 | 내용 |
|------|------|
| **문서 ID** | CYBERSEC-002 |
| **버전** | v1.0 |
| **작성일** | 2026-03-31 |
| **프로젝트** | HnX-R1 HnVue |
| **목적** | FDA 침투 테스트 2대 요건(독립성 + 기술 전문성) 충족을 위한 전수 조사 결과 |

---

## 1. FDA 원문 근거 (팩트 기반)

### 1.1 FDA 가이던스 원문 (2026.02.03 최신판, Section V.C)

> "Device manufacturers should indicate in the test reports **by whom the testing was performed (e.g., independent internal testers, external testers)** and **what level of independence** those responsible for testing devices have from the developers responsible for designing devices. **In some cases, it may be necessary to use third parties** to ensure an appropriate level of independence between the two groups, such that vulnerabilities or other issues revealed during testing are appropriately addressed."

> "Penetration test reports should be provided and include the following elements:
> - **Independence** and technical expertise of testers;
> - Scope of testing;
> - Duration of testing;
> - Testing methods employed; and
> - Test results, findings, and observations."

**출처:** [FDA Cybersecurity in Medical Devices: QMS Considerations and Content of Premarket Submissions](https://www.fda.gov/media/119933/download), Section V.C. Cybersecurity Testing

### 1.2 핵심 요건 정리

| 요건 | FDA 원문 표현 | 의미 |
|------|-------------|------|
| **독립성 (Independence)** | "independent internal testers, external testers", "level of independence from developers" | 개발팀과 분리된 사람이 테스트해야 함 |
| **기술 전문성 (Technical Expertise)** | "Independence and technical expertise of testers" | 테스터가 충분한 보안 기술 역량을 갖추어야 함 |

**FDA가 명시하지 않은 것:**
- 특정 자격증(CEH, OSCP 등) 요구 없음
- 제3자(외부 업체) 의무 사항 아님 — "in some cases" 조건부
- 구체적인 교육 시간이나 경력 년수 기준 없음

---

## 2. 요건 1: 독립성(Independence) 충족 방법 — 전수 목록

### 2.1 국제 표준별 독립성 등급 체계

**IEC 62443-4-1 (산업 보안 표준, SVV-5)** — 가장 구체적인 독립성 정의:

| 독립성 등급 | 정의 | 적용 대상 |
|-----------|------|----------|
| **Independent Person** | 테스터가 해당 제품의 개발자가 아니어야 함 | SVV-1 (보안요건), SVV-2 (위협완화) |
| **Independent Department** | 테스터가 개발자와 같은 1차 관리자(팀장) 아래 보고하지 않아야 함. QA부서 인력도 가능 | SVV-3 (취약점 테스트) |
| **Independent Organization** | 테스터가 개발자와 다른 조직(별도 법인, 사업부, 또는 다른 임원 산하 부서)에 속해야 함 | SVV-4 (침투 테스트) |

**출처:** [CertX — IEC 62443-4-1 Penetration Tests](https://certx.com/cybersecurity/how-to-implement-cyber-security-acc-to-iec-62443-ep-4-penetrations-tests/)

> 참고: IEC 62443-4-1은 침투 테스트(SVV-4)에 **Independent Organization** 수준을 요구합니다. 이는 FDA보다 엄격한 기준이며, FDA는 "independent internal testers"도 허용하므로 Independent Person~Department 수준도 가능합니다.

**IEC 81001-5-1 (의료기기 사이버보안 표준):**
- "The need to perform pen tests with an independent organization"
- "Security Advisor(s) shall have appropriate evidence of qualification"
- EU MDR 하에서 IEC 81001-5-1은 2028년 harmonization 예정이나, Notified Body들이 이미 적용 중

**출처:** [IEC 81001-5-1 Right Here Right Now](https://blog.cm-dm.com/post/2024/02/23/IEC-81001-5-1-Right-Here-Right-Now)

**PCI DSS (참조 표준):**
- "Qualified internal resources or a qualified third party may perform the penetration test as long as they are **organizationally independent**. This means the penetration tester must be organizationally separate from the management of the target systems."

**출처:** [PCI Security Standards Council — Penetration Testing Guidance](https://listings.pcisecuritystandards.org/documents/Penetration-Testing-Guidance-v1_1.pdf)

### 2.2 독립성 충족 방법 — 전체 옵션 목록

#### 옵션 A: 외부 제3자 위탁 (가장 강한 독립성)

| 방법 | 설명 | 비용 범위 | 독립성 강도 |
|------|------|----------|-----------|
| **A1. 전문 침투 테스트 업체** | 의료기기 전문 보안 회사 | $5,000~$50,000+ | ★★★★★ |
| **A2. CREST 인증 업체** | CREST 인증 보안 컨설팅 (UK/EU 기반) | $8,000~$30,000 | ★★★★★ |
| **A3. UL Solutions CAP** | UL 사이버보안 인증 프로그램 | $10,000~$40,000 | ★★★★★ |
| **A4. IEEE 2621 인증 랩** | IEEE 의료기기 사이버보안 인증 프로그램 | 견적 필요 | ★★★★★ |
| **A5. 크라우드소싱 플랫폼** | Bugcrowd, Synack 등 | $5,000~$25,000 | ★★★★★ |
| **A6. 프리랜서 펜테스터** | OSCP/CEH 보유 프리랜서 | $2,000~$8,000 | ★★★★☆ |

**의료기기 전문 침투 테스트 업체 (확인된 업체):**

| 업체명 | 지역 | 특징 |
|--------|------|------|
| [Blue Goat Cyber](https://bluegoatcyber.com) | 미국 | 의료기기 침투 테스트 전문, 10년+ 경험 |
| [Sekurno](https://www.sekurno.com) | 유럽 | FDA-aligned 침투 테스트 서비스 |
| [StealthNet AI](https://www.stealthnet.ai) | 미국 | SaMD/의료기기 전문 |
| [Periculo](https://www.periculo.co.uk) | 영국 | CREST 인증, IEC TR 60601-4-5 aligned |
| [UL Solutions](https://www.ul.com) | 글로벌 | Medical CAP 프로그램 |
| [Cyberintelsys](https://cyberintelsys.com) | 유럽 | CREST 인증, IEC 60601 aligned |
| [SolaSec](https://www.solasec.io) | 미국 | Class II/III 의료기기 전문 |
| [TÜV SÜD](https://blog.naver.com/tuv-sud) | 독일/글로벌 | ISO/IEC 17025 인증 시험소 |
| [Johner Institute](https://blog.johner-institute.com) | 독일 | 의료기기 규제 + 침투 테스트 컨설팅 |

#### 옵션 B: 내부 독립 인력 (비용 절감)

| 방법 | 설명 | 비용 | 독립성 강도 |
|------|------|------|-----------|
| **B1. 타 부서 보안 담당자** | 개발팀이 아닌 QA/IT/보안부서 인력 | 인건비만 | ★★★★☆ |
| **B2. 타 프로젝트 개발자** | 해당 제품 개발에 참여하지 않은 개발자 | 인건비만 | ★★★☆☆ |
| **B3. 신규 채용 보안 인력** | 보안 전문가 채용 | 연봉 | ★★★★☆ |

> **주의:** [Innolitics](https://innolitics.com/articles/medical-device-cybersecurity-best-practices-faqs-and-examples/) 실무 경험에 따르면, "FDA will almost certainly not allow an engineer who helped develop the device do the testing."

**B1 방법의 FDA 수용 조건 (Innolitics 실무 확인):**
1. 해당 제품 개발에 관여하지 않은 인력
2. 침투 테스트 자격증/인증 보유
3. 리포트에 독립성과 전문성을 명확히 기재

#### 옵션 C: 하이브리드 (권장)

| 방법 | 설명 | 비용 | 독립성 강도 |
|------|------|------|-----------|
| **C1. 내부 SAST/SCA + 외부 침투 테스트** | 정적 분석·SCA는 내부, 침투 테스트만 외부 | 외부 $5K~$15K | ★★★★★ |
| **C2. 내부 테스트 + 외부 검증** | 내부에서 1차 테스트 후 외부에서 독립 검증 | 외부 $3K~$8K | ★★★★★ |
| **C3. 크라우드소싱 + 내부 관리** | Bugcrowd/Synack에서 테스트, 내부에서 관리 | $5K~$15K | ★★★★★ |

#### 옵션 D: 학술/정부 연계 (최저 비용)

| 방법 | 설명 | 비용 | 독립성 강도 |
|------|------|------|-----------|
| **D1. 대학 보안 연구실 연계** | 대학 정보보안 연구실과 MOU | 무료~$2,000 | ★★★★☆ |
| **D2. CISA 무료 서비스** | CISA Cyber Hygiene Services (미국) | 무료 | ★★★★★ |
| **D3. KISA 취약점 점검** | 한국인터넷진흥원 보안 점검 (한국) | 무료~저비용 | ★★★★★ |
| **D4. Bug Bounty 프로그램 운영** | 자체 VDP/Bug Bounty 개설 | 무료~보상금 | ★★★★☆ |

**CISA 무료 서비스 상세:**
- [CISA Cyber Hygiene Services](https://www.cisa.gov/cyber-hygiene-services): 취약점 스캐닝, 웹 애플리케이션 스캐닝, 피싱 시뮬레이션 무료 제공
- 대상: 미국 내 의료 기관 및 제조업체
- 제한: 침투 테스트(exploit) 자체는 미포함, 취약점 식별까지만

**출처:** [CISA and HHS Cybersecurity Toolkit](https://www.mayerbrown.com/en/insights/publications/2023/11/cisa-and-hhs-provide-cyber-toolkit-for-healthcare-organizations)

### 2.3 독립성 입증 문서화 방법

침투 테스트 리포트에 반드시 포함해야 할 독립성 증빙:

```
1. 테스터 프로필
   - 이름, 소속, 직위
   - 해당 제품 개발 참여 여부: "참여하지 않음" 명시

2. 조직적 분리 증빙
   - 조직도상 개발팀과의 관계 (보고 라인 분리 입증)
   - 외부 업체인 경우: 계약서, 회사 정보

3. 이해충돌 선언
   - "테스터는 테스트 대상 제품의 설계, 개발, 코딩에
     참여한 적이 없으며, 개발팀과 독립적인 보고 라인에 있음"

4. (해당 시) 자격 인증
   - 보유 인증서 사본 또는 인증 번호
```

---

## 3. 요건 2: 기술 전문성(Technical Expertise) 입증 방법 — 전수 목록

### 3.1 공인 자격증/인증 — 전체 목록

#### Tier 1: 입문 수준 (비용 $200~$500)

| 자격증 | 발급 기관 | 비용 | 소요 시간 | 특징 |
|--------|----------|------|----------|------|
| **eJPT v2** | INE (eLearnSecurity) | $249 | 25~40시간 | 가장 저렴한 실습형 인증 |
| **CompTIA PenTest+** | CompTIA | $392 (시험) | 40~60시간 | 벤더 중립, DoD 8140 인정 |
| **PJPT** | TCM Security | ~$300 | 30~40시간 | 실습 기반, 리포트 작성 포함 |
| **CPT** | Infosec Institute | $499 | 40시간 | 9개 도메인 커버 |

#### Tier 2: 중급 수준 (비용 $900~$2,000)

| 자격증 | 발급 기관 | 비용 | 소요 시간 | 특징 |
|--------|----------|------|----------|------|
| **CEH** | EC-Council | $950~$1,199 | 5일 부트캠프 | 가장 널리 인정, 23만+ 보유자 |
| **GPEN** | GIAC/SANS | $999 (시험) + ~$7,500 (교육) | 5~6일 | ANSI 인증, DoD 인정, 가장 권위 |
| **PNPT** | TCM Security | ~$400 | 30~50시간 | 실무 중심, AD 환경 포함 |
| **CPENT** | EC-Council | $999 + 교육 | 40시간 | 고급 실습형 |

#### Tier 3: 고급/전문가 수준 (비용 $1,500~$3,000+)

| 자격증 | 발급 기관 | 비용 | 소요 시간 | 특징 |
|--------|----------|------|----------|------|
| **OSCP+** | OffSec | $1,499~$2,499 | 2~3개월 | 24시간 실기 시험, 업계 골드 스탠다드 |
| **GXPN** | GIAC/SANS | $999 + 교육 | 6일 | 고급 exploit, 연봉 $120K |
| **OSWE** | OffSec | $1,499~$2,499 | 2~3개월 | 웹 애플리케이션 전문 |
| **OSCE3** | OffSec | $2,749 | 3개월+ | 최상위 OffSec 인증 |
| **LPT Master** | EC-Council | $500 + 교육 | 2~3개월 | 24시간 실기, 90% 합격 기준 |

#### 의료기기/산업 특화 인증

| 자격증 | 발급 기관 | 비용 | 특징 |
|--------|----------|------|------|
| **ISA/IEC 62443 Cybersecurity Certificate** | ISA | 과정별 상이 | 4단계 인증 체계, 산업 보안 전문 |
| **IEEE 2621 관련 역량** | IEEE | - | FDA 인정 합의표준 (Recognized Consensus Standard) |
| **UL 2900-2-1 관련 역량** | UL Solutions | - | 의료기기 사이버보안 인증 |
| **CREST 인증** | CREST | £475~£650 (시험) | UK/EU 기준, 의료기기 업체에서 선호 |
| **Medical Device Networking & Cybersecurity Certificate** | St. Petersburg College | 23학점 | 의료기기 네트워킹 + 사이버보안 전문 |

**출처:** [Dumpsgate Best Penetration Tester Certifications 2026](https://dumpsgate.com/best-penetration-tester-certifications/), [Infosec Institute Top 10](https://www.infosecinstitute.com/resources/professional-development/top-5-penetration-testing-certifications-security-professionals/), [Programs.com](https://programs.com/certs/penetration-testing/), [ISA Certificate Program](https://www.isa.org/certification/certificate-programs/isa-iec-62443-cybersecurity-certificate-program)

### 3.2 무료/저비용 교육 경로 — 전수 목록

#### 완전 무료

| 리소스 | 제공자 | 내용 | URL |
|--------|--------|------|-----|
| **Practical Ethical Hacking (15시간)** | TCM Security / freeCodeCamp | 침투 테스트 전 과정 | [YouTube](https://www.freecodecamp.org/news/full-penetration-testing-course/) |
| **TCM Security Free Tier** | TCM Security | 4개 무료 과정 | [tcm-sec.com](https://academy.tcm-sec.com/) |
| **Introduction to Penetration Testing** | Security Blue Team | 입문 과정 | [securityblue.team](https://www.securityblue.team/courses/introduction-to-penetration-testing) |
| **SEED Security Labs** | Syracuse University | 보안 실습 랩 | [seedsecuritylabs.org](https://seedsecuritylabs.org/) |
| **API Sec University** | APISec | API 보안 3개 과정 | [apisecuniversity.com](https://www.apisecuniversity.com/) |
| **Linux Foundation 69개 과정** | Linux Foundation | 다양한 보안 과정 | [training.linuxfoundation.org](https://training.linuxfoundation.org/) |
| **ENISA 교육 자료** | EU ENISA | 아티팩트 분석, 포렌식 | [enisa.europa.eu](https://www.enisa.europa.eu/) |
| **TryHackMe 무료 경로** | TryHackMe | 기초~중급 해킹 경로 | [tryhackme.com](https://tryhackme.com/) |
| **Hack The Box Academy** | Hack The Box | 모듈 기반 보안 교육 | [academy.hackthebox.com](https://academy.hackthebox.com/) |
| **PortSwigger Web Security Academy** | Burp Suite 제작사 | 웹 보안 전문 (무료) | [portswigger.net](https://portswigger.net/web-security) |
| **OWASP 가이드** | OWASP | Testing Guide, Top 10 | [owasp.org](https://owasp.org/) |
| **NIST SP 800-115** | NIST | 기술 보안 테스트 가이드 | [nist.gov](https://csrc.nist.gov/pubs/sp/800/115/final) |
| **PTES (Penetration Testing Execution Standard)** | PTES.org | 침투 테스트 표준 방법론 | [ptes.org](http://www.pentest-standard.org/) |

#### 저비용 ($100~$500)

| 리소스 | 비용 | 내용 |
|--------|------|------|
| **TCM Security PEH + eJPT** | ~$300+$249 | 교육 + 인증까지 최저비용 경로 |
| **TryHackMe Premium** | $10/월 | 체계적 학습 경로 |
| **Hack The Box VIP** | $14/월 | 실전 머신 + 아카데미 |
| **OffSec Proving Grounds** | $19/월 | OSCP 준비용 실전 환경 |

### 3.3 자격증 없이 기술 전문성을 입증하는 방법

FDA는 특정 자격증을 요구하지 않으므로, 다음 방법으로도 "technical expertise"를 입증할 수 있습니다:

#### 방법 1: 포트폴리오 기반 입증

```
- CTF(Capture The Flag) 대회 참가 이력 및 성적
- Hack The Box / TryHackMe 프로필 (해결한 머신 수, 랭킹)
- Bug Bounty 제출 이력 (Bugcrowd, HackerOne)
- 보안 관련 기술 블로그 또는 발표 자료
- 오픈소스 보안 도구 기여 이력
```

#### 방법 2: 경력/교육 이력 기반 입증

```
- 이전 침투 테스트 수행 경력 및 리포트 샘플 (비밀 유지 처리)
- 보안 관련 교육 이수 증명서
- 보안 관련 학위 (정보보안, 컴퓨터공학 등)
- 사내 보안 교육 프로그램 이수 기록
```

#### 방법 3: 방법론 기반 입증

```
- OWASP Testing Guide v4 준수 선언
- PTES (Penetration Testing Execution Standard) 준수
- NIST SP 800-115 기반 테스트 수행
- 체계적 테스트 계획서 + 상세 결과 리포트
```

> **실무 경고 ([Innolitics](https://innolitics.com/articles/medical-device-cybersecurity-best-practices-faqs-and-examples/)):** "They also won't be content with someone who just picked up some pen-testing tools online and ran them against the software." — 도구만 돌린 것은 불충분하며, 체계적 방법론과 전문성이 입증되어야 합니다.

### 3.4 기술 전문성 입증 문서화 방법

침투 테스트 리포트에 반드시 포함할 전문성 증빙:

```
1. 테스터 자격 (Tester Qualifications)
   - 보유 인증/자격증 목록 (또는 동등 역량 증빙)
   - 침투 테스트 수행 경력 (년수, 건수)
   - 관련 교육 이수 이력

2. 테스트 방법론 (Methodology)
   - 사용한 프레임워크 (OWASP, PTES, NIST SP 800-115 등)
   - 테스트 접근 방식 (블랙박스/그레이박스/화이트박스)
   - 사용 도구 목록 및 버전

3. 테스트 범위 및 깊이 (Scope & Depth)
   - 테스트 대상 시스템/인터페이스 목록
   - 테스트 기간 (시작~종료)
   - 투입 인력 및 공수 (man-days)

4. 결과 품질 (Quality of Findings)
   - 재현 가능한 취약점 기술
   - 스크린샷/로그/패킷 캡처 증빙
   - CVSS 점수 및 위험도 평가
   - 의료기기 맥락에서의 임상적 영향 분석
```

---

## 4. 규제 기관별 요건 비교

| 요건 | FDA (미국) | EU MDR (유럽) | MFDS (한국) |
|------|----------|-------------|-------------|
| **침투 테스트 의무** | "권장" (사실상 필수) | IEC 81001-5-1 요구 | 가이드라인 요구 |
| **독립성 수준** | "independent internal testers" 허용 | "independent organization" 선호 | 명시적 기준 없음 |
| **제3자 필수** | 아님 ("in some cases") | Notified Body가 사실상 요구 | 명시적 요구 없음 |
| **자격 요건** | "technical expertise" (비특정) | "appropriate evidence of qualification" | 명시 없음 |
| **관련 표준** | FDA Guidance Sec V.C | IEC 81001-5-1, MDCG 2019-16 | MFDS 사이버보안 가이드라인 2025 |
| **기술문서 제출** | eSTAR 포함 | 기술문서 Annex II/III | 허가심사 시 제출 |

**규제별 핵심 차이:**

- **FDA**: 가장 유연. 내부 독립 인력도 명시적으로 허용. 단, 리포트에 독립성+전문성 기재 필수.
- **EU MDR**: 가장 엄격. IEC 81001-5-1이 "independent organization" 수준 요구. Notified Body가 외부 테스트를 사실상 기대.
- **MFDS**: 중간. 2025 개정 가이드라인에서 Vulnerability Assessment + Penetration Testing 요구하나, 수행자 독립성에 대한 구체적 기준은 미확립.

**출처:**
- [MFDS 사이버보안 가이드라인 2025](https://wisecompany.org/medical-device-cybersecurity-guideline-2025/)
- [MDCG 2019-16 EU MDR Cybersecurity](https://www.emergobyul.com/news/new-guidance-published-medical-device-and-ivd-cybersecurity-under-mdr-and-ivdr-europe)
- [Assured AB — EU Cybersecurity Requirements](https://www.assured.se/areas/medtech-security/cybersecurity-requirements-in-medtech)

---

## 5. HnX-R1 HnVue에 대한 권장 전략

### 5.1 제품 특성 분석

| 항목 | 내용 | 영향 |
|------|------|------|
| 제품 유형 | X-ray 촬영 콘솔 SW (WPF .NET 8) | 소프트웨어 의료기기 |
| 네트워크 | 병원 내부 네트워크, DICOM 통신 | Cyber device 해당 |
| IEC 62304 분류 | Class B | 중등도 위험 |
| FDA 경로 | 510(k) | 표준 사이버보안 문서 필요 |
| 대상 시장 | FDA + EU MDR + MFDS | 3개 규제 모두 충족 필요 |

### 5.2 권장 전략: 3-시장 동시 충족

**EU MDR이 가장 엄격하므로, EU 기준에 맞추면 FDA/MFDS는 자동 충족됩니다.**

#### 추천 방안: C1 (내부 SAST/SCA + 외부 침투 테스트)

**내부 수행 (무료/저비용):**

| 테스트 유형 | 도구 | 비용 |
|-----------|------|------|
| SAST (정적 분석) | Semgrep, .NET Security Guard, Roslyn Analyzers | 무료 |
| SCA (소프트웨어 구성 분석) | OWASP Dependency-Check, Trivy | 무료 |
| SBOM 생성 | CycloneDX, Syft | 무료 |
| Fuzz Testing | AFL++, custom DICOM fuzzer | 무료 |
| 취약점 스캐닝 | OpenVAS, Nmap | 무료 |
| DAST (동적 분석) | OWASP ZAP | 무료 |

**외부 위탁 (침투 테스트만):**

| 옵션 | 비용 | 근거 |
|------|------|------|
| 의료기기 전문 업체 | 별도 견적 | 업체별 상이, 직접 문의 필요 |
| CREST 인증 업체 | 별도 견적 | EU MDR 대응에 유리 |
| 프리랜서 (OSCP+) | ⚠️ 협의 필요 (크몽 기준 350만~550만원) | 크몽 게시 가격이며 실제는 범위/난이도에 따라 다름 |

### 5.3 내부 인력 역량 확보 경로 (자격증이 없는 경우)

**최소 비용 경로 (약 $550, 3~4개월):**

```
Step 1: 무료 교육 (4~6주)
├── TCM Security Free Tier 기초 과정
├── freeCodeCamp 15시간 침투 테스트 과정
├── PortSwigger Web Security Academy (웹 보안)
└── TryHackMe 무료 경로 (기초→중급)

Step 2: 실습 ($10~14/월, 4~8주)
├── TryHackMe Premium 또는 Hack The Box VIP
├── OWASP WebGoat (취약 웹앱 실습)
└── Metasploitable / DVWA (취약 시스템 실습)

Step 3: 인증 취득 ($249~$392, 1~2주)
├── 옵션 A: eJPT v2 ($249) — 가장 저렴
├── 옵션 B: CompTIA PenTest+ ($392) — 가장 범용
└── 옵션 C: PNPT ($400) — 가장 실무적

Step 4: 의료기기 특화 역량 (무료, 2~4주)
├── OWASP Medical Device 가이드 학습
├── IEC 81001-5-1 요구사항 숙지
├── DICOM/HL7 프로토콜 보안 이해
└── AAMI TIR57 보안 위험관리 프레임워크
```

**중급 경로 (약 $1,500~$2,500, 4~6개월):**

```
Step 1~2: 위와 동일

Step 3: CEH ($950~$1,199) 또는 OSCP ($1,499~$2,499)
└── CEH: 보편적 인정, FDA 리뷰어 인지도 높음
└── OSCP: 기술적으로 가장 강력, 업계 골드 스탠다드

Step 4: 의료기기 특화 (위와 동일)
```

---

## 6. 침투 테스트 리포트 템플릿 — FDA 제출용

```markdown
# Penetration Test Report
## [제품명] — HnVue v1.0

### 1. Executive Summary
- 테스트 기간: YYYY-MM-DD ~ YYYY-MM-DD
- 총 투입 공수: X man-days
- 발견 취약점: Critical X건, High X건, Medium X건, Low X건
- 최종 판정: [Pass/Conditional Pass/Fail]

### 2. Tester Independence & Qualifications
#### 2.1 Independence Declaration
- 테스터 성명: [이름]
- 소속: [조직명] (제품 개발 조직과 별도)
- 제품 개발 참여 여부: 없음
- 보고 라인: [개발팀과 독립된 보고 라인 설명]
- 이해충돌 선언: "본 테스터는 테스트 대상 제품의 설계, 개발,
  코딩에 참여한 적이 없으며, 개발팀과 조직적으로 독립되어 있음"

#### 2.2 Technical Expertise
- 보유 자격: [CEH/OSCP/GPEN/eJPT 등]
- 침투 테스트 경력: X년, Y건 수행
- 관련 교육: [교육 프로그램 목록]
- 추가 역량: [CTF 실적, Bug Bounty 이력 등]

### 3. Scope
- 테스트 대상: [HnVue WPF 클라이언트, DICOM 통신 모듈, ...]
- 테스트 환경: [테스트 장비/네트워크 구성]
- 테스트 접근 방식: [블랙박스/그레이박스/화이트박스]
- 제외 항목: [테스트하지 않은 항목 및 사유]

### 4. Methodology
- 기준 프레임워크: OWASP Testing Guide v4, NIST SP 800-115
- 의료기기 특화 기준: IEC TR 60601-4-5, IEC 81001-5-1
- 도구:
  - Nmap (네트워크 스캐닝)
  - OWASP ZAP (웹 애플리케이션)
  - Metasploit Framework (exploit 검증)
  - Burp Suite (프록시/인터셉터)
  - [추가 도구]

### 5. Findings
[각 취약점별]
- ID: PT-001
- 제목: [취약점명]
- CVSS v3.1 점수: X.X
- 심각도: [Critical/High/Medium/Low]
- 영향: [기밀성/무결성/가용성에 대한 영향]
- 임상적 영향: [환자 안전에 대한 영향 분석]
- 재현 절차: [단계별 재현 방법]
- 증빙: [스크린샷/로그/패킷 캡처]
- 권장 조치: [구체적 수정 방안]
- 조치 결과: [수정 완료/보류/위험 수용 + 사유]

### 6. Retest Results
[수정 후 재테스트 결과]

### 7. Residual Risk Assessment
- 잔여 위험 목록 및 수용 근거
- 위험관리 파일(ISO 14971) 연동 참조

### 8. Conclusion
[전체 평가 요약 및 권장사항]
```

---

## 7. 결론 및 의사결정 매트릭스

### 비용-효과-규제수용성 종합 평가

| 방안 | 비용 | FDA 수용성 | EU MDR 수용성 | MFDS 수용성 | 독립성 | 전문성 | 추천도 |
|------|------|----------|-------------|-------------|--------|--------|--------|
| A1. 외부 전문 업체 | $5K~$50K | ★★★★★ | ★★★★★ | ★★★★★ | ★★★★★ | ★★★★★ | **최고** |
| A6. 프리랜서 (OSCP+) | $2K~$8K | ★★★★☆ | ★★★★☆ | ★★★★☆ | ★★★★☆ | ★★★★☆ | **우수** |
| B1. 내부 타 부서 (자격 있음) | 인건비 | ★★★★☆ | ★★★☆☆ | ★★★★☆ | ★★★★☆ | ★★★★☆ | **양호** |
| B1. 내부 타 부서 (자격 없음) | 인건비 | ★★☆☆☆ | ★☆☆☆☆ | ★★★☆☆ | ★★★★☆ | ★★☆☆☆ | **위험** |
| C1. 내부 + 외부 침투만 | $5K~$15K | ★★★★★ | ★★★★★ | ★★★★★ | ★★★★★ | ★★★★★ | **최적** |
| D1. 대학 연구실 | $0~$2K | ★★★☆☆ | ★★★☆☆ | ★★★☆☆ | ★★★★☆ | ★★★☆☆ | **보조** |

### HnX-R1 프로젝트 최종 권장안

```
═══════════════════════════════════════════════════════
  권장: C1 방안 — 내부 SAST/SCA + 외부 침투 테스트
  비용: 별도 견적 필요 (⚠️ 업체별 상이, 공개 가격 없음)
  
  내부: SAST + SCA + SBOM + Fuzzing + DAST (무료 도구)
  외부: 침투 테스트만 전문 업체 위탁
  
  3개 시장(FDA/EU MDR/MFDS) 동시 충족
  
  선택지:
  ├── 예산 여유 → 의료기기 전문 업체 (Blue Goat, Sekurno 등)
  ├── 비용 절감 → OSCP+ 프리랜서
  └── EU 중시 → CREST 인증 업체 (Periculo 등)
═══════════════════════════════════════════════════════
```

---

## 부록: 참고 자료

### 1차 소스 (규제 원문)
1. [FDA Cybersecurity Guidance (2026.02.03)](https://www.fda.gov/media/119933/download)
2. [FDA Cybersecurity FAQ](https://www.fda.gov/medical-devices/digital-health-center-excellence/cybersecurity-medical-devices-frequently-asked-questions-faqs)
3. [MFDS 사이버보안 가이드라인 2025](https://wisecompany.org/medical-device-cybersecurity-guideline-2025/)
4. [MDCG 2019-16 EU MDR Cybersecurity](https://www.emergobyul.com/news/new-guidance-published-medical-device-and-ivd-cybersecurity-under-mdr-and-ivdr-europe)
5. [IEC 62443-4-1 SVV-5 Independence](https://certx.com/cybersecurity/how-to-implement-cyber-security-acc-to-iec-62443-ep-4-penetrations-tests/)

### 실무 소스 (FDA 제출 경험 기반)
6. [Innolitics — Medical Device Cybersecurity FAQ](https://innolitics.com/articles/medical-device-cybersecurity-best-practices-faqs-and-examples/)
7. [ICS — FDA 510(k) Q&A Webinar](https://www.ics.com/questions-answers-fdas-510k-requirements-webinar)
8. [Johner Institute — FDA Cybersecurity Guidance](https://blog.johner-institute.com/iec-62304-medical-software/fda-guidance-on-cybersecurity/)
9. [UL Solutions — Medical Device Penetration Testing](https://www.ul.com/resources/medical-device-penetration-testing-goes-beyond-cyber)
10. [Censinet — 510(k) Cybersecurity Testing Guide](https://censinet.com/perspectives/cybersecurity-testing-510k-submissions-guide)

### 정부/공공 자원
11. [CISA Cyber Hygiene Services](https://www.mayerbrown.com/en/insights/publications/2023/11/cisa-and-hhs-provide-cyber-toolkit-for-healthcare-organizations)
12. [IEEE 2621 Medical Device Cybersecurity Certification](https://standards.ieee.org/products-programs/icap/programs/medical-devices-cybersecurity/)
13. [ISA/IEC 62443 Certificate Program](https://www.isa.org/certification/certificate-programs/isa-iec-62443-cybersecurity-certificate-program)
