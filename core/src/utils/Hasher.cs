using Byter;
using System.Security.Cryptography;

namespace Conster.Core.Utils;

public static class Hasher
{
    private static byte[] GetBytes(in string value)
    {
        // Using Sha<2>, More secure than Sha<0,1> and more performed than Sha<3>.
        return SHA256.HashData(value.GetBytes());
    }

    public static long ToLong(string value)
    {
        return BitConverter.ToInt64(GetBytes(in value));
    }

    public static ulong ToULong(string value)
    {
        return BitConverter.ToUInt64(GetBytes(in value));
    }

    public static int ToInt(string value)
    {
        return BitConverter.ToInt32(GetBytes(in value));
    }

    public static int ToUInt(string value)
    {
        return BitConverter.ToInt32(GetBytes(in value));
    }
}