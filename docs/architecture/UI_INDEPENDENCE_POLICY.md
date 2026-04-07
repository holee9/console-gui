# UI Layer Independence Policy

## Overview

HnVue UI 레이어(HnVue.UI, HnVue.UI.ViewModels, HnVue.UI.Contracts)는 비즈니스 모듈에 직접 의존하지 않아야 합니다.
이 경계는 `tests/HnVue.Architecture.Tests/UILayerArchitectureTests.cs` 에 있는 아키텍처 테스트로 자동 검증됩니다.

---

## Architecture Boundary Rules

### Allowed Dependencies

| Project | Allowed References |
|---|---|
| `HnVue.UI` | `HnVue.Common`, `HnVue.UI.Contracts`, MahApps.Metro, CommunityToolkit.Mvvm, LiveChartsCore |
| `HnVue.UI.ViewModels` | `HnVue.UI.Contracts`, `HnVue.Common`, CommunityToolkit.Mvvm |
| `HnVue.UI.Contracts` | `HnVue.Common` (models and enums only) |
| `HnVue.App` | Everything — this is the DI composition root |

### Forbidden Dependencies

The following namespaces must never be referenced from `HnVue.UI`, `HnVue.UI.ViewModels`, or `HnVue.UI.Contracts`:

- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Workflow`
- `HnVue.Imaging`
- `HnVue.Dicom`
- `HnVue.Dose`
- `HnVue.PatientManagement`
- `HnVue.Incident`
- `HnVue.Update`
- `HnVue.SystemAdmin`
- `HnVue.CDBurning`

---

## Why This Boundary Exists

HnVue는 IEC 62304 Class B 의료기기 소프트웨어입니다. UI 레이어를 비즈니스 모듈로부터 격리하면 다음 이점이 있습니다.

1. **독립적 테스트**: UI를 실제 하드웨어나 DB 없이 단독으로 테스트할 수 있습니다.
2. **교체 용이성**: UI 구현을 변경해도 비즈니스 로직이 영향을 받지 않습니다.
3. **IEC 62304 추적성**: UI와 도메인 로직의 변경 이력이 분리되어 추적이 명확합니다.
4. **빌드 시간 단축**: UI 프로젝트 변경이 전체 비즈니스 모듈 재빌드를 유발하지 않습니다.

---

## How to Add New UI Components

### Step 1: Define the contract interface

`HnVue.UI.Contracts`에 인터페이스를 추가합니다.

```csharp
// src/HnVue.UI.Contracts/ViewModels/INewFeatureViewModel.cs
namespace HnVue.UI.Contracts.ViewModels;

public interface INewFeatureViewModel
{
    string Title { get; }
    ICommand ExecuteCommand { get; }
}
```

필요한 이벤트 인자나 데이터 모델은 `HnVue.UI.Contracts.Events` 또는 `HnVue.Common.Models`에 정의합니다.

### Step 2: Implement the ViewModel

`HnVue.UI.ViewModels`에 구현체를 추가합니다. 비즈니스 서비스는 생성자 파라미터로 인터페이스만 받습니다.

```csharp
// src/HnVue.UI.ViewModels/ViewModels/NewFeatureViewModel.cs
namespace HnVue.UI.ViewModels.ViewModels;

public sealed partial class NewFeatureViewModel : ObservableObject, INewFeatureViewModel
{
    private readonly ISomeBusinessService _service; // interface from HnVue.UI.Contracts

    public NewFeatureViewModel(ISomeBusinessService service)
    {
        _service = service;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    // Explicit interface implementation for ICommand bridge pattern
    ICommand INewFeatureViewModel.ExecuteCommand => ExecuteCommand;

    [RelayCommand]
    private async Task ExecuteAsync() { /* ... */ }
}
```

### Step 3: Register in DI (HnVue.App only)

의존성 등록은 반드시 `HnVue.App` 프로젝트의 DI 등록 코드에서만 수행합니다.

```csharp
// src/HnVue.App/DependencyInjection/UIServiceRegistration.cs
services.AddTransient<INewFeatureViewModel, NewFeatureViewModel>();
services.AddSingleton<ISomeBusinessService, ConcreteBusinessService>(); // HnVue.SomeModule
```

### Step 4: Use in View

`HnVue.UI` 뷰에서는 인터페이스 타입으로만 ViewModel을 참조합니다.

```csharp
// src/HnVue.UI/Components/NewFeatureView.xaml.cs
public partial class NewFeatureView : UserControl
{
    public NewFeatureView(INewFeatureViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

---

## Enforcement

`tests/HnVue.Architecture.Tests/UILayerArchitectureTests.cs`에 세 개의 아키텍처 테스트가 정의되어 있습니다.

| Test | What It Checks |
|---|---|
| `UI_Should_Not_Depend_On_Business_Modules` | HnVue.UI 어셈블리가 금지된 비즈니스 네임스페이스를 참조하지 않음 |
| `ViewModels_Should_Only_Depend_On_Contracts_And_Common` | HnVue.UI.ViewModels가 비즈니스 모듈을 직접 참조하지 않음 |
| `Contracts_Should_Have_No_Implementation_Dependencies` | HnVue.UI.Contracts가 구현 모듈이나 HnVue.UI를 참조하지 않음 |

이 테스트들은 `NetArchTest.Rules` 패키지를 사용하며, CI 빌드 및 `dotnet test` 실행 시 자동으로 검증됩니다.

```bash
dotnet test tests/HnVue.Architecture.Tests/HnVue.Architecture.Tests.csproj
```

테스트가 실패하면 금지된 의존성이 추가된 것입니다. 오류 메시지에 어떤 네임스페이스가 위반되었는지 표시됩니다. 비즈니스 로직에 접근해야 한다면 항상 인터페이스를 `HnVue.UI.Contracts`에 추가하는 방식으로 해결합니다.

---

## Frequently Asked Questions

**Q: ViewModel에서 데이터베이스 조회 결과가 필요합니다.**

`HnVue.UI.Contracts`에 조회 인터페이스(`IPatientQueryService` 등)를 정의하고, 구현체는 `HnVue.Data` 또는 해당 모듈에 작성한 뒤 `HnVue.App`에서 등록합니다.

**Q: HnVue.Security의 현재 사용자 정보가 필요합니다.**

`HnVue.Common.Abstractions`에 이미 `ISecurityContext` 인터페이스가 있습니다. 이 인터페이스를 생성자 주입으로 사용합니다.

**Q: 아키텍처 테스트가 오탐(false positive)을 발생시킵니다.**

`NetArchTest.Rules`는 어셈블리의 IL 메타데이터를 분석합니다. 간접 전이 의존성(transitive)은 검사하지 않고 직접 `ProjectReference`만 검사합니다. 오탐이 발생하면 실제로 금지된 참조가 추가된 것이므로 `ProjectReference`를 점검합니다.
