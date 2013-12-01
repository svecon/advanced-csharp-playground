using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GateNetworkTests")]

namespace GateNetwork {

    abstract class Function<T> {

        protected Array data;

        public virtual void Set(int[] index, T value)
        {
            data.SetValue(value, index);
        }

        public virtual T Get(int[] index)
        {
            return (T)data.GetValue(index);
        }

    }

    class MultiDimensionalFunction<T> : Function<T> {

        int nInputs;

        public MultiDimensionalFunction(int nInputs, int possibleValues)
        {
            this.nInputs = nInputs;

            int[] lengths = new int[nInputs];
            for (int i = 0; i < nInputs; i++)
                lengths[i] = possibleValues;

            data = Array.CreateInstance(typeof(T), lengths);
        }
    }

    class SignalFunction : MultiDimensionalFunction<SignalValues[]> {

        public SignalFunction(int nInputs, int possibleValues)
            : base(nInputs, possibleValues) { }
    }

    class ConstantSignalFunction : Function<SignalValues[]> {

        int mOutputs;
        new SignalValues[] data;

        public override void Set(int[] index, SignalValues[] value)
        {
            data = value;
        }

        public override SignalValues[] Get(int[] index)
        {
            return data;
        }

    }

    enum SignalValues { unknown, zero, one };

    class Helper {

        public static string ConvertFromSignalValue(SignalValues v)
        {
            switch (v)
            {
                case SignalValues.unknown: return "?";
                case SignalValues.zero: return "0";
                case SignalValues.one: return "1";
                default: throw new InvalidDataException();
            }
        }

        public static SignalValues ConvertToSignalValue(string s)
        {
            switch (s)
            {
                case "?": return SignalValues.unknown;
                case "0": return SignalValues.zero;
                case "1": return SignalValues.one;
                default: throw new InvalidDataException();
            }
        }

    }

    class LineParser : IDisposable {

        public class Separator {
            public string representation;
            public bool skippable;

            public Separator(string representation, bool skippable)
            {
                this.representation = representation;
                this.skippable = skippable;
            }
        }

        TextReader reader;
        Separator[] separators = new[] { new Separator(" ", true), new Separator("\t", true) };
        string[] cache;
        int used;
        int currentLineNumber = 0;

        public LineParser(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.reader = reader;
            this.cache = new string[4];
        }

        public LineParser SetSeparators(Separator[] separators)
        {
            this.separators = separators;
            return this;
        }

        public LineParser AddSeparator(string representation, bool skippable)
        {
            Separator[] nsep = new Separator[separators.Length + 1];
            
            for (int i = 0; i < separators.Length; i++)
                nsep[i] = separators[i];
            
            nsep[nsep.Length - 1] = new Separator(representation, skippable);
            return this;
        }

        enum IsSeparator { No, Unchecked, Maybe, Yes };

        //IsSeparator canBeSeparator(char c, int position)
        //{
        //    IsSeparator possibilities = IsSeparator.No;

        //    foreach (string separator in separators)
        //    {
        //        if (separator.Length <= position) continue;

        //        if (separator[position] == c && separator.Length == position + 1)
        //        {
        //            possibilities = IsSeparator.Yes;
        //            break;
        //        }
        //        else if (separator[position] == c)
        //        {
        //            possibilities = IsSeparator.Maybe;
        //        }
        //    }

        //    return possibilities;
        //}

        Separator isSeparator(String s)
        {

            for (int i = 0; i < separators.Length; i++)
                if (s.Contains(separators[i].representation))
                    return separators[i];

            return null;
        }

        void addToCache(string s)
        {
            // check whether to increase size
            if (used > cache.Length - 1)
            {
                string[] tempCache = new string[cache.Length * 2 + 2];
                for (int i = 0; i < cache.Length; i++)
                    tempCache[i] = cache[i];
                cache = tempCache;
            }

            cache[used++] = s;
        }

        virtual public void SplitLineFaster()
        {
            //string input = "123xx456yy789";
            //string[] delimiters = { "xx", "yy" };

            //int[] nextPosition = delimiters.Select(d => input.IndexOf(d)).ToArray();
            //List<string> result = new List<string>();
            //int pos = 0;
            //while (true)
            //{
            //    int firstPos = int.MaxValue;
            //    string delimiter = null;
            //    for (int i = 0; i < nextPosition.Length; i++)
            //    {
            //        if (nextPosition[i] != -1 && nextPosition[i] < firstPos)
            //        {
            //            firstPos = nextPosition[i];
            //            delimiter = delimiters[i];
            //        }
            //    }
            //    if (firstPos != int.MaxValue)
            //    {
            //        result.Add(input.Substring(pos, firstPos - pos));
            //        result.Add(delimiter);
            //        pos = firstPos + delimiter.Length;
            //        for (int i = 0; i < nextPosition.Length; i++)
            //        {
            //            if (nextPosition[i] != -1 && nextPosition[i] < pos)
            //            {
            //                nextPosition[i] = input.IndexOf(delimiters[i], pos);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        result.Add(input.Substring(pos));
            //        break;
            //    }
            //}
        }

