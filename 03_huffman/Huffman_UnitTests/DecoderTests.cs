using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using Huffman;

namespace Huffman_UnitTests {
    [TestClass]
    public class DecoderTests {

        BinaryWriter writer;
        ByteReader reader;
        ByteReader check;

        void checkFile(String file)
        {
            reader = new ByteReader(File.OpenRead("decoding" + Path.DirectorySeparatorChar + file + ".huff"));
            var resultMem = new byte[1024*16];
            writer = new BinaryWriter(new MemoryStream(resultMem));

            DecodeFile encoder = new DecodeFile(reader, writer);
            encoder.checkHeader().buildTree().decodeStream();

            check = new ByteReader(File.OpenRead("decoding" + Path.DirectorySeparatorChar + file));

            int i = 0;
            while (!check.IsEnd())
            {
                Assert.AreEqual(check.ReadByte(), resultMem[i++]);
            }
        }

        [TestMethod]
        public void Decoder_SimpleTests()
        {
            String[] testFiles = { "simple.in", "simple2.in", "simple3.in", "simple4.in" };

            foreach (String file in testFiles)
            {
                checkFile(file);
            }
        }

        [TestMethod]
        public void Decoder_BinaryTests()
        {
            String[] testFiles = { "binary.in" };

            foreach (String file in testFiles)
            {
                checkFile(file);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Decoder_FormatExceptionTests()
        {
            String[] testFiles = { "binary_bad.in", "simple4_bad.in", "simple4_bad2.in", "simple4_bad3.in" };

            foreach (String file in testFiles)
            {
                checkFile(file);
            }
        }
    }
}
