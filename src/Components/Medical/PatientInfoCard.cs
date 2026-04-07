using System;
using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Medical
{
    /// <summary>
    /// Displays patient information with medical device safety considerations.
    /// Critical for patient identification and preventing medical errors.
    /// </summary>
    public class PatientInfoCard : ContentControl
    {
        static PatientInfoCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PatientInfoCard),
                new FrameworkPropertyMetadata(typeof(PatientInfoCard)));
        }

        public static readonly DependencyProperty PatientIdProperty =
            DependencyProperty.Register(
                nameof(PatientId),
                typeof(string),
                typeof(PatientInfoCard),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Unique patient identifier for safe patient matching.
        /// </summary>
        public string PatientId
        {
            get => (string)GetValue(PatientIdProperty);
            set => SetValue(PatientIdProperty, value);
        }

        public static readonly DependencyProperty PatientNameProperty =
            DependencyProperty.Register(
                nameof(PatientName),
                typeof(string),
                typeof(PatientInfoCard),
                new PropertyMetadata(string.Empty));

        public string PatientName
        {
            get => (string)GetValue(PatientNameProperty);
            set => SetValue(PatientNameProperty, value);
        }

        public static readonly DependencyProperty DateOfBirthProperty =
            DependencyProperty.Register(
                nameof(DateOfBirth),
                typeof(DateTime?),
                typeof(PatientInfoCard),
                new PropertyMetadata(null, OnDateOfBirthChanged));

        public DateTime? DateOfBirth
        {
            get => (DateTime?)GetValue(DateOfBirthProperty);
            set => SetValue(DateOfBirthProperty, value);
        }

        public static readonly DependencyProperty AgeProperty =
            DependencyProperty.Register(
                nameof(Age),
                typeof(string),
                typeof(PatientInfoCard),
                new PropertyMetadata(string.Empty));

        public string Age
        {
            get => (string)GetValue(AgeProperty);
            set => SetValue(AgeProperty, value);
        }

        public static readonly DependencyProperty SexProperty =
            DependencyProperty.Register(
                nameof(Sex),
                typeof(PatientSex),
                typeof(PatientInfoCard),
                new PropertyMetadata(PatientSex.Unknown));

        public PatientSex Sex
        {
            get => (PatientSex)GetValue(SexProperty);
            set => SetValue(SexProperty, value);
        }

        private static void OnDateOfBirthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PatientInfoCard card && e.NewValue is DateTime dob)
            {
                // Calculate age automatically
                int age = DateTime.Today.Year - dob.Year;
                if (DateTime.Today < dob.AddYears(age)) age--;
                card.Age = $"{age}세";
            }
        }
    }

    /// <summary>
    /// Patient sex enumeration following HL7/FHIR standards.
    /// </summary>
    public enum PatientSex
    {
        Unknown,
        Male,    // M
        Female,  // F
        Other    // O
    }
}
