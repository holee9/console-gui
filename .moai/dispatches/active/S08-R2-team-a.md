# DISPATCH: Team A — S08 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S08 R2 — 아키텍처 테스트: 디렉토리 소유권 검증 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R2-team-a.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1에서 Coordinator가 Design 소유 영역(`DesignTime/`)에 파일을 생성하는 충돌 발생.
role-matrix v2.0에서 디렉토리 단위 소유권 테이블을 추가했으나, 아키텍처 테스트 검증이 없음.
물리적 차단 레이어를 아키텍처 테스트에 추가.

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
```

---

## Task 1 (P2): 디렉토리 소유권 아키텍처 테스트 추가

role-matrix v2.0의 디렉토리 단위 소유권을 NetArchTest로 검증.

**수행 사항**:
- `tests/HnVue.Architecture.Tests/`에 새 테스트 파일 추가 (예: `DirectoryOwnershipTests.cs`)
- 검증 규칙:
  1. `src/HnVue.UI/DesignTime/` 내 .cs 파일에 `HnVue.UI.ViewModels` 네임스페이스 사용 금지 (Coordinator 침범 방지)
  2. `src/HnVue.UI/DesignTime/` 파일은 UI 프로젝트에만 존재해야 함
  3. `src/HnVue.UI.ViewModels/` 파일에 XAML 관련 타입 참조 금지 (View-ViewModel 분리)

**목표**: Coordinator가 실수로 DesignTime/에 파일 생성하면 빌드 타임에 감지

---

## 파일 소유권 (role-matrix v2.0)

| 파일 | 소유 | 비고 |
|------|------|------|
| `tests/HnVue.Architecture.Tests/**` | Team A (구현) + QA (집행) | 공동 소유 |

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/HnVue.Architecture.Tests/
git commit -m "test(team-a): S08-R2 디렉토리 소유권 아키텍처 테스트 추가 (#issue)"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 디렉토리 소유권 테스트 (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
