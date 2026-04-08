# Worklist / Studylist 페이지 분석 보고서

**문서 버전:** v1.0  
**작성일:** 2026-04-07  
**참조 소스:** ★HnVUE UI 변경 최종안_251118.pptx (슬라이드 4~6), slide4.xml / slide5.xml / slide6.xml  
**분석 대상 모듈:** `PatientListView.xaml`, `StudylistView.xaml`, `PatientListViewModel.cs`, `StudylistViewModel.cs`

---

## 1. PPT 디자인 명세 요약

### 1.1 슬라이드 4 — Worklist 현재 디자인 (참조 이미지)
- 현재 상태의 Worklist 스크린샷 참조용
- 제목: "1. Worklist 이전(현재)"

### 1.2 슬라이드 5 — Worklist 구현 코드 (HTML+CSS)
PPT에 AI 에이전트용 HTML+CSS 코드로 완전한 UI 명세 포함.  
SOURCE: `Slide 2 - HnVUE Worklist 이전(현재)`

#### 전체 레이아웃 구조
```
body (flex-column, height:100vh, bg:#1a2540)
├── AppHeader (height:30px, bg:#0d1b36)
│   ├── Logo "HnVUE"
│   ├── AppMenu [Worklist(active), Study, Report, Teaching File, Admin, Statistics]
│   └── HeaderRight [날짜, 사용자, 로그아웃]
└── main-layout (flex-row, flex:1)
    ├── worklist-area (flex:1, border-right:2px #2e4070)
    │   ├── WorklistToolbar (height:32px, bg:#152035)
    │   ├── FilterBar (bg:#1a2a44)
    │   └── table-wrapper (flex:1, overflow:auto)
    │       └── WorklistTable
    └── detail-panel (width:230px, bg:#101a2e)
        ├── DetailHeader "Study 상세"
        ├── PatientSearch input
        ├── InfoSection [환자 정보]
        ├── InfoSection [Study 정보]
        ├── ToggleRow [Suspend, Hide]
        ├── ViewerPreview (flex:1, bg:#000)
        └── ActionButtons
```

