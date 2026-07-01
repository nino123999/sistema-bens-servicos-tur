using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Features.Empresas;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Tests;

public class EmpresaDashboardServiceTests
{
    [Fact]
    public async Task SaveAsync_creates_and_edits_empresa()
    {
        var fixture = await CreateFixtureAsync();
        var service = new EmpresaDashboardService(fixture.Factory);

        var added = await service.SaveAsync(
            fixture.City.Id,
            null,
            new EmpresaFormModel
            {
                NomeEmpresa = " Restaurante Central ",
                Cnpj = " 12.345.678/0001-90 ",
                CategoriaId = fixture.FoodCategory.Id,
                TipoId = fixture.FoodType.Id,
                SubtipoId = fixture.RestaurantSubtype.Id
            },
            fixture.City.Nome);

        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.Equal("Restaurante Central", added.NomeEmpresa);
        Assert.Equal("12.345.678/0001-90", added.Cnpj);
        Assert.Equal(fixture.City.Id, added.CidadeTenantId);
        Assert.Equal(fixture.City.Nome, added.Cidade);
        Assert.Equal(fixture.RestaurantSubtype.Id, added.SubtipoId);

        var edited = await service.SaveAsync(
            fixture.City.Id,
            added.Id,
            new EmpresaFormModel
            {
                NomeEmpresa = "Hotel Central",
                Cnpj = "98.765.432/0001-10",
                Cidade = fixture.City.Nome,
                CategoriaId = fixture.HostingCategory.Id,
                TipoId = fixture.HostingType.Id,
                SubtipoId = fixture.HotelSubtype.Id
            },
            fixture.City.Nome);

        Assert.Equal(added.Id, edited.Id);
        Assert.Equal("Hotel Central", edited.NomeEmpresa);
        Assert.Equal("98.765.432/0001-10", edited.Cnpj);
        Assert.Equal(fixture.HotelSubtype.Id, edited.SubtipoId);

        await using var dbContext = fixture.Factory.CreateDbContext();
        var empresas = await dbContext.Empresas.AsNoTracking().ToListAsync();
        Assert.Single(empresas);
        Assert.Equal("Hotel Central", empresas[0].NomeEmpresa);
    }

    [Fact]
    public async Task SearchAsync_filters_by_name_category_type_subtype_and_city()
    {
        var fixture = await CreateFixtureAsync();
        var service = new EmpresaDashboardService(fixture.Factory);

        await service.SaveAsync(fixture.City.Id, null, new EmpresaFormModel
        {
            NomeEmpresa = "Restaurante Central",
            CategoriaId = fixture.FoodCategory.Id,
            TipoId = fixture.FoodType.Id,
            SubtipoId = fixture.RestaurantSubtype.Id
        }, fixture.City.Nome);

        await service.SaveAsync(fixture.City.Id, null, new EmpresaFormModel
        {
            NomeEmpresa = "Hotel Atlântico",
            CategoriaId = fixture.HostingCategory.Id,
            TipoId = fixture.HostingType.Id,
            SubtipoId = fixture.HotelSubtype.Id
        }, fixture.City.Nome);

        await service.SaveAsync(fixture.OtherCity.Id, null, new EmpresaFormModel
        {
            NomeEmpresa = "Restaurante Fora da Cidade",
            CategoriaId = fixture.FoodCategory.Id,
            TipoId = fixture.FoodType.Id,
            SubtipoId = fixture.RestaurantSubtype.Id
        }, fixture.OtherCity.Nome);

        var byName = await service.SearchAsync(new EmpresaDashboardQuery(fixture.City.Id, SearchText: "hotel"));
        Assert.Single(byName.Items);
        Assert.Equal("Hotel Atlântico", byName.Items[0].NomeEmpresa);

        var byCategory = await service.SearchAsync(new EmpresaDashboardQuery(fixture.City.Id, CategoriaId: fixture.FoodCategory.Id));
        Assert.Single(byCategory.Items);
        Assert.Equal("Restaurante Central", byCategory.Items[0].NomeEmpresa);

        var byType = await service.SearchAsync(new EmpresaDashboardQuery(fixture.City.Id, TipoId: fixture.HostingType.Id));
        Assert.Single(byType.Items);
        Assert.Equal("Hotel Atlântico", byType.Items[0].NomeEmpresa);

        var bySubtype = await service.SearchAsync(new EmpresaDashboardQuery(fixture.City.Id, SubtipoId: fixture.RestaurantSubtype.Id));
        Assert.Single(bySubtype.Items);
        Assert.Equal("Restaurante Central", bySubtype.Items[0].NomeEmpresa);

        Assert.DoesNotContain(bySubtype.Items, empresa => empresa.CidadeTenantId == fixture.OtherCity.Id);
    }

