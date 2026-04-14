# DISPATCH: RA — S07 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression) |
| **대상** | RA |
| **브랜치** | team/ra |
| **유형** | S07 R1 — IEC 62304 문서 동기화 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R1-ra.md)만 Status 업데이트.

---

## 컨텍스트

S06-R2에서 SDK 5종 추가됨. SBOM/SOUP은 업데이트 완료.
다음 우선순위: DOC-042 CMP 완성 + RTM 업데이트.

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
```

---

## Task 1 (P1): DOC-042 CMP (Configuration Management Plan) Draft → Complete

현재 상태: Draft. IEC 62304 §8.2.2 요구사항 충족 필요.

확인 항목:
- 소프트웨어 식별 체계 (버전 네이밍 규칙)
- 변경 관리 절차
- 빌드/릴리즈 절차
- 백업/복구 절차

---

## Task 2 (P2): DOC-032 RTM 업데이트

S06-R2 신규 기능(Detector SDK 통합)에 대한 요구사항-테스트 매핑 갱신.

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/
git commit -m "docs(ra): S07-R1 CMP 완성 + RTM 업데이트 (#issue)"
git push origin team/ra
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DOC-042 CMP 완성 (P1) | COMPLETED | 2026-04-14 | v2.1: IEC 62304 Section 8.2.2 백업/복구 절차 신설, CI-006 SDK 5종 상세화, v1.0 Obsolete 표시 |
| Task 2: DOC-032 RTM 업데이트 (P2) | COMPLETED | 2026-04-14 | v2.2: SWR-DET-010(7TC), SWR-DT-060(1TC), SWR-DT-061(1TC), SWR-WF-030(3TC) = 12TC 100% 매핑 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 | commit fcb1e85, pushed to origin/main |
