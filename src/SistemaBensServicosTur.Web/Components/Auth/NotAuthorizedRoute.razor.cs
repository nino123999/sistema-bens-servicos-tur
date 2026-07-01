using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace SistemaBensServicosTur.Web.Components.Auth;

public partial class NotAuthorizedRoute
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool _authChecked;
    private bool _isAuthenticated;

    protected override async Task OnParametersSetAsync()
    {
        if (AuthenticationStateTask is null)
        {
            _authChecked = true;
            _isAuthenticated = false;
            return;
        }

        var authState = await AuthenticationStateTask;
        _isAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
        _authChecked = true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender || !_authChecked || _isAuthenticated)
        {
            return;
        }

        Navigation.NavigateTo(BuildLoginRedirectUrl(), true);
    }

    private string BuildLoginRedirectUrl()
    {
        var relativePath = Navigation.ToBaseRelativePath(Navigation.Uri);
        var returnUrl = string.IsNullOrWhiteSpace(relativePath) ? "/" : $"/{relativePath}";
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
        return $"/login?returnUrl={encodedReturnUrl}";
    }

}
