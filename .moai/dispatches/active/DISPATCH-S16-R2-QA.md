# DISPATCH S16-R2 — QA (Quality Assurance)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: QA
## Priority: HIGH (품질 게이트 회복)
## 근거 문서: team-common.md Quality Standards + S14-R2 CONDITIONAL PASS 잔여

---

## 배경

S14-R2에서 QA가 CONDITIONAL PASS (4107/4124 = 99.47%) 판정.
이후 S14-R2/S15/S16-R1 3개 Sprint 동안 후속 품질 작업 없이 IDLE CONFIRM만 수행.
이번 라운드에서 전체 커버리지 재측정 + CONDITIONAL PASS 해소 + Safety-Critical 90% 확인.

---

## Tasks

### T1: 전체 솔루션 빌드 + 테스트 재측정 [P1]
- **설명**: `HnVue.sln` 전체 빌드 + 모든 테스트 프로젝트 실행
- **체크리스트**:
  - [ ] `dotnet restore HnVue.sln`
  - [ ] `dotnet build HnVue.sln -c Release` — 0 errors, 0 warnings 확인
  - [ ] `dotnet test HnVue.sln --settings coverage.runsettings --results-directory TestReports/S16-R2/` — 모든 테스트 실행
  - [ ] 실패 테스트 목록 확인 (S14-R2의 17개 실패 테스트 상태)
- **완료 조건**: 빌드/테스트 결과 기록

### T2: Safety-Critical 90% 커버리지 검증 [P1]
- **설명**: Dose, Incident, Security, Update 4개 Safety-Critical 모듈 커버리지 확인
- **체크리스트**:
  - [ ] Cobertura 리포트 파싱
  - [ ] 4개 모듈별 Line Coverage % 기록
  - [ ] 90% 미달 모듈 식별 → Gitea 이슈 생성 (`qa-result` + `priority-high`)
  - [ ] 90%+ 달성 모듈은 PASS 기록
- **완료 조건**: 4개 모듈 현황 리포트 + 미달 이슈 등록

### T3: CONDITIONAL PASS 해소 상태 분석 [P2]
- **설명**: S14-R2의 17개 실패 테스트가 현재 어떤 상태인지 분석
- **체크리스트**:
  - [ ] S14-R2 QA 리포트(TestReports/) 참조
  - [ ] 현재 실패 테스트와 교차 비교
  - [ ] 회복 여부 / 미해결 여부 분류
- **완료 조건**: CONDITIONAL → PASS 또는 FAIL 최종 판정

### T4: 릴리즈 준비도 스냅샷 [P3]
- **설명**: `scripts/qa/Generate-ReleaseReport.ps1` 실행
- **체크리스트**:
  - [ ] 스크립트 실행 가능 여부 확인
  - [ ] `TestReports/RELEASE_READY_20260422.html` 생성
  - [ ] 블로킹 항목 리스트 보고
- **완료 조건**: HTML 리포트 생성 (실패 시 BLOCKED 보고)

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 전체 빌드 + 테스트 재측정 | NOT_STARTED | QA | P1 | - | Release 구성 |
| T2 | Safety-Critical 90% 검증 | NOT_STARTED | QA | P1 | - | Dose/Incident/Security/Update |
| T3 | CONDITIONAL PASS 해소 분석 | NOT_STARTED | QA | P2 | - | S14-R2 비교 |
| T4 | 릴리즈 준비도 스냅샷 | NOT_STARTED | QA | P3 | - | RELEASE_READY HTML |

---

## Constraints

- [HARD] 구현 금지 — QA는 검증 전용
- [HARD] PASS/FAIL 판정은 QA 독립 권한 (CC가 번복 불가)
- [HARD] 소유 도구만 사용: `dotnet build`, `dotnet test`, Coverlet, Stryker, SonarCloud, StyleCop, OWASP 도구
- [HARD] Safety-Critical 미달 시 `priority-high` 이슈 + 해당 팀 통지
- [HARD] ScheduleWakeup(300초) 유지

## Evidence Required

완료 보고 시:
1. `dotnet build` 0 errors 증명 (요약 문자열)
2. 전체 테스트 결과 (PASS/FAIL/SKIP 수치)
3. Safety-Critical 4개 모듈 커버리지 % 테이블
4. CONDITIONAL PASS 해소 판정 (PASS | FAIL | 여전히 CONDITIONAL)
5. 생성된 리포트 경로

---

## 참고 문서

- `.claude/rules/teams/qa.md` — QA 소유권 + 독립성
- `.claude/rules/teams/team-common.md` Quality Standards (Single Source of Truth)
- `coverage.runsettings`, `stryker-config.json`, `.stylecop.json`
- `docs/testing/DOC-011 ~ DOC-034` — V&V 계획
