using System;

namespace KRouter.Core.Geometry
{
    public readonly record struct Line2D(Point2D Start, Point2D End, long Width = 0)
    {
        public double Length => Math.Sqrt(Math.Pow(Start.X - End.X, 2) + Math.Pow(Start.Y - End.Y, 2));
    }
}
