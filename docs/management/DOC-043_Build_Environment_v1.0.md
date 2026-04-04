# 소스코드 및 빌드 환경 기록 (Source Code & Build Environment Record)

| 항목 | 내용 |
|------|------|
| **문서 번호** | DOC-043 |
| **버전** | v1.0 (Draft) |
| **작성일** | 2026-03-31 |
| **작성자** | [작성 필요] |
| **검토자** | [작성 필요] |
| **승인자** | [작성 필요] |
| **제품** | HnVue Console SW (HnVue) |
| **회사** | HnVue (가칭) |
| **분류** | ✅ 최소 필수 |
| **적용 시장** | FDA 510(k) ✅ / MFDS 2등급 ✅ / EU MDR Class IIa △ (참조 활용) |
| **근거 규격** | IEC 62304:2006+AMD1:2015 §5.5, §8.1, FDA SW Guidance (2019) Section IV-H, MFDS 안내서-1425-01, EU MDR Annex II §3.2 |
| **IEC 62304 Class** | B (Basic Level) |

---

## 변경 이력 (Revision History)

| 버전 | 일자 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| v1.0 | 2026-03-31 | 최초 작성 (개발 착수 전 빌드 환경 계획서) | [작성 필요] |

---

## 승인란

| 역할 | 성명 | 서명 | 일자 |
|------|------|------|------|
| 작성자 (SW 개발 책임자) | [작성 필요] | [작성 필요] | [작성 필요] |
| 검토자 (QA 책임자) | [작성 필요] | [작성 필요] | [작성 필요] |
| 검토자 (RA 담당자) | [작성 필요] | [작성 필요] | [작성 필요] |
| 승인자 (대표이사/QA 총괄) | [작성 필요] | [작성 필요] | [작성 필요] |

---

## 목차

1. 목적
2. 범위
3. 관련 문서
4. 본문
   - 4.1 형상관리 도구
   - 4.2 소스코드 정보
   - 4.3 빌드 도구 및 버전
   - 4.4 의존성 관리
   - 4.5 재현 가능한 빌드 절차
   - 4.6 빌드 결과물
   - 4.7 빌드 환경 보존
5. 비고: 이 문서가 필요한 이유

---

## 1. 목적

본 문서는 FDA 510(k) 심사, MFDS 인허가, 또는 GMP Inspection(현장 감사) 요청 시 허가된 버전의 HnVue Console SW (HnVue)를 정확히 재현할 수 있도록 소스코드 위치와 빌드 환경 전체를 문서화한다.

소스코드 자체를 포함하지 않으며, 소스코드의 위치·접근 방법·재현 가능한 빌드 절차의 명세서로서 기능한다.

형상관리 계획서(DOC-042)와 연계하여 모든 릴리즈 버전의 소스코드 및 빌드 환경을 추적한다.

본 문서는 다음 규격 요건을 충족한다:

- **IEC 62304:2006+AMD1:2015** §5.5 (SW 통합 및 테스트), §8.1 (형상관리 — 빌드 이력 포함)
- **FDA SW Guidance (2019)** Section IV-H (빌드 환경 및 재현 절차)
- **MFDS 안내서-1425-01** SW 이력 관리 요건 (빌드 이력 포함)
- **EU MDR Annex II §3.2** 소프트웨어 제조 정보

---

## 2. 범위

| 항목 | 내용 |
|------|------|
| 적용 SW | HnVue Console SW (HnVue) v[TBD - 개발 완료 후 작성] |
| 적용 대상 | 소스코드, 빌드 도구, 의존성(SOUP/NuGet), 빌드 스크립트, 코드서명 환경, CI/CD 파이프라인 |
| 적용 시장 | FDA 510(k) (필수) / MFDS 2등급 (필수) / EU MDR Class IIa (참조 활용) |
| 소스코드 자체 | 본 문서에 포함하지 않음 — 위치 및 접근 방법만 기재 |
| 런타임 환경 | Windows 10/11 기반 산업용 워크스테이션 (배포 환경) — 별도 기재 |

---

## 3. 관련 문서

