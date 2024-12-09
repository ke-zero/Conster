namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class HostStatusResponse
    {
        public string Name { get; set; } = string.Empty;
        public string OSName { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public uint TotalMemory { get; set; }
        public uint FreeMemory { get; set; }
        public uint CPUCore { get; set; }
        public uint CPUThread { get; set; }
        public uint CPUPercent { get; set; }
        public uint CPUClock { get; set; }
        public DateTime StartedAt { get; set; }
    }
}