using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Repositories;

public static class HtmlHelper
{
    public static int GetYear(string str)
    {
        var years = Regex.Matches(str, @"\d{4}");
        var yearList = years.Select(o => Convert.ToInt32(o.Value));
        return yearList.FirstOrDefault(o => o > 1900 && o < 2999);
    }

    public static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
    }

    internal async static Task DownloadPNG(string webFile, string destinationFile)
    {
        if (string.IsNullOrWhiteSpace(webFile))
        {
            return;
        }

        destinationFile = $"{destinationFile}.png";

        FileRepsitory.Delete(destinationFile);

        if (File.Exists(destinationFile))
        {
            return;
        }

        var directory = Path.GetDirectoryName(destinationFile) ?? string.Empty;

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (webFile == null || webFile == "N/A")
        {
            return;
        }

        await DownloadFile(webFile, destinationFile);
    }

    public async static Task DownloadPNGFromWebpage(string url, string destinationFile)
    {
        FileRepsitory.Delete($"{destinationFile}.png");

        var htmlDocument = await DownloadWebpage(url);
        await DownloadPNGFromDocument(htmlDocument, url, destinationFile);
    }

    internal async static Task DownloadPNGFromDocument(HtmlDocument? htmlDocument, string url, string destinationFile)
    {
        var imageNode = htmlDocument?.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
            ?? htmlDocument?.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
        var imageUrl = imageNode?.GetAttributeValue("content", string.Empty);

        if (Uri.TryCreate(url, UriKind.Absolute, out var pageUri)
            && Uri.TryCreate(pageUri, imageUrl, out var absoluteImageUri))
        {
            await DownloadPNG(absoluteImageUri.ToString(), destinationFile);
        }
    }

    private async static Task DownloadFile(string imageUrl, string imagePath)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        using var ms = await response.Content.ReadAsStreamAsync();
        using var fs = File.Create(imagePath);
        await ms.CopyToAsync(fs);
        fs.Flush();
    }

    public static string CleanUrl(string url)
    {
        return url?.Trim() ?? string.Empty;
    }

    internal async static Task<HtmlDocument> DownloadWebpage(string url)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null!;
            }

            var text = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);

            return htmlDocument;
        }
        catch
        {
            return null!;
        }
    }
}
