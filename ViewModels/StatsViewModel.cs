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
    public ObservableCollection<SiteVisitStat> TopSites { get; } = [];
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

        var siteResults = events
            .Join(
                links,
                itemEvent => itemEvent.ItemID,
                link => link.ID,
                (itemEvent, link) => new { itemEvent, link })
            .Select(item => new
            {
                item.itemEvent,
                Site = GetSiteName(item.link.Url)
            })
            .Where(item => item.Site is not null)
            .GroupBy(item => item.Site!, StringComparer.OrdinalIgnoreCase)
            .Select(group => new SiteVisitStat
            {
                Rank = 0,
                Site = group.Key,
                Visits = group.Count(),
                LastVisited = group.Max(item => item.itemEvent.Date)
            })
            .OrderByDescending(item => item.Visits)
            .ThenByDescending(item => item.LastVisited)
            .Take(50)
            .ToList();

        TopSites.Clear();

        for (var index = 0; index < siteResults.Count; index++)
        {
            siteResults[index].Rank = index + 1;
            TopSites.Add(siteResults[index]);
        }
    }

    private static string? GetSiteName(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? uri.Host[4..]
            : uri.Host;
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

public class SiteVisitStat
{
    public int Rank { get; set; }
    public string Site { get; set; } = string.Empty;
    public int Visits { get; set; }
    public DateTime LastVisited { get; set; }
}
