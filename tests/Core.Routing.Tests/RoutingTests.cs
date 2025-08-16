using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using KRouter.Core.DRC;
using KRouter.Core.DRC.Models;
using KRouter.Core.Geometry;
using KRouter.Core.Routing;
using KRouter.Core.Routing.Algorithms;
using KRouter.Core.Routing.Models;

namespace KRouter.Tests.Core.Routing
{
    public class AStarRouterTests
    {
        [Fact]
        public void AStarRouter_FindsSimplePath()
        {
            var bounds = new BoundingBox(new Point2D(0, 0), new Point2D(10_000_000, 10_000_000));
            var graph = new RoutingGraph(bounds, 100_000, new List<string> { "F.Cu" });
            var router = new AStarRouter();
            var costFunction = new CostFunction();

            var start = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var end = new RoutingNode(new Point2D(1_000_000, 0), "F.Cu", 0);

            var path = router.FindPath(start, end, graph, costFunction);

            Assert.NotNull(path);
            Assert.True(path!.Nodes.Count > 0);
            Assert.Equal(start, path.Nodes.First());
            Assert.Equal(end, path.Nodes.Last());
        }

        [Fact]
        public void AStarRouter_AvoidsObstacles()
        {
            var bounds = new BoundingBox(new Point2D(0, 0), new Point2D(10_000_000, 10_000_000));
            var graph = new RoutingGraph(bounds, 100_000, new List<string> { "F.Cu" });
            var router = new AStarRouter();
            var costFunction = new CostFunction();

            var obstacle = new Line2D(
                new Point2D(500_000, -1_000_000),
                new Point2D(500_000, 1_000_000)
            );
            graph.AddObstacleLine(obstacle, "F.Cu");

            var start = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var end = new RoutingNode(new Point2D(1_000_000, 0), "F.Cu", 0);

            var path = router.FindPath(start, end, graph, costFunction);

            Assert.NotNull(path);
            Assert.Contains(path!.Nodes, n => n.Position.Y != 0);
        }

        [Fact]
        public void AStarRouter_NoPath_ReturnsNull()
        {
            var bounds = new BoundingBox(new Point2D(0, 0), new Point2D(10_000_000, 10_000_000));
            var graph = new RoutingGraph(bounds, 100_000, new List<string> { "F.Cu" });
            var router = new AStarRouter();
            var costFunction = new CostFunction();

            // Create a solid vertical wall across the entire routing area so that
            // no path from start to end exists on the given layer. Using a line
            // ensures every grid point along the wall is marked as an obstacle.
            var blockingLine = new Line2D(
                new Point2D(500_000, 0),
                new Point2D(500_000, 10_000_000)
            );
            graph.AddObstacleLine(blockingLine, "F.Cu");

            var start = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var end = new RoutingNode(new Point2D(1_000_000, 0), "F.Cu", 0);

            var path = router.FindPath(start, end, graph, costFunction);

            Assert.Null(path);
        }
    }

    public class RoutingEngineTests
    {
        private RoutingEngine CreateEngine()
        {
            var ruleEngine = new RuleEngine();
            ruleEngine.LoadRules(new DesignRules());
            var algorithm = new AStarRouter();
            return new RoutingEngine(algorithm, ruleEngine);
        }

        [Fact]
        public async Task RoutingEngine_RoutesSimpleNet()
        {
            var engine = CreateEngine();
            var nets = new List<Net>
            {
                new Net
                {
                    Name = "NET1",
                    Pins = new List<Point2D>
                    {
                        new Point2D(0, 0),
                        new Point2D(1_000_000, 0)
                    },
                    Priority = 1
                }
            };

            var bounds = new BoundingBox(new Point2D(-1_000_000, -1_000_000), new Point2D(10_000_000, 10_000_000));
            var result = await engine.RouteBoard(nets, bounds, 100_000, new List<string> { "F.Cu" });

            Assert.True(result.Success);
            Assert.Single(result.RoutedNets);
            Assert.Empty(result.FailedNets);
            Assert.True(result.TotalLength > 0);
        }

        [Fact]
        public async Task RoutingEngine_RoutesMultipleNets()
        {
            var engine = CreateEngine();
            var nets = new List<Net>
            {
                new Net
                {
                    Name = "NET1",
                    Pins = new List<Point2D>
                    {
                        new Point2D(0, 0),
                        new Point2D(2_000_000, 0)
                    },
                    Priority = 2
                },
                new Net
                {
                    Name = "NET2",
                    Pins = new List<Point2D>
                    {
                        new Point2D(0, 1_000_000),
                        new Point2D(2_000_000, 1_000_000)
                    },
                    Priority = 1
                }
            };

            var bounds = new BoundingBox(new Point2D(-1_000_000, -1_000_000), new Point2D(10_000_000, 10_000_000));
            var result = await engine.RouteBoard(nets, bounds, 100_000, new List<string> { "F.Cu", "B.Cu" });

            Assert.True(result.Success);
            Assert.Equal(2, result.RoutedNets.Count);
            Assert.Empty(result.FailedNets);
        }

        [Fact]
        public async Task RoutingEngine_RespectsNetPriority()
        {
            var engine = CreateEngine();
            var nets = new List<Net>
            {
                new Net
                {
                    Name = "LOW_PRIORITY",
                    Pins = new List<Point2D>
                    {
                        new Point2D(0, 500_000),
                        new Point2D(2_000_000, 500_000)
                    },
                    Priority = 1
                },
                new Net
                {
                    Name = "HIGH_PRIORITY",
                    Pins = new List<Point2D>
                    {
                        new Point2D(1_000_000, 0),
                        new Point2D(1_000_000, 1_000_000)
                    },
                    Priority = 10
                }
            };

            var bounds = new BoundingBox(new Point2D(-1_000_000, -1_000_000), new Point2D(10_000_000, 10_000_000));
            var result = await engine.RouteBoard(nets, bounds, 100_000, new List<string> { "F.Cu" });

            var highPriorityNet = result.RoutedNets.FirstOrDefault(n => n.Name == "HIGH_PRIORITY");
            Assert.NotNull(highPriorityNet);
            Assert.NotNull(highPriorityNet!.Route);
            
            var segments = highPriorityNet.Route!.ToSegments();
            Assert.True(segments.All(s => s.Start.X == s.End.X));
        }
    }

    public class CostFunctionTests
    {
        [Fact]
        public void CostFunction_CalculatesBasicDistance()
        {
            var costFunc = new CostFunction { LengthWeight = 1.0 };
            var from = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var to = new RoutingNode(new Point2D(1_000_000, 0), "F.Cu", 0);

            var cost = costFunc.CalculateCost(from, to);

            Assert.Equal(1_000_000, cost);
        }

        [Fact]
        public void CostFunction_AddsViaPenalty()
        {
            var costFunc = new CostFunction 
            { 
                LengthWeight = 1.0,
                ViaWeight = 50.0
            };
            var from = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var to = new RoutingNode(new Point2D(0, 0), "B.Cu", 1);

            var cost = costFunc.CalculateCost(from, to);

            Assert.Equal(50.0, cost);
        }

        [Fact]
        public void CostFunction_ConsidersCongestion()
        {
            var costFunc = new CostFunction 
            { 
                LengthWeight = 0,
                CongestionWeight = 10.0
            };
            var from = new RoutingNode(new Point2D(0, 0), "F.Cu", 0);
            var to = new RoutingNode(new Point2D(100_000, 0), "F.Cu", 0);

            costFunc.UpdateCongestion(to, 3);

            var cost = costFunc.CalculateCost(from, to);

            Assert.Equal(90.0, cost);
        }
    }
}
