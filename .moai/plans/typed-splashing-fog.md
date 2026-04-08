# HnVue Worktree 기반 팀 분리 개발 운영 전략 (최종 v5)

## Context

HnVue: WPF/C# 의료 방사선 영상 솔루션 (IEC 62304 Class B).
17개 프로덕션 + 15개+ 테스트 프로젝트, 216개 규제 문서.
**3 구현팀 + Coordinator + QA + RA = 6개 worktree** 체계.
각 팀은 독립 Claude 세션 + 팀 전용 context + Gitea/GitHub 이슈 기반 작업 이력 관리.

### 교차검증 결과 요약

기존 프로젝트 문서와의 정합성 확인 완료:

| 검증 영역 | 결과 | 핵심 발견 |
|-----------|------|----------|
| QA 문서 (DOC-030, DOC-012, DOC-011) | 정합 | 130+ QA TC 존재, V-Model 5단계, 커버리지 기준 명확 |
| 릴리즈 기준 (DOC-034, RELEASE_READY) | 정합 | 10항목 체크리스트 + 4인 서명 게이트 존재 |
| RA 문서 (DOC-035 DHF, DOC-019 SBOM) | 정합 | CycloneDX SBOM 42컴포넌트, RTM 3-tier 추적 |
| 디자인 워크플로우 (DESIGN_TO_XAML) | 정합 | 5-Phase + QA/RA 리뷰 게이트 이미 정의 |
| CI/CD (desktop-ci.yml) | 확장필요 | 커버리지/정적분석 미포함 — QA가 확장 |
| Git 전략 (git-strategy.yaml) | 정합 | team mode, github-flow, conventional commits |
| **GAP: CMP-042** | **결함** | 설정관리계획 Draft 상태, 작성자/승인자 미지정 — RA 우선 해결 |
| **GAP: 팀 조직 구조** | **미정의** | RACI, 에스컬레이션, 결함관리 워크플로우 미문서화 |

---

## Part 1: 팀 구성 및 Worktree

### 전체 구조

```
D:\workspace-gitea\
├── Console-GUI\              ← main (릴리즈 통합)
├── Console-GUI-TeamA\        ← Team A (Infrastructure)
├── Console-GUI-TeamB\        ← Team B (Medical Domain)
├── Console-GUI-Design\       ← Design Team (Pure UI)
├── Console-GUI-Coord\        ← Coordinator (Integration)
├── Console-GUI-QA\           ← QA Team (Quality Assurance)
└── Console-GUI-RA\           ← RA Team (Regulatory Affairs)
```

### 브랜치 전략 (기존 git-strategy.yaml 정합)

```
main                           ← 릴리즈 통합 (항상 빌드 가능)
  ├── team-a/infra             ← Team A
  ├── team-b/domain            ← Team B
  ├── team-design/ui           ← Design Team
  ├── coordinator/contracts    ← Coordinator
  ├── qa/infrastructure        ← QA Team
  └── ra/documentation         ← RA Team
```

기존 `git-strategy.yaml` 설정(`workflow: github-flow`, `branch_prefix: feature/SPEC-`)과 병행.
팀 브랜치는 `team-*/` 접두사, SPEC 구현은 `feature/SPEC-*` 접두사 유지.

### Worktree 생성 명령어

```bash
cd D:\workspace-gitea\Console-GUI
git worktree add ../Console-GUI-TeamA     -b team-a/infra
git worktree add ../Console-GUI-TeamB     -b team-b/domain
git worktree add ../Console-GUI-Design    -b team-design/ui
git worktree add ../Console-GUI-Coord     -b coordinator/contracts
git worktree add ../Console-GUI-QA        -b qa/infrastructure
git worktree add ../Console-GUI-RA        -b ra/documentation
```

---

### Team A — Infrastructure & Foundation

**소유 모듈:** `Common` · `Data` · `Security` · `SystemAdmin` · `Update`
**소유 테스트:** Common, Data, Security, SystemAdmin, Update, Architecture Tests
**핵심 책임:**
- `Directory.Packages.props` NuGet 중앙 관리 독점
- DB 스키마 마이그레이션 (EF Core) 독점
- `HnVue.Common` 인터페이스 변경 시 `breaking-change` 이슈 생성 + Coordinator 동의

### Team B — Medical Imaging Pipeline

