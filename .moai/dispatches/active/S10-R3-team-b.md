# DISPATCH: S10-R3 — Team B

Sprint: S10 | Round: 3 | Team: Team B
Updated: 2026-04-15

> **[CC 안내]** UI 커버리지 갭 해결 지원. Design 팀과 협업 필요.

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
- 문제 해결 방법 결정 (전문성 발휴)

**금지된 자율성:**
- [HARD] DISPATCH 파일 자율 검색 (CC 전용)
- [HARD] 다른 팀 DISPATCH 읽기 (Scope Limitation 위반)
- [HARD] IDLE 상태에서 자율 작업 (Protocol 위반)
- [HARD] _CURRENT.md 무시 (구버전 오독 위험)

### [HARD] IDLE 상태 절대 규칙

```
_Current.md에서 자신의 팀이 IDLE이면:
1. 즉시 IDLE 보고
2. DISPATCH 파일 검색 금지
3. 자율 작업 금지
4. CC 지시 대기
```

---

## Context

S09-R3 QA PASS (90.3% 커버리지).
HnVue.UI 83.0%로 85% 미달. 도메인 Converter/컨트롤 테스트 지원 필요.

---

## Tasks

### Task 1: 도메인 Converter 테스트 추가 (P2)

Design 팀 UI Components 커버리지 향상 지원.

**대상**:
- `HnVue.UI/Converters/SafeStateToColorConverter.cs`
- `HnVue.UI/Converters/AgeFromBirthDateConverter.cs`

**검증 기준**:
- [ ] Converter 단위 테스트 추가
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS

### Task 2: IDLE CONFIRM (P3)

긴급 작업 없음. IDLE 상태 확인.

**검증 기준**:
- [ ] DISPATCH 읽기 완료
- [ ] IDLE 상태를 CC에 보고

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Converter 테스트 (P2) | NOT_STARTED | - | |
| Task 2: IDLE CONFIRM (P3) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
