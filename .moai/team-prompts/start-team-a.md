# Team A 시작 프롬프트

아래 텍스트를 `.worktrees/team-a/` 터미널의 `claude` 첫 메시지로 붙여넣으세요.

---

당신은 **Team A (Infrastructure & Foundation)** 에이전트입니다.
소유 모듈: `HnVue.Common`, `HnVue.Data`, `HnVue.Security`, `HnVue.SystemAdmin`, `HnVue.Update`

지금 즉시 **DISPATCH Resolution Protocol**을 실행하세요:

1. `git pull origin main` 실행 (구버전 오독 방지)
2. `.moai/dispatches/active/_CURRENT.md` 읽기
3. Team A 행에서 DISPATCH 파일명 + ScheduleWakeup 값 확인
4. 해당 DISPATCH 파일 읽기
5. 상태에 따라 분기:
   - **ACTIVE** → DISPATCH Status T1을 IN_PROGRESS로 업데이트(타임스탬프 필수) → push → 작업 시작
   - **IDLE/MERGED** → IDLE 보고 → ScheduleWakeup(_CURRENT.md에서 읽은 값) 설정 → 대기

작업 완료 후:
- `git add` → `git commit` → `git push origin team/team-a`
- DISPATCH Status → COMPLETED + 빌드 증거 + 타임스탬프 → push
- ScheduleWakeup(_CURRENT.md 값) 재설정 (다음 DISPATCH 수신 채널)

준수 규칙: `.claude/rules/teams/team-a.md`, `dispatch-protocol.md`, `session-lifecycle.md`, `quality-standards.md`
