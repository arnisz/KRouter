using KRouter.Cli;
using Xunit;

namespace KRouter.Tests.Cli
{
    public class SesGenerationTests
    {
        [Fact]
        public void GenerateKiCadCompatibleSES_UsesRoutingSection()
        {
            var ses = Program.GenerateKiCadCompatibleSES("design", 2);
            Assert.Contains("(routing", ses);
            Assert.DoesNotContain("(was_is", ses);
            Assert.DoesNotContain("(routes", ses);
            Assert.Contains("(library", ses);
            Assert.Contains("(network", ses);
        }
    }
}
