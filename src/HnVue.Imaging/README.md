# HnVue.Imaging

> DR/CR 검출기 영상 처리 라이브러리 (IEC 62304 Class B)

## 목적

X-ray DICOM 파일 및 Raw 이미지 파일에 대한 창/레벨, 줌, 패닝, 회전, 뒤집기 연산을 제공합니다.
영상 품질이 진단 정확도에 직접 영향을 미치는 안전-필수 구성 요소입니다.

---

## 주요 타입

| 타입 | 종류 | 설명 |
|------|------|------|
| `ImageProcessor` | `sealed class` | `IImageProcessor` 구현체. 모든 영상 처리 연산의 진입점 |

---

## ImageProcessor 공개 메서드

| 메서드 | 반환 타입 | SWR | 설명 |
|--------|-----------|-----|------|
| `ProcessAsync(string rawImagePath, ProcessingParameters, CancellationToken)` | `Task<Result<ProcessedImage>>` | SWR-IP-020 | DICOM 파싱 시도 후 Raw 바이트 폴백. 16비트 픽셀을 8비트 표시 버퍼로 정규화. DICOM VOI LUT 태그 자동 추출 (0028,1050/0028,1051). `RawPixelData16`에 원본 16비트 값 보존 |
| `ApplyWindowLevel(ProcessedImage, double windowCenter, double windowWidth)` | `Result<ProcessedImage>` | SWR-IP-022 | DICOM 표준 선형 W/L LUT 적용. output = clamp((px − (center − width/2)) / width, 0, 1) × 255 (Issue #2) |
| `Zoom(ProcessedImage, double factor)` | `Result<ProcessedImage>` | SWR-IP-022, SWR-IP-024 | factor > 1 → Bicubic(Catmull-Rom) 업스케일. factor ≤ 1 → Area Average 다운스케일 (Issue #4) |
| `Pan(ProcessedImage, int deltaX, int deltaY)` | `Result<ProcessedImage>` | SWR-IP-026 | 픽셀 데이터 불변. PanOffsetX/PanOffsetY 누적. 렌더링 레이어(WriteableBitmap 뷰포트)가 오프셋 사용 (Issue #1) |
| `Rotate(ProcessedImage, int degrees)` | `Result<ProcessedImage>` | SWR-IP-027 | 90·180·270° 시계방향 회전 (픽셀 좌표 매핑, Issue #3) |
| `Flip(ProcessedImage, bool horizontal)` | `Result<ProcessedImage>` | SWR-IP-027 | horizontal=true: 좌우(수평) 반전, false: 상하(수직) 반전 (Issue #3) |
| `ApplyGainOffsetCorrection(image, gainMap, offsetMap)` | `Result<ProcessedImage>` | SWR-IP-039 | Gain/Offset 캘리브레이션 보정. gainMap/offsetMap null 시 `ErrorCode.CalibrationDataMissing` 반환. 안전-필수 (HAZ-RAD, HAZ-SW) |
| `ApplyNoiseReduction(image, strength)` | `Result<ProcessedImage>` | SWR-IP-041 | 3×3 가우시안 커널 기반 적응형 노이즈 제거. strength 0.0~1.0. 안전-필수 (HAZ-RAD) |
| `ApplyEdgeEnhancement(image, strength)` | `Result<ProcessedImage>` | SWR-IP-043 | Unsharp mask (5×5 가우시안). strength 0.0~1.0 |
| `ApplyScatterCorrection(image)` | `Result<ProcessedImage>` | SWR-IP-045 | 대형 가우시안 블러 빼기 방식 산란선 보정. 안전-필수 (HAZ-RAD) |
| `ApplyAutoTrimming(image, threshold)` | `Result<ProcessedImage>` | SWR-IP-047 | 임계값 기반 어두운 테두리 마스킹. threshold 기본값 10 |
| `ApplyClahe(image, clipLimit, tileSize)` | `Result<ProcessedImage>` | SWR-IP-050 | 완전 CLAHE 순수 C# 구현. clipLimit 기본 2.0, tileSize 기본 8 |
| `ApplyBrightnessOffset(image, offset)` | `Result<ProcessedImage>` | SWR-IP-052 | 밝기 오프셋 -255~+255 |
| `ApplyBlackMask(image, left, top, right, bottom, apply)` | `Result<ProcessedImage>` | SWR-IP-049 | Black Mask On/Off 토글. apply=false 시 RawPixelData16에서 복원 |

### 내부 헬퍼 (private)

| 헬퍼 | 설명 |
|------|------|
| `ProcessDicomFile` | DICOM 태그(Rows/Columns/BitsAllocated/PixelData/VOI LUT) 파싱 |
| `ProcessRawFile` | 호출자 제공 크기 우선, 없으면 제곱근 추정 (Issue #7) |
| `ExtractDicomPixelBytes` | OtherByte / OtherWord / FragmentSequence 지원 |
| `ExtractRaw16BitValues` | 리틀엔디언 바이트쌍 → ushort[] 변환 (SWR-IP-036, Issue #8) |
| `Normalize16BitTo8Bit` | min-max 선형 신장 → 8비트 출력 |
| `BicubicResample` | Catmull-Rom 커널 업스케일 (SWR-IP-024) |
| `AreaAverageResample` | 박스 필터 다운스케일, 앨리어싱 방지 (SWR-IP-024) |
| `BilinearResample` | 양선형 보간 (내부 유틸) |
| `ComputeAutoWindow` | 평균 ± 2×표준편차 자동 W/L 산출 |
| `ApplyGaussian3x3` | 3×3 가우시안 필터 |
| `ApplyGaussian5x5` | 5×5 가우시안 필터 |
| `ApplyLargeGaussian` | 대형 가우시안 블러 (산란선 보정용) |
| `BoxBlur` | 박스 블러 (CLAHE 기반 필터) |

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 제공 항목 |
|----------|-----------|
| `HnVue.Common` | `IImageProcessor`, `ProcessedImage`, `ProcessingParameters`, `Result<T>`, `ErrorCode` |

### NuGet 패키지

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `fo-dicom` | 5.x | DICOM 파일 파싱 및 픽셀 데이터 추출 |

> fo-dicom 5.x `DicomFile.OpenAsync()`는 `CancellationToken`을 지원하지 않습니다.
> 취소 요청은 I/O 호출 직전에 확인하는 방식으로 처리합니다. (Issue #6, fo-dicom 6+ 개선 예정)

---

## DI 등록

별도의 `AddXxx()` 확장 메서드 없음 — 호스트 애플리케이션에서 직접 등록합니다.

```csharp
services.AddSingleton<IImageProcessor, ImageProcessor>();
```

---

## 테스트

| 항목 | 내용 |
|------|------|
| 테스트 프로젝트 | `tests/HnVue.Imaging.Tests/` |
| 테스트 파일 | `ImageProcessorTests.cs` |
| 테스트 케이스 수 | **45개** (`[Fact]` / `[Theory]`) — 신규 8개 메서드 각 1-2개 시나리오 포함 |

---

## SWR 참조

| SWR ID | 대상 메서드 | 내용 | 안전 등급 |
|--------|-------------|------|-----------|
| SWR-IP-020 | `ProcessAsync` | DICOM/Raw 파일 로드 및 픽셀 추출 | Functional |
| SWR-IP-022 | `ApplyWindowLevel`, `Zoom` | 창/레벨 LUT 적용 | Functional |
| SWR-IP-024 | `Zoom` | 업스케일 Bicubic, 다운스케일 Area Average | Functional |
| SWR-IP-026 | `Pan` | PanOffsetX/Y 누적, 픽셀 데이터 불변 | Functional |
| SWR-IP-027 | `Rotate`, `Flip` | 90/180/270° 회전, 수평/수직 반전 | Functional |
| SWR-IP-036 | `ProcessAsync` | ROI 통계용 RawPixelData16 보존 (0~65535) | Functional |
| SWR-IP-039 | `ApplyGainOffsetCorrection` | Gain/Offset 캘리브레이션 보정 | Safety-related (HAZ-RAD, HAZ-SW) |
| SWR-IP-041 | `ApplyNoiseReduction` | 적응형 노이즈 제거 | Safety-related (HAZ-RAD) |
| SWR-IP-043 | `ApplyEdgeEnhancement` | Unsharp mask 엣지 강화 | Functional |
| SWR-IP-045 | `ApplyScatterCorrection` | 산란선 보정 | Safety-related (HAZ-RAD) |
| SWR-IP-047 | `ApplyAutoTrimming` | 테두리 마스킹 | Functional |
| SWR-IP-049 | `ApplyBlackMask` | Black Mask On/Off 토글 | Functional |
| SWR-IP-050 | `ApplyClahe` | CLAHE 명암 강화 | Functional |
| SWR-IP-052 | `ApplyBrightnessOffset` | 밝기 오프셋 연산 | Functional |
