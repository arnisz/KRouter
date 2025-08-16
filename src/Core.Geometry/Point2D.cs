using System;

namespace KRouter.Core.Geometry
{
    public readonly record struct Point2D(long X, long Y)
    {
        public static Point2D operator +(Point2D a, Point2D b) => new Point2D(a.X + b.X, a.Y + b.Y);
        public long ManhattanDistanceTo(Point2D other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }
}
