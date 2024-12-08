namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyMessageRequest
    {
        public List<string> IDs { get; set; } = [];
        public string Zone { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}