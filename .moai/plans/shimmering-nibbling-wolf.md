# HnVue Console SW - 소스 코드 체계 및 프로젝트 구현 사양서

> **v3.0** | 교차검증 완료 | SDS/SAD/SRS/RTM/FRS/DOC-021(사용성) 6개 문서 딥싱크 + GUI 교차검증 반영

## Context

HnVue Console SW는 의료 진단 X-Ray 콘솔 소프트웨어(IEC 62304 Class B)로, 52개 이상의 규제 문서가 완성되었으나 소스 코드가 전무한 그린필드 프로젝트이다. 이 사양서는 SDS v2.0의 12개 모듈(35+ 클래스, 80+ 메서드), SAD v2.0의 10-스레드 동시성 모델, SRS v2.0의 185+ SWR을 교차검증하여, 구현 완성도와 코드 품질을 보장하는 소스 코드 체계를 정의한다.

**교차검증 범위:** SDS v2.0 ↔ SAD v2.0 ↔ SRS v2.0 ↔ RTM v2.0 ↔ FRS v2.0

---

## 1. 교차검증 결과: 문서간 불일치 및 갭

### 1.1 심각 (구현 전 해소 필수)

| ID | 불일치 | 영향 | 해결 방안 |
|---|---|---|---|
| **GAP-01** | RTM의 SAD/SDS 매핑 컬럼이 전부 "TBD" (90개 PR) | FDA 21 CFR 820.30(d) 설계 출력 추적 불가 | 본 사양서의 모듈-SWR 매핑으로 RTM 보완 |
| **GAP-02** | RTM의 테스트 케이스 ID가 전부 미정의 (UT/IT/ST/VT) | 검증 계획 없이 구현 착수 불가 | 모듈별 독립 테스트 프로젝트에서 TC-ID 자동 생성 |
| **GAP-03** | SRS의 비밀번호 해싱: Argon2id (64MB) vs SDS: bcrypt (cost=12) | 보안 구현 기준 모호 | **bcrypt 채택** (SDS 권위, .NET 8 네이티브 지원, Argon2id는 메모리 과다) |
| **GAP-04** | SRS의 RBAC 4역할 vs 초기 계획 2역할 | 접근 제어 설계 누락 | **SDS 기준 4역할** (Radiographer, Radiologist, Admin, Service) |
| **GAP-05** | SDS UI: MaterialDesignInXaml vs DOC-043: MahApps.Metro | UI 테마 라이브러리 충돌 | **MahApps.Metro 채택** (WPF 의료 앱 성숙도, 접근성) |
| **GAP-06** | SWR-CS-084 (코드 서명)과 SWR-SA-076 (업데이트 검증) 중복 | 구현 책임 모호 | CS→서명 검증, SA→롤백 메커닉으로 분리 |

### 1.2 주요 (Phase 1 테스트 전 해소)

| ID | 불일치 | 영향 | 해결 방안 |
|---|---|---|---|
| **GAP-07** | SBOM .NET 6 vs SAD/SDS .NET 8 | 패키지 버전 불일치 | SBOM v2.0으로 업데이트 |
| **GAP-08** | SBOM Moq vs NSubstitute | 테스트 프레임워크 불일치 | NSubstitute 채택 |
| **GAP-09** | SBOM에 DCMTK/DicomObjects 포함 | 불필요한 SOUP 의존성 | Phase 1에서 제거 (fo-dicom만 사용) |
| **GAP-10** | SWR 번호 갭 (PM-005~009, PM-021~029 등) | 추적성 모호 | 예약 번호로 문서화, RTM에 "Reserved" 표시 |
| **GAP-11** | 안전 SWR 3~5건의 HAZ 참조 누락 (WF-027, SA-063) | ISO 14971 추적 단절 | RMP 검토 후 HAZ 매핑 보완 |
| **GAP-12** | SOUP→SWR 의존성 매핑 부재 | SOUP 위험 전파 추적 불가 | RTM에 SOUP 의존성 컬럼 추가 |

---

## 2. 솔루션 구조 설계

### 2.1 설계 원칙

1. **SDS 1:1 매핑**: 12개 SDS 모듈 = 12개 .csproj (IEC 62304 §5.3 추적성)
2. **모듈별 독립 검증**: 각 모듈에 전용 테스트 프로젝트 (IEC 62304 §5.5 단위 검증)
3. **인터페이스 우선 설계**: 모든 모듈 간 통신은 인터페이스를 통해 DI로 연결
4. **안전 모듈 격리**: 안전 임계 모듈(WF, DM, CS, INC, UPD)은 비안전 모듈에 의존하지 않음
5. **10-스레드 동시성 모델**: SAD의 스레드 우선순위 설계를 코드 구조에 반영

### 2.2 전체 디렉토리 구조 (27개 프로젝트)

```
HnVue/
├── HnVue.sln
├── Directory.Build.props                   # 중앙 빌드: 버전, nullable, analyzers, deterministic
├── Directory.Packages.props                # 중앙 NuGet 패키지 버전 관리
├── global.json                             # .NET SDK 8.0 LTS 고정
├── nuget.config                            # NuGet 소스 (nuget.org + 내부)
├── .editorconfig                           # C# 코딩 표준 (Microsoft conventions)
│
├── src/                                     # 14개 소스 프로젝트
│   ├── HnVue.App/                          # 진입점, DI 컨테이너, 호스트 빌더
│   ├── HnVue.Common/                       # 공통 추상화 (Result<T>, 열거형, 인터페이스)
│   ├── HnVue.PatientManagement/            # SDS-PM-1xx (SWR-PM-001~053)
│   ├── HnVue.Workflow/                     # SDS-WF-2xx (SWR-WF-010~034, SWR-GEN-001~005)
│   ├── HnVue.Imaging/                      # SDS-IP-3xx (SWR-IP-020~050)
│   ├── HnVue.Dose/                         # SDS-DM-4xx (SWR-DM-040~051)
│   ├── HnVue.Dicom/                        # SDS-DC-5xx (SWR-DC-001~064)
│   ├── HnVue.SystemAdmin/                  # SDS-SA-6xx (SWR-SA-060~077)
│   ├── HnVue.Security/                     # SDS-CS-7xx (SWR-CS-070~087)
│   ├── HnVue.UI/                           # SDS-UI-8xx (SWR-UI-001~020)
│   ├── HnVue.Data/                         # SDS-DB-9xx (SWR-DB-001~015)
│   ├── HnVue.CDBurning/                    # SDS-CD-10xx (SWR-WF-032~034)
│   ├── HnVue.Incident/                     # SDS-INC-11xx (SWR-CS-086~087)
│   └── HnVue.Update/                       # SDS-UPD-12xx (SWR-SA-076~077, SWR-CS-084~085)
│
├── tests/                                   # 13개 테스트 프로젝트 (모듈별 독립 검증)
│   ├── HnVue.Common.Tests/
│   ├── HnVue.PatientManagement.Tests/
│   ├── HnVue.Workflow.Tests/               # 안전 임계: 90%+
│   ├── HnVue.Imaging.Tests/
│   ├── HnVue.Dose.Tests/                   # 안전 임계: 90%+
│   ├── HnVue.Dicom.Tests/
│   ├── HnVue.SystemAdmin.Tests/
│   ├── HnVue.Security.Tests/               # 안전 임계: 90%+
│   ├── HnVue.UI.Tests/                     # ViewModel만 테스트
│   ├── HnVue.Data.Tests/
│   ├── HnVue.CDBurning.Tests/
│   ├── HnVue.Incident.Tests/               # 안전 임계: 90%+
│   └── HnVue.Update.Tests/                 # 안전 임계: 85%+
│
├── tests.integration/                       # 1개 통합 테스트 프로젝트
│   └── HnVue.IntegrationTests/             # 모듈간 교차 시나리오
│
├── installer/
│   └── HnVue.Installer/                    # WiX v4 MSI
│
├── build/
│   └── scripts/
│       ├── build-release.ps1
│       ├── sign-package.ps1
│       └── generate-sbom.ps1
│
├── docs/                                    # 기존 규제 문서
└── scripts/                                 # 기존 유틸리티
```

