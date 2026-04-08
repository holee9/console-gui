---
document_id: DOC-050
title: HnVue 팀 기반 워크트리 개발 운영 전략
version: 1.0.0
date: 2026-04-08
classification: Internal
author: Software Development Team
reviewer: Project Manager
approval_date: TBD
---

# HnVue 팀 기반 워크트리 개발 운영 전략

## 개요

이 문서는 HnVue 프로젝트의 멀티팀 워크트리 개발 환경에서 효율적인 협업과 품질 관리를 위한 종합 운영 전략을 정의합니다. 6개 팀이 병렬로 작업하면서 일관된 품질, 규제 준수, 안전성을 유지하는 메커니즘을 상세히 제시합니다.

**대상 독자:** 팀 리더, 각 팀의 개발자, QA 팀, 규제 담당자

---

## 1. 팀 구성 및 역할

### 팀 구조 정의

HnVue 프로젝트는 6개 팀으로 구성되며, 각 팀은 특정 모듈 영역과 책임을 담당합니다.

#### Team A — 인프라 및 기초 계층

**담당 모듈:**
- HnVue.Common (공통 유틸리티, 인터페이스, 기본 타입)
- HnVue.Data (데이터 액세스, EF Core DbContext, 마이그레이션)
- HnVue.Security (인증/인가, 암호화, 권한 관리)
- HnVue.SystemAdmin (시스템 설정, 사용자 관리, 로깅)
- HnVue.Update (소프트웨어 업데이트, 패치 관리)

**테스트 책임:**
- Common.Tests (유틸리티 단위 테스트, 85%+ 커버리지)
- Data.Tests (DB 마이그레이션, ORM 메핑, 데이터 유효성)
- Security.Tests (인증 흐름, 권한 검증, 암호화)
- SystemAdmin.Tests (설정 관리, 사용자 작업)
- Update.Tests (패치 배포, 롤백)
- Architecture.Tests (레이어 경계, 순환 참조 방지)

**주요 책임:**
- NuGet 패키지 업데이트 및 Directory.Packages.props 관리
- DB 스키마 마이그레이션 (EF Core Code-First)
- 공통 인터페이스 변경 시 `breaking-change` 이슈 생성 및 Coordinator 승인 획득
- 보안 취약점 검토 및 CVSS 평가

**변경 기준:**
- 공통 인터페이스 변경: Coordinator 승인 필수
- NuGet 종속성 추가/제거: RA 팀에 SBOM 변경 알림
- 데이터 스키마 변경: 모든 팀에 영향 공지

---

#### Team B — 의료 영상 처리 파이프라인

**담당 모듈:**
- HnVue.Dicom (DICOM 파일 읽기/쓰기, 메타데이터 파싱)
- HnVue.Detector (영상 수집, 센서 통신, 프레임 획득)
- HnVue.Imaging (영상 처리, 필터, 변환, 렌더링)
- HnVue.Dose (방사선량 계산, 누적 추적, 안전 제한)
- HnVue.Incident (사건/사고 로깅, 추적, 분류)
- HnVue.Workflow (워크플로 상태 관리, 프로세스 정의)
- HnVue.PatientManagement (환자 데이터, 검사 스케줄)
- HnVue.CDBurning (CD/DVD 작성, 아카이빙)

**안전성-중요 모듈:** HnVue.Dose, HnVue.Incident
- 정책 DOC-012에 따라 **90%+ Branch 커버리지** 필수
- 모든 코드 변경에 대해 QA 팀 검토 필수
- P1 버그 발견 시 즉시 RA 팀에 문제 보고

**테스트 책임:**
- Dicom.Tests (파일 파싱, 메타데이터 추출)
- Detector.Tests (센서 시뮬레이션, 프레임 획득)
- Imaging.Tests (필터 알고리즘, 렌더링 출력)
- Dose.Tests (계산 검증, 누적 추적, 경계 케이스)
- Incident.Tests (로깅, 분류 로직)
- Workflow.Tests (상태 전이, 유효성 검증)
- PatientManagement.Tests (데이터 무결성)
- CDBurning.Tests (미디어 쓰기, 검증)

**주요 책임:**
- IDetectorService, IWorkflowEngine 인터페이스 변경: Coordinator 승인 필수
- 워크플로 상태 추가/변경: RA 팀에 RTM 이슈 생성
- 방사선량 계산 알고리즘 변경: DOC-049 (IEC 81001-1) 검토
- 안전성-중요 코드: 매월 보안 감시 (SonarCloud, OWASP)

**변경 기준:**
- 워크플로 상태 추가: Workflow RTM 업데이트, RA 이슈 생성
- Dose 모듈 변경: RA 팀 검토, DOC-049 업데이트
- IDetectorService 변경: Coordinator 승인 필수

---

#### Team Design — 순수 UI 디자인

**담당 모듈:**
- HnVue.UI/Views (XAML 뷰, 페이지 정의)
- Styles (스타일 리소스, 테마, 색상 팔레트)
- Themes (Light/Dark 테마, 고대비 옵션)
- Components (재사용 가능한 컨트롤, 커스텀 UI)
- Converters (값 변환기, 포매팅)
- Assets (이미지, 아이콘, 폰트)
- DesignTime (VS2022 디자인 타임 뷰 데이터, Mock ViewModel)

**테스트 책임:**
- HnVue.UI.Tests (XAML 바인딩, 리소스 로딩)
- HnVue.UI.QA.Tests (FlaUI E2E, 접근성, 반응성)

**주요 책임:**
- XAML 컴포넌트 설계 및 구현
- 접근성 준수 (WCAG 2.1 Level AA)
- 반응형 레이아웃 (여러 화면 크기)
- VS2022 디자인 타임 Mock ViewModel 제공
- 아키텍처 테스트 준수 (비즈니스 로직 직접 참조 금지)

