using System;
using System.Linq;
using System.IO;

namespace Vyrazy {
    public abstract class StandardExpressionError : Exception {
        private readonly string outputMessage;

        public StandardExpressionError(string outputMessage, string message = null)
            : base(message)
        {
            this.outputMessage = outputMessage;
        }

        public string OutputMessage
        {
            get
            {
                return outputMessage;
            }
        }
    }

    public class FormatError : StandardExpressionError {
        public FormatError(string message = null) : base("Format Error", message) { }
    }

    public class DivideError : StandardExpressionError {
        public DivideError(string message = null) : base("Divide Error", message) { }
    }

    public class OverflowError : StandardExpressionError {
        public OverflowError(string message = null) : base("Overflow Error", message) { }
    }

    public interface NodeVisitor<T> {
        T Visit(PlusNode plus);
        T Visit(MinusNode minus);
        T Visit(MultiplyNode multiply);
        T Visit(DivideNode divide);
        T Visit(UnaryMinusNode unaryMinus);
        T Visit(ConstantNode constantNode);
    };

    public abstract class Node {
        public abstract T Accept<T>(NodeVisitor<T> visitor);
    };

    public class ConstantNode : Node {
        private readonly int value;

        public int Value
        {
            get { return value; }
        }

        public ConstantNode(int value)
        {
            this.value = value;
        }

        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public abstract class NAryNode : Node {
        private Node[] children;
        public Node[] Children
        {
            get
            {
                return children;
            }
            set
            {
                if (!ChildrenOK(value)) throw new Exception("Children not OK");
                children = value;
            }
        }
        protected abstract bool ChildrenOK(Node[] children);
    }

    public abstract class BinaryNode : NAryNode {
        protected override bool ChildrenOK(Node[] children) { return children.Length == 2; }
        public Node LeftChild { get { return Children[0]; } }
        public Node RightChild { get { return Children[1]; } }
    };

    public abstract class UnaryNode : NAryNode {
        protected override bool ChildrenOK(Node[] children) { return children.Length == 1; }
        public Node Child { get { return Children[0]; } }
    };

