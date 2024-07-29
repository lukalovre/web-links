using System.Collections.Generic;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Models.Interfaces;

namespace Repositories;

public interface IDatasource
{
    void Add<T>(T item, Event e) where T : IItem;

    List<T> GetList<T>(string type) where T : IItem;

    List<Event> GetEventList(string type);

    List<Event> GetEventListConvert<T>() where T : IItem;

    void MakeBackup(string path);

    void Update<T>(T item) where T : IItem;
    void Update(Event item);
}