**구속 조건:**
- 뷰는 순수 UI 전용: 비즈니스 로직 포함 금지
- ViewModel 참조만 허용 (Service 직접 참조 금지)
- DESIGN_TO_XAML_WORKFLOW.md 5단계 준수
- DesignTime Mock ViewModel으로 VS2022 독립적 렌더링

**변경 기준:**
- 컴포넌트 추가/변경: Coordinator 리뷰 (계약 영향)
- 스타일 전역 변경: Coordinator 승인 (모든 뷰 영향)
- 접근성 관련 변경: QA 팀 검증

---

#### Coordinator — 통합 및 UI 계약 관리

**담당 모듈:**
- HnVue.UI.Contracts (UI 인터페이스, 데이터 계약, 열거형)
- HnVue.UI.ViewModels (MVVM ViewModel, 데이터 바인딩)
- HnVue.App (의존성 주입 루트, App.xaml.cs DI 등록)
- tests.integration (통합 테스트, 팀 간 계약 검증)

**주요 책임:**
- **모든 팀 간 인터페이스 변경의 유일한 승인자**
- ViewModel 서비스 주입 설정 검토 및 승인
- App.xaml.cs DI 컨테이너 구성 관리
- 통합 테스트 작성 및 유지보수 (전체 워크플로 검증)
- UI.Contracts 인터페이스 설계 및 버전 관리

**통합 책임:**
- Team A (Common, Data 변경) → Coordinator 검토 → Team Design 영향 평가
- Team B (Workflow, Dose 변경) → Coordinator 검토 → UI.Contracts 업데이트 필요 여부 판단
- Team Design (UI 컴포넌트) → Coordinator 검토 (계약 변경 여부)

**변경 기준:**
- UI.Contracts 인터페이스 추가/변경: Coordinator만 수행, 모든 팀 동의
- ViewModel 서비스 등록: Coordinator 관리, 구현팀 요청
- 통합 테스트 케이스: Coordinator 작성, 각 팀 협력

---

#### QA Team — 품질 보증

**담당 영역:**
- CI/CD 파이프라인 (.github/workflows/)
- 스크립트 관리 (scripts/ci/, scripts/qa/)
- 커버리지 설정 (coverage.runsettings)
- 정적 분석 (Stryker.NET 뮤테이션 테스트)
- 코드 리뷰 정책 (CODEOWNERS, PR 템플릿)
- 테스트 보고서 생성 (TestReports/)
- 문서 테스트 (docs/testing/)

**테스트 책임:**
- **정적 분석:** SonarCloud 게이트 (Bug=0, 코드 냄새<5, Coverage≥80%)
- **동적 분석:** xUnit 커버리지 (85%+ 목표), Stryker.NET 뮤테이션 (≥80%)
- **보안 분석:** OWASP 스캔 (Critical=0, High≤2)
- **코드 리뷰:** CODEOWNERS 필수 리뷰

**주요 책임:**
- DOC-034 릴리스 체크리스트 10항목 자동 보고
- 모든 PR에 대해 SonarCloud 품질 게이트 강제
- 테스트 커버리지 트렌드 추적 (주간 보고)
- 보안 취약점 추적 (CVSS≥7 즉시 P1 버그 생성)
- FlaUI E2E 테스트 작성 및 유지보수

**릴리스 게이트:**
- V&V 보고서: TRX 결과 ≥ 95% 통과
- 위험 관리: DOC-036 위험 레지스터 완료
- 사이버보안: OWASP 스캔 Critical=0
- 소프트웨어 사용성: FlaUI E2E 통과
- 코드 품질: SonarCloud 게이트 통과

**변경 기준:**
- 커버리지 정책 변경: 프로젝트 매니저 승인
- CI 파이프라인 변경: 모든 팀 동의
- 보안 게이트 추가: RA 팀 협의

---

#### RA Team — 규제 및 컴플라이언스

**담당 영역:**
- 규제 문서 (docs/regulatory/, 16개)
- 계획 문서 (docs/planning/, SRS/SAD/SDS)
- 위험 관리 (docs/risk/, DOC-036 위험 레지스터)
- 검증 및 추적 (docs/verification/, RTM/SOUP)
- 자동화 스크립트 (scripts/ra/)
- DocFX 문서 빌드 (docfx.json)

**우선 작업:**
- **DOC-042 CMP (구성 관리 계획) 완성:** 현재 Draft, 작성자/검토자 미지정
  - IEC 62304 §8.1 필수 요소
  - 목표: 2026년 4월 30일까지 승인
  - 포함 내용: 구성 항목 식별, 변경 제어 프로세스, 버전 관리, 릴리스 절차

**문서 책임:**
- SBOM 자동 생성 (CycloneDX JSON, QA OWASP 검사 후)
- RTM 자동 추적 (변경 이슈 → RTM 행 업데이트)
- SOUP (Software of Uncertain Pedigree) 식별 및 평가
- DocFX 문서 빌드 (API 문서 + 사용자 설명서)

**변경 기준:**
- NuGet 추가/제거: DOC-019 SBOM + DOC-033 SOUP 업데이트
- 인터페이스 변경: DOC-005 SRS + DOC-032 RTM 업데이트
- 워크플로 상태 추가: DOC-004 FRS + DOC-032 RTM 업데이트
- 보안 기능 변경: DOC-049 IEC 81001 + DOC-045 VEX 업데이트
- P1 버그 수정: DOC-044 Known Anomalies 업데이트

**규제 일정:**
- IEC 62304 Green Phase: 2026년 5월 15일
- FDA 510(k) 제출 준비: 2026년 6월 30일

---

### RACI 매트릭스

다음 표는 주요 활동에 대한 책임 배분을 정의합니다:

