# DISPATCH: Coordinator — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | Coordinator 팀 |
| **브랜치** | team/coordinator |
| **유형** | S04 Round 1 — Null Repository EF Core 교체 |
| **우선순위** | P0 (Null Stub 6개 교체), P1 (통합 테스트 커버리지) |
| **SPEC 참조** | SPEC-COORDINATOR-001 |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 1 완료 후 Task 2 착수
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

S03까지 App.xaml.cs에 6개 NullRepository stub이 등록된 상태로 남아 있음.
이로 인해 Dose, Worklist, Incident, Update, SystemSettings, CdStudy 기능이
런타임에 무동작(no-op)으로 처리되는 심각한 결함.

S04 핵심 차단 항목으로, DI 통합 검증(Coordinator I1)의 선행 조건임.

**App.xaml.cs 현재 NullRepository 위치**:
- Line 179: `services.AddSingleton<IDoseRepository, NullDoseRepository>()`
- Line 184: `services.AddSingleton<IWorklistRepository, NullWorklistRepository>()`
- Line 190: `services.AddSingleton<IIncidentRepository, NullIncidentRepository>()`
- Line 195: `services.AddSingleton<IUpdateRepository, NullUpdateRepository>()`
- Line 203: `services.AddSingleton<ISystemSettingsRepository, NullSystemSettingsRepository>()`
- Line 210: `services.AddSingleton<HnVue.CDBurning.IStudyRepository, NullCdStudyRepository>()`

---

## 파일 소유권

```
HnVue.UI.Contracts/
HnVue.UI.ViewModels/
HnVue.App/
tests.integration/HnVue.IntegrationTests/
HnVue.Data/Repositories/Ef*.cs  (신규 파일 — 이번 DISPATCH 전용)
```

---

## Task 1 (P0): 6개 NullRepository → EF Core Repository 교체

### 사전 확인

작업 전 다음을 확인하라:
```bash
# 1. 현재 인터페이스 정의 위치 파악
grep -r "IDoseRepository" src/ --include="*.cs" -l
grep -r "IWorklistRepository" src/ --include="*.cs" -l
grep -r "IIncidentRepository" src/ --include="*.cs" -l
grep -r "IUpdateRepository" src/ --include="*.cs" -l
grep -r "ISystemSettingsRepository" src/ --include="*.cs" -l
grep -r "IStudyRepository" src/HnVue.CDBurning/ --include="*.cs" -l

# 2. 기존 DbContext 확인
# HnVue.Data/HnVueDbContext.cs 또는 ApplicationDbContext.cs 읽기
```

### 구현 대상

**신규 파일 (HnVue.Data/Repositories/):**

#### 1. EfDoseRepository.cs

```csharp
// HnVue.Data/Repositories/EfDoseRepository.cs
// IDoseRepository 인터페이스의 모든 메서드 구현
// HnVueDbContext 생성자 주입
// CRUD + 집계 메서드 (인터페이스 정의 확인 후 구현)
```

#### 2. EfWorklistRepository.cs

```csharp
// HnVue.Data/Repositories/EfWorklistRepository.cs
// IWorklistRepository 인터페이스 전체 구현
// 페이징, 날짜 필터, 상태 필터 지원
```

#### 3. EfIncidentRepository.cs

```csharp
// HnVue.Data/Repositories/EfIncidentRepository.cs
// IIncidentRepository 인터페이스 전체 구현
// Safety-Critical: 모든 쓰기를 트랜잭션 내에서 실행
```

#### 4. EfUpdateRepository.cs

```csharp
// HnVue.Data/Repositories/EfUpdateRepository.cs
// IUpdateRepository 인터페이스 전체 구현
// 최신 버전 조회, 업데이트 이력 저장
```

#### 5. EfSystemSettingsRepository.cs

```csharp
// HnVue.Data/Repositories/EfSystemSettingsRepository.cs
// ISystemSettingsRepository 인터페이스 전체 구현
// 설정 변경 시 감사 로그 트리거 (SystemAdmin 규칙 준수)
// 존재하지 않는 키 조회 시 기본값 반환
```

#### 6. EfCdStudyRepository.cs

```csharp
// HnVue.Data/Repositories/EfCdStudyRepository.cs
// HnVue.CDBurning.IStudyRepository 구현
// StudyEntity를 CDBurning Study 모델로 매핑
```

### DI 등록 교체

**파일**: `HnVue.App/App.xaml.cs`

```csharp
// 교체 전 (제거):
// services.AddSingleton<IDoseRepository, NullDoseRepository>();
// services.AddSingleton<IWorklistRepository, NullWorklistRepository>();
// services.AddSingleton<IIncidentRepository, NullIncidentRepository>();
// services.AddSingleton<IUpdateRepository, NullUpdateRepository>();
// services.AddSingleton<ISystemSettingsRepository, NullSystemSettingsRepository>();
// services.AddSingleton<HnVue.CDBurning.IStudyRepository, NullCdStudyRepository>();

// 교체 후 (추가):
// services.AddScoped<IDoseRepository, EfDoseRepository>();
// services.AddScoped<IWorklistRepository, EfWorklistRepository>();
// services.AddScoped<IIncidentRepository, EfIncidentRepository>();
// services.AddScoped<IUpdateRepository, EfUpdateRepository>();
// services.AddScoped<ISystemSettingsRepository, EfSystemSettingsRepository>();
// services.AddScoped<HnVue.CDBurning.IStudyRepository, EfCdStudyRepository>();
```

