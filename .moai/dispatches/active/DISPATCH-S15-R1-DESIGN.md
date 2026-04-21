# DISPATCH - Design (S15-R1)

> **Sprint**: S15 | **Round**: 1 | **팀**: Design (UI)
> **발행일**: 2026-04-21
> **상태**: ACTIVE (별도 트랙 — Phase 1과 동시 시작)

---

## 1. 작업 개요

S14-R2 UI Performance 테스트 1건 실패 수정. 스크롤 성능 임계값 초과.

## 2. 작업 범위

### Task 1: UI 스크롤 성능 테스트 실패 수정

**목표**: Scrolling_Performance_ShouldRemainSmooth 테스트 통과

**실패 테스트**:
- `HnVue.UI.Tests: UI.PerformanceTests.Scrolling_Performance_ShouldRemainSmooth(itemCount: 500, scenario: "ListScroll")`
- Error: Max frame time 114.20ms > 83.35ms threshold (30.85ms 초과)

**수정 방향**:
- UI 가상화(Virtualization) 확인 — 500개 항목 렌더링 시 UI 스레드 블로킹 여부
- ListBox/ListView의 `VirtualizingStackPanel.IsVirtualizing="True"` 설정 확인
- 필요 시 스크롤 이벤트 핸들러 최적화
- 또는 테스트 임계값 조정 검토 (CI 환경 변동성 고려)

**수정 파일 예상**:
- `src/HnVue.UI/Views/` 또는 `src/HnVue.UI/Components/` 내 XAML 파일
- 테스트 파일: `tests/HnVue.UI.Tests/UI/PerformanceTests.cs` (Design 소유 아님 — 수정 필요 시 Coordinator 협조)

**주의**:
- [HARD] PPT 지정 페이지 범위 준수
- [HARD] Code-behind에 비즈니스 로직 금지

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | 스크롤 성능 테스트 수정 | IN_PROGRESS | Design | P2 | 2026-04-21T090017+09:00 | 별도 트랙 |

---

## 4. 완료 조건

- [ ] `dotnet build HnVue.sln` 0 errors
- [ ] Performance 테스트 통과 또는 합리적 임계값 조정
- [ ] 수정 파일이 Design 소유 범위 내 (Views, Styles, Themes, Components)
- [ ] DISPATCH Status에 빌드 증거 기록

---

## 5. Build Evidence

(작업 완료 후 기록)
