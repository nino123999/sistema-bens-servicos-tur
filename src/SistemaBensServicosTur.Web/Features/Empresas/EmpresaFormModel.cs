using System.ComponentModel.DataAnnotations;

namespace SistemaBensServicosTur.Web.Features.Empresas;

public class EmpresaFormModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(200)]
    public string NomeEmpresa { get; set; } = "";

    [MaxLength(200)]
    public string? RazaoSocial { get; set; }

    [MaxLength(18)]
    public string? Cnpj { get; set; }

    public string? Cidade { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? TipoId { get; set; }
    public Guid? SubtipoId { get; set; }

    [MaxLength(300)]
    public string? Endereco { get; set; }

    public string? Complemento { get; set; }

    [MaxLength(9)]
    public string? Cep { get; set; }

    [MaxLength(100)]
    public string? Bairro { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? Telefone { get; set; }

    [MaxLength(15)]
    public string? TelefoneEmpresa { get; set; }

    [MaxLength(15)]
    public string? CelularEmpresa { get; set; }

    [MaxLength(15)]
    public string? WhatsAppEmpresa { get; set; }

    public string? Email { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    public string? EmailEmpresa { get; set; }

    public string? Site { get; set; }

    [MaxLength(200)]
    public string? SiteEmpresa { get; set; }

    [MaxLength(200)]
    public string? InstagramEmpresa { get; set; }

    [MaxLength(200)]
    public string? FacebookEmpresa { get; set; }

    [MaxLength(200)]
    public string? Proprietario1 { get; set; }

    [MaxLength(15)]
    public string? CelularProprietario1 { get; set; }

    [MaxLength(15)]
    public string? WhatsAppProprietario1 { get; set; }

    public string? InformacoesServicos { get; set; }
    public string? Historia { get; set; }
    public string? Cadastur { get; set; }
    public List<string> FotoUrls { get; set; } = new();
}
