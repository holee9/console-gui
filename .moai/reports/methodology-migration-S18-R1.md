# Methodology Migration Report — S18-R1

> Source: SPEC-METHODOLOGY-001 REQ-METHODOLOGY-005
> Effective: 2026-05-10
> Target Round: S18-R1 (first round under new methodology)
> Status: **DRAFT — pending S17-R1 freeze release**

---

## 1. Migration Trigger

S17-R1 진행 중 사용자가 메타작업(자율주행 개발방법론 재정립)을 지시.
다음 사고 패턴이 누적되어 더 이상 패치-on-패치로 해결 불가:

- **사망 나선 (S15~S16-R1)**: 3 Sprint, 15+ 라운드 동안 실질 제품 커밋 0건
- **Phase 동기화 실패 (S14-R2)**: main pull 누락이 Trait 87건 누락으로 확산
- **무한 대기 (S15-R2)**: Design 미응답으로 라운드 진행 불가
- **소유권 침범 반복 (S08, S09)**: 디렉토리 단위 소유권 명세 부족

해결책: 6개 SPEC군(메타 + 5축)으로 헌법화.

## 2. SPEC 발효 매트릭스

| SPEC | 발효 시점 | Owner | 핵심 효과 |
|------|----------|-------|----------|
| SPEC-METHODOLOGY-001 | S18-R1 발행 시점 | 사용자 | 5축 통합, GOVERNANCE-001 SUPERSEDED |
| SPEC-CONSTITUTION-001 | S18-R1 동시 | 사용자 | 7팀 헌법 + CC 권한 경계 + 사망 나선 헌법 조항 |
| SPEC-DISPATCH-V2-001 | S18-R1 동시 | CC + 사용자 | Phase 종속성 강제, Stall/TIMEOUT, DISPATCH v2 |
| SPEC-AUTODRIVE-GATE-001 | S18-R2 활성화 | QA + 사용자 | Evidence-Based Completion 자동화 |
| SPEC-SPEC-TRIAGE-001 | S18-R1 ~ S18-R2 | 사용자 | 7개 기존 SPEC 분류 정리 |
| SPEC-ROADMAP-001 | S18-R1 부터 | 사용자 + CC | S18+ 라운드 계획 |

## 3. 헌법 동기화 작업 (2026-05-10 완료)

| 파일 | 변경 | 버전 |
|------|------|------|
| `.claude/rules/teams/role-matrix.md` | §8 사망 나선 등재 + §9 6개 신규 SPEC 거버넌스 매트릭스 | 5.1.0 → 5.2.0 |
| `CLAUDE.md` | §5 메타 SPEC 인덱스 + §7 Self-Verification 7항목 | (동기화) |
| `.claude/rules/teams/dispatch-protocol.md` | §1·§6 SPEC-DISPATCH-V2-001 REQ 인용 | (동기화) |
| `.claude/rules/teams/quality-standards.md` | §2 사망 나선 메트릭 2행 + §3 7번째 체크 | (동기화) |
| `.moai/specs/SPEC-GOVERNANCE-001/spec.md` | SUPERSEDED by METHODOLOGY-001 | 1.0.0 → 1.1.0 |

## 4. S17-R1 Freeze 해제 조건

S17-R1 freeze는 사용자가 명시적으로 해제할 때까지 유지. 해제 절차:

1. SPEC 6건 + 헌법 동기화 PR 사용자 검토 완료
2. PR 머지 후 `_CURRENT.md` FROZEN 헤더 제거
3. CC v2가 S17-R1 ACTIVE 5팀 재개 안내 또는 S18-R1 신 라운드 발행
4. 신 라운드는 SPEC-DISPATCH-V2-001 REQ-DISPATCH-V2-001 (FIRST ACTION) + REQ-DISPATCH-V2-003 (Phase 종속성) 적용

## 5. S18-R1 첫 라운드 가이드

S18-R1 발행 시 CC가 만족해야 할 조건:

- DISPATCH 파일 명명: `DISPATCH-S18-R1-{TEAM}.md`
- 각 DISPATCH는 **반드시** 근거 SPEC 또는 문서 인용 (헌법 조항)
- _CURRENT.md FROZEN 표기 제거
- Round Issue (Gitea) 신규 생성 + SPEC-METHODOLOGY-001 발효 명시
- ScheduleWakeup 값 _CURRENT.md에서 읽기 (하드코딩 금지)
- Evidence-Based Completion: `dotnet build HnVue.sln` + `dotnet test` 자기 소유 + git diff 증거 의무

## 6. 위험 요인 및 대응

| 위험 | 대응 |
|------|------|
| 6 SPEC 동시 발효로 팀 혼란 | S18-R1은 CONSTITUTION+DISPATCH-V2만, AUTODRIVE-GATE는 R2 활성화로 단계 도입 |
| 기존 SPEC(INFRA-002 등)과 충돌 | TRIAGE-001로 명시 분류, ACTIVE SPEC은 신 방법론과 호환 검증 |
| FROZEN 변경에 대한 팀 저항 | 사용자 단독 승인 + 사고 케이스 8건 인용으로 정당성 확보 |
| 사망 나선 감지 false positive | Substantive Commit 정의 명확화 (소스/문서/SPEC = 실질, 프로토콜/스케줄 = 비실질) |

## 7. RTM 매핑 요약 (GOVERNANCE-001 → 신 SPEC군)

| 구 REQ | 신 REQ | 흡수 위치 |
|--------|--------|----------|
| REQ-GOV-001 (커밋 의무) | REQ-AUTOGATE-009 | SPEC-AUTODRIVE-GATE-001 |
| REQ-GOV-002 (워크트리 계층) | REQ-CONST-001 | SPEC-CONSTITUTION-001 |
| REQ-GOV-003 (Coordinator 통합) | REQ-DISPATCH-V2-003 | SPEC-DISPATCH-V2-001 |
| REQ-GOV-004 (Issue-First) | REQ-DISPATCH-V2-007 | SPEC-DISPATCH-V2-001 |
| REQ-GOV-005 (한글 인코딩) | REQ-DISPATCH-V2-008 | SPEC-DISPATCH-V2-001 |
| REQ-GOV-006 (QA 보고서) | REQ-AUTOGATE-008 | SPEC-AUTODRIVE-GATE-001 |
| REQ-GOV-007 (수명주기 이행) | REQ-DISPATCH-V2-010 | SPEC-DISPATCH-V2-001 |
| REQ-GOV-008 (블로커 이슈) | REQ-CONST-008 | SPEC-CONSTITUTION-001 |

---

Author: MoAI Orchestrator
Cross-ref: SPEC-METHODOLOGY-001, SPEC-CONSTITUTION-001, SPEC-DISPATCH-V2-001, SPEC-AUTODRIVE-GATE-001, SPEC-SPEC-TRIAGE-001, SPEC-ROADMAP-001
