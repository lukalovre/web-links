using Avalonia.Controls;

namespace AvaloniaApplication1.Views;

public partial class LinksView : UserControl
{
    public LinksView()
    {
        ViewHelper.AddConverters(Resources);
        InitializeComponent();
    }
}
