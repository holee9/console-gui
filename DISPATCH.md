# DISPATCH: Team B — 빌드 오류 수정 + Safety-Critical 커버리지

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: **P0-Blocker** (빌드 오류) + P1-Critical (커버리지)
Supersedes: 이전 DISPATCH (상태 허위 — COMPLETED 기록했으나 체크박스 0/16 미완료)

## Team B 역할 재확인 (.claude/rules/teams/team-b.md)

- **소유 모듈**: Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning
- **Safety-Critical 기준**: Dose/Incident 90%+ Branch coverage (DOC-012)
- **FPD Detector**: IDetectorService 추상화, Simulator 어댑터 패턴
- **인터페이스 변경 시**: Coordinator 승인 필수

## How to Execute

1. **Task 1 (P0-Blocker)부터 반드시 먼저 수행**
2. 각 Task 완료 후 검증 기준 체크박스 `[x]` 업데이트
3. 모든 Task 완료 후 Final Build Verification
4. Status 섹션 정확하게 업데이트 (허위 보고 금지)

## Task 1: VendorAdapterTemplateTests 빌드 오류 수정 (P0-Blocker)

**오류**: `DetectorStateChangedEventArgs` 네임스페이스 누락 (CS0246, CS1503)
**파일**: `tests/HnVue.Detector.Tests/VendorAdapterTemplateTests.cs`
**수행**: `using HnVue.Common.Models;` 추가 또는 중복 로컬 정의 제거

**검증 기준**:
- [ ] HnVue.Detector.Tests 빌드 오류 0건
- [ ] 기존 Detector 테스트 전부 통과

## Task 2: Detector 42.6% → 85.0% (P1-Critical)

**규칙**: FPD Detector SDK Adapter Pattern — IDetectorService 추상화, Simulator 어댑터 테스트
**0% 클래스**: OwnDetectorAdapter, OwnDetectorConfig, VendorAdapterTemplate
**테스트 원칙**: Initialize→Connect→Configure→Acquire→Disconnect 라이프사이클, 하드웨어 Mock

**검증 기준**:
- [ ] Detector line coverage 85%+
- [ ] 0% 클래스 3개 모두 70%+
- [ ] 빌드 + 테스트 통과

## Task 3: Dose 67.6% → 90.0% (P1-Critical, Safety-Critical)

**규칙**: Dose 인터록 4-level 로직 불변, 변경 시 RA 위험평가 필요
**0% 클래스**: DoseRepository
**HARD GATE**: branch coverage 90%+ (DOC-012)

**검증 기준**:
- [ ] Dose line coverage 90%+
- [ ] Dose branch coverage 90%+
- [ ] DoseRepository 0% → 80%+
- [ ] 빌드 + 테스트 통과

## Task 4: Dicom 66.9% → 80.0% (P2-High)

**규칙**: fo-dicom 5.1.3, C-STORE SCP/SCU association negotiation, IHE TF 준수
**0% 클래스**: MppsScu

**검증 기준**:
- [ ] Dicom line coverage 80%+
- [ ] 빌드 + 테스트 통과

## Task 5: PatientManagement 72.7% → 80.0% (P2-High)

**규칙**: Patient data model 변경 시 Team A 조율 필요
**0% 클래스**: WorklistRepository

**검증 기준**:
- [ ] PatientManagement line coverage 80%+
- [ ] 빌드 + 테스트 통과

## Constraints

- Team B 소유 파일만 수정
- Safety-critical 소스 수정 시 characterization test 선행
- IDetectorService/IWorkflowEngine 변경 시 Coordinator 승인 필수


## Final Verification [HARD — 이 섹션 미완료 시 COMPLETED 보고 금지]

1. 자기 모듈 빌드: `dotnet build` → 오류 0건
2. 자기 테스트: `dotnet test {소유 테스트}` → 전원 통과
3. 전체 솔루션 빌드: `dotnet build HnVue.sln -c Release` → 결과 기록
4. 빌드 출력 요약을 Status에 복사

## Git Completion Protocol [HARD]

1. git add (DISPATCH.md + 변경 파일)
2. git commit (conventional commit 형식)
3. git push origin team/team-b
4. PR 생성 (기존 open PR 확인 후 중복 방지)
5. PR URL을 Status에 기록

## Status

- **State**: NOT_STARTED
- **Build Evidence**: (미완료)
- **PR**: (미생성)
- **Results**: Task 1→PENDING, Task 2→PENDING, Task 3→PENDING, Task 4→PENDING, Task 5→PENDING
