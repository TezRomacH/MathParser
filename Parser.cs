using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Parser_C_Sharp
{
    public struct AstData
    {
        public Stack<TreeNode> TreeNodeStack;

        public LinkedList<Token> Tokens;
    }

    public struct ParserData
    {
        /// <summary>
        /// ������� (�����������) ������
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// ���� ����������� �������
        /// </summary>
        public Stack<Token> TokenStack;

        /// <summary>
        /// �������������� ������ ������� � ����������� �����
        /// </summary>
        public LinkedList<Token> Postfix;

        /// <summary>
        /// �����������, �������� ��������� ��������
        /// </summary>
        public Dictionary<string, int> PrecedenceMap;
    }

    public class Parser
    {
        /// <summary>
        ///  ����������� ����� ��������� � ������
        /// </summary>
        /// <typeparam name="T">���������</typeparam>
        /// <param name="col"></param>
        /// <returns></returns>
        private static string to_string<T>(IEnumerable<T> col)
            => col.Aggregate("", (current, x) => current + x.ToString() + " ");

        /// <summary>
        /// �������� ����� ������� ��������, ��� ������������ �������
        /// </summary>
        /// <param name="formula">�������������� �������</param>
        /// <returns></returns>
        private static string ReplaceUnOperators(string formula)
        {
            int ind = 0;
            string result = formula;

            while (ind < result.Length)
            {
                if (IsOperator(result[ind]))
                {
                    var sub = result.Substring(0, ind).TrimEnd();

                    if (sub == "" || sub[sub.Length - 1] == '(' || IsOperator(sub[sub.Length - 1]))
                        result = result.Insert(ind++, "0");
                }
                ++ind;
            }

            return result;
        }

        private ParserData _data;
        private int _currentPos;
        private AstData _ast;

        /// <summary>
        /// ���������, ����� �� ���������� ���������� ���� ������
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsOkSymbol(char c) => (
            IsOperator(c) ||
            c == '(' || c == ')' ||
            c == ' ' || c == '.' || c == ',' ||
            IsDigit(c));

        private static bool IsOperator(char c) => 
            (c == '+' || c == '-' || c == '*' || c == '/' || c == '^');

        /// <summary>
        /// ���������, �������� �� ������ ������
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsDigit(char c) => (
            c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || 
            c == '5' || c == '6' || c == '7' || c == '8' || c == '9');

        /// <summary>
        /// ������ �����
        /// </summary>
        /// <returns>����� �����</returns>
        private Token ParseNumber()
        {
            int spacePos = _currentPos + 1;

            while (spacePos < _data.Input.Length)
            {
                if (IsOkSymbol(_data.Input[spacePos]))
                {
                    if (IsDigit(_data.Input[spacePos]) || _data.Input[spacePos] == ',' || _data.Input[spacePos] == '.')
                        ++spacePos;
                    else
                        break;
                }
                else
                    throw new Exception("������! ����������� ������.");
            }

            string x = _data.Input.Substring(_currentPos, spacePos - _currentPos);

            _currentPos = spacePos;

            double n = double.Parse(x, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);

            return new Number(n);
        }

        /// <summary>
        /// ������ ��������
        /// </summary>
        /// <returns>����� ���������</returns>
        private Token ParseOperator()
        {
            return new Op(_data.Input.Substring(_currentPos++, 1));
        }

        /// <summary>
        /// �������� ��������� ����� �� ������
        /// </summary>
        /// <returns></returns>
        private Token NextToken()
        {
            Token result;
            if (_currentPos > _data.Input.Length)
                return null;

            while (_data.Input[_currentPos] == ' ')
                ++_currentPos;

            switch (_data.Input[_currentPos])
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '^':
                    result = ParseOperator();
                    break;

                case '(':
                    result = new LeftPar();
                    ++_currentPos;
                    break;

                case ')':
                    result = new RightPar();
                    ++_currentPos;
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    result = ParseNumber();
                    break;

                default:
                    throw new Exception("������! ����������� ������.");
            }

            return result;
        }

        /// <summary>
        /// ������ ����������� �������������� ������
        /// </summary>
        private void BuildAst()
        {
            foreach (var x in _ast.Tokens)
                x.Update(_ast);
        }

        /// <summary>
        /// ����������� ������ ������������ ��������������� ������
        /// </summary>
        /// <param name="root">������ ���</param>
        /// <param name="n">������ �� ������ ����</param>
        private void PostfixPrint(TreeNode root, int n = 0)
        {
            if (root == null)
                return;

            PostfixPrint(root.Right, n + 4);

            for (int i = 1; i < n; ++i)
                Console.Write(" ");

            Console.Write(root.Data.ToString());

            if (root.Left  != null) Console.Write('/');
            if (root.Right != null) Console.Write('\\');

            Console.WriteLine("\n");
            
            PostfixPrint(root.Left, n + 4);
        }

        /// <summary>
        /// ������� ������ �������
        /// </summary>
        /// <param name="filename">��� ����� � ������� � ���������� ��������</param>
        public Parser(string filename)
        {
            List<string> x = new List<string>(File.ReadAllLines(filename, Encoding.Default));

            _data.Postfix = new LinkedList<Token>();
            _data.Input = "";
            _data.TokenStack = new Stack<Token>();
            _data.PrecedenceMap = new Dictionary<string, int>();

            _ast.Tokens = _data.Postfix;
            _ast.TreeNodeStack = new Stack<TreeNode>();

            _currentPos = 0;

            foreach (var s in x.Select(v => v.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)))
            {
                _data.PrecedenceMap.Add(s[0], int.Parse(s[1]));
            }
        }

        /// <summary>
        /// ������ ������� ������
        /// </summary>
        /// <param name="input"></param>
        /// <returns>�������� �������� ������ �������� ������</returns>
        public string Parse(string input)
        {
            _data.Postfix.Clear();

            _data.Input = ReplaceUnOperators(input) + ")";
            _data.TokenStack.Push(new LeftPar());

            while (_data.TokenStack.Count != 0)
            {
                Token t = NextToken();
                t.Update(_data);
            }

            string result = Parser.to_string(_data.Postfix);

            BuildAst();

            return result;
        }

        /// <summary>
        /// �������� ����������� �������������� ������
        /// </summary>
        public void PrintAst()
        {
            if(_ast.TreeNodeStack.Count == 0)
                return;
            
            PostfixPrint(_ast.TreeNodeStack.Peek());
        }

        public double Solve()
        {
            if (_ast.TreeNodeStack.Count == 0)
                return 0;

            return Apply(_ast.TreeNodeStack.Peek());
        }

        private double Apply(TreeNode root)
        {
            double result = 0.0;

            if (root.Data is Number)
                return ((Number) root.Data).Value;

            if (root.Data is Op)
            {
                string x = ((Op) root.Data).ToString();

                switch (x)
                {
                    case "+":
                        result = Apply(root.Left) + Apply(root.Right);
                        break;
                    case "-":
                        result = Apply(root.Left) - Apply(root.Right);
                        break;
                    case "*":
                        result = Apply(root.Left) * Apply(root.Right);
                        break;
                    case "/":
                        result = Apply(root.Left) / Apply(root.Right);
                        break;
                    case "^":
                        result = Math.Pow(Apply(root.Left), Apply(root.Right));
                        break;
                }
            }

            return result;
        }
    }
}