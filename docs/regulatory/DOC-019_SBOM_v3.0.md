# 소프트웨어 자재 명세서 (Software Bill of Materials, SBOM)
## HnVue Console SW

---

## 문서 메타데이터 (Document Metadata)

| 항목 | 내용 |
|------|------|
| **문서 ID** | SBOM-XRAY-GUI-001 |
| **문서명** | HnVue Console SW 소프트웨어 자재 명세서 |
| **버전** | v3.0 |
| **작성일** | 2026-04-13 |
| **작성자** | SW 개발팀, RA 팀 |
| **검토자** | SW Dev Lead |
| **승인자** | PM |
| **상태** | Approved |
| **기준 규격** | FDA Section 524B, NTIA SBOM Minimum Elements, CycloneDX 1.5, IEC 62304 §8 |

### 개정 이력 (Revision History)

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| v1.0 | 2026-03-18 | 최초 작성 — Phase 1 전체 구성요소 목록 | SW 개발팀 |
| v1.1 | 2026-04-08 | 개발 전용 Roslyn 분석기 3종 추가 (SBOM-043~045): StyleCop.Analyzers, Roslynator.Analyzers, SecurityCodeScan.VS2019; 구성요소 수 42 → 45 | QA팀 |
| v2.0 | 2026-04-11 | S04 R1 NuGet 업그레이드 반영 (Team A SPEC-INFRA-001): fo-dicom 5.1.3→5.2.5, EF Core 8.0.2→9.0.0, Microsoft.Data.Sqlite 8.0.2→9.0.0, Microsoft.Extensions.* 8.x→9.0.0, System.Text.Json 9.0.0 신규, SQLitePCLRaw.bundle_e_sqlcipher 2.1.8 신규 (SQLCipher PHI 암호화), BCrypt.Net-Next 4.0.3 신규, System.IdentityModel.Tokens.Jwt 7.3.1 신규, System.IO.Ports 8.0.0 신규, NetArchTest.Rules 1.3.2 신규, FlaUI 4.0.0 신규 (E2E 테스트) | RA팀 |
| v3.0 | 2026-04-13 | S06 R2 Detector SDK 추가 (Team B SPEC-DETECTOR-001): AbyzSdk 0.1.0.0 (자사 CsI FPD), AbyzSdk.Imaging 0.1.0.0 (자사 FPD 이미징), HME libxd2 2.0 (2G 무선 FPD, 146 exports), HME libxd 1.0 (1G FPD, 100 exports), HME CIB_Mgr 1.0 (CR/DR 노출 제어, 9 exports) | RA팀 |

---

## 목차 (Table of Contents)

1. 목적 및 범위
2. SBOM 정책
3. NTIA 최소 요소
4. 구성요소 목록
5. 의존성 그래프
6. 취약점 관리 프로세스
7. SOUP 관리(IEC 62304 §8)
8. SBOM 갱신 절차
9. 라이선스 호환성 분석
10. CycloneDX 형식 예시

---

## 1. 목적 및 범위 (Purpose and Scope)

### 1.1 목적 (Purpose)

본 문서는 HnVue Console SW에 포함된 모든 소프트웨어 구성요소의 자재 명세서 (SBOM)를 문서화한다.

FDA Section 524B에 따라 Cyber Device로 분류된 의료기기는 FDA 510(k) 제출 시 SBOM을 포함해야 하며, 본 문서는 다음을 제공한다:
1. **NTIA SBOM Minimum Elements** 준수 구성요소 목록
2. **알려진 취약점 (CVE) 현황** 및 관리 프로세스
3. **IEC 62304 §8 SOUP 관리** 요구사항 충족
4. **라이선스 호환성** 분석

### 1.2 범위 (Scope)

| 구분 | 내용 |
|------|------|
| **대상** | HnVue Console SW v1.0 Phase 1 (S04 R1 업그레이드 반영) |
| **포함** | OS, 런타임, 프레임워크, 라이브러리, 도구 |
| **형식** | CycloneDX 1.5 JSON (기계 판독용) + Markdown (사람 판독용) |

---

## 2. SBOM 정책 (SBOM Policy)

### 2.1 SBOM 생성 원칙

