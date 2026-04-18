using System.ComponentModel;
using FluentAssertions;
using HnVue.UI.Components.Common;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for ViewModelBase class.
/// Tests INotifyPropertyChanged implementation and SetProperty method.
/// </summary>
public class ViewModelBaseTests
{
    // ====================================================================
    // Test ViewModel Implementation
    // ====================================================================

    private class TestViewModel : ViewModelBase
    {
        private string _stringProperty = string.Empty;
        private int _intProperty;
        private bool _boolProperty;

        public string StringProperty
        {
            get => _stringProperty;
            set => SetProperty(ref _stringProperty, value);
        }

        public int IntProperty
        {
            get => _intProperty;
            set => SetProperty(ref _intProperty, value);
        }

        public bool BoolProperty
        {
            get => _boolProperty;
            set => SetProperty(ref _boolProperty, value);
        }
    }

    // ====================================================================
    // INotifyPropertyChanged Tests
    // ====================================================================

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_Implements_INotifyPropertyChanged()
    {
        var viewModel = new TestViewModel();
        viewModel.Should().BeAssignableTo<INotifyPropertyChanged>();
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_StringValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        viewModel.StringProperty = "NewValue";

        propertyName.Should().Be(nameof(TestViewModel.StringProperty));
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_IntValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        viewModel.IntProperty = 42;

        propertyName.Should().Be(nameof(TestViewModel.IntProperty));
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_BoolValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        viewModel.BoolProperty = true;

        propertyName.Should().Be(nameof(TestViewModel.BoolProperty));
    }

    // ====================================================================
    // SetProperty Return Value Tests
    // ====================================================================

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_DifferentValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        int eventCount = 0;

        viewModel.PropertyChanged += (s, e) => eventCount++;
        viewModel.StringProperty = "NewValue";

        eventCount.Should().Be(1);
        viewModel.StringProperty.Should().Be("NewValue");
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_SameValue_DoesNotChangeProperty()
    {
        var viewModel = new TestViewModel { StringProperty = "Value" };

        viewModel.StringProperty = "Value";

        viewModel.StringProperty.Should().Be("Value");
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_SameValue_DoesNotRaisePropertyChanged()
    {
        var viewModel = new TestViewModel { StringProperty = "Value" };
        int eventCount = 0;

        viewModel.PropertyChanged += (s, e) => eventCount++;

        viewModel.StringProperty = "Value";

        eventCount.Should().Be(0);
    }

    // ====================================================================
    // Property Value Update Tests
    // ====================================================================

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_UpdatesPropertyValue()
    {
        var viewModel = new TestViewModel();

        viewModel.StringProperty = "Updated";

        viewModel.StringProperty.Should().Be("Updated");
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_MultipleUpdates_EachRaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        int eventCount = 0;

        viewModel.PropertyChanged += (s, e) => eventCount++;

        viewModel.StringProperty = "First";
        viewModel.StringProperty = "Second";
        viewModel.StringProperty = "Third";

        eventCount.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_SetProperty_DefaultValue_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel { IntProperty = 10 };
        string? propertyName = null;

        viewModel.PropertyChanged += (s, e) => propertyName = e.PropertyName;

        viewModel.IntProperty = 0;

        propertyName.Should().Be(nameof(TestViewModel.IntProperty));
    }

    // ====================================================================
    // PropertyChanged Event Tests
    // ====================================================================

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_PropertyChanged_Sender_IsViewModel()
    {
        var viewModel = new TestViewModel();
        object? sender = null;

        viewModel.PropertyChanged += (s, e) => sender = s;

        viewModel.StringProperty = "Value";

        sender.Should().Be(viewModel);
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_Property_CanHaveMultipleSubscribers()
    {
        var viewModel = new TestViewModel();
        int count1 = 0, count2 = 0, count3 = 0;

        viewModel.PropertyChanged += (s, e) => count1++;
        viewModel.PropertyChanged += (s, e) => count2++;
        viewModel.PropertyChanged += (s, e) => count3++;

        viewModel.StringProperty = "Value";

        count1.Should().Be(1);
        count2.Should().Be(1);
        count3.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "ViewModels")]
    [Trait("ViewModel", "ViewModelBase")]
    public void ViewModelBase_PropertyChanged_CanUnsubscribe()
    {
        var viewModel = new TestViewModel();
        int eventCount = 0;

        PropertyChangedEventHandler handler = (s, e) => eventCount++;
        viewModel.PropertyChanged += handler;
        viewModel.PropertyChanged -= handler;

        viewModel.StringProperty = "Value";

        eventCount.Should().Be(0);
    }
}
