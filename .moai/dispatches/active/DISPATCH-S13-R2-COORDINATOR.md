# DISPATCH - Coordinator (S13-R2)

> **Sprint**: S13 | **Round**: 2 | **팀**: Coordinator (Integration)
> **발행일**: 2026-04-19
> **상태**: ACTIVE

---

## 1. 작업 개요

S13-R1 신규 서비스 DI 등록 완료 확인 + SettingsViewModel SaveAsync 플레이스홀더 구현.

## 2. 작업 범위

### Task 1: S13-R1 신규 서비스 DI 등록 검증

**목표**: S13-R1에서 추가된 서비스의 DI 등록 상태 확인

- ITlsConnectionService 등록 확인 (Team A STRIDE 보안 통제)
- IPrintService 등록 확인 (Team B Print SCU)
- 통합테스트에서 누락된 DI 등록 테스트 보강
- App.xaml.cs ServiceCollection 전체 검증

### Task 2: SettingsViewModel SaveAsync 구현

**목표**: SettingsViewModel 내 플레이스홀더 SaveAsync 실제 구현

- 현재 SaveAsync 구현 상태 확인
- 필요한 서비스 인터페이스(UI.Contracts) 정의
- 설정 저장 로직 구현
- 관련 통합테스트 추가

### Task 3: 통합테스트 누락 시나리오 보강

**목표**: S13-R1 코드 기반 누락된 통합테스트 시나리오 추가

- Print SCU → PACS 전송 엔드투엔드 테스트
- TLS 연결 → DICOM 통신 시나리오 테스트
- Workflow 상태 전이 → 선량 인터락 연동 테스트

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | S13-R1 DI 등록 검증 | NOT_STARTED | Coordinator | P0 | 신규 서비스 누락 방지 |
| T2 | SettingsViewModel SaveAsync | NOT_STARTED | Coordinator | P1 | 플레이스홀더 해결 |
| T3 | 통합테스트 누락 시나리오 | NOT_STARTED | Coordinator | P2 | 엔드투엔드 |

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
