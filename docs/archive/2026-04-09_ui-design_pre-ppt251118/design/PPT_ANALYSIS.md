# PPT 분석 보고서

**분석 대상:**
1. `★HnVUE UI 변경 최종안_251118.pptx` (22슬라이드, 작성자: Sang Rim Lee, 최종수정: 2025-11-18)
2. `hnvue_abyz_plan.pptx` (3슬라이드, 내재화 계획서, 최종수정: 2026-03-10)

---

## 1. UI 변경 최종안 분석 (`★HnVUE UI 변경 최종안_251118.pptx`)

### 슬라이드 구성 개요

| 슬라이드 | 내용 |
|---------|------|
| 1 | 로그인 창 |
| 2~4 | 1. Worklist 창 (현재 / 1안 / 2안) |
| 5~7 | 2. Studylist 창 (현재 / 1안 / 2안) |
| 8 | 3. Add Patient/Procedure 창 |
| 9~11 | 4. Acquisition 창 (현재 / 1안 / 2안) |
| 12~13 | 4. Merge 창 (현재 / 1안) |
| 14 | Setting 창 - 크기/레이아웃 문제 |
| 15 | Setting 창 - System / Account 섹션 |
| 16 | Setting 창 - Detector / Generator 섹션 |
| 17 | Setting 창 - Network / Dicom Print 섹션 |
| 18 | Setting 창 - Display / Option / Database 섹션 |
| 19 | Setting 창 - RIS Code / DicomSet / Procedure 섹션 |
| 20 | Setting 창 - RIS Code Edit 화면 |
| 21 | Setting 창 - Procedure Step Edit 화면 |
| 22 | (기타/종합) |

---

## 2. 화면별 UI 변경 명세

### 2.1 로그인 창 (슬라이드 1)

**변경 사항:**
- ID 입력 방식을 **드롭다운 리스트 형태**로 변경
  - 기존: 직접 텍스트 입력
  - 변경: 등록된 사용자 ID를 드롭다운으로 선택

**보존 사항:**
- 로그인 창의 기본 레이아웃 구조

---

### 2.2 Worklist 창 (슬라이드 2~4)

#### 현재 상태 (슬라이드 2)
- 기존 UI 스크린샷 참조

#### 1안 (슬라이드 3)
- ExamDate 컬럼 위치 변경/추가
- 레이아웃 재배치 위주

#### 2안 (슬라이드 4) - **최종안으로 추정**

**변경 사항:**
- **필터 버튼 추가:** Today / 3Days / 1Week / All / 1Month 기간 필터 버튼 상단 배치
- **컬럼 재배치:** Accession No, Ref. Physician, Exam Date 컬럼 추가 또는 재배치
- **색상 변경 (슬라이드 내 명시):**
  - `#242424` - 배경색 (다크 테마 유지)
  - `#000000` - 텍스트/경계선
  - `#3B3B3B` - 보조 배경 또는 행 색상

**Worklist/Studylist 공통 원칙:**
- Worklist 창 목록 = Studylist 창 목록 = Merge 창 목록 **통일감 유지**

---

### 2.3 Studylist 창 (슬라이드 5~7)

#### 현재 상태 (슬라이드 5)
- 기존 UI 스크린샷 참조

#### 1안 (슬라이드 6)
- 레이아웃 재배치 위주

#### 2안 (슬라이드 7) - **최종안으로 추정**

**변경 사항:**
- **이전/다음 내비게이션 버튼** (`<` / `>`) 추가
- **PACS 서버 선택** 드롭다운 또는 표시 영역 추가
- **기간 필터 버튼 추가:** Today / 3Days / 1Week / All / 1Month
- **컬럼:** Accession No 포함

---

### 2.4 Add Patient/Procedure 창 (슬라이드 8)

**변경 사항 (노트 참조):**

1. **창 통합:** Add Patient 창과 Procedure 창을 **하나의 창으로 통합**
2. **필수 항목 표시:** 필수 입력 필드에 `(*)` 마크 표시
3. **Acc No / Patient ID 자동생성 옵션:**
   - Auto-Generate on/off 버튼 추가 (슬라이드에 `Auto-Generate` 2개 표시됨)
4. **View Projection 추가:** 체위 선택 기능 포함
   - 예: "Chest Series × / Hand Series ×" 형태로 선택된 항목 표시 (삭제 버튼 포함)
5. **Description 탭:**
   - Drop-down 선택 방식과 수동 작성 방식 병용
   - RIS Code 매핑 리스트와 호환
   - 멀티 추가 기능: 하단에 말풍선(bubble) 형태로 Description 리스트 나열
