namespace SistemaBensServicosTur.Web.Features.Relatorios;

public class RelatorioResult
{
    public int Total { get; set; }
    public int ComFotos { get; set; }
    public int ComHistoria { get; set; }
    public List<CategoriaResumo> PorCategoria { get; set; } = new();
}
