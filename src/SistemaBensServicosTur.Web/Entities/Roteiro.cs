namespace SistemaBensServicosTur.Web.Entities;

public class Roteiro
{
    public Guid Id { get; set; }
    public Guid CidadeTenantId { get; set; }
    public string Nome { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? Descricao { get; set; }
    public bool Inicio { get; set; }
    public bool Fim { get; set; }
    public List<RoteiroEmpresa> RoteiroEmpresas { get; set; } = new();
}
