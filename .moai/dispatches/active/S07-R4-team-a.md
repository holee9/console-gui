# DISPATCH: Team A — S07 Round 4

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S07 R4 — 커버리지 유지 + Security 테스트 안정화 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R4-team-a.md)만 Status 업데이트.

---

## 컨텍스트

S07-R3에서 Data/Update 커버리지 갭 보강 완료.
현재 Security.Tests에서 전체 솔루션 실행 시 1건 간헐적 실패 (개별 실행 시 통과).
이는 비동기 테스트 실행 순서 또는 타이밍 이슈로 추정.

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
```

---

## Task 1 (P1): Security Flaky Test 안정화

전체 솔루션 `dotnet test` 실행 시 HnVue.Security.Tests 1건 간헐적 실패.
개별 실행 시 286/286 통과.

**수행 사항**:
- Security.Tests에서 `[Fact]` 비동기 테스트 타이밍 이슈 조사
- 공유 상태(정적 필드, 싱글톤) 사용 테스트 식별
- 필요시 `[Collection("Sequential")]` 적용 또는 `IDisposable` 정리 강화
- BCrypt 해시 비용 테스트(1381ms 소요) 타임아웃 조정 검토

**목표**: 전체 솔루션 실행 시 Security 286/286 통과 (0 실패)

---

## Task 2 (P2): 커버리지 85% 유지 확인

S07-R3에서 보강한 Data/Update 모듈 커버리지가 여전히 85% 이상인지 확인.
필요시 누락 브랜치/예외 경로 추가 테스트.

**목표**: Data 85%+, Update 85%+, Security 90%+ (Safety-Critical)

---

## Task 3 (P3): Repository 인터페이스 일치성 검증

Coordinator Task 1에서 DI Null Stub을 실제 Repository로 교체.
Team A 소유 Repository 구현체가 모든 인터페이스 메서드를 올바르게 구현했는지 검증.

**수행 사항**:
- IUserRepository, IStudyRepository, IAuditRepository 구현 일치성 확인
- ISystemSettingsRepository, IUpdateRepository, IWorklistRepository 확인
- 인터페이스에 정의된 모든 메서드가 Repository에 구현되어 있는지 검증 테스트

**목표**: 모든 Repository 인터페이스 100% 구현

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.Data/ src/HnVue.Security/ tests/HnVue.Data.Tests/ tests/HnVue.Security.Tests/
git commit -m "fix(team-a): S07-R4 Security flaky 안정화 + Repository 일치성 검증 (#issue)"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Security Flaky 안정화 (P1) | NOT_STARTED | | |
| Task 2: 커버리지 85% 유지 (P2) | NOT_STARTED | | |
| Task 3: Repository 일치성 검증 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
