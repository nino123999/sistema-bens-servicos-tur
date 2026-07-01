using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Features.Empresas;

public class EmpresaDashboardResult
{
    public List<Empresa> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public int DisplayStart { get; set; }
    public int DisplayEnd { get; set; }
}
