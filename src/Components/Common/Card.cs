using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Common
{
    /// <summary>
    /// A card container component for displaying grouped content with consistent styling.
    /// Follows medical UI design principles with clear visual hierarchy.
    /// </summary>
    public class Card : ContentControl
    {
        static Card()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Card),
                new FrameworkPropertyMetadata(typeof(Card)));
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(Card),
                new PropertyMetadata(null));

        /// <summary>
        /// Optional header content displayed at the top of the card.
        /// </summary>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate),
                typeof(DataTemplate),
                typeof(Card),
                new PropertyMetadata(null));

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderStringFormatProperty =
            DependencyProperty.Register(
                nameof(HeaderStringFormat),
                typeof(string),
                typeof(Card),
                new PropertyMetadata(null));

        public string HeaderStringFormat
        {
            get => (string)GetValue(HeaderStringFormatProperty);
            set => SetValue(HeaderStringFormatProperty, value);
        }

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register(
                nameof(Footer),
                typeof(object),
                typeof(Card),
                new PropertyMetadata(null));

        /// <summary>
        /// Optional footer content displayed at the bottom of the card.
        /// </summary>
        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.Register(
                nameof(FooterTemplate),
                typeof(DataTemplate),
                typeof(Card),
                new PropertyMetadata(null));

        public DataTemplate FooterTemplate
        {
            get => (DataTemplate)GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }

        public static readonly DependencyProperty FooterStringFormatProperty =
            DependencyProperty.Register(
                nameof(FooterStringFormat),
                typeof(string),
                typeof(Card),
                new PropertyMetadata(null));

        public string FooterStringFormat
        {
            get => (string)GetValue(FooterStringFormatProperty);
            set => SetValue(FooterStringFormatProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(Card),
                new PropertyMetadata(new CornerRadius(12)));

        /// <summary>
        /// The corner radius of the card.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(Card),
                new PropertyMetadata(new Thickness(16)));

        /// <summary>
        /// Internal padding of the card content.
        /// </summary>
        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
    }
}
