# DISPATCH: S09-R1 — RA

Sprint: S09 | Round: 1 | Team: RA
Updated: 2026-04-14

---

## Context

S08-R2 MERGED 완료. SAD/SDS 업데이트 완료.
S09-R1에서는 Issue #91 Detector SDK SBOM/SOUP 등록.

---

## Tasks

### Task 1: Detector SDK SBOM/SOUP 등록 (P1) — Issue #91

**목표**: 신규 Detector SDK 컴포넌트를 SBOM(DOC-019)과 SOUP(DOC-033)에 등록

**상세**:
- DOC-019 SBOM: Detector SDK 어댑터 NuGet 패키지 또는 SDK 라이브러리 항목 추가
- DOC-033 SOUP: Third-party 컴포넌트로 분류 필요한 항목 식별 및 등록
- DOC-032 RTM: 관련 SWR-TC 매핑 업데이트 (필요시)

**수용 기준**:
- [ ] SBOM에 Detector SDK 관련 컴포넌트 등록 완료
- [ ] SOUP 리스트 업데이트
- [ ] RTM 매핑 확인 (갭 없음)

### Task 2: IDLE CONFIRM (P3)

SBOM/SOUP 업데이트 완료 후 대기.

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: SBOM/SOUP (P1) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |

---

## Self-Verification Checklist

- [ ] SBOM 업데이트 확인
- [ ] SOUP 업데이트 확인
- [ ] `git push origin team/ra`
