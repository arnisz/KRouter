using Xunit;
using KRouter.Core.Geometry;

namespace Core.Geometry.Tests
{
    public class Point2DTests
    {
        [Fact]
        public void OperatorPlus_AddsCorrectly()
        {
            var a = new Point2D(1, 2);
            var b = new Point2D(3, 4);
            Assert.Equal(new Point2D(4, 6), a + b);
        }

        [Fact]
        public void ManhattanDistanceTo_CalculatesCorrectly()
        {
            var a = new Point2D(1, 2);
            var b = new Point2D(4, 6);
            Assert.Equal(7, a.ManhattanDistanceTo(b));
        }
    }
}
