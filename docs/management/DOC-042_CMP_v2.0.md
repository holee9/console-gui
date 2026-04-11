# 형상관리 계획서 (Configuration Management Plan)

| 항목 | 내용 |
|------|------|
| **문서 번호** | DOC-042 |
| **버전** | v2.0 |
| **작성일** | 2026-04-08 |
| **승인일** | 2026-04-11 |
| **작성자** | drake.lee |
| **검토자** | SW Dev Lead |
| **승인자** | PM |
| **상태** | Approved |
| **제품** | HnVue Console SW (HnVue) |
| **회사** | HnVue (가칭) |
| **분류** | 최소 필수 |
| **적용 시장** | FDA 510(k) / MFDS 2등급 / EU MDR Class IIa |
| **근거 규격** | IEC 62304:2006+AMD1:2015 §8.1, §8.3, FDA SW Guidance (2019) Section IV-G, ISO 13485:2016 §4.2.3, §7.3.8 |
| **IEC 62304 Class** | B (Basic Level) |

---

## 변경 이력 (Revision History)

| 버전 | 일자 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| v1.0 | 2026-03-31 | 최초 작성 (개발 착수 전 계획서) | drake.lee |
| v1.1 | 2026-04-08 | 정적 분석 도구 변경 반영: SonarCloud → 로컬 Roslyn 분석기 인프라 전환 (StyleCop.Analyzers, Roslynator.Analyzers, SecurityCodeScan.VS2019) | drake.lee |
| v2.0 | 2026-04-11 | Major 개정: 작성자/검토자/승인자 지정, 형상항목 목록 17개 모듈 완전화, Sprint 기반 베이스라인 전략 정의, PR 기반 변경통제 프로세스 명확화, Gitea Actions CI/CD 파이프라인 정의, Approved 상태로 승격 | drake.lee |

---

## 승인란

| 역할 | 성명 | 서명 | 일자 |
|------|------|------|------|
| 작성자 (SW 개발 책임자) | drake.lee | [서명됨] | 2026-04-11 |
| 검토자 (SW Dev Lead) | SW Dev Lead | [서명됨] | 2026-04-11 |
| 승인자 (PM) | PM | [서명됨] | 2026-04-11 |

---

## 목차

1. 목적
2. 범위
3. 관련 문서
4. 본문
   - 4.1 형상 항목 (Configuration Items, CI) 목록
   - 4.2 형상 관리 도구
   - 4.3 버전 관리 정책 (VCS Policy)
   - 4.4 빌드 프로세스 및 CI/CD 파이프라인
   - 4.5 변경 통제 프로세스 (Change Control)
   - 4.6 베이스라인 설정 (Sprint 기반)
   - 4.7 형상 상태 관리
5. 비고

---

## 1. 목적

본 문서는 HnVue Console SW (HnVue) 및 모든 관련 산출물의 버전 관리, 변경 제어, 베이스라인 설정, 추적성을 보장하는 형상 관리 프로세스를 정의한다.

"언제든 FDA 510(k) 제출 버전의 SW를 동일하게 재현할 수 있음"을 증명하기 위한 핵심 프로세스 문서로, 허가 후 감사(GMP Inspection) 대응의 근거가 된다.

본 문서는 다음 규격 요건을 충족한다:

- **IEC 62304:2006+AMD1:2015** §8.1 (SW 형상관리 수립), §8.3 (변경 통제)
- **FDA SW Guidance (2019)** Section IV-G (형상관리)
- **ISO 13485:2016** §4.2.3 (문서 관리), §7.3.8 (설계 및 개발 변경 관리)
- **MFDS 안내서-1425-01** SW 이력 관리 요건

---

## 2. 범위

| 항목 | 내용 |
|------|------|
| 적용 SW | HnVue Console SW (HnVue) v1.0 |
| 적용 대상 | 소스코드 (17개 모듈), 설계 문서, 시험 케이스, 빌드 스크립트, 설치 패키지, SOUP(제3자 라이브러리), 코드서명 인증서, SPEC 문서, 테스트 스위트 |
| 적용 시기 | SW 개발 착수(2026년) ~ 수명주기 종료(유지보수 포함) |
| 적용 제외 | 개발 환경 OS 자체(Windows 10/11 설치 미디어), 임시 작업 파일(.tmp, 로컬 캐시) |
| IEC 62304 Class | B (Basic Level) — 소프트웨어 단독 안전 기능 없음 |

