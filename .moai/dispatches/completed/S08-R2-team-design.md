# DISPATCH: Design — S08 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | Design |
| **브랜치** | team/team-design |
| **유형** | S08 R2 — DesignTime Mock 정비 + DesignTime 소유권 재확인 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R2-team-design.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1에서 Coordinator가 Design 소유 영역(`DesignTime/`)에 파일을 생성하여 충돌 발생.
role-matrix v2.0에서 `DesignTime/`의 Design 단독 소유권을 명시함.
S08-R2에서 DesignTime Mock을 정비하고 단독 소유권을 재확인.

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
```

---

## Task 1 (P2): DesignTimeStudylistViewModel 정비

머지 충돌 해결로 병합된 파일을 Design 팀 기준으로 재검토.

**수행 사항**:
- `src/HnVue.UI/DesignTime/DesignTimeStudylistViewModel.cs` 검토
- Coordinator가 추가한 `[ObservableProperty]` 속성 중 Design에 불필요한 항목 정리
- 샘플 데이터가 PPT slides 5-7에 부합하는지 확인
- Design 관점에서 Mock 데이터 적절성 검증

**목표**: DesignTime Mock이 Design 팀 관리 기준에 부합

---

## 파일 소유권 (role-matrix v2.0)

| 파일 | 소유 | 비고 |
|------|------|------|
| `src/HnVue.UI/DesignTime/**` | **Design 단독** | Coordinator 수정 금지 |
| `src/HnVue.UI/Views/**` | Design | XAML + code-behind |

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/DesignTime/
git commit -m "style(team-design): S08-R2 DesignTimeStudylistViewModel 정비 (#issue)"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DesignTime Mock 정비 (P2) | **COMPLETED** | 2026-04-14 | 검토 완료, 현재 상태 올바름 (변경 불필요) |
| Git 완료 프로토콜 | NOT_STARTED | | 변경 없으므로 skip |
