# Release Readiness Review (S12-R1)

## HnVue Console SW v1.0.0 릴리즈 준비도 검토 보고서

---

## 문서 메타데이터

| 항목 | 내용 |
|------|------|
| **문서 ID** | DOC-RELEASE-READINESS_S12-R1 |
| **버전** | v1.0 |
| **작성일** | 2026-04-18 |
| **작성자** | RA팀 |
| **상태** | Draft (검토 중) |
| **기준 규격** | IEC 62304 §5.8, FDA 21 CFR 820.30, ISO 14971:2019 |
| **관련 문서** | DOC-034 (ReleaseDoc), DOC-042 (CMP v2.2), DOC-044 (Known Anomalies), DOC-032 (RTM v2.7) |

---

## 1. 목적

S12-R1 시점에서 HnVue Console SW v1.0.0 릴리즈 준비도를 평가하고, 릴리즈 게이트 통과를 위한
미해결 사항 및 개선 필요 항목을 식별한다.

---

## 2. 릴리즈 체크리스트 현황 (DOC-034 기준)

### 2.1 10개 블로킹 항목 평가

| # | 확인 항목 | 상태 | 증빙 문서 | 갭 분석 |
|---|----------|------|----------|---------|
| 1 | V&V 종합 보고서 합격 | **부분** | DOC-025 VVSummary v1.0 | Final V&V Summary 업데이트 필요 (S12-R1 추적성 반영) |
| 2 | 위험 관리 보고서 완료 | **부분** | DOC-010 RMR (미확인), DOC-009 FMEA | RMP v2.0 업데이트 planned 2026-05, MR-072 + 4-Tier 통합 미완 |
| 3 | 사이버보안 테스트 합격 | **진행 중** | DOC-016 Cybersecurity Plan, DOC-046 | OWASP 스캔 주기적 실행 중, CVSS>=7 0건 유지 필요 |
| 4 | 사용적합성 테스트 합격 | **미확인** | [없음] | IEC 62366 사용적합성 테스트 보고서 필요 |
| 5 | QA 검증 합격 | **CONDITIONAL** | S11-R2 PASS 99.97% | S12-R1 PASS 전환 목표 (진행 중) |
| 6 | SBOM 최종 확인 | **현행** | DOC-019 SBOM v3.0 | CycloneDX 1.5, 42+ 컴포넌트 추적 유지 |
| 7 | 모든 결함 해결 | **부분** | DOC-044 Known Anomalies v1.0 Draft | DOC-044 Approved 상태 전환 필요 |
| 8 | 릴리즈 노트 작성 | **부분** | DOC-034 Section 3 | S10-R2~S12-R1 변경사항 릴리즈 노트 반영 필요 |
| 9 | 사용 설명서 (IFU) 완료 | **부분** | DOC-040 IFU v1.0 | 현재 v1.0 완성도 검증 필요 |
| 10 | 코드 서명 완료 | **미완료** | - | 릴리즈 빌드 시 생성 예정 |

### 2.2 종합 점수

- 완료 (Complete): 1/10 (10%)
- 부분 (Partial): 5/10 (50%)
- 진행 중 (In Progress): 2/10 (20%)
- 미완료 (Not Started): 2/10 (20%)

**릴리즈 준비도**: **55%** (Target: 100%)

---

## 3. 누락된 문서 식별

### 3.1 신규 작성 필요 문서

| 우선순위 | 문서 | 사유 |
|---------|------|------|
| P1 | DOC-010 RMR (Risk Management Report) | 릴리즈 게이트 필수. 현재 위치 미확인 |
| P1 | DOC-025 V&V Summary Report (최종본) | S12-R1 기준 업데이트 필요 |
| P2 | 사용적합성 테스트 보고서 (IEC 62366) | 블로킹 항목 #4 |
| P2 | DOC-044 Known Anomalies Approved 버전 | 현재 Draft v1.0, 승인 필요 |
| P3 | DOC-034 ReleaseDoc v2.0 (S12-R1 반영) | 릴리즈 노트 Section 3 업데이트 |

### 3.2 업데이트 필요 문서 (이미 존재, 갱신 필요)

