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

    public EventViewModel EventViewModel { get; }
    public GridFilterViewModel GridFilterViewModel { get; }

    private string _inputUrl = string.Empty;
    private bool _isFullAmount;
    private int _newItemAmount;

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

    private async void InputUrlChanged()
    {
        NewItem = await _external.GetItem(InputUrl);

        NewImage = FileRepsitory.GetImageTemp<TItem>();
        NewEvent = new Event
        {

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
        NewEvent ??= new Event();

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            Date = DateTime.Now
        };

        _datasource.Add(NewItem, newEvent);

        ReloadData();
        ClearNewItemControls();
    }

    private void AddEventClickAction()
    {
        var lastEvent = Events.MaxBy(o => o.Date) ?? Events.LastOrDefault();

        var date = !EventViewModel.IsEditDate
        ? DateTime.Now
        : EventViewModel?.SelectedEvent?.Date ?? DateTime.Now;

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            Date = date
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
                    .OrderByDescending(o => o.Date)
                    .DistinctBy(o => o.ItemID)
                    .OrderBy(o => o.Date)
                    .ToList();

        if (DateTimeFilter.HasValue
            && DateTimeFilter != DateTime.MinValue
            && string.IsNullOrWhiteSpace(searchText))
        {
            result = result
            .Where(o => o.Date >= DateTimeFilter.Value
            && o.Date < new DateTime(DateTimeFilter.Value.Year + 1, 1, 1))
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
            .Where(o => o.Bookmarked.HasValue && o.Bookmarked.Value == true)
            .OrderByDescending(o => o.Date)
            .DistinctBy(o => o.ItemID)
            .OrderBy(o => o.Date)
            .Where(o => o.Date <= dateFilter)
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
                .Where(o => o.ItemID == SelectedItem.ID)
                .OrderBy(o => o.Date)
        );

        if (!Events.Any())
        {
            Events.AddRange(_eventList.Where(o => o.ItemID == SelectedItem.ID));
        }

        var item = _itemList.First(o => o.ID == SelectedItem.ID);
        Image = FileRepsitory.GetImage<TItem>(item.ID);

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
