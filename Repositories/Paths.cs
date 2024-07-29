using System;
using System.IO;
using AvaloniaApplication1.Repositories;
using AvaloniaApplication1.ViewModels;

namespace Repositories;

public static class Paths
{
    public static string SettingsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
    public static string Images => Path.Combine(GetRootPath(), "Images");
    public static string APIKeys => Path.Combine(GetRootPath(), ".Keys");
    public static string Data => GetRootPath();

    public static string GetImagePath<T>(int itemID) => Path.Combine(Images, Helpers.GetClassName<T>(), $"{itemID}.png");

    public static string GetTempPath<T>()
    {
        var path = Path.Combine(GetRootPath(), ".Temp", Helpers.GetClassName<T>());
        CreatePath(path);
        return path;
    }

    public static string GetImagesPath<T>()
    {
        var path = Path.Combine(Images, Helpers.GetClassName<T>());
        CreatePath(path);
        return path;
    }

    private static string GetRootPath()
    {
        var rootPath = Path.Combine(Settings.Instance.DatasourcePath, "Links");
        CreatePath(rootPath);
        return rootPath;
    }

    private static void CreatePath(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string GetAPIKeyFilePath(string fileName)
    {
        var keyFilePath = Path.Combine(APIKeys, fileName);

        if (!File.Exists(keyFilePath))
        {
            var directoryPath = Path.GetDirectoryName(keyFilePath);
            CreatePath(directoryPath ?? string.Empty);
            File.Create(keyFilePath);
        }

        return keyFilePath;
    }
}
