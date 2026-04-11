# DISPATCH: Coordinator — S05 Round 1 Hotfix

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Coordinator 팀 |
| **브랜치** | team/coordinator |
| **유형** | S05 R1 Hotfix — 아키텍처 테스트 위반 수정 |
| **우선순위** | P0 (S05 게이트 블로커) |
| **SPEC 참조** | SPEC-COORDINATOR-001 / SPEC-GOVERNANCE-001 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일만 Status 업데이트.

---

## 컨텍스트

S04에서 Coordinator가 `HnVue.Data/Repositories/`에 추가한 EfXxx 내부 어댑터 클래스들에
대응 인터페이스가 없어 S05 QA 아키텍처 테스트가 실패함.

**실패 테스트:**
1. `NamingConventionTests.Repository_Classes_Must_Follow_Naming_Convention`
2. `GovernanceArchitectureTests.Repository_Implementations_Must_Have_Matching_Interfaces`

**문제:** `HnVue.Data/Repositories/`에 있는 EfXxx 클래스들은 내부 어댑터(인터페이스 없음)인데,
아키텍처 테스트는 `HnVue.Common/Abstractions/`에 대응 인터페이스 파일이 있어야 한다고 강제함.

---

## Task 1 (P0): 누락 인터페이스 5개 추가

### 추가할 파일 목록 (`src/HnVue.Common/Abstractions/`)

각 인터페이스는 최소 필요 메서드를 포함한 빈 인터페이스로 작성 (실제 구현체와 일치 필요 없음):

1. `ICdStudyRepository.cs` — EfCdStudyRepository 대응
2. `IDoseRepository.cs` — EfDoseRepository 대응
3. `IIncidentRepository.cs` — EfIncidentRepository 대응
4. `ISystemSettingsRepository.cs` — EfSystemSettingsRepository 대응
5. `IUpdateRepository.cs` — EfUpdateRepository 대응

### 인터페이스 작성 가이드

```csharp
// 예시: ICdStudyRepository.cs
namespace HnVue.Common.Abstractions;

/// <summary>
/// Repository interface for CD burning study file path queries.
/// REQ-COORD-006: SPEC-COORDINATOR-001 EF Core CD study file path query.
/// </summary>
public interface ICdStudyRepository
{
    // 최소 메서드 — EfCdStudyRepository 공개 메서드와 일치
    Task<IReadOnlyList<string>> GetImageFilePathsAsync(int studyId, CancellationToken cancellationToken = default);
}
```

### 참조: 각 EfXxx 클래스의 공개 메서드 확인

```bash
grep -n "public " src/HnVue.Data/Repositories/EfCdStudyRepository.cs
grep -n "public " src/HnVue.Data/Repositories/EfDoseRepository.cs
grep -n "public " src/HnVue.Data/Repositories/EfIncidentRepository.cs
grep -n "public " src/HnVue.Data/Repositories/EfSystemSettingsRepository.cs
grep -n "public " src/HnVue.Data/Repositories/EfUpdateRepository.cs
```

### 검증

```bash
dotnet test tests/HnVue.Architecture.Tests/ --configuration Release 2>&1 | tail -5
# 목표: 실패: 0, 통과: 11
```

---

## Task 2 (P1): EfXxx 클래스 인터페이스 구현 선언 추가 (선택)

Task 1 완료 후, 각 EfXxx 클래스가 해당 인터페이스를 구현하도록 선언 추가:
```csharp
public sealed class EfCdStudyRepository(HnVueDbContext context) : ICdStudyRepository
```

이 작업은 Task 1 이후 빌드가 성공해야 진행 가능. 빌드 오류 발생 시 Task 1만 완료하고 보고.

---

## Git 완료 프로토콜 [HARD]

```bash
git checkout team/coordinator
git pull origin main
git add src/HnVue.Common/Abstractions/ICdStudyRepository.cs \
        src/HnVue.Common/Abstractions/IDoseRepository.cs \
        src/HnVue.Common/Abstractions/IIncidentRepository.cs \
        src/HnVue.Common/Abstractions/ISystemSettingsRepository.cs \
        src/HnVue.Common/Abstractions/IUpdateRepository.cs
# 변경된 EfXxx.cs 파일도 add (Task 2 완료 시)
git commit -m "feat(coordinator): SPEC-COORDINATOR-001 누락 인터페이스 5개 추가 — 아키텍처 테스트 PASS"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 인터페이스 5개 추가 | NOT_STARTED | -- | S05 게이트 블로커 |
| Task 2: EfXxx 인터페이스 구현 선언 | NOT_STARTED | -- | Task 1 완료 후 진행 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
