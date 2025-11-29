// ============================================================================
// Fișier: RegexToDfaConverter.cs
// Partea2 din cerință: funcția RegexToDFA.
// Scop: Transformă o expresie regulată într-un automat finit determinist (AFD).
// Modificat: folosește construcția Thompson pentru a obține mai întâi un AFN cu
// tranziții lambda (ε) și apoi construcția subset pentru obținerea AFD-ului.
// Păstrează structura și comentariile inițiale.
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

            // --- BUILD NFA (Thompson) -----------------------------------------
            var nfa = BuildNfaFromPostfix(postfix);

            // --- CONVERT NFA (with ε) to DFA (subset construction) ----------
            var dfa = nfa.ToDeterministic();

            return dfa;
        }

        // Build an NFA from postfix expression using Thompson construction
        private Automata.NondeterministicFiniteAutomaton BuildNfaFromPostfix(string postfix)
        {
            var stack = new Stack<Automata.NondeterministicFiniteAutomaton>();
            int counter = 0;

            foreach (char token in postfix)
            {
                if (IsOperator(token))
                {
                    if (token == '|')
                    {
                        var B = stack.Pop();
                        var A = stack.Pop();
                        var C = ThompsonUnion(A, B, ref counter);
                        stack.Push(C);
                    }
                    else if (token == '.')
                    {
                        var B = stack.Pop();
                        var A = stack.Pop();
                        var C = ThompsonConcat(A, B);
                        stack.Push(C);
                    }
                    else if (token == '*')
                    {
                        var A = stack.Pop();
                        var C = ThompsonStar(A, ref counter);
                        stack.Push(C);
                    }
                    else if (token == '+')
                    {
                        var A = stack.Pop();
                        var C = ThompsonPlus(A, ref counter);
                        stack.Push(C);
                    }
                    else if (token == '?')
                    {
                        var A = stack.Pop();
                        var C = ThompsonQuestion(A, ref counter);
                        stack.Push(C);
                    }
                    else
                    {
                        // Unknown operator - ignore
                    }
                }
                else
                {
                    // symbol (including '#' end marker)
                    var A = ThompsonSymbol(token, ref counter);
                    stack.Push(A);
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Postfix expression invalid - NFA stack size !=1");

            return stack.Pop();
        }

        // Helpers: Thompson fragments
        private Automata.NondeterministicFiniteAutomaton ThompsonSymbol(char c, ref int counter)
        {
            var nfa = new Automata.NondeterministicFiniteAutomaton();
            string s = "q" + (counter++);
            string t = "q" + (counter++);
            nfa.AddState(s);
            nfa.AddState(t);
            nfa.q0 = s;
            nfa.F.Add(t);
            nfa.AddSymbol(c);
            nfa.AddTransition(s, c, t);
            return nfa;
        }

        private Automata.NondeterministicFiniteAutomaton ThompsonConcat(Automata.NondeterministicFiniteAutomaton A, Automata.NondeterministicFiniteAutomaton B)
        {
            // Merge B into A, connect A.final -> ε -> B.start (but we will rewire: A.final loses final and merge with B.start)
            var C = Automata.NondeterministicFiniteAutomaton.Merge(A, B);

            // For each final state of A connect epsilon to B.q0
            foreach (var f in A.F)
            {
                C.AddEpsilonTransition(f, B.q0!);
            }

            // New initial is A.q0; final states are B.F
            C.q0 = A.q0;
            C.F.Clear();
            foreach (var f in B.F)
                C.F.Add(f);

            return C;
        }

        private Automata.NondeterministicFiniteAutomaton ThompsonUnion(Automata.NondeterministicFiniteAutomaton A, Automata.NondeterministicFiniteAutomaton B, ref int counter)
        {
            var C = Automata.NondeterministicFiniteAutomaton.Merge(A, B);

            string newStart = "q" + (counter++);
            string newFinal = "q" + (counter++);
            C.AddState(newStart);
            C.AddState(newFinal);

            // ε from newStart to A.q0 and B.q0
            C.AddEpsilonTransition(newStart, A.q0!);
            C.AddEpsilonTransition(newStart, B.q0!);

            // ε from each final of A and B to newFinal
            foreach (var f in A.F)
                C.AddEpsilonTransition(f, newFinal);
            foreach (var f in B.F)
                C.AddEpsilonTransition(f, newFinal);

            C.q0 = newStart;
            C.F.Clear();
            C.F.Add(newFinal);

            return C;
        }

        private Automata.NondeterministicFiniteAutomaton ThompsonStar(Automata.NondeterministicFiniteAutomaton A, ref int counter)
        {
            var C = Automata.NondeterministicFiniteAutomaton.Clone(A);

            string newStart = "q" + (counter++);
            string newFinal = "q" + (counter++);
            C.AddState(newStart);
            C.AddState(newFinal);

            // ε transitions
            C.AddEpsilonTransition(newStart, C.q0!);
            C.AddEpsilonTransition(newStart, newFinal);
            foreach (var f in A.F)
            {
                C.AddEpsilonTransition(f, C.q0!);
                C.AddEpsilonTransition(f, newFinal);
            }

            C.q0 = newStart;
            C.F.Clear();
            C.F.Add(newFinal);

            return C;
        }

        private Automata.NondeterministicFiniteAutomaton ThompsonPlus(Automata.NondeterministicFiniteAutomaton A, ref int counter)
        {
            // A+ = A concat A* (but we implement as: A with ε from finals to start and new start/new final ensuring at least one)
            var C = Automata.NondeterministicFiniteAutomaton.Clone(A);

            string newStart = "q" + (counter++);
            string newFinal = "q" + (counter++);
            C.AddState(newStart);
            C.AddState(newFinal);

            C.AddEpsilonTransition(newStart, C.q0!);
            foreach (var f in A.F)
            {
                C.AddEpsilonTransition(f, C.q0!);
                C.AddEpsilonTransition(f, newFinal);
            }

            C.q0 = newStart;
            C.F.Clear();
            C.F.Add(newFinal);

            return C;
        }

        private Automata.NondeterministicFiniteAutomaton ThompsonQuestion(Automata.NondeterministicFiniteAutomaton A, ref int counter)
        {
            var C = Automata.NondeterministicFiniteAutomaton.Clone(A);

            string newStart = "q" + (counter++);
            string newFinal = "q" + (counter++);
            C.AddState(newStart);
            C.AddState(newFinal);

            // ε from newStart to old start and to newFinal
            C.AddEpsilonTransition(newStart, C.q0!);
            C.AddEpsilonTransition(newStart, newFinal);

            // ε from old finals to newFinal
            foreach (var f in A.F)
                C.AddEpsilonTransition(f, newFinal);

            C.q0 = newStart;
            C.F.Clear();
            C.F.Add(newFinal);

            return C;
        }

        // Simple operator check
        private bool IsOperator(char c)
        {
            return c == '|' || c == '.' || c == '*' || c == '+' || c == '?';
        }
    }
}