| 문서 번호 | 문서명 | 관계 |
|-----------|--------|------|
| DOC-042 | 형상관리 계획서 (CMP) | 형상 항목 관리 기준, 버전 관리 정책 |
| DOC-019 (SBOM-XRAY-GUI-001) | 소프트웨어 자재 명세서 (SBOM) | SOUP/의존성 버전 상세 목록 |
| DOC-034 (SRD-XRAY-GUI-001) | SW 릴리즈 기록 | 릴리즈별 빌드 정보 연계 |
| DOC-044 | 알려진 결함 목록 | 릴리즈 빌드와 연계된 잔여 결함 |
| DOC-006 (SAD-XRAY-GUI-001) | SW 아키텍처 설계서 | 소스코드 구조 참조 |
| SDP-RC-001 | SW 개발 업무 절차서 | 빌드 및 릴리즈 절차 |
| IEC 62304:2006+AMD1:2015 | SW 수명주기 표준 | 근거 규격 §5.5, §8.1 |
| FDA SW Guidance (2019) | FDA SW 심사 가이던스 | Section IV-H |

---

## 4. 본문

### 4.1 형상관리 도구

| 도구 | 용도 | 버전/설정 |
|------|------|----------|
| Git | 분산 버전 관리 시스템 | [작성 필요 — 권장: 2.40 이상] |
| [GitHub / Azure DevOps — 작성 필요] | 원격 저장소 호스팅, PR 리뷰, 이슈 관리 | [작성 필요] |
| 브랜칭 전략 | Git Flow (DOC-042 §4.3.2 참조) | — |
| 태그 정책 | `v[MAJOR.MINOR.PATCH]` (예: `v1.0.0`) | DOC-042 §4.3.4 |

---

### 4.2 소스코드 정보

#### 4.2.1 저장소 정보

