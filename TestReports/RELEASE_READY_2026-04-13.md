# Release Readiness Report — S05 R2

> DOC-034 기준 릴리즈 준비도 종합 평가
> Generated: 2026-04-13 | QA Team | Branch: team/qa

---

## Executive Summary

| 항목 | 상태 | 비고 |
|------|------|------|
| **Overall Readiness** | CONDITIONAL PASS | Blocker 1건 존재 |
| **Build** | PARTIAL FAIL | Data.Tests 270 에러 (Team A 소유) |
| **Tests** | PASS (2024/2025) | 99.95% 통과율 (Data.Tests 제외) |
| **Coverage** | 10/16 PASS | 6개 모듈 85% 미달 |
| **Architecture** | 1 FAIL | StudyItem 구체클래스 위반 (Coordinator 소유) |

---

## 1. Build Status

| 항목 | 결과 |
|------|------|
| **에러** | 270 (HnVue.Data.Tests 전체) |
| **경고** | 12,064 |
| **정상 빌드 프로젝트** | 33/34 |

### Blocker: HnVue.Data.Tests 빌드 실패

- **원인**: `await using var (ctx, connection) = CreateSqliteContext()` — 유효하지 않은 C# 튜플 디컨스트럭션 + await using 문법
- **영향 파일**: EfWorklistRepositoryTests.cs, EfUpdateRepositoryTests.cs, EfIncidentRepositoryTests.cs, EfDoseRepositoryTests.cs, EfSystemSettingsRepositoryTests.cs, EfCdStudyRepositoryTests.cs
- **소유**: Team A (HnVue.Data.Tests)
- **조치**: Team A DISPATCH S05-R2에서 수정 필요

---

## 2. Test Results

### 전체 요약 (Data.Tests 제외)

| 지표 | 수치 |
|------|------|
| 총 테스트 | 2,025 |
| 통과 | 2,024 |
| 실패 | 1 |
| 통과율 | 99.95% |

### 프로젝트별 결과

| 테스트 프로젝트 | 통과 | 실패 | 전체 | 상태 |
|----------------|------|------|------|------|
| HnVue.Common.Tests | 120 | 0 | 120 | PASS |
| HnVue.Security.Tests | 223 | 0 | 223 | PASS |
| HnVue.Update.Tests | 142 | 0 | 142 | PASS |
| HnVue.Detector.Tests | 122 | 0 | 122 | PASS |
| HnVue.Dicom.Tests | 204 | 0 | 204 | PASS |
| HnVue.Imaging.Tests | 54 | 0 | 54 | PASS |
| HnVue.Dose.Tests | 111 | 0 | 111 | PASS |
| HnVue.Incident.Tests | 59 | 0 | 59 | PASS |
| HnVue.Workflow.Tests | 179 | 0 | 179 | PASS |
| HnVue.PatientManagement.Tests | 59 | 0 | 59 | PASS |
| HnVue.CDBurning.Tests | 46 | 0 | 46 | PASS |
| HnVue.SystemAdmin.Tests | 62 | 0 | 62 | PASS |
| HnVue.UI.Tests | 515 | 0 | 515 | PASS |
| HnVue.UI.QA.Tests | 65 | 0 | 65 | PASS |
| HnVue.IntegrationTests | 53 | 0 | 53 | PASS |
| HnVue.Architecture.Tests | 10 | 1 | 11 | FAIL |
| **HnVue.Data.Tests** | **N/A** | **N/A** | **N/A** | **BUILD FAIL** |

### 실패 테스트 상세

1. **Architecture: `Contracts_Should_Contain_Only_Interfaces_And_Allowed_Dtos`**
   - 원인: `HnVue.UI.Contracts.Models.StudyItem` 구체 클래스 발견
   - 소유: Coordinator (UI.Contracts)
   - 영향: 아키텍처 거버넌스 위반

---

## 3. Module Coverage (runsettings 기반)

### Safety-Critical Modules (목표 90%+)

| 모듈 | 커버리지 | 목표 | 상태 | Gap |
|------|---------|------|------|-----|
| HnVue.Dose | 90.8% | 90% | PASS | +0.8% |
| HnVue.Incident | 82.8% | 90% | FAIL | -7.2% |
| HnVue.Security | 91.4% | 90% | PASS | +1.4% |
| HnVue.Update | 92.5% | 90% | PASS | +3.5% |

### Standard Modules (목표 85%+)

