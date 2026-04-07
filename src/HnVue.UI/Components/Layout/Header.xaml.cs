using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout;

/// <summary>
/// Header component for application screens.
/// Displays title, navigation controls, and action buttons.
/// </summary>
public class Header : Control
{
    /// <summary>Identifies the Title dependency property.</summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(Header),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the LeftContent dependency property.</summary>
    public static readonly DependencyProperty LeftContentProperty =
        DependencyProperty.Register(
            nameof(LeftContent),
            typeof(UIElement),
            typeof(Header),
            new PropertyMetadata(null));

    /// <summary>Identifies the RightContent dependency property.</summary>
    public static readonly DependencyProperty RightContentProperty =
        DependencyProperty.Register(
            nameof(RightContent),
            typeof(UIElement),
            typeof(Header),
            new PropertyMetadata(null));

    /// <summary>Identifies the ShowBorder dependency property.</summary>
    public static readonly DependencyProperty ShowBorderProperty =
        DependencyProperty.Register(
            nameof(ShowBorder),
            typeof(bool),
            typeof(Header),
            new PropertyMetadata(true));

    /// <summary>Identifies the ActionButtons dependency property.</summary>
    public static readonly DependencyProperty ActionButtonsProperty =
        DependencyProperty.Register(
            nameof(ActionButtons),
            typeof(ObservableCollection<UIElement>),
            typeof(Header),
            new PropertyMetadata(null));

    static Header()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Header),
            new FrameworkPropertyMetadata(typeof(Header)));
    }

    /// <summary>
    /// Initializes a new instance of the Header class.
    /// Sets default Height to 56 per design system specification.
    /// </summary>
    public Header()
    {
        Height = 56;
        ActionButtons = new ObservableCollection<UIElement>();
    }

    /// <summary>Gets or sets the header title.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the left content (menu, back button, etc).</summary>
    public UIElement? LeftContent
    {
        get => (UIElement?)GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    /// <summary>Gets or sets the right content area (user actions, notifications, etc).</summary>
    public UIElement? RightContent
    {
        get => (UIElement?)GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    /// <summary>Gets or sets whether to show the bottom border of the header.</summary>
    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        set => SetValue(ShowBorderProperty, value);
    }

    /// <summary>Gets the collection of action buttons on the right side.</summary>
    public ObservableCollection<UIElement> ActionButtons
    {
        get => (ObservableCollection<UIElement>)GetValue(ActionButtonsProperty)!;
        private init => SetValue(ActionButtonsProperty, value);
    }
}
