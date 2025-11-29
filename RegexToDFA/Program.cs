// ============================================================================
// Fișier: Program.cs
// Partea3 din cerință: funcția main.
// Scop: Punctul de pornire al aplicației. Citește expresia regulată din fișier,
// apelează conversia RegexToDFA și gestionează meniul pentru interacțiunea cu utilizatorul.
// ============================================================================
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using RegexToDFA.Automata;
using RegexToDFA.Converters;

namespace RegexToDFA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RegexToDFA - Tema1");

            // Folosim fisierul input.txt si output.txt din directorul proiectului
            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string inputPath = Path.Combine(projectRoot, "input.txt");
            string outputPath = Path.Combine(projectRoot, "output.txt");

            Console.WriteLine("Using project input/output files.");
            if (!File.Exists(inputPath))
            {
                // Exemplu de expresie regulata
                File.WriteAllText(inputPath, "aba(aa|bb)*c(ab)*");
                Console.WriteLine("Fisier input.txt creat cu exemplu.");
            }

            string regex;
            try
            {
                regex = File.ReadAllText(inputPath).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la citirea fisierului: {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(regex))
            {
                Console.WriteLine("Expresia regulata din fisier este goala. Iesire.");
                return;
            }

            var parser = new Regex.RegexParser();

            // Extind expresia cu markerul final si concatenari explicite
            string extended = parser.InsertExplicitConcat(regex + "#");

            // Obtine forma postfixata
            string postfix = parser.ToPostFix(extended);

            // Construiește arborele sintactic
            var root = parser.BuildSyntaxTreeFromPostfix(postfix);

            // Construiește AFD folosind converterul (Thompson + subset)
            var converter = new RegexToDfaConverter();
            DeterministicFiniteAutomaton? dfa = null;
            try
            {
                dfa = converter.RegexToDFA(regex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la construirea automatului: {ex.Message}");
                return;
            }

            // Meniu
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Meniu:");
                Console.WriteLine("1 - Afiseaza forma poloneza postfixata");
                Console.WriteLine("2 - Afiseaza arborele sintactic");
                Console.WriteLine("3 - Afiseaza automatul (consola)");
                Console.WriteLine("4 - Salveaza automatul intr-un fisier (output.txt in proiect)");
                Console.WriteLine("5 - Verifica unul sau mai multe cuvinte");
                Console.WriteLine("0 - Iesire");
                Console.Write("Alege o optiune: ");
                string? opt = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(opt)) continue;

                if (opt == "0") break;

                if (opt == "1")
                {
                    Console.WriteLine("Postfix: ");
                    Console.WriteLine(postfix);
                }
                else if (opt == "2")
                {
                    Console.WriteLine("Arbore sintactic:");
                    PrintSyntaxTree(root, Console.Out);
                }
                else if (opt == "3")
                {
                    Console.WriteLine("Automatul determinist rezultat:");
                    dfa.PrintAutomaton();
                }
                else if (opt == "4")
                {
                    try
                    {
                        dfa.SaveToFile(outputPath);
                        Console.WriteLine("Automatul a fost salvat in fisierul output.txt din proiect.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eroare la salvare: {ex.Message}");
                    }
                }
                else if (opt == "5")
                {
                    Console.WriteLine("Introduceti cuvinte (un cuvant pe linie). Lasati gol pentru a termina.");
                    while (true)
                    {
                        Console.Write("cuvant> ");
                        string? w = Console.ReadLine();
                        if (w == null || w == "") break;
                        bool ok = dfa.CheckWord(w);
                        Console.WriteLine(ok ? "ACCEPTAT" : "RESPINS");
                    }
                }
                else
                {
                    Console.WriteLine("Optiune necunoscuta.");
                }
            }

            Console.WriteLine("La revedere.");
        }

        // Afișează arborele sintactic într-o manieră recursivă
        private static void PrintSyntaxTree(Regex.SyntaxTree.SyntaxNode node, TextWriter writer, int indent =0)
        {
            if (node == null) return;

            string pad = new string(' ', indent *2);
            writer.Write(pad);
            writer.WriteLine($"Type: {node.Type}" + (node.Symbol != '\0' ? $", Symbol: '{node.Symbol}'" : "") + $", Pos: {node.Position}");

            // afisam nullable, firstpos, lastpos daca sunt disponibile
            writer.Write(pad);
            writer.WriteLine($"{(node.Nullable ? "Nullable" : "NotNullable")}, FirstPos={{" + string.Join(",", node.FirstPos.OrderBy(x => x)) + "}}, LastPos={{" + string.Join(",", node.LastPos.OrderBy(x => x)) + "}}");

            if (node.Left != null)
            {
                writer.Write(pad);
                writer.WriteLine("Left:");
                PrintSyntaxTree(node.Left, writer, indent +1);
            }
            if (node.Right != null)
            {
                writer.Write(pad);
                writer.WriteLine("Right:");
                PrintSyntaxTree(node.Right, writer, indent +1);
            }
        }
    }
}