6. **Study Group 기능 삭제**
7. **RIS Code 실시간 추가/업데이트 기능 탑재**
8. **MWL 서버 없이 Manual RIS Code 생성 시:** HnVUE에서 Code ID 자동 부여
9. **개발 순서 명시:**
   - (1) 수동 작성 시: 하단 Procedure list 맨 우측 공란 → 수동 매핑 후 촬영 창 진입
   - (2) Drop-down 선택 시: Procedure list에 RIS Code 매핑된 리스트 자동 연계 + 수동 뷰 추가/삭제/RIS Code update 기능
   - (3) Description 멀티 추가 기능

---

### 2.5 Acquisition 창 (슬라이드 9~11)

#### 현재 상태 (슬라이드 9)
- 기존 UI 스크린샷 참조

#### 1안 (슬라이드 10)

**변경 사항:**
- 환자 정보 표시 영역 상단에 명확히 배치:
  - `ID: 20251013002`
  - `홍길동`
  - `1988-01-01  M`
- 헤더 영역에 환자 정보 표시 형식 변경

#### 2안 (슬라이드 11)

**변경 사항:**
- 환자 정보 표시 위치 재배치 (1안 대비 위치 조정)
- 상단에 ID/이름/생년월일/성별 정보 표시
- 더 간결한 헤더 디자인

---

### 2.6 Merge 창 (슬라이드 12~13)

#### 현재 상태 (슬라이드 12)
- 기존 UI 스크린샷 참조

#### 1안 (슬라이드 13) - **변경 사항**

1. **레이아웃 변경:**
   - 좌측: Patient A (ID or Name) 검색/목록
   - 우측: Patient B (ID or Name) 검색/목록
   - 중앙: Preview 영상 표시 영역 (2개)
2. **"Same Studylist" → "Sync Study"로 명칭 변경**
3. **Preview 기능 강화:**
   - Merge 대상 영상 클릭 시 우측에 크게 Preview 영상 display
   - Merge 시 중앙 섹션 Thumbnail에 추가됨
4. **통일감:** Worklist/Studylist 창 목록과 동일한 스타일 유지

---

### 2.7 Setting 창 (슬라이드 14~21)

#### 전체 Setting 창 공통 변경 (슬라이드 14)

**버그 수정 및 레이아웃 변경:**
- **창 크기 up 후 언어 변경 시 비율 안 맞는 문제 해결** (특히 스페인어)
- **메뉴창을 설정 창 상단으로 이동** (기존 위치에서 상단으로 재배치)

#### Setting 창 내부 구조 재편 (슬라이드 15~21)

**Setting > System (슬라이드 15)**
- Priority 기능 추가
- ID 관리
- Privilege Level 설정
- 메인 설정 / 무결성 관련 항목 위주 재배치
- Login Popup 대신 **Access Notice**로 명칭 변경

**Setting > Account (슬라이드 15)**
- Operator 항목 **삭제**
- 권한/강도 설정 모두 **Dropdown 형태로 변경**
- Priority 기능 추가: ID 추가될 때마다 Priority 리스트에 자동 등록
- 로그인 창에서 아이디 Drop-down 순서 배정과 연동

**Setting > Detector (슬라이드 16)**
- **현행 구조 유지**
- Blue 디텍터 선택 시에만 우측 맵파일 설정 화살표 아이콘 나타나기 (조건부 표시)

**Setting > Generator (슬라이드 16)**
- **현행 구조 유지**
- Port No 필요 모델의 경우, 작성 칸 나타나기 (조건부 표시)

**Setting > Network (슬라이드 17)**
- **PACS, Worklist, Print 모두 Network 탭으로 취합** (3개 → 1개)
- 각 섹션별 Add / Edit / Delete 버튼 생성
  - Add 클릭 시 Edit도 가능하도록 설계
  - 추후 확장성: Server별 송/수신 우선순위 설정
- Print Server: Add/Edit 클릭 시 Print 설정 Preference 옵션 창 함께 생성
- **Clear** 기능 포함
- **Dicom Print** 설정 포함

**Setting > Display (슬라이드 18)**
- Marker, Annotation, Overlay 등 메인 촬영 화면 Display 관련 설정 통합
- 추후 Marker size 변경 기능 추가 예정
- 공간 협소 시 Display / Overlay로 탭 구분 검토

**Setting > Option (슬라이드 18)**
- Image 저장 용량, Database 관련 설정
- Stitching save ratio도 이미지 용량과 연관하여 포함
- Delete 옵션 구분:
  - 수동 전체 삭제
  - 자동 삭제 옵션 → Backup 섹션과 통일감 조성
- **Import** 기능 포함
- **Delete Option / Storage Reset** 구분

**Setting > Database (슬라이드 18)**
- 용량/정리 관련 설정 포함

**Setting > DicomSet (슬라이드 19)**
- Output DICOM tag에 영향을 주는 인자 관리
- 추후 부서명, 병원주소, Ref Physician 등 현장 필요 tag 값 추가 예정

