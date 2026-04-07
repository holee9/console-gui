using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Common
{
    /// <summary>
    /// Enhanced TextBox for medical device UI with validation support.
    /// </summary>
    public class MedicalTextBox : TextBox
    {
        static MedicalTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MedicalTextBox),
                new FrameworkPropertyMetadata(typeof(MedicalTextBox)));
        }

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register(
                nameof(HasError),
                typeof(bool),
                typeof(MedicalTextBox),
                new PropertyMetadata(false, OnHasErrorChanged));

        /// <summary>
        /// Indicates the input has validation errors.
        /// </summary>
        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MedicalTextBox textBox)
            {
                // Update Tag for error state in template trigger
                textBox.Tag = textBox.HasError ? "Error" : null;
            }
        }

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(PlaceholderText),
                typeof(string),
                typeof(MedicalTextBox),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Text to display when the TextBox is empty.
        /// </summary>
        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty InputTypeProperty =
            DependencyProperty.Register(
                nameof(InputType),
                typeof(InputType),
                typeof(MedicalTextBox),
                new PropertyMetadata(InputType.Text));

        public InputType InputType
        {
            get => (InputType)GetValue(InputTypeProperty);
            set => SetValue(InputTypeProperty, value);
        }
    }

    public enum InputType
    {
        Text,
        Numeric,
        PatientId,
        AccessionNumber,
        Date
    }
}
