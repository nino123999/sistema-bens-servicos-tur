using System.Globalization;

namespace SistemaBensServicosTur.Web.Core;

public static class GeoHelpers
{
    public static string BuildMapsUrl(double latitude, double longitude)
    {
        var lat = latitude.ToString("G", CultureInfo.InvariantCulture);
        var lng = longitude.ToString("G", CultureInfo.InvariantCulture);
        return $"https://www.google.com/maps?q={lat},{lng}";
    }
}