**소유 모듈:** `Dicom` · `Detector` · `Imaging` · `Dose` · `Incident` · `Workflow` · `PatientManagement` · `CDBurning`
**소유 테스트:** 각 모듈별 단위 테스트
**핵심 책임:**
- 안전임계 모듈 (Dose, Incident) 커버리지: DOC-012 기준 100% Branch 유지
- `IDetectorService`, `IWorkflowEngine` 변경 시 Coordinator 동의
- Workflow 상태전이 변경 → RA팀 RTM(DOC-032) 이슈 알림

### Team Design — Pure UI Design

**소유 모듈:** `HnVue.UI/Views` · `Styles` · `Themes` · `Components` · `Converters` · `Assets` · `DesignTime/`
**소유 테스트:** `HnVue.UI.Tests`, `HnVue.UI.QA.Tests` (FlaUI E2E)
**핵심 책임:**
- 비즈니스 로직 직접 참조 금지 (Architecture Tests 자동 검증)
- `DESIGN_TO_XAML_WORKFLOW.md` 5-Phase 준수
- DesignTime Mock ViewModel으로 VS2022 독립 렌더링
- Emergency Stop 버튼 위치 변경 시 QA/RA 리뷰 게이트 필수

### Coordinator — Integration & UI Contracts

**소유 모듈:** `UI.Contracts` · `UI.ViewModels` · `App` (DI 루트) · `tests.integration/`
**핵심 책임:**
- 모든 팀 간 인터페이스 변경의 유일한 게이트키퍼
- ViewModel에서 도메인 서비스 주입 조합
- `App.xaml.cs` DI 등록 관리
- 통합 테스트 운영 및 팀 간 PR 리뷰 조율

---

## Part 2: QA Team 상세 구성

### 역할 (기존 문서 교차검증 반영)

기존 DOC-030에 정의된 130+ QA TC와 DOC-011 V-Model을 **운용화**하는 것이 핵심.

| 책임 | 기존 문서 | QA팀 역할 |
|------|----------|----------|
| 정적 분석 | .editorconfig (존재) | SonarCloud + StyleCop **신규 도입** |
| 동적 분석 | coverlet (존재, 게이트 없음) | 커버리지 게이트 + Stryker.NET **신규** |
| 코드 리뷰 | CODEOWNERS (없음) | CODEOWNERS + PR 템플릿 **신규** |
| 테스트 실행 | TC-QA-xxx 130+ (DOC-030) | 자동화 파이프라인 구축 |
| 릴리즈 보고 | RELEASE_READY_REPORT (수동) | 자동 생성 스크립트 **신규** |
| 결함 관리 | bugs.json (3건), bug_tracking_list.md (6건) | 이슈 자동 연동 **신규** |

### QA 인프라 구축 상세

#### 2-A. 정적 분석 파이프라인

**StyleCop Analyzers** (`Directory.Packages.props`에 추가):
- 기존 `.editorconfig` 네이밍 규칙 보완
- `.stylecop.json` 프로젝트별 규칙 커스터마이징

**SonarCloud** (`.github/workflows/sonar.yml` 신규):
- PR당 자동 코드 품질 분석
- 품질 게이트: Bug=0, Vulnerability=0, Code Smell<50, Coverage≥85%
- 기존 `desktop-ci.yml` Step 4 이후 확장 포인트 활용

**OWASP Dependency-Check** (`.github/workflows/security-scan.yml` 신규):
- DOC-019 SBOM(42 컴포넌트) 자동 CVE 스캔
- CVSS ≥ 7.0 → 빌드 실패 (기존 DOC-019 §2.1 정책 준수)
- 결과 → RA팀 이슈 자동 생성 (SBOM 업데이트 트리거)

#### 2-B. 동적 분석 파이프라인

**커버리지 게이트** (`coverage.runsettings` 신규):
- 기존 DOC-012 기준 반영:
  - 일반 모듈: 80%+ Statement (quality.yaml 85%와 조율 → **85% 적용**)
  - 안전임계 (Dose, Incident, Security, Update): **90%+ Branch**
- CI 파이프라인에 통합 (현재 미적용 → QA가 확장)

**Stryker.NET** (`stryker-config.json` 신규):
- 안전임계 모듈 대상 뮤테이션 테스트
- Mutation Score 목표: ≥70%
- DesignTime/ 디렉토리 제외 설정

