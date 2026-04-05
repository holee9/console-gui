using HnVue.UI.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="SystemAdminView"/>.</summary>
public partial class SystemAdminView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="SystemAdminView"/>.</summary>
    public SystemAdminView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="SystemAdminView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public SystemAdminView(SystemAdminViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
