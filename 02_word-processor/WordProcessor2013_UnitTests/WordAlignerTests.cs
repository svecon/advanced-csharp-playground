using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using WordProcessor2013;

namespace WordProcessor2013_UnitTests
{
    [TestClass]
    public class WordAlignerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullTextWriter()
        {
            new WordAligner(null, 0);
        }

        [TestMethod]
        public void ProcessAndFinish_NoWords()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 0);
            aligner.Finish();

            Assert.AreEqual("", result.ToString());
        }

        [TestMethod]
        public void ProcessWord_OneWordIsAlignedLeft()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 99);
            aligner.ProcessWord("Word");
            aligner.Finish();

            Assert.AreEqual(result.ToString(), "Word" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_EvenBlock()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 10);

            var words = new[] { "aa", "b", "c", "d", "e", "Ending" };
            foreach (var word in words)
                aligner.ProcessWord(word);
            aligner.Finish();

            Assert.AreEqual(result.ToString(), "aa b c d e" + Environment.NewLine + "Ending" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_OneBigSpace()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 10);

            var words = new[] { "a", "b", "VerylongEndingOnANewLine" };
            foreach (var word in words)
                aligner.ProcessWord(word);
            aligner.Finish();

            Assert.AreEqual(result.ToString(), "a        b" + Environment.NewLine + "VerylongEndingOnANewLine" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_NewParagraph()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 10);

            var words = new[] { "a", ParagraphReader.paragraphEnding, "b", "VerylongEndingOnANewLine" };
            foreach (var word in words)
                aligner.ProcessWord(word);
            aligner.Finish();

            Assert.AreEqual(result.ToString(), "a" + Environment.NewLine + "" + Environment.NewLine + "b" + Environment.NewLine + "VerylongEndingOnANewLine" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_UnEvenBlock()
        {
            var result = new StringWriter();
            var aligner = new WordAligner(result, 10);

            var words = new[] { "a", "b", "c", "d", "e", "ending" };
            foreach (var word in words)
                aligner.ProcessWord(word);
            aligner.Finish();

            Assert.AreEqual(result.ToString(), "a  b c d e" + Environment.NewLine + "ending" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_CodexExample()
        {
            var words = new[] { "If", "a", "train", "station", "is", "where", "the", "train", "stops,", "what", "is", "a", "work", "station?" };
            var result = new StringWriter();

            var aligner = new WordAligner(result, 17);

            foreach (var word in words)
                aligner.ProcessWord(word);

            aligner.Finish();

            Assert.AreEqual(result.ToString(), "If     a    train" + Environment.NewLine + "station  is where" + Environment.NewLine + "the  train stops," + Environment.NewLine + "what  is  a  work" + Environment.NewLine + "station?" + Environment.NewLine);
        }

        [TestMethod]
        public void ProcessWord_CodexLoremIpsumExample()
        {
            var reader = new ParagraphReader(new StreamReader("lorem.txt"));
            var result = new StringWriter();

            var aligner = new WordAligner(result, 40);

            string word;
            while ((word = reader.ReadWord()) != null)
                aligner.ProcessWord(word);

            aligner.Finish();

            var correct = new StreamReader("lorem-result.txt");

            Assert.AreEqual(result.ToString(), correct.ReadToEnd());
        }
    }
}
