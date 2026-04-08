# UI 변경 관리 프로세스 (Usability-Driven Change Management)

## 문서 정보

| 항목 | 내용 |
|------|------|
| 문서 ID | UX-CMP-001 |
| 버전 | 1.0 |
| 작성일 | 2026-04-06 |
| 규격 기준 | IEC (International Electrotechnical Commission) 62366-1:2015, FDA (Food and Drug Administration) HFE Guidance, SPEC-UI-001 |
| 상태 | 승인됨 |

---

## 1. 개요

이 문서는 HnVue Console SW의 UI 변경에 대한 사용성 평가 기반 의사결정 프로세스를 정의합니다.

**목적**: 사용성 평가 결과가 UI 변경 결정에 체계적으로 반영되고, 변경으로 인한 안전성·사용성 저하를 방지합니다.

**기준선 (Baseline)**:
- SUS 점수: 82.3 (DOC-028, 2026-03-18)
- 표준 촬영 사이클: 68초 평균
- 치명적 사용 오류: 0건

---

## 2. 화면 분류 (Screen Classification)

### Critical Path 화면 (최고 수준 검증 필요)

| 화면 | 이유 |
|------|------|
| Acquisition (영상 획득) | 방사선 노출 제어, 환자 안전 직결 |
| Login (로그인) | 접근 제어, 보안 관문 |
| Worklist (환자 선택) | 환자 ID 혼동 시 오검사 위험 |

### Non-Critical 화면 (표준 검증)

| 화면 | 이유 |
|------|------|
| Studylist | 기록 조회 (실시간 안전 무관) |
| Merge | 선택적 기능 |
| Settings | 관리자 기능 |
| Add Patient | 등록 기능 |

---

## 3. 변경 유형 정의

| 유형 | 설명 | 예시 |
|------|------|------|
| A - Visual | 색상, 글꼴, 간격만 변경 | 버튼 색상 조정 |
| B - Layout | 컴포넌트 위치/크기 변경 | 패널 재배치 |
| C - Workflow | 상호작용 흐름 변경 | 버튼 클릭 순서 변경 |
| D - Safety-Critical | 안전 관련 컴포넌트 변경 | Emergency Stop 위치/스타일 |

---

## 4. 변경 승인 매트릭스

| 화면 분류 | 변경 유형 A | 변경 유형 B | 변경 유형 C | 변경 유형 D |
|-----------|------------|------------|------------|------------|
| Critical Path | 동료 검토 | 휴리스틱 평가 + 동료 검토 | 완전 평가 + QA/RA 서명 | 완전 평가 + QA/RA 서명 + 사용성 테스트 |
| Non-Critical | 자기 검토 | 동료 검토 | 휴리스틱 평가 + 동료 검토 | 완전 평가 + QA/RA 서명 |

---

## 5. 변경 관리 프로세스 단계

### Step 1: 사용성 문제 식별 (Issue Identification)

**트리거**:
- 사용자 피드백 (QA 팀, 방사선사 등)
- 정기 휴리스틱 평가 (분기별)
- SUS 점수 추이 모니터링
- 사용 오류 사건 보고

**조치**:
1. Gitea 이슈 등록: 레이블 `usability`, `ui-change`, 심각도 레이블 (`critical`/`high`/`medium`/`low`)
2. 영향 화면 및 변경 유형(A/B/C/D) 분류
3. Critical Path 여부 판단

**이슈 템플릿** (`.gitea/ISSUE_TEMPLATE/usability_issue.md` 참조):
```
화면: [Login/Worklist/Acquisition/...]
변경 유형: [A/B/C/D]
현재 상태: [현재 UI 동작 설명]
문제점: [사용성 문제 설명]
제안: [제안하는 변경 내용]
SUS 기준선 영향 예상: [영향 없음/낮음/높음]
```

---

### Step 2: 사전 평가 (Pre-Change Evaluation)

**휴리스틱 평가** (변경 유형 B 이상):

Nielsen 10 원칙 + 의료기기 추가 기준으로 0~10점 평가:

| 원칙 | 가중치 | 의료기기 고려사항 |
|------|--------|-----------------|
| 1. 시스템 상태 가시성 | 15% | 방사선 노출 상태, 장비 상태 항상 표시 |
| 2. 현실 세계 일치 | 10% | 방사선사 용어, 해부학적 표기 |
| 3. 사용자 통제 및 자유 | 10% | 취소/중단 항상 가능 |
| 4. 일관성과 표준 | 10% | 디자인 시스템 준수 |
| 5. 오류 방지 | 15% | 위험 조작 이중 확인 필수 |
| 6. 인식 vs 회상 | 10% | 자주 사용 정보 항상 표시 |
| 7. 유연성과 효율성 | 10% | 단축키, 빠른 접근 |
| 8. 심미적 최소주의 | 5% | 불필요 요소 제거 |
| 9. 오류 인식/진단 지원 | 10% | 명확한 오류 메시지 |
| 10. 도움말/문서 | 5% | 컨텍스트 도움말 |

합산 점수 기준:
- Critical Path 화면: ≥ 75/100 유지 또는 개선
- Non-Critical 화면: ≥ 70/100 유지 또는 개선

**기준선 측정**:
- 현재 SUS 점수 확인 (`docs/usability/METRICS_HISTORY.md`)
- 해당 화면의 과업 완료 시간 측정 (기준: DOC-028)

---

### Step 3: 디자인 작성 (Design Creation)

**도구**: Pencil (무료) 또는 Figma (무료 티어)