        virtual public string[] SplitLine()
        {
            var line = reader.ReadLine();
            currentLineNumber++;

            if (line == null)
                return null;

            var wordsBuilder = new StringBuilder();
            Separator separator = null;

            for (int i = 0; i < line.Length; i++)
            {
                wordsBuilder.Append(line[i]);

                separator = isSeparator(wordsBuilder.ToString());

                if (separator != null)
                {
                    wordsBuilder.Length -= separator.representation.Length;
                    if (wordsBuilder.Length > 0)
                        addToCache(wordsBuilder.ToString());

                    if (!separator.skippable)
                        addToCache(separator.representation);

                    wordsBuilder.Length = 0;
                    separator = null;
                }

            }

            if (wordsBuilder.Length > 0)
                addToCache(wordsBuilder.ToString());

            if (used == 0)
                return SplitLine();

            // make correctly sized array and destroy cache
            string[] lineParsed = new string[used];
            for (int i = 0; i < used; i++)
            {
                lineParsed[i] = cache[i];
                cache[i] = null;
            }
            used = 0;

            return lineParsed;
        }

        public void Dispose() { reader.Dispose(); }

        public int GetLine() { return currentLineNumber; }
    }

    class Builder {

        static Builder instance;
        LineParser parser;
        //Dictionary<string, Gate> gates = new Dictionary<string, Gate>();
        public Dictionary<string, INotifiable> connections = new Dictionary<string, INotifiable>();
        protected Builder(LineParser parser)
        {
            this.parser = parser
                .AddSeparator("\r", true)
                //.AddSeparator(".", false) @TODO
                .AddSeparator(";", false)
                .AddSeparator("->", false)
                .AddSeparator("end", false);
 
            //gates = new Dictionary<string, Gate>();
            connections = new Dictionary<string, INotifiable>();
        }

        public static Builder GetInstance(LineParser parser)
        {
            if (instance == null)
                instance = new Builder(parser);

            return instance;
        }

        public static Builder GetInstance()
        {
            if (instance == null)
                throw new InvalidOperationException("Builder was not yet initialized.");

            return instance;
        }


        public void Build()
        {


            string[] parsed;
            while ((parsed = parser.SplitLine()) != null)
            {

                // skip comments
                if (parsed[0].Equals(";"))
                    continue;

                IBuildable circuit;

                switch (parsed[0])
                {
                    case "gate":
                        if (parsed.Length != 2) // gate $name
                            throw new SyntaxErrorException(parser.GetLine());

                        circuit = new SimpleGate(parsed[1]);
                        break;

                    case "network":
                        if (parsed.Length != 1) // network
                            throw new SyntaxErrorException(parser.GetLine());

                        circuit = new Network();
                        break;

                    default:
                        throw new SyntaxErrorException(parser.GetLine());
                }

                circuit.Build(parser);
                circuit.Register("_");

            }
        }

    }

    interface IRegisterable {
        string Name { get; set; }
        void Register(string parentNamespace);
    }
    interface IBuildable : IRegisterable, ICloneable {

        void Build(LineParser parser);
    }

    interface INotifiable : IRegisterable, ICloneable {

        SignalValues Val { get; set; }

        void Notify(SignalValues val);

        void AddListener(INotifiable listener);
    }

    abstract class Gate : IBuildable, INotifiable {

        public string Name { get; set; }
        public string FullName { get; set; }

        public SignalValues Val { get; set; }

        public INotifiable[] outputs;
        public INotifiable[] inputs;

        public Function<SignalValues[]> f;

        protected LineParser parser;

        public Gate(String name)
        {
            this.Name = name;
        }

        public void Notify(SignalValues val)
        {
            if (Network.Iterations >= 1000000)
                return;
            Network.Iterations++;

            int[] coord = new int[inputs.Length];
            for (int i = 0; i < coord.Length; i++)
                coord[i] = (int)inputs[i].Val;

            SignalValues[] result = f.Get(coord);

            if (result == null)
            {
                SignalValues scalarResult = SignalValues.zero;
                for (int i = 0; i < coord.Length; i++)
                {
                    if (inputs[i].Val == SignalValues.unknown)
                    {
                        scalarResult = SignalValues.unknown;
                        break;
                    }
                }

                for (int j = 0; j < outputs.Length; j++)
                    outputs[j].Notify(scalarResult);

                return;
            }

            for (int i = 0; i < result.Length; i++)
                outputs[i].Notify(result[i]);
        }

