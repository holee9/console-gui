# DISPATCH - Coordinator (S13-R1)

> **Sprint**: S13 | **Round**: 1 | **팀**: Coordinator (Integration)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

---

## 1. 작업 개요

ViewModel TODO 해결 + 통합테스트 보강 + UI.Contracts 업데이트.

## 2. 작업 범위

### Task 1: ViewModel TODO 9건 해결

**목표**: UI.ViewModels 프로젝트 내 TODO 항목 정리

- 각 ViewModel의 TODO 주석 분석
- 구현 가능한 항목 즉시 구현 (서비스 연동, 커맨드 바인딩)
- 인터페이스 변경 필요 시 UI.Contracts에 반영
- 구현 불가 항목은 NOTE 주석으로 사유 명시

### Task 2: 통합테스트 보강

**목표**: S13-R1 변경 모듈에 대한 통합테스트 추가

- STRIDE 보안 통제 관련 통합테스트 (Team A 변경분)
- PACS 전송 파이프라인 통합테스트 (Team B 변경분)
- DI 컨테이너 정상 등록 확인 테스트
- tests.integration/ 프로젝트 내에 작성

### Task 3: UI.Contracts 인터페이스 업데이트

**목표**: S13-R1 신규 기능에 필요한 인터페이스 추가/수정

- ITlsConnectionService 인터페이스 추가 (Team A 요청 시)
- IPrintService 인터페이스 추가 (Team B Print SCU)
- 기존 인터페이스 변경 시 영향 분석 + 전팀 통지

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | ViewModel TODO 해결 | NOT_STARTED | Coordinator | P1 | 코드 품질 향상 |
| T2 | 통합테스트 보강 | NOT_STARTED | Coordinator | P0 | 회귀 방지 |
| T3 | UI.Contracts 업데이트 | NOT_STARTED | Coordinator | P2 | 필요 시 수행 |

---

## 4. 완료 조건

- [ ] dotnet build 0 errors
- [ ] 통합테스트 전체 통과
- [ ] UI.Contracts, UI.ViewModels, App, tests.integration 범위 내 수정만
- [ ] DesignTime/ 수정 금지 (Design 팀 소유)
- [ ] DISPATCH Status COMPLETED + 빌드 증거

---

## 5. Build Evidence

_(작업 완료 후 기록)_

---

## 6. 비고

- DesignTime/은 Design 팀 단독 소유 — 절대 수정 금지
- 통합테스트 Mock은 tests.integration/에 별도 생성
- UI.Contracts 인터페이스 변경 시 Team A/B/Design에 통지
