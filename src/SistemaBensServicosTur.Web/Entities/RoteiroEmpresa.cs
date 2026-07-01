namespace SistemaBensServicosTur.Web.Entities;

public class RoteiroEmpresa
{
    public Guid RoteiroId { get; set; }
    public Guid EmpresaId { get; set; }
    public int Ordem { get; set; }

    public Empresa? Empresa { get; set; }
}
