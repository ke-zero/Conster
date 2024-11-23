using System.Net;
using dotenv.net;

namespace Conster.Worker;

public static class Config
{
    public static string API_KEY { get; private set; } = string.Empty;
    public static ushort PORT { get; private set; } = ushort.MinValue;
    public static IPAddress HOST { get; private set; } = IPAddress.None;

    public static void Initialize()
    {
        DotEnv.Load();

        var env = DotEnv.Read();

        API_KEY = new string(Guid.NewGuid().ToString().ToCharArray(0, sizeof(long)));
        PORT = 10101;
        HOST = IPAddress.Any;

        if (env.TryGetValue(nameof(API_KEY), out var apiKey)) API_KEY = apiKey;
        if (env.TryGetValue(nameof(PORT), out var port)) PORT = ushort.Parse(port);
        if (env.TryGetValue(nameof(HOST), out var host)) HOST = IPAddress.Parse(host);
    }

    public static void Debug()
    {
        Console.WriteLine(">>>> ENVIRONMENT");
        {
            Console.WriteLine($"   > {nameof(API_KEY)}: {API_KEY}");
            Console.WriteLine($"   > {nameof(PORT)}: {PORT}");
            Console.WriteLine($"   > {nameof(HOST)}: {HOST}");
        }
        Console.WriteLine("<<<<");
    }
}