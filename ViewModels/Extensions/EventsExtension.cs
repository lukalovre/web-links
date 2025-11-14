using System;
using System.Collections.Generic;
using System.Linq;
using WebLinks.Models;

namespace WebLinks.ViewModels.Extensions;

public static class EventsExtension
{
    public static DateTime LastEventDate(this IEnumerable<Event> eventList)
    {
        return eventList.MaxBy(o => o.Date)?.Date ?? DateTime.MinValue;
    }
}
