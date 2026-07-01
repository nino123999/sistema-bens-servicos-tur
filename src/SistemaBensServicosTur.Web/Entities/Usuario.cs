namespace SistemaBensServicosTur.Web.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public bool IsAdmin { get; set; }
    public Guid? CidadeId { get; set; }
}
