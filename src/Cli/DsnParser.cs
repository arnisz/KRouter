using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using KRouter.Core.Geometry;

namespace KRouter.Cli
{
    /// <summary>
    /// Basic parser for a minimal subset of the Specctra DSN format.
    /// Extracts resolution, layers, board boundary and net pin positions.
    /// </summary>
    public class DsnParser
    {
        /// <summary>Parses DSN content and returns structured data.</summary>
        /// <param name="dsnContent">Raw DSN file content.</param>
        /// <returns>Parsed DSN information.</returns>
        public DsnData Parse(string dsnContent)
        {
            var resolution = GetResolution(dsnContent);
            var layers = ParseLayers(dsnContent).ToList();
            var placements = ParsePlacements(dsnContent);
            var library = ParseLibrary(dsnContent);
            var nets = ParseNets(dsnContent, placements, library, resolution).ToList();
            var boundary = ParseBoundary(dsnContent, resolution);
            return new DsnData
            {
                Resolution = resolution,
                Layers = layers,
                Boundary = boundary,
                Nets = nets
            };
        }

        private static long GetResolution(string dsn)
        {
            var m = Regex.Match(dsn, "\\(resolution\\s+\\w+\\s+(\\d+)\\)");
            if (m.Success && long.TryParse(m.Groups[1].Value, out var r))
                return r;
            return 1;
        }

        private static IEnumerable<string> ParseLayers(string dsn)
        {
            var matches = Regex.Matches(dsn, "\\(layer\\s+([^\\s\\)]+)");
            foreach (Match m in matches)
                yield return m.Groups[1].Value;
        }

