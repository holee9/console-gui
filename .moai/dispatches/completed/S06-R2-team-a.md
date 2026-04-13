# DISPATCH: Team A — S06 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S06 R2 — Data.Tests 구문 오류 검증 + SystemAdmin 커버리지 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R2-team-a.md)만 Status 업데이트.

---

## 컨텍스트

S06-R1 QA 결과에서 2가지 이슈 발견:
1. **HnVue.Data.Tests 구문 오류**: QA가 임시 수정 적용 (Team A 소유 파일)
   - `await using var (ctx, connection) = ...` C# tuple deconstruction 비호환 구문
   - 6개 파일 수정됨
2. **SystemAdmin 커버리지 62.9%**: 목표 85% 대비 -22.1% 갭

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
```

---

## Task 1 (P1): Data.Tests 구문 오류 검증 및 확정

QA가 수정한 6개 파일을 검토하고 올바른 패턴인지 확인:
- `tests/HnVue.Data.Tests/Repositories/EfWorklistRepositoryTests.cs`
- `tests/HnVue.Data.Tests/Repositories/EfCdStudyRepositoryTests.cs`
- `tests/HnVue.Data.Tests/Repositories/EfDoseRepositoryTests.cs`
- `tests/HnVue.Data.Tests/Repositories/EfIncidentRepositoryTests.cs`
- `tests/HnVue.Data.Tests/Repositories/EfUpdateRepositoryTests.cs`
- `tests/HnVue.Data.Tests/Repositories/EfSystemSettingsRepositoryTests.cs`

수정 내용 확인:
```bash
git show 7773b7d -- tests/HnVue.Data.Tests/Repositories/ 2>/dev/null | head -60
```

검증:
```bash
dotnet build tests/HnVue.Data.Tests/ --configuration Release 2>&1 | tail -5
dotnet test tests/HnVue.Data.Tests/ 2>&1 | tail -10
```

문제가 있으면 수정, 정상이면 확인 완료로 기록.

---

## Task 2 (P2): SystemAdmin 커버리지 85% 달성

현재: 62.9% → 목표: 85% (+22.1%)

1. 현재 커버리지 미달 경로 분석:
```bash
dotnet test tests/HnVue.SystemAdmin.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

2. 미커버 항목 식별 후 테스트 추가 (`tests/HnVue.SystemAdmin.Tests/`)

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/HnVue.Data.Tests/ tests/HnVue.SystemAdmin.Tests/
git commit -m "fix(team-a): Data.Tests 구문 검증 + SystemAdmin 커버리지 보강 (#issue)"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data.Tests 구문 검증 (P1) | NOT_STARTED | -- | QA 수정 검토 |
| Task 2: SystemAdmin 85% (P2) | NOT_STARTED | -- | 62.9% → 85% |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
