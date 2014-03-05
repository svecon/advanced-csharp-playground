using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParallelMergeSort;

namespace ParallelMergeSortTests {
    [TestClass]
    public class SortTests {
        private List<int> GenRandomList(int length) {
            var list = new List<int>(length);
            Random random = new Random(12345);
            for (int i = 0; i < length; ++i) {
                list.Add(random.Next());
            }
            return list;
        }

        private void TestSorted(int[] array) {
            for (int i = 0; i < array.Length-1; ++i) {
                Assert.IsTrue(array[i] <= array[i + 1], "List unordered at " + i);
            }
        }

        private void Test(int nElements, int nThreads) {
            
            int repetitions = 10;
            StreamWriter measure = new StreamWriter("measure.txt", true);
            measure.WriteLine("--TEST--");

            long avg = 0;
            for (int i = 0; i < repetitions; ++i) {
                int[] arr = GenRandomList(nElements).ToArray();
                
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var sorter = new ParallelMergeSort<int>(arr);
                arr = sorter.Sort(nThreads);

                sw.Stop();
                measure.WriteLine(
                    "Threads: {0} Elements: {1} Iteration: {2} Elapsed: {3}ms",
                    nThreads, nElements, i, sw.ElapsedMilliseconds);
                TestSorted(arr);
                avg += sw.ElapsedMilliseconds;
            }
            avg /= repetitions;
            measure.WriteLine("Test average: {0}", avg);
            measure.Close();
        }

        [TestMethod]
        public void Elements10_1Threaded() {
            Test(10, 1);
        }
        [TestMethod]
        public void Elements10_2Threaded() {
            Test(10, 2);
        }
        [TestMethod]
        public void Elements10_3Threaded() {
            Test(10, 3);
        }
        [TestMethod]
        public void Elements10_4Threaded() {
            Test(10, 4);
        }
        [TestMethod]
        public void Elements1000000_1Threaded() {
            Test(1000000, 1);
        }
        [TestMethod]
        public void Elements1000000_2Threaded() {
            Test(1000000, 2);
        }
        [TestMethod]
        public void Elements1000000_3Threaded() {
            Test(1000000, 3);
        }
        [TestMethod]
        public void Elements1000000_4Threaded() {
            Test(1000000, 4);
        }
        [TestMethod]
        public void Elements1000000_256Threaded() {
            Test(1000000, 256);
        }
        [TestMethod]
        public void Elements10000000_3Threaded()
        {
            Test(10000000, 3);
        }
        [TestMethod]
        public void Elements10000000_4Threaded()
        {
            Test(10000000, 4);
        }
    }

    [TestClass]
    public class ProgramTest {
        public void Test(string[] args, string input, string expOutput) {
            StringReader reader = new StringReader(input);
            StringBuilder builder = new StringBuilder();
            StringWriter writer = new StringWriter(builder);
            Program.Run(args, reader, writer);
            string output = builder.ToString();
            Assert.AreEqual(expOutput, output);
        }

        [TestMethod]
        public void CorrectArgs() {
            Test(new string[] {"1"}, "2\r\n1\r\n\r\n", "1\r\n2\r\n");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NoArgs() {
            Test(new string[] { }, "blem", "Argument Error\r\n");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgZero() {
            Test(new string[] { "0" }, "blem", "Argument Error\r\n");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgThousand() {
            Test(new string[] { "1000" }, "blem", "Argument Error\r\n");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgNonNumeric() {
            Test(new string[] { "ah0j" }, "blem", "Argument Error\r\n");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void FormatError() {
            Test(new string[] { "5" }, "2\r\n1a\r\n\r\n", "Format Error\r\n");
        }
    }
}
