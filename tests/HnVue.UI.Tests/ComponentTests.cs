using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Xunit;
using HnVue.UI.Components.Common;
using HnVue.UI.Components.Layout;
using HnVue.UI.Components.Medical;
using HnVue.UI.Converters;

namespace HnVue.UI.Tests
{
    /// <summary>
    /// Unit tests for WPF component library.
    /// Tests dependency properties, default values, and state changes.
    /// </summary>
    public class ComponentTests
    {
        // ====================================================================
        // COMMON COMPONENT TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "MedicalButton")]
        public void MedicalButton_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var button = new MedicalButton();

                // Assert
                button.ButtonType.Should().Be(ButtonType.Primary);
                button.IsCritical.Should().BeFalse();
                // Note: Height is NaN before layout measurement in WPF desktop apps
            });
        }

        [Theory]
        [InlineData(ButtonType.Primary)]
        [InlineData(ButtonType.Secondary)]
        [InlineData(ButtonType.Danger)]
        [InlineData(ButtonType.Success)]
        [Trait("Category", "Components")]
        [Trait("Component", "MedicalButton")]
        public void MedicalButton_ButtonType_SetsCorrectly(ButtonType type)
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var button = new MedicalButton();

                // Act
                button.ButtonType = type;

                // Assert
                button.ButtonType.Should().Be(type);
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "MedicalTextBox")]
        public void MedicalTextBox_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var textBox = new MedicalTextBox();

                // Assert
                textBox.HasError.Should().BeFalse();
                textBox.PlaceholderText.Should().BeEmpty();
                textBox.InputType.Should().Be(InputType.Text);
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "MedicalTextBox")]
        public void MedicalTextBox_HasError_UpdatesTag()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var textBox = new MedicalTextBox();

                // Act
                textBox.HasError = true;

                // Assert
                textBox.Tag.Should().Be("Error");
            });
        }

        [Theory]
        [InlineData(InputType.Text)]
        [InlineData(InputType.Numeric)]
        [InlineData(InputType.PatientId)]
        [InlineData(InputType.AccessionNumber)]
        [InlineData(InputType.Date)]
        [Trait("Category", "Components")]
        [Trait("Component", "MedicalTextBox")]
        public void MedicalTextBox_InputType_SetsCorrectly(InputType type)
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var textBox = new MedicalTextBox();

                // Act
                textBox.InputType = type;

                // Assert
                textBox.InputType.Should().Be(type);
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Card")]
        public void Card_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var card = new Card();

                // Assert
                card.Header.Should().BeNull();
                card.Footer.Should().BeNull();
                card.CornerRadius.Should().Be(new CornerRadius(12));
                card.Padding.Should().Be(new Thickness(16));
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Card")]
        public void Card_CornerRadius_SetsCorrectly()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var card = new Card();
                var expected = new CornerRadius(8);

                // Act
                card.CornerRadius = expected;

                // Assert
                card.CornerRadius.Should().Be(expected);
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Modal")]
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
        [Trait("Category", "Components")]
        [Trait("Component", "Modal")]
        public void Modal_IsOpen_TogglesCorrectly()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var modal = new Modal();

                // Act
                modal.IsOpen = true;

                // Assert
                modal.IsOpen.Should().BeTrue();
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void Toast_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var toastItem = new ToastItem();

            // Assert
            toastItem.Message.Should().BeEmpty();
            toastItem.Severity.Should().Be(ToastSeverity.Info);
            toastItem.Duration.Should().Be(3000);
        }

        [Theory]
        [InlineData(ToastSeverity.Success)]
        [InlineData(ToastSeverity.Warning)]
        [InlineData(ToastSeverity.Error)]
        [InlineData(ToastSeverity.Info)]
        [Trait("Category", "Components")]
        [Trait("Component", "Toast")]
        public void Toast_Severity_SetsCorrectly(ToastSeverity severity)
        {
            // Arrange
            var toastItem = new ToastItem();

            // Act
            toastItem.Severity = severity;

            // Assert
            toastItem.Severity.Should().Be(severity);
        }

        // ====================================================================
        // MEDICAL COMPONENT TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "PatientInfoCard")]
        public void PatientInfoCard_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var card = new PatientInfoCard();

                // Assert
                card.PatientId.Should().BeEmpty();
                card.PatientName.Should().BeEmpty();
                card.BirthDate.Should().BeEmpty();
                card.Age.Should().BeNull();
                card.Sex.Should().BeEmpty();
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "PatientInfoCard")]
        public void PatientInfoCard_Sex_SetsCorrectly()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var card = new PatientInfoCard();

                // Act
                card.Sex = "M";

                // Assert
                card.Sex.Should().Be("M");
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "PatientInfoCard")]
        public void PatientInfoCard_IsEmergency_SetsCorrectly()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var card = new PatientInfoCard();

                // Act
                card.IsEmergency = true;

                // Assert
                card.IsEmergency.Should().BeTrue();
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "StudyThumbnail")]
        public void StudyThumbnail_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var thumbnail = new StudyThumbnail();

                // Assert
                thumbnail.StudyInstanceUid.Should().BeEmpty();
                thumbnail.ThumbnailImage.Should().BeNull();
                thumbnail.StudyDate.Should().BeNull();
                thumbnail.StudyDescription.Should().BeEmpty();
                thumbnail.Modality.Should().Be("CR");
                thumbnail.StudyStatus.Should().Be(StudyStatus.Pending);
                thumbnail.IsSelected.Should().BeFalse();
            });
        }

        [Theory]
        [InlineData(StudyStatus.Pending)]
        [InlineData(StudyStatus.InProgress)]
        [InlineData(StudyStatus.Completed)]
        [InlineData(StudyStatus.Error)]
        [Trait("Category", "Components")]
        [Trait("Component", "StudyThumbnail")]
        public void StudyThumbnail_Status_SetsCorrectly(StudyStatus status)
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var thumbnail = new StudyThumbnail();

                // Act
                thumbnail.StudyStatus = status;

                // Assert
                thumbnail.StudyStatus.Should().Be(status);
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "AcquisitionPreview")]
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
                preview.BodyPart.Should().Be("--");
                preview.Projection.Should().Be("--");
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "AcquisitionPreview")]
        public void AcquisitionPreview_BodyPart_SetsCorrectly()
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var preview = new AcquisitionPreview();

                // Act
                preview.BodyPart = "Chest";

                // Assert
                preview.BodyPart.Should().Be("Chest");
            });
        }

        [Theory]
        [InlineData("Chest")]
        [InlineData("Abdomen")]
        [InlineData("Hand")]
        [InlineData("--")]
        [Trait("Category", "Components")]
        [Trait("Component", "AcquisitionPreview")]
        public void AcquisitionPreview_BodyPartString_SetsCorrectly(string part)
        {
            StaRunner.Run(() =>
            {
                // Arrange
                var preview = new AcquisitionPreview();

                // Act
                preview.BodyPart = part;

                // Assert
                preview.BodyPart.Should().Be(part);
            });
        }

        // ====================================================================
        // LAYOUT COMPONENT TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Sidebar")]
        public void Sidebar_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var sidebar = new Sidebar();

                // Assert
                sidebar.Logo.Should().BeNull();
                sidebar.MenuItems.Should().BeNull();
                sidebar.FooterContent.Should().BeNull();
                sidebar.Width.Should().Be(240);
                sidebar.IsExpanded.Should().BeTrue();
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "Header")]
        public void Header_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var header = new Header();

                // Assert
                header.Title.Should().BeEmpty();
                header.LeftContent.Should().BeNull();
                header.RightContent.Should().BeNull();
                header.Height.Should().Be(56);
                header.ShowBorder.Should().BeTrue();
            });
        }

        [Fact]
        [Trait("Category", "Components")]
        [Trait("Component", "StatusBar")]
        public void StatusBar_DefaultValues_AreCorrect()
        {
            StaRunner.Run(() =>
            {
                // Arrange & Act
                var statusBar = new StatusBar();

                // Assert
                statusBar.StatusMessage.Should().BeEmpty();
                statusBar.ConnectionStatus.Should().Be("Connected");
                statusBar.ShowTime.Should().BeTrue();
                statusBar.Height.Should().Be(32);
            });
        }

        // ====================================================================
        // CONVERTER TESTS
        // ====================================================================

        [Fact]
        [Trait("Category", "Converters")]
        public void BoolToVisibilityConverter_Convert_WorksCorrectly()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act & Assert
            converter.Convert(true, null, null, null).Should().Be(Visibility.Visible);
            converter.Convert(false, null, null, null).Should().Be(Visibility.Collapsed);
        }

        [Fact]
        [Trait("Category", "Converters")]
        public void BoolToVisibilityConverter_Inverted_WorksCorrectly()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter { IsInverted = true };

            // Act & Assert
            converter.Convert(true, null, null, null).Should().Be(Visibility.Collapsed);
            converter.Convert(false, null, null, null).Should().Be(Visibility.Visible);
        }

        [Fact]
        [Trait("Category", "Converters")]
        public void NullToVisibilityConverter_Convert_WorksCorrectly()
        {
            // Arrange
            var converter = new NullToVisibilityConverter();

            // Act & Assert
            converter.Convert(null, null, null, null).Should().Be(Visibility.Collapsed);
            converter.Convert("value", null, null, null).Should().Be(Visibility.Visible);
        }

        [Fact]
        [Trait("Category", "Converters")]
        public void BoolToVisibilityConverter_WithInvertParameter_WorksCorrectly()
        {
            // Arrange
            var converter = new BoolToVisibilityConverter();

            // Act & Assert - Invert via ConverterParameter
            converter.Convert(true, null, "Invert", null).Should().Be(Visibility.Collapsed);
            converter.Convert(false, null, "Invert", null).Should().Be(Visibility.Visible);
        }
    }
}
