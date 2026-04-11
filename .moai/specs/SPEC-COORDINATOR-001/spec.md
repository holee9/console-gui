---
id: SPEC-COORDINATOR-001
version: 1.0.0
status: approved
created: "2026-04-11"
updated: "2026-04-11"
author: moai
priority: P0-Blocker
issue_number: 0
team: coordinator
sprint: S04
---

# SPEC-COORDINATOR-001: Null Repository Stub 교체 — EF Core 실제 구현

## HISTORY

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 1.0.0 | 2026-04-11 | 최초 작성 — S04 Coordinator DI 정상화 SPEC | MoAI |

## 개요

App.xaml.cs에 등록된 6개의 NullRepository stub을 EF Core 기반 실제 구현체로 교체한다.
현재 DI 컨테이너에 Null 구현체가 등록되어 있어 런타임 시 모든 Repository 호출이 무동작(no-op)으로 처리됨.
이로 인해 Dose 측정, Worklist 조회, Incident 보고, Update 처리, SystemSettings 저장, CD Burning 스터디 조회가
실제 데이터베이스와 연동되지 않는 상태다.

**배경**: S03 완료 후 S04 진입 게이트에서 식별된 최우선 차단 항목. DI 통합 검증(Coordinator I1)의
선행 조건이며, 전체 파이프라인 E2E 정상화의 핵심 의존성이다.

## 범위

### 포함 (In Scope)

- `HnVue.Data`: 6개 EF Core Repository 구현체 신규 작성
  - `EfDoseRepository` (IDoseRepository 구현)
  - `EfWorklistRepository` (IWorklistRepository 구현)
  - `EfIncidentRepository` (IIncidentRepository 구현)
  - `EfUpdateRepository` (IUpdateRepository 구현)
  - `EfSystemSettingsRepository` (ISystemSettingsRepository 구현)
  - `EfCdStudyRepository` (HnVue.CDBurning.IStudyRepository 구현)
- `HnVue.App/App.xaml.cs`: DI 등록 교체 (Null → EF Core 구현체)
- `tests.integration/HnVue.IntegrationTests`: 통합 테스트 6개 이상 추가

### 제외 (Out of Scope)

- Repository 인터페이스 변경 (Coordinator 권한이지만 이번 SPEC 범위 아님)
- EF Core Migration 신규 추가 (기존 스키마 활용)
- 캐싱 레이어 추가
- 비동기 배치 처리 구현

---

## 요구사항

### REQ-COORD-001: EfDoseRepository 구현

**EARS 패턴**: DI 컨테이너가 IDoseRepository를 요청하는 경우, 시스템은 SQLCipher 암호화 DbContext를 통해 Dose 데이터를 읽고 쓰는 EfDoseRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:179`에서 `NullDoseRepository` 등록 — 모든 Dose 저장 호출이 무시됨.

**요구 동작**:
- `HnVue.Data/Repositories/EfDoseRepository.cs` 신규 작성
- `HnVueDbContext`를 생성자 주입으로 수신
- `IDoseRepository` 인터페이스 전 메서드 구현 (CRUD + 집계)
- `App.xaml.cs:179`에서 `EfDoseRepository`로 교체

**수용 기준**:
- `IDoseRepository`의 모든 메서드 구현 완료
- 단위 테스트: in-memory SQLite DbContext 활용 3개 이상
- 통합 테스트: 실제 DbContext와 연결 1개 이상

---

### REQ-COORD-002: EfWorklistRepository 구현

**EARS 패턴**: DI 컨테이너가 IWorklistRepository를 요청하는 경우, 시스템은 EF Core를 통해 Worklist(환자 검사 목록)를 조회하고 갱신하는 EfWorklistRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:184`에서 `NullWorklistRepository` 등록.

**요구 동작**:
- `HnVue.Data/Repositories/EfWorklistRepository.cs` 신규 작성
- 페이징, 날짜 필터, 상태 필터 지원
- `App.xaml.cs:184`에서 `EfWorklistRepository`로 교체

