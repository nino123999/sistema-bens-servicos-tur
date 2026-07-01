using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SistemaBensServicosTur.Web.Features.Relatorios;
using SistemaBensServicosTur.Web.Features.Users;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Relatorios
{
    private readonly List<CategoriaResumo> _porCategoria = new();
    private int _total;
    private int _comFotos;
    private int _comHistoria;
    private bool _loading = true;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await CarregarRelatorioAsync();
    }

    private async Task CarregarRelatorioAsync()
    {
        _loading = true;
        _errorMessage = null;

        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userContext = UserContextResolver.Resolve(authState.User);

            var result = await RelatorioService.GetRelatorioAsync(userContext.CidadeId, userContext.IsAdmin);

            _total = result.Total;
            _comFotos = result.ComFotos;
            _comHistoria = result.ComHistoria;

            _porCategoria.Clear();
            _porCategoria.AddRange(result.PorCategoria);
        }
        catch
        {
            _errorMessage = "Não foi possível carregar o relatório.";
        }
        finally
        {
            _loading = false;
        }
    }
}
