using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout;

/// <summary>
/// Sidebar navigation component for HnVue Design System 2026.
/// Provides collapsible navigation with logo, menu items, and footer content.
/// </summary>
public class Sidebar : Control
{
    /// <summary>Identifies the Logo dependency property.</summary>
    public static readonly DependencyProperty LogoProperty =
        DependencyProperty.Register(
            nameof(Logo),
            typeof(object),
            typeof(Sidebar),
            new PropertyMetadata(null));

    /// <summary>Identifies the MenuItems dependency property.</summary>
    public static readonly DependencyProperty MenuItemsProperty =
        DependencyProperty.Register(
            nameof(MenuItems),
            typeof(IEnumerable),
            typeof(Sidebar),
            new PropertyMetadata(null));

    /// <summary>Identifies the FooterContent dependency property.</summary>
    public static readonly DependencyProperty FooterContentProperty =
        DependencyProperty.Register(
            nameof(FooterContent),
            typeof(object),
            typeof(Sidebar),
            new PropertyMetadata(null));

    /// <summary>Identifies the IsExpanded dependency property.</summary>
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(Sidebar),
            new PropertyMetadata(true));

    static Sidebar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Sidebar),
            new FrameworkPropertyMetadata(typeof(Sidebar)));
    }

    /// <summary>
    /// Initializes a new instance of the Sidebar class.
    /// Sets default Width to 240 per design system specification.
    /// </summary>
    public Sidebar()
    {
        Width = 240;
    }

    /// <summary>Gets or sets the logo content displayed at the top of the sidebar.</summary>
    public object? Logo
    {
        get => GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    /// <summary>Gets or sets the collection of navigation menu items.</summary>
    public IEnumerable? MenuItems
    {
        get => (IEnumerable?)GetValue(MenuItemsProperty);
        set => SetValue(MenuItemsProperty, value);
    }

    /// <summary>Gets or sets the footer content at the bottom of the sidebar.</summary>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>Gets or sets whether the sidebar is in expanded state.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}