**수용 기준**:
- 필터 조합 쿼리 단위 테스트 3개 이상
- 빈 결과 케이스 단위 테스트 1개 이상

---

### REQ-COORD-003: EfIncidentRepository 구현

**EARS 패턴**: DI 컨테이너가 IIncidentRepository를 요청하는 경우, 시스템은 EF Core를 통해 방사선 사고 기록을 저장하고 조회하는 EfIncidentRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:190`에서 `NullIncidentRepository` 등록.

**요구 동작**:
- `HnVue.Data/Repositories/EfIncidentRepository.cs` 신규 작성
- Safety-Critical: 모든 쓰기 동작은 트랜잭션 내에서 실행
- `App.xaml.cs:190`에서 `EfIncidentRepository`로 교체

**수용 기준**:
- 저장 + 조회 단위 테스트 3개 이상
- 트랜잭션 롤백 시나리오 테스트 1개 이상

---

### REQ-COORD-004: EfUpdateRepository 구현

**EARS 패턴**: DI 컨테이너가 IUpdateRepository를 요청하는 경우, 시스템은 EF Core를 통해 SW 업데이트 이력과 버전 정보를 관리하는 EfUpdateRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:195`에서 `NullUpdateRepository` 등록.

**요구 동작**:
- `HnVue.Data/Repositories/EfUpdateRepository.cs` 신규 작성
- 최신 버전 조회, 업데이트 이력 저장 지원
- `App.xaml.cs:195`에서 `EfUpdateRepository`로 교체

**수용 기준**:
- 버전 비교 로직 단위 테스트 2개 이상
- 이력 저장 단위 테스트 1개 이상

---

### REQ-COORD-005: EfSystemSettingsRepository 구현

