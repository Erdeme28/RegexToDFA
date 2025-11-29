// ============================================================================
// Fișier: SyntaxTree.cs
// Partea 2 din cerință (utilitar pentru RegexToDFA).
// Scop: Definește structura arborelui sintactic pentru expresia regulată.
// Fiecare nod reprezintă un operator sau simbol din expresie.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RegexToDFA.Regex
{
    internal class SyntaxTree
    {
        // Enumeration for node types in the syntax tree
        internal enum NodeType
        {
            Symbol,
            Concat,
            Union,
            Star,
            Plus,
            Question,
            EndMarker
        }

        // Class representing a node in the syntax tree
        internal class SyntaxNode
        {
            public NodeType Type { get; set; }
            public char Symbol { get; set; }
            public SyntaxNode? Left { get; set; }
            public SyntaxNode? Right { get; set; }
            public bool Nullable { get; set; }
            public HashSet<int> FirstPos { get; set; } = new HashSet<int>();
            public HashSet<int> LastPos { get; set; } = new HashSet<int>();
            public int Position { get; set; }
            public SyntaxNode(NodeType type, char symbol = '\0')
            {
                Type = type;
                Symbol = symbol;
            }
        }

        // Traverse preorder helper
        public static void TraversePreorder(SyntaxNode? node, Action<SyntaxNode> action)
        {
            if (node == null) return;
            action(node);
            if (node.Left != null) TraversePreorder(node.Left, action);
            if (node.Right != null) TraversePreorder(node.Right, action);
        }

        // Simple visual printer for the syntax tree using ASCII branches
        internal static class SyntaxTreePrinter
        {
            // Public entry point
            public static void Print(SyntaxNode? root, TextWriter writer)
            {
                if (root == null) return;
                PrintNode(root, writer, "", true);
                writer.Flush();
            }

            private static void PrintNode(SyntaxNode node, TextWriter writer, string indent, bool last)
            {
                // branch prefix
                writer.Write(indent);
                string prefix;
                string newIndent;
                if (last)
                {
                    prefix = "└── ";
                    newIndent = indent + " ";
                }
                else
                {
                    prefix = "├── ";
                    newIndent = indent + "│ ";
                }

                writer.Write(prefix);

                // node label
                string label = NodeLabel(node);
                writer.WriteLine(label);

                // print extra info line (nullable, firstpos, lastpos) with indentation
                string info = $"(Nullable: {node.Nullable}, First: {{{string.Join(",", node.FirstPos.OrderBy(x=>x))}}}, Last: {{{string.Join(",", node.LastPos.OrderBy(x=>x))}}})";
                writer.Write(newIndent);
                writer.WriteLine(info);

                // collect children
                var children = new List<SyntaxNode>();
                if (node.Left != null) children.Add(node.Left);
                if (node.Right != null) children.Add(node.Right);

                for (int i =0; i < children.Count; i++)
                {
                    PrintNode(children[i], writer, newIndent, i == children.Count -1);
                }
            }

            private static string NodeLabel(SyntaxNode node)
            {
                string type = node.Type.ToString();
                if (node.Type == NodeType.Symbol || node.Type == NodeType.EndMarker)
                {
                    char s = node.Symbol;
                    if (s == '\0') return $"{type} (pos={node.Position})";
                    // escape control characters
                    string sym = s == '#' ? "#" : s.ToString();
                    return $"{type} '{sym}' (pos={node.Position})";
                }
                return type;
            }
        }

        // TODO: implementează o clasă statică SyntaxTreePrinter care afișează arborele în format ASCII
    }
}