**절차**:
1. `docs/ui_mockups/screens/{screen-name}.pen` 파일 생성/수정
2. `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` 컬러 토큰 적용
3. 디자인 결정 사항 `docs/ui_mockups/screens/{screen-name}.md`에 기록
4. PNG 익스포트 → Gitea PR에 첨부

**필수 확인사항**:
- [ ] Emergency Stop 버튼: 항상 표시, 최소 44×44px, #D50000 (Acquisition 화면)
- [ ] 환자 ID: 28px Display 폰트, 상단 고정
- [ ] 색상 대비: 일반 텍스트 ≥ 4.5:1, 큰 텍스트 ≥ 3:1
- [ ] 모든 인터랙티브 요소: 최소 44×44px 터치 타겟

---

### Step 4: 검토 및 승인 (Review & Approval)

**검토 절차** (변경 유형/화면 분류에 따라):

```
자기 검토 (Self Review):
  - 본인이 디자인 체크리스트 확인
  - Gitea PR 생성 (본인 머지 가능)

동료 검토 (Peer Review):
  - 최소 1명 개발자 검토 및 승인
  - 디자인 체크리스트 확인
  - Gitea PR: 1 approval 필요

완전 평가 (Full Evaluation):
  - 휴리스틱 평가 점수 문서화
  - QA/RA 엔지니어 서명 (물리적 또는 디지털)
  - Gitea PR: QA/RA approval 필요
  - 사용성 테스트 (필요 시)
```

---

### Step 5: WPF XAML 구현 (Implementation)

**규칙**:
1. Pencil/Figma 승인된 디자인만 구현
2. 모든 리소스 키는 `HnVue.Core.*` 네임스페이스 사용 (하드코딩 금지)
3. MahApps.Metro 컨트롤 우선 사용
4. ViewModel 바인딩은 인터페이스를 통해서만

**금지 사항**:
```xaml
<!-- 금지: 하드코딩된 색상 -->
<Button Background="#1B4F8A" />

<!-- 허용: 디자인 토큰 사용 -->
<Button Background="{StaticResource HnVue.Core.Brush.ButtonPrimary}" />
```

---

### Step 6: 사후 검증 (Post-Change Validation)

**검증 항목**:

| 항목 | 방법 | 합격 기준 |
|------|------|----------|
| SUS 점수 | 사용자 설문 (n≥5) | ≥ 기준선 82.3 |
| 과업 완료 시간 | 시간 측정 | ≤ 기준선 + 10% |
| 휴리스틱 점수 | 평가 위원 채점 | ≥ 유형별 기준 |
| 접근성 | High Contrast + 키보드 내비게이션 | 100% 통과 |
| 아키텍처 테스트 | `dotnet test` | 0 failures |

**결과 기록**: `docs/usability/METRICS_HISTORY.md` 업데이트

---

### Step 7: 배포 및 모니터링 (Deployment & Monitoring)

**배포 전략** (단계적 롤아웃):
1. **Alpha**: 내부 QA 팀 (1주)
2. **Beta**: 선정된 임상 사용자 (2주)
3. **Full**: 전체 배포

**모니터링**:
- 배포 후 1주간 사용 오류 사건 추적
- 사용자 피드백 수집
- SUS 점수 변화 추적

---

## 6. 롤백 메커니즘 (Rollback Mechanism)

### 롤백 트리거 조건

| 조건 | 조치 |
|------|------|
| SUS 점수 < 78 (기준선 -4.3) | 자동 롤백 권고 |
| 치명적 사용 오류 발생 | 즉시 롤백 (QA/RA 명령) |
| 과업 완료 시간 > 기준선 +20% | 롤백 검토 |
| Critical Path 오류율 증가 | 즉시 롤백 (QA/RA 명령) |

### 롤백 절차

**Code-Level 롤백 (WPF ResourceDictionary)**:

```csharp
// ThemeRollbackService.cs
public class ThemeRollbackService
{
    private const string CurrentThemePath = "Themes/HnVueTheme.xaml";
    private const string PreviousThemePath = "Themes/HnVueTheme.previous.xaml";

    public void RollbackToPrevious()
    {
        var dict = Application.Current.Resources.MergedDictionaries;
        var currentTheme = dict.FirstOrDefault(d => d.Source?.ToString().Contains("HnVueTheme") == true);
        if (currentTheme != null)
        {
            dict.Remove(currentTheme);
            dict.Add(new ResourceDictionary
            {
                Source = new Uri(PreviousThemePath, UriKind.Relative)
            });
        }
    }
}
```

**Git-Level 롤백**:
```bash
# 특정 파일만 이전 커밋으로 롤백
git checkout HEAD~1 -- src/HnVue.UI/Themes/HnVueTheme.xaml
git checkout HEAD~1 -- src/HnVue.UI/Views/{TargetView}.xaml
```

---

## 7. 관련 문서

| 문서 | 위치 |
|------|------|
| 사용성 측정 이력 | `docs/usability/METRICS_HISTORY.md` |
| 아키텍처 독립성 정책 | `docs/architecture/UI_INDEPENDENCE_POLICY.md` |
| Pencil → XAML 워크플로우 | `docs/architecture/DESIGN_TO_XAML_WORKFLOW.md` |
| SPEC-UI-001 | `.moai/specs/SPEC-UI-001/spec.md` |
| 사용성 테스트 보고서 (기준선) | `docs/testing/DOC-028_UsabilityTestReport_v1.0.md` |

---

버전: 1.0 | 2026-04-06 | 상태: 승인됨
