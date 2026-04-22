> 🛑 **[HOLD — 2026-04-22]** 이 DISPATCH는 시스템 재정비 완료 후 재발행 예정. **ScheduleWakeup 설정 금지. 작업 착수 금지. 세션 종료 권장.**

# DISPATCH S16-R2 — Team B (Medical Imaging)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: Team B
## Priority: HIGH (Safety-Critical 커버리지)
## 근거 SPEC: SPEC-TEAMB-COV-001

---

## 배경

SPEC-TEAMB-COV-001은 plan/acceptance 산출물이 이미 완비된 상태로 실행 대기 중.
Safety-Critical 모듈(Dose, Incident)의 90% 커버리지는 IEC 62304 Class B 요구사항.
이번 라운드에서 Dose 또는 Incident 중 하나를 90%+ 달성.

---

## Tasks

### T1: SPEC-TEAMB-COV-001 현황 재확인 [P1]
- **설명**: `.moai/specs/SPEC-TEAMB-COV-001/` 문서 상태 및 현재 커버리지 측정
- **체크리스트**:
  - [ ] `.moai/specs/SPEC-TEAMB-COV-001/spec.md` 재읽기 — 수용 기준 확인
  - [ ] `.moai/specs/SPEC-TEAMB-COV-001/acceptance.md` 재읽기
  - [ ] `dotnet test tests/HnVue.Dose.Tests/` + 커버리지 측정 — 현재 % 기록
  - [ ] `dotnet test tests/HnVue.Incident.Tests/` + 커버리지 측정 — 현재 % 기록
- **완료 조건**: 현재 커버리지 수치 DISPATCH Status에 기록

### T2: Dose 모듈 또는 Incident 모듈 90% 달성 [P1]
- **설명**: 우선순위 높은 모듈부터 커버리지 90% 목표 달성
- **체크리스트**:
  - [ ] 우선 모듈 선택 (현재 커버리지 더 낮은 쪽 우선)
  - [ ] 미커버 라인 식별 → characterization test 또는 TDD 추가 테스트 작성
  - [ ] 테스트 추가 후 재측정 → 90%+ 확인
  - [ ] 4-level interlock 로직 불변(invariant) 유지 — 비즈니스 로직 변경 금지
- **완료 조건**: Dose 또는 Incident 중 1개 모듈 90%+ 달성

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 커버리지 수치 및 진행 상태 실시간 반영
- **완료 조건**: 커버리지 before/after 수치가 Status 비고 열에 기록

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 커버리지 현황 재측정 | NOT_STARTED | Team B | P1 | - | Dose + Incident |
| T2 | Safety-Critical 90% 달성 | NOT_STARTED | Team B | P1 | - | Dose 또는 Incident |
| T3 | DISPATCH Status 업데이트 | NOT_STARTED | Team B | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.Dicom`, `HnVue.Detector`, `HnVue.Imaging`, `HnVue.Dose`, `HnVue.Incident`, `HnVue.Workflow`, `HnVue.PatientManagement`, `HnVue.CDBurning`
- [HARD] Dose 4-level interlock 로직 불변 — 비즈니스 로직 변경 시 RA 위험 평가 필요
- [HARD] characterization test로 기존 동작 보존
- [HARD] 빌드/테스트 검증 없이 COMPLETED 보고 금지
- [HARD] ScheduleWakeup 설정 금지 (HOLD 중 — _CURRENT.md 참조)

## Evidence Required

완료 보고 시:
1. `dotnet build` 0 errors
2. 모듈별 커버리지 before/after % (Cobertura XML 경로)
3. 추가된 테스트 파일 목록

---

## 참고 문서

- `.moai/specs/SPEC-TEAMB-COV-001/spec.md`, `plan.md`, `acceptance.md`
- `.claude/rules/teams/team-b.md` — Safety-Critical 표준
