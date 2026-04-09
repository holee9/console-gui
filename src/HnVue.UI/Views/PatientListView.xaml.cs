using System.Threading.Tasks;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="PatientListView"/>.</summary>
public partial class PatientListView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="PatientListView"/>.</summary>
    public PatientListView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>Initialises a new instance of <see cref="PatientListView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public PatientListView(IPatientListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not IPatientListViewModel vm)
            return;

        if (vm.Patients.Count == 0 && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            await Task.Delay(600);
        }

        if (vm.SelectedPatient is null && vm.Patients.Count > 0)
        {
            vm.SelectedPatient = vm.Patients[0];
        }
    }
}
