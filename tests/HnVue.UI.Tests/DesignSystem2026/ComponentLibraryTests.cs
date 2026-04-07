using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using FluentAssertions;
using HnVue.UI.Components.Common;
using HnVue.UI.Components.Layout;
using HnVue.UI.Components.Medical;
using Xunit;
using HnVue.UI.Tests;
using StatusBar = HnVue.UI.Components.Layout.StatusBar;

namespace HnVue.UI.Tests.DesignSystem2026;

/// <summary>
/// Unit tests for Design System 2026 component library.
/// Tests dependency properties, default values, and basic behavior.
/// </summary>
public class ComponentLibraryTests
{
    [Fact]
    public void PatientInfoCard_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var card = new PatientInfoCard();

            // Assert
            card.PatientName.Should().BeEmpty();
            card.PatientId.Should().BeEmpty();
            card.BirthDate.Should().BeEmpty();
            card.Sex.Should().BeEmpty();
            card.Age.Should().BeNull();
            card.AccessionNumber.Should().BeNull();
            card.StudyDate.Should().BeNull();
            card.IsEmergency.Should().BeFalse();
        });
    }

    [Fact]
    public void PatientInfoCard_SetProperties_UpdatesValues()
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var card = new PatientInfoCard();

            // Act
            card.PatientName = "Hong Gildong";
            card.PatientId = "P12345";
            card.BirthDate = "1988-01-01";
            card.Sex = "M";
            card.Age = "36";
            card.IsEmergency = true;

            // Assert
            card.PatientName.Should().Be("Hong Gildong");
            card.PatientId.Should().Be("P12345");
            card.BirthDate.Should().Be("1988-01-01");
            card.Sex.Should().Be("M");
            card.Age.Should().Be("36");
            card.IsEmergency.Should().BeTrue();
        });
    }

    [Fact]
    public void StudyThumbnail_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var thumbnail = new StudyThumbnail();

            // Assert
            thumbnail.ThumbnailSource.Should().BeNull();
            thumbnail.SeriesDescription.Should().Be("Unknown Series");
            thumbnail.ImageCount.Should().Be(1);
            thumbnail.PlaceholderText.Should().Be("No Image");
            thumbnail.IsSelected.Should().BeFalse();
            thumbnail.Status.Should().Be(StudyStatus.Pending);
        });
    }

    [Theory]
    [InlineData(StudyStatus.Pending, "#A0A0B0")]
    [InlineData(StudyStatus.InProgress, "#1E90FF")]
    [InlineData(StudyStatus.Completed, "#2ED573")]
    [InlineData(StudyStatus.Warning, "#FFA502")]
    [InlineData(StudyStatus.Error, "#FF4757")]
    public void StudyThumbnail_Status_SetsCorrectBrush(StudyStatus status, string expectedColorHex)
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var thumbnail = new StudyThumbnail();
            // Force a status change to trigger OnStatusChanged callback even when value equals default
            thumbnail.Status = StudyStatus.Error;

            // Act
            thumbnail.Status = status;

            // Assert
            thumbnail.StatusBrush.Should().NotBeNull();
            thumbnail.StatusBrush.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)thumbnail.StatusBrush;
            Color expected = ColorConverter.ConvertFromString(expectedColorHex) is Color color ? color : default;
            brush.Color.Should().Be(expected);
        });
    }

    [Fact]
    public void AcquisitionPreview_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var preview = new AcquisitionPreview();

            // Assert
            preview.ImageSource.Should().BeNull();
            preview.IsLive.Should().BeFalse();
            preview.IsExposing.Should().BeFalse();
            preview.ExposureInfo.Should().Be("kV: -- mA: --");
            preview.Resolution.Should().Be("0 x 0");
            preview.BodyPart.Should().Be("--");
            preview.Projection.Should().Be("--");
            preview.DoseInfo.Should().Be("0.00 mGy");
            preview.DoseLevel.Should().Be(DoseLevel.Normal);
            preview.ShowCrosshair.Should().BeTrue();
        });
    }

    [Theory]
    [InlineData(DoseLevel.Normal, "#2ED573")]
    [InlineData(DoseLevel.Elevated, "#FFA502")]
    [InlineData(DoseLevel.High, "#FF4757")]
    public void AcquisitionPreview_DoseLevel_SetsCorrectBrush(DoseLevel level, string expectedColorHex)
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var preview = new AcquisitionPreview();

            // Act
            preview.DoseLevel = level;

            // Assert
            preview.DoseIndicatorBackground.Should().NotBeNull();
            preview.DoseIndicatorBackground.Should().BeOfType<SolidColorBrush>();
            var brush = (SolidColorBrush)preview.DoseIndicatorBackground;
            Color expected = ColorConverter.ConvertFromString(expectedColorHex) is Color color ? color : default;
            brush.Color.Should().Be(expected);
        });
    }

    [Fact]
    public void Modal_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var modal = new Modal();

            // Assert
            modal.IsOpen.Should().BeFalse();
            modal.Title.Should().BeEmpty();
            modal.HasCloseButton.Should().BeTrue();
            modal.ShowFooter.Should().BeTrue();
            modal.MaxWidth.Should().Be(600);
        });
    }

    [Fact]
    public void Modal_CanSetProperties()
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var modal = new Modal();
            var command = new RelayCommand(_ => { });

            // Act
            modal.Title = "Test Modal";
            modal.CloseCommand = command;
            modal.MaxWidth = 800;

            // Assert
            modal.Title.Should().Be("Test Modal");
            modal.CloseCommand.Should().Be(command);
            modal.MaxWidth.Should().Be(800);
        });
    }

    [Fact]
    public void StatusBar_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var statusBar = new StatusBar();

            // Assert
            statusBar.Message.Should().BeEmpty();
            statusBar.StatusItems.Should().NotBeNull();
            statusBar.InfoItems.Should().NotBeNull();
            statusBar.StatusItems.Should().BeEmpty();
            statusBar.InfoItems.Should().BeEmpty();
        });
    }

    [Fact]
    public void StatusBar_CanAddItems()
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var statusBar = new StatusBar();

            // Act
            statusBar.Message = "Ready";
            statusBar.StatusItems.Add(new StatusBarItem { Content = "System: Online" });
            statusBar.InfoItems.Add(new StatusBarItem { Content = "v1.0.0" });

            // Assert
            statusBar.Message.Should().Be("Ready");
            statusBar.StatusItems.Should().HaveCount(1);
            statusBar.InfoItems.Should().HaveCount(1);
        });
    }

    [Fact]
    public void Header_DefaultValues_AreCorrect()
    {
        StaRunner.Run(() =>
        {
            // Arrange & Act
            var header = new Header();

            // Assert
            header.Title.Should().BeEmpty();
            header.LeftContent.Should().BeNull();
            header.ActionButtons.Should().NotBeNull();
            header.ActionButtons.Should().BeEmpty();
        });
    }

    [Fact]
    public void Header_CanSetProperties()
    {
        StaRunner.Run(() =>
        {
            // Arrange
            var header = new Header();

            // Act
            header.Title = "Test Screen";
            var button = new Button { Content = "Save" };
            header.ActionButtons.Add(button);

            // Assert
            header.Title.Should().Be("Test Screen");
            header.ActionButtons.Should().HaveCount(1);
        });
    }

    [Fact]
    public void ToastService_Show_AddsToast()
    {
        // Arrange
        var service = new ToastService();

        // Act
        service.Show("Test message", ToastSeverity.Info);

        // Assert
        service.Toasts.Should().HaveCount(1);
        service.Toasts.First().Message.Should().Be("Test message");
        service.Toasts.First().Severity.Should().Be(ToastSeverity.Info);
    }

    [Fact]
    public void ToastService_Remove_RemovesToast()
    {
        // Arrange
        var service = new ToastService();
        service.Show("Test message");

        // Act
        var toast = service.Toasts.First();
        service.Remove(toast);

        // Assert
        service.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void ToastService_ClearAll_RemovesAllToasts()
    {
        // Arrange
        var service = new ToastService();
        service.Show("Message 1");
        service.Show("Message 2");
        service.Show("Message 3");

        // Act
        service.ClearAll();

        // Assert
        service.Toasts.Should().BeEmpty();
    }

    [Fact]
    public void ToastService_ConvenienceMethods_WorkCorrectly()
    {
        // Arrange
        var service = new ToastService();

        // Act
        service.ShowInfo("Info message");
        service.ShowSuccess("Success message");
        service.ShowWarning("Warning message");
        service.ShowError("Error message");

        // Assert
        service.Toasts.Should().HaveCount(4);
        service.Toasts.ElementAt(0).Severity.Should().Be(ToastSeverity.Info);
        service.Toasts.ElementAt(1).Severity.Should().Be(ToastSeverity.Success);
        service.Toasts.ElementAt(2).Severity.Should().Be(ToastSeverity.Warning);
        service.Toasts.ElementAt(3).Severity.Should().Be(ToastSeverity.Error);
    }
}
