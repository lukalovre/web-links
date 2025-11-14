using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WebLinks.ViewModels;
using WebLinks.Views;
using Repositories;

namespace WebLinks;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(new TsvDatasource()),
                Position = new PixelPoint(0, 1000),
                WindowState = WindowState.Maximized
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
