using Conster.Application;
using Conster.Application.Services;
using Conster.Application.Components;
using Blazored.LocalStorage;
using Conster.Application.Middlewares;

Env.Initialize();

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
    builder.Services.AddSingleton<IClusterService, ClusterService>();
    builder.Services.AddSingleton<IAuthService, AuthService>();
    builder.Services.AddScoped<ILocalSessionData, LocalSessionData>();
    builder.Services.AddBlazoredLocalStorage();
}

var app = builder.Build();
{
    if (app.Environment.IsProduction()) app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseMiddleware<UseConsterPagesAuthentication>();
    app.UseHsts();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode();
    app.Run();
}