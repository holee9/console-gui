# PPT 미적용 화면 요구사항 정리

> 작성일: 2026-04-14
> 작성자: Design Team
> 목적: S07-R4 Task 3 — PPT 미적용 화면 현황 정리

---

## 개요

현재 PPT 디자인 적용 현황: 3/9 화면 완료
- ✅ 완료: LoginView, PatientListView, StudylistView
- ⏳ 미완: 6개 화면 (본 문서에서 정리)

---

## 1. AddPatientProcedureView (슬라이드 8)

### PPT 범위
- 슬라이드 8: "환자 추가/수차 절차" 화면

### 주요 UI 요구사항
1. **입력 필드 그룹**
   - 환자 ID, 성명, 생년월일, 성별
   - 검사 종류, 검사 부위
   - 차트 번호, 임상 정보

2. **버튼 레이아웃**
   - 저장, 취소, 초기화
   - 필수 입력 항목 표시

3. **Validation 피드백**
   - 필수 항목 미입력 시 경고
   - 중복 환자 확인

### 필요한 Design Token
- 입력 필드: TextBox style ( borders, focus state )
- 버튼: Primary, Secondary, Danger variants
- Validation: StatusWarning, StatusEmergency colors
- 라벨: LabelMuted color, 10px/600

### 참조 PPT 섹션
- 슬라이드 8 전체
- Section badge: "환자 추가 창" (슬라이드 3 참조)

---

## 2. WorkflowView (슬라이드 9-11)

### PPT 범위
- 슬라이드 9: 썸네일 스트립 + 이미지 뷰어
- 슬라이드 10: 이미지 조작 도구 모음
- 슬라이드 11: 검색 파라미터 + 노출 버튼

### 주요 UI 요구사항
1. **썸네일 스트립 (슬라이드 9)**
   - 가로 스크롤 썸네일 목록
   - 선택된 이미지 하이라이트
   - 9개 슬롯 표시

2. **이미지 뷰어 (슬라이드 9)**
   - 메인 이미지 표시 영역
   - 줌, 패닝 인터랙션
   - 오버레이 정보 (환자 ID, 검사 일자)

3. **조작 도구 (슬라이드 10)**
   - 밝기/대비 조절 슬라이더
   - 회전, 플립 버튼
   - 윈도우 레벨 프리셋

4. **노출 제어 (슬라이드 11)**
   - 검색 파라미터 입력 (kV, mAs, etc.)
   - 노출 버튼 (비상 정지와 결합)
   - 노출 상태 표시 (Ready, Exposing, Processing)

### 필요한 Design Token
- 썸네일: Component.ImageViewer.* token
- 선택 하이라이트: BorderFocus color
- 오버레이 텍스트: OverlayText color
- 노출 상태: ExposureIndicator.* token (Ready/Exposing/Processing/Error)
- 비상 정지: EmergencyStop.* token (항상 표시)

### 참조 PPT 섹션
- 슬라이드 9-11 전체
- Section badge: "촬영 화면" (슬라이드 3 참조)

---

## 3. MergeView (슬라이드 12-13)

### PPT 범위
- 슬라이드 12: 병합 대상 스터디 목록
- 슬라이드 13: 병합 미리보기 + 확인

### 주요 UI 요구사항
1. **대상 목록 (슬라이드 12)**
   - DataGrid 표시 (2개 스터디 비교)
   - 선택 컬럼 (체크박스)
   - 병합 가능/불가능 표시

2. **미리보기 (슬라이드 13)**
   - 병합 결과 미리보기
   - 충돌 필드 하이라이트
   - 최종 승인/취소 버튼

### 필요한 Design Token
- DataGrid: Component.DataGrid.* token
- 상태 표시: StatusSafe, StatusWarning colors
- 버튼: Button.Primary, Button.Secondary

### 참조 PPT 섹션
- 슬라이드 12-13 전체
- Section badge: "병합 화면" (슬라이드 3 참조)

---

## 4. SettingsView (슬라이드 14-22)

### PPT 범위
- 슬라이드 14-22: 다중 탭 설정 화면

### 주요 UI 요구사항
1. **탭 구조 (슬라이드 14)**
   - 일반, 디스플레이, 네트워크, DICOM, 검사器, 보안, 로그, 정보

