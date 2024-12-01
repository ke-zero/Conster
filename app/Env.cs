using System.Collections.Generic;
using dotenv.net;

namespace Conster.Application;

public static class Env
{
    private static bool _init;

    public static string APP_KEY { get; private set; } = null!;
    public static string APP_NAME { get; private set; } = null!;
    public static string APP_VERSION { get; private set; } = null!;
    public static string ADMIN_KEY { get; private set; } = null!;

    public static void Initialize()
    {
        if (_init) return;
        _init = true;

        DotEnv.Load();
        var vars = DotEnv.Read();

        APP_KEY = GetValue(ref vars, nameof(APP_KEY), true);
        APP_NAME = GetValue(ref vars, nameof(APP_NAME), true);
        APP_VERSION = GetValue(ref vars, nameof(APP_VERSION), true);
        ADMIN_KEY = GetValue(ref vars, nameof(ADMIN_KEY), true);
    }

    private static string GetValue(ref IDictionary<string, string> vars, string key, bool nullIsException)
    {
        if (!vars.TryGetValue(key, out var value) && nullIsException)
            throw new KeyNotFoundException($"{nameof(Env)} -> Key not found: {key}");

        return value ?? string.Empty;
    }
}