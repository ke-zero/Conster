using Conster.Core.Worker;
using Netly;
using Netly.Interfaces;

namespace Conster.Worker.Interfaces;

public interface IApplication
{
    HTTP.Server Server { get; }
    Dictionary<long, IConnection> Connections { get; }

    IAddons HostManager { get; }
    IAddons SocketManager { get; }
    IAddons ServerManager { get; }
    IAddons InstanceManager { get; }

    void OnInitialize();
    void OnStart();
    void OnStop();
    void Freeze();
    void Destroy();

    bool IsNotMaster(IHTTP.ServerRequest request, IHTTP.ServerResponse response, bool autoCloseConnection,
        out string token);

    bool IsNotClient(IHTTP.ServerRequest request, IHTTP.ServerResponse response, bool autoCloseConnection,
        out WorkerData.LobbyToken token);
}