**주의**: DbContext가 Scoped이므로 Repository도 Scoped 권장.

### Task 1 수용 기준

- `HnVue.Data/Repositories/EfDoseRepository.cs` 존재
- `HnVue.Data/Repositories/EfWorklistRepository.cs` 존재
- `HnVue.Data/Repositories/EfIncidentRepository.cs` 존재
- `HnVue.Data/Repositories/EfUpdateRepository.cs` 존재
- `HnVue.Data/Repositories/EfSystemSettingsRepository.cs` 존재
- `HnVue.Data/Repositories/EfCdStudyRepository.cs` 존재
- App.xaml.cs에서 `NullDoseRepository`, `NullWorklistRepository`, `NullIncidentRepository`, `NullUpdateRepository`, `NullSystemSettingsRepository`, `NullCdStudyRepository` 미등장
- `dotnet build` 0 에러

---

## Task 2 (P1): 단위 테스트 + 통합 테스트 작성

### 단위 테스트 (HnVue.Data.Tests 또는 동등한 프로젝트)

각 Repository당 최소 3개 단위 테스트:
- in-memory SQLite DbContext 사용
- 저장/조회 기본 동작 검증
- 경계 케이스 (빈 결과, null 처리)

총 단위 테스트: 18개 이상 (Repository 6개 × 3개)

### 통합 테스트 (tests.integration/HnVue.IntegrationTests/)

각 Repository당 최소 1개 통합 테스트:
- 실제 in-memory SQLite DbContext 초기화
- Repository 실제 동작 검증
- 시드 데이터 → CRUD → 검증

총 통합 테스트: 6개 이상

### 테스트 명명 규칙

```
{Repository}_{시나리오}_{예상결과}
예: EfDoseRepository_SaveDoseRecord_PersistsToDatabase
    EfWorklistRepository_QueryWithDateFilter_ReturnsFilteredResults
```

### Task 2 수용 기준

- 단위 테스트 18개 이상 전원 PASS
- 통합 테스트 6개 이상 전원 PASS
- `dotnet test` 0 실패

---

## 빌드 검증

```bash
dotnet build HnVue.sln
dotnet test tests/HnVue.Data.Tests/ --no-build
dotnet test tests.integration/HnVue.IntegrationTests/ --no-build
```

---

## Git 완료 프로토콜 [HARD]

모든 Task 완료 후 순서대로 실행:

```bash
# 1. 스테이징
git add HnVue.Data/Repositories/EfDoseRepository.cs
git add HnVue.Data/Repositories/EfWorklistRepository.cs
git add HnVue.Data/Repositories/EfIncidentRepository.cs
git add HnVue.Data/Repositories/EfUpdateRepository.cs
git add HnVue.Data/Repositories/EfSystemSettingsRepository.cs
git add HnVue.Data/Repositories/EfCdStudyRepository.cs
git add HnVue.App/App.xaml.cs
git add tests/  # 신규 테스트 파일
git add tests.integration/

# 2. 커밋
git commit -m "feat(coordinator): replace 6 null repository stubs with EF Core implementations (SPEC-COORDINATOR-001)"

# 3. 푸시
git push origin team/coordinator

# 4. PR 생성 (기존 PR 확인 후 없으면 신규 생성)
curl -X POST "http://10.11.1.40:7001/api/v1/repos/drake.lee/Console-GUI/pulls" \
  -H "Authorization: token a4cb79626194b34a2d52835de05fb770162af014" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "[S04-R1-Coordinator] Null Repository 6개 EF Core 교체 (SPEC-COORDINATOR-001)",
    "body": "S04 Round 1 Coordinator DISPATCH 완료\n\n## 변경 사항\n- EfDoseRepository 신규 구현\n- EfWorklistRepository 신규 구현\n- EfIncidentRepository 신규 구현\n- EfUpdateRepository 신규 구현\n- EfSystemSettingsRepository 신규 구현\n- EfCdStudyRepository 신규 구현\n- App.xaml.cs: 6개 NullRepository → EfRepository 교체\n\n## 테스트\n- 단위 테스트 18개+ PASS\n- 통합 테스트 6개+ PASS\n- 빌드 0 에러\n\n## SPEC\n- SPEC-COORDINATOR-001 수용 기준 전체 충족",
    "head": "team/coordinator",
    "base": "main"
  }'
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 6개 Repository 구현 + DI 교체 | NOT_STARTED | -- | -- |
| Task 2: 단위 테스트 + 통합 테스트 | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 빌드 검증 결과

```
# 여기에 실제 빌드/테스트 결과 기록
dotnet build: ?
단위 테스트: ?
통합 테스트: ?
```
