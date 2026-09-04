using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Repositories;
using WebLinks.Models;
using WebLinks.Repositories;

namespace WebLinks.ViewModels;

public class StatsViewModel : ViewModelBase
{
    private readonly IDatasource _datasource;

    public StatsViewModel(IDatasource datasource)
    {
        _datasource = datasource;
        Refresh = ReactiveCommand.Create(Reload);
        Reload();
    }

    public ObservableCollection<WebpageVisitStat> TopWebpages { get; } = [];
    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public void Reload()
    {
        var links = _datasource.GetList<Link>(Helpers.GetClassName<Link>());
        var linksById = links.ToDictionary(link => link.ID);
        var events = _datasource.GetEventList(Helpers.GetClassName<Link>());

        var results = events
            .GroupBy(itemEvent => itemEvent.ItemID)
            .Where(group => linksById.ContainsKey(group.Key))
            .Select(group => new WebpageVisitStat
            {
                Rank = 0,
                Title = linksById[group.Key].Title,
                Url = linksById[group.Key].Url,
                Visits = group.Count(),
                LastVisited = group.Max(itemEvent => itemEvent.Date)
            })
            .OrderByDescending(item => item.Visits)
            .ThenByDescending(item => item.LastVisited)
            .Take(50)
            .ToList();

        TopWebpages.Clear();

        for (var index = 0; index < results.Count; index++)
        {
            results[index].Rank = index + 1;
            TopWebpages.Add(results[index]);
        }
    }
}

public class WebpageVisitStat
{
    public int Rank { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Visits { get; set; }
    public DateTime LastVisited { get; set; }
}
