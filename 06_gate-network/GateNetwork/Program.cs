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

    class SignalValueFunction : Function<SignalValues[]> {

        public virtual SignalValues[] Get(Connection[] input, int mOutputs)
        {

            bool isThereUnknownValue = false;
            int[] coord = new int[input.Length];

            for (int i = 0; i < coord.Length; i++)
            {
                coord[i] = (int)input[i].Value;

                if (input[i].Value == SignalValues.unknown)
                    isThereUnknownValue = true;
            }

            SignalValues[] result = this.Get(coord);

            if (result != null)
                return result;

            result = new SignalValues[mOutputs];

            for (int i = 0; i < result.Length; i++)
                result[i] = isThereUnknownValue ? SignalValues.unknown : SignalValues.zero;

            return result;
        }

    }

    class MultiDimensionalSignalFunction : SignalValueFunction {

        int nInputs;

        public MultiDimensionalSignalFunction(int nInputs, int possibleValues)
        {
            this.nInputs = nInputs;

            int[] lengths = new int[nInputs];
            for (int i = 0; i < nInputs; i++)
                lengths[i] = possibleValues;

            data = Array.CreateInstance(typeof(SignalValues[]), lengths);
        }
    }

    class ScalarSignalFunction : SignalValueFunction {

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
        bool canReturnEmpty;
        bool skipComments;

        public LineParser(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.reader = reader;
            this.cache = new string[4];
            this.canReturnEmpty = false;
            this.skipComments = false;
        }

        public LineParser SetSkipComments(bool skip) {
            this.skipComments = skip;
            return this;
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
            separators = nsep;
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

                // "end" only at the beggining
                // ~ not a separator if followed by a valid character
                if (i - 3 >= 0 && separator != null && separator.representation == "end" &&
                    null == isSeparator(line[i - 3].ToString())
                    )
                    continue;

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

            if (used == 0 && !canReturnEmpty)
                return SplitLine();

            if (skipComments && cache[0] == ";")
            {
                used = 0;
                return SplitLine();
            }

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

        public void CanReturnEmptyLine(bool can)
        {
            canReturnEmpty = can;
        }
    }

    abstract class Mockup {

        protected string name;

        protected LineParser parser;

        protected string[] inputs;

        protected string[] outputs;

        public Mockup(string name, LineParser parser)
        {
            this.name = name;
            this.parser = parser;
        }

        public string GetName() { return name; }

        public abstract void Build();

        public abstract Gate Instantiate(Network network);

        protected void checkDuplicateNames(string[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals("."))
                    throw new SyntaxErrorException(parser.GetLine());

                for (int j = 0; j < a.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (a[i].Equals(a[j]))
                        throw new DuplicateException(parser.GetLine());
                }
            }
        }

        protected virtual void buildInputs()
        {
            string[] s = parser.SplitLine();

            if (!s[0].Equals("inputs"))
                throw new MissingKeywordException(parser.GetLine());

            inputs = new string[s.Length - 1];
            for (int i = 1; i < s.Length; i++)
                inputs[i - 1] = s[i];

            checkDuplicateNames(inputs);
        }

        protected void buildOutputs()
        {
            string[] s = parser.SplitLine();

            if (!s[0].Equals("outputs"))
                throw new MissingKeywordException(parser.GetLine());
            else if (s.Length < 2)
                throw new SyntaxErrorException(parser.GetLine());

            outputs = new string[s.Length - 1];
            for (int i = 1; i < s.Length; i++)
                outputs[i - 1] = s[i];

            checkDuplicateNames(outputs);
        }

        protected void instantiateInputsOutputs(Gate instance, Dictionary<string, INotifiable> connections, bool routeInputes)
        {

            Connection[] inputs = new Connection[this.inputs.Length];
            Connection[] outputs = new Connection[this.outputs.Length];

            Connection temp;

            for (int i = 0; i < this.inputs.Length; i++)
            {
                temp = routeInputes
                    ? new Connection(this.inputs[i], instance)
                    : new Connection(this.inputs[i]);

                connections.Add(this.inputs[i], temp);
                inputs[i] = temp;
            }

            for (int i = 0; i < this.outputs.Length; i++)
            {
                temp = new Connection(this.outputs[i]);
                if (routeInputes)
                {
                    temp.SetIsOutput(true);  
                } 
                connections.Add(this.outputs[i], temp);
                outputs[i] = temp;
            }

            instance.SetInputsOutputs(inputs, outputs);

        }

    }

    class FunctionGateMockup : Mockup {

        SignalValueFunction f;

        public FunctionGateMockup(string name, LineParser parser) : base(name, parser) { }

        public override void Build()
        {
            buildInputs();

            buildOutputs();

            buildFunction();
        }

        void buildFunction()
        {
            if (inputs.Length == 0)
                f = new ScalarSignalFunction();
            else
                f = new MultiDimensionalSignalFunction(inputs.Length, 3);

            string[] s;

            while ((s = parser.SplitLine())[0] != "end")
            {
                int[] functionInputs = new int[inputs.Length];
                SignalValues[] functionOutputs = new SignalValues[outputs.Length];

                if (s.Length != inputs.Length + outputs.Length)
                    throw new SyntaxErrorException(parser.GetLine());

                for (int i = 0; i < inputs.Length; i++)
                    functionInputs[i] = (int)Helper.ConvertToSignalValue(s[i]);

                for (int i = inputs.Length; i < inputs.Length + outputs.Length; i++)
                    functionOutputs[i - inputs.Length] = Helper.ConvertToSignalValue(s[i]);

                if (f.Get(functionInputs) != null)
                    throw new DuplicateException(parser.GetLine());

                f.Set(functionInputs, functionOutputs);

            }
        }

        public override Gate Instantiate(Network network)
        {
            var instance = new FunctionGate(name, f, network);

            Dictionary<string, INotifiable> connections = new Dictionary<string, INotifiable>();

            instance.SetConnections(connections);

            instantiateInputsOutputs(instance, connections, true);

            return instance;
        }
    }

    class CompositeGateMockup : Mockup {

        protected Dictionary<string, Mockup> mockups;
        protected Dictionary<string, Mockup> subgates;
        protected Connection[] connections;

        protected struct Connection {

            public string[] from;
            public string[] to;

            public Connection(string[] to, string[] from)
            {
                this.from = from;
                this.to = to;
            }

        }

        public CompositeGateMockup(string name, LineParser parser, Dictionary<string, Mockup> mockups)
            : base(name, parser)
        {
            this.mockups = mockups;
            this.connections = new Connection[4];
        }

        public override void Build()
        {
            buildInputs();

            buildOutputs();

            string[] s;
            while ((s = parser.SplitLine())[0] == "gate")
                buildGate(s);

            if (subgates == null) // no subgates
                throw new MissingKeywordException(parser.GetLine());

            if (s[0] == "end") // no bindings
                throw new BindingRuleException(parser.GetLine());

            List<string[]> connectionsBuffer = new List<string[]>();

            try
            {
                do { connectionsBuffer.Add(s); }
                while ((s = parser.SplitLine())[0] != "end");
            }
            catch (NullReferenceException) { throw new MissingKeywordException(parser.GetLine()); }

            buildConnections(connectionsBuffer);

            checkConnections();
        }

        protected override void buildInputs()
        {
            string[] s = parser.SplitLine();

            if (!s[0].Equals("inputs"))
                throw new MissingKeywordException(parser.GetLine());

            inputs = new string[s.Length - 1 + 2];
            inputs[0] = "0";
            inputs[1] = "1";
            for (int i = 1; i < s.Length; i++)
                inputs[i - 1 + 2] = s[i];

            checkDuplicateNames(inputs);
        }

        protected void buildGate(string[] s)
        {
            if (subgates == null)
                subgates = new Dictionary<string, Mockup>();

            if (s.Length != 3)
                throw new SyntaxErrorException(parser.GetLine());

            if (!mockups.ContainsKey(s[2]))
                throw new SyntaxErrorException(parser.GetLine());

            if (subgates.ContainsKey(s[1]))
                throw new DuplicateException(parser.GetLine());

            subgates.Add(s[1], mockups[s[2]]);
        }

        protected void buildConnections(List<string[]> arr)
        {
            connections = new Connection[arr.Count];

            string[] s;
            for (int i = 0; i < arr.Count; i++)
            {
                s = arr[i];

                if (s.Length != 3 && s.Length != 5 && s.Length != 7)
                    throw new SyntaxErrorException(parser.GetLine());

                if (s.Length == 3 && s[1] == "->")
                    throw new BindingRuleException(parser.GetLine() - arr.Count + i);
                //connections[i] = new Connection(new[] { s[0] }, new[] { s[2] });
                else if (s.Length == 5 && s[1] == "->")
                    connections[i] = new Connection(new[] { s[0] }, new[] { s[2], s[4] });
                else if (s.Length == 5 && s[3] == "->")
                    connections[i] = new Connection(new[] { s[0], s[2] }, new[] { s[4] });
                else if (s.Length == 7 && s[3] == "->")
                    connections[i] = new Connection(new[] { s[0], s[2] }, new[] { s[4], s[6] });
                else
                    throw new SyntaxErrorException(parser.GetLine());
            }

        }

        protected virtual void checkConnections(){
            int seen;

            // output has exactly one connection
            for (int i = 0; i < outputs.Length; i++)
            {
                seen = 0;

                for (int j = 0; j < connections.Length; j++)
                    if (connections[j].to[0] == outputs[i])
                        seen++;

                if (seen != 1)
                    throw new BindingRuleException(parser.GetLine());
            }
        }

        public override Gate Instantiate(Network network)
        {
            var instance = new CompositeGate(name, network);

            Dictionary<string, INotifiable> connections = new Dictionary<string, INotifiable>();

            instance.SetConnections(connections);

            instantiateInputsOutputs(instance, connections, false);

            instantiateSubGates(network, instance, connections);

            instantiateConnections(instance, connections);

            return instance;
        }

        protected void instantiateSubGates(Network network, CompositeGate instance, Dictionary<string, INotifiable> connections)
        {
            Dictionary<string, Gate> subgates = new Dictionary<string, Gate>();

            foreach (KeyValuePair<string, Mockup> item in this.subgates){
                Gate g = item.Value.Instantiate(network);
                connections.Add(item.Key, g);
                g.FullName = item.Key;
            }

        }

        protected void instantiateConnections(CompositeGate instance, Dictionary<string, INotifiable> connections)
        {

            foreach (Connection item in this.connections)
            {

                INotifiable from = connections[item.from[0]];
                if (item.from.Length == 2)
                    from = ((Gate)from).connections[item.from[1]];

                INotifiable to = connections[item.to[0]];
                if (item.to.Length == 2)
                    to = ((Gate)to).connections[item.to[1]];

                from.AddListener(to);
            }

        }
    }

    class NetworkGateMockup : CompositeGateMockup {
        public NetworkGateMockup(string name, LineParser parser, Dictionary<string, Mockup> mockups)
            : base(name, parser, mockups) { }

        protected override void buildInputs()
        {
            base.buildInputs();

            if (inputs.Length == 0 + 2) // network must have >= 1 input
                throw new SyntaxErrorException(parser.GetLine());
        }
        
        protected override void checkConnections()
        {
            base.checkConnections();

            int seen;

            // input has one or more connections
            for (int i = 0 + 2; i < inputs.Length; i++)
            {
                seen = 0;

                for (int j = 0; j < connections.Length; j++)
                    if (connections[j].from[0] == inputs[i])
                        seen++;

                if (seen == 0)
                    throw new BindingRuleException(parser.GetLine());
            }
        }

        public override Gate Instantiate(Network network)
        {
            // network is null !

            var instance = network = new Network();

            Dictionary<string, INotifiable> connections = new Dictionary<string, INotifiable>();

            instance.SetConnections(connections);

            instantiateInputsOutputs(instance, connections, false);

            instantiateSubGates(instance, instance, connections);

            instantiateConnections(instance, connections);

            return instance;
        }
    }

    class Builder {

        LineParser parser;

        Dictionary<string, Mockup> mockups;

        public Network network;

        public Builder(LineParser parser)
        {
            this.parser = parser
                .AddSeparator("\r", true)
                .AddSeparator(".", false)
                .AddSeparator(";", false)
                .AddSeparator("->", false)
                .AddSeparator("end", false);

            this.parser.SetSkipComments(true);

            mockups = new Dictionary<string, Mockup>();
        }

        public Network Build()
        {
            NetworkGateMockup networkMockup = null;

            string[] parsed;
            while ((parsed = parser.SplitLine()) != null)
            {
                // skip comments
                if (parsed[0].Equals(";"))
                    continue;

                Mockup mockup;

                switch (parsed[0])
                {
                    case "gate":
                        if (parsed.Length != 2) // instance $name
                            throw new SyntaxErrorException(parser.GetLine());

                        mockup = new FunctionGateMockup(parsed[1], parser);
                        break;

                    case "composite":
                        if (parsed.Length != 2) // composite $name
                            throw new SyntaxErrorException(parser.GetLine());

                        mockup = new CompositeGateMockup(parsed[1], parser, mockups);
                        break;

                    case "network":
                        if (parsed.Length != 1) // network
                            throw new MissingKeywordException(parser.GetLine());

                        if (mockups.Count == 0) // no gates
                            throw new MissingKeywordException(parser.GetLine());

                        if (networkMockup != null)
                            throw new DuplicateException(parser.GetLine());

                        mockup = networkMockup = new NetworkGateMockup("network", parser, mockups);
                        break;

                    default:
                        throw new SyntaxErrorException(parser.GetLine());
                }

                if (mockups.ContainsKey(mockup.GetName()))
                    throw new DuplicateException(parser.GetLine());

                mockup.Build();

                mockups.Add(mockup.GetName(), mockup);

            }

            parser.Dispose();

            if (networkMockup == null) // network not found
                throw new MissingKeywordException(parser.GetLine());

            return (Network)networkMockup.Instantiate(null);

        }

    }

    interface INotifiable {

        void Notify(SignalValues val);

        void AddListener(INotifiable listener);
    }

    abstract class Gate : INotifiable {

        public string Name { get; set; }

        public string FullName { get; set; }

        public override string ToString()
        {
            return FullName;
        }

        protected Connection[] inputs;

        protected Connection[] outputs;

        public Dictionary<string, INotifiable> connections;

        protected Network network;

        public Gate(String name, Network network)
        {
            this.Name = name;
            this.network = network;
        }

        public virtual void Notify(SignalValues val)
        {
            throw new NotImplementedException();
        }

        public void SetConnections(Dictionary<string, INotifiable> connections)
        {
            this.connections = connections;
        }

        public void SetNetwork(Network network)
        {
            this.network = network;
        }

        public virtual void SetInputsOutputs(Connection[] inputs, Connection[] outputs)
        {
            this.inputs = inputs;
            this.outputs = outputs;
        }

        public void AddListener(INotifiable listener)
        {
            throw new NotImplementedException();
        }
    }

    class FunctionGate : Gate {

        SignalValueFunction f;

        bool isEnqueued;

        SignalValues[] result;

        public FunctionGate(string name, SignalValueFunction f, Network network)
            : base(name, network)
        {
            this.f = f;
            //this.isEnqueued = false;
        }

        public override void SetInputsOutputs(Connection[] inputs, Connection[] outputs)
        {
            base.SetInputsOutputs(inputs, outputs);

            if (inputs.Length == 0)
                Notify(SignalValues.unknown);
        }

        public override void Notify(SignalValues val)
        {
            if (!isEnqueued)
            {
                isEnqueued = true;
                network.nextQueue.Enqueue(this);
            }
        }

        public void Evaluate()
        {
            isEnqueued = false;

            result = f.Get(inputs, outputs.Length);

            //for (int i = 0; i < result.Length; i++)
            //    outputs[i].Notify(result[i]);
        }

        public void NotifyResult() {
            for (int i = 0; i < result.Length; i++)
                outputs[i].Notify(result[i]);
        }

    }

    class CompositeGate : Gate {

        public CompositeGate(string name, Network network) : base(name, network) { }

    }

    class Network : CompositeGate {

        int iterations = 0;

        bool initConstants;

        public Queue<FunctionGate> queue;
        public Queue<FunctionGate> nextQueue;
        public Queue<FunctionGate> constantGates;

        public Network()
            : base("network", null)
        {
            network = this;
            nextQueue = new Queue<FunctionGate>();
            initConstants = true;
        }

        public int Evaluate(SignalValues[] input)
        {
            iterations = 0;
            initConstants = true;

            if (input.Length != inputs.Length - 2)
                throw new SyntaxInputException();

            FunctionGate temp;

            if (initConstants && constantGates == null)
            {
                constantGates = nextQueue;
                nextQueue = new Queue<FunctionGate>();
                if (initConstants)
                {
                    while (constantGates.Count > 0)
                    {
                        temp = constantGates.Dequeue();
                        temp.Evaluate();
                        temp.NotifyResult();
                    }
                    initConstants = false;
                }
            }

            inputs[0].Notify(SignalValues.zero);
            inputs[1].Notify(SignalValues.one);
            for (int i = 0; i < input.Length; i++)
                inputs[i + 2].Notify(input[i]);

            while (iterations < 1000000 && nextQueue.Count > 0)
            {
                queue = nextQueue;
                nextQueue = new Queue<FunctionGate>();

                var notifyQueue = new Queue<FunctionGate>();

                while (queue.Count > 0)
                {
                    temp = queue.Dequeue();
                    temp.Evaluate();
                    notifyQueue.Enqueue(temp);
                }

                while (notifyQueue.Count > 0)
                {
                    temp = notifyQueue.Dequeue();
                    temp.NotifyResult();
                }

                iterations++;
            }

            queue = null;
            //nextQueue = new Queue<FunctionGate>();

            return iterations;
        }

        public Connection[] GetOutputs()
        {
            return outputs;
        }
    }

    class Connection : INotifiable {

        public string Name { get; set; }

        INotifiable[] connections;

        int usedConnections;

        bool isOutput;

        public SignalValues Value { get; set; }

        public Connection(string name)
        {
            this.Name = name;
            connections = new INotifiable[2];
            //isOutput = false;
        }

        public Connection(string name, INotifiable to)
        {
            this.Name = name;
            connections = new INotifiable[1];
            connections[0] = to;
            usedConnections++;
            //isOutput = true;
        }

        public void SetIsOutput(bool isOutput){
            this.isOutput = isOutput;
        }

        public void Notify(SignalValues val)
        {
            //if (!isOutput && this.Value == val)
            if(this.Value == val)
                return;

            this.Value = val;

            for (int i = 0; i < usedConnections; i++)
                connections[i].Notify(val);
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
                Builder builder = new Builder(lp);

                Network network = builder.Build();

                LineParser lpInput = new LineParser(reader);
                lpInput.CanReturnEmptyLine(true);

                string[] input; SignalValues[] signalInput;

                while (true)
                {
                    try
                    {
                        input = lpInput.SplitLine();
                        if (input == null || input.Length == 0)
                            throw new SyntaxInputException();

                        if (input[0] == "end")
                            break;


                        signalInput = new SignalValues[input.Length];
                        for (int i = 0; i < input.Length; i++)
                            signalInput[i] = Helper.ConvertToSignalValue(input[i]);

                        int iterations = network.Evaluate(signalInput);

                        writer.Write(iterations);

                        for (int i = 0; i < network.GetOutputs().Length; i++)
                            writer.Write(" " + Helper.ConvertFromSignalValue(network.GetOutputs()[i].Value));

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
