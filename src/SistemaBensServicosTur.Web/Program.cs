using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaBensServicosTur.Web.Components;
using SistemaBensServicosTur.Web.Features.Admin;
using SistemaBensServicosTur.Web.Features.EmpresaHistoria;
using SistemaBensServicosTur.Web.Features.Empresas;
using SistemaBensServicosTur.Web.Features.Mapa;
using SistemaBensServicosTur.Web.Features.Relatorios;
using SistemaBensServicosTur.Web.Features.Roteiros;
using SistemaBensServicosTur.Web.Features.Users;
using SistemaBensServicosTur.Web.Infrastructure.Data;
using SistemaBensServicosTur.Web.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? "Host=localhost;Port=5432;Database=sistema_bens_servicos_tur;Username=postgres;Password=postgres";

builder.Services.AddDbContextFactory<CadastroInvturDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<EmpresaDashboardService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<RoteiroService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<MapaService>();
builder.Services.AddScoped<EmpresaHistoriaService>();

builder.Services.AddScoped<CidadeFilterState>();
builder.Services.AddScoped<DashboardActions>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapPost("/auth/login", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString().Trim();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        ctx.Response.Redirect("/login?error=invalid");
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CadastroInvturDbContext>>();
    await using var db = factory.CreateDbContext();

    var usuario = await db.Usuarios
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Username == username);

    if (usuario is null || !PasswordHelper.Verify(password, usuario.SenhaHash))
    {
        ctx.Response.Redirect("/login?error=invalid");
        return;
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, usuario.Username),
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    };

    if (usuario.IsAdmin)
    {
        claims.Add(new Claim(ClaimTypes.Role, AuthConstants.Roles.Admin));
    }
    else if (usuario.CidadeId.HasValue)
    {
        claims.Add(new Claim(AuthConstants.Claims.CidadeId, usuario.CidadeId.Value.ToString()));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    var redirect = string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith('/') ? "/" : returnUrl;
    ctx.Response.Redirect(redirect);
});

app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    ctx.Response.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply migrations and seed initial admin
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CadastroInvturDbContext>>();
    await using var db = factory.CreateDbContext();
    await db.Database.EnsureCreatedAsync();

    var adminSetupPass = app.Configuration["ADMIN_SETUP_PASS"];
    if (!string.IsNullOrEmpty(adminSetupPass) && !await db.Usuarios.AnyAsync())
    {
        db.Usuarios.Add(new SistemaBensServicosTur.Web.Entities.Usuario
        {
            Username = "admin",
            SenhaHash = PasswordHelper.Hash(adminSetupPass),
            IsAdmin = true
        });
        await db.SaveChangesAsync();
    }
}

app.Run();