- CI/CD 파이프라인 (Gitea Actions)에서 빌드 시 자동 SBOM 생성
- OWASP Dependency-Check SCA 취약점 스캔 통합
- CVSS >= 7.0 발견 시 빌드 실패 정책 적용

### 2.2 SBOM 갱신 트리거

1. 신규 구성요소 추가 또는 기존 구성요소 버전 업데이트
2. 신규 CVE 발견 (CVSS >= 7.0)
3. 구성요소 제거 또는 교체
4. 정기 검토 (분기별)
5. 릴리스 빌드 시 자동 생성

---

## 3. NTIA 최소 요소 (NTIA Minimum Elements)

| NTIA 요소 | 설명 | 본 문서 적용 |
|-----------|------|-------------|
| Supplier Name | 구성요소 공급자 | 각 항목별 공급자 기재 |
| Component Name | 구성요소 이름 | 정식 명칭 사용 |
| Version | 구성요소 버전 | 정확한 버전 번호 (v2.0 갱신) |
| Unique Identifier | 고유 식별자 | CPE, Package URL (purl) |
| Dependency Relationship | 의존성 관계 | 직접(Direct)/간접(Transitive) |
| Author of SBOM Data | SBOM 작성자 | RA팀 |
| Timestamp | 생성 시각 | 2026-04-11T00:00:00Z |

---

## 4. 구성요소 목록 (Component Inventory)

### v2.0 버전 변경 요약 (v1.1 대비)

| 구성요소 | v1.1 버전 | v2.0 버전 | 변경 유형 | 비고 |
|---------|---------|---------|---------|------|
| fo-dicom | 5.1.3 | 5.2.5 | 업그레이드 | fo-dicom 최신, 버그픽스 포함 |
| Microsoft.EntityFrameworkCore | 8.0.2 | 9.0.0 | 업그레이드 | GHSA 취약점 수정 포함 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.2 | 9.0.0 | 업그레이드 | EF Core 9 연동 |
| Microsoft.EntityFrameworkCore.Design | (없음) | 9.0.0 | 신규 | Design-time 도구 |
| Microsoft.Data.Sqlite | 8.0.2 | (제거됨, EF9 내포) | 통합 | EF Core 9에 내포 |
| Microsoft.Extensions.* | 8.x | 9.0.0 | 업그레이드 | GHSA-qj66-m88j-hmgj 취약점 수정 |
| System.Text.Json | (없음) | 9.0.0 | 신규 | JSON 직렬화 취약점 대응 |
| System.IO.Ports | (없음) | 8.0.0 | 신규 | FPD Detector 시리얼 포트 |
| SQLitePCLRaw.bundle_e_sqlcipher | (없음) | 2.1.8 | 신규 | SQLCipher AES-256 PHI 암호화 |
| BCrypt.Net-Next | (없음) | 4.0.3 | 신규 | bcrypt 패스워드 해싱 |
| System.IdentityModel.Tokens.Jwt | (없음) | 7.3.1 | 신규 | JWT 인증 토큰 |
| Microsoft.IdentityModel.Tokens | (없음) | 7.3.1 | 신규 | JWT 검증 |
| Microsoft.Extensions.Caching.Memory | (없음) | 9.0.0 | 신규 | 취약점 대응 transitive override |
| NetArchTest.Rules | (없음) | 1.3.2 | 신규 | 아키텍처 테스트 (비배포) |
| FlaUI.Core | (없음) | 4.0.0 | 신규 | E2E UI 자동화 테스트 (비배포) |
| FlaUI.UIA3 | (없음) | 4.0.0 | 신규 | E2E UI 자동화 테스트 (비배포) |
| System.Drawing.Common | (없음) | 8.0.0 | 신규 | FlaUI 의존 (비배포) |

### 4.1 운영 체제 및 런타임 (OS & Runtime)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|------|--------|---------|-----------|--------|---------|--------|
| SBOM-001 | Windows 10 IoT Enterprise LTSC | 21H2 (19044) | Microsoft | Commercial | Class B | OS | 정기 패치 적용 | Medium |
| SBOM-002 | .NET 8.0 Runtime (LTS) | 8.0.x | Microsoft | MIT | Class B | Direct | CVE 모니터링 중 | Low |
| SBOM-003 | .NET 8.0 SDK | 8.0.x | Microsoft | MIT | N/A (빌드) | Direct | 런타임 미포함 | Low |
| SBOM-004 | ASP.NET Core Runtime | 8.0.x | Microsoft | MIT | Class B | Direct | CVE 모니터링 중 | Low |

