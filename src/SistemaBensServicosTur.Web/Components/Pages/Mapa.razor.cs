using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SistemaBensServicosTur.Web.Features.Mapa;
using SistemaBensServicosTur.Web.Features.Users;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Mapa
{
    private readonly List<MapPoint> _mapPoints = new();
    private readonly string MapElementId = $"estabelecimentos-map-{Guid.NewGuid():N}";
    private bool _loading = true;
    private bool _mapNeedsRender;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await CarregarMapaAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_mapNeedsRender || _loading || _mapPoints.Count == 0)
        {
            return;
        }

        _mapNeedsRender = false;
        await Js.InvokeVoidAsync("estabelecimentosMap.render", MapElementId, _mapPoints);
    }

    private async Task CarregarMapaAsync()
    {
        _loading = true;
        _errorMessage = null;

        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userContext = UserContextResolver.Resolve(authState.User);

            var result = await MapaService.GetMapDataAsync(userContext.CidadeId, userContext.IsAdmin);

            if (result.ErrorMessage is not null)
            {
                _errorMessage = result.ErrorMessage;
                _mapPoints.Clear();
                return;
            }

            _mapPoints.Clear();
            _mapPoints.AddRange(result.Points);
            _mapNeedsRender = _mapPoints.Count > 0;
        }
        catch
        {
            _errorMessage = "Não foi possível carregar os pontos do mapa.";
            _mapPoints.Clear();
        }
        finally
        {
            _loading = false;
        }
    }
}
