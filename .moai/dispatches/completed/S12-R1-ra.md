# DISPATCH: S12-R1 — RA

> **Sprint**: S12 | **Round**: 1 | **Date**: 2026-04-18
> **Team**: RA (Regulatory Affairs)
> **Priority**: P3

---

## Context

S11-R2 완료. S12-R1 목표: PASS 전환.

규제 문서 최신화 필요. 릴리즈 준비 시작.

---

## Tasks

### Task 1: DOC-032 RTM 업데이트 (P3)

**파일**: `docs/verification/RTM-DOC-032_v2.0.md`

**목표**: S12-R1 변경사항 SWR-TC 매핑

**구현 항목**:
1. S12-R1 새로운 기능/버그 수정 SWR 등록
2. TC(테스트 케이스) 매핑
3. 추적성 100% 달성 확인

**대상 기능**:
- Team A: Update 테스트 수정, 커버리지 개선
- Team B: Detector TODO 정리, Dicom 안정성
- Coordinator: UI 커버리지 개선
- Design: UI 커버리지 개선, TODO 정리
- QA: 테스트 리포트, PASS 판정

### Task 2: 릴리즈 준비 문서 검토 (P3)

**대상**: 릴리즈 관련 문서

**구현 항목**:
1. 릴리즈 준비 상태 점검
2. 누락된 문서 식별
3. 개선 필요 사항 도출

---

## Acceptance Criteria

- [x] DOC-032 RTM 업데이트 완료 (v2.6 → v2.7, 부록 G 추가, 11 SWR 매핑)
- [x] 릴리즈 준비 문서 검토 완료 (DOC-RELEASE-READINESS_S12-R1_v1.0 신규 작성)
- [x] 소유권 준수 (docs/verification/ + docs/management/ 만 수정)

## Evidence

### Task 1: DOC-032 RTM v2.7
- 파일: `docs/verification/DOC-032_RTM_v2.2.md`
- 변경 이력: v2.6 (S09-R3) → v2.7 (S10-R2~S12-R1)
- 부록 G 추가: 11개 SWR, 11개 TC, 100% 매핑
- 커버된 SWR:
  - SWR-UP-010 (EfUpdateRepository, Team A)
  - SWR-DC-060 (DICOM C-STORE error handling, Team B)
  - SWR-DC-061 (DicomStatus namespace, Team B)
  - SWR-DT-070 (OwnDetectorConfig null check, Team B)
  - SWR-UI-VM-010 (ViewModel coverage 92.58%, Coordinator)
  - SWR-UI-TOAST-010 (ToastItem coverage, Coordinator)
  - SWR-UI-MERGE-010 (MergeView PPT, Design)
  - SWR-UI-ACQ-010 (AcquisitionView, Design)
  - SWR-UI-COV-010 (UI coverage + DesignTime TODO, Design + Coordinator)
- 라인 수: 1204 → 1424 (+220)

### Task 2: Release Readiness Review
- 파일: `docs/management/DOC-RELEASE-READINESS_S12-R1_v1.0.md`
- 평가: 10개 블로킹 항목 체크 (DOC-034 기준)
  - Complete: 1/10 (10%)
  - Partial: 5/10 (50%)
  - In Progress: 2/10 (20%)
  - Not Started: 2/10 (20%)
- 릴리즈 준비도: 55% (Target 100%)
- 식별된 누락 문서: DOC-010 RMR, 사용적합성 테스트 보고서, DOC-044 Approved
- 업데이트 필요 문서: 6건 (우선순위 P1~P3 분류)
- 릴리즈 게이트 통과 로드맵: S12 → S13 → S14~S15 → Release

### 규제 준수
- IEC 62304 §5.8 (Software Release) 체크리스트 준수
- FDA 21 CFR 820.30 Design Controls 추적성 유지
- ISO 14971:2019 위험관리 문서 연계
- Minor version update (v2.6→v2.7): 규제 재심사 불필요 (non-substantive 추적성 보강)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DOC-032 RTM 업데이트 (P3) | COMPLETED | 2026-04-18 | v2.6→v2.7. 부록 G 추가 (11 SWR, 11 TC, 100% 매핑). S10-R2~S12-R1 변경사항 반영. 1204→1424줄 (+220) |
| Task 2: 릴리즈 준비 문서 검토 (P3) | COMPLETED | 2026-04-18 | DOC-RELEASE-READINESS_S12-R1_v1.0.md 신규 작성. 10개 블로킹 항목 평가, 릴리즈 준비도 55% 진단, 갭 분석 완료 |

---

## Self-Verification Checklist

- [x] RTM 업데이트 완료 (v2.7, 부록 G 추가)
- [x] 릴리즈 준비 검토 완료 (DOC-RELEASE-READINESS_S12-R1_v1.0 신규)
- [x] DISPATCH Status 업데이트 (NOT_STARTED → IN_PROGRESS → COMPLETED)
- [x] 소유권 준수 (docs/verification/, docs/management/ 만 수정 — 소스코드 미수정)
- [x] Minor version 정책 준수 (v2.6 → v2.7 non-substantive 추적성 보강)
- [x] RTM 추적성 gap 없음 (11 SWR → 11 TC 100% 매핑)
- [ ] `/clear` 실행 (COMPLETED push 후)
