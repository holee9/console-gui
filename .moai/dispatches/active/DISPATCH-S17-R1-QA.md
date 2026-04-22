# DISPATCH S17-R1 — QA (Quality Assurance)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: QA
## Priority: P1-Critical (Safety-Critical 4/4 PASS 최종 검증)
## 근거 문서: Quality Standards (quality-standards.md) + S16-R2 CONDITIONAL PASS

---

## 배경

S16-R2 QA 판정: CONDITIONAL PASS
- Safety-Critical 3/4 PASS (Dose 100%, Incident 95.24%, Update 96%)
- **Security 89.62% < 90%** → Issue #109, Team A 보강 중
- 13개 테스트 실패 (S14-R2 17개에서 4개 회복)
- Standard 모듈: Common 48.74%, Dicom 54.10%, SystemAdmin 62.90%

이 라운드에서 Team A Security 보강 후 Safety-Critical 4/4 PASS 확인 + 전체 재측정.

---

## Tasks

### T1: 전체 솔루션 빌드 + 테스트 재측정 [P1]
- **설명**: 구현팀(Team A, Team B) 작업 반영 후 전체 빌드/테스트
- **체크리스트**:
  - [ ] `dotnet restore HnVue.sln`
  - [ ] `dotnet build HnVue.sln -c Release` — 0 errors 확인
  - [ ] `dotnet test HnVue.sln --settings coverage.runsettings --results-directory TestReports/S17-R1/`
  - [ ] S16-R2 대비 테스트 결과 비교 (13개 실패 변화)
- **완료 조건**: 빌드/테스트 결과 기록

### T2: Safety-Critical 4/4 검증 [P1]
- **설명**: Dose, Incident, Security, Update 4개 모듈 90%+ 확인
- **체크리스트**:
  - [ ] Cobertura 리포트 파싱
  - [ ] **Security: 90%+ 확인** (Issue #109 해소)
  - [ ] Incident: branch 90%+ 확인 (Team B 보강)
  - [ ] Dose, Update: 기존 90%+ 유지 확인
  - [ ] 90%+ 달성 시 PASS, 미달 시 이슈 등록
- **완료 조건**: Safety-Critical 4/4 PASS 또는 미달 이슈 등록

### T3: Standard 모듈 커버리지 현황 리포트 [P2]
- **설명**: Common, Dicom, SystemAdmin 및 기타 모듈 현황 업데이트
- **체크리스트**:
  - [ ] 전체 모듈별 line/branch coverage 기록
  - [ ] S16-R2 대비 변화량 분석
  - [ ] 85% 미달 모듈 목록 업데이트
  - [ ] `TestReports/S17-R1/FINAL-COVERAGE.md` 작성
- **완료 조건**: 전체 모듈 커버리지 현황 리포트 작성

### T4: 최종 판정 [P1]
- **설명**: S17-R1 PASS / CONDITIONAL PASS / FAIL 판정
- **체크리스트**:
  - [ ] Build Gate: 0 errors
  - [ ] Test Gate: 99%+ success rate
  - [ ] Safety-Critical Gate: 4/4 모듈 90%+
  - [ ] Standard Gate: 개선 추세 확인
- **완료 조건**: 최종 판정 기록 + 이슈 업데이트

### T5: DISPATCH Status 실시간 업데이트
- **완료 조건**: 타임스탬프 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 전체 빌드 + 테스트 재측정 | NOT_STARTED | QA | P1 | - | - |
| T2 | Safety-Critical 4/4 검증 | NOT_STARTED | QA | P1 | - | Issue #109 해소 확인 |
| T3 | Standard 모듈 커버리지 리포트 | NOT_STARTED | QA | P2 | - | - |
| T4 | 최종 판정 | NOT_STARTED | QA | P1 | - | PASS / CONDITIONAL / FAIL |
| T5 | DISPATCH Status 업데이트 | NOT_STARTED | QA | P3 | - | 상시 |

---

## Constraints

- [HARD] 구현 금지 — QA는 검증 전용
- [HARD] PASS/FAIL 판정은 QA 독립 권한 (CC가 번복 불가)
- [HARD] 소유 도구만 사용: `dotnet build`, `dotnet test`, Coverlet, Stryker, SonarCloud, StyleCop, OWASP 도구
- [HARD] ScheduleWakeup(1020초) 유지 — 작업 완료 push 직후 재설정 (Phase 3, _CURRENT.md)

## Evidence Required

완료 보고 시:
1. `dotnet build` 0 errors 증명
2. 전체 테스트 결과 (PASS/FAIL/SKIP 수치)
3. Safety-Critical 4개 모듈 커버리지 % 테이블
4. 최종 판정 (PASS | CONDITIONAL PASS | FAIL)
5. `TestReports/S17-R1/FINAL-COVERAGE.md`

---

## 참고 문서

- `.claude/rules/teams/qa.md` — QA 소유권 + 독립성
- `.claude/rules/teams/quality-standards.md` — Quality Metrics Table (SSOT)
- `TestReports/S16-R2/` — 이전 측정 결과
- Issue #109 — Security coverage gap
