# HnVue Console SW — API Documentation

X-ray 촬영 콘솔 소프트웨어 API 레퍼런스 문서입니다.

## 개요

HnVue Console SW는 IEC 62304 Class B 의료기기 소프트웨어로, 14개 모듈로 구성된 WPF .NET 8 애플리케이션입니다.

## 모듈 구조

| 계층 | 모듈 | 설명 |
|------|------|------|
| Core | HnVue.Common | 공유 추상화, 모델, 인터페이스 |
| Data | HnVue.Data | EF Core + SQLite/SQLCipher |
| Domain | HnVue.Security | 인증/인가/감사 (FDA §524B) |
| Domain | HnVue.Dicom | DICOM 네트워킹 (fo-dicom 5.x) |
| Domain | HnVue.Workflow | 촬영 워크플로우 상태 머신 |
| Domain | HnVue.Imaging | 의료 영상 처리 |
| Domain | HnVue.Dose | 방사선량 관리 |
| Domain | HnVue.PatientManagement | 환자/워크리스트 관리 |
| Domain | HnVue.Incident | 인시던트 대응 |
| Domain | HnVue.CDBurning | CD/DVD 내보내기 |
| Domain | HnVue.Update | SW 업데이트/백업 |
| Domain | HnVue.SystemAdmin | 시스템 관리 |
| UI | HnVue.UI | WPF MVVM 컴포넌트 |
| App | HnVue.App | Composition Root |

## 빌드 방법

### 사전 요구사항

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows)
- [DocFX](https://dotnet.github.io/docfx/) v2.75+

### API 문서 생성

```bash
# 1. 프로젝트 빌드 (XML 문서 생성)
dotnet build -c Release

# 2. DocFX 메타데이터 추출 + 사이트 빌드
docfx docfx.json

# 3. 로컬 미리보기
docfx serve _site
```

## 규제 컨텍스트

- IEC 62304: Class B 소프트웨어 수명주기
- FDA 21 CFR Part 11: 전자서명, 감사 추적
- FDA §524B: 사이버보안 요구사항
- IEC 62366-1: 사용적합성
