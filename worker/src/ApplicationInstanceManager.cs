using Conster.Worker.Interfaces;

namespace Conster.Worker;

public class ApplicationInstanceManager(ref Application application) : IAddons
{
    public IApplication Application { get; } = application;

    public void OnInitialize()
    {
    }

    public void OnStart()
    {
    }

    public void OnStop()
    {
    }
}