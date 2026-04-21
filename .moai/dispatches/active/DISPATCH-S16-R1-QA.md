# DISPATCH S16-R1 — QA (Quality Assurance)

## Sprint: S16 | Round: R1
## Team: QA
## Priority: High

---

## 중요: ScheduleWakeup + node PATH 문제

**S15-R3에서 ScheduleWakeup 설정이 node 없음으로 실패했습니다.**
**PATH에 node가 있는지 먼저 확인하고, 없으면 대체 방안을 사용하세요.**

---

## Tasks

### T1: ScheduleWakeup 필수 설정 [CRITICAL — 최우선]
- **설명**: S15-R3에서 node 없음으로 ScheduleWakeup 실패. 대체 방안 필요
- **체크리스트**:
  - [ ] _CURRENT.md에서 '팀 모니터링 설정' ScheduleWakeup 값 읽기 (300초)
  - [ ] `which node` 확인 — node 없으면 PATH 추가 시도
  - [ ] ScheduleWakeup(300) 실행
  - [ ] 실패 시 CC에 BLOCKED 보고 (node PATH 문제)
- **완료 조건**: ScheduleWakeup 활성 또는 BLOCKED 보고

### T2: IDLE CONFIRM
- **설명**: S15-R3 BLOCKED (Bash 권한) 이후 상태 확인
- **체크리스트**:
  - [ ] _CURRENT.md에서 S16-R1 ACTIVE 확인
  - [ ] QA 권한(dotnet build/test) 가능 여부 확인
  - [ ] DISPATCH Status NOT_STARTED → IN_PROGRESS 업데이트
- **완료 조건**: IDLE CONFIRM + DISPATCH Status COMPLETED 또는 BLOCKED

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | ScheduleWakeup 필수 설정 | NOT_STARTED | QA | P1 | - | CRITICAL |
| T2 | IDLE CONFIRM | NOT_STARTED | QA | P2 | - | - |

---

## Constraints
- QA는 구현에 관여하지 않고 검증에만 관여
- 소유 도구만 사용: dotnet build/test, 커버리지, 정적분석
- PASS/FAIL 판정은 QA 독립 권한
