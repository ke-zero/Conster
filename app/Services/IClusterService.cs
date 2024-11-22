using Conster.Core;

namespace Conster.Application.Services;

public interface IClusterService
{
    List<Cluster> Clusters { get; }
}