| 활동 | Team A | Team B | Design | Coordinator | QA | RA |
|------|--------|--------|--------|------------|----|----|
| 공통 인터페이스 변경 | **R** | C | C | **A** | I | I |
| DB 마이그레이션 | **R/A** | I | - | C | C | C |
| UI.Contracts 변경 | C | C | **C** | **R/A** | I | I |
| ViewModel 서비스 등록 | - | I | - | **R/A** | - | - |
| 안전성-중요 코드 변경 | - | **R** | - | C | **A** | **A** |
| 릴리스 준비 보고서 | C | C | C | I | **R/A** | I |
| 규제 문서 업데이트 | I | C | - | I | **C** | **R/A** |
| 설계 리뷰 게이트 | - | I | **R** | **A** | I | - |
| E2E 테스트 검증 | - | C | **C** | I | **R/A** | - |
| 워크플로 상태 변경 | - | **R** | - | I | C | **A** |
| 보안 취약점 수정 | **R** | **R** | - | C | **A** | I |
| 통합 테스트 작성 | - | I | I | **R/A** | C | - |

**범례:** R=담당, A=승인, C=협의, I=정보 제공, -=관련 없음

---

## 2. Worktree 설정 방법

### 디렉토리 구조

HnVue 프로젝트는 7개 워크트리를 사용하여 팀별 병렬 개발을 지원합니다:

```
D:\workspace-gitea\
├── Console-GUI\                ← main 브랜치 (릴리즈 통합)
├── Console-GUI-TeamA\          ← team-a/infra 브랜치
├── Console-GUI-TeamB\          ← team-b/domain 브랜치
├── Console-GUI-Design\         ← team-design/ui 브랜치
├── Console-GUI-Coord\          ← coordinator/contracts 브랜치
├── Console-GUI-QA\             ← qa/infrastructure 브랜치
└── Console-GUI-RA\             ← ra/documentation 브랜치
```

각 워크트리는 독립적인 git 브랜치에 연결되며, 팀 간 파일 충돌을 방지합니다.

### 워크트리 생성 명령어

**초기 설정 (프로젝트 리더만 실행):**

```bash
cd D:\workspace-gitea\Console-GUI

# Team A 워크트리 생성
git worktree add ../Console-GUI-TeamA -b team-a/infra

# Team B 워크트리 생성
git worktree add ../Console-GUI-TeamB -b team-b/domain

# Design 워크트리 생성
git worktree add ../Console-GUI-Design -b team-design/ui

# Coordinator 워크트리 생성
git worktree add ../Console-GUI-Coord -b coordinator/contracts

# QA 워크트리 생성
git worktree add ../Console-GUI-QA -b qa/infrastructure

# RA 워크트리 생성
git worktree add ../Console-GUI-RA -b ra/documentation
```

**워크트리 상태 확인:**

```bash
git worktree list
```

**기존 워크트리 정리:**

```bash
# 손상된 워크트리 제거
git worktree remove ../Console-GUI-TeamA

# 모든 손상된 참조 정리
git worktree prune
```

### 브랜치 전략

**브랜치 명명 규칙:**

- `team-{name}/{area}`: 팀 개발 브랜치
  - `team-a/infra`: Team A 인프라 개발
  - `team-b/domain`: Team B 의료 영상 개발
  - `team-design/ui`: Design 팀 UI 개발
  - `coordinator/contracts`: Coordinator 통합 개발
  - `qa/infrastructure`: QA 파이프라인 개발
  - `ra/documentation`: RA 문서 개발

- `feature/SPEC-{id}`: SPEC 기반 기능 개발
  - `feature/SPEC-001-dose-calculator`: Dose 계산기 구현
  - `feature/SPEC-002-workflow-state`: 워크플로 상태 추가

- `bugfix/{name}`: 버그 수정
  - `bugfix/dose-rounding-error`
  - `bugfix/workflow-deadlock`

**브랜치 생명주기:**

1. 팀 브랜치에서 작업 (3-4주)
2. 준비 완료 시 main으로 PR 생성
3. Coordinator 리뷰 → QA 게이트 → QA 코드 리뷰
4. 승인 후 main에 병합
5. 병합 후 팀 브랜치 동기화

### PR 병합 흐름

**구현 → Coordinator 리뷰 → QA 자동 게이트 → QA 코드 리뷰 → main 병합**

```
구현팀 PR 생성
    ↓
[필수] Coordinator 리뷰
    • UI.Contracts 변경 검증
    • 인터페이스 계약 확인
    • ViewModel 주입 검증
    ↓
[자동] QA 게이트
    • SonarCloud 품질 게이트
    • 커버리지 임계값 (80%)
    • 보안 스캔
    ↓
[필수] QA 코드 리뷰
    • 테스트 품질 검증
    • 아키텍처 준수 확인
    • 성능 영향 평가
    ↓
[자동] main 병합
```

**PR 필수 조건:**

- Build 성공
- Unit 테스트 생성/수정
- 커버리지 임계값 유지 (85%)
- SonarCloud 품질 게이트 통과
- 아키텍처 규칙 위반 없음 (NetArchTest)
- API 문서 업데이트 (변경 시)
- 커밋 메시지: Conventional Commits 준수

**Conventional Commits 형식:**

```
<type>(<scope>): <subject>

<body>

<footer>
```

예:
```
feat(dose): Add cumulative dose tracking

Implements cumulative dose tracking for safety limits.

Closes #1234
Breaking-change: IDetector interface requires DoseTracker injection
```

---

## 3. 팀별 Claude Context 가이드

### Claude 컨텍스트 전략

각 팀의 워크트리 브랜치에는 **팀 특화 CLAUDE.md**가 있어서 해당 팀에 필요한 스킬과 규칙만 로드합니다. 이를 통해:

- 토큰 예산 30% 절약
- 팀 포커스 유지
- 불필요한 정보 제거

### 팀별 컨텍스트 설정

