using System;
using System.IO;
using KRouter.Cli;
using Xunit;

namespace KRouter.Tests.Cli
{
    public class SpectraSessionGeneratorTests
    {
        [Fact]
        public void FromDsn_ProducesPlacementsAndNets()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var dsnPath = Path.Combine(projectRoot, "samples", "boards", "example.dsn");
            var dsn = File.ReadAllText(dsnPath);

            var ses = SpectraSessionGenerator.FromDsn(dsn, "example");

            Assert.Contains("(routes", ses);
            Assert.Contains("(component \"Package_TO_SOT_SMD:SOT-23\"", ses);
            Assert.Contains("(place Q1 1259375 -946750 front 0)", ses);
            Assert.Contains("(net GND", ses);
        }

        [Fact]
        public void FromRouting_IncludesWireSegments()
        {
            var data = new DsnData { Resolution = 10, Layers = new System.Collections.Generic.List<string> { "F.Cu" } };
            var net = new KRouter.Core.Routing.Net
            {
                Name = "N1",
                Route = new KRouter.Core.Routing.Models.RoutingPath("N1")
            };
            net.Route!.Nodes.Add(new KRouter.Core.Routing.Models.RoutingNode(new KRouter.Core.Geometry.Point2D(0, 0), "F.Cu", 0));
            net.Route.Nodes.Add(new KRouter.Core.Routing.Models.RoutingNode(new KRouter.Core.Geometry.Point2D(1000, 0), "F.Cu", 0));
            var result = new KRouter.Core.Routing.RoutingResult();
            result.RoutedNets.Add(net);

            var ses = SpectraSessionGenerator.FromRouting(data, result, "design");

            Assert.Contains("(wire", ses);
            Assert.Contains("network_out", ses);
        }
    }
}

