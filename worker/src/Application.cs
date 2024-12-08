using System.Net.Http.Headers;
using Conster.Core;
using Conster.Core.Worker;
using Conster.Worker.Interfaces;
using Netly;
using Netly.Interfaces;

namespace Conster.Worker;

public class Application : IApplication
{
    public Application()
    {
        var application = this;
        Server = new HTTP.Server();
        Connections = new Dictionary<long, IConnection>();
        HostManager = new ApplicationHostManager(ref application);
        SocketManager = new ApplicationSocketManager(ref application);
        ServerManager = new ApplicationServerManager(ref application);
        InstanceManager = new ApplicationInstanceManager(ref application);
    }

    public HTTP.Server Server { get; }
    public Dictionary<long, IConnection> Connections { get; }
    public IAddons HostManager { get; }
    public IAddons SocketManager { get; }
    public IAddons ServerManager { get; }
    public IAddons InstanceManager { get; }

    private const string HEADER_TOKEN_KEY = "TOKEN";

    public void OnInitialize()
    {
        HostManager.OnInitialize();
        SocketManager.OnInitialize();
        ServerManager.OnInitialize();
        InstanceManager.OnInitialize();

        Server.On.Open(() =>
        {
            Clear();
            Console.WriteLine($"Server started at: {Server.Host}");
        });

        Server.On.Close(() =>
        {
            Clear();
            Console.WriteLine($"Server stopped at: {Server.Host}");
        });

        Server.On.Error(exception =>
        {
            Clear();
            Console.WriteLine($"Server error: {exception}");
        });

        Server.Middleware.Add((_, response, next) =>
        {
            response.Headers["Content-Type"] = "application/json";
            next();
        });
        
        return;

        void Clear()
        {
            foreach (var connection in Connections) connection.Value.WebSocket.To.Close();
            Connections.Clear();
        }
    }

    public void OnStart()
    {
        HostManager.OnStart();
        SocketManager.OnStart();
        ServerManager.OnStart();
        InstanceManager.OnStart();

        Server.To.Open(new Uri($"https://{new Host(Config.IP, Config.PORT)}"));
    }

    public void OnStop()
    {
        HostManager.OnStop();
        SocketManager.OnStop();
        ServerManager.OnStop();
        InstanceManager.OnStop();
    }

    public void Freeze()
    {
        uint times = 0;

        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Q)
            {
                times++;
                Console.Write($"\r `{key.KeyChar}` is invalid input. (Press `q` to Quit). {times}x");
                continue;
            }

            var quit = false;
            bool reading;

            var time = DateTime.Now;
            Console.WriteLine($"\nWanna you quit? `{time}`\n\t Press: `y` (Yes) or `n` (No)");

            do
            {
                var next = Console.ReadKey(true);

                switch (next.Key)
                {
                    case ConsoleKey.Y:
                    {
                        reading = false;
                        quit = true;
                        Console.WriteLine("`y` Selected. Program will be destroyed...");
                        break;
                    }
                    case ConsoleKey.N:
                    {
                        reading = false;
                        quit = false;
                        Console.WriteLine("`n` Selected. Program will continue...");
                        break;
                    }
                    default:
                    {
                        reading = true;
                        break;
                    }
                }
            } while (reading);

            if (quit) break;
        }
    }

    public void Destroy()
    {
        Environment.Exit(0);
    }

    public bool IsNotMaster(IHTTP.ServerRequest request, IHTTP.ServerResponse response, bool autoCloseConnection,
        out string token)
    {
        const StringComparison method = StringComparison.OrdinalIgnoreCase;

        token = request.Headers.FirstOrDefault(x => x.Key.Equals(HEADER_TOKEN_KEY, method)).Value!;

        var isEquals = Config.API_KEY.Equals(token);

        if (!isEquals && autoCloseConnection) response.Send(401);

        return !isEquals;
    }

    public bool IsNotClient(IHTTP.ServerRequest request, IHTTP.ServerResponse response, bool autoCloseConnection,
        out WorkerData.LobbyToken token)
    {
        token = null!;
        const StringComparison method = StringComparison.OrdinalIgnoreCase;

        var value = request.Headers.FirstOrDefault(x => x.Key.Equals(HEADER_TOKEN_KEY, method)).Value!;
        var key = Config.API_KEY;

        if (JWTSolver.Solve<WorkerData.LobbyToken>(value, key, out var data) && data != null && data.IsValid())
        {
            token = data;
            return false;
        }

        if (autoCloseConnection) response.Send(401);
        return true;
    }
}