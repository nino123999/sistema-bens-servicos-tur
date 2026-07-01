using Microsoft.AspNetCore.Components;
using SistemaBensServicosTur.Web.Features.EmpresaHistoria;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class EmpresaHistoria
{
    [Parameter] public Guid EmpresaId { get; set; }

    private Entities.Empresa? _empresa;
    private List<string> _fotoUrls = new();
    private bool _loading = true;
    private string? _errorMessage;

    protected override async Task OnParametersSetAsync()
    {
        await CarregarEmpresaAsync();
    }

    private async Task CarregarEmpresaAsync()
    {
        _loading = true;
        _errorMessage = null;
        _empresa = null;
        _fotoUrls = new List<string>();

        try
        {
            var result = await EmpresaHistoriaService.GetByIdAsync(EmpresaId);

            if (result is null)
            {
                _errorMessage = "Empresa não encontrada.";
                return;
            }

            _empresa = result.Empresa;
            _fotoUrls = result.FotoUrls;
        }
        catch
        {
            _errorMessage = "Não foi possível carregar a página de história.";
        }
        finally
        {
            _loading = false;
        }
    }

    private void VoltarParaEmpresas()
    {
        Navigation.NavigateTo("/empresas");
    }

    private static string ObterTextoOuPadrao(string? texto, string padrao)
    {
        return string.IsNullOrWhiteSpace(texto) ? padrao : texto.Trim();
    }
}
