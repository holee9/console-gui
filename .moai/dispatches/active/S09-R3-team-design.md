# DISPATCH: S09-R3 — Design

Sprint: S09 | Round: 3 | Team: Design
Updated: 2026-04-15

---

## Context

S09-R2 QA 품질게이트 결과: CONDITIONAL PASS. DesignSystemConverters 14건 NullReferenceException 실패 감지.
Design 팀이 R2에서 구현한 DesignSystemConverters가 단위테스트 환경에서 `Application.Current` null 접근으로 실패.

---

## Tasks

### Task 1: DesignSystemConverters NullReference 수정 (P1)

**문제**: `DesignTokenResources.ResolveStatusBrush()` 메서드에서 `Application.Current.TryFindResource()` 호출 시
단위테스트 환경에서 `Application.Current`가 null이어서 NullReferenceException 발생.

**수정 대상 파일**: `src/HnVue.UI/Converters/DesignSystemConverters.cs`

**수정 방법**:
1. `ResolveStatusBrush()` 메서드에 null guard 추가:
   ```csharp
   var app = Application.Current;
   if (app == null) return Brushes.Gray; // DesignTime/Test fallback
   if (app.TryFindResource(resourceKey) is Brush brush) return brush;
   return Brushes.Gray;
   ```
2. 동일한 패턴의 다른 Converter 메서드도 null guard 일괄 적용
3. Converter 테스트 파일에서 확인

**검증 기준**:
- [ ] `dotnet build` 0 errors
- [ ] DesignSystemConverter 관련 14개 테스트 전원 PASS
- [ ] 기존 정상 동작(런타임 토큰 해석) regression 없음

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Converter 수정 (P1) | NOT_STARTED | - | NullReference 14건 |

---

## Self-Verification Checklist

- [ ] 빌드 0에러 확인
- [ ] Converter 14개 테스트 전원 통과
- [ ] 수정 범위가 Design 소유 모듈(Converter) 내인지 확인
- [ ] DISPATCH Status에 빌드 증거 기록
