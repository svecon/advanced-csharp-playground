using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ExpressionEvaluation_UnitTests")]

namespace ExpressionEvaluation {

    class Evaluator {

        class Function {

            int rnarity;
            String representation;
            Func<int[], int> function;

            public Function(String representation, int rnarity, Func<int[], int> function)
            {
                this.representation = representation;
                this.rnarity = rnarity;
                this.function = function;
            }

            public int GetRnarity() { return rnarity; }

            public int Evaluate(int[] vals) { return this.function(vals); }

        }

        interface Node { int Evaluate(); }

        class FunctionNode : Node {

            Function function;
            Node[] subnodes;
            byte currentSubnode;

            public FunctionNode(Function function)
            {
                this.function = function;
                subnodes = new Node[function.GetRnarity()];
            }

            public void AddSubnode(Node node) { subnodes[currentSubnode++] = node; }

            public bool IsFull() { return function.GetRnarity() == currentSubnode; }

            public int Evaluate()
            {
                int[] values = new int[function.GetRnarity()];

                for (int i = 0; i < function.GetRnarity(); i++)
                    values[i] = subnodes[i].Evaluate();

                return function.Evaluate(values);
            }
        }

        class ValueNode : Node {
            int value;
            public ValueNode(int value) { this.value = value; }

            public int Evaluate() { return this.value; }
        }

        static Dictionary<string, Function> operations;
        Stack<Node> buildingStack;
        Node root;

        public Evaluator()
        {
            buildingStack = new Stack<Node>();

            if (operations != null) return;

            operations = new Dictionary<string, Function>();
            operations.Add("+", new Function("+", 2, x => checked(x[0] + x[1])));
            operations.Add("-", new Function("-", 2, x => checked(x[0] - x[1])));
            operations.Add("*", new Function("*", 2, x => checked(x[0] * x[1])));
            operations.Add("/", new Function("/", 2, x => x[0] / x[1]));
            operations.Add("~", new Function("~", 1, x => (-1) * x[0]));
            operations.Add("pow", new Function("pow", 2, x => checked((int)Math.Pow(x[0], x[1]))));
            operations.Add("max", new Function("max", 3, x => Math.Max(x[0], Math.Max(x[1], x[2]))));
        }

        public Evaluator CreateExpression(String line)
        {
            String[] words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
                AddWord(words[i]);

            return this;
        }

        public Evaluator AddWord(String word)
        {
            int value;
            Node node;

            if (int.TryParse(word, out value))
                node = new ValueNode(value);
            else
                if (operations.ContainsKey(word))
                    node = new FunctionNode(operations[word]);
                else throw new FormatException("Function not found");

            if (root == null)
                root = node;
            else if (buildingStack.Count > 0)
            {
                FunctionNode parent = (FunctionNode)buildingStack.Peek();
                parent.AddSubnode(node);

                if (parent.IsFull()) buildingStack.Pop();
            }
            else
                throw new FormatException("Building stack is empty.");

            if (node is FunctionNode)
                buildingStack.Push(node);

            return this;
        }

        public int Evaluate()
        {
            finish();
            return root.Evaluate();
        }

        void finish()
        {
            if (root == null)
                throw new FormatException("No formula entered");
            if (buildingStack.Count > 0)
                throw new FormatException("Building stack is not empty after Finish");
        }

    }

    class Program {

        public static void ReportDivisionByZero() { Console.WriteLine("Divide Error"); }

        public static void ReportFormatError() { Console.WriteLine("Format Error"); }

        public static void ReportOverflowError() { Console.WriteLine("Overflow Error"); }

        static void Main(string[] args)
        {
            try
            {
                Evaluator e = new Evaluator();
                e.CreateExpression(Console.ReadLine());
                Console.WriteLine(e.Evaluate());
            }
            catch (DivideByZeroException) { ReportDivisionByZero(); }
            catch (FormatException) { ReportFormatError(); }
            catch (OverflowException) { ReportOverflowError(); }
        }
    }
}
