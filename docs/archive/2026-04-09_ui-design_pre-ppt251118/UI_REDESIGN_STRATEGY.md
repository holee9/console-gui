# HnVue UI Redesign Strategy - 정책 보고서

## 📊 Executive Summary

**분석 결과**: HnVue는 이미 훌률한 모듈러 아키텍처를 가지고 있음  
**디자인 시스템**: DesignSystem2026.xaml이 이미 완벽하게 구현됨  
**필요한 것**: Pencil/Figma 기반 시각 디자인 + WPF XAML 프로토타입

---

## 🔍 깊은 아키텍처 분석 (Sequential Thinking)

### 1. 모듈 독립성 검증

**데이터 흐름 추적**:
```
사용자 입력 → WPF View → ViewModel → Interface → Business Module
```

**검증 포인트**:
1. `HnVue.UI.Contracts` - 순수 인터페이스 정의 (구현체 없음)
2. `HnVue.UI.csproj` - 다른 비즈니스 모듈에 대한 직접 참조 없음
3. MahApps.Metro + CommunityToolkit.Mvvm - 이미 최신 WPF 프레임워크 사용

**결론**: UI가 비즈니스 로직과 완전히 분리됨 → **독립적 UI 업데이트 가능**

### 2. 기존 디자인 시스템 확인

**DesignSystem2026.xaml 분석**:
- 컬러 팔레트: #1B4F8A (MahApps.Metro Blue) + 다크 모드 테마
- 타이포그래피: Segoe UI + Malgun Gothic (한국어 지원)
- 스페이싱: 4px 기반 시스템 (XS~XXXL)
- 모서리 반경: 4~16px (의료기기 최적화)

**MahApps.Metro 통합 확인**:
- 이미 사용 중인 최신 WPF UI 프레임워크
- Material Design 영감의 모던 컨트롤 제공
- 다크 모드 네이티브 지원

### 3. 기존 컴포넌트 라이브러리

**이미 구현된 컴포넌트**:
- Modal.xaml (다이얼로그)
- Toast.xaml (알림)
- AcquisitionPreview.xaml (영상 획듡 미리보기 - CRITICAL)
- PatientInfoCard.xaml (환자 정보)
- StudyThumbnail.xaml (검사 썸네일)

**스타일 시스템**:
- ButtonStyles.xaml (Primary, Secondary, Danger)
- InputStyles.xaml (입력, 검색)
- CardStyles.xaml (카드 컨테이너)

---

## 🎨 올바른 디자인 접근 방법

### Pencil을 활용한 시각 디자인 워크플로우

#### 1단계: Pencil 다운로드 및 설치
```
공식 웹사이트: https://pencil.evolus.vn/
무료 오픈소스 GUI 프로토타이핑 도구
```

#### 2단계: Pencil로 와이어프레임/목업 작성
- 7개 화면에 대한 와이어프레임 작성
- 다크 모드 색상으로 구성
- 의료기기 특화 컴포넌트 배치 (비상 정지, 환자 ID 표시)

