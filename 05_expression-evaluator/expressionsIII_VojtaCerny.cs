using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTestProject1")]
namespace ExpressionLib {
    interface IExpressionVisitor<T> {
        T Visit(Constant c);
        T Visit(NegUnaryOperator op);
        T Visit(PlusBinaryOperator op);
        T Visit(MinusBinaryOperator op);
        T Visit(TimesBinaryOperator op);
        T Visit(DivBinaryOperator op);
    }
    
    abstract class Expression {
        public readonly int Arity;
        public Expression(int arity) {
            Arity = arity;
        }
        public virtual Expression this[int i] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);

        // static
        protected static Expression ParseToken(string s) {
            // using shortened evaluation, condition will suceed as soon as first parsing suceeds
            Expression expression;
            if (
                Constant.TryParse(s, out expression) ||
                NegUnaryOperator.TryParse(s, out expression) ||
                PlusBinaryOperator.TryParse(s, out expression) ||
                MinusBinaryOperator.TryParse(s, out expression) ||
                TimesBinaryOperator.TryParse(s, out expression) ||
                DivBinaryOperator.TryParse(s, out expression)
                ) {
                return expression;
            }
            throw new FormatException();
        }
        public static Expression Parse(string s) {
            string[] arr = s.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int ptr = 0;
            Expression expr = Parse(arr, ref ptr);
            if (ptr != arr.Length) { // trailing characters
                throw new FormatException();
            }
            return expr;
        }
        private static Expression Parse(string[] arr, ref int ptr) {
            if (ptr >= arr.Length) {
                throw new FormatException();
            }
            Expression expr = ParseToken(arr[ptr++]);
            for (int i = 0; i < expr.Arity; ++i) {
                expr[i] = Parse(arr, ref ptr);
            }
            return expr;
        }
    }

    class Constant : Expression {
        public readonly int Value;
        public Constant(int value) : base(0) {
            Value = value;
        }
        public static bool TryParse(string s, out Expression expr) {
            int a;
            if (int.TryParse(s, out a)) {
                expr = new Constant(a);
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
    abstract class UnaryOperator : Expression {
        public Expression operand;
        public UnaryOperator() : base(1) { }
        public override Expression this[int i] {
            get {
                return operand;
            }
            set {
                operand = value;
            }
        }
    }
    abstract class BinaryOperator : Expression {
        public Expression[] operands = new Expression[2];
        public BinaryOperator() : base(2) { }
        public override Expression this[int i] {
            get {
                return operands[i];
            }
            set {
                operands[i] = value;
            }
        }
    }
    class NegUnaryOperator : UnaryOperator {
        public static bool TryParse(string s, out Expression expr) {
            if (s == "~") {
                expr = new NegUnaryOperator();
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
    class PlusBinaryOperator : BinaryOperator {
        public static bool TryParse(string s, out Expression expr) {
            if (s == "+") {
                expr = new PlusBinaryOperator();
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
    class MinusBinaryOperator : BinaryOperator {
        public static bool TryParse(string s, out Expression expr) {
            if (s == "-") {
                expr = new MinusBinaryOperator();
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
    class TimesBinaryOperator : BinaryOperator {
        public static bool TryParse(string s, out Expression expr) {
            if (s == "*") {
                expr = new TimesBinaryOperator();
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
    class DivBinaryOperator : BinaryOperator {
        public static bool TryParse(string s, out Expression expr) {
            if (s == "/") {
                expr = new DivBinaryOperator();
                return true;
            }
            expr = null;
            return false;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }

    sealed class NoReturn {
        // singleton
        public static readonly NoReturn Instance = new NoReturn();
        private NoReturn() { }
    }

    class IntEvaluingVisitor : IExpressionVisitor<int> {
        public int Visit(Constant c) {
            return c.Value;
        }
        public int Visit(NegUnaryOperator op) {
            return checked(-op.operand.Accept(this));
        }
        public int Visit(PlusBinaryOperator op) {
            return checked(op.operands[0].Accept(this) + op.operands[1].Accept(this));
        }
        public int Visit(MinusBinaryOperator op) {
            return checked(op.operands[0].Accept(this) - op.operands[1].Accept(this));
        }
        public int Visit(TimesBinaryOperator op) {
            return checked(op.operands[0].Accept(this) * op.operands[1].Accept(this));
        }
        public int Visit(DivBinaryOperator op) {
            // DivideByZeroException may be thrown
            return checked(op.operands[0].Accept(this) / op.operands[1].Accept(this));
        }
    }

    // painful to copy-paste, but there is no easy way in c# to use arithmetic on generic types
    class DoubleEvaluingVisitor : IExpressionVisitor<double> {
        public double Visit(Constant c) {
            return (double)c.Value;
        }
        public double Visit(NegUnaryOperator op) {
            return checked(-op.operand.Accept(this));
        }
        public double Visit(PlusBinaryOperator op) {
            return checked(op.operands[0].Accept(this) + op.operands[1].Accept(this));
        }
        public double Visit(MinusBinaryOperator op) {
            return checked(op.operands[0].Accept(this) - op.operands[1].Accept(this));
        }
        public double Visit(TimesBinaryOperator op) {
            return checked(op.operands[0].Accept(this) * op.operands[1].Accept(this));
        }
        public double Visit(DivBinaryOperator op) {
            // DivideByZeroException may be thrown
            return checked(op.operands[0].Accept(this) / op.operands[1].Accept(this));
        }
    }

    class FullBracketPrintingVisitor : IExpressionVisitor<NoReturn> {
        private TextWriter output;
        public FullBracketPrintingVisitor(TextWriter output) {
            this.output = output;
        }
        public NoReturn Visit(Constant c) {
            output.Write(c.Value);
            return NoReturn.Instance;
        }
        public NoReturn Visit(NegUnaryOperator op) {
            output.Write("(-");
            op.operand.Accept(this);
            output.Write(")");
            return NoReturn.Instance;
        }
        
        private void Print(BinaryOperator op, String operString) {
            output.Write("(");
            op.operands[0].Accept(this);
            output.Write(operString);
            op.operands[1].Accept(this);
            output.Write(")");
        }
        public NoReturn Visit(PlusBinaryOperator op) {
            Print(op, "+");
            return NoReturn.Instance;
        }
        public NoReturn Visit(MinusBinaryOperator op) {
            Print(op, "-");
            return NoReturn.Instance;
        }
        public NoReturn Visit(TimesBinaryOperator op) {
            Print(op, "*");
            return NoReturn.Instance;
        }
        public NoReturn Visit(DivBinaryOperator op) {
            Print(op, "/");
            return NoReturn.Instance;
        }
    }

    static class PriorityExtension {
        public static int Priority (this Expression expr) {
            if (expr.GetType() == typeof(NegUnaryOperator)) {
                return 4;
            } else if (expr.GetType() == typeof(PlusBinaryOperator)) {
                return 1;
            } else if (expr.GetType() == typeof(MinusBinaryOperator)) {
                return 1;
            } else if (expr.GetType() == typeof(TimesBinaryOperator)) {
                return 2;
            } else if (expr.GetType() == typeof(DivBinaryOperator)) {
                return 2;
            } else if (expr.GetType() == typeof(Constant)) {
                return int.MaxValue;
            }
            // should never happen
            return -1;
        }
    }
    static class AssociativityExtension {
        public static bool IsAssociative(this Expression expr) {
            if (expr.GetType() == typeof(MinusBinaryOperator) ||
                expr.GetType() == typeof(DivBinaryOperator)) {
                return false;
            }
            return true;
        }
    }
    class MinimalBracketPrintingVisitor : IExpressionVisitor<NoReturn> {
        private TextWriter output;
        public MinimalBracketPrintingVisitor(TextWriter output) {
            this.output = output;
        }
        public NoReturn Visit(Constant c) {
            output.Write(c.Value);
            return NoReturn.Instance;
        }
        private void AcceptAndPrintBracketsIf(bool condition, Expression expr){
            if (condition) {
                output.Write("(");
                expr.Accept(this);
                output.Write(")");
            } else {
                expr.Accept(this);
            }
        }
        public NoReturn Visit(NegUnaryOperator op) {
            output.Write("-");
            AcceptAndPrintBracketsIf(
                op.operand.Priority() < op.Priority(),
                op.operand
            );
            return NoReturn.Instance;
        }
        private void Print(BinaryOperator op, string operString) {
            AcceptAndPrintBracketsIf(
                op.operands[0].Priority() < op.Priority(),
                op.operands[0]
            );

            output.Write(operString);

            AcceptAndPrintBracketsIf(
                op.operands[1].Priority() < op.Priority() || (
                op.operands[1].Priority() == op.Priority() &&
                !op.IsAssociative()),

                op.operands[1]
            );
        }
        public NoReturn Visit(PlusBinaryOperator op) {
            Print(op, "+");
            return NoReturn.Instance;
        }
        public NoReturn Visit(MinusBinaryOperator op) {
            Print(op, "-");
            return NoReturn.Instance;
        }
        public NoReturn Visit(TimesBinaryOperator op) {
            Print(op, "*");
            return NoReturn.Instance;
        }
        public NoReturn Visit(DivBinaryOperator op) {
            Print(op, "/");
            return NoReturn.Instance;
        }
    }

    class ProgramBody {
        public static void MainFunc(string[] args, TextReader input, TextWriter output) {
            Expression expr = null;
            string cachedFullBrackets = null;
            string cachedMinimalBrackets = null;
            while (true) {
                try {
                    #region 1-end of input and parsing
                    string s = input.ReadLine();
                    if (s == null || s == "end") {
                        break;
                    } else if (s.Length >= 1 && s[0] == '=') {
                        cachedFullBrackets = null;
                        cachedMinimalBrackets = null;
                        try {
                            if (s.Length < 2 || s[1] != ' ') {
                                throw new FormatException();
                            }
                            expr = Expression.Parse(s.Substring(2));
                        } catch (Exception) {
                            expr = null;
                            throw;
                        }
                        continue;
                    }
                    #endregion
                    #region 2-common errors and exceptions
                    switch (s) {
                        case "i": case "d": case "p": case "P":
                            if (expr == null) {
                                output.WriteLine("Expression Missing");
                                continue;
                            }
                            break;
                        case "":
                            continue; //not in description, figured from test data
                        default:
                            throw new FormatException();
                    }
                    #endregion
                    #region 3-actual actions
                    switch (s) {
                        case "i":
                            var visitor = new IntEvaluingVisitor();
                            output.WriteLine(expr.Accept(visitor));
                            break;
                        case "d":
                            var visitor2 = new DoubleEvaluingVisitor();
                            output.WriteLine(expr.Accept(visitor2).ToString(
                                "F5",
                                CultureInfo.InvariantCulture
                                ));
                            break;
                        case "p":
                            if (cachedFullBrackets != null) {
                                output.WriteLine(cachedFullBrackets);
                                break;
                            }
                            StringWriter sw = new StringWriter();
                            var visitor3 = new FullBracketPrintingVisitor(sw);
                            expr.Accept(visitor3);
                            cachedFullBrackets = sw.ToString();
                            output.WriteLine(cachedFullBrackets);
                            break;
                        case "P":
                            if (cachedMinimalBrackets != null) {
                                output.WriteLine(cachedMinimalBrackets);
                                break;
                            }
                            StringWriter sw2 = new StringWriter();
                            var visitor4 = new MinimalBracketPrintingVisitor(sw2);
                            expr.Accept(visitor4);
                            cachedMinimalBrackets = sw2.ToString();
                            output.WriteLine(cachedMinimalBrackets);
                            break;
                    }
                    #endregion
                } catch (FormatException) {
                    output.WriteLine("Format Error");
                } catch (OverflowException) {
                    output.WriteLine("Overflow Error");
                } catch (DivideByZeroException) {
                    output.WriteLine("Divide Error");
                }
            }
        }
    }

    public class Program {
        static void Main(string[] args) {
            ProgramBody.MainFunc(args, Console.In, Console.Out);
        }
    }
}