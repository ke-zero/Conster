using Conster.Core.Utils;
using Conster.Worker.Interfaces;
using Netly.Interfaces;

namespace Conster.Worker;

public class Connection(string id, bool isMaster, string zone, ref IHTTP.WebSocket socket) : IConnection
{
    public string ID { get; } = id;
    public string Zone { get; } = zone;
    public long IDHash { get; } = Hasher.ToLong(id);
    public bool IsMaster { get; } = isMaster;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public IHTTP.WebSocket WebSocket { get; } = socket;
}