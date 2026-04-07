using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Common
{
    /// <summary>
    /// Base button class for medical device UI with safety features.
    /// Ensures touch targets meet accessibility requirements (minimum 44x44px).
    /// </summary>
    public class MedicalButton : Button
    {
        static MedicalButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MedicalButton),
                new FrameworkPropertyMetadata(typeof(MedicalButton)));
        }

        public static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register(
                nameof(ButtonType),
                typeof(ButtonType),
                typeof(MedicalButton),
                new PropertyMetadata(ButtonType.Primary));

        public ButtonType ButtonType
        {
            get => (ButtonType)GetValue(ButtonTypeProperty);
            set => SetValue(ButtonTypeProperty, value);
        }

        public static readonly DependencyProperty IsCriticalProperty =
            DependencyProperty.Register(
                nameof(IsCritical),
                typeof(bool),
                typeof(MedicalButton),
                new PropertyMetadata(false));

        /// <summary>
        /// Indicates this button performs a critical action requiring confirmation.
        /// Critical buttons should have additional visual warnings and confirmation dialogs.
        /// </summary>
        public bool IsCritical
        {
            get => (bool)GetValue(IsCriticalProperty);
            set => SetValue(IsCriticalProperty, value);
        }
    }

    public enum ButtonType
    {
        Primary,
        Secondary,
        Danger,
        Success
    }
}
