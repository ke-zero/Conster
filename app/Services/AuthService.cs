using Conster.Application;

namespace Conster.Application.Services;

public class AuthService : IAuthService
{
    public bool IsAdmin(string key)
    {
        var input = key ?? string.Empty;

        return input.Equals(Env.ADMIN_KEY);
    }

    public bool IsManager(string username, string password)
    {
        // TODO: Search on database or cache.
        return false;
    }
}