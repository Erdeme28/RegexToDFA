using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegexToDFA.Automata;

namespace RegexToDFA.Utils
{
    internal static class TablePrinter
    {
        public static void PrintAutomaton(DeterministicFiniteAutomaton dfa)
            => PrintAutomaton(dfa, Console.Out);

        public static void PrintAutomaton(DeterministicFiniteAutomaton dfa, TextWriter writer)
        {
            if (!dfa.VerifyAutomaton(out var errors))
            {
                writer.WriteLine("Automaton is not well-defined:");
                foreach (var error in errors)
                    writer.WriteLine($"- {error}");
                return;
            }

            // Header: Q, Sigma, q0, F
            writer.WriteLine("Q (States):");
            writer.WriteLine(" { " + string.Join(", ", dfa.Q.OrderBy(s => s)) + " }\n");

            writer.WriteLine("Σ (Alphabet):");
            writer.WriteLine(" { " + string.Join(", ", dfa.Sigma.OrderBy(c => c)) + " }\n");

            writer.WriteLine("Initial State (q0):");
            writer.WriteLine(" " + (dfa.q0 ?? "UNDEFINED") + "\n");

            writer.WriteLine("F (Final States):");
            writer.WriteLine(" { " + string.Join(", ", dfa.F.OrderBy(s => s)) + " }\n");

            var symbols = dfa.Sigma.OrderBy(s => s).ToList();
            var states = dfa.Q.OrderBy(s => s).ToList();

            bool isConsole = object.ReferenceEquals(writer, Console.Out);
            int consoleWidth = 0;
            if (isConsole)
            {
                try { consoleWidth = Console.WindowWidth; } catch { consoleWidth = 0; }
            }

            if (isConsole && consoleWidth > 0 && consoleWidth < 100)
            {
                foreach (var state in states)
                {
                    string mark = (state == dfa.q0 ? "->" : " ") + (dfa.F.Contains(state) ? "*" : " ");
                    writer.WriteLine(mark + state);
                    foreach (var symbol in symbols)
                    {
                        string next = dfa.Delta[state].TryGetValue(symbol, out var ns) ? ns : "-";
                        writer.WriteLine($" {symbol} -> {next}");
                    }
                    writer.WriteLine(new string('-', Math.Min(consoleWidth, 60)));
                }
                writer.Flush();
                return;
            }

            int maxStateName = states.Max(s => s.Length);
            int stateColWidth = Math.Max(maxStateName + 4, 12);
            int symbolColWidth = Math.Max(8, Math.Min(24, Math.Max(8, maxStateName / 2 + 6)));

            if (isConsole && consoleWidth > 0)
            {
                int separators = (symbols.Count) * 3; // ' | '
                int minTotal = stateColWidth + separators + symbols.Count * 6;
                if (minTotal > consoleWidth)
                {
                    int spare = Math.Max(0, consoleWidth - (separators + symbols.Count * 6));
                    stateColWidth = Math.Max(8, spare);
                }
                int remaining = Math.Max(0, consoleWidth - stateColWidth - separators);
                if (symbols.Count > 0)
                {
                    symbolColWidth = Math.Max(6, remaining / symbols.Count - 1);
                }
            }

            writer.Write("State".PadRight(stateColWidth));
            foreach (var symbol in symbols)
            {
                writer.Write(" | ");
                writer.Write(symbol.ToString().PadRight(symbolColWidth));
            }
            writer.WriteLine();

            writer.Write(new string('-', stateColWidth));
            foreach (var symbol in symbols)
            {
                writer.Write("-+-");
                writer.Write(new string('-', symbolColWidth));
            }
            writer.WriteLine();

            string Truncate(string s, int width)
            {
                if (s.Length <= width) return s;
                if (width <= 3) return s.Substring(0, width);
                return s.Substring(0, width - 3) + "...";
            }

            foreach (var state in states)
            {
                string mark = (state == dfa.q0 ? "->" : " ") + (dfa.F.Contains(state) ? "*" : " ");
                string displayState = mark + Truncate(state, Math.Max(0, stateColWidth - mark.Length));
                writer.Write(displayState.PadRight(stateColWidth));
                foreach (var symbol in symbols)
                {
                    writer.Write(" | ");
                    string nextState = dfa.Delta[state].TryGetValue(symbol, out var ns) ? ns : "-";
                    writer.Write(Truncate(nextState, symbolColWidth).PadRight(symbolColWidth));
                }
                writer.WriteLine();
            }

            writer.WriteLine();
            writer.WriteLine("Legend:");
            writer.WriteLine(" Row = current state");
            writer.WriteLine(" Column = symbol");
            writer.WriteLine(" Entry δ(q, a) = next state\n");

            writer.Flush();
        }

        public static List<string> ReadWords()
        {
            var words = new List<string>();
            while (true)
            {
                string? line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                words.Add(line);
            }
            return words;
        }

    }
}
