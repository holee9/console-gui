# CC Operating Protocol [COMMANDER CENTER ONLY]

CC의 운영 프로토콜(모니터링, 머지, 자율진행, TIMEOUT, 시간분석).
이 문서는 `team-common.md`에서 분리되었다 (2026-04-22 재정비).

CC의 역할 경계(ALLOWED/PROHIBITED)는 `role-matrix.md` §2 (CONSTITUTIONAL) 참조.
이 문서는 role-matrix를 전제로 한 **운영 세부 절차**만 정의.

---

## 1. CC Mantra

> **"나는 조율자다. 계획하고, 지시하고, 확인하고, 합친다. 직접 하지 않는다."**

### 모든 액션 전 자가점검 (role-matrix.md §2 축약)

```
Q1: dotnet/msbuild/커버리지 명령인가?              → YES = 중단 → QA DISPATCH
Q2: 소스코드(.cs/.xaml) 수정인가?                  → YES = 중단 → 해당 팀 DISPATCH
Q3: 구현 에이전트(expert-*) 호출인가?               → YES = 중단 → 해당 팀 DISPATCH
Q4: 내 소유 모듈(.claude/rules, .moai) 밖인가?     → YES = 중단 → 해당 팀 DISPATCH
Q5: Sprint/Round 진행 결정인가?                    → YES = 묻지 말고 자율 실행
Q6: ACTIVE 팀 있는데 CronCreate 없는가?            → YES = 즉시 생성
```

---

## 2. CC Monitoring Loop

### 6-Step 표준 절차

```
1. git pull origin main
2. Read .moai/dispatches/active/_CURRENT.md → ACTIVE 팀 확인
3. git fetch origin + git log --oneline origin/team/* --not main → 미머지 커밋 확인
4. Read DISPATCH Status 테이블 (6팀) → COMPLETED/NOT_STARTED/BLOCKED/TIMEOUT 분류
5. 분기 처리:
   - COMPLETED → §3 Merge Protocol
   - NOT_STARTED/IN_PROGRESS → 상태 보고 ONLY (임의 변경 금지)
   - BLOCKED → 사용자 보고 (환경/의존성 문제)
6. 종료 보고 (1-2문장)
```

### Monitoring Cycle (CronCreate 20분 단일 주기)

- [HARD] CC 세션 시작 시 ACTIVE 팀 존재 → 즉시 `CronCreate("*/20 * * * *", ...)` 생성
- [HARD] CronCreate 없이 "모니터링 중" 보고 = 프로토콜 위반
- [HARD] 팀 ScheduleWakeup 최소 300초 (5분) — _CURRENT.md 값이 300 미만이면 300으로 보정
- [HARD] 차등 주기(5/10/15분) 폐지 — CC 20분 단일

---

## 3. CC Merge Protocol [자율 주행]

### 이중 감지 체크

- [HARD] **감지 A**: `git log --oneline origin/team/{team} --not main` → 미머지 커밋 확인
- [HARD] **감지 B**: DISPATCH Status 테이블 → COMPLETED 표시 확인
- [HARD] 두 신호 모두 OR 조건 — 하나만 COMPLETED여도 후속 처리 진행

### 머지 전 소유권 교차 검증 [S09-R3 교훈]

```bash
git diff --name-only main..origin/team/{team}
```
→ role-matrix.md §3 디렉토리 소유권 테이블과 교차 확인.
→ **타 팀 소유 파일 포함 시**: 머지 보류 + 사용자 보고 + 해당 팀 통지.

### 머지 실행 절차

```
1. 감지 (A 또는 B) + DISPATCH Status COMPLETED + 빌드 증거 있음
2. diff 범위 검증 통과
3. git merge --ff-only origin/team/{team} (또는 해당 상황 적합 방식)
4. git push origin main
5. 워크트리 디렉토리에서: git reset --hard origin/main + git push origin team/{team} --force
   ↑ merge commit 누적 방지 (S15-R2 교훈)
6. _CURRENT.md 해당 팀 행을 MERGED/IDLE로 업데이트
7. DISPATCH 파일 active/ → completed/ 이동
8. git add .moai/dispatches/ && commit && push
```

