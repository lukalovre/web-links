using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AvaloniaApplication1.Models;
using ReactiveUI;

namespace AvaloniaApplication1.ViewModels;

public partial class EventViewModel : ViewModelBase
{
    private Event _selectedEvent = null!;
    private bool _isEditDate;
    private TimeSpan _time;
    private DateTime _date;

    public ObservableCollection<Event> Events { get; set; }

    public DateTime Date
    {
        get => _date;
        set
        {
            this.RaiseAndSetIfChanged(ref _date, value);
        }
    }

    public TimeSpan Time
    {
        get => _time;
        set
        {
            this.RaiseAndSetIfChanged(ref _time, value);
        }
    }

    public bool IsEditDate
    {
        get => _isEditDate;
        set => this.RaiseAndSetIfChanged(ref _isEditDate, value);
    }

    public Event SelectedEvent
    {
        get => _selectedEvent;
        set => this.RaiseAndSetIfChanged(ref _selectedEvent, value);
    }

    public EventViewModel(ObservableCollection<Event> events)
    {
        Events = events;
        Events.CollectionChanged += CollectionChanged;
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var events = sender as ObservableCollection<Event>;
        SelectedEvent = events?.MaxBy(o => o.Date)!;

        if (SelectedEvent == null)
        {
            return;
        }

        _date = SelectedEvent.Date;
        _time = _date.TimeOfDay;
    }
}
