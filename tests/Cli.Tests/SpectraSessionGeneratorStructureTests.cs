using System.Collections.Generic;
using Xunit;
using KRouter.Cli;

namespace Cli.Tests
{
    public class SpectraSessionGeneratorStructureTests
    {
        [Fact]
        public void ExtractStructure_FindsBoundariesAndLayers()
        {
            // Beispielhafte S-Expression für structure mit boundary und layer_descriptor
            string dsn = @"
            (pcb ""board1""
              (structure
                (boundary (rect pcb 0 0 1000 2000))
                (boundary (rect signal 10 10 990 1990))
                (layer_descriptor F.Cu)
                (layer_descriptor B.Cu)
              )
            )";
            var root = SExprParser.Parse(dsn);
            var info = typeof(SpectraSessionGenerator)
                .GetMethod("ExtractStructure", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { root });

            var boundaries = (List<(string, string, string, string, string)>)info.GetType().GetProperty("Boundaries").GetValue(info);
            var layers = (List<string>)info.GetType().GetProperty("Layers").GetValue(info);

            Assert.Contains(boundaries, b => b.Item1 == "pcb" && b.Item2 == "0" && b.Item3 == "0" && b.Item4 == "1000" && b.Item5 == "2000");
            Assert.Contains(boundaries, b => b.Item1 == "signal" && b.Item2 == "10" && b.Item3 == "10" && b.Item4 == "990" && b.Item5 == "1990");
            Assert.Contains(layers, l => l == "F.Cu");
            Assert.Contains(layers, l => l == "B.Cu");
        }
    }
}
