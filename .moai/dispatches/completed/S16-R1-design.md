# DISPATCH S16-R1 — Design (Pure UI)

## Sprint: S16 | Round: R1
## Team: Design
## Priority: High

---

## 중요: ScheduleWakeup 설정이 최우선

**이전 라운드에서 Design이 2회 연속 TIMEOUT되었습니다. 원인은 ScheduleWakeup 미설정입니다.**
**이번 세션에서는 어떤 작업보다 ScheduleWakeup(300초)을 먼저 설정하세요.**

---

## Tasks

### T1: ScheduleWakeup 필수 설정 [CRITICAL — 최우선]
- **설명**: 이전 2라운드 연속 TIMEOUT 원인. /clear 후 세션 재시작 시 ScheduleWakeup이 소멸됨
- **체크리스트**:
  - [ ] _CURRENT.md에서 '팀 모니터링 설정' ScheduleWakeup 값 읽기 (300초)
  - [ ] ScheduleWakeup(300) 실행
  - [ ] ScheduleWakeup 설정 성공 확인
  - [ ] 실패 시 CC에 즉시 BLOCKED 보고
- **완료 조건**: ScheduleWakeup 활성 상태

### T2: IDLE CONFIRM
- **설명**: S15-R3 TIMEOUT 완료. 이번 라운드에서 수행할 새로운 UI 작업이 있는지 확인
- **체크리스트**:
  - [ ] _CURRENT.md에서 S16-R1 ACTIVE 확인
  - [ ] 기존 UI 모듈(Views, Styles, Themes) 빌드 정상 확인
  - [ ] DISPATCH Status NOT_STARTED → IN_PROGRESS 업데이트
- **완료 조건**: IDLE CONFIRM + DISPATCH Status COMPLETED

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | ScheduleWakeup 필수 설정 | NOT_STARTED | Design | P1 | - | CRITICAL — 최우선 |
| T2 | IDLE CONFIRM | NOT_STARTED | Design | P2 | - | - |

---

## Constraints
- 소유 모듈만 수정: Views, Styles, Themes, Components, Converters, Assets, DesignTime/
- PPT 지정 페이지 외 UI 요소 구현 절대 금지
- DISPATCH 없는 자율 작업 금지
