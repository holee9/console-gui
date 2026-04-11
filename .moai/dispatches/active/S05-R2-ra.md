# DISPATCH: RA Team — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | RA Team |
| **브랜치** | team/ra |
| **유형** | S05 Round 2 — RMP v2.0 업데이트 |
| **우선순위** | P1 |
| **SPEC 참조** | IEC 62304 §7 / DOC-008 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-ra.md)만 Status 업데이트.

---

## 컨텍스트

S05 R1에서 DOC-042 CMP v2.0 완료 (IEC 62304 §8.1 충족).
RA 우선 작업 목록 #2: **RMP v2.0 업데이트** — 4-Tier 우선순위 시스템 + MR-072 통합.

현재 RMP: `docs/risk/DOC-008_Risk_Management_Plan_v1.0.md`

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 RMP 상태 확인
head -30 docs/risk/DOC-008_Risk_Management_Plan_v1.0.md
```

---

## Task 1 (P1): DOC-008 RMP v2.0 업데이트

### 업데이트 항목

1. **4-Tier 우선순위 시스템 통합**: MR 추적 체계에서 사용 중인 Priority 1-4 분류를 RMP에 반영
2. **MR-072 반영**: 최신 요구사항 추적 관계 업데이트
3. **버전 정보**: v1.0 → v2.0 (규제 재검토 필요 수준의 변경)
4. **승인일**: 2026-04-12

### IEC 62304 준수 확인

- §7.1 소프트웨어 개발 계획: 위험 관리 방법 기술
- §7.4 소프트웨어 위험 분석: FMEA 연계 확인
- 문서 버전 정책 준수 (Major 버전업 → 규제 재검토)

### 파일명 규칙

- 기존: `DOC-008_Risk_Management_Plan_v1.0.md`
- 신규: `DOC-008_Risk_Management_Plan_v2.0.md`
- 기존 파일은 `docs/archive/`로 이동 또는 버전 히스토리에 기록

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/risk/DOC-008_Risk_Management_Plan_v2.0.md
git add docs/archive/  # 구 버전 아카이브 시
# DISPATCH.md, CLAUDE.md 절대 추가 금지
git commit -m "docs(ra): DOC-008 RMP v2.0 업데이트 — 4-Tier 우선순위 시스템 + MR-072 통합"
git push origin team/ra
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DOC-008 RMP v2.0 업데이트 | NOT_STARTED | -- | 4-Tier + MR-072 반영 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
