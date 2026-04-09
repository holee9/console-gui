# HnVue FAQ 및 문제 해결

> 원본: README.md "FAQ 및 문제 해결" 섹션에서 분리 (2026-04-09)

## Q: 빌드 실패 -- ".NET 8.0.419를 찾을 수 없습니다"

**A:** `global.json`의 .NET 버전 확인
```bash
dotnet --version  # 8.0.419 이상 확인
dotnet --list-sdks  # 설치된 SDK 목록
```

SDK가 없으면 [dotnet.microsoft.com](https://dotnet.microsoft.com) 에서 .NET 8.0.419 LTS 다운로드.

## Q: 테스트 실패 -- "System.InvalidOperationException: No database provider"

**A:** `appsettings.json` 확인
```json
{
  "ConnectionStrings": {
    "HnVueDb": "Data Source={appDataPath}/hnvue.db; Password=dev-password-12345;"
  }
}
```

## Q: DICOM 전송 실패 -- "Cannot connect to PACS server"

**A:** DICOM 서버 설정 확인
- `appsettings.json`에서 DICOM 서버 주소/포트 확인
- 테스트: 로컬 DICOM 에코 서버 사용 (DCM4CHEE, Conquest DICOM)
- `DicomStoreScu` 테스트는 서버 없이 모킹으로 진행

## Q: CD 굽기 실패 -- "No CD/DVD drive detected"

**A:** IMAPI2 COM 인터페이스 확인
- Windows: IMAPI2 서비스 실행 중 확인 (`services.msc`)
- 테스트: `IMAPIComWrapper` 시뮬레이션 모드 사용
- 실제 드라이브: Windows 11에서 검증 필요

## Q: JWT 토큰 만료 오류

**A:** 시스템 시간 동기화
```bash
# Windows
w32tm /resync

# Linux
sudo ntpdate -s time.nist.gov
```

JWT 토큰 유효시간: **15분**

---

문서 최종 업데이트: 2026-04-09
