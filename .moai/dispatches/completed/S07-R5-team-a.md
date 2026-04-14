# DISPATCH: Team A — S07 Round 5

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S07 R5 — Flaky Test 수정 + 커버리지 갭 해소 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R5-team-a.md)만 Status 업데이트.

---

## 컨텍스트

S07-R4 QA 검증 결과: 2372/2374 passed, 2건 flaky (Security PasswordHasher).
평균 커버리지 89.4%, 3개 모듈 85% 미만, Safety-Critical 3/4.
Team A는 Security flaky 수정 + 인프라 모듈 커버리지 보강 필요.

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
```

---

## Task 1 (P1): Security PasswordHasher Flaky Test 수정

R4 QA에서 Security.Tests 2건 flaky 확인. PasswordHasher 500-661ms 타이밍 이슈.

**수행 사항**:
- PasswordHasher 테스트 타임아웃/대기 시간 조정
- bcrypt work factor 테스트에서 고정 시간 대신 결과 검증으로 전환
- 3회 반복 실행으로 flaky 재현 불확인
- 테스트 격리 보장 (공유 상태 제거)

**목표**: Security.Tests 3회 연속 0실패, flaky 재현 불가

---

## Task 2 (P2): 인프라 모듈 커버리지 85%+ 보장

QA 리포트에서 85% 미만 모듈 중 Team A 소유 모듈 확인 후 보강.

**수행 사항**:
- 현재 커버리지 측정 (Common, Data, Security, SystemAdmin, Update)
- 85% 미만 모듈 식별 후 테스트 보강
- Safety-Critical(Security, Update) 90%+ 확인
- 테스트 특성 측정 후 DISPATCH Status에 기록

**목표**: 전 모듈 85%+, Safety-Critical 90%+

---

## Task 3 (P3): Flaky Test 수정 검증

Task 1 수정 후 전체 솔루션 빌드/테스트로 회귀 없음 확인.

**수행 사항**:
- `dotnet build` 0에러 확인
- `dotnet test` 전체 실행 (3323+ 테스트)
- Security.Tests 3회 반복 실행으로 안정성 확인

**목표**: 빌드 0에러, 테스트 0실패, Security flaky 0건

---

## Git 완료 프로토콜 [HARD]

```bash
git add [수정 파일]
git commit -m "fix(team-a): Security flaky test 안정화 + 커버리지 보강 (#issue)"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Security flaky 수정 (P1) | NOT_STARTED | | |
| Task 2: 커버리지 보강 (P2) | NOT_STARTED | | |
| Task 3: 수정 검증 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
