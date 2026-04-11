# DISPATCH: QA — Gate Enforcement & Automation

Issued: 2026-04-09
Issued By: Main (MoAI Orchestrator)
Priority: P2-High
Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Execute tasks in order
3. Update Status section after each task

## Context

Phase 1 커버리지 갭 분석 완료 (PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md).
QA는 소스 모듈 coverage owner가 아니라 **gate owner**.

확정 게이트:
- interim gate: 전체 line coverage **80%+**
- release gate: 전체 line coverage **85%+**
- safety-critical branch gate: Dose/Incident **90%+**

## File Ownership

- .github/workflows/**
- scripts/ci/**, scripts/qa/**
- TestReports/**

## Tasks

### Task 1: CI Coverage Gate 자동화 (P1-Critical)

**목적**: PR 머지 전 커버리지 게이트 자동 검증

**실행**:
1. `scripts/qa/Invoke-CoverageGate.ps1` 생성
   - 전체 line coverage 80%+ 검증 (interim)
   - 모듈별 floor 검증 (팀별 확정 목표 기준)
   - Safety-critical branch coverage 90%+ 검증 (Dose, Incident)
   - 결과: PASS/FAIL + 상세 리포트
2. `desktop-ci.yml`에 coverage-gate job 추가
   - `dotnet test` → `reportgenerator` → `Invoke-CoverageGate.ps1`
   - FAIL 시 PR 머지 블록

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 현재 커버리지에서 정확히 FAIL 반환 (75.6% < 80%)
- [ ] 모듈별 floor 체크 동작

### Task 2: FlaUI E2E 프레임워크 구축 (P2-High)

**목적**: BUG-002 해소

**실행**:
1. FlaUI NuGet 패키지 추가 (Directory.Packages.props)
2. `tests.e2e/HnVue.E2ETests/` 프로젝트 생성
3. 앱 실행 → 스크린샷 캡처 헬퍼
4. Login 화면 smoke test 1개

**검증 기준**:
- [ ] 프로젝트 빌드 성공
- [ ] Login smoke test 작성

### Task 3: Performance Baseline 스크립트 (P2-High)

**목적**: BUG-003, QA-002 해소

**실행**:
1. `scripts/qa/Invoke-PerformanceBaseline.ps1` 생성
2. 측정: 앱 시작, 화면 전환, 검색 응답, 메모리
3. DOC-027 기준값 대비 Pass/Fail 판정

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 4개 메트릭 측정 구현

### Task 4: 주간 Coverage Trend Report 템플릿 (P3-Medium)

**목적**: QA 확정 목표 — 주간 트렌드 리포트 발행 100%

**실행**:
1. `scripts/qa/Generate-CoverageTrend.ps1` 생성
2. 팀별 current/target/gap 표 자동 생성
3. 이전 주 대비 증감 표시
4. `TestReports/COVERAGE_TREND_{date}.md` 출력

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 팀별 표 정확 출력

## Status

- **State**: SUPERSEDED
- **Started**: -
- **Completed**: 2026-04-11
- **Results**: S04-R1-qa.md로 대체됨
