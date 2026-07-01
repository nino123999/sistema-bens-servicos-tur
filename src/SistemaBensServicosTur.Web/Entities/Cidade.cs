namespace SistemaBensServicosTur.Web.Entities;

public class Cidade
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string? FotoUrl { get; set; }
    public string? FotoSecretariaUrl { get; set; }
}
