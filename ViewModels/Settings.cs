using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Repositories;

namespace AvaloniaApplication1.ViewModels;

public class Settings
{

    public string DatasourcePath { get; set; } = string.Empty;

    public Dictionary<string, ItemSettingsJson> ItemSettings { get; set; } = [];

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Include,
        Formatting = Formatting.Indented,
    };

    private static Settings? _instance;

    public static Settings Instance
    {
        get
        {
            _instance ??= Load();
            return _instance;
        }
    }

    private static Settings Load()
    {
        if (!File.Exists(Paths.SettingsFilePath))
        {
            File.WriteAllText(Paths.SettingsFilePath, JsonConvert.SerializeObject(new Settings(), _jsonSettings));
        }

        var settingsText = File.ReadAllText(Paths.SettingsFilePath);
        return JsonConvert.DeserializeObject<Settings>(settingsText) ?? new Settings();
    }

    internal static void Save()
    {
        var text = JsonConvert.SerializeObject(Instance, Formatting.Indented);
        File.WriteAllText(Paths.SettingsFilePath, text);
    }

    internal ItemSettings GetItemSettigns<T>()
    {
        var itemTypeName = typeof(T).Name;

        if (!ItemSettings.TryGetValue(itemTypeName, out var result))
        {
            return new();
        }

        return new ItemSettings
        {
            AmountToMinutesModifier = result.AmountToMinutesModifier,
            AmountVerb = result.AmountVerb,
            DateTimeFilter = ConvertDateTimeFilter(result.DateTimeFilter),
            DefaultAddAmount = result.DefaultAddAmount,
            DefaultNewItemBookmakred = result.DefaultNewItemBookmakred,
            DefaultNewItemChapter = result.DefaultNewItemChapter,
            DefaultNewItemCompleted = result.DefaultNewItemCompleted,
            DefaultNewItemPlatform = result.DefaultNewItemPlatform,
            DefaultNewItemRating = result.DefaultNewItemRating,
            IsFullAmountDefaultValue = result.IsFullAmountDefaultValue,
            NewItemAmountOverride = result.NewItemAmountOverride,
            OpenItemLinkUrl = result.OpenItemLinkUrl,
            ShowYearFilter = result.ShowYearFilter
        };
    }

    private static DateTime? ConvertDateTimeFilter(string dateTimeFilter)
    {
        return dateTimeFilter switch
        {
            "min" => DateTime.MinValue,
            "-24" => DateTime.Now.AddHours(-24),
            _ => null
        };
    }
}