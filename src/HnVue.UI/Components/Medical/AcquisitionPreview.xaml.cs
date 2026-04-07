using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HnVue.UI.Components.Medical;

/// <summary>
/// Dose level categories for exposure safety indication.
/// </summary>
public enum DoseLevel
{
    /// <summary>Normal dose (green)</summary>
    Normal,
    /// <summary>Elevated dose (amber)</summary>
    Elevated,
    /// <summary>High dose (red)</summary>
    High
}

/// <summary>
/// Acquisition preview component for real-time medical imaging display.
/// Displays live detector feed, exposure status, and dose information.
/// </summary>
public class AcquisitionPreview : Control
{
    /// <summary>Identifies the ImageSource dependency property.</summary>
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(AcquisitionPreview),
            new PropertyMetadata(null));

    /// <summary>Identifies the IsLive dependency property.</summary>
    public static readonly DependencyProperty IsLiveProperty =
        DependencyProperty.Register(
            nameof(IsLive),
            typeof(bool),
            typeof(AcquisitionPreview),
            new PropertyMetadata(false));

    /// <summary>Identifies the IsExposing dependency property.</summary>
    public static readonly DependencyProperty IsExposingProperty =
        DependencyProperty.Register(
            nameof(IsExposing),
            typeof(bool),
            typeof(AcquisitionPreview),
            new PropertyMetadata(false));

    /// <summary>Identifies the ExposureInfo dependency property.</summary>
    public static readonly DependencyProperty ExposureInfoProperty =
        DependencyProperty.Register(
            nameof(ExposureInfo),
            typeof(string),
            typeof(AcquisitionPreview),
            new PropertyMetadata("kV: -- mA: --"));

    /// <summary>Identifies the Resolution dependency property.</summary>
    public static readonly DependencyProperty ResolutionProperty =
        DependencyProperty.Register(
            nameof(Resolution),
            typeof(string),
            typeof(AcquisitionPreview),
            new PropertyMetadata("0 x 0"));

    /// <summary>Identifies the BodyPart dependency property.</summary>
    public static readonly DependencyProperty BodyPartProperty =
        DependencyProperty.Register(
            nameof(BodyPart),
            typeof(string),
            typeof(AcquisitionPreview),
            new PropertyMetadata("--"));

    /// <summary>Identifies the Projection dependency property.</summary>
    public static readonly DependencyProperty ProjectionProperty =
        DependencyProperty.Register(
            nameof(Projection),
            typeof(string),
            typeof(AcquisitionPreview),
            new PropertyMetadata("--"));

    /// <summary>Identifies the DoseInfo dependency property.</summary>
    public static readonly DependencyProperty DoseInfoProperty =
        DependencyProperty.Register(
            nameof(DoseInfo),
            typeof(string),
            typeof(AcquisitionPreview),
            new PropertyMetadata("0.00 mGy"));

    /// <summary>Identifies the DoseLevel dependency property.</summary>
    public static readonly DependencyProperty DoseLevelProperty =
        DependencyProperty.Register(
            nameof(DoseLevel),
            typeof(DoseLevel),
            typeof(AcquisitionPreview),
            new PropertyMetadata(DoseLevel.Normal, OnDoseLevelChanged));

    /// <summary>Identifies the DoseIndicatorBackground dependency property.</summary>
    public static readonly DependencyProperty DoseIndicatorBackgroundProperty =
        DependencyProperty.Register(
            nameof(DoseIndicatorBackground),
            typeof(Brush),
            typeof(AcquisitionPreview),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(46, 213, 115))));

    /// <summary>Identifies the ShowCrosshair dependency property.</summary>
    public static readonly DependencyProperty ShowCrosshairProperty =
        DependencyProperty.Register(
            nameof(ShowCrosshair),
            typeof(bool),
            typeof(AcquisitionPreview),
            new PropertyMetadata(true));

    /// <summary>Identifies the ExposureAngle dependency property (for animation).</summary>
    public static readonly DependencyProperty ExposureAngleProperty =
        DependencyProperty.Register(
            nameof(ExposureAngle),
            typeof(double),
            typeof(AcquisitionPreview),
            new PropertyMetadata(0.0));

    static AcquisitionPreview()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AcquisitionPreview),
            new FrameworkPropertyMetadata(typeof(AcquisitionPreview)));
    }

    /// <summary>Gets or sets the current image source from detector.</summary>
    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>Gets or sets whether the preview is live.</summary>
    public bool IsLive
    {
        get => (bool)GetValue(IsLiveProperty);
        set => SetValue(IsLiveProperty, value);
    }

    /// <summary>Gets or sets whether exposure is in progress.</summary>
    public bool IsExposing
    {
        get => (bool)GetValue(IsExposingProperty);
        set
        {
            SetValue(IsExposingProperty, value);
            StartExposureAnimation(value);
        }
    }

    /// <summary>Gets or sets the exposure parameters display.</summary>
    public string ExposureInfo
    {
        get => (string)GetValue(ExposureInfoProperty);
        set => SetValue(ExposureInfoProperty, value);
    }

    /// <summary>Gets or sets the resolution display.</summary>
    public string Resolution
    {
        get => (string)GetValue(ResolutionProperty);
        set => SetValue(ResolutionProperty, value);
    }

    /// <summary>Gets or sets the body part being imaged.</summary>
    public string BodyPart
    {
        get => (string)GetValue(BodyPartProperty);
        set => SetValue(BodyPartProperty, value);
    }

    /// <summary>Gets or sets the projection (AP/PA/Lateral/etc).</summary>
    public string Projection
    {
        get => (string)GetValue(ProjectionProperty);
        set => SetValue(ProjectionProperty, value);
    }

    /// <summary>Gets or sets the dose information display.</summary>
    public string DoseInfo
    {
        get => (string)GetValue(DoseInfoProperty);
        set => SetValue(DoseInfoProperty, value);
    }

    /// <summary>Gets or sets the current dose level.</summary>
    public DoseLevel DoseLevel
    {
        get => (DoseLevel)GetValue(DoseLevelProperty);
        set => SetValue(DoseLevelProperty, value);
    }

    /// <summary>Gets the background brush for the dose indicator.</summary>
    public Brush DoseIndicatorBackground
    {
        get => (Brush)GetValue(DoseIndicatorBackgroundProperty);
        private set => SetValue(DoseIndicatorBackgroundProperty, value);
    }

    /// <summary>Gets or sets whether to show center crosshair.</summary>
    public bool ShowCrosshair
    {
        get => (bool)GetValue(ShowCrosshairProperty);
        set => SetValue(ShowCrosshairProperty, value);
    }

    /// <summary>Gets or sets the exposure animation angle.</summary>
    public double ExposureAngle
    {
        get => (double)GetValue(ExposureAngleProperty);
        set => SetValue(ExposureAngleProperty, value);
    }

    private static void OnDoseLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AcquisitionPreview preview)
        {
            preview.DoseIndicatorBackground = preview.DoseLevel switch
            {
                DoseLevel.Normal => new SolidColorBrush(Color.FromRgb(46, 213, 115)),
                DoseLevel.Elevated => new SolidColorBrush(Color.FromRgb(255, 165, 2)),
                DoseLevel.High => new SolidColorBrush(Color.FromRgb(255, 71, 87)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }

    private void StartExposureAnimation(bool isExposing)
    {
        if (isExposing)
        {
            var animation = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(1000))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            BeginAnimation(ExposureAngleProperty, animation);
        }
        else
        {
            BeginAnimation(ExposureAngleProperty, null);
            ExposureAngle = 0;
        }
    }
}