### 4.2 UI 프레임워크 (UI Framework)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|------|--------|---------|-----------|--------|---------|--------|
| SBOM-005 | WPF (.NET 8) | 8.0.x | Microsoft | MIT | Class B | Direct | 알려진 CVE 없음 | Low |
| SBOM-006 | CommunityToolkit.Mvvm | 8.2.2 | Microsoft | MIT | Class A | Direct | 알려진 CVE 없음 | Low |
| SBOM-007 | MahApps.Metro | 2.4.10 | MahApps | MIT | Class A | Direct | 알려진 CVE 없음 | Low |
| SBOM-008 | LiveChartsCore.SkiaSharpView.WPF | 2.0.0-rc3 | LiveCharts | MIT | Class A | Direct | 알려진 CVE 없음 | Low |

### 4.3 DICOM 라이브러리 (DICOM Libraries)

| SBOM-ID | 구성요소명 | v1.1 버전 | v2.0 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|---------|---------|--------|---------|-----------|--------|---------|--------|
| SBOM-009 | fo-dicom | 5.1.3 | **5.2.5** | fo-dicom contributors | MS-PL | Class B | Direct | 알려진 CVE 없음 (업그레이드) | Medium |

> **v2.0 변경**: fo-dicom 5.1.3 → 5.2.5 업그레이드 (S04 R1 Team A SPEC-INFRA-001). 최신 버그픽스 포함. DICOM TLS 1.3 지원 개선.

### 4.4 데이터베이스 (Database)

| SBOM-ID | 구성요소명 | v1.1 버전 | v2.0 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|---------|---------|--------|---------|-----------|--------|---------|--------|
| SBOM-016 | SQLite | 3.45.1 | 3.45.1 | SQLite Consortium | Public Domain | Class B | Direct | 알려진 CVE 없음 | Low |
| SBOM-016b | SQLitePCLRaw.bundle_e_sqlcipher | (신규) | **2.1.8** | Eric Sink | Apache 2.0 | Class B | Direct | 알려진 CVE 없음 | High |
| SBOM-017 | Microsoft.EntityFrameworkCore | 8.0.2 | **9.0.0** | Microsoft | MIT | Class A | Direct | GHSA 취약점 수정 완료 | Low |
| SBOM-017b | Microsoft.EntityFrameworkCore.Sqlite | 8.0.2 | **9.0.0** | Microsoft | MIT | Class A | Direct | EF Core 9 연동 | Low |
| SBOM-017c | Microsoft.EntityFrameworkCore.Design | (신규) | **9.0.0** | Microsoft | MIT | N/A (빌드) | Direct | 런타임 미포함 | Low |

> **v2.0 변경**: SQLitePCLRaw.bundle_e_sqlcipher 2.1.8 신규 추가 — PHI AES-256-GCM 암호화(SPEC-INFRA-002)를 위한 SQLCipher 지원. EF Core 8.0.2 → 9.0.0 업그레이드 — GHSA 취약점 수정 포함.

### 4.5 보안 (Security)

| SBOM-ID | 구성요소명 | v2.0 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|---------|--------|---------|-----------|--------|---------|--------|
| SBOM-019 | BCrypt.Net-Next | **4.0.3** (신규) | Ryan D'Angelo | MIT | Class B | Direct | 알려진 CVE 없음 | Medium |
| SBOM-020 | System.IdentityModel.Tokens.Jwt | **7.3.1** (신규) | Microsoft | MIT | Class B | Direct | 알려진 CVE 없음 | Medium |
| SBOM-021 | Microsoft.IdentityModel.Tokens | **7.3.1** (신규) | Microsoft | MIT | Class B | Direct | 알려진 CVE 없음 | Medium |
| SBOM-022 | System.Security.Cryptography | 8.0.0 | Microsoft | MIT | Class B | Direct | .NET 보안 업데이트 | Medium |

> **v2.0 변경**: BCrypt.Net-Next 4.0.3, System.IdentityModel.Tokens.Jwt 7.3.1, Microsoft.IdentityModel.Tokens 7.3.1 신규 추가 — Team A 보안 인프라(SPEC-INFRA-001) 구현.

