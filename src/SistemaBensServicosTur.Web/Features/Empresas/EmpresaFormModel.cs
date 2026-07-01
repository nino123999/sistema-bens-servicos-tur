namespace SistemaBensServicosTur.Web.Features.Empresas;

public class EmpresaFormModel
{
    public string NomeEmpresa { get; set; } = "";
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public string? Cidade { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? TipoId { get; set; }
    public Guid? SubtipoId { get; set; }
    public string? Endereco { get; set; }
    public string? Complemento { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Site { get; set; }
    public string? Historia { get; set; }
    public string? Cadastur { get; set; }
    public List<string> FotoUrls { get; set; } = new();
}
