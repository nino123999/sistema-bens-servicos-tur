namespace SistemaBensServicosTur.Web.Entities;

public class Tipo
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public Guid CategoriaId { get; set; }
}
