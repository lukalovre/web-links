using Avalonia.Controls;

namespace WebLinks.Views;

public partial class EventView : UserControl
{
    public EventView()
    {
        ViewHelper.AddConverters(Resources);
        InitializeComponent();
    }
}
