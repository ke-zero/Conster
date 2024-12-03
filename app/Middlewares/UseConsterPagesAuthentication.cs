using System.Text.Json;
using Conster.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conster.Application.Middlewares;

public class UseConsterPagesAuthentication(RequestDelegate next)
{
    [IgnoreAntiforgeryToken]
    public async Task InvokeAsync
    (
        HttpContext context,
        [FromServices] IAuthService authService,
        [FromServices] ILocalSessionData sessionData
    )
    {
        var match = false;
        context.Request.Cookies.TryGetValue(nameof(ILocalSessionData), out var cookie);

        if (!string.IsNullOrEmpty(cookie)) TryLogin(cookie);

        var path = context.Request.Path.Value ?? string.Empty;

        var isPost = context.Request.Method.Equals(HttpMethod.Post.Method, StringComparison.CurrentCultureIgnoreCase);

        if (path is "/auth/logout" or "/auth/logout/")
        {
            if (sessionData.IsAuthenticated)
            {
                // now need logout
                if (isPost)
                {
                    match = true;
                    Logout();
                    context.Response.Redirect("/auth", false, false);
                }
                // else? NO: is GET method need load page
            }
            else
            {
                // need login first to allow access logout page
                context.Response.Redirect("/auth", false, false);
            }
        }

        if (path is "/auth" or "/auth/")
        {
            if (isPost && context.Request.HasFormContentType && !sessionData.IsAuthenticated)
            {
                match = true;
                var form = context.Request.Form;
                string adminKey = string.Empty, managerUsername = string.Empty, managerPassword = string.Empty;

                if (form.TryGetValue(nameof(ILocalSessionData.AdminKey), out var value)) adminKey = value[0]!.Trim();
                if (form.TryGetValue(nameof(ILocalSessionData.AdminKey), out value)) managerUsername = value[0]!.Trim();
                if (form.TryGetValue(nameof(ILocalSessionData.AdminKey), out value)) managerPassword = value[0]!.Trim();

                var json = JsonSerializer.Serialize(new LocalSessionData
                {
                    IsAuthenticated = false,
                    IsAdmin = !string.IsNullOrWhiteSpace(value),
                    AdminKey = adminKey,
                    ManagerUsername = managerUsername,
                    ManagerPassword = managerPassword
                });

                Console.WriteLine($"TryLogin: {json}");

                TryLogin(json);

                if (!sessionData.IsAuthenticated) Logout();
            }

            if (sessionData.IsAuthenticated)
                context.Response.Redirect("/dashboard", false, false);
        }

        if (!sessionData.IsAuthenticated && path.StartsWith("/dashboard"))
        {
            Logout();
            context.Response.Redirect("/auth", false, false);
        }

        Console.WriteLine($"PATH: {path}, IS_AUTHENTICATED: {sessionData.IsAuthenticated}");

        // This is a bypass for several ASPNET Middleware that make post request HARD.
        // It modifies the request data and method to make, HARD middleware cant detected it.
        if (match)
        {
            // delete form data
            context.Request.Form = new FormCollection(new());
            // update POST request to GET
            context.Request.Method = "GET";
        }

        await next(context);

        ///////////////////////////////////////////////////////////////////////////////////////////
        return;


        void TryLogin(string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<LocalSessionData>(json) ?? throw new Exception();

                switch (data.IsAdmin)
                {
                    case true when authService.IsAdmin(data.AdminKey):
                    case false when authService.IsManager(data.ManagerUsername, data.ManagerPassword):
                    {
                        sessionData.SetValue(data);
                        sessionData.IsAuthenticated = true;
                        context.Response.Cookies.Append
                        (
                            nameof(ILocalSessionData),
                            JsonSerializer.Serialize(sessionData),
                            new CookieOptions { MaxAge = TimeSpan.FromDays(1) }
                        );
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                context.Response.Cookies.Delete(nameof(ILocalSessionData));
            }
        }

        void Logout()
        {
            sessionData.SetValue(new LocalSessionData());
            context.Response.Cookies.Delete(nameof(ILocalSessionData));
        }
    }
}