### 4.6 Microsoft Extensions (의존성 인프라)

| SBOM-ID | 구성요소명 | v1.1 버전 | v2.0 버전 | 공급자 | 라이선스 | SOUP Class | CVE 현황 | 위험도 |
|---------|-----------|---------|---------|--------|---------|-----------|---------|--------|
| SBOM-E01 | Microsoft.Extensions.Hosting | 8.x | **9.0.0** | Microsoft | MIT | Class A | GHSA-qj66-m88j-hmgj 수정 | Low |
| SBOM-E02 | Microsoft.Extensions.DependencyInjection | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E03 | Microsoft.Extensions.DependencyInjection.Abstractions | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E04 | Microsoft.Extensions.Configuration | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E05 | Microsoft.Extensions.Http | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E06 | Microsoft.Extensions.Options | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E07 | Microsoft.Extensions.Logging | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E08 | Microsoft.Extensions.Logging.Abstractions | 8.x | **9.0.0** | Microsoft | MIT | Class A | 취약점 수정 | Low |
| SBOM-E09 | Microsoft.Extensions.Caching.Memory | (신규) | **9.0.0** | Microsoft | MIT | Class A | transitive 취약점 override | Low |
| SBOM-E10 | System.Text.Json | (신규) | **9.0.0** | Microsoft | MIT | Class A | 취약점 대응 override | Low |
| SBOM-E11 | System.IO.Ports | (신규) | **8.0.0** | Microsoft | MIT | Class A | 알려진 CVE 없음 | Low |

> **v2.0 변경**: Microsoft.Extensions.* 전체 8.x → 9.0.0 업그레이드 (GHSA-qj66-m88j-hmgj 취약점 수정). System.Text.Json 9.0.0, System.IO.Ports 8.0.0 신규 추가.

### 4.7 직렬화/유틸리티 (Serialization/Utility)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | SOUP Class | CVE 현황 | 위험도 |
|---------|-----------|------|--------|---------|-----------|---------|--------|
| SBOM-026 | Polly | 8.3.1 | App vNext | BSD-3 | Class A | 알려진 CVE 없음 | Low |
| SBOM-027 | FluentValidation | 11.9.0 | Jeremy Skinner | Apache 2.0 | Class A | 알려진 CVE 없음 | Low |

### 4.8 로깅/모니터링 (Logging/Monitoring)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | SOUP Class | CVE 현황 | 위험도 |
|---------|-----------|------|--------|---------|-----------|---------|--------|
| SBOM-032 | Serilog | 3.1.1 | Serilog Contributors | Apache 2.0 | Class A | 알려진 CVE 없음 | Low |
| SBOM-033 | Serilog.Sinks.File | 5.0.0 | Serilog | Apache 2.0 | Class A | 알려진 CVE 없음 | Low |

### 4.9 테스트 도구 (Test — 빌드 전용, 배포 미포함)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | 비고 |
|---------|-----------|------|--------|---------|------|
| SBOM-039 | xunit | 2.7.0 | xUnit.net | Apache 2.0 | 단위 테스트 |
| SBOM-039b | xunit.runner.visualstudio | 2.5.7 | xUnit.net | Apache 2.0 | VS 테스트 러너 |
| SBOM-039c | Microsoft.NET.Test.Sdk | 17.9.0 | Microsoft | MIT | 테스트 SDK |
| SBOM-040 | NSubstitute | 5.1.0 | NSubstitute contributors | BSD-3 | 목 프레임워크 |
| SBOM-041 | FluentAssertions | 6.12.0 | Dennis Doomen | Apache 2.0 | 어설션 |
| SBOM-042 | coverlet.collector | 6.0.0 | tonerdo | MIT | 코드 커버리지 |
| SBOM-042b | coverlet.msbuild | 6.0.0 | tonerdo | MIT | MSBuild 커버리지 |
| SBOM-042c | Microsoft.EntityFrameworkCore.InMemory | 9.0.0 | Microsoft | MIT | 통합 테스트용 In-Memory DB |
| SBOM-042d | NetArchTest.Rules | **1.3.2** (신규) | Ben Morris | MIT | 아키텍처 규칙 테스트 |
| SBOM-042e | FlaUI.Core | **4.0.0** (신규) | FlaUI contributors | MIT | E2E UI 자동화 (FlaUI) |
| SBOM-042f | FlaUI.UIA3 | **4.0.0** (신규) | FlaUI contributors | MIT | FlaUI UIA3 드라이버 |
| SBOM-042g | System.Drawing.Common | **8.0.0** (신규) | Microsoft | MIT | FlaUI 의존 |

