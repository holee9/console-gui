# DISPATCH S17-R1 — Coordinator (Integration)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: Coordinator
## Priority: P2-High (통합 안정성 검증)
## 근거 SPEC/문서: SPEC-COORDINATOR-001 (6개 Repository 실구현 검증)

---

## 배경

S16-R2에서 Coordinator가 6개 NullRepository를 EF Core 기반 실구현으로 교체 + 통합테스트 12개 작성.
DISPATCH에 "S17에서 Team A가 품질 보강" 권고가 있었음.
이 라운드에서 6개 Repository 통합 안정성 검증 + 필요시 테스트 보강.

---

## Tasks

### T1: 6개 Repository 통합 안정성 검증 [P1]
- **설명**: S16-R2에서 구현한 6개 EF Repository의 통합 안정성 확인
- **체크리스트**:
  - [ ] `dotnet build HnVue.sln` 전체 빌드 — 0 errors 확인
  - [ ] `dotnet test tests.integration/` 기존 12개 통합테스트 모두 PASS 확인
  - [ ] App.xaml.cs DI 등록 상태 확인 (6개 모두 EF 구현체로 교체됨)
  - [ ] 각 Repository 기본 CRUD 동작 검증
- **완료 조건**: 전체 빌드 0 errors + 통합테스트 12/12 PASS

### T2: 통합테스트 보강 (필요시) [P2]
- **설명**: 누락된 엣지케이스 통합테스트 추가
- **체크리스트**:
  - [ ] 기존 12개 테스트 커버리지 분석
  - [ ] 누락된 시나리오 식별 (동시성, 예외, 경계값)
  - [ ] 추가 통합테스트 작성 (in-memory SQLite)
  - [ ] `dotnet test tests.integration/` 전체 통과
- **완료 조건**: 통합테스트 증가 + 모두 PASS

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 작업 시작 시 IN_PROGRESS, 완료 시 COMPLETED
- **완료 조건**: 타임스탬프 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 6개 Repository 통합 검증 | NOT_STARTED | Coordinator | P1 | - | - |
| T2 | 통합테스트 보강 | NOT_STARTED | Coordinator | P2 | - | - |
| T3 | DISPATCH Status 업데이트 | NOT_STARTED | Coordinator | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.UI.Contracts`, `HnVue.UI.ViewModels`, `HnVue.App`, `tests.integration/`
- [HARD] DesignTime/ 수정 금지 (Design 단독 소유)
- [HARD] 통합테스트는 in-memory SQLite 사용 (실 DB 의존성 없음)
- [HARD] ScheduleWakeup(960초) 유지 — 작업 완료 push 직후 재설정 (Phase 2, _CURRENT.md)

## Evidence Required

완료 보고 시:
1. `dotnet build HnVue.sln` 0 errors
2. `dotnet test tests.integration/` 결과 (PASS/FAIL 수치)
3. DI 등록 상태 확인 결과
4. 변경 파일 목록 (있는 경우)

---

## 참고 문서

- `.moai/specs/SPEC-COORDINATOR-001/spec.md`, `plan.md`, `acceptance.md`, `tasks.md`
- `src/HnVue.App/App.xaml.cs` — DI 등록
- `.claude/rules/teams/coordinator.md` — DI Registration Standards
