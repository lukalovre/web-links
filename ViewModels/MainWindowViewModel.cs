using AvaloniaApplication1.Repositories;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public class MainWindowViewModel(IDatasource datasource) : ViewModelBase
{
    public GamesViewModel GamesViewModel { get; } = new GamesViewModel(datasource, null);
}
