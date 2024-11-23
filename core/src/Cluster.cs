using System.Net;
using System.Security.AccessControl;

namespace Conster.Core;


/// <summary>
/// Cluster Definition
/// </summary>
public class Cluster
{
    /// <summary>
    /// Cluster Id
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Cluster Name
    /// </summary>
    public string Name { get; set; } = Guid.NewGuid().ToString();


    /// <summary>
    /// Cluster CPU Details (`core count`C/`thread count`T @ `frequency` GHz) e.g: `2C/2T @ 3.5 GHz`
    /// </summary>
    public string CPU { get; set; } = "2C/2T @ 3.5 GHz";
    
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
    /// Return `true` if server is active an `false` otherwise.
    /// </summary>
    public bool Active { get; set; } = default;
    
    /// <summary>
    /// Cluster Secret Key. Used to authenticate services!
    /// </summary>
    public string SecretKey { get; set; } = Guid.NewGuid().ToString();

    
    /// <summary>
    /// Cluster creation Time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}