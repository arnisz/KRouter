using Xunit;
using KRouter.Core.Geometry;

namespace Core.Geometry.Tests
{
    public class GeometryHelpersTests
    {
        [Fact]
        public void SnapToGrid_SnapsCorrectly()
        {
            var p = new Point2D(13, 27);
            var snapped = GeometryHelpers.SnapToGrid(p, 10);
            Assert.Equal(new Point2D(10, 20), snapped);
        }
    }
}