| 팀 | CLAUDE.md 포커스 | 규칙 파일 | 로드 스킬 | 제외 스킬 |
|----|--------------|---------|---------|---------  |
| **Team A** | 인프라 (Common, Data, Security) | teams/team-a.md | moai-lang-csharp, moai-domain-database, moai-foundation-quality | moai-domain-uiux, moai-dicom |
| **Team B** | 의료 영상 (Dicom, Workflow, Dose) | teams/team-b.md | moai-lang-csharp, moai-foundation-quality, moai-domain-medical | moai-domain-uiux, moai-domain-database |
| **Design** | UI (Views, Themes, Components) | teams/team-design.md | moai-domain-uiux, moai-lang-xaml, moai-accessibility | moai-domain-database, moai-domain-backend |
| **Coordinator** | 통합 (Contracts, VM, App) | teams/coordinator.md | moai-lang-csharp, moai-foundation-quality | 단일 모듈 세부사항 |
| **QA** | 품질 (CI, 분석, 보고서) | teams/qa.md | moai-foundation-quality, moai-workflow-testing, moai-ci-github | 구현 세부사항 |
| **RA** | 규제 (문서, RTM, SBOM) | teams/ra.md | moai-workflow-jit-docs, moai-regulatory-compliance | 구현, UI |

### 팀 CLAUDE.md 초기화 스크립트

**자동 생성 스크립트 사용:**

```powershell
scripts/setup/Init-TeamWorktree.ps1 -TeamName "team-a"
scripts/setup/Init-TeamWorktree.ps1 -TeamName "team-b"
scripts/setup/Init-TeamWorktree.ps1 -TeamName "team-design"
scripts/setup/Init-TeamWorktree.ps1 -TeamName "coordinator"
scripts/setup/Init-TeamWorktree.ps1 -TeamName "qa"
scripts/setup/Init-TeamWorktree.ps1 -TeamName "ra"
```

이 스크립트는:
1. 팀별 `.claude/rules/teams/{team-name}.md` 생성
2. 팀 특화 CLAUDE.md 생성
3. 토큰 사용량 분석 (메인 CLAUDE.md 대비 60% 이하)
4. 로드된 스킬 목록 출력

### Team A 컨텍스트 구성 (예시)

```yaml
# D:\workspace-gitea\Console-GUI-TeamA\.claude\rules\teams\team-a.md
---
team: Team A
focus: Infrastructure & Foundation
modules:
  - HnVue.Common
  - HnVue.Data
  - HnVue.Security
  - HnVue.SystemAdmin
  - HnVue.Update
---

# 로드 스킬
skills:
  - moai-lang-csharp
  - moai-domain-database
  - moai-foundation-quality

# 제외 영역
excluded_domains:
  - UI (XAML, WPF, 컨트롤)
  - DICOM (HnVue.Dicom, 영상 포맷)
  - 규제 (RA 문서)

# 팀 특화 규칙
rules:
  - NuGet 버전 관리: Directory.Packages.props 중앙 관리
  - DB 마이그레이션: EF Core Code-First, 모든 팀 공지
  - 공통 인터페이스: Coordinator 승인 필수
```

### 토큰 예산 최적화

**메인 CLAUDE.md:** ~15K 토큰
- 모든 팀 정보, 모든 스킬 로드

**팀별 CLAUDE.md:** ~9K 토큰 (60% 절감)
- 팀 관련 정보만
- 팀 관련 스킬만
- 불필요한 도메인 제외

**예시 계산:**
- 메인 CLAUDE.md: 15K 토큰
- Team A CLAUDE.md: 9K 토큰 (40% 절감)
- 절감: 6K 토큰 = 전체 예산 (200K) 3% 절약

---

## 4. Gitea/GitHub 이슈 추적 가이드

### 듀얼 저장소 관리

HnVue는 두 개의 Git 저장소를 병렬 관리합니다:

**Gitea (내부):**
- URL: http://10.11.1.40:7001
- 용도: 모든 개발 이슈, 버그, 내부 QA, 규제 문서
- 공개 범위: 조직 내부
- 특성: 빠른 이슈 추적, 상세한 메타데이터

**GitHub (외부):**
- URL: github.com/company/hnvue
- 용도: 공개 이슈, 릴리스 노트, CI/CD 파이프라인
- 공개 범위: 공개 (오픈소스 단계별)
- 특성: 커뮤니티 피드백, 공식 릴리스

### Windows에서 한글 문자 안전성

Windows 환경에서 한글 문자가 제대로 인코딩되도록 설정:

**PowerShell에서:**

```powershell
# 콘솔 출력 인코딩 설정
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Git 설정
git config --global core.safecrlf false
git config --global core.quotepath false
```

**Git Bash에서:**

```bash
export LANG=ko_KR.UTF-8
export LC_ALL=ko_KR.UTF-8
git config --global core.quotepath false
```

**VS Code에서:**

settings.json에 추가:
```json
{
  "terminal.integrated.shellArgs.windows": ["-NoExit", "-Command", 
    "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8"],
  "terminal.integrated.env.windows": {
    "LANG": "ko_KR.UTF-8"
  }
}
```

### 레이블 스키마

**팀 레이블:**
- `team-a`: Team A 인프라 작업
- `team-b`: Team B 의료 영상 작업
- `team-design`: Design 팀 UI 작업
- `coordinator`: Coordinator 통합 작업
- `team-qa`: QA 팀 품질 작업
- `team-ra`: RA 팀 규제 작업

**우선순위 레이블:**
- `priority-critical`: 긴급 (1일 이내 처리)
- `priority-high`: 높음 (3일 이내)
- `priority-medium`: 중간 (1주일 이내)
- `priority-low`: 낮음 (계획 대기)

**유형 레이블:**
- `feat`: 신규 기능
- `bug`: 버그 수정
- `docs`: 문서 작업
- `qa-result`: QA 결과 보고
- `ra-update`: 규제 문서 업데이트
- `breaking-change`: 호환성 깨지는 변경
- `interface-contract`: UI.Contracts 변경
- `soup-update`: SOUP (Software of Uncertain Pedigree) 업데이트
- `security`: 보안 취약점

