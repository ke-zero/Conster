using Conster.Application;

namespace Conster.Application.Services;

public interface IAuthService
{
    bool IsAdmin(string key);
    bool IsManager(string username, string password);
}