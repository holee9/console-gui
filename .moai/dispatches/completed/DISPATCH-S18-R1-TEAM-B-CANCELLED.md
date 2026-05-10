# DISPATCH S18-R1 — Team B (Medical Imaging Pipeline)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: Team B
## Priority: P1-Critical
## 근거 SPEC/문서: SPEC-TEAMB-COV-001 + SPEC-METHODOLOGY-001 (S18-R1)

---

## 배경

S17-R1 freeze 시점 carryover. Incident branch coverage 90%+ 잔여 + Dicom 모듈 향상.
신 방법론(SPEC-AUTODRIVE-GATE-001) 하에서 Evidence-Based Completion 적용.

---

## Tasks

### T1: HnVue.Incident branch coverage 90%+ [P1]
- 설명: Safety-Critical 모듈 branch coverage 90% 게이트 통과
- 체크리스트:
  - [ ] 현 branch coverage 측정
  - [ ] 미커버 분기 식별
  - [ ] 분기 테스트 추가
  - [ ] 90%+ 달성 + Stryker mutation 70%+ 확인
- 완료 조건: branch >= 90%, mutation >= 70%

### T2: HnVue.Dicom 향상 [P2]
- 설명: Dicom 모듈 line coverage 향상 (현재 → 70%+)
- 체크리스트:
  - [ ] 현 커버리지 측정
  - [ ] 우선순위 시나리오 식별 (C-STORE, MWL)
  - [ ] 통합 테스트 추가
- 완료 조건: Dicom line coverage 70%+

### T3: 신 Self-Verification 7항목 적용 [P1, 신규]
- 설명: 실질 커밋 SHA 기재 + 전체 솔루션 빌드 확인

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Incident branch 90%+ | NOT_STARTED | Team B | P1 | - | Safety-Critical |
| T2 | Dicom 향상 | NOT_STARTED | Team B | P2 | - | - |
| T3 | Self-Verification 7항목 | NOT_STARTED | Team B | P1 | - | 신규 |

---

## Constraints

- [HARD] 소유 모듈만 수정: Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning
- [HARD] Safety-Critical 모듈(Dose, Incident) 변경 시 characterization test 우선
- [HARD] 빌드/테스트 검증 없이 COMPLETED 금지
- [HARD] 실질 커밋 없으면 라운드 무효
- [HARD] ScheduleWakeup(900초) — Phase 1

## Evidence Required

1. `dotnet build HnVue.sln` 결과
2. Coverage before/after (Incident branch + Dicom line)
3. Stryker mutation score
4. 실질 커밋 SHA

---

## 참고 문서

- `.moai/specs/SPEC-TEAMB-COV-001/`, `SPEC-METHODOLOGY-001/`
- `.claude/rules/teams/team-b.md`, `quality-standards.md`
