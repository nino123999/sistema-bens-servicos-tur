namespace SistemaBensServicosTur.Web.Features.Mapa;

public class MapaResult
{
    public List<MapPoint> Points { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
