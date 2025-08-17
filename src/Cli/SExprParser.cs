using System;
using System.Collections.Generic;
using System.Text;

namespace KRouter.Cli
{
    public class SExprNode
    {
        public bool IsAtom { get; set; }
        public string Value { get; set; }
        public List<SExprNode> Children { get; set; } = new List<SExprNode>();
        public override string ToString() => IsAtom ? Value : $"({string.Join(" ", Children)})";
    }

    public static class SExprParser
    {
        public static SExprNode Parse(string input)
        {
            int pos = 0;
            return ParseNode(input, ref pos);
        }

        private static SExprNode ParseNode(string input, ref int pos)
        {
            SkipWhitespace(input, ref pos);
            if (pos >= input.Length) throw new Exception("Unerwartetes Dateiende");
            if (input[pos] == '(')
            {
                pos++; // '('
                var node = new SExprNode { IsAtom = false };
                while (true)
                {
                    SkipWhitespace(input, ref pos);
                    if (pos >= input.Length) throw new Exception("Fehlende schließende Klammer");
                    if (input[pos] == ')') { pos++; break; }
                    node.Children.Add(ParseNode(input, ref pos));
                }
                return node;
            }
            else
            {
                var sb = new StringBuilder();
                // Handle quoted strings
                if (input[pos] == '"')
                {
                    pos++; // Skip opening quote
                    sb.Append('"');
                    while (pos < input.Length && input[pos] != '"')
                    {
                        if (input[pos] == '\\' && pos + 1 < input.Length)
                        {
                            sb.Append(input[pos]); // backslash
                            pos++;
                            sb.Append(input[pos]); // escaped character
                        }
                        else
                        {
                            sb.Append(input[pos]);
                        }
                        pos++;
                    }
                    if (pos < input.Length && input[pos] == '"')
                    {
                        sb.Append('"');
                        pos++; // Skip closing quote
                    }
                }
                else
                {
                    // Regular atom
                    while (pos < input.Length && !char.IsWhiteSpace(input[pos]) && input[pos] != '(' && input[pos] != ')')
                    {
                        sb.Append(input[pos]);
                        pos++;
                    }
                }
                return new SExprNode { IsAtom = true, Value = sb.ToString() };
            }
        }

        private static void SkipWhitespace(string input, ref int pos)
        {
            while (pos < input.Length && char.IsWhiteSpace(input[pos])) pos++;
        }
    }
}
