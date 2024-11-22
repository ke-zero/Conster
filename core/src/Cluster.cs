using System.Net;

namespace Conster.Core;


/// <summary>
/// Cluster Definition
/// </summary>
public class Cluster
{
    /// <summary>
    /// Cluster Name
    /// </summary>
    public string Name { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Cluster Internet IPv4
    /// </summary>
    public string IPv4 { get; set; } = IPAddress.Any.ToString();
    
    /// <summary>
    /// Cluster RAM Memory (Bytes), default 1GB (1024)
    /// </summary>
    public uint RAM { get; set; } = 1024; // 1GB
    
    /// <summary>
    /// Cluster Storage (Bytes), default 20GB (20480)
    /// </summary>
    public uint Storage { get; set; } = 20480; // 20GB
    
    /// <summary>
    /// Cluster Secret Key. Used to authenticate services!
    /// </summary>
    public string SecretKey { get; set; } = Guid.NewGuid().ToString();
}