#### Worklist 테이블 컬럼 (PPT 명세)
| 컬럼 | 정렬 | 특이사항 |
|------|------|---------|
| checkbox | center | 전체선택 체크박스 |
| # | - | 순번 |
| 환자ID (sortable) | left | 파란색 강조 (#7bc8f5) |
| 환자이름 (sortable) | left | 밝은 흰색 |
| 연령/성별 (sortable) | left | "65M", "42M" 형식 |
| Modality | center | 색상 뱃지 (CT/MR/XR/US/NM) |
| StudyDate (sortable) | left | yyyy-MM-dd |
| StudyDescription | left | - |
| BodyPart | left | CHEST/BRAIN/ABDOMEN 등 |
| Priority (sortable) | left | 긴급(빨강)/보통 |
| Status | left | 컬러 dot + 텍스트 |
| 담당자 | left | - |

#### 툴바 버튼 (PPT 명세)
- **새로등록** (Primary blue)
- **현재촬영**
- **판독완료**
- **삭제**
- Total 카운트 레이블 (오른쪽 정렬)

#### 필터 바 (PPT 명세)
- 기간 (날짜 From/To)
- Modality 드롭다운 (전체/CT/MR/XR)
- 상태 드롭다운 (전체/Reported/Pending/In Progress)
- 환자명/ID 검색 + 검색 버튼

#### 우측 Detail Panel (PPT 명세)
| 구성요소 | 상세 |
|---------|------|
| 환자검색 input | "Input Patient Name..." placeholder |
| 환자 정보 섹션 | 환자ID, 환자이름, 연령/성별, 생년월일 |
| Study 정보 섹션 | Modality, Body Part, Study Date, Images 수, Priority, Status |
| Toggle | Suspend, Hide 체크박스 |
| 뷰어 미리보기 | 검정 배경, DICOM 오버레이 정보 |
| 액션 버튼 | 뷰어 열기(full-width), 보고서, 이전 검색, 복사, 보관, 삭제 |

### 1.3 슬라이드 6 — 디자인 토큰 명세
완전한 CSS 변수 토큰 세트 포함:
- 배경색: `#0d1527` ~ `#1a2540` (5단계 다크 네이비)
- 경계선: `#2e4070` (기본), `#2a3a58` (테이블), `#1e2e48` (행)
- Primary: `#1f6fc7` / hover: `#2a88e8`
- Modality 뱃지: CT(#c0392b), MR(#2980b9), XR(#27ae60), US(#8e44ad), NM(#d35400)
- Status dot: Reported(#2ecc71), Pending(#f39c12), InProgress(#3498db), Urgent(#e74c3c)
- 행 상태: Urgent(적색 왼쪽 테두리), Normal(녹색), Waiting(황색)
- 폰트: 'Malgun Gothic', 9~15px 범위

---

## 2. 현재 구현 분석

### 2.1 PatientListView.xaml — Worklist 매핑

**파일 경로:** `src/HnVue.UI/Views/PatientListView.xaml`  
**코멘트:** `@MX:NOTE PatientListView — full-width Worklist DataGrid (PPT Slide 4 spec)`

#### 헤더/툴바 영역

| PPT 명세 요소 | 구현 여부 | 구현 방식 | 비고 |
|--------------|---------|---------|------|
| 섹션 뱃지 "Worklist" | ✅ 구현 완료 | `HnVue.Component.SectionBadge.Bg` | |
| 환자 목록 타이틀 | ✅ 구현 완료 | "환자 목록" TextBlock | |
| 카운트 뱃지 | ✅ 구현 완료 | `Patients.Count` 원형 뱃지 | |
| 검색박스 | ✅ 구현 완료 | 돋보기 아이콘 + TextBox | "환자 검색..." placeholder |
| 기간 필터 버튼 | ✅ 구현 완료 | 전체/오늘/3일/1주일/1개월 | PPT 명세 일치 |
| 새로등록 버튼 | ✅ 구현 완료 | Primary 스타일 + 아이콘 | RegisterPatientCommand |
| 현재촬영 버튼 | ⚠️ 부분 구현 | IsEnabled=False | 기능 미연결 |
| 판독완료 버튼 | ⚠️ 부분 구현 | IsEnabled=False | 기능 미연결 |
| 삭제 버튼 | ⚠️ 부분 구현 | IsEnabled=False | 기능 미연결 |
| Total 카운트 레이블 | ✅ 구현 완료 | `Patients.Count` 바인딩 | |

#### DataGrid 컬럼 매핑

| PPT 컬럼 | 구현 컬럼 | 매핑 여부 | 비고 |
|---------|---------|---------|------|
| checkbox | ✅ DataGridTemplateColumn | 매핑 완료 | IsSelected 기반 |
| # (순번) | ❌ 없음 | 미구현 | |
| 환자ID | ✅ "환자ID" → PatientId | 매핑 완료 | SemiBold 강조 |
| 환자이름 | ✅ "환자이름" → Name | 매핑 완료 | |
| 연령/성별 | ⚠️ 성별만 → Sex | 부분 매핑 | 나이 컬럼 없음 |
| 생년월일 | ✅ "생년월일" → DateOfBirth | 구현 (PPT 없음) | 추가 컬럼 |
| 등록일시 | ✅ "등록일시" → CreatedAt | 구현 (PPT없음) | StudyDate 대체 |
| Modality | ❌ 없음 | **미구현** | CT/MR/XR 뱃지 없음 |
| StudyDate | ❌ 없음 | **미구현** | 등록일시로 대체 |
| StudyDescription | ❌ 없음 | **미구현** | |
| BodyPart | ❌ 없음 | **미구현** | |
| Priority | ❌ 없음 | **미구현** | 긴급/보통 구분 없음 |
| Status (dot) | ⚠️ 부분 구현 | IsEmergency → 긴급/대기 | Status dot 미구현 |
| 담당자 | ✅ "담당자" → CreatedBy | 매핑 완료 | |

#### 우측 Detail Panel

| PPT 명세 요소 | 구현 여부 | 비고 |
|--------------|---------|------|
| 230px Detail Panel 전체 | ❌ **미구현** | PatientListView에 없음 |
| Study 상세 헤더 | ❌ 미구현 | |
| 환자 검색 입력 | ❌ 미구현 | |
| 환자 정보 섹션 | ❌ 미구현 | |
| Study 정보 섹션 | ❌ 미구현 | |
| Suspend/Hide 토글 | ❌ 미구현 | |
| 뷰어 미리보기 | ❌ 미구현 | |
| 액션 버튼 그룹 | ❌ 미구현 | |

### 2.2 StudylistView.xaml — Study 탭 매핑

**파일 경로:** `src/HnVue.UI/Views/StudylistView.xaml`  
**코멘트:** `@MX:NOTE StudylistView — PPT 슬라이드 7 (Studylist 2안)`

> PPT 슬라이드 7은 PPT 파일의 별도 슬라이드이며, StudyList를 독립 뷰로 구현.  
> PPT Worklist의 "Study" 탭 클릭 시 이동하는 페이지에 해당.

| PPT 요소 | 구현 여부 | 구현 방식 | 비고 |
|---------|---------|---------|------|
| Prev/Next 네비게이션 | ✅ 구현 완료 | ChevronLeft/Right 버튼 | NavigatePrevious/NextCommand |
| Section 타이틀 | ✅ 구현 완료 | "Study List" | |
| PACS 서버 드롭다운 | ✅ 구현 완료 | ComboBox PacsServers | |
| 기간 필터 버튼 | ✅ 구현 완료 | Today/3Days/1Week/All/1Month | |
| 검색 박스 | ✅ 구현 완료 | 돋보기 + TextBox | "Search studies..." |
| 환자ID 컬럼 | ✅ 구현 완료 | "Patient ID" → PatientId | Width=130 |
| 접수번호 컬럼 | ✅ 구현 완료 | "Acc No" → AccessionNumber | |
| 검사일자 컬럼 | ✅ 구현 완료 | "Exam Date" → StudyDate | yyyy-MM-dd |
| Body Part 컬럼 | ✅ 구현 완료 | "Body Part" → BodyPart | |
| Description 컬럼 | ✅ 구현 완료 | "Description" → Description | |
| Modality 컬럼 | ❌ 미구현 | 없음 | CT/MR 뱃지 없음 |
| Priority 컬럼 | ❌ 미구현 | 없음 | |
| Status 컬럼 | ❌ 미구현 | 없음 | |
| 담당자 컬럼 | ❌ 미구현 | 없음 | |
| 스터디 카운트 상태바 | ✅ 구현 완료 | `Studies.Count studies` | |
| 오류 메시지 표시 | ✅ 구현 완료 | Emergency 색상 TextBlock | |

---

## 3. 갭(Gap) 분석 요약

### 3.1 전체 구현율

| 페이지 | PPT 명세 항목 수 | 구현 완료 | 부분 구현 | 미구현 | 구현율 |
|--------|---------------|---------|---------|-------|-------|
| PatientListView (Worklist) | 32 | 14 | 5 | 13 | ~44% |
| StudylistView | 16 | 10 | 0 | 6 | ~63% |

### 3.2 PatientListView 미구현 항목 (우선순위)

#### 높음 (기능 영향)
| 항목 | 설명 | 영향 |
|------|------|------|
| 우측 Detail Panel (230px) | Study 상세 정보 + 뷰어 미리보기 + 액션 버튼 | PACS 워크플로우 핵심 |
| Modality 컬럼 + 색상 뱃지 | CT/MR/XR/US/NM 뱃지 | 임상 정보 표시 |
| Status 컬러 dot | Reported/Pending/InProgress/Urgent | 검사 상태 가시화 |
| 행 왼쪽 상태 테두리 | 긴급(빨강)/보통(녹색)/대기(황색) | 우선순위 시각화 |
| Priority 컬럼 | 긴급/보통 구분 | 임상 우선순위 |

#### 중간 (UX 개선)
| 항목 | 설명 |
|------|------|
| 필터바 Modality/Status 드롭다운 | 현재 기간 필터만 있음 |
| 현재촬영/판독완료/삭제 버튼 활성화 | IsEnabled=False 상태 |
| 순번(#) 컬럼 | PPT 명세에 있으나 미구현 |
| 연령 표시 | Sex만 있고 나이 없음 |

#### 낮음 (UI 개선)
| 항목 | 설명 |
|------|------|
| 행 hover 배경 애니메이션 | transition: 0.1s |
| 스크롤바 커스텀 스타일 | PPT: 5px, #2e4070 |
| Total 레이블 색상 | PPT: #7bc8f5, 현재: Primary 색상 |

### 3.3 StudylistView 미구현 항목

| 항목 | 우선순위 | 설명 |
|------|---------|------|
| Modality 컬럼 | 높음 | CT/MR/XR 색상 뱃지 |
| Status 컬럼 | 높음 | 검사 상태 dot |
| Priority 컬럼 | 중간 | 긴급/보통 |
| 담당자 컬럼 | 낮음 | Radiologist 정보 |

---

## 4. 아키텍처 연계성 분석

### 4.1 PPT 설계 의도 vs 현재 구현 차이

PPT는 **PACS 통합 워크스테이션** 관점에서 설계:
- 좌측: Worklist (환자/검사 목록)
- 우측: Study 상세 + DICOM 뷰어 미리보기 + 액션 버튼

현재 구현은 **FPD X-ray 촬영 시스템** 관점:
- PatientListView: 환자 등록 및 조회 (Worklist 역할)
- StudylistView: 선택된 환자의 Study 목록 (별도 뷰)
- ImageViewerView: DICOM 뷰어 (별도 뷰)

**결론:** 기능은 분산되어 있으나 PPT의 통합 레이아웃(분할 패널)으로의 재구성이 필요.

### 4.2 데이터 모델 매핑

| PPT 데이터 필드 | 현재 모델 | 매핑 상태 |
|---------------|---------|---------|
| 환자ID | `PatientRecord.PatientId` | ✅ 완료 |
| 환자이름 | `PatientRecord.Name` | ✅ 완료 |
| 연령/성별 | `PatientRecord.Sex` | ⚠️ 나이 없음 |
| 생년월일 | `PatientRecord.DateOfBirth` | ✅ 완료 |
| Modality | 없음 | ❌ 모델 없음 |
| StudyDate | `StudyRecord.StudyDate` | ✅ StudylistView |
| BodyPart | `StudyRecord.BodyPart` | ✅ StudylistView |
| Priority/IsEmergency | `PatientRecord.IsEmergency` | ⚠️ 단순 bool |
| Status (Reported 등) | 없음 | ❌ 모델 없음 |
| AccessionNumber | `StudyRecord.AccessionNumber` | ✅ StudylistView |
| StudyDescription | `StudyRecord.Description` | ✅ StudylistView |

---

## 5. 개선 로드맵 제안

### Phase 1 — 데이터 모델 확장 (기반 작업)
- `PatientRecord`에 `Age` (계산 속성) 추가
- `StudyRecord`에 `Modality`, `Status`, `Priority`, `AssignedTo` 추가
- Status Enum: `Reported`, `Pending`, `InProgress`, `Urgent`

### Phase 2 — PatientListView 컬럼 확장
- Modality 뱃지 컬럼 추가
- Status dot 컬럼 추가 (DataGridTemplateColumn)
- 행 왼쪽 상태 테두리 (DataTrigger 기반)
- 현재촬영/판독완료/삭제 버튼 기능 연결

### Phase 3 — Detail Panel 구현 (대규모)
- `PatientListView`에 우측 230px Detail Panel 추가
- Study 상세 정보 섹션 구현
- PatientInfoCard 컴포넌트 재사용
- 뷰어 미리보기 (AcquisitionPreview 컴포넌트 참조)
- 액션 버튼 그룹 구현

### Phase 4 — 필터 기능 확장
- Modality 드롭다운 필터 추가
- Status 드롭다운 필터 추가
- 날짜 범위 From/To 필터 추가

---

## 6. 결론

| 페이지 | 현황 | 핵심 갭 |
|--------|------|---------|
| **LoginView** | PPT 95% 구현 | 사용자 목록 동적 로딩만 필요 |
| **PatientListView (Worklist)** | PPT 44% 구현 | 우측 Detail Panel, Modality/Status 컬럼 미구현 |
| **StudylistView** | PPT 63% 구현 | Modality/Status/Priority 컬럼 미구현 |

**전체 우선순위 1순위**: 우측 Detail Panel 구현 (PatientListView에 Study 상세 + 액션 버튼)  
**전체 우선순위 2순위**: Modality 뱃지 컬럼 + Status dot 컬럼 (데이터 모델 확장 전제)
