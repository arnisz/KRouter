using KRouter.Core.Routing.Models;

namespace KRouter.Core.Routing.Algorithms
{
    public interface IRoutingAlgorithm
    {
        RoutingPath? FindPath(RoutingNode start, RoutingNode end, RoutingGraph graph, CostFunction costFunction);
    }
}
