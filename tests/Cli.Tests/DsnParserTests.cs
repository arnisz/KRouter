using System;
using System.IO;
using KRouter.Cli;
using Xunit;

namespace KRouter.Tests.Cli
{
    public class DsnParserTests
    {
        [Fact]
        public void Parse_ExampleDsn_FindsNets()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var dsnPath = Path.Combine(projectRoot, "samples", "boards", "example.dsn");
            var dsn = File.ReadAllText(dsnPath);
            var parser = new DsnParser();

            var data = parser.Parse(dsn);

            Assert.Contains(data.Nets, n => n.Name == "GND");
            Assert.All(data.Nets, n => Assert.True(n.Pins.Count >= 2));
        }
    }
}
