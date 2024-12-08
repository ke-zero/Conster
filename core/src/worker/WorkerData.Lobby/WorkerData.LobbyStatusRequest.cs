namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyStatusRequest
    {
        public List<string> IDs { get; set; } = [];
        public string Zone { get; set; } = string.Empty;
    }
}