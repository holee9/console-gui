using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HnVue.UI.Components.Medical;

/// <summary>
/// Study status for thumbnail indicators.
/// </summary>
public enum StudyStatus
{
    /// <summary>Study is pending/queued (gray)</summary>
    Pending,
    /// <summary>Study is in progress (blue)</summary>
    InProgress,
    /// <summary>Study is completed (green)</summary>
    Completed,
    /// <summary>Study has warning (amber)</summary>
    Warning,
    /// <summary>Study has error (red)</summary>
    Error
}

/// <summary>
/// Study thumbnail component for displaying medical image previews.
/// </summary>
public class StudyThumbnail : Control
{
    /// <summary>Identifies the ThumbnailSource dependency property.</summary>
    public static readonly DependencyProperty ThumbnailSourceProperty =
        DependencyProperty.Register(
            nameof(ThumbnailSource),
            typeof(ImageSource),
            typeof(StudyThumbnail),
            new PropertyMetadata(null));

    /// <summary>Identifies the SeriesDescription dependency property.</summary>
    public static readonly DependencyProperty SeriesDescriptionProperty =
        DependencyProperty.Register(
            nameof(SeriesDescription),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata("Unknown Series"));

    /// <summary>Identifies the ImageCount dependency property.</summary>
    public static readonly DependencyProperty ImageCountProperty =
        DependencyProperty.Register(
            nameof(ImageCount),
            typeof(int),
            typeof(StudyThumbnail),
            new PropertyMetadata(1, OnImageCountChanged));

    /// <summary>Identifies the PlaceholderText dependency property.</summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata("No Image"));

    /// <summary>Identifies the StudyInstanceUid dependency property.</summary>
    public static readonly DependencyProperty StudyInstanceUidProperty =
        DependencyProperty.Register(
            nameof(StudyInstanceUid),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the ThumbnailImage dependency property.</summary>
    public static readonly DependencyProperty ThumbnailImageProperty =
        DependencyProperty.Register(
            nameof(ThumbnailImage),
            typeof(ImageSource),
            typeof(StudyThumbnail),
            new PropertyMetadata(null));

    /// <summary>Identifies the StudyDate dependency property.</summary>
    public static readonly DependencyProperty StudyDateProperty =
        DependencyProperty.Register(
            nameof(StudyDate),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata(null));

    /// <summary>Identifies the StudyDescription dependency property.</summary>
    public static readonly DependencyProperty StudyDescriptionProperty =
        DependencyProperty.Register(
            nameof(StudyDescription),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the Modality dependency property.</summary>
    public static readonly DependencyProperty ModalityProperty =
        DependencyProperty.Register(
            nameof(Modality),
            typeof(string),
            typeof(StudyThumbnail),
            new PropertyMetadata("CR"));

    /// <summary>Identifies the StudyStatus dependency property.</summary>
    public static readonly DependencyProperty StudyStatusProperty =
        DependencyProperty.Register(
            nameof(StudyStatus),
            typeof(StudyStatus),
            typeof(StudyThumbnail),
            new PropertyMetadata(StudyStatus.Pending, OnStudyStatusChanged));

    /// <summary>Identifies the IsSelected dependency property.</summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(StudyThumbnail),
            new PropertyMetadata(false));

    /// <summary>Identifies the Status dependency property.</summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(StudyStatus),
            typeof(StudyThumbnail),
            new PropertyMetadata(StudyStatus.Pending, OnStatusChanged));

    /// <summary>Identifies the StatusBrush dependency property.</summary>
    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(
            nameof(StatusBrush),
            typeof(Brush),
            typeof(StudyThumbnail),
            new PropertyMetadata(Brushes.Gray));

    /// <summary>Identifies the Command dependency property.</summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StudyThumbnail),
            new PropertyMetadata(null));

    /// <summary>Identifies the CommandParameter dependency property.</summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StudyThumbnail),
            new PropertyMetadata(null));

    static StudyThumbnail()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StudyThumbnail),
            new FrameworkPropertyMetadata(typeof(StudyThumbnail)));
    }

    /// <summary>Gets or sets the thumbnail image source.</summary>
    public ImageSource? ThumbnailSource
    {
        get => (ImageSource?)GetValue(ThumbnailSourceProperty);
        set => SetValue(ThumbnailSourceProperty, value);
    }

    /// <summary>Gets or sets the series description text.</summary>
    public string SeriesDescription
    {
        get => (string)GetValue(SeriesDescriptionProperty);
        set => SetValue(SeriesDescriptionProperty, value);
    }

    /// <summary>Gets or sets the number of images in the series.</summary>
    public int ImageCount
    {
        get => (int)GetValue(ImageCountProperty);
        set => SetValue(ImageCountProperty, value);
    }

    /// <summary>Gets or sets the placeholder text when no image is available.</summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Gets or sets the DICOM Study Instance UID.</summary>
    public string StudyInstanceUid
    {
        get => (string)GetValue(StudyInstanceUidProperty);
        set => SetValue(StudyInstanceUidProperty, value);
    }

    /// <summary>Gets or sets the thumbnail image source.</summary>
    public ImageSource? ThumbnailImage
    {
        get => (ImageSource?)GetValue(ThumbnailImageProperty);
        set => SetValue(ThumbnailImageProperty, value);
    }

    /// <summary>Gets or sets the study date string (nullable).</summary>
    public string? StudyDate
    {
        get => (string?)GetValue(StudyDateProperty);
        set => SetValue(StudyDateProperty, value);
    }

    /// <summary>Gets or sets the study description text.</summary>
    public string StudyDescription
    {
        get => (string)GetValue(StudyDescriptionProperty);
        set => SetValue(StudyDescriptionProperty, value);
    }

    /// <summary>Gets or sets the imaging modality (e.g. CR, CT, MR).</summary>
    public string Modality
    {
        get => (string)GetValue(ModalityProperty);
        set => SetValue(ModalityProperty, value);
    }

    /// <summary>Gets or sets the study workflow status.</summary>
    public StudyStatus StudyStatus
    {
        get => (StudyStatus)GetValue(StudyStatusProperty);
        set => SetValue(StudyStatusProperty, value);
    }

    /// <summary>Gets or sets whether this thumbnail is selected.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Gets or sets the study status.</summary>
    public StudyStatus Status
    {
        get => (StudyStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Gets the status brush based on the current status.</summary>
    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        private set => SetValue(StatusBrushProperty, value);
    }

    /// <summary>Gets or sets the command to execute on click.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Gets or sets the parameter for the command.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private static void OnImageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Could trigger additional visual updates based on count
    }

    private static void OnStudyStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // StudyStatus mirrors Status for backward compatibility
        if (d is StudyThumbnail thumb)
        {
            thumb.SetValue(StatusProperty, thumb.StudyStatus);
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StudyThumbnail thumb)
        {
            thumb.StatusBrush = thumb.Status switch
            {
                StudyStatus.Pending => new SolidColorBrush(Color.FromRgb(160, 160, 176)),
                StudyStatus.InProgress => new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                StudyStatus.Completed => new SolidColorBrush(Color.FromRgb(46, 213, 115)),
                StudyStatus.Warning => new SolidColorBrush(Color.FromRgb(255, 165, 2)),
                StudyStatus.Error => new SolidColorBrush(Color.FromRgb(255, 71, 87)),
                _ => Brushes.Gray
            };
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        Command?.Execute(CommandParameter);
    }
}
