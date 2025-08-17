using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KRouter.Core.Routing;

namespace KRouter.Cli
{
    /// <summary>
    /// Minimal converter creating a Specctra SES session from a DSN design file.
    /// Only placement and net names are preserved; no actual routing is performed.
    /// </summary>
    internal static class SpectraSessionGenerator
    {
        /// <summary>
        /// Generates an SES session text from DSN content.
        /// </summary>
        /// <param name="dsnContent">Raw DSN file content.</param>
        /// <param name="designName">Design name without extension.</param>
        /// <returns>SES file content.</returns>
        public static string FromDsn(string dsnContent, string designName)
        {
            var factor = GetResolutionFactor(dsnContent);
            var placements = ParsePlacements(dsnContent, factor);
            var nets = ParseNetNames(dsnContent);

            var sb = new StringBuilder();
            sb.AppendLine($"(session {designName}.ses");
            sb.AppendLine($"  (base_design {designName}.dsn)");
            sb.AppendLine("  (placement");
            sb.AppendLine("    (resolution um 10)");
            foreach (var comp in placements)
            {
                sb.AppendLine($"    (component {comp.Key}");
                foreach (var p in comp.Value)
                {
                    sb.AppendLine($"      (place {p.Reference} {p.X} {p.Y} {p.Layer} {p.Rotation})");
                }
                sb.AppendLine("    )");
            }
            sb.AppendLine("  )");
            sb.AppendLine("  (was_is");
            sb.AppendLine("  )");
            sb.AppendLine("  (routes");
            sb.AppendLine("    (resolution um 10)");
            sb.AppendLine("    (parser");
            sb.AppendLine("      (host_cad \"KRouter\")");
            sb.AppendLine("      (host_version \"1.0.0\")");
            sb.AppendLine("    )");
            sb.AppendLine("    (network");
            foreach (var net in nets)
            {
                sb.AppendLine($"      (net {net})");
            }
            sb.AppendLine("    )");
            sb.AppendLine("  )");
            sb.AppendLine(")");
            return sb.ToString();
        }

        /// <summary>
        /// Generates a Specctra SES file from routing results.
        /// </summary>
        /// <param name="dsn">Parsed DSN data.</param>
        /// <param name="result">Routing result containing routed nets.</param>
        /// <param name="designName">Design name.</param>
        /// <returns>SES file content.</returns>
        public static string FromRouting(DsnData dsn, RoutingResult result, string designName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"(session {designName}.ses");
            sb.AppendLine($"  (base_design {designName}.dsn)");
            sb.AppendLine("  (routes");
            sb.AppendLine($"    (resolution um {dsn.Resolution})");
            sb.AppendLine("    (parser");
            sb.AppendLine("      (host_cad \"KRouter\")");
            sb.AppendLine("      (host_version \"1.0.0\")");
            sb.AppendLine("    )");
            sb.AppendLine("    (network");
            foreach (var n in dsn.Nets)
            {
                sb.AppendLine($"      (net {n.Name})");
            }
            sb.AppendLine("    )");

            foreach (var net in result.RoutedNets.Where(n => n.Route != null))
            {
                sb.AppendLine("    (network_out");
                sb.AppendLine($"      (net {net.Name}");
                foreach (var seg in net.Route!.ToSegments())
                {
                    sb.AppendLine("        (wire");
                    sb.AppendLine($"          (path {dsn.Layers.First()} {seg.Width}");
                    sb.AppendLine($"            {seg.Start.X} {seg.Start.Y} {seg.End.X} {seg.End.Y}");
                    sb.AppendLine("          )");
                    sb.AppendLine("        )");
                }
                foreach (var via in net.Route.GetVias())
                {
                    sb.AppendLine($"        (via \"Via\" {via.Position.X} {via.Position.Y})");
                }
                sb.AppendLine("      )");
                sb.AppendLine("    )");
            }

            sb.AppendLine("  )");
            sb.AppendLine(")");
            return sb.ToString();
        }

        private static long GetResolutionFactor(string dsn)
        {
            var m = Regex.Match(dsn, @"\(resolution\s+\w+\s+(\d+)\)");
            if (m.Success && long.TryParse(m.Groups[1].Value, out var f))
                return f;
            return 1;
        }

        private static Dictionary<string, List<Placement>> ParsePlacements(string dsn, long factor)
        {
            var result = new Dictionary<string, List<Placement>>();
            var lines = dsn.Split('\n');
            bool inPlacement = false;
            int depth = 0;
            string? current = null;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (!inPlacement)
                {
                    if (line.StartsWith("(placement"))
                    {
                        inPlacement = true;
                        depth = 1;
                    }
                    continue;
                }

                depth += line.Count(c => c == '(') - line.Count(c => c == ')');
                if (line.StartsWith("(component"))
                {
                    var m = Regex.Match(line, @"^\(component\s+(.+)$");
                    if (m.Success)
                    {
                        current = m.Groups[1].Value.Trim();
                        if (!result.ContainsKey(current))
                            result[current] = new List<Placement>();
                    }
                }
                else if (line.StartsWith("(place") && current != null)
                {
                    var m = Regex.Match(line, @"^\(place\s+(\S+)\s+([-+]?\d*\.?\d+)\s+([-+]?\d*\.?\d+)\s+(front|back)\s+([-+]?\d*\.?\d+)");
                    if (m.Success)
                    {
                        var reference = m.Groups[1].Value;
                        var x = Scale(m.Groups[2].Value, factor);
                        var y = Scale(m.Groups[3].Value, factor);
                        var layer = m.Groups[4].Value;
                        var rot = (int)Math.Round(double.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture));
                        result[current].Add(new Placement(reference, x, y, layer, rot));
                    }
                }
                if (depth <= 0)
                    break;
            }
            return result;
        }

        private static IEnumerable<string> ParseNetNames(string dsn)
        {
            var nets = new List<string>();
            var lines = dsn.Split('\n');
            bool inNet = false;
            int depth = 0;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (!inNet)
                {
                    if (line.StartsWith("(network"))
                    {
                        inNet = true;
                        depth = 1;
                    }
                    continue;
                }

                if (line.StartsWith("(net "))
                {
                    var m = Regex.Match(line, @"^\(net\s+([^\s\)]+)");
                    if (m.Success)
                        nets.Add(m.Groups[1].Value);
                }
                depth += line.Count(c => c == '(') - line.Count(c => c == ')');
                if (depth <= 0)
                    break;
            }
            return nets;
        }

        private static long Scale(string value, long factor)
        {
            var d = double.Parse(value, CultureInfo.InvariantCulture);
            return (long)Math.Round(d * factor);
        }

        private record struct Placement(string Reference, long X, long Y, string Layer, int Rotation);
    }
}

