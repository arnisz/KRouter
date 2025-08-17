using System;
using System.IO;
using System.Threading.Tasks;
using KRouter.Cli;
using KRouter.Core.DRC;
using KRouter.Core.DRC.Models;
using KRouter.Core.Routing;
using KRouter.Core.Routing.Algorithms;
using Xunit;

namespace KRouter.Tests.Cli
{
    public class RouteServiceTests
    {
        private static RouteService CreateService()
        {
            var ruleEngine = new RuleEngine();
            ruleEngine.LoadRules(new DesignRules());
            var engine = new RoutingEngine(new AStarRouter(), ruleEngine);
            return new RouteService(engine);
        }

        [Fact]
        public async Task RouteAsync_CreatesOutputFile_AndReport()
        {
            var service = CreateService();
            var tempDir = Path.Combine(Path.GetTempPath(), "krouter_test", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var input = Path.Combine(tempDir, "board.dsn");
            var dsn = "(pcb test (parser) (resolution um 1) (unit um) " +
                      "(structure (layer F.Cu) (boundary (path pcb 0 0 0 10 0 10 10 0 10))) " +
                      "(library (image foo (pin pad 1 0 0))) " +
                      "(placement (component foo (place A 0 0 front 0)) (component foo (place B 10 0 front 0))) " +
                      "(network (net N1 (pins A-1 B-1))) )";
            await File.WriteAllTextAsync(input, dsn);
            var output = Path.Combine(tempDir, "board.ses");

            var response = await service.RouteAsync(new RouteRequest
            {
                InputFile = new FileInfo(input),
                OutputFile = new FileInfo(output),
                Profile = "Fast",
                Layers = new[] { "F.Cu" },
                GridSize = 50_000
            });

            Assert.True(response.Success);
            Assert.True(File.Exists(output));
            Assert.Contains("Routing Report", response.HumanReadableReport);
            Assert.NotEmpty(response.ToJson());
            var sesContent = await File.ReadAllTextAsync(output);
            Assert.Contains("(routes", sesContent);
        }
    }
}

