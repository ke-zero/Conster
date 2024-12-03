namespace Conster.Application.Services;

public class LocalSessionData : ILocalSessionData
{
    public bool IsAdmin { get; set; }
    public bool IsAuthenticated { get; set; }

    public string AdminKey { get; set; } = null!;
    public string ManagerUsername { get; set; } = null!;
    public string ManagerPassword { get; set; } = null!;

    public void SetValue(ILocalSessionData data)
    {
        IsAdmin = data.IsAdmin;
        IsAuthenticated = data.IsAuthenticated;

        AdminKey = data.AdminKey;
        ManagerUsername = data.ManagerUsername;
        ManagerPassword = data.ManagerPassword;
    }
}