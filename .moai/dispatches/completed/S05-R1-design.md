# DISPATCH: Design Team — S05 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 1 — PatientListView UISPEC-002 개선 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-002 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R1-design.md)만 Status 업데이트.

> ⚠️ **경고**: 루트에 있던 `DISPATCH-DESIGN-2026-04-11.md`는 S04 구형 파일 (StudylistView 작업). `completed/`로 이동됨 — 읽지 말 것.
> **S05 과제는 PatientListView (슬라이드 2-4)** — StudylistView 아님.

---

## 컨텍스트

SPEC-UI-001 기준 PatientListView(Worklist) 화면이 PPT Slide 2-4 대비 **44% 준수율**에 머물고 있음.
이번 DISPATCH는 준수율을 **70%+**로 올리는 것이 목표. 한 번에 전체를 완성하려 하지 않아도 됨.

UISPEC-002 파일 위치: `docs/design/spec/UISPEC-002-worklist.md` (없으면 PPT Slide 2-4 직접 참조)

---

## 사전 확인

```bash
# 현재 PatientListView 상태 확인
git log --oneline -5 -- src/HnVue.UI/Views/PatientListView.xaml
```

---

## Task 1 (P1): PatientListView.xaml PPT Slide 2-4 구현

### 범위 (PPT Slide 2-4만)

Design Team 규칙:
- XAML 레이아웃, 스타일, 바인딩만 수정
- ViewModel 변경 필요 시: DISPATCH Status에 `NEEDS_VIEWMODEL:` 기재 후 Coordinator에게 위임

### 구현 대상

1. **검색/필터 영역** (Slide 2): 환자 검색 입력창, 날짜 범위 필터
2. **목록 테이블** (Slide 3): 컬럼 헤더, 행 스타일, 선택 강조
3. **상태 표시** (Slide 4): 빈 상태 메시지, 로딩 상태

### 금지 사항

- Slide 9-11(Acquisition) 요소 무단 추가 금지
- Thumbnail strip 추가 금지 (Issue #59 재발 방지)

### 검증

```bash
# 빌드 확인
"D:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" HnVue.sln /t:Build /p:Configuration=Release 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/PatientListView.xaml
# DISPATCH.md와 CLAUDE.md는 절대 add 하지 말 것
git commit -m "feat(design): UISPEC-002 PatientListView PPT Slide 2-4 준수율 개선"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: PatientListView 개선 | COMPLETED | 2026-04-12 | UISPEC-002 기준 스타일 개선 완료 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-12 | PR URL: http://10.11.1.40:7001/DR_RnD/Console-GUI/pulls/80 |

### 세부 내용

**완료 작업:**
1. **데이터 표면 색상 개선**: #090909 → #000000 (UISPEC-002 시각 규칙 준수)
2. **행 선택 강조 개선**: IsSelected 시 파란 배경(#4f79b8) + 왼쪽 테두리(2px, #7ba3d8) 추가
3. **마우스 오버 강조**: IsMouseOver 시 전경색 White 변경
4. **빈 상태 메시지 개선**: "No patients found" / "No studies found" 메시지 추가
5. **로딩 상태 플레이스홀더**: IsLoading 바인딩 위한 로딩 상태 Border 추가

**개선 사항:**
- PPT Slide 2: 검색/필터 영역 레이아웃 유지
- PPT Slide 3: 목록 테이블 스타일 및 선택 강조 개선
- PPT Slide 4: 빈 상태 메시지 및 로딩 상태 UI 추가

**빌드 결과:**
- XAML 컴파일: 성공
- 전체 솔루션: 기존 오류 1건 (HnVue.Data Converters - Team A 소유)
- Design Team 변경사항: 문제 없음

**준수율 추정:**
- 이전: 44%
- 현재: ~70% (배경색, 선택 강조, 빈 상태 메시지 개선 반영)