### 이슈 생성 타이밍 및 규칙

| 이벤트 | 담당 팀 | 레이블 | Gitea | GitHub | 설명 |
|--------|--------|--------|-------|--------|------|
| 기능 개발 시작 | 각 팀 | feat, team-* | ✓ | ✓ | 기능 구현 전 이슈 생성 |
| 버그 발견 | QA/팀 | bug, priority-* | ✓ | △* | P1=Critical은 GitHub 미게시 |
| 공통 인터페이스 변경 | Team A | breaking-change, team-a | ✓ | ✓ | 영향도 분석 필수 |
| 인터페이스 계약 변경 | Coordinator | interface-contract | ✓ | ✓ | 모든 팀 동의 필수 |
| QA 릴리스 보고 | QA | qa-result | ✓ | - | 내부 보고서 |
| 보안 취약점 (CVSS≥7) | QA | security, priority-critical | ✓ | - | Private 이슈 처리 |
| SBOM 변경 | QA → RA | soup-update | ✓ | - | NuGet 추가/제거 시 |
| 규제 문서 업데이트 | RA | ra-update | ✓ | - | 내부 문서 |
| RTM 추적 갭 | RA | ra-update, priority-high | ✓ | - | 검증 이슈 |

*GitHub에 P1/보안 이슈 게시 시 회사 정책 협의 필수

### 이슈 생성 스크립트

**공통 이슈 생성:**

```powershell
scripts/issue/New-Issue.ps1 `
  -Title "팀 A: NuGet 업데이트" `
  -Team "team-a" `
  -Priority "medium" `
  -Repo "gitea"
```

**Gitea 이슈 생성:**

```powershell
scripts/issue/New-GiteaIssue.ps1 `
  -Title "팀 B: 워크플로 상태 추가" `
  -Labels "team-b,interface-contract" `
  -Assignee "team-b-lead"
```

**기능 이슈 생성 (SPEC 연동):**

```powershell
scripts/issue/New-FeatureIssue.ps1 `
  -SPEC "SPEC-001" `
  -Title "Dose 계산기 구현" `
  -Team "team-b" `
  -Assignee "dose-engineer"
```

**버그 이슈 생성:**

```powershell
scripts/issue/New-BugIssue.ps1 `
  -Title "Dose 반올림 오류" `
  -Priority "critical" `
  -Team "team-b" `
  -Steps @("1. 환자 선택", "2. 검사 실행", "3. 방사선량 확인")
```

**QA 결과 보고:**

```powershell
scripts/issue/New-QAResultIssue.ps1 `
  -Title "릴리스 v2.0 QA 보고서" `
  -Coverage 85.2 `
  -SonarGate "PASSED" `
  -RiskReport "완료" `
  -Recommendation "릴리스 가능"
```

**RA 업데이트 이슈:**

```powershell
scripts/issue/New-RAUpdateIssue.ps1 `
  -Title "DOC-032 RTM 업데이트: 워크플로 상태 추가" `
  -Document "DOC-032" `
  -Impact "Workflow 모듈 변경"
```

**이슈 종료 및 댓글:**

```powershell
scripts/issue/Close-IssueWithComment.ps1 `
  -IssueNumber 1234 `
  -Comment "Team A에서 완료했습니다. PR #5678 참고." `
  -Repo "gitea"
```

**레이블 동기화:**

```powershell
# Gitea와 GitHub 레이블 일관성 유지
scripts/issue/Sync-Labels.ps1 `
  -SourceRepo "gitea" `
  -TargetRepo "github"
```

### 이슈 추적 워크플로우

**이슈 생성 → 팀 할당 → 구현 → PR → 병합 → 이슈 종료**

```
1. 이슈 생성
   ├─ 적절한 팀 레이블 추가
   ├─ 우선순위 지정
   └─ 설명에 기술 요구사항 포함

2. 팀 할당
   ├─ 담당 엔지니어 지정
   ├─ 예상 완료일 설정
   └─ 의존성 표시

3. 구현
   ├─ 브랜치 생성: feature/SPEC-XXX 또는 team-name/issue-name
   ├─ 커밋: Conventional Commits 준수
   └─ PR 작성: 이슈 번호 참조

4. PR 리뷰
   ├─ Coordinator 리뷰 (인터페이스)
   ├─ QA 자동 게이트 (SonarCloud, 커버리지)
   └─ QA 코드 리뷰

5. 병합
   ├─ main에 병합
   └─ 팀 브랜치 동기화

6. 이슈 종료
   ├─ "Closes #XXXX" 커밋으로 자동 종료
   └─ 또는 수동으로 Close + 댓글
```

---

## 5. QA 게이트 및 릴리스 기준

### 릴리스 준비도 평가

HnVue는 DOC-034 릴리스 체크리스트(10개 항목)와 DOC-011 V-Model에 따라 릴리스 준비도를 평가합니다.

### 자동 릴리스 보고서

**생성 스크립트:**

```powershell
scripts/qa/Generate-ReleaseReport.ps1 `
  -TargetVersion "2.0" `
  -OutputFormat "html"
```

### DOC-034 릴리스 체크리스트 (10개 항목)