| 항목 | 내용 |
|------|------|
| 저장소 위치 (URL) | [작성 필요 — 예: https://github.com/hnvue/hnvue] |
| 접근 권한 | 내부 인원만 접근 (비공개 저장소, 역할 기반 권한 부여) |
| 허가 제출 버전 태그 | [TBD - 개발 완료 후 작성] (예: `v1.0.0`) |
| 허가 제출 버전 Commit SHA | [TBD - 개발 완료 후 작성] (40자 SHA-1 해시) |
| 브랜치 | `main` (허가 버전) |

#### 4.2.2 디렉토리 구조 개요

HnVue의 소스코드 구조는 IEC 62304 §5.3 아키텍처 설계(DOC-006)에 정의된 SW 아이템 분해를 따른다.

```
HnVue/
├── HnVue.sln                              # Visual Studio 솔루션 파일 (28개 프로젝트)
├── Directory.Build.props                   # 전역 빌드 속성 (버전, nullable, analyzers, deterministic)
├── Directory.Packages.props                # 중앙 NuGet 패키지 버전 관리 (Central Package Management)
├── global.json                             # .NET SDK 8.0 LTS 버전 고정
├── nuget.config                            # NuGet 소스 설정
├── .editorconfig                           # C# 코딩 표준 (Microsoft conventions)
├── packages.lock.json                      # NuGet 의존성 잠금 파일 (버전 고정)
│
├── src/                                     # 소스 프로젝트 (14개)
│   ├── HnVue.App/                          # WPF Application Host (진입점, DI 컨테이너)
│   ├── HnVue.Common/                       # 공통 추상화 (Result<T>, 열거형, 인터페이스)
│   ├── HnVue.PatientManagement/            # SDS-PM-1xx: 환자관리, MWL 연동
│   ├── HnVue.Workflow/                     # SDS-WF-2xx: 촬영 워크플로우, Generator/FPD 인터페이스
│   ├── HnVue.Imaging/                      # SDS-IP-3xx: 영상처리 (W/L, Zoom, Rotate)
│   ├── HnVue.Dose/                         # SDS-DM-4xx: 선량관리 (DAP, DRL 인터록)
│   ├── HnVue.Dicom/                        # SDS-DC-5xx: DICOM 통신 (C-STORE, MWL, Print SCU)
│   ├── HnVue.SystemAdmin/                  # SDS-SA-6xx: 시스템 설정, 프로토콜 관리
│   ├── HnVue.Security/                     # SDS-CS-7xx: RBAC, bcrypt, PHI 암호화, 감사 로그
│   ├── HnVue.UI/                           # SDS-UI-8xx: WPF MVVM (Views, ViewModels, Themes)
│   ├── HnVue.Data/                         # SDS-DB-9xx: EF Core + SQLCipher (AES-256)
│   ├── HnVue.CDBurning/                    # SDS-CD-10xx: CD/DVD 굽기 (IMAPI2)
│   ├── HnVue.Incident/                     # SDS-INC-11xx: 인시던트 대응 (IEC 81001-5-1)
│   └── HnVue.Update/                       # SDS-UPD-12xx: SW 업데이트 (FDA 524B)
│
├── tests/                                   # 단위 테스트 프로젝트 (13개, 모듈별 독립 검증)
│   ├── HnVue.Common.Tests/                 # Common 모듈 테스트
│   ├── HnVue.PatientManagement.Tests/      # SDS-PM 독립 검증
│   ├── HnVue.Workflow.Tests/               # SDS-WF 독립 검증 (안전 임계: 90%+)
│   ├── HnVue.Imaging.Tests/               # SDS-IP 독립 검증
│   ├── HnVue.Dose.Tests/                   # SDS-DM 독립 검증 (안전 임계: 90%+)
│   ├── HnVue.Dicom.Tests/                 # SDS-DC 독립 검증
│   ├── HnVue.SystemAdmin.Tests/           # SDS-SA 독립 검증
│   ├── HnVue.Security.Tests/              # SDS-CS 독립 검증 (안전 임계: 90%+)
│   ├── HnVue.UI.Tests/                    # SDS-UI 독립 검증 (ViewModel만)
│   ├── HnVue.Data.Tests/                  # SDS-DB 독립 검증
│   ├── HnVue.CDBurning.Tests/            # SDS-CD 독립 검증
│   ├── HnVue.Incident.Tests/             # SDS-INC 독립 검증 (안전 임계: 90%+)
│   └── HnVue.Update.Tests/               # SDS-UPD 독립 검증 (안전 임계: 85%+)
│
├── tests.integration/                       # 통합 테스트 프로젝트 (1개)
│   └── HnVue.IntegrationTests/             # 모듈간 교차 시나리오 검증
│
├── installer/
│   └── HnVue.Installer/                    # WiX v4 MSI 설치 패키지 프로젝트
│       └── HnVue.Installer.wixproj
│
├── build/
│   └── scripts/
│       ├── build-release.ps1               # 릴리즈 빌드 자동화 스크립트 (PowerShell)
│       ├── sign-package.ps1                # 코드서명 스크립트
│       └── generate-sbom.ps1              # CycloneDX SBOM 자동 생성
│
├── docs/                                    # 규제 산출물 문서
│   ├── management/
│   ├── planning/
│   ├── testing/
│   ├── risk/
│   ├── verification/
│   └── regulatory/
│
├── scripts/
│   └── sync_docs.py                        # 문서 동기화 자동화 스크립트
│
└── .gitea/
    └── workflows/
        ├── ci.yml                           # CI 파이프라인 (PR 빌드, 단위 테스트, SBOM)
        └── release.yml                      # 릴리즈 빌드 파이프라인 (코드서명, MSI 생성)
```

> **주의**: 위 구조는 SDS v2.0의 12개 소프트웨어 모듈에 1:1 매핑하는 14개 소스 프로젝트 + 13개 모듈별 독립 테스트 프로젝트로 구성된다. IEC 62304 §5.3 추적성 및 §5.5 독립 검증 요구사항을 충족한다.

#### 4.2.3 소스코드 보호 조치

| 보호 항목 | 조치 내용 |
|----------|----------|
| 접근 제어 | 역할 기반 접근 — Developer(Read/Write), Reviewer(Read), Admin([작성 필요]) |
| 직접 Push 금지 | `main` 브랜치 Push Protection 활성화, `release/*` 브랜치 보호 |
| 코드 리뷰 | `main` 병합: PR + 2인 이상 Approve 필수 |
| 코드 서명 | GPG 서명 또는 [Authenticode — 작성 필요] |
| 백업 정책 | [일일 자동 백업 / 미러 저장소 — 작성 필요] |
| 보존 기간 | 최소 허가 유효기간 + 5년 (IEC 62304 수명주기 종료 후 보존) |

---

### 4.3 빌드 도구 및 버전

> **현재 상태**: 개발 착수 전 계획 단계. 아래 표는 DOC-006 (SAD) 및 DOC-019 (SBOM)에 정의된 기술 스택을 기준으로 작성되었으며, 개발 착수 후 실제 사용 버전으로 확정한다.

| 항목 | 명세 (계획) | 확정 버전 | 비고 |
|------|------------|---------|------|
| 빌드 OS | Windows 10 IoT Enterprise LTSC (21H2) 또는 Windows Server 2022 64-bit | [TBD - 개발 완료 후 작성] | 재현 빌드 환경 |
| .NET SDK | .NET 8.0 SDK (LTS) | [TBD - 개발 완료 후 작성] | SBOM-003 참조 |
| .NET Runtime | .NET 8.0 Runtime (LTS) | [TBD - 개발 완료 후 작성] | SBOM-002 참조 |
| IDE | Visual Studio 2022 Community / Professional / Enterprise | [TBD - 개발 완료 후 작성] | 빌드 및 디버깅 |
| MSBuild | Visual Studio 2022 내장 MSBuild | [TBD - 개발 완료 후 작성] | 주 빌드 도구 |
| WiX Toolset | WiX v4.x (MSI 설치 패키지 생성) | [TBD - 개발 완료 후 작성] | 설치 패키지 |
| CI/CD 도구 | [GitHub Actions / Azure Pipelines — 작성 필요] | [TBD - 개발 완료 후 작성] | 파이프라인 |
| NuGet 클라이언트 | .NET SDK 내장 NuGet | [TBD - 개발 완료 후 작성] | 의존성 관리 |
| 정적 분석 | Roslyn Analyzers (.NET 내장) + [SonarQube — 작성 필요] | [TBD - 개발 완료 후 작성] | 코드 품질 |
| 코드 커버리지 | Coverlet 6.0.0 (xUnit 연동) | [TBD - 개발 완료 후 작성] | 목표: 라인 커버리지 ≥ 80% |
| 코드서명 도구 | signtool.exe (Windows SDK 내장) | [TBD - 개발 완료 후 작성] | MSI 서명 |
| 코드서명 인증서 | EV Code Signing Certificate (권장) | 발급기관: [작성 필요], 유효기간: [작성 필요] | [작성 필요] |
| Git | Git for Windows | [TBD - 개발 완료 후 작성] | 버전 관리 |

---

### 4.4 의존성 관리

#### 4.4.1 패키지 관리 도구

| 도구 | 용도 | 설정 파일 | 비고 |
|------|------|----------|------|
| NuGet (dotnet CLI) | .NET 8 / WPF 의존성 관리 | `packages.lock.json`, `*.csproj` | CI 빌드 시 `--locked-mode` 사용 |
| Git Submodule (해당 시) | 소스 형태 SOUP 관리 (예: DCMTK 빌드 시) | `.gitmodules` | 해당 시에만 적용 |

**버전 고정 원칙**:
- 모든 NuGet 의존성은 정확한 버전 번호로 고정한다 (범위 지정 `[*]` 금지).
- `packages.lock.json`을 소스코드 저장소에 함께 커밋한다.
- CI 빌드 시 `dotnet restore --locked-mode`를 사용하여 버전 일관성을 강제한다.
- 의존성 업데이트는 반드시 DOC-042 §4.5 변경 통제 프로세스를 통해서만 수행한다.

#### 4.4.2 주요 의존성 목록 (SOUP 참조)

SBOM의 상세 목록은 DOC-019 (SBOM-XRAY-GUI-001)를 참조한다. 아래는 빌드에 직접 영향을 미치는 주요 의존성이다.

| SBOM-ID | 패키지명 | 고정 버전 | 출처 / 공급자 | IEC 62304 Class | 비고 |
|---------|---------|---------|-------------|----------------|------|
| SBOM-002 | .NET 8.0 Runtime | [TBD - 개발 완료 후 작성] | Microsoft | B | WPF 런타임 기반 |
| SBOM-005 | WPF (.NET 8) | [TBD - 개발 완료 후 작성] | Microsoft | B | UI 프레임워크 |
| SBOM-006 | CommunityToolkit.Mvvm | 8.2.2 | Microsoft | A | MVVM 패턴 지원 |
| SBOM-007 | MahApps.Metro | 2.4.10 | MahApps | A | UI 테마 |
| SBOM-009 | fo-dicom | 5.1.3 | fo-dicom contributors | B | DICOM 핵심 라이브러리 |
| SBOM-010 | DCMTK | 3.6.8 | OFFIS | B | DICOM C-FIND/C-MOVE |
| SBOM-012 | OpenCvSharp4 | 4.9.0 | shimat | B | 영상 처리 |
| SBOM-016 | SQLite | 3.45.1 | SQLite Consortium | B | 로컬 DB (Public Domain) |
| SBOM-017 | Microsoft.Data.Sqlite | 8.0.2 | Microsoft | A | EF Core SQLite 어댑터 |
| SBOM-018 | Entity Framework Core | 8.0.2 | Microsoft | A | ORM |
| SBOM-019 | BouncyCastle.Cryptography | 2.3.0 | Legion of the Bouncy Castle | B | 암호화 |
| SBOM-020 | OpenSSL (Native) | 3.2.1 | OpenSSL Project | B | TLS 통신 |
| SBOM-024 | NHapi (HL7 v2 Parser) | 3.2.0 | nHapi contributors | B | HL7 통신 |
| SBOM-032 | Serilog | 3.1.1 | Serilog Contributors | A | 감사 로그 |
| SBOM-039 | xUnit | 2.7.0 | xUnit.net | — | 단위 테스트 (비배포) |
| SBOM-040 | NSubstitute | 5.1.0 | NSubstitute contributors | — | Mock 프레임워크 (비배포) |
| SBOM-042 | Coverlet | 6.0.0 | tonerdo | — | 코드 커버리지 (비배포) |

> **주의**: 위 버전은 DOC-019 (SBOM) v1.0 기준이다. 개발 진행 중 변경 시 DOC-019를 먼저 업데이트하고 본 문서를 동기화한다.

---

### 4.5 재현 가능한 빌드 절차

#### 4.5.1 빌드 환경 설정

**전제 조건**:

| 항목 | 요구사항 | 확인 방법 |
|------|---------|---------|
| 빌드 OS | Windows 10 IoT Enterprise LTSC 21H2 64-bit 또는 Windows Server 2022 (클린 설치) | `winver` 명령어 |
| .NET 8 SDK | .NET 8.0.x SDK (LTS, 최신 패치 버전) | `dotnet --version` |
| Visual Studio | Visual Studio 2022 (MSBuild 17.x 포함) — .NET 데스크톱 워크로드 설치 | `msbuild -version` |
| WiX Toolset | WiX v4.x | `wix --version` |
| Git | Git for Windows 2.40+ | `git --version` |
| 코드서명 인증서 | EV Code Signing Certificate (보안 저장소에서 로드) | `signtool verify` |

**빌드 환경 고정 방법 (택일)**:

_옵션 A. Docker 이미지 (권장)_:
```
이미지명: hnvue-build:[버전] (예: hnvue-build:1.0.0)
저장 위치: [Docker Registry URL — TBD - 개발 완료 후 작성]
이미지 SHA-256: [TBD - 개발 완료 후 작성]
기반 이미지: mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022
```

_옵션 B. VM 스냅샷_:
```
스냅샷명: HnVue-Build-v[버전]
저장 위치: [TBD - 개발 완료 후 작성]
스냅샷 생성일: [TBD - 개발 완료 후 작성]
하이퍼바이저: [Hyper-V / VMware ESXi — 작성 필요]
```

> **선택 기준**: Docker 이미지는 CI/CD 환경과 통합이 용이하고 재현성이 높다. VM 스냅샷은 Windows 전용 도구(signtool 등) 사용이 필요한 경우에 적합하다.

#### 4.5.2 단계별 빌드 명령어 (계획)

> 아래 명령어는 개발 착수 전 계획 단계이며, 개발 완료 후 실제 명령어로 업데이트한다.

```powershell
# 1. 저장소 클론 (특정 릴리즈 태그)
git clone [저장소 URL]
cd HnVue
git checkout v1.0.0    # 허가 제출 버전 태그

# 2. 의존성 복원 (버전 잠금 모드)
dotnet restore --locked-mode

# 3. 빌드 실행 (Release 모드, x64)
dotnet build HnVue.sln --configuration Release --no-incremental

# 또는 MSBuild 직접 사용:
# msbuild HnVue.sln /p:Configuration=Release /p:Platform=x64 /p:RestoreLockedMode=true

# 4. 단위 테스트 실행 (빌드 품질 확인)
dotnet test tests/HnVue.Unit --configuration Release --no-build --collect:"XPlat Code Coverage"

# 5. 설치 패키지(MSI) 생성 (WiX Toolset)
dotnet build installer/HnVueSetup.wixproj --configuration Release

# 또는:
# [TBD - WiX v4 명령어 — 개발 완료 후 작성]

# 6. 코드서명 (MSI)
signtool sign /fd SHA256 /tr [타임스탬프 서버 URL — 작성 필요] /td SHA256 `
    installer/bin/Release/HnVue-v1.0.0-setup.msi

# 7. SHA-256 해시 생성 및 기록
certutil -hashfile installer/bin/Release/HnVue-v1.0.0-setup.msi SHA256
# → §4.6 빌드 결과물 표에 해시값 기록

# 8. 코드서명 검증
signtool verify /pa installer/bin/Release/HnVue-v1.0.0-setup.msi
```

> **주의**: 위 명령어는 실제 빌드 환경에 맞게 개발 완료 후 수정한다.  
> 빌드 스크립트 위치: `build/scripts/build-release.ps1`

#### 4.5.3 빌드 재현 검증 방법

1. §4.5.1 빌드 환경 설정에 따라 클린 환경을 준비한다.
2. §4.5.2 단계별 빌드 명령어를 실행하여 빌드를 수행한다.
3. 생성된 MSI 설치 패키지의 SHA-256 해시를 계산한다.
4. 본 문서 §4.6에 기록된 예상 해시와 비교한다.
5. **일치 시**: 재현 성공 — 빌드 환경이 올바르게 구성된 것으로 판정
6. **불일치 시**: 빌드 환경 차이 조사 (타임스탬프 서명 차이, .NET SDK 패치 버전 불일치, OS 업데이트 여부 등)

> **참고**: .NET 8 기반 빌드에서 완전한 바이트 동일성(bit-for-bit reproducible build)을 달성하기 어려운 경우, "기능적으로 동일함을 단위/통합 테스트 전체 통과로 검증"하는 현실적 대안을 채택하고 그 근거를 본 섹션에 기재한다.

---

### 4.6 빌드 결과물

> **현재 상태**: 개발 완료 후 실제 빌드 결과를 기재한다.

| 산출물명 | 파일명 | SW 버전 | 빌드 날짜 | SHA-256 해시 | 비고 |
|---------|--------|---------|----------|------------|------|
| 실행 파일 | HnVue.exe | [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | .NET 8 자체 포함 실행 파일 |
| MSI 설치 패키지 | HnVue-v[X.X.X]-setup.msi | [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | EV Code Signing 적용 |
| 코드서명 검증 결과 | — | — | [TBD] | — | `signtool verify /pa` 결과 |
| 단위 테스트 결과 | TestResults_[날짜].xml | — | [TBD] | — | xUnit 결과 (전체 Pass 필요) |
| 코드 커버리지 보고서 | coverage_[날짜].xml | — | [TBD] | — | Coverlet 생성 (목표: ≥ 80%) |

**빌드 환경 요약** (개발 완료 후 기재):

| 항목 | 실제 사용 버전 |
|------|------------|
| 빌드 OS | [TBD - 개발 완료 후 작성] |
| .NET 8 SDK 버전 | [TBD - 개발 완료 후 작성] |
| Visual Studio 버전 | [TBD - 개발 완료 후 작성] |
| MSBuild 버전 | [TBD - 개발 완료 후 작성] |
| WiX Toolset 버전 | [TBD - 개발 완료 후 작성] |
| 빌드 담당자 | [TBD - 개발 완료 후 작성] |
| 빌드 일시 | [TBD - 개발 완료 후 작성] |

---

### 4.7 빌드 환경 보존

| 항목 | 내용 |
|------|------|
| 보존 방법 | Docker 이미지 태그 고정 (`hnvue-build:[버전]`) 또는 VM 스냅샷 |
| 이미지/스냅샷 저장 위치 | [TBD - 개발 완료 후 작성] |
| 보존 기간 | 최소 허가 유효기간 + 5년 (수명주기 종료 후 폐기 전 RA 승인 필요) |
| 과거 버전 재빌드 보장 | 각 릴리즈 태그(`v[버전]`)에 대응하는 빌드 환경 스냅샷/이미지 유지 |
| 백업 주기 | [일일 자동 백업 — 작성 필요] |
| 빌드 환경 변경 시 | 새 이미지/스냅샷 생성 → 이전 이미지 보존 → DOC-043 개정 (DOC-042 변경 통제 적용) |

**릴리즈별 빌드 환경 참조 기록** (개발 완료 후 누적 기재):

| SW 버전 | 빌드 환경 이미지/스냅샷 | 이미지 SHA-256 | 생성일 | 비고 |
|---------|---------------------|-------------|------|------|
| v1.0.0 | [TBD - 개발 완료 후 작성] | [TBD] | [TBD] | Phase 1 최초 릴리즈 |

---

## 5. 비고: 이 문서가 필요한 이유

### 이 문서가 없으면?

빌드 명세서가 없으면 "허가된 버전의 바이너리를 재현할 수 있다"는 것을 증명할 방법이 없다. FDA는 현장 감사(Inspection) 시 "허가 시 제출한 버전을 지금 당장 빌드해 보여달라"고 요청할 수 있으며, 이 요청에 응하지 못하면 품질 시스템 부적합 판정을 받는다.

- **FDA 510(k)**: eSTAR 제출 시 빌드 환경과 재현 절차를 포함한 SW Documentation 제출이 필요하다. GMP Inspection 시 "허가된 버전을 재현할 수 있는가"는 핵심 감사 항목이다. 재현 불가 시 21 CFR Part 820 품질 시스템 규정 위반으로 Warning Letter 발행 사례가 있다.
- **MFDS 2등급**: 의료기기 소프트웨어 이력 관리 요건 하에 각 허가 버전의 빌드 이력 보관을 요구한다. 빌드 명세서 없이는 이력 관리가 불가능하다.
- **EU MDR Class IIa**: Annex II §3.2에서 소프트웨어 제조 정보를 요구하며, ISO 13485 §7.5.1(생산 및 서비스 제공 관리)은 빌드 재현 가능성을 전제한다.

### 시장별 요구 수준

| 시장 | 요구 수준 | 설명 |
|------|----------|------|
| FDA 510(k) | 필수 | GMP Inspection 핵심 항목. 재현 불가 시 Warning Letter 위험. 21 CFR Part 820 위반 |
| MFDS 2등급 | 필수 | 소프트웨어 이력 관리 요건 이행 문서. 빌드 이력 없이는 버전 관리 증명 불가 |
| EU MDR Class IIa | 필수 | Annex II §3.2 제조 정보 요건. ISO 13485 §7.5.1 빌드 재현 가능성 전제 |