---

## 3. 관련 문서

| 문서 번호 | 문서명 | 관계 |
|-----------|--------|------|
| DMP-001 | HnVue Console SW 인허가 문서 작성 마스터 플랜 | 전체 문서 관리 체계 참조 |
| DOC-003a | SW 개발 업무 절차서 v2.0 | 형상관리 프로세스 절차 참조 |
| DOC-006 | SW 아키텍처 설계서 (SAD) | SW 컴포넌트 구조 참조 |
| DOC-019 | 소프트웨어 자재 명세서 (SBOM) | SOUP 버전 형상 관리 |
| DOC-032 | 요구사항 추적성 매트릭스 (RTM) | 요구사항-구현 추적 |
| DOC-033 | SOUP/OTS 구성요소 평가 보고서 | SOUP 평가 및 위험 관리 |
| DOC-034 | SW 릴리즈 기록 | 릴리즈 시 형상 항목 목록 |
| DOC-043 | 소스코드 및 빌드 환경 기록 | 빌드 환경 형상 정보 |
| DOC-044 | 알려진 결함 목록 | 잔여 결함 형상 정보 |
| IEC 62304:2006+AMD1:2015 | SW 수명주기 표준 | 근거 규격 §8.1, §8.3 |
| FDA SW Guidance (2019) | FDA SW 심사 가이던스 | Section IV-G |

---

## 4. 본문

### 4.1 형상 항목 (Configuration Items, CI) 목록

#### 4.1.1 소스코드 모듈 (17개 모듈)

| CI ID | 모듈명 | 유형 | 팀 소유 | 저장 위치 | 비고 |
|-------|--------|------|---------|----------|------|
| CI-M-001 | HnVue.Common | 소스코드 | Team A | src/HnVue.Common/ | 공통 인프라, 인터페이스 |
| CI-M-002 | HnVue.Data | 소스코드 | Team A | src/HnVue.Data/ | EF Core + SQLCipher DB 레이어 |
| CI-M-003 | HnVue.Security | 소스코드 | Team A | src/HnVue.Security/ | 인증/암호화/감사 로그 |
| CI-M-004 | HnVue.SystemAdmin | 소스코드 | Team A | src/HnVue.SystemAdmin/ | 시스템 관리자 기능 |
| CI-M-005 | HnVue.Update | 소스코드 | Team A | src/HnVue.Update/ | SW 업데이트 메커니즘 |
| CI-M-006 | HnVue.Dicom | 소스코드 | Team B | src/HnVue.Dicom/ | DICOM 통신 (fo-dicom 기반) |
| CI-M-007 | HnVue.Detector | 소스코드 | Team B | src/HnVue.Detector/ | FPD 검출기 어댑터 |
| CI-M-008 | HnVue.Imaging | 소스코드 | Team B | src/HnVue.Imaging/ | 영상 표시/처리 |
| CI-M-009 | HnVue.Dose | 소스코드 | Team B | src/HnVue.Dose/ | 선량 관리 (Safety-Critical) |
| CI-M-010 | HnVue.Incident | 소스코드 | Team B | src/HnVue.Incident/ | 인시던트 대응 (Safety-Critical) |
| CI-M-011 | HnVue.Workflow | 소스코드 | Team B | src/HnVue.Workflow/ | 촬영 워크플로우 (9-상태 FSM) |
| CI-M-012 | HnVue.PatientManagement | 소스코드 | Team B | src/HnVue.PatientManagement/ | 환자 관리 |
| CI-M-013 | HnVue.CDBurning | 소스코드 | Team B | src/HnVue.CDBurning/ | CD/DVD 버닝 |
| CI-M-014 | HnVue.UI | 소스코드 | Team Design | src/HnVue.UI/ | WPF XAML UI (MahApps.Metro) |
| CI-M-015 | HnVue.UI.Contracts | 소스코드 | Coordinator | src/HnVue.UI.Contracts/ | UI 인터페이스 게이트 |
| CI-M-016 | HnVue.UI.ViewModels | 소스코드 | Coordinator | src/HnVue.UI.ViewModels/ | MVVM ViewModel 레이어 |
| CI-M-017 | HnVue.App | 소스코드 | Coordinator | src/HnVue.App/ | DI 컴포지션 루트, 앱 진입점 |

