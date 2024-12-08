namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class HostStatusResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public uint Memory { get; set; } = 0;
        public uint Storage { get; set; } = 0;
    }
}