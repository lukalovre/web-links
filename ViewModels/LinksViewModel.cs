using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Media.Imaging;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Repositories;
using AvaloniaApplication1.ViewModels.Extensions;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public class LinksViewModel : ViewModelBase, IDataGrid
{
    public LinksViewModel(IDatasource datasource, IExternal<Link> external)
    {
        _settings = Settings.Instance.GetItemSettigns<Link>();

        _datasource = datasource;
        _external = external;

        GridFilterViewModel = new GridFilterViewModel(this)
        {
            ShowYearFilter = _settings.ShowYearFilter
        };

        GridItems = [];
        GridItemsBookmarked = [];
        ReloadData();

        Events = [];
        EventViewModel = new EventViewModel(Events);

        AddItemClick = ReactiveCommand.Create(AddItemClickAction);
        OpenLink = ReactiveCommand.Create(OpenLinkAction);
        Unfollow = ReactiveCommand.Create(UnfollowAction);
        OpenImage = ReactiveCommand.Create(OpenImageAction);

        SelectedGridItem = GridItems.LastOrDefault()!;
        NewEvent = new Event();
        NewItem = (Link)Activator.CreateInstance(typeof(Link))!;

        IsFullAmount = _settings.IsFullAmountDefaultValue;
    }

    private void UnfollowAction()
    {
        var lastEvent = Events.MaxBy(o => o.Date) ?? Events.LastOrDefault();

        if (lastEvent is null)
        {
            return;
        }

        lastEvent.Bookmarked = null;
        _datasource.Update(lastEvent);

        ReloadData();
    }

    protected DateTime? DateTimeFilter
    {
        get
        {
            return _settings.DateTimeFilter ?? new DateTime(GridFilterViewModel.YearFilter, 1, 1);
        }
    }

    private readonly ItemSettings _settings = new();
    private readonly IDatasource _datasource;
    private readonly IExternal<Link> _external;
    private LinkGridItem _selectedGridItem = default!;
    private List<Link> _itemList = [];
    protected List<Event> _eventList = [];
    private Link _newItem = default!;
    private Bitmap? _itemImage;
    private Bitmap? _newItemImage;
    private Event _newEvent = new();
    private bool _useNewDate;
    private Link _selectedItem = default!;

    private int _gridCountItemsBookmarked;

    public EventViewModel EventViewModel { get; }
    public GridFilterViewModel GridFilterViewModel { get; }

    private string _inputUrl = string.Empty;
    private bool _isFullAmount;
    private int _newItemAmount;
    private string _selectedCategory;

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

    public ObservableCollection<LinkGridItem> GridItems { get; set; }
    public ObservableCollection<LinkGridItem> GridItemsBookmarked { get; set; }

    public Link SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public ObservableCollection<Event> Events { get; set; }

    public ReactiveCommand<Unit, Unit> AddItemClick { get; }
    public ReactiveCommand<Unit, Unit> OpenLink { get; }
    public ReactiveCommand<Unit, Unit> Unfollow { get; }
    public ReactiveCommand<Unit, Unit> OpenImage { get; }

    public Link NewItem
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

    public LinkGridItem SelectedGridItem
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

    public ObservableCollection<string> Categories { get; set; }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    private async void InputUrlChanged()
    {
        NewItem = await _external.GetItem(InputUrl);

        NewImage = FileRepsitory.GetImageTemp<Link>();
        NewEvent = new Event();

        _inputUrl = string.Empty;
    }

    private void OpenImageAction()
    {
        throw new NotImplementedException();
    }

    protected virtual List<string> GetAlternativeOpenLinkSearchParams() => [];

    private void OpenLinkAction()
    {
        HtmlHelper.OpenLink(SelectedItem.Url);

        NewEvent ??= new Event();

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            Date = DateTime.Now,
            Bookmarked = true
        };

        _datasource.Add(SelectedItem, newEvent);

        ReloadData();
    }

    private void AddItemClickAction()
    {
        NewEvent ??= new Event();

        var newEvent = new Event
        {
            ID = 0,
            ItemID = 0,
            Date = DateTime.Now,
            Bookmarked = true
        };

        _datasource.Add(NewItem, newEvent);

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
        NewItem = (Link)Activator.CreateInstance(typeof(Link))!;
    }

    internal List<LinkGridItem> LoadData(string searchText = null!)
    {
        var type = Helpers.GetClassName<Link>();

        _itemList = _datasource.GetList<Link>(type);
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

    protected List<LinkGridItem> LoadDataBookmarked(int? yearsAgo = null)
    {
        var type = Helpers.GetClassName<Link>();
        _itemList = _datasource.GetList<Link>(type);
        _eventList = _datasource.GetEventList(type);

        var dateFilter = yearsAgo.HasValue
            ? DateTime.Now.AddYears(-yearsAgo.Value)
            : DateTime.MaxValue;

        return _eventList
            .OrderByDescending(o => o.Date)
            .DistinctBy(o => o.ItemID)
            .OrderByDescending(o => o.Date)
            .Where(o => o.Date <= dateFilter)
            .Where(o => o.Bookmarked.HasValue && o.Bookmarked.Value == true)
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

    protected LinkGridItem Convert(Event e, Link i, IEnumerable<Event> eventList)
    {
        return new LinkGridItem(
            i.ID,
            i.Title,
            i.Category,
            eventList.LastEventDate());
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
        Image = FileRepsitory.GetImage<Link>(item.ID);

    }

    private ObservableCollection<LinkGridItem> GetSelectedGrid()
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
