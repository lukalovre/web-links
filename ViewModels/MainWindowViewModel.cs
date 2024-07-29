using Repositories;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel(IDatasource datasource) : ViewModelBase
{
    public LinksViewModel LinksViewModel { get; } = new LinksViewModel(datasource, null);
}
