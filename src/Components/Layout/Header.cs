using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout
{
    /// <summary>
    /// An application header component for displaying title and action buttons.
    /// Provides consistent top navigation across all application screens.
    /// </summary>
    public class Header : Control
    {
        static Header()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Header),
                new FrameworkPropertyMetadata(typeof(Header)));
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(Header),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Main title displayed in the center of the header.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty LeftContentProperty =
            DependencyProperty.Register(
                nameof(LeftContent),
                typeof(object),
                typeof(Header),
                new PropertyMetadata(null));

        /// <summary>
        /// Optional content displayed on the left side (e.g., back button, breadcrumb).
        /// </summary>
        public object LeftContent
        {
            get => GetValue(LeftContent);
            set => SetValue(LeftContentProperty, value);
        }

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register(
                nameof(RightContent),
                typeof(object),
                typeof(Header),
                new PropertyMetadata(null));

        /// <summary>
        /// Optional content displayed on the right side (e.g., user menu, notifications).
        /// </summary>
        public object RightContent
        {
            get => GetValue(RightContent);
            set => SetValue(RightContentProperty, value);
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(
                nameof(Height),
                typeof(double),
                typeof(Header),
                new PropertyMetadata(56.0));

        /// <summary>
        /// Height of the header in pixels.
        /// </summary>
        public new double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly DependencyProperty ShowBorderProperty =
            DependencyProperty.Register(
                nameof(ShowBorder),
                typeof(bool),
                typeof(Header),
                new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether to show the bottom border.
        /// </summary>
        public bool ShowBorder
        {
            get => (bool)GetValue(ShowBorderProperty);
            set => SetValue(ShowBorderProperty, value);
        }
    }
}
