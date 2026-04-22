# DISPATCH S16-R2 — Team A (Infrastructure)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: Team A
## Priority: HIGH (P0-Blocker SPEC 실행)
## 근거 SPEC: SPEC-INFRA-002 (PHI AES-256-GCM 암호화 완전 구현)

---

## 배경

S14-R2 이후 실질 개발 0건. 팀 역량은 대기 중이나 근거 SPEC 집행이 멈춤.
SPEC-INFRA-002는 P0-Blocker(priority: P0-Blocker)로 2026-04-11 승인되었으나 spec.md만 존재.
이번 라운드에서 planning 산출물 완성 + 구현 착수.

---

## Tasks

### T1: SPEC-INFRA-002 Planning 산출물 작성 [P1]
- **설명**: `.moai/specs/SPEC-INFRA-002/`에 plan.md, acceptance.md, tasks.md 작성
- **체크리스트**:
  - [ ] research.md — 기존 NullPhiEncryptionService 참조 경로, HKDF 적용 가능 지점, SQLCipher 키 접근 경로 조사
  - [ ] plan.md — AES-256-GCM 구현 전략, HKDF 키 파생, PatientEntity 필드별 암호화 적용 순서
  - [ ] acceptance.md — SWR-CS-080 수용 기준 + 단위 10개/통합 2개 테스트 식별
  - [ ] tasks.md — RED-GREEN-REFACTOR 순서로 최소 단위 Task 분할
- **완료 조건**: 4개 파일 생성 + commit + push

### T2: AesGcmPhiEncryptionService 스켈레톤 + RED 테스트 [P2]
- **설명**: TDD RED phase로 진입. 실패하는 단위 테스트 최소 3개 작성 + 빈 서비스 클래스
- **체크리스트**:
  - [ ] `src/HnVue.Data/Services/AesGcmPhiEncryptionService.cs` 스켈레톤 (throw NotImplementedException)
  - [ ] `tests/HnVue.Data.Tests/Services/AesGcmPhiEncryptionServiceTests.cs` 신규
    - [ ] Encrypt_WithValidKey_ReturnsCiphertextDifferentFromPlaintext
    - [ ] Decrypt_WithTamperedAuthTag_ThrowsCryptographicException
    - [ ] Encrypt_TwiceWithSameInput_ProducesDifferentCiphertext (IV 무작위성)
  - [ ] `dotnet test --filter AesGcmPhiEncryptionServiceTests` 실행 → RED 확인 (모두 실패)
- **완료 조건**: 3개 테스트 RED 상태 commit + push

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 작업 시작 시 IN_PROGRESS, 완료 시 COMPLETED로 타임스탬프와 함께 업데이트
- **완료 조건**: Status 테이블 정확 반영 + push

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | SPEC-INFRA-002 Planning 산출물 | NOT_STARTED | Team A | P1 | - | research/plan/acceptance/tasks |
| T2 | AesGcmPhiEncryptionService RED | NOT_STARTED | Team A | P2 | - | TDD RED phase |
| T3 | DISPATCH Status 업데이트 | NOT_STARTED | Team A | P3 | - | 상시 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.Common`, `HnVue.Data`, `HnVue.Security`, `HnVue.SystemAdmin`, `HnVue.Update`
- [HARD] SPEC-INFRA-002 범위 엄수 — MFA, Key Rotation, 기존 데이터 마이그레이션은 Out of Scope
- [HARD] 빌드/테스트 검증 없이 COMPLETED 보고 금지 — `dotnet build` 0 errors 확인
- [HARD] RED 테스트는 "실패해야 함" — 테스트가 실수로 PASS되면 테스트 로직 재검토
- [HARD] ScheduleWakeup(900초) 유지 — 작업 완료 push 직후 재설정 (Phase 1, _CURRENT.md §팀별설정)

## Evidence Required

완료 보고 시 DISPATCH Status 비고 열에 아래 3개 증거 포함:
1. `dotnet build` 결과 요약 (errors/warnings 개수)
2. `dotnet test --filter AesGcmPhiEncryptionServiceTests` 결과 (RED 확인)
3. 생성된 4개 파일 경로 리스트

---

## 참고 문서

- `.moai/specs/SPEC-INFRA-002/spec.md` — 핵심 요구사항 (SWR-CS-080)
- `.moai/specs/SPEC-INFRA-001/` — 선행 SPEC (NullPhiEncryptionService 등록 위치 참조)
- `.claude/rules/teams/team-a.md` — Security 코드 표준 (AES-256, HKDF)
