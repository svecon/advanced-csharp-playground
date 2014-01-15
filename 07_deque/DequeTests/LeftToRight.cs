using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DequeTests {
    [TestClass]
    public class LeftToRight {
        [TestMethod]
        public void Add_Count_Foreach()
        {
            int[] values = new int[3];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length; i++)
            {
                Random r = new Random();
                values[i] = r.Next(1000);
                list.Add(values[i]);
            }

            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(values[j], item);
                j++;
            }

            Assert.AreEqual(values.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_Around()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length; i++)
            {
                Random r = new Random();
                values[i] = r.Next(1000);
                list.Add(values[i]);
            }

            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(values[j], item);
                j++;
            }

            Assert.AreEqual(values.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_Flipped()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length; i++)
            {
                Random r = new Random();
                values[i] = r.Next(1000);
                list.Add(values[i]);
            }

            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(values[j], item);
                j++;
            }

            var listFlipped = list.Flip();
            j = values.Length - 1;
            foreach (var item in list)
            {
                Assert.AreEqual(values[j], item);
                j--;
            }

            Assert.AreEqual(values.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_RemoveAt()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            Random r = new Random();
            for (int i = 0; i < values.Length - 1; i++)
            {
                values[i] = i;
                list.Add(values[i]);
            }

            list.RemoveAt(4);

            var j = 0;
            foreach (var item in list)
            {
                if (j == 1)
                {
                    j++;
                }
                Assert.AreEqual(values[j], item);
                j++;
            }

            Assert.AreEqual(values.Length - 2, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_RemoveAtFlipped()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = i;
                list.Add(values[i]);
            }
            list = list.Flip();
            list.RemoveAt(5);

            var correct = new int[] { 6, 5, 4, 3, 1, 0 };
            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(correct[j], item);
                j++;
            }

            Assert.AreEqual(correct.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_Insert()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length - 1; i++)
            {
                values[i] = i;
                list.Add(values[i]);
            }

            list.Insert(6, 99);

            var correct = new int[] { 0, 1, 2, 99, 3, 4, 5 };
            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(correct[j], item);
                j++;
            }

            Assert.AreEqual(correct.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_InsertRound()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length - 1; i++)
            {
                values[i] = i;
                list.Add(values[i]);
            }

            list.Insert(1, 99);

            var correct = new int[] { 0, 1, 2, 3, 4, 99, 5 };
            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(correct[j], item);
                j++;
            }

            Assert.AreEqual(correct.Length, list.Count);
        }

        [TestMethod]
        public void Add_Count_Foreach_InsertFlipped()
        {
            int[] values = new int[7];

            var list = new Deque<int>();
            for (int i = 0; i < values.Length - 1; i++)
            {
                values[i] = i;
                list.Add(values[i]);
            }

            list = list.Flip();
            list.Insert(1, 99);

            var correct = new int[] { 5, 99, 4, 3, 2, 1, 0 };
            var j = 0;
            foreach (var item in list)
            {
                Assert.AreEqual(correct[j], item);
                j++;
            }

            Assert.AreEqual(correct.Length, list.Count);
        }
    }
}
