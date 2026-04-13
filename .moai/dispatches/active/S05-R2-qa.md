# DISPATCH: QA Team — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | QA Team |
| **브랜치** | team/qa |
| **유형** | S05 Round 2 — 릴리즈 준비도 보고서 생성 |
| **우선순위** | P1 |
| **SPEC 참조** | DOC-034 릴리즈 체크리스트 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-qa.md)만 Status 업데이트.

---

## 컨텍스트

S05 R1에서 아키텍처 테스트 수정 완료. 전팀 커버리지 85%+ 달성 중.
이번 라운드는 **릴리즈 준비도 보고서(DOC-034)** 생성 및 현재 품질 상태 종합 평가.

---

## 사전 확인

```bash
git checkout team/qa
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
```

---

## Task 1 (P1): 릴리즈 준비도 보고서 생성

### 방법

```bash
# 전체 솔루션 빌드 확인
dotnet build HnVue.sln 2>&1 | tail -10

# 전체 테스트 실행
dotnet test HnVue.sln --logger "trx" 2>&1 | tail -20

# 커버리지 수집
dotnet test HnVue.sln --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

### 보고서 항목 (DOC-034 기준)

1. **빌드 상태**: 에러 0, 경고 수
2. **테스트 통과율**: 전체 테스트 수 / 통과 수
3. **모듈별 커버리지**: Safety-Critical 90%+, 일반 85%+
4. **아키텍처 테스트**: 위반 0 확인
5. **미해결 이슈**: priority-high 레이블 이슈 수

### 출력

- `TestReports/RELEASE_READY_2026-04-12.md` (Markdown 형식)
- DISPATCH Status에 핵심 수치 기재

---

## Task 2 (P2): 커버리지 갭 이슈 생성

현재 85% 미달 모듈이 있다면:
- Gitea 이슈 생성: `qa-result` + `priority-high` 레이블
- 이슈 번호를 DISPATCH Status에 기재

---

## Git 완료 프로토콜 [HARD]

```bash
git add TestReports/
# DISPATCH.md, CLAUDE.md 절대 추가 금지
git commit -m "docs(qa): S05-R2 릴리즈 준비도 보고서 생성 — DOC-034 기준"
git push origin team/qa
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 릴리즈 준비도 보고서 | NOT_STARTED | -- | DOC-034 기준 |
| Task 2: 커버리지 갭 이슈 | NOT_STARTED | -- | 해당 없으면 SKIP |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
