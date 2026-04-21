# DISPATCH - Team A (S15-R1)

> **Sprint**: S15 | **Round**: 1 | **팀**: Team A (Infrastructure)
> **발행일**: 2026-04-21
> **상태**: ACTIVE (Phase 1)
> **의존성**: 없음 (Phase 1 즉시 시작)

---

## 1. 작업 개요

S14-R2 QA 검증에서 HnVue.Update.Tests 5건 실패 수정. 임시 디렉토리 경로 문제가 근원 원인.

## 2. 작업 범위

### Task 1: HnVue.Update.Tests 백업 관련 5건 실패 수정

**목표**: Update.Tests 전체 통과 (현재 312/317 passed, 5 failures)

**실패 테스트 목록**:
1. `UpdateCoverageBoostTests.BackupManager_MultipleBackups_GetLatestReturnsNewest` — DirectoryNotFoundException (temp 경로 미생성)
2. `BackupManagerTests.RestoreFromBackupAsync_SelectsMostRecentBackup` — DirectoryNotFoundException
3. `UpdateSafetyCriticalCoverageTests.BackupService_ListBackups_WithBackups_ReturnsSortedList` — Expected 2 items, found 0
4. `BackupServiceCoverageTests.ListBackups_WithBackups_ReturnsSortedList` — DirectoryNotFoundException
5. `BackupManagerTests.ListBackups_AfterTwoBackups_ReturnsTwoInDescendingOrder` — Expected 2, found 0

**근원 원인 분석**:
- `DirectoryNotFoundException`: 백업 테스트에서 임시 디렉토리 생성 후 파일 쓰기 시 중간 디렉토리가 존재하지 않음
- `Expected 2, found 0`: 백업 파일 생성 실패로 인한 빈 목록 반환

**수정 방향**:
- 테스트 Setup에서 `Directory.CreateDirectory()`로 전체 경로 생성 보장
- 백업 테스트의 temp 경로 생성 로직 보강

**수정 파일 예상**:
- `tests/HnVue.Update.Tests/UpdateCoverageBoostTests.cs`
- `tests/HnVue.Update.Tests/BackupManagerTests.cs`
- `tests/HnVue.Update.Tests/UpdateSafetyCriticalCoverageTests.cs`
- `tests/HnVue.Update.Tests/BackupServiceCoverageTests.cs`

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Update.Tests 5건 실패 수정 | IN_PROGRESS | Team A | P0 | 2026-04-21T09:05:00+09:00 | Phase 1 시작 |

---

## 4. 완료 조건

- [ ] `dotnet build HnVue.sln` 0 errors
- [ ] `dotnet test tests/HnVue.Update.Tests/` 0 failures (전체 통과)
- [ ] 수정 파일이 Team A 소유 범위 내 (Update 모듈 + 테스트)
- [ ] DISPATCH Status에 빌드 증거 기록

---

## 5. Build Evidence

(작업 완료 후 기록)
