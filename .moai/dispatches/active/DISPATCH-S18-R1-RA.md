# DISPATCH S18-R1 — RA (Regulatory Affairs)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: RA
## Priority: P2-High
## 근거 SPEC/문서: SPEC-SPEC-TRIAGE-001 (액션 A2) + SPEC-METHODOLOGY-001 + DOC-032 RTM

---

## 배경

S17-R1에서 SPEC-GOVERNANCE-001 추적성 감사는 SUPERSEDED로 종결.
신 방법론 발효에 따라 SPEC 재고 정리(TRIAGE A2~A4) + RTM 갱신.

---

## Tasks

### T1: SPEC-INFRA-001 → COMPLETED 정식 분류 [P1] (TRIAGE A2)
- 설명: SPEC-INFRA-001(status: implemented) 머지 PR 증거 수집 + COMPLETED 정식 분류
- 체크리스트:
  - [ ] 머지 PR 식별 (git log + Gitea API)
  - [ ] frontmatter status: completed로 변경
  - [ ] HISTORY 항목 추가
  - [ ] `.moai/specs/_archive/completed/` 디렉토리 생성 + 이동
- 완료 조건: SPEC-INFRA-001 archive 완료, RTM 갱신

### T2: methodology-migration-S18-R1 보고서 검증 [P1]
- 설명: `.moai/reports/methodology-migration-S18-R1.md` RTM 매핑 검증
- 체크리스트:
  - [ ] GOVERNANCE-001 8 REQ → 신 SPEC 매핑 정확성 확인
  - [ ] 누락된 사고 케이스 점검
- 완료 조건: 매핑 정확 확인 또는 보정안 제출

### T3: DOC-032 RTM 갱신 [P2]
- 설명: 신 6 SPEC을 RTM에 등재 + GOVERNANCE-001 SUPERSEDED 표기
- 완료 조건: DOC-032 v? → v? 갱신

### T4: Phase 4 게이트 검증 [P2, 신규]
- Phase 1+2+3 완료 후 진행

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | SPEC-INFRA-001 archive | NOT_STARTED | RA | P1 | - | TRIAGE A2 |
| T2 | 마이그레이션 보고서 검증 | NOT_STARTED | RA | P1 | - | 신규 |
| T3 | DOC-032 RTM 갱신 | NOT_STARTED | RA | P2 | - | - |
| T4 | Phase 4 게이트 | NOT_STARTED | RA | P2 | - | 신규 |

---

## Constraints

- [HARD] RA 소유: docs/regulatory/, docs/planning/, docs/risk/, docs/verification/, docs/management/, scripts/ra/, docfx.json
- [HARD] 빌드 불필요 — 문서 작업만
- [HARD] Phase 1+2+3 미완료 시 IDLE 대기
- [HARD] 실질 커밋 없으면 라운드 무효
- [HARD] ScheduleWakeup(1080초) — Phase 4

## Evidence Required

1. SPEC-INFRA-001 archive 이동 결과 (`git mv` evidence)
2. RTM 갱신 diff
3. methodology-migration-S18-R1 검증 의견
4. 실질 커밋 SHA

---

## 참고 문서

- `.moai/specs/SPEC-SPEC-TRIAGE-001/`, `SPEC-METHODOLOGY-001/`, `SPEC-INFRA-001/`
- `.moai/reports/methodology-migration-S18-R1.md`
- `docs/verification/RTM_v2.md` (DOC-032)
- `.claude/rules/teams/ra.md`
