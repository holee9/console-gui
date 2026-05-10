---
id: SPEC-METHODOLOGY-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
---

## Definition of Done

본 메타 SPEC은 다음 기준을 모두 만족할 때 `implemented` 상태로 전환된다:

- [ ] 5개 부속 SPEC 디렉토리(.moai/specs/SPEC-{CONSTITUTION|DISPATCH-V2|AUTODRIVE-GATE|SPEC-TRIAGE|ROADMAP}-001/) 모두 존재
- [ ] 각 부속 SPEC에 spec.md + acceptance.md 파일 모두 존재
- [ ] 각 부속 SPEC frontmatter에 `parent: SPEC-METHODOLOGY-001` 명시
- [ ] SPEC-GOVERNANCE-001 흡수 매핑 표(REQ-METHODOLOGY-002)의 8개 매핑 모두 부속 SPEC에서 검증 가능
- [ ] 마이그레이션 보고서(.moai/reports/methodology-migration-S18-R1.md) 생성
- [ ] 부속 SPEC 의존 그래프가 단방향(REQ-METHODOLOGY-004 명세 일치)
- [ ] role-matrix.md §8 사고 8건이 부속 SPEC 요구사항 본문에 1회 이상 인용

---

## 시나리오 1: 5축 부속 SPEC 일관성 검증

**Given** SPEC-METHODOLOGY-001과 5개 부속 SPEC 파일이 모두 작성된 상태이고
**When** 메타 SPEC RTM 표와 각 부속 SPEC의 `parent:` 필드를 자동 비교하면
**Then** 매핑 일치율이 100%이고, 누락된 부속 SPEC ID가 없어야 한다.

검증 방법:
- `grep -l "parent: SPEC-METHODOLOGY-001" .moai/specs/SPEC-*/spec.md`의 결과가 정확히 5개여야 한다.
- 5개 ID는 {CONSTITUTION-001, DISPATCH-V2-001, AUTODRIVE-GATE-001, SPEC-TRIAGE-001, ROADMAP-001}이어야 한다.

---

## 시나리오 2: GOVERNANCE-001 흡수 커버리지

**Given** SPEC-GOVERNANCE-001의 REQ-GOV-001~008이 정의되어 있고
**When** 5개 부속 SPEC에서 각 REQ-GOV-NNN을 인용한 횟수를 집계하면
**Then** 8개 REQ-GOV 항목 모두 최소 1회 이상 인용되어야 하며, 누락 0건이어야 한다.

검증 방법:
- `grep -c "REQ-GOV-001" .moai/specs/SPEC-{...}-001/spec.md` ≥ 1
- 8개 REQ-GOV에 대해 동일 검증 반복

---

## 시나리오 3: 단방향 의존 그래프

**Given** 5개 부속 SPEC이 작성된 상태에서
**When** 각 부속 SPEC의 cross-reference를 추출하면
**Then** 다음 의존 그래프가 형성되어야 한다:
- CONSTITUTION-001은 다른 부속 SPEC을 참조하지 않는다 (root)
- DISPATCH-V2-001은 CONSTITUTION-001만 참조 가능
- AUTODRIVE-GATE-001은 CONSTITUTION-001, DISPATCH-V2-001 참조 가능
- SPEC-TRIAGE-001은 CONSTITUTION-001만 참조 가능
- ROADMAP-001은 4개 모두 참조 가능
- 순환 참조 0건

검증 방법:
- 각 부속 SPEC에서 `SPEC-{...}-001` 패턴을 grep
- 의존 그래프 위반 시 BLOCKED

---

## 시나리오 4: 사고 케이스 인용 완결성

**Given** role-matrix.md §8에 8건의 사고가 기록된 상태에서
**When** 부속 SPEC 5개의 spec.md 본문에서 사고 시그너처(S05~S07, S07-R4, S08-R1, S09-R3, S14-R2, S15-R2, S15~S16, S17-R1)를 grep하면
**Then** 8건 모두 최소 1회 이상 인용되어야 한다.

검증 방법:
- 각 사고 시그너처에 대해 `grep -l "S14-R2"` 등의 형식으로 부속 SPEC 본문 검색
- 누락 시 해당 부속 SPEC의 본문 보강 필요

---

## 시나리오 5: 마이그레이션 보고서 생성

**Given** SPEC-METHODOLOGY-001 status가 `approved`로 전환되고
**When** 오케스트레이터가 마이그레이션 단계 M1~M3을 실행하면
**Then** `.moai/reports/methodology-migration-S18-R1.md` 파일이 생성되어야 하며, 다음 섹션을 포함해야 한다:
- GOVERNANCE-001 → 5축 흡수 매핑 결과
- 7개 기존 SPEC 분류 결과 (ACTIVE/COMPLETED/DEPRECATED)
- 사용자 수행 액션 리스트

---

## 엣지 케이스

- **부속 SPEC 1개 DEPRECATED 시**: METHODOLOGY-001의 RTM 표 갱신 + HISTORY에 사유 기록 (REQ-METHODOLOGY-003)
- **GOVERNANCE-001 일부 요구사항 흡수 누락 발견 시**: 해당 부속 SPEC HISTORY에 보강 항목 추가 후 status: draft 회귀
- **부속 SPEC 간 순환 참조 발견 시**: 의존 그래프 위반 BLOCKED → 부속 SPEC 본문 재구조화 필요

---

## Quality Gate

- TRUST 5 매핑 표 본 spec.md 포함 ✓
- 모든 EARS 요구사항이 측정 가능한 acceptance와 1:1 연결 ✓
- 비목표(NG-1~NG-5) 위반 0건 ✓
- 시간 예측 사용 0회 ✓
