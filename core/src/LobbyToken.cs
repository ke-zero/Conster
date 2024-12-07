namespace Conster.Core;

[Serializable]
public class LobbyToken
{
    public string Id { get; set; } = null!;
    public string Zone { get; set; } = null!;


    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Zone);
    }
}