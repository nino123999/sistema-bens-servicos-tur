namespace SistemaBensServicosTur.Web.Features.Users;

public record UserContext(bool IsAuthenticated, bool IsAdmin, Guid? CidadeId, string? Username);
