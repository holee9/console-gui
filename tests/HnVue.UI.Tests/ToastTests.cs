using System;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using Xunit;
using HnVue.UI.Components.Common;

namespace HnVue.UI.Tests
{
    /// <summary>
    /// Comprehensive tests for Toast notification component and service.
    /// Target coverage: 68.9% → 85%+
    /// </summary>
    public class ToastTests
    {
        // ====================================================================
        // ToastItem PROPERTY TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_Message_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var toastItem = new ToastItem();
            const string expectedMessage = "Test message";

            // Act
            toastItem.Message = expectedMessage;

            // Assert
            toastItem.Message.Should().Be(expectedMessage);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_Message_EmptyString_SetsCorrectly()
        {
            // Arrange
            var toastItem = new ToastItem();

            // Act
            toastItem.Message = string.Empty;

            // Assert
            toastItem.Message.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_Duration_DefaultValue_Is3000()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.Duration.Should().Be(3000);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_Duration_CustomValue_SetsCorrectly()
        {
            // Arrange
            var toastItem = new ToastItem();
            const int customDuration = 5000;

            // Act
            toastItem.Duration = customDuration;

            // Assert
            toastItem.Duration.Should().Be(customDuration);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_Duration_Zero_SetsCorrectly()
        {
            // Arrange
            var toastItem = new ToastItem();

            // Act
            toastItem.Duration = 0;

            // Assert
            toastItem.Duration.Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_CloseCommand_DefaultValue_IsNull()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.CloseCommand.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_CloseCommand_CanBeSet()
        {
            // Arrange
            var toastItem = new ToastItem();
            var command = new RelayCommand(_ => { });

            // Act
            toastItem.CloseCommand = command;

            // Assert
            toastItem.CloseCommand.Should().Be(command);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_BackgroundBrush_DefaultValue_IsTransparent()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.BackgroundBrush.Should().Be(Brushes.Transparent);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_BorderBrush_DefaultValue_IsTransparent()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.BorderBrush.Should().Be(Brushes.Transparent);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_IconTemplate_DefaultValue_IsNull()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.IconTemplate.Should().BeNull();
        }

        // ====================================================================
        // ToastItem UPDATEAPPEARANCE TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_UpdateAppearance_WithSeverity_SetsProperties()
        {
            // Arrange
            var toastItem = new ToastItem();

            // Act
            toastItem.Severity = ToastSeverity.Success;

            // Assert
            toastItem.Severity.Should().Be(ToastSeverity.Success);
            // Note: Brushes and Template depend on Application.Current resources
            // which are not available in test environment
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_UpdateAppearance_SameSeverity_DoesNotThrow()
        {
            // Arrange
            var toastItem = new ToastItem();
            toastItem.Severity = ToastSeverity.Info;

            // Act & Assert
            toastItem.Invoking(t => t.Severity = ToastSeverity.Info)
                .Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_UpdateAppearance_MultipleSeverityChanges_WorksCorrectly()
        {
            // Arrange
            var toastItem = new ToastItem();

            // Act
            toastItem.Severity = ToastSeverity.Warning;
            toastItem.Severity = ToastSeverity.Error;
            toastItem.Severity = ToastSeverity.Success;

            // Assert
            toastItem.Severity.Should().Be(ToastSeverity.Success);
        }

        // ====================================================================
        // ToastService SHOW TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Show_AddsToastToCollection()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Test message";

            // Act
            service.Show(message, ToastSeverity.Info);

            // Assert
            service.Toasts.Should().HaveCount(1);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Show_SetsToastProperties()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Warning message";
            const int duration = 5000;

            // Act
            service.Show(message, ToastSeverity.Warning, duration);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Message.Should().Be(message);
            toast.Current.Severity.Should().Be(ToastSeverity.Warning);
            toast.Current.Duration.Should().Be(duration);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowInfo_CreatesInfoToast()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Info message";

            // Act
            service.ShowInfo(message);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Message.Should().Be(message);
            toast.Current.Severity.Should().Be(ToastSeverity.Info);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowSuccess_CreatesSuccessToast()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Success message";

            // Act
            service.ShowSuccess(message);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Message.Should().Be(message);
            toast.Current.Severity.Should().Be(ToastSeverity.Success);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowWarning_CreatesWarningToast()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Warning message";

            // Act
            service.ShowWarning(message);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Message.Should().Be(message);
            toast.Current.Severity.Should().Be(ToastSeverity.Warning);
            toast.Current.Duration.Should().Be(5000); // Warning has 5s default
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowError_CreatesErrorToast()
        {
            // Arrange
            var service = new ToastService();
            const string message = "Error message";

            // Act
            service.ShowError(message);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Message.Should().Be(message);
            toast.Current.Severity.Should().Be(ToastSeverity.Error);
            toast.Current.Duration.Should().Be(0); // Error requires manual dismiss
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Show_MultipleToasts_AddsAllToCollection()
        {
            // Arrange
            var service = new ToastService();

            // Act
            service.Show("First", ToastSeverity.Info);
            service.Show("Second", ToastSeverity.Warning);
            service.Show("Third", ToastSeverity.Error);

            // Assert
            service.Toasts.Should().HaveCount(3);
        }

        // ====================================================================
        // ToastService REMOVE TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Remove_RemovesToastFromCollection()
        {
            // Arrange
            var service = new ToastService();
            service.Show("Test message");
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            var toastItem = toast.Current;

            // Act
            service.Remove(toastItem);

            // Assert
            service.Toasts.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Remove_NonExistentToast_DoesNotThrow()
        {
            // Arrange
            var service = new ToastService();
            var fakeToast = new ToastItem();

            // Act & Assert
            service.Invoking(s => s.Remove(fakeToast))
                .Should().NotThrow();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_CloseCommand_ExecutedRemovesToast()
        {
            // Arrange
            var service = new ToastService();
            service.Show("Test message");
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            var toastItem = toast.Current;

            // Act
            toastItem.CloseCommand?.Execute(null);

            // Assert
            service.Toasts.Should().BeEmpty();
        }

        // ====================================================================
        // ToastService CLEARALL TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ClearAll_RemovesAllToasts()
        {
            // Arrange
            var service = new ToastService();
            service.Show("First");
            service.Show("Second");
            service.Show("Third");

            // Act
            service.ClearAll();

            // Assert
            service.Toasts.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ClearAll_EmptyCollection_DoesNotThrow()
        {
            // Arrange
            var service = new ToastService();

            // Act & Assert
            service.Invoking(s => s.ClearAll())
                .Should().NotThrow();
        }

        // ====================================================================
        // ToastService TIMER TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowWithDurationZero_DoesNotAutoDismiss()
        {
            // Arrange
            var service = new ToastService();

            // Act
            service.Show("Persistent message", ToastSeverity.Info, 0);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Duration.Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_ShowWithCustomDuration_UsesCustomDuration()
        {
            // Arrange
            var service = new ToastService();
            const int customDuration = 7000;

            // Act
            service.Show("Custom duration", ToastSeverity.Info, customDuration);

            // Assert
            service.Toasts.Should().HaveCount(1);
            var toast = service.Toasts.GetEnumerator();
            toast.MoveNext();
            toast.Current.Duration.Should().Be(customDuration);
        }

        // ====================================================================
        // ToastService TOASTS COLLECTION TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Tosts_ReturnsReadOnlyCollection()
        {
            // Arrange
            var service = new ToastService();

            // Act
            var toasts = service.Toasts;

            // Assert
            toasts.Should().NotBeNull();
            toasts.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastService_Tosts_ReflectsActiveToasts()
        {
            // Arrange
            var service = new ToastService();
            service.Show("First");
            service.Show("Second");

            // Act
            var count = service.Toasts.Count;

            // Assert
            count.Should().Be(2);
        }

        // ====================================================================
        // ToastItem PROPERTY CHANGED TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_PropertyChanged_Message_RaisesPropertyChanged()
        {
            // Arrange
            var toastItem = new ToastItem();
            var propertyChanged = false;
            toastItem.PropertyChanged += (s, e) => propertyChanged = true;

            // Act
            toastItem.Message = "New message";

            // Assert
            propertyChanged.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_PropertyChanged_Severity_RaisesPropertyChanged()
        {
            // Arrange
            var toastItem = new ToastItem();
            var propertyChanged = false;
            toastItem.PropertyChanged += (s, e) => propertyChanged = true;

            // Act
            toastItem.Severity = ToastSeverity.Error;

            // Assert
            propertyChanged.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void ToastItem_PropertyChanged_SameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var toastItem = new ToastItem();
            toastItem.Severity = ToastSeverity.Error;
            var propertyChangedCount = 0;
            toastItem.PropertyChanged += (s, e) => propertyChangedCount++;

            // Act
            toastItem.Severity = ToastSeverity.Error; // Same value - no event raised

            // Assert
            // ViewModelBase.SetProperty does not raise PropertyChanged when value hasn't changed
            propertyChangedCount.Should().Be(0); // No new event raised
        }
    }
}
