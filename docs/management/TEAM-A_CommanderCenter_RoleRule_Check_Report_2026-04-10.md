# Team A 역할 및 규칙 점검 보고서

| 항목 | 내용 |
|------|------|
| 보고 일자 | 2026-04-10 |
| 보고 대상 | Main Branch MoAI Commander Center |
| 작성 주체 | Team A Worktree 점검 |
| 점검 목적 | Team A 워크트리의 역할, 작업 규칙, 이슈 선행 규칙을 재확인하고 현재 작업 기준을 명문화 |
| 점검 범위 | Team A 전용 규칙, 운영 가이드, 병렬 개발 전략, 현재 DISPATCH |

---

## 1. 점검 근거 문서

- `.claude/rules/teams/team-a.md`
- `docs/OPERATIONS.md`
- `docs/STRATEGY-002_ParallelDevelopment_v1.0.md`
- `DISPATCH.md`
- `README.md`

---

## 2. Team A 역할 점검 결과

### 2.1 역할 확인 결과

Team A는 인프라 및 기반 계층 전담 팀으로 정의되어 있으며, 현재 워크트리는 `team/team-a` 브랜치에 연결된 Team A 전용 작업 공간임을 확인했다.

### 2.2 소유 모듈

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.SystemAdmin`
- `HnVue.Update`

### 2.3 책임 범위

- 공통 인터페이스와 기반 모델 유지
- EF Core 및 데이터 접근 계층 관리
- 인증, 권한, 감사로그 등 보안 기반 관리
- 시스템 설정 및 업데이트 모듈 유지
- Team A 소유 모듈 관련 테스트 유지
- NuGet 중앙 버전 관리 및 관련 규제 영향 식별

### 2.4 역할 경계 점검 결과

- Team A의 소유 범위는 문서 간 일관되게 정의되어 있음
- Coordinator 승인 없이 공통 인터페이스를 변경하면 안 됨
- RA 연계 없이 SOUP 영향 패키지 변경을 진행하면 안 됨
- Team A 비소유 모듈에 대한 임의 수정은 운영 규칙 위반 소지가 있음

판정: `역할 경계 명확 / 타 팀 의존 지점 명확`

---

## 3. Team A 작업 규칙 점검 결과

### 3.1 기술 규칙

- Repository는 `HnVue.Common` 인터페이스를 구현해야 함
- 데이터 접근은 async 기반으로 작성해야 함
- async 메서드는 `CancellationToken`을 포함해야 함
- 트랜잭션 작업은 `IUnitOfWork` 기준을 따라야 함
- 패키지 버전은 `Directory.Packages.props`에서 중앙 관리해야 함

### 3.2 보안 규칙

- bcrypt work factor는 최소 12 이상 유지
- JWT는 HS256 서명과 구성 가능한 만료 정책 유지
- 감사로그는 HMAC-SHA256 해시 체인 무결성 유지
- SQLCipher 키는 외부 보안 설정에서 공급되어야 하며 하드코딩 금지
- SQLCipher 연결 후 즉시 `PRAGMA key` 적용 필요

### 3.3 DB 및 마이그레이션 규칙

- 마이그레이션 명명 규칙: `YYYYMMDD_DescriptiveName`
- `Up()` 및 `Down()` 메서드 모두 구현 필수
- PR 전 `dotnet ef database update` 검증 필요
- 스키마 변경 전 Coordinator 통지 필요

### 3.4 변경 통제 규칙

- 공통 인터페이스 변경: `breaking-change` 이슈 생성 + Coordinator 알림
- NuGet 추가 또는 SOUP 영향 변경: `soup-update` 이슈 생성 + RA 알림
- DB 마이그레이션: `team-a` + `feat` 라벨 이슈 생성

판정: `규칙 정의 충분 / 사전 승인 체계 명확 / 규제 연동 지점 명확`

---

## 4. 작업 전 Git 이슈 규칙 점검 결과

### 4.1 기본 원칙

운영 가이드상 기본 흐름은 아래와 같다.

`이슈 생성 -> 팀 할당 -> 구현 -> PR -> 병합 -> 이슈 종료`

즉, Team A 작업은 원칙적으로 작업 시작 전에 이슈를 먼저 등록하고 진행해야 한다.

### 4.2 Team A 적용 기준

- 기능 개발 시작: `feat` + `team-a` 라벨 이슈 선행
- 공통 인터페이스 변경: `breaking-change` + `team-a`
- NuGet 추가/변경: `soup-update`
- DB 마이그레이션: `team-a` + `feat`
- 규제 문서 영향 발생 시 구현팀이 직접 문서를 수정하지 않고 `ra-update` 이슈로 연계

### 4.3 저장소 기준

- Gitea: 내부 개발 이슈의 기본 시스템
- GitHub: 공개 이슈, 릴리스 노트, CI/CD 연동용
- 보안 및 민감 이슈는 Gitea 우선 처리

판정: `이슈 선행 원칙 확인 / Team A 라벨 규칙 확인 / 내부 우선 처리 원칙 확인`

---

## 5. 현재 DISPATCH 기준 Team A 우선 작업 재확인

현재 Team A DISPATCH에서 확인된 우선순위는 다음과 같다.

1. `P0`: `SystemSettingsRepository` 관련 빌드 오류 수정
2. `P1`: 취약 패키지 대응 및 중앙 버전 관리 정리
3. `P2`: Team A 소유 모듈 StyleCop 경고 감축

추가 제약도 재확인했다.

- Common 인터페이스 변경 시 `breaking-change` 이슈와 Coordinator 알림 필요
- 패키지 변경 시 DISPATCH 보고에 `soup-update` 필요 명시
- Security 암호화, 해시, JWT 핵심 코드의 불필요한 변경 금지

판정: `현재 작업 지시와 Team A 규칙이 상충하지 않음`

---

## 6. 점검 종합 결론

### 6.1 종합 판정

Team A 워크트리의 역할, 소유 모듈, 변경 승인 체계, 이슈 선행 규칙은 문서상 명확하게 정리되어 있으며 상호 모순 없이 연결되어 있다.

### 6.2 현재 작업 기준

Team A는 앞으로 아래 기준으로 작업해야 한다.

- 소유 모듈 범위 내에서만 우선 작업
- 작업 시작 전 관련 이슈 선행 등록 여부 확인
- 인터페이스, 패키지, 스키마 변경은 승인 및 통지 규칙 준수
- RA 및 Coordinator 연계가 필요한 변경은 단독 처리 금지

### 6.3 커맨더센터 보고 사항

- Team A 역할 및 규칙 점검 완료
- Team A 워크트리는 `team/team-a` 전용 작업 공간으로 확인
- 작업 전 Git 이슈 선행 원칙 확인 완료
- 현재 DISPATCH 수행 시 필요한 승인 및 라벨 규칙 확인 완료
- 후속 실작업은 Team A 소유 범위와 이슈 규칙을 기준으로 진행 가능

---

## 7. Commander Center 요청사항

- Team A 관련 신규 작업 지시 시 이슈 번호 또는 생성 대상 저장소를 함께 지정해 주면 실행 일관성이 높아짐
- Common 인터페이스 변경 가능성이 있는 작업은 사전 Coordinator 판단을 병행하는 것이 안전함
- NuGet 및 SBOM 영향 작업은 RA 알림 요구 여부를 DISPATCH에 계속 명시하는 것이 바람직함

---

## 8. 최종 상태

- 상태: `점검 완료`
- 차기 단계: `이슈 선행 확인 후 DISPATCH 작업 착수`
