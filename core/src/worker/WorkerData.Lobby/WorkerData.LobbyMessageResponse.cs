namespace Conster.Core.Worker;

public static partial class WorkerData
{
    [Serializable]
    public class LobbyMessageResponse
    {
        public List<ResponseParameter> Data { get; set; } = [];
        public int DataLength { get; set; }
        public int Deliveries { get; set; }

        [Serializable]
        public class ResponseParameter
        {
            public string ID { get; set; } = string.Empty;
            public bool Success { get; set; } = false;
        }

        [Serializable]
        public class MessagePackage
        {
            public string Name { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}