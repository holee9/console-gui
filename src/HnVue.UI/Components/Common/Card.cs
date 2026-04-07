using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Common;

/// <summary>
/// Card container component for HnVue Design System 2026.
/// Provides a rounded, padded surface for grouping related content.
/// </summary>
public class Card : ContentControl
{
    /// <summary>Identifies the Header dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(Card),
            new PropertyMetadata(null));

    /// <summary>Identifies the Footer dependency property.</summary>
    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(Card),
            new PropertyMetadata(null));

    /// <summary>Identifies the CornerRadius dependency property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(Card),
            new PropertyMetadata(new CornerRadius(12)));

    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card)));
    }

    /// <summary>
    /// Initializes a new instance of the Card class.
    /// Sets default Padding to 16 per design system specification.
    /// </summary>
    public Card()
    {
        Padding = new Thickness(16);
    }

    /// <summary>Gets or sets the card header content.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the card footer content.</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>Gets or sets the corner radius of the card border.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
