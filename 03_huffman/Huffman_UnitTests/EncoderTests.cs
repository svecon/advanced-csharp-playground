using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Huffman;
using System.IO;

namespace Huffman_UnitTests {
    [TestClass]
    public class EncoderTests {

        byte[] resultMem;
        BinaryWriter writer;
        EncodeFile en;
        const int HEADER_LENGTH = 8;

        protected void init() {
            resultMem = new byte[1024];
            writer = new BinaryWriter(new MemoryStream(resultMem));
            en = new EncodeFile(writer);

            en.addEncoding(97, new bool[] { false });
            en.addEncoding(98, new bool[] { true, true });
            en.addEncoding(99, new bool[] { true, false, false });
            en.addEncoding(100, new bool[] { true, false, true });

            en.WriteHeaders();
        }

        [TestMethod]
        public void Encoder_Header()
        {
            init();

            var result = new int[] { 0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66 };

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(result[i], resultMem[i]);
        }

        [TestMethod]
        public void Encoder_CodexExample_1()
        {
            init();

            var input = new char[] { 'b', 'd', 'a', 'a', 'c', 'b' };
            var result = new int[] { 0x97, 0x0C };

            for (int i = 0; i < input.Length; i++)
                en.ProcessByte(Convert.ToByte(input[i]));
            en.Finish();

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(result[i], resultMem[HEADER_LENGTH + i]);

        }

        [TestMethod]
        public void Encoder_CodexExample_2()
        {
            init();

            var input = new char[] { 'b', 'd', 'a', 'a', 'c', 'b', 'a', 'a' };
            var result = new int[] { 0x97, 0x0C };

            for (int i = 0; i < input.Length; i++)
                en.ProcessByte(Convert.ToByte(input[i]));
            en.Finish();

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(result[i], resultMem[HEADER_LENGTH + i]);
        }

        [TestMethod]
        public void Encoder_FullStack()
        {
            var inputString = "aaabbc";
            byte[] input = new byte[inputString.Length];
            for (int i = 0; i < inputString.Length; i++)
                input[i] = Convert.ToByte(inputString[i]);

            var reader = new ByteReader(new MemoryStream(input, 0, input.Length));

            var forest = new OrderedForest();

            foreach (byte item in input)
                forest.ProcessByte(item);
            forest.Finish();

            //Assert.AreEqual(result, forest.JoinAllTrees().ToString());
            //checkJoinedTree(forest, input, "6 *97:3 3 *99:1 *98:2");
            var tree = forest.JoinAllTrees();
            resultMem = new byte[1024];
            writer = new BinaryWriter(new MemoryStream(resultMem));
            en = new EncodeFile(tree, writer);
            en.WriteHeaders();
            en.WriteEncodedTree();

            var result = new int[] { 0x7b, 0x68, 0x75, 0x7c, 0x6d, 0x7d, 0x66, 0x66, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x61, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf8, 0x00 };

            for (int i = 0; i < input.Length; i++)
                en.ProcessByte(Convert.ToByte(input[i]));
            en.Finish();

            for (int i = 0; i < result.Length; i++)
                Assert.AreEqual(result[i], resultMem[i]);
        }

    }
}