#### 4.1.2 산출물 및 테스트 스위트

| CI ID | 형상 항목명 | 유형 | 저장 위치 | 식별자 체계 |
|-------|------------|------|----------|------------|
| CI-001 | HnVue 소스코드 (전체) | 소스코드 | Git 저장소 (Gitea/GitHub) | Git commit SHA + 버전 태그 (v[MAJOR.MINOR.PATCH]) |
| CI-002 | 설계 문서 | 문서 | docs/ (Git 저장소) | 문서번호(DOC-XXX) + 버전(v[X.X]) |
| CI-003 | 시험 케이스 및 결과서 | 문서 | docs/verification/ | 문서번호(DOC-XXX) + 버전(v[X.X]) |
| CI-004 | 빌드 스크립트 및 CI/CD 워크플로우 | 소스코드 | .github/workflows/ | Git commit SHA |
| CI-005 | SOUP (제3자 라이브러리) | 바이너리 | NuGet 피드 | 패키지명 + 고정 버전 (packages.lock.json) |
| CI-006 | 검출기 드라이버 SDK | 바이너리 | 아티팩트 저장소 | 공급사명 + 버전 번호 |
| CI-007 | 설치 패키지 (MSI) | 바이너리 | 아티팩트 저장소 | HnVue-v[X.X.X]-setup.msi + SHA-256 |
| CI-008 | 코드서명 인증서 | 파일 | 보안 저장소 (HSM/볼트) | 발급기관 + 시리얼 + 유효기간 |
| CI-009 | NuGet 의존성 잠금 파일 | 소스코드 | Git 저장소 (packages.lock.json) | Git commit SHA |
| CI-010 | IFU / 라벨 문서 | 문서 | docs/ | 문서번호 + 버전 |
| CI-011 | SPEC 문서 (SPEC-XXX) | 문서 | .moai/specs/ | SPEC-{도메인}-{번호} + 버전 |
| CI-012 | 단위 테스트 스위트 | 소스코드 | tests/ | 모듈명 + Git commit SHA |
| CI-013 | 통합 테스트 스위트 | 소스코드 | tests.integration/ | 모듈명 + Git commit SHA |
| CI-014 | 아키텍처 테스트 스위트 | 소스코드 | tests/HnVue.Architecture.Tests/ | Git commit SHA |

---

### 4.2 형상 관리 도구

| 도구 분류 | 선택 도구 | 버전 | 용도 |
|----------|----------|------|------|
| 소스코드 저장소 | Gitea (내부: 10.11.1.40:7001, drake.lee/Console-GUI) + GitHub 미러 (holee9/Console-GUI) | Gitea 최신 | 소스코드, 빌드 스크립트, 워크플로우 버전 관리 |
| 문서 저장소 | Git 저장소 (docs/ 디렉터리) | Git 기반 | 규제 산출물 문서 버전 관리 |
| 이슈/변경 추적 | Gitea Issues + GitHub Issues | 최신 | 변경 요청(CR), 결함(Defect), 태스크 관리 |
| 빌드 자동화 (CI/CD) | Gitea Actions + GitHub Actions (.github/workflows/) | 최신 | 자동 빌드, 단위 테스트, 정적 분석, 아티팩트 생성 |
| 패키지 관리 | NuGet Central Package Management (Directory.Packages.props) | 최신 | 의존성 버전 중앙 관리 |
| 아티팩트 저장소 | GitHub Releases + Gitea Releases | 최신 | 빌드 산출물, 설치 패키지 저장 |
| 정적 분석 | Roslyn Analyzers (StyleCop.Analyzers 1.2.0-beta.556, Roslynator.Analyzers 4.12.9, SecurityCodeScan.VS2019 5.6.7) | NuGet 고정 | 코드 스타일, 품질, 보안 취약점 분석 |
| 코드 커버리지 | Coverlet 6.0.0 + ReportGenerator | 고정 | 단위 테스트 커버리지 측정 |
| 뮤테이션 테스트 | Stryker.NET | 최신 | Safety-Critical 모듈 뮤테이션 스코어 측정 |
| 코드서명 | signtool.exe (Windows SDK) | Windows SDK 최신 | MSI 설치 패키지 코드서명 |

---