### 4.10 개발 전용 분석 도구 (Analyzer — PrivateAssets=all)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | 비고 |
|---------|-----------|------|--------|---------|------|
| SBOM-043 | StyleCop.Analyzers | 1.2.0-beta.556 | StyleCop 기여자 | Apache 2.0 | 코드 스타일 정적 분석 |
| SBOM-044 | Roslynator.Analyzers | 4.12.9 | Josef Pihrt | Apache 2.0 | 코드 품질 정적 분석 |
| SBOM-045 | SecurityCodeScan.VS2019 | 5.6.7 | SecurityCodeScan 기여자 | LGPL-3.0 | 보안 취약점 정적 분석 |

> SBOM-043~045는 `PrivateAssets=all` 설정으로 빌드 시에만 사용되며 배포 패키지에 포함되지 않는다.

### 4.11 Detector SDK (FPD Detector Interface)

| SBOM-ID | 구성요소명 | 버전 | 공급자 | 라이선스 | SOUP Class | 의존성 | CVE 현황 | 위험도 |
|---------|-----------|------|--------|---------|-----------|--------|---------|--------|
| SBOM-046 | AbyzSdk | **0.1.0.0** (신규) | Abyzr Co.,Ltd. | Proprietary | N/A (자사) | Direct | 알려진 CVE 없음 | Low |
| SBOM-047 | AbyzSdk.Imaging | **0.1.0.0** (신규) | Abyzr Co.,Ltd. | Proprietary | N/A (자사) | Direct | 알려진 CVE 없음 | Low |
| SBOM-048 | libxd2 | **2.0** (신규) | HME | Commercial | Class B | Direct | 알려진 CVE 없음 | Medium |
| SBOM-049 | libxd | **1.0** (신규) | HME | Commercial | Class B | Direct | 알려진 CVE 없음 | Medium |
| SBOM-050 | CIB_Mgr | **1.0** (신규) | HME | Commercial | Class B | Direct | 알려진 CVE 없음 | High |

> **v3.0 변경**: 자사 CsI FPD SDK 2종 (AbyzSdk, AbyzSdk.Imaging) 추가 — .NET managed, IL Only CLR 2.05, 의존성 Microsoft.Extensions.* 및 Microsoft.Extensions.DependencyInjection. HME 2G/1G 무선 FPD SDK 3종 (libxd2, libxd, CIB_Mgr) 추가 — Native C DLL, 총 255 exported functions, 지원 모델 S4335-CA/SZ4335-W/S4343-CA, TCP 5-소켓 (Port 25000-25004).

#### libxd2 기능 분류 (146 exported functions)
| 기능군 | 함수 수 | 대표 함수 | 환자 안전 영향 |
|--------|---------|-----------|----------------|
| Detector Lifecycle | 10+ | SD_Create/Destroy, CheckConnection | 낮음 |
| Acquisition | 30+ | SDAcq_CreateEx_*, Execute, Abort | **높음** (노출제어) |
| Calibration | 14 | SDCal_*, GenerateBPM, Validate | **높음** (영상품질) |
| Sleep/Power | 5 | Sleep, WakeUp, PowerOff, Reboot | 중간 |
| Firmware Update | 15 | SDUpdater_* | 중간 |
| Diagnostic | 10+ | SDDiag_*, SDDebug_* | 낮음 |
| File Transfer | 14 | SDFile_*, SDRemote_* | 낮음 |

#### HME SDK 모델 지원
| SDK | 지원 모델 | 프레임 크기 | 네트워크 |
|-----|----------|-----------|---------|
| libxd2 (2G) | S4335-CA, SZ4335-W | 3072x2560 | TCP 5-소켓 (25000-25004) |
| libxd (1G) | S4335-CA | 3072x2560 | TCP 연결 |
| CIB_Mgr | CR/DR 모드 지원 | — | CR/DR 회전/노출 제어 |

