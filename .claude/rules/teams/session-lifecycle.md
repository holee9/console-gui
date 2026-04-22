# Session Lifecycle [ALL TEAMS]

팀 세션의 생애주기 관리: ScheduleWakeup 폴링, /clear, TIMEOUT, Stall Detection.
이 문서는 `team-common.md`에서 분리되었다 (2026-04-22 재정비).

---

## 1. 팀 세션 모델

팀은 독립 Claude Code 세션으로 실행되며, 다음 사이클을 반복한다:

```
IDLE 대기 (ScheduleWakeup)
    ↓  깨어남
git pull + _CURRENT.md 읽기
    ↓
ACTIVE 감지 → DISPATCH 읽기 → 작업 → COMPLETED push
    ↓
ScheduleWakeup 재설정 → IDLE 대기
```

오케스트레이터(사용자)가 팀에 "알림" 보내는 채널은 **없음**. 오직 git pull polling만 존재.

---

## 2. ScheduleWakeup 폴링 규칙

### 표준 폴링 루프

```
1. _CURRENT.md 읽기 → '팀 모니터링 설정' ScheduleWakeup 값 획득
2. ScheduleWakeup(값) 실행
3. 깨어나면 → git pull origin main
4. _CURRENT.md 재읽기 → 자기 팀 행 + ScheduleWakeup 값 재확인 (사용자가 변경 가능)
5. IDLE/MERGED → IDLE 보고 → ScheduleWakeup(새 값) 재대기
6. ACTIVE → 즉시 작업 시작 (ScheduleWakeup 먼저 설정 금지)
```

### HARD 규칙

- [HARD] **최소 300초 (5분)** — `_CURRENT.md` 값이 300 미만이면 300으로 보정
- [HARD] **하드코딩 금지** — 반드시 `_CURRENT.md`에서 읽을 것
- [HARD] 작업 완료 push 직후 ScheduleWakeup(읽은 값) 재설정 — **다음 DISPATCH 수신의 유일한 채널**
- [HARD] ScheduleWakeup 없이 IDLE 보고 = 단절 = 자율주행 불가
- [HARD] ACTIVE 감지 시 → 대기 없이 즉시 작업 (ScheduleWakeup 먼저 설정 X)
- [HARD] `/clear` 후 새 세션 시작 시 → DISPATCH Resolution Protocol 즉시 실행 (대기 없이)

---

## 3. Session Lifecycle — 완료 처리

DISPATCH 작업 완료 시:

```
1. git add + git commit + git push origin team/{team}
2. DISPATCH Status → COMPLETED + 빌드 증거 + 타임스탬프 → push
3. ScheduleWakeup(값) 재설정   ← [HARD — 최우선]
4. (선택) 사용자가 /clear 지시 시 → 세션 재시작 → DISPATCH Resolution 재실행
```

### /clear 정책 [HARD — S15-R3 개정 확정]

- [HARD] `/clear`는 **사용자 지시 시에만** 실행
- [HARD] 팀이 자동으로 `/clear`하지 않는다 (Worktree/브랜치는 보존)
- [HARD] `/clear` 후 세션 재시작 시 DISPATCH Resolution Protocol 즉시 실행 + ScheduleWakeup 설정

### 완료 프로세스 흐름

```
DISPATCH 작업 완료
    ↓
git add → git commit → git push origin team/{team-name}
    ↓
DISPATCH Status COMPLETED 업데이트 → push
    ↓
[HARD] ScheduleWakeup(읽은 값) 재설정 ← 다음 DISPATCH 수신 채널 확보
    ↓
사용자: DISPATCH Status 확인 → 머지 → _CURRENT.md 업데이트
    ↓
새 DISPATCH 발행 → 팀 ScheduleWakeup으로 감지 → 작업 시작
```

---

## 4. 팀 세션 상태 인식

팀 세션 상태는 다음 3가지 git 신호로 측정된다:

| 신호 | 해석 |
|------|------|
| `origin/team/{team}` 최신 커밋 시각 | 팀 활동 시각 |
| DISPATCH Status 테이블의 타임스탬프 | Task 단위 진행 |
| `_CURRENT.md`의 팀 행 상태 | 라운드 단위 상태 (사용자가 관리) |

팀이 ScheduleWakeup 없이 idle 상태면 팀 상태를 알 수 없음 → 반드시 재설정 필요.

---

## 5. Stall Detection [S09-R3 교훈]

NOT_STARTED가 지속되는 팀 감지:

- 3회 연속 NOT_STARTED → 사용자에게 "작업 지연 의심" 경고
- 5회 연속 NOT_STARTED → 사용자에게 조치 요청
- [HARD] 팀 Status를 임의 변경 금지 — 경고만 수행
- 사례: QA 12회 연속 NOT_STARTED → 임의로 BLOCKED 변경 → 실제로는 QA 작업 중 → 상태 왜곡

---

## 6. TIMEOUT Protocol [S15-R2 교훈]

미응답 팀 처리 — 무한 대기 금지.

```
N-1팀 MERGED + 1팀 미응답:
1. DISPATCH 발행 후 60분 경과 → TIMEOUT 선언
2. DISPATCH 파일 active/ → completed/ 이동 (Status: TIMEOUT)
3. _CURRENT.md 해당 팀 IDLE 업데이트
4. 전팀 IDLE 확인 → 즉시 갭 분석 → 다음 라운드 발행
5. TIMEOUT 팀은 다음 라운드에서 정상 포함 (재시도)
```

- [HARD] 미응답 팀은 페널티 없이 다음 라운드 포함 (워크트리/에이전트 미실행이 원인일 수 있음)
- [HARD] 연속 3회 TIMEOUT → 사용자 보고 (워크트리/에이전트 구성 문제 의심)

---

## 7. 세션 독립성

- [HARD] 각 팀 세션은 **독립 Claude Code 인스턴스** — 다른 팀 컨텍스트 접근 불가
- [HARD] 팀간 통신은 **git + DISPATCH 파일**을 통해서만 (직접 메시지 X)

---

Version: 1.2.0 (CC v2 도입 — CC ScheduleWakeup 600초, 팀은 기존 유지)
Effective: 2026-04-22
Cross-ref: `dispatch-protocol.md`, `quality-standards.md`