#### 2-C. 릴리즈 준비도 자동 보고

**`scripts/qa/Generate-ReleaseReport.ps1`** — 기존 DOC-034 릴리즈 체크리스트 10항목 자동 집계:

| # | DOC-034 체크항목 | 자동 수집 소스 | 목표 |
|---|-----------------|-------------|------|
| 1 | V&V 종합 보고서 합격 | TRX 테스트 결과 | Pass |
| 2 | 위험 관리 보고서 완료 | RA 이슈 상태 확인 | 완료 |
| 3 | 사이버보안 테스트 합격 | OWASP 스캔 결과 | Critical=0 |
| 4 | 사용적합성 테스트 합격 | FlaUI E2E 결과 | Pass |
| 5 | QA 검증 합격 | SonarCloud 게이트 | Passed |
| 6 | SBOM 최종 확인 | CycloneDX JSON 존재 여부 | 확인 |
| 7 | 모든 결함 해결 | bugs.json + Gitea 이슈 | P1=0 |
| 8 | 릴리스 노트 작성 | CHANGELOG.md 최신 엔트리 | 확인 |
| 9 | IFU 완료 | DOC-040 존재 여부 | 확인 |
| 10 | 코드 서명 완료 | 빌드 아티팩트 서명 | 확인 |

**산출물:** `TestReports/RELEASE_READY_{date}.html` (자동 생성)

### QA팀 소유 파일 전체 목록

| 파일 | 현황 | 비고 |
|------|------|------|
| `.github/workflows/desktop-ci.yml` | 기존 | 커버리지/분석 확장 |
| `.github/workflows/sonar.yml` | **신규** | SonarCloud |
| `.github/workflows/security-scan.yml` | **신규** | OWASP |
| `.github/workflows/qa-issue-reporter.yml` | **신규** | 이슈 자동생성 |
| `scripts/ci/Invoke-DesktopCi.ps1` | 기존 | TRX 파싱 확장 |
| `scripts/qa/Generate-ReleaseReport.ps1` | **신규** | 릴리즈 리포트 |
| `scripts/qa/Invoke-MutationTest.ps1` | **신규** | Stryker |
| `scripts/qa/Invoke-SecurityScan.ps1` | **신규** | OWASP |
| `coverage.runsettings` | **신규** | 커버리지 임계값 |
| `stryker-config.json` | **신규** | 뮤테이션 설정 |
| `.stylecop.json` | **신규** | 코드 스타일 |
| `CODEOWNERS` | **신규** | 팀별 소유권 |
| `.github/pull_request_template.md` | **신규** | PR 체크리스트 |
| `TestReports/` | 기존 | 보고서 집계 |
| `docs/testing/DOC-012~031` | 기존 | 유지보수 |

---

## Part 3: RA Team 상세 구성

### 역할 (기존 문서 교차검증 반영)

| 책임 | 기존 문서 현황 | RA팀 역할 |
|------|-------------|----------|
| DHF 유지 | DOC-035 v1.0 (구조 정의 완료) | 최신화 + 릴리즈별 스냅샷 |
| SBOM | DOC-019 v1.0 (42컴포넌트, CycloneDX) | 자동 갱신 연동 |
| RTM | DOC-032 v2.0 (MR→PR→SWR→TC) | 추적 단절 모니터링 |
| 릴리즈 승인 | DOC-034 4인 서명 체인 | RA/QA 책임자 서명 담당 |
| 위험관리 | DOC-008 v1.0 (v2.0 → 2026-05 계획) | RMP v2.0 작성 주도 |
| **CMP-042** | **v1.0 Draft (작성자 미지정!)** | **우선 과제: CMP 완성** |
| FDA 제출 | DOC-036 eSTAR v2.0 (계획) | 제출 패키지 준비 |
| CE MDR | DOC-037 v1.0 (계획) | Technical Documentation |
| KFDA | DOC-039 v1.0 (계획) | 품목 허가 신청서 |

### RA 우선 과제: CMP-042 완성

**교차검증에서 발견된 중대 결함:**
- `docs/management/DOC-042_ConfigMgmt_v1.0.md` — Draft 상태, 작성자/검토자/승인자 **모두 미지정**
- IEC 62304 §8.1 및 FDA 21 CFR 820 필수 요구사항
- **RA팀 첫 번째 우선 과제로 지정**

