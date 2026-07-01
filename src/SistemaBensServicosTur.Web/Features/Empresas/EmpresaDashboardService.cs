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
        empresa.Cep = model.Cep?.Trim();
        empresa.Bairro = model.Bairro?.Trim();
        empresa.Latitude = model.Latitude;
        empresa.Longitude = model.Longitude;
        empresa.Telefone = model.Telefone?.Trim();
        empresa.TelefoneEmpresa = model.TelefoneEmpresa?.Trim();
        empresa.CelularEmpresa = model.CelularEmpresa?.Trim();
        empresa.WhatsAppEmpresa = model.WhatsAppEmpresa?.Trim();
        empresa.Email = model.Email?.Trim();
        empresa.EmailEmpresa = model.EmailEmpresa?.Trim();
        empresa.Site = model.Site?.Trim();
        empresa.SiteEmpresa = model.SiteEmpresa?.Trim();
        empresa.InstagramEmpresa = model.InstagramEmpresa?.Trim();
        empresa.FacebookEmpresa = model.FacebookEmpresa?.Trim();
        empresa.Proprietario1 = model.Proprietario1?.Trim();
        empresa.CelularProprietario1 = model.CelularProprietario1?.Trim();
        empresa.WhatsAppProprietario1 = model.WhatsAppProprietario1?.Trim();
        empresa.InformacoesServicos = model.InformacoesServicos?.Trim();
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
            Cep = empresa.Cep,
            Bairro = empresa.Bairro,
            Latitude = empresa.Latitude,
            Longitude = empresa.Longitude,
            Telefone = empresa.Telefone,
            TelefoneEmpresa = empresa.TelefoneEmpresa,
            CelularEmpresa = empresa.CelularEmpresa,
            WhatsAppEmpresa = empresa.WhatsAppEmpresa,
            Email = empresa.Email,
            EmailEmpresa = empresa.EmailEmpresa,
            Site = empresa.Site,
            SiteEmpresa = empresa.SiteEmpresa,
            InstagramEmpresa = empresa.InstagramEmpresa,
            FacebookEmpresa = empresa.FacebookEmpresa,
            Proprietario1 = empresa.Proprietario1,
            CelularProprietario1 = empresa.CelularProprietario1,
            WhatsAppProprietario1 = empresa.WhatsAppProprietario1,
            InformacoesServicos = empresa.InformacoesServicos,
            Historia = empresa.Historia,
            Cadastur = empresa.Cadastur,
            FotoUrls = new List<string>(empresa.FotoUrls)
        };
    }
}
