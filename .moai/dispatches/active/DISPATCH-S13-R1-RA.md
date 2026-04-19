# DISPATCH - RA (S13-R1)

> **Sprint**: S13 | **Round**: 1 | **팀**: RA (Regulatory Affairs)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

---

## 1. 작업 개요

M1 Gate 문서 준비 — STRIDE 검토, RTM 동기화, 인허가 문서 업데이트.

## 2. 작업 범위

### Task 1: STRIDE 위협모델 검토 승인 (M1 Gate)

**목표**: Team A STRIDE 구현 결과를 규제 관점에서 검토

- STRIDE 6개 시나리오 구현 상태 확인 (코드 리뷰 아님 — 문서 기준)
- 위협모델 문서(DOC-017) 업데이트 반영 여부 확인
- 보안 통제가 IEC 81001-5-1 요구사항 충족하는지 평가
- M1 Gate 통과 여부 판정 근거 문서화

### Task 2: RTM 동기화 (S13 진행분)

**목표**: S13-R1 구현 변경분을 RTM에 반영

- STRIDE 보안 통제 → 관련 SWR-SEC-xxx 매핑 업데이트
- Print SCU → 관련 SWR-DICOM-xxx 매핑 업데이트
- PACS 파이프라인 → 관련 SWR-WF-xxx 매핑 업데이트
- RTM DOC-032 v2.x 업데이트

### Task 3: SBOM/SOUP 업데이트 (변경 있을 시)

**목표**: 신규 NuGet 패키지 또는 의존성 변경 시 SBOM 업데이트

- S13-R1에서 신규 패키지 추가 여부 확인
- 추가 시: DOC-019 SBOM + DOC-033 SOUP 업데이트
- 변경 없을 시: "변경 없음" 기록

### Task 4: DOC-042 CMP v2.1 업데이트

**목표**: Configuration Management Plan 최신화

- S12-R4 → S13-R1 변경 이력 반영
- 현재 Sprint 상태 업데이트
- 마일스톤 진행 상태 반영

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | STRIDE 검토 승인 | NOT_STARTED | RA | P0 | M1 Gate 항목 |
| T2 | RTM 동기화 | NOT_STARTED | RA | P1 | 추적성 관리 |
| T3 | SBOM/SOUP 업데이트 | NOT_STARTED | RA | P2 | 변경 있을 시만 |
| T4 | CMP v2.1 업데이트 | NOT_STARTED | RA | P2 | 관리 문서 |

---

## 4. 완료 조건

- [ ] STRIDE 검토 완료 + M1 Gate 판정 근거
- [ ] RTM DOC-032 업데이트
- [ ] SBOM/SOUP 업데이트 (변경 있을 시)
- [ ] CMP DOC-042 v2.1 업데이트
- [ ] docs/regulatory/, docs/planning/, docs/verification/ 범위 내 수정만
- [ ] 소스코드 수정 금지
- [ ] DISPATCH Status COMPLETED + 문서 업데이트 증거

---

## 5. Build Evidence

_(문서 작업 — 빌드 불필요)_

---

## 6. 비고

- M1 Gate (2026-05-15)까지 STRIDE 검토 완료 필요
- RTM 100% SWR→TC 매핑 유지 목표
- SBOM: CycloneDX 1.5 형식, 42+ 컴포넌트 추적
