# DISPATCH S17-R1 — RA (Regulatory Affairs)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: RA
## Priority: P2-High (추적성 감사 + Governance SPEC 진행)
## 근거 SPEC/문서: SPEC-GOVERNANCE-001 + S16-R2 T3 미완료 (추적성 감사)

---

## 배경

S16-R2에서 RA가 T1(CMP v2.0), T2(SPEC-GOVERNANCE-001 progress)를 완료했으나,
**T3(임플리멘테이션 추적성 감사)가 NOT_STARTED**로 남음.
S14-R2 이후 다수의 구현 변경(Trait 87개, Repository 6개, Security/AES 등)이 문서에 반영되었는지
추적성 검증이 필요.

---

## Tasks

### T1: 임플리멘테이션 추적성 감사 [P1]
- **설명**: S16-R2에서 미완료한 T3 — SWR → TC 매핑 점검
- **체크리스트**:
  - [ ] 최근 30일 `git log` 기반 변경된 SWR 식별
  - [ ] `docs/verification/DOC-032_RTM*.md`에서 SWR → TC 매핑 확인
  - [ ] 매핑 누락 항목 리스트업
  - [ ] 누락 항목 있으면 `ra-update` + `priority-high` 이슈 등록
  - [ ] 갭 없으면 "추적성 갭 없음" 리포트 작성
- **완료 조건**: 추적성 감사 완료 + 누락 이슈 등록 또는 "갭 없음" 리포트

### T2: SPEC-GOVERNANCE-001 진행 계속 [P2]
- **설명**: S16-R2에서 시작한 progress.md 기반 다음 수용 기준 작업
- **체크리스트**:
  - [ ] `.moai/specs/SPEC-GOVERNANCE-001/progress.md` 재확인
  - [ ] 다음 수용 기준 활성화 + 산출물 작성
  - [ ] 진행 상황 progress.md 업데이트
- **완료 조건**: progress.md에 새 진입 기록 + 1개 수용 기준 진행

### T3: S16-R2→S17-R1 변경사항 문서 영향 평가 [P2]
- **설명**: S16-R2 구현팀 작업 결과가 규제 문서에 미치는 영향 평가
- **체크리스트**:
  - [ ] SPEC-INFRA-002 (AES-256-GCM) → DOC-019 SBOM, DOC-033 SOUP 영향?
  - [ ] SPEC-COORDINATOR-001 (Repository 6개) → DOC-032 RTM 영향?
  - [ ] SPEC-TEAMB-COV-001 (Coverage) → DOC-011 V&V 영향?
  - [ ] 영향 있는 경우 `ra-update` 이슈 등록
- **완료 조건**: 영향 평가 완료 + 필요 이슈 등록

### T4: DISPATCH Status 실시간 업데이트
- **완료 조건**: 타임스탬프 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 임플리멘테이션 추적성 감사 | NOT_STARTED | RA | P1 | - | S16-R2 T3 미완료 |
| T2 | SPEC-GOVERNANCE-001 진행 | NOT_STARTED | RA | P2 | - | progress.md |
| T3 | 변경사항 문서 영향 평가 | NOT_STARTED | RA | P2 | - | - |
| T4 | DISPATCH Status 업데이트 | NOT_STARTED | RA | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 문서만 수정: `docs/regulatory/`, `docs/planning/`, `docs/risk/`, `docs/verification/`, `docs/management/`, `docs/development/`, `docs/research/`, `docs/archive/`, `docs/docfx/`
- [HARD] 공동 소유 디렉토리는 해당 팀과 협의 후 수정
- [HARD] IEC 62304 문서 버전 정책 준수 (major vs minor)
- [HARD] 코드 수정 금지 — 문서만 담당
- [HARD] ScheduleWakeup(1080초) 유지 — 작업 완료 push 직후 재설정 (Phase 4, _CURRENT.md)

## Evidence Required

완료 보고 시:
1. 추적성 감사 결과 (갭 수 또는 "0")
2. SPEC-GOVERNANCE-001 progress.md 업데이트 내용
3. 문서 영향 평가 결과
4. 생성된 이슈 번호 (해당 시)

---

## 참고 문서

- `.moai/specs/SPEC-GOVERNANCE-001/spec.md`, `plan.md`, `acceptance.md`, `progress.md`
- `docs/verification/DOC-032_RTM*.md` — 추적성 매트릭스
- `docs/management/DOC-042_CMP*.md` — CMP v2.0
- `.claude/rules/teams/ra.md` — 문서 버전 정책
