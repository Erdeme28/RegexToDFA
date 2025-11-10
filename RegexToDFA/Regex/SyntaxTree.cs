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
        // TODO: definește o enumerare (enum) NodeType pentru tipurile de noduri din arbore (Symbol, Concat, Union, Star, Plus, Question, EndMarker)
        // TODO: definește o clasă SyntaxNode care să conțină:
        //       - tipul nodului (NodeType)
        //       - simbolul caracterului (dacă este nod de tip Symbol)
        //       - referințe către nodurile copil (Left, Right)
        //       - atribute precum Nullable, FirstPos, LastPos, Position (pentru metoda followpos)
        // TODO: implementează constructori și metode utile (ex: TraversePreorder)
        // TODO: implementează o clasă statică SyntaxTreePrinter care afișează arborele în format ASCII
    }
}
