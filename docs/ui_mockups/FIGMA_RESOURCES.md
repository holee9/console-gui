# Figma Community 의료 UI 리소스 목록

## 문서 정보

| 항목 | 내용 |
|------|------|
| 문서 ID | UX-FIG-001 |
| 버전 | 1.0 |
| 작성일 | 2026-04-06 |
| 규격 기준 | SPEC-UI-001 §DT-004 |

---

## 개요

이 문서는 HnVue 의료기기 UI 디자인에 참조할 수 있는 Figma Community 무료 리소스를 정리합니다.

**활용 원칙**:
- 디자인 영감과 패턴 참조 목적 (복사 금지)
- HnVue CoreTokens 색상 팔레트를 기준으로 적용
- Figma 코드 익스포트는 참조용 — WPF XAML로 수동 변환 필요
- 라이선스 확인 필수 (Community Free 또는 CC 라이선스만 사용)

---

## 추천 검색 키워드 (Figma Community 검색창)

```
medical dashboard
healthcare UI kit
hospital design system
radiology interface
clinical workflow
EMR interface
patient monitoring
medical device HMI
dark theme medical
```

---

## 주요 참조 리소스 유형

### 1. Medical Dashboard UI Kits
**용도**: Worklist, Studylist 화면 레이아웃 참조

**참조 패턴**:
- 환자 목록 테이블 구조 (상태 색상 코딩)
- 필터 바 / 검색 영역 배치
- 실시간 상태 표시 위젯

**HnVue 적용**: Worklist (검사 대기 목록), Studylist (검사 이력 목록) 화면

---

### 2. Healthcare/Hospital Dark Theme Kits
**용도**: 전체 다크 모드 색상 팔레트 검증

**참조 패턴**:
- Dark background (#1A1A2E~#0F3460 범위)
- 정보 계층별 텍스트 색상 (Primary/Secondary)
- 상태 표시 색상 (Emergency/Warning/Safe)

**HnVue 적용**: CoreTokens.xaml 색상 검증, 의료 환경 최적화 확인

---

### 3. Medical Device HMI (Human-Machine Interface) Kits
**용도**: Acquisition 화면 컨트롤 레이아웃

**참조 패턴**:
- 비상 정지 버튼 배치 원칙 (IEC 62366 준수)
- 실시간 파라미터 표시
- 알림 우선순위 시각화

**HnVue 적용**: AcquisitionView.xaml 컨트롤 배치, Emergency Stop 스타일

---

### 4. Form / Data Entry UI Kits
**용도**: Add Patient, Settings 화면

**참조 패턴**:
- 폼 필드 레이아웃 (레이블 + 입력)
- 필수 항목 표시
- 오류 메시지 패턴
- 다단계 설정 화면 (사이드바 내비게이션)

**HnVue 적용**: AddPatientView.xaml, SettingsView.xaml

---

## Pencil 무료 스텐실 리소스

### Pencil 기본 스텐실
Pencil 설치 후 기본 제공:
- Generic UI (버튼, 입력, 체크박스 등)
- iOS, Android, Bootstrap 컴포넌트

### 추가 스텐실 다운로드
Pencil 공식 스텐실 저장소: https://pencil.evolus.vn/Stencils.html

**의료기기 관련**:
- 대부분의 의료 특화 스텐실은 없음 → Generic UI 스텐실로 커스터마이징
- 아이콘: Material Design Icons 세트 활용 권장

---

## HnVue 디자인 결정 원칙 (Figma/외부 참조 시)

외부 리소스를 참조할 때 항상 아래 기준으로 적용 가능성 판단:

| 기준 | HnVue 요구사항 | 참조 가능 조건 |
|------|--------------|--------------|
| 색상 | CoreTokens.xaml 기준 | 유사 다크 팔레트인 경우 |
| 레이아웃 | WPF 데스크탑 (1920×1080) | 데스크탑 레이아웃인 경우 |
| 안전 색상 | IEC 62366 (Red=Emergency) | 동일한 색상 의미론인 경우 |
| 터치 타겟 | 최소 44×44px | 충분한 클릭 영역인 경우 |
| 폰트 | Segoe UI (Windows) | 비슷한 산세리프인 경우 |

---

## 라이선스 확인 체크리스트

Figma Community 킷 사용 시:
- [ ] Community Free 라이선스 확인
- [ ] 상업적 사용 허용 여부 확인
- [ ] 의료기기 소프트웨어 적용 제한 없음 확인
- [ ] 저작권 귀속 요건 확인

**권장**: CC0, CC-BY, Community Free 라이선스만 사용

---

버전: 1.0 | 2026-04-06