        private static Dictionary<string, ComponentPlacement> ParsePlacements(string dsn)
        {
            var result = new Dictionary<string, ComponentPlacement>();
            var lines = dsn.Split('\n');
            bool inPlacement = false;
            int depth = 0;
            string? currentImage = null;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (!inPlacement)
                {
                    if (line.StartsWith("(placement"))
                    {
                        inPlacement = true; depth = 1;
                    }
                    continue;
                }
                depth += line.Count(c => c == '(') - line.Count(c => c == ')');
                if (line.StartsWith("(component"))
                {
                    var m = Regex.Match(line, "^\\(component\\s+\"?([^\\\"]+)\"?");
                    if (m.Success)
                        currentImage = m.Groups[1].Value;
                }
                else if (line.StartsWith("(place") && currentImage != null)
                {
                    var m = Regex.Match(line, "^\\(place\\s+(\\S+)\\s+([-+]?\\d*\\.?\\d+)\\s+([-+]?\\d*\\.?\\d+)\\s+(front|back)\\s+([-+]?\\d*\\.?\\d+)");
                    if (m.Success)
                    {
                        var reference = m.Groups[1].Value;
                        var x = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                        var y = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                        var rot = double.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture);
                        result[reference] = new ComponentPlacement(currentImage, x, y, rot);
                    }
                }
                if (depth <= 0)
                    break;
            }
            return result;
        }

        private static Dictionary<string, Dictionary<string, (double X, double Y)>> ParseLibrary(string dsn)
        {
            var result = new Dictionary<string, Dictionary<string, (double X, double Y)>>();
            var lines = dsn.Split('\n');
            bool inLibrary = false;
            int depth = 0;
            string? currentImage = null;
            int imageDepth = 0;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (!inLibrary)
                {
                    if (line.StartsWith("(library"))
                    {
                        inLibrary = true; depth = 1;
                    }
                    continue;
                }
                depth += line.Count(c => c == '(') - line.Count(c => c == ')');
                if (line.StartsWith("(image"))
                {
                    var m = Regex.Match(line, "^\\(image\\s+\"?([^\\\"]+)\"?");
                    if (m.Success)
                    {
                        currentImage = m.Groups[1].Value;
                        result[currentImage] = new Dictionary<string, (double, double)>();
                        imageDepth = 1;
                    }
                    continue;
                }
                if (currentImage != null)
                {
                    imageDepth += line.Count(c => c == '(') - line.Count(c => c == ')');
                    if (line.StartsWith("(pin"))
                    {
                        var m = Regex.Match(line, "\\(pin[^\\)]*\\s(\\S+)\\s([-+]?\\d*\\.?\\d+)\\s([-+]?\\d*\\.?\\d+)\\)");
                        if (m.Success)
                        {
                            var pad = m.Groups[1].Value;
                            var x = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                            var y = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                            result[currentImage][pad] = (x, y);
                        }
                    }
                    if (imageDepth <= 0)
                        currentImage = null;
                }
                if (depth <= 0)
                    break;
            }
            return result;
        }

        private static IEnumerable<DsnNet> ParseNets(string dsn,
            Dictionary<string, ComponentPlacement> placements,
            Dictionary<string, Dictionary<string, (double X, double Y)>> library,
            long resolution)
        {
            var matches = Regex.Matches(dsn, "\\(net\\s+\\\"?([^\\\"\\s]+)\\\"?\\s*\\(pins\\s+([^\\)]+)\\)\\s*\\)", RegexOptions.Singleline);
            foreach (Match m in matches)
            {
                var net = new DsnNet { Name = m.Groups[1].Value };
                var pins = m.Groups[2].Value.Trim()
                    .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in pins)
                {
                    var parts = p.Split('-');
                    if (parts.Length != 2) continue;
                    if (!placements.TryGetValue(parts[0], out var placement)) continue;
                    if (!library.TryGetValue(placement.Image, out var pinsDict)) continue;
                    if (!pinsDict.TryGetValue(parts[1], out var rel)) continue;
                    var (absX, absY) = Transform(rel.X, rel.Y, placement);
                    var scaledX = Scale(absX + placement.X, resolution);
                    var scaledY = Scale(absY + placement.Y, resolution);
                    net.Pins.Add(new Point2D(scaledX, scaledY));
                }
                yield return net;
            }
        }

        private static (double X, double Y) Transform(double x, double y, ComponentPlacement placement)
        {
            var angle = placement.Rotation * Math.PI / 180.0;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var rx = x * cos - y * sin;
            var ry = x * sin + y * cos;
            return (rx, ry);
        }

        private static BoundingBox ParseBoundary(string dsn, long resolution)
        {
            var m = Regex.Match(dsn, "\\(boundary\\s*\\(path\\s+pcb\\s+\\d+\\s+([^\\)]+)\\)\\)", RegexOptions.Singleline);
            if (!m.Success)
                return new BoundingBox(new Point2D(0, 0), new Point2D(0, 0));
            var nums = Regex.Matches(m.Groups[1].Value, "[-+]?\\d*\\.?\\d+");
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            bool isX = true;
            foreach (Match num in nums)
            {
                var v = double.Parse(num.Value, CultureInfo.InvariantCulture);
                if (isX)
                {
                    minX = Math.Min(minX, v); maxX = Math.Max(maxX, v);
                }
                else
                {
                    minY = Math.Min(minY, v); maxY = Math.Max(maxY, v);
                }
                isX = !isX;
            }
            return new BoundingBox(
                new Point2D(Scale(minX, resolution), Scale(minY, resolution)),
                new Point2D(Scale(maxX, resolution), Scale(maxY, resolution)));
        }

        private static long Scale(double value, long resolution)
        {
            return (long)Math.Round(value * resolution);
        }
    }

    /// <summary>Represents parsed DSN information.</summary>
    public class DsnData
    {
        /// <summary>Database resolution (units per micron).</summary>
        public long Resolution { get; set; }
        /// <summary>All available layers.</summary>
        public List<string> Layers { get; set; } = new();
        /// <summary>Board boundary.</summary>
        public BoundingBox Boundary { get; set; }
            = new BoundingBox(new Point2D(0, 0), new Point2D(0, 0));
        /// <summary>Net definitions with pin positions.</summary>
        public List<DsnNet> Nets { get; set; } = new();
    }

    /// <summary>Represents a parsed net.</summary>
    public class DsnNet
    {
        /// <summary>Name of the net.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Absolute pin positions.</summary>
        public List<Point2D> Pins { get; } = new();
    }

    internal record struct ComponentPlacement(string Image, double X, double Y, double Rotation);
}

