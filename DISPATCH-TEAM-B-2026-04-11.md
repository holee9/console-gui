# DISPATCH: S04 R2 — Team B (Medical Imaging Pipeline)

Issued: 2026-04-11
Issued By: Main (MoAI Commander Center)
Sprint: S04 Round 2
SPEC: SPEC-TEAMB-COV-001 (partial → 목표 달성)
Priority: P1-Critical

## Objective

Team B 모듈 테스트 커버리지 목표 달성. Dicom 49.6%→80%, Update 75%→85%, Workflow 81.9%→85%.
S04 R1에서 Detector/Dose/PatientManagement은 목표 달성, Dicom/Update/Workflow 잔여.

## SPEC Reference

`.moai/specs/SPEC-TEAMB-COV-001/spec.md`

## Tasks

### T1: Dicom 커버리지 49.6% → 80% (REQ-COV-001)

**대상 모듈**: `src/HnVue.Dicom/`

현재 상태: Line 49.6%, Branch 52.3%

우선 작업 영역:
1. `DicomService.cs` — C-STORE/MWL 핸들러 예외 경로 테스트
2. `MppsScu.cs` — N-CREATE/N-SET 요청/응답 시나리오
3. `DicomStoreScu.cs` — C-STORE 연결/전송/해제 시나리오
4. `DicomTagHelper.cs` — 태그 조회/변환 엣지케이스
5. 네트워크 예외 (Timeout, ConnectionRefused, AssociationReject) 핸들링

테스트 파일: `tests/HnVue.Dicom.Tests/` 에 신규/추가

**목표**: 최소 25개 신규 테스트로 Line 80% 달성

### T2: Update 커버리지 75% → 85% (REQ-COV-002)

**대상 모듈**: `src/HnVue.Update/`

현재 상태: Line 75.0%, Branch 55.7%

우선 작업 영역:
1. `UpdateService.cs` — 버전 비교, 다운로드, 설치 플로우
2. `UpdateCheckService.cs` — 자동/수동 체크 시나리오
3. 롤백 시나리오 테스트
4. 네트워크 오류 복구 테스트

테스트 파일: `tests/HnVue.Update.Tests/` 에 신규/추가

**목표**: 최소 10개 신규 테스트로 Line 85% 달성

### T3: Workflow 커버리지 81.9% → 85% (REQ-COV-003)

**대상 모듈**: `src/HnVue.Workflow/`

현재 상태: Line 81.9%, Branch 74.5%

우선 작업 영역:
1. `WorkflowEngine.cs` — 상태전이 엣지케이스
2. `WorkflowState.cs` — InvalidTransition 예외
3. 이벤트 발행 검증

테스트 파일: `tests/HnVue.Workflow.Tests/` 에 신규/추가

**목표**: 최소 8개 신규 테스트로 Line 85% 달성

## Safety-Critical Rules

- Dose, Incident 모듈은 90%+ 유지 (현재 Dose 99.4%, Incident 96.1%)
- Safety-Critical 영역 수정 시 characterization test 먼저 작성
- 상태전이 로직 수정 금지 (WorkflowEngine 9-state 모델 불변)

## Build Verification [HARD]

```bash
dotnet build Console-GUI.sln --no-incremental
dotnet test Console-GUI.sln --filter "FullyQualifiedName~HnVue.Dicom|FullyQualifiedName~HnVue.Update|FullyQualifiedName~HnVue.Workflow" --no-build
```

**게이트**: 0 에러, 모든 신규 테스트 통과, 기존 테스트 regression 없음

## Git Protocol [HARD]

1. `git add` 관련 파일만
2. `git commit -m "test(team-b): SPEC-TEAMB-COV-001 Dicom/Update/Workflow 커버리지 목표 달성"`
3. `git push origin team/team-b`
4. PR 생성 (기존 open PR 있으면 업데이트)
5. PR URL을 DISPATCH.md Status에 기록

## Status

- **State**: PENDING
- **Assigned**: Team B
- **PR**: (작성 후 기록)
- **Started**: (시작 시 기록)
- **Completed**: (완료 시 기록)
