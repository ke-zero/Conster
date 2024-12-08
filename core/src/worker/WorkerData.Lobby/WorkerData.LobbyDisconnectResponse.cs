namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyDisconnectResponse
    {
        public List<ResponseParameter> Data { get; set; } = [];
        public int DataLength { get; set; } = 0;

        [Serializable]
        public class ResponseParameter
        {
            public string ID { get; set; } = string.Empty;
            public bool Success { get; set; } = false;
        }
    }
}