    public class PlusNode : BinaryNode {
        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public class MinusNode : BinaryNode {
        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public class MultiplyNode : BinaryNode {
        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public class DivideNode : BinaryNode {
        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public class UnaryMinusNode : UnaryNode {
        public override T Accept<T>(NodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    };

    public abstract class NodeParser {
        public abstract bool TryParse(NodeParser recursiveParser, string[] tokens, int start, out int consumed, out Node node);

        public static NAryNodeParser<T> BinaryOperator<T>(string op) where T : NAryNode, new()
        {
            return new NAryNodeParser<T>(op, 2);
        }

        public static NAryNodeParser<T> UnaryOperator<T>(string op) where T : NAryNode, new()
        {
            return new NAryNodeParser<T>(op, 1);
        }

        public static NodeParser BuildDefault()
        {
            return new ConsumerParser("=", new CombinedParser(new NodeParser[] {
				BinaryOperator<PlusNode>("+"),
				BinaryOperator<MinusNode>("-"),
				BinaryOperator<MultiplyNode>("*"),
				BinaryOperator<DivideNode>("/"),
				UnaryOperator<UnaryMinusNode>("~"),
				ConstantParser.Instance
			}));
        }
    };

    public class ConstantParser : NodeParser {
        public override bool TryParse(NodeParser unused_recursiveParser, string[] tokens, int start, out int consumed, out Node node)
        {
            consumed = 0;
            node = null;

            if (tokens.Length <= start)
            {
                return false;
            }

            string token = tokens[start];
            int value;

            if (int.TryParse(token, out value))
            {
                node = new ConstantNode(value);
                consumed = 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static ConstantParser _Instance;

        public static ConstantParser Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ConstantParser();
                }
                return _Instance;
            }
        }
    };

    public class ConsumerParser : NodeParser {
        private NodeParser parser;
        private string token;

        public ConsumerParser(string token, NodeParser parser)
        {
            this.token = token;
            this.parser = parser;
        }

        public override bool TryParse(NodeParser recursiveParser, string[] tokens, int start, out int consumed, out Node node)
        {
            consumed = 0;
            node = null;

            if (tokens.Length <= start || tokens[start] != token)
            {
                return false;
            }

            int consumedNow;
            if (!parser.TryParse(parser, tokens, start + 1, out consumedNow, out node))
            {
                throw new FormatError("Child failed to parse.");
            }

            consumed = consumedNow + 1;
            return true;
        }
    };

    public class NAryNodeParser<T> : NodeParser where T : NAryNode, new() {
        private string op;
        private int arity;

        public NAryNodeParser(string op, int arity)
        {
            this.op = op;
            this.arity = arity;
        }

        public override bool TryParse(NodeParser recursiveParser, string[] tokens, int start, out int consumed, out Node node)
        {
            consumed = 0;
            node = null;

            if (tokens.Length <= start)
            {
                return false;
            }

            string token = tokens[start];

            if (token != this.op) return false; // This is not for us to parse.
            consumed = 1;

            Node[] children = new Node[arity];
            for (int i = 0; i < arity; i++)
            {
                int consumedNow;
                if (!recursiveParser.TryParse(recursiveParser, tokens, start + consumed, out consumedNow, out children[i]))
                {
                    throw new FormatError("Child failed to parse.");
                }
                consumed += consumedNow;
            }

            // XXX: cannot force constructor...
            NAryNode _node = new T();
            _node.Children = children;
            node = _node;

            return true;
        }
    };

    public class CombinedParser : NodeParser {
        private NodeParser[] parsers;

        public CombinedParser(NodeParser[] parsers)
        {
            this.parsers = parsers;
        }

        public override bool TryParse(NodeParser recursiveParser, string[] tokens, int start, out int consumed, out Node node)
        {
            consumed = 0;
            node = null;

            foreach (NodeParser parser in parsers)
            {
                // Don't pass our out arguments directly - we want to keep CombinedParser behavior predictable
                // (e.g. "returned false -> node is null and consumed is zero)
                int parserConsumed;
                Node parserGenerated;

                if (parser.TryParse(recursiveParser, tokens, start, out parserConsumed, out parserGenerated))
                {
                    consumed = parserConsumed;
                    node = parserGenerated;
                    return true;
                }
            }

            // No parser parsed the tokens. Sorry.
            return false;
        }
    };

    public class IntEvaluator : NodeVisitor<int> {
        private int BinaryMap(BinaryNode node, Func<int, int, int> func)
        {
            try
            {
                int left = node.LeftChild.Accept(this), right = node.RightChild.Accept(this);
                return func(left, right);
            }
            catch (OverflowException)
            {
                throw new OverflowError();
            }
        }

        public int Visit(ConstantNode constantNode) { return constantNode.Value; }
        public int Visit(PlusNode node) { return BinaryMap(node, (a, b) => checked(a + b)); }
        public int Visit(MinusNode node) { return BinaryMap(node, (a, b) => checked(a - b)); }
        public int Visit(MultiplyNode node) { return BinaryMap(node, (a, b) => checked(a * b)); }
        public int Visit(DivideNode node)
        {
            return BinaryMap(node, (a, b) =>
            {
                if (b == 0) throw new DivideError();
                return checked(a / b);
            });
        }
        public int Visit(UnaryMinusNode node) { return -node.Child.Accept(this); }
    }

    // Cannot limit to 'types that can do arithmetic', so this boilerplate needs to be repeated :( (like 'Num x => ...' in Haskell...)
    public class DoubleEvaluator : NodeVisitor<double> {
        private double BinaryMap(BinaryNode node, Func<double, double, double> func)
        {
            double left = node.LeftChild.Accept(this), right = node.RightChild.Accept(this);
            return func(left, right);
        }

        public double Visit(ConstantNode constantNode) { return constantNode.Value; }
        public double Visit(PlusNode node) { return BinaryMap(node, (a, b) => a + b); }
        public double Visit(MinusNode node) { return BinaryMap(node, (a, b) => a - b); }
        public double Visit(MultiplyNode node) { return BinaryMap(node, (a, b) => a * b); }
        public double Visit(DivideNode node) { return BinaryMap(node, (a, b) => a / b); }
        public double Visit(UnaryMinusNode node) { return -node.Child.Accept(this); }
    }

    public class Calculator {
        private static readonly char[] TOKEN_SEPARATORS = new char[] { ' ' };

        private Node root;

        public string RunVisitor<T>(NodeVisitor<T> visitor, Func<T, string> stringFunc)
        {
            if (root == null)
            {
                return "Expression Missing";
            }

            try
            {
                return stringFunc(root.Accept(visitor));
            }
            catch (StandardExpressionError error)
            {
                // For debugging:
                // Console.WriteLine(error.Message);
                return error.OutputMessage;
            }
        }

        public string Calculate(string input)
        {
            if (input == "") return null;

            if (input == "i") return RunVisitor(new IntEvaluator(), a => a.ToString());
            if (input == "d")
            {
                return RunVisitor(new DoubleEvaluator(), a => String.Format("{0:F5}", a));
            }

            NodeParser parser = NodeParser.BuildDefault();
            string[] tokens = input.Split(TOKEN_SEPARATORS);

            try
            {
                if (tokens.Length == 0) throw new FormatError("No tokens of input");

                int consumed;

                if (!parser.TryParse(parser, tokens, 0, out consumed, out root))
                {
                    throw new FormatError("No parser could parse the input");
                }

                if (consumed < tokens.Length)
                {
                    throw new FormatError("Some tokens remained unparsed");
                }

                return null;
            }
            catch (StandardExpressionError error)
            {
                // For debugging:
                // Console.WriteLine(error.Message);
                return error.OutputMessage;
            }
        }
    }

    public class Program {
        public static void RunOnStreams(TextReader input, TextWriter output)
        {
            Calculator calculator = new Calculator();

            string line;
            while ((line = input.ReadLine()) != null && line != "end")
            {
                string outLine = calculator.Calculate(line);
                if (outLine != null)
                {
                    output.WriteLine(outLine);
                }
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "__TEST__")
            {
                //new Tests().Test();
            }
            else
            {
                RunOnStreams(Console.In, Console.Out);
            }
        }
    }
}
