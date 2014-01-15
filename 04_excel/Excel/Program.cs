using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Excel_UnitTests")]

namespace Excel {

    static class Helper {

        public static void EnlargeArray<T>(ref T[] array)
        {
            T[] a = new T[array.Length * 2 + 2];
            for (int i = 0; i < array.Length; i++)
            {
                a[i] = array[i];
            }
            array = a;
        }

        public static void EnlargeArray<T>(ref T[][] array)
        {
            T[][] a = new T[array.GetLength(0) * 2 + 2][];
            for (int i = 0; i < array.Length; i++)
            {
                a[i] = array[i];
            }
            array = a;
        }

        public static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z');
        }

        public static bool IsOperation(char c)
        {
            return
                    (c == '+') ||
                    (c == '-') ||
                    (c == '*') ||
                    (c == '/');
        }

        public static bool IsBlank(this char c)
        {
            return
                    (c == ' ') ||
                    (c == '\t') ||
                    (c == '\r') ||
                    (c == '\n');
        }

    }

    struct Coordinates {
        public int col;
        public int row;
        public Sheet sheet;

        public Coordinates(int col, int row, Sheet sheet)
        {
            this.col = col;
            this.row = row;
            this.sheet = sheet;
        }
    }

    abstract class Cell {

        public enum Errors { OK, InvVal, Error, Div0, Cycle, MissOp, Formula }

        protected Errors error;

        protected int value = 0;

        public virtual int Value() { return value; }

        public bool HasError() { return error > 0; }

        public Errors GetError() { return error; }

        public override string ToString()
        {
            return error == Errors.OK ? value.ToString() : errorMessage(error);
        }

        static String errorMessage(Errors err)
        {
            switch (err)
            {
                case Errors.InvVal:
                    return "#INVVAL";
                case Errors.Error:
                    return "#ERROR";
                case Errors.Div0:
                    return "#DIV0";
                case Errors.Cycle:
                    return "#CYCLE";
                case Errors.MissOp:
                    return "#MISSOP";
                case Errors.Formula:
                    return "#FORMULA";
                default:
                    break;
            }
            throw new ArgumentException("This error does not have message assigned.");
        }

        public abstract void Append(char c);

        public virtual void Finish() { }

    }

    class ValueCell : Cell {

        public ValueCell() { }

        public ValueCell(int val)
        {
            this.value = val;
        }

        public override void Append(char c)
        {
            if (!Helper.IsNumeric(c))
            {
                error = Errors.InvVal;
                return;
            }

            value = value * 10 + (c - '0');

            // overflow? (only positive numbers in input)
            if (value < 0)
                error = Errors.InvVal;
        }
    }

    class ExpressionCell : Cell {

        char operation;
        Coordinates left, right;

        short evaluated = 0;
        static sbyte buildingState = 0;
        static StringBuilder linkingBuffer = new StringBuilder();

        public ExpressionCell()
        {
            left = new Coordinates();
            right = new Coordinates();
        }

        public override void Append(char c)
        {
            if (HasError() || (buildingState == 0 && c == '='))
                return;

            if ((buildingState == 0 || buildingState == 1) && (Helper.IsLetter(c) || Helper.IsNumeric(c)))
            {
                linkingBuffer.Append(c);
                buildingState = 1;
            }
            else if ((buildingState == 1) && (c == '!' || Helper.IsOperation(c)))
            {
                buildingState = 2;

                String buff = linkingBuffer.ToString();
                if (c == '!')
                {
                    this.left.sheet = Sheet.book.GetSheet(buff);
                    if (this.left.sheet == null)
                        error = Errors.Error;
                }
                else
                {
                    for (int i = 0; i < buff.Length; i++)
                        Append(buff[i]);

                    if (Helper.IsOperation(c))
                        Append(c);
                }

                linkingBuffer = new StringBuilder();
            }
            else if ((buildingState == 2 || buildingState == 3) && Helper.IsLetter(c))
            {
                left.col = left.col * 26 + (c - 'A' + 1);
                buildingState = 3;
            }
            else if ((buildingState == 3 || buildingState == 4) && Helper.IsNumeric(c))
            {
                left.row = left.row * 10 + (c - '0');
                buildingState = 4;
            }
            else if ((buildingState == 4) && Helper.IsOperation(c))
            {
                operation = c;
                buildingState = 5;
            }
            else if ((buildingState == 5 || buildingState == 6) && (Helper.IsLetter(c) || Helper.IsNumeric(c)))
            {
                linkingBuffer.Append(c);
                buildingState = 6;
            }
            else if ((buildingState == 6) && (c == '!' || Helper.IsOperation(c)))
            {
                buildingState = 7;

                String buff = linkingBuffer.ToString();
                if (c == '!')
                {
                    this.right.sheet = Sheet.book.GetSheet(buff);
                    if (this.right.sheet == null)
                        error = Errors.Error;
                }
                else
                {
                    for (int i = 0; i < buff.Length; i++)
                        Append(buff[i]);

                    if (Helper.IsOperation(c))
                        Append(c);
                }

                linkingBuffer = new StringBuilder();
            }
            else if ((buildingState == 7 || buildingState == 8) && Helper.IsLetter(c))
            {
                right.col = right.col * 26 + (c - 'A' + 1);
                buildingState = 8;
            }
            else if ((buildingState == 8 || buildingState == 9) && Helper.IsNumeric(c))
            {
                right.row = right.row * 10 + (c - '0');
                buildingState = 9;
            }
            else
            {
                error = (operation == 0 && !Helper.IsOperation(c)) ? Errors.MissOp : Errors.Formula;
            }
        }

        public override void Finish()
        {
            base.Finish();

            if (left.sheet == null)
                left.sheet = Sheet.CurrentSheet;
            if (right.sheet == null)
                right.sheet = Sheet.CurrentSheet;

            if (linkingBuffer.Length > 0)
            {
                buildingState = 7;
                String buff = linkingBuffer.ToString();
                for (int i = 0; i < buff.Length; i++)
                    Append(buff[i]);
                linkingBuffer = new StringBuilder();
            }

            if (!HasError())
                if (operation == 0)
                    error = Errors.MissOp;
                else if (buildingState < 9)
                    error = Errors.Formula;

            buildingState = 0;

            left.row--; left.col--;
            right.row--; right.col--;
        }

        public override int Value()
        {
            if (HasError() || evaluated == -1)
                return value;

            evaluated++;

            Cell leftCell = left.sheet.Find(left);
            Cell rightCell = right.sheet.Find(right);

            if (leftCell.HasError() || rightCell.HasError())
                error = Errors.Error;

            if (evaluated >= 3 || leftCell == this || rightCell == this)
                error = Errors.Cycle;

            if (HasError() || evaluated == -1)
                return value;

            switch (operation)
            {
                case '+':
                    value = leftCell.Value() + rightCell.Value();
                    break;

                case '-':
                    value = leftCell.Value() - rightCell.Value();
                    break;

                case '*':
                    value = leftCell.Value() * rightCell.Value();
                    break;

                case '/':
                    if (rightCell.Value() == 0)
                        this.error = Errors.Div0;
                    else
                        value = leftCell.Value() / rightCell.Value();
                    break;

                default:
                    throw new InvalidDataException("Operator not found.");
            }

            if (leftCell.HasError() || rightCell.HasError())
                error = Errors.Cycle;

            evaluated = -1;

            return value;
        }

    }

    class EmptyCell : Cell {

        public override void Append(char c)
        {
            if ((c != '[') && (c != ']'))
                error = Errors.InvVal;
        }

        public override string ToString()
        {
            return "[]";
        }

    }

    class Sheet {

        /// <summary>
        /// Primary data storage for cells
        /// </summary>
        Cell[][] rows = new Cell[4][];

        int rowsUsed;

        StreamReader reader;

        public static Book book;

        static ValueCell zeroCell = new ValueCell(0);

        public static Sheet CurrentSheet;

        bool initialized;

        public Sheet(Book book, StreamReader reader)
        {
            this.reader = reader;
            rowsUsed = 0;
        }

        void addCell(ref Cell cell, ref Cell[] buffer, ref int bufferUsed)
        {
            if (bufferUsed >= buffer.Length - 1)
                Helper.EnlargeArray(ref buffer);

            cell.Finish();
            buffer[bufferUsed++] = cell;
            cell = null;
        }

        public Sheet Load()
        {
            String ln;
            Cell[] buffer = new Cell[4];
            int bufferUsed = 0;
            initialized = true;
            CurrentSheet = this;

            Cell c = null;

            while ((ln = reader.ReadLine()) != null)
            {
                for (int i = 0; i < ln.Length; i++)
                {
                    // If there was an error, skip all characters to first blank
                    if (c != null && c.HasError())
                    {
                        addCell(ref c, ref buffer, ref bufferUsed);

                        while (i + 1 < ln.Length && !ln[i + 1].IsBlank()) { i++; }
                        continue;
                    }

                    // Skip all blank characters
                    if (Helper.IsBlank(ln[i]))
                    {
                        if (c != null)
                            addCell(ref c, ref buffer, ref bufferUsed);

                        while (i + 1 < ln.Length && Helper.IsBlank(ln[i + 1])) { i++; }
                        continue;
                    }

                    // Create cell depending on first character
                    if (c == null)
                    {
                        switch (ln[i])
                        {
                            case '[':
                                c = new EmptyCell();
                                break;
                            case '=':
                                c = new ExpressionCell();
                                break;
                            default:
                                c = new ValueCell();
                                break;
                        }
                    }

                    c.Append(ln[i]);
                }

                if (c != null)
                    addCell(ref c, ref buffer, ref bufferUsed);

                addRow(buffer, bufferUsed);
                bufferUsed = 0;
            }
            return this;
        }

        void addRow(Cell[] buffer, int bufferUsed)
        {
            if (rowsUsed >= rows.GetLength(0) - 1)
                Helper.EnlargeArray(ref rows);

            rows[rowsUsed] = new Cell[bufferUsed];
            for (int i = 0; i < bufferUsed; i++)
            {
                rows[rowsUsed][i] = buffer[i];
                buffer[i] = null;
            }

            rowsUsed++;
        }

        public Sheet Evaluate()
        {
            for (int i = 0; i < rowsUsed; i++)
                for (int j = 0; j < rows[i].Length; j++)
                    if (rows[i][j] is ExpressionCell)
                        ((ExpressionCell)rows[i][j]).Value();
            return this;
        }

        public void Print(TextWriter writer)
        {
            for (int i = 0; i < rowsUsed; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                {
                    if (j >= 1)
                        writer.Write(" ");

                    writer.Write(rows[i][j].ToString());
                }
                writer.WriteLine();
            }
            writer.Close();
        }

        public Cell Find(Coordinates coord)
        {
            if (!initialized)
                Load();

            if (coord.row < 0 ||
                coord.col < 0 ||
                coord.row >= rowsUsed ||
                coord.col >= rows[coord.row].Length
                )
                return zeroCell;

            return rows[coord.row][coord.col];
        }

    }

    class Book {

        Dictionary<String, Sheet> sheets;
        Sheet mainSheet;

        public Book(String mainSheetName)
        {
            sheets = new Dictionary<String, Sheet>();
            Sheet.book = this;
            mainSheet = addSheet(mainSheetName).Load();
        }

        Sheet addSheet(String name)
        {
            Sheet sheet = new Sheet(this, File.OpenText(name));
            sheets.Add(name, sheet);
            return sheet;
        }

        public void PrintMainSheet(TextWriter writer) { mainSheet.Evaluate().Print(writer); }

        public Sheet GetSheet(String sheetName)
        {
            if (sheets.ContainsKey(sheetName + ".sheet"))
                return sheets[sheetName + ".sheet"];

            try { return addSheet(sheetName + ".sheet"); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (System.Security.SecurityException) { return null; }
        }

    }

    class Program {
        public static void ReportArgumentError() { Console.WriteLine("Argument Error"); }

        public static void ReportFileError() { Console.WriteLine("File Error"); }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                ReportArgumentError();
                return;
            }

            try
            {
                Book book = new Book(args[0]);
                book.PrintMainSheet(File.CreateText(args[1]));
            }
            catch (IOException) { ReportFileError(); }
            catch (UnauthorizedAccessException) { ReportFileError(); }
            catch (System.Security.SecurityException) { ReportFileError(); }
        }
    }
}
