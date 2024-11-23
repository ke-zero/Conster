using Conster.Core;

namespace Conster.Application.Services;

public class ClusterService : IClusterService
{
    public List<Cluster> Clusters { get; }

    public Cluster Add(Cluster cluster)
    {
        if (string.IsNullOrWhiteSpace(cluster.Id)) cluster.Id = Guid.NewGuid().ToString();
        
        cluster.CreatedAt = DateTime.UtcNow;

        Clusters.Add(cluster);

        return cluster;
    }

    public Cluster? Remove(string id)
    {
        var cluster = Clusters.FirstOrDefault(x => x.Id == id);

        if (cluster != null) Clusters.Remove(cluster);

        return cluster;
    }

    public void Update(Cluster cluster)
    {
        var selected = Clusters.FirstOrDefault(x => x.Id == cluster.Id);

        if (selected == null) return;

        Clusters[Clusters.IndexOf(selected)] = cluster;
    }

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