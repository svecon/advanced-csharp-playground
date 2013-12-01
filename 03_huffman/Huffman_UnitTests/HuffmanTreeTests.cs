using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Huffman;

namespace Huffman_UnitTests {
    [TestClass]

    public class HuffmanTreeTests {

        [TestMethod]
        public void Huffman_Creation()
        {
            byte input = 10;
            var huffman = new HuffmanTree(input);
        }

        [TestMethod]
        public void Huffman_Addition()
        {
            var huffman = new HuffmanTree(10);
            huffman.value++;
            huffman.value++;

            Assert.AreEqual(3u, huffman.value);
        }

        [TestMethod]
        public void Huffman_SimpleJoin()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);
            huffman1.value++;

            var joined = huffman1.join(huffman2);

            Assert.AreEqual(3u, joined.value);
            Assert.AreEqual(1u, joined.root.left.value);
            Assert.AreEqual(2u, joined.root.right.value);
        }

        [TestMethod]
        public void Huffman_Join_LeavesAreLighter()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);
            var huffman3 = new HuffmanTree(30);
            huffman1.value++;
            huffman1.value++;
            huffman2.value++;

            var joined1 = huffman2.join(huffman3);
            var joined2 = huffman1.join(joined1);

            Assert.AreEqual(3u, joined1.value);
            Assert.AreEqual(3u, huffman1.value);

            Assert.AreEqual(6u, joined2.value);
            Assert.AreEqual(10u, joined2.root.left.key);
        }

        [TestMethod]
        public void Huffman_Join_BytesAreLighter()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);

            var joined = huffman1.join(huffman2);

            Assert.AreEqual(2u, joined.value);
            Assert.AreEqual(10u, joined.root.left.key);
            Assert.AreEqual(20u, joined.root.right.key);
        }

        [TestMethod]
        public void Huffman_Age()
        {
            HuffmanTree.HuffmanNode.totalAge = 0;
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);
            var huffman3 = new HuffmanTree(30);
            huffman1.value++;
            huffman1.value++;
            huffman2.value++;

            var joined1 = huffman2.join(huffman3);
            var joined2 = huffman1.join(joined1);

            Assert.AreEqual(0, huffman1.root.age);
            Assert.AreEqual(1, huffman2.root.age);
            Assert.AreEqual(2, huffman3.root.age);
            Assert.AreEqual(3, joined1.root.age);
            Assert.AreEqual(4, joined2.root.age);
        }

        //////

        [TestMethod]
        public void Huffman_Print_Creation()
        {
            byte input = 10;
            var huffman = new HuffmanTree(input);

            Assert.AreEqual("*10:1", huffman.ToString());
        }

        [TestMethod]
        public void Huffman_Print_Addition()
        {
            var huffman = new HuffmanTree(10);
            huffman.value++;
            huffman.value++;

            Assert.AreEqual("*10:3", huffman.ToString());
        }

        [TestMethod]
        public void Huffman_Print_SimpleJoin()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);
            huffman1.value++;

            var joined = huffman1.join(huffman2);

            Assert.AreEqual("3 *20:1 *10:2", joined.ToString());
        }

        [TestMethod]
        public void Huffman_Print_Join_LeavesAreLighter()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);
            var huffman3 = new HuffmanTree(30);
            huffman1.value++;
            huffman1.value++;
            huffman2.value++;

            var joined1 = huffman2.join(huffman3);
            var joined2 = huffman1.join(joined1);

            Assert.AreEqual("6 *10:3 3 *30:1 *20:2", joined2.ToString());
        }

        [TestMethod]
        public void Huffman_Print_Join_BytesAreLighter()
        {
            var huffman1 = new HuffmanTree(10);
            var huffman2 = new HuffmanTree(20);

            var joined = huffman1.join(huffman2);

            Assert.AreEqual("2 *10:1 *20:1", joined.ToString());
        }

    }
}