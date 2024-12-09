using System.Net;
using Conster.Core.Worker;

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
    /// Cluster Internet Port
    /// </summary>
    public ushort Port { get; set; } = 10101;

    /// <summary>
    /// Cluster Internet IPv4
    /// </summary>
    public string IPv4 { get; set; } = IPAddress.Any.ToString();

    /// <summary>
    /// Cluster Api Key. Used to authenticate services!
    /// </summary>
    public string ApiKey { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Cluster Active status
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Cluster Realtime Status
    /// </summary>
    public WorkerData.HostStatusResponse Status { get; set; } = new();

    /// <summary>
    /// Cluster activation time (reset when disconnect)
    /// </summary>
    public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cluster creation Time
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}