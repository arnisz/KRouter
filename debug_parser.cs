using System;
using System.IO;
using KRouter.Cli;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
var dsnPath = Path.Combine(projectRoot, "samples", "boards", "example.dsn");
var dsn = File.ReadAllText(dsnPath);

Console.WriteLine("=== Testing SExprParser ===");
try 
{
    var root = SExprParser.Parse(dsn);
    Console.WriteLine($"Parsed successfully! Root is atom: {root.IsAtom}, Children count: {root.Children.Count}");
    
    if (!root.IsAtom && root.Children.Count > 0)
    {
        Console.WriteLine($"First child: {root.Children[0].Value}");
        
        // Test placement extraction
        var placementNode = FindNode(root, "placement");
        Console.WriteLine($"Placement node found: {placementNode != null}");
        if (placementNode != null)
        {
            Console.WriteLine($"Placement children count: {placementNode.Children.Count}");
        }
        
        // Test network extraction  
        var networkNode = FindNode(root, "network");
        Console.WriteLine($"Network node found: {networkNode != null}");
        if (networkNode != null)
        {
            Console.WriteLine($"Network children count: {networkNode.Children.Count}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Parser error: {ex.Message}");
}

static SExprNode FindNode(SExprNode node, string atom)
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
