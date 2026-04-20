# DISPATCH - Team B (S14-R1)

> **Sprint**: S14 | **Round**: 1 | **팀**: Team B (Medical Imaging)
> **발행일**: 2026-04-20
> **상태**: ACTIVE

---

## 1. 작업 개요

의료영상 모듈 커버리지 85%+ 달성 + 누락 Dicom 통합테스트 지원.

## 2. 작업 범위

### Task 1: Dicom 모듈 커버리지 개선

**목표**: HnVue.Dicom 커버리지 85%+ 달성

- 현재 커버리지 측정
- 미커버 시나리오 식별 (C-STORE, MWL, 에러 핸들링)
- 누락 테스트 케이스 추가
- fo-dicom 5.1.3 API 컨벤션 준수

### Task 2: Dose 모듈 커버리지 유지 (Safety-Critical 90%+)

**목표**: Safety-Critical 90%+ 유지 확인

- 현재 커버리지 확인
- 4-level dose interlock 로직 커버리지 검증
- 누락 엣지 케이스 보완

### Task 3: Workflow/PatientManagement 커버리지 개선

**목표**: 워크플로우 + 환자관리 모듈 85%+ 달성

- Workflow 9-state FSM 전이 커버리지 검증
- PatientManagement CRUD 시나리오 테스트 보완
- 전체 모듈 dotnet test 0 failures 확인

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | Dicom 커버리지 85%+ | IN_PROGRESS | Team B | P0 | 2026-04-20T20:30:00+09:00 | Standard |
| T2 | Dose 커버리지 90%+ 유지 | NOT_STARTED | Team B | P1 | _ | Safety-Critical |
| T3 | Workflow/PM 커버리지 85%+ | NOT_STARTED | Team B | P2 | _ | Standard |

---

## 4. 완료 조건

- [ ] Dicom 커버리지 >= 85%
- [ ] Dose 커버리지 >= 90%
- [ ] Workflow, PatientManagement 커버리지 >= 85%
- [ ] 전체 모듈 dotnet build 0 errors
- [ ] DISPATCH Status에 빌드 증거 기록

---

## 5. Build Evidence

_(작업 완료 후 기록)_