### RA 인프라 구축 상세

#### 3-A. 문서 자동화

**SBOM 자동 생성** (`scripts/ra/Generate-SBOM.ps1` 신규):
- `dotnet list package --format json` → CycloneDX 1.5 JSON 변환
- 기존 DOC-019의 42 컴포넌트 목록과 diff 비교
- OWASP 스캔 결과 병합 → VEX(DOC-045) 자동 갱신 입력

**RTM 자동 추적** (`scripts/ra/Update-RTM.ps1` 신규):
- 코드의 `[Trait("SWR", "SWR-xxx")]` xUnit 어노테이션 → RTM 매핑 (기존 DOC-012 v2.0 방식)
- 테스트 결과와 SWR 자동 연결
- 추적 단절 → `priority-high` 이슈 자동 생성

**DocFX 자동 빌드** (`.github/workflows/docs.yml` 신규):
- 기존 `docfx.json` 설정 활용 (이미 구성 완료)
- main 병합 시 API 문서 자동 생성 → `_site/` 배포

#### 3-B. 구현 변경 → RA 문서 업데이트 매핑

기존 DMP-001 §7의 Phase별 개정 로드맵 기반:

| 구현 변경 이벤트 | 영향 문서 | 자동 이슈 트리거 |
|----------------|----------|---------------|
| NuGet 패키지 추가/삭제 | DOC-019 SBOM, DOC-033 SOUP | QA OWASP → RA 이슈 |
| 인터페이스 계약 변경 | DOC-005 SRS, DOC-032 RTM | Coordinator → RA 이슈 |
| Workflow 상태 추가/삭제 | DOC-004 FRS, DOC-032 RTM | Team B → RA 이슈 |
| 보안 기능 변경 | DOC-049 IEC81001, DOC-045 VEX | Team A → RA 이슈 |
| P1 버그 수정 | DOC-044 Known Anomalies | QA → RA 이슈 |
| 아키텍처 변경 | DOC-006 SAD, DOC-007 SDS | Coordinator → RA 이슈 |

#### 3-C. 릴리즈 게이트 (기존 DOC-034 서명 체인 유지)

```
QA 릴리즈 리포트 Green
    ↓
RA 체크리스트:
  ✓ DOC-032 RTM 추적 완전성
  ✓ DOC-019 SBOM 최신
  ✓ DOC-044 Known Anomalies 0건
  ✓ CMP-042 설정관리 준수
    ↓
4인 서명 게이트 (기존 DOC-034 §5):
  SW개발책임자 → QA팀장 → RA/QA책임자 → 프로젝트관리자
    ↓
릴리즈 승인
```

### RA팀 소유 파일 전체 목록

| 파일 | 현황 | 비고 |
|------|------|------|
| `docs/regulatory/` (16개) | 기존 | 유지보수 |
| `docs/planning/DOC-005,006,007` | 기존 | SRS, SAD, SDS 최신화 |
| `docs/risk/` (5개) | 기존 | RMP v2.0 작성 (2026-05) |
| `docs/verification/DOC-032` | 기존 | RTM 추적 |
| `docs/verification/DOC-033` | 기존 | SOUP 목록 |
| `docs/management/DOC-042` | 기존(Draft) | **CMP 완성 (우선!)** |
| `scripts/ra/Generate-SBOM.ps1` | **신규** | SBOM 자동화 |
| `scripts/ra/Update-RTM.ps1` | **신규** | RTM 자동 추적 |
| `scripts/ra/Package-Submission.ps1` | **신규** | 제출 패키지 |
| `.github/workflows/docs.yml` | **신규** | DocFX 빌드 |
| `CHANGELOG.md` | **신규** | 루트 변경 이력 |
| `docs/VERSIONING.md` | **신규** | 문서 버전 정책 |
| `docfx.json` | 기존 | 설정 유지 |

---

## Part 4: 팀별 Claude Context 최적화

### 원칙

- 각 팀 worktree 브랜치에서 **CLAUDE.md를 팀 전용 버전으로 교체**
- `.claude/rules/teams/` 디렉토리에 팀별 규칙 파일 배치 (main에 tracked)
- main의 CLAUDE.md는 전체 오케스트레이터용 유지 → 팀 브랜치에서만 override
- **Context 토큰 절감 목표: 팀 전용 CLAUDE.md 로드 시 메인 대비 60% 이하**

