---
id: SPEC-SPEC-TRIAGE-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
parent: SPEC-METHODOLOGY-001
---

## Definition of Done

- [ ] 5개 요구사항(REQ-TRIAGE-001~005) 모두 EARS 형식
- [ ] 7개 기존 SPEC 분류 매트릭스 표 (REQ-TRIAGE-002) 작성
- [ ] SPEC frontmatter 필수 필드 표 (REQ-TRIAGE-004) 작성
- [ ] 마이그레이션 액션 리스트 6항목(A1~A6) 작성
- [ ] SUPERSEDED 마킹 절차 5단계 (REQ-TRIAGE-003) 명시

---

## 시나리오 1: 7개 SPEC 분류 정확성

**Given** `.moai/specs/` 디렉토리에 7개 SPEC이 존재하는 상태에서
**When** 본 SPEC의 REQ-TRIAGE-002 매트릭스를 적용하면
**Then** 다음 분류 결과가 도출되어야 한다:
- ACTIVE: 5개 (COORDINATOR-001, INFRA-002, TEAMB-COV-001, TEAMB-FIX-001, UI-001)
- COMPLETED: 1개 (INFRA-001 — 머지 증거 확인 후 확정)
- DEPRECATED: 1개 (GOVERNANCE-001)

검증 방법:
- 각 SPEC의 frontmatter `status`, `updated` 필드 + git log 활동 분석.
- 분류 결과가 매트릭스와 일치하지 않으면 BLOCKED.

---

## 시나리오 2: SPEC-UI-001 메타데이터 보강

**Given** SPEC-UI-001/spec.md frontmatter에 status/priority/team 필드가 누락된 상태에서
**When** A1 액션이 실행되면
**Then** 누락된 3개 필드가 보강되어야 한다.

검증 방법:
- `grep -E "^(status|priority|team):" .moai/specs/SPEC-UI-001/spec.md` ≥ 3
- 위반 발견 시 SPEC-UI-001은 ACTIVE 분류 자격 미달 → 보강 후 재분류.

---

## 시나리오 3: GOVERNANCE-001 SUPERSEDED 처리

**Given** SPEC-METHODOLOGY-001 status가 `approved`로 전환되고 A3 액션이 실행되는 상황에서
**When** REQ-TRIAGE-003 절차가 적용되면
**Then**:
- SPEC-GOVERNANCE-001/spec.md frontmatter에 `status: deprecated`, `superseded_by: SPEC-METHODOLOGY-001` 추가됨
- HISTORY 섹션에 SUPERSEDED 기록 추가됨
- 본문에 안내 박스 추가됨
- 파일 자체는 60일간 현 위치 유지

검증 방법:
- `grep "superseded_by: SPEC-METHODOLOGY-001" .moai/specs/SPEC-GOVERNANCE-001/spec.md` ≥ 1
- HISTORY 섹션에 SUPERSEDED 행 존재.

---

## 시나리오 4: SPEC frontmatter 표준화

**Given** 모든 ACTIVE SPEC이 분류된 상태에서
**When** REQ-TRIAGE-004 필수 필드 검증을 수행하면
**Then** 5개 ACTIVE SPEC 모두 9개 필수 필드(id, version, status, priority, team, title, created, updated, [parent or supersedes])를 보유해야 한다.

검증 방법:
- 각 ACTIVE SPEC에 대해 `grep -E "^(id|version|status|priority|team|title|created|updated):" spec.md` 결과 ≥ 8.

---

## 시나리오 5: RTM 갱신 보고서 생성

**Given** A1~A4 액션이 완료된 상태에서
**When** A5 액션(RTM 갱신 보고서)이 실행되면
**Then** `.moai/reports/spec-triage-S18-R1.md` 파일이 생성되어야 하며, 다음 섹션을 포함해야 한다:
- 분류 매트릭스 결과 표
- 메타데이터 보강 작업 결과
- SUPERSEDED 마킹 결과
- RTM 표 갱신 요약

검증 방법:
- 파일 존재 확인.
- 파일 본문에 4개 섹션 헤더 모두 존재.

---

## 엣지 케이스

- **INFRA-001 머지 증거 부재**: 머지 PR 증거가 확인되지 않으면 ACTIVE로 재분류 (보수적 분류 원칙).
- **30일/60일 임계 분쟁**: SPEC의 `updated` 필드와 git 활동 중 더 최근 시점을 활동 시점으로 간주.
- **여러 SPEC이 동일 SPEC을 supersedes**: 가장 최근 updated의 SPEC이 우선 (HISTORY 표에 충돌 기록).

---

## Quality Gate

- 7개 SPEC 분류 정확도 100% ✓
- SPEC frontmatter 필수 필드 9개 표준화 ✓
- SUPERSEDED 마킹 본 SPEC에서 직접 편집하지 않음 (오케스트레이터 task) ✓
- 시간 예측 사용 0회 ✓
