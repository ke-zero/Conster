using Conster.Core;

namespace Conster.Application.Services;

public class ClusterService : IClusterService
{
    public List<Cluster> Clusters { get; }

    public ClusterService()
    {
        Clusters =
        [
            new Cluster(),
            new Cluster(),
            new Cluster()
        ];
    }
}