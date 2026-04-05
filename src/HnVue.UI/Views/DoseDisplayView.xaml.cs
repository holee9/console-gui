using HnVue.UI.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="DoseDisplayView"/>.</summary>
public partial class DoseDisplayView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="DoseDisplayView"/>.</summary>
    public DoseDisplayView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="DoseDisplayView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public DoseDisplayView(DoseDisplayViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
