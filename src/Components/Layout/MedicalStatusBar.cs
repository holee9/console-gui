using System;
using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout
{
    /// <summary>
    /// Status bar displaying system health, connection status, and critical alerts.
    /// Must remain visible at all times for safety monitoring.
    /// </summary>
    public class MedicalStatusBar : ContentControl
    {
        static MedicalStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MedicalStatusBar),
                new FrameworkPropertyMetadata(typeof(MedicalStatusBar)));
        }

        public static readonly DependencyProperty SystemStatusProperty =
            DependencyProperty.Register(
                nameof(SystemStatus),
                typeof(SystemStatus),
                typeof(MedicalStatusBar),
                new PropertyMetadata(SystemStatus.Normal));

        /// <summary>
        /// Overall system health status.
        /// </summary>
        public SystemStatus SystemStatus
        {
            get => (SystemStatus)GetValue(SystemStatusProperty);
            set => SetValue(SystemStatusProperty, value);
        }

        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register(
                nameof(ConnectionStatus),
                typeof(ConnectionStatus),
                typeof(MedicalStatusBar),
                new PropertyMetadata(ConnectionStatus.Connected));

        /// <summary>
        /// RIS/PACS connection status.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get => (ConnectionStatus)GetValue(ConnectionStatusProperty);
            set => SetValue(ConnectionStatusProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(MedicalStatusBar),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Current status message displayed to user.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(
                nameof(CurrentTime),
                typeof(DateTime),
                typeof(MedicalStatusBar),
                new PropertyMetadata(DateTime.Now));

        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly DependencyProperty ActiveUserProperty =
            DependencyProperty.Register(
                nameof(ActiveUser),
                typeof(string),
                typeof(MedicalStatusBar),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Currently logged-in user for accountability.
        /// </summary>
        public string ActiveUser
        {
            get => (string)GetValue(ActiveUserProperty);
            set => SetValue(ActiveUserProperty, value);
        }
    }

    public enum SystemStatus
    {
        Normal,    // Normal operation
        Warning,   // Non-critical issue
        Error,     // System error
        Critical   // Safety-critical failure
    }

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
        Connecting,
        Error
    }
}