### 2.3 SDS 모듈 ↔ 프로젝트 ↔ SWR 추적성 매트릭스

| SDS 모듈 | .csproj | SWR 범위 | 안전 임계 | 클래스 수 | 테스트 프로젝트 |
|---|---|---|---|---|---|
| SDS-PM-1xx | HnVue.PatientManagement | SWR-PM-001~053 | No | 5 | HnVue.PatientManagement.Tests |
| SDS-WF-2xx | HnVue.Workflow | SWR-WF-010~034, SWR-GEN-001~005 | **YES** | 4 | HnVue.Workflow.Tests |
| SDS-IP-3xx | HnVue.Imaging | SWR-IP-020~050 | No | 3 | HnVue.Imaging.Tests |
| SDS-DM-4xx | HnVue.Dose | SWR-DM-040~051 | **YES** | 1+DTO | HnVue.Dose.Tests |
| SDS-DC-5xx | HnVue.Dicom | SWR-DC-001~064 | No | 7 | HnVue.Dicom.Tests |
| SDS-SA-6xx | HnVue.SystemAdmin | SWR-SA-060~077 | No | 3 | HnVue.SystemAdmin.Tests |
| SDS-CS-7xx | HnVue.Security | SWR-CS-070~087 | **YES** | 5 | HnVue.Security.Tests |
| SDS-UI-8xx | HnVue.UI | SWR-UI-001~020 | No | 7 VM | HnVue.UI.Tests |
| SDS-DB-9xx | HnVue.Data | SWR-DB-001~015 | No | 6 Entity | HnVue.Data.Tests |
| SDS-CD-10xx | HnVue.CDBurning | SWR-WF-032~034 | No | 5 | HnVue.CDBurning.Tests |
| SDS-INC-11xx | HnVue.Incident | SWR-CS-086~087 | **YES** | 5 | HnVue.Incident.Tests |
| SDS-UPD-12xx | HnVue.Update | SWR-SA-076~077, SWR-CS-084~085 | **YES** | 5 | HnVue.Update.Tests |

---

## 3. 모듈별 상세 사양 (SDS 교차검증 완료)

### 3.1 HnVue.Common (공통 추상화)

**역할:** 모든 모듈의 공통 타입, 인터페이스 추상화, 에러 패턴

```
HnVue.Common/
├── Results/
│   ├── Result.cs                   # Result<T> 모나드 (Success/Failure)
│   └── ErrorCode.cs               # 표준화 에러 코드
├── Abstractions/
│   ├── IAuditService.cs           # 감사 로그 인터페이스
│   ├── ISecurityContext.cs        # 보안 컨텍스트 (현재 사용자, 권한)
│   └── IRetryPolicyFactory.cs    # Polly 정책 팩토리
├── Enums/
│   ├── SafeState.cs               # SS-IDLE, SS-DEGRADED, SS-BLOCKED, SS-EMERGENCY
│   ├── UserRole.cs                # Radiographer, Radiologist, Admin, Service
│   ├── WorkflowState.cs          # 9개 상태 (IDLE→COMPLETED/ERROR)
│   ├── GeneratorState.cs         # Disconnected, Idle, Preparing, Ready, Exposing, Done, Error
│   └── IncidentSeverity.cs       # Critical, High, Medium, Low
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Configuration/
    └── HnVueOptions.cs
```

**NuGet:** Microsoft.Extensions.DI.Abstractions, Polly 8.3.1, FluentValidation 11.9.0

### 3.2 HnVue.Security (SDS-CS-7xx) — 안전 임계

**클래스 (SDS 정의 기준):**

| 클래스 | 주요 메서드 | SWR |
|---|---|---|
| SecurityService | AuthenticateAsync, CheckAuthorizationAsync, LockAccountAsync | SWR-CS-070~073 |
| PasswordHasher | HashPassword(bcrypt cost=12), VerifyPassword | SWR-CS-070 |
| JwtTokenService | GenerateToken, ValidateToken, IsTokenExpired | SWR-CS-075~077 |
| AuditService | WriteAuditAsync, ComputeHmacChain, VerifyChainIntegrity | SWR-CS-080 |
| RbacPolicy | CanExpose, CanBurnCD, CanManageUsers, CanViewAuditLog | SWR-CS-071 |

**RBAC 매트릭스 (SDS 기준 4역할):**

| 권한 | Radiographer | Radiologist | Admin | Service |
|---|:---:|:---:|:---:|:---:|
| 환자 조회/등록 | O | O | O | - |
| 촬영 수행 | O | O | - | - |
| 영상 판독 | - | O | - | - |
| CD/DVD 굽기 | - | O | O | - |
| 시스템 설정 | - | - | O | O |
| 감사 로그 조회 | - | - | O | O |
| SW 업데이트 | - | - | O | O |

**보안 사양:** bcrypt cost=12 (300ms), 5회 실패→계정 잠금, JWT 15분 만료, HMAC-SHA256 해시 체인

**NuGet:** BCrypt.Net-Next 4.x, System.IdentityModel.Tokens.Jwt, Serilog 3.1.1, Serilog.Sinks.File

