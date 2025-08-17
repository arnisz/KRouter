using System.Collections.Generic;
using Xunit;
using KRouter.Cli;

namespace Cli.Tests
{
    public class SpectraSessionGeneratorParserOptionsTests
    {
        [Fact]
        public void ExtractParserOptions_FindsAllOptions()
        {
            string dsn = @"
            (pcb ""board1""
              (parser
                (string_quote "")
                (space_in_quoted_tokens on)
                (case_sensitive off)
                (write_resolution 10000)
                (host_cad KICAD)
                (host_version ""8"")
              )
            )";
            var root = SExprParser.Parse(dsn);
            var options = typeof(SpectraSessionGenerator)
                .GetMethod("ExtractParserOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { root }) as Dictionary<string, string>;

            Assert.Equal("\"", options["string_quote"]);
            Assert.Equal("on", options["space_in_quoted_tokens"]);
            Assert.Equal("off", options["case_sensitive"]);
            Assert.Equal("10000", options["write_resolution"]);
            Assert.Equal("KICAD", options["host_cad"]);
            Assert.Equal("8", options["host_version"]);
        }
    }
}
