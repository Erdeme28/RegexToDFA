// ============================================================================
// Fișier: RegexToDfaConverter.cs
// Partea 2 din cerință: funcția RegexToDFA.
// Scop: Transformă o expresie regulată într-un automat finit determinist (AFD).
// Folosește metoda arborelui sintactic și calculele followpos (construcția directă).
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexToDFA.Converters
{
    internal class RegexToDfaConverter
    {
        // Method to convert regex to DFA
        public Automata.DeterministicFiniteAutomaton RegexToDFA(string regex)
        {
            var parser = new Regex.RegexParser();

            // Add final symbol #
            string extended = parser.InsertExplicitConcat(regex + "#");

            // Convert to postfix
            string postfix = parser.ToPostFix(extended);

            // Build syntax tree
            var root = parser.BuildSyntaxTreeFromPostfix(postfix);

            // Compute nullable
            ComputeNullable(root);

            // Compute firstpos
            ComputeFirstPos(root);

            // Compute lastpos
            ComputeLastPos(root);

            // Compute followpos
            var followpos = new Dictionary<int, HashSet<int>>();
            ComputeFollowPos(root, followpos);

            // --- BUILD DFA -----------------------------------------

            var dfa = new Automata.DeterministicFiniteAutomaton();

            // Map: position -> symbol
            var positionToSymbol = new Dictionary<int, char>();
            CollectSymbolPositions(root, positionToSymbol);

            // Initial state = firstpos(root)
            var startState = new HashSet<int>(root.FirstPos);
            string startName = StateName(startState);

            dfa.Q.Add(startName);
            dfa.q0 = startName;

            // Worklist
            var unmarked = new Queue<HashSet<int>>();
            unmarked.Enqueue(startState);

            while (unmarked.Count > 0)
            {
                var current = unmarked.Dequeue();
                string currentName = StateName(current);

                // For each symbol in alphabet:
                foreach (char a in positionToSymbol.Values.Distinct())
                {
                    var U = new HashSet<int>();

                    // For each position i in current which has symbol 'a'
                    foreach (var i in current)
                    {
                        if (positionToSymbol[i] == a)
                        {
                            if (followpos.ContainsKey(i))
                                U.UnionWith(followpos[i]);
                        }
                    }

                    if (U.Count == 0)
                        continue;

                    string Uname = StateName(U);

                    if (!dfa.Q.Contains(Uname))
                    {
                        dfa.Q.Add(Uname);
                        unmarked.Enqueue(U);
                    }

                    dfa.AddTransition(currentName, a, Uname);
                }
            }

            // FINAL STATES: any state containing position of #
            int endPos = positionToSymbol.First(p => p.Value == '#').Key;

            foreach (var q in dfa.Q)
            {
                var set = DecodeState(q);
                if (set.Contains(endPos))
                    dfa.F.Add(q);
            }

            return dfa;
        }

        // Auxiliary methods for the syntax tree
        private void ComputeNullable(Regex.SyntaxTree.SyntaxNode node)
        {
            if (node == null)
                return;

            if (node.Type == Regex.SyntaxTree.NodeType.Symbol ||
                node.Type == Regex.SyntaxTree.NodeType.EndMarker)
            {
                node.Nullable = false;
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Star ||
                node.Type == Regex.SyntaxTree.NodeType.Question)
            {
                node.Nullable = true;
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Plus)
            {
                ComputeNullable(node.Left);
                node.Nullable = node.Left.Nullable;
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Union)
            {
                ComputeNullable(node.Left);
                ComputeNullable(node.Right);
                node.Nullable = node.Left.Nullable || node.Right.Nullable;
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Concat)
            {
                ComputeNullable(node.Left);
                ComputeNullable(node.Right);
                node.Nullable = node.Left.Nullable && node.Right.Nullable;
                return;
            }
        }

        private void ComputeFirstPos(Regex.SyntaxTree.SyntaxNode node)
        {
            if (node == null)
                return;

            if (node.Type == Regex.SyntaxTree.NodeType.Symbol ||
                node.Type == Regex.SyntaxTree.NodeType.EndMarker)
            {
                node.FirstPos.Add(node.Position);
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Star ||
                node.Type == Regex.SyntaxTree.NodeType.Plus ||
                node.Type == Regex.SyntaxTree.NodeType.Question)
            {
                ComputeFirstPos(node.Left);
                node.FirstPos = new HashSet<int>(node.Left.FirstPos);
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Union)
            {
                ComputeFirstPos(node.Left);
                ComputeFirstPos(node.Right);
                node.FirstPos = node.Left.FirstPos.Union(node.Right.FirstPos).ToHashSet();
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Concat)
            {
                ComputeFirstPos(node.Left);
                ComputeFirstPos(node.Right);

                if (node.Left.Nullable)
                    node.FirstPos = node.Left.FirstPos.Union(node.Right.FirstPos).ToHashSet();
                else
                    node.FirstPos = new HashSet<int>(node.Left.FirstPos);

                return;
            }
        }

        private void ComputeLastPos(Regex.SyntaxTree.SyntaxNode node)
        {
            if (node == null)
                return;

            if (node.Type == Regex.SyntaxTree.NodeType.Symbol ||
                node.Type == Regex.SyntaxTree.NodeType.EndMarker)
            {
                node.LastPos.Add(node.Position);
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Star ||
                node.Type == Regex.SyntaxTree.NodeType.Plus ||
                node.Type == Regex.SyntaxTree.NodeType.Question)
            {
                ComputeLastPos(node.Left);
                node.LastPos = new HashSet<int>(node.Left.LastPos);
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Union)
            {
                ComputeLastPos(node.Left);
                ComputeLastPos(node.Right);
                node.LastPos = node.Left.LastPos.Union(node.Right.LastPos).ToHashSet();
                return;
            }

            if (node.Type == Regex.SyntaxTree.NodeType.Concat)
            {
                ComputeLastPos(node.Left);
                ComputeLastPos(node.Right);

                if (node.Right.Nullable)
                    node.LastPos = node.Left.LastPos.Union(node.Right.LastPos).ToHashSet();
                else
                    node.LastPos = new HashSet<int>(node.Right.LastPos);

                return;
            }
        }

        private void ComputeFollowPos(
            Regex.SyntaxTree.SyntaxNode node,
            Dictionary<int, HashSet<int>> followpos)
        {
            if (node == null)
                return;

            ComputeFollowPos(node.Left, followpos);
            ComputeFollowPos(node.Right, followpos);

            // Concat rule
            if (node.Type == Regex.SyntaxTree.NodeType.Concat)
            {
                foreach (var i in node.Left.LastPos)
                {
                    if (!followpos.ContainsKey(i))
                        followpos[i] = new HashSet<int>();

                    followpos[i].UnionWith(node.Right.FirstPos);
                }
            }

            // Star or Plus rule
            if (node.Type == Regex.SyntaxTree.NodeType.Star ||
                node.Type == Regex.SyntaxTree.NodeType.Plus)
            {
                foreach (var i in node.Left.LastPos)
                {
                    if (!followpos.ContainsKey(i))
                        followpos[i] = new HashSet<int>();

                    followpos[i].UnionWith(node.Left.FirstPos);
                }
            }
        }

        // Helper: extract all positions with their symbols
        private void CollectSymbolPositions(
            Regex.SyntaxTree.SyntaxNode node,
            Dictionary<int, char> map)
        {
            if (node == null)
                return;

            if (node.Type == Regex.SyntaxTree.NodeType.Symbol ||
                node.Type == Regex.SyntaxTree.NodeType.EndMarker)
            {
                map[node.Position] = node.Symbol;
            }

            CollectSymbolPositions(node.Left, map);
            CollectSymbolPositions(node.Right, map);
        }

        // Convert a set of positions to a DFA state name
        private string StateName(HashSet<int> set)
            => "{" + string.Join(",", set.OrderBy(x => x)) + "}";

        // Parse a state name back into a set of positions
        private HashSet<int> DecodeState(string name)
        {
            name = name.Trim('{', '}');
            if (name == "")
                return new HashSet<int>();

            return name.Split(',')
                       .Select(int.Parse)
                       .ToHashSet();
        }
    }
}
