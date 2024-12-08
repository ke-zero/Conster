namespace Conster.Worker.Interfaces;

public interface IAddons
{
    IApplication Application { get; }
    void OnInitialize();
    void OnStart();
    void OnStop();
}