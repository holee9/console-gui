# DISPATCH: S12-R2 — Team B

> **Sprint**: S12 | **Round**: 2 | **Date**: 2026-04-18
> **Team**: Team B (Medical Imaging Pipeline)
> **Priority**: P1

---

## Context

S12-R1 QA 결과: HnVue.Dicom 커버리지 **11.3%** (85% 기준 대비 심각한 갭)

전체 평균 76.2% 달성 실패의 주원인은 HnVue.Dicom 11.3%.
Team B Safety-Critical (Dose 99.6%, Incident 94.7%) PASS.

---

## Tasks

### Task 1: HnVue.Dicom 커버리지 개선 (P1)

**목표**: 50%+ 달성 (85% 목표는 R3에서 달성, R2는 기반 구축)

**구현 항목**:
1. `tests/HnVue.Dicom.Tests/` 현황 파악
2. 미테스트 주요 클래스/메서드 식별
3. 핵심 경로 테스트 작성 (C-STORE SCP/SCU, DicomTag 파싱, MWL)
4. `dotnet test tests/HnVue.Dicom.Tests/` → 커버리지 확인

---

## Acceptance Criteria

- [ ] HnVue.Dicom 커버리지 50%+ (from 11.3%)
- [ ] 기존 Team B 테스트 리그레션 없음
- [ ] 소유권 준수 (Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: HnVue.Dicom 커버리지 50%+ (P1) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet test tests/HnVue.Dicom.Tests/` 커버리지 50%+
- [ ] 기존 Dose/Incident/Imaging/Workflow 테스트 PASS 유지
- [ ] DISPATCH Status COMPLETED + 빌드 증거
- [ ] `/clear` 실행 완료
