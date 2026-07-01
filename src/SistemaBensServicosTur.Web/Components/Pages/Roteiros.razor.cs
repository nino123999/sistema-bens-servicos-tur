using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SistemaBensServicosTur.Web.Core;
using SistemaBensServicosTur.Web.Components.Shared;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Features.Roteiros;
using SistemaBensServicosTur.Web.Features.Users;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Roteiros : IDisposable
{
    private readonly List<Roteiro> _roteiros = new();
    private readonly List<Empresa> _empresasDisponiveis = new();
    private RoteiroFormModel _model = new();
    private Roteiro? _roteiroExclusaoPendente;
    private Guid? _roteiroSelecionadoId;
    private Guid? _cidadeUsuarioId;
    private bool _isAdmin;
    private bool _contextoCarregado;
    private bool _loading = true;
    private bool _isPanelOpen;
    private bool _isCreatingNew;
    private bool _isSaving;
    private bool _isDeleting;
    private string? _errorMessage;
    private readonly string MapElementId = $"roteiro-map-{Guid.NewGuid():N}";
    private bool _mapNeedsRender;

    protected override async Task OnInitializedAsync()
    {
        CidadeFilterState.OnChange += OnCidadeFilterStateChanged;
        await CarregarContextoUsuarioAsync();
        await CarregarDadosAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_mapNeedsRender || _loading)
        {
            return;
        }

        _mapNeedsRender = false;

        var roteiroAtivo = _roteiros.FirstOrDefault(r => r.Id == _roteiroSelecionadoId);
        if (roteiroAtivo is null)
        {
            return;
        }

        var routePoints = roteiroAtivo.RoteiroEmpresas
            .OrderBy(re => re.Ordem)
            .Where(re => re.Empresa?.Latitude.HasValue == true && re.Empresa?.Longitude.HasValue == true)
            .Select(re => new
            {
                title = re.Empresa!.NomeEmpresa,
                latitude = re.Empresa.Latitude!.Value,
                longitude = re.Empresa.Longitude!.Value,
                mapsUrl = GeoHelpers.BuildMapsUrl(re.Empresa.Latitude.Value, re.Empresa.Longitude.Value)
            })
            .ToList();

        if (routePoints.Count > 0)
        {
            await Js.InvokeVoidAsync("roteiroMap.render", MapElementId, routePoints);
        }
    }

    private async Task CarregarContextoUsuarioAsync()
    {
        _contextoCarregado = false;
        _errorMessage = null;

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var userContext = UserContextResolver.Resolve(authState.User);

        if (!userContext.IsAuthenticated)
        {
            Navigation.NavigateTo("/login", true);
            return;
        }

        _isAdmin = userContext.IsAdmin;

        if (!_isAdmin && !userContext.CidadeId.HasValue)
        {
            _errorMessage = "Seu usuário não está vinculado a uma cidade.";
            _contextoCarregado = true;
            return;
        }

        _cidadeUsuarioId = userContext.CidadeId;
        _contextoCarregado = true;
    }

    private async void OnCidadeFilterStateChanged()
    {
        _roteiroSelecionadoId = null;
        await CarregarDadosAsync();
        await InvokeAsync(StateHasChanged);
    }

    private Guid? ObterCidadeFiltroAtual()
    {
        return _isAdmin ? CidadeFilterState.CidadeSelecionadaId : _cidadeUsuarioId;
    }

    private async Task CarregarDadosAsync()
    {
        if (!_contextoCarregado) return;

        _loading = true;
        _errorMessage = null;

        try
        {
            var cidadeFiltroId = ObterCidadeFiltroAtual();
            if (!cidadeFiltroId.HasValue)
            {
                _roteiros.Clear();
                _empresasDisponiveis.Clear();
                _errorMessage = _isAdmin
                    ? "Selecione uma cidade para visualizar os roteiros."
                    : "Seu usuário não está vinculado a uma cidade.";
                return;
            }

            _roteiros.Clear();
            var roteiros = await RoteiroService.GetAllAsync(cidadeFiltroId.Value);
            _roteiros.AddRange(roteiros);

            _empresasDisponiveis.Clear();
            var empresas = await RoteiroService.GetAllEmpresasAsync(cidadeFiltroId.Value);
            _empresasDisponiveis.AddRange(empresas);

            if (_roteiros.Count > 0 && _roteiroSelecionadoId is null)
            {
                _roteiroSelecionadoId = _roteiros[0].Id;
                _mapNeedsRender = true;
            }
        }
        catch
        {
            _errorMessage = "Não foi possível carregar os roteiros.";
        }
        finally
        {
            _loading = false;
        }
    }

    private void SelecionarRoteiro(Guid roteiroId)
    {
        if (_roteiroSelecionadoId == roteiroId) return;

        _roteiroSelecionadoId = roteiroId;
        _mapNeedsRender = true;
        StateHasChanged();
    }

    private void AbrirNovoRoteiro()
    {
        if (!ObterCidadeFiltroAtual().HasValue)
        {
            _errorMessage = "Selecione uma cidade para criar um roteiro.";
            return;
        }

        _isCreatingNew = true;
        _isPanelOpen = true;
        _errorMessage = null;
        _model = new RoteiroFormModel();
    }

    private async Task EditarRoteiro(Guid roteiroId)
    {
        var cidadeFiltroId = ObterCidadeFiltroAtual();
        if (!cidadeFiltroId.HasValue)
        {
            _errorMessage = "Selecione uma cidade para editar.";
            return;
        }

        var roteiro = await RoteiroService.GetByIdAsync(cidadeFiltroId.Value, roteiroId);
        if (roteiro is null)
        {
            _errorMessage = "Roteiro não encontrado.";
            return;
        }

        _isCreatingNew = false;
        _isPanelOpen = true;
        _errorMessage = null;
        _model = RoteiroFormModel.FromEntity(roteiro);
    }

    private async Task SalvarAsync()
    {
        if (_isSaving) return;

        _isSaving = true;
        _errorMessage = null;

        try
        {
            var cidadeFiltroId = ObterCidadeFiltroAtual();
            if (!cidadeFiltroId.HasValue)
            {
                _errorMessage = "Selecione uma cidade para salvar.";
                return;
            }

            Guid? roteiroId = _isCreatingNew ? null : _roteiroSelecionadoId;
            var roteiro = await RoteiroService.SaveAsync(cidadeFiltroId.Value, roteiroId, _model);

            _roteiroSelecionadoId = roteiro.Id;
            _isCreatingNew = false;
            FecharPainel();
            await CarregarDadosAsync();
        }
        catch
        {
            _errorMessage = "Não foi possível salvar o roteiro.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void ExcluirRoteiro(Roteiro roteiro)
    {
        _errorMessage = null;
        _roteiroExclusaoPendente = roteiro;
    }

    private void CancelarExclusao()
    {
        if (_isDeleting) return;
        _roteiroExclusaoPendente = null;
    }

    private async Task ConfirmarExclusaoAsync()
    {
        if (_roteiroExclusaoPendente is null || _isDeleting) return;

        var roteiro = _roteiroExclusaoPendente;
        _isDeleting = true;
        _errorMessage = null;

        try
        {
            var cidadeFiltroId = ObterCidadeFiltroAtual();
            if (!cidadeFiltroId.HasValue)
            {
                _errorMessage = "Selecione uma cidade para excluir.";
                return;
            }

            await RoteiroService.DeleteAsync(cidadeFiltroId.Value, roteiro.Id);

            if (_roteiroSelecionadoId == roteiro.Id)
            {
                _roteiroSelecionadoId = null;
            }

            _roteiroExclusaoPendente = null;
            await CarregarDadosAsync();
        }
        catch
        {
            _errorMessage = "Não foi possível excluir o roteiro.";
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private void FecharPainel()
    {
        _isPanelOpen = false;
    }

    private void OnEmpresaSelecionada(ChangeEventArgs e)
    {
        if (e.Value is string selectedValue && Guid.TryParse(selectedValue, out var empresaId))
        {
            if (!_model.EmpresaIds.Contains(empresaId))
            {
                _model.EmpresaIds.Add(empresaId);
            }
        }
    }

    private void MoverEmpresa(int index, int direction)
    {
        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= _model.EmpresaIds.Count) return;

        (_model.EmpresaIds[index], _model.EmpresaIds[newIndex]) = (_model.EmpresaIds[newIndex], _model.EmpresaIds[index]);
    }

    private void RemoverEmpresa(Guid empresaId)
    {
        _model.EmpresaIds.Remove(empresaId);
    }

    public void Dispose()
    {
        CidadeFilterState.OnChange -= OnCidadeFilterStateChanged;
    }
}
