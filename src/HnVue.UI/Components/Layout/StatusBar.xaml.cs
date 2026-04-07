using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HnVue.UI.Components.Layout;

/// <summary>
/// System status level for status bar indicators.
/// </summary>
public enum SystemStatus
{
    /// <summary>System is operational (green)</summary>
    Online,
    /// <summary>System is busy (blue)</summary>
    Busy,
    /// <summary>System has warning (amber)</summary>
    Warning,
    /// <summary>System is offline/error (red)</summary>
    Offline,
    /// <summary>System is blocked (orange)</summary>
    Blocked
}

/// <summary>
/// Status bar component for displaying system status and information.
/// </summary>
public class StatusBar : Control
{
    /// <summary>Identifies the StatusMessage dependency property.</summary>
    public static readonly DependencyProperty StatusMessageProperty =
        DependencyProperty.Register(
            nameof(StatusMessage),
            typeof(string),
            typeof(StatusBar),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the ConnectionStatus dependency property.</summary>
    public static readonly DependencyProperty ConnectionStatusProperty =
        DependencyProperty.Register(
            nameof(ConnectionStatus),
            typeof(string),
            typeof(StatusBar),
            new PropertyMetadata("Connected"));

    /// <summary>Identifies the ShowTime dependency property.</summary>
    public static readonly DependencyProperty ShowTimeProperty =
        DependencyProperty.Register(
            nameof(ShowTime),
            typeof(bool),
            typeof(StatusBar),
            new PropertyMetadata(true));

    /// <summary>Identifies the Message dependency property.</summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(StatusBar),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the StatusItems dependency property.</summary>
    public static readonly DependencyProperty StatusItemsProperty =
        DependencyProperty.Register(
            nameof(StatusItems),
            typeof(ObservableCollection<StatusBarItem>),
            typeof(StatusBar),
            new PropertyMetadata(null));

    /// <summary>Identifies the InfoItems dependency property.</summary>
    public static readonly DependencyProperty InfoItemsProperty =
        DependencyProperty.Register(
            nameof(InfoItems),
            typeof(ObservableCollection<StatusBarItem>),
            typeof(StatusBar),
            new PropertyMetadata(null));

    static StatusBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBar),
            new FrameworkPropertyMetadata(typeof(StatusBar)));
    }

    /// <summary>
    /// Initializes a new instance of the StatusBar class.
    /// Sets default Height to 32 per design system specification.
    /// </summary>
    public StatusBar()
    {
        Height = 32;
        StatusItems = new ObservableCollection<StatusBarItem>();
        InfoItems = new ObservableCollection<StatusBarItem>();
    }

    /// <summary>Gets or sets the primary status message text.</summary>
    public string StatusMessage
    {
        get => (string)GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    /// <summary>Gets or sets the connection status label (e.g. "Connected", "Offline").</summary>
    public string ConnectionStatus
    {
        get => (string)GetValue(ConnectionStatusProperty);
        set => SetValue(ConnectionStatusProperty, value);
    }

    /// <summary>Gets or sets whether to display the current time in the status bar.</summary>
    public bool ShowTime
    {
        get => (bool)GetValue(ShowTimeProperty);
        set => SetValue(ShowTimeProperty, value);
    }

    /// <summary>Gets or sets the center message text.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets the collection of left-side status items.</summary>
    public ObservableCollection<StatusBarItem> StatusItems
    {
        get => (ObservableCollection<StatusBarItem>)GetValue(StatusItemsProperty)!;
        private init => SetValue(StatusItemsProperty, value);
    }

    /// <summary>Gets the collection of right-side info items.</summary>
    public ObservableCollection<StatusBarItem> InfoItems
    {
        get => (ObservableCollection<StatusBarItem>)GetValue(InfoItemsProperty)!;
        private init => SetValue(InfoItemsProperty, value);
    }
}

/// <summary>
/// Represents a single status bar item with text and status indicator.
/// </summary>
public class StatusBarItem : ContentControl
{
    /// <summary>Identifies the Status dependency property.</summary>
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(SystemStatus),
            typeof(StatusBarItem),
            new PropertyMetadata(SystemStatus.Online, OnStatusChanged));

    /// <summary>Identifies the IsPulse dependency property.</summary>
    public static readonly DependencyProperty IsPulseProperty =
        DependencyProperty.Register(
            nameof(IsPulse),
            typeof(bool),
            typeof(StatusBarItem),
            new PropertyMetadata(false));

    /// <summary>Identifies the StatusBrush dependency property.</summary>
    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(
            nameof(StatusBrush),
            typeof(Brush),
            typeof(StatusBarItem),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(46, 213, 115))));

    static StatusBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBarItem),
            new FrameworkPropertyMetadata(typeof(StatusBarItem)));
    }

    /// <summary>Gets or sets the status level.</summary>
    public SystemStatus Status
    {
        get => (SystemStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Gets or sets whether the indicator should pulse.</summary>
    public bool IsPulse
    {
        get => (bool)GetValue(IsPulseProperty);
        set => SetValue(IsPulseProperty, value);
    }

    /// <summary>Gets the brush for the status indicator.</summary>
    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        private set => SetValue(StatusBrushProperty, value);
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBarItem item)
        {
            item.StatusBrush = item.Status switch
            {
                SystemStatus.Online => new SolidColorBrush(Color.FromRgb(46, 213, 115)),
                SystemStatus.Busy => new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                SystemStatus.Warning => new SolidColorBrush(Color.FromRgb(255, 165, 2)),
                SystemStatus.Offline => new SolidColorBrush(Color.FromRgb(255, 71, 87)),
                SystemStatus.Blocked => new SolidColorBrush(Color.FromRgb(255, 109, 0)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
    }
}
