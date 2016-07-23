using System;
using System.Globalization;

namespace Parser_C_Sharp
{
    /// <summary>
    /// Абстрактный токен
    /// </summary>
    public abstract class Token
    {
        public abstract void Update(ParserData pd);
        public abstract void Update(AstData ad);

        protected void UnwindOpsWithHigherPrecedence(ParserData pd, int precedence)
        {
            Token x = null;

            if (pd.TokenStack.Count != 0)
                x = pd.TokenStack.Peek();

            while (pd.TokenStack.Count != 0
                && x is Op
                && pd.PrecedenceMap[(x as Op).ToString()] >= precedence)
            {
                x = pd.TokenStack.Pop();

                pd.Postfix.AddLast(x);

                if (pd.TokenStack.Count == 0)
                    break;

                x = pd.TokenStack.Peek();
            }
        }

        protected virtual void UnwindUntillLeftPar(ParserData pd)
        {
            Token x = null;
            if (pd.TokenStack.Count != 0)
                x = pd.TokenStack.Pop();

            while (pd.TokenStack.Count != 0
                && !(x is LeftPar))
            {
                pd.Postfix.AddLast(x);

                if (pd.TokenStack.Count == 0)
                    break;
                x = pd.TokenStack.Pop();
            }
        }
    }

    /// <summary>
    /// Представление числа
    /// </summary>
    public class Number : Token
    {
        private double _number;

        public Number(double number)
        {
            _number = number;
        }

        public override void Update(ParserData pd)
        {
            pd.Postfix.AddLast(this);
        }

        public override void Update(AstData ad)
        {
            ad.TreeNodeStack.Push(new TreeNode(this));
        }

        public override string ToString()
        {
            return _number.ToString(CultureInfo.InvariantCulture);
        }

        public double Value => _number;
    }

    /// <summary>
    /// Представление операции
    /// </summary>
    public class Op : Token
    {
        private string _operation;

        public Op(string operation)
        {
            _operation = operation;
        }

        public override void Update(AstData ad)
        {
#if TEST
            TreeNode r = ad.TreeNodeStack.Pop();
            TreeNode l;
            if (ad.TreeNodeStack.Count == 0)
{  
  l = new Number(0);
}

#endif
            TreeNode r = ad.TreeNodeStack.Pop();
            TreeNode l = ad.TreeNodeStack.Pop();

            ad.TreeNodeStack.Push(new TreeNode(this, l, r));
        }

        public override void Update(ParserData pd)
        {
            UnwindOpsWithHigherPrecedence(pd, pd.PrecedenceMap[_operation]);
            pd.TokenStack.Push(this);
        }

        public override string ToString()
        {
            return _operation;
        }
    }

    /// <summary>
    /// Представление открывающей скобки
    /// </summary>
    public class LeftPar : Token
    {
        public override void Update(ParserData pd)
        {
            pd.TokenStack.Push(this);
        }

        public override void Update(AstData ad)
        {
            throw new Exception("Неожиданный символ \"(\"!");
        }

        protected override void UnwindUntillLeftPar(ParserData pd)
        {
            base.UnwindUntillLeftPar(pd);

            if (pd.TokenStack.Count != 0)
                pd.TokenStack.Pop();
        }

        public override string ToString()
        {
            return "(";
        }
    }

    /// <summary>
    /// Представление закрывающей скобки
    /// </summary>
    public class RightPar : Token
    {
        public override void Update(AstData ad)
        {
            throw new Exception("Неожиданный символ \")\"!");
        }

        public override void Update(ParserData pd)
        {
            UnwindUntillLeftPar(pd);
        }

        public override string ToString()
        {
            return ")";
        }
    }
}