using System.ComponentModel.DataAnnotations;
using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Features.Roteiros;

public class RoteiroFormModel
{
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "O título é obrigatório.")]
    [MaxLength(200)]
    public string Titulo { get; set; } = "";

    public string? Descricao { get; set; }
    public bool Inicio { get; set; }
    public bool Fim { get; set; }
    public List<Guid> EmpresaIds { get; set; } = new();

    public static RoteiroFormModel FromEntity(Roteiro roteiro) => new()
    {
        Nome = roteiro.Nome,
        Titulo = roteiro.Titulo,
        Descricao = roteiro.Descricao,
        Inicio = roteiro.Inicio,
        Fim = roteiro.Fim,
        EmpresaIds = roteiro.RoteiroEmpresas
            .OrderBy(re => re.Ordem)
            .Select(re => re.EmpresaId)
            .ToList()
    };
}