        protected void buildInputs()
        {
            string[] inpArr = parser.SplitLine();

            if (!inpArr[0].Equals("inputs"))
                throw new MissingKeywordException(parser.GetLine());

            inputs = new INotifiable[inpArr.Length - 1];
            for (int i = 1; i < inpArr.Length; i++)
                inputs[i - 1] = new Connection(inpArr[i]);
        }

        protected void buildOutputs()
        {
            string[] outArr = parser.SplitLine();

            if (!outArr[0].Equals("outputs"))
                throw new MissingKeywordException(parser.GetLine());
            else if (outArr.Length < 2)
                throw new SyntaxErrorException(parser.GetLine());

            outputs = new INotifiable[outArr.Length - 1];
            for (int i = 1; i < outArr.Length; i++)
                outputs[i - 1] = new Connection(outArr[i]);
        }

        public abstract void Build(LineParser parser);

        public abstract object Clone();

        public void Register()
        {
            string wholeNameSpace = Name;
            Builder.GetInstance().connections.Add(wholeNameSpace, this);

            for (int i = 0; i < inputs.Length; i++)
                inputs[i].Register(wholeNameSpace);

            for (int i = 0; i < outputs.Length; i++)
                outputs[i].Register(wholeNameSpace);
        }

        public void Register(string parentNamespace)
        {
            string wholeNameSpace = parentNamespace + "." + Name;
            FullName = wholeNameSpace;
            Builder.GetInstance().connections.Add(wholeNameSpace, this);

            for (int i = 0; i < inputs.Length; i++)
                inputs[i].Register(wholeNameSpace);

            for (int i = 0; i < outputs.Length; i++)
                outputs[i].Register(wholeNameSpace);
        }


        public void AddListener(INotifiable listener)
        {
            throw new NotImplementedException();
        }
    }

    class SimpleGate : Gate {
        public SimpleGate(string name) : base(name) { }

        protected void buildFunction()
        {
            if (inputs.Length == 0)
                f = new ConstantSignalFunction();
            else
                f = new SignalFunction(inputs.Length, 3);

            string[] functionDef;

            while ((functionDef = parser.SplitLine())[0] != "end")
            {
                int[] functionInputs = new int[inputs.Length];
                SignalValues[] functionOutputs = new SignalValues[outputs.Length];

                if (functionDef.Length != inputs.Length + outputs.Length)
                    throw new SyntaxErrorException(parser.GetLine());

                for (int i = 0; i < inputs.Length; i++)
                    functionInputs[i] = (int)Helper.ConvertToSignalValue(functionDef[i]);

                for (int i = inputs.Length; i < inputs.Length + outputs.Length; i++)
                    functionOutputs[i - inputs.Length] = Helper.ConvertToSignalValue(functionDef[i]);

                f.Set(functionInputs, functionOutputs);

            }
        }

        public override sealed void Build(LineParser parser)
        {
            this.parser = parser;

            buildInputs();

            buildOutputs();

            buildFunction();
        }

        public override object Clone()
        {
            SimpleGate clone = new SimpleGate(Name);

            clone.inputs = new INotifiable[inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                clone.inputs[i] = (INotifiable)inputs[i].Clone();
                clone.inputs[i].AddListener(clone);
            }

            clone.outputs = new INotifiable[outputs.Length];

            for (int i = 0; i < outputs.Length; i++)
                clone.outputs[i] = (INotifiable)outputs[i].Clone();

            clone.f = f;

            return clone;
        }
    }

    class Network : Gate {

        public static int Iterations = 0;

        public Network() : base("network") { }

        protected void buildGate(string[] definition)
        {
            if (definition.Length != 3)
                throw new SyntaxErrorException(parser.GetLine());

            INotifiable gate = (INotifiable)Builder.GetInstance().connections["_." + definition[2]].Clone();
            gate.Name = definition[1];
            gate.Register(Name);
        }

        protected void buildConnection(string[] definition)
        {
            Builder b = Builder.GetInstance();

            INotifiable to = b.connections["network." + definition[0]];
            INotifiable from = b.connections["network." + definition[2]];

            from.AddListener(to);
        }

