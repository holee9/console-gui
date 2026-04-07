using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HnVue.UI.Components.Layout
{
    /// <summary>
    /// A status bar component for displaying system status and information.
    /// Provides real-time feedback on system state for medical device monitoring.
    /// </summary>
    public class StatusBar : Control
    {
        private DispatcherTimer? _timeUpdateTimer;

        static StatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBar),
                new FrameworkPropertyMetadata(typeof(StatusBar)));
        }

        public StatusBar()
        {
            InitializeTimer();
        }

        public static readonly DependencyProperty StatusMessageProperty =
            DependencyProperty.Register(
                nameof(StatusMessage),
                typeof(string),
                typeof(StatusBar),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Primary status message displayed on the left.
        /// </summary>
        public string StatusMessage
        {
            get => (string)GetValue(StatusMessageProperty);
            set => SetValue(StatusMessageProperty, value);
        }

        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register(
                nameof(ConnectionStatus),
                typeof(string),
                typeof(StatusBar),
                new PropertyMetadata("Connected"));

        /// <summary>
        /// Connection status indicator (e.g., "Connected", "Disconnected").
        /// </summary>
        public string ConnectionStatus
        {
            get => (string)GetValue(ConnectionStatusProperty);
            set => SetValue(ConnectionStatusProperty, value);
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                nameof(CurrentTime),
                typeof(string),
                typeof(StatusBar),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Current time display, automatically updated.
        /// </summary>
        public string CurrentTime
        {
            get => (string)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly DependencyProperty ShowTimeProperty =
            DependencyProperty.Register(
                nameof(ShowTime),
                typeof(bool),
                typeof(StatusBar),
                new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether to display the current time.
        /// </summary>
        public bool ShowTime
        {
            get => (bool)GetValue(ShowTimeProperty);
            set => SetValue(ShowTimeProperty, value);
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(
                nameof(Height),
                typeof(double),
                typeof(StatusBar),
                new PropertyMetadata(32.0));

        /// <summary>
        /// Height of the status bar in pixels.
        /// </summary>
        public new double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        private void InitializeTimer()
        {
            _timeUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timeUpdateTimer.Tick += (s, e) => UpdateCurrentTime();
            _timeUpdateTimer.Start();
            UpdateCurrentTime();
        }

        private void UpdateCurrentTime()
        {
            if (ShowTime)
            {
                CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
