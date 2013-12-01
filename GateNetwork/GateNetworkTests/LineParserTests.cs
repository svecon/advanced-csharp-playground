using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GateNetwork;
using System.IO;

namespace GateNetworkTests {
    [TestClass]
    public class LineParserTests {
        
        void checkResult(string input, string[] expected, LineParser parser){
            string[] result = parser.SplitLine();
            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
            Assert.AreEqual(null, parser.SplitLine());
        }

        [TestMethod]
        public void LineParser_Empty()
        {
            var input = new StringReader("");
            var parser = new LineParser(input);

            Assert.AreEqual(null, parser.SplitLine());
        }

        [TestMethod]
        public void LineParser_TwoWords()
        {
            var input = "ahoj hello";
            var expected = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var parser = new LineParser(new StringReader(input));

            checkResult(input, expected, parser);
        }

        [TestMethod]
        public void LineParser_MultipleSpaces()
        {
            var input = "ahoj    hello";
            var expected = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var parser = new LineParser(new StringReader(input));

            checkResult(input, expected, parser);
        }

        [TestMethod]
        public void LineParser_CustomSeparators()
        {
            var input = "ahoj->helloendhi";
            var expected = new[] { "ahoj", "->", "hello", "end", "hi" };
            var parser = new LineParser(new StringReader(input));
            parser.AddSeparator("->", false).AddSeparator("end", false);

            checkResult(input, expected, parser);
        }

        [TestMethod]
        public void LineParser_BlankLines()
        {
            var input = "one\n\n\n\ntwo";
            var expected = new[,] { {"one"}, {"two"} };
            var parser = new LineParser(new StringReader(input));
            parser.AddSeparator("->", false).AddSeparator("end", false);

            
            for (int i = 0; i < expected.GetLength(0); i++)
            {
                string[] result = parser.SplitLine();
                for (int j = 0; j < expected.GetLength(1); j++)
                {
                    Assert.AreEqual(expected[i,j], result[j]);    
                }
                
            }
            Assert.AreEqual(null, parser.SplitLine());
        }
    }
}
