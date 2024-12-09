namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyToken : IWorkerRequestData
    {
        public string Id { get; set; } = null!;
        public string Zone { get; set; } = null!;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Zone);
        }
    }
}