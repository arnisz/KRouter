using Xunit;
using KRouter.Core.Geometry;

namespace Core.Geometry.Tests
{
    public class BoundingBoxTests
    {
        [Fact]
        public void Contains_PointInside_ReturnsTrue()
        {
            var box = new BoundingBox(new Point2D(0, 0), new Point2D(10, 10));
            Assert.True(box.Contains(new Point2D(5, 5)));
        }

        [Fact]
        public void Contains_PointOutside_ReturnsFalse()
        {
            var box = new BoundingBox(new Point2D(0, 0), new Point2D(10, 10));
            Assert.False(box.Contains(new Point2D(15, 5)));
        }
    }
}

