using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Infrastructure.Data;
using SistemaBensServicosTur.Web.Infrastructure.Security;

namespace SistemaBensServicosTur.Web.Features.Admin;

public class AdminService(IDbContextFactory<CadastroInvturDbContext> factory)
{
    public async Task<List<Cidade>> GetCidadesAsync()
    {
        await using var db = factory.CreateDbContext();
        return await db.Cidades.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<List<UsuarioResumo>> GetUsuariosResumoAsync()
    {
        await using var db = factory.CreateDbContext();

        var usuarios = await db.Usuarios.AsNoTracking().ToListAsync();
        var cidadeIds = usuarios.Where(u => u.CidadeId.HasValue).Select(u => u.CidadeId!.Value).Distinct().ToList();
        var cidades = await db.Cidades.AsNoTracking()
            .Where(c => cidadeIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Nome);

        return usuarios
            .Select(u => new UsuarioResumo(
                u.Id,
                u.Username,
                u.IsAdmin,
                u.CidadeId.HasValue ? cidades.GetValueOrDefault(u.CidadeId.Value) : null))
            .OrderBy(u => u.Username)
            .ToList();
    }

    public async Task<(bool Success, string? Error)> CreateCidadeAsync(CidadeFormModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Nome))
            return (false, "O nome da cidade é obrigatório.");

        await using var db = factory.CreateDbContext();

        var exists = await db.Cidades.AnyAsync(c => c.Nome.ToLower() == model.Nome.ToLower().Trim());
        if (exists)
            return (false, "Já existe uma cidade com esse nome.");

        db.Cidades.Add(new Cidade
        {
            Nome = model.Nome.Trim(),
            FotoUrl = model.FotoUrl?.Trim(),
            FotoSecretariaUrl = model.FotoSecretariaUrl?.Trim()
        });

        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CreateUsuarioAsync(UsuarioFormModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username))
            return (false, "O usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(model.Senha))
            return (false, "A senha é obrigatória.");

        if (!model.IsAdmin && !model.CidadeId.HasValue)
            return (false, "Selecione uma cidade para o usuário.");

        await using var db = factory.CreateDbContext();

        var exists = await db.Usuarios.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower().Trim());
        if (exists)
            return (false, "Já existe um usuário com esse nome.");

        db.Usuarios.Add(new Usuario
        {
            Username = model.Username.Trim(),
            SenhaHash = PasswordHelper.Hash(model.Senha),
            IsAdmin = model.IsAdmin,
            CidadeId = model.IsAdmin ? null : model.CidadeId
        });

        await db.SaveChangesAsync();
        return (true, null);
    }
}
