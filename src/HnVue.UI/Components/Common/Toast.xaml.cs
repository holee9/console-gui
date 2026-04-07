using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HnVue.UI.Components.Common;

/// <summary>
/// Toast notification severity levels.
/// </summary>
public enum ToastSeverity
{
    /// <summary>Informational message (blue)</summary>
    Info,
    /// <summary>Success message (green)</summary>
    Success,
    /// <summary>Warning message (amber)</summary>
    Warning,
    /// <summary>Error message (red)</summary>
    Error
}

/// <summary>
/// Represents a single toast notification item.
/// </summary>
public class ToastItem : ViewModelBase
{
    private string _message = string.Empty;
    private ToastSeverity _severity = ToastSeverity.Info;

    /// <summary>
    /// Gets or sets the message to display.
    /// </summary>
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    /// <summary>
    /// Gets or sets the severity level.
    /// </summary>
    public ToastSeverity Severity
    {
        get => _severity;
        set
        {
            if (SetProperty(ref _severity, value))
            {
                UpdateAppearance();
            }
        }
    }

    /// <summary>
    /// Gets the background brush based on severity.
    /// </summary>
    public Brush BackgroundBrush { get; private set; } = Brushes.Transparent;

    /// <summary>
    /// Gets the border brush based on severity.
    /// </summary>
    public Brush BorderBrush { get; private set; } = Brushes.Transparent;

    /// <summary>
    /// Gets the icon template based on severity.
    /// </summary>
    public ControlTemplate? IconTemplate { get; private set; }

    /// <summary>
    /// Gets or sets the command to close the toast.
    /// </summary>
    public ICommand? CloseCommand { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds before auto-dismiss (0 = no auto-dismiss).
    /// </summary>
    public int Duration { get; set; } = 3000;

    private void UpdateAppearance()
    {
        var app = Application.Current;
        if (app?.TryFindResource($"DS2026.Brush.{Severity}") is Brush brush)
        {
            BackgroundBrush = brush;
            BorderBrush = brush;
            BorderBrush.Opacity = 0.3;
        }

        if (app?.TryFindResource($"DS2026.Toast.Icon.{Severity}") is ControlTemplate template)
        {
            IconTemplate = template;
        }
    }
}

/// <summary>
/// Toast notification service for managing toast notifications.
/// </summary>
public class ToastService : ViewModelBase
{
    private readonly ObservableCollection<ToastItem> _toasts = new();

    /// <summary>
    /// Gets the collection of active toasts.
    /// </summary>
    public IReadOnlyCollection<ToastItem> Toasts => _toasts;

    /// <summary>
    /// Shows a toast notification with the specified message and severity.
    /// </summary>
    public void Show(string message, ToastSeverity severity = ToastSeverity.Info, int duration = 3000)
    {
        var toast = new ToastItem
        {
            Message = message,
            Severity = severity,
            Duration = duration,
        };
        toast.CloseCommand = new RelayCommand(_ => Remove(toast));

        _toasts.Add(toast);

        if (duration > 0)
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(duration)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Remove(toast);
            };
            timer.Start();
        }
    }

    /// <summary>
    /// Shows an informational toast.
    /// </summary>
    public void ShowInfo(string message, int duration = 3000) => Show(message, ToastSeverity.Info, duration);

    /// <summary>
    /// Shows a success toast.
    /// </summary>
    public void ShowSuccess(string message, int duration = 3000) => Show(message, ToastSeverity.Success, duration);

    /// <summary>
    /// Shows a warning toast.
    /// </summary>
    public void ShowWarning(string message, int duration = 5000) => Show(message, ToastSeverity.Warning, duration);

    /// <summary>
    /// Shows an error toast (requires user action to dismiss).
    /// </summary>
    public void ShowError(string message) => Show(message, ToastSeverity.Error, 0);

    /// <summary>
    /// Removes the specified toast.
    /// </summary>
    public void Remove(ToastItem toast) => _toasts.Remove(toast);

    /// <summary>
    /// Clears all toasts.
    /// </summary>
    public void ClearAll() => _toasts.Clear();
}
