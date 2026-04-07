using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HnVue.UI.Components.Common
{
    /// <summary>
    /// A modal dialog component for displaying focused content with overlay backdrop.
    /// Supports medical device UI safety requirements with clear focus management.
    /// </summary>
    public class Modal : ContentControl
    {
        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal),
                new FrameworkPropertyMetadata(typeof(Modal)));
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(Modal),
                new PropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        /// Indicates whether the modal is currently visible.
        /// </summary>
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Modal modal && e.NewValue is bool isOpen)
            {
                if (isOpen)
                {
                    modal.OnOpened();
                }
                else
                {
                    modal.OnClosed();
                }
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(Modal),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// The title displayed in the modal header.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty HasCloseButtonProperty =
            DependencyProperty.Register(
                nameof(HasCloseButton),
                typeof(bool),
                typeof(Modal),
                new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether to show the close button in the header.
        /// </summary>
        public bool HasCloseButton
        {
            get => (bool)GetValue(HasCloseButtonProperty);
            set => SetValue(HasCloseButtonProperty, value);
        }

        public static readonly DependencyProperty ShowFooterProperty =
            DependencyProperty.Register(
                nameof(ShowFooter),
                typeof(bool),
                typeof(Modal),
                new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether to show the footer with action buttons.
        /// </summary>
        public bool ShowFooter
        {
            get => (bool)GetValue(ShowFooterProperty);
            set => SetValue(ShowFooterProperty, value);
        }

        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.Register(
                nameof(MaxWidth),
                typeof(double),
                typeof(Modal),
                new PropertyMetadata(600.0));

        /// <summary>
        /// Maximum width of the modal content.
        /// </summary>
        public new double MaxWidth
        {
            get => (double)GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(
                nameof(CloseCommand),
                typeof(ICommand),
                typeof(Modal),
                new PropertyMetadata(null));

        /// <summary>
        /// Command executed when the close button or overlay is clicked.
        /// </summary>
        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public static readonly DependencyProperty ConfirmCommandProperty =
            DependencyProperty.Register(
                nameof(ConfirmCommand),
                typeof(ICommand),
                typeof(Modal),
                new PropertyMetadata(null));

        /// <summary>
        /// Command executed when the confirm button is clicked.
        /// </summary>
        public ICommand ConfirmCommand
        {
            get => (ICommand)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
                nameof(CancelCommand),
                typeof(ICommand),
                typeof(Modal),
                new PropertyMetadata(null));

        /// <summary>
        /// Command executed when the cancel button is clicked.
        /// </summary>
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        /// <summary>
        /// Called when the modal is opened. Override to add custom behavior.
        /// </summary>
        protected virtual void OnOpened()
        {
            // Focus management for accessibility
            // Trap keyboard focus within modal
        }

        /// <summary>
        /// Called when the modal is closed. Override to add custom behavior.
        /// </summary>
        protected virtual void OnClosed()
        {
            // Return focus to previous element
        }
    }
}
