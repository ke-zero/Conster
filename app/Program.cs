using Conster.Application;
using Conster.Application.Services;
using Conster.Application.Components;

Env.Initialize();

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddSingleton<IClusterService, ClusterService>();
}

var app = builder.Build();
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
    app.Run();
}