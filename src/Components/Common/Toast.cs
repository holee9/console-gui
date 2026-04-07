using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HnVue.UI.Components.Common
{
    /// <summary>
    /// A toast notification component for displaying temporary messages.
    /// Supports medical device UI feedback requirements with appropriate duration.
    /// </summary>
    public class Toast : ContentControl
    {
        private System.Windows.Threading.DispatcherTimer? _dismissTimer;

        static Toast()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Toast),
                new FrameworkPropertyMetadata(typeof(Toast)));
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(Toast),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// The message text to display.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty ToastTypeProperty =
            DependencyProperty.Register(
                nameof(ToastType),
                typeof(ToastType),
                typeof(Toast),
                new PropertyMetadata(ToastType.Info));

        /// <summary>
        /// The type of toast notification, determines icon and color.
        /// </summary>
        public ToastType ToastType
        {
            get => (ToastType)GetValue(ToastTypeProperty);
            set => SetValue(ToastTypeProperty, value);
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(
                nameof(Duration),
                typeof(TimeSpan),
                typeof(Toast),
                new PropertyMetadata(TimeSpan.FromSeconds(3)));

        /// <summary>
        /// How long the toast remains visible before auto-dismissing.
        /// </summary>
        public TimeSpan Duration
        {
            get => (TimeSpan)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(Toast),
                new PropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        /// Indicates whether the toast is currently visible.
        /// </summary>
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Toast toast && e.NewValue is bool isOpen)
            {
                if (isOpen)
                {
                    toast.StartDismissTimer();
                }
                else
                {
                    toast.StopDismissTimer();
                }
            }
        }

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(
                nameof(CloseCommand),
                typeof(ICommand),
                typeof(Toast),
                new PropertyMetadata(null));

        /// <summary>
        /// Command executed when the close button is clicked or toast is dismissed.
        /// </summary>
        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        private void StartDismissTimer()
        {
            StopDismissTimer();
            _dismissTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = Duration
            };
            _dismissTimer.Tick += (s, e) =>
            {
                IsOpen = false;
                StopDismissTimer();
            };
            _dismissTimer.Start();
        }

        private void StopDismissTimer()
        {
            _dismissTimer?.Stop();
            _dismissTimer = null;
        }
    }

    /// <summary>
    /// Toast notification types following medical device UI feedback standards.
    /// </summary>
    public enum ToastType
    {
        /// <summary>
        /// Success message (green) - operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Warning message (amber) - caution, requires attention.
        /// </summary>
        Warning,

        /// <summary>
        /// Error message (red) - operation failed, requires action.
        /// </summary>
        Error,

        /// <summary>
        /// Info message (blue) - general information.
        /// </summary>
        Info
    }
}
