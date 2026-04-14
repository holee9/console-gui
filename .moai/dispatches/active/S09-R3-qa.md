# DISPATCH: S09-R3 — QA

Sprint: S09 | Round: 3 | Team: QA
Updated: 2026-04-15

---

## Context

S09-R2 QA CONDITIONAL PASS. Design Team이 Converter NullReference 14건 수정 후 재검증 필요.
커버리지 수집 0% 이슈 해결 필요.

---

## Tasks

### Task 1: Design Converter 수정 후 재검증 (P1)

Design Team이 S09-R3에서 Converter NullReference 수정 완료 후 품질게이트 재검증.

**의존성**: Design Team S09-R3 COMPLETED 후 실행

**검증 항목**:
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전원 통과 (2584/2584 PASS 목표 — 14개 실패 해소)
- [ ] 아키텍처 테스트 통과
- [ ] Design System Converter 14건 PASS 확인

### Task 2: 커버리지 수집 도구 복구 (P2)

S09-R2에서 커버리지 0% 수집 이슈 해결.

**원인 분석**:
- runsettings 구성 문제 또는 Coverlet 미연동 가능성
- 기존 `coverage.runsettings` 확인 및 수정

**해결 방안**:
1. `coverage.runsettings` 설정 점검 및 수정
2. Coverlet 패키지 버전 확인
3. 대안: dotnet-coverage 도구 시도
4. 최소 1개 모듈에서 커버리지 수집 성공 확인

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 재검증 (P1) | NOT_STARTED | - | Design 완료 후 |
| Task 2: 커버리지 복구 (P2) | NOT_STARTED | - | runsettings 점검 |

---

## Self-Verification Checklist

- [ ] 빌드 0에러 확인
- [ ] 전체 테스트 2584/2584 PASS 확인
- [ ] 커버리지 수집 성공 (0% 아님)
- [ ] DISPATCH Status에 빌드 증거 기록
