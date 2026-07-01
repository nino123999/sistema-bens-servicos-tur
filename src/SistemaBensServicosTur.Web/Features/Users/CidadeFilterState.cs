namespace SistemaBensServicosTur.Web.Features.Users;

public class CidadeFilterState
{
    private Guid? _cidadeSelecionadaId;
    private List<CidadeDisponivel> _cidadesDisponiveis = new();

    public event Action? OnChange;

    public bool IsAdmin { get; set; }

    public Guid? CidadeSelecionadaId
    {
        get => _cidadeSelecionadaId;
        set
        {
            if (_cidadeSelecionadaId == value) return;
            _cidadeSelecionadaId = value;
            OnChange?.Invoke();
        }
    }

    public IReadOnlyList<CidadeDisponivel> CidadesDisponiveis => _cidadesDisponiveis;

    public void SetCidades(List<CidadeDisponivel> cidades)
    {
        _cidadesDisponiveis = cidades;
    }
}
