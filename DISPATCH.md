# DISPATCH: QA — Gate 자동화 + E2E + Performance Baseline

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: P1-Critical (게이트) + P2-High (E2E/성능)
Supersedes: 이전 DISPATCH (상태 미기록, 체크 0/10)

## QA 역할 재확인 (rules/teams/qa.md)

- **소유**: .github/workflows/, scripts/ci/, scripts/qa/, TestReports/
- **QA는 gate owner — 소스 모듈 커버리지 작성자가 아님**
- **Coverage gate**: 85% overall, 90% safety-critical
- **PR 리뷰**: CODEOWNERS 필수 리뷰어, 아키텍처 위반=PR 차단

## How to Execute

1. Task 순서대로 수행
2. 체크박스 + Status 업데이트

## Task 1: CI Coverage Gate 자동화 (P1-Critical)

**수행**:
1. `scripts/qa/Invoke-CoverageGate.ps1` — 전체 80%+, 모듈별 floor, Safety-critical 90%+
2. `desktop-ci.yml`에 coverage-gate job 추가

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 현재 75.6%에서 FAIL 반환
- [ ] 모듈별 floor 체크 동작

## Task 2: FlaUI E2E 프레임워크 (P2-High)

**수행**: FlaUI NuGet + tests.e2e/ 프로젝트 + Login smoke test
**NuGet 추가 시**: Team A에 `soup-update` 이슈 필요

**검증 기준**:
- [ ] 프로젝트 빌드 성공
- [ ] Login smoke test 작성

## Task 3: Performance Baseline (P2-High)

**수행**: `scripts/qa/Invoke-PerformanceBaseline.ps1` — 4메트릭

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 4개 메트릭 구현

## Task 4: Coverage Trend Report (P3-Medium)

**검증 기준**:
- [ ] 스크립트 실행 가능
- [ ] 팀별 표 출력

## Constraints

- QA 소유 파일만 수정
- 소스 모듈 테스트 코드 작성 금지
- NuGet 추가 시 Team A `soup-update` 이슈
- 보안 취약점 → `security` + `priority-critical` Gitea 이슈

## Status

- **State**: NOT_STARTED
- **Results**: Task 1→PENDING, Task 2→PENDING, Task 3→PENDING, Task 4→PENDING
