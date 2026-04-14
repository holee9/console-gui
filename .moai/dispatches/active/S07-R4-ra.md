# DISPATCH: RA — S07 Round 4

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | RA |
| **브랜치** | team/ra |
| **유형** | S07 R4 — RMP v2.0 검토 + RTM 갱신 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moji/dispatches/active/S07-R4-ra.md)만 Status 업데이트.

---

## 컨텍스트

S07-R3에서 RMP v2.0 초안 + DOC-006 SAD 업데이트 완료.
Coordinator가 DI Null Stub 교체 예정으로, 구현 변경에 따른 문서 갱신 필요.

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
```

---

## Task 1 (P1): RMP v2.0 리뷰 및 확정

S07-R3에서 작성된 RMP v2.0 초안 리뷰.

**수행 사항**:
- RMP v2.0 초안 내용 검토
- 4-Tier 우선순위 시스템 반영 확인
- MR-072 통합 여부 확인
- 필요시 수정 후 v2.0 확정

**목표**: RMP v2.0 확정

---

## Task 2 (P2): RTM 추적성 갱신

S07 전체 라운드 구현 변경사항을 RTM에 반영.

**수행 사항**:
- SWR → TC 매핑 업데이트
- IStudyItem 인터페이스 분리 (S07-R2) 관련 SWR 갱신
- DI Null Stub 교체 (S07-R4 Coordinator) 관련 SWR 갱신 준비
- 100% SWR → TC 매핑 유지 확인

**목표**: RTM 100% 추적성 유지

---

## Task 3 (P3): DOC-042 CMP v2.1 검증

DOC-042 CMP v2.1 완료 상태 확인.

**수행 사항**:
- CMP v2.1 내용 검토
- 구성 관리 항목 누락 여부 확인
- 빌드 환경 (DOC-043) 정합성 확인

**목표**: CMP v2.1 검증 완료

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/risk/ docs/verification/ docs/management/
git commit -m "docs(ra): S07-R4 RMP v2.0 확정 + RTM 갱신 (#issue)"
git push origin team/ra
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: RMP v2.0 확정 (P1) | NOT_STARTED | | |
| Task 2: RTM 갱신 (P2) | NOT_STARTED | | |
| Task 3: CMP v2.1 검증 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
