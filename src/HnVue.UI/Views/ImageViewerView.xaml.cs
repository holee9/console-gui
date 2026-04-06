using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.Views;

/// <summary>Code-behind for <see cref="ImageViewerView"/>.</summary>
public partial class ImageViewerView : System.Windows.Controls.UserControl
{
    /// <summary>Initialises a new instance of <see cref="ImageViewerView"/>.</summary>
    public ImageViewerView()
    {
        InitializeComponent();
    }

    /// <summary>Initialises a new instance of <see cref="ImageViewerView"/> with the given ViewModel.</summary>
    /// <param name="viewModel">The ViewModel to bind to this view.</param>
    public ImageViewerView(IImageViewerViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
