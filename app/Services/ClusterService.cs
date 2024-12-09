using System.Text.Json;
using Conster.Core;
using Conster.Core.Worker;
using Netly;

namespace Conster.Application.Services;

public class ClusterService : IClusterService
{
    private readonly List<Connection> _connections = new();

    public List<Cluster> Clusters => GetClusters();

    private List<Cluster> GetClusters()
    {
        return _connections.Select(x => x.Cluster).ToList();
    }

    public Cluster Add(Cluster cluster)
    {
        if (string.IsNullOrWhiteSpace(cluster.Id)) cluster.Id = Guid.NewGuid().ToString();

        cluster.CreatedAt = DateTime.UtcNow;

        var socket = new HTTP.WebSocket();

        socket.On.Open(() =>
        {
            cluster.Status = new();
            cluster.StatusUpdatedAt = DateTime.UtcNow;
            Console.WriteLine($"Cluster connected at: {socket.Host} ({cluster.Name})");
        });

        socket.On.Error(e =>
        {
            cluster.IsActive = false;
            cluster.Status = new();
            Console.WriteLine($"Cluster error ({cluster.Name}): {e.Message}");
        });

        socket.On.Close(() =>
        {
            cluster.IsActive = false;
            cluster.Status = new();
            cluster.StatusUpdatedAt = DateTime.UtcNow;
            Console.WriteLine($"Cluster disconnected at: {socket.Host} ({cluster.Name})");
        });

        var http = new HTTP.Client();

        http.On.Error(e =>
        {
            cluster.Status = new();
            Console.WriteLine($"Cluster http error ({cluster.Name}): ({e})");
        });

        http.On.Open(response =>
        {
            if (response.Status != 200)
            {
                Console.WriteLine(
                    $"Cluster http invalid status ({cluster.Name}): {response.Status} ({response.Body.Text})");

                cluster.Status = new();
                return;
            }

            try
            {
                cluster.Status = JsonSerializer.Deserialize<WorkerData.HostStatusResponse>(response.Body.Text) ??
                                 throw new NullReferenceException($"Couldn't resolve body: {response.Body.Text}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cluster http fetch error ({cluster.Name}): ({e})");
            }
        });

        _connections.Add(new Connection(socket, cluster, () =>
        {
            cluster.IsActive = socket.IsOpened;

            if (socket.IsOpened)
            {
                if (!http.IsOpened)
                {
                    http.Headers["TOKEN"] = cluster.ApiKey;
                    http.To.Open("GET", $"http://{cluster.IPv4}:{cluster.Port}/host/status");
                }
            }
            else
            {
                socket.Headers["TOKEN"] = cluster.ApiKey;
                socket.To.Open(new Uri($"ws://{cluster.IPv4}:{cluster.Port}/lobby"));
            }
        }));

        return cluster;
    }

    public Cluster? Remove(string id)
    {
        var connection = _connections.FirstOrDefault(x => x.Cluster.Id == id);

        if (connection != null) _connections.Remove(connection);

        return connection?.Cluster;
    }

    public void Update(Cluster cluster)
    {
        var selected = _connections.FirstOrDefault(x => x.Cluster.Id == cluster.Id);

        if (selected == null) return;

        selected.Cluster = cluster;
        selected.Socket.To.Close(); // force start connection
    }

    public ClusterService()
    {
        new Thread(BackgroundJob) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
        Console.WriteLine($"[{GetType().Name}] -> {nameof(BackgroundJob)} `STARTED!`");
    }

    private void BackgroundJob()
    {
        // uint times = 0;

        while (Thread.CurrentThread.IsAlive)
        {
            // Console.WriteLine($"[{GetType().Name}] Refresh connections ({_connections.Count}). {times++}x");

            foreach (var _connection in _connections) _connection.OnUpdate();

            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        Console.WriteLine($"[{GetType().Name}] -> {nameof(BackgroundJob)} `END!`");
    }

    private class Connection(HTTP.WebSocket socket, Cluster cluster, Action onUpdate)
    {
        public bool IsStarted { get; set; } = false;
        public HTTP.WebSocket Socket { get; set; } = socket;
        public Cluster Cluster { get; set; } = cluster;
        public Action OnUpdate { get; set; } = onUpdate;
    }
}