using System.Threading.Tasks;
using WebLinks.Models.Interfaces;

namespace WebLinks.Repositories;

public interface IExternal<T> where T : IItem
{
    public Task<T> GetItem(string url);
}
