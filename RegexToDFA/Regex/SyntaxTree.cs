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
        // TODO: implementează constructori și metode utile (ex: TraversePreorder)
        // TODO: implementează o clasă statică SyntaxTreePrinter care afișează arborele în format ASCII
    }
}