**Setting > RIS Code (슬라이드 19)**
- 전체적인 아이콘 위치 재배치
- RIS Code / Description 매칭 설정 → **RIS Code Edit** 화면으로 이동 배치
- "Add Auto Generate ID" 클릭 시 추가되던 Procedure list 칸 → 하위 배치로 변경 (기존: Option 창)

**Setting > RIS Code Edit (슬라이드 20)**
- "Only No matching" 명칭/배치 → **"Un-Matched"**로 변경
- **Matching / Un-Matched** 탭 구분
- **Code (0020:0010)** 태그 표시
- Code/Description 매칭 옵션을 Load Worklist 옆으로 이동
- Code/Description 드롭다운 선택 후 Load Worklist 연계
- 추후 다른 tag 값 RIS Code 사용 가능성 고려 → Drop-down 확장성, Tag attribution (Tag address) 형태로 기입

**ProcedureStepName 섹션 개선:**
- 하단 Add, Delete 제거 → 맨 우측 Position 섹션 하단 Edit 아이콘 생성 → Procedure Edit 창으로 진입
- 현재 문제: Add/Delete가 통일성 없이 동작 중 (Add: Procedure Edit 창 진입 | Delete: 단순 리스트 삭제)
- 개선: ProcedureStepName 삭제 시 해당 리스트 더블클릭 → 우측 Position 창으로 이동하는 방식

**Setting > Procedure Step Edit (슬라이드 21)**
- 동물용 UI 개선 요청 창과 유사한 구조로 변경
- **Un-Matched / Matching** 탭 구분
- **Code (0020:0010)** 태그 표시
- 지난 동물용 UI 구조 개선과 유사한 구조/배치 유지

---

## 3. 보존해야 할 현행 UI 요소

슬라이드 및 노트 내용을 기반으로 **현행 구조 유지**가 명시된 항목:

| 화면 | 보존 항목 |
|------|---------|
| Setting > Detector | 현행 구조 유지 전반 |
| Setting > Generator | 현행 구조 유지 전반 |
| 다크 테마 색상 | #242424, #000000, #3B3B3B 계열 유지 |
| 전반적 창 구조 | Worklist/Studylist/Merge 목록 스타일 통일 |
| Procedure/RIS Code 기본 개념 | 구조 개편 있으나 기본 매핑 개념 유지 |

---

## 4. 핵심 디자인 결정 및 근거

### 4.1 통일감 (Consistency)
- **Worklist 목록 = Studylist 목록 = Merge 창 목록** 동일한 스타일
- Setting 창 내부 Add/Edit/Delete 버튼 패턴 통일

### 4.2 Network 탭 통합
- 기존 PACS, Worklist, Print 3개 별도 탭 → 1개 Network 탭으로 통합
- 근거: 관련 기능의 집약으로 사용 편의성 향상
- 여유 공간으로 Display/Overlay 탭 분리 검토 가능

### 4.3 Add Patient/Procedure 창 통합
- 기존 별도 창 → 하나의 창으로 통합
- 근거: 워크플로우 단축, 필수 항목 명시로 입력 오류 감소

### 4.4 조건부 UI 표시
- Detector: Blue 디텍터 선택 시에만 맵파일 설정 아이콘 표시
- Generator: Port No 필요 모델에서만 입력 칸 표시
- 근거: 관련 없는 옵션을 숨겨 UI 복잡도 감소

### 4.5 드롭다운 전환
- 로그인 ID, Account 권한/강도 설정 모두 드롭다운 방식 채택
- 근거: 입력 오류 감소, 일관성 향상

### 4.6 명칭 표준화
- "Same Studylist" → "Sync Study"
- "Login Popup" → "Access Notice"
- "Only No matching" → "Un-Matched"

---

## 5. 색상/크기 명세

### 슬라이드 4에서 명시된 색상값

| 색상 코드 | 용도 추정 |
|-----------|---------|
| `#242424` | 주 배경색 (다크) |
| `#000000` | 텍스트/경계 |
| `#3B3B3B` | 보조 배경 (행 색상 등) |

