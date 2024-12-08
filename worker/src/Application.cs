using Conster.Worker.Interfaces;

namespace Conster.Worker;

public class Application : IApplication
{
    public Application()
    {
        var application = this;
        Connections = new Dictionary<long, IConnection>();
        HostManager = new ApplicationHostManager(ref application);
        SocketManager = new ApplicationSocketManager(ref application);
        ServerManager = new ApplicationServerManager(ref application);
        InstanceManager = new ApplicationInstanceManager(ref application);
    }

    public Dictionary<long, IConnection> Connections { get; }
    public IAddons HostManager { get; }
    public IAddons SocketManager { get; }
    public IAddons ServerManager { get; }
    public IAddons InstanceManager { get; }

    public void OnInitialize()
    {
        HostManager.OnInitialize();
        SocketManager.OnInitialize();
        ServerManager.OnInitialize();
        InstanceManager.OnInitialize();
    }

    public void OnStart()
    {
        HostManager.OnStart();
        SocketManager.OnStart();
        ServerManager.OnStart();
        InstanceManager.OnStart();
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
}