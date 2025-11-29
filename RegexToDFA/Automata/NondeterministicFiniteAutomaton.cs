// ============================================================================
// Fi?ier: NondeterministicFiniteAutomaton.cs
// Scop: Reprezint? un AFN care poate con?ine tranzi?ii epsilon (lambda).
// Este folosit pentru construc?ia Thompson ?i apoi conversia la AFD.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegexToDFA.Automata
{
 internal class NondeterministicFiniteAutomaton
 {
 public HashSet<string> Q { get; } = new HashSet<string>();
 public HashSet<char> Sigma { get; } = new HashSet<char>();
 // transitions: from -> (symbol -> set of to)
 // we use '\0' char to represent epsilon (?)
 public Dictionary<string, Dictionary<char, HashSet<string>>> Delta { get; } = new Dictionary<string, Dictionary<char, HashSet<string>>>();
 public string? q0 { get; set; }
 public HashSet<string> F { get; } = new HashSet<string>();

 public void AddState(string state)
 {
 if (!Q.Contains(state))
 Q.Add(state);
 if (!Delta.ContainsKey(state))
 Delta[state] = new Dictionary<char, HashSet<string>>();
 }

 public void AddSymbol(char c)
 {
 if (c == '\0')
 return; // epsilon not part of alphabet
 if (!Sigma.Contains(c))
 Sigma.Add(c);
 }

 public void AddTransition(string from, char symbol, string to)
 {
 AddState(from);
 AddState(to);
 if (symbol != '\0')
 AddSymbol(symbol);
 if (!Delta[from].ContainsKey(symbol))
 Delta[from][symbol] = new HashSet<string>();
 Delta[from][symbol].Add(to);
 }

 public void AddEpsilonTransition(string from, string to)
 {
 AddTransition(from, '\0', to);
 }

 // Merge two NFAs (disjoint states assumed by naming scheme)
 public static NondeterministicFiniteAutomaton Merge(NondeterministicFiniteAutomaton A, NondeterministicFiniteAutomaton B)
 {
 var C = new NondeterministicFiniteAutomaton();
 foreach (var s in A.Q)
 {
 C.AddState(s);
 if (A.Delta.ContainsKey(s))
 {
 foreach (var kv in A.Delta[s])
 {
 foreach (var t in kv.Value)
 C.AddTransition(s, kv.Key, t);
 }
 }
 }
 foreach (var s in B.Q)
 {
 C.AddState(s);
 if (B.Delta.ContainsKey(s))
 {
 foreach (var kv in B.Delta[s])
 {
 foreach (var t in kv.Value)
 C.AddTransition(s, kv.Key, t);
 }
 }
 }
 // alphabet
 foreach (var c in A.Sigma) C.AddSymbol(c);
 foreach (var c in B.Sigma) C.AddSymbol(c);

 return C;
 }

 // Clone NFA
 public static NondeterministicFiniteAutomaton Clone(NondeterministicFiniteAutomaton A)
 {
 var C = new NondeterministicFiniteAutomaton();
 foreach (var s in A.Q)
 {
 C.AddState(s);
 if (A.Delta.ContainsKey(s))
 {
 foreach (var kv in A.Delta[s])
 {
 foreach (var t in kv.Value)
 C.AddTransition(s, kv.Key, t);
 }
 }
 }
 foreach (var c in A.Sigma) C.AddSymbol(c);
 C.q0 = A.q0;
 foreach (var f in A.F) C.F.Add(f);
 return C;
 }

 // Convert to DFA using subset construction (epsilon-closure aware)
 public DeterministicFiniteAutomaton ToDeterministic()
 {
 var dfa = new DeterministicFiniteAutomaton();

 // alphabet
 foreach (var c in Sigma)
 dfa.AddSymbol(c);

 // helper: epsilon-closure
 HashSet<string> EpsilonClosure(HashSet<string> states)
 {
 var stack = new Stack<string>(states);
 var closure = new HashSet<string>(states);
 while (stack.Count >0)
 {
 var s = stack.Pop();
 if (Delta.ContainsKey(s) && Delta[s].ContainsKey('\0'))
 {
 foreach (var t in Delta[s]['\0'])
 {
 if (!closure.Contains(t))
 {
 closure.Add(t);
 stack.Push(t);
 }
 }
 }
 }
 return closure;
 }

 // initial DFA state = epsilon-closure({q0})
 var startSet = EpsilonClosure(new HashSet<string> { q0! });
 string startName = StateName(startSet);
 dfa.AddState(startName, false);
 dfa.q0 = startName;

 var unmarked = new Queue<HashSet<string>>();
 var setToName = new Dictionary<string, HashSet<string>>();
 unmarked.Enqueue(startSet);
 setToName[startName] = startSet;

 while (unmarked.Count >0)
 {
 var current = unmarked.Dequeue();
 string currentName = StateName(current);

 foreach (var a in Sigma)
 {
 var move = new HashSet<string>();
 foreach (var s in current)
 {
 if (Delta.ContainsKey(s) && Delta[s].ContainsKey(a))
 move.UnionWith(Delta[s][a]);
 }

 if (move.Count ==0) continue;

 var target = EpsilonClosure(move);
 string tname = StateName(target);
 if (!dfa.Q.Contains(tname))
 {
 dfa.AddState(tname);
 unmarked.Enqueue(target);
 }
 dfa.AddTransition(currentName, a, tname);
 }
 }

 // finals: any DFA state containing an NFA final
 foreach (var q in dfa.Q)
 {
 var set = DecodeState(q);
 if (set.Overlaps(this.F))
 dfa.F.Add(q);
 }

 // Make DFA total by adding a sink state for missing transitions
 // This ensures VerifyAutomaton passes (delta is total over Sigma)
 var sigmaList = dfa.Sigma.OrderBy(c => c).ToList();
 string sink = "{DEAD}";
 if (!dfa.Q.Contains(sink))
 {
 dfa.AddState(sink);
 // sink loops to itself on all symbols
 foreach (var a in sigmaList)
 {
 dfa.AddTransition(sink, a, sink);
 }
 }

 foreach (var state in dfa.Q.ToList())
 {
 foreach (var a in sigmaList)
 {
 if (!dfa.Delta[state].ContainsKey(a))
 {
 dfa.AddTransition(state, a, sink);
 }
 }
 }

 return dfa;
 }

 // Helpers to encode/decode sets as names (reuse same format as DFA)
 private static string StateName(HashSet<string> set)
 {
 return "{" + string.Join(",", set.OrderBy(s => s)) + "}";
 }

 private static HashSet<string> DecodeState(string name)
 {
 name = name.Trim('{', '}');
 if (name == "") return new HashSet<string>();
 return name.Split(',').ToHashSet();
 }
 }
}
