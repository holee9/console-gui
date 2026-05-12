# DISPATCH S18-R2 — Team B

## Sprint: S18 | Round: R2 | Issued: 2026-05-12
## Team: Team B
## Priority: P2-High
## 근거 SPEC/문서: **SPEC-TEAMB-COV-001** (Team B 모듈 테스트 커버리지 목표 달성)
## Gitea Issue: **#129**

---

## 배경 (Background)

SPEC-TEAMB-COV-001의 목표: Team B 관할 4개 모듈(Detector, Dose, Dicom, PatientManagement)의 커버리지 격차 해소. S18-R2에서는 Dicom 모듈(REQ-COV-003)과 Update 모듈에 집중. Dicom 66.9%→80%, Update 75%→85%. Safety-Critical 모듈(Dose 90%+)은 S16-R1에서 99.5% 달성 완료.

---

## Tasks

### T1: MppsScu 테스트 커버리지 0% → 60%+ [P2]
- **설명**: `src/HnVue.Dicom/MppsScu.cs`의 SendInProgressAsync, SendCompletedAsync 메서드에 대한 단위 테스트 작성
- **체크리스트**:
  - [ ] SendInProgressAsync: 유효 파라미터 → MppsUid 반환 검증
  - [ ] SendInProgressAsync: Host 누락 → Result.Failure(ErrorCode.DicomConnectionFailed) 검증
  - [ ] SendInProgressAsync: 네트워크 예외 → ErrorCode.DicomConnectionFailed 검증
  - [ ] SendInProgressAsync: N-CREATE 비성공 응답 → Result.Failure 검증
  - [ ] SendInProgressAsync: CancellationToken 취소 동작 검증
  - [ ] SendCompletedAsync: 유효 파라미터 → Result.Success 검증
  - [ ] SendCompletedAsync: Completed/Discontinued 상태 매핑 검증
  - [ ] SendCompletedAsync: Null mppsUid → ArgumentNullException 검증
  - [ ] SendCompletedAsync: 네트워크 예외 처리 검증
  - [ ] SendCompletedAsync: 취소 동작 검증
- **완료 조건**: `tests/HnVue.Dicom.Tests/MppsScuTests.cs` 생성, 커버리지 60%+ 달성, `dotnet test` 통과

### T2: DicomOutbox 추가 테스트 62.5% → 80%+ [P2]
- **설명**: `src/HnVue.Dicom/DicomOutbox.cs`의 누락 경로 테스트 추가
- **체크리스트**:
  - [ ] EnqueueAsync 성공 시나리오 검증
  - [ ] Polly 지수 백오프 검증 (재시도 로직)
  - [ ] Dead-letter 로깅 동작 검증
  - [ ] 다중 아이템 순차 처리 검증
- **완료 조건**: `tests/HnVue.Dicom.Tests/DicomOutboxTests.cs` 확장, 커버리지 80%+ 달성

### T3: DicomService 추가 테스트 69.3% → 80%+ [P2]
- **설명**: `src/HnVue.Dicom/DicomService.cs`의 Store/Query/Print 경로 테스트 추가
- **체크리스트**:
  - [ ] StoreAsync 성공/실패 C-STORE 검증
  - [ ] QueryWorklistAsync 성공 C-FIND + 다중 응답 검증
  - [ ] PrintAsync N-CREATE/N-ACTION 워크플로우 검증
  - [ ] BuildWorklistRequest 날짜 범위/환자ID 필터 검증
  - [ ] MapToWorklistItem 복합 데이터셋/누락 태그 검증
- **완료 조건**: `tests/HnVue.Dicom.Tests/DicomServiceTests.cs` 확장, 커버리지 80%+ 달성

### T4: Update 모듈 테스트 75% → 85%+ [P2]
- **설명**: `src/HnVue.Update/`의 StagedUpdateService, UpdateRepository 추가 테스트
- **체크리스트**:
  - [ ] StagedUpdateService.DownloadUpdateAsync 단계별 검증
  - [ ] StagedUpdateService.ValidateUpdateAsync 해시/서명 검증
  - [ ] StagedUpdateService.ApplyUpdateAsync 백업/롤백 검증
  - [ ] UpdateRepository.QueryAvailableUpdatesAsync 필터링 검증
  - [ ] UpdateRepository.RecordUpdateHistoryAsync 기록 검증
- **완료 조건**: `tests/HnVue.Update.Tests/` 확장, 커버리지 85%+ 달성

### T5: DISPATCH Status 실시간 업데이트 (표준)
- **설명**: 작업 시작 시 IN_PROGRESS, 완료 시 COMPLETED, 차단 시 BLOCKED — 타임스탬프 필수
- **완료 조건**: Status 테이블에 모든 Task 타임스탬프가 정확히 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | MppsScu 테스트 커버리지 | NOT_STARTED | Team B | P2 | - | - |
| T2 | DicomOutbox 추가 테스트 | NOT_STARTED | Team B | P2 | - | - |
| T3 | DicomService 추가 테스트 | NOT_STARTED | Team B | P2 | - | - |
| T4 | Update 모듈 테스트 | NOT_STARTED | Team B | P2 | - | - |
| T5 | DISPATCH Status 업데이트 | NOT_STARTED | Team B | P3 | - | - |

상태 전환 규칙: `dispatch-protocol.md` §2 참조.
타임스탬프 포맷: `YYYY-MM-DDTHH:MM:SS+09:00` (KST, ISO-8601).

---

## Constraints [HARD]

- 소유 모듈만 수정: HnVue.Dicom, HnVue.Update, HnVue.Detector, HnVue.Dose, HnVue.Incident, HnVue.Workflow, HnVue.PatientManagement, HnVue.CDBurning
- 테스트 프로젝트만 수정: tests/HnVue.Dicom.Tests/, tests/HnVue.Update.Tests/
- 프로덕션 코드(src/) 수정 금지 — 테스트 코드만 작성
- DISPATCH 없는 자율 작업 금지
- 빌드/테스트 검증 없이 COMPLETED 보고 금지 (quality-standards.md §3 Self-Verification)
- ScheduleWakeup(300초) 유지 (session-lifecycle.md)

---

## Evidence Required

완료 보고 시 DISPATCH Status 비고 열에 아래 3개 증거 필수 (quality-standards.md §4):

1. `dotnet build HnVue.sln -c Release` 결과 (errors/warnings 개수)
2. `dotnet test tests/HnVue.Dicom.Tests tests/HnVue.Update.Tests` 결과 (PASS/FAIL/SKIP 수치)
3. `git diff --name-only main..HEAD` 결과 (테스트 파일만 수정 확인)

---

## 참고 문서

- **근거 SPEC**: `.moai/specs/SPEC-TEAMB-COV-001/spec.md` (REQ-COV-003: Dicom 80%, REQ-COV-002: Dose 90% — 이미 달성)
- **팀 규칙**: `.claude/rules/teams/team-b.md`
- **공통 프로토콜**: `.claude/rules/teams/dispatch-protocol.md`, `quality-standards.md`, `session-lifecycle.md`
- **역할 경계**: `.claude/rules/teams/role-matrix.md` (CONSTITUTIONAL)

---
