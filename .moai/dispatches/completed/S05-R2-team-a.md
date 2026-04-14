# DISPATCH: Team A — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S05 Round 2 — PHI AES-256-GCM 암호화 구현 |
| **우선순위** | P0-Blocker |
| **SPEC 참조** | SPEC-INFRA-002 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-team-a.md)만 Status 업데이트.

---

## 컨텍스트

SPEC-INFRA-002: PHI AES-256-GCM 암호화 구현.

현재 `NullPhiEncryptionService`가 DI에 등록되어 PHI 필드가 평문 저장.
IEC 62304, HIPAA 및 국내 의료기기 개인정보 보호 기준 위반 상태.

이 작업은 의료기기 규제 준수를 위한 **P0-Blocker**.

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 NullPhiEncryptionService 확인
grep -r "NullPhiEncryptionService" src/HnVue.Data/ src/HnVue.Security/
grep -r "IPhiEncryptionService" src/HnVue.Common/ src/HnVue.Data/ src/HnVue.Security/
```

---

## Task 1 (P0): AES-256-GCM 암호화 서비스 구현

### 범위 (SPEC-INFRA-002 기준)

1. `PhiEncryptionService` 클래스 구현 (AES-256-GCM)
   - 키 관리: secure configuration에서 키 로드
   - 암호화/복호화 메서드
   - IV(Initialization Vector) 난수 생성
   - 태그 검증 (GCM 인증)
2. DI 등록: `NullPhiEncryptionService` → `PhiEncryptionService` 교체
3. 단위 테스트:
   - 암호화/복호화 round-trip 테스트
   - 잘못된 키 복호화 실패 테스트
   - 빈 입력/null 입력 처리 테스트
   - IV 고유성 테스트

### 대상 파일

- `src/HnVue.Security/Encryption/PhiEncryptionService.cs` — 신규 또는 기존 수정
- `src/HnVue.Data/` — DI 등록 변경 (NullPhiEncryptionService → PhiEncryptionService)
- `tests/HnVue.Security.Tests/` — 단위 테스트

### 확인 사항

- AES-256-GCM: `System.Security.Cryptography.AesGcm` 클래스 사용 (.NET 6+)
- 키는 환경 변수 또는 secure config에서 로드 (절대 하드코딩 금지)
- 기존 데이터 마이그레이션은 이번 Scope 제외 (별도 Task)

### 검증

```bash
dotnet build HnVue.sln 2>&1 | tail -5
dotnet test tests/HnVue.Security.Tests/ 2>&1 | tail -10
```

---

## Task 2 (P2): NullPhiEncryptionService 제거

Task 1 완료 후:

1. `NullPhiEncryptionService` 클래스 제거
2. 모든 참조를 `PhiEncryptionService`로 교체
3. 빌드 확인

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.Security/ src/HnVue.Data/ tests/HnVue.Security.Tests/
git commit -m "feat(team-a): SPEC-INFRA-002 PHI AES-256-GCM 암호화 구현 — NullPhiEncryptionService 교체"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AES-256-GCM 구현 (P0) | NOT_STARTED | -- | IEC 62304 의무 |
| Task 2: NullPhiEncryptionService 제거 (P2) | NOT_STARTED | -- | Task 1 완료 후 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
