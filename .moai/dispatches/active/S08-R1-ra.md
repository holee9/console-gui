# DISPATCH: RA — S08 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | RA |
| **브랜치** | team/ra |
| **유형** | S08 R1 — StudylistView 문서 동기화 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R1-ra.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1은 StudylistView (PPT slides 5-7) 구현 라운드.
Coordinator가 IStudylistViewModel 인터페이스 추가 → SRS/RTM 업데이트 필요.
Design이 StudylistView.xaml 추가 → FRS 업데이트 필요.

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
```

---

## Task 1 (P2): RTM 업데이트

IStudylistViewModel 인터페이스 추가에 따른 RTM 갱신.

**수행 사항**:
- Coordinator 인터페이스 변경사항 확인
- 신규 SWR 등록 (필요시)
- TC 매핑 업데이트
- RTM v2.3 → v2.4 갱신

**목표**: RTM 100% SWR→TC 매핑 유지

---

## Task 2 (P3): SRS/FRS 동기화 확인

StudylistView 기능 추가에 따른 SRS/FRS 업데이트 필요 여부 확인.

**수행 사항**:
- DOC-005 SRS: 스터디리스트 기능 요구사항 반영 여부 확인
- DOC-004 FRS: 스터디리스트 화면 요구사항 반영 여부 확인
- 변경 필요시 업데이트, 불필요시 확인 완료

**목표**: 문서 동기화 상태 확인

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/verification/ docs/planning/
git commit -m "docs(ra): S08-R1 RTM v2.4 + 문서 동기화 (#issue)"
git push origin team/ra
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: RTM 업데이트 (P2) | NOT_STARTED | | |
| Task 2: SRS/FRS 동기화 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
