using Avalonia.Controls;

namespace AvaloniaApplication1.Views;

public partial class EventView : UserControl
{
    public EventView()
    {
        ViewHelper.AddConverters(Resources);
        InitializeComponent();
    }
}
