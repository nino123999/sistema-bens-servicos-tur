namespace SistemaBensServicosTur.Web.Features.Empresas;

public record EmpresaDashboardQuery(
    Guid CidadeId,
    string? SearchText = null,
    Guid? CategoriaId = null,
    Guid? TipoId = null,
    Guid? SubtipoId = null,
    int PageNumber = 1,
    int PageSize = 10);
