using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Core;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.Mapa;

public class MapaService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<MapaResult> GetMapDataAsync(Guid? cidadeId, bool isAdmin)
    {
        await using var db = factory.CreateDbContext();

        if (!isAdmin && !cidadeId.HasValue)
            return new MapaResult { ErrorMessage = "Seu usuário não está vinculado a uma cidade." };

        var query = db.Empresas.AsNoTracking()
            .Where(e => e.Latitude.HasValue && e.Longitude.HasValue);

        if (!isAdmin && cidadeId.HasValue)
            query = query.Where(e => e.CidadeTenantId == cidadeId.Value);

        var empresas = await query.ToListAsync();

        var points = empresas.Select(e => new MapPoint(
            e.NomeEmpresa,
            e.Latitude!.Value,
            e.Longitude!.Value,
            GeoHelpers.BuildMapsUrl(e.Latitude.Value, e.Longitude.Value)
        )).ToList();

        return new MapaResult { Points = points };
    }
}
