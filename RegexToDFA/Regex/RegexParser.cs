// ============================================================================
// Fișier: RegexParser.cs
// Partea 2 din cerință (pregătirea pentru RegexToDFA).
// Scop: Parsează expresia regulată - adaugă concatenări explicite,
// o transformă în forma postfixată (notare poloneză) și construiește arborele sintactic.
// ============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexToDFA.Regex
{
    internal class RegexParser
    {
        // Method to insert explicit concatenation operators in the regex
        public string InsertExplicitConcat(string regex)
        {
            var result = new StringBuilder();

            bool IsSymbol(char c) => char.IsLetterOrDigit(c);

            for (int i = 0; i < regex.Length; i++)
            {
                char current = regex[i];
                result.Append(current);

                if (i + 1 < regex.Length)
                {
                    char next = regex[i + 1];
                    bool currentCanEndToken =
                        IsSymbol(current) ||
                        current == ')' ||
                        current == '*' ||
                        current == '+' ||
                        current == '?';

                    bool nextCanStartToken =
                        IsSymbol(next) ||
                        next == '(';

                    if (currentCanEndToken && nextCanStartToken)
                        result.Append('.');
                }
            }
            return result.ToString();
        }

        // Method to convert infix regex to postfix notation using Shunting Yard algorithm
        public string ToPostFix(string regex)
        {
            var output = new StringBuilder();
            var operators = new Stack<char>();

            Dictionary<char, int> precedence = new Dictionary<char, int>
            {
                { '*', 3 },
                { '+', 3 },
                { '?', 3 },
                { '.', 2 },
                { '|', 1 }
            };

            bool IsOperator(char c) => precedence.ContainsKey(c);

            foreach (char c in regex)
            {
                if (char.IsLetterOrDigit(c))
                    output.Append(c);

                else if (c == '(')
                    operators.Push(c);

                else if (c == ')')
                {
                    while (operators.Count > 0 && operators.Peek() != '(')
                        output.Append(operators.Pop());
                    if (operators.Count > 0)
                        operators.Pop();
                }

                else if (IsOperator(c))
                {
                    bool rightAssociative = (c == '*' || c == '+' || c == '?');

                    while (operators.Count > 0 &&
                            IsOperator(operators.Peek()) &&
                            (
                                (!rightAssociative &&
                                precedence[operators.Peek()] >= precedence[c])
                                ||
                                (rightAssociative &&
                                precedence[operators.Peek()] > precedence[c])
                         ))
                        output.Append(operators.Pop());

                    operators.Push(c);
                }
            }

            while (operators.Count > 0)
                output.Append(operators.Pop());

            return output.ToString();
        }

        // Method to build the syntax tree from the postfix regex
        public SyntaxTree.SyntaxNode BuildSyntaxTreeFromPostfix(string postfix)
        {
            var stack = new Stack<SyntaxTree.SyntaxNode>();
            int position = 1;

            foreach (char c in postfix)
            {
                switch (c)
                {
                    case '*':
                        {
                            var child = stack.Pop();
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Star)
                            {
                                Left = child
                            };
                            stack.Push(node);
                            break;
                        }

                    case '+':
                        {
                            var child = stack.Pop();
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Plus)
                            {
                                Left = child
                            };
                            stack.Push(node);
                            break;
                        }

                    case '?':
                        {
                            var child = stack.Pop();
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Question)
                            {
                                Left = child
                            };
                            stack.Push(node);
                            break;
                        }

                    case '.':
                        {
                            var right = stack.Pop();
                            var left = stack.Pop();
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Concat)
                            {
                                Left = left,
                                Right = right
                            };
                            stack.Push(node);
                            break;
                        }

                    case '|':
                        {
                            var right = stack.Pop();
                            var left = stack.Pop();
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Union)
                            {
                                Left = left,
                                Right = right
                            };
                            stack.Push(node);
                            break;
                        }

                    default:
                        {
                            var node = new SyntaxTree.SyntaxNode(SyntaxTree.NodeType.Symbol, c)
                            {
                                Position = position++
                            };
                            stack.Push(node);
                            break;
                        }
                }
            }
            return stack.Pop();
        }

        // Method to check if a regex is valid before processing
        public bool IsValidRegex(string regex, out string error)
        {
            error = "";

            if (string.IsNullOrEmpty(regex))
            {
                error = "Regex is empty.";
                return false;
            }

            //Check for invalid characters
            foreach (char c in regex)
            {
                if (!(char.IsLetterOrDigit(c) ||
              c == '(' || c == ')' ||
              c == '*' || c == '+' || c == '?' ||
              c == '|'))
                {
                    error = $"Invalid character in regex: '{c}'";
                    return false;
                }
            }

            // Check for parantheses balance
            int balance = 0;
            foreach (char c in regex)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0)
                {
                    error = "Closing parenthesis without matching opening parenthesis.";
                    return false;
                }
            }

            if (balance != 0)
            {
                error = "Mismatched parentheses.";
                return false;
            }

            // Check illegal operator placement
            for (int i = 0; i < regex.Length; i++)
            {
                char c = regex[i];

                // Unary operators cannot be at the start
                if ((c == '*' || c == '+' || c == '?') && i == 0)
                {
                    error = $"Operator '{c}' cannot appear at the beginning.";
                    return false;
                }

                // Binary operators cannot appear at start or end
                if ((c == '|') && (i == 0 || i == regex.Length - 1))
                {
                    error = $"Operator '{c}' cannot be first or last.";
                    return false;
                }

                // Two binary operators in a row -> invalid: || or |)
                if (i > 0)
                {
                    char prev = regex[i - 1];
                    if ((c == '|' && prev == '|'))
                    {
                        error = "Two '|' operators in a row are not allowed.";
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