### 3.3 HnVue.Workflow (SDS-WF-2xx) — 안전 임계

**클래스:**

| 클래스 | 역할 | 스레드 |
|---|---|---|
| WorkflowEngine | 촬영 시퀀스 오케스트레이터 | WorkflowEngine Thread (Normal+1) |
| WorkflowStateMachine | 9-상태 전이 관리 | WorkflowEngine Thread |
| GeneratorSerialPort | RS-232 실제 통신 | GeneratorControl Thread (AboveNormal) |
| GeneratorSimulator | 개발용 시뮬레이터 | GeneratorControl Thread |
| FpdSdkWrapper | FPD SDK 실제 래핑 | ImageAcquisition Thread (AboveNormal) |
| DetectorSimulator | 개발용 시뮬레이터 | ImageAcquisition Thread |

**상태 머신 (9 상태):**
```
IDLE → PATIENT_SELECTED → PROTOCOL_LOADED → READY_TO_EXPOSE → EXPOSING
  → IMAGE_ACQUIRING → IMAGE_PROCESSING → IMAGE_REVIEW → COMPLETED
  (모든 상태에서 → ERROR 전이 가능)
```

**Generator 명령 세트:**
- SET_KVP, SET_MAS, LOAD_APR, PREP, EXPOSE, ABORT, GET_STATUS, GET_HEAT_UNITS
- 프레임: STX(0x02) + 데이터 + ETX(0x03), 타임아웃 1~30초

**NuGet:** System.IO.Ports

### 3.4 HnVue.Dose (SDS-DM-4xx) — 안전 임계

**선량 인터록 4단계 방어:**

| 단계 | 주체 | 동작 |
|---|---|---|
| Level 1 | HW 과부하 회로 | Generator 내장 독립 차단 |
| Level 2 | SW DoseService | kVp×mAs×부위계수 범위 검증 → ALLOW/WARN/BLOCK |
| Level 3 | WorkflowEngine | Dose 클리어런스 없이 FIRE 명령 차단 |
| Level 4 | UI 알림 | DRL 초과 다이얼로그 (사용자 확인 필수) |

### 3.5 HnVue.Dicom (SDS-DC-5xx)

**클래스 (7개):**

| 클래스 | DICOM 서비스 | SWR |
|---|---|---|
| DicomStoreScu | C-STORE SCU (PACS 전송) | SWR-DC-050~052 |
| DicomFindScu | C-FIND SCU (MWL 조회) | SWR-DC-053~055 |
| DicomPrintScu | Print SCU (N-CREATE/N-SET/N-ACTION) | SWR-DC-060~062 |
| DicomFileIO | DICOM 파일 I/O | SWR-DC-056 |
| TlsConfig | TLS 1.2/1.3 설정 | SWR-NF-SC-041 |
| HnVueTlsInitiator | ITlsInitiator 구현 | SWR-NF-SC-041 |
| DicomOutboxQueue | 로컬 영속 큐 (실패 시 재전송) | SWR-DC-051 |

**Polly 재시도 정책:** C-STORE 3회 (2/4/8초), MWL 2회 (2/4초), Print 1회 (5초)

**NuGet:** fo-dicom 5.1.3

### 3.6 HnVue.Data (SDS-DB-9xx)

**엔티티 (SDS §4 데이터 구조):**

| 테이블 | PK | 주요 컬럼 | 관계 |
|---|---|---|---|
| Patients | PatientId | Name, Dob, Sex, IsEmergency, CreatedBy | 1:N → Studies |
| Studies | StudyInstanceUID | PatientId(FK), Date, Desc, AccessionNumber, BodyPart | 1:N → Images |
| Images | SopInstanceUID | StudyInstanceUID(FK), FilePath, WC, WW | N:1 → Studies |
| DoseRecords | DoseId | StudyInstanceUID(FK), Dap, Ei, EffectiveDose, BodyPart | 1:1 → Studies |
| Users | UserId | DisplayName, PasswordHash(bcrypt), Role(4종), FailedLoginCount, IsLocked | - |
| AuditLogs | LogId(AI) | Timestamp, UserId, Action, Details, PreviousHash, CurrentHash | - |
| UpdateHistory | UpdateId(AI) | FromVersion, ToVersion, Status(SUCCESS/ROLLBACK/FAILED), PackageHash | - |

**NuGet:** EF Core 8.0.2, Microsoft.Data.Sqlite 8.0.2, SQLitePCLRaw.bundle_sqlcipher

### 3.7~3.12 나머지 모듈 (간략)

| 모듈 | 핵심 클래스 | 핵심 기능 |
|---|---|---|
| PatientManagement | PatientService, WorklistService | 환자 CRUD, MWL 10초 폴링, 응급 ID 생성 |
| Imaging | ImageProcessor, DicomImageBuilder | 9단계 파이프라인 (Offset→Gain→Defect→Noise→Edge→W/L→16to8→DICOM→WPF) |
| SystemAdmin | SystemAdminService | 사용자 관리, 프로토콜 편집, 시스템 설정 |
| CDBurning | CDDVDBurnService, IMAPIComWrapper | IMAPI2 COM, DICOMDIR, AES-256 옵션 암호화, 검증 |
| Incident | IncidentResponseService, CveDatabase | 4단계 분류(Critical/High/Medium/Low), NVD 조회, PSIRT 알림 |
| Update | SWUpdateService, CodeSignVerifier, BackupService | Authenticode SHA-256+RSA-2048, 해시 검증, 백업/롤백 |

---

## 4. 동시성 모델 (SAD §5.3 반영)

### 4.1 스레드 모델 (10+1 스레드)

| 스레드 | 우선순위 | 담당 모듈 | 동기화 |
|---|---|---|---|
| Main (WPF Dispatcher) | Normal | HnVue.UI | Dispatcher.Invoke |
| WorkflowEngine | Normal+1 | HnVue.Workflow | Mutex(Generator) |
| ImageAcquisition | AboveNormal | HnVue.Workflow(FPD) | ConcurrentQueue→IP |
| ImageProcessing (4x) | BelowNormal | HnVue.Imaging | Task.Run 콜백 |
| DICOMNetwork | BelowNormal | HnVue.Dicom | async/await |
| GeneratorControl | AboveNormal | HnVue.Workflow(Gen) | Mutex(WF) |
| DoseCalculation | Normal+1 | HnVue.Dose | Events→Main |
| AuditLogger | Lowest | HnVue.Security | Channel&lt;T&gt; |
| Watchdog | AboveNormal | HnVue.Common | 50ms Heartbeat |
| IncidentResponse | BelowNormal | HnVue.Incident | Events→Main |
| SWUpdate | Lowest | HnVue.Update | Events→Main |

