namespace SistemaBensServicosTur.Web.Entities;

public class Subtipo
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nome { get; set; } = "";
    public Guid TipoId { get; set; }
}
