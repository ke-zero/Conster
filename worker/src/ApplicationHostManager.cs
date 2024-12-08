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
    private uint _totalMemory, _freeMemory, _cpuCore, _cpuThread, _cpuPercent;
    private string? _osName, _osVersion, _name;

    public void OnInitialize()
    {
        Application.Server.Map.Get("/host/status", (request, response) =>
        {
            if (Application.IsNotMaster(request, response, true, out _)) return;

            var data = new WorkerData.HostStatusResponse
            {
                Name = _name!,
                OSName = _osName!,
                OSVersion = _osVersion!,
                TotalMemory = _totalMemory,
                FreeMemory = _freeMemory,
                CPUCore = _cpuCore,
                CPUThread = _cpuThread,
                CPUPercent = _cpuPercent,
                StartedAt = _startedAt
            };

            response.Send(200, JsonSerializer.Serialize(data));
        });
    }

    public void OnStart()
    {
        _hardware.RefreshOperatingSystem();
        _hardware.RefreshCPUList();
        _hardware.RefreshMemoryList();
        _hardware.RefreshMemoryStatus();

        _name = Environment.MachineName;
        _osName = _hardware.OperatingSystem.Name;
        _osVersion = _hardware.OperatingSystem.VersionString;
        _cpuCore = _hardware.CpuList[0].NumberOfCores;
        _cpuThread = _hardware.CpuList[0].NumberOfLogicalProcessors;
        _hardware.MemoryList.ForEach(x => _totalMemory += (uint)(x.Capacity / (1024 * 1024)));

        Task.Run(() =>
        {
            do
            {
                _hardware.RefreshCPUList();
                _hardware.RefreshMemoryStatus();

                // TODO: Find a good method to get os cpu usage.
                _cpuPercent = (uint)_hardware.CpuList[0].PercentProcessorTime;
                _freeMemory = (uint)(_hardware.MemoryStatus.AvailablePhysical / (1024 * 1024));

                Thread.Sleep(TimeSpan.FromSeconds(5));
            } while (Application.Server.IsOpened);
        });
    }

    public void OnStop()
    {
    }
}