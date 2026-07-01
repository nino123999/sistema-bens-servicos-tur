using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.EmpresaHistoria;

public class EmpresaHistoriaService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<EmpresaHistoriaResult?> GetByIdAsync(Guid empresaId)
    {
        await using var db = factory.CreateDbContext();

        var empresa = await db.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId);

        if (empresa is null) return null;

        return new EmpresaHistoriaResult
        {
            Empresa = empresa,
            FotoUrls = new List<string>(empresa.FotoUrls)
        };
    }
}
