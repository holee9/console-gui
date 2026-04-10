# DISPATCH: RA — 규제 문서 동기화

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: P2-High
Supersedes: 이전 DISPATCH (Round 1 COMPLETE)

## RA 역할 재확인 (.claude/rules/teams/ra.md)

- **소유**: docs/regulatory/, docs/planning/, docs/risk/, docs/verification/, docs/management/, scripts/ra/
- **IEC 62304 버전**: 메이저=규제 재검토, 마이너=수정/보완
- **RTM**: 100% SWR→TC 매핑 필수
- **변경→문서**: NuGet 추가/제거 → DOC-019 SBOM + DOC-033 SOUP

## How to Execute

1. Task 1은 Team A Task 2 완료 후 수행 (의존성)
2. Task 2-3 독립 수행 가능
3. 체크박스 + Status 업데이트

## Task 1: SBOM/SOUP — Team A 패키지 변경 반영 (P2-High, BLOCKED by Team A)

**트리거**: Team A Microsoft.Extensions 패키지 업그레이드
**수행**: DOC-019 SBOM + DOC-033 SOUP 버전 업데이트, 문서 마이너 버전 증가

**검증 기준**:
- [ ] DOC-019 패키지 버전 반영
- [ ] DOC-033 패키지 버전 반영
- [ ] 문서 버전 업데이트

## Task 2: RTM 갭 점검 (P2-High)

**수행**: [Trait("SWR")] 어노테이션 현황 점검, SWR→TC 갭 식별

**검증 기준**:
- [ ] RTM 갭 분석 완료
- [ ] 갭 시 `ra-update` + `priority-high` 이슈 생성

## Task 3: DOC-042 CMP 완성도 점검 (P3-Medium)

**수행**: author/reviewer/approver 점검, Draft→Review 전환 준비

**검증 기준**:
- [ ] DOC-042 필드 완성
- [ ] Draft→Review 전환 가능

## Constraints

- RA 소유 파일만 수정
- 소스 코드 수정 금지
- IEC 62304 문서 버전 정책 준수


## Final Verification [HARD — 이 섹션 미완료 시 COMPLETED 보고 금지]

1. 자기 모듈 빌드: `dotnet build` → 오류 0건
2. 자기 테스트: `dotnet test {소유 테스트}` → 전원 통과
3. 전체 솔루션 빌드: `dotnet build HnVue.sln -c Release` → 결과 기록
4. 빌드 출력 요약을 Status에 복사

## Git Completion Protocol [HARD]

1. git add (DISPATCH.md + 변경 파일)
2. git commit (conventional commit 형식)
3. git push origin team/ra
4. PR 생성 (기존 open PR 확인 후 중복 방지)
5. PR URL을 Status에 기록

## Status

- **State**: NOT_STARTED
- **Build Evidence**: (미완료)
- **PR**: (미생성)
- **Results**: Task 1→BLOCKED(Team A 대기), Task 2→PENDING, Task 3→PENDING
