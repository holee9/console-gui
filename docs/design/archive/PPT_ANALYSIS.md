# PPT 분석 보고서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 원천 | `docs/★HnVUE UI 변경 최종안_251118.pptx` |
| 분석 방식 | slide XML, notes XML, embedded screenshot 확인 |
| 갱신일 | 2026-04-09 |

## 분석 결론

현재 활성 설계 해석은 `HnVUE UI 변경 최종안_251118.pptx`를 단일 원천으로 삼는다. `변경 2안`이 있는 화면은 `2안`을 최종안으로 채택하고, `2안`이 없는 화면은 마지막 제시안과 notes를 최종안으로 본다.

이 PPT는 `MainWindow 3열 셸`을 기준으로 설명하지 않는다. 활성 기준에서는 아래 창들을 독립 화면으로 관리한다.

- Login
- Worklist
- Studylist
- Image
- Acquisition
- Add Patient / Procedure
- Merge
- Setting

## 슬라이드별 판정

| 화면 | 슬라이드 | 판정 | 핵심 포인트 |
|------|----------|------|-------------|
| Login | 1 | Final | 중앙 정렬, 단순 입력 2개, icon confirm/cancel, note로 ID dropdown 요구 |
| Worklist | 2-4 | Slide 4 Final | 독립 창, 상단 기간 필터, dual list 성격, `#242424/#000000/#3B3B3B` 표기 |
| Studylist | 5-7 | Slide 7 Final | 독립 창, `<` `>` 이동, PACS, 기간 필터, Worklist/Merge와 목록 통일 |
| Add Patient/Procedure | 8 | Final | 단일 통합 창, required mark, auto-generate, view projection |
| Acquisition | 9-11 | Slide 11 Final | 독립 창, 환자 정보 상단 배치, 대형 viewer surface, control rail |
| Merge | 12-13 | Slide 13 Final | 목록 통일, Sync 명칭, thumbnail + preview 강화 |
| Setting | 14-21 | Slide family Final | 독립 설정 창, 좌측 섹션 네비, 편집 서브화면 포함 |

## 직접 확인한 근거

### Login

- slide 1 screenshot은 단색 배경 위 중앙 로고와 입력 2개, 체크/취소 아이콘 버튼 구성을 보여준다.
- notesSlide1은 로그인 ID를 dropdown list로 변경하라고 지시한다.

### Worklist

- slide 4 title은 `1. Worklist 창 (2안)`이다.
- slide 텍스트에 `Today / 3Days / 1Week / All / 1Month`가 포함되어 있어 상단 기간 필터가 명시된다.
- slide 텍스트에 `Accession No`, `Ref. Physician`, `Exam Date`가 포함되어 있어 컬럼 재구성이 의도되었다.
- embedded screenshot은 Worklist 창 내부에 좌측 환자 목록과 우측 연관 목록이 함께 배치된 독립 창 구조를 보여준다.

### Studylist

- slide 7 title은 `2. Studylist 창 (2안)`이다.
- slide 텍스트에 `<`, `>`, `PACS`, 기간 필터 버튼이 포함된다.
- notes는 Worklist/Studylist/Merge 목록 스타일을 통일하라고 지시한다.

### Add Patient / Procedure

- slide 8 screenshot은 파란 title bar를 가진 독립 입력 창이다.
- notesSlide2는 Add Patient와 Procedure를 하나의 창으로 통합하고, required `(*)`, auto-generate, view projection, manual RIS Code 운용을 요구한다.

### Acquisition

- slide 11 title은 `3. Acquisition 창 (2안)`이다.
- slide 텍스트는 상단 환자 정보 `ID / 이름 / BirthDate / Sex` 배치를 직접 언급한다.
- embedded screenshot은 좌측 control rail, 중앙 black canvas, 우측 tool area가 있는 viewer 중심 창을 보여준다.

### Merge

- slide 13 screenshot은 환자 A/B 비교, 중앙 thumbnail, 우측 preview를 보여준다.
- notesSlide4는 `Same Studylist`를 `Sync`로 바꾸고, 우측 preview를 더 크게, 중앙 thumbnail을 추가하라고 지시한다.

### Setting

- slides 14-21은 모두 독립 Setting 창 계열이다.
- 화면군은 좌측 section navigation, 우측 content area, 하단 action bar 구조를 반복한다.

## 핵심 해석 규칙

1. 화면 단위
   - Worklist, Studylist, Image, Acquisition은 각각 독립 창으로 다룬다.
2. 목록 통일
   - Worklist, Studylist, Merge의 목록 표면은 동일 계열의 visual grammar를 사용한다.
3. viewer 분리
   - Image는 Acquisition 내부 패널이 아니라 별도 창으로 취급한다.
   - 다만 PPT에는 Image 단독 title slide가 없으므로 viewer grammar는 slides 10-11과 Merge preview에서 역추적한다.
4. mockup 폐기
   - HTML/Pencil/Figma 결과물은 활성 설계 근거에서 제외한다.

## 활성 설계에 반영되는 결정

- `MainWindow 3열 셸` 전제 폐기
- `변경 2안` 우선 적용
- 독립 창 구조 채택
- PPT와 notes에 없는 내용은 보수적으로 서술
- 구현 구조는 별도 handoff 문서에서만 참고
