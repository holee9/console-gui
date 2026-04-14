# DISPATCH: Design — S07 Round 4

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Design |
| **브랜치** | team/team-design |
| **유형** | S07 R4 — UI Design Token 마이그레이션 유지 + 접근성 검증 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moji/dispatches/active/S07-R4-design.md)만 Status 업데이트.

---

## 컨텍스트

S07-R3에서 하드코딩 91건 semantic token 마이그레이션 완료.
현재 PPT 디자인 적용은 3/9 화면 완료 (Login, PatientList, Studylist).
나머지 6개 화면은 PPT 슬라이드 범위 준수 필수 (Issue #59 교훈).

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
```

---

## Task 1 (P1): Semantic Token 마이그레이션 검증

S07-R3에서 마이그레이션한 91건 semantic token이 정상 동작하는지 검증.

**수행 사항**:
- 3테마 (Light, Dark, High Contrast) 전환 시 색상 정상 적용 확인
- CoreTokens.xaml → SemanticTokens.xaml → ComponentTokens.xaml 체인 검증
- 누락된 하드코딩 색상 잔존 여부 재스캔

**목표**: 하드코딩 색상 0건, 3테마 정상 전환

---

## Task 2 (P2): 접근성 (IEC 62366 / WCAG 2.1 AA) 검증

현재 UI의 접근성 기준 충족 여부 검증.

**수행 사항**:
- 색상 대비 비율 4.5:1 이상 확인 (핵심 화면)
- 터치 타겟 44x44px 이상 확인
- Tab 내비게이션 논리적 순서 확인
- AutomationProperties 설정 여부 확인

**목표**: 핵심 화면 접근성 기준 충족

---

## Task 3 (P3): PPT 미적용 화면 현황 정리

현재 PPT 디자인 미적용 화면 6개의 구현 준비 상태 정리.
실제 PPT 구현은 다음 라운드에서 진행.

**수행 사항**:
- AddPatientProcedureView (슬라이드 8) PPT 요구사항 정리
- WorkflowView (슬라이드 9-11) PPT 요구사항 정리
- MergeView (슬라이드 12-13) PPT 요구사항 정리
- SettingsView (슬라이드 14-22) PPT 요구사항 정리
- 각 화면별 design token 필요항목 리스트업

**목표**: 화면별 PPT 요구사항 문서화

---

## PPT Scope 준수 [HARD — Issue #59]

- [HARD] 지정된 PPT 슬라이드 범위 외 UI 요소 구현 절대 금지
- [HARD] 썸네일 스트립/이미지 뷰어는 WorkflowView (슬라이드 9-11) 전용
- [HARD] Task 3은 현황 정리만. 실제 XAML 수정은 다음 라운드.

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/
git commit -m "chore(design): S07-R4 토큰 검증 + 접근성 확인 (#issue)"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Semantic Token 검증 (P1) | NOT_STARTED | | |
| Task 2: 접근성 검증 (P2) | NOT_STARTED | | |
| Task 3: PPT 미적용 화면 정리 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
