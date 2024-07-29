using System.Collections.Generic;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Repositories;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public partial class StandupViewModel(IDatasource datasource, IExternal<Standup> external) : ItemViewModel<Standup, StandupGridItem>(datasource, external)
{
    protected override StandupGridItem Convert(Event e, Standup i, IEnumerable<Event> eventList)
    {
        return new StandupGridItem(
            i.ID,
            i.Title,
            i.Performer,
            i.Year,
            e?.Rating ?? 0
        );
    }

}