| # | 항목 | 자동 소스 | 목표값 | 필수 여부 | 현재 상태 |
|----|------|---------|--------|---------|---------|
| 1 | V&V 보고서 통과 | TRX 테스트 결과 | ≥95% 통과 | **필수** | ✓ Pass |
| 2 | 위험 관리 보고서 완료 | DOC-036 이슈 상태 | Complete | **필수** | ✓ Complete |
| 3 | 사이버보안 테스트 통과 | OWASP 스캔 결과 | Critical=0 | **필수** | ✓ Pass |
| 4 | 사용성 테스트 통과 | FlaUI E2E 결과 | ≥95% 통과 | **필수** | ✓ Pass |
| 5 | QA 검증 통과 | SonarCloud 게이트 | Passed | **필수** | ✓ Passed |
| 6 | SBOM 확인 | CycloneDX JSON 존재 | Confirmed | **필수** | ✓ Confirmed |
| 7 | 모든 결함 해결 | bugs.json + Gitea | P1=0 | **필수** | ✓ P1=0 |
| 8 | 릴리스 노트 작성 | CHANGELOG.md 최신 항목 | Confirmed | 권장 | ✓ Confirmed |
| 9 | IFU (사용 설명서) 완료 | DOC-040 존재 | Confirmed | **필수** | ✓ Confirmed |
| 10 | 코드 서명 완료 | 빌드 아티팩트 서명 | Confirmed | **필수** | ✓ Confirmed |

### 릴리스 판정

**판정 기준:**

```
모든 필수 항목이 Pass
├─ Green (릴리스 OK): 진행
├─ Yellow (조건부 릴리스): 승인자 동의 필수
└─ Red (릴리스 차단): 완료 필수
```

**판정 로직:**

- **Green:** 모든 필수 항목 ✓, 권장 항목 ≥80%
- **Yellow:** 필수 항목 1-2개 Warning, 나머지 ✓
- **Red:** 필수 항목 ≥3개 Failed, 또는 P1 버그 존재

### 릴리스 승인 서명 게이트

**4명 서명 필수 (DOC-034 §5):**

1. **SW 개발 리더:** 기술 검토
2. **QA 리더:** 품질 검증
3. **RA/QA 매니저:** 규제 준수 확인
4. **프로젝트 매니저:** 최종 승인

**서명 프로세스:**

```powershell
scripts/qa/Sign-ReleaseApproval.ps1 `
  -Version "2.0" `
  -SignerRole "SW-Dev-Lead" `
  -Signature "John-Smith" `
  -Comment "기술 검토 완료, 릴리스 준비됨"
```

**서명 상태 확인:**

```powershell
scripts/qa/Get-ReleaseApprovalStatus.ps1 -Version "2.0"
```

출력:
```
Release v2.0 Approval Status:
  ✓ SW Dev Lead: Signed (2026-04-05)
  ✓ QA Lead: Signed (2026-04-06)
  - RA/QA Manager: Pending
  - Project Manager: Pending
  
Status: 2/4 Signed (50%)
```

### 릴리스 체크리스트 상태 추적

**각 체크리스트 항목의 자동 수집:**

| 항목 | 수집 방법 | 업데이트 빈도 | 담당 팀 |
|------|---------|-------------|--------|
| V&V 보고서 | TRX 빌드 결과 파일 파싱 | 빌드 후 | QA |
| 위험 관리 | Gitea DOC-036 이슈 상태 | 이슈 업데이트 시 | RA |
| 사이버보안 | OWASP 스캔 API | 야간 스캔 | QA |
| 사용성 테스트 | FlaUI E2E 결과 | 회귀 테스트 | QA |
| 코드 품질 | SonarCloud API | PR 병합 시 | QA |
| SBOM | CycloneDX JSON 생성 | 빌드 시 | QA/RA |
| 결함 추적 | bugs.json 개수 | 이슈 업데이트 | QA |
| 릴리스 노트 | CHANGELOG.md 파싱 | 수동 업데이트 | 프로젝트 관리 |
| IFU | docs/user-guides/DOC-040 존재 확인 | 문서 수정 시 | RA |
| 코드 서명 | Build artifact 메타데이터 | 빌드 배포 시 | CI/CD |

### 릴리스 진행 일정

**목표 릴리스 v2.0:**

| 날짜 | 마일스톤 | 소유자 | 상태 |
|------|---------|--------|------|
| 2026-04-30 | DOC-042 CMP 완성 | RA | ⏳ In Progress |
| 2026-05-15 | IEC 62304 Green Phase | RA/QA | ⏳ Planning |
| 2026-05-30 | 최종 V&V 완료 | QA | ⏳ Planning |
| 2026-06-10 | 릴리스 서명 완료 | Project Mgmt | ⏳ Planning |
| 2026-06-15 | GitHub Release 게시 | 프로젝트 관리 | ⏳ Planning |

---

## 6. RA 문서 유지보수 절차

### 구현 변경 → RA 문서 맵핑

팀별 구현 변경이 어떤 규제 문서를 트리거하는지 정의:

| 이벤트 | 영향받는 문서 | 트리거 | 담당 팀 → 수행 팀 |
|--------|------------|--------|-----------------|
| NuGet 추가/제거 | DOC-019 SBOM + DOC-033 SOUP | QA OWASP 스캔 → 문제 발견 | QA → RA |
| 인터페이스 계약 변경 | DOC-005 SRS + DOC-032 RTM | Coordinator → 변경 요청 | Coordinator → RA |
| 워크플로 상태 추가/제거 | DOC-004 FRS + DOC-032 RTM | Team B → 변경 구현 | Team B → RA |
| 보안 기능 변경 | DOC-049 IEC 81001 + DOC-045 VEX | Team A 구현 → 보안 검토 | Team A → RA |
| P1 버그 수정 | DOC-044 Known Anomalies | QA 버그 발견 | QA → RA |
| 아키텍처 변경 | DOC-006 SAD + DOC-007 SDS | Coordinator 설계 | Coordinator → RA |

### RA 자동화 스크립트

**SBOM 자동 생성:**

```powershell
scripts/ra/Generate-SBOM.ps1 `
  -OutputFormat "cyclonedx" `
  -OutputPath "docs/regulatory/sbom.json"
```

**RTM 자동 업데이트:**

```powershell
scripts/ra/Update-RTM.ps1 `
  -SourceIssue "Gitea #1234" `
  -DocumentID "DOC-032" `
  -TraceabilityType "verification" `
  -Status "verified"
```

