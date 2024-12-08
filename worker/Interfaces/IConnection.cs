using Netly.Interfaces;

namespace Conster.Worker.Interfaces;

public interface IConnection
{
    string ID { get; }
    string Zone { get; }
    long IDHash { get; }
    bool IsMaster { get; }
    DateTime CreatedAt { get; }
    IHTTP.WebSocket WebSocket { get; }
}