using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Huffman_UnitTests")]

namespace Huffman {

    interface IByteReader {
        byte ReadByte();
    }

    interface IByteProcessor {
        void ProcessByte(byte b);
        void Finish();
    }

    interface IJoinable<T> {
        T join(T item);
    }

    class ByteReader : IByteReader, IDisposable {
        const int BLOCK_SIZE = 16384; // 16kB

        Stream reader;
        byte[] cache;
        int cacheSize;
        int current;

        public ByteReader(Stream reader)
        {
            this.reader = reader;
            cache = new byte[BLOCK_SIZE];
        }

        bool loadToBuffer()
        {
            if (cacheSize <= current)
            {
                current = 0;
                cacheSize = reader.Read(cache, 0, BLOCK_SIZE);

                if (cacheSize == 0)
                    return false;
            }

            return true;
        }

        public bool IsEnd()
        {
            return !loadToBuffer();
        }

        public byte ReadByte()
        {
            if (!loadToBuffer())
                throw new EndOfStreamException();

            return cache[current++];
        }

        public ulong ReadLong()
        {
            ulong ret = 0;

            var temp = new byte[8];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = ReadByte();

            for (int i = temp.Length - 1; i >= 0; i--)
            {
                ret <<= 8;
                ret |= temp[i];
            }

            return ret;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }

    class OrderedForest : IByteProcessor {

        HuffmanTree[] forest;
        List<HuffmanTree> order;

        public OrderedForest()
        {
            forest = new HuffmanTree[256];
            order = new List<HuffmanTree>();
        }

        public void ProcessByte(byte b)
        {
            if (forest[b] != null)
                forest[b].value++;
            else
            {
                forest[b] = new HuffmanTree(b);
                order.Add(forest[b]);
            }
        }

        public void Finish()
        {
            this.forest = null;

            order.Sort();
        }

        public HuffmanTree GetMin()
        {
            if (order.Count == 0)
                return null;

            int index = 0;
            var min = order[index];
            order.RemoveAt(index);

            return min;
        }

        public void JoinTwoSmallestTrees()
        {
            order.Add(GetMin().join(GetMin()));
            order.Sort();
        }

        public HuffmanTree JoinAllTrees()
        {
            if (order.Count == 0)
                return null;

            while (order.Count > 1)
                JoinTwoSmallestTrees();

            var last = order[0];
            order.Clear();
            return last;
        }
    }

    class HuffmanTree : IJoinable<HuffmanTree>, IComparable<HuffmanTree> {

        public class HuffmanNode {

            public static int totalAge = 0;

            public byte key;
            public ulong value;
            public HuffmanNode left;
            public HuffmanNode right;
            public bool isLeaf;
            public int age;

            public HuffmanNode(byte key)
            {
                init(key, null, null);
                isLeaf = true;
            }

            public HuffmanNode(byte key, HuffmanNode left, HuffmanNode right)
            {
                init(key, left, right);
                isLeaf = false;
            }

            void init(byte key, HuffmanNode left, HuffmanNode right)
            {
                this.key = key;
                this.left = left;
                this.right = right;
                this.value = 1;
                this.age = totalAge++;
            }

            public void Print(StringBuilder writer)
            {
                if (writer.Length != 0) writer.Append(" ");

                if (this.isLeaf)
                    writer.AppendFormat("*{0}:{1}", key, value);
                else
                {
                    writer.Append(this.value);
                    left.Print(writer);
                    right.Print(writer);
                }
            }

            public void Encode(BinaryWriter writer)
            {
                ulong buffer = 0;

                if (isLeaf)
                    buffer |= key;

                buffer <<= 55;

                buffer |= (value & 0x7FFFFFFFFFFFFF);

                buffer <<= 1;

                if (isLeaf)
                    buffer |= 0x01;

                writer.Write(buffer);

                if (!isLeaf)
                {
                    left.Encode(writer);
                    right.Encode(writer);
                }
            }

            public static HuffmanNode Decode(ulong num)
            {
                bool isLeaf = (num & 0x1) == 1;
                num >>= 1;

                ulong count = (num & 0x7FFFFFFFFFFFFF);
                num >>= 55;

                byte key = (byte)num;

                HuffmanNode node = new HuffmanNode(key);
                node.isLeaf = isLeaf;
                node.value = count;

                return node;
            }
        }

