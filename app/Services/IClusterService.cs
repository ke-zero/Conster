using Conster.Core;

namespace Conster.Application.Services;

public interface IClusterService
{
    List<Cluster> Clusters { get; }

    Cluster Add(Cluster cluster);
    Cluster? Remove(string id);
    void Update(Cluster cluster);
}