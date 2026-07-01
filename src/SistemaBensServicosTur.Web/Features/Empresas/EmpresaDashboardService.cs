using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.Empresas;

public class EmpresaDashboardService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<EmpresaDashboardResult> SearchAsync(EmpresaDashboardQuery query)
    {
        await using var db = factory.CreateDbContext();

        var filter = db.Empresas.AsNoTracking()
            .Where(e => e.CidadeTenantId == query.CidadeId);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var lower = query.SearchText.ToLower();
            filter = filter.Where(e => e.NomeEmpresa.ToLower().Contains(lower));
        }

        if (query.CategoriaId.HasValue)
            filter = filter.Where(e => e.CategoriaId == query.CategoriaId.Value);

        if (query.TipoId.HasValue)
            filter = filter.Where(e => e.TipoId == query.TipoId.Value);

        if (query.SubtipoId.HasValue)
            filter = filter.Where(e => e.SubtipoId == query.SubtipoId.Value);

        var totalCount = await filter.CountAsync();
        var pageSize = Math.Max(1, query.PageSize);
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
        var pageNumber = Math.Clamp(query.PageNumber, 1, totalPages);

        var items = await filter
            .OrderBy(e => e.NomeEmpresa)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var displayStart = totalCount == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
        var displayEnd = Math.Min(pageNumber * pageSize, totalCount);

        return new EmpresaDashboardResult
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageNumber = pageNumber,
            DisplayStart = displayStart,
            DisplayEnd = displayEnd
        };
    }

    public async Task<Empresa> SaveAsync(Guid cidadeId, Guid? empresaId, EmpresaFormModel model, string? cidadeNome)
    {
        await using var db = factory.CreateDbContext();

        Empresa empresa;

        if (empresaId.HasValue)
        {
            empresa = await db.Empresas.FirstAsync(e => e.Id == empresaId.Value && e.CidadeTenantId == cidadeId);
        }
        else
        {
            empresa = new Empresa { CidadeTenantId = cidadeId };
            db.Empresas.Add(empresa);
        }

        empresa.NomeEmpresa = model.NomeEmpresa?.Trim() ?? "";
        empresa.RazaoSocial = model.RazaoSocial?.Trim();
        empresa.Cnpj = model.Cnpj?.Trim();
        empresa.Cidade = cidadeNome ?? model.Cidade?.Trim() ?? "";
        empresa.CategoriaId = model.CategoriaId;
        empresa.TipoId = model.TipoId;
        empresa.SubtipoId = model.SubtipoId;
        empresa.Endereco = model.Endereco?.Trim();
        empresa.Complemento = model.Complemento?.Trim();
        empresa.Latitude = model.Latitude;
        empresa.Longitude = model.Longitude;
        empresa.Telefone = model.Telefone?.Trim();
        empresa.Email = model.Email?.Trim();
        empresa.Site = model.Site?.Trim();
        empresa.Historia = model.Historia?.Trim();
        empresa.Cadastur = model.Cadastur?.Trim();
        empresa.FotoUrls = model.FotoUrls;

        await db.SaveChangesAsync();
        return empresa;
    }

    public async Task<EmpresaFormModel?> GetFormModelAsync(Guid cidadeId, Guid empresaId)
    {
        await using var db = factory.CreateDbContext();

        var empresa = await db.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId && e.CidadeTenantId == cidadeId);

        if (empresa is null) return null;

        return new EmpresaFormModel
        {
            NomeEmpresa = empresa.NomeEmpresa,
            RazaoSocial = empresa.RazaoSocial,
            Cnpj = empresa.Cnpj,
            Cidade = empresa.Cidade,
            CategoriaId = empresa.CategoriaId,
            TipoId = empresa.TipoId,
            SubtipoId = empresa.SubtipoId,
            Endereco = empresa.Endereco,
            Complemento = empresa.Complemento,
            Latitude = empresa.Latitude,
            Longitude = empresa.Longitude,
            Telefone = empresa.Telefone,
            Email = empresa.Email,
            Site = empresa.Site,
            Historia = empresa.Historia,
            Cadastur = empresa.Cadastur,
            FotoUrls = new List<string>(empresa.FotoUrls)
        };
    }
}