#### 3단계: 디자인 토큰 추출
- Pencil에서 PNG/PDF로 익스포트
- 컬러 코드 추출 (#1B4F8A 등)
- 폰트 사이즈, 간격 값 확인

#### 4단계: WPF XAML 변환
- Pencil PNG를 참고하여 WPF View 작성
- DesignSystem2026.xaml 토큰 사용
- MahApps.Metro 컨트롤 활용

### Figma Community 활용 (대안)

#### 무료 의료기기 UI 킷 검색 키워드:
- "medical dashboard"
- "healthcare kit"
- "hospital UI"
- "clinic design system"
- "telemedicine kit"

#### 무료 템플릿 (검증 필요):
- Material Design Medical Kit (Google)
- Healthcare Dashboard Kit
- Medical Components Library

#### 활용 방법:
1. Figma Community에서 킷 다운로드
2. Figma에서 WPF로 디자인 토큰 익스포트
3. DesignSystem2026.xaml에 토큰 병합
4. 컴포넌트 스타일 적용

---

## 📋 사용성 평가 프레임워크

### 1. 휴리스틱 평가 (Heuristic Evaluation)

**Nielsen 10 원칙 + 의료기기 요구**:
| 원칙 | 점수 (0-10) | Checkpoint |
|------|-------------|-----------|
| 1. 시스템 상태 가시성 | ? | 화면별 현재 상태 항상 표시 |
| 2. 시스템与现实世界的 일치 | ? | 의료기기 용어, 기호 사용 |
| 3. 사용자 통제 및 자유 | ? | 취소, 실행 취소 항상 제공 |
| 4. 일관성과 표준 | ? | 디자인 시스템 준수 |
| 5. 오류 방지 | ? | 입력 검증, 확인 대화상자 |
| 6. 인식 대체 회상하기보다 | ? | 오류 메시지 명확히 |
| 7. 유연성과 효율성 | ? | 자주 사용 작업 단축 |
| 8 | 심미적이고 미적으로 줄이치있는 사용 | ? | 미니멀리즘, 불필요한 요소 제거 |
| 9. 사용자가 오류를 인지하고 diagnose하도록 돕다 | ? | 오류 메시지, 해결 제안 |
| 10. 도움말과 문서 | ? | F1 도움말, 컨텍스트 도움 |

**의료기기 추가 기준**:
- 환자 안전: 환자 ID 항상 100% 정확
- 비상 정지: 항상 표시, 즉시 접근
- 선량 표시: 실시간 방사선 정보
- 알림 우선순위: Critical > Warning > Info

### 2. 작업 완료 시간 측정

**기준 워크플로우**:
1. 환자 검색 (Patient Search)
2. 검사 할당 (Worklist Assignment)
3. 영상 획득 (Acquisition)
4. 검사 조회 (Studylist Lookup)
5. 환자 등록 (Patient Registration)

**측정 방법**:
- 현재 UI 시간 측정 (baseline)
- 디자인 변경 후 시간 측정
- 개선율 계산: (baseline - new) / baseline × 100%

### 3. SUS 만족도 조사

**SUS (System Usability Scale)**:
- 10문항, 각 0-4점 (동의하지 않음 ~ 동의함)
- 점수 범위: 0-100
- 기준: 68점 이상 (Good), 80점 이상 (Excellent)

**설문 문항 (예시)**:
1. 나는 이 시스템을 불필요하게 자주 사용한다고 생각한다.
2. 시스템이 불필요하게 복잡하다.
3. 시스템을 사용하는 데 있어 일관성이 있다.
4. 나는 시스템을 효율적으로 사용할 수 있다.
5. 나는 시스템을 사용하지 않고 내 업무를 수행할 수 있다.
6. 시스템에 불규칙이 있다.
7. 나는 이 시스템을 다른 사람들에게 배우게 배울 수 있다.
8. 시스템을 배우는 데 도움이 필요하다.
9. 음성의 부족한 점이 있다고 해결되었다.

### 4. 접근성 감사 (Accessibility Audit)

**도구**:
- WAVE (WebAIM Accessibility Evaluator)
- Keyboard-only navigation
- Screen reader (NVDA)
- High contrast mode

**WCAG 2.2 AA 준수 사항**:
- 색상 대비: 4.5:1 (일반 텍스트), 3.0:1 (큰 텍스트)
- 터치 타겟: 최소 44×44px
- 키보드 내비게이션 가능
- 초점 표시기 명확
- 화면 리더기 지원

---

## 🔄 유연한 UI 대응 체계

### 1. UI 변경 승인 워크플로우

```
┌─────────────────────────────────────────────────────────────┐
│                    UI 변경 제안                            │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │  사용성 평가           │
              │  (휴리스틱 + 시간)     │
              └───────────────────────┘
                          │
                ┌─────────────┴─────────────┐
                ▼                             │
        ┌─────────────┐              ┌──────────────┐
        │ 점수 80+   │              │ 점수 <70    │
        └──────┬──────┘              └──────┬───────┘
               │                              │
               ▼                              ▼
        ┌──────────────────┐          ┌──────────────┐
        │  구현 및 배포     │          │  재설계 또는  │
        └──────────────────┘          │  원인 분석   │
                                       └──────────────┘
```

### 2. 단계적 롤아웃 전략

**Alpha Release** (내부 테스트):
- Worklist, Studylist 화면만 변경
- 1주간 사용성 평가
- 문제점 발견 시 즉시 수정

**Beta Release** (선택적 사용자):
- Acquisition, Settings 추가
- 2주간 사용성 평가
- 피드백 수집 및 개선

**Full Release** (전체 배포):
- 모든 화면 변경
- 최종 사용성 평가
- 정식 배포

### 3. 롤백 메커니즘

**롤백 트리거**:
- 이전 UI로 즉시 복원 가능 (App.xaml 리소스 교체)
- A/B 테스팅 지원 (일부 사용자만 신규 UI)
- Blue-Green 배포 전략

**롤백 절차**:
1. 사용자 로그아웃 즉시 이전 버전으로 복귀
2. 배포 중지
3. 원인 분석 후 재배포

---

## 📋 구현 로드맵

### Phase 1: Discovery & Design (2주)
| 작업 | 기간 | 산출물 |
|------|------|--------|
| 아키텍처 분석 | 완료 | HNVue_ARCHITECTURE_ANALYSIS.md |
| Pencil 다운로드 | 1일 | Pencil 설치 완료 |
| 와이어프레임 작성 | 3일 | 7화면 와이어프레임 |
| 디자인 토큰 추출 | 1일 | 컬러 코드, 스페이싱 값 |
| 사용성 평가 프레임워크 | 2일 | Heuristic rubric, SUS 설문 |

### Phase 2: XAML Prototyping (2주)
| 작업 | 기간 | 산출물 |
|------|------|--------|
| XAML Views 생성 | 1주 | 7개 Window/UserControl |
| ViewModels 작성 | 1주 | CommunityToolkit.Mvvm |
| 네비게이션 연결 | 2일 | 화면 간 이동 |
| DesignSystem2026 병합 | 1일 | App.xaml 리소스 |
| MahApps.Metro 스타일 적용 | 2일 | 테마 적용 |

### Phase 3: Testing & Validation (2주)
| 작업 | 기간 | 산출물 |
|------|------|--------|
| 휴리스틱 평가 | 3일 | 점수표, 개선안 |
| 작업 시간 측정 | 3일 | baseline vs 새 UI |
| SUS 조사 | 2일 | 설문 결과, 점수 |
| 접근성 감사 | 2일 | WAVE, keyboard nav |
| 문제점 수정 | 4일 | 수정된 UI |

### Phase 4: Deployment (1주)
| 작업 | 기간 | 산출물 |
|------|------|--------|
| Alpha 배포 | 2일 | 내부 테스트 |
| 피드백 수집 | 2일 | 개선사항 |
| 최종 배포 | 3일 | 정식 릴리즈 |

---

## 🎁 성공 전략

### 1. 기존 자산 활용

**이미 완성된 것**:
- ✅ DesignSystem2026.xaml (컬러, 타이포그래피, 스페이싱)
- ✅ MahApps.Metro 통합
- ✅ CommunityToolkit.Mvvm MVVM
- ✅ 기존 컴포넌트 (Modal, Toast, etc.)

**추가로 필요한 것**:
- ❌ Pencil 와이어프레임 (시각 디자인)
- ❌ XAML Views (LoginWindow.xaml, etc.)
- ❌ 사용성 평가 체계

### 2. Pencil 활용 전략

**Pencil의 장점**:
- 무료, 오픈소스
- 빠른 와이어프레임 작성
- 의료기기 스텔실 포함 (있을 수 있음)
- PDF/PNG 익스포트

**Pencil 사용 방법**:
1. Pencil 설치 (https://pencil.evolus.vn/)
2. 7개 화면 와이어프레임 작성
3. 다크 모드 색상으로 구성
4. PNG로 익스포트 후 WPF 변환 참고

### 3. Figma Community 활용 전략

**Figma Community 장점**:
- 풍부한 무료 템플릿
- 모던 의료기기 UI 패턴
- 접근성 기본 내장
- WPF로 변환 가능한 디자인 토큰

**검색 키워드**:
- "medical dashboard"
- "healthcare kit"
- "hospital UI"

---

## 📊 위험 평가 및 완화 전략

### 주요 위험 요인

| 위험 | 영향 | 확률 | 완화 전략 |
|------|------|------|----------|
| 디자인 불일치 | 높음 | 중 | Pencil 미리검토 |
| 사용성 저하 | 높음 | 중 | Alpha 테스트 |
| 접근성 위반 | 중간 | 낮음 | WAVE 사전 검사 |
| 배포 실패 | 중간 | 낮음 | 롤백 메커니즘 |

### 완화 전략

1. **기존 UI 백업** - 배포 전 전 현재 UI 백업
2. **A/B 테스팅** - 일부 사용자만 신규 UI 적용
3. **단계적 배포** - Alpha → Beta → Full
4. **모니터링** - 배포 후 사용 로그 분석

---

## 📌 핵심 성공 요인

1. **모듈성 보장** - HnVue.UI 독립성 유지
2. **디자인 시스템 재사용** - DesignSystem2026 활용
3. **사용성 중심** - 평가 프레임워크 엄격 준수
4. **안전 최우선** - 환자 안전, 비상 정지 보장
5. **유연한 대응** - 롤백 가능한 배포 전략

---

## 📞 다음 단계

1. **Pencil 다운로드 및 설치**
2. **7화면 와이어프레임 작성**
3. **사용성 평가 프레임워크 구축**
4. **XAML Views 프로토타이프 작성**

---

**작성일**: 2026-04-06  
**버전**: 1.0  
**상태**: 전략 수립 완료 - 구현 단계 대기

*이 보고서는 Sequential Thinking을 활용한 깊은 분석 기반입니다.*