**안전 우선순위:** Watchdog > GeneratorControl = ImageAcquisition > DoseCalculation > WorkflowEngine

### 4.2 안전 상태 전이 (SAD §9.4)

```
SS-IDLE ←→ SS-DEGRADED (PACS/MWL/캘리브레이션 실패)
SS-IDLE ←→ SS-BLOCKED  (Generator/FPD/TLS 실패)
SS-IDLE  → SS-EMERGENCY (HW 임계 에러, 미처리 예외)
SS-EMERGENCY → SS-IDLE  (안전 종료 + Admin 확인 + 재시작, 수동만)
```

---

## 5. 모듈 의존성 그래프 (SAD §6 검증)

```
Layer 0 (Base):     HnVue.Common
Layer 1 (Data):     HnVue.Data ← Common
Layer 2 (Security): HnVue.Security ← Common, Data
Layer 3 (Core):     HnVue.Dicom ← Common, Data, Security
                    HnVue.Imaging ← Common, Data
                    HnVue.Dose ← Common, Data
                    HnVue.Incident ← Common, Security, Data
                    HnVue.Update ← Common, Security, Data
Layer 4 (Domain):   HnVue.PatientManagement ← Common, Data, Dicom, Security
                    HnVue.CDBurning ← Common, Data, Security
                    HnVue.SystemAdmin ← Common, Data, Security, Update
Layer 5 (Workflow): HnVue.Workflow ← Common, PatientMgmt, Imaging, Dose, Dicom, Data, Incident
Layer 6 (UI):       HnVue.UI ← Common (인터페이스만, DI를 통한 구현체 주입)
Layer 7 (Host):     HnVue.App ← 전체 (DI 등록)
```

**안전 격리 규칙:** 안전 임계 모듈(WF, DM, CS, INC, UPD)은 Layer 1~3에만 의존. UI/App에 직접 의존하지 않음.

---

## 6. 모듈별 독립 구현-검증 사이클

### 6.1 각 모듈의 "구현→테스트→검증 완료" 사이클

```
[모듈 N 구현]
    ↓
[모듈 N 단위 테스트 작성] → dotnet test HnVue.{Module}.Tests
    ↓
[커버리지 검증] → dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
    ↓
[모듈 N 독립 빌드 검증] → dotnet build src/HnVue.{Module}/HnVue.{Module}.csproj
    ↓
[TC-ID 자동 매핑] → xUnit DisplayName에 SWR-xxx 태그 포함
    ↓
[모듈 N 검증 완료 ✓] → DOC-022 (단위 테스트 보고서)에 증거 기록
```

### 6.2 테스트 메서드 명명 규칙 (SWR 추적성)

```csharp
// SWR-CS-070: 인증 실패 5회 시 계정 잠금
[Fact]
[Trait("SWR", "SWR-CS-070")]
[Trait("HAZ", "HAZ-SEC")]
public async Task AuthenticateAsync_FiveFailedAttempts_LocksAccount()
{
    // Arrange, Act, Assert
}
```

**장점:** xUnit의 Trait 필터링으로 SWR별, HAZ별 테스트 실행 가능
- `dotnet test --filter "SWR=SWR-CS-070"` → 특정 SWR 검증
- `dotnet test --filter "HAZ=HAZ-SEC"` → 안전 관련 전체 검증

### 6.3 모듈별 커버리지 목표 및 근거

| 모듈 | 목표 | 근거 | HAZ 참조 |
|---|---|---|---|
| HnVue.Security | 90%+ | RBAC, 인증, PHI 암호화 (안전 임계) | HAZ-SEC |
| HnVue.Workflow | 90%+ | Generator 제어, 촬영 상태 머신 (안전 임계) | HAZ-RAD, HAZ-HW |
| HnVue.Dose | 90%+ | 선량 인터록, DRL 알림 (안전 임계) | HAZ-RAD |
| HnVue.Incident | 90%+ | 보안 인시던트 탐지/대응 (안전 임계) | HAZ-SEC |
| HnVue.Update | 85%+ | 코드 서명, 롤백 (SW 무결성) | HAZ-SW |
| HnVue.Dicom | 80%+ | DICOM 통신 핵심 인프라 | HAZ-DATA |
| HnVue.Data | 80%+ | 데이터 영속성, SQLCipher | HAZ-DATA |
| HnVue.PatientManagement | 80%+ | 환자 데이터 무결성 | HAZ-DATA |
| HnVue.Imaging | 80%+ | 영상 처리 파이프라인 | - |
| HnVue.SystemAdmin | 80%+ | 프로토콜 관리, 설정 | - |
| HnVue.CDBurning | 80%+ | 미디어 출력 | - |
| HnVue.UI (ViewModel) | 70%+ | XAML View 테스트 제외 | - |

---

## 7. 에러 처리 매트릭스 (SAD §13 + SDS §13 통합)

### 7.1 DICOM 통신 에러

| 시나리오 | 감지 | 1차 대응 | 2차 대응 | 안전 상태 |
|---|---|---|---|---|
| PACS 연결 실패 | Association 타임아웃 10초 | Polly 3회 재시도 (2/4/8초) | 로컬 큐 전환 | SS-DEGRADED |
| C-STORE 실패 | DIMSE 타임아웃 30초 | 동일 Association 1회 재시도 | 새 Association | SS-DEGRADED |
| MWL 실패 | DIMSE 타임아웃 15초 | 2회 재시도 (2초) | 캐시 + 수동 입력 | SS-DEGRADED |
| TLS 핸드셰이크 실패 | 타임아웃 10초 | 인증서 재검증 | 차단 (폴백 없음) | SS-BLOCKED |

### 7.2 Generator 통신 에러

| 코드 | 설명 | 대응 | 안전 상태 |
|---|---|---|---|
| E01/E02 | 통신 에러 | 3회 재시도 (1초) | SS-BLOCKED |
| E09 | 과부하 | 촬영 중단, 30분 쿨다운 | SS-EMERGENCY |
| E36/E37 | 튜브 과열 | Heat Unit 경고, 쿨다운 타이머 | SS-BLOCKED |
| E50 | 조작자 중단 | 정상 중지 | SS-IDLE |

### 7.3 FPD 에러

| 시나리오 | 감지 | 대응 | 안전 상태 |
|---|---|---|---|
| GigE 연결 끊김 | Heartbeat 5초 타임아웃 | 자동 재연결 3회 | SS-BLOCKED |
| 이미지 CRC 에러 | 프레임 CRC 불일치 | 폐기, 재촬영 요청 | SS-BLOCKED |
| 캘리브레이션 만료 | 마지막 캘리브 > 24시간 | 경고, 촬영 허용 | SS-DEGRADED |

