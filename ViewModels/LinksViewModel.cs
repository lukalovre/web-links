using System.Collections.Generic;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Repositories;
using AvaloniaApplication1.ViewModels.Extensions;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public partial class LinksViewModel(IDatasource datasource, IExternal<Link> external) : ItemViewModel<Link, LinkGridItem>(datasource, external)
{
    protected override LinkGridItem Convert(Event e, Link i, IEnumerable<Event> eventList)
    {
        return new LinkGridItem(
            i.ID,
            i.Title,
            i.Category,
            i.SubCategory,
            eventList.LastEventDate());
    }
}