        public HuffmanNode root;

        public ulong value
        {
            get { return root.value; }
            set { root.value = value; }
        }

        public byte key
        {
            get { return root.key; }
            set { root.key = value; }
        }

        public HuffmanTree() { prefixStack = new Stack<HuffmanNode>(); }

        public HuffmanTree(byte key)
        {
            root = new HuffmanNode(key);
        }

        public HuffmanTree(byte key, HuffmanNode left, HuffmanNode right)
        {
            root = new HuffmanNode(key, left, right);
            root.value = left.value + right.value;
        }

        public HuffmanTree join(HuffmanTree item)
        {
            switch (CompareTo(item))
            {
                case -1: return new HuffmanTree(0, this.root, item.root);
                case 1: return new HuffmanTree(0, item.root, this.root);
                default: throw new InvalidDataException();
            }
        }

        public override string ToString()
        {
            var writer = new StringBuilder();
            root.Print(writer);
            return writer.ToString();
        }

        public void EncodeTree(BinaryWriter writer)
        {
            root.Encode(writer);
            writer.Write(0x00UL);
        }

        public int CompareTo(HuffmanTree other)
        {
            if (this.value < other.value)
                return -1;
            if (this.value > other.value)
                return 1;

            if (this.root.isLeaf && !other.root.isLeaf)
                return -1;
            if (!this.root.isLeaf && other.root.isLeaf)
                return 1;

            if (this.key < other.key)
                return -1;
            if (this.key > other.key)
                return 1;

            if (this.root.age < other.root.age)
                return -1;
            if (this.root.age > other.root.age)
                return 1;

            return 0;
        }

        Stack<HuffmanNode> prefixStack;
        public void AddPrefixNode(HuffmanNode node)
        {
            if (root == null) { root = node; prefixStack.Push(node); return; }

            if (prefixStack.Count == 0)
                throw new FormatException("Error durign building a tree: empty stack.");

            HuffmanNode parent = prefixStack.Peek();

            if (parent.left == null)
                parent.left = node;
            else if (parent.right == null)
                parent.right = node;
            else
                throw new Exception("Invalid node?");

            if (!node.isLeaf)
            {
                prefixStack.Push(node);
            }
            else if (node.isLeaf && parent.right != null)
            {
                while (prefixStack.Count >= 1 && prefixStack.Peek().right != null)
                    prefixStack.Pop();
            }
        }
    }

    class EncodeFile : IByteProcessor {

        bool[][] byteEncoding;
        BinaryWriter writer;
        byte buffer = 0;
        byte bufferSize;
        HuffmanTree tree;

        public static byte[] header = { 0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66 };

        public EncodeFile(HuffmanTree t, BinaryWriter writer)
        {
            this.writer = writer;
            byteEncoding = new bool[256][];
            transformTree(t.root, new List<bool>());

            this.tree = t;
        }

        #region for UNIT TESTING only! do not use this
        internal EncodeFile(BinaryWriter writer)
        {
            this.writer = writer;
            byteEncoding = new bool[256][];
        }

        internal void addEncoding(byte b, bool[] en)
        {
            byteEncoding[b] = en;
        }
        #endregion

        public void WriteHeaders()
        {
            for (int i = 0; i < header.Length; i++)
                writer.Write(header[i]);
        }

        public void WriteEncodedTree()
        {
            tree.EncodeTree(writer);
        }

        void transformTree(HuffmanTree.HuffmanNode node, List<bool> encoding)
        {
            if (node.isLeaf)
            {
                byteEncoding[node.key] = encoding.ToArray();
                return;
            }

            encoding.Add(false);
            transformTree(node.left, encoding);
            encoding.RemoveAt(encoding.Count - 1);

            encoding.Add(true);
            transformTree(node.right, encoding);
            encoding.RemoveAt(encoding.Count - 1);
        }

        public void ProcessByte(byte b)
        {
            for (int i = 0; i < byteEncoding[b].Length; i++)
            {
                if (bufferSize >= 8)
                    flushBuffer();

                buffer >>= 1;
                if (byteEncoding[b][i])
                    buffer |= 0x80;
                bufferSize++;
            }
        }

        void flushBuffer()
        {
            buffer >>= 8 - bufferSize;

            writer.Write(buffer);

            buffer = 0;
            bufferSize = 0;
        }

