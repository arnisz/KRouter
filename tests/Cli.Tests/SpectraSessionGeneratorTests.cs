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
    }
}

