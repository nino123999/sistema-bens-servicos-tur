using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaBensServicosTur.Web.Components.Pages;

public partial class Login
{
    [SupplyParameterFromQuery(Name = "error")]
    public string? Error { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    private string ReturnUrlValue { get; set; } = "/";

    private string? ErrorMessage
        => string.Equals(Error, "invalid", StringComparison.OrdinalIgnoreCase)
            ? "Usuário ou senha inválidos."
            : null;

    protected override async Task OnParametersSetAsync()
    {
        ReturnUrlValue = NormalizeReturnUrl(ReturnUrl);

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated ?? false)
        {
            Navigation.NavigateTo(ReturnUrlValue, true);
        }
    }

    private static string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "/";
        }

        if (!returnUrl.StartsWith("/", StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal) ||
            returnUrl.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        return Uri.TryCreate(returnUrl, UriKind.Relative, out _)
            ? returnUrl
            : "/";
    }
}
