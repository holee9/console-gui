# DISPATCH S17-R1 — Team A (Infrastructure)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: Team A
## Priority: P1-Critical (Safety-Critical 4/4 PASS)
## 근거 SPEC/문서: SPEC-INFRA-002 + Issue #109 (Security 89.62% < 90%)

---

## 배경

S16-R2 QA 최종 판정에서 **HnVue.Security 89.62%** 가 Safety-Critical 90% 게이트를 통과하지 못함
(Issue #109). 나머지 3개 Safety-Critical(Dose 100%, Incident 95.24%, Update 96%)은 PASS.
이 라운드에서 Security 0.38%p 보강으로 Safety-Critical 4/4 PASS 달성.

또한 SPEC-INFRA-002는 S16-R2에서 planning + AesGcmPhiEncryptionService 14개 GREEN 테스트까지 완료.
이 라운드에서 REFACTOR 단계 진행 및 DI 등록 교체 완료.

---

## Tasks

### T1: HnVue.Security 90%+ 커버리지 달성 [P1]
- **설명**: Issue #109 해결 — Security 모듈 line coverage 89.62% → 90%+ 달성
- **체크리스트**:
  - [ ] `dotnet test tests/HnVue.Security.Tests/ --collect:"XPlat Code Coverage"` 현재 커버리지 재측정
  - [ ] 미커버 라인/브랜치 식별 (Cobertura XML 분석)
  - [ ] 부족한 테스트 케이스 작성 (characterization test 또는 TDD)
  - [ ] 90%+ 달성 확인
- **완료 조건**: Security line coverage >= 90%, Issue #109 close

### T2: SPEC-INFRA-002 REFACTOR + DI 등록 교체 [P2]
- **설명**: S16-R2에서 구현한 AesGcmPhiEncryptionService의 REFACTOR 단계 + App.xaml.cs DI 교체
- **체크리스트**:
  - [ ] `.moai/specs/SPEC-INFRA-002/tasks.md` REFACTOR 항목 점검
  - [ ] AesGcmPhiEncryptionService 코드 품질 리뷰 (SOLID, 예외 처리)
  - [ ] `src/HnVue.App/App.xaml.cs`에서 NullPhiEncryptionService → AesGcmPhiEncryptionService DI 교체
  - [ ] `dotnet build HnVue.sln` 0 errors 확인
  - [ ] `dotnet test` 기존 테스트 모두 PASS 확인 (회귀 방지)
- **완료 조건**: DI 교체 완료 + 전체 빌드/테스트 통과

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 작업 시작 시 IN_PROGRESS, 완료 시 COMPLETED로 타임스탬프와 함께 업데이트
- **완료 조건**: Status 테이블 정확 반영 + push

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Security 90%+ 달성 | COMPLETED | Team A | P1 | 2026-04-22T18:45:00+09:00 | ✅ 90.04% 달성 (89.62%→90.04%), Build: 0 errors/440 warnings, Issue #109 close |
| T2 | SPEC-INFRA-002 REFACTOR + DI 교체 | COMPLETED | Team A | P2 | 2026-04-22T19:05:00+09:00 | REFACTOR S16-R2 완료 확인, DI 교체 완료(NullPhiEncryptionService 제거), Build: 0 errors, 373/373 tests PASS |
| T3 | DISPATCH Status 업데이트 | COMPLETED | Team A | P3 | 2026-04-22T14:30:00+09:00 | 상시 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.Common`, `HnVue.Data`, `HnVue.Security`, `HnVue.SystemAdmin`, `HnVue.Update`
- [HARD] SPEC-INFRA-002 범위 엄수 — MFA, Key Rotation, 기존 데이터 마이그레이션은 Out of Scope
- [HARD] 빌드/테스트 검증 없이 COMPLETED 보고 금지 — `dotnet build` 0 errors 확인
- [HARD] ScheduleWakeup(900초) 유지 — 작업 완료 push 직후 재설정 (Phase 1, _CURRENT.md)

## Evidence Required

완료 보고 시 DISPATCH Status 비고 열에 아래 3개 증거 포함:
1. `dotnet build` 결과 요약 (errors/warnings 개수)
2. Security 커버리지 before/after %
3. 변경 파일 목록 (`git diff --name-only`)

---

## 참고 문서

- `.moai/specs/SPEC-INFRA-002/spec.md`, `plan.md`, `acceptance.md`, `tasks.md`
- Issue #109: Security coverage 89.62% < 90%
- `.claude/rules/teams/team-a.md` — Security 코드 표준
- `TestReports/S16-R2/` — 이전 QA 측정 결과
