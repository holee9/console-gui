using System.Windows;
using System.Windows.Controls;

namespace HnVue.UI.Components.Layout
{
    /// <summary>
    /// Primary navigation sidebar for medical device console.
    /// Provides quick access to critical functions with keyboard shortcuts.
    /// </summary>
    public class MedicalSidebar : ItemsControl
    {
        static MedicalSidebar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MedicalSidebar),
                new FrameworkPropertyMetadata(typeof(MedicalSidebar)));
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(MedicalSidebar),
                new PropertyMetadata(null));

        public new object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(
                nameof(IsCollapsed),
                typeof(bool),
                typeof(MedicalSidebar),
                new PropertyMetadata(false));

        public bool IsCollapsed
        {
            get => (bool)GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }
    }

    /// <summary>
    /// Navigation menu items for the medical sidebar.
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public string Icon { get; set; }
        public string Shortcut { get; set; }
        public bool IsCritical { get; set; }
        public object Tag { get; set; }

        public static NavMenuItem Worklist => new() { Label = "Worklist", Icon = "📋", Shortcut = "Alt+1" };
        public static NavMenuItem Studylist => new() { Label = "Studylist", Icon = "📁", Shortcut = "Alt+2" };
        public static NavMenuItem Acquisition => new() { Label = "Acquisition", Icon = "🎯", Shortcut = "Alt+3", IsCritical = true };
        public static NavMenuItem Settings => new() { Label = "Settings", Icon = "⚙️", Shortcut = "Alt+4" };
        public static NavMenuItem Help => new() { Label = "Help", Icon = "❓", Shortcut = "F1" };
    }
}
