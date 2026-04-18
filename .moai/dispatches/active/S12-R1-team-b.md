# DISPATCH: S12-R1 — Team B

> **Sprint**: S12 | **Round**: 1 | **Date**: 2026-04-18
> **Team**: Team B (Medical Imaging)
> **Priority**: P2

---

## Context

S11-R2 완료. S12-R1 목표: PASS 전환.

TODO/FIXME 정리 필요. Team B 소유 모듈에 3개 TODO 존재.

---

## Tasks

### Task 1: Detector TODO 정리 (P2)

**대상**: `src/HnVue.Detector/`

**TODO 항목**:
1. `OwnDetectorNativeMethods.cs:8` - TODO 주석 처리
2. `ThirdParty/VendorAdapterTemplate.cs:12` - TODO 주석 처리
3. `ThirdParty/Hme/HmeDetectorAdapter.cs:10` - TODO 주석 처리
4. `README.md:1` - README TODO 정리

**구현 항목**:
1. TODO 내용 검토
2. 구현 필요 항목: 이슈 등록 후 구현
3. 문서용 TODO: 설명으로 변경
4. 불필한 TODO: 삭제

### Task 2: Dicom 모듈 안정성 확인 (P3)

**대상**: `src/HnVue.Dicom/`

**구현 항목**:
1. C-STORE 에러 처리 개선 검증
2. MWL 쿼리 기능 확인
3. 안정성 테스트

---

## Acceptance Criteria

- [ ] Detector TODO 3개 정리 완료
- [ ] Dicom 모듈 안정성 확인
- [ ] 소유권 준수 (Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Detector TODO 정리 (P2) | NOT_STARTED | - | - |
| Task 2: Dicom 안정성 확인 (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] 소유권 준수 (Team B 모듈만)
- [ ] TODO 정리 완료
- [ ] Dicom 안정성 확인
- [ ] DISPATCH Status 업데이트
- [ ] `/clear` 실행 완료
