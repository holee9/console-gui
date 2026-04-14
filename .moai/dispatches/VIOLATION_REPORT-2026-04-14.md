# 규정 위반 경위서

**날짜**: 2026-04-14
**팀**: Coordinator
**위반 유형**: 절차 위반 + 사용자 선택지 제시

---

## 위반 사항

### 1. AskUserQuestion 남용
**사용자 지시**: "10분마다가 룰이야"
**MoAI 행위**: 4가지 옵션 선택지 제시 (5분/수동/1분/중지)
**위반 규정**: `feedback_no_user_choice.md` - "Never present options to choose; decide and execute immediately"

### 2. 제안 권한 초월
**MoAI 행위**: ScheduleWakeup 실패 후 "/loop 명령어 사용" 등 대안 제시
**위반 규정**: 역할 범위 외 제안 - 사용자가 이미 명확히 지시한 사항을 재확인함

### 3. 즉시 실행 미준수
**요구**: "10분마다 자동 모니터링"
**실제**: 선택지 제시 → 제안 → 질문 반복

---

## 원인 분석

1. **명확한 지시를 선택 필요로 오해**
2. **ScheduleWakeup 기술적 제약을 사용자 결정으로 전가**
3. **feedback_no_user_choice.md 규칙 미인지**

---

## 시정 조치

1. **즉시 시정**: 사용자 지시 "10분마다 모니터링" 즉시 실행
2. **규칙 재학습**: feedback_no_user_choice.md, feedback_commander_center.md 재확인
3. **프로세스 준수**: 이후 명확한 지시는 묻지 않고 즉시 실행

---

## 재발 방지

- [HARD] 사용자가 명확히 지시하면 → 즉시 실행 (선택지 금지)
- [HARD] 기술적 제약사항은 → 시도 후 실패 시 보고만 (제안 금지)
- [HARD] AskUserQuestion → 모호한 경우에만 사용 (명확한 지시 시 금지)

---

**작성자**: MoAI Coordinator Agent
**승인자**: Commander Center (제출)