### 4.12 구성요소 요약 통계 (v3.0)

| 분류 | v2.0 항목 수 | v3.0 항목 수 | 변경 내용 |
|------|------------|------------|---------|
| OS/런타임 | 4 | 4 | 변경 없음 |
| UI 프레임워크 | 4 | 4 | 변경 없음 |
| DICOM | 1 | 1 | 변경 없음 |
| 데이터베이스 | 5 | 5 | 변경 없음 |
| 보안 | 4 | 4 | 변경 없음 |
| Microsoft Extensions | 11 | 11 | 변경 없음 |
| 직렬화/유틸 | 2 | 2 | 변경 없음 |
| 로깅 | 2 | 2 | 변경 없음 |
| Detector SDK | — | **5** | **AbyzSdk 2종 + HME 3종 신규** |
| 테스트 (비배포) | 12 | 12 | 변경 없음 |
| 분석기 (비배포) | 3 | 3 | 변경 없음 |
| **합계 (배포 포함)** | **~47** | **~52** | **신규 Detector SDK 5종** |

---

## 5. 취약점 관리 현황 (v3.0 기준)

### 5.1 CVSS >= 7.0 항목 현황

**CVSS >= 7.0 항목: 0건** (v3.0 갱신 후)

| 조치 완료 항목 | 취약점 ID | 이전 버전 | 조치 버전 | CVSS | 조치 일자 |
|-------------|---------|---------|---------|------|---------|
| Microsoft.Extensions.Hosting 외 | GHSA-qj66-m88j-hmgj | 8.x | 9.0.0 | 6.5 (Medium) | 2026-04-11 |
| fo-dicom | 내부 버그 수정 | 5.1.3 | 5.2.5 | N/A | 2026-04-11 |
| EF Core | GHSA-* | 8.0.2 | 9.0.0 | Low | 2026-04-11 |

> Directory.Packages.props 주석: "Microsoft.Extensions.Hosting Version=9.0.0 to fix GHSA-qj66-m88j-hmgj"

### 5.2 지속 모니터링 대상

| 우선순위 | 구성요소 | 근거 | 다음 검토 |
|---------|---------|------|---------|
| 1 | libxd2 2.0 | FPD 획득/노출 제어 — **환자 안전 핵심** | 2026-07 |
| 2 | CIB_Mgr 1.0 | CR/DR 회전/노출 제어 — **환자 안전 핵심** | 2026-07 |
| 3 | SQLitePCLRaw.bundle_e_sqlcipher 2.1.8 | PHI 암호화 핵심 | 2026-07 |
| 4 | fo-dicom 5.2.5 | DICOM 파서, 네트워크 노출 | 2026-07 |
| 5 | Microsoft.IdentityModel.Tokens 7.3.1 | JWT 검증 핵심 | 2026-07 |
| 4 | .NET 8.0 Runtime | 플랫폼 전체 영향 | 2026-07 |

---

## 6. SOUP 관리(IEC 62304 §8) — v3.0

### 6.1 SOUP 위험 등급 변경 사항

| 구성요소 | 변경 사유 | SOUP Class | 환자 안전 영향 |
|---------|---------|-----------|------------|
| libxd2 2.0 | FPD 획득/노출 제어 — **신규** | Class B | **환자 안전 직접 영향 (노출제어)** |
| CIB_Mgr 1.0 | CR/DR 회전/노출 제어 — **신규** | Class B | **환자 안전 직접 영향 (노출제어)** |
| libxd 1.0 | FPD 1G 레거시 — **신규** | Class B | 환자 안전 간접 영향 |
| SQLitePCLRaw.bundle_e_sqlcipher | PHI 암호화 핵심 | Class B | PHI 데이터 보호 직접 관련 |
| BCrypt.Net-Next | 패스워드 해싱 | Class B | 인증 보안 |
| System.IdentityModel.Tokens.Jwt | JWT 세션 관리 | Class B | 세션 인증 |
| fo-dicom 5.2.5 | 버전 업그레이드 | Class B | DICOM 영상 전송 |

### 6.2 DOC-033 SOUP 동기화 상태

DOC-033 SOUP Report v2.1로 갱신 필요 (신규 Detector SDK 5종 추가).