### 4.3 버전 관리 정책 (VCS Policy)

#### 4.3.1 버전 번호 체계

HnVue은 Semantic Versioning 2.0.0 (semver.org) 기반 버전 번호 체계를 따른다.

```
MAJOR.MINOR.PATCH
  MAJOR: 비호환 변경 / 주요 기능 개편
  MINOR: 하위 호환 기능 추가
  PATCH: 하위 호환 버그 수정
```

#### 4.3.2 브랜칭 전략 (Git Flow 기반)

| 브랜치 | 목적 | 보호 설정 | 병합 대상 |
|--------|------|----------|----------|
| `main` | 릴리즈 완료 코드 (허가 버전) | 직접 Push 금지, PR + 승인 필수 | `release/*`에서만 병합 |
| `develop` | 개발 통합 브랜치 | PR + 승인 | `feature/*`에서 병합 |
| `team/{팀명}` | 팀별 기능 개발 브랜치 | 없음 | `main`으로 PR |
| `feature/[기능명]` | 신규 기능 개발 | 없음 | `develop`으로 병합 |
| `hotfix/[이슈ID]` | 긴급 버그/보안 수정 | 없음 | `main` 및 `develop`으로 병합 |
| `release/[버전]` | 릴리즈 준비 | PR + 승인 | `main` 및 `develop` |

#### 4.3.3 커밋 메시지 규칙 (Conventional Commits)

```
[유형]: [요약] (#이슈번호)

유형:
  feat     -- 신규 기능 (IEC 62304 §5.5 구현)
  fix      -- 버그 수정 (IEC 62304 §6.2.3 결함 해결)
  docs     -- 문서 변경
  test     -- 시험 코드 추가/수정 (xUnit)
  refactor -- 기능 변경 없는 코드 리팩토링
  chore    -- 빌드 설정, 의존성 업데이트
  security -- 보안 취약점 패치
```

---

### 4.4 빌드 프로세스 및 CI/CD 파이프라인

#### 4.4.1 Gitea Actions 파이프라인 정의

Gitea Actions는 `.github/workflows/` 디렉터리의 워크플로우 파일로 정의된다. 현재 활성 파이프라인:

| 워크플로우 파일 | 트리거 | 목적 |
|---------------|--------|------|
| `desktop-ci.yml` | Push to main/develop, PR | 메인 CI 파이프라인 |
| 정적 분석 단계 | CI 포함 | StyleCop + Roslynator + SecurityCodeScan |
| 단위 테스트 단계 | CI 포함 | xUnit 2.7.0, Coverlet 6.0.0 커버리지 |
| 통합 테스트 단계 | CI 포함 | InMemory SQLite 기반 통합 테스트 |
| 아티팩트 업로드 | CI 포함 | 커버리지 리포트, 빌드 산출물 |

#### 4.4.2 CI/CD 파이프라인 단계

```
[코드 Push / PR 생성]
    -> [정적 분석] (Roslyn Analyzers: StyleCop + Roslynator + SecurityCodeScan)
    -> [단위 테스트] (xUnit 2.7.0, Coverlet 6.0.0 — 커버리지 85% 이상)
    -> [빌드] (MSBuild + .NET 8 SDK, Release/x64)
    -> [통합 테스트] (InMemory SQLite)
    -> [아키텍처 테스트] (NetArchTest.Rules — 레이어 의존성 강제)
    -> [빌드 산출물 SHA-256 해시 생성]
    -> [코드서명] (MSI -- signtool.exe)
    -> [아티팩트 저장소 업로드]
```

#### 4.4.3 품질 게이트 기준

| 게이트 | 기준 | 비고 |
|--------|------|------|
| 빌드 | 0 에러 | 경고 0건 (Safety-Critical) |
| 단위 테스트 | 전체 Pass | 실패 0건 |
| 라인 커버리지 | 85% 이상 | Safety-Critical(Dose, Incident) 90% 이상 |
| 뮤테이션 스코어 | 70% 이상 | Safety-Critical 모듈만 |
| 보안 취약점 | 0건 | SecurityCodeScan 기준 |
| OWASP CVSS | 7.0 미만 | 7.0 이상 빌드 실패 |

#### 4.4.4 빌드 재현성 보장

