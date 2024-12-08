using System.Net;
using dotenv.net;
using Netly;

namespace Conster.Worker;

public static class Config
{
    public static string API_KEY { get; private set; } = string.Empty;
    public static ushort PORT { get; private set; } = ushort.MinValue;
    public static IPAddress HOST { get; private set; } = IPAddress.None;
    public static bool DEBUG_LOG { get; private set; } = true;

    public static void Initialize()
    {
        DotEnv.Load();

        var env = DotEnv.Read();

        API_KEY = new string(Guid.NewGuid().ToString().ToCharArray(0, sizeof(long)));
        PORT = 10101;
        HOST = IPAddress.Any;
        DEBUG_LOG = false;

        if (!env.TryGetValue(nameof(API_KEY), out var apiKey)) throw new KeyNotFoundException(nameof(API_KEY));
        if (!env.TryGetValue(nameof(PORT), out var port)) throw new KeyNotFoundException(nameof(PORT));
        if (!env.TryGetValue(nameof(HOST), out var host)) throw new KeyNotFoundException(nameof(HOST));
        if (!env.TryGetValue(nameof(DEBUG_LOG), out var debugLog)) throw new KeyNotFoundException(nameof(DEBUG_LOG));

        const byte MIN_API_SIZE = 32;
        
        API_KEY = apiKey is { Length: < MIN_API_SIZE }
            ? throw new Exception($"Min {nameof(API_KEY)} size is {MIN_API_SIZE} ({MIN_API_SIZE * 8} bits)")
            : apiKey;
        
        PORT = ushort.Parse(port);
        HOST = IPAddress.Parse(host);
        DEBUG_LOG = byte.Parse(debugLog) switch
        {
            1 => true, 0 => false, _ => throw new ArgumentOutOfRangeException(nameof(DEBUG_LOG))
        };
    }

    public static void Debug()
    {
        if (!DEBUG_LOG) return;
        NetlyEnvironment.Logger.On((string message) => Console.WriteLine($"NETLY MESSAGE: {message}"));
        NetlyEnvironment.Logger.On((Exception exception) => Console.WriteLine($"NETLY EXCEPTION: {exception}"));

        Console.WriteLine(">>>> ENVIRONMENT");
        {
            Console.WriteLine($"   > {nameof(API_KEY)}: {API_KEY}");
            Console.WriteLine($"   > {nameof(PORT)}: {PORT}");
            Console.WriteLine($"   > {nameof(HOST)}: {HOST}");
        }
        Console.WriteLine("<<<<");
    }
}