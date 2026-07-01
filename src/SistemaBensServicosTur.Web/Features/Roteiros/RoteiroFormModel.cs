using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Features.Roteiros;

public class RoteiroFormModel
{
    public string Nome { get; set; } = "";
    public List<Guid> EmpresaIds { get; set; } = new();

    public static RoteiroFormModel FromEntity(Roteiro roteiro) => new()
    {
        Nome = roteiro.Nome,
        EmpresaIds = roteiro.RoteiroEmpresas
            .OrderBy(re => re.Ordem)
            .Select(re => re.EmpresaId)
            .ToList()
    };
}
