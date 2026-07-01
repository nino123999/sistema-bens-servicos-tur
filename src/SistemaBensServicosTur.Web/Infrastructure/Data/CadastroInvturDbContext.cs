using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Infrastructure.Data;

public class CadastroInvturDbContext(DbContextOptions<CadastroInvturDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cidade> Cidades => Set<Cidade>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Tipo> Tipos => Set<Tipo>();
    public DbSet<Subtipo> Subtipos => Set<Subtipo>();
    public DbSet<Roteiro> Roteiros => Set<Roteiro>();
    public DbSet<RoteiroEmpresa> RoteiroEmpresas => Set<RoteiroEmpresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FotoUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
        });

        modelBuilder.Entity<Cidade>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Tipo>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.HasOne<Categoria>().WithMany().HasForeignKey(t => t.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subtipo>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.HasOne<Tipo>().WithMany().HasForeignKey(s => s.TipoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Roteiro>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<RoteiroEmpresa>(entity =>
        {
            entity.HasKey(re => new { re.RoteiroId, re.EmpresaId });
            entity.HasOne<Roteiro>()
                .WithMany(r => r.RoteiroEmpresas)
                .HasForeignKey(re => re.RoteiroId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(re => re.Empresa)
                .WithMany()
                .HasForeignKey(re => re.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}
