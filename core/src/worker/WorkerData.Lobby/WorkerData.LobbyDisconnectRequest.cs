namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyDisconnectRequest : IWorkerRequestData
    {
        public List<string> IDs { get; set; } = [];
        public string Zone { get; set; } = string.Empty;

        public bool IsValid()
        {
            if (IDs.Count <= 0) return false;
            return true;
        }
    }
}