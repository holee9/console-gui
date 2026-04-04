using HnVue.UI.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="PatientListView"/>.</summary>
public partial class PatientListView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="PatientListView"/>.</summary>
    public PatientListView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="PatientListView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public PatientListView(PatientListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
