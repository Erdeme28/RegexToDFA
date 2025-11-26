// ============================================================================
// Fișier: DeterministicFiniteAutomaton.cs
// Partea 1 din cerință: definirea clasei DeterministicFiniteAutomaton.
// Scop: Reprezintă automatul finit determinist (AFD) construit dintr-o expresie regulată.
// Conține membrii: Q, Σ, δ, q0 și F.
// Include metodele cerute: VerifyAutomaton, PrintAutomaton, CheckWord.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexToDFA.Automata
{
    internal class DeterministicFiniteAutomaton
    {
        public HashSet<string> Q { get; } = new HashSet<string>();
        public HashSet<char> Sigma { get; } = new HashSet<char>();
        public Dictionary<string, Dictionary<char, string>> Delta { get; } = new Dictionary<string, Dictionary<char, string>>();
        public string? q0 { get; set; }
        public HashSet<string> F { get; } = new HashSet<string>();

        // Verifys if the automaton is well-defined
        public bool VerifyAutomaton(out List<string> errors)
        {
            errors = new List<string>();

            // Q & Sigma are not empty
            if (Q.Count == 0)
            {
                errors.Add("Set of states Q is empty.");
            }

            if (Sigma.Count == 0)
            {
                errors.Add("Alphabet Σ is empty.");
            }

            // q0 is in Q
            if (q0 == null || !Q.Contains(q0))
            {
                errors.Add("Initial state q0 is not defined or not in Q.");
            }

            // F is subset of Q
            if (!F.IsSubsetOf(Q))
            {
                errors.Add("Set of final states F is not a subset of Q.");
            }

            // δ is a total function from Q x Σ to Q
            foreach (var state in Q)
            {
                if (!Delta.ContainsKey(state))
                {
                    errors.Add($"No transitions defined for state '{state}'.");
                    continue;
                }
                foreach (var symbol in Sigma)
                {
                    if (!Delta[state].ContainsKey(symbol))
                    {
                        errors.Add($"Transition missing for state '{state}' on symbol '{symbol}'.");
                    }
                    else
                    {
                        var nextState = Delta[state][symbol];
                        if (!Q.Contains(nextState))
                        {
                            errors.Add($"Transition from state '{state}' on symbol '{symbol}' leads to undefined state '{nextState}'.");
                        }
                    }
                }
            }

            // Return true if no errors found
            return errors.Count == 0;
        }

        // Prints the automaton in a readable table format
        public void PrintAutomaton()
        {
            if(!VerifyAutomaton(out var errors))
            {
                Console.WriteLine("Automaton is not well-defined:");
                foreach(var error in errors)
                {
                    Console.WriteLine($"- {error}");
                }
                return;
            }

            var symbols = Sigma.OrderBy(s => s).ToList();
            var states = Q.OrderBy(s => s).ToList();

            // Column width for states
            int stateColWidth = Math.Max(states.Max(s => s.Length)+ 3, 10); // +3 for padding and 10 for header

            // Header table 
            Console.Write("State".PadRight(stateColWidth));
            foreach (var symbol in symbols)
            {
                Console.Write(symbol.ToString().PadRight(6));
            }
            Console.WriteLine();

            // Line separator
            Console.WriteLine(new string('-', stateColWidth + symbols.Count * 6));

            // Rows
            foreach (var state in states)
            {
                //Mark special states
                string mark = "";

                if (state == q0) mark += "->";
                else mark += "  ";

                if (F.Contains(state)) mark += "*";
                else mark += " ";

                // State column
                Console.Write((mark + state).PadRight(stateColWidth));

                // Symbol transitions
                foreach (var symbol in symbols)
                {
                    string nextState = Delta[state].TryGetValue(symbol, out var ns) ? ns : "-";
                    Console.Write(nextState.PadRight(6));
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n Legend: -> inital, * final");
        }

        // TODO: implementează metoda CheckWord() pentru verificarea unui cuvânt
        // TODO: implementează metoda SaveToFile() pentru salvarea automatului într-un fișier text
    }
}
