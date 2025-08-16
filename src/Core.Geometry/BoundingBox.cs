namespace KRouter.Core.Geometry
{
    public readonly record struct BoundingBox(Point2D Min, Point2D Max)
    {
        public bool Contains(Point2D p) => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;
    }
}
