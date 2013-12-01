using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

namespace WordProcessor2013_UnitTests {
	using WordProcessor2013;

	[TestClass]
	public class WordCounterTests {

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_NullTextWriter() {
			var counter = new WordCounter(null);
		}

		[TestMethod]
		public void ProcessAndFinish_NoWords() {
			var result = new StringWriter();
			var counter = new WordCounter(result);

			counter.Finish();

			Assert.AreEqual("0" + Environment.NewLine, result.ToString());
		}

		[TestMethod]
		public void ProcessAndFinish_SomeWords() {
			var result = new StringWriter();
			var counter = new WordCounter(result);

			var words = new[] { "The", "rain", "in", "Spain", "falls", "mainly", "on", "the", "plain." };
			foreach (var word in words) {
				counter.ProcessWord(word);
			}
			counter.Finish();

			Assert.AreEqual(words.Length.ToString() + Environment.NewLine, result.ToString());
		}
	}
}