### 7.4 Polly 재시도 정책 요약

| 대상 | 재시도 | 간격 | Circuit Breaker |
|---|---|---|---|
| PACS C-STORE | 3회 | 2/4/8초 (지수 백오프) | 5회 연속 실패→30초 차단 |
| MWL | 2회 | 2/4초 | - |
| Generator | 3회 | 1/1/1초 (고정) | - |
| FPD | 3회 | 2/4/8초 | - |
| DB | 3회 | 100/200/500ms | - |
| Print | 1회 | 5초 | - |

---

## 8. NuGet 패키지 배정 (Central Package Management)

| 프로젝트 | 주요 패키지 | SOUP Class |
|---|---|---|
| HnVue.Common | Microsoft.Extensions.DI.Abstractions, Polly 8.3.1, FluentValidation 11.9.0 | A |
| HnVue.Data | EF Core 8.0.2, Microsoft.Data.Sqlite 8.0.2, SQLitePCLRaw.bundle_sqlcipher | B |
| HnVue.Security | BCrypt.Net-Next 4.x, Serilog 3.1.1, Serilog.Sinks.File 5.0.0 | B |
| HnVue.Dicom | fo-dicom 5.1.3 | B |
| HnVue.Imaging | OpenCvSharp4 4.9.0 | B |
| HnVue.Workflow | System.IO.Ports | B |
| HnVue.UI | CommunityToolkit.Mvvm 8.2.2, MahApps.Metro 2.4.10 | A |
| HnVue.App | Microsoft.Extensions.Hosting, Serilog.Extensions.Hosting | A |
| 모든 Tests | xUnit 2.7.0, NSubstitute 5.x, FluentAssertions 6.12.0, Coverlet 6.0.0 | - |

---

## 9. 개발 단계 및 모듈별 구현-검증 순서

### Phase 1a: 기반 구축 + 독립 검증 (Week 1-3) → M1

| 순서 | 모듈 | 구현 | 검증 |
|---|---|---|---|
| 1 | HnVue.Common | Result<T>, 열거형, 인터페이스 | HnVue.Common.Tests |
| 2 | HnVue.Data | DbContext, Entity, SQLCipher, 마이그레이션 | HnVue.Data.Tests |
| 3 | HnVue.Security | RBAC 4역할, bcrypt, HMAC 해시 체인 | HnVue.Security.Tests (90%+) |
| 4 | HnVue.App (스켈레톤) | DI, Program.cs, 호스트 빌더 | 빌드 검증 |
| 5 | HnVue.UI (스켈레톤) | MainWindow, LoginView, LoginViewModel | HnVue.UI.Tests |

**M1 검증 기준:** 로그인→인증→RBAC→감사 로그 해시 체인 E2E 동작

### Phase 1b: Tier 1 핵심 + 독립 검증 (Week 4-12) → M2

**SW1 (리드):** 각 모듈 완료 시 즉시 독립 검증

| 순서 | 모듈 | 핵심 구현 | 검증 목표 |
|---|---|---|---|
| 6 | HnVue.Dicom | C-STORE, MWL, Print, TLS, OutboxQueue | 80%+, fo-dicom 인메모리 서버 |
| 7 | HnVue.Workflow | 9-상태 머신, Generator/FPD 시뮬레이터 | 90%+, 모든 상태 전이 검증 |
| 8 | HnVue.Incident | 4단계 분류, CVE 조회, 알림 | 90%+, 심각도별 대응 검증 |
| 9 | HnVue.Update | Authenticode 검증, SHA-256, 백업/롤백 | 85%+, 롤백 시나리오 검증 |

### Phase 1c: Tier 2 기능 + 독립 검증 (Week 4-16, 병렬) → M3

**SW2 (개발자):**

| 순서 | 모듈 | 핵심 구현 | 검증 목표 |
|---|---|---|---|
| 10 | HnVue.PatientManagement | 환자 CRUD, MWL 10초 폴링, 응급 ID | 80%+ |
| 11 | HnVue.Imaging | 9단계 파이프라인, W/L, Zoom/Pan | 80%+, 알려진 픽셀값 테스트 |
| 12 | HnVue.Dose | DAP 계산, DRL 알림, 4단계 인터록 | 90%+, ALLOW/WARN/BLOCK 전체 |
| 13 | HnVue.CDBurning | IMAPI2, DICOMDIR, 검증 | 80%+ |
| 14 | HnVue.SystemAdmin | 설정, 프로토콜 편집, 업데이트 연동 | 80%+ |

### Phase 1d: UI 통합 + 통합 테스트 (Week 12-20) → M4

| 작업 | 내용 |
|---|---|
| UI 전체 View/ViewModel | 7개 View-ViewModel 쌍 구현 |
| DI 완전 연결 | HnVue.App에서 모든 모듈 DI 등록 |
| 통합 테스트 | HnVue.IntegrationTests (모듈간 시나리오) |

**통합 테스트 시나리오:**
- 촬영 워크플로우: WF→Gen(sim)→FPD(sim)→IP→DICOM→Data
- DICOM 네트워크: StoreSCU→로컬SCP (fo-dicom 서버)
- 인증 플로우: Login→Security→Data→Audit
- CD 굽기: Study 선택→암호화→DICOMDIR→(시뮬 굽기)

### Phase 1e: 시스템 테스트 + 릴리즈 (Week 20-32) → M5/M6

| 작업 | 검증 기준 |
|---|---|
| 전체 빌드 | `dotnet build HnVue.sln` 성공 |
| 전체 테스트 | `dotnet test` 모든 프로젝트 통과 |
| 커버리지 | 안전 임계 모듈 90%+, 기타 80%+ |
| SBOM | `dotnet CycloneDX` 생성 + NVD 취약점 0건 |
| 코드 서명 | `signtool.exe` Authenticode 서명 |
| MSI 설치 | WiX v4 패키지 빌드 + 설치/제거 검증 |

---

## 10. 비기능 요구사항 검증 체크리스트 (SRS 교차검증)

