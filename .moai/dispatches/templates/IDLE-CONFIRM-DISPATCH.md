# DISPATCH {SPRINT}-{ROUND} — {Team Name} [IDLE CONFIRM]

## Sprint: S{NN} | Round: R{M} | Issued: {YYYY-MM-DD}
## Team: {Team Name}
## Priority: P4-Low
## Type: **IDLE CONFIRM** (실질 업무 없음 — 상태 유지만 확인)
## 근거: {갭 분석 결과 이 팀에 할 업무 없음 — 구체적 이유 명시}

---

## [HARD] IDLE CONFIRM 사용 제한 규칙

- [HARD] IDLE CONFIRM은 **2라운드 연속 발행 금지** — 3라운드 연속 시 프로세스 사망 나선 경고
- [HARD] IDLE CONFIRM 발행 전 팀 소유 모듈/문서를 **반드시** 갭 분석 — 근거 SPEC 또는 개선 여지 없음을 증명
- [HARD] IDLE CONFIRM은 `STANDARD-DISPATCH.md` 대체재가 아님 — 예외적 수단
- [HARD] 실질 커밋 0건이 2라운드 이상 지속되면 사용자에게 즉시 보고

---

## Tasks

### T1: IDLE CONFIRM
- **설명**: {팀명} S{NN}-R{M-1} 완료 이후 신규 업무 없음을 확인
- **체크리스트**:
  - [ ] `_CURRENT.md`에서 S{NN}-R{M} ACTIVE 및 IDLE CONFIRM 상태 확인
  - [ ] ScheduleWakeup({300초 이상}) 재설정
  - [ ] 자기 소유 모듈 빌드 여전히 정상 확인 (`dotnet build {module}`)
  - [ ] DISPATCH Status NOT_STARTED → COMPLETED 업데이트 + 타임스탬프
- **완료 조건**: 빌드 정상 + Status COMPLETED + ScheduleWakeup 활성

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | IDLE CONFIRM | NOT_STARTED | {Team} | P4 | - | 실질 업무 없음 |

---

## Constraints

- 소유 모듈만 빌드 검증 (변경 금지)
- 다른 팀 작업 엿보기 금지
- 실질 개발 작업 금지 (DISPATCH 없는 자율 작업 = 프로토콜 위반)

---

## 이 팀에 할 일이 2라운드 연속 없는 경우

1. 갭 분석 결과를 `_CURRENT.md` DISPATCH 라운드 이력에 기록
2. 3라운드 연속 IDLE CONFIRM 발생 시 사용자에게 보고
3. 보고 내용: "S{NN}-R{M} ~ S{NN}-R{M+2} 3라운드 동안 {팀명}에게 실질 업무 할당 불가. 근본 원인 분석 필요."

---

Version: 1.1.0 (CC 역할 제거)
Effective: 2026-04-22
Cross-ref: `STANDARD-DISPATCH.md` (정상 템플릿)
