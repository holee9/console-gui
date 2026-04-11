# DISPATCH: S04 R2 — QA Team (Quality Assurance)

Issued: 2026-04-11
Issued By: Main (MoAI Commander Center)
Sprint: S04 Round 2
SPEC: SPEC-GOVERNANCE-001 (Draft → 구현)
Priority: P1-Critical

## Objective

1. 팀 개발 거버넌스 강제화 (Architecture Tests)
2. 커버리지 제외 정책 문서화 + 스크립트
3. 릴리즈 준비도 검증 체계 강화

## SPEC Reference

`.moai/specs/SPEC-GOVERNANCE-001/spec.md`

## Tasks

### T1: NetArchTest 아키텍처 테스트 구현 (REQ-GOV-001)

**파일**: `tests/HnVue.Architecture.Tests/` (신규 프로젝트 또는 기존 활용)

구현할 아키텍처 규칙:
1. **HnVue.UI 종속성 제한**: UI는 Common, UI.Contracts, MahApps.Metro, CommunityToolkit.Mvvm, LiveChartsCore만 참조 가능
2. **UI 금지 참조 검증**: Data, Security, Workflow, Imaging, Dicom, Dose, PatientManagement, Incident, Update, SystemAdmin, CDBurning 직접 참조 금지
3. **ViewModel 비즈니스 로직 금지**: ViewModel에서 인프라 계층 직접 호출 금지
4. **명명 규칙**: Repository는 `I{Name}Repository` / `Ef{Name}Repository` 패턴 준수
5. **Service 인터페이스**: 모든 Service는 인터페이스 가져야 함

최소 5개 아키텍처 테스트 작성.

### T2: 커버리지 제외 정책 문서화 (REQ-GOV-002)

**파일**: `docs/testing/coverage-exclusion-policy.md` (신규)

내용:
- WPF Views (코드비하인드): 0% 정당성 (UI 자동화 불가)
- EF Core Migrations: 0% 정당성 (생성 코드)
- 제외 기준과 승인 프로세스
- 유효 커버리지 계산 공식
- 목표: 유효 커버리지 85%+ (Views/Migrations 제외)

### T3: 커버리지 측정 스크립트 업데이트 (REQ-GOV-003)

**파일**: `scripts/qa/Generate-CoverageReport.ps1` (기존 수정 또는 신규)

기능:
- Views/Migrations 제외 커버리지 자동 계산
- 모듈별 목표 대비 달성률 리포트
- Safety-Critical 모듈 90% 게이트 검증
- JSON + HTML 리포트 출력

### T4: StyleCop 규칙 강화 (REQ-GOV-004)

**파일**: `.stylecop.json` (기존 수정)

- 문서화 규칙: Public 메서드 XML 주석 필수
- 명명 규칙: 필드 _camelCase, 상수 UPPER_SNAKE_CASE
- 정적 분석 위반 = 빌드 경고

## Build Verification [HARD]

```bash
dotnet build Console-GUI.sln --no-incremental
dotnet test Console-GUI.sln --filter "FullyQualifiedName~Architecture" --no-build
```

**게이트**: 0 에러, 아키텍처 테스트 모두 통과

## Git Protocol [HARD]

1. `git add` 관련 파일만
2. `git commit -m "feat(qa): SPEC-GOVERNANCE-001 아키텍처 테스트 + 커버리지 정책 구현"`
3. `git push origin team/qa`
4. PR 생성
5. PR URL을 DISPATCH.md Status에 기록

## Status

- **State**: PENDING
- **Assigned**: QA
- **PR**: (작성 후 기록)
- **Started**: (시작 시 기록)
- **Completed**: (완료 시 기록)
