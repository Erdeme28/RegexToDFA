using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RegexToDFA.Automata
{
    internal class DeterministicFiniteAutomaton
    {
        public HashSet<string> Q { get; } = new HashSet<string>();
        public HashSet<char> Sigma { get; } = new HashSet<char>();
        public Dictionary<string, Dictionary<char, string>> Delta { get; } = new Dictionary<string, Dictionary<char, string>>();
        public string? q0 { get; set; }
        public HashSet<string> F { get; } = new HashSet<string>();

        public void AddState(string state, bool isFinal = false)
        {
            if (!Q.Contains(state))
                Q.Add(state);

            if (!Delta.ContainsKey(state))
                Delta[state] = new Dictionary<char, string>();

            if (isFinal)
                F.Add(state);
        }

        public void AddSymbol(char c)
        {
            if (!Sigma.Contains(c))
                Sigma.Add(c);
        }

        public void AddTransition(string from, char symbol, string to)
        {
            AddState(from);
            AddState(to);
            AddSymbol(symbol);

            if (!Delta[from].ContainsKey(symbol))
                Delta[from][symbol] = to;
            else if (Delta[from][symbol] != to)
                throw new InvalidOperationException(
                    $"Non-deterministic transition detected: ({from}, {symbol}) -> {Delta[from][symbol]} / {to}");
        }

        public bool VerifyAutomaton(out List<string> errors)
        {
            errors = new List<string>();

            if (Q.Count == 0)
                errors.Add("Set of states Q is empty.");

            if (Sigma.Count == 0)
                errors.Add("Alphabet Sigma is empty.");

            if (q0 == null || !Q.Contains(q0))
                errors.Add("Initial state q0 is not defined or not in Q.");

            if (!F.IsSubsetOf(Q))
                errors.Add("Set of final states F is not a subset of Q.");

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
                        errors.Add($"Transition missing for state '{state}' on symbol '{symbol}'.");
                    else if (!Q.Contains(Delta[state][symbol]))
                        errors.Add($"Transition from '{state}' on symbol '{symbol}' leads to undefined state '{Delta[state][symbol]}'.");
                }
            }

            return errors.Count == 0;
        }


        public bool CheckWord(string word)
        {
            if (q0 == null)
                throw new InvalidOperationException("Automaton is not well-defined: initial state q0 is not set.");

            if (!VerifyAutomaton(out var errors))
            {
                Console.WriteLine("Automaton is not well-defined:");
                foreach (var error in errors)
                    Console.WriteLine($"- {error}");
                return false;
            }

            string currentState = q0;

            foreach (char symbol in word)
            {
                if (!Sigma.Contains(symbol))
                    return false;

                if (Delta[currentState].TryGetValue(symbol, out var nextState))
                    currentState = nextState;
                else
                    return false;
            }

            return F.Contains(currentState);
        }


        public void SaveToFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Q (States):");
                writer.WriteLine("  { " + string.Join(", ", Q.OrderBy(s => s)) + " }\n");

                writer.WriteLine("Σ (Alphabet):");
                writer.WriteLine("  { " + string.Join(", ", Sigma.OrderBy(c => c)) + " }\n");

                writer.WriteLine("Initial State (q0):");
                writer.WriteLine("  " + (q0 ?? "UNDEFINED") + "\n");

                writer.WriteLine("F (Final States):");
                writer.WriteLine("  { " + string.Join(", ", F.OrderBy(s => s)) + " }\n");

                writer.WriteLine("Transition Function δ(q, a):");
                writer.WriteLine("-------------------------------------");

                var symbols = Sigma.OrderBy(s => s).ToList();
                var states = Q.OrderBy(s => s).ToList();

                writer.Write("State".PadRight(12));
                foreach (var symbol in symbols)
                    writer.Write(symbol.ToString().PadRight(8));
                writer.WriteLine();

                writer.WriteLine(new string('-', 12 + symbols.Count * 8));

                foreach (var state in states)
                {
                    writer.Write(state.PadRight(12));
                    foreach (var symbol in symbols)
                    {
                        string nextState = Delta[state].TryGetValue(symbol, out var ns) ? ns : "-";
                        writer.Write(nextState.PadRight(8));
                    }
                    writer.WriteLine();
                }

                writer.WriteLine("\nLegend:");
                writer.WriteLine("  Row = current state");
                writer.WriteLine("  Column = symbol");
                writer.WriteLine("  Entry δ(q, a) = next state\n");
            }

            Console.WriteLine($"Automaton saved to file: {filePath}");
        }
    }
}
