using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.Roteiros;

public class RoteiroService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<List<Roteiro>> GetAllAsync(Guid cidadeId)
    {
        await using var db = factory.CreateDbContext();
        return await db.Roteiros
            .AsNoTracking()
            .Where(r => r.CidadeTenantId == cidadeId)
            .Include(r => r.RoteiroEmpresas)
                .ThenInclude(re => re.Empresa)
            .OrderBy(r => r.Nome)
            .ToListAsync();
    }

    public async Task<List<Empresa>> GetAllEmpresasAsync(Guid cidadeId)
    {
        await using var db = factory.CreateDbContext();
        return await db.Empresas
            .AsNoTracking()
            .Where(e => e.CidadeTenantId == cidadeId)
            .OrderBy(e => e.NomeEmpresa)
            .ToListAsync();
    }

    public async Task<Roteiro?> GetByIdAsync(Guid cidadeId, Guid roteiroId)
    {
        await using var db = factory.CreateDbContext();
        return await db.Roteiros
            .AsNoTracking()
            .Where(r => r.Id == roteiroId && r.CidadeTenantId == cidadeId)
            .Include(r => r.RoteiroEmpresas)
                .ThenInclude(re => re.Empresa)
            .FirstOrDefaultAsync();
    }

    public async Task<Roteiro> SaveAsync(Guid cidadeId, Guid? roteiroId, RoteiroFormModel model)
    {
        await using var db = factory.CreateDbContext();

        Roteiro roteiro;

        if (roteiroId.HasValue)
        {
            roteiro = await db.Roteiros
                .Include(r => r.RoteiroEmpresas)
                .FirstAsync(r => r.Id == roteiroId.Value && r.CidadeTenantId == cidadeId);

            db.RoteiroEmpresas.RemoveRange(roteiro.RoteiroEmpresas);
            roteiro.RoteiroEmpresas.Clear();
        }
        else
        {
            roteiro = new Roteiro { CidadeTenantId = cidadeId };
            db.Roteiros.Add(roteiro);
        }

        roteiro.Nome = model.Nome?.Trim() ?? "";

        for (var i = 0; i < model.EmpresaIds.Count; i++)
        {
            roteiro.RoteiroEmpresas.Add(new RoteiroEmpresa
            {
                EmpresaId = model.EmpresaIds[i],
                Ordem = i + 1
            });
        }

        await db.SaveChangesAsync();
        return roteiro;
    }

    public async Task DeleteAsync(Guid cidadeId, Guid roteiroId)
    {
        await using var db = factory.CreateDbContext();

        var roteiro = await db.Roteiros
            .Include(r => r.RoteiroEmpresas)
            .FirstAsync(r => r.Id == roteiroId && r.CidadeTenantId == cidadeId);

        db.Roteiros.Remove(roteiro);
        await db.SaveChangesAsync();
    }
}
