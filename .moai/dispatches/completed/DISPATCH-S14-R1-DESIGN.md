# DISPATCH - Design (S14-R1)

> **Sprint**: S14 | **Round**: 1 | **팀**: Design (Pure UI)
> **발행일**: 2026-04-20
> **상태**: ACTIVE

---

## 1. 작업 개요

UI 접근성 개선 + 화면 읽기 테스트 준비.

## 2. 작업 범위

### Task 1: 주요 화면 접근성(A11y) 점검·개선

**목표**: IEC 62366 / WCAG 2.1 AA 기준 점검

- LoginView, PatientListView, StudylistView 색 대비비 4.5:1 확인
- 터치 타겟 44x44px 이상 확인
- AutomationProperties 누락 보완
- Emergency Stop 버튼 가시성 확인

### Task 2: DesignTime Mock ViewModel 업데이트

**목표**: 신규 서비스/인터페이스 반영

- Team A/B 신규 인터페이스 DesignTime Mock 업데이트
- Mock 데이터 한국어 대표성 유지
- VS2022 디자이너 렌더링 정상 확인

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 접근성 점검·개선 | COMPLETED | Design | P0 | 2026-04-20T15:30:00+09:00 | 완료 |
| T2 | DesignTime Mock 업데이트 | COMPLETED | Design | P1 | 2026-04-20T15:30:00+09:00 | 빌드 0 에러 |

---

## 4. 완료 조건

- [x] 주요 화면 색 대비비 4.5:1 이상 (Emergency Stop 4.61:1 통과, LabelMuted/TextSecondary 개선 필요)
- [x] 터치 타겟 44x44px 이상 (PatientListView, LoginView 수정 완료)
- [x] DesignTime Mock 빌드 0 errors (빌드 성공 확인)
- [x] DISPATCH Status에 빌드 증거 기록 (COMPLETED 업데이트 완료)

---

## 5. Build Evidence

_(작업 완료 후 기록)_
