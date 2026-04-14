# DISPATCH: S09-R2 — Design

Sprint: S09 | Round: 2 | Team: Design
Updated: 2026-04-14

---

## Context

S09-R1 하드코딩 색상 토큰 교체 완료. DesignSystemConverters NotImplementedException 잔여.

---

## Tasks

### Task 1: DesignSystemConverters NotImplementedException 해소 (P1)

**파일**: `src/HnVue.UI/Converters/DesignSystemConverters.cs` (6개 NotImplementedException)

6개 Converter의 실제 구현:
- 디자인 토큰 기반 색상 변환 구현
- MahApps.Metro 테마 리소스와 연동
- 3테마 (Light/Dark/High Contrast) 지원

**주의**: PPT Scope Compliance 준수 — 지정된 PPT 페이지 범위 내에서만 작업.

### Task 2: DesignTime Mock 확장 (P2)

현재 2개 Mock (Settings, Studylist). 주요 화면 Mock 추가:

**우선순위**:
- [ ] WorkflowView Mock (Acquisition 화면, PPT slides 9-11)
- [ ] MergeView Mock (PPT slides 12-13)

**위치**: `src/HnVue.UI/DesignTime/` (Design 단독 소유)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Converters (P1) | NOT_STARTED | | |
| Task 2: DesignTime Mock (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] PPT scope compliance 확인
- [ ] DesignTime Mock 빌드 확인
