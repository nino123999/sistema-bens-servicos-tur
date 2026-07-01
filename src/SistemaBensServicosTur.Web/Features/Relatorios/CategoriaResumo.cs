namespace SistemaBensServicosTur.Web.Features.Relatorios;

public class CategoriaResumo
{
    public string NomeCategoria { get; set; } = "";
    public string Categoria => NomeCategoria;
    public int Total { get; set; }
    public int ComFotos { get; set; }
    public int ComHistoria { get; set; }
}
