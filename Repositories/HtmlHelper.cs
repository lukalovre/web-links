using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
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

        try
        {
            var htmlDocument = await DownloadWebpage(url);
            var imageUrl = GetImageUrl(htmlDocument, url);

            if (imageUrl is null)
            {
                imageUrl = await FindImageWithDuckDuckGo(url);
            }

            await DownloadPNG(imageUrl ?? string.Empty, destinationFile);
        }
        catch
        {
            // Image discovery is optional; an unavailable website must not prevent item loading.
        }
    }

    internal async static Task DownloadPNGFromDocument(HtmlDocument? htmlDocument, string url, string destinationFile)
    {
        var imageUrl = GetImageUrl(htmlDocument, url);

        await DownloadPNG(imageUrl ?? string.Empty, destinationFile);
    }

    private static string? GetImageUrl(HtmlDocument? htmlDocument, string url)
    {
        var imageNode = htmlDocument?.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
            ?? htmlDocument?.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
        var imageUrl = imageNode?.GetAttributeValue("content", string.Empty);

        return Uri.TryCreate(url, UriKind.Absolute, out var pageUri)
            && Uri.TryCreate(pageUri, imageUrl, out var absoluteImageUri)
            ? absoluteImageUri.ToString()
            : null;
    }

    private async static Task<string?> FindImageWithDuckDuckGo(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var pageUri))
        {
            return null;
        }

        var siteName = pageUri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
            ? pageUri.Host[4..]
            : pageUri.Host;
        var searchUrl = $"https://duckduckgo.com/?q={Uri.EscapeDataString($"{siteName} logo")}&iax=images&ia=images";
        var searchDocument = await DownloadWebpage(searchUrl);
        var pageHtml = searchDocument?.DocumentNode.OuterHtml ?? string.Empty;
        var vqdMatch = Regex.Match(pageHtml, "vqd=\\\"([^\\\"]+)\\\"");

        if (vqdMatch.Success)
        {
            var imageSearchUrl = $"https://duckduckgo.com/i.js?l=us-en&o=json&q={Uri.EscapeDataString($"{siteName} logo")}&vqd={Uri.EscapeDataString(vqdMatch.Groups[1].Value)}&f=,,,&p=1";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            using var response = await client.GetAsync(imageSearchUrl);

            if (response.IsSuccessStatusCode)
            {
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var apiImageUrl = json.RootElement
                    .GetProperty("results")[0]
                    .GetProperty("image")
                    .GetString();

                if (Uri.TryCreate(apiImageUrl, UriKind.Absolute, out _))
                {
                    return apiImageUrl;
                }
            }
        }

        var imageNode = searchDocument?.DocumentNode.SelectSingleNode("//img[contains(@class, 'tile--img__img')]")
            ?? searchDocument?.DocumentNode.SelectSingleNode("//img[@data-src]");
        var imageUrl = imageNode?.GetAttributeValue("data-src", string.Empty);

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            imageUrl = imageNode?.GetAttributeValue("src", string.Empty);
        }

        imageUrl = WebUtility.HtmlDecode(imageUrl);

        return Uri.TryCreate(searchUrl, UriKind.Absolute, out var searchUri)
            && Uri.TryCreate(searchUri, imageUrl, out var absoluteImageUri)
            ? absoluteImageUri.ToString()
            : null;
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