### 팀별 Context 로드 맵

| 팀 | CLAUDE.md (팀 전용) | 추가 규칙 | 추가 스킬 | 제외 |
|----|-------------------|----------|----------|------|
| Team A | Infra 전용 (Common, Data, Security) | `teams/team-a.md` | `moai-lang-csharp`, `moai-domain-database` | UI, DICOM, RA |
| Team B | Domain 전용 (Dicom, Workflow, Dose) | `teams/team-b.md` | `moai-lang-csharp`, `moai-foundation-quality` | UI, 인프라, RA |
| Design | UI 전용 (Views, Themes, Components) | `teams/team-design.md` | `moai-domain-uiux`, `moai-domain-frontend` | 백엔드, DB, RA |
| Coordinator | 통합 전용 (Contracts, VM, App) | `teams/coordinator.md` | `moai-lang-csharp`, `moai-ref-api-patterns` | 단일 모듈 상세 |
| QA | 품질 전용 (CI, 분석, 리포트) | `teams/qa.md` | `moai-foundation-quality`, `moai-workflow-testing` | 구현 상세 |
| RA | 규제 전용 (문서, RTM, SBOM) | `teams/ra.md` | `moai-workflow-jit-docs` | 구현 상세, UI |

### 팀 규칙 파일 구조 (.claude/rules/teams/)

```
.claude/rules/teams/
├── team-a.md          # EF Core 패턴, SQLCipher, Repository 표준
├── team-b.md          # DICOM 태그, FPD SDK, Workflow 상태머신, Dose 인터락
├── team-design.md     # MahApps.Metro, MVVM 바인딩, 접근성, DesignTime Mock
├── coordinator.md     # DI 등록, UI.Contracts 영향도, 통합 테스트
├── qa.md              # SonarCloud, OWASP, Stryker, 릴리즈 기준
└── ra.md              # IEC 62304, RTM ID 규칙, FDA eSTAR, SOUP 절차
```

각 파일은 `paths` frontmatter 없이 항상 로드 (팀 브랜치에서만 활성화).

### 팀별 CLAUDE.md 자동 생성 스크립트

`scripts/setup/Init-TeamWorktree.ps1`:
- 파라미터: `-TeamName` (team-a, team-b, design, coordinator, qa, ra)
- 팀 전용 CLAUDE.md 생성 (관련 스킬/규칙만 @import)
- git tracked되지 않는 로컬 파일로 생성 (`.gitignore`에 추가 불필요 — worktree별 독립)

---

## Part 5: Gitea / GitHub 이슈 추적

### 이중 레포 관리

| 플랫폼 | URL | 용도 |
|--------|-----|------|
| Gitea (내부) | `http://10.11.1.40:7001` | 모든 작업 이슈, 버그, 내부 QA, RA |
| GitHub (외부) | `github.com/holee9` | 공개 이슈, 릴리즈 노트, CI/CD |

### 한글 깨짐 방지 (Windows 환경)

**원인**: Windows 콘솔 CP949 → API UTF-8 인코딩 불일치

**해결: `scripts/issue/New-Issue.ps1` 공통 스크립트**:
```powershell
# UTF-8 강제 설정 (모든 이슈 스크립트 상단에 적용)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
$OutputEncoding = [System.Text.Encoding]::UTF8
```

**Gitea API 호출 (`Invoke-RestMethod`):**
```powershell
$bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($jsonPayload)
Invoke-RestMethod -Uri $uri -Method Post -Headers $headers `
    -Body $bodyBytes -ContentType "application/json; charset=utf-8"
```

**GitHub CLI (`gh`):**
```powershell
$body | Out-File -FilePath $tmpFile -Encoding utf8NoBOM
gh issue create --title $Title --body-file $tmpFile --label $Labels
```

**GitHub Actions에서 한글:**
```yaml
env:
  LANG: ko_KR.UTF-8
  PYTHONIOENCODING: utf-8
  DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION: "1"
