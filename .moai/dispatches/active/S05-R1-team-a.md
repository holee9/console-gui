# DISPATCH: Team A — S05 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S05 Round 1 — Data 모듈 커버리지 강화 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-INFRA-001 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일만 Status 업데이트.

---

## 컨텍스트

S04에서 PHI AES-256-GCM 암호화(SPEC-INFRA-002) 완료. Security 모듈 95.5% 달성.
이번 라운드는 **HnVue.Data 모듈** 커버리지를 강화 — S04에서 추가된 6개 EfRepository에 대한
Data 레이어 테스트가 필요.

현재 Data 커버리지: ~82% → 목표: **85%+**

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
# S04에서 추가된 Repository 확인
ls src/HnVue.Data/Repositories/Ef*.cs
```

---

## Task 1 (P1): HnVue.Data EfRepository 테스트 강화

### 대상

`tests/HnVue.Data.Tests/` 내 EfRepository 테스트 추가:
- EfDoseRepository: CRUD + 예외 케이스
- EfWorklistRepository: 쿼리 필터링
- 기타 EfRepository 중 우선순위 높은 것

### 품질 기준

- 각 Repository 최소 3개 테스트
- SQLite InMemory 사용 (프로덕션 DB 연결 금지)
- `[Trait("Category", "Data")]` 어트리뷰트 추가

### 검증

```bash
dotnet test tests/HnVue.Data.Tests/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/HnVue.Data.Tests/
# DISPATCH.md, CLAUDE.md 절대 추가 금지
git commit -m "test(team-a): SPEC-INFRA-001 HnVue.Data EfRepository 테스트 강화"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: EfRepository 테스트 | NOT_STARTED | -- | 목표: Data 85%+ |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