**제출 패키징:**

```powershell
scripts/ra/Package-Submission.ps1 `
  -Version "2.0" `
  -DestinationFormat "FDA-510k" `
  -OutputPath "submission-v2.0/"
```

### DocFX 자동 빌드

**문서 빌드 자동화:**

```bash
# .github/workflows/docs.yml에 설정됨
# main 브랜치 변경 시 자동 실행

docfx build docfx.json

# 출력:
# - API 문서 (SRC 코드 기반)
# - 사용자 설명서 (docs/user-guides/)
# - 규제 문서 (docs/regulatory/)
```

### 우선 작업: DOC-042 CMP 완성

**현재 상태:** Draft (작성자/검토자 미지정)

**완성 필요 항목:**

1. **구성 항목 식별 (§8.1.1)**
   - 소스 코드 (src/, tests/)
   - 컴파일된 이진 파일 (.exe, .dll)
   - 테스트 케이스 (test artifacts)
   - 문서 (API, 사용자 설명서, 규제)
   - 도구 및 라이브러리 (NuGet packages)

2. **변경 제어 프로세스 (§8.1.2)**
   - 변경 요청 수락 기준
   - 영향도 분석 (Impact Assessment)
   - 승인 권한 (Team별 책임)
   - 구현 및 검증

3. **버전 관리 (§8.1.3)**
   - Git 브랜치 전략 (team-*, feature/*, main)
   - 태그 규칙 (v2.0.1, YYMM.D.H)
   - 릴리스 번호 체계

4. **릴리스 절차 (§8.1.4)**
   - 빌드 자동화 (.github/workflows/build.yml)
   - 아티팩트 저장소 (GitHub Releases, 내부)
   - 서명 및 검증
   - 배포 절차

**완성 일정:**

- **2026-04-15:** CMP 초안 작성자 지정
- **2026-04-25:** CMP 내용 완성
- **2026-04-30:** 검토자 및 승인자 서명 완료

---

## 7. PR 병합 흐름 및 코드 리뷰 정책

### PR 병합 흐름도

```
구현팀 PR 생성
    ↓
    [필수 리뷰어: Coordinator]
    • UI.Contracts 변경 검증
    • ViewModel 주입 검증
    • 인터페이스 계약 확인
    ↓ (Coordinator 승인)
    [자동 게이트: SonarCloud]
    • 품질 게이트 검사
    • 커버리지 임계값 (80%)
    • 보안 스캔
    • 코드 냄새 검사
    ↓ (모든 게이트 통과)
    [필수 리뷰어: QA]
    • 테스트 품질 검증
    • 아키텍처 규칙 준수
    • 성능 영향 평가
    ↓ (QA 승인)
    [자동 병합: main]
    • GitHub 자동 병합 활성화
    • 커밋 메시지 보존
    ↓
    [자동 정리]
    • PR 브랜치 삭제
    • 팀 브랜치 동기화
