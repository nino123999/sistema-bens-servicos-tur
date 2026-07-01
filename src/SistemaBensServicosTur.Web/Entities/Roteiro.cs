namespace SistemaBensServicosTur.Web.Entities;

public class Roteiro
{
    public Guid Id { get; set; }
    public Guid CidadeTenantId { get; set; }
    public string Nome { get; set; } = "";
    public List<RoteiroEmpresa> RoteiroEmpresas { get; set; } = new();
}
