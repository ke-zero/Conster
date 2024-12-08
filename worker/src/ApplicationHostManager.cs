using System.Text.Json;
using Conster.Core.Worker;
using Conster.Worker.Interfaces;
using Hardware.Info;

namespace Conster.Worker;

public class ApplicationHostManager(ref Application application) : IAddons
{
    public IApplication Application { get; } = application;
    private readonly DateTime _startedAt = DateTime.UtcNow;
    private readonly HardwareInfo _hardware = new();

    public void OnInitialize()
    {
        var timer = DateTime.UtcNow;

        Application.Server.Map.Get("/host/status", (request, response) =>
        {
            if (Application.IsNotMaster(request, response, true, out _)) return;

            _hardware.RefreshMemoryStatus();

            var data = new WorkerData.HostStatusResponse
            {
                Name = Environment.MachineName,
                OS = $"{_hardware.OperatingSystem.Name} : {_hardware.OperatingSystem.VersionString}",
                TotalMemory = (uint)_hardware.MemoryStatus.TotalPhysical / (1024 * 1024),
                FreeMemory = (uint)_hardware.MemoryStatus.AvailablePhysical / (1024 * 1024),
                Storage = 0,
                FreeStorage = 0,
                StartedAt = _startedAt
            };

            response.Send(200, JsonSerializer.Serialize(data));
        });
    }

    public void OnStart()
    {
        _hardware.RefreshMemoryList();
        _hardware.RefreshCPUList();
        _hardware.RefreshOperatingSystem();
        _hardware.RefreshMemoryStatus();
    }

    public void OnStop()
    {
    }
}