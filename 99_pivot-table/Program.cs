using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using System.IO;

class Parser {

    TextReader reader;

    readonly char[] separators = { ' ', '\t', '\r' };

    public Parser(TextReader reader)
    {
        this.reader = reader;
    }

    public string[] ParseNextLine()
    {

        String ln = reader.ReadLine();

        if (ln == null) return null;

        return ln.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public void Close() { reader.Close(); }

}

class Tree {

    public SortedDictionary<String, Tree> data;

    public Tree()
    {
        data = new SortedDictionary<string, Tree>();
    }

    public Tree this[String s]
    {
        get { return this.data[s]; }
        set { this.data[s] = value; }
    }

    public virtual void Print(TextWriter writer, int spaces, bool firstLevel)
    {
        bool first = true;
        foreach (var item in data)
        {
            if (!first)
                for (int i = 0; i < spaces; i++)
                    writer.Write(" ");

            writer.Write(item.Key + " ");
            data[item.Key].Print(writer, spaces + item.Key.Length + 1, first);

            first = false;
        }
    }

}

class IntTree : Tree {

    public new SortedDictionary<String, int> data;

    public IntTree()
    {
        data = new SortedDictionary<string, int>();
    }

    public new int this[String s]
    {
        get { return this.data[s]; }
        set { this.data[s] = value; }
    }

    public override void Print(TextWriter writer, int spaces, bool firstLevel)
    {
        foreach (var item in data)
            writer.Write("{0}:{1} ", item.Key, item.Value);

        writer.WriteLine();
    }

}

class PivotTable {

    Parser parser;
    Dictionary<String, int> header;
    int headerWidth;
    Tree data;

    String[] rows;
    String column;
    String sumWith;

    public PivotTable(Parser p, String sumWith, String columns, String[] rows)
    {
        parser = p;
        this.column = columns;
        this.rows = rows;
        this.sumWith = sumWith;
    }

    public void LoadData()
    {
        loadHeader();

        data = new Tree();

        String[] line;
        while ((line = parser.ParseNextLine()) != null)
        {

            if (line.Length != headerWidth)
                throw new WrongNumberOfColumnsException();

            var currentLevel = data;
            int colNum = -1;

            for (int i = 0; i < rows.Length; i++)
            {
                if (!header.ContainsKey(rows[i]))
                    throw new ColumnNotFoundException();

                colNum = header[rows[i]];

                if (!currentLevel.data.ContainsKey(line[colNum]))
                {
                    Tree newTree = (i == rows.Length - 1) ? new IntTree() : new Tree();
                    currentLevel.data.Add(line[colNum], newTree);
                }

                currentLevel = currentLevel[line[colNum]];

            }

            IntTree valueLevel = (IntTree)currentLevel;

            if (!header.ContainsKey(column) || !header.ContainsKey(sumWith))
                throw new ColumnNotFoundException();

            colNum = header[column];

            if (!valueLevel.data.ContainsKey(line[colNum]))
                valueLevel.data.Add(line[colNum], 0);

            int value;
            if (!Int32.TryParse(line[header[sumWith]], out value))
                throw new NotANumberException();

            valueLevel[line[colNum]] += value;

        }
    }

    void loadHeader()
    {
        header = new Dictionary<string, int>();

        var data = parser.ParseNextLine();

        if (data == null)
            throw new InsufficientDataException();

        headerWidth = data.Length;

        for (int i = 0; i < data.Length; i++)
            header.Add(data[i], i);
    }

    public void Print(TextWriter writer)
    {
        data.Print(writer, 0, true);
    }

}

class InsufficientDataException : ApplicationException {
    public InsufficientDataException() : base("Data need to have at least a header.") { }
}

class WrongNumberOfColumnsException : ApplicationException {
    public WrongNumberOfColumnsException() : base("Table has some missing data.") { }
}

class NotANumberException : ApplicationException {
    public NotANumberException() : base("This column does not have integer value.") { }
}

class ColumnNotFoundException : ApplicationException {
    public ColumnNotFoundException() : base("Column has not been found!") { }
}

class Program {

    static void Run(string[] args, TextReader reader, TextWriter writer)
    {

        Parser parser = new Parser(reader);

        var rows = new String[args.Length - 4];
        for (int i = 4; i < args.Length; i++)
            rows[i - 4] = args[i];

        PivotTable table = new PivotTable(parser, args[2], args[3], rows);

        table.LoadData();
        parser.Close();

        table.Print(writer);
        writer.Close();

    }

    static void Main(string[] args)
    {
        if (args.Length < 5)
        {
            Console.WriteLine("Provide Input, Output and 3 columns");
            return;
        }

        TextReader reader = null;
        TextWriter writer = null;

        try
        {
            reader = File.OpenText(args[0]);
            writer = File.CreateText(args[1]);
            Run(args, reader, writer);
        }
        catch (IOException)
        {
            Console.WriteLine("Could not open given files.");
        }
        catch (ApplicationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (reader != null) reader.Close();
            if (writer != null) writer.Close();
        }

    }
}

