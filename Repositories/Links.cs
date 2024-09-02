using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Repositories;

namespace Repositories;

public class Links : IExternal<Link>
{
    public async Task<Link> GetItem(string url)
    {
        var htmlDocument = await HtmlHelper.DownloadWebpage(url);

        if (htmlDocument is null)
        {
            return default!;
        }

        var node = htmlDocument.DocumentNode.SelectSingleNode("//title");
        var ttle = node.InnerHtml.Trim();

        return new Link
        {
            Url = url,
            Title = ttle
        };
    }
}