| 모듈 | 커버리지 | 목표 | 상태 | Gap |
|------|---------|------|------|-----|
| HnVue.Common | 89.0% | 85% | PASS | +4.0% |
| HnVue.Data | 43.6% | 85% | FAIL | -41.4% |
| HnVue.Detector | 80.0% | 85% | FAIL | -5.0% |
| HnVue.Dicom | 87.4% | 85% | PASS | +2.4% |
| HnVue.Imaging | 88.2% | 85% | PASS | +3.2% |
| HnVue.Workflow | 91.9% | 85% | PASS | +6.9% |
| HnVue.PatientManagement | 100.0% | 85% | PASS | +15.0% |
| HnVue.CDBurning | 100.0% | 85% | PASS | +15.0% |
| HnVue.SystemAdmin | 60.3% | 85% | FAIL | -24.7% |
| HnVue.UI | 83.1% | 85% | FAIL | -1.9% |
| HnVue.UI.Contracts | 90.0% | 85% | PASS | +5.0% |
| HnVue.UI.ViewModels | 81.2% | 85% | FAIL | -3.8% |

### 요약

- **PASS**: 10/16 모듈 (62.5%)
- **FAIL**: 6/16 모듈 (37.5%)
- **Safety-Critical PASS**: 3/4 (Incident 제외)

---

## 4. Architecture Tests

| 항목 | 결과 |
|------|------|
| 총 테스트 | 11 |
| 통과 | 10 |
| 실패 | 1 |
| 위반 | `StudyItem` 구체 클래스 in UI.Contracts |

---

## 5. Blocker 이슈

### BLOCKER-1: HnVue.Data.Tests 빌드 실패 (P0)

- **소유**: Team A
- **원인**: `await using var (ctx, connection) = CreateSqliteContext()` — 무효 C# 문법
- **해결**: `var (ctx, connection) = CreateSqliteContext(); await using var _ = ctx;` 또는 기존 `using` 블록 패턴으로 변경
- **참고**: `HnVueDbContextTests.cs`의 기존 패턴(`var (ctx, conn) = ...; using(conn) using(ctx)`) 참조

### BLOCKER-2: HnVue.UI.Contracts 아키텍처 위반 (P1)

- **소유**: Coordinator
- **원인**: `StudyItem` 구체 클래스가 UI.Contracts에 존재
- **해결**: 인터페이스/DTO/열거형만 허용 규칙에 맞게 StudyItem을 DTO로 변환 또는 제거

---

## 6. 커버리지 갭 이슈 (85% 미달)

| 우선순위 | 모듈 | 현재 | Gap | 소유 팀 | 비고 |
|---------|------|------|-----|---------|------|
| P0 | HnVue.Data | 43.6% | -41.4% | Team A | 테스트 빌드 불가로 실제는 N/A |
| P1 | HnVue.SystemAdmin | 60.3% | -24.7% | Team A | 상대적 갭 최대 |
| P1 | HnVue.Incident | 82.8% | -7.2% | Team B | Safety-Critical |
| P2 | HnVue.Detector | 80.0% | -5.0% | Team B | Team B DISPATCH 진행 중 |
| P2 | HnVue.UI.ViewModels | 81.2% | -3.8% | Coordinator | |
| P2 | HnVue.UI | 83.1% | -1.9% | Design | 1.9% 갭, 근접 |

---

## 7. DOC-034 릴리즈 체크리스트 평가

| # | 항목 | 상태 | 비고 |
|---|------|------|------|
| 1 | 빌드 에러 0건 | FAIL | Data.Tests 270 에러 |
| 2 | 전체 테스트 통과 | PASS | 2024/2025 통과 (Data.Tests 제외) |
| 3 | Safety-Critical 90%+ | PARTIAL | Incident 82.8% 미달 |
| 4 | 일반 모듈 85%+ | PARTIAL | 5개 모듈 미달 |
| 5 | 아키텍처 테스트 통과 | FAIL | 1건 위반 |
| 6 | OWASP CVSS < 7.0 | PASS | 이전 스캔 기준 (SBOM 42컴포넌트) |
| 7 | 코드 리뷰 완료 | N/A | PR 미생성 (본 보고서 선행) |
| 8 | 릴리즈 노트 작성 | PENDING | |
| 9 | DB 마이그레이션 검증 | N/A | 이번 릴리즈 해당 없음 |
| 10 | 4-서명 승인 | PENDING | SW Dev Lead -> QA Lead -> RA/QA Manager -> PM |

---

## 8. 권고사항

1. **Team A 우선 조치**: Data.Tests 빌드 에러 수정 (`await using` 튜플 디컨스트럭션 문법 오류)
2. **Team A 후속**: SystemAdmin 커버리지 60.3% → 85% 개선
3. **Team B**: Incident 커버리지 82.8% → 90% (Safety-Critical), Dicom 개선 DISPATCH 이미 진행 중
4. **Coordinator**: StudyItem 아키텍처 위반 해결, UI.ViewModels 커버리지 개선

---

*Report generated by QA Team | DISPATCH S05-R2 Task 1*
*Coverage collected with coverage.runsettings (SkipAutoProps=true, Exclude DesignTime/Migrations)*