| SWR | 요구사항 | 검증 방법 |
|---|---|---|
| SWR-NF-PF-001 | 영상 표시 지연 ≤1,000ms (5MP 16-bit) | Stopwatch 프로파일링 |
| SWR-NF-PF-002 | 환자 검색 ≤500ms (10,000건) | SQLite 벤치마크 |
| SWR-NF-PF-004 | GUI 응답 ≤200ms | UI 자동화 측정 |
| SWR-NF-PF-005 | 시스템 부팅 ≤60초 | 콜드 스타트 측정 |
| SWR-NF-RL-010 | 가용성 ≥99.9% | 72시간 연속 운전 테스트 |
| SWR-NF-RL-012 | WAL FULL sync, 정전 시 데이터 무손실 | 강제 종료 복원 테스트 |
| SWR-NF-RL-013 | SW 크래시 자동 복구 ≤30초 | Guardian 프로세스 테스트 |
| SWR-NF-RL-015 | 메모리 증가 ≤10MB/24h (72시간) | 장기 운전 메모리 프로파일링 |

---

## 11. 문서 업데이트 필요 목록

### 구현 착수 전 (필수)

| 문서 | 업데이트 내용 | 우선순위 |
|---|---|---|
| DOC-043 Build Environment v1.0 | 9→14개 프로젝트 구조, 13개 테스트 프로젝트 반영 | 필수 |
| DOC-019 SBOM v1.0 | .NET 8, NSubstitute, DCMTK/DicomObjects 제거 반영 | 필수 |
| DOC-032 RTM v2.0 | SAD/SDS 매핑 컬럼 채우기 (본 사양서 기준) | 필수 |
| SDS v2.0 §3.8 | MaterialDesignInXaml → MahApps.Metro 확정 | 필수 |

### Phase 1 테스트 전

| 문서 | 업데이트 내용 |
|---|---|
| DOC-032 RTM v2.0 | TC-ID 컬럼 (xUnit Trait 기반 자동 매핑) |
| DOC-008 RMP v1.0 | HAZ 누락 3~5건 보완 (WF-027, SA-063 등) |
| SRS v2.0 | SWR-CS-084/SA-076 중복 해소, Argon2id→bcrypt 확정 |

---

## 12. 핵심 참조 파일

| 파일 | 용도 |
|---|---|
| docs/planning/DOC-007_SDS_v2.0.md | 12개 모듈 상세 설계 (클래스, 메서드, 시퀀스) — 구현의 1차 입력 |
| docs/planning/DOC-006_SAD_v2.0.md | 아키텍처 (스레드, 안전, 보안, SOUP) — 구조 결정의 근거 |
| docs/planning/DOC-005_SRS_v2.0.md | 185+ SWR — 테스트 케이스 출처 |
| docs/verification/DOC-032_RTM_v2.0.md | MR→PR→SWR→TC 추적 — GAP-01/02 해소 필요 |
| docs/management/DOC-043_Build_Environment_v1.0.md | 빌드 환경 — 솔루션 구조 업데이트 필요 |
| docs/management/WBS-001_WBS_v2.0.md | WBS (24-36MM, 6 마일스톤) — 일정 참조 |
| docs/regulatory/DOC-019_SBOM_v1.0.md | SBOM — GAP-07/08/09 해소 필요 |

---

## 13. 검증 방법 (최종)

| 검증 항목 | 명령 | 기준 |
|---|---|---|
| 전체 솔루션 빌드 | `dotnet build HnVue.sln` | 0 errors, 0 warnings |
| 모듈별 독립 빌드 | `dotnet build src/HnVue.{Module}/` | 각 모듈 독립 빌드 성공 |
| 모듈별 독립 테스트 | `dotnet test tests/HnVue.{Module}.Tests/` | 각 모듈 테스트 100% pass |
| SWR별 테스트 | `dotnet test --filter "SWR=SWR-CS-070"` | 특정 SWR 검증 가능 |
| HAZ별 테스트 | `dotnet test --filter "HAZ=HAZ-SEC"` | 안전 테스트 일괄 실행 |
| 안전 모듈 커버리지 | Coverlet 리포트 | Security/Workflow/Dose: 90%+ |
| 통합 테스트 | `dotnet test tests.integration/` | 모듈간 시나리오 pass |
| SBOM 생성 | `dotnet CycloneDX` | 패키지 목록 + NVD 매칭 |
| E2E 앱 검증 | 수동 + WPF 자동화 | 로그인→촬영→PACS→감사로그 |

---

## 14. GUI 교차검증 결과: 구현 모듈 ↔ UI 의존성 및 설계 갭

### 14.1 구현 모듈 ↔ GUI 의존성 매트릭스

**UI는 HnVue.Common만 참조** — 비즈니스 모듈과 완전 분리. GUI 변경이 비즈니스 로직에 영향 없음.

| ViewModel | 주입받는 인터페이스 (Common에 정의) | 대응 비즈니스 모듈 | View |
|---|---|---|---|
| LoginViewModel | ISecurityService, IAuditService | Security | LoginView.xaml |
| PatientListViewModel | IPatientService, IWorklistService | PatientManagement | PatientListView.xaml |
| WorkflowViewModel | IWorkflowEngine, IDoseService, IGeneratorInterface | Workflow, Dose | WorkflowView.xaml |
| ImageViewerViewModel | IImageProcessor, IDicomFileIO | Imaging, Dicom | ImageViewerView.xaml |
| DoseDisplayViewModel | IDoseService | Dose | DoseDisplayView.xaml |
| SystemAdminViewModel | ISystemAdminService, ISWUpdateService | SystemAdmin, Update | SystemAdminView.xaml |
| CDDVDBurnViewModel | ICDDVDBurnService | CDBurning | CDDVDBurnView.xaml |

**GUI 변경 영향 범위:**
- 색상/테마 변경 → HnVue.UI 내부만 (ResourceDictionary)
- 레이아웃 변경 → HnVue.UI의 XAML만
- 비즈니스 로직 변경 → 비즈니스 모듈만 (UI 무관)
- 새 화면 추가 → HnVue.UI에 View/ViewModel 추가 + Common에 인터페이스 추가(필요 시)

### 14.2 GUI 설계 시스템 성숙도 평가

