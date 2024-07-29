using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Media.Imaging;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Models.Interfaces;
using AvaloniaApplication1.Repositories;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public class ItemViewModel<TItem, TGridItem> : ViewModelBase, IDataGrid where TItem : IItem where TGridItem : IGridItem
{
    public ItemViewModel(IDatasource datasource, IExternal<TItem> external)
    {
        _settings = Settings.Instance.GetItemSettigns<TItem>();

        _datasource = datasource;
        _external = external;

        GridFilterViewModel = new GridFilterViewModel(this)
        {
            ShowYearFilter = _settings.ShowYearFilter
        };
        People = new PeopleSelectionViewModel();

        GridItems = [];
        GridItemsBookmarked = [];
        ReloadData();

        Events = [];
        EventViewModel = new EventViewModel(Events, _settings.PlatformTypes);

        AddItemClick = ReactiveCommand.Create(AddItemClickAction);
        AddEventClick = ReactiveCommand.Create(AddEventClickAction);

        OpenLink = ReactiveCommand.Create(OpenLinkAction);
        OpenImage = ReactiveCommand.Create(OpenImageAction);

        SelectedGridItem = GridItems.LastOrDefault()!;
        NewEvent = new Event();
        NewItem = (TItem)Activator.CreateInstance(typeof(TItem))!;

        IsFullAmount = _settings.IsFullAmountDefaultValue;
    }

    protected DateTime? DateTimeFilter
    {
        get
        {
            return _settings.DateTimeFilter ?? new DateTime(GridFilterViewModel.YearFilter, 1, 1);
        }
    }

    public ObservableCollection<string> PlatformTypes => _settings.PlatformTypes;
    private readonly ItemSettings _settings = new();
    private readonly IDatasource _datasource;
    private readonly IExternal<TItem> _external;
    private TGridItem _selectedGridItem = default!;
    private List<TItem> _itemList = [];
    protected List<Event> _eventList = [];
    private TItem _newItem = default!;
    private Bitmap? _itemImage;
    private Bitmap? _newItemImage;
    private Event _newEvent = new();
    private bool _useNewDate;
    private TItem _selectedItem = default!;

    private int _gridCountItemsBookmarked;
    private int _addAmount;
    private string _addAmountString = string.Empty;

    public EventViewModel EventViewModel { get; }
    public GridFilterViewModel GridFilterViewModel { get; }

    public int AddAmount
    {
        get => _addAmount;
        set
        {
            this.RaiseAndSetIfChanged(ref _addAmount, value);
            _addAmount = SetAmount(value);
        }
    }

    private int _newAmount;
    private string _inputUrl = string.Empty;
    private bool _isFullAmount;
    private int _newItemAmount;

    public string AddAmountString
    {
        get => _addAmountString;
        set => this.RaiseAndSetIfChanged(ref _addAmountString, value);
    }

    public bool UseNewDate
    {
        get => _useNewDate;
        set => this.RaiseAndSetIfChanged(ref _useNewDate, value);
    }

    public bool IsFullAmount
    {
        get => _isFullAmount;
        set => this.RaiseAndSetIfChanged(ref _isFullAmount, value);
    }

    public PeopleSelectionViewModel People { get; set; }

    public ObservableCollection<TGridItem> GridItems { get; set; }
    public ObservableCollection<TGridItem> GridItemsBookmarked { get; set; }

    public TItem SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<Event> Events { get; set; }

    public ReactiveCommand<Unit, Unit> AddItemClick { get; }
    public ReactiveCommand<Unit, Unit> AddEventClick { get; }
    public ReactiveCommand<Unit, Unit> OpenLink { get; }
    public ReactiveCommand<Unit, Unit> OpenImage { get; }

    public TItem NewItem
    {
        get => _newItem;
        set => this.RaiseAndSetIfChanged(ref _newItem, value);
    }

    public DateTime NewDate { get; set; } = DateTime.Now;

    public Event NewEvent
    {
        get => _newEvent;
        set => this.RaiseAndSetIfChanged(ref _newEvent, value);
    }

    public Bitmap? Image
    {
        get => _itemImage;
        private set => this.RaiseAndSetIfChanged(ref _itemImage, value);
    }

    public Bitmap? NewImage
    {
        get => _newItemImage;
        private set => this.RaiseAndSetIfChanged(ref _newItemImage, value);
    }

    public int GridCountItemsBookmarked
    {
        get => _gridCountItemsBookmarked;
        private set => this.RaiseAndSetIfChanged(ref _gridCountItemsBookmarked, value);
    }

    public int NewItemAmount
    {
        get => _newItemAmount;
        set => this.RaiseAndSetIfChanged(ref _newItemAmount, value);
    }

    public TGridItem SelectedGridItem
    {
        get => _selectedGridItem;
        set
        {
            _selectedGridItem = value;
            SelectedItemChanged();
        }
    }

    public string InputUrl
    {
        get => _inputUrl;
        set
        {
            this.RaiseAndSetIfChanged(ref _inputUrl, value);
            InputUrlChanged();
        }
    }

    private int SetAmount(int value)
    {
        if (SelectedItem is null)
        {
            return 0;
        }

        var events = _eventList.Where(o => o.ItemID == SelectedItem.ID);
        var currentAmount = IsFullAmount ? 0 : GetItemAmount(events);
        var newAmount = value - currentAmount;

        _newAmount = newAmount;
        AddAmountString = $"    Adding {newAmount} {_settings.AmountVerb}";
        return value;
    }

    private async void InputUrlChanged()
    {
        NewItem = await _external.GetItem(InputUrl);

        NewImage = FileRepsitory.GetImageTemp<TItem>();
        NewEvent = new Event
        {
            Rating = _settings.DefaultNewItemRating,
            Platform = _settings.DefaultNewItemPlatform,
            Completed = _settings.DefaultNewItemCompleted,
            Bookmakred = _settings.DefaultNewItemBookmakred
        };

        _inputUrl = string.Empty;
    }

    private void OpenImageAction()
    {
        throw new NotImplementedException();
    }

    protected virtual List<string> GetAlternativeOpenLinkSearchParams() => [];

    private void OpenLinkAction()
    {
        var link = string.Empty;

        if (_settings.OpenItemLinkUrl)
        {
            link = (SelectedItem as IExternal)?.ExternalID ?? string.Empty;
        }

        HtmlHelper.OpenLink(link, [.. GetAlternativeOpenLinkSearchParams()]);
    }

    private void AddItemClickAction()
    {
        var overrideAmount = _settings.NewItemAmountOverride;

        if (overrideAmount == -1)
        {
            overrideAmount = (NewItem as IRuntime)?.Runtime ?? 0;
        }

        var amount = overrideAmount ?? NewItemAmount;
        var dateEnd = UseNewDate ? NewDate : DateTime.Now;
        var people = People.GetPeople();

        var amountType = _settings.AmountType ?? NewEvent.AmountType;

        NewEvent ??= new Event();
        var chapter = NewEvent.Chapter ?? _settings.DefaultNewItemChapter;

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            ExternalID = string.Empty,
            DateEnd = dateEnd,
            Rating = NewEvent.Rating,
            Bookmakred = NewEvent.Bookmakred,
            Chapter = chapter,
            Amount = amount,
            AmountType = amountType,
            Completed = NewEvent.Completed,
            Comment = NewEvent.Comment,
            People = people,
            Platform = NewEvent.Platform,
            LocationID = NewEvent.LocationID
        };

        _datasource.Add(NewItem, newEvent);

        ReloadData();
        ClearNewItemControls();
    }

    private void AddEventClickAction()
    {
        var lastEvent = Events.MaxBy(o => o.DateEnd) ?? Events.LastOrDefault();

        var amount = _newAmount == 0
        ? lastEvent?.Amount ?? 0
        : _newAmount;

        var dateEnd = !EventViewModel.IsEditDate
        ? DateTime.Now
        : EventViewModel?.SelectedEvent?.DateEnd ?? DateTime.Now;

        var people = EventViewModel?.People.GetPeople() ?? string.Empty;

        int? chapter = null;

        if (_settings.DefaultNewItemChapter is not null)
        {
            chapter = EventViewModel?.NewEventChapter ?? _settings.DefaultNewItemChapter;
        }

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            ExternalID = string.Empty,
            DateEnd = dateEnd,
            Rating = lastEvent?.Rating ?? 0,
            Bookmakred = lastEvent?.Bookmakred ?? false,
            Chapter = chapter,
            Amount = amount,
            AmountType = lastEvent?.AmountType ?? 0,
            Completed = lastEvent?.Completed ?? false,
            Comment = lastEvent?.Comment ?? string.Empty,
            People = people,
            Platform = EventViewModel?.SelectedPlatformType ?? string.Empty,
            LocationID = lastEvent?.LocationID
        };

        _datasource.Add(SelectedItem, newEvent);

        ReloadData();
        ClearNewItemControls();
    }

    protected virtual void ReloadData()
    {
        GridItems.Clear();
        GridItems.AddRange(LoadData());
        GridFilterViewModel.GridCountItems = GridItems.Count;

        GridItemsBookmarked.Clear();
        GridItemsBookmarked.AddRange(LoadDataBookmarked());
    }

    private void ClearNewItemControls()
    {
        this.RaiseAndSetIfChanged(ref _inputUrl, string.Empty);
        NewImage = default;
        NewEvent = new Event();
        NewItem = (TItem)Activator.CreateInstance(typeof(TItem))!;
    }

    internal List<TGridItem> LoadData(string searchText = null!)
    {
        var type = Helpers.GetClassName<TItem>();

        _itemList = _datasource.GetList<TItem>(type);
        _eventList = _datasource.GetEventList(type);
        searchText ??= GridFilterViewModel.SearchText;

        var result = _eventList
                    .OrderByDescending(o => o.DateEnd)
                    .DistinctBy(o => o.ItemID)
                    .OrderBy(o => o.DateEnd)
                    .ToList();

        if (DateTimeFilter.HasValue
            && DateTimeFilter != DateTime.MinValue
            && string.IsNullOrWhiteSpace(searchText))
        {
            result = result
            .Where(o =>
            o.DateEnd.HasValue
            && o.DateEnd.Value >= DateTimeFilter.Value
            && o.DateEnd.Value < new DateTime(DateTimeFilter.Value.Year + 1, 1, 1))
            .ToList();
        }

        var resultGrid = result.Select(o =>
                        Convert(
                            o,
                            _itemList.First(m => m.ID == o.ItemID),
                            _eventList.Where(e => e.ItemID == o.ItemID)
                        )
                        ).ToList();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            resultGrid = resultGrid
            .Where(o =>
                JsonConvert.SerializeObject(o)
                .Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        }

        return resultGrid;
    }

    protected List<TGridItem> LoadDataBookmarked(int? yearsAgo = null)
    {
        var type = Helpers.GetClassName<TItem>();
        _itemList = _datasource.GetList<TItem>(type);
        _eventList = _datasource.GetEventList(type);

        var dateFilter = yearsAgo.HasValue
            ? DateTime.Now.AddYears(-yearsAgo.Value)
            : DateTime.MaxValue;

        return _eventList
            .OrderByDescending(o => o.DateEnd)
            .DistinctBy(o => o.ItemID)
            .OrderBy(o => o.DateEnd)
            .Where(o => o.DateEnd.HasValue && o.DateEnd.Value <= dateFilter)
            .Where(o => o.Bookmakred)
            .Select(
                o =>
                    Convert(
                        o,
                        _itemList.First(m => m.ID == o.ItemID),
                        _eventList.Where(e => e.ItemID == o.ItemID)
                    )
            )
            .ToList();
    }

    protected virtual TGridItem Convert(Event e, TItem i, IEnumerable<Event> events)
    {
        return default!;
    }

    protected int GetItemAmount(IEnumerable<Event> eventList)
    {
        // This is for the case that the Item is already completed by you are rereading it.
        var lastCompletedDate = eventList.Where(o => o.Completed)?.MaxBy(o => o.DateEnd)?.DateEnd ?? DateTime.MinValue;
        var lastDate = eventList.MaxBy(o => o.DateEnd)?.DateEnd ?? DateTime.MinValue;
        var dateFilter = lastCompletedDate;

        var lastChapter = eventList.LastOrDefault()?.Chapter;

        if (EventViewModel is not null
            && lastChapter.HasValue
            && lastChapter < EventViewModel.NewEventChapter)
        {
            lastChapter = EventViewModel.NewEventChapter;
        }

        var eventsByChapter = eventList.Where(o => o.Chapter == lastChapter);

        if (lastCompletedDate == lastDate)
        {
            return eventsByChapter.Sum(o => o.Amount);
        }

        return eventsByChapter.Where(o => o.DateEnd > dateFilter).Sum(o => o.Amount);
    }

    public void SelectedItemChanged()
    {
        Events.Clear();
        Image = null;

        if (SelectedGridItem == null)
        {
            return;
        }

        SelectedItem = _itemList.First(o => o.ID == SelectedGridItem.ID);
        Events.AddRange(
            _eventList
                .Where(o => o.ItemID == SelectedItem.ID && o.DateEnd.HasValue)
                .OrderBy(o => o.DateEnd)
        );

        if (!Events.Any())
        {
            Events.AddRange(_eventList.Where(o => o.ItemID == SelectedItem.ID));
        }

        var item = _itemList.First(o => o.ID == SelectedItem.ID);
        Image = FileRepsitory.GetImage<TItem>(item.ID);

        if (_settings.DefaultAddAmount == -1)
        {
            AddAmount = Events.LastOrDefault()?.Amount ?? 0;
        }
        else
        {
            AddAmount = _settings.DefaultAddAmount;
        }
    }

    private ObservableCollection<TGridItem> GetSelectedGrid()
    {
        return GridItems;
    }

    int IDataGrid.ReloadData()
    {
        var selectedGrid = GetSelectedGrid();
        selectedGrid.Clear();
        selectedGrid.AddRange(LoadData());
        return selectedGrid.Count;
    }
}
