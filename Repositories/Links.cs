using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Repositories;

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
        var title = node.InnerHtml.Trim();

        return new Link
        {
            Url = url,
            Title = title
        };
    }
}
