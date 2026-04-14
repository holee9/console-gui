# DISPATCH: Team A — S05 Round 2 (Hotfix)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S05 Round 2 Hotfix — Data.Tests build failure fix |
| **우선순위** | P0-Blocker |
| **Issue 참조** | #83 (BLOCKER), #84 (coverage gap) |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-team-a-2.md)만 Status 업데이트.

---

## 컨텍스트

QA 릴리즈 준비도 보고서에서 **HnVue.Data.Tests 빌드 실패 (14 errors)** 발견.
Issue #83 (BLOCKER) 등록됨.

에러 원인:
1. `ImageEntity.ImagePosition` 속성 미존재
2. `decimal` → `double` 변환 에러 (DoseRecord 생성자)
3. `ErrorCode?.Code` 속성 미존재
4. `DateOnly` → `DateTime` 변환 에러

---

## Task 1 (P0): Data.Tests 빌드 에러 14건 수정

### 사전 확인

```bash
git checkout team/team-a
git pull origin main
dotnet build tests/HnVue.Data.Tests/ 2>&1 | grep "error CS"
```

### 수정 대상 파일

- `tests/HnVue.Data.Tests/Repositories/EfCdStudyRepositoryTests.cs` — ImageEntity.ImagePosition → 실제 속성명 확인
- `tests/HnVue.Data.Tests/Repositories/EfDoseRepositoryTests.cs` — decimal → double 캐스팅
- `tests/HnVue.Data.Tests/Repositories/EfIncidentRepositoryTests.cs` — ErrorCode?.Code → ErrorCode 자체가 값
- `tests/HnVue.Data.Tests/Repositories/EfWorklistRepositoryTests.cs` — DateOnly → DateTime 변환

### 방법

1. 각 Entity/Model의 실제 속성 타입을 먼저 확인 (src/ 하위 소스 코드 읽기)
2. 테스트 코드를 실제 API에 맞게 수정
3. `dotnet build tests/HnVue.Data.Tests/` → 0 errors 확인

### 검증

```bash
dotnet build tests/HnVue.Data.Tests/ 2>&1 | tail -3
# Expected: 0 errors
```

---

## Task 2 (P2): Data 및 SystemAdmin 커버리지 개선

Issue #84 기준:
- HnVue.Data: 43.6% → 85%
- HnVue.SystemAdmin: 60.3% → 85%

Task 1 완료 후 착수. 필수는 아님 — 커버리지 개선 테스트 추가.

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/HnVue.Data.Tests/
git commit -m "fix(team-a): #83 Data.Tests build errors — API type mismatches resolved"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data.Tests 빌드 수정 (P0) | NOT_STARTED | -- | Issue #83 BLOCKER |
| Task 2: 커버리지 개선 (P2) | NOT_STARTED | -- | Issue #84 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
