using Xunit;
using KRouter.Core.Geometry;

namespace Core.Geometry.Tests
{
    public class Line2DTests
    {
        [Fact]
        public void Length_CalculatesCorrectly()
        {
            var l = new Line2D(new Point2D(0, 0), new Point2D(3, 4));
            Assert.Equal(5, l.Length, 5);
        }
    }
}

