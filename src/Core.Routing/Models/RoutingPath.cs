using System.Collections.Generic;
using KRouter.Core.Geometry;

namespace KRouter.Core.Routing.Models
{
    public class RoutingPath
    {
        public List<RoutingNode> Nodes { get; }
        public double TotalCost { get; set; }
        public string NetName { get; set; }
        public long TrackWidth { get; set; }

        public RoutingPath(string netName = "", long trackWidth = 250_000)
        {
            Nodes = new List<RoutingNode>();
            NetName = netName;
            TrackWidth = trackWidth;
        }

        public List<Line2D> ToSegments()
        {
            var segments = new List<Line2D>();
            
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                if (Nodes[i].Layer == Nodes[i + 1].Layer)
                {
                    segments.Add(new Line2D(Nodes[i].Position, Nodes[i + 1].Position, TrackWidth));
                }
            }
            
            return segments;
        }

        public List<Via> GetVias()
        {
            var vias = new List<Via>();
            
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                if (Nodes[i].Layer != Nodes[i + 1].Layer)
                {
                    vias.Add(new Via
                    {
                        Position = Nodes[i].Position,
                        FromLayer = Nodes[i].Layer,
                        ToLayer = Nodes[i + 1].Layer
                    });
                }
            }
            
            return vias;
        }
    }

    public class Via
    {
        public Point2D Position { get; set; }
        public string FromLayer { get; set; } = "";
        public string ToLayer { get; set; } = "";
        public long Diameter { get; set; } = 600_000;
        public long Drill { get; set; } = 300_000;
    }
}
