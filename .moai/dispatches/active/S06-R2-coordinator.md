# DISPATCH: Coordinator — S06 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S06 R2 — Detector 어댑터 DI 등록 준비 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R2-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

Team B가 AbyzSdk 어댑터와 HME libxd2 어댑터를 구현 중.
완료 후 DI 등록을 조건부로 변경해야 함. 이번 라운드는 준비 작업.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
```

---

## Task 1 (P2): Detector DI 등록 — 조건부 전환 준비

현재 `src/HnVue.App/App.xaml.cs` 에서 `DetectorSimulator`로 등록됨.
SDK DLL 존재 여부에 따라 조건부 등록 로직 추가.

### 구현 방식
```csharp
// App.xaml.cs — 조건부 Detector 등록
private static void RegisterDetectorService(IServiceCollection services)
{
    var sdkPath = Path.Combine(AppContext.BaseDirectory, "AbyzSdk.dll");
    var hmePath = Path.Combine(AppContext.BaseDirectory, "libxd2.dll");

    if (File.Exists(sdkPath))
    {
        // 자사 AbyzSdk 어댑터
        services.AddSingleton<IDetectorInterface>(new OwnDetectorAdapter(
            new OwnDetectorConfig(Host: "192.168.1.100", Port: 8888)));
    }
    else if (File.Exists(hmePath))
    {
        // HME 어댑터 (Team B 구현 완료 후 활성화)
        // services.AddSingleton<IDetectorInterface>(new HmeDetectorAdapter(...));
        services.AddSingleton<IDetectorInterface, DetectorSimulator>(); // 임시
    }
    else
    {
        // 개발/테스트용 시뮬레이터
        services.AddSingleton<IDetectorInterface, DetectorSimulator>();
    }
}
```

### 검증
```bash
dotnet build src/HnVue.App/ --configuration Release 2>&1 | tail -5
dotnet test tests/integration/ 2>&1 | tail -10
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.App/App.xaml.cs
git commit -m "feat(coordinator): Detector DI 조건부 등록 준비 (#issue)"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DI 조건부 등록 (P2) | NOT_STARTED | -- | App.xaml.cs |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
