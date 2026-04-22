# 팀 시작 프롬프트 사용법

각 팀 워크트리 터미널에서 `claude` 실행 후, 해당 팀의 프롬프트 파일 내용을 첫 메시지로 붙여넣으세요.

## 워크트리 → 프롬프트 매핑

| 워크트리 경로 | 프롬프트 파일 |
|--------------|-------------|
| `.worktrees/team-a/` | `start-team-a.md` |
| `.worktrees/team-b/` | `start-team-b.md` |
| `.worktrees/coordinator/` | `start-coordinator.md` |
| `.worktrees/team-design/` | `start-design.md` |
| `.worktrees/qa/` | `start-qa.md` |
| `.worktrees/ra/` | `start-ra.md` |

## 동작 흐름

```
사용자: 워크트리 터미널에서 claude 실행
사용자: 해당 팀 프롬프트 붙여넣기
   ↓
에이전트: DISPATCH Resolution Protocol 실행
   ↓
에이전트: _CURRENT.md → DISPATCH 파일 읽기
   ↓
ACTIVE → 작업 시작 → COMPLETED push → ScheduleWakeup(300) 재설정
IDLE   → ScheduleWakeup(300) 설정 → 대기
   ↓
300초 후 자동 깨어남 → 다시 _CURRENT.md 확인 → 반복
```

## CC 역할

- CC 세션(메인 터미널)에서 CronCreate로 20분마다 팀 상태 모니터링
- COMPLETED 감지 → 자동 머지 → 다음 라운드 DISPATCH 발행
- 팀들은 ScheduleWakeup으로 새 DISPATCH 자동 감지

## 주의사항

- 터미널 창을 닫으면 ScheduleWakeup이 소멸 → 세션 종료됨
- 작업 완료 후에도 창을 열어두면 자동으로 다음 DISPATCH를 폴링함
- CC가 새 DISPATCH 발행 → 팀이 다음 폴링 주기(최대 5분)에 감지 → 자동 시작
