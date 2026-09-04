using System;
using System.Threading.Tasks;
using WebLinks.Models;
using WebLinks.Repositories;

namespace Repositories;

public class Links : IExternal<Link>
{
    public async Task<Link> GetItem(string url)
    {
        url = HtmlHelper.CleanUrl(url);
        var htmlDocument = await HtmlHelper.DownloadWebpage(url);

        if (htmlDocument is null)
        {
            return new Link
            {
                Url = url
            };
        }

        var node = htmlDocument.DocumentNode.SelectSingleNode("//title");
        var title = node?.InnerHtml.Trim() ?? string.Empty;

        var imageNode = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
        var imageUrl = imageNode?.GetAttributeValue("content", string.Empty);

        if (Uri.TryCreate(url, UriKind.Absolute, out var pageUri)
            && Uri.TryCreate(pageUri, imageUrl, out var absoluteImageUri))
        {
            await HtmlHelper.DownloadPNG(absoluteImageUri.ToString(), Paths.GetTempPath<Link>());
        }

        return new Link
        {
            Url = url,
            Title = title
        };
    }
}
