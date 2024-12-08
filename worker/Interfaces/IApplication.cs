namespace Conster.Worker.Interfaces;

public interface IApplication
{
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
}