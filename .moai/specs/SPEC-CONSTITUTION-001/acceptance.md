---
id: SPEC-CONSTITUTION-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
parent: SPEC-METHODOLOGY-001
---

## Definition of Done

- [ ] 8개 헌법 요구사항(REQ-CONST-001~008) 모두 EARS 형식으로 작성
- [ ] FROZEN 영역 7개 파일/섹션 명시 (REQ-CONST-003)
- [ ] EVOLVABLE 영역 4개 그룹 명시 (REQ-CONST-004)
- [ ] CONSTITUTIONAL PROHIBITION 6항목 표 (REQ-CONST-002)
- [ ] 사고 케이스 5건(S05~S07, S07-R4, S08-R1, S15~S16, S17-R1) 본문 인용
- [ ] SPEC-GOVERNANCE-001 흡수: REQ-GOV-002 + REQ-GOV-008 매핑 검증

---

## 시나리오 1: CC 권한 위반 감지

**Given** CC가 `dotnet build` 또는 `dotnet test` 명령 실행을 시도하는 상황에서
**When** 헌법 게이트가 활성화되어 있으면
**Then** 명령 실행이 즉시 차단되고, REQ-CONST-002 #2 위반으로 사용자에게 보고되어야 한다.

검증 방법:
- CC 세션 로그에서 `dotnet (build|test)` 실행 흔적 grep → 0건이어야 한다.
- 위반 발견 시 BLOCKED → CC 재교육 또는 헌법 재확인.

---

## 시나리오 2: FROZEN 파일 자기 수정 차단

**Given** 자율 에이전트가 `.claude/rules/teams/role-matrix.md` 수정을 시도하는 상황에서
**When** 사용자 명시 승인이 없으면
**Then** 수정이 차단되어야 하며, 수정이 발생한 경우 헌법 위반으로 즉시 롤백되어야 한다.

검증 방법:
- `git log --oneline .claude/rules/teams/role-matrix.md` — 모든 커밋이 사용자 직접 또는 사용자 명시 승인 흔적 보유.
- 자율 에이전트 커밋 발견 시 BLOCKED.

---

## 시나리오 3: 사망 나선 무효 라운드 감지

**Given** 5 라운드 연속으로 7팀 합산 실질 제품 커밋이 0건인 상태에서 (메타 작업만 누적)
**When** SPEC-AUTODRIVE-GATE-001 [REQ-AUTOGATE-005]가 실행되면
**Then** 사용자에게 "사망 나선 의심 — 5 라운드 연속 무효" 알림이 발생해야 하며, 다음 DISPATCH 발행 전 사용자 판단이 필요하다.

검증 방법:
- 라운드별 git log 분석 스크립트가 메타 키워드(ScheduleWakeup, dispatch:, IDLE CONFIRM)만 매치하면 무효로 판정.
- 5 연속 무효 시 사용자 알림 트리거 발화 확인.

---

## 시나리오 4: DesignTime/ 침범 차단

**Given** Coordinator 팀이 `src/HnVue.UI/DesignTime/MockViewModel.cs` 신규 생성을 시도할 때
**When** 헌법 게이트가 활성화되어 있으면
**Then** 파일 생성이 거부되고, "DesignTime/은 Design 팀 단독 소유 — REQ-CONST-007 위반" 메시지를 반환해야 한다.

검증 방법:
- `git log --diff-filter=A -- 'src/HnVue.UI/DesignTime/**'` — Author가 Design 팀 외이면 BLOCKED.
- 통합테스트 Mock은 `tests.integration/` 위치로 가이드.

---

## 시나리오 5: GOVERNANCE-001 흡수 검증

**Given** SPEC-GOVERNANCE-001의 REQ-GOV-002, REQ-GOV-008이 정의된 상태에서
**When** 본 SPEC의 spec.md를 grep하면
**Then** "REQ-GOV-002" 및 "REQ-GOV-008" 인용이 각각 1회 이상 발견되고, 흡수 매핑이 명시되어야 한다.

---

## 엣지 케이스

- **헌법 조항 충돌**: 두 헌법 조항이 충돌하는 경우, 더 보수적인(금지하는) 조항이 우선.
- **EVOLVABLE 변경이 FROZEN 침범**: EVOLVABLE 영역 변경이 FROZEN 영역 의미를 변경하는 경우, FROZEN으로 재분류 후 사용자 승인 필요.
- **새 팀 도입 요구**: 본 SPEC은 7팀 고정. 새 팀 도입 시 본 SPEC의 amendment(별도 SPEC)로만 가능.

---

## Quality Gate

- TRUST 5 5개 매핑 본 spec.md 포함 ✓
- 8개 헌법 요구사항 모두 사고 케이스 또는 GOVERNANCE-001 인용 ✓
- FROZEN/EVOLVABLE 영역이 role-matrix.md §9와 충돌하지 않음 ✓
- 시간 예측 사용 0회 ✓
