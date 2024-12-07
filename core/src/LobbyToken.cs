namespace Conster.Core;

[Serializable]
public class LobbyToken
{
    public string Id { get; set; } = null!;
    public string Server { get; set; } = null!;


    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Server);
    }
}