| 영역 | 성숙도 | 정의된 것 | 누락된 것 |
|---|---|---|---|
| **색상 체계** | 60% | 안전 5색 (#FF0000, #FFA500, #00AA00, #0066CC, #999999) | hover/active/disabled 상태색, 배경/표면색, 그라데이션 |
| **타이포그래피** | 0% | 없음 | 폰트 패밀리, 크기 체계, 굵기, 행간 전부 미정의 |
| **간격 체계** | 30% | 터치 최소 44×44px만 | 패딩/마진 그리드, 컴포넌트 간격 규칙 |
| **컴포넌트 스타일** | 40% | MVVM 구조, View-ViewModel 쌍 | 버튼/입력/테이블/다이얼로그 상세 스타일 |
| **접근성** | 70% | WCAG AA 대비 4.5:1, 터치 44px, DPI 100~200% | 키보드 내비게이션, 스크린 리더, 포커스 링 |
| **반응형** | 50% | 1920~3840 해상도, Per-Monitor DPI v2 | 브레이크포인트, 레이아웃 리플로우 전략 |
| **다국어** | 80% | 한/영 지원, ResourceDictionary i18n | RTL 미대응 (현재 불필요) |
| **커스터마이징** | 20% | 색상 일부 설정 가능 (SWR-SA-065) | 병원 로고, 테마 API, 색상 피커 |
| **전체** | **~44%** | | |

### 14.3 GUI 교차검증 발견 갭 (15건)

#### 심각 (구현 품질에 직접 영향)

| ID | 갭 | 영향 | 해결 방안 |
|---|---|---|---|
| **UI-GAP-01** | 타이포그래피 전무 | 개발자마다 다른 폰트/크기 사용 → 비일관적 UI | ResourceDictionary에 FontFamily/Size 토큰 정의 |
| **UI-GAP-02** | 간격 체계 미정의 | 패딩/마진 산발적 → 정렬 불량 | 8px 기반 그리드 (4/8/12/16/24/32px) 정의 |
| **UI-GAP-03** | 버튼 스타일 미정의 | Primary/Secondary/Danger 버튼 구분 불가 | 3단계 버튼 (Filled/Outlined/Text) + 상태별 색상 |
| **UI-GAP-04** | 폼 검증 시각 피드백 미정의 | 에러 필드 표시 방식 모호 | 에러 테두리(빨강), 아이콘, 메시지 위치 정의 |
| **UI-GAP-05** | 다이얼로그 크기/위치 미정의 | 모달 표시 비일관 | MaxWidth 600px, 중앙 배치, 배경 dim 50% |

#### 주요 (Phase 1.1에서 해소 가능)

| ID | 갭 | 해결 방안 |
|---|---|---|
| **UI-GAP-06** | hover/active/disabled 상태색 미정의 | 색상 팔레트를 상태별로 확장 |
| **UI-GAP-07** | 그림자/깊이(elevation) 미정의 | 3단계 elevation 정의 (0/2/8px) |
| **UI-GAP-08** | 애니메이션/전환 미정의 | 페이드 200ms, 슬라이드 300ms, ease-in-out |
| **UI-GAP-09** | 포커스 링 미정의 | #0066FF, 2px, offset 2px |
| **UI-GAP-10** | 테이블 행 스타일 미정의 | 행 높이 48px, hover 배경, 교대색 |

#### 권장 (Phase 2+)

| ID | 갭 | 해결 방안 |
|---|---|---|
| **UI-GAP-11** | 다크 모드 미정의 | 의료 기기는 밝은 환경 기본이므로 Phase 2 |
| **UI-GAP-12** | 병원 브랜딩 API 미정의 | 테마 JSON 스키마 + 로고 업로드 |
| **UI-GAP-13** | 인쇄 스타일시트 미정의 | 인쇄 시 버튼 숨김, 흑백 최적화 |
| **UI-GAP-14** | 드래그&드롭 피드백 미정의 | 드래그 프리뷰, 드롭존 하이라이트 |
| **UI-GAP-15** | 키보드 단축키 매핑 미정의 | F5=새로고침, Enter=촬영 등 정의 |

### 14.4 UI 폴더 구조: 변동 최소화 설계

GUI 변동(색상, 위치, 테마)이 잦을 것을 고려한 리소스 분리 구조:

```
src/HnVue.UI/
├── Themes/                          # 변동 잦음 — 테마 토큰 집중 관리
│   ├── HnVueTheme.xaml             # 마스터 테마 (MahApps.Metro 기반)
│   ├── Colors.xaml                  # 색상 토큰 딕셔너리 (변경 시 여기만)
│   ├── Typography.xaml              # 폰트 토큰 딕셔너리 (변경 시 여기만)
│   ├── Spacing.xaml                 # 간격 토큰 딕셔너리
│   └── ButtonStyles.xaml            # 버튼 스타일 (Primary/Secondary/Danger)
│
├── Resources/                       # 변동 보통 — 정적 리소스
│   ├── Icons/                       # MaterialDesign 아이콘 확장
│   └── Strings/                     # 다국어 문자열
│       ├── Strings.ko-KR.xaml       # 한국어
│       └── Strings.en-US.xaml       # 영어
│
├── Views/                           # 변동 잦음 — 레이아웃
│   ├── MainWindow.xaml              # 5-패널 대시보드
│   ├── LoginView.xaml
│   ├── PatientListView.xaml
│   ├── WorkflowView.xaml
│   ├── ImageViewerView.xaml
│   ├── DoseDisplayView.xaml
│   ├── SystemAdminView.xaml
│   └── CDDVDBurnView.xaml
│
├── ViewModels/                      # 변동 적음 — 로직
│   ├── MainWindowViewModel.cs
│   ├── LoginViewModel.cs
│   └── ... (7개)
│
├── Converters/                      # 변동 거의 없음
│   ├── BoolToVisibilityConverter.cs
│   └── SafeStateToColorConverter.cs
│
└── Controls/                        # 변동 적음 — 커스텀 컨트롤
    ├── InactivityTimer.cs           # 15분 자동 잠금
    └── DoseGauge.cs                 # 선량 게이지 컨트롤
```

**핵심:** 색상 변경 → `Colors.xaml` 1개 파일만. 폰트 변경 → `Typography.xaml` 1개 파일만. 레이아웃 변경 → 해당 View.xaml만. ViewModel/비즈니스 모듈에 영향 없음.

### 14.5 문서 업데이트 추가 항목

| 문서 | 추가 업데이트 | 우선순위 |
|---|---|---|
| SDS v2.0 §3.8 | UI-GAP-01~05 해소: 타이포그래피, 간격, 버튼 스타일, 폼 검증, 다이얼로그 상세 추가 | Phase 1a (기반 구축 시) |
| DOC-021 사용성 파일 v2.0 | 디자인 토큰 테이블 추가 (색상, 폰트, 간격 토큰 정의) | Phase 1a |
| DOC-043 Build Environment | UI 리소스 폴더 구조 (Themes/, Resources/) 반영 | Phase 1a |
| ★HnVUE UI 변경 최종안.pptx | SDS §3.8과 정합성 확인 필요 (바이너리 파일 수동 검토) | Phase 1a |

---

## 15. Phase 1 준비상태 딥싱크 교차검증 (2026-04-03 기준)

### 15.1 실제 프로젝트 상태

| 항목 | 상태 | 상세 |
|---|---|---|
| 소스 코드 (.cs, .xaml, .csproj, .sln) | **0건** | 그린필드 — 프로덕션 코드 전무 |
| 빌드 인프라 (global.json, Directory.Build.props 등) | **0/5건** | 5개 핵심 파일 모두 미생성 |
| CI/CD 파이프라인 | **0건** | .gitea/workflows/ 미존재 |
| .NET SDK | **PATH 미등록** | `dotnet --version` 실행 불가 |
| MSBuild | **존재** | VS2022 Professional 확인 |
| Git Remote | **이중 구성** | origin(Gitea 10.11.1.40:7001) + github(holee9) |
| 규제 문서 | **129개 MD** | 52+ 규제 문서 완비 |
| MOAI 설정 | **20개 yaml** | 프로젝트 설정 완료 |

### 15.2 GAP 해소 상태 (5건 검증)

| GAP | 상태 | 문서 증거 | 차단 여부 |
|---|---|---|---|
| **GAP-01** RTM SDS 매핑 | **미해소** | RTM에 "TBD" 102건 잔존 | Phase 1 설계 검증 차단 |
| **GAP-03** bcrypt vs Argon2id | **미해소** | SRS에 "Argon2id(64MB, 반복3)" 3회 등장 | 보안 모듈 구현 기준 모호 |
| **GAP-05** MaterialDesign vs MahApps | **부분 해소** | SBOM에 MahApps 기재, SDS에 MaterialDesign 잔존 | SDS 1줄 수정 필요 |
| **GAP-07** SBOM .NET 6 vs .NET 8 | **미해소** | SBOM에 ".NET 6.0 Runtime 6.0.36" 명시 | 빌드 환경 불일치 |
| **GAP-08** Moq vs NSubstitute | **미해소** | SBOM에 "Moq 4.20.70" 명시 | 테스트 인프라 불일치 |

### 15.3 IEC 62304 Phase Gate 충족 상태

**PG-A (기반 문서 게이트) — DMP v2.0 §12.2 기준:**

| 필수 문서 | 상태 | 비고 |
|---|---|---|
| MRD v3.0 (4-Tier) | ✅ 완료 | 72개 MR, Tier 1~4 분류 |
| PRD v2.0 | ✅ 완료 | 65개 PR, MR 추적 |
| FRS v2.0 | ✅ 완료 | 기능 요구사항 |
| SRS v2.0 | ✅ 완료 | 185+ SWR (단, GAP-03 Argon2id 미확정) |
| SAD v2.0 | ✅ 완료 | 12 모듈, 10-스레드, STRIDE |
| SDS v2.0 | ✅ 완료 | 35+ 클래스, 80+ 메서드 (단, GAP-05 UI 테마 미확정) |
| WBS v2.0 | ✅ 완료 | 6 마일스톤, 24-36 MM |
| DMP v2.0 | ✅ 완료 | 문서 마스터 플랜 |
| SDP v2.0 | ✅ 완료 | 개발 절차, CI/CD, STRIDE |
| CMP v1.0 | ⚠️ 부분 | Git Flow 정의됨, 도구 버전 일부 TBD |
| RTM v2.0 | ❌ 미완 | MR→PR→SWR 체인 완료, SAD/SDS/TC 매핑 전부 TBD |
| RMP v1.0 | ⚠️ 구버전 | 4-Tier 미반영, MR-072 등 누락 |
| Threat Model (STRIDE) | ❌ 미작성 | MR-050 대응 문서 부재 |
| V&V Master Plan | ❌ 미작성 | 검증 전략 미수립 |

### 15.4 최종 준비상태 판정

```
┌──────────────────────────────────────────────────────┐
│  Phase 1a 준비상태: 🟡 조건부 착수 가능                  │
│                                                      │
│  ✅ 충족 (10건):                                      │
│    - 핵심 추적 체인 (MRD→PRD→FRS→SRS→SAD→SDS)          │
│    - WBS/DMP/SDP/CMP 기반 문서                         │
│    - 아키텍처 상세 (12 모듈, 35+ 클래스)                 │
│    - Git 인프라 (이중 Remote)                           │
│    - VS2022 Professional + MSBuild                    │
│                                                      │
│  ❌ 미충족 — 즉시 해소 필수 (4건):                       │
│    1. .NET 8 SDK 설치 + PATH 등록                      │
│    2. GAP-03 확정 (bcrypt) → SRS 1줄 수정              │
│    3. GAP-07/08 확정 → SBOM .NET 8 + NSubstitute 반영  │
│    4. 빌드 인프라 5개 파일 생성                           │
│                                                      │
│  ⚠️ 미충족 — 병렬 진행 가능 (4건):                       │
│    1. RTM SAD/SDS 매핑 (102 TBD) → Week 2-3           │
│    2. RMP v2.0 업데이트 → M1 게이트 전까지              │
│    3. STRIDE Threat Model 작성 → M1 게이트 전까지       │
│    4. V&V Master Plan → M2 게이트 전까지               │
│                                                      │
│  착수 가능 시점: 즉시 해소 4건 완료 후 (예상 1일)         │
└──────────────────────────────────────────────────────┘
```

### 15.5 Phase 1a 착수 체크리스트

**즉시 해소 (구현 착수 전, ~1일):**

- [ ] .NET 8 SDK 설치 → `dotnet --version` = 8.0.x 확인
- [ ] SRS v2.0: "Argon2id" 3건 → "bcrypt (cost=12)" 확정 수정
- [ ] SBOM v1.0: .NET 6.0 → .NET 8.0, Moq → NSubstitute 수정
- [ ] SDS v2.0 §3.8: "MaterialDesignInXaml" → "MahApps.Metro" 수정
- [ ] 빌드 인프라 5개 파일 생성 (global.json, Directory.Build.props, Directory.Packages.props, nuget.config, .editorconfig)
- [ ] HnVue.sln + 27개 프로젝트 스캐폴딩
- [ ] `dotnet build HnVue.sln` 성공 확인

**병렬 진행 (Week 2-3, 구현과 동시):**

- [ ] DOC-043: 프로젝트 구조 14+13개로 업데이트
- [ ] DOC-032 RTM: SAD/SDS 매핑 102건 채우기 (계획서 §2.3 기준)
- [ ] DOC-008 RMP: 4-Tier + MR-072/037/039 반영 v2.0 업데이트
- [ ] .gitea/workflows/ci.yml: 빌드+테스트 자동화

**M1 게이트 전 (Week 3-6):**

- [ ] DOC-017 STRIDE Threat Model 초안
- [ ] DOC-021 사용성 파일: 디자인 토큰 테이블 추가
- [ ] CI/CD 파이프라인 운영 확인