---

## 7. CycloneDX 형식 예시 (신규 Detector SDK 항목)

```json
{
  "bomFormat": "CycloneDX",
  "specVersion": "1.5",
  "serialNumber": "urn:uuid:sbom-hnvue-v3.0-20260413",
  "version": 3,
  "metadata": {
    "timestamp": "2026-04-13T00:00:00Z",
    "tools": [{"vendor": "HnVue RA Team", "name": "Manual SBOM", "version": "3.0"}],
    "component": {"type": "application", "name": "HnVue Console SW", "version": "1.0"}
  },
  "components": [
    {
      "type": "library",
      "name": "AbyzSdk",
      "version": "0.1.0.0",
      "purl": "pkg:nuget/AbyzSdk@0.1.0.0",
      "licenses": [{"license": {"name": "Proprietary"}}],
      "description": "자사 CsI FPD 디텍터 SDK (.NET managed, IL Only, CLR 2.05)",
      "supplier": {"name": "Abyzr Co.,Ltd."},
      "properties": [{"name": "addedInVersion", "value": "v3.0"}]
    },
    {
      "type": "library",
      "name": "AbyzSdk.Imaging",
      "version": "0.1.0.0",
      "purl": "pkg:nuget/AbyzSdk.Imaging@0.1.0.0",
      "licenses": [{"license": {"name": "Proprietary"}}],
      "description": "자사 FPD 이미징 처리 라이브러리 (.NET managed)",
      "supplier": {"name": "Abyzr Co.,Ltd."},
      "properties": [{"name": "addedInVersion", "value": "v3.0"}]
    },
    {
      "type": "library",
      "name": "libxd2",
      "version": "2.0",
      "licenses": [{"license": {"name": "Commercial (HME license)"}}],
      "description": "HME 2G Wireless FPD Detector SDK — Native C DLL (146 exported functions: lifecycle, acquisition, calibration, diagnostics, firmware update)",
      "supplier": {"name": "HME"},
      "properties": [
        {"name": "supported_models", "value": "S4335-CA, SZ4335-W, S4343-CA"},
        {"name": "protocol", "value": "TCP 5-socket (ports 25000-25004)"},
        {"name": "frame_sizes", "value": "3072x2560, 3072x3072"}
      ]
    },
    {
      "type": "library",
      "name": "libxd",
      "version": "1.0",
      "licenses": [{"license": {"name": "Commercial (HME license)"}}],
      "description": "HME 1G FPD Detector SDK — Native C DLL (100 exported functions)",
      "supplier": {"name": "HME"}
    },
    {
      "type": "library",
      "name": "CIB_Mgr",
      "version": "1.0",
      "licenses": [{"license": {"name": "Commercial (HME license)"}}],
      "description": "HME CIB 제어 모듈 — CR/DR 회전/노출 제어 (9 exports: ComOpen/Close, ExposeOn, CR_DR mode)",
      "supplier": {"name": "HME"}
    },
    {
      "type": "library",
      "name": "fo-dicom",
      "version": "5.2.5",
      "purl": "pkg:nuget/fo-dicom@5.2.5",
      "licenses": [{"license": {"id": "MS-PL"}}],
      "supplier": {"name": "fo-dicom contributors"},
      "properties": [{"name": "previousVersion", "value": "5.1.3"}]
    },
    {
      "type": "library",
      "name": "SQLitePCLRaw.bundle_e_sqlcipher",
      "version": "2.1.8",
      "purl": "pkg:nuget/SQLitePCLRaw.bundle_e_sqlcipher@2.1.8",
      "licenses": [{"license": {"id": "Apache-2.0"}}],
      "supplier": {"name": "Eric Sink"},
      "properties": [{"name": "addedInVersion", "value": "v2.0"}, {"name": "purpose", "value": "PHI AES-256-GCM encryption"}]
    },
    {
      "type": "library",
      "name": "BCrypt.Net-Next",
      "version": "4.0.3",
      "purl": "pkg:nuget/BCrypt.Net-Next@4.0.3",
      "licenses": [{"license": {"id": "MIT"}}],
      "supplier": {"name": "Ryan D'Angelo"},
      "properties": [{"name": "addedInVersion", "value": "v2.0"}]
    }
  ]
}
```