        public void Finish()
        {
            flushBuffer();
            writer.Close();
        }

    }

    class DecodeFile {

        ByteReader reader;
        BinaryWriter writer;
        HuffmanTree tree;
        HuffmanTree.HuffmanNode treePosition;

        public DecodeFile(ByteReader reader, BinaryWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
            this.tree = new HuffmanTree();
        }

        public DecodeFile checkHeader()
        {
            for (int i = 0; i < EncodeFile.header.Length; i++)
            {
                if (reader.ReadByte() != EncodeFile.header[i])
                {
                    throw new FormatException("Wrong header");
                }
            }

            return this;
        }

        public DecodeFile buildTree()
        {
            ulong node;

            while ((node = reader.ReadLong()) != 0)
            {
                tree.AddPrefixNode(HuffmanTree.HuffmanNode.Decode(node));
            }

            return this;
        }

        public DecodeFile decodeStream()
        {
            byte stream;
            treePosition = tree.root;

            while (!reader.IsEnd())
            {
                stream = reader.ReadByte();

                for (int i = 0; i < 8; i++)
                {
                    if ((stream & 0x01) == 0)
                        treePosition = treePosition.left;
                    else
                        treePosition = treePosition.right;

                    stream >>= 1;

                    if (!treePosition.isLeaf) continue;

                    if (treePosition.value == 0)
                    {
                        treePosition = tree.root;
                        continue;
                    }
                    else
                    {
                        treePosition.value--;
                        writer.Write(treePosition.key);
                    }

                    treePosition = tree.root;

                }

            }

            writer.Close();

            return this;
        }

    }

    class Program {
        static void ReportFileError() { Console.WriteLine("File Error"); }
        static void ReportArgumentError() { Console.WriteLine("Argument Error"); }

        static ByteReader reader;
        static BinaryWriter writer;

        static void Main(string[] args)
        {
            decode(args);
        }

        static void decode(string[] args)
        {
            if (args.Length != 1 || args[0] == "" || !args[0].EndsWith(".huff") || args[0].Length - ".huff".Length == 0)
            {
                ReportArgumentError();
                return;
            }

            try
            {
                reader = new ByteReader(File.OpenRead(args[0]));
                writer = new BinaryWriter(File.OpenWrite(args[0].Remove(args[0].Length - ".huff".Length)));

                DecodeFile encoder = new DecodeFile(reader, writer);
                encoder.checkHeader().buildTree().decodeStream();

            }
            catch (EndOfStreamException) { ReportFileError(); }
            catch (NullReferenceException) { ReportFileError(); }
            catch (FormatException) { ReportFileError(); }
            catch (FileNotFoundException) { ReportFileError(); }
            catch (IOException) { ReportFileError(); }
            catch (UnauthorizedAccessException) { ReportFileError(); }
            catch (System.Security.SecurityException) { ReportFileError(); }
            finally { if (reader != null) reader.Dispose(); if (writer != null) writer.Close(); }
        }

        static void encode(string[] args)
        {
            if (args.Length != 1 || args[0] == "")
            {
                ReportArgumentError();
                return;
            }

            try
            {
                reader = new ByteReader(File.OpenRead(args[0]));
                var forest = new OrderedForest();

                while (!reader.IsEnd())
                    forest.ProcessByte(reader.ReadByte());
                forest.Finish();
                reader.Dispose();

                var tree = forest.JoinAllTrees();
                //if (tree != null)
                //Console.WriteLine(tree.ToString());

                writer = new BinaryWriter(File.Open(args[0] + ".huff", FileMode.Create));
                reader = new ByteReader(File.OpenRead(args[0]));
                EncodeFile encoder = new EncodeFile(tree, writer);
                encoder.WriteHeaders();
                encoder.WriteEncodedTree();

                while (!reader.IsEnd())
                    encoder.ProcessByte(reader.ReadByte());
                encoder.Finish();
                reader.Dispose();
            }
            catch (FileNotFoundException) { ReportFileError(); }
            catch (IOException) { ReportFileError(); }
            catch (UnauthorizedAccessException) { ReportFileError(); }
            catch (System.Security.SecurityException) { ReportFileError(); }
            finally { if (reader != null) reader.Dispose(); }
        }
    }
}
