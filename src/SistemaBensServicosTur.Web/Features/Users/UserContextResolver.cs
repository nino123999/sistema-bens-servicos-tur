using System.Security.Claims;
using SistemaBensServicosTur.Web.Infrastructure.Security;

namespace SistemaBensServicosTur.Web.Features.Users;

public static class UserContextResolver
{
    public static UserContext Resolve(ClaimsPrincipal user)
    {
        if (!(user.Identity?.IsAuthenticated ?? false))
            return new UserContext(false, false, null, null);

        var isAdmin = user.IsInRole(AuthConstants.Roles.Admin);
        var cidadeIdStr = user.FindFirstValue(AuthConstants.Claims.CidadeId);
        var cidadeId = Guid.TryParse(cidadeIdStr, out var g) ? (Guid?)g : null;
        var username = user.FindFirstValue(ClaimTypes.Name);

        return new UserContext(true, isAdmin, cidadeId, username);
    }
}
