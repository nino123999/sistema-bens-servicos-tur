using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SistemaBensServicosTur.Web.Components.Shared;

namespace SistemaBensServicosTur.Web.Components.Layout;

public partial class TopNavbar
{
    private bool _mobileMenuOpen;

    [Inject]
    private CidadeFilterState _cidadeFilterState { get; set; } = default!;

    private void ToggleMobileMenu()
    {
        _mobileMenuOpen = !_mobileMenuOpen;
    }

    private async Task AbrirCadastroAsync()
    {
        _mobileMenuOpen = false;

        var handled = await DashboardActions.RequestAddAsync();
        if (!handled)
        {
            Navigation.NavigateTo("/");
        }
    }

    private static string GetDisplayName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? "Usuário" : name.Trim();
    }

    private static string GetInitials(string? name)
    {
        var displayName = GetDisplayName(name);
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "US";
        }

        if (parts.Length == 1)
        {
            return parts[0].Length == 1
                ? parts[0][0].ToString().ToUpperInvariant()
                : parts[0][..2].ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }
    
    private string NavClass(string href)
    {
        var current = Navigation.ToBaseRelativePath(Navigation.Uri).TrimEnd('/');
        var target = href.TrimStart('/').TrimEnd('/');
        var isActive = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);
        return isActive
            ? "flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-primary bg-surface no-underline"
            : "flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-secondary no-underline transition-all hover:bg-surface hover:text-primary";
    }

    private string MobileNavClass(string href)
    {
        var current = Navigation.ToBaseRelativePath(Navigation.Uri).TrimEnd('/');
        var target = href.TrimStart('/').TrimEnd('/');
        var isActive = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);
        return isActive
            ? "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-semibold text-primary bg-surface no-underline"
            : "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-semibold text-secondary no-underline hover:bg-surface hover:text-primary";
    }
    
    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
        _cidadeFilterState.OnChange += OnCidadeFilterStateChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnCidadeFilterStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task OnCidadeChangedAsync(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var cidadeId))
        {
            _cidadeFilterState.CidadeSelecionadaId = cidadeId;
        }
        else
        {
            _cidadeFilterState.CidadeSelecionadaId = null;
        }
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        _cidadeFilterState.OnChange -= OnCidadeFilterStateChanged;
    }
}
