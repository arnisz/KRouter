namespace KRouter.Core.Geometry
{
    public static class GeometryHelpers
    {
        public static Point2D SnapToGrid(Point2D p, long gridSize)
        {
            return new Point2D(
                (p.X / gridSize) * gridSize,
                (p.Y / gridSize) * gridSize
            );
        }
    }
}
