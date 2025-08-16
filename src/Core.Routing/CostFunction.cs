using System;
using System.Collections.Generic;
using KRouter.Core.Geometry;
using KRouter.Core.Routing.Models;

namespace KRouter.Core.Routing
{
    public class CostFunction
    {
        public double LengthWeight { get; set; } = 1.0;
        public double ViaWeight { get; set; } = 50.0;
        public double DirectionChangeWeight { get; set; } = 5.0;
        public double CongestionWeight { get; set; } = 10.0;
        public double HistoryWeight { get; set; } = 20.0;
        public double PreferredDirectionWeight { get; set; } = 0.5;

        private readonly Dictionary<RoutingNode, int> _congestionMap = new();
        private readonly Dictionary<RoutingNode, int> _historyMap = new();

        public double CalculateCost(RoutingNode from, RoutingNode to, RoutingNode? previous = null)
        {
            double cost = 0;

            // Base distance cost
            var distance = from.Position.ManhattanDistanceTo(to.Position);
            cost += distance * LengthWeight;

            // Via cost
            if (from.Layer != to.Layer)
            {
                cost += ViaWeight;
            }

            // Direction change cost
            if (previous != null && from.Layer == to.Layer && previous.Layer == from.Layer)
            {
                var dir1 = GetDirection(previous.Position, from.Position);
                var dir2 = GetDirection(from.Position, to.Position);
                if (dir1 != dir2)
                {
                    cost += DirectionChangeWeight;
                }
            }

            // Congestion cost
            if (_congestionMap.ContainsKey(to))
            {
                var congestion = _congestionMap[to];
                cost += Math.Pow(congestion, 2) * CongestionWeight;
            }

            // History cost (for rip-up and reroute)
            if (_historyMap.ContainsKey(to))
            {
                cost += _historyMap[to] * HistoryWeight;
            }

            // Preferred direction (horizontal on even layers, vertical on odd)
            // Instead of rewarding movement in the preferred direction by reducing
            // cost (which could lead to negative costs), we only add a penalty when
            // the movement is primarily in the non-preferred direction. This keeps
            // the base distance cost intact while still biasing the search toward
            // preferred orientations.
            if (from.Layer == to.Layer)
            {
                var dx = Math.Abs(to.Position.X - from.Position.X);
                var dy = Math.Abs(to.Position.Y - from.Position.Y);

                // Even layers favour horizontal movement. Penalize vertical-dominant
                // moves on these layers.
                if (from.LayerIndex % 2 == 0 && dy > dx)
                {
                    cost += PreferredDirectionWeight * distance;
                }
                // Odd layers favour vertical movement. Penalize horizontal-dominant
                // moves on these layers.
                else if (from.LayerIndex % 2 == 1 && dx > dy)
                {
                    cost += PreferredDirectionWeight * distance;
                }
            }

            return cost;
        }

        public void UpdateCongestion(RoutingNode node, int delta)
        {
            if (!_congestionMap.ContainsKey(node))
                _congestionMap[node] = 0;
            _congestionMap[node] += delta;
        }

        public void UpdateHistory(RoutingNode node, int ripUpCount)
        {
            _historyMap[node] = ripUpCount;
        }

        private enum Direction
        {
            Horizontal,
            Vertical,
            Diagonal,
            None
        }

        private Direction GetDirection(Point2D from, Point2D to)
        {
            if (from == to) return Direction.None;
            
            var dx = Math.Abs(to.X - from.X);
            var dy = Math.Abs(to.Y - from.Y);
            
            if (dx == 0) return Direction.Vertical;
            if (dy == 0) return Direction.Horizontal;
            if (dx == dy) return Direction.Diagonal;
            
            return dx > dy ? Direction.Horizontal : Direction.Vertical;
        }
    }
}
