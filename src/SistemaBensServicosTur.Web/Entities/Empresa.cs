namespace SistemaBensServicosTur.Web.Entities;

public class Empresa
{
    public Guid Id { get; set; }
    public string NomeEmpresa { get; set; } = "";
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public Guid CidadeTenantId { get; set; }
    public string Cidade { get; set; } = "";
    public Guid? CategoriaId { get; set; }
    public Guid? TipoId { get; set; }
    public Guid? SubtipoId { get; set; }
    public string? Endereco { get; set; }
    public string? Complemento { get; set; }
    public string? Cep { get; set; }
    public string? Bairro { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Telefone { get; set; }
    public string? TelefoneEmpresa { get; set; }
    public string? CelularEmpresa { get; set; }
    public string? WhatsAppEmpresa { get; set; }
    public string? Email { get; set; }
    public string? EmailEmpresa { get; set; }
    public string? Site { get; set; }
    public string? SiteEmpresa { get; set; }
    public string? InstagramEmpresa { get; set; }
    public string? FacebookEmpresa { get; set; }
    public string? Proprietario1 { get; set; }
    public string? CelularProprietario1 { get; set; }
    public string? WhatsAppProprietario1 { get; set; }
    public string? InformacoesServicos { get; set; }
    public string? Historia { get; set; }
    public string? Cadastur { get; set; }
    public List<string> FotoUrls { get; set; } = new();

    public Cidade? CidadeTenant { get; set; }
}
