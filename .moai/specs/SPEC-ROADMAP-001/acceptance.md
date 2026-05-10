---
id: SPEC-ROADMAP-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
parent: SPEC-METHODOLOGY-001
---

## Definition of Done

- [ ] 6개 요구사항(REQ-ROADMAP-001~006) 모두 EARS 형식
- [ ] 시간 예측 단어("일/주/월/이내") 본문 사용 0회
- [ ] 우선순위 라벨 P0/P1/P2만 사용
- [ ] 라운드 종속 그래프 본문 포함
- [ ] 사고 케이스 2건(S14-R2, S15~S16-R1) 인용
- [ ] 부속 SPEC 4개 모두 참조

---

## 시나리오 1: S18-R1 코드 변경 금지

**Given** S18-R1 라운드가 발행된 상태에서
**When** Team A 또는 Team B가 소스코드 변경 작업을 시도하면
**Then** REQ-ROADMAP-001 게이트에 의해 차단되고, "S18-R1은 방법론 정착 전용 — 코드 변경 금지" 메시지를 반환해야 한다.

검증 방법:
- S18-R1 종료 시점 git log 분석 — 소스코드(.cs/.xaml/.sql) 커밋 0건이어야 함.
- 위반 발견 시 BLOCKED + S18-R1 재실행.

---

## 시나리오 2: 라운드 종속 그래프 준수

**Given** 사용자가 S18-R3을 직접 발행하려 할 때 (S18-R2 미완료 상태)
**When** REQ-ROADMAP-002 + REQ-ROADMAP-003 종속이 검증되면
**Then** S18-R3 발행이 차단되고, "S18-R2 정상 종료 후 발행 가능" 메시지를 반환해야 한다.

검증 방법:
- `_CURRENT.md` 라운드 카운터 + 전 라운드 종료 상태 확인.
- S18-R2 미완료 시 S18-R3 발행 BLOCKED.

---

## 시나리오 3: 시간 예측 사용 금지

**Given** SPEC-ROADMAP-001/spec.md 본문에서
**When** 시간 단어 grep을 수행하면 (`-iE "[0-9]+\s?(일|주|month|week|day|시간|hour)"`)
**Then** 매치 0건이어야 한다.

검증 방법:
- grep -iE 패턴 매치 0건.
- 위반 발견 시 본 SPEC 재작성 필요.

---

## 시나리오 4: 라운드 종료 회귀 보고서 생성

**Given** S18-R1 라운드가 종료되는 상황에서
**When** REQ-ROADMAP-005가 동작하면
**Then** `.moai/reports/round-S18-R1-retro.md` 파일이 생성되어야 하며, 다음 항목을 포함해야 한다:
- 실질 커밋 수치 (S18-R1은 0이어야 정상)
- Self-Verification 7항목 통과율
- 사망 나선 카운터 변화
- TIMEOUT 발생 횟수

검증 방법:
- 파일 존재 + 4개 항목 모두 기록 확인.

---

## 시나리오 5: 사용자 결정 게이트 발화

**Given** S19에서 사망 나선 5 라운드 임계 도달
**When** REQ-ROADMAP-006이 동작하면
**Then** CC가 다음 라운드 발행을 정지하고, 사용자에게 명시 승인 요청 메시지를 발송해야 한다.

검증 방법:
- DISPATCH 발행 흔적 0건 (사용자 승인 전).
- 사용자 알림 트리거 발화.

---

## 엣지 케이스

- **S18-R1 SKIP 합리화**: 사용자가 명시적으로 S18-R1 SKIP 결정 시, 마이그레이션 보고서가 부재하므로 S18-R2에서 누락 보고서 생성 필요.
- **S18-R2와 S18-R3 병합 요청**: 사용자가 게이트 동시 활성화를 요청하면 본 SPEC amendment(별도 SPEC) 필요. 현 SPEC에서는 분리 라운드 강제.
- **S19에서 새 SPEC 발행 필요**: 현재 ROADMAP은 7개 SPEC만 다룸. 새 SPEC 발행 시 ROADMAP-002 amendment 필요.

---

## Quality Gate

- 6개 요구사항이 우선순위 라벨만 사용 ✓
- 시간 예측 0회 ✓
- 라운드 종속 그래프 본문 포함 ✓
- 부속 SPEC 4개 모두 참조 ✓
- 사용자 결정 게이트(REQ-ROADMAP-006) 4가지 트리거 명시 ✓
