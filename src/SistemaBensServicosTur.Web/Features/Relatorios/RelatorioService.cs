using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.Relatorios;

public class RelatorioService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<RelatorioResult> GetRelatorioAsync(Guid? cidadeId, bool isAdmin)
    {
        await using var db = factory.CreateDbContext();

        var query = db.Empresas.AsNoTracking();

        if (!isAdmin && cidadeId.HasValue)
            query = query.Where(e => e.CidadeTenantId == cidadeId.Value);
        else if (!isAdmin)
            return new RelatorioResult();

        var empresas = await query.ToListAsync();

        var categoriaIds = empresas
            .Where(e => e.CategoriaId.HasValue)
            .Select(e => e.CategoriaId!.Value)
            .Distinct()
            .ToList();

        var categorias = await db.Categorias
            .AsNoTracking()
            .Where(c => categoriaIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nome);

        var porCategoria = empresas
            .GroupBy(e => e.CategoriaId)
            .Where(g => g.Key.HasValue)
            .Select(g => new CategoriaResumo
            {
                NomeCategoria = categorias.GetValueOrDefault(g.Key!.Value, "Desconhecida"),
                Total = g.Count(),
                ComFotos = g.Count(e => e.FotoUrls.Count > 0),
                ComHistoria = g.Count(e => !string.IsNullOrWhiteSpace(e.Historia))
            })
            .OrderBy(c => c.NomeCategoria)
            .ToList();

        return new RelatorioResult
        {
            Total = empresas.Count,
            ComFotos = empresas.Count(e => e.FotoUrls.Count > 0),
            ComHistoria = empresas.Count(e => !string.IsNullOrWhiteSpace(e.Historia)),
            PorCategoria = porCategoria
        };
    }
}
