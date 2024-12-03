using Conster.Application;

namespace Conster.Application.Services;

public class AuthService : IAuthService
{
    public bool IsAdmin(string key)
    {
        return key.Equals(Env.ADMIN_KEY);
    }

    public bool IsManager(string username, string password)
    {
        // TODO: Search on database or cache.
        return false;
    }
}