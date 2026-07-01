using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SistemaBensServicosTur.Web.Features.Empresas;
using SistemaBensServicosTur.Web.Features.Users;
using SistemaBensServicosTur.Web.Components.Shared;
using SistemaBensServicosTur.Web.Entities;
using SistemaBensServicosTur.Web.Infrastructure.Security;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Home : IDisposable
{
    private static readonly IReadOnlyList<SidePanelTab> SidebarTabs =
    [
        new("geral", "Geral", "<path d=\"M4 5h16M4 12h16M4 19h10\" stroke-linecap=\"round\" />"),
        new("endereco", "Endereço", "<path d=\"M12 21s7-5.4 7-11a7 7 0 1 0-14 0c0 5.6 7 11 7 11z\" /><circle cx=\"12\" cy=\"10\" r=\"2.4\" />"),
        new("contato", "Contato", "<path d=\"M5 6.5A2.5 2.5 0 0 1 7.5 4h9A2.5 2.5 0 0 1 19 6.5v11A2.5 2.5 0 0 1 16.5 20h-9A2.5 2.5 0 0 1 5 17.5v-11z\" /><path d=\"M8 8h8M8 12h8M8 16h5\" stroke-linecap=\"round\" />"),
        new("mais", "Mais", "<circle cx=\"12\" cy=\"12\" r=\"8\" /><path d=\"M12 8v8M8 12h8\" stroke-linecap=\"round\" />"),
        new("historia", "História", "<path d=\"M7 4h8l4 4v12H7a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z\" /><path d=\"M15 4v5h4M8.5 13h7M8.5 16h5\" stroke-linecap=\"round\" />"),
        new("cadastur", "Cadastur", "<path d=\"M8 3h8l2 4v14H6V7l2-4z\" /><path d=\"M8 7h8M9 12h6M9 16h6\" stroke-linecap=\"round\" />")
    ];

    private readonly List<Empresa> _empresas = new();
    private EmpresaFormModel _model = new();
    private Empresa? _empresaExclusaoPendente;
    private Cidade? _cidadeAtual;
    private Guid? _selectedEmpresaId;
    private Guid? _cidadeUsuarioId;
    private Guid? _categoriaId;
    private Guid? _tipoId;
    private Guid? _subtipoId;
    private bool _isAdmin;
    private bool _contextoCarregado;
    private bool _loading;
    private bool _isPanelOpen;
    private bool _isCreatingNew;
    private bool _isSaving;
    private bool _isUploadingPhotos;
    private bool _isDeleting;
    private int _totalEmpresas;
    private int _currentPage = 1;
    private int _pageSize = 10;
    private string _searchText = string.Empty;
    private string _tabAtiva = "geral";
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        DashboardActions.AddRequested += AbrirNovoPainelAsync;
        CidadeFilterState.OnChange += OnCidadeFilterStateChanged;
        await CarregarContextoUsuarioAsync();
        await CarregarEmpresasAsync();

        if (DashboardActions.ConsumePendingAddRequest())
        {
            await AbrirNovoPainelAsync();
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

        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        if (_isAdmin)
        {
            var cidades = await dbContext.Cidades
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .ToListAsync();

            CidadeFilterState.IsAdmin = true;
            CidadeFilterState.SetCidades(cidades.Select(c => new CidadeDisponivel(c.Id, c.Nome)).ToList());

            if (!CidadeFilterState.CidadeSelecionadaId.HasValue && cidades.Count > 0)
            {
                CidadeFilterState.CidadeSelecionadaId = cidades[0].Id;
            }

            await AtualizarCidadeAtualParaAdmin();
            _contextoCarregado = true;
            return;
        }

        if (!userContext.CidadeId.HasValue)
        {
            _cidadeAtual = null;
            _errorMessage = "Seu usuário não está vinculado a uma cidade.";
            _contextoCarregado = true;
            return;
        }

        _cidadeUsuarioId = userContext.CidadeId.Value;

        _cidadeAtual = await dbContext.Cidades
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == _cidadeUsuarioId.Value);

        if (_cidadeAtual is null)
        {
            _errorMessage = "Cidade vinculada ao usuário não encontrada.";
        }

        _contextoCarregado = true;
    }

    private async void OnCidadeFilterStateChanged()
    {
        _currentPage = 1;
        await AtualizarCidadeAtualParaAdmin();
        FecharPainel();
        await CarregarEmpresasAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task AtualizarCidadeAtualParaAdmin()
    {
        if (!_isAdmin) return;
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        var cidadeId = CidadeFilterState.CidadeSelecionadaId;
        var cidadeInfo = CidadeFilterState.CidadesDisponiveis.FirstOrDefault(c => c.Id == cidadeId);
        var cidadeData = await dbContext.Cidades.FirstOrDefaultAsync(x => x.Id == cidadeId);

        _cidadeAtual = cidadeInfo is not null
            ? new Cidade { Id = cidadeInfo.Id, Nome = cidadeInfo.Nome, FotoUrl = cidadeData.FotoUrl, FotoSecretariaUrl = cidadeData.FotoSecretariaUrl }
            : null;
    }

    private Guid? ObterCidadeFiltroAtual()
    {
        return _isAdmin ? CidadeFilterState.CidadeSelecionadaId : _cidadeUsuarioId;
    }

    private async Task CarregarEmpresasAsync()
    {
        if (!_contextoCarregado) return;

        _loading = true;
        _errorMessage = null;

        try
        {
            var cidadeFiltroId = ObterCidadeFiltroAtual();
            if (!cidadeFiltroId.HasValue)
            {
                _empresas.Clear();
                _totalEmpresas = 0;
                _errorMessage = _isAdmin
                    ? "Cadastre e selecione uma cidade para visualizar os estabelecimentos."
                    : "Seu usuário não está vinculado a uma cidade.";
                return;
            }

            var result = await EmpresaDashboardService.SearchAsync(new EmpresaDashboardQuery(
                cidadeFiltroId.Value,
                _searchText,
                _categoriaId,
                _tipoId,
                _subtipoId,
                _currentPage,
                _pageSize));

            _empresas.Clear();
            _empresas.AddRange(result.Items);
            _totalEmpresas = result.TotalCount;
            _currentPage = result.PageNumber;
        }
        catch
        {
            _empresas.Clear();
            _totalEmpresas = 0;
            _errorMessage = "Não foi possível carregar os estabelecimentos.";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnSearchTextChanged(string value)
    {
        _searchText = value;
        _currentPage = 1;
        await CarregarEmpresasAsync();
    }

    private Task OnCategoriaChanged(Guid? value) { _categoriaId = value; _currentPage = 1; return Task.CompletedTask; }
    private Task OnTipoChanged(Guid? value) { _tipoId = value; _currentPage = 1; return Task.CompletedTask; }
    private Task OnSubtipoChanged(Guid? value) { _subtipoId = value; _currentPage = 1; return Task.CompletedTask; }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        _currentPage = pageNumber;
        await CarregarEmpresasAsync();
    }

    private async Task ClearFiltersAsync()
    {
        _searchText = string.Empty;
        _categoriaId = null;
        _tipoId = null;
        _subtipoId = null;
        _currentPage = 1;
        await CarregarEmpresasAsync();
    }

    private async Task AbrirNovoPainelAsync()
    {
        NovaEmpresa();
        await InvokeAsync(StateHasChanged);
    }

    private void NovaEmpresa()
    {
        if (!ObterCidadeFiltroAtual().HasValue)
        {
            _errorMessage = "Selecione uma cidade para criar um cadastro.";
            return;
        }

        _selectedEmpresaId = null;
        _isCreatingNew = true;
        _isPanelOpen = true;
        _tabAtiva = "geral";
        _errorMessage = null;
        _model = new EmpresaFormModel { Cidade = _cidadeAtual?.Nome };
    }

    private async Task SelecionarEmpresaAsync(Guid empresaId)
    {
        _selectedEmpresaId = empresaId;
        _isCreatingNew = false;
        _isPanelOpen = true;
        _tabAtiva = "geral";
        _errorMessage = null;

        var cidadeFiltroId = ObterCidadeFiltroAtual();
        if (!cidadeFiltroId.HasValue)
        {
            _errorMessage = "Nenhuma cidade selecionada para carregar o cadastro.";
            return;
        }

        var formModel = await EmpresaDashboardService.GetFormModelAsync(cidadeFiltroId.Value, empresaId);
        if (formModel is null)
        {
            _errorMessage = "Cadastro não encontrado.";
            FecharPainel();
            return;
        }

        _model = formModel;
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
                _errorMessage = "Selecione uma cidade para salvar o cadastro.";
                return;
            }

            var empresa = await EmpresaDashboardService.SaveAsync(
                cidadeFiltroId.Value,
                _selectedEmpresaId.HasValue && !_isCreatingNew ? _selectedEmpresaId : null,
                _model,
                _cidadeAtual?.Nome);

            _selectedEmpresaId = empresa.Id;
            _isCreatingNew = false;
            _currentPage = 1;
            FecharPainel();
            await CarregarEmpresasAsync();
        }
        catch
        {
            _errorMessage = "Não foi possível salvar o cadastro.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private Task ExcluirAsync(Empresa empresa)
    {
        _errorMessage = null;
        _empresaExclusaoPendente = empresa;
        return Task.CompletedTask;
    }

    private void CancelarExclusao()
    {
        if (_isDeleting) return;
        _empresaExclusaoPendente = null;
    }

    private async Task ConfirmarExclusaoAsync()
    {
        if (_empresaExclusaoPendente is null || _isDeleting) return;

        var empresa = _empresaExclusaoPendente;
        _isDeleting = true;
        _errorMessage = null;

        try
        {
            var cidadeFiltroId = ObterCidadeFiltroAtual();
            if (!cidadeFiltroId.HasValue)
            {
                _errorMessage = "Selecione uma cidade para excluir o cadastro.";
                return;
            }

            var deleted = await EmpresaService.DeleteAsync(cidadeFiltroId.Value, empresa.Id);
            if (!deleted)
            {
                _empresaExclusaoPendente = null;
                await CarregarEmpresasAsync();
                return;
            }

            if (_selectedEmpresaId == empresa.Id) FecharPainel();

            _empresaExclusaoPendente = null;
            await CarregarEmpresasAsync();
        }
        catch
        {
            _errorMessage = "Não foi possível excluir o cadastro.";
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private async Task BaixarQrCodeHistoriaAsync(Guid empresaId)
    {
        try
        {
            var qrBytes = await EmpresaService.GenerateQrCodePngAsync(empresaId, Navigation.BaseUri);
            if (qrBytes is null)
            {
                _errorMessage = "Não foi possível gerar o QR Code.";
                return;
            }

            var base64 = Convert.ToBase64String(qrBytes);
            await Js.InvokeVoidAsync("downloadFileFromBase64", $"historia-empresa-{empresaId:N}.png", "image/png", base64);
        }
        catch
        {
            _errorMessage = "Não foi possível gerar o QR Code da história.";
        }
    }

    private async Task OnPhotosSelectedAsync(InputFileChangeEventArgs args)
    {
        _isUploadingPhotos = true;
        _errorMessage = null;

        try
        {
            var files = args.GetMultipleFiles(12)
                .Where(f => f.Size > 0 && f.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var savedUrls = await EmpresaService.SavePhotosAsync(files);
            _model.FotoUrls.AddRange(savedUrls);
        }
        catch
        {
            _errorMessage = "Não foi possível enviar uma ou mais fotos.";
        }
        finally
        {
            _isUploadingPhotos = false;
        }
    }

    private void RemoverFoto(string fotoUrl)
    {
        _model.FotoUrls.RemoveAll(existing => string.Equals(existing, fotoUrl, StringComparison.OrdinalIgnoreCase));
    }

    private void DefinirTabAtiva(string tab) => _tabAtiva = tab;
    private void FecharPainel() => _isPanelOpen = false;

    private static string ObterTextoOuTraco(string? valor, string fallback = "-")
    {
        return string.IsNullOrWhiteSpace(valor) ? fallback : valor.Trim();
    }

    public void Dispose()
    {
        DashboardActions.AddRequested -= AbrirNovoPainelAsync;
        CidadeFilterState.OnChange -= OnCidadeFilterStateChanged;
    }
}
