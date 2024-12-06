namespace Conster.Core;

[Serializable]
public class WorkerClientStatusResponse
{
    public List<StatusData> Data { get; set; } = new();

    [Serializable]
    public class StatusData
    {
        public string Id { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public DateTime At { get; set; } = DateTime.UnixEpoch;
    }
}