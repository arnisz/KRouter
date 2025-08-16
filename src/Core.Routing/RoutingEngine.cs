using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KRouter.Core.DRC;
using KRouter.Core.DRC.Models;
using KRouter.Core.Geometry;
using KRouter.Core.Routing.Algorithms;
using KRouter.Core.Routing.Models;

namespace KRouter.Core.Routing
{
    public class Net
    {
        public string Name { get; set; } = "";
        public List<Point2D> Pins { get; set; } = new();
        public int Priority { get; set; } = 0;
        public bool IsRouted { get; set; } = false;
        public RoutingPath? Route { get; set; }
    }

    public class RoutingResult
    {
        public bool Success { get; set; }
        public List<Net> RoutedNets { get; set; } = new();
        public List<Net> FailedNets { get; set; } = new();
        public List<Violation> Violations { get; set; } = new();
        public TimeSpan ElapsedTime { get; set; }
        public int TotalVias { get; set; }
        public double TotalLength { get; set; }
    }

    public class RoutingEngine
    {
        private readonly IRoutingAlgorithm _algorithm;
        private readonly IRuleEngine _ruleEngine;
        private readonly CostFunction _costFunction;
        private RoutingGraph? _graph;

        public RoutingEngine(IRoutingAlgorithm algorithm, IRuleEngine ruleEngine)
        {
            _algorithm = algorithm;
            _ruleEngine = ruleEngine;
            _costFunction = new CostFunction();
        }

        public async Task<RoutingResult> RouteBoard(
            List<Net> nets,
            BoundingBox bounds,
            long gridSize,
            List<string> layers,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var result = new RoutingResult();

            _graph = new RoutingGraph(bounds, gridSize, layers);

            var sortedNets = nets.OrderByDescending(n => n.Priority).ToList();

            foreach (var net in sortedNets)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var routed = await RouteNet(net, layers);
                
                if (routed)
                {
                    result.RoutedNets.Add(net);
                    
                    if (net.Route != null)
                    {
                        foreach (var segment in net.Route.ToSegments())
                        {
                            var layer = net.Route.Nodes.First(n =>
                                n.Position == segment.Start || n.Position == segment.End).Layer;
                            _graph.AddObstacleLine(segment, layer);
                        }
                    }
                }
                else
                {
                    result.FailedNets.Add(net);
                    
                    if (await RipUpAndReroute(net, sortedNets, layers))
                    {
                        result.FailedNets.Remove(net);
                        result.RoutedNets.Add(net);
                    }
                }
            }

            result.Success = result.FailedNets.Count == 0;
            result.ElapsedTime = DateTime.UtcNow - startTime;
            
            foreach (var net in result.RoutedNets.Where(n => n.Route != null))
            {
                result.TotalVias += net.Route!.GetVias().Count;
                result.TotalLength += net.Route.ToSegments().Sum(s => s.Length);
            }

            return result;
        }

        private async Task<bool> RouteNet(Net net, List<string> layers)
        {
            if (net.Pins.Count < 2)
                return true;

            var path = new RoutingPath(net.Name);
            
            for (int i = 0; i < net.Pins.Count - 1; i++)
            {
                var startNode = new RoutingNode(net.Pins[i], layers[0], 0);
                var endNode = new RoutingNode(net.Pins[i + 1], layers[0], 0);

                var segment = _algorithm.FindPath(startNode, endNode, _graph!, _costFunction);
                
                if (segment == null)
                    return false;

                if (path.Nodes.Count > 0 && path.Nodes.Last().Equals(segment.Nodes.First()))
                {
                    path.Nodes.AddRange(segment.Nodes.Skip(1));
                }
                else
                {
                    path.Nodes.AddRange(segment.Nodes);
                }
            }

            net.Route = path;
            net.IsRouted = true;
            return true;
        }

        private async Task<bool> RipUpAndReroute(Net failedNet, List<Net> allNets, List<string> layers)
        {
            var conflictingNets = FindConflictingNets(failedNet, allNets);
            
            foreach (var conflictingNet in conflictingNets.Take(1))
            {
                _graph!.ClearObstaclesForNet(conflictingNet.Name);
                conflictingNet.IsRouted = false;
                conflictingNet.Route = null;

                if (await RouteNet(failedNet, layers))
                {
                    await RouteNet(conflictingNet, layers);
                    return true;
                }
            }

            return false;
        }

        private List<Net> FindConflictingNets(Net net, List<Net> allNets)
        {
            return allNets
                .Where(n => n.IsRouted && n.Priority < net.Priority)
                .OrderBy(n => n.Priority)
                .ToList();
        }
    }
}
