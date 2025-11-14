using Repositories;

namespace WebLinks.ViewModels;

public class MainWindowViewModel(IDatasource datasource) : ViewModelBase
{
    public LinksViewModel LinksViewModel { get; } = new LinksViewModel(datasource, new Links());
}
