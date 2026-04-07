using System.Windows.Controls;

namespace HnVue.UI.Views;

/// <summary>
/// Code-behind for <see cref="MergeView"/>.
/// All logic is delegated to <c>IMergeViewModel</c> via data binding.
/// PPT 슬라이드 13: "Sync Study" (formerly "Same Studylist") dialog.
/// </summary>
public partial class MergeView : UserControl
{
    /// <summary>Initialises a new instance of <see cref="MergeView"/>.</summary>
    public MergeView()
    {
        InitializeComponent();
    }
}
