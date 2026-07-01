namespace SistemaBensServicosTur.Web.Features.Admin;

public class UsuarioFormModel
{
    public string Username { get; set; } = "";
    public string Senha { get; set; } = "";
    public bool IsAdmin { get; set; }
    public Guid? CidadeId { get; set; }
}
