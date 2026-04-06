using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="CDBurnView"/>.</summary>
public partial class CDBurnView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="CDBurnView"/>.</summary>
    public CDBurnView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="CDBurnView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public CDBurnView(ICDBurnViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
