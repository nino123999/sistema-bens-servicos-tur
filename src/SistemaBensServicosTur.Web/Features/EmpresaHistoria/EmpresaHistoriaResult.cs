using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Features.EmpresaHistoria;

public class EmpresaHistoriaResult
{
    public Empresa Empresa { get; set; } = default!;
    public List<string> FotoUrls { get; set; } = new();
}
