using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KRouter.Cli; // Für SExprParser und SExprNode

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
            var root = SExprParser.Parse(dsnContent);
            var parserOptions = ExtractParserOptions(root);
            var resolution = ExtractResolution(root) ?? ("mm", 100000L);
            var placements = ExtractPlacements(root, resolution.Item2);
            var nets = ExtractNetNames(root);

            var sb = new StringBuilder();
            sb.AppendLine($"(session {designName}.ses");
            sb.AppendLine($"  (base_design {designName}.dsn)");
            sb.AppendLine("  (placement");
            sb.AppendLine($"    (resolution {resolution.Item1} {resolution.Item2})");
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
            sb.AppendLine($"    (resolution {resolution.Item1} {resolution.Item2})");
            sb.AppendLine("    (parser");
            foreach (var opt in parserOptions)
            {
                sb.AppendLine($"      ({opt.Key} {opt.Value})");
            }
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

        // Extrahiere parser-Optionen als Dictionary
        private static Dictionary<string, string> ExtractParserOptions(SExprNode root)
        {
            var result = new Dictionary<string, string>();
            var parserNode = FindNode(root, "parser");
            if (parserNode == null) return result;
            foreach (var child in parserNode.Children.Skip(1))
            {
                if (!child.IsAtom && child.Children.Count >= 2 && child.Children[0].IsAtom)
                {
                    var key = child.Children[0].Value;
                    var value = child.Children[1].IsAtom ? child.Children[1].Value : "";
                    // Entferne Anführungszeichen am Anfang und Ende, falls vorhanden
                    if (value.Length > 1 && value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);
                    result[key] = value;
                }
            }
            return result;
        }

        // Extrahiere (resolution <unit> <factor>) aus dem Baum
        private static (string, long)? ExtractResolution(SExprNode root)
        {
            var resNode = FindNode(root, "resolution");
            if (resNode != null && resNode.Children.Count >= 3 && resNode.Children[1].IsAtom && resNode.Children[2].IsAtom)
            {
                var unit = resNode.Children[1].Value;
                if (long.TryParse(resNode.Children[2].Value, out var factor))
                    return (unit, factor);
            }
            return null;
        }

        // Extrahiere alle Placements aus (placement ...)
        private static Dictionary<string, List<Placement>> ExtractPlacements(SExprNode root, long factor)
        {
            var result = new Dictionary<string, List<Placement>>();
            var placementNode = FindNode(root, "placement");
            if (placementNode == null) return result;
            
            foreach (var compNode in placementNode.Children.Where(n => !n.IsAtom && n.Children.Count > 0 && n.Children[0].IsAtom && n.Children[0].Value == "component"))
            {
                if (compNode.Children.Count < 2) continue;
                
                // Component ID mit Anführungszeichen bereinigen
                var compId = compNode.Children[1].IsAtom ? compNode.Children[1].Value : "?";
                if (compId.StartsWith("\"") && compId.EndsWith("\""))
                    compId = compId.Substring(1, compId.Length - 2);
                    
                if (!result.ContainsKey(compId)) result[compId] = new List<Placement>();
                
                foreach (var placeNode in compNode.Children.Where(n => !n.IsAtom && n.Children.Count > 0 && n.Children[0].IsAtom && n.Children[0].Value == "place"))
                {
                    if (placeNode.Children.Count >= 6)
                    {
                        var reference = placeNode.Children[1].Value;
                        var x = Scale(placeNode.Children[2].Value, factor);
                        var y = Scale(placeNode.Children[3].Value, factor);
                        var layer = placeNode.Children[4].Value;
                        var rot = (int)Math.Round(double.Parse(placeNode.Children[5].Value, CultureInfo.InvariantCulture));
                        result[compId].Add(new Placement(reference, x, y, layer, rot));
                    }
                }
            }
            return result;
        }

        // Extrahiere alle Netznamen aus (network ...)
        private static IEnumerable<string> ExtractNetNames(SExprNode root)
        {
            var nets = new List<string>();
            var networkNode = FindNode(root, "network");
            if (networkNode == null) return nets;
            
            foreach (var netNode in networkNode.Children.Where(n => !n.IsAtom && n.Children.Count > 0 && n.Children[0].IsAtom && n.Children[0].Value == "net"))
            {
                if (netNode.Children.Count >= 2 && netNode.Children[1].IsAtom)
                {
                    var netName = netNode.Children[1].Value;
                    // Entferne Anführungszeichen falls vorhanden
                    if (netName.StartsWith("\"") && netName.EndsWith("\""))
                        netName = netName.Substring(1, netName.Length - 2);
                    nets.Add(netName);
                }
            }
            return nets;
        }

        // Hilfsmethode: Suche nach erstem Kind mit bestimmtem Atomwert (rekursiv im gesamten Baum)
        private static SExprNode? FindNode(SExprNode node, string atom)
        {
            if (!node.IsAtom && node.Children.Count > 0 && node.Children[0].IsAtom && node.Children[0].Value == atom)
                return node;
            foreach (var child in node.Children)
            {
                var found = FindNode(child, atom);
                if (found != null) return found;
            }
            return null;
        }

        private static long Scale(string value, long factor)
        {
            var d = double.Parse(value, CultureInfo.InvariantCulture);
            return (long)Math.Round(d * factor);
        }

        // Extrahiere Boardgrenzen, Layer und Regionen aus (structure ...)
        private static StructureInfo ExtractStructure(SExprNode root)
        {
            var info = new StructureInfo();
            var structureNode = FindNode(root, "structure");
            if (structureNode == null) return info;
            foreach (var child in structureNode.Children.Skip(1))
            {
                if (!child.IsAtom && child.Children.Count > 0 && child.Children[0].IsAtom)
                {
                    var type = child.Children[0].Value;
                    if (type == "boundary" && child.Children.Count > 1 && !child.Children[1].IsAtom)
                    {
                        var rectNode = child.Children[1];
                        if (rectNode.Children.Count >= 6 && rectNode.Children[0].IsAtom && rectNode.Children[0].Value == "rect")
                        {
                            var name = rectNode.Children[1].IsAtom ? rectNode.Children[1].Value : "";
                            var x0 = rectNode.Children[2].IsAtom ? rectNode.Children[2].Value : "0";
                            var y0 = rectNode.Children[3].IsAtom ? rectNode.Children[3].Value : "0";
                            var x1 = rectNode.Children[4].IsAtom ? rectNode.Children[4].Value : "0";
                            var y1 = rectNode.Children[5].IsAtom ? rectNode.Children[5].Value : "0";
                            info.Boundaries.Add((name, x0, y0, x1, y1));
                        }
                    }
                    else if (type == "layer_descriptor")
                    {
                        // (layer_descriptor <name> ...)
                        if (child.Children.Count > 1 && child.Children[1].IsAtom)
                            info.Layers.Add(child.Children[1].Value);
                    }
                    // Weitere Typen wie keepout, region etc. können hier ergänzt werden
                }
            }
            return info;
        }

        private class StructureInfo
        {
            public List<(string Name, string X0, string Y0, string X1, string Y1)> Boundaries { get; } = new();
            public List<string> Layers { get; } = new();
        }

        private record struct Placement(string Reference, long X, long Y, string Layer, int Rotation);
    }
}
