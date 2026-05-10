# DISPATCH S18-R1 — QA (Quality Assurance)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: QA
## Priority: P1-Critical
## 근거 SPEC/문서: SPEC-AUTODRIVE-GATE-001 + SPEC-METHODOLOGY-001 + quality-standards.md v1.3+

---

## 배경

S18-R1은 신 방법론 첫 라운드. QA는 AUTODRIVE-GATE-001 자동화 준비 + 신 메트릭(Substantive Commit Rate) 첫 측정.
Safety-Critical 4/4 PASS 검증은 carryover.

---

## Tasks

### T1: Safety-Critical 4/4 PASS 검증 [P1] (S17 carryover)
- 설명: Dose, Incident, Security, Update 90%+ 모두 통과 확인
- 체크리스트:
  - [ ] Team A의 Security 90%+ 검증 (T1 완료 후)
  - [ ] Team B의 Incident 90%+ 검증 (T1 완료 후)
  - [ ] 4/4 PASS 보고서 생성
- 완료 조건: TestReports/S18-R1/safety-critical-pass-summary.md 작성

### T2: 신 메트릭 측정 — Substantive Commit Rate [P1, 신규]
- 설명: SPEC-AUTODRIVE-GATE-001 REQ-AUTOGATE-005 첫 측정
- 체크리스트:
  - [ ] S18-R1 모든 팀 커밋을 실질/비실질로 분류
  - [ ] 실질: 소스/문서/SPEC 변경 / 비실질: ScheduleWakeup·프로토콜 패치·IDLE CONFIRM
  - [ ] 라운드별 통계 보고
- 완료 조건: `.moai/reports/substantive-commit-S18-R1.md` 생성

### T3: 전체 모듈 커버리지 리포트 [P2]
- 설명: 통상 라운드 마감 커버리지 측정

### T4: Phase 3 게이트 검증 [P1, 신규]
- 설명: Phase 1+2 완료 의존성 확인 후 진행 (DISPATCH-V2 §6)

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Safety-Critical 4/4 검증 | NOT_STARTED | QA | P1 | - | S17 carryover |
| T2 | Substantive Commit Rate | NOT_STARTED | QA | P1 | - | 신규 |
| T3 | 전체 커버리지 리포트 | NOT_STARTED | QA | P2 | - | - |
| T4 | Phase 3 게이트 검증 | NOT_STARTED | QA | P1 | - | 신규 |

---

## Constraints

- [HARD] QA 판정은 최종 — 사용자 승인 없이 번복 불가
- [HARD] QA 소유: .github/workflows/, scripts/ci/, scripts/qa/, TestReports/
- [HARD] Phase 1+2 미완료 시 검증 가능 모듈만 측정 (UNVERIFIED 명시)
- [HARD] 실질 커밋 없으면 라운드 무효
- [HARD] ScheduleWakeup(1020초) — Phase 3

## Evidence Required

1. `dotnet build HnVue.sln` 결과 (전체 솔루션)
2. `dotnet test --collect:"XPlat Code Coverage"` 전체 결과
3. Stryker mutation score (Safety-Critical만)
4. Substantive Commit Rate 측정값
5. 실질 커밋 SHA

---

## 참고 문서

- `.moai/specs/SPEC-AUTODRIVE-GATE-001/`, `SPEC-METHODOLOGY-001/`
- `.claude/rules/teams/qa.md`, `quality-standards.md`
