using System;
using System.Text;
using System.Collections.Generic;

namespace ExpressionEvaluator {

    interface IVisitor<T> {
        T Visit(ConstantExpression exp);
        T Visit(UnaryMinusExpression exp);
        T Visit(PlusExpression exp);
        T Visit(MinusExpression exp);
        T Visit(MultiplyExpression exp);
        T Visit(DivideExpression exp);
    }

    abstract class Expression {
        public static Expression ParsePrefixExpression(string exprString)
        {
            string[] tokens = exprString.Substring(1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Expression result = null;
            Stack<OperatorExpression> unresolved = new Stack<OperatorExpression>();
            foreach (string token in tokens)
            {
                if (result != null)
                {
                    // We correctly parsed the whole tree, but there was at least one more unprocessed token left.
                    // This implies incorrect input, thus return null.

                    return null;
                }

                switch (token)
                {
                    case "+":
                        unresolved.Push(new PlusExpression());
                        break;

                    case "-":
                        unresolved.Push(new MinusExpression());
                        break;

                    case "*":
                        unresolved.Push(new MultiplyExpression());
                        break;

                    case "/":
                        unresolved.Push(new DivideExpression());
                        break;

                    case "~":
                        unresolved.Push(new UnaryMinusExpression());
                        break;

                    default:
                        int value;
                        if (!int.TryParse(token, out value))
                        {
                            return null;	// Invalid token format
                        }

                        Expression expr = new ConstantExpression(value);
                        while (unresolved.Count > 0)
                        {
                            OperatorExpression oper = unresolved.Peek();
                            if (oper.AddOperand(expr))
                            {
                                unresolved.Pop();
                                expr = oper;
                            }
                            else
                            {
                                expr = null;
                                break;
                            }
                        }

                        if (expr != null)
                        {
                            result = expr;
                        }

                        break;
                }
            }

            return result;
        }

        public abstract T Accept<T>(IVisitor<T> visitor);

        public abstract int Evaluate();
    }

    abstract class ValueExpression : Expression {
        public abstract int Value
        {
            get;
        }

        public sealed override int Evaluate()
        {
            return Value;
        }
    }

    sealed class ConstantExpression : ValueExpression {
        private int value;

        public ConstantExpression(int value)
        {
            this.value = value;
        }

