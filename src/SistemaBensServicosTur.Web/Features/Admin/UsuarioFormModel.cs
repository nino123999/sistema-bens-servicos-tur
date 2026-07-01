using System.ComponentModel.DataAnnotations;

namespace SistemaBensServicosTur.Web.Features.Admin;

public class UsuarioFormModel
{
    [Required(ErrorMessage = "O usuário é obrigatório.")]
    public string Username { get; set; } = "";

    public string NomeUsuario
    {
        get => Username;
        set => Username = value;
    }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = "";

    public bool IsAdmin { get; set; }
    public Guid? CidadeId { get; set; }
}