| 우선순위 | 문서 | 현재 버전 | 목표 버전 | 갱신 내용 |
|---------|------|-----------|-----------|-----------|
| P1 | DOC-008 RMP | v1.x | v2.0 (2026-05) | 4-Tier + MR-072 통합 (planned) |
| P1 | DOC-009 FMEA | 현행 | 현행+ | S10~S12 구현 반영 |
| P2 | DOC-042 CMP | v2.2 | v2.3 | S10~S12 Sprint 이력 추가 |
| P2 | DOC-033 SOUP Report | v2.1 | v2.2 | 최근 NuGet 변경 반영 |
| P3 | DOC-019 SBOM | v3.0 | v3.1 | 분기별 업데이트 (2026-Q2) |
| P3 | DOC-040 IFU | v1.0 | v1.1 | UI 변경사항 반영 (MergeView, AcquisitionView) |

---

## 4. 개선 필요 사항 도출

### 4.1 규제 제출 패키지 준비

| 제출 패키지 | 상태 | 우선순위 |
|------------|------|---------|
| FDA 510(k) eSTAR (DOC-036 v2.0) | 준비 | P1 |
| CE Technical Documentation (DOC-037 v1.0) | 초안 | P1 |
| KFDA 제출 (DOC-039 v1.0) | 초안 | P2 |
| Predicate Comparison (DOC-050) | 초안 | P1 |
| GSPR Checklist (DOC-052) | 초안 | P1 |

### 4.2 추적성 매트릭스 검증

- DOC-032 RTM v2.7 (2026-04-18 개정)에서 S10-R2~S12-R1 변경사항 11개 SWR 추가 매핑
- 모든 SWR -> TC 100% 매핑 유지
- xUnit Trait 기반 코드-요구사항 추적 유지

### 4.3 4-서명 릴리즈 게이트 (DOC-034 Section 5)

릴리즈 승인에 필요한 4명의 서명:
1. SW 개발 책임자 (SW Dev Lead) — **미서명**
2. QA 팀장 (QA Lead) — **미서명**
3. RA/QA 책임자 (RA/QA Manager) — **미서명**
4. 프로젝트 관리자 (PM) — **미서명**

**릴리즈 전 전 4인의 서명 필수**

---

## 5. 권고 사항

### 5.1 즉시 조치 (S12-R1 라운드)

1. **DOC-032 RTM v2.7 업데이트 완료** (본 문서와 별도 작업)
2. **DOC-044 Known Anomalies** 작성자/검토자/승인자 지정 및 Approved 상태 전환
3. **DOC-025 V&V Summary** S12-R1 시점 재작성

### 5.2 Sprint S13 라운드 추진 권고

1. **DOC-010 RMR** 위치 파악 및 최신화 (또는 신규 작성)
2. **사용적합성 테스트** 계획 수립 및 실행
3. **DOC-034 ReleaseDoc v2.0** 개정 (S10~S12 반영)

### 5.3 Sprint S14~S15 라운드 추진 권고

1. **DOC-008 RMP v2.0** 업데이트 (2026-05 planned)
2. **FDA 510(k) 제출 패키지** 완성 (DOC-036 eSTAR)
3. **CE Technical Documentation** 완성 (DOC-037)

---

## 6. 릴리즈 게이트 통과 로드맵

```
S12-R1 (현재, 2026-04-18)
    ↓ RTM v2.7 + 릴리즈 준비도 검토
S12-R2~R4
    ↓ DOC-044 Approved, DOC-025 갱신, 사용적합성 테스트 계획
S13
    ↓ DOC-010 RMR, DOC-008 RMP v2.0, DOC-034 v2.0
S14~S15
    ↓ FDA/CE/KFDA 제출 패키지 완성
Release Gate
    ↓ 4-서명 확보
v1.0.0 Release (2026-09-01 예정)
```

---

## 7. 결론

S12-R1 현재 릴리즈 준비도는 **55%** 수준이다. Safety-Critical 모듈 커버리지는 90%+ 유지 중이며
RTM 추적성은 100% 매핑 달성했다. 단, 릴리즈 블로킹 항목 중:

- **사용적합성 테스트 보고서 부재** (IEC 62366)
- **DOC-044 Known Anomalies Draft 상태**
- **DOC-010 RMR 위치 미확인**
- **릴리즈 노트 S10~S12 미반영**

위 4개 항목이 핵심 리스크이다. S13 라운드에서 우선 처리하여 릴리즈 준비도 80%+ 달성을 권고한다.

---

*문서 끝 (End of Document)*

| 문서 ID | DOC-RELEASE-READINESS_S12-R1 |
|---------|-----------------------------|
| 버전 | v1.0 (Draft) |
| 작성일 | 2026-04-18 |
| 다음 검토 | S13-R1 시점 |
