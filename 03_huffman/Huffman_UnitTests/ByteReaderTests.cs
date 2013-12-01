using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Huffman;
using System.IO;

namespace Huffman_UnitTests
{
    [TestClass]
    public class ByteReaderTests
    {
        [TestMethod]
        public void ReadByte_EmptyInput()
        {
            var reader = new ByteReader(new MemoryStream());

            Assert.IsTrue(reader.IsEnd());
        }

        [TestMethod]
        public void ReadByte_NonEmptyInput()
        {
            var result = new[] { 'a', 's', 'd' };
            byte[] input = new byte[result.Length];
            for (int i = 0; i < result.Length; i++)
                input[i] = Convert.ToByte(result[i]);

            var reader = new ByteReader(new MemoryStream(input, 0, input.Length));

            foreach (byte oneByte in input)
            {
                Assert.AreEqual(oneByte, reader.ReadByte());
            }

            Assert.IsTrue(reader.IsEnd());
        }
    }
}
