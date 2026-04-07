using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Medical;

/// <summary>
/// Patient information card component for medical imaging workflow.
/// Displays patient demographics and study information.
/// </summary>
public class PatientInfoCard : Control
{
    /// <summary>Identifies the PatientName dependency property.</summary>
    public static readonly DependencyProperty PatientNameProperty =
        DependencyProperty.Register(
            nameof(PatientName),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the PatientId dependency property.</summary>
    public static readonly DependencyProperty PatientIdProperty =
        DependencyProperty.Register(
            nameof(PatientId),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the BirthDate dependency property.</summary>
    public static readonly DependencyProperty BirthDateProperty =
        DependencyProperty.Register(
            nameof(BirthDate),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the Sex dependency property.</summary>
    public static readonly DependencyProperty SexProperty =
        DependencyProperty.Register(
            nameof(Sex),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the Age dependency property.</summary>
    public static readonly DependencyProperty AgeProperty =
        DependencyProperty.Register(
            nameof(Age),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(null));

    /// <summary>Identifies the AccessionNumber dependency property.</summary>
    public static readonly DependencyProperty AccessionNumberProperty =
        DependencyProperty.Register(
            nameof(AccessionNumber),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(null));

    /// <summary>Identifies the StudyDate dependency property.</summary>
    public static readonly DependencyProperty StudyDateProperty =
        DependencyProperty.Register(
            nameof(StudyDate),
            typeof(string),
            typeof(PatientInfoCard),
            new PropertyMetadata(null));

    /// <summary>Identifies the IsEmergency dependency property.</summary>
    public static readonly DependencyProperty IsEmergencyProperty =
        DependencyProperty.Register(
            nameof(IsEmergency),
            typeof(bool),
            typeof(PatientInfoCard),
            new PropertyMetadata(false));

    static PatientInfoCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PatientInfoCard),
            new FrameworkPropertyMetadata(typeof(PatientInfoCard)));
    }

    /// <summary>Gets or sets the patient's full name.</summary>
    public string PatientName
    {
        get => (string)GetValue(PatientNameProperty);
        set => SetValue(PatientNameProperty, value);
    }

    /// <summary>Gets or sets the patient ID.</summary>
    public string PatientId
    {
        get => (string)GetValue(PatientIdProperty);
        set => SetValue(PatientIdProperty, value);
    }

    /// <summary>Gets or sets the patient's birth date.</summary>
    public string BirthDate
    {
        get => (string)GetValue(BirthDateProperty);
        set => SetValue(BirthDateProperty, value);
    }

    /// <summary>Gets or sets the patient's sex (M/F/O).</summary>
    public string Sex
    {
        get => (string)GetValue(SexProperty);
        set => SetValue(SexProperty, value);
    }

    /// <summary>Gets or sets the patient's age.</summary>
    public string? Age
    {
        get => (string?)GetValue(AgeProperty);
        set => SetValue(AgeProperty, value);
    }

    /// <summary>Gets or sets the accession number.</summary>
    public string? AccessionNumber
    {
        get => (string?)GetValue(AccessionNumberProperty);
        set => SetValue(AccessionNumberProperty, value);
    }

    /// <summary>Gets or sets the study date.</summary>
    public string? StudyDate
    {
        get => (string?)GetValue(StudyDateProperty);
        set => SetValue(StudyDateProperty, value);
    }

    /// <summary>Gets or sets whether this is an emergency case.</summary>
    public bool IsEmergency
    {
        get => (bool)GetValue(IsEmergencyProperty);
        set => SetValue(IsEmergencyProperty, value);
    }
}
