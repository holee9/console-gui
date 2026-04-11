# DISPATCH: Coordinator — S05 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Coordinator 팀 |
| **브랜치** | team/coordinator |
| **유형** | S05 Round 1 — ViewModel 단위 테스트 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-COORDINATOR-001 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일만 Status 업데이트.

---

## 컨텍스트

S04에서 NullRepository 6개를 EfRepository로 교체 완료. 다음 단계는 새로 교체된 Repository들이
DI 컨테이너를 통해 올바르게 주입되는지 **통합 검증**과 **ViewModel 단위 테스트** 강화.

---

## 사전 확인

```bash
# 브랜치를 main으로 최신화
git checkout team/coordinator
git pull origin main
git push origin team/coordinator
```

---

## Task 1 (P1): 신규 EfRepository 단위 테스트

### 대상

`HnVue.Data/Repositories/` 아래 S04에서 신규 생성된 파일들:
- EfDoseRepository.cs
- EfWorklistRepository.cs
- EfIncidentRepository.cs
- EfUpdateRepository.cs
- EfSystemSettingsRepository.cs
- EfCdStudyRepository.cs

### 요구사항

- 각 Repository 최소 2개 테스트 메서드 (CRUD 기본 검증)
- InMemory SQLite 사용 (기존 IntegrationTests 패턴 참조)
- 테스트 위치: `tests.integration/HnVue.IntegrationTests/`

### 검증 기준

```bash
dotnet test tests.integration/HnVue.IntegrationTests/ --no-build 2>&1 | tail -10
# 새 테스트 포함 전체 통과
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests.integration/HnVue.IntegrationTests/
# DISPATCH.md, CLAUDE.md 절대 추가 금지
git commit -m "test(coordinator): EfRepository 단위 테스트 — SPEC-COORDINATOR-001 검증"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: EfRepository 테스트 | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
