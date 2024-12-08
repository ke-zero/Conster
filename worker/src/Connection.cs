using Conster.Core.Utils;
using Conster.Worker.Interfaces;
using Netly.Interfaces;

namespace Conster.Worker;

public class Connection(string id, IHTTP.WebSocket socket) : IConnection
{
    public string ID { get; } = id;
    public long IDHash { get; } = Hasher.ToLong(id);
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public IHTTP.WebSocket WebSocket { get; } = socket;
}