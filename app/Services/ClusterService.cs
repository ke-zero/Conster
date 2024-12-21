using System.Text.Json;
using Byter;
using Conster.Core;
using Conster.Core.Worker;
using Netly;

namespace Conster.Application.Services;

public class ClusterService : IClusterService
{
    private readonly List<Connection> _connections = new();
    private static readonly string FILE_DIRECTORY = $"{Directory.GetCurrentDirectory()}/runtime";
    private static readonly string FILE_PATH = $"{FILE_DIRECTORY}/{nameof(IClusterService)}.json";
    private static readonly object FILE_LOCKER = new();

    public List<Cluster> Clusters => GetClusters();

    private List<Cluster> GetClusters()
    {
        return _connections.Select(x => x.Cluster).ToList();
    }

    public Cluster Add(Cluster cluster)
    {
        if (string.IsNullOrWhiteSpace(cluster.Id))
        {
            // new connection.
            cluster.Id = Guid.NewGuid().ToString();
            cluster.CreatedAt = DateTime.UtcNow;;
        }
        
        var socket = new HTTP.WebSocket();
        var http = new HTTP.Client();

        InitSocket();
        InitHTTP();

        _connections.Add(new Connection(cluster, OnUpdate, ToClose));

        Console.WriteLine($"Add new cluster ({cluster.Name}): {cluster.Id}");

        SaveClustersOnDisk(Clusters);

        return cluster;

        void OnUpdate()
        {
            const string httpSchema = "http";
            const string websocketSchema = "ws";
            var host = $"{cluster.IPv4}:{cluster.Port}";

            cluster.IsActive = socket.IsOpened;

            if (socket.IsOpened)
            {
                if (!http.IsOpened)
                {
                    http.Headers["TOKEN"] = cluster.ApiKey;
                    http.To.Open("GET", $"{httpSchema}://{host}/host/status");
                }
            }
            else
            {
                socket.Headers["TOKEN"] = cluster.ApiKey;
                socket.To.Open(new Uri($"{websocketSchema}://{host}/lobby")).Wait();
            }
        }

        void ToClose()
        {
            socket.To.Close();
        }

        void InitSocket()
        {
            socket.On.Open(() =>
            {
                cluster.Status = new();
                cluster.StatusUpdatedAt = DateTime.UtcNow;
                Console.WriteLine($"Cluster connected at: {socket.Host} ({cluster.Name})");
                SaveClustersOnDisk(Clusters);
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
                SaveClustersOnDisk(Clusters);
            });
            
            socket.On.Data((data, _) =>
            {
                Console.WriteLine($"[WS_DATA] ({cluster.Name}): {data.GetString()}");
            });
        }

        void InitHTTP()
        {
            http.Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;

            http.On.Error(e =>
            {
                cluster.Status = new();
                Console.WriteLine($"Cluster http error ({cluster.Name}): ({e.Message})");
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
                    Console.WriteLine($"Cluster http fetch error ({cluster.Name}): ({e.Message})");
                }
            });
        }
    }

    public Cluster? Remove(string id)
    {
        var connection = _connections.FirstOrDefault(x => x.Cluster.Id == id);

        if (connection == null) return null;

        connection.ToClose();
        _connections.Remove(connection);

        Console.WriteLine($"Remove cluster ({connection.Cluster.Name}): {connection.Cluster.Id}");
        SaveClustersOnDisk(Clusters);

        return connection.Cluster;
    }

    public void Update(Cluster cluster)
    {
        var selected = _connections.FirstOrDefault(x => x.Cluster.Id == cluster.Id);

        if (selected == null) return;

        selected.Cluster = cluster;
        Console.WriteLine($"Update Cluster: {cluster.Name}");
        selected.ToClose(); // force start connection
        
        SaveClustersOnDisk(Clusters);
    }

    public ClusterService()
    {
        var clusters = LoadClustersFromDisk();

        clusters.ForEach(x => Add(x));

        new Thread(BackgroundJob) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();

        Console.WriteLine($"[{GetType().Name}] -> {nameof(BackgroundJob)} `STARTED!`");
    }

    private void BackgroundJob()
    {
        // uint times = 0;

        while (Thread.CurrentThread.IsAlive)
        {
            // Console.WriteLine($"[{GetType().Name}] Refresh connections ({_connections.Count}). {times++}x");

            foreach (var _connection in _connections)
            {
                _connection.OnUpdate();
                Thread.Sleep(byte.MaxValue);
            }

            Thread.Sleep(TimeSpan.FromSeconds(15));
        }

        Console.WriteLine($"[{GetType().Name}] -> {nameof(BackgroundJob)} `END!`");
    }

    private class Connection(Cluster cluster, Action onUpdate, Action toClose)
    {
        public Cluster Cluster { get; set; } = cluster;
        public Action OnUpdate { get; } = onUpdate;
        public Action ToClose { get; } = toClose;
    }

    private static List<Cluster> LoadClustersFromDisk()
    {
        lock (FILE_LOCKER)
        {
            Console.WriteLine("Load clusters from disk...");

            if (File.Exists(FILE_PATH))
            {
                try
                {
                    var buffer = File.ReadAllBytes(FILE_PATH);

                    var clusters = JsonSerializer.Deserialize<List<Cluster>>(buffer) ??
                                   throw new InvalidDataException("bytes loaded is corrupted data.");

                    Console.WriteLine($"--- Success on load cluster from disk: Clusters found ({clusters.Count})");

                    return clusters;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"--- Error on load cluster from disk: {e}");
                }
            }
            else
            {
                Console.WriteLine($"--- Failed on load cluster from disk: File not found on path ({FILE_PATH}).");
            }

            return new();
        }
    }

    private static void SaveClustersOnDisk(List<Cluster> clusters)
    {
        lock (FILE_LOCKER)
        {
            Console.WriteLine("Saving clusters on disk...");

            var data = JsonSerializer.SerializeToUtf8Bytes(clusters);

            if (File.Exists(FILE_PATH))
            {
                Console.WriteLine("--- Deleting existent cluster file on disk...");
                try
                {
                    File.Delete(FILE_PATH);
                    Console.WriteLine("--- Success on delete old cluster file on disk.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"--- Error on delete old cluster file on disk: {e}");
                }
            }
            else
            {
                Console.WriteLine("--- Not found old cluster file on disk. ");
            }

            try
            {
                if (!Directory.Exists(FILE_DIRECTORY))
                {
                    Console.WriteLine($"--- Create output file directory: {FILE_DIRECTORY}");
                    Directory.CreateDirectory(FILE_DIRECTORY);
                }

                File.WriteAllBytes(FILE_PATH, data);
                Console.WriteLine($"--- Success on save cluster file. ({FILE_PATH})");
            }
            catch (Exception e)
            {
                Console.WriteLine($"--- Error on save cluster file: {e}");
            }
        }
    }
}