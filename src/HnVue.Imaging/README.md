# HnVue.Imaging

> 의료 영상 처리 (이미지 프로세싱)

## 목적

X-ray 원시 이미지에 대한 전처리 및 후처리 파이프라인을 제공합니다. 윈도우 레벨링, 필터링 등의 영상 처리를 수행합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `ImageProcessor` | IImageProcessor 구현체 — 영상 처리 파이프라인 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- ProcessedImage, ProcessingParameters 모델은 HnVue.Common에 정의
- IEC 62304 Class B — 환자 진단에 영향
