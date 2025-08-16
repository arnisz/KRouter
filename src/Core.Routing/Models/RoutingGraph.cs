using System;
using System.Collections.Generic;
using KRouter.Core.Geometry;

namespace KRouter.Core.Routing.Models
{
    public class RoutingGraph
    {
        private readonly Dictionary<RoutingNode, List<RoutingNode>> _adjacency;
        private readonly Dictionary<Point2D, HashSet<string>> _obstacles;
        private readonly long _gridSize;
        private readonly BoundingBox _bounds;
        private readonly List<string> _layers;

        public long GridSize => _gridSize;
        public BoundingBox Bounds => _bounds;
        public IReadOnlyList<string> Layers => _layers;

        public RoutingGraph(BoundingBox bounds, long gridSize, List<string> layers)
        {
            _bounds = bounds;
            _gridSize = gridSize;
            _layers = layers;
            _adjacency = new Dictionary<RoutingNode, List<RoutingNode>>();
            _obstacles = new Dictionary<Point2D, HashSet<string>>();
        }

        public void AddObstacle(Point2D position, string layer)
        {
            var snapped = GeometryHelpers.SnapToGrid(position, _gridSize);
            
            if (!_obstacles.ContainsKey(snapped))
                _obstacles[snapped] = new HashSet<string>();
            
            _obstacles[snapped].Add(layer);
        }

        public void AddObstacleLine(Line2D line, string layer)
        {
            // Bresenham's line algorithm to mark all grid points along the line
            var start = GeometryHelpers.SnapToGrid(line.Start, _gridSize);
            var end = GeometryHelpers.SnapToGrid(line.End, _gridSize);
            
            var dx = Math.Abs(end.X - start.X);
            var dy = Math.Abs(end.Y - start.Y);
            var sx = start.X < end.X ? _gridSize : -_gridSize;
            var sy = start.Y < end.Y ? _gridSize : -_gridSize;
            var err = dx - dy;
            
            var current = start;
            while (true)
            {
                AddObstacle(current, layer);
                
                if (current == end) break;
                
                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    current = new Point2D(current.X + sx, current.Y);
                }
                if (e2 < dx)
                {
                    err += dx;
                    current = new Point2D(current.X, current.Y + sy);
                }
            }
        }

        public bool IsObstacle(RoutingNode node)
        {
            return _obstacles.ContainsKey(node.Position) && 
                   _obstacles[node.Position].Contains(node.Layer);
        }

        public IEnumerable<RoutingNode> GetNeighbors(RoutingNode node)
        {
            var neighbors = new List<RoutingNode>();
            
            // Same layer movements (orthogonal and 45-degree)
            var directions = new[]
            {
                new Point2D(_gridSize, 0),           // Right
                new Point2D(-_gridSize, 0),          // Left
                new Point2D(0, _gridSize),           // Up
                new Point2D(0, -_gridSize),          // Down
                new Point2D(_gridSize, _gridSize),   // Diagonal UR
                new Point2D(_gridSize, -_gridSize),  // Diagonal DR
                new Point2D(-_gridSize, _gridSize),  // Diagonal UL
                new Point2D(-_gridSize, -_gridSize)  // Diagonal DL
            };
            
            foreach (var dir in directions)
            {
                var newPos = node.Position + dir;
                
                if (!_bounds.Contains(newPos))
                    continue;
                
                var newNode = new RoutingNode(newPos, node.Layer, node.LayerIndex);
                
                if (!IsObstacle(newNode))
                {
                    neighbors.Add(newNode);
                }
            }
            
            // Layer transitions (vias)
            if (node.LayerIndex > 0)
            {
                var belowNode = new RoutingNode(node.Position, _layers[node.LayerIndex - 1], node.LayerIndex - 1);
                if (!IsObstacle(belowNode))
                {
                    neighbors.Add(belowNode);
                }
            }
            
            if (node.LayerIndex < _layers.Count - 1)
            {
                var aboveNode = new RoutingNode(node.Position, _layers[node.LayerIndex + 1], node.LayerIndex + 1);
                if (!IsObstacle(aboveNode))
                {
                    neighbors.Add(aboveNode);
                }
            }
            
            return neighbors;
        }

        public void ClearObstaclesForNet(string netName)
        {
            // Placeholder for clearing obstacles by net
        }
    }
}
