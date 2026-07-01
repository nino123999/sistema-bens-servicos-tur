using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SistemaBensServicosTur.Web.Infrastructure.Data;

namespace SistemaBensServicosTur.Web.Features.Empresas;

public class EmpresaService(IDbContextFactory<CadastroInvturDbContext> factory, IWebHostEnvironment env)
{
    public async Task<bool> DeleteAsync(Guid cidadeId, Guid empresaId)
    {
        await using var db = factory.CreateDbContext();

        var empresa = await db.Empresas
            .FirstOrDefaultAsync(e => e.Id == empresaId && e.CidadeTenantId == cidadeId);

        if (empresa is null) return false;

        db.Empresas.Remove(empresa);
        await db.SaveChangesAsync();
        return true;
    }

    public Task<byte[]?> GenerateQrCodePngAsync(Guid empresaId, string baseUri)
    {
        var url = $"{baseUri.TrimEnd('/')}/empresa-historia/{empresaId}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var bytes = qrCode.GetGraphic(20);

        return Task.FromResult<byte[]?>(bytes);
    }

    public async Task<List<string>> SavePhotosAsync(IReadOnlyList<IBrowserFile> files)
    {
        var urls = new List<string>();
        var uploadPath = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.Name).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            await using var fs = File.Create(filePath);
            await stream.CopyToAsync(fs);

            urls.Add($"/uploads/{fileName}");
        }

        return urls;
    }
}