> 참고: 슬라이드 내 하이라이트 표시용으로 노란색(#FFFF00)과 빨간색(#FF0000)이 사용되었으나, 이는 PPT 편집용 마킹 색상이며 실제 UI 색상이 아님.

### PPTX 내 이미지 수
- pptx1: 67개 PNG + 5개 WDP(HD Photo) + 1개 EMF = 73개 이미지 포함
- 각 슬라이드는 현재 화면 스크린샷과 제안 화면 비교 방식으로 구성됨

---

## 6. 내재화 계획서 분석 (`hnvue_abyz_plan.pptx`)

### 슬라이드 1: 휴이넘 Console 구조 분석
- HnVUE Console 아키텍처 다이어그램 (이미지 중심)
- 내재화를 위한 시스템 구조 이해 목적

### 슬라이드 2: 휴이넘 Console 구성요소 (DLL 목록)

**오픈소스 DLL (무료 사용 가능):**

| DLL | 용도 | 라이선스 |
|-----|------|---------|
| BCrypt.Net.dll | Bcrypt 암호화 | MIT |
| BouncyCastle.Cryptography.dll | RSA, AES, ECC 암호화 | MIT/Apache |
| ClosedXML.dll | Excel 처리 | MIT |
| CommunityToolkit.HighPerformance.dll | Span/Memory 최적화 | MIT |
| Dapper.dll | SQL 매핑 | Apache 2.0 |
| DocumentFormat.OpenXml.dll | Word/Excel/PPT 조작 | 무료 |
| fo-dicom.core.dll | DICOM 처리 | MIT |
| libcurl.dll | 네트워크 통신(http/ftp) | MIT-like |
| LiveCharts.dll / LiveCharts.Wpf.dll | 데이터 시각화 차트 | 무료 |
| Newtonsoft.Json.dll | JSON 포맷 처리 | MIT |
| OpenCvSharp.dll / opencv_world452.dll | 영상 처리 | BSD |
| Serilog.dll / log4net.dll | 로그 기록 | Apache 2.0 |
| PdfSharp.dll | PDF 작성/편집 | MIT |
| itextsharp.dll | PDF 작성/변환 | AGPL + 상업 |
| SixLabors.Fonts.dll | 텍스트 렌더링/폰트 | MIT |
| SQLitePCLRaw.dll | DB 연동 | MIT |

**상업용/불명 DLL (내재화 시 주의):**

| DLL | 용도 | 비고 |
|-----|------|------|
| HnVUE.dll | HnVUE main DLL | 휴이넘 상업용 |
| ImageProcess.dll | 영상 처리 모듈 | 휴이넘 상업용 |
| ImageViewer.dll | 영상 뷰어 모듈 | 휴이넘 상업용 |
| CPIGenerator.dll | CPI사 장비 모듈 | CPI 상업용 |
| ECOTRON.dll | ECOTRON사 장비 모듈 | ECOTRON 상업용 |
| Synics.dll / SynicsraySGX400_x64.dll | Synicsray사 장비 모듈 | 상업용 |
| Rockey4SClass.dll / Ry4S_x64.dll | 락키 라이선스 관련 | 상업용 |
| Lfbmpu.dll / Lfcmpu.dll / Lftifu.dll | 이미지 코덱 | LeadTools (확인 필요) |
| CDRAST.dll | DICOM 처리 | 출처 불명 |
| DbHandler.dll | DB 래퍼 | 출처 불명 |
| Dicom.Core.dll / DicomCore.dll / DICOMSCU.dll | DICOM 처리 | 출처 불명 |
| HARDIMP.dll | 용도 추정 불가 | 출처 불명 |

### 슬라이드 3: 주요 일정 (2026년 내재화 로드맵)

| 단계 | 기간 |
|------|------|
| 선행 분석 (↳ 유관부서 협의) | 2026년 1Q (1월~2월) |
| 기능 개발 | 2026년 2Q~4Q (3월~10월) |
| 기능 평가 | 2026년 3Q~4Q (7월~12월) |
| 중간보고 | 2026년 5월 30일 |

**주요 사항:**
- CyberSecurity 등 인증 계획 수립 필요
- 3Q부터 개발과 평가 병행 진행

---

## 7. 개발 우선순위 요약

PPT 내용을 토대로 파악한 UI 변경 작업의 우선순위:

### 높은 우선순위 (기능적 변경)
1. Add Patient/Procedure 창 통합 (슬라이드 8, 노트 상세 명세 있음)
2. RIS Code Edit / Procedure Step Edit 화면 개선 (슬라이드 20, 21)
3. Setting > Network 통합 (PACS+Worklist+Print → Network)
4. Worklist/Studylist 기간 필터 버튼 추가
5. 로그인 ID 드롭다운 변경

### 중간 우선순위 (레이아웃 변경)
1. Setting 창 크기 비율 버그 수정 (스페인어 등 다국어)
2. Setting 창 메뉴를 상단으로 이동
3. Merge 창 레이아웃 개선 (Preview 강화)
4. Account 설정 Dropdown 전환

### 낮은 우선순위 (명칭/표시 변경)
1. "Same Studylist" → "Sync Study" 명칭 변경
2. "Login Popup" → "Access Notice" 명칭 변경
3. "Only No matching" → "Un-Matched" 명칭 변경
4. Detector/Generator 조건부 UI 표시

---

*분석 일자: 2026-04-06*
*원본 파일: `docs/★HnVUE UI 변경 최종안_251118.pptx`, `docs/hnvue_abyz_plan.pptx`*
