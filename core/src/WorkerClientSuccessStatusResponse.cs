namespace Conster.Core;

[Serializable]
public class WorkerClientSuccessStatusResponse
{
    public List<SuccessStatusData> Data { get; set; } = new();

    [Serializable]
    public class SuccessStatusData
    {
        public string Id { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
    }
}