    [Fact]
    public async Task SearchAsync_paginates_and_clamps_requested_page()
    {
        var fixture = await CreateFixtureAsync();
        var service = new EmpresaDashboardService(fixture.Factory);

        for (var i = 1; i <= 25; i++)
        {
            await service.SaveAsync(fixture.City.Id, null, new EmpresaFormModel
            {
                NomeEmpresa = $"Empresa {i:000}",
                CategoriaId = fixture.FoodCategory.Id,
                TipoId = fixture.FoodType.Id,
                SubtipoId = fixture.RestaurantSubtype.Id
            }, fixture.City.Nome);
        }

        var secondPage = await service.SearchAsync(new EmpresaDashboardQuery(
            fixture.City.Id,
            PageNumber: 2,
            PageSize: 10));

        Assert.Equal(25, secondPage.TotalCount);
        Assert.Equal(3, secondPage.TotalPages);
        Assert.Equal(2, secondPage.PageNumber);
        Assert.Equal(10, secondPage.Items.Count);
        Assert.Equal(11, secondPage.DisplayStart);
        Assert.Equal(20, secondPage.DisplayEnd);
        Assert.Equal("Empresa 011", secondPage.Items[0].NomeEmpresa);

        var clampedPage = await service.SearchAsync(new EmpresaDashboardQuery(
            fixture.City.Id,
            PageNumber: 99,
            PageSize: 10));

        Assert.Equal(3, clampedPage.PageNumber);
        Assert.Equal(5, clampedPage.Items.Count);
        Assert.Equal(21, clampedPage.DisplayStart);
        Assert.Equal(25, clampedPage.DisplayEnd);
        Assert.Equal("Empresa 021", clampedPage.Items[0].NomeEmpresa);
    }

    private static async Task<TestFixture> CreateFixtureAsync()
    {
        var options = new DbContextOptionsBuilder<CadastroInvturDbContext>()
            .UseInMemoryDatabase($"empresa-dashboard-tests-{Guid.NewGuid():N}")
            .EnableSensitiveDataLogging()
            .Options;

        var factory = new TestDbContextFactory(options);

        await using var dbContext = factory.CreateDbContext();

        var city = new Cidade { Nome = "Cidade Teste" };
        var otherCity = new Cidade { Nome = "Outra Cidade" };
        dbContext.Cidades.AddRange(city, otherCity);
        await dbContext.SaveChangesAsync();

        var foodCategory = new Categoria { Codigo = "1", Nome = "Alimentos" };
        var hostingCategory = new Categoria { Codigo = "2", Nome = "Hospedagem" };
        dbContext.Categorias.AddRange(foodCategory, hostingCategory);
        await dbContext.SaveChangesAsync();

        var foodType = new Tipo
        {
            Codigo = "1.1",
            Nome = "Restaurantes",
            CategoriaId = foodCategory.Id
        };
        var hostingType = new Tipo
        {
            Codigo = "2.1",
            Nome = "Hotéis",
            CategoriaId = hostingCategory.Id
        };
        dbContext.Tipos.AddRange(foodType, hostingType);
        await dbContext.SaveChangesAsync();

        var restaurantSubtype = new Subtipo
        {
            Codigo = "1.1.1",
            Nome = "Restaurante",
            TipoId = foodType.Id
        };
        var hotelSubtype = new Subtipo
        {
            Codigo = "2.1.1",
            Nome = "Hotel",
            TipoId = hostingType.Id
        };
        dbContext.Subtipos.AddRange(restaurantSubtype, hotelSubtype);
        await dbContext.SaveChangesAsync();

        return new TestFixture(
            factory,
            city,
            otherCity,
            foodCategory,
            hostingCategory,
            foodType,
            hostingType,
            restaurantSubtype,
            hotelSubtype);
    }

    private sealed record TestFixture(
        TestDbContextFactory Factory,
        Cidade City,
        Cidade OtherCity,
        Categoria FoodCategory,
        Categoria HostingCategory,
        Tipo FoodType,
        Tipo HostingType,
        Subtipo RestaurantSubtype,
        Subtipo HotelSubtype);

    private sealed class TestDbContextFactory(DbContextOptions<CadastroInvturDbContext> options) : IDbContextFactory<CadastroInvturDbContext>
    {
        public CadastroInvturDbContext CreateDbContext() => new(options);

        public ValueTask<CadastroInvturDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => new(CreateDbContext());
    }
}
