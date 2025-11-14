using Avalonia.Controls;

namespace WebLinks.Views;

public partial class GridFilterView : UserControl
{
    public GridFilterView()
    {
        ViewHelper.AddConverters(Resources);
        InitializeComponent();
    }
}
