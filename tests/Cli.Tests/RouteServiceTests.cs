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
            await File.WriteAllTextAsync(input, "(design (net N1) (net N2))");
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
        }
    }
}