**EARS 패턴**: DI 컨테이너가 ISystemSettingsRepository를 요청하는 경우, 시스템은 EF Core를 통해 시스템 설정값을 Key-Value 형태로 저장하고 조회하는 EfSystemSettingsRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:203`에서 `NullSystemSettingsRepository` 등록.

**요구 동작**:
- `HnVue.Data/Repositories/EfSystemSettingsRepository.cs` 신규 작성
- 설정 변경 시 감사 로그 트리거 (SystemAdmin 팀 규칙 준수)
- `App.xaml.cs:203`에서 `EfSystemSettingsRepository`로 교체

**수용 기준**:
- 설정 읽기/쓰기 단위 테스트 3개 이상
- 존재하지 않는 키 조회 시 기본값 반환 테스트 1개 이상

---

### REQ-COORD-006: EfCdStudyRepository 구현

**EARS 패턴**: DI 컨테이너가 HnVue.CDBurning.IStudyRepository를 요청하는 경우, 시스템은 EF Core를 통해 CD 굽기 대상 스터디 목록을 제공하는 EfCdStudyRepository를 반환해야 한다.

**현재 동작**: `App.xaml.cs:210`에서 `NullCdStudyRepository` 등록.

**요구 동작**:
- `HnVue.Data/Repositories/EfCdStudyRepository.cs` 신규 작성
- StudyEntity를 CDBurning 도메인의 Study 모델로 매핑
- `App.xaml.cs:210`에서 `EfCdStudyRepository`로 교체

**수용 기준**:
- 스터디 목록 조회 단위 테스트 2개 이상
- 빈 목록 케이스 단위 테스트 1개 이상

---

### REQ-COORD-007: App.xaml.cs DI 등록 교체

**EARS 패턴**: 애플리케이션이 시작되는 경우, DI 컨테이너에 Null 구현체가 존재하면 안 되며, 6개 Repository는 반드시 EF Core 구현체로 등록되어야 한다.

**현재 동작**: `App.xaml.cs:178-210` 범위에 6개 NullRepository 등록.

**요구 동작**:
- 6개 NullRepository 등록 라인을 EF Core 구현체로 교체
- `AddSingleton` → `AddScoped` 전환 검토 (DbContext 수명 주기와 일치)
- 교체 후 주석으로 변경 이유 명시

**수용 기준**:
- `App.xaml.cs`에서 `NullDoseRepository`, `NullWorklistRepository`, `NullIncidentRepository`, `NullUpdateRepository`, `NullSystemSettingsRepository`, `NullCdStudyRepository` 미등장
- 앱 시작 후 DI 해석 오류 없음

---

### REQ-COORD-008: 통합 테스트 커버리지

**EARS 패턴**: EF Core Repository 구현체가 등록된 경우, 통합 테스트는 실제 SQLite in-memory 데이터베이스를 사용하여 Repository 동작을 검증해야 한다.

**요구 동작**:
- `tests.integration/HnVue.IntegrationTests/` 내 통합 테스트 6개 이상
- 각 Repository당 최소 1개의 통합 테스트 시나리오
- WebApplicationFactory 또는 직접 DbContext 초기화 방식 사용

**수용 기준**:
- 6개 통합 테스트 전원 통과
- 빌드 에러 0건

---

## 수용 기준 종합

| ID | 항목 | 검증 방법 | 우선순위 |
|----|------|-----------|---------|
| AC-001 | `NullDoseRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-002 | `NullWorklistRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-003 | `NullIncidentRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-004 | `NullUpdateRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-005 | `NullSystemSettingsRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-006 | `NullCdStudyRepository` App.xaml.cs에서 미등장 | 코드 검색 | P0 |
| AC-007 | 6개 EfRepository 클래스 신규 작성 완료 | 파일 존재 확인 | P0 |
| AC-008 | 단위 테스트 총 18개 이상 통과 | `dotnet test` | P0 |
| AC-009 | 통합 테스트 6개 이상 통과 | `dotnet test` | P0 |
| AC-010 | 전체 솔루션 빌드 0 에러 | `dotnet build` | P0 |
| AC-011 | DI 해석 오류 없이 앱 시작 | 앱 실행 확인 | P1 |

---

## 기술 접근 방안

### Repository 구현 패턴

```
HnVue.Data/
  Repositories/
    EfDoseRepository.cs          (신규)
    EfWorklistRepository.cs      (신규)
    EfIncidentRepository.cs      (신규)
    EfUpdateRepository.cs        (신규)
    EfSystemSettingsRepository.cs (신규)
    EfCdStudyRepository.cs       (신규)
```

### DI 교체 패턴

- 기존: `services.AddSingleton<IDoseRepository, NullDoseRepository>()`
- 변경: `services.AddScoped<IDoseRepository, EfDoseRepository>()`
- DbContext가 Scoped이므로 Repository도 Scoped 권장

### 통합 테스트 패턴

- 테스트당 새 in-memory SQLite DbContext 생성
- EnsureCreated() 후 시드 데이터 삽입
- Repository 동작 검증 후 Dispose

---

## 관련 문서

- `HnVue.App/App.xaml.cs` (라인 178-210)
- `SPEC-INFRA-001`: Team A 인프라 모듈 (ISystemSettingsRepository 인터페이스 정의)
- `.claude/rules/teams/coordinator.md`: Coordinator DI 등록 기준
- `WBS-001 v3.1`: 5.1.x Repository 구현 항목

---

## Git 완료 프로토콜

1. `git add` 변경 파일 (비밀키, 임시 파일 제외)
2. `git commit` — 커밋 메시지: `feat(coordinator): replace 6 null repository stubs with EF Core implementations`
3. `git push origin team/coordinator`
4. Gitea API로 PR 생성 (`http://10.11.1.40:7001/api/v1`, repo: `drake.lee/Console-GUI`)
5. PR URL을 DISPATCH Status 섹션에 기록
