using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

// Nejasnosti:
//	- muze mit brana vstup a vystup stejneho jmena?
//		(musi byt vstupy a vystupy disjunktni?)
//	
//	- muzou se konstanty napojovat rovnou na vystup?

namespace Hradla {
    static class Logger {
        //		public static bool LOG = true;
        public static bool LOG = false;
        public static void Log(string format, params object[] args)
        {
            if (LOG) Console.WriteLine(format, args);
        }

        public static void Log(Exception e)
        {
            if (LOG) Console.WriteLine(e);
        }
    }

    static class Util {
        public static bool In<T>(T[] ary, T thing)
        {
            foreach (T item in ary) if (EqualityComparer<T>.Default.Equals(item, thing)) return true;
            return false;
        }
        public static int Index<T>(T[] ary, T thing)
        {
            for (int i = 0; i < ary.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(ary[i], thing)) return i;
            }
            throw new InternalException("No such index");
        }
        // O(n^2)
        public static bool Unique<T>(T[] ary)
        {
            for (int i = 0; i < ary.Length; i++)
            {
                for (int j = i + 1; j < ary.Length; j++)
                {
                    if (EqualityComparer<T>.Default.Equals(ary[i], ary[j])) return false;
                }
            }
            return true;
        }
    }

    class InternalException : Exception {
        public InternalException() { }
        public InternalException(string msg) : base(msg) { }
    }
    class NetworkFileUnreadable : Exception { }
    class ParseError : Exception {
        private long line;
        private string outputMessage;

        public ParseError(long line, string outputMessage, Exception innerEx, string message)
            : base(message, innerEx)
        {
            this.line = line;
            this.outputMessage = outputMessage;
        }

        public ParseError(long line, string outputMessage, Exception innerEx) : this(line, outputMessage, innerEx, null) { }

        public string ToOutputString()
        {
            return String.Format("Line {0}: {1}", line, outputMessage);
        }
    }
    class SyntaxError : ParseError {
        public SyntaxError(long line, string message) : base(line, "Syntax error.", null, message) { }
        public SyntaxError(long line, Exception innerEx) : base(line, "Syntax error.", innerEx) { }
    }
    class MissingKeyword : ParseError {
        private static readonly string MSG = "Missing keyword.";
        public MissingKeyword(long line, string message) : base(line, MSG, null, message) { }
        public MissingKeyword(long line, Exception innerEx) : base(line, MSG, innerEx) { }
    }
    class DuplicateError : ParseError {
        private static readonly string MSG = "Duplicate.";
        public DuplicateError(long line, string message) : base(line, MSG, null, message) { }
        public DuplicateError(long line, Exception innerEx) : base(line, MSG, innerEx) { }
    }
    class BindingRuleError : ParseError {
        private static readonly string MSG = "Binding rule.";
        public BindingRuleError(long line, string message) : base(line, MSG, null, message) { }
        public BindingRuleError(long line, Exception innerEx) : base(line, MSG, innerEx) { }
    }
    class UnknownGate : Exception { }
    class DuplicateGate : Exception { }
    class DuplicateGateDefinition : Exception { }
    class ArityMismatchError : Exception {
        public ArityMismatchError() { }
        public ArityMismatchError(string msg) : base(msg) { }
    }
    class BindingRulesViolated : Exception {
        public BindingRulesViolated() { }
        public BindingRulesViolated(string msg) : base(msg) { }
    };
    class ChargeParseError : Exception { }
    class EOF : Exception { }
    class DoubleBehaviorSpecException : Exception { }
    class InvalidNameException : Exception { }
    class PinAlreadyOccupied : Exception {
        public PinAlreadyOccupied() { }
        public PinAlreadyOccupied(string msg) : base(msg) { }
    }

    enum Charge { HIGH = 0, LOW = 1, UNDEFINED = 2, _COUNT = 3 };

    class ParserState {
        private StreamReader reader;
        private long currentLine = 0;

        public ParserState(StreamReader reader)
        {
            this.reader = reader;
        }

        public long CurrentLine
        {
            get
            {
                return currentLine;
            }
        }

        private string peekLine = null;

        public string PeekLine()
        {
            if (peekLine == null)
            {
                do
                {
                    peekLine = reader.ReadLine();
                    currentLine++;
                } while (peekLine != null &&
                    (
                        peekLine.Trim().Length == 0 ||
                        peekLine[0] == ';'
                    )
                );
                if (peekLine == null)
                {
                    Logger.Log("Cannot peek a new line. EOF reached.");
                    throw new EOF();
                }
            }
            return peekLine;
        }

        public void NextLine()
        {
            peekLine = null;
        }

        public void SyntaxError(string message) { throw new SyntaxError(currentLine, message); }
        public void SyntaxError(Exception innerEx) { throw new SyntaxError(currentLine, innerEx); }
        public void MissingKeyword(string message) { throw new MissingKeyword(currentLine, message); }
        public void MissingKeyword(Exception innerEx) { throw new MissingKeyword(currentLine, innerEx); }
        public void DuplicateError(string message) { throw new DuplicateError(currentLine, message); }
        public void DuplicateError(Exception innerEx) { throw new DuplicateError(currentLine, innerEx); }
        public void BindingRuleError(Exception innerEx) { throw new BindingRuleError(currentLine, innerEx); }
        public void BindingRuleError(string message) { throw new BindingRuleError(currentLine, message); }

        public static readonly char[] SPACE = new char[] { ' ' };

        public string[] SplitLine()
        {
            return PeekLine().Split(SPACE);
        }

        public void SplitCommand(out string command, out string[] args)
        {
            string[] split = SplitLine();
            command = split[0];
            args = new string[split.Length - 1]; // TODO: there must be a method for this
            for (int i = 1; i < split.Length; i++)
            {
                args[i - 1] = split[i];
            }
        }

        public void ReadCommand(out string command)
        {
            string[] _ignoreArgs;
            SplitCommand(out command, out _ignoreArgs);
        }

        public void ExpectCommand(string command, out string[] args)
        {
            string read;
            SplitCommand(out read, out args);
            if (read != command)
            {
                SyntaxError("Expecting command '" + command + "', got '" + read + "'");
            }
        }

        public void ExpectBareCommand(string command)
        {
            string[] args;
            ExpectCommand(command, out args);
            if (args.Length > 0) SyntaxError("Command '" + command + "' expects no arguments.");
        }

        public void ExpectCommand(string command, int arguments, out string[] args)
        {
            string read;
            SplitCommand(out read, out args);
            if (read != command)
            {
                SyntaxError("Expecting command '" + command + "', got '" + read + "'");
            }

            if (args.Length != arguments)
            {
                SyntaxError("Command '" + command + "' takes " + arguments.ToString() + " arguments.");
            }
        }
    }

    static class ChargeUtil {
        public static Charge Parse(string charge)
        {
            if (charge.Length != 1) throw new ChargeParseError();
            switch (charge[0])
            {
                case '0': return Charge.LOW;
                case '1': return Charge.HIGH;
                case '?': return Charge.UNDEFINED;
                default: throw new ChargeParseError();
            }
        }

        public static string ToOutputString(Charge charge)
        {
            switch (charge)
            {
                case Charge.LOW: return "0";
                case Charge.HIGH: return "1";
                case Charge.UNDEFINED: return "?";
                default: throw new InternalException();
            }
        }
    }

    class GateDefinitionBuilder {
        private string name;
        private string[] inputs;
        private string[] outputs;
        private Dictionary<ChargeVector, ChargeVector> behavior;

        public GateDefinitionBuilder()
        {
            inputs = null;
            outputs = null;
            behavior = new Dictionary<ChargeVector, ChargeVector>(new ChargeVectorComparer());
        }

        public string[] Inputs
        {
            set
            {
                if (inputs != null)
                {
                    throw new InternalException();
                }
                if (!Util.Unique(value))
                {
                    throw new DoubleBehaviorSpecException();
                }
                if (!ValidNames(value))
                {
                    throw new InvalidNameException();
                }
                Logger.Log("Gate inputs: [{0}]", string.Join(", ", value));
                inputs = value;
            }
        }

        public string[] Outputs
        {
            set
            {
                if (outputs != null)
                {
                    throw new InternalException();
                }
                if (!Util.Unique(value))
                {
                    throw new DoubleBehaviorSpecException();
                }
                if (!ValidNames(value))
                {
                    throw new InvalidNameException();
                }
                Logger.Log("Gate outputs: [{0}]", string.Join(", ", value));
                outputs = value;
            }
        }

        public void AddBehavior(ChargeVector key, ChargeVector value)
        {
            if (key.Size != inputs.Length)
            {
                throw new InternalException("Wrong behavior key size");
            }
            if (value.Size != outputs.Length)
            {
                throw new InternalException("Wrong behavior value size");
            } if (behavior.ContainsKey(key))
            {
                throw new DoubleBehaviorSpecException();
            }
            behavior.Add(key, value);
        }

        public void AddBehaviorVector(ChargeVector behaviorVector)
        {
            Logger.Log("Adding behavior vector: [{0}]", behaviorVector.ToOutputString());
            if (behaviorVector.Size != inputs.Length + outputs.Length)
            {
                throw new ArityMismatchError("Wrong gate behavior key-value pair size");
            }
            ChargeVector key, value;
            behaviorVector.Split(inputs.Length, out key, out value);
            AddBehavior(key, value);
        }

        public GateDefinition Create()
        {
            return new GateDefinition(name, behavior, inputs, outputs);
        }

        public string Name
        {
            set
            {
                if (name != null)
                {
                    throw new InternalException();
                }
                name = value;
            }
        }

        public static bool ValidNames(string[] names)
        {
            foreach (var name in names)
            {
                if (!ValidName(name))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ValidName(string name)
        {
            // Whitespace
            if (name.Contains(" ") || name.Contains("\t") ||
                name.Contains("\n") || name.Contains("\r"))
            {
                return false;
            }

            if (name.Contains(".") || name.Contains(";") || name.Contains("->") ||
                name.StartsWith("end"))
            {
                return false;
            }

            if (name.Length == 0) return false;

            return true;
        }

        // netDefinition: null if not wanted
        public static GateDefinition Parse(ParserState state, NetworkDefinitionBuilder netDefinition)
        {
            GateDefinitionBuilder builder = new GateDefinitionBuilder();

            string[] args;
            state.ExpectCommand("gate", 1, out args);
            if (!ValidName(args[0]))
            {
                state.SyntaxError(string.Format("Invalid gate name: [{0}]", args[0]));
                return null;
            }

            builder.Name = args[0];

            if (netDefinition.ContainsGateDefinition(args[0]))
            {
                state.DuplicateError(String.Format("Duplicate gate definition: [{0}]", args[0]));
                return null;
            }

            state.NextLine();

            Logger.Log("Parsing gate definition");

            string command;
            state.SplitCommand(out command, out args);
            if (command != "inputs")
            {
                state.MissingKeyword("Expecting inputs definition");
                return null;
            }
            try
            {
                builder.Inputs = args;
            }
            catch (DoubleBehaviorSpecException)
            {
                state.DuplicateError("Duplicate in inputs spec.");
                return null;
            }
            catch (InvalidNameException)
            {
                state.SyntaxError("Invalid name in inputs.");
                return null;
            }
            state.NextLine();

            state.SplitCommand(out command, out args);
            if (command != "outputs")
            {
                state.MissingKeyword("Expecting outputs definition");
                return null;
            }
            if (args.Length == 0)
            { // At least 1 output is required.
                // state.SyntaxError("Zero gate outputs"); XXX
                state.MissingKeyword("Zero gate outputs");
                return null;
            }
            try
            {
                builder.Outputs = args;
            }
            catch (DoubleBehaviorSpecException)
            {
                state.DuplicateError("Duplicate in outputs spec.");
                return null;
            }
            catch (InvalidNameException)
            {
                state.SyntaxError("Invalid name in outputs.");
                return null;
            }
            state.NextLine();

            do
            {
                state.SplitCommand(out command, out args);
                if (command == "end")
                {
                    Logger.Log("Gate definition block finished");
                    state.ExpectBareCommand("end");
                    state.NextLine();
                    break;
                }
                else
                {
                    try
                    {
                        builder.AddBehaviorVector(ChargeVector.Parse(state.SplitLine()));
                    }
                    catch (DoubleBehaviorSpecException)
                    {
                        state.DuplicateError("Duplicate behavior definition.");
                        return null;
                    }
                    catch (ArityMismatchError)
                    {
                        state.SyntaxError("Behavior key-value pair size mismatch");
                        return null;
                    }
                }

                state.NextLine();
            } while (true);

            return builder.Create(); // TODO checky
        }
    }

    abstract class PartDefinition {
        protected string[] inputNames;
        protected string[] outputNames;

        public PartDefinition(string[] inputNames, string[] outputNames)
        {
            this.inputNames = inputNames;
            this.outputNames = outputNames;
        }

        public int InputSize { get { return inputNames.Length; } }
        public virtual int InputCount { get { return InputSize; } }
        public int OutputSize { get { return outputNames.Length; } }
        public int OutputCount { get { return OutputSize; } }
        public string[] InputNames { get { return inputNames; } }
        public string[] OutputNames { get { return outputNames; } }

        public bool HasInputPin(string name)
        {
            return Util.In(inputNames, name);
        }

        public int InputPinIndex(string name)
        {
            return Util.Index(inputNames, name);
        }

        public bool HasOutputPin(string name)
        {
            return Util.In(outputNames, name);
        }

        public int OutputPinIndex(string name)
        {
            return Util.Index(outputNames, name);
        }

        public abstract RunResult GetResult(ChargeVector input);
    }

    class ConstantGate : PartDefinition {
        private static readonly string[] INPUTS = new string[] { };
        private static readonly string[] OUTPUTS = new string[] { "$out" };

        private Charge charge;
        public ConstantGate(Charge charge)
            : base(INPUTS, OUTPUTS)
        {
            this.charge = charge;
            RESULT = ChargeVector.Constant(charge);
        }

        public string Name { get { return "$Const_" + ChargeUtil.ToOutputString(charge); } }

        private readonly ChargeVector RESULT;

        public override RunResult GetResult(ChargeVector _ignore)
        {
            return new RunResult(RESULT, 1);
        }
    }

    class GateDefinition : PartDefinition {
        private Dictionary<ChargeVector, ChargeVector> behavior;

        private string name;
        public string Name { get { return name; } }

        private void CheckBehavior()
        {
            if (behavior.Count == 0) return;
            foreach (var key in behavior.Keys)
            {
                if (key.Size != InputCount)
                {
                    throw new InternalException("Wrong behavior key size");
                }
                if (behavior[key].Size != OutputCount)
                {
                    throw new InternalException("Wrong behavior value size");
                }
            }
        }

        public GateDefinition(string name, Dictionary<ChargeVector, ChargeVector> behavior, string[] inputNames, string[] outputNames)
            : base(inputNames, outputNames)
        {
            this.name = name;

            // We need comparison by logical content.
            this.behavior = new Dictionary<ChargeVector, ChargeVector>(behavior, new ChargeVectorComparer());

            CheckBehavior();

            RESULT_UNDEFINED = ChargeVector.Constant(Charge.UNDEFINED, OutputCount);
            RESULT_LOW = ChargeVector.Constant(Charge.LOW, OutputCount);
        }

        private readonly ChargeVector RESULT_UNDEFINED;
        private readonly ChargeVector RESULT_LOW;

        public override RunResult GetResult(ChargeVector input)
        {
            if (input.Size != InputCount)
            {
                throw new InternalException("Wrong input size for gate");
            }

            if (behavior.ContainsKey(input))
            {
                return new RunResult(behavior[input], 1);
            }
            else
            {
                if (input.IsAnywhereUndefined)
                {
                    return new RunResult(RESULT_UNDEFINED, 1);
                }
                else
                {
                    return new RunResult(RESULT_LOW, 1);
                }
            }
        }
    }

    interface IPartInstance {
        bool HasOutputPin(string pin);
        int OutputPinIndex(string pin);
        bool HasInputPin(string pin);
        int InputPinIndex(string pin);

        PartInputPin[] InputPins { get; }
        PartOutputPin[] OutputPins { get; }

        void WireUpInput(int index, Wire wire);
        void WireUpOutput(int index, Wire wire);

        string[] InputNames { get; }
        string[] OutputNames { get; }

        Wire[] InputWires { get; }
        List<Wire>[] OutputWires { get; }

        RunResult Evaluate(ChargeVector inputs);

        string Name { get; }

        int ID { get; set; } // Intended for users.
    }

    class GateInstance : IPartInstance {
        private PartDefinition definition;
        protected Wire[] inputWires;
        protected List<Wire>[] outputWires;

        public RunResult Evaluate(ChargeVector inputs)
        {
            return definition.GetResult(inputs);
        }

        private string name;

        private PartInputPin[] inputPins; private PartOutputPin[] outputPins;

        public GateInstance(string name, PartDefinition definition)
        {
            this.name = name;
            this.definition = definition;
            inputWires = new Wire[definition.InputCount];
            outputWires = new List<Wire>[definition.OutputCount];

            inputPins = new PartInputPin[definition.InputCount];
            outputPins = new PartOutputPin[definition.OutputCount];

            for (int i = 0; i < definition.OutputCount; i++)
            {
                outputWires[i] = new List<Wire>();
                outputPins[i] = new PartOutputPin(this, i);
            }

            for (int i = 0; i < definition.InputCount; i++)
            {
                inputPins[i] = new PartInputPin(this, i);
            }
        }

        public PartInputPin[] InputPins { get { return inputPins; } }
        public PartOutputPin[] OutputPins { get { return outputPins; } }

        public string[] InputNames { get { return definition.InputNames; } }
        public string[] OutputNames { get { return definition.OutputNames; } }

        public string Name { get { return name; } }

        public bool HasOutputPin(string pin)
        {
            return definition.HasOutputPin(pin);
        }

        public int OutputPinIndex(string pin)
        {
            return definition.OutputPinIndex(pin);
        }

        public bool HasInputPin(string pin)
        {
            return definition.HasInputPin(pin);
        }

        public int InputPinIndex(string pin)
        {
            return definition.InputPinIndex(pin);
        }

        public void WireUpInput(int index, Wire wire)
        {
            if (inputWires[index] != null)
            {
                throw new PinAlreadyOccupied(string.Format("Normal part input pin {0} already occupied.", index));
            }

            inputWires[index] = wire;
        }

        public void WireUpOutput(int index, Wire wire)
        {
            outputWires[index].Add(wire); // Outputs don't need to be uniquely wired.
        }

        public Wire[] InputWires
        {
            get
            {
                return inputWires;
            }
        }

        public List<Wire>[] OutputWires
        {
            get
            {
                return outputWires;
            }
        }

        public int ID
        {
            get;
            set;
        }
    }

    abstract class PartPin {
        private IPartInstance part;
        private int index;

        public PartPin(IPartInstance part, int index)
        {
            this.part = part;
            this.index = index;
        }

        public IPartInstance Part
        {
            get
            {
                return part;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public abstract string GetPinName();

        public string Name
        {
            get
            {
                return string.Format("{0}.{1}(#{2})", part.Name, GetPinName(), ID);
            }
        }

        // For users
        public int ID { get; set; }
    }

    class PartInputPin : PartPin {
        public PartInputPin(IPartInstance part, int index) : base(part, index) { }

        public override string GetPinName()
        {
            return Part.InputNames[Index];
        }
    }

    class PartOutputPin : PartPin {
        public PartOutputPin(IPartInstance part, int index) : base(part, index) { }

        public override string GetPinName()
        {
            return Part.OutputNames[Index];
        }
    }

    class Wire {
        private PartInputPin input;
        private PartOutputPin output;

        public Wire(PartInputPin input, PartOutputPin output)
        {
            this.input = input;
            this.output = output;
        }

        public PartInputPin Input { get { return input; } }
        public PartOutputPin Output { get { return output; } }

        public override string ToString()
        {
            return string.Format("[#{2}] {0} -> {1}", output.Name, input.Name, ID);
        }

        // For users
        public int ID { get; set; }
    }

    // Part definition: for external use
    // Part instance: for internal use as a wire endpoint
    class NetworkDefinition : PartDefinition {
        class InternalMockPart : IPartInstance {
            public string Name { get { return "#Network"; } }

            // Network inputs become outputs.
            // Network outputs become inputs.
            private NetworkDefinition network;

            private PartInputPin[] inputPins;
            private PartOutputPin[] outputPins;
            public InternalMockPart(NetworkDefinition network)
            {
                this.network = network;
                if (network == null)
                {
                    throw new InternalException("Null network passed to InternalMockPart constructor");
                }

                networkInputWires = new List<Wire>[network.InputCount + 2];
                networkOutputWires = new Wire[network.OutputCount];

                inputPins = new PartInputPin[network.OutputCount];
                outputPins = new PartOutputPin[network.InputCount + 2];

                for (int i = 0; i < network.InputCount + 2; i++)
                {
                    networkInputWires[i] = new List<Wire>();
                    outputPins[i] = new PartOutputPin(this, i);
                }

                for (int i = 0; i < network.OutputCount; i++)
                {
                    inputPins[i] = new PartInputPin(this, i);
                }
            }
            public PartInputPin[] InputPins { get { return inputPins; } }
            public PartOutputPin[] OutputPins { get { return outputPins; } }
            public bool HasOutputPin(string pin)
            {
                return Util.In(network.InputNames, pin);
            }
            public int OutputPinIndex(string pin)
            {
                return Util.Index(network.InputNames, pin);
            }
            public bool HasInputPin(string pin)
            {
                return Util.In(network.OutputNames, pin);
            }
            public int InputPinIndex(string pin)
            {
                return Util.Index(network.OutputNames, pin);
            }

            protected List<Wire>[] networkInputWires;
            protected Wire[] networkOutputWires;

            public void WireUpInput(int index, Wire wire)
            {
                if (networkOutputWires[index] != null)
                {
                    throw new PinAlreadyOccupied(string.Format("Internal mock input pin {0} already occupied.", index));
                }

                networkOutputWires[index] = wire;
            }
            public void WireUpOutput(int index, Wire wire)
            {
                networkInputWires[index].Add(wire);
            }

            public Wire[] InputWires
            {
                get
                {
                    return networkOutputWires;
                }
            }

            public List<Wire>[] OutputWires
            {
                get
                {
                    return networkInputWires;
                }
            }

            public RunResult Evaluate(ChargeVector vector)
            {
                // Do nothing.
                return new RunResult();
            }

            public string[] InputNames
            {
                get
                {
                    return network.OutputNames;
                }
            }

            public string[] OutputNames
            {
                get
                {
                    return network.InputNames;
                }
            }

            public int ID { get; set; }
        };

        // XXX FUJ
        public override int InputCount { get { return inputNames.Length - 2; } }

        private Dictionary<string, GateInstance> gates;
        private InternalMockPart internalPart;

        public NetworkDefinition(Dictionary<string, GateInstance> gates, string[] inputNames, string[] outputNames)
            : base(inputNames, outputNames)
        {
            this.gates = gates;
            this.internalPart = new InternalMockPart(this);
        }

        public IPartInstance InternalPart
        {
            get
            {
                return internalPart;
            }
        }

        public IEnumerable<IPartInstance> EachPart()
        {
            yield return internalPart;
            foreach (var gate in gates.Values)
            {
                yield return gate;
            }
        }

        public IEnumerable<Wire> EachWire()
        {
            foreach (var part in EachPart())
            {
                foreach (var wire in part.InputWires)
                {
                    yield return wire;
                }
            }
        }

        public override RunResult GetResult(ChargeVector input)
        {
            throw new NotImplementedException("Asking network for result");
        }
    }

    class NetworkDefinitionBuilder {
        private Dictionary<string, GateDefinition> gateDefinitions;
        private Dictionary<string, GateInstance> gateInstances;

        private string[] inputNames;
        private string[] outputNames;

        public string[] InputNames
        {
            set
            {
                if (inputNames != null)
                {
                    throw new InternalException();
                }
                if (!Util.Unique(value))
                {
                    throw new DoubleBehaviorSpecException();
                }
                if (!GateDefinitionBuilder.ValidNames(value))
                {
                    throw new InvalidNameException();
                }
                inputNames = value;
            }
        }

        public string[] OutputNames
        {
            set
            {
                if (outputNames != null)
                {
                    throw new InternalException();
                }
                if (!Util.Unique(value))
                {
                    throw new DoubleBehaviorSpecException();
                }
                if (!GateDefinitionBuilder.ValidNames(value))
                {
                    throw new InvalidNameException();
                }
                outputNames = value;
            }
        }

        public NetworkDefinition Create()
        {
            EnsureNetwork();
            return network;
        }

        public NetworkDefinitionBuilder()
        {
            gateDefinitions = new Dictionary<string, GateDefinition>();
            gateInstances = new Dictionary<string, GateInstance>();
        }

        public int GateCount
        {
            get
            {
                return gateInstances.Count;
            }
        }

        public void AddGateInstance(string instanceName, string typeName)
        {
            EnsureNoNetwork();

            Logger.Log("Adding gate {0} as instance of {1}", instanceName, typeName);

            if (!ContainsGateDefinition(typeName))
            {
                throw new UnknownGate();
            }

            if (ContainsGateInstance(instanceName))
            {
                throw new DuplicateGate();
            }

            gateInstances.Add(instanceName, new GateInstance(instanceName, gateDefinitions[typeName]));
        }

        protected GateInstance FindGateInstance(string name)
        {
            if (!ContainsGateInstance(name))
            {
                throw new InternalException();
            }
            return gateInstances[name];
        }

        public bool ContainsGateInstance(string name)
        {
            return gateInstances.ContainsKey(name);
        }

        public bool ContainsGateDefinition(string name)
        {
            return gateDefinitions.ContainsKey(name);
        }

        public void AddGateDefinition(GateDefinition definition)
        {
            EnsureNoNetwork();

            if (ContainsGateDefinition(definition.Name))
            {
                throw new DuplicateGateDefinition();
            }

            gateDefinitions.Add(definition.Name, definition);
        }

        private NetworkDefinition network;

        private void EnsureNetwork()
        {
            if (network == null)
            {
                string[] inputNames2 = new string[inputNames.Length + 2];
                for (int i = 0; i < inputNames.Length; i++)
                {
                    inputNames2[i] = inputNames[i];
                }
                inputNames2[inputNames.Length] = "0";
                inputNames2[inputNames.Length + 1] = "1";
                network = new NetworkDefinition(gateInstances, inputNames2, outputNames);
            }
        }

        private void EnsureNoNetwork()
        {
            if (network != null)
            {
                throw new InternalException("Network already commited!");
            }
        }

        public void AddWire(Wire wire)
        {
            EnsureNetwork();

            PartInputPin input = wire.Input;
            PartOutputPin output = wire.Output;

            input.Part.WireUpInput(input.Index, wire);
            output.Part.WireUpOutput(output.Index, wire);
        }

        private static readonly char[] PIN_DELIMITER = new char[] { '.' };
        private static readonly string[] WIRE_DELIMITER = new string[] { "->" };

        protected static void SplitPinSpec(ParserState state, string spec, NetworkDefinitionBuilder builder, out IPartInstance instance, out string indexSpec)
        {
            string[] split = spec.Split(PIN_DELIMITER);
            if (split.Length > 2) state.SyntaxError("Output with more than 2 parts");

            builder.EnsureNetwork();

            if (split.Length == 1)
            {
                Logger.Log("Pin spec [{0}]: network pin", spec);
                instance = builder.network.InternalPart;
                indexSpec = split[0];
            }
            else
            {
                Logger.Log("Pin spec [{0}]: gate instance {1} pin (index {2})", spec, split[0], split[0]);
                if (!builder.ContainsGateInstance(split[0]))
                {
                    state.SyntaxError(string.Format("Unknown gate instance [{0}]", split[0]));
                    instance = null;
                    indexSpec = null;
                }
                else
                {
                    instance = builder.FindGateInstance(split[0]);
                    indexSpec = split[1];
                }
            }
        }

        protected static PartInputPin ParseInputPin(ParserState state, string spec, NetworkDefinitionBuilder builder)
        {
            IPartInstance instance;
            string indexSpec;
            SplitPinSpec(state, spec, builder, out instance, out indexSpec);

            if (instance.HasInputPin(indexSpec))
            {
                return instance.InputPins[instance.InputPinIndex(indexSpec)];
            }
            else if (instance.HasOutputPin(indexSpec))
            {
                state.BindingRuleError("Output pin given instead of an input one.");
                return null;
            }
            else
            {
                state.SyntaxError(string.Format("No such input pin: {0}", spec));
                return null;
            }
        }

        protected static PartOutputPin ParseOutputPin(ParserState state, string spec, NetworkDefinitionBuilder builder)
        {
            IPartInstance instance;

            builder.EnsureNetwork();

            string indexSpec;
            SplitPinSpec(state, spec, builder, out instance, out indexSpec);

            if (instance.HasOutputPin(indexSpec))
            {
                return instance.OutputPins[instance.OutputPinIndex(indexSpec)];
            }
            else if (instance.HasInputPin(indexSpec))
            {
                state.BindingRuleError("Input pin given instead of an output one.");
                return null;
            }
            else
            {
                state.SyntaxError(string.Format("No such output pin: {0}", spec));
                return null;
            }
        }

        protected static Wire ParseWire(ParserState state, NetworkDefinitionBuilder builder)
        {
            string spec;
            string[] args;
            state.SplitCommand(out spec, out args);
            if (args.Length > 0)
            {
                state.SyntaxError("Wire with spaces");
                return null;
            }

            string[] split = spec.Split(WIRE_DELIMITER, StringSplitOptions.None);
            if (split.Length != 2)
            {
                state.SyntaxError("Wire with more than 2 ends");
                return null;
            }

            PartInputPin input = ParseInputPin(state, split[0], builder);
            PartOutputPin output = ParseOutputPin(state, split[1], builder);

            if (input.Part == builder.network.InternalPart && output.Part == builder.network.InternalPart)
            {
                // state.SyntaxError("Network input - network output wires are prohibited."); XXX
                state.BindingRuleError("Network input - network output wires are prohibited.");
                return null;
            }

            return new Wire(input, output);
        }

        public static void LoadNetworkDefinition(ParserState state, NetworkDefinitionBuilder builder)
        {
            Logger.Log("Loading network definition");

            state.ExpectBareCommand("network");
            state.NextLine();

            string command;
            string[] args;
            state.SplitCommand(out command, out args);
            if (command != "inputs")
            {
                state.MissingKeyword("Expecting inputs definition");
                return;
            }
            if (args.Length < 1)
            {
                // state.SyntaxError("Inputs must not be empty"); XXX
                state.MissingKeyword("Inputs must not be empty");
                return;
            }
            if (Util.In(args, "0") || Util.In(args, "1"))
            {
                state.SyntaxError("0 and 1 must not be network inputs.");
                return;
            }
            try
            {
                builder.InputNames = args;
            }
            catch (DoubleBehaviorSpecException)
            {
                state.DuplicateError("Duplicate in inputs spec.");
                return;
            }
            catch (InvalidNameException)
            {
                state.SyntaxError("Invalid name in inputs.");
                return;
            }
            state.NextLine();

            state.SplitCommand(out command, out args);
            if (command != "outputs")
            {
                state.MissingKeyword("Expecting outputs definition");
                return;
            }
            if (args.Length < 1)
            {
                // state.SyntaxError("Outputs must not be empty"); XXX
                state.MissingKeyword("Outputs must not be empty");
                return;
            }

            try
            {
                builder.OutputNames = args;
            }
            catch (DoubleBehaviorSpecException)
            {
                state.DuplicateError("Duplicate in outputs spec.");
                return;
            }
            catch (InvalidNameException)
            {
                state.SyntaxError("Invalid name in outputs.");
                return;
            }
            state.NextLine();

            do
            {
                state.SplitCommand(out command, out args);
                if (command == "end")
                {
                    if (builder.GateCount == 0)
                    {
                        state.MissingKeyword("Missing a gate");
                        return;
                    }
                    Logger.Log("Network definition block finished");
                    state.ExpectBareCommand("end");
                    CheckBindingRules(state, builder);
                    state.NextLine();
                    break;
                }
                else if (command == "gate")
                {
                    if (args.Length < 2)
                    {
                        state.MissingKeyword("Missing gate command arguments");
                        return;
                    }
                    if (args.Length != 2)
                    {
                        state.SyntaxError("Wrong 'gate' syntax (expecting 'gate (instance_name) (type_name)')");
                        return;
                    }
                    string instanceName = args[0];
                    string typeName = args[1];

                    try
                    {
                        builder.AddGateInstance(instanceName, typeName);
                    }
                    catch (DuplicateGate)
                    {
                        state.DuplicateError(String.Format("Gate instance [{0}] is duplicated.", instanceName));
                        return;
                    }
                    catch (UnknownGate)
                    {
                        state.SyntaxError(String.Format("Unknown gate type [{0}].", typeName));
                        return;
                    }
                }
                else
                {
                    try
                    {
                        builder.AddWire(ParseWire(state, builder));
                    }
                    catch (PinAlreadyOccupied error)
                    {
                        state.DuplicateError(error);
                        return;
                    }
                }
                state.NextLine();
            } while (true);
        }

        public void CheckBindingRules()
        {
            EnsureNetwork();
            IPartInstance part = network.InternalPart;
            // 1) Vsechny vystupy site musi byt nekam napojeny.
            foreach (Wire w in part.InputWires)
            {
                if (w == null) throw new BindingRulesViolated("Unconnected network output");
            }
            // 2) Na kazdy vstup site musi byt neco pripojeno.
            // -2: fakove inputy na konstanty
            for (int i = 0; i < part.OutputWires.Length - 2; i++)
            {
                if (part.OutputWires[i].Count == 0) throw new BindingRulesViolated("Unconnected network input");
            }

            // Vstupy hradel muzou zustat nezapojene.
        }

        public static void CheckBindingRules(ParserState state, NetworkDefinitionBuilder netDefinition)
        {
            try
            {
                netDefinition.CheckBindingRules();
            }
            catch (BindingRulesViolated ex)
            {
                state.BindingRuleError(ex);
            }
        }

        public static NetworkDefinition Load(ParserState state)
        {
            NetworkDefinitionBuilder builder = new NetworkDefinitionBuilder();

            bool gateLoaded = false, networkLoaded = false;
            do
            {
                string command;
                try
                {
                    state.ReadCommand(out command);
                }
                catch (EOF)
                {
                    break;
                }

                try
                {
                    if (command == "gate")
                    {
                        GateDefinition gate = GateDefinitionBuilder.Parse(state, builder);
                        builder.AddGateDefinition(gate);
                        gateLoaded = true;
                    }
                    else if (command == "network")
                    {
                        if (!gateLoaded)
                        {
                            state.MissingKeyword("Missing gate definitions.");
                        }
                        LoadNetworkDefinition(state, builder);
                        networkLoaded = true;
                    }
                    else
                    {
                        state.SyntaxError(String.Format("Unknown command: [{0}]", command));
                    }
                }
                catch (EOF)
                {
                    state.MissingKeyword("End of file reached");
                }
            } while (true);

            if (!networkLoaded)
            {
                state.MissingKeyword("Network not loaded.");
            }

            return builder.Create();
        }

        public static NetworkDefinition Load(string path)
        {
            StreamReader reader;
            try
            {
                reader = new StreamReader(path);
            }
            catch (FileNotFoundException)
            {
                throw new NetworkFileUnreadable();
            }
            catch (DirectoryNotFoundException)
            {
                throw new NetworkFileUnreadable();
            }
            catch (ArgumentException)
            {
                throw new NetworkFileUnreadable();
            }
            catch (IOException)
            {
                throw new NetworkFileUnreadable();
            }

            return Load(new ParserState(reader));
        }
    }

    struct RunResult {
        private ChargeVector outputs;
        private long runningTime;

        public RunResult(ChargeVector outputs, long runningTime)
        {
            this.outputs = outputs;
            this.runningTime = runningTime;
        }

        public string ToOutputString()
        {
            return runningTime.ToString() + " " + outputs.ToOutputString();
        }

        public ChargeVector Outputs
        {
            get
            {
                return outputs;
            }
        }

        public bool IsNothing
        {
            get
            {
                return outputs == null;
            }
        }
    }

    class ChargeVector {
        private Charge[] charges;

        private static int[] ZOBRIST_HASH_COEFFICIENTS;
        private static readonly int MAX_SIZE = 1000;

        static ChargeVector()
        {
            ZOBRIST_HASH_COEFFICIENTS = new int[MAX_SIZE * (int)Charge._COUNT];

            Random rand = new Random(100);
            for (int i = 0; i < ZOBRIST_HASH_COEFFICIENTS.Length; i++)
            {
                ZOBRIST_HASH_COEFFICIENTS[i] = rand.Next();
            }
        }

        public ChargeVector(Charge[] charges)
        {
            if (charges.Length > MAX_SIZE) throw new InternalException();
            this.charges = charges;
            this.hashCode = BuildHashCode();
        }

        private int BuildHashCode()
        {
            int h = 0;
            for (int i = 0; i < Size; i++)
            {
                h ^= ZOBRIST_HASH_COEFFICIENTS[(i * (int)Charge._COUNT) + ((int)charges[i])];
            }
            return h;
        }

        public int Count { get { return charges.Length; } }
        public int Size { get { return Count; } }

        public static ChargeVector Parse(string[] parts)
        {
            Charge[] charges = new Charge[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                charges[i] = ChargeUtil.Parse(parts[i]);
            }
            return new ChargeVector(charges);
        }

        public static ChargeVector Parse(string str)
        {
            return Parse(str.Split(ParserState.SPACE));
        }

        public static ChargeVector Constant(Charge charge)
        {
            return Constant(charge, 1);
        }

        public static ChargeVector Constant(Charge charge, int size)
        {
            Charge[] charges = new Charge[size];
            for (int i = 0; i < size; i++) charges[i] = charge;
            return new ChargeVector(charges);
        }

        public string ToOutputString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                if (i != 0) builder.Append(" ");
                builder.Append(ChargeUtil.ToOutputString(charges[i]));
            }
            return builder.ToString();
        }

        public static readonly ChargeVector Empty = new ChargeVector(new Charge[] { });

        public Charge this[int i]
        {
            get
            {
                return charges[i];
            }
            set
            {
                if (charges[i] != value)
                {
                    hashCode ^= ZOBRIST_HASH_COEFFICIENTS[(i * (int)Charge._COUNT) + ((int)charges[i])];
                    charges[i] = value;
                    hashCode ^= ZOBRIST_HASH_COEFFICIENTS[(i * (int)Charge._COUNT) + ((int)charges[i])];
                }
            }
        }

        private int hashCode;

        public override int GetHashCode()
        {
            return hashCode;
        }

        public void Split(int leftLength, out ChargeVector left, out ChargeVector right)
        {
            if (leftLength > Size) throw new InternalException("Too long left size");
            Charge[] chargeLeft = new Charge[leftLength], chargeRight = new Charge[Size - leftLength];
            for (int i = 0; i < Size; i++)
            {
                if (i < leftLength)
                {
                    chargeLeft[i] = charges[i];
                }
                else
                {
                    chargeRight[i - leftLength] = charges[i];
                }
            }
            left = new ChargeVector(chargeLeft);
            right = new ChargeVector(chargeRight);
        }

        public bool IsAnywhereUndefined
        {
            get
            {
                foreach (Charge c in charges)
                {
                    if (c == Charge.UNDEFINED)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    };

    class ChargeVectorComparer : IEqualityComparer<ChargeVector> {
        public int GetHashCode(ChargeVector a)
        {
            return a.GetHashCode();

        }

        public bool Equals(ChargeVector a, ChargeVector b)
        {
            return a.GetHashCode() == b.GetHashCode();
        }
    }

    class NetworkRunner {
        private NetworkDefinition definition;
        private Charge[] charges;

        private IEnumerable<Wire> EachWire() { return definition.EachWire(); }

        #region Gate evaluation
        private bool[] needsEvaluation;
        private List<IPartInstance> toEvaluate = new List<IPartInstance>();

        private void SubmitForEvaluation(IPartInstance gate)
        {
            if (!needsEvaluation[gate.ID])
            {
                needsEvaluation[gate.ID] = true;
                toEvaluate.Add(gate);
            }
        }

        private void EvaluateGate(IPartInstance part)
        {
            RunResult result = part.Evaluate(GetInputVector(part));
            if (result.IsNothing) return; // 'null' means "explicitly no change"

            ChargeVector gateOutputs = result.Outputs;
            if (gateOutputs.Size != part.OutputWires.Length) throw new InternalException();

            for (int i = 0; i < gateOutputs.Size; i++)
            {
                if (charges[part.OutputPins[i].ID] != gateOutputs[i])
                {
                    charges[part.OutputPins[i].ID] = gateOutputs[i];

                    foreach (Wire w in part.OutputWires[i])
                    {
                        TripWire(w);
                    }
                }
            }
        }

        private void CommitEvaluation()
        {
            foreach (GateInstance gate in toEvaluate)
            {
                EvaluateGate(gate);
                needsEvaluation[gate.ID] = false;
            }
            toEvaluate.Clear();
        }
        #endregion

        private void DoInitialRound()
        {
            foreach (var wire in EachWire())
            {
                if (wire.Output.Part == definition.InternalPart)
                {
                    SubmitForEvaluation(wire.Input.Part);
                }
            }

            // Evaluate zero arity
            foreach (var part in EachPart())
            {
                if (part.InputPins.Length == 0)
                {
                    SubmitForEvaluation(part);
                }
            }
            CommitEvaluation();
        }

        private IEnumerable<IPartInstance> EachPart()
        {
            return definition.EachPart();
        }

        private IEnumerable<PartPin> EachPin()
        {
            foreach (var gate in EachPart())
            {
                foreach (var pin in gate.InputPins) yield return pin;
                foreach (var pin in gate.OutputPins) yield return pin;
            }
        }

        private void AssignPinIDs()
        {
            int n = 0;
            foreach (var pin in EachPin())
            {
                pin.ID = n++;
            }
            charges = new Charge[n];
            for (int i = 0; i < n; i++) charges[i] = Charge.UNDEFINED;
        }

        private void AssignPartIDs()
        {
            int n = 0;
            foreach (var part in EachPart())
            {
                part.ID = n++;
            }
            partInputs = new ChargeVector[n];
            needsEvaluation = new bool[n];
            foreach (var part in EachPart())
            {
                partInputs[part.ID] = new ChargeVector(new Charge[part.InputWires.Length]);
                needsEvaluation[part.ID] = false;
            }
        }

        private void InitCharges()
        {
            AssignPinIDs();
            AssignPartIDs();
            AssignWireIDs();
        }

        private ChargeVector[] partInputs;

        public NetworkRunner(NetworkDefinition definition)
        {
            this.definition = definition;

            foreach (var part in EachPart())
            {
                Logger.Log("Part: {0}", part.Name);
            }

            InitCharges();
            DoInitialRound();
        }


        public ChargeVector GetInputVector(IPartInstance part)
        {
            ChargeVector inputs = partInputs[part.ID];
            for (int i = 0; i < part.InputWires.Length; i++)
            {
                inputs[i] = charges[part.InputPins[i].ID];
            }
            return inputs;
        }

        public static readonly long MAXIMUM_STEPS = 1000000;

        private void ConnectWirePins(Wire wire, out bool didChange)
        {
            PartPin outPin = wire.Input.Part.InputPins[wire.Input.Index];
            PartPin inPin = wire.Output.Part.OutputPins[wire.Output.Index];

#if SLOW_DEBUGGING
			Logger.Log("Connecting pin charges: {0} ({2}) -> {1} ({3})", inPin.Name, outPin.Name, ChargeUtil.ToOutputString(charges[inPin.ID]), ChargeUtil.ToOutputString(charges[outPin.ID]));
#endif

            didChange = charges[outPin.ID] != charges[inPin.ID];
            charges[outPin.ID] = charges[inPin.ID];

            // Special case.
            if (wire.Input.Part == definition.InternalPart)
            {
                didChange = false;
            }

            if (didChange)
            {
#if SLOW_DEBUGGING
				Logger.Log("Gate [{0}] has updated inputs", wire.Input.Part.Name);
#endif
                SubmitForEvaluation(wire.Input.Part);
            }
        }

        private bool maximumStepsReached = false;

        #region Wire tripping
        private List<Wire> trippedWires = new List<Wire>();
        private bool[] wireTripped;

        private void TripWire(Wire w)
        {
            if (!wireTripped[w.ID])
            {
#if SLOW_DEBUGGING
				Logger.Log("Trip wire: {0}", w);
#endif
                trippedWires.Add(w);
                wireTripped[w.ID] = true;
            }
        }

        private void AssignWireIDs()
        {
            int n = 0;
            foreach (var wire in EachWire())
            {
                wire.ID = n++;
            }
            wireTripped = new bool[n];
            foreach (var wire in EachWire())
            {
                wireTripped[wire.ID] = false;
            }
        }

        private void UntripWires()
        {
#if SLOW_DEBUGGING
			Logger.Log("Untripping all wires");
#endif
            foreach (Wire w in trippedWires)
            {
                wireTripped[w.ID] = false;
            }
            trippedWires.Clear();
        }
        #endregion

        private void ConnectTripped(Wire wire, ref bool someWireChanged)
        {
#if SLOW_DEBUGGING
				Logger.Log("Tripped wire: {0}", wire);
#endif
            bool didChangeWire;
            ConnectWirePins(wire, out didChangeWire);
            if (didChangeWire)
            {
#if SLOW_DEBUGGING
					Logger.Log("This wire changed.");
#endif
                someWireChanged = true;
            }
        }

        private void ConnectTrippedWires(out bool someWireChanged)
        {
            someWireChanged = false;

            if (trippedWires.Count == 0)
            {
                foreach (var wire in EachWire())
                {
                    ConnectTripped(wire, ref someWireChanged);
                }
            }
            else
            {
                for (int i = 0; i < trippedWires.Count; i++)
                {
                    ConnectTripped(trippedWires[i], ref someWireChanged);
                }
            }
        }

        public RunResult Run(ChargeVector inputs)
        {
            int round = 0;

            // Special: set network inputs.
            if (inputs.Size != definition.InputSize - 2)
            {
                throw new InternalException();
            }

            for (int i = 0; i < inputs.Size; i++)
            {
                charges[definition.InternalPart.OutputPins[i].ID] = inputs[i];
            }
            // Hack for constants.
            charges[definition.InternalPart.OutputPins[inputs.Size].ID] = Charge.LOW;
            charges[definition.InternalPart.OutputPins[inputs.Size + 1].ID] = Charge.HIGH;

            UntripWires();

            do
            {
#if SLOW_DEBUGGING
				Logger.Log("Round {0} started...", round);
#endif
                bool someWireChanged;
                ConnectTrippedWires(out someWireChanged);

                if (round == MAXIMUM_STEPS)
                {
                    maximumStepsReached = true;
                    break;
                }

                if (!someWireChanged)
                {
#if SLOW_DEBUGGING
					Logger.Log("== No wires changed");
#endif
                    if (!maximumStepsReached)
                    {
                        break;
                    }
                    if (round != 0)
                    {
                        break;
                    }
                }
                round++;

                CommitEvaluation();
            } while (true);

            return new RunResult(GetInputVector(definition.InternalPart), round);
        }
    }

    class NetworkInteractor {
        private NetworkRunner runner;
        private NetworkDefinition network;

        public NetworkInteractor(NetworkDefinition network)
        {
            this.network = network;
            runner = new NetworkRunner(network);
        }

        public string ExecuteCommand(string command)
        {
            Logger.Log(">>> {0}", command);
            ChargeVector inputs;
            try
            {
                inputs = ChargeVector.Parse(command);
                if (inputs.Count != network.InputCount)
                {
                    throw new ArityMismatchError();
                }
            }
            catch (ChargeParseError)
            {
                return "Syntax error.";
            }
            catch (ArityMismatchError)
            {
                return "Syntax error.";
            }

            RunResult result = runner.Run(inputs);
            return result.ToOutputString();
        }

        public void Interact(TextReader input, TextWriter output)
        {
            string line;
            do
            {
                line = input.ReadLine();
                if (line == null || line == "end")
                {
                    break;
                }
                string lineOut = ExecuteCommand(line);
                output.WriteLine(lineOut);
                Logger.Log("<<< {0}", lineOut);
            } while (true);
        }
    }

    class TestSuite {
        private void Fail(string msg)
        {
            Console.WriteLine("Test failed: {0}", msg);
            Environment.Exit(1);
        }

        private void Log(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void FileTest(string input, string output, string file)
        {
            Log("File test: input {0}, output {1}, file {2}", input, output, file);
            StreamReader inputReader = new StreamReader(input);
            StringWriter outWriter = new StringWriter();

            Program.RunOnFile(file, inputReader, outWriter);

            string expect = File.ReadAllText(output);
            string got = outWriter.ToString();

            if (!got.Equals(expect))
            {
                Log("Output got: >>{0}<<", got);
                Log("Output expect: >>{0}<<", expect);

                Fail(String.Format("Failed on files: {0}, {1}, {2}", input, output, file));
            }
        }

        public void AcceptanceTests()
        {
            string[] dirs = new string[] { "01", "04", "05", "07", "08", "09", "16", "17", "20" };
            foreach (string dir in dirs)
            {
                string input = string.Format("data/{0}/std.in", dir);
                string output = string.Format("data/{0}/std.out", dir);
                string file = string.Format("data/{0}/hradla.in", dir);
                FileTest(input, output, file);
            }
        }

        public void FileTests()
        {
            string[] dirs = new string[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "orig-21" };
            foreach (string dir in dirs)
            {
                string input = string.Format("test-mine/{0}/std.in", dir);
                string output = string.Format("test-mine/{0}/std.out", dir);
                string file = string.Format("test-mine/{0}/hradla.in", dir);
                FileTest(input, output, file);
            }
        }

        public void Run()
        {
            AcceptanceTests();
            FileTests();

            Log("");
            Log("-- Tests ran OK --");
        }
    }

    class Program {
        public static void RunOnFile(string filename, TextReader input, TextWriter output)
        {
            NetworkDefinition network;
            try
            {
                network = NetworkDefinitionBuilder.Load(filename);
            }
            catch (NetworkFileUnreadable)
            {
                output.WriteLine("File error.");
                return;
            }
            catch (ParseError e)
            {
                Logger.Log("Parse error caught.");
                Logger.Log(e);
                output.WriteLine(e.ToOutputString());
                return;
            }

            NetworkInteractor interactor = new NetworkInteractor(network);
            interactor.Interact(input, output);
        }

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Argument error.");
                return;
            }

            string filename = args[0];
            if (filename == "__TEST__")
            {
                new TestSuite().Run();
                return;
            }

            Logger.LOG = false;
            RunOnFile(filename, Console.In, Console.Out);
        }
    }
}
