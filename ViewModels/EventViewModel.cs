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

    private string _selectedPlatformType = string.Empty;
    private string _selectedPersonString = string.Empty;
    private int _newEventChapter = 1;

    public string SelectedPersonString
    {
        get => _selectedPersonString;
        set => this.RaiseAndSetIfChanged(ref _selectedPersonString, value);
    }

    public ObservableCollection<string> PlatformTypes { get; set; }

    public PeopleSelectionViewModel People { get; }

    public string SelectedPlatformType
    {
        get => _selectedPlatformType;
        set => this.RaiseAndSetIfChanged(ref _selectedPlatformType, value);
    }

    public int NewEventChapter
    {
        get => _newEventChapter;
        set => this.RaiseAndSetIfChanged(ref _newEventChapter, value);
    }

    public EventViewModel(ObservableCollection<Event> events, ObservableCollection<string> platformTypes)
    {
        Events = events;
        Events.CollectionChanged += CollectionChanged;
        PlatformTypes = platformTypes;
        People = new PeopleSelectionViewModel();
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
