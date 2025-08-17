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
            // Teste für tatsächlich vorhandene Komponenten aus der example.dsn
            Assert.Contains("(component \"Package_TO_SOT_SMD:SOT-23\"", ses);
            Assert.Contains("(place Q1 125937", ses); // Q1 existiert in der DSN
            Assert.Contains("(net GND", ses); // GND existiert in der network Sektion
        }
    }
}
