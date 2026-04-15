# DISPATCH: S10-R2 — Design

Sprint: S10 | Round: 2 | Team: Design
Updated: 2026-04-15

> **[CC 안내]** S10-R1 Task 미완료. 재진행 지시.

---

## [HARD] Protocol — 자율주행 철학

**자율주행 = "마음대로"가 아닙니다. 명확한 룰 내에서 자율적으로 실행하는 것입니다.**

### [HARD] FIRST ACTION (세션 시작 시 반드시 실행)

```
Step 0: git pull origin main  ← _CURRENT.md 읽기 전 반드시 실행
Step 1: Read _CURRENT.md
Step 2: 자신의 팀 행(row)에서 파일명 확인
Step 3: 해당 파일만 읽기 (다른 팀 DISPATCH 절대 읽기 금지)
Step 4: 상태가 IDLE이면 → 즉시 IDLE 보고 (다른 작업 금지)
```

### [HARD] 자율주행 범위

**허용된 자율성:**
- Task 구현 방법 선택 (기술적 판단)
- 작업 순서 최적화 (효율성 추구)
- 문제 해결 방법 결정 (전문성 발휘)

**금지된 자율성:**
- [HARD] DISPATCH 파일 자율 검색 (CC 전용)
- [HARD] 다른 팀 DISPATCH 읽기 (Scope Limitation 위반)
- [HARD] IDLE 상태에서 자율 작업 (Protocol 위반)
- [HARD] _CURRENT.md 무시 (구버전 오독 위험)

### [HARD] IDLE 상태 절대 규칙

```
_CURRENT.md에서 자신의 팀이 IDLE이면:
1. 즉시 IDLE 보고
2. DISPATCH 파일 검색 금지
3. 자율 작업 금지
4. CC 지시 대기
```

---

## Context

S10-R1 Design Task 완료되지 않음. MergeView PPT 구현 필요.

---

## Tasks

### Task 1: PPT 미구현 화면 구현 — MergeView (P1)

PPT slides 12-13에 해당하는 MergeView 화면 구현.

**PPT Scope Compliance [HARD]**:
- Slides 12-13: MergeView.xaml ONLY
- 다른 화면 요소 절대 포함 금지

**검증 기준**:
- [ ] PPT 1:1 비교 완료
- [ ] `dotnet build` 0 errors
- [ ] DesignTime Mock 렌더링 정상

### Task 2: UI Components 커버리지 향상 (P2)

HnVue.UI 83.0% → 85%+ 기여. Components/Converters 테스트 가능 코드 개선.

**검증 기준**:
- [ ] UI Components 테스트 커버리지 향상
- [ ] `dotnet build` 0 errors

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: MergeView (P1) | COMPLETED | 2026-04-15 | PPT 1:1 비교 완료, build 0 errors, DesignTime 정상 |
| Task 2: UI 커버리지 (P2) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] PPT 1:1 비교 완료
- [ ] DesignTime Mock 렌더링 정상
