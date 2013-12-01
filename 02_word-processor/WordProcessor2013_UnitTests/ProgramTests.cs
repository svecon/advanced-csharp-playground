using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.IO;

namespace WordProcessor2013_UnitTests {
	using WordProcessor2013;

	class WordReaderMockup : IWordReader {
		private string[] words;
		private int nextWord = 0;

		public WordReaderMockup(string[] words) {
			this.words = words;
		}

		public string ReadWord() {
			if (nextWord >= words.Length) {
				return null;
			}

			return words[nextWord++];
		}
	}

	class WordProcessorMockup : IWordProcessor {
		public List<string> Words = new List<string>();

		public void ProcessWord(string word) {
			Words.Add(word);
		}

		public void Finish() {
			// Nothing to do here.
		}
	}

	[TestClass]
	public class ProgramTests {
		public void ProcessWords_TestOnSetOfWords(string[] words) {
			var reader = new WordReaderMockup(words);
			var processor = new WordProcessorMockup();

			Program.ProcessWords(reader, processor);

			CollectionAssert.AreEqual(words, processor.Words);
		}

		[TestMethod]
		public void ProcessWords_NonEmptyListOfWords() {
			ProcessWords_TestOnSetOfWords(new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." });
		}

		[TestMethod]
		public void ProcessWords_EmptyListOfWords() {
			ProcessWords_TestOnSetOfWords(new string [0]);
		}

		//
		//
		//

		public string Call_Run(string[] args) {
			var writer = new StringWriter();
			Program.Run(args, writer);
			return writer.ToString();
		}

		[TestMethod]
		public void Run_NoArguments() {
			var result = Call_Run(new string[0]);
			
			Assert.AreEqual("Argument Error" + Environment.NewLine, result);
		}

		[TestMethod]
		public void Run_EmptyArgument() {
			var result = Call_Run(new [] { "" });
			
			Assert.AreEqual("Argument Error" + Environment.NewLine, result);
		}

		[TestMethod]
		public void Run_NonExistentFile() {
			File.Delete("xxyyzz.txt");

            var result = Call_Run(new[] { "xxyyzz.txt", "dummy222.txt", "0" });
			
			Assert.AreEqual("File Error" + Environment.NewLine, result);
		}

		[TestMethod]
		public void Run_ValidFile() {
			File.Create("validtestinputfile.txt").Dispose();

            var result = Call_Run(new[] { "validtestinputfile.txt", "dummy.txt", "0" });
			
			Assert.AreEqual("", result);
		}

		[TestMethod]
		public void Run_OpensValidFileAndClosesIt() {
			File.Create("validtestinputfile.txt").Dispose();

            var result = Call_Run(new[] { "validtestinputfile.txt", "xxyyzz.txt", "0" });
			Assert.AreEqual("", result);

			// Try to open the input file to verify it was closed by Program.Run method.
			File.OpenRead("validtestinputfile.txt").Dispose();
		}

		[TestMethod]
		public void Run_ValidFileAndAdditionalDummyArguments() {
			File.Create("validtestinputfile.txt").Dispose();

            var result = Call_Run(new[] { "validtestinputfile.txt", "xxyyzz.txt", "dummyargument1", "dummyargument2" });

			Assert.AreEqual("Argument Error" + Environment.NewLine, result);
		}
	}
}
