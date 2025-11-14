namespace WebLinks.Repositories;

public static class Helpers
{
    public static string GetClassName<T>()
    {
        return typeof(T).Name;
    }
}