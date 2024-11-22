using Conster.Application.Components;
using Conster.Application.Services;

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