```

### 이슈 레이블 체계

```
[팀]              [우선순위]          [타입]
team-a            priority-critical   feat
team-b            priority-high       bug
team-design       priority-medium     docs
coordinator       priority-low        qa-result
team-qa                               ra-update
team-ra                               breaking-change
                                      interface-contract
                                      soup-update
                                      security
```

### 이슈 생성 시점 규칙

| 작업 유형 | 이슈 생성 담당 | 레이블 | 플랫폼 |
|---------|-------------|--------|--------|
| 기능 구현 시작 | 각 구현팀 | `feat` + `team-*` | Gitea + GitHub |
| 버그 발견 | QA / 각 팀 | `bug` + `priority-*` | Gitea |
| Common 인터페이스 변경 | Team A | `breaking-change` | Gitea + GitHub |
| 인터페이스 계약 변경 | Coordinator | `interface-contract` | Gitea + GitHub |
| QA 릴리즈 리포트 | QA | `qa-result` | Gitea |
| 보안 취약점 (CVSS≥7) | QA | `security` + `priority-critical` | Gitea (비공개) |
| SBOM 변경 | QA → RA | `soup-update` | Gitea |
| 규제 문서 업데이트 | RA | `ra-update` | Gitea |
| RTM 추적 단절 | RA | `ra-update` + `priority-high` | Gitea |

### 이슈 스크립트 (`scripts/issue/`)

| 파일 | 용도 |
|------|------|
| `New-Issue.ps1` | 공통 이슈 생성 (UTF-8 안전, Gitea + GitHub 동시) |
| `New-GiteaIssue.ps1` | Gitea API 직접 호출 |
| `New-FeatureIssue.ps1` | 기능 이슈 템플릿 |
| `New-BugIssue.ps1` | 버그 이슈 템플릿 |
| `New-QAResultIssue.ps1` | QA 결과 자동 이슈 |
| `New-RAUpdateIssue.ps1` | RA 문서 업데이트 이슈 |
| `Close-IssueWithComment.ps1` | 이슈 종료 + 한글 코멘트 |
| `Sync-Labels.ps1` | Gitea/GitHub 레이블 동기화 |

### 이슈 템플릿

`.github/ISSUE_TEMPLATE/` 및 `.gitea/issue_template/` 동일 구성:

| 템플릿 | 용도 |
|--------|------|
| `feature.md` | 기능 구현 요청 |
| `bug.md` | 버그 리포트 |
| `qa-result.md` | QA 분석 결과 |
| `ra-update.md` | 규제 문서 업데이트 |

---

## Part 6: 운영 전략 문서 및 README 연결

### `docs/OPERATIONS.md` (신규 생성)

전체 운영 전략을 한 곳에서 참조하는 마스터 문서:
- 섹션 1: 팀 구성 및 역할 (RACI 매트릭스 포함)
- 섹션 2: Worktree 설정 방법
- 섹션 3: 팀별 Claude Context 가이드
- 섹션 4: Gitea/GitHub 이슈 추적 가이드
- 섹션 5: QA 게이트 및 릴리즈 기준
- 섹션 6: RA 문서 유지보수 절차
- 섹션 7: PR 병합 흐름 및 코드 리뷰 정책

### `README.md` 수정 (Line ~1503 — "문서 체계" 섹션 이후)

```markdown
## 개발 운영 전략

팀 기반 Worktree 분리 개발 체계로 운영됩니다.