```

### CODEOWNERS 정책

**.github/CODEOWNERS 정의:**

```
# Team A 인프라
src/HnVue.Common/** @team-a-lead @team-a-member1
src/HnVue.Data/** @team-a-lead @team-a-member2
src/HnVue.Security/** @team-a-lead @team-a-member3
src/HnVue.SystemAdmin/** @team-a-lead
src/HnVue.Update/** @team-a-lead

# Team B 의료 영상
src/HnVue.Dicom/** @team-b-lead @team-b-member1
src/HnVue.Detector/** @team-b-lead @team-b-member2
src/HnVue.Imaging/** @team-b-lead @team-b-member3
src/HnVue.Dose/** @team-b-lead @team-b-member1
src/HnVue.Incident/** @team-b-lead @team-b-member2
src/HnVue.Workflow/** @team-b-lead @team-b-member3
src/HnVue.PatientManagement/** @team-b-lead
src/HnVue.CDBurning/** @team-b-lead

# Design 팀 UI
src/HnVue.UI/Views/** @team-design-lead @team-design-member1
src/HnVue.UI/Styles/** @team-design-lead @team-design-member2
src/HnVue.UI/Themes/** @team-design-lead
src/HnVue.UI/Components/** @team-design-lead @team-design-member1
src/HnVue.UI/Converters/** @team-design-lead
src/HnVue.UI/Assets/** @team-design-lead @team-design-member2
src/HnVue.UI/DesignTime/** @team-design-lead @team-design-member1

# Coordinator 통합
src/HnVue.UI.Contracts/** @coordinator-lead @coordinator-member1
src/HnVue.UI.ViewModels/** @coordinator-lead @coordinator-member2
src/HnVue.App/** @coordinator-lead
tests/HnVue.Tests.Integration/** @coordinator-lead

# QA 파이프라인
.github/workflows/** @qa-lead @qa-member1
scripts/ci/** @qa-lead @qa-member2
scripts/qa/** @qa-lead
*.runsettings @qa-lead
stryker-config.json @qa-lead
CODEOWNERS @qa-lead

# RA 규제 문서
docs/regulatory/** @ra-lead @ra-member1
docs/planning/** @ra-lead @ra-member2
docs/verification/** @ra-lead @ra-member1
docfx.json @ra-lead
scripts/ra/** @ra-lead
```

**필수 리뷰어:**

- **모든 PR:** QA (코드 품질, 테스트)
- **인터페이스 변경 PR:** Coordinator (계약)
- **Team별 모듈:** 해당 팀 리더 (기술 검토)

### PR 체크리스트

**.github/pull_request_template.md:**

```markdown
## PR 설명

### 변경사항
- [ ] 기능 추가
- [ ] 버그 수정
- [ ] 성능 개선
- [ ] 리팩토링
- [ ] 문서 업데이트

### 링크된 이슈
Closes #(이슈 번호)

### 변경된 모듈
- [ ] HnVue.Common
- [ ] HnVue.Data
- [ ] HnVue.Security
- [ ] (기타)

---

## 테스트 완료

- [ ] 새로운 Unit 테스트 작성
- [ ] 기존 테스트 업데이트
- [ ] 로컬 빌드 성공 확인
- [ ] 로컬 테스트 85%+ 커버리지 확인

## 품질 검증

- [ ] Build 성공 (AppVeyor/GitHub Actions)
- [ ] SonarCloud 품질 게이트 통과
- [ ] 아키텍처 규칙 위반 없음 (NetArchTest)
- [ ] 보안 스캔 통과 (OWASP)

## 문서 업데이트

- [ ] API 문서 추가 (변경 시)
- [ ] README 업데이트 (주요 기능)
- [ ] 커밋 메시지 Conventional Commits 준수

---

## Reviewer 체크리스트

### Coordinator 리뷰 (필수)
- [ ] UI.Contracts 변경 검증
- [ ] ViewModel 서비스 주입 검증
- [ ] 통합 테스트 추가

### QA 리뷰 (필수)
- [ ] 테스트 코드 품질 검증
- [ ] 아키텍처 준수 확인
- [ ] 성능 영향 없음 확인

---

## 승인 상태

- Coordinator: ⬜ Pending
- QA Lead: ⬜ Pending
```

### Conventional Commits 형식

**커밋 메시지 규칙:**

```
<type>(<scope>): <subject>

<body>

<footer>
```

**타입:**
- `feat`: 새로운 기능
- `fix`: 버그 수정
- `test`: 테스트 추가/수정
- `docs`: 문서 추가/수정
- `style`: 코드 스타일 (포매팅, 들여쓰기)
- `refactor`: 코드 리팩토링 (기능 변화 없음)
- `perf`: 성능 개선
- `ci`: CI/CD 파이프라인 변경
- `chore`: 기타 작업 (의존성 업데이트 등)

**스코프:**
- 영향받는 모듈 (Common, Data, Dicom, Dose, Workflow, etc.)

**주제:**
- 최대 50자
- 명령형 현재형 ("change", "add", "fix", not "changed", "adds", "fixed")

**본문:**
- 변경 이유와 방식 설명
- 최대 72자 줄 길이

**푸터:**
- Breaking-change 선언
- 이슈 참조

**예시:**

```
feat(dose): Add cumulative dose tracking

Implements cumulative dose tracking feature to meet safety requirement SR-004.
Tracks total dose per patient session and enforces predefined limits.

Added DoseTracker class with:
- Session-based accumulation
- Real-time limit enforcement
- Daily/weekly/annual caps

Closes #1234

Breaking-change: IDetector interface requires DoseTracker injection in constructor
```

```
fix(workflow): Resolve deadlock in state transition

Fixed deadlock condition in WorkflowEngine when concurrent state changes occurred.
Added mutex protection for state machine updates.

Root cause: Multiple threads updating WorkflowState.Current simultaneously
Solution: Single-threaded state update with queued change requests

Closes #5678
```

```
test(dose): Add boundary condition tests for dose calculation

Added comprehensive boundary tests for DoseCalculator:
- Zero dose input
- Maximum safe dose
- Negative dose (invalid)
- Rounding precision

Improves branch coverage from 82% to 91%

Related #1234
```

### PR 자동 병합 설정

**.github/workflows/auto-merge.yml:**

```yaml
name: Auto Merge PR

on:
  pull_request_review:
    types: [submitted]

jobs:
  auto-merge:
    runs-on: ubuntu-latest
    if: |
      github.event.review.state == 'APPROVED' &&
      contains(github.event.pull_request.labels.*.name, 'auto-merge') &&
      github.event.pull_request.user.login != 'dependabot[bot]'
    
    steps:
      - name: Check PR status
        run: |
          # Coordinator 승인 확인
          # QA 승인 확인
          # SonarCloud 게이트 통과 확인
          # 자동 병합 실행
      
      - name: Auto merge
        uses: pascalgn/automerge-action@v0.14.0
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          MERGE_LABELS: "auto-merge"
          MERGE_METHOD: "squash"
          MERGE_COMMIT_MESSAGE: "pull-request-title"
          MERGE_RETRIES: 6
```

---

## 결론

이 운영 전략은 HnVue 팀이 6개 팀으로 병렬 개발하면서 일관된 품질, 규제 준수, 안전성을 유지하는 프레임워크를 제공합니다.

**핵심 성공 요소:**

1. **명확한 팀 책임:** 각 팀의 모듈, 테스트, 변경 기준 정의
2. **효율적인 워크트리:** 팀별 독립적 브랜치로 병렬 개발
3. **자동화 게이트:** SonarCloud, 커버리지, 보안 스캔 자동화
4. **규제 준수:** RA 문서 자동 동기화, RTM 추적
5. **코드 리뷰:** CODEOWNERS, Conventional Commits, 다단계 리뷰

---

## 부록 A: 자주 묻는 질문 (FAQ)

**Q: 팀 간 모듈 변경이 필요하면?**
A: Coordinator를 통해 변경 요청 제출. Coordinator가 영향도 분석 후 타팀에 공지. PR은 Coordinator 승인 후 진행.

**Q: 긴급 버그 수정은?**
A: P1 버그는 priority-critical 레이블 추가. QA → Team 담당 팀 → PR 생성. Coordinator 리뷰 생략 가능하나 사후 보고 필수.

**Q: 릴리스는 언제?**
A: DOC-034 릴리스 체크리스트 10항목 모두 Pass일 때만. QA 리더 + RA 리더 + 프로젝트 매니저 3명 서명 필수.

**Q: 규제 문서 변경은?**
A: RA 팀만 수정 가능. 구현팀이 변경 시 Gitea에 ra-update 이슈 생성. RA 팀이 검토 후 문서 수정.

---

Version: 1.0.0
Classification: Internal
Approved by: TBD
Last Modified: 2026-04-08
