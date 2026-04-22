# DISPATCH S17-R1 — Team B (Medical Imaging)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: Team B
## Priority: P2-High (Safety-Critical Incident branch 90% + Standard 모듈 보강)
## 근거 SPEC/문서: SPEC-TEAMB-COV-001 + QA S16-R2 FINAL-COVERAGE.md

---

## 배경

S16-R2에서 Dose 모듈 99.68% line / 93.8% branch 달성으로 Safety-Critical 90%+ 게이트 통과.
그러나 **Incident 모듈 branch coverage 77.38%** 가 여전히 90% 미달.
또한 표준 모듈 중 Dicom 54.10%가 85% 게이트에 크게 미달.
이 라운드에서 Incident branch 90% 달성 + Dicom 커버리지 향상.

---

## Tasks

### T1: Incident 모듈 branch coverage 90% 달성 [P1]
- **설명**: Incident branch coverage 77.38% → 90%+ 달성 (Safety-Critical)
- **체크리스트**:
  - [ ] `dotnet test tests/HnVue.Incident.Tests/ --collect:"XPlat Code Coverage"` 현재 상태 재측정
  - [ ] Cobertura XML에서 미커버 branch 식별
  - [ ] 미커버 branch 대상 테스트 작성 (분기 조건, 예외 경로, 경계값)
  - [ ] 90%+ 달성 확인
- **완료 조건**: Incident branch coverage >= 90%

### T2: Dicom 모듈 커버리지 향상 [P2]
- **설명**: Dicom 54.10% → 목표 70%+ 향상 (85% 게이트는 후속 라운드)
- **체크리스트**:
  - [ ] 현재 Dicom 테스트 현황 및 미커버 영역 분석
  - [ ] 우선순위 높은 미커버 클래스/메서드 식별
  - [ ] 테스트 추가 (C-STORE 핸들러, MWL 쿼리, 태그 파싱 등)
  - [ ] 향상된 커버리지 % 기록
- **완료 조건**: Dicom line coverage 70%+ 또는 가능한 최대 향상

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 커버리지 수치 및 진행 상태 실시간 반영
- **완료 조건**: 커버리지 before/after 수치가 Status 비고 열에 기록

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Incident branch 90% 달성 | IN_PROGRESS | Team B | P1 | 2026-04-22T15:45:00+09:00 | Safety-Critical 77.38% → 90%+ |
| T2 | Dicom 커버리지 향상 | NOT_STARTED | Team B | P2 | - | 54.10% → 70%+ |
| T3 | DISPATCH Status 업데이트 | COMPLETED | Team B | P3 | 2026-04-22T15:45:00+09:00 | IN_PROGRESS 전환 완료 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.Dicom`, `HnVue.Detector`, `HnVue.Imaging`, `HnVue.Dose`, `HnVue.Incident`, `HnVue.Workflow`, `HnVue.PatientManagement`, `HnVue.CDBurning`
- [HARD] Dose 4-level interlock 로직 불변 — 비즈니스 로직 변경 시 RA 위험 평가 필요
- [HARD] characterization test로 기존 동작 보존
- [HARD] 빌드/테스트 검증 없이 COMPLETED 보고 금지
- [HARD] ScheduleWakeup(900초) 유지 — 작업 완료 push 직후 재설정 (Phase 1, _CURRENT.md)

## Evidence Required

완료 보고 시:
1. `dotnet build` 0 errors
2. 모듈별 커버리지 before/after % (Cobertura XML 경로)
3. 추가된 테스트 파일 목록

---

## 참고 문서

- `.moai/specs/SPEC-TEAMB-COV-001/spec.md`, `plan.md`, `acceptance.md`
- `TestReports/S16-R2/FINAL-COVERAGE.md` — QA 측정 결과
- `.claude/rules/teams/team-b.md` — Safety-Critical 표준
