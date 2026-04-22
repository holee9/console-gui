# DISPATCH S16-R2 — RA (Regulatory Affairs)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: RA
## Priority: HIGH (CMP 승격 + Governance SPEC 실행)
## 근거 SPEC/문서: SPEC-GOVERNANCE-001 + DOC-042 CMP Priority

---

## 배경

RA 우선순위(`ra.md` Priority Tasks):
1. **DOC-042 CMP (Configuration Management Plan) — Draft** → v2.0 승격 필요
2. RMP v2.0 업데이트 (2026-05 계획) — 4-Tier 우선순위 + MR-072 통합
3. FDA 510(k) submission 패키지 준비 (DOC-036 eSTAR)

SPEC-GOVERNANCE-001은 plan/acceptance 산출물이 이미 완비된 상태로 실행 대기 중.
이번 라운드에서 CMP v2.0 작성 + SPEC-GOVERNANCE-001 실행 착수.

---

## Tasks

### T1: DOC-042 CMP Draft → v2.0 승격 [P1]
- **설명**: `docs/management/DOC-042_CMP_*.md` Draft 완성
- **체크리스트**:
  - [ ] 현재 DOC-042 위치 및 Draft 상태 확인
  - [ ] IEC 62304 Section 8 Configuration Management 요구사항 대조
  - [ ] 형상 항목 식별(Source Code, Documentation, Test Artifacts, SOUP, 설정 파일)
  - [ ] 변경 통제 프로세스 기술 (PR 기반)
  - [ ] 빌드 식별 및 릴리즈 식별 정책
  - [ ] 버전: Draft → v2.0 Approved
- **완료 조건**: DOC-042 v2.0 파일 생성 + commit

### T2: SPEC-GOVERNANCE-001 현황 재확인 및 착수 [P2]
- **설명**: `.moai/specs/SPEC-GOVERNANCE-001/` 완비된 acceptance 기준 기반 실행
- **체크리스트**:
  - [ ] `.moai/specs/SPEC-GOVERNANCE-001/spec.md`, `plan.md`, `acceptance.md` 재읽기
  - [ ] 수용 기준 중 1개 이상 구체적 산출물 작성 착수
  - [ ] 진행 기록: `.moai/specs/SPEC-GOVERNANCE-001/progress.md` 신규 생성
- **완료 조건**: progress.md에 첫 진입 기록 + 1개 수용 기준 활성화

### T3: 임플리멘테이션 추적성 감사 [P2]
- **설명**: S14-R2 이후 변경된 구현 사항이 문서에 반영되었는지 점검
- **체크리스트**:
  - [ ] 최근 30일 `git log` 기반 SWR 추적성 확인
  - [ ] SWR → TC 매핑 누락 항목 식별 (DOC-032 RTM)
  - [ ] 누락 항목 `ra-update` + `priority-high` 이슈 등록
- **완료 조건**: 추적성 갭 목록 작성 또는 "갭 없음" 확인 리포트

### T4: DISPATCH Status 실시간 업데이트
- **완료 조건**: 타임스탬프 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | DOC-042 CMP v2.0 승격 | NOT_STARTED | RA | P1 | - | Draft → v2.0 |
| T2 | SPEC-GOVERNANCE-001 착수 | NOT_STARTED | RA | P2 | - | progress.md + 수용 기준 1개 |
| T3 | 임플리멘테이션 추적성 감사 | NOT_STARTED | RA | P2 | - | SWR → TC |
| T4 | DISPATCH Status 업데이트 | NOT_STARTED | RA | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 문서만 수정: `docs/regulatory/`, `docs/planning/`, `docs/risk/`, `docs/verification/`, `docs/management/`, `docs/development/`, `docs/research/`, `docs/archive/`, `docs/docfx/`
- [HARD] 공동 소유 디렉토리(`docs/architecture/` with Coordinator, `docs/deployment/` with QA)는 해당 팀과 협의 후 수정
- [HARD] IEC 62304 문서 버전 정책 준수 (major vs minor)
- [HARD] 코드 수정 금지 — 문서만 담당
- [HARD] ScheduleWakeup(300초) 유지

## Evidence Required

완료 보고 시:
1. DOC-042 v2.0 파일 경로 + 섹션 수
2. SPEC-GOVERNANCE-001 progress.md 내용 요약
3. 추적성 감사 결과 (갭 수 또는 "0")
4. 생성된 이슈 번호 (해당 시)

---

## 참고 문서

- `.moai/specs/SPEC-GOVERNANCE-001/spec.md`, `plan.md`, `acceptance.md`
- `docs/management/` — CMP 대상 디렉토리
- `docs/verification/DOC-032_RTM*.md` — 추적성 매트릭스
- `.claude/rules/teams/ra.md` — 문서 버전 정책