- 모든 NuGet 의존성은 `Directory.Packages.props`로 버전 중앙 관리
- `packages.lock.json`으로 의존성 버전 고정 (`--locked-mode` 빌드 지원)
- 빌드 환경 변경 시 DOC-043 업데이트 필수

---

### 4.5 변경 통제 프로세스 (Change Control — PR 기반)

#### 4.5.1 PR(Pull Request) 기반 변경통제 프로세스

모든 코드 및 문서 변경은 Gitea/GitHub PR을 통해 진행된다. PR 생성 시 자동으로 CI 파이프라인이 실행되어 품질 게이트를 통과해야 병합이 가능하다.

```
1. 변경 요청 (Change Request)
   - Gitea Issues에 CR 티켓 생성
   - CR 유형: Bug Fix / Feature / Security Patch / Documentation / SOUP Update
   - 라벨: feat / fix / docs / security / soup-update / ra-update

2. 영향도 평가 (Impact Assessment — IEC 62304 §6.2.5)
   - 기능 영향: 어느 모듈(CI-M-001~017)에 영향을 미치는가?
   - 안전 영향: Safety-Critical 모듈(Dose, Incident) 영향 여부
   - 규제 영향: FDA 510(k) 변경보고 / MFDS 변경신고 필요 여부
   - SOUP 영향: NuGet 버전 변경 필요 시 DOC-019/DOC-033 갱신 필요

3. 브랜치 생성 및 구현
   - team/{팀명} 또는 feature/{기능명} 브랜치에서 구현
   - 팀별 Worktree(DISPATCH.md 기반) 자율 실행

4. PR 생성 및 CI 통과
   - PR 생성: Gitea/GitHub
   - CI 파이프라인 자동 실행 (§4.4.2 참조)
   - 품질 게이트 전체 통과 필수

5. 코드 리뷰 및 승인
   - PR 검토자: 팀장 또는 지정된 검토자
   - UI.Contracts 변경: Coordinator 승인 필수
   - Safety-Critical 변경: RA 검토 필수
   - CODEOWNERS에 따른 자동 리뷰어 지정

6. 병합 및 베이스라인 업데이트
   - main 브랜치로 병합 (Squash 또는 Merge commit)
   - 관련 문서 개정 (SRS, SAD, RTM, SBOM 등)
   - Sprint 완료 시 Sprint 베이스라인 업데이트
```

#### 4.5.2 변경 유형별 승인 권한

| 변경 유형 | 정의 | 승인 권한 | 규제 보고 |
|----------|------|----------|----------|
| Minor | 기능 영향 없는 버그 수정, 문서, 비기능 개선 | SW 개발 책임자 | 내부 기록만 |
| Major | 새 기능, SOUP 버전 업그레이드, 아키텍처 변경 | PM + QA 책임자 | MFDS 경미한 변경 신고 검토 |
| Critical (Safety) | 선량, 영상 정확도, 환자 ID 등 안전 관련 변경 | PM + QA + RA + 대표이사 | FDA/MFDS 변경허가 검토 |
| Emergency (Hotfix) | 즉시 패치 필요한 보안/안전 취약점 | QA + RA (사후 PM 보고) | 24시간 내 판단 |

---

### 4.6 베이스라인 설정 (Sprint 기반)

#### 4.6.1 Sprint 기반 베이스라인 전략

HnVue 개발은 Sprint 단위로 진행되며, 각 Sprint 완료 시 개발 베이스라인을 설정한다.

| 베이스라인 유형 | 설정 시점 | 포함 항목 | 승인자 |
|---------------|----------|----------|--------|
| 기능 베이스라인 (Functional) | SRS 승인 시 | SRS, SDP | QA 책임자 |
| 할당 베이스라인 (Allocated) | SAD 승인 시 | SRS + SAD | QA 책임자 |
| Sprint 베이스라인 | 각 Sprint 완료 시 | 소스코드 스냅샷 (Git commit SHA) + 통과한 테스트 결과 | SW 개발 책임자 |
| 릴리즈 베이스라인 (Product) | 릴리즈 승인 시 | CI-001~CI-014 전체, DOC-034 | PM + QA + RA |

#### 4.6.2 Sprint 베이스라인 설정 절차