        public override int Value
        {
            get { return this.value; }
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    abstract class OperatorExpression : Expression {
        public abstract bool AddOperand(Expression op);
    }

    abstract class UnaryExpression : OperatorExpression {
        protected Expression op;

        public Expression Op
        {
            get { return op; }
            set { op = value; }
        }

        public override bool AddOperand(Expression op)
        {
            if (this.op == null)
            {
                this.op = op;
            }
            return true;
        }

        public sealed override int Evaluate()
        {
            return Evaluate(op.Evaluate());
        }

        protected abstract int Evaluate(int opValue);
    }

    abstract class BinaryExpression : OperatorExpression {
        protected Expression op0, op1;

        public Expression Op0
        {
            get { return op0; }
            set { op0 = value; }
        }

        public Expression Op1
        {
            get { return op1; }
            set { op1 = value; }
        }

        public override bool AddOperand(Expression op)
        {
            if (op0 == null)
            {
                op0 = op;
                return false;
            }
            else if (op1 == null)
            {
                op1 = op;
            }
            return true;
        }

        public sealed override int Evaluate()
        {
            return Evaluate(op0.Evaluate(), op1.Evaluate());
        }

        protected abstract int Evaluate(int op0Value, int op1Value);
    }

    sealed class PlusExpression : BinaryExpression {
        protected override int Evaluate(int op0Value, int op1Value)
        {
            return checked(op0Value + op1Value);
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    sealed class MinusExpression : BinaryExpression {
        protected override int Evaluate(int op0Value, int op1Value)
        {
            return checked(op0Value - op1Value);
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    sealed class MultiplyExpression : BinaryExpression {
        protected override int Evaluate(int op0Value, int op1Value)
        {
            return checked(op0Value * op1Value);
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    sealed class DivideExpression : BinaryExpression {
        protected override int Evaluate(int op0Value, int op1Value)
        {
            return checked(op0Value / op1Value);	// Can generate DivideByZeroException
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    sealed class UnaryMinusExpression : UnaryExpression {
        protected override int Evaluate(int opValue)
        {
            return checked(-opValue);
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    class DoubleVisitor : IVisitor<double> {

        public double Visit(ConstantExpression exp)
        {
            return exp.Value;
        }

        public double Visit(UnaryMinusExpression exp)
        {
            return checked((-1) * exp.Op.Accept(this));
        }

        public double Visit(PlusExpression exp)
        {
            return checked(exp.Op0.Accept(this) + exp.Op1.Accept(this));
        }

        public double Visit(MinusExpression exp)
        {
            return checked(exp.Op0.Accept(this) - exp.Op1.Accept(this));
        }

        public double Visit(MultiplyExpression exp)
        {
            return checked(exp.Op0.Accept(this) * exp.Op1.Accept(this));
        }

        public double Visit(DivideExpression exp)
        {
            return checked(exp.Op0.Accept(this) / exp.Op1.Accept(this));
        }
    }

    class ParenthesesVisitor : IVisitor<string> {

        public string Visit(ConstantExpression exp)
        {
            return exp.Value.ToString();
        }

        public string Visit(UnaryMinusExpression exp)
        {
            return "(-" + exp.Op.Accept(this) + ")";
        }

        public string Visit(PlusExpression exp)
        {
            return "(" + exp.Op0.Accept(this) + "+" + exp.Op1.Accept(this) + ")";
        }

        public string Visit(MinusExpression exp)
        {
            return "(" + exp.Op0.Accept(this) + "-" + exp.Op1.Accept(this) + ")";
        }

        public string Visit(MultiplyExpression exp)
        {
            return "(" + exp.Op0.Accept(this) + "*" + exp.Op1.Accept(this) + ")";
        }

        public string Visit(DivideExpression exp)
        {
            return "(" + exp.Op0.Accept(this) + "/" + exp.Op1.Accept(this) + ")";
        }
    }

    class SmartParenthesesVisitor : IVisitor<StringBuilder> {

        StringBuilder sb;

        public SmartParenthesesVisitor()
        {
            sb = new StringBuilder();
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        int precedence(Expression expr)
        {
            if (expr.GetType() == typeof(ConstantExpression))
                return 128;
            if (expr.GetType() == typeof(UnaryMinusExpression))
                return 64;
            else if (expr.GetType() == typeof(PlusExpression))
                return 16;
            else if (expr.GetType() == typeof(MinusExpression))
                return 16;
            else if (expr.GetType() == typeof(MultiplyExpression))
                return 32;
            else if (expr.GetType() == typeof(DivideExpression))
                return 32;

            throw new ArgumentException("Unknown class.");
        }

        bool isAsociative(Expression expr)
        {
            if (expr.GetType() == typeof(MinusExpression))
                return false;
            else if (expr.GetType() == typeof(DivideExpression))
                return false;

            return true;
        }

        StringBuilder unaryAppender(Expression parent, Expression sub, String operation)
        {
            sb.Append(operation);

            int sbLength = sb.Length;

            if (sub.GetType() != typeof(ConstantExpression)
                && sub.GetType() != typeof(UnaryMinusExpression))
                return sub.Accept(this).Insert(sbLength, "(").Append(")");
            else
                return sub.Accept(this);
        }

        StringBuilder binaryAppender(Expression parent, Expression left, Expression right, String operation)
        {
            int sbLength = sb.Length;

            if (precedence(left) < precedence(parent))
                left.Accept(this).Insert(sbLength, "(").Append(")");
            else
                left.Accept(this);

            sb.Append(operation);

            sbLength = sb.Length;

            if ((precedence(right) < precedence(parent)) || (precedence(right) == precedence(parent) && !isAsociative(parent)))
                return right.Accept(this).Insert(sbLength, "(").Append(")");
            else
                return right.Accept(this);
        }

        public StringBuilder Visit(ConstantExpression exp)
        {
            return sb.Append(exp.Value.ToString());
        }

        public StringBuilder Visit(UnaryMinusExpression exp)
        {
            return unaryAppender(exp, exp.Op, "-");
        }

        public StringBuilder Visit(PlusExpression exp)
        {
            return binaryAppender(exp, exp.Op0, exp.Op1, "+");
        }

        public StringBuilder Visit(MinusExpression exp)
        {
            return binaryAppender(exp, exp.Op0, exp.Op1, "-");
        }

        public StringBuilder Visit(MultiplyExpression exp)
        {
            return binaryAppender(exp, exp.Op0, exp.Op1, "*");
        }

        public StringBuilder Visit(DivideExpression exp)
        {
            return binaryAppender(exp, exp.Op0, exp.Op1, "/");
        }
    }

    class Program {
        static void Main(string[] args)
        {
            Expression expr = null;
            String ln;
            while ((ln = Console.ReadLine()) != null && ln != "end")
            {
                ln = ln.Trim();

                if (ln.Length == 0)
                    continue;

                if (expr == null && (ln == "i" || ln == "d" || ln == "p" || ln == "P"))
                {
                    Console.WriteLine("Expression Missing");
                    continue;
                }

                try
                {
                    if (ln[0] == '=')
                    {
                        expr = Expression.ParsePrefixExpression(ln);

                        if (expr == null)
                            Console.WriteLine("Format Error");

                        continue;
                    }

                    if (ln == "i")
                    {
                        Console.WriteLine(expr.Evaluate());
                        continue;
                    }

                    if (ln == "d")
                    {
                        Console.WriteLine(String.Format("{0:F5}", expr.Accept<double>(new DoubleVisitor())));
                        continue;
                    }

                    if (ln == "p")
                    {
                        Console.WriteLine(expr.Accept<string>(new ParenthesesVisitor()));
                        continue;
                    }

                    if (ln == "P")
                    {
                        var visitorSmartParentheses = new SmartParenthesesVisitor();
                        expr.Accept<StringBuilder>(visitorSmartParentheses);

                        Console.WriteLine(visitorSmartParentheses.ToString());
                        continue;
                    }

                    Console.WriteLine("Format Error");

                }
                catch (DivideByZeroException) { Console.WriteLine("Divide Error"); }
                catch (OverflowException) { Console.WriteLine("Overflow Error"); }
            }
        }
    }
}