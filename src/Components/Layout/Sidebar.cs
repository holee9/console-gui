using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout
{
    /// <summary>
    /// A sidebar navigation component for main application navigation.
    /// Follows medical device UI standards for consistent navigation patterns.
    /// </summary>
    public class Sidebar : Control
    {
        static Sidebar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Sidebar),
                new FrameworkPropertyMetadata(typeof(Sidebar)));
        }

        public static readonly DependencyProperty LogoProperty =
            DependencyProperty.Register(
                nameof(Logo),
                typeof(object),
                typeof(Sidebar),
                new PropertyMetadata(null));

        /// <summary>
        /// Logo or branding content displayed at the top of the sidebar.
        /// </summary>
        public object Logo
        {
            get => GetValue(LogoProperty);
            set => SetValue(LogoProperty, value);
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(
                nameof(MenuItems),
                typeof(IEnumerable<MenuItem>),
                typeof(Sidebar),
                new PropertyMetadata(null));

        /// <summary>
        /// Collection of navigation menu items.
        /// </summary>
        public IEnumerable<MenuItem> MenuItems
        {
            get => (IEnumerable<MenuItem>)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(
                nameof(FooterContent),
                typeof(object),
                typeof(Sidebar),
                new PropertyMetadata(null));

        /// <summary>
        /// Optional footer content displayed at the bottom of the sidebar.
        /// </summary>
        public object FooterContent
        {
            get => GetValue(FooterContent);
            set => SetValue(FooterContentProperty, value);
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register(
                nameof(Width),
                typeof(double),
                typeof(Sidebar),
                new PropertyMetadata(240.0));

        /// <summary>
        /// Width of the sidebar in pixels.
        /// </summary>
        public new double Width
        {
            get => (double)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(Sidebar),
                new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether the sidebar is expanded or collapsed.
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }
    }

    /// <summary>
    /// Represents a navigation menu item in the sidebar.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Display label for the menu item.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Icon character or symbol for visual identification.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Command to execute when the menu item is clicked.
        /// </summary>
        public System.Windows.Input.ICommand? Command { get; set; }

        /// <summary>
        /// Command parameter to pass to the command.
        /// </summary>
        public object? CommandParameter { get; set; }

        /// <summary>
        /// Optional keyboard shortcut hint.
        /// </summary>
        public string? Shortcut { get; set; }

        /// <summary>
        /// Indicates whether this item is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Indicates whether this item should be visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
