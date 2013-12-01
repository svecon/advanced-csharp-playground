using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Huffman;
using System.IO;

namespace Huffman_UnitTests {

    [TestClass]
    public class OrderedForestTests {

        void checkOrder(OrderedForest forest, byte[] input, byte[] result)
        {

            foreach (byte item in input)
                forest.ProcessByte(item);
            forest.Finish();

            foreach (byte item in result)
                Assert.AreEqual(item, forest.GetMin().key);

            Assert.IsNull(forest.GetMin());
        }

        [TestMethod]
        public void Forest_Empty()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { };
            byte[] result = new byte[] { };

            checkOrder(forest, input, result);

            Assert.IsNull(forest.JoinAllTrees());
        }

        [TestMethod]
        public void Forest_StableOrdering()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 30 };
            byte[] result = new byte[] { 10, 20, 30 };

            checkOrder(forest, input, result);
        }

        [TestMethod]
        public void Forest_LowestToLargest()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 20, 30, 30, 30 };
            byte[] result = new byte[] { 10, 20, 30 };

            checkOrder(forest, input, result);
        }

        [TestMethod]
        public void Forest_LargestToLowest()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 10, 10, 20, 20, 30 };
            byte[] result = new byte[] { 30, 20, 10 };

            checkOrder(forest, input, result);
        }

        [TestMethod]
        public void Forest_LowestToLargestStable()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 30, 20, 30 };
            byte[] result = new byte[] { 10, 20, 30 };

            checkOrder(forest, input, result);
        }

        [TestMethod]
        public void Forest_LargestToLowestStable()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 10, 20, 20, 30 };
            byte[] result = new byte[] { 30, 10, 20 };

            checkOrder(forest, input, result);
        }

        //
        
        void checkJoinedTree(OrderedForest forest, byte[] input, string result)
        {
            foreach (byte item in input)
                forest.ProcessByte(item);
            forest.Finish();

            Assert.AreEqual(result, forest.JoinAllTrees().ToString());
        }

        [TestMethod]
        public void Forest_Tree_Empty()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { };
            byte[] result = new byte[] { };

            checkOrder(forest, input, result);

            Assert.IsNull(forest.JoinAllTrees());
        }

        [TestMethod]
        public void Forest_Tree_StableOrdering()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 30 };

            checkJoinedTree(forest, input, "3 *30:1 2 *10:1 *20:1");
        }

        [TestMethod]
        public void Forest_Tree_LowestToLargest()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 20, 30, 30, 30 };

            checkJoinedTree(forest, input, "6 *30:3 3 *10:1 *20:2");
        }

        [TestMethod]
        public void Forest_Tree_LargestToLowest()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 10, 10, 20, 20, 30 };

            checkJoinedTree(forest, input, "6 *10:3 3 *30:1 *20:2");
        }

        [TestMethod]
        public void Forest_Tree_LowestToLargestStable()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 20, 30, 20, 30 };

            checkJoinedTree(forest, input, "5 *30:2 3 *10:1 *20:2");
        }

        [TestMethod]
        public void Forest_Tree_LargestToLowestStable()
        {
            var forest = new OrderedForest();
            byte[] input = new byte[] { 10, 10, 20, 20, 30 };

            checkJoinedTree(forest, input, "5 *20:2 3 *30:1 *10:2");
        }

        [TestMethod]
        public void Forest_Tree_Codex()
        {
            var inputString = "aaabbc";
            byte[] input = new byte[inputString.Length];
            for (int i = 0; i < inputString.Length; i++)
                input[i] = Convert.ToByte(inputString[i]);

            var reader = new ByteReader(new MemoryStream(input, 0, input.Length));

            var forest = new OrderedForest();

            checkJoinedTree(forest, input, "6 *97:3 3 *99:1 *98:2");
        }

    }
}
