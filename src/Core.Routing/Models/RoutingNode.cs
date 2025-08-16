using System;
using KRouter.Core.Geometry;

namespace KRouter.Core.Routing.Models
{
    public class RoutingNode : IEquatable<RoutingNode>
    {
        public Point2D Position { get; }
        public string Layer { get; }
        public int LayerIndex { get; }

        public RoutingNode(Point2D position, string layer, int layerIndex)
        {
            Position = position;
            Layer = layer;
            LayerIndex = layerIndex;
        }

        public bool Equals(RoutingNode? other)
        {
            if (other == null) return false;
            return Position.Equals(other.Position) && Layer == other.Layer;
        }

        public override bool Equals(object? obj) => obj is RoutingNode node && Equals(node);
        public override int GetHashCode() => HashCode.Combine(Position, Layer);
        public override string ToString() => $"{Position}@{Layer}";
    }
}
