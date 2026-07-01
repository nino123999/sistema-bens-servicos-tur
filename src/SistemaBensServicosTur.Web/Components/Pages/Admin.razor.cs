using Microsoft.AspNetCore.Components;
using SistemaBensServicosTur.Web.Features.Admin;
using SistemaBensServicosTur.Web.Entities;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Admin
{
    private readonly List<Cidade> _cidades = new();
    private readonly List<UsuarioResumo> _usuarios = new();

    private CidadeFormModel _cidadeModel = new();
    private UsuarioFormModel _usuarioModel = new();

    private bool _loading = true;
    private bool _salvandoCidade;
    private bool _salvandoUsuario;
    private string? _cidadeFeedback;
    private bool _cidadeFeedbackError;
    private string? _usuarioFeedback;
    private bool _usuarioFeedbackError;

    protected override async Task OnInitializedAsync()
    {
        await CarregarDadosAsync();
    }

    private async Task CarregarDadosAsync()
    {
        _loading = true;

        _cidades.Clear();
        _cidades.AddRange(await AdminService.GetCidadesAsync());

        _usuarios.Clear();
        _usuarios.AddRange(await AdminService.GetUsuariosResumoAsync());

        if (!_usuarioModel.CidadeId.HasValue && _cidades.Count > 0)
        {
            _usuarioModel.CidadeId = _cidades[0].Id;
        }

        _loading = false;
    }

    private async Task SalvarCidadeAsync()
    {
        if (_salvandoCidade) return;

        _salvandoCidade = true;
        _cidadeFeedback = null;
        _cidadeFeedbackError = false;

        try
        {
            var (success, error) = await AdminService.CreateCidadeAsync(_cidadeModel);

            if (!success)
            {
                _cidadeFeedback = error;
                _cidadeFeedbackError = true;
                return;
            }

            _cidadeModel = new CidadeFormModel();
            _cidadeFeedback = "Cidade cadastrada com sucesso.";
            _cidadeFeedbackError = false;

            await CarregarDadosAsync();
        }
        catch
        {
            _cidadeFeedback = "Não foi possível cadastrar a cidade.";
            _cidadeFeedbackError = true;
        }
        finally
        {
            _salvandoCidade = false;
        }
    }

    private async Task SalvarUsuarioAsync()
    {
        if (_salvandoUsuario) return;

        _salvandoUsuario = true;
        _usuarioFeedback = null;
        _usuarioFeedbackError = false;

        try
        {
            var (success, error) = await AdminService.CreateUsuarioAsync(_usuarioModel);

            if (!success)
            {
                _usuarioFeedback = error;
                _usuarioFeedbackError = true;
                return;
            }

            _usuarioModel = new UsuarioFormModel
            {
                CidadeId = _cidades.Count > 0 ? _cidades[0].Id : null
            };
            _usuarioFeedback = "Usuário cadastrado com sucesso.";
            _usuarioFeedbackError = false;

            await CarregarDadosAsync();
        }
        catch
        {
            _usuarioFeedback = "Não foi possível cadastrar o usuário.";
            _usuarioFeedbackError = true;
        }
        finally
        {
            _salvandoUsuario = false;
        }
    }

    private static string GetAlertClass(bool error)
    {
        return error
            ? "mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-800"
            : "mt-4 rounded-xl border border-green-200 bg-green-50 px-4 py-3 text-sm font-semibold text-green-800";
    }
}