        public override sealed void Build(LineParser parser)
        {
            this.parser = parser;

            buildInputs();

            buildOutputs();

            string[] definition;
            while ((definition = parser.SplitLine())[0] == "gate")
                buildGate(definition);

            Register();

            do
            {
                buildConnection(definition);
            } while ((definition = parser.SplitLine())[0] != "end");
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public int Evaluate(SignalValues[] input)
        {
            if (input.Length != inputs.Length)
                throw new SyntaxInputException();

            for (int i = 0; i < input.Length; i++)
                inputs[i].Notify(input[i]);

            return Iterations;
        }
    }

    class Connection : INotifiable {


        public string Name { get; set; }

        public string FullName { get; set; }

        public SignalValues Val { get; set; }

        public Connection(string name)
        {
            this.Name = name;
            connections = new INotifiable[2];
        }

        INotifiable[] connections;
        int usedConnections;

        public void Notify(SignalValues val)
        {
            if (this.Val == val)
                return;

            this.Val = val;

            for (int i = 0; i < usedConnections; i++)
            {
                connections[i].Notify(val);
            }
        }

        public object Clone()
        {
            return new Connection(Name);
        }

        public void Register(string parentNamespace)
        {
            FullName = parentNamespace + "." + Name;
            Builder.GetInstance().connections.Add(parentNamespace + "." + Name, this);
        }

        public void AddListener(INotifiable listener)
        {
            // possible increase
            if (usedConnections >= connections.Length - 1)
            {
                INotifiable[] nconn = new INotifiable[connections.Length * 2 + 2];
                for (int i = 0; i < connections.Length; i++)
                    nconn[i] = connections[i];
                connections = nconn;
            }

            connections[usedConnections++] = listener;
        }
    }

    class SyntaxInputException : Exception { }
    abstract class FileInputFormatException : Exception {

        int line;
        public FileInputFormatException(int line)
        {
            this.line = line;
        }
        public override string ToString() { return String.Format("Line {0}: ", line); }
    }
    class DuplicateException : FileInputFormatException {
        public DuplicateException(int line) : base(line) { }
        public override String ToString() { return base.ToString() + "Duplicate."; }
    }
    class MissingKeywordException : FileInputFormatException {
        public MissingKeywordException(int line) : base(line) { }
        public override String ToString() { return base.ToString() + "Missing keyword."; }
    }
    class BindingRuleException : FileInputFormatException {
        public BindingRuleException(int line) : base(line) { }
        public override String ToString() { return base.ToString() + "Binding rule."; }
    }
    class SyntaxErrorException : FileInputFormatException {
        public SyntaxErrorException(int line) : base(line) { }
        public override String ToString() { return base.ToString() + "Syntax error."; }
    }

    class Program {
        public static void Main(string[] args) { Run(args, Console.Out, Console.In); }

        public static void Run(string[] args, TextWriter writer, TextReader reader)
        {
            if (args.Length != 1)
            {
                writer.WriteLine(ReportArgumentError());
                return;
            }

            try
            {
                LineParser lp = new LineParser(File.OpenText(args[0]));
                Builder builder = Builder.GetInstance(lp);

                builder.Build();

                Network network = (Network)builder.connections["network"];

                LineParser lpInput = new LineParser(reader);
                string[] input; SignalValues[] signalInput;
                while ((input = lpInput.SplitLine())[0] != "end")
                {
                    try
                    {
                        signalInput = new SignalValues[input.Length];
                        for (int i = 0; i < input.Length; i++)
                            signalInput[i] = Helper.ConvertToSignalValue(input[i]);

                        Network.Iterations = 0;
                        int iterations = network.Evaluate(signalInput);

                        writer.Write(iterations);

                        for (int i = 0; i < network.outputs.Length; i++)
                            writer.Write(" " + Helper.ConvertFromSignalValue(network.outputs[i].Val));

                        writer.WriteLine();
                    }
                    catch (SyntaxInputException) { writer.WriteLine(ReportInputError()); continue; }
                    catch (InvalidDataException) { writer.WriteLine(ReportInputError()); continue; }
                }

            }
            catch (IOException) { writer.WriteLine(ReportFileError()); }
            catch (UnauthorizedAccessException) { writer.WriteLine(ReportFileError()); }
            catch (System.Security.SecurityException) { writer.WriteLine(ReportFileError()); }
            catch (FileInputFormatException e) { writer.WriteLine(e); }
            finally
            {
                reader.Dispose();
                writer.Dispose();
            }
        }

        static string ReportArgumentError() { return "Argument error."; }
        static string ReportFileError() { return "File error."; }
        static string ReportInputError() { return "Syntax error."; }
    }

}