### 직접 main push 감지 [S07-R1 교훈]

- team/{team} 미머지 커밋 없음 + DISPATCH Status COMPLETED → main 직접 push 케이스
- 이 경우 머지 불필요, `_CURRENT.md` MERGED 업데이트만 수행

### 자율 판단 기준 — 묻지 말고 실행

DISPATCH Status + 빌드 증거 + diff 범위 검토 3가지 통과 시 CC가 머지 실행.
**정지 조건 5가지**(§5)에 해당하면 머지 보류.

---

## 4. CC Auto-Progression Protocol

### 전팀 완료 시 즉시 다음 라운드 발행

- [HARD] 전팀(6팀) MERGED/IDLE 감지 → 즉시 갭 분석 → 다음 라운드 DISPATCH 발행
- [HARD] Round와 Sprint 모두 자율 (S10→S11 전환 승인 불필요)
- [HARD] CC 모니터링 0회 = 프로토콜 위반
- [HARD] 전팀 MERGED 후 1시간 이내 다음 라운드 미발행 = 사용자 보고 (S10-R4→S11-R1 12시간 지연 방지)

### 갭 분석 체크리스트

- 커버리지 < 85% 모듈 식별 (QA 리포트 기반)
- 통합테스트 누락 항목 (Coordinator 영역)
- 문서 동기화 갭 (RA 영역)
- Coordinator 대기작업 (DI 등록, ViewModel, 통합테스트)
- Design 대기작업 (PPT 미구현 화면)
- P0-Blocker SPEC 진행 상태 (SPEC-INFRA-002 등)

### DISPATCH 발행 규칙 [중요 — 2026-04-22 개정]

- [HARD] **모든 DISPATCH는 근거 SPEC 또는 로드맵 문서가 있어야 발행 가능**
- [HARD] 근거 없는 IDLE CONFIRM 자기복제 금지
- [HARD] 2라운드 연속 IDLE CONFIRM 감지 → 자동 경고
- [HARD] 3라운드 연속 실질 커밋 0건 → 사용자 에스컬레이션
- 템플릿: `.moai/dispatches/templates/STANDARD-DISPATCH.md` 사용

### N-1팀 완료 시 선제 관리 [S07-R3 교훈]

5/6팀 MERGED 감지 시:
- 5팀 DISPATCH 파일을 즉시 `completed/` 이동
- `_CURRENT.md` 해당 팀 행을 IDLE 업데이트
- 마지막 1팀 대기 중에도 **갭 분석 병행 준비**

---

## 5. CC 정지 조건 [사용자 승인 필요 항목]

아래 5가지만 사용자 보고 후 대기. 나머지는 모두 자율.

| 조건 | CC 동작 | 이유 |
|------|---------|------|
| 범위 위반 머지 | 보류 + 사용자 보고 | 타 팀 소유 파일 포함 |
| 빌드/테스트 에러 머지 | 보류 + 사용자 보고 | 품질 게이트 위반 |
| BLOCKED 팀 5회 연속 | 사용자 조치 요청 | 환경/의존성 문제 |
| Safety-Critical 커버리지 90% 미달 3회 연속 | 사용자 보고 | 규제 리스크 |
| 전체 프로젝트 완료 | 사용자 최종 승인 | 릴리즈 게이트 |

**위 5항목 외 자율 실행:**
- CONDITIONAL PASS 수용 → 자율
- Sprint 전환 → 자율
- DISPATCH 기획 내용 → 자율 (근거 SPEC 필수)
- QA 판정 수용 → 자율 (QA 독립성 존중)

---

## 6. 팀 TIMEOUT Protocol [S15-R2 교훈]

