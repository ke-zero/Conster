using Netly.Interfaces;

namespace Conster.Worker.Interfaces;

public interface IConnection
{
    string ID { get; }
    long IDHash { get; }
    DateTime CreatedAt { get; }
    IHTTP.WebSocket WebSocket { get; }
}