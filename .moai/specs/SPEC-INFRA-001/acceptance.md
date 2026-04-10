# Acceptance Criteria: SPEC-INFRA-001

## REQ-SEC-001: 감사 로그 무결성 검증 수정

### AC-SEC-001-1: 변조된 체인 감지
```gherkin
Given 감사 로그에 10개의 항목이 체인으로 연결되어 있고
And 5번째 항목의 PreviousHash가 변조된 경우
When VerifyChainAsync를 호출하면
Then 5번째 항목에서 Result.IsSuccess == false를 반환해야 한다
And 에러 메시지에 "integrity violation"이 포함되어야 한다
```

### AC-SEC-001-2: 정상 체인 통과
```gherkin
Given 변조되지 않은 감사 로그 체인이 있는 경우
When VerifyChainAsync를 호출하면
Then 모든 항목에서 Result.IsSuccess == true를 반환해야 한다
```

## REQ-SEC-002: JWT 교착상태 수정

### AC-SEC-002-1: 비동기 검증
```gherkin
Given 유효한 JWT 토큰이 생성된 경우
When ValidateTokenAsync를 호출하면
Then .GetAwaiter().GetResult() 호출 없이 정상 검증되어야 한다
And IsRevokedAsync가 비동기로 호출되어야 한다
```

### AC-SEC-002-2: 취소된 토큰 거부
```gherkin
Given 토큰이 거부목록에 추가된 경우
When 해당 토큰으로 ValidateTokenAsync를 호출하면
Then Result.IsSuccess == false를 반환해야 한다
```

## REQ-SEC-003: 토큰 거부목록 손상 처리

### AC-SEC-003-1: 파일 손상 감지
```gherkin
Given 토큰 거부목록 파일이 손상된 경우 (잘못된 JSON)
When PersistentTokenDenylist가 초기화되면
Then 빈 거부목록으로 시작해야 한다
And ILogger를 통해 경고가 기록되어야 한다
And 기존에 발급된 토큰이 거부목록을 우회하지 못해야 한다
```

## REQ-DATA-001: PHI 암호화 구현

### AC-DATA-001-1: 암호화 라운드트립
```gherkin
Given 환자 이름 "홍길동"이 입력된 경우
When EncryptAsync 후 DecryptAsync를 수행하면
Then 원본 "홍길동"이 복원되어야 한다
```

### AC-DATA-001-2: 암호문 차이
```gherkin
Given 동일한 평문 "990101-1234567"이 두 번 암호화된 경우
When 두 암호문을 비교하면
Then 서로 달라야 한다 (다른 nonce 사용)
```

## REQ-DATA-002: 감사 추적 사용자 속성

### AC-DATA-002-1: 실제 사용자 기록
```gherkin
Given ISecurityContext에 "technician.kim"이 설정된 경우
When PatientRepository가 환자 정보를 저장하면
Then 감사 로그 항목의 사용자 필드가 "technician.kim"이어야 한다
```

### AC-DATA-002-2: 미인증 시 폴백
```gherkin
Given ISecurityContext에 사용자가 설정되지 않은 경우
When 감사 로그를 생성하면
Then 사용자 필드가 "anonymous"로 기록되어야 한다
```

## REQ-DATA-003: 성능 인덱스

### AC-DATA-003-1: 마이그레이션 적용
```gherkin
Given 기존 데이터베이스에 데이터가 있는 경우
When 인덱스 마이그레이션을 적용하면
Then 오류 없이 적용되어야 한다
And 기존 데이터가 손실되지 않아야 한다
```

## REQ-UPDATE-001: 원자적 롤백

### AC-UPDATE-001-1: 스테이징 실패 시 롤백
```gherkin
Given 백업이 성공적으로 생성된 경우
And 업데이트 스테이징 단계에서 오류가 발생한 경우
When ApplyUpdateAsync가 실패하면
Then 백업에서 복원되어야 한다
And 임시 파일이 정리되어야 한다
And 상태가 RolledBack으로 설정되어야 한다
```

### AC-UPDATE-001-2: 백업 없는 업데이트 차단
```gherkin
Given 디스크 공간이 부족하여 백업 생성이 실패한 경우
When ApplyUpdateAsync를 호출하면
Then 업데이트가 시작되지 않아야 한다
And 적절한 오류 메시지가 반환되어야 한다
```

## REQ-UPDATE-002: HTTPS 강제

### AC-UPDATE-002-1: HTTP URL 거부
```gherkin
Given UpdateServerUrl이 "http://updates.example.com"으로 설정된 경우
When UpdateOptions.Validate를 호출하면
Then 검증이 실패해야 한다
```

### AC-UPDATE-002-2: HTTPS URL 허용
```gherkin
Given UpdateServerUrl이 "https://updates.example.com"으로 설정된 경우
When UpdateOptions.Validate를 호출하면
Then 검증이 성공해야 한다
```

## REQ-UPDATE-003: 코드 서명 체인 검증

### AC-UPDATE-003-1: 만료된 인증서 거부
```gherkin
Given 코드가 만료된 인증서로 서명된 경우
When 서명을 검증하면
Then 검증이 실패해야 한다
```

### AC-UPDATE-003-2: 프로덕션 서명 검증 비활성화 방지
```gherkin
Given 프로덕션 환경에서 RequireAuthenticodeSignature가 false로 설정된 경우
When UpdateOptions.Validate를 호출하면
Then 검증이 실패해야 한다
```

## REQ-COMMON-001: 스레드 안전성

### AC-COMMON-001-1: 동시 접근
```gherkin
Given 10개의 스레드가 동시에 ISecurityContext에 접근하는 경우
When 각 스레드가 SetUser/ClearUser를 교차 호출하면
Then 예외가 발생하지 않아야 한다
And 데이터 손상이 없어야 한다
```

## REQ-COMMON-002: 에러 코드 확장

### AC-COMMON-002-1: 네트워크 에러 코드
```gherkin
Given ErrorCode에 NetworkTimeout, CommunicationFailure가 추가된 경우
When 해당 코드로 Result.Failure를 생성하면
Then 정상적으로 생성되어야 한다
```

## REQ-SYSADMIN-001: 설정 변경 감사

### AC-SYSADMIN-001-1: 변경 감사 기록
```gherkin
Given 관리자가 시스템 설정을 변경하는 경우
When SaveSettingsAsync가 호출되면
Then IAuditRepository.AppendAsync가 호출되어야 한다
And 감사 항목에 이전 값과 새 값이 포함되어야 한다
```

## 최종 검증 조건

- `dotnet build HnVue.sln --configuration Release`: 0 에러
- `dotnet test` Team A 전체: 100% 통과
- 기존 440개 테스트 회귀 없음
- 신규 테스트 커버리지: 수정된 모든 메서드 90%+ 브랜치 커버리지