2. **일반 설정 (슬라이드 15)**
   - 언어, 테마 선택
   - 자동 저장 간격

3. **디스플레이 설정 (슬라이드 16)**
   - 밝기, 대비
   - 색상 보정

4. **네트워크 설정 (슬라이드 17)**
   - PACS 서버 목록
   - 연결 테스트 버튼

5. **DICOM 설정 (슬라이드 18)**
   - AE Title, Port
   - 커넥션 타임아웃

6. **검사기 설정 (슬라이드 19)**
   - FPD 파라미터
   - 노출 한도

7. **보안 설정 (슬라이드 20)**
   - 세션 타임아웃
   - 비밀번호 정책

8. **로그 (슬라이드 21)**
   - 로그 레벨 필터
   - 로그 내용 표시

9. **정보 (슬라이드 22)**
   - 버전 정보
   - 라이선스 정보

### 필요한 Design Token
- 탭 컨트롤: Navigation.* token
- 입력 필드: TextBox, ComboBox style
- 버튼: Button.Primary, Button.Secondary
- 상태 표시: StatusSafe, StatusWarning

### 참조 PPT 섹션
- 슬라이드 14-22 전체
- Section badge: "설정 화면" (슬라이드 3 참조)

---

## 5. CDBurnView

### PPT 범위
- PPT에 명시된 슬라이드 없음
- 기존 UI 유지 보수

### 주요 UI 요구사항
1. **CD 굽기 진행률 표시**
2. **성공/실패 메시지**
3. **취소 버튼**

### 필요한 Design Token
- 진행률 바: Accent color
- 상태 메시지: StatusSafe, StatusEmergency
- 버튼: Button.Secondary

---

## 6. DoseDisplayView

### PPT 범위
- PPT에 명시된 슬라이드 없음
- IEC 60601-2-54 요구사항 준수

### 주요 UI 요구사항
1. **DRL (Diagnostic Reference Level) 게이지**
   - 0-69%: Safe
   - 70-89%: Warning
   - 90-99%: Blocked
   - 100%+: Emergency

2. **누적 선량 표시**
3. **경고 알림**

### 필요한 Design Token
- DosePanel.* token (이미 정의됨)
- GaugeSafe, GaugeWarning, GaugeBlocked, GaugeEmergency
- LabelText, ValueText

---

## Design Token 공통 요구사항

### 모든 화면에 필요한 Token
1. **Surface 색상**
   - Page, Panel, Card background

2. **Text 색상**
   - Primary, Secondary, Disabled

3. **Border 색상**
   - Default, Focus

4. **상태 색상**
   - Safe, Warning, Blocked, Emergency

5. **인터랙티브 요소**
   - Button (Primary, Secondary, Danger)
   - TextBox, ComboBox, DatePicker
   - DataGrid

---

## 우선순위 제안

1. **P1 (최우선)**: WorkflowView (슬라이드 9-11)
   - 비상 정지 버튼 항상 표시 (IEC 62366)
   - 썸네일 스트립 + 이미지 뷰어 핵심 기능

2. **P2**: AddPatientProcedureView (슬라이드 8)
   - 입력 필드 validation 피드백
   - 필수 항목 표시

3. **P3**: MergeView (슬라이드 12-13)
   - 병합 로직 복잡도로 인해 후순위

4. **P4**: SettingsView (슬라이드 14-22)
   - 다중 탭 구현으로 작업량 많음

5. **P5**: CDBurnView, DoseDisplayView
   - PPT 미정의 화면 (기존 UI 유지)

---

## 다음 라운드 작업 계획

1. **WorkflowView 구현**
   - 썸네일 스트립 XAML
   - 이미지 뷰어 컨트롤
   - 비상 정지 버튼 위치 결정

2. **AddPatientProcedureView 구현**
   - 입력 필드 그룹 레이아웃
   - Validation 피드백 UI

3. **접근성 개선**
   - LoginView, PatientListView에 AutomationProperties 추가
   - TabIndex 논리적 순서 확인

---

## 참조

- PPT 슬라이드 범위: docs/ui_mockups/HnVue UI PPT.pptx
- Design System: docs/ui_mockups/design_system.pen
- Issue #59 (PPT Scope Compliance)
- IEC 62366 (Medical Device UI Usability)
