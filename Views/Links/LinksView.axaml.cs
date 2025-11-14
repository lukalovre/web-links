using Avalonia.Controls;

namespace WebLinks.Views;

public partial class LinksView : UserControl
{
    public LinksView()
    {
        ViewHelper.AddConverters(Resources);
        InitializeComponent();
    }
}
