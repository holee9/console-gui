# DISPATCH S16-R1 — RA (Regulatory Affairs)

## Sprint: S16 | Round: R1
## Team: RA
## Priority: Medium

---

## Tasks

### T1: IDLE CONFIRM
- **설명**: S15-R3 MERGED 완료. 이번 라운드에서 수행할 새로운 규제 작업이 있는지 확인
- **체크리스트**:
  - [ ] _CURRENT.md에서 S16-R1 ACTIVE 확인
  - [ ] ScheduleWakeup(300초) 즉시 설정 — /clear 후 첫 액션으로
  - [ ] 규제 문서 상태 확인
  - [ ] DISPATCH Status NOT_STARTED → IN_PROGRESS 업데이트
- **완료 조건**: IDLE CONFIRM + ScheduleWakeup 설정 + DISPATCH Status COMPLETED

### T2: ScheduleWakeup 필수 설정 [CRITICAL]
- **설명**: /clear 후 세션 재시작 시 ScheduleWakeup이 소멸됨. IDLE 보고 전 반드시 재설정
- **체크리스트**:
  - [ ] _CURRENT.md에서 '팀 모니터링 설정' ScheduleWakeup 값 읽기 (300초)
  - [ ] ScheduleWakeup(300) 실행
  - [ ] ScheduleWakeup 설정 성공 확인
- **완료 조건**: ScheduleWakeup 활성 상태

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | IDLE CONFIRM | NOT_STARTED | RA | P2 | - | - |
| T2 | ScheduleWakeup 필수 설정 | NOT_STARTED | RA | P1 | - | CRITICAL |

---

## Constraints
- 소유 모듈만 수정: docs/regulatory/, docs/planning/, docs/risk/, docs/verification/, docs/management/
- IEC 62304 문서 버전 정책 준수
- DISPATCH 없는 자율 작업 금지
