# HnVue 코드 문서화 및 품질 표준

> 원본: README.md "코드 문서화" + "코드 품질 표준" 섹션에서 분리 (2026-04-09)

## 코드 문서화

HnVue는 3계층 코드 문서화 체계를 갖추고 있습니다.

### 1. XML Documentation Comments (100% 커버리지)

모든 public 멤버에 XML 문서 주석이 작성되어 있습니다.

- `Directory.Build.props`에 `<GenerateDocumentationFile>true</GenerateDocumentationFile>` 설정
- 257/257 public 멤버 주석 완비
- 빌드 시 `.xml` 파일 자동 생성
- IntelliSense 지원

### 2. 모듈별 README.md (15개)

주요 모듈 15개에 README.md가 포함되어 있습니다.

| 모듈 | 설명 |
|------|------|
| `src/HnVue.Common/README.md` | 공유 추상화, 모델, 인터페이스 (Core Layer) |
| `src/HnVue.Data/README.md` | 데이터 접근 계층 (EF Core + SQLite/SQLCipher) |
| `src/HnVue.Security/README.md` | 인증, 인가, 감사 로그 (FDA S524B) |
| `src/HnVue.Dicom/README.md` | DICOM 네트워킹 (fo-dicom 5.x) |
| `src/HnVue.Detector/README.md` | FPD 검출기 인터페이스 + 자사/타사 SDK 어댑터 |
| `src/HnVue.Workflow/README.md` | X-ray 촬영 워크플로우 엔진 (검출기 ARM 통합) |
| `src/HnVue.Imaging/README.md` | 의료 영상 처리 |
| `src/HnVue.Dose/README.md` | 방사선량 관리 |
| `src/HnVue.PatientManagement/README.md` | 환자 관리 및 워크리스트 |
| `src/HnVue.Incident/README.md` | 인시던트 대응 및 기록 |
| `src/HnVue.CDBurning/README.md` | CD/DVD 굽기 서비스 |
| `src/HnVue.Update/README.md` | 소프트웨어 업데이트 및 백업 |
| `src/HnVue.SystemAdmin/README.md` | 시스템 관리 서비스 |
| `src/HnVue.UI/README.md` | WPF UI 컴포넌트 (MVVM) |
| `src/HnVue.App/README.md` | WPF 애플리케이션 진입점 (Composition Root) |

### 3. DocFX API Reference

DocFX를 사용하여 XML 문서 주석으로부터 API 레퍼런스 사이트를 생성합니다.

```bash
# 사전 요구사항: .NET 8.0 SDK (Windows), DocFX 2.75+
dotnet tool install -g docfx

# 빌드 -> 메타데이터 추출 -> 사이트 생성
dotnet build -c Release
docfx docfx.json

# 로컬 미리보기
docfx serve _site
```

- 설정 파일: `docfx.json`
- 템플릿: `default` + `modern`
- 빌드 출력: `_site/` (`.gitignore`에 포함)

---

## 코드 품질 표준 (TRUST 5)

HnVue는 **TRUST 5 프레임워크**를 준수합니다:

| 항목 | 기준 | 현황 |
|------|------|------|
| **Tested** | 85%+ 커버리지, 안전 임계 90%+ | 1,135개 테스트, 90%+ 안전 임계 |
| **Readable** | XML 문서 주석, PascalCase 규칙 | 모든 public 멤버 주석 완비 |
| **Unified** | Result<T> 패턴, async/await ConfigureAwait(false) | 일관 적용 |
| **Secured** | OWASP 준수, RBAC 검증, 입력 검증 | bcrypt, JWT, HMAC, SQLCipher |
| **Trackable** | IEC 62304 S번호 주석, SWR Trait | [Trait("SWR", "SWR-XXX")] 추적성 |

### 코드 스타일

- **언어:** C# 12 (.NET 8)
- **네이밍:** PascalCase (public), camelCase (private)
- **주석:** XML doc comments (///)
- **포매팅:** .editorconfig 자동 적용
- **라이선스:** 모든 타사 라이브러리 라이선스 확인 (SBOM 참조)

---

문서 최종 업데이트: 2026-04-09
