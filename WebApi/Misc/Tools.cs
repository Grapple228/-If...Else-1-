namespace WebApi.Misc;

public static class Tools
{
    public static string GetUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}";
    }
}