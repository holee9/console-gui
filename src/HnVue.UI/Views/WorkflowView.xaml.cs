using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="WorkflowView"/>.</summary>
public partial class WorkflowView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="WorkflowView"/>.</summary>
    public WorkflowView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="WorkflowView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public WorkflowView(IWorkflowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
