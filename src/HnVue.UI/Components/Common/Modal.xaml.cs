using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HnVue.UI.Components.Common;

/// <summary>
/// Modal dialog component for HnVue Design System 2026.
/// IEC 62366 compliant modal with overlay and keyboard support.
/// </summary>
public class Modal : ContentControl
{
    /// <summary>Identifies the IsOpen dependency property.</summary>
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(Modal),
            new PropertyMetadata(false));

    /// <summary>Identifies the Title dependency property.</summary>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(Modal),
            new PropertyMetadata(string.Empty));

    /// <summary>Identifies the HasCloseButton dependency property.</summary>
    public static readonly DependencyProperty HasCloseButtonProperty =
        DependencyProperty.Register(
            nameof(HasCloseButton),
            typeof(bool),
            typeof(Modal),
            new PropertyMetadata(true));

    /// <summary>Identifies the ShowFooter dependency property.</summary>
    public static readonly DependencyProperty ShowFooterProperty =
        DependencyProperty.Register(
            nameof(ShowFooter),
            typeof(bool),
            typeof(Modal),
            new PropertyMetadata(true));

    /// <summary>
    /// Identifies the Header dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(Modal),
            new PropertyMetadata(null));

    /// <summary>
    /// Identifies the CloseCommand dependency property.
    /// </summary>
    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(Modal),
            new PropertyMetadata(null));

    /// <summary>
    /// Identifies the FooterActions dependency property.
    /// </summary>
    public static readonly DependencyProperty FooterActionsProperty =
        DependencyProperty.Register(
            nameof(FooterActions),
            typeof(ObservableCollection<UIElement>),
            typeof(Modal),
            new PropertyMetadata(new ObservableCollection<UIElement>()));

    /// <summary>
    /// Identifies the ModalMaxWidth dependency property.
    /// Named ModalMaxWidth to avoid shadowing FrameworkElement.MaxWidthProperty.
    /// </summary>
    public static readonly DependencyProperty ModalMaxWidthProperty =
        DependencyProperty.Register(
            nameof(ModalMaxWidth),
            typeof(double),
            typeof(Modal),
            new PropertyMetadata(600.0));

    /// <summary>
    /// Identifies the ModalMaxHeight dependency property.
    /// Named ModalMaxHeight to avoid shadowing FrameworkElement.MaxHeightProperty.
    /// </summary>
    public static readonly DependencyProperty ModalMaxHeightProperty =
        DependencyProperty.Register(
            nameof(ModalMaxHeight),
            typeof(double),
            typeof(Modal),
            new PropertyMetadata(800.0));

    static Modal()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal),
            new FrameworkPropertyMetadata(typeof(Modal)));
    }

    /// <summary>
    /// Initializes a new instance of the Modal class.
    /// Sets MaxWidth to 600 per design system specification.
    /// </summary>
    public Modal()
    {
        MaxWidth = 600;
    }

    /// <summary>Gets or sets whether the modal is open/visible.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets or sets the modal title text.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets whether the modal has a close button.</summary>
    public bool HasCloseButton
    {
        get => (bool)GetValue(HasCloseButtonProperty);
        set => SetValue(HasCloseButtonProperty, value);
    }

    /// <summary>Gets or sets whether to show the modal footer.</summary>
    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the modal header content.
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the modal is closed.
    /// </summary>
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>
    /// Gets the collection of footer action buttons.
    /// </summary>
    public ObservableCollection<UIElement> FooterActions
    {
        get => (ObservableCollection<UIElement>)GetValue(FooterActionsProperty);
        init => SetValue(FooterActionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum width of the modal panel.
    /// </summary>
    public double ModalMaxWidth
    {
        get => (double)GetValue(ModalMaxWidthProperty);
        set => SetValue(ModalMaxWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum height of the modal panel.
    /// </summary>
    public double ModalMaxHeight
    {
        get => (double)GetValue(ModalMaxHeightProperty);
        set => SetValue(ModalMaxHeightProperty, value);
    }
}
