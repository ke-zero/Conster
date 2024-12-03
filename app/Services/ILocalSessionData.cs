namespace Conster.Application.Services;

public interface ILocalSessionData
{
    bool IsAdmin { get; set; }
    string AdminKey { get; set; }
    string ManagerUsername { get; set; }
    string ManagerPassword { get; set; }
    bool IsAuthenticated { get; set; }
    void SetValue(ILocalSessionData data);
}