# DISPATCH - Team A (S15-R2)

> **Sprint**: S15 | **Round**: 2 | **팀**: Team A (Infrastructure)
> **발행일**: 2026-04-21
> **상태**: ACTIVE (Phase 1)

---

## 1. 작업 개요

App.xaml.cs AssemblyResolve 핸들러 코드 리뷰 및 승인. 해당 수정은 CC가 WPF BAML 어셈블리 해석 버그 임시수정으로 추가함.

## 2. 작업 범위

### Task 1: AssemblyResolve 핸들러 리뷰

**목표**: App.xaml.cs에 추가된 AssemblyResolve 정적 생성자 코드 리뷰

검토 포인트:
- Assembly.LoadFrom 사용의 보안 영향 평가
- Generic Host + WPF BAML 충돌 원인 분석
- 더 나은 대안이 있는지 검토 (예: AssemblyLoadContext 기반)

**파일**: `src/HnVue.App/App.xaml.cs` (static App() 생성자)

### Task 2: IDLE CONFIRM (리뷰 불필요 시)

리뷰 후 수정 필요 없으면 IDLE CONFIRM.

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | AssemblyResolve 핸들러 리뷰 | NOT_STARTED | Team A | P2 | _ | Phase 1 |
| T2 | IDLE CONFIRM (선택) | NOT_STARTED | Team A | P3 | _ | 리뷰 불필요 시 |

---

## 4. 완료 조건

- [ ] AssemblyResolve 코드 리뷰 완료
- [ ] 필요 시 개선 PR 또는 IDLE CONFIRM
- [ ] DISPATCH Status에 결과 기록

---

## 5. Build Evidence

(작업 완료 후 기록)
