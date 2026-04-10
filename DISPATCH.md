# DISPATCH: Team A — 빌드 오류 수정 + 취약 패키지 + StyleCop

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: **P0-Blocker** (빌드 오류) + P1-Critical (보안) + P2-High (품질)
Supersedes: 이전 DISPATCH (Round 1 COMPLETE)

## Team A 역할 재확인 (.claude/rules/teams/team-a.md)

- **소유 모듈**: Common, Data, Security, SystemAdmin, Update
- **NuGet 관리**: Directory.Packages.props 중앙 관리, 추가 시 보안 리뷰 + RA SOUP 알림
- **Security 코드**: bcrypt 12+, JWT HS256, HMAC-SHA256 감사로그 — 암호화 코드 변경 금지
- **Common 인터페이스 변경 시**: `breaking-change` 라벨 이슈 + Coordinator 알림

## How to Execute

1. **Task 1 (P0-Blocker)부터** 수행
2. 각 Task 완료 후 체크박스 업데이트
3. Final Build Verification 수행
4. Status 업데이트

## Task 1: SystemSettingsRepository 빌드 오류 수정 (P0-Blocker)

**오류**: `SystemSettingsRepository` 생성자 시그니처 불일치 (CS1729)
**파일**: `tests/HnVue.SystemAdmin.Tests/SystemSettingsRepositoryTests.cs`
**수행**: 현재 생성자 시그니처 확인 → 테스트의 생성자 호출 수정

**검증 기준**:
- [ ] HnVue.SystemAdmin.Tests 빌드 오류 0건
- [ ] 기존 테스트 전부 통과

## Task 2: 취약 패키지 업그레이드 (P1-Critical)

**규칙**: 모든 버전 Directory.Packages.props 중앙 관리, RA SOUP 알림 필요
**대상**: Microsoft.Extensions.Caching.Memory 8.0.0, System.Text.Json 8.0.0 → 9.x
**RA 알림**: 패키지 변경 후 DISPATCH 보고에 `soup-update` 필요 명시

**검증 기준**:
- [ ] `dotnet list package --vulnerable` HIGH/CRITICAL 0건
- [ ] Microsoft.Extensions.* 메이저 버전 통일
- [ ] 빌드 성공

## Task 3: StyleCop — Common, Data (P2-High)

**제약**: 메서드 시그니처/동작 변경 금지, 빈 XML doc 금지
**검증 기준**:
- [ ] Common SA* 경고 수 감소 (before/after 기록)
- [ ] Data SA* 경고 수 감소
- [ ] 빌드 + 테스트 통과

## Task 4: StyleCop — Security, SystemAdmin, Update (P2-High)

**추가 제약**: Security 모듈 암호화/해싱/JWT 코드 변경 금지

**검증 기준**:
- [ ] Security/SystemAdmin/Update SA* 경고 수 감소
- [ ] 빌드 + 테스트 통과

## Constraints

- Team A 소유 파일만 수정, NuGet은 Directory.Packages.props만
- Common 인터페이스 변경 시 breaking-change 이슈 + Coordinator 알림
- Security 암호화/해싱/JWT 코드 변경 금지


## Final Verification [HARD — 이 섹션 미완료 시 COMPLETED 보고 금지]

1. 자기 모듈 빌드: `dotnet build` → 오류 0건
2. 자기 테스트: `dotnet test {소유 테스트}` → 전원 통과
3. 전체 솔루션 빌드: `dotnet build HnVue.sln -c Release` → 결과 기록
4. 빌드 출력 요약을 Status에 복사

## Git Completion Protocol [HARD]

1. git add (DISPATCH.md + 변경 파일)
2. git commit (conventional commit 형식)
3. git push origin team/team-a
4. PR 생성 (기존 open PR 확인 후 중복 방지)
5. PR URL을 Status에 기록

## Status

- **State**: NOT_STARTED
- **Build Evidence**: (미완료)
- **PR**: (미생성)
- **Results**: Task 1→PENDING, Task 2→PENDING, Task 3→PENDING, Task 4→PENDING