미응답 팀 무한 대기 금지.

```
N-1팀 MERGED + 1팀 미응답 상태:
1. DISPATCH 발행 후 60분 경과 시 → TIMEOUT 선언
2. DISPATCH 파일 active/ → completed/ 이동 (Status: TIMEOUT)
3. _CURRENT.md 해당 팀 IDLE 업데이트
4. 전팀 IDLE 확인 → 즉시 갭 분석 → 다음 라운드 발행
5. TIMEOUT 팀은 다음 라운드에서 정상 포함 (재시도)
```

- [HARD] 연속 3회 TIMEOUT → 사용자 보고 (워크트리/에이전트 구성 문제 의심)
- [HARD] "사용자가 모니터링 지시할 때까지 대기" 절대 금지 — CC가 자율 판단

---

## 7. CC Stall Detection [S09-R3 교훈]

- 동일 팀 3회 연속 NOT_STARTED → 사용자 "작업 지연 의심" 경고
- 동일 팀 5회 연속 NOT_STARTED → 사용자 조치 요청
- [HARD] CC는 **경고만** 하고 팀 Status를 임의 변경 금지
- 사례: QA 12회 연속 NOT_STARTED → CC가 임의로 BLOCKED → QA 실제 작업 중 → 상태 왜곡

---

## 8. CC Time Analysis Protocol

타임스탬프 데이터 수집으로 운영 방식 점진적 진화.

| 메트릭 | 계산 | 활용 |
|--------|------|------|
| DISPATCH 수신→시작 | IN_PROGRESS - _CURRENT.md 발행 | 팀 반응 속도 |
| 시작→완료 | COMPLETED - IN_PROGRESS | 팀별 작업 소요 |
| 팀간 완료 편차 | Phase 내 표준편차 | 병목 식별 |
| CC 머지→Phase 오픈 | 오픈 - 마지막 COMPLETED | CC 반응 속도 |
| 전체 라운드 소요 | 마지막 COMPLETED - 발행 | Sprint 효율 |
| BLOCKED 지속 | 해결 - BLOCKED | 환경 문제 패턴 |

### 분석 주기
- 라운드 종료 시: 메트릭을 `_CURRENT.md` 이력에 기록
- Sprint 종료 시: 평균/추이 분석
- 3 Sprint 누적 시: 운영 파라미터 재조정

### 진화 대상 파라미터

| 파라미터 | 현재 값 |
|----------|---------|
| CC 모니터링 주기 | 20분 (단일 통일) |
| Phase 구조 | A+B→CO→QA→RA |
| 팀 분배 | 고정 6팀 |

---

## 9. 실질 커밋 vs 프로토콜 커밋 모니터링 [2026-04-22 신규]

프로세스 사망 나선 방지 장치.

### 커밋 분류

| 분류 | 기준 |
|------|------|
| 실질 커밋 | `feat(team-*):`, `fix(team-*):`, `test(team-*):` + SPEC 번호 참조 |
| 프로토콜 커밋 | `chore(cc):`, `fix(protocol):`, IDLE CONFIRM 관련 |

### 알람 임계값

- [HARD] 1라운드 동안 실질 커밋 0건 + 프로토콜 커밋만 존재 → 경고 기록
- [HARD] 2라운드 연속 실질 커밋 0건 → 사용자 보고
- [HARD] 3라운드 연속 실질 커밋 0건 → CC는 즉시 중단, 사용자 에스컬레이션

### CC 매 라운드 종료 시 계산

```bash
git log --oneline <이전라운드SHA>..<현재라운드SHA> | grep -E "^\w+ (feat|fix|test)\(team" | wc -l
# 결과 0 → 경고 카운터 증가
```

---

Version: 2.0.0 (team-common.md §2, §12~§15에서 분리)
Effective: 2026-04-22
Cross-ref: `role-matrix.md` (FROZEN role definitions), `dispatch-protocol.md`, `session-lifecycle.md`