| 문서 | 내용 |
|------|------|
| [운영 전략 가이드](docs/OPERATIONS.md) | 팀 구성, Worktree, 이슈 추적, QA/RA 기준 |
| [QA 릴리즈 기준](docs/OPERATIONS.md#qa-게이트) | 릴리즈 체크리스트, 자동화 파이프라인 |
| [RA 문서 절차](docs/OPERATIONS.md#ra-문서-유지보수) | 규제 문서 업데이트 트리거 및 흐름 |
```

---

## 구현 순서

### Phase 1: 기반 구축 (즉시 실행)
1. `git worktree add` × 6 (worktree 생성)
2. `.claude/rules/teams/` × 6 (팀별 규칙 파일)
3. `scripts/setup/Init-TeamWorktree.ps1` (CLAUDE.md 자동생성)
4. `CODEOWNERS` (팀별 파일 소유권)
5. `.github/pull_request_template.md` (PR 체크리스트)

### Phase 2: 이슈 추적 인프라 (1주)
1. `scripts/issue/` × 8 (이슈 스크립트, 한글 안전)
2. `.github/ISSUE_TEMPLATE/` × 4 (이슈 템플릿)
3. `.gitea/issue_template/` × 4 (Gitea 동일)
4. `scripts/issue/Sync-Labels.ps1` (레이블 동기화)
5. Gitea + GitHub 레이블 초기 생성 실행

### Phase 3: QA 인프라 (1~2주)
1. `coverage.runsettings` + `.stylecop.json` + `stryker-config.json`
2. `.github/workflows/sonar.yml` + `security-scan.yml` + `qa-issue-reporter.yml`
3. `scripts/qa/` × 3 (릴리즈리포트, 뮤테이션, 보안스캔)
4. `desktop-ci.yml` 확장 (커버리지 수집 단계 추가)

### Phase 4: RA 인프라 (1~2주)
1. **DOC-042 CMP 완성 (최우선!)**
2. `CHANGELOG.md` + `docs/VERSIONING.md`
3. `scripts/ra/` × 3 (SBOM, RTM, 제출패키지)
4. `.github/workflows/docs.yml` (DocFX)

### Phase 5: 디자인팀 환경 (1주)
1. `src/HnVue.UI/DesignTime/` Mock ViewModel 디렉토리
2. DesignTime Mode 분기

### Phase 6: 운영 문서 (최종)
1. `docs/OPERATIONS.md` 작성 (이 플랜 내용 기반)
2. `README.md` Line ~1503 이후 운영 전략 링크 섹션 추가
3. main 브랜치에 커밋 + 푸시

---

## 검증 방법

1. **독립 빌드**: 각 worktree `dotnet build HnVue.sln` → 0 에러
2. **이슈 한글 검증**: `New-Issue.ps1 -Title "한글 이슈 테스트"` → Gitea/GitHub 정상 표시
3. **QA 파이프라인**: `Generate-ReleaseReport.ps1` → DOC-034 10항목 자동 집계
4. **커버리지 게이트**: `dotnet test --settings coverage.runsettings` → 임계값 적용
5. **SonarCloud**: PR에서 품질 게이트 자동 실행
6. **SBOM 자동화**: `Generate-SBOM.ps1` → DOC-019 CycloneDX 갱신
7. **통합 빌드**: main에서 전체 1,135개+ 테스트 통과
8. **README 링크**: `docs/OPERATIONS.md` 경로 클릭 → 정상 열림
9. **Context 최적화**: 팀 전용 CLAUDE.md 토큰 사용량 ≤ 메인 60%

---

## 신규 생성 파일 전체 목록 (48개)

### 팀 Context (7개)
`.claude/rules/teams/team-a.md`, `team-b.md`, `team-design.md`, `coordinator.md`, `qa.md`, `ra.md`
`scripts/setup/Init-TeamWorktree.ps1`

### 이슈 추적 (13개)
`scripts/issue/New-Issue.ps1`, `New-GiteaIssue.ps1`, `New-FeatureIssue.ps1`, `New-BugIssue.ps1`, `New-QAResultIssue.ps1`, `New-RAUpdateIssue.ps1`, `Close-IssueWithComment.ps1`, `Sync-Labels.ps1`
`.github/ISSUE_TEMPLATE/feature.md`, `bug.md`, `qa-result.md`, `ra-update.md`
`.gitea/issue_template/` (4개 동일 — 별도 카운트 시 +4)

### QA 인프라 (9개)
`.github/workflows/sonar.yml`, `security-scan.yml`, `qa-issue-reporter.yml`
`scripts/qa/Generate-ReleaseReport.ps1`, `Invoke-MutationTest.ps1`, `Invoke-SecurityScan.ps1`
`coverage.runsettings`, `stryker-config.json`, `.stylecop.json`

### RA 인프라 (6개)
`.github/workflows/docs.yml`
`scripts/ra/Generate-SBOM.ps1`, `Update-RTM.ps1`, `Package-Submission.ps1`
`docs/VERSIONING.md`, `CHANGELOG.md`

### 운영 문서 + 기타 (5개)
`docs/OPERATIONS.md`, `CODEOWNERS`, `.github/pull_request_template.md`
`src/HnVue.UI/DesignTime/` (디렉토리), `README.md` (수정)