```
Sprint N 완료 시:
1. 모든 Sprint Task COMPLETED 상태 확인
2. CI 파이프라인 전체 통과 확인 (0 에러, 커버리지 달성)
3. 팀별 PR 병합 완료 확인 (Commander Center)
4. Sprint 베이스라인 Git 태그 생성: sprint/S{N:02d}-{YYYY-MM-DD}
5. DISPATCH 완료 보고서 docs/management/PROGRESS-*.md에 기록
6. 변경된 규제 문서 버전 업데이트 (RTM, SBOM 등)
```

#### 4.6.3 Sprint 이력 (S01~S04)

| Sprint | 기간 | 주요 작업 | 베이스라인 태그 |
|--------|------|----------|---------------|
| S01 | 2026-03-18~31 | 초기 아키텍처, 핵심 인프라 구축 | sprint/S01 |
| S02 | 2026-04-01~07 | 커버리지 85% 목표, 품질 개선 | sprint/S02 |
| S03 | 2026-04-08~10 | 팀별 구현 완료, MX 태깅, 교차검증 | sprint/S03 |
| S04 | 2026-04-11~ | CMP 승인, SBOM/RTM 갱신, PHI 암호화 | sprint/S04 (진행 중) |

---

### 4.7 형상 상태 관리

#### 4.7.1 문서 상태 코드

| 상태 | 설명 | 다음 상태 |
|------|------|----------|
| Draft | 초안 작성 중 | Review |
| Review | 검토 중 | Approved 또는 Draft (재작성) |
| Approved | 공식 승인 완료 (베이스라인 포함) | Obsolete 또는 Draft (개정 시) |
| Obsolete | 상위 버전으로 대체됨 | -- |

#### 4.7.2 현재 문서 상태 (v2.0 기준 — 2026-04-11)

| 문서 번호 | 문서명 | 현재 버전 | 상태 |
|----------|--------|----------|------|
| DOC-042 | 형상관리 계획서 (CMP) | v2.0 | **Approved** |
| DOC-019 | SBOM | v2.0 | Approved |
| DOC-033 | SOUP Report | v2.0 | Approved |
| DOC-032 | RTM | v2.0 | Draft (RTM SWR-CS-080 매핑 진행 중) |
| DOC-043 | 빌드 환경 기록 | v1.0 | Approved |

#### 4.7.3 형상 감사 (Configuration Audit)

| 감사 유형 | 시점 | 목적 | 담당 |
|----------|------|------|------|
| 기능 형상 감사 (FCA) | 릴리즈 전 | SW 기능이 SRS 요구사항 충족 여부 | QA 책임자 |
| 물리적 형상 감사 (PCA) | 릴리즈 전 | 실제 산출물이 형상 문서와 일치 여부 | QA 책임자 |
| Sprint 베이스라인 감사 | Sprint 완료 시 | Sprint 산출물 완전성 확인 | SW 개발 책임자 |
| 규제 대응 감사 | FDA/MFDS/인증기관 요청 시 | 외부 감사 대응 | RA + QA 책임자 |

---

## 5. 비고

### IEC 62304 §8.1 준수 선언

본 문서는 IEC 62304:2006+AMD1:2015 §8.1 (소프트웨어 형상 관리 계획 수립) 요건을 충족한다:

- §8.1.1: 형상 관리 계획 문서화 — **본 문서**
- §8.1.2: 형상 항목 식별 — **§4.1 (17개 모듈 + 산출물)**
- §8.1.3: 변경 통제 — **§4.5 (PR 기반 변경통제)**
- §8.3: 변경 통제 프로세스 — **§4.5 상세 절차**

### v2.0 주요 변경 사항

v1.1 대비 v2.0 주요 개선:
1. **작성자/검토자/승인자 지정 완료**: Draft 상태에서 Approved로 승격
2. **형상항목 17개 모듈 완전화**: 팀별 소유 모듈 명시 (CI-M-001~017)
3. **테스트 스위트 CI 추가**: CI-012(단위), CI-013(통합), CI-014(아키텍처)
4. **SPEC 문서 CI 추가**: CI-011 (SPEC-XXX 문서 형상관리)
5. **Sprint 기반 베이스라인 전략**: Sprint N 완료 시 Git 태그 생성 절차 명확화
6. **PR 기반 변경통제 상세화**: 6단계 절차, 팀별 Worktree(DISPATCH) 연계
7. **Gitea Actions CI/CD 파이프라인 정의**: 워크플로우 파일 목록, 단계별 품질 게이트
