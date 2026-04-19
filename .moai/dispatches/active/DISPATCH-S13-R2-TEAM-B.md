# DISPATCH - Team B (S13-R2)

> **Sprint**: S13 | **Round**: 2 | **팀**: Team B (Medical Imaging)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

---

## 1. 작업 개요

Imaging 모듈 테스트 대폭 보강 (현재 4개 파일 → 최소 10개) + CDBurning 테스트 확장.

## 2. 작업 범위

### Task 1: Imaging 모듈 테스트 대폭 보강 [P0]

**목표**: HnVue.Imaging 테스트 파일 4개 → 최소 10개 확장

- 이미지 로딩/렌더링 파이프라인 테스트
- 이미지 처리 필터 테스트 (밝기, 대비, 확대/축소)
- 이미지 포맷 변환 테스트
- 성능 임계값 테스트 (대용량 이미지 처리)
- 오류 복구 테스트 (손상된 이미지, 메모리 부족)
- Safety-Adjacent 모듈: 85%+ 커버리지 목표

### Task 2: CDBurning 테스트 보강

**목표**: HnVue.CDBurning 테스트 파일 5개 → 최소 8개 확장

- DICOM CD/DVD 굽기 파이프라인 테스트
- 미디어 상태 확인 테스트
- 굽기 진행률 추적 테스트
- 오류 처리 (미디어 없음, 공간 부족)

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | Imaging 테스트 대폭 보강 | NOT_STARTED | Team B | P0 | 현재 4개 → 10개 목표 |
| T2 | CDBurning 테스트 보강 | NOT_STARTED | Team B | P2 | 현재 5개 → 8개 목표 |

---

## 4. 완료 조건

- [ ] dotnet build 0 errors
- [ ] dotnet test all passed
- [ ] HnVue.Imaging.Tests, HnVue.CDBurning.Tests 범위 내 수정만
- [ ] DISPATCH Status COMPLETED + 빌드 증거

---

## 5. Build Evidence

_(작업 완료 후 기록)_
