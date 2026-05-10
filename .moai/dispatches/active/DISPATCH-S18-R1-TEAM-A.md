# DISPATCH S18-R1 — Team A (Infrastructure)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: Team A
## Priority: P1-Critical (신 방법론 적용 첫 라운드)
## 근거 SPEC/문서: SPEC-INFRA-002 + Issue #109 + SPEC-METHODOLOGY-001 (S18-R1 ROADMAP)

---

## 배경

S18-R1은 **SPEC-METHODOLOGY-001** 발효 후 첫 라운드. SPEC-CONSTITUTION-001 + SPEC-DISPATCH-V2-001 + SPEC-AUTODRIVE-GATE-001이 발효된 상태에서 작업 수행.

S17-R1에서 freeze 시점에 미완료된 Security 90%+ 잔여(89.62% → 90%+)를 신 Self-Verification 7항목 게이트 하에서 완수.

---

## Tasks

### T1: HnVue.Security 90%+ 달성 [P1] (S17 carryover)
- 설명: Issue #109 해결 — Security line coverage 89.62% → 90%+
- 체크리스트:
  - [ ] `dotnet test tests/HnVue.Security.Tests/ --collect:"XPlat Code Coverage"` 재측정
  - [ ] 미커버 라인/브랜치 식별
  - [ ] 부족 테스트 작성 (TDD 또는 characterization)
  - [ ] 90%+ 달성 확인
- 완료 조건: Security line coverage >= 90%, Issue #109 close

### T2: SPEC-INFRA-002 REFACTOR + DI 교체 [P2]
- 설명: AesGcmPhiEncryptionService DI 교체 + REFACTOR
- 체크리스트:
  - [ ] App.xaml.cs DI 교체 (NullPhiEncryptionService → AesGcmPhiEncryptionService)
  - [ ] `dotnet build HnVue.sln` 0 errors
  - [ ] `dotnet test` 회귀 없음
- 완료 조건: DI 교체 + 전체 빌드/테스트 통과

### T3: 신 Self-Verification 7항목 첫 적용 [P1, 신규]
- 설명: SPEC-AUTODRIVE-GATE-001 REQ-AUTOGATE-009에 따른 Self-Verification 신 7번째 항목 적용
- 체크리스트:
  - [ ] DISPATCH Status 비고에 "실질 커밋 발생: YES — {commit hash}" 기재
  - [ ] 전체 솔루션 빌드 결과 명시 (모듈 빌드만 X)
  - [ ] Issue #109 코멘트에 증거 첨부
- 완료 조건: 신 게이트 통과 증거 명시

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Security 90%+ | NOT_STARTED | Team A | P1 | - | Issue #109, S17 carryover |
| T2 | INFRA-002 DI 교체 | NOT_STARTED | Team A | P2 | - | - |
| T3 | Self-Verification 7항목 | NOT_STARTED | Team A | P1 | - | 신규 (METHODOLOGY-001) |

---

## Constraints

- [HARD] 소유 모듈만 수정: HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update
- [HARD] 빌드/테스트 검증 없이 COMPLETED 금지 (SPEC-AUTODRIVE-GATE-001)
- [HARD] 실질 커밋 없으면 라운드 무효 (SPEC-CONSTITUTION-001 REQ-CONST-005, 사망 나선 헌법 조항)
- [HARD] ScheduleWakeup(900초) 유지 — Phase 1

## Evidence Required (강화)

DISPATCH Status 비고 열에:
1. `dotnet build HnVue.sln` 결과 (errors/warnings)
2. Security coverage before/after %
3. `git diff --name-only origin/main..HEAD` 출력
4. **실질 커밋 SHA** (신규, 사망 나선 가드)

---

## 참고 문서

- `.moai/specs/SPEC-INFRA-002/`, `.moai/specs/SPEC-METHODOLOGY-001/`, `.moai/specs/SPEC-AUTODRIVE-GATE-001/`
- Issue #109
- `.claude/rules/teams/team-a.md`, `quality-standards.md` v 사망 